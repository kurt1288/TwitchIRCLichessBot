using LichessIRCBot.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LichessIRCBot
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Enter bot username:");
            string botUsername = Console.ReadLine();
            Console.WriteLine("Enter oauth token:");
            string botOAuth = Console.ReadLine();
            Console.WriteLine("Enter channel to join:");
            string joinChannel = Console.ReadLine();

            Twitch twitch = new Twitch();
            twitch.Connect(botUsername, botOAuth, joinChannel);

            CancellationToken token = new CancellationToken();
            ProcessQueue(token);

            Console.Read();
            twitch.Disconnect();
        }

        private static async Task ProcessQueue(CancellationToken token)
        {
            // How often, in milliseconds, to check for updates to watched users/games
            int updateInterval = 5000;
            while (true)
            {
                if (Lichess.AutoQueue.Count == 0)
                {
                    await Task.Delay(updateInterval);
                    continue;
                }

                try
                {
                    Lichess.AutoQueue.TryDequeue(out string item);
                    
                    if (item.Contains("@")) // Lichess profile URLs contain @
                    {
                        Lichess.User userInfo = await Lichess.GetUser(item.Split('/').Last());
                        var userInList = Lichess.UserList.Find(x => x.Username == userInfo.Username);
                        if (userInList == null) // If null, the user has been unwatched
                        {
                            continue;
                        }
                        if (userInfo.Online != userInList.Online)
                        {
                            Twitch.client.SendMessage($"{userInfo.Username} is {(userInfo.Online ? "online" : "offline")}");
                            userInList.Online = userInfo.Online;
                        }
                        if (userInfo.Playing != null && (userInfo.Playing != userInList.Playing))
                        {
                            Twitch.client.SendMessage($"{userInfo.Username} is playing a new game: {userInfo.Playing}");
                            // Automatically add the game to watchlist, so we get notified on its result
                            await Lichess.AddGame(userInfo.Playing, false);
                            userInList.Playing = userInfo.Playing;
                        }
                    }
                    else
                    {
                        Lichess.Game game = await Lichess.GetGame(item);
                        var gameInList = Lichess.GameList.Find(x => new Uri(x).Segments[1].TrimEnd('/') == game.Id);
                        if (gameInList == null) // If null, the game has been unwatched or has ended
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(game.Winner))
                        {
                            Twitch.client.SendMessage($"Game {game.Url} has ended. The result: {(game.Winner == "white" ? "1 - 0" : "0 - 1")}");
                            Lichess.RemoveGame(gameInList);
                        }
                        else if (game.Status == "draw")
                        {
                            Twitch.client.SendMessage($"Game {game.Url} has ended. The result: 1/2 - 1/2");
                            Lichess.RemoveGame(gameInList);
                        }
                    }
                    Lichess.AutoQueue.Enqueue(item);
                    await Task.Delay(updateInterval, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error and the bot has stopped running. Please restart. Error: {ex.Message}");
                }
            }
        }
    }
}
