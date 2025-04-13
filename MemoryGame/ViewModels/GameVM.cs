using MemoryGame.Commands;
using MemoryGame.Models;
using MemoryGame.Services;
using MemoryGame.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;


namespace MemoryGame.ViewModels
{
    public class GameVM : ViewModelBase
    {
        public ObservableCollection<Card> Cards { get; set; }

        private int _gridRows;
        public int GridRows
        {
            get => _gridRows;
            set { _gridRows = value; OnPropertyChanged(); }
        }

        private int _gridColumns;
        public int GridColumns
        {
            get => _gridColumns;
            set { _gridColumns = value; OnPropertyChanged(); }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }


        private DispatcherTimer _gameTimer;
        private TimeSpan _timeRemaining;
        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set { _timeRemaining = value; OnPropertyChanged(); }
        }

        public ICommand CardSelectedCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand SaveGameCommand { get; set; }

        private Card _firstFlippedCard;
        private Card _secondFlippedCard;
        private bool _isProcessing;

        private User _loggedinUser;
        public GameVM(string category, int gridRows, int gridColumns, TimeSpan gameTime, User loggedinUser)
        {
            Category = category;
            GridRows = gridRows;
            GridColumns = gridColumns;
            _loggedinUser = loggedinUser;

            Cards = new ObservableCollection<Card>();
            InitializeCards(category, gridRows, gridColumns);

            TimeRemaining = gameTime;
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();

            CardSelectedCommand = new RelayCommand(param => FlipCard(param as Card), param => !_isProcessing);
            SaveGameCommand = new RelayCommand(_ => SaveGame(), _ => true);
            ExitCommand = new RelayCommand(param => ExitGame());

            _loggedinUser = loggedinUser;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (TimeRemaining.TotalSeconds > 1)
            {
                TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                _gameTimer.Stop();
                TimeRemaining = TimeSpan.Zero;
                
                EndGame(won: false);
            }
        }

        private void InitializeCards(string category, int rows, int columns)
        {
            int totalCells = rows * columns;
            bool hasPlaceholder = totalCells % 2 != 0;
            int totalPairs = hasPlaceholder ? (totalCells - 1) / 2 : totalCells / 2;

            List<string> imagePaths = GetImagePathsForCategory(category, totalPairs);

            var cardList = new List<Card>();
            int id = 1;
            foreach (var image in imagePaths)
            {
                cardList.Add(new Card
                {
                    Id = id,
                    ImagePath = image,
                    IsFaceUp = false,
                    IsMatched = false,
                    IsPlaceholder = false
                });
                cardList.Add(new Card
                {
                    Id = id,
                    ImagePath = image,
                    IsFaceUp = false,
                    IsMatched = false,
                    IsPlaceholder = false
                });
                id++;
            }

            var rnd = new Random();
            var shuffledCards = cardList.OrderBy(card => rnd.Next()).ToList();

            if (hasPlaceholder)
            {
                var placeholderCard = new Card
                {
                    IsPlaceholder = true,
                    IsFaceUp = false,
                    IsMatched = true,
                    ImagePath = string.Empty
                };

                Cards = new ObservableCollection<Card>();

                int centerIndex = (rows / 2) * columns + (columns / 2);
                int currentIndex = 0;
                int cardIndex = 0;

                while (currentIndex < totalCells)
                {
                    if (currentIndex == centerIndex)
                    {
                        Cards.Add(placeholderCard);
                    }
                    else
                    {
                        Cards.Add(shuffledCards[cardIndex]);
                        cardIndex++;
                    }
                    currentIndex++;
                }
            }
            else
            {
                Cards = new ObservableCollection<Card>(shuffledCards);
            }
        }

        private List<string> GetImagePathsForCategory(string category, int numberOfPairs)
        {
            List<string> images = new List<string>();
            for (int i = 1; i <= numberOfPairs; i++)
            {
                if(category == "Animals")
                    images.Add($"/Images/GameImages/animals{i}.jpg");
                else
                    images.Add($"/Images/GameImages/food{i}.png");
            }
            return images;
        }

        private async void FlipCard(Card card)
        {
            if (card == null || card.IsFaceUp || card.IsMatched || card.IsPlaceholder || _isProcessing)
                return;

            _isProcessing = true;

            card.IsFaceUp = true;

            if (_firstFlippedCard == null)
            {
                _firstFlippedCard = card;
                _isProcessing = false;
            }
            else if (_secondFlippedCard == null)
            {
                _secondFlippedCard = card;

                if (_firstFlippedCard.Id == _secondFlippedCard.Id)
                {
                    _firstFlippedCard.IsMatched = true;
                    _secondFlippedCard.IsMatched = true;
                    if (Cards.All(c => c.IsMatched || false))
                    {
                        _gameTimer.Stop();
                        EndGame(won: true);
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    _firstFlippedCard.IsFaceUp = false;
                    _secondFlippedCard.IsFaceUp = false;
                }

                _firstFlippedCard = null;
                _secondFlippedCard = null;
                _isProcessing = false;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void EndGame(bool won)
        {
            _loggedinUser.GamesPlayed++;

            if (won)
            {
                _loggedinUser.GamesWon++;
            }

            UserServiceText.UpdateUser(_loggedinUser);

            MessageBox.Show(won ? "You won!" : "Time's up!");
            ExitGame();
        }

        private void SaveGame()
        {
            string username = _loggedinUser.Username;

            var gameState = new GameState
            {
                Category = this.Category,
                GridRows = this.GridRows,
                GridColumns = this.GridColumns,
                TimeRemaining = this.TimeRemaining,
                CardStates = Cards.Select(card => new CardState
                {
                    Id = card.Id,
                    ImagePath = card.ImagePath,
                    IsFaceUp = card.IsFaceUp,
                    IsMatched = card.IsMatched,
                    IsPlaceholder = card.IsPlaceholder
                }).ToList()
            };

            string fileName = $"savedGame-{username}.json";
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            string json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(savePath, json);

            _gameTimer.Stop();

            MessageBox.Show("The game was saved successfully!", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            ExitGame();
        }

        private void ExitGame()
        {
            _gameTimer.Stop();

            var gameWindow = Application.Current.Windows.OfType<GameWindow>().FirstOrDefault();
            if (gameWindow != null)
            {
                gameWindow.Close();
            }
        }

    }
}
