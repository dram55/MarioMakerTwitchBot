using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotLib.SMMAPi
{

    interface APIParser
    {
        Queue<string> APIParseTree { get; }
        string APIDirectory { get;}
    }

    class LevelParser : APIParser
    {
        public string APIDirectory
        {
            get
            {
                return "course/";
            }
        }

        private Queue<string> apiParseTree;
        public Queue<string> APIParseTree
        {
            get
            {
                if (apiParseTree == null)
                {
                    apiParseTree = new Queue<string>();
                    apiParseTree.Enqueue("attempts");
                    apiParseTree.Enqueue("clear_rate");
                    apiParseTree.Enqueue("display_name");
                    apiParseTree.Enqueue("thumbnail");
                    apiParseTree.Enqueue("title");
                }
                return new Queue<string>(apiParseTree);
            }
        }
    }

    public class Level
    {
        public string Attempts { get;  }
        public string ClearRate { get; }
        public string Author { get; }
        public string ThumbnailURL { get; }
        public string Title { get; }

        public Level(Queue<string> properties)
        {
            Attempts = properties.Dequeue();
            ClearRate = properties.Dequeue();
            Author = properties.Dequeue();
            ThumbnailURL = properties.Dequeue();
            Title = properties.Dequeue();
        }

    }



    public class SMMApi
    {
        public static Level GetLevel(string code)
        {
            Queue<string> levelProperties = ParseAPI(new LevelParser(), code);
            if (levelProperties != null)
            {
                Level tempLevel = new Level(levelProperties);
                return tempLevel;
            }
            else
            {
                return null; //or throw exception for level not found
            }
        }

        private static Queue<string> ParseAPI(APIParser APIToParse, string query)
        {
            Queue<string> returnTree = new Queue<string>();

            try
            {
                string requestUrl = "http://smm.butthole.tv/"+ APIToParse.APIDirectory + query;
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(requestUrl);
                using (StreamReader reader = new StreamReader(stream))
                {

                    Queue<string> parseTree = APIToParse.APIParseTree;


                    while (!reader.EndOfStream && parseTree.Count > 0 )
                    {
                        string temp = reader.ReadLine();
                        string format_tag = "\"" + parseTree.Peek() + "\":";
                        if (temp.Contains(format_tag))
                        {
                            string nodeValue = temp.Replace(format_tag, "").Replace("\"", "").Trim().TrimEnd(',');
                            returnTree.Enqueue(nodeValue);
                            parseTree.Dequeue();
                            //Console.WriteLine(nodeValue);
                        }
                    }

                    if (parseTree.Count > 0 )
                    {
                        //Console.WriteLine("Return JSON unable to be parsed completely. Did not find all attributes.");
                        return null;
                    }

                }
            }
            catch(WebException e)
            {
                //Console.WriteLine("Return Invavlid Level"); 404 not found
                return null;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Invalid Request");
                return null;
            }

            return returnTree;
            

        }
    }
}
