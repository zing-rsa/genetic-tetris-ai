using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    public class Personality
    {
        public Bias rowsCleared         { get; set; }
        public Bias weightedHeight      { get; set; }
        public Bias cumulativeHeight    { get; set; }
        public Bias relativeHeight      { get; set; }
        public Bias holes               { get; set; }
        public Bias roughness           { get; set; }
        public Bias filledRatio         { get; set; }

    }
}
