How to set up this Twitch Chatbot for your chat. There's probably an easier way to implement this, but this 
is what it is for now. 

 - Unzip everything somewhere to your computer.
 
 - You need to do some setup to authenticate the bot into your chat & subscriber list. 
      + Open settings.xml, fill in the values with the following
	     UserName: Your Twitch.tv username.
	     OAuthChat: Get this from here https://twitchapps.com/tmi/
		 BotOAuth: 
		    1) Paste this into your browser: http://goo.gl/53mMa2
			2) Click the button to authorize the app.
			3) You should be navigated to a page which displays your BotOAuth.
			
 - If you don't want the sounds to play in your chat just delete or rename the \sounds directory. 