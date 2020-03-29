using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    public class GameStats
    {
        public int rowsCleared { get; set; }
        public int weightedHeight { get; set; }
        public int cumulativeHeight { get; set; }
        public int relativeHeight { get; set; }
        public int holes { get; set; }
        public int roughness { get; set; }

    }
}
