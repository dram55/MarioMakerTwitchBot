using System;
using System.Collections.Generic;
using System.Linq;
using TwitchCSharp.Models;
using TwitchCSharp.Clients;
using TwitchCSharp.Helpers;

namespace TwitchBotLib
{
    class TwitchAPI
    {

        private TwitchAuthenticatedClient twitchClient;
        private bool connected;

        private List<string> _subscribers;
        public List<string> Subscribers
        {
            get {
                if (_subscribers!=null && connected)
                {
                    UpdateSubscribers();
                    return _subscribers;
                }
                else return new List<string>();
            }
        }
        

        public TwitchAPI(string botAuth, string botClientID)
        {
            _subscribers = new List<string>();
            connected = false;

            try
            {
                //TYPE THIS INTO BROWSER TO RECEIVE OAUTH FOR BOT + USER
                //string uri = "https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=fjhkjex3dosfwql6jcne4klacgixv80&redirect_uri=http://dram55.com/bot&scope=user_read+channel_subscriptions+channel_check_subscription";
                twitchClient = new TwitchAuthenticatedClient(botAuth, botClientID);
                connected = true;
            }
            catch (Exception)
            {
                Console.WriteLine("WARNING: Unable to retrieve subs list. Invalid BotOAuth in Settings.");
                Console.WriteLine("Update settings and restart bot if you'd like to connect to the API. ");
                Console.WriteLine();
            }

        }

        private void UpdateSubscribers()
        {
            PagingInfo p = new PagingInfo();
            p.PageSize = 100;

            _subscribers.Clear();
            TwitchList<Subscription> temp = new TwitchList<Subscription>();

            //Twitch only allows requests for 100 subscribers at a time. 
            //Need to keep incrementing pages until you got all the subs.
            do
            {
                temp = twitchClient.GetSubscribers(pagingInfo: p);
                _subscribers.AddRange(temp.List.Select(t => t.User.DisplayName.ToLower()));
                p.Page++;
            }
            while (temp.Total > _subscribers.Count);
        }
    }
}
