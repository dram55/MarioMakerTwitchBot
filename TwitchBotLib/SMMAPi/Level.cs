using System;
using Newtonsoft.Json.Linq;

namespace TwitchBotLib.SMMAPi
{
    /// <summary>
    /// Public facing API object for a SMM level
    /// </summary>
    public class Level
    {
        public string Attempts { get; }
        public decimal ClearRate { get; }
        public string Author { get; }
        public string ThumbnailURL { get; }
        public string Title { get; }

        public Level(string json)
        {
            try
            {
                JToken response = JObject.Parse(json);
                Attempts = (string)response.SelectToken("attempts");
                ClearRate = (decimal)response.SelectToken("clear_rate");
                Author = (string)(response.SelectToken("creator").SelectToken("display_name"));
                ThumbnailURL = (string)response.SelectToken("thumbnail");
                Title = (string)response.SelectToken("title");
            }
            catch (Exception e)
            {
                throw new ArgumentException();
            }
        }

    }
}
