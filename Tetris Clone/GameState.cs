using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Clone
{
    public struct GameState
    {
        public Piece piece { get; set; }

        public ShapeEnum[,] deadGrid;
        public bool isGameOver { get; set; }
        public bool softGameOver { get; set; }
        public bool isActive { get; set; }

        public int TotalLinesCleared { get; set; }

        public GameState(Game state)
        {
            this.piece = state.piece;
            this.deadGrid = state.deadGrid;
            this.isGameOver = state.isGameOver;
            this.softGameOver = state.softGameOver;
            this.isActive = state.isActive;
            this.TotalLinesCleared = state.TotalLinesCleared;

        }
    }
}
