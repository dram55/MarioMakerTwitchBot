namespace BotRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitchBotLib.BotSettings.LoadSettings();
            TwitchBotLib.BotMain f = new TwitchBotLib.BotMain();
            f.StartBot();
        }
    }
}
