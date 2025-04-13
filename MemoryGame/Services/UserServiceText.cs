using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MemoryGame.Models;

namespace MemoryGame.Services
{
    class UserServiceText
    {
        private static string filePath = "users.txt";

        public static List<User> LoadUsers()
        {
            var users = new List<User>();
            if(File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                users = lines
                    .Select(line => User.FromFileString(line))
                    .Where(user => user!=null)
                    .ToList();
            }
            return users;
        }

        public static void SaveUsers(List<User> users)
        {
            string[] lines= users.Select(u=> u.ToFileString()).ToArray();
            File.WriteAllLines(filePath, lines);
        }

        public static void UpdateUser(User updatedUser)
        {
            var users = LoadUsers();

            var existingUser = users.FirstOrDefault(u => u.Username == updatedUser.Username);
            if (existingUser != null)
            {
                existingUser.GamesPlayed = updatedUser.GamesPlayed;
                existingUser.GamesWon = updatedUser.GamesWon;
            }

            SaveUsers(users);
        }

    }
}
