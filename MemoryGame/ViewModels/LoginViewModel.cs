using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryGame.Commands;
using MemoryGame.Services;
using MemoryGame.Models;
using MemoryGame.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Text.Json;

namespace MemoryGame.ViewModels
{
    public class LoginViewModel: ViewModelBase
    {
        private Window _loginWindow;

        private List<string> _imagePaths = new List<string>
        {
            "/Images/Image1.png",
            "/Images/Image2.png",
            "/Images/Image3.png",
            "/Images/Image4.png",
            "/Images/Image5.png",
            "/Images/Image6.png",
            "/Images/Image7.png"
        };

        private int _currentImageIndex = 0;


        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { _users = value; OnPropertyChanged(); }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get { return _selectedUser; }
            set 
            {
                _selectedUser = value;
                OnPropertyChanged();

                if(_selectedUser != null)
                {
                    int index = _imagePaths.IndexOf(_selectedUser.ImagePath);
                    _currentImageIndex = index >= 0 ? index : 0;
                }
            }
        }

        public ICommand NewUserCommand { get; set; }
        public ICommand DeleteUserCommand { get; set; }
        public ICommand PreviousImageCommand { get; set; }
        public ICommand NextImageCommand { get; set; }
        public ICommand PlayCommand { get; set; }

        public LoginViewModel(Window loginWindow)
        {
            _loginWindow = loginWindow;

            var loadedUsers = UserServiceText.LoadUsers();
            Users = new ObservableCollection<User>(loadedUsers);

            NewUserCommand = new RelayCommand(_ => NewUser());
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null);
            PreviousImageCommand = new RelayCommand(_ => SwitchImage(-1), _ => SelectedUser != null);
            NextImageCommand = new RelayCommand(_ => SwitchImage(1), _ => SelectedUser != null);
            PlayCommand = new RelayCommand(_ => PlayGame(), _ => SelectedUser != null);
        }

        private void PlayGame()
        {
            var gameChooserWindow = new GameChooserWindow(SelectedUser);

            gameChooserWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            gameChooserWindow.Left = _loginWindow.Left;
            gameChooserWindow.Top = _loginWindow.Top;

            gameChooserWindow.Show();
            _loginWindow.Close();
        }

        private void SwitchImage(int direction)
        {
            _currentImageIndex = (_currentImageIndex + direction + _imagePaths.Count) % _imagePaths.Count;

            if (SelectedUser != null)
            {
                SelectedUser.ImagePath = _imagePaths[_currentImageIndex];
                OnPropertyChanged(nameof(SelectedUser));
                SaveUsers();
            }
        }

        private void NewUser()
        {
            string username = Microsoft.VisualBasic.Interaction.InputBox("Enter user name:", "New User", "Player");
            if (string.IsNullOrWhiteSpace(username))
                return;

            string imagePath = _imagePaths[0];
            var newUser = new User(username, imagePath);
            Users.Add(newUser);
            SaveUsers();
        }

        private void DeleteUser()
        {
            if (SelectedUser == null)
                return;

            string username = SelectedUser.Username;
            string fileName = $"savedGame-{username}.json";
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            if (File.Exists(savePath))
            {
                try
                {
                    File.Delete(savePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"GameState delete error: {ex.Message}");
                }
            }

            Users.Remove(SelectedUser);
            SaveUsers();
        }

        private void SaveUsers()
        {
            UserServiceText.SaveUsers(Users.ToList());
        }
    }
}
