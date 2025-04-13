using MemoryGame.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Models
{
    public class Card : ViewModelBase
    {
        public int Id { get; set; }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }

        private bool _isFaceUp;
        public bool IsFaceUp
        {
            get => _isFaceUp;
            set { _isFaceUp = value; OnPropertyChanged(); }
        }

        private bool _isMatched;
        public bool IsMatched
        {
            get => _isMatched;
            set { _isMatched = value; OnPropertyChanged(); }
        }

        private bool _isPlaceholder;
        public bool IsPlaceholder
        {
            get => _isPlaceholder;
            set { _isPlaceholder = value; OnPropertyChanged(); }
        }
    }
}
