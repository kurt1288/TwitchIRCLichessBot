using System;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using TwitchLib.Services;

namespace LichessIRCBot.Models
{
    class Twitch
    {
        internal static TwitchClient client;

        internal void Connect(string botName, string oauthToken, string channel)
        {
            if (string.IsNullOrEmpty(botName) || string.IsNullOrEmpty(oauthToken) || string.IsNullOrEmpty(channel)) return;
            Console.WriteLine("Attempting to connect...");
            
            ConnectionCredentials credentials = new ConnectionCredentials(botName, oauthToken);
            client = new TwitchClient(credentials, channel, logging: false); // Change logging to true for verbose output to the console

            // Enable the throttler to both chat and whispers, so that Twitch doesn't ban the bot because of spam
            MessageThrottler ChatThrottler = new MessageThrottler(client, 10, TimeSpan.FromSeconds(30));
            MessageThrottler WhisperThrottler = new MessageThrottler(client, 10, TimeSpan.FromSeconds(30));

            // Various Twitch client events
            client.OnConnectionError += Client_OnConnectionError;
            client.OnConnected += Client_OnConnected;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnLeftChannel += Client_OnLeftChannel;
            client.OnChatCommandReceived += Client_OnCommandReceived;
            client.OnIncorrectLogin += Client_OnIncorrectLogin;

            // Connect to the server and channel
            client.Connect();
        }

        private void Client_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            Console.WriteLine($"Login failed. Please check the username and oauth token and try again.");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.Clear();
            Console.WriteLine($"{e.BotUsername} has connected.");
            Console.WriteLine("-- Press Enter to disconnect --");
        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error while connecting: {e.Error}");
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        internal void Disconnect()
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Disconnected from server.");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"Connected to channel: {e.Channel}");
            client.SendMessage(string.Concat(e.BotUsername, " has joined the channel"));
        }

        private void Client_OnLeftChannel(object sender, OnLeftChannelArgs e)
        {
            // Already left the channel, so we can't say anything in Twitch chat. Send message to app console instead.
            Console.WriteLine("Bot has left the channel");
        }

        private async void Client_OnCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            string response;
            switch (e.Command.CommandText)
            {
                case "help":
                    client.SendMessage("!watchUser [username] to follow a new user, and will notify the channel when they are online and if they start a game. !unwatchUser [username] removes a followed user. !watchGame [game_url] to watch a game and be notified when it ends. !unwatchGame [game_url] removes a game being watched.");
                    break;
                case "watchUser":
                    response = await Lichess.AddUser(e.Command.ArgumentsAsString);
                    client.SendMessage(response);
                    break;
                case "unwatchUser":
                    response = Lichess.RemoveUser(e.Command.ArgumentsAsString);
                    client.SendMessage(response);
                    break;
                case "watchGame":
                    response = await Lichess.AddGame(e.Command.ArgumentsAsString, true);
                    client.SendMessage(response);
                    break;
                case "unwatchGame":
                    response = Lichess.RemoveGame(e.Command.ArgumentsAsString);
                    client.SendMessage(response);
                    break;
                default:
                    break;
            }
        }
    }
}
