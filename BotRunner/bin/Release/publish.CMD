IF EXIST TwitchBot.exe DEL TwitchBot.exe
IF EXIST TwitchCSharp.pdb DEL TwitchCSharp.pdb
ilmerge twitchchatbot.exe TwitchCSharp.dll TwitchBotLib.dll /out:TwitchBot.exe /target:exe
IF EXIST twitchchatbot.exe DEL twitchchatbot.exe