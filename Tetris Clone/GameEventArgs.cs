using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Clone
{
    public class GameEventArgs : EventArgs
    {
        public Game arg;
        public GameEventArgs(Game game)
        {
            this.arg = game;
        }

    }
}
