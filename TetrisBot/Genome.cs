using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    public class Genome
    {
        public int id { get; set; }
        public int movesTaken { get; set; }
        public Personality personality { get; set; }
        public int fitness { get; set; }
        public bool played { get; set; }
    }
}
