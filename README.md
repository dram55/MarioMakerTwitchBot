# MarioMakerTwitchBot
Take Level Requests from Twitch chat ```!submit 0000-1111-AAAA-EEEE```. Requests are only accepted while bot is in an 'Open' status. When bot is 'Closed' it randomly picks levels for you to play, the rest are discarded. Mods and subs have 5x increased odds their level will be picked. Levels can also be manually force added to the queue.

Submission status & current level are written to a '\text' directory where files can be used within OBS to display on stream. (open.txt, close.txt, nextLevel.txt)

If desired, the current queue of levels can be embedded to an html page using {details} and {date} tags and FTP'd to a webserver of your choice. Viewers can refer to this page. [Example](http://dram55.com/test/queue)

The goal of this bot is to take level submissions without being overwhelmed & having a huge queue. 


## To Use Program
 + Download zip from [here](http://dram55.com/programs)
 + Follow the Readme.txt to help set up the bot authorizations, configuration, etc..

###Bot Commands
_Used in the CMD window of the program_

	*Commands for Mario Maker*
	o           - Open Submissions
	c           - Close Submissions and create a queue
	Enter Key   - Next Level
	prev        - Previous Level
	add <n> <l> - Force add level to current queue. <n>=name, <l>=level code
	q           - Display Remaining Queue
	limit 13    - When Submissions close, the bot will choose 13 levels at random
	max 3       - Maximum of 3 level submissions per person
	s <comment> - Save the current level to levels.csv with a comment
	
	*General Commands*
	v 30        - Set volume of media player to 30
	cool 65     - Set cooldown of sound commands to 65 seconds
	restart     - Restarts bot
	settings    - Change settings
	help		- display help menu
	exit        - Quit

###Twitch Chat commands
_Used in your Twitch Chat_

	*General Commands*
	!submit 1234-2222-3333-4444		- submit a Mario Maker level
	
	*Mod + Subscriber Only Commands*
	!uptime							- Displays a random uptime.
	!bfb							- Plays airhorn
	!speed							- "Speedrun bitch"
	!dik							- "speedrun my dik"
	!yeah							- Little John "yeaaaaah"
	
_To turn off sounds just delete or rename the \sounds directory_
 
## Issues
 + Please [report any issues](https://github.com/dram55/MarioMakerTwitchBot/issues/new) if you come across any. 

## Code
### Get Source
 + Download [Master](https://github.com/dram55/MarioMakerTwitchBot/archive/master.zip) branch. 
 
 OR
 
 
 + Use git 
1. [Fork](https://github.com/dram55/MarioMakerTwitchBot/fork)
2. Clone ```git clone https://github.com/You/MarioMakerTwitchBot.git```


### Documentation

	
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
