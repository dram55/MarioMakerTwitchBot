using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IrcDotNet;
using System.Net;
using WMPLib;
using System.IO;
using SettingsHelp;
using System.Windows.Threading;
using log4net;
using TwitchBotLib.Objects;

namespace TwitchBotLib
{
    public class BotMain 
    {
        //IRC variables, static
        static int invalidSubmission;
        static LevelSubmitter levels;
        static short soundPlayerVolume;
        static DateTime soundCommandCooldown;
        static int cooldownSeconds;
        static IEnumerable<string> MAINCHANNEL;
        static bool isConnectedToIRC;
        static SettingsHelp.MainWindow settingsHelpWindow;
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsExit { get; set; }

        public bool Restart { get; set; }

        public BotMain()
        {
            InitializeVariables();
            DisplayMainMenu();
        }

        private static void DisplayMainMenu()
        {
            Console.WriteLine("Welcome to Chat Bot!");
            Console.WriteLine("");
            Console.WriteLine("Commands for Mario Maker:");
            Console.WriteLine("o           - Open Submissions");
            Console.WriteLine("c           - Close Submissions and create a queue");
            Console.WriteLine("Enter Key   - Next Level");
            Console.WriteLine("prev        - Previous Level");
            Console.WriteLine("add <n> <l> - Force add level to current queue. <n>=name, <l>=level code");
            Console.WriteLine("q           - Display Remaining Queue");
            Console.WriteLine("limit 15    - When submissions close - bot picks 15 levels at random");
            Console.WriteLine("max 3       - Maximum of 3 level submissions per person");
            Console.WriteLine("s <cmnt>    - Save the current level to levels.csv with a comment");
            Console.WriteLine("");
            Console.WriteLine("General Commands:");
            Console.WriteLine("v 30        - Set volume of media player to 30");
            Console.WriteLine("cool 65     - Set cooldown of sound commands to 65 seconds.");
            Console.WriteLine();
            Console.WriteLine("restart     - Restarts bot");
            Console.WriteLine("settings    - Change settings");
            Console.WriteLine("exit        - Quit");
            Console.WriteLine("");
            Console.WriteLine("______________________________________________________");
            Console.WriteLine("");
        }

        private void InitializeVariables()
        {
            levels = new LevelSubmitter();
            soundCommandCooldown = DateTime.MinValue;
            cooldownSeconds = 60;
            soundPlayerVolume = 15;
            invalidSubmission = 0;
            isConnectedToIRC = true;
            MAINCHANNEL = new List<string> { "#" + BotSettings.UserName };  //A List to work with IrcDotNet
        }

        public void StartBot()
        {
            string server = BotSettings.TwitchIRC;
            logger.Debug("Connecting to IRC...");
            Console.WriteLine("Connecting...");
            Console.WriteLine("");
            using (var client = new TwitchIrcClient())
            {
                client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
                client.Registered += IrcClient_Registered;
                // Wait until connection has succeeded or timed out.
                using (var registeredEvent = new ManualResetEventSlim(false))
                {
                    //Group chat - for whisper (not using)
                    //byte[]ip = {199,9,253,119};
                    //IPAddress p = new IPAddress(ip);
                    //IPEndPoint i = new IPEndPoint(p, 443);

                        using (var connectedEvent = new ManualResetEventSlim(false))
                        {
                            client.Connected += (sender2, e2) => connectedEvent.Set();
                            client.Registered += (sender2, e2) => registeredEvent.Set();
                            client.Connect(server, false,
                                new IrcUserRegistrationInfo()
                                {
                                    NickName = BotSettings.UserName,
                                    Password = BotSettings.OAuthChat,
                                    UserName = BotSettings.UserName
                                });
                            if (!connectedEvent.Wait(3000))
                            {
                                isConnectedToIRC = false;
                                DisplayConnectionError(server);
                                OpenSettingsWindow();
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine("Press Enter to restart Bot and apply new settings..");
                                Console.WriteLine();
                                Console.ReadLine();
                                Restart = true;
                        }
                    }

                    if (!registeredEvent.Wait(3000))
                    {
                        if (isConnectedToIRC)
                        {
                            isConnectedToIRC = false;
                            DisplayConnectionError(server);
                            OpenSettingsWindow();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Press Enter to restart Bot and apply new settings.");
                            Console.WriteLine();
                            Console.ReadLine();
                            Restart = true;
                        }
                    }
                }

                if (isConnectedToIRC)
                {
                    logger.Debug("Connected, about to join channel.");
                    client.SendRawMessage("CAP REQ :twitch.tv/membership");  //request to have Twitch IRC send join/part & modes.
                    client.Join(MAINCHANNEL);
                    HandleEventLoop(client);
                }

            }

        }


