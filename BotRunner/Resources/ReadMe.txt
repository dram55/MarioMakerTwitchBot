########      Before you do anything    ########
################################################
 - Unzip everything somewhere to your computer.



###########      Bot Chat Setup     ###########
###############################################

1) Open the program.
2) The default settings will not allow you to connect to IRC and the settings helper will open.
3) Put in all settings, (use the 'GET' buttons for help).
4) FTP stuff is optional. 
5) Use open.txt | close.txt | nextLevel.txt in OBS to put stuff on your stream.



#######      Display Queue to webpage     #######
#################################################

To use this feature you're going to need a webserver (or a friend with a webserver) with an FTP account.

In settings.xml there is a <HTMLPage> node that defaults to 'queue.html'. You must have a corresponding 
file in the same folder as the program named 'queue.html'. In this file the program uses tags {detail} and {date}
to fill in the current queue and date. See the provided queue.html to get an idea. Customize this page to your liking.

Then you need to fill in the <FTPAddress>, <FTPUserName> & <FTPPassword> nodes that correspond to your server.

Any time you 'Close' the queue or force add a level the program will attempt to FTP queue.html to the webserver.



#########      How the bot works     ##########
###############################################

1) open the queue.
2) people use !submit 1234-1234-1234-1234
3) close the queue
4) the bot randomly picks 10 levels.
5) Repeat.