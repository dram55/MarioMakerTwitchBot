How to set up this Twitch Chatbot for your chat. There's probably an easier way to implement this, but this 
is what it is for now. 

########      Before you do anything    ########
################################################
 - Unzip everything somewhere to your computer.



###########      Bot Chat Setup     ###########
###############################################

 - UserName: Your Twitch.tv username.

 - OAuthChat (to use the bot): 
		    1) Get this from here https://twitchapps.com/tmi/
			2) Paste into settings.xml - <OAuthChat>oauth:yourSamplevalue</OAuthChat>

 - BotOAuth (to access your subscribers): 
		    1) Paste this into your browser: http://goo.gl/53mMa2
			2) Click the button to authorize the app.
			3) You should be navigated to a page which displays your BotOAuth.
			4) Paste into settings.xml - <BotOAuth>yourSampleBotvalue</BotOAuth>
			
 - If you don't want the sounds to play in your chat just delete or rename the \sounds directory. 



#######      Display Queue to webpage     #######
#################################################

To use this feature you're going to need a webserver (or a friend with a webserver) with an FTP account.

In settings.xml there is a <HTMLPage> node that defaults to 'queue.html'. You must have a corresponding 
file in the same folder as the program named queue.html. In this file the program uses tags {detail} and {date}
to fill in the current queue and date. See the provided queue.html to get an idea. Customize this page to your liking.

Then you need to fill in the <FTPAddress>, <FTPUserName> & <FTPPassword> nodes that correspond to your server.

Any time you 'Close' the queue or force add a level the program will attempt to FTP queue.html to the webserver.



