using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IrcDotNet;
using System.Net;
using WMPLib;
using System.IO;
using TwitchCSharp.Models;

namespace TwitchBotLib
{
    public class BotMain 
    {
        //IRC variables, static
        static int invalidSubmission;
        static TwitchAPI twitchAPI;
        static LevelSubmitter levels;
        static short soundPlayerVolume;
        static DateTime soundCommandCooldown;
        static int cooldownSeconds;
        static IEnumerable<string> MAINCHANNEL;

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
            Console.WriteLine("o         - Open Queue");
            Console.WriteLine("c         - Close Queue");
            Console.WriteLine("Enter Key - Next Level");
            Console.WriteLine("add <n> <l> - Force add level to current queue. <n> name, <l> level");
            Console.WriteLine("q         - Display Remaining Queue");
            Console.WriteLine("limit 15  - Bot chooses 15 submitted levels at random");
            Console.WriteLine("max 3     - Bot chooses maximum of 3 levels from 1 person");
            Console.WriteLine("s <cmnt>  - Save your favorite levels to levels.csv with a comment");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Commands for Sounds:");
            Console.WriteLine("v 30       - Set volume of media player to 30");
            Console.WriteLine("cool 65    - Set cooldown of sound commands to 65 seconds.");
            Console.WriteLine("exit       - Quit");
            Console.WriteLine("");
            Console.WriteLine("______________________________________________________");
            Console.WriteLine("");
        }

        private void InitializeVariables()
        {
            levels = new LevelSubmitter();
            twitchAPI = new TwitchAPI(BotSettings.BotOAuth, BotSettings.BotClientID);
            soundCommandCooldown = DateTime.MinValue;
            cooldownSeconds = 60;
            soundPlayerVolume = 15;
            invalidSubmission = 0;
            MAINCHANNEL = new List<string> { "#" + BotSettings.UserName };  //A list to easily work with IrcDotNet
        }

        public void StartBot()
        {
            string server = BotSettings.TwitchIRC;

            Console.WriteLine("Connecting...");
            Console.WriteLine("");
            using (var client = new IrcDotNet.TwitchIrcClient())
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
                            if (!connectedEvent.Wait(8000))
                            {
                                Console.WriteLine("ERROR: Can't connect to " + server);
                                Console.WriteLine("UserName and/or OAuthChat in settings.xml are invalid");
                                Console.WriteLine("See ReadMe.txt");
                                Console.WriteLine(" ");
                                Console.WriteLine("Press Enter to Exit...");
                                Console.Read();
                                return;
                            }
                        }

