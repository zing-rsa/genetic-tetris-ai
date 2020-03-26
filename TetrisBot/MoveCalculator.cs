using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetris_Clone;

namespace TetrisBot
{
    class MoveCalculator
    {

        public List<Move> getAllPossibleMoves(GameState state)
        {
            List<int> originalPos = new List<int>();
            List<Move> possibleMoves = new List<Move>();

            //saveState(StateSynced);

            GameState orgState = new GameState(state);

            for (int rotations = 0; rotations < 4; rotations++)
            {
                for (int translations = -5; translations <= 5; translations++)
                {
                    //loadState(prevGameState);

                    //rotate the shape
                    for (int i = 0; i < rotations; i++)
                    {
                        state.CurrentPiece.RotateClockwise();
                    }

                    //move the shape
                    if (translations < 0)
                    {
                        for (int i = 0; i < Math.Abs(translations); i++)
                        {
                            gameState.PlayerInput(PlayerInput.Left);
                        }

                    }
                    else if (translations > 0)
                    {
                        for (int i = 0; i < translations; i++)
                        {
                            gameState.PlayerInput(PlayerInput.Right);
                        }
                    }
                }
            }
        }
    }
}
