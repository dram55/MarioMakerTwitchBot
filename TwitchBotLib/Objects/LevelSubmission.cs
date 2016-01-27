using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IrcDotNet;
using TwitchBotLib.SMMAPi;

namespace TwitchBotLib.Objects
{
    public class LevelSubmission
    {
        private const string MarioMakerBookmarkWebSite = @"https://supermariomakerbookmark.nintendo.net/courses/";

        public string LevelID { get; private set; }
        public IrcUser User { get; private set; }
        private string bookmarkURL;
        public string BookmarkURL
        {
            get { return String.Format("{0}/{1}", MarioMakerBookmarkWebSite, LevelID); }
            set { bookmarkURL = value; }
        }

        public bool IsPlayed { get; set; }
        public bool IsQueued { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public Level Level { get; private set; }

        public LevelSubmission(string levelID, IrcUser user, bool forceAdd)
        {

            //If you are force adding the level, bypass error checking, formatting and API call.
            if (!forceAdd)
            {
                if (!IsValidLevelCode(levelID))
                    throw new ArgumentException("Invalid level code format");
                LevelID = FormatLevelCode(levelID);
                Level = SMMApi.GetLevel(LevelID);
                if (Level == null)
                    throw new ArgumentException("Level does not exist");
            }
            else
                LevelID = levelID;
            //Initiaze variables
            User = user;
            IsPlayed = false;
            IsQueued = false;
        }

        private string FormatLevelCode (string levelCode)
        {
            if (levelCode.Split('-').Length < 4)
            {
                levelCode = levelCode.Replace("-", "");
                levelCode = levelCode.Substring(0, 4) + "-" + levelCode.Substring(4, 4) + "-" + levelCode.Substring(8, 4) + "-" + levelCode.Substring(12, 4);
                levelCode = levelCode.ToUpper();
            }

            return levelCode;
        }

        private bool IsValidLevelCode(string levelCode)
        {
            //Capture hex in the format 0000-FFFF-EEEE-DDDD OR 0000FFFFEEEEDDDD (0000-FFFF-EEEEDDDD would work too.)
            Regex r = new Regex("^([0-9A-Fa-f]{4}-?){3}[0-9A-Fa-f]{4}$");
            if (r.IsMatch(levelCode)) return true;
            else return false;
        }


    }
}