                    if (!registeredEvent.Wait(8000))
                    {
                        Console.WriteLine("ERROR: Can't connect to " + server);
                        Console.WriteLine("UserName and/or OAuthChat in settings.xml are invalid");
                        Console.WriteLine("See ReadMe.txt");
                        Console.WriteLine(" ");
                        Console.WriteLine("Press Enter to Exit...");
                        Console.Read();
                        return;
                    }
                }


                client.SendRawMessage("CAP REQ :twitch.tv/membership");  //request to have Twitch IRC send join/part & modes.

                client.Join(MAINCHANNEL);

                HandleEventLoop(client);
            }
        }


        #region Command Line Main Loop
        private static void HandleEventLoop(IrcDotNet.IrcClient client)
        {
            bool isExit = false;
            while (!isExit)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                switch (command)
                {
                    case "exit":
                        isExit = true;
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
                                    client.SendPrivateMessage(MAINCHANNEL, "/me dramAhh Submissions Open dramAhh");
                                    client.SendPrivateMessage(MAINCHANNEL, "/me Submit levels with !submit");
                                }
                            }

                            else if (command == "c")
                            {
                                if (levels.Open)
                                {
                                    levels.CloseQueue();
                                    client.SendPrivateMessage(MAINCHANNEL, "/me dramBoo Submissions Closed dramBoo");
                                    if (levels.FinalLevels.Count > 0)
                                    {
                                        string plural = (levels.FinalLevels.Count != 1) ? " levels " : " level ";
                                        client.SendPrivateMessage(MAINCHANNEL, "/me " + levels.FinalLevels.Count + plural + "will be randomly picked.");
                                        client.SendPrivateMessage(MAINCHANNEL, "/me Now Playing: " + levels.CurrentLevel);
                                        Console.WriteLine();
                                        Console.WriteLine(levels.CurrentLevel + " (" + levels.FinalLevels.Count + ")");
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
                                if (Int32.TryParse(command, out tempCooldown))
                                {
                                    cooldownSeconds = tempCooldown;
                                }
                            }

                            else if (command.Equals("q"))
                            {
                                foreach (var level in levels.FinalLevels)
                                {
                                    Console.WriteLine(level.Item2 + " " + level.Item1);
                                }
                            }


                            else if (command.StartsWith("add "))
                            {
                                string[]args = command.Split(' ');
                                if (args.Length > 2)
                                {
                                    if (LevelSubmitter.IsValidLevelCode(args[2]))
                                    { 
                                        levels.ForceAddLevel(args[2], args[1]);
                                        PostToWebsite();
                                    }
                                }
                            }
                        }

                        //ELSE - command IsNullOrEmpty - (Enter Key pressed)
                        else 
                        {
                            if (levels.FinalLevels.Count > 0)
                            {
                                levels.NextLevel();

                                if (levels.FinalLevels.Count > 0)
                                {
                                    client.SendPrivateMessage(MAINCHANNEL, "/me Now Playing: " + levels.CurrentLevel);
                                    Console.WriteLine();
                                    Console.WriteLine(levels.CurrentLevel + " ("+ levels.FinalLevels.Count + ")");
                                    Console.WriteLine();
                                }
                                else
                                    Console.WriteLine("Queue Empty.");
                            }
                        }

                        break;
                }
            }

        client.Disconnect();

        }



        #endregion


        #region IRC Main Logic

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
                    Console.WriteLine("");
                    Console.WriteLine(command);
                    Console.WriteLine("");
                }

                
                else if (command.StartsWith("!") && command.Length > 1)
                {
                    command = command.Remove(0,1);

                    //SUB and Operator only commands
                    if (user.IsOperator || twitchAPI.Subscribers.Contains(user.NickName))
                    {
                        switch (command)
                        {
                            case "bfb": PlaySound("sounds\\bfb.mp3"); break;
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

                        if (LevelSubmitter.IsValidLevelCode(command))
                        {
                            if (user.IsOperator || twitchAPI.Subscribers.Contains(user.NickName))
                                levels.AddLevel(command.ToUpper(), user.NickName, 5);
                            else
                                levels.AddLevel(command.ToUpper(), user.NickName,1);
                        }
                    }
                }
            }
        }

        private static void IrcClient_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
            client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }

        private static void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
        }

        private static void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;

            Console.WriteLine("Bot ready in IRC channel {0}.", e.Channel.Name);
            Console.Write("> ");
        }

        #endregion


        #region Helper Methods - TODO: Good candidates to be refactored elsewhere.





        private static void SaveLevel(string comment)
        {
            if (levels.CurrentLevel != String.Empty)
            {
                if (comment.Contains("\""))
                {
                    comment = comment.Replace("\"", "\"\"");
                }

                if (comment.Contains(","))
                {
                    comment = comment.Replace(",", " ");
                }

                if (comment.Contains(System.Environment.NewLine))
                {
                    comment = String.Format("\"{0}\"", comment);
                }

                try
                {
                    using (StreamWriter sc = new StreamWriter("levels.csv", true))
                    {
                        sc.WriteLine(levels.CurrentLevel + ", " + comment);
                        Console.WriteLine("Saved to levels.csv " + levels.CurrentLevel);
                        Console.WriteLine("");
                    }
                }
                catch
                {
                    Console.WriteLine("Could not save level.");
                    Console.WriteLine("Is it opened in another program?");
                    Console.WriteLine("");
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

            foreach (var level in levels.FinalLevels)
            {
                returnHTML.AppendLine("<tr>");
                returnHTML.AppendLine("   <td class='cell'>" + level.Item2);
                returnHTML.AppendLine("   <td class='cell'>" + level.Item1);
                returnHTML.AppendLine("</tr>");
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
                ftpClient.Credentials = new System.Net.NetworkCredential(ftpusername, ftppassword);
                ftpClient.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                ftpClient.UseBinary = true;
                ftpClient.KeepAlive = true;
                System.IO.FileInfo fi = new System.IO.FileInfo(tempFile);
                ftpClient.ContentLength = fi.Length;
                byte[] buffer = new byte[4097];
                int bytes = 0;
                int total_bytes = (int)fi.Length;
                System.IO.FileStream fs = fi.OpenRead();
                System.IO.Stream rs = ftpClient.GetRequestStream();
                while (total_bytes > 0)
                {
                    bytes = fs.Read(buffer, 0, buffer.Length);
                    rs.Write(buffer, 0, bytes);
                    total_bytes = total_bytes - bytes;
                }
                //fs.Flush();
                fs.Close();
                rs.Close();
                FtpWebResponse uploadResponse = (FtpWebResponse)ftpClient.GetResponse();
                string value = uploadResponse.StatusDescription;
                uploadResponse.Close();

                File.Delete(tempFile);
                Directory.Delete(tempDir);
                Console.WriteLine("HTML Page updated successfully.");
                Console.WriteLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine("HTML Page creation Error: " + ex.Message);
                Console.WriteLine();
            }


        }

        private static void UpdateHTMLPage(string fileName, string tempDir, string tempFile)
        {

            string html = String.Empty;

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