        #region Command Line Main Loop
        private void HandleEventLoop(IrcDotNet.IrcClient client)
        {
            logger.Debug("In HandleEventLoop");

            IsExit = false;
            while (!IsExit)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                switch (command)
                {
                    case "exit":
                    case "quit":
                        IsExit = true;
                        break;
                    default:
                        if (!string.IsNullOrEmpty(command))
                        {
                            if (command.StartsWith("limit"))
                            {
                                short tempLimit;
                                if (Int16.TryParse(command.Substring(6).Trim(), out tempLimit))
                                    levels.LevelLimit = tempLimit;
                            }

                            else if (command == "o")
                            {
                                if (!levels.Open)
                                {
                                    levels.OpenQueue();
                                    client.SendPrivateMessage(MAINCHANNEL, "/me Submissions Open");
                                    client.SendPrivateMessage(MAINCHANNEL, "/me Submit levels with !submit");
                                }
                            }

                            else if (command == "c")
                            {
                                if (levels.Open)
                                {
                                    levels.CloseQueue();
                                    client.SendPrivateMessage(MAINCHANNEL, "/me Submissions Closed");
                                    if (levels.FinalLevels.Count >= 0)
                                    {
                                        string plural = (levels.FinalLevels.Count != 1) ? " levels " : " level ";
                                        client.SendPrivateMessage(MAINCHANNEL, "/me " + levels.FinalLevels.Count + plural + "will be randomly picked.");
                                        client.SendPrivateMessage(MAINCHANNEL, "/me Now Playing: " + levels.CurrentLevel.User + " " + levels.CurrentLevel.LevelID);
                                        Console.WriteLine();
                                        Console.WriteLine(levels.CurrentLevel.User + " " + levels.CurrentLevel.LevelID + " (" + levels.Remaining + ")");
                                        Console.WriteLine();
                                        PostToWebsite();
                                    }
                                    else
                                        client.SendPrivateMessage(MAINCHANNEL, "/me No Levels submitted.");

                                }
                            }

                            else if (command.StartsWith("s "))
                            {
                                command = command.Remove(0, 2).Trim();
                                SaveLevel(command);
                            }

                            else if (command == "settings")
                            {
                                OpenSettingsWindow();
                            }

                            else if (command == "restart")
                            {
                                this.Restart = true;
                                return;
                            }

                            else if (command.StartsWith("v "))
                            {
                                command = command.Remove(0, 2).Trim();
                                short vol;
                                if(Int16.TryParse(command,out vol))
                                {
                                    soundPlayerVolume = vol;
                                }
                            }

                            else if (command.StartsWith("max "))
                            {
                                command = command.Remove(0, 4).Trim();
                                int amt;
                                if (Int32.TryParse(command, out amt))
                                {
                                    if (amt<=0)
                                        break;
                                    BotSettings.MaxSubmissionsForSingleUser = amt;
                                    Console.WriteLine("User can only submit " + amt + " level(s) per round.");
                                    Console.WriteLine();
                                }
                            }

                            else if (command.StartsWith("cool "))
                            {
                                command = command.Remove(0, 5).Trim();
                                int tempCooldown;
                                if (int.TryParse(command, out tempCooldown))
                                {
                                    cooldownSeconds = tempCooldown;
                                }
                            }

                            else if (command == "q")
                            {
                                foreach (var level in levels.FinalLevels)
                                {
                                    Console.WriteLine(level.User.NickName + " " + level.LevelID);
                                }
                            }


                            else if (command.StartsWith("add "))
                            {
                                string[]args = command.Split(' ');
                                if (args.Length > 2)
                                {
                                    if (LevelSubmitter.IsValidLevelCode(ref args[2]))
                                    { 
                                        levels.ForceAddLevel(args[2].ToUpper(), args[1]);
                                        PostToWebsite();
                                        if (levels.Remaining==0)
                                            Console.WriteLine(levels.CurrentLevel.User +" " + levels.CurrentLevel.LevelID + " (" + levels.Remaining + ")");
                                    }
                                }
                            }

                            else if (command == "prev")
                            {
                                if (levels.Remaining == levels.FinalLevels.Count)
                                    break;
                                levels.PreviousLevel();
                                PostToWebsite();
                                Console.WriteLine();
                                Console.WriteLine(levels.CurrentLevel.User + " " + levels.CurrentLevel.LevelID + " (" + levels.Remaining + ")");
                                Console.WriteLine();
                            }

                            else if (command == "h" || command == "help")
                            {
                                DisplayMainMenu();
                            }

                        }

                        //ELSE - command IsNullOrEmpty - (Enter Key pressed)
                        else 
                        {
                            if (levels.Remaining > 0)
                            {
                                levels.NextLevel();
                                PostToWebsite();
                                client.SendPrivateMessage(MAINCHANNEL, "/me Now Playing: " + levels.CurrentLevel.User + " " + levels.CurrentLevel.LevelID);
                                Console.WriteLine();
                                Console.WriteLine(levels.CurrentLevel.User + " " + levels.CurrentLevel.LevelID + " (" + levels.Remaining + ")");
                                Console.WriteLine();
                            }
                        }

                        break;
                }
            }

