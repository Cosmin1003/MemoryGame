using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Models
{
    public class GameState
    {
        public string Category { get; set; }
        public int GridRows { get; set; }
        public int GridColumns { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public List<CardState> CardStates { get; set; }
    }

    public class CardState
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsFaceUp { get; set; }
        public bool IsMatched { get; set; }
        public bool IsPlaceholder { get; set; }
    }
}
