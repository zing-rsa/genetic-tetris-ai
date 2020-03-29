using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    public class Move
    {
        public int rotation { get; set; }
        public int translation { get; set; }
        public int rating { get; set; }
        public GameStats gameStats { get; set; }
        

    }
}
