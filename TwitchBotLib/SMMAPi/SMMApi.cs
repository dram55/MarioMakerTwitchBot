using System;
using System.IO;
using System.Net;


namespace TwitchBotLib.SMMAPi
{


    public class SMMApi
    {
        const string MARIO_API_SITE = "http://smm.butthole.tv/";

        public static Level GetLevel(string code)
        {
            string returnJSON = APIRequest("course/", code);
            if (returnJSON != "")
            {
                Level tempLevel = new Level(returnJSON);
                return tempLevel;
            }
            else
            {
                return null; //or throw exception for level not found
            }
        }

        private static string APIRequest(string directory, string query)
        {
            string json = "";
            
            try
            {
                string requestUrl = MARIO_API_SITE + directory + query;
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(requestUrl);
                
                using (StreamReader reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
            }
            catch(WebException e)
            {
                //Console.WriteLine("Return Invavlid Level"); 404 not found
                return "";
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Invalid Request");
                return "";
            }

            return json;
        }
    }
}
