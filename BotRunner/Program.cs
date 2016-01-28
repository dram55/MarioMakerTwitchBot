[assembly: log4net.Config.XmlConfigurator(Watch=true)]

namespace BotRunner
{
    class Program
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            logger.Debug("Starting Bot...");
            
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
