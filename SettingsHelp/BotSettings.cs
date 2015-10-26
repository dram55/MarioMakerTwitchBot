using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace TwitchBotLib
{
    /// <summary>
    /// Internally holds settings with a Dictionary and has public facing properties for type safe access.
    /// 
    /// To add a new setting:
    ///      1) add a public property, 
    ///      2) update in LoadPublicProperties() 
    ///      3) add Dictionary Entry in InitializeSettings()
    /// </summary>
    public static class BotSettings
    {
        private const string XML_FILE = "settings.xml";
        private static string _userName;
        public static string UserName { get { return _userName.ToLower(); } private set { _userName = value.ToLower();} }
        public static string OAuthChat { get; private set; }
        public static string TwitchIRC { get; private set; }
        public static string BotClientID { get; private set; }
        public static string BotOAuth { get; private set; }
        public static string OpenSubmissionMessage { get; private set; }
        public static string CloseSubmissionsMessage { get; private set; }
        private static string _maxSubmissionsForSingleUser;
        public static int MaxSubmissionsForSingleUser {
            get {
                int t;
                if (Int32.TryParse(_maxSubmissionsForSingleUser, out t))
                    return t;
                else
                    return 2;
            }
            set { _maxSubmissionsForSingleUser = value.ToString(); }
        }
        public static string FTPAddress { get; private set; }
        public static string FTPUserName { get; private set; }
        public static string FTPPassword { get; private set; }
        public static string HTMLPage { get; private set; }
        public static string RootDirectory {get{return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";}}
        //Add other settings here
        //public static string NewSetting { get; private set; }

        public static Dictionary<string, BotSetting> _settings;


        public static void Load()
        {
            InitializeDictionary();

            if (File.Exists(XML_FILE))
                LoadExistingSettingsFile();
            else
                CreateSettingsFile();

            LoadPublicProperties();
        }

        public static void Save()
        {
            SaveToSettingsFile();
            LoadPublicProperties();
        }

        private static void LoadPublicProperties()
        {

            UserName = _settings["UserName"].Value.ToLower();
            OAuthChat = _settings["OAuthChat"].Value;
            BotClientID = _settings["BotClientID"].Value;
            BotOAuth = _settings["BotOAuth"].Value;
            TwitchIRC = _settings["TwitchIRC"].Value;
            OpenSubmissionMessage = _settings["OpenSubmissionMessage"].Value;
            CloseSubmissionsMessage = _settings["CloseSubmissionsMessage"].Value;
            _maxSubmissionsForSingleUser = _settings["MaxSubmissionsForSingleUser"].Value;
            FTPAddress = _settings["FTPAddress"].Value;
            FTPUserName = _settings["FTPUserName"].Value;
            FTPPassword = _settings["FTPPassword"].Value;
            HTMLPage = _settings["HTMLPage"].Value;
            //Add other settings here
            //NewSettingName = _settings["NewSettingName"].Value;
        }

        private static void InitializeDictionary()
        {
            _settings = new Dictionary<string, BotSetting>()
            {
                { "UserName", new BotSetting() { DefaultValue = "YourTwitchName" } },
                { "OAuthChat", new BotSetting() { DefaultValue = "Get From: https://twitchapps.com/tmi/"  } },
                { "BotClientID", new BotSetting() { DefaultValue = "fjhkjex3dosfwql6jcne4klacgixv80" } },
                { "BotOAuth", new BotSetting() { DefaultValue = "Get From: http://goo.gl/53mMa2" } },
                { "TwitchIRC", new BotSetting() { DefaultValue = "irc.twitch.tv" } },
                { "OpenSubmissionMessage", new BotSetting() { DefaultValue = "Submissions Open!\r\nUse !submit in chat." } },
                { "CloseSubmissionsMessage", new BotSetting() { DefaultValue = "Submissions closed!\r\nWill re-open when\r\n queue is empty." } },
                { "MaxSubmissionsForSingleUser", new BotSetting() { DefaultValue = "2" } },
                { "FTPAddress", new BotSetting() { DefaultValue = "ftp.dram55.com" } },
                { "FTPUserName", new BotSetting() { DefaultValue = "username@dram55.com" } },
                { "FTPPassword", new BotSetting() { DefaultValue = "" } },
                { "HTMLPage", new BotSetting() { DefaultValue = "queue.html" } },
                //Add other settings here
                //{ "NewSettingName", new BotSetting() { DefaultValue = "New Setting Default Value" } },
            };

        }


        private static void LoadExistingSettingsFile()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(XML_FILE);
            XmlNode body = doc.SelectSingleNode("BotSettings");

            foreach (var setting in _settings)
            {
                var node = body.SelectSingleNode(setting.Key);
                if (node != null)
                    setting.Value.Value = node.InnerText.Trim();
            }

            doc.Save(XML_FILE);
        }

        private static void SaveToSettingsFile()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(XML_FILE);
            XmlNode body = doc.SelectSingleNode("BotSettings");

            foreach (var setting in _settings)
            {
                var node = body.SelectSingleNode(setting.Key);
                if (node != null)
                    node.InnerText = setting.Value.Value;
            }

            doc.Save(XML_FILE);
        }

        private static void CreateSettingsFile()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlNode settings = doc.CreateElement("BotSettings");
            doc.AppendChild(settings);

            foreach (var setting in _settings)
            {
                XmlNode tempNode = doc.CreateElement(setting.Key);
                tempNode.AppendChild(doc.CreateTextNode(setting.Value.DefaultValue));
                settings.AppendChild(tempNode);
                setting.Value.Value = setting.Value.DefaultValue;
            }

            doc.Save(XML_FILE);
        }

        public class BotSetting
        {
            public string Value { get; set; }
            public string DefaultValue { get; set; }
        }

    }
}
