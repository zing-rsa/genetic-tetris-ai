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


        //Personality
        public int rowsCleared { get; set; }
        public int weightedHeight { get; set; }
        public int cumulativeHeight { get; set; }
        public int relativeHeight { get; set; }
        public int holes { get; set; }
        public int roughness { get; set; }
        public int fitness { get; set; }

        public void MakeNextMove(int moveLimit, int currentRowsCleared, List<Move> allPossibleMoves)
        {
            movesTaken++;

            if (movesTaken > moveLimit)
            {

                fitness = currentRowsCleared;
                return;

            }
            else
            {
                var possibleMoves = getAllPossibleMoves();

                saveState(StateSynced);


                //Move move = getHighestRatedMove(possibleMoves);
                //The above could be replace with the following LINQ syntax:
                Move move = possibleMoves.OrderByDescending(mv => mv.rating).First();

                for (var rotations = 0; rotations < move.rotation; rotations++)
                {
                    StateSynced.PlayerInput(PlayerInput.RotateClockwise);

                }
                if (move.translation < 0)
                {
                    for (var lefts = 0; lefts < Math.Abs(move.translation); lefts++)
                    {
                        StateSynced.PlayerInput(PlayerInput.Left);

                    }
                }
                else if (move.translation > 0)
                {
                    for (var rights = 0; rights < move.translation; rights++)
                    {
                        StateSynced.PlayerInput(PlayerInput.Right);
                    }
                }

                if (inspectMoveSelection)
                {
                    moveAlgorithim = move.algorithim;
                }

                //Would need to send details to the view with algorothim behavior
            }
        }

    }
}
