namespace BotRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitchBotLib.BotSettings.Load();
            TwitchBotLib.BotMain f = new TwitchBotLib.BotMain();
            f.StartBot();

            while (!f.IsExit)
            {

                if (f.Restart)
                {
                    f = new TwitchBotLib.BotMain();
                    f.StartBot();
                }

            }
           
        }
    }
}
