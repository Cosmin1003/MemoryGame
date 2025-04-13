using MemoryGame.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.ViewModels
{
    public class StatisticsVM
    {
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public StatisticsVM()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            string filePath = "users.txt";
            var users = LoadUsersFromFile(filePath);
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private static ObservableCollection<User> LoadUsersFromFile(string filePath)
        {
            var users = new ObservableCollection<User>();

            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    var user = User.FromFileString(line);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
            }

            return users;
        }
    }
}
