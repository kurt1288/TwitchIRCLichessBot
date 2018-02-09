# TwitchIRCLichessBot
A Twitch IRC bot that sends updates on Lichess users and games to a Twitch channel.

## How to Use
First, you'll need a Twitch account and OAuth key. You can create a separate Twitch account just for this bot if you want. To get the OAuth key, go to this site: https://twitchapps.com/tmi/

Once you have done that, download the app. You have two options:
1. Download the *LichessIRCBot.zip* file [here](https://github.com/kurt1288/TwitchIRCLichessBot/releases). Extract the zip file anywhere you want, and then run the LichessIRCBot.exe file.
2. Download the git repository and compile the program yourself (you'll need Visual Studio 2017).

When you run the app you will be prompted to enter the Twitch username, OAuth key, and the channel you want the bot to join. If there are any issues connecting, you can simple close the app and run it again.

## Commands
`!watchUser [username]` - Where 'username' is the Lichess username. This notifies the channel when the user has come online, gone offline, or started playing a new game

`!unwatchUser [username]` - The channel will no longer be notified about the Lichess user.

`!watchGame [Lichess game URL]` - Where 'Lichess game URL' is the full URL for the game. Notifies the channel when the game ends and the result. Note, when a User that is being watch starts a new game, that game is automatically watched.

`!unwatchGame [Lichess game URL]` - The channel will no longer be notified when the game ends. Note, when a watched game ends, it is automatically unwatched.

`!help` - Tells the channel the available commands.

## Other
Currently, login information and watched users/games are not saved. When the app is closed, that information will need to be re-entered.
