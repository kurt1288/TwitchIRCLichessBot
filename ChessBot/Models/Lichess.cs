using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LichessIRCBot.Models
{
    class Lichess
    {
        private static string LichessAPIURL = "https://lichess.org/api/";
        internal static HttpClient LichessClient = new HttpClient();

        // This are "master" lists of the users and games we want to get auto updates
        internal static List<User> UserList = new List<User>();
        internal static List<string> GameList = new List<string>();
        internal static ConcurrentQueue<string> AutoQueue = new ConcurrentQueue<string>();

        internal class User
        {
            public string Username { get; set; }
            public bool Online { get; set; }
            public string Playing { get; set; } // Playing is a URL to the game currently being played
            public string Url { get; set; }
        }

        internal class Game
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Winner { get; set; }
            public string Url { get; set; }
        }

        /// <summary>
        /// Get user info (need for game url)
        /// </summary>
        internal async static Task<User> GetUser(string user)
        {
            User userInfo = null;
            string url = string.Concat(LichessAPIURL, "user/", user);
            HttpResponseMessage response = await LichessClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                userInfo = await response.Content.ReadAsAsync<User>();
            }
            return userInfo;
        }

        /// <summary>
        /// Gets game information by given id
        /// </summary>
        internal async static Task<Game> GetGame(string gameString)
        {
            Uri gameUrl = new Uri(gameString);
            string gameID = gameUrl.Segments[1].TrimEnd('/');
            Game gameInfo = null;
            string url = string.Concat(LichessAPIURL, "game/", gameID);
            HttpResponseMessage response = await LichessClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                gameInfo = await response.Content.ReadAsAsync<Game>();
            }
            return gameInfo;
        }

        internal async static Task<string> AddUser(string userString)
        {
            User userinfo = await GetUser(userString);
            if (userinfo == null) return "User Not Found";
            else
            {
                AutoQueue.Enqueue(userinfo.Url);
                UserList.Add(userinfo);
                return $"User {userinfo.Username} added to watchlist";
            }
        }

        internal static string RemoveUser(string userString)
        {
            UserList.RemoveAll(x => x.Username == userString);
            return $"Stopped watching {userString}";
        }

        internal async static Task<string> AddGame(string gameString, bool checkExist)
        {
            if (checkExist)
            {
                Game gameinfo = await GetGame(gameString);
                if (gameinfo == null) return "Game Not Found";
            }
            AutoQueue.Enqueue(gameString);
            GameList.Add(gameString);
            return "Game added to watchlist";
        }

        internal static string RemoveGame(string gameString)
        {
            GameList.Remove(gameString);
            return $"Stopped watching game {gameString}";
        }
    }
}
