using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace TwitchBotLib
{
    public class LevelSubmitter
    {
        public bool Open { get; private set; }
        public int LevelLimit { get; set; }
        public List<Tuple<string, string>> AllLevels { get; private set; }

        string OPEN_TEXT_FILE = BotSettings.RootDirectory + "text\\open.txt";
        string CLOSE_TEXT_FILE = BotSettings.RootDirectory + "text\\close.txt";
        string NEXT_LEVEL_FILE = BotSettings.RootDirectory + "text\\nextLevel.txt";

        public string CurrentLevel
        {
            get {
                if (_finalLevels.Count > 0)
                    return _finalLevels.First.Value.Item2 + ", " + _finalLevels.First.Value.Item1;
                else
                    return String.Empty;
            }

        }
        private LinkedList<Tuple<string, string>> _finalLevels;
        public LinkedList<Tuple<string, string>> FinalLevels
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
            AllLevels = new List<Tuple<string, string>>();
            _finalLevels = new LinkedList<Tuple<string, string>>();
            LevelLimit = 10;
            CreateDirectories();
        }

        private void CreateDirectories()
        {
            if (!Directory.Exists("text"))
                Directory.CreateDirectory("text");
        }

        public void NextLevel()
        {
            _finalLevels.RemoveFirst();

            if (_finalLevels.Count <= 0)
                return;

            displayNextLevel();
        }

        private void displayNextLevel()
        {
            if (_finalLevels.Count > 0)
            {
                using (StreamWriter file = new StreamWriter(NEXT_LEVEL_FILE, false))
                {
                    var currentLevel = _finalLevels.First.Value;
                    file.WriteLine(currentLevel.Item2);
                    file.WriteLine(currentLevel.Item1);
                    file.WriteLine("Levels: " + _finalLevels.Count);
                }
            }

        }

        public void OpenQueue()
        {
            AllLevels = new List<Tuple<string, string>>();
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
            _finalLevels = new LinkedList<Tuple<string, string>>();
            randomlyPickLevels();
            displayCloseQueueText();
            displayNextLevel();
            AllLevels = new List<Tuple<string, string>>();
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

        public void AddLevel(string levelCode, string submitter, int timesToEnter)
        {
            if (AllLevels.Any(t => t.Item1 == levelCode))
                return;
            for (int i = 1; i <= timesToEnter; i++)
            {
                AllLevels.Add(new Tuple<string, string>(levelCode, submitter));
            }
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
                Random r = new Random();
                int rand = r.Next(0, AllLevels.Count);
                Tuple<string,string> randomlySelectedLevel = AllLevels[rand];
                _finalLevels.AddLast(randomlySelectedLevel);
                AllLevels.RemoveAll(t => t.Item1 == randomlySelectedLevel.Item1);
                if (_finalLevels.Count(t => t.Item2 == randomlySelectedLevel.Item2) >= BotSettings.MaxSubmissionsForSingleUser)
                    AllLevels.RemoveAll(t => t.Item2 == randomlySelectedLevel.Item2);
                counter--;
            }
        }


        public void ForceAddLevel(string levelCode,string submitter)
        {
            _finalLevels.AddLast(new Tuple<string, string>(levelCode, submitter));
            if (_finalLevels.Count == 1)
                displayNextLevel();
        }




        public static bool IsValidLevelCode(string levelCode)
        {
            //Capture hex in the format 0000-FFFF-EEEE-DDDD OR 0000FFFFEEEEDDDD (0000-FFFF-EEEEDDDD would work too.)
            Regex r = new Regex("^([0-9A-Fa-f]{4}-?){3}[0-9A-Fa-f]{4}$");
            if (r.IsMatch(levelCode)) return true;
            else return false;
        }


    }
}
