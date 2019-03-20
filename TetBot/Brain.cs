using System;
using System.Collections.Generic;
using Tetris_Clone;


namespace TetBot
{
    public class Brain
    {

        public Game gameState { get; set; }

        public Brain(Game gameState)
        {
            this.gameState = gameState;
            
        }



        public int getRowsCleared()
        {
            return this.gameState.TotalLinesCleared;
        }

        public int weightedHeight()
        {
            
            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (gameState.DeadGrid[x, y] == ShapeEnum.Filled)
                    {
                        return 20 - y;
                    }
                }
            }

            return 21;
        }

        public int cumulativeHeight()
        {
            int cumHeight = 0;

            //allows us to skip columns that we have already checked
            List<int> excludes = new List<int>();

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (gameState.DeadGrid[x, y] == ShapeEnum.Filled && !excludes.Contains(x))
                    {
                        cumHeight += 20-y;
                        excludes.Add(x);
                    }
                }
            }
            
            return cumHeight;
        }



    }
}