        client.Disconnect();

        }

        #endregion


        #region IRC Event Handler Logic

        private static void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            
            var channel = (IrcChannel)sender;
            if (e.Source is IrcUser)
            {
                IrcUser user = (IrcUser)e.Source;
                string command = e.Text;

                //Twitch notify will let you know if you have a new sub or are hosted.
                if (user.NickName == "twitchnotify")
                {
                    //If you are getting an alert that someone subscribed..
                    //then don't even bother with the API, just update IsSubscriber to true
                    if (command.Contains(" subscribed "))
                    {
                        bool newUser;
                        string userName = command.Split(' ')[0];
                        IrcUser subscribedUser = channel.Client.GetUserFromNickName(userName, true, out newUser);
                        subscribedUser.IsSubscriber = true;

                    }
                    Console.WriteLine("");
                    Console.WriteLine(command);
                    Console.WriteLine("");
                }

                
                else if (command.StartsWith("!") && command.Length > 1)
                {
                    command = command.Remove(0,1);

                    //SUB and Operator only commands
                    if (user.IsOperator || user.IsSubscriber)
                    {
                        switch (command)
                        {
                            case "bfb": PlaySound("sounds\\bfb.mp3"); break;
                            case "bes": PlaySound("sounds\\bes.mp3"); break;
                            case "speed": PlaySound("sounds\\speed.mp3"); break;
                            case "yeah": PlaySound("sounds\\yeah.mp3"); break;
                            case "dik": PlaySound("sounds\\dik.mp3"); break;
                            case "uptime":
                                Random m = new Random();
                                string hr = m.Next(2, 999).ToString();
                                channel.Client.SendPrivateMessage(MAINCHANNEL,"Uptime: " + hr + " hours.");
                                break;
                            default:
                                break;
                        }
                    }


                    //Any other commands:
                    if (command.StartsWith("submit "))
                    {
                        //People trying to submit when the QUEUE is closed, get warned - to avoid spam.
                        if (!levels.Open)
                        {
                            InvalidSubmission(channel);
                            return;
                        }

                        command = command.Remove(0,6).Trim();

                        try
                        {
                            LevelSubmission currentSubmission = new LevelSubmission(command, user, false);
                            levels.AddLevel(currentSubmission);
                        }
                        catch (ArgumentException)
                        {
                            //Invalid level code or API fuckup
                            //code could go here to alert the user they f'd up.
                        }

                    }
                }
            }
        }

        private static void IrcClient_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
            logger.Debug("IrcClient_Registered - client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;");
            client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }

        private static void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
        }

        private static void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            logger.Debug("IrcClient_LocalUser_JoinedChannel - e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;");
            e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
            Console.WriteLine("Bot ready in IRC channel {0}.", e.Channel.Name);
            Console.Write("> ");
        }

        #endregion


        #region Helper Methods - TODO: Good candidates to be refactored elsewhere.


        private static void OpenSettingsWindow()
        {
            if (isConnectedToIRC)
            {
                Console.WriteLine("");
                Console.WriteLine("NOTE: If you update IRC settings, you must restart the bot for it to take effect.");
                Console.WriteLine("");
            }

            Console.WriteLine("Opening Settings...");

            Thread thread = new Thread(() =>
            {
                settingsHelpWindow = new MainWindow();
                settingsHelpWindow.Show();
                settingsHelpWindow.Closed += (s, e) => settingsHelpWindow.Dispatcher.InvokeShutdown();
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();


        }


        private static void SaveLevel(string comment)
        {
            if (levels.CurrentLevel != null)
            {
                if (comment.Contains("\""))
                {
                    comment = comment.Replace("\"", "\"\"");
                }

                if (comment.Contains(","))
                {
                    comment = comment.Replace(",", " ");
                }

                if (comment.Contains(Environment.NewLine))
                {
                    comment = string.Format("\"{0}\"", comment);
                }

                try
                {
                    using (StreamWriter sc = new StreamWriter("levels.csv", true))
                    {
                        sc.WriteLine("{0},{1},{2},{3}",levels.CurrentLevel.User , levels.CurrentLevel.LevelID, levels.CurrentLevel.BookmarkURL, comment);
                        Console.WriteLine("Saved to levels.csv");
                        Console.WriteLine("");
                    }
                }
                catch
                {
                    Console.WriteLine("Could not save level.");
                }
            }
        }

        private static void PlaySound(string file)
        {

            double secondsPassed = (DateTime.Now - soundCommandCooldown).TotalSeconds;

            if (secondsPassed >= cooldownSeconds)
            {
                //Create new sound player so multiple sounds can be played at once. 
                WindowsMediaPlayer soundPlayer = new WindowsMediaPlayer(); 
                soundPlayer.settings.volume = soundPlayerVolume;
                soundPlayer.URL = file;
                soundPlayer.controls.play();
                soundCommandCooldown = DateTime.Now;
            }

        }

        private static string WriteHTMLList()
        {
            StringBuilder returnHTML = new StringBuilder();

            int index = levels.FinalLevels.Count - 1; //Cheesy way to highlight the current level. TODO: Change this. 
            foreach (var level in levels.FinalLevels)
            {
                string htmlClass = "cell";
                if (levels.Remaining == index)
                    htmlClass += " current";

                string levelLinkClass = "";
                string thumbnailURL = "";
                string dataValues = "";
                if (level.Level != null)
                {
                    levelLinkClass = "preview";
                    thumbnailURL = level.Level.ThumbnailURL;
                    if (level.Level.Title != null)
                        dataValues += " data-title=\"" + level.Level.Title +"\" ";
                    if (level.Level.Attempts != null)
                        dataValues += " data-clear=\"" + String.Format("{0:F2}",level.Level.ClearRate) + "%\" ";
                }


                returnHTML.AppendLine("<tr>");
                returnHTML.AppendLine("   <td class='" + htmlClass + "'>" + level.User.NickName);
                returnHTML.AppendLine("   <td class='" + htmlClass + "'><a href=\""+ level.BookmarkURL 
                    + "\" target=\"_blank\" title=\""+ thumbnailURL + "\" "+dataValues+" class=\"" + levelLinkClass + "\">" + level.LevelID + "</a>");
                returnHTML.AppendLine("</tr>");
                index--;
            }

            return returnHTML.ToString();
        }


        private static void PostToWebsite()
        {
            try
            {
                string fileName = BotSettings.RootDirectory + BotSettings.HTMLPage;
                string tempFile = BotSettings.RootDirectory + "temp\\" + BotSettings.HTMLPage;
                string tempDir = BotSettings.RootDirectory + "temp";

                if (!File.Exists(fileName))
                {
                    Console.WriteLine("Did not update HTML Page. No page to edit.");
                    Console.WriteLine();
                    return;
                }

                if (BotSettings.FTPUserName == "")
                {
                    Console.WriteLine("Did not update HTML Page. FTPUserName is blank.");
                    Console.WriteLine();
                    return;
                }

                if (BotSettings.FTPAddress == "")
                {
                    Console.WriteLine("Did not update HTML Page. FTPAddress is blank.");
                    Console.WriteLine();
                    return;
                }

                UpdateHTMLPage(fileName, tempDir, tempFile);

                string ftpurl = BotSettings.FTPAddress;
                string ftpusername = BotSettings.FTPUserName;
                string ftppassword = BotSettings.FTPPassword;

                FtpWebRequest ftpClient = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpurl + "/" + BotSettings.HTMLPage));
                ftpClient.Credentials = new NetworkCredential(ftpusername, ftppassword);
                ftpClient.Method = WebRequestMethods.Ftp.UploadFile;
                ftpClient.UseBinary = true;
                ftpClient.KeepAlive = true;
                FileInfo fi = new FileInfo(tempFile);
                ftpClient.ContentLength = fi.Length;
                byte[] buffer = new byte[4097];
                int bytes = 0;
                int total_bytes = (int)fi.Length;


                using (FileStream fs = fi.OpenRead())
                {
                    using (System.IO.Stream rs = ftpClient.GetRequestStream())
                    {

                        while (total_bytes > 0)
                        {
                            bytes = fs.Read(buffer, 0, buffer.Length);
                            rs.Write(buffer, 0, bytes);
                            total_bytes = total_bytes - bytes;
                        }
                        //fs.Close();
                        //rs.Close();
                    }
                }

                //FtpWebResponse uploadResponse = (FtpWebResponse)ftpClient.GetResponse();
                //uploadResponse.Close();

                File.Delete(tempFile);
                Directory.Delete(tempDir);
                logger.Debug("HTML Page updated successfully.");
                //Console.WriteLine("HTML Page updated successfully.");
                //Console.WriteLine();
            }
            catch(Exception ex)
            {
                logger.Debug("HTML Page creation Error: " + ex.Message);
                Console.WriteLine("HTML Page creation Error: " + ex.Message);
                Console.WriteLine();
            }


        }

        private static void UpdateHTMLPage(string fileName, string tempDir, string tempFile)
        {

            string html ="";

            using (StreamReader sr = new StreamReader(fileName))
            {
                html = sr.ReadToEnd();
                string newTable = WriteHTMLList();
                html = html.Replace("{details}", newTable);
                html = html.Replace("{date}", DateTime.Now.ToString());
            }

            Directory.CreateDirectory(tempDir);

            using (StreamWriter sw = new StreamWriter(tempFile, false))
            {
                sw.Write(html);
            }

        }

        private static void DisplayConnectionError(string server)
        {
            Console.WriteLine("ERROR: Can't connect to " + server);
            Console.WriteLine("User Name and/or OAuthChat are invalid in the Settings.");
            Console.WriteLine(" ");
            Console.WriteLine("Press Enter to Edit Settings");
            Console.ReadLine();
        }

        private static void InvalidSubmission(IrcChannel channel)
        {
            invalidSubmission++;

            if (invalidSubmission > 2)
            {
                invalidSubmission = 0;
                channel.Client.SendPrivateMessage(MAINCHANNEL, "Submissions closed. Please read http://pastebin.com/WuWarXiC");
            }
        }

        #endregion

    }

}
