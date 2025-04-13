using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MemoryGame.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }

        public User(string username,  string imagePath)
        {
            Username = username;
            ImagePath = imagePath;
            GamesPlayed = 0;
            GamesWon = 0;
        }

        public override string ToString() => Username;

        public string ToFileString()
        {
            return $"{Username}|{ImagePath}|{GamesPlayed}|{GamesWon}";
        }

        public static User FromFileString(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 2)
                return null;

            var user = new User (parts[0], parts[1]);
            if(parts.Length > 2)
            {
                int.TryParse(parts[2], out int gamesPlayed);
                int.TryParse(parts[3], out int gamesWon);
                user.GamesPlayed = gamesPlayed;
                user.GamesWon = gamesWon;
            }
            return user;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
