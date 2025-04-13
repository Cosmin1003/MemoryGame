using MemoryGame.Commands;
using MemoryGame.Models;
using MemoryGame.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Text.Json;


namespace MemoryGame.ViewModels
{
    public class GameChooserVM : ViewModelBase
    {

        GameChooserWindow _gameChooserWindow = App.Current.Windows.OfType<GameChooserWindow>().FirstOrDefault();

        private User _loggedInUser;
        public User LoggedInUser
        {
            get { return _loggedInUser; }
            set { _loggedInUser = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Categories { get; set; }
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }
        private string _selectedCategory;

        public ObservableCollection<string> GameModes { get; set; }
        public string SelectedGameMode
        {
            get => _selectedGameMode;
            set
            {
                if (_selectedGameMode != value)
                {
                    _selectedGameMode = value;
                    OnPropertyChanged();

                    if (_selectedGameMode == "Standard")
                    {
                        SelectedGameSize = "4x4";
                        IsGameSizeEnabled = false;
                    }
                    else
                    {
                        IsGameSizeEnabled = true;
                    }
                }
            }
        }
        private string _selectedGameMode;

        public ObservableCollection<string> GameSizes { get; set; }
        public string SelectedGameSize
        {
            get => _selectedGameSize;
            set { _selectedGameSize = value; OnPropertyChanged(); }
        }
        private string _selectedGameSize;

        private bool _isGameSizeEnabled;
        public bool IsGameSizeEnabled
        {
            get => _isGameSizeEnabled;
            set { _isGameSizeEnabled = value; OnPropertyChanged(); }
        }


        public ICommand NewGameCommand { get; set; }
        public ICommand OpenGameCommand { get; set; }
        public ICommand StatisticsCommand { get; set; }
        public ICommand ExitCommand { get; set; }


        public GameChooserVM(User loggedInUser)
        {
            LoggedInUser = loggedInUser;

            Categories = new ObservableCollection<string> { "Food", "Animals" };
            SelectedCategory = Categories.First();

            GameModes = new ObservableCollection<string> { "Standard", "Custom" };
            SelectedGameMode = GameModes.First();

            GameSizes = new ObservableCollection<string> { "2x2", "3x3", "4x4", "5x5", "6x6" };
            SelectedGameSize = GameSizes[2];

            NewGameCommand = new RelayCommand(_ => StartNewGame());
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            StatisticsCommand = new RelayCommand(_ => OpenStatisticsWindow());
            ExitCommand = new RelayCommand(_ => ExitToLogin());

        }

        private void StartNewGame()
        {
            if (_gameChooserWindow != null)
            {
                var sizeParts = SelectedGameSize.Split('x');
                int rows = int.Parse(sizeParts[0]);
                int columns = int.Parse(sizeParts[1]);

                TimeSpan gameTime = TimeSpan.FromSeconds(60);

                var gameWindow = new GameWindow();
                gameWindow.DataContext = new GameVM(SelectedCategory, rows, columns, gameTime, LoggedInUser);

                gameWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                gameWindow.Left = _gameChooserWindow.Left;
                gameWindow.Top = _gameChooserWindow.Top;

                _gameChooserWindow.Hide();

                gameWindow.Closed += (s, e) =>
                {
                    _gameChooserWindow.Show();
                };

                gameWindow.Show();
            }
        }

        private void OpenGame()
        {
            string username = _loggedInUser.Username;
            string fileName = $"savedGame-{username}.json";
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            if (!File.Exists(savePath))
            {
                MessageBox.Show("No saved game was found for the current user.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string json = File.ReadAllText(savePath);
            GameState loadedState = JsonSerializer.Deserialize<GameState>(json);

            if (loadedState == null)
            {
                MessageBox.Show("Error loading game state.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int rows = loadedState.GridRows;
            int columns = loadedState.GridColumns;
            TimeSpan gameTime = loadedState.TimeRemaining;

            var gameWindow = new GameWindow();

            var gameVM = new GameVM(loadedState.Category, rows, columns, gameTime, LoggedInUser)
            {
                Cards = new ObservableCollection<Card>(loadedState.CardStates.Select(cardState => new Card
                {
                    Id = cardState.Id,
                    ImagePath = cardState.ImagePath,
                    IsFaceUp = cardState.IsFaceUp,
                    IsMatched = cardState.IsMatched,
                    IsPlaceholder = cardState.IsPlaceholder
                }))
            };

            gameWindow.DataContext = gameVM;
            gameWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            gameWindow.Left = _gameChooserWindow.Left;
            gameWindow.Top = _gameChooserWindow.Top;

            _gameChooserWindow.Hide();

            gameWindow.Closed += (s, e) =>
            {
                _gameChooserWindow.Show();
            };

            gameWindow.Show();
        }


        private void OpenStatisticsWindow()
        {
            if (_gameChooserWindow != null)
            {
                var statisticsWindow = new StatisticsWindow(LoggedInUser);
                statisticsWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                statisticsWindow.Left = _gameChooserWindow.Left;
                statisticsWindow.Top = _gameChooserWindow.Top;

                _gameChooserWindow.Hide();

                statisticsWindow.Closed += (s, e) =>
                {
                    _gameChooserWindow.Show();
                };

                statisticsWindow.Show();

            }
        }


        private void ExitToLogin()
        {
            if (_gameChooserWindow != null)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                loginWindow.Left = _gameChooserWindow.Left;
                loginWindow.Top = _gameChooserWindow.Top;

                loginWindow.Show();
                _gameChooserWindow.Close();
            }
        }
    }
}
