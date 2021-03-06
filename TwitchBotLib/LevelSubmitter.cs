﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using TwitchBotLib.Objects;

namespace TwitchBotLib
{
    public class LevelSubmitter
    {
        public bool Open { get; private set; }
        public int LevelLimit { get; set; }
        public List<LevelSubmission> AllLevels { get; private set; }
        private int currentIndex;
        static Random r = new Random();

        string OPEN_TEXT_FILE = BotSettings.RootDirectory + "text\\open.txt";
        string CLOSE_TEXT_FILE = BotSettings.RootDirectory + "text\\close.txt";
        string NEXT_LEVEL_FILE = BotSettings.RootDirectory + "text\\nextLevel.txt";

        public int Remaining 
        {
            get
            {
                return (_finalLevels.Count - (currentIndex+1));
            }
        }

        //public string CurrentLevel
        //{
        //    get {
        //        if (_finalLevels.Count > 0)
        //            return _finalLevels[currentIndex].User.NickName + ", " + _finalLevels[currentIndex].LevelID;
        //        else
        //            return String.Empty;
        //    }
        //}
        public LevelSubmission CurrentLevel
        {
            get
            {
                if (_finalLevels.Count > 0)
                    return _finalLevels[currentIndex];
                else
                    return null;
            }
        }

        private List<LevelSubmission> _finalLevels;
        public List<LevelSubmission> FinalLevels
        {
            get {
                    return _finalLevels;
            }
            set {
                _finalLevels = value;
            }

        }

        public LevelSubmitter()
        {
            Open = false;
            AllLevels = new List<LevelSubmission>();
            _finalLevels = new List<LevelSubmission>();
            LevelLimit = 10;
            currentIndex = 0;
            CreateDirectories();
        }

        private void CreateDirectories()
        {
            if (!Directory.Exists("text"))
                Directory.CreateDirectory("text");
        }


        public void PreviousLevel()
        {
            if (currentIndex > 0)
                currentIndex--;
         
            displayNextLevel();
        }

        public void NextLevel()
        {
            if (Remaining <=0)
                return;
            currentIndex++;
            displayNextLevel();
        }

        private void displayNextLevel()
        {
            if (Remaining >= 0)
            {
                using (StreamWriter file = new StreamWriter(NEXT_LEVEL_FILE, false))
                {
                    var currentLevel = _finalLevels[currentIndex];
                    file.WriteLine(currentLevel.User.NickName);
                    file.WriteLine(currentLevel.LevelID);
                }
            }

        }

        public void OpenQueue()
        {
            AllLevels = new List<LevelSubmission>();
            displayOpenQueueText();
            this.Open = true;
        }

        private void displayOpenQueueText()
        {
            using (StreamWriter file = new StreamWriter(OPEN_TEXT_FILE, false))
            {
                file.WriteLine(BotSettings.OpenSubmissionMessage);
                
            }

            using (StreamWriter file = new StreamWriter(CLOSE_TEXT_FILE, false))
            {
                file.WriteLine("");
            }
        }

        public void CloseQueue()
        {
            _finalLevels = new List<LevelSubmission>();
            currentIndex = 0;
            randomlyPickLevels();
            displayCloseQueueText();
            displayNextLevel();
            AllLevels = new List<LevelSubmission>();
            Open = false;
        }

        private void displayCloseQueueText()
        {
            using (StreamWriter file = new StreamWriter(OPEN_TEXT_FILE, false))
            {
                file.WriteLine("");
            }

            using (StreamWriter file = new StreamWriter(CLOSE_TEXT_FILE, false))
            {
                file.WriteLine(BotSettings.CloseSubmissionsMessage);
            }
        }

        private void AddLevelHelper(LevelSubmission submission, int timesToEnter)
        {
            for (int i = 1; i <= timesToEnter; i++)
                AllLevels.Add(submission);
        }

        internal void AddLevel(LevelSubmission currentSubmission)
        {
            //If level has already been submitted, then bail
            if (AllLevels.Any(t => t.LevelID == currentSubmission.LevelID))
                return;

            if (currentSubmission.User.IsOperator || currentSubmission.User.IsSubscriber)
                AddLevelHelper(currentSubmission, 5);
            else
                AddLevelHelper(currentSubmission, 1);

        }

        /// <summary>
        /// Randomly picks LevelLimit amount of levels from AllLevels
        /// and puts them in FinalLevels
        /// </summary>
        private void randomlyPickLevels()
        {

            int counter = LevelLimit;
            while (counter > 0 && AllLevels.Count > 0)
            {
                int rand = r.Next(0, AllLevels.Count);
                LevelSubmission randomlySelectedLevel = AllLevels[rand];
                _finalLevels.Add(randomlySelectedLevel);
                AllLevels.RemoveAll(t => t.LevelID == randomlySelectedLevel.LevelID);
                if (_finalLevels.Count(t => t.User.NickName == randomlySelectedLevel.User.NickName) >= BotSettings.MaxSubmissionsForSingleUser)
                    AllLevels.RemoveAll(t => t.User.NickName == randomlySelectedLevel.User.NickName);
                counter--;
            }
        }


        public void ForceAddLevel(string levelCode,string submitter)
        {
            IrcDotNet.IrcUser tempUser = new IrcDotNet.IrcUser { NickName = submitter };
            LevelSubmission submission = new LevelSubmission(levelCode, tempUser, true);
            _finalLevels.Add(submission);
            if (Remaining == 0)
                displayNextLevel();
        }




        public static bool IsValidLevelCode(ref string levelCode)
        {
            //Capture hex in the format 0000-FFFF-EEEE-DDDD OR 0000FFFFEEEEDDDD (0000-FFFF-EEEEDDDD would work too.)
            Regex r = new Regex("^([0-9A-Fa-f]{4}-?){3}[0-9A-Fa-f]{4}$");
            if (!r.IsMatch(levelCode))
                return false;
            else
            {
                if (levelCode.Split('-').Length < 4)
                {
                    levelCode = levelCode.Replace("-", "");
                    levelCode = levelCode.Substring(0,4) + "-" + levelCode.Substring(4, 4) + "-" + levelCode.Substring(8, 4) + "-" + levelCode.Substring(12, 4);
                }
                return true;
            }
        }


    }
}
