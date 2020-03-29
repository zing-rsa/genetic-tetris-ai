using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    public class Bias
    {
        public double weight { get; set; }
        public double bias { get; set; }



        public Bias(double weight, double bias)
        {
            this.weight = weight;
            this.bias = bias;
        }

        public override string ToString()
        {
            return "Weight: " + this.weight / 1000 + ", Bias: " + getBias(); 
        }

        public String getBias()
        {
            switch (this.bias){
                case -1:
                    return "Negative";
                case 1:
                    return "Positive";
                default:
                    return "Positive";

            }
        }

    }
}
