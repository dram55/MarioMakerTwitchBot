# MarioMakerTwitchBot
Take Level Requests from Twitch chat ```!submit 0000-1111-AAAA-EEEE```. Requests are only accepted while bot is in an 'Open' status. When bot is 'Closed' it randomly picks levels for you to play. 

Submission status & current level are written to respective text files which can be used within OBS. (open.txt/close.txt/nextLevel.txt)

If desired, the current queue of levels can be embedded to an html page using {details} and {date} tags and FTP'd to a webserver of your choice.


## To Use Program
 + Download zip from [here](http://dram55.com/programs)
 + Follow the Readme.txt to help set up the both authorizations, configuration, etc..



## Code
### Get Source
 + Download [Master](https://github.com/dram55/MarioMakerTwitchBot/archive/master.zip) branch. 
 
 OR
 
 
 + Use git 
1. [Fork](https://github.com/dram55/MarioMakerTwitchBot/fork)
2. Clone ```git clone https://github.com/You/MarioMakerTwitchBot.git```


### Documentation
*Not Yet*

### Building For the first time

 + On your first build the program will create an empty settings.xml & required files to the bin directory. 
 + Follow the instructions in the BotSource\Resources\ReadMe.txt and apply it to the settings.xml in your bin directory.
 + Your bot should now be able to connect.

#### Build Events
 + When BotRunner.csproj is built, a post-build event is ran and will put all relevant files to bin\Release\Publish. Use this directory to easily distribute the program. This contains an initialized settings.xml and queue.html which are copied from the BotSource\Resources directory - and do not contain any of your personal info. 
 


### Resources Used
 - [IrcDotNet](https://github.com/alexreg/IrcDotNet) - For Twitch Chat.
 - [TwitchCSharp](https://github.com/michidk/TwitchCSharp) - For Twitch API.
 - [ILMerge](http://www.microsoft.com/en-us/download/details.aspx?id=17630) - Used to consolidate binaries during the build. 
