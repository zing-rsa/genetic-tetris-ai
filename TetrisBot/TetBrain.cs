using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Tetris_Clone;
using Newtonsoft.Json;

//Need to build the fitness function and evolve 

namespace TetrisBot
{
    public class TetBrain
    {
        #region Properties

        public bool inspectMoveSelection { get; set; }
        public bool isGameover { get; set; }
        
        public int populationSize { get; set; }
        public List<Genome> genomes { get; set; }
        public int currentGenome { get; set; }

        public Game gameState {get;set;} 
        public Game prevGameState { get; set; }
        public Game StateSynced { get; set; }
        public int movesTaken { get; set; }
        public int moveLimit { get; set; }

        public Algorithim moveAlgorithim { get; set; }

        #endregion


        public TetBrain(int popSize = 50)
        {
            this.moveLimit = 500;
            this.genomes = new List<Genome>();

            this.populationSize = popSize;

            //gameState.GameOver += game_GameOver;//This might need some work

            CreateIntialPopulation();
        }

        public TetBrain(Game gameState, int  popSize = 50)
        {

            this.moveLimit = 500;
            this.genomes = new List<Genome>();

            this.StateSynced = gameState;

            loadState(StateSynced);
             
            this.populationSize = popSize;

            //gameState.GameOver += game_GameOver;//This might need some work

            CreateIntialPopulation();
        }

        //private void game_GameOver(object sender, EventArgs e)
        //{
        //    this.isGameover = true;
        //}

        private void CreateIntialPopulation()
        {
            Random rnd = new Random();

            for (int i = 0; i < populationSize; i++)
            {
                //creating the first gen of genomes with completely random weights to indicate the 
                //importance of each property in the decision making proccess
                Genome newG = new Genome() {

                    id               = rnd.Next(0, 1000),
                    rowsCleared      = rnd.Next(0, 1000) - 2,
                    weightedHeight   = rnd.Next(0, 1000) - 2,
                    cumulativeHeight = rnd.Next(0, 1000) - 2,
                    relativeHeight   = rnd.Next(0, 1000) - 2,
                    holes            = rnd.Next(0, 1000) * 2,
                    roughness        = rnd.Next(0, 1000) - 2,

                };

                genomes.Add(newG);
                
            }
            //
            //put this in the void for handling the game started event.
        }

        public void StartAI(object sender, EventArgs e)
        {
            evaluateNextGenome();
        }

        public void evaluateNextGenome()
        {
            currentGenome++;

            if (currentGenome == populationSize)
            {
                currentGenome = 0;
                //evolve();
            }

            movesTaken = 0;
            MakeNextMove();
        }


        public void MakeNextMove()
        {
            movesTaken++;

            if (movesTaken > moveLimit)
            {
                
                genomes[currentGenome].fitness = getRowsCleared();
                evaluateNextGenome();

            }
            else
            {
                var possibleMoves = getAllPossibleMoves();

                saveState(StateSynced);

                Move move = getHighestRatedMove(possibleMoves);
                
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

        public void hitBottom(object sender, EventArgs e)
        {
            //Need to add MakenextMove call here, but cant because the if 
            //statement never returns true. The reason is that the StateSynced
            //objects params are already reset by its own CheckForDeactivations() method by the time it gets here

            MakeNextMove();
            
        }

        private Move getHighestRatedMove(List<Move> moves)
        {

            int maxRating = -100000000;
            int maxMove = -1;
            List<int> ties = new List<int>();

            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].rating > maxRating)
                {
                    maxRating = moves[i].rating;
                    maxMove = i;

                    ties = new List<int>() { i };//NB find out what this does
                }
                else if (moves[i].rating == maxRating)
                {
                    ties.Add(i);
                }
            }

            Move move = moves[ties[0]];
            move.algorithim.tieCount = ties.Count;
            return move;
            
        }

        /// <summary>
        /// Run through all the possible moves that can be made in a given situation
        /// </summary>
        /// <returns>
        /// A list of Move objects which all have corresponding ratings
        /// </returns>
        public List<Move> getAllPossibleMoves()
        {
            this.isGameover = false;

            List<Move> possibleMoves = new List<Move>();

            saveState(StateSynced);

            for (int rotations = 0; rotations < 4; rotations++)
            {
                List<int> originalPos = new List<int>();

                for (int translations = -5; translations <= 5; translations++)
                {
                    loadState(prevGameState);

                    //rotate the shape
                    for (int i = 0; i < rotations; i++)
                    {
                        gameState.CurrentPiece.RotateClockwise();
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

                    if (!originalPos.Contains(gameState.CurrentPiece.PositionX))
                    {
                        Result MoveDownResult = moveToBottom();

                        Algorithim alg = new Algorithim
                        {
                            rowsCleared = MoveDownResult.rowsCleared * genomes[currentGenome].rowsCleared,
                            weightedHeight = getWeightedHeight(gameState) * genomes[currentGenome].weightedHeight,
                            cumulativeHeight = getCumulativeHeight(gameState) * genomes[currentGenome].cumulativeHeight,
                            relativeHeight = getRealtiveHeight(gameState) * genomes[currentGenome].relativeHeight,
                            holes = getHoles(gameState) * genomes[currentGenome].holes,
                            roughness = getRoughness(gameState) * genomes[currentGenome].roughness
                        };

                        int rating = 0;

                        rating += alg.rowsCleared * genomes[currentGenome].rowsCleared;
                        rating += alg.weightedHeight * genomes[currentGenome].weightedHeight;
                        rating += alg.cumulativeHeight * genomes[currentGenome].cumulativeHeight;
                        rating += alg.relativeHeight * genomes[currentGenome].relativeHeight;
                        rating += alg.holes * genomes[currentGenome].holes;
                        rating += alg.roughness * genomes[currentGenome].roughness;

                        if(this.isGameover == true)
                        {
                            rating -= 500;
                        }

                        possibleMoves.Add(new Move()
                        {
                            rotation = rotations,
                            translation = translations,
                            rating = rating,
                            algorithim = alg
                        });
                        
                    }
                }
            }

            loadState(StateSynced);

            return possibleMoves;
        }

        public Result moveToBottom()
        {
            //problem is here
            Result result = new Result { lose = false, moved = true, rowsCleared = 0};
            
            while (!gameState.CheckForDeactivations())
            {
                gameState.PlayerInput(PlayerInput.Down);
            }

            result.rowsCleared = getRowsCleared();

            return result;

        }

        /// <summary>
        /// Creates a deep copy of a Game object
        /// </summary>
        public void loadState(Game game)
        {
            gameState = game.Clone(game);
            gameState.isActive = false;
        }

        public void saveState(Game game)
        {
            prevGameState = game.Clone(game); 
            prevGameState.isActive = false;
        }

        //get the rows that have been cleared so far in the game
        public int getRowsCleared()
        {
            return this.StateSynced.TotalLinesCleared;
        }

        //Returns a list of all of the heights of each of the columns, which is inherently 
        //sorted because of the way the algorithim reads the heights
        public List<int> getAllHeights(Game State)
        {

            List<int> heights = new List<int>();
            List<int> excludes = new List<int>();

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (State.DeadGrid[x, y] == ShapeEnum.Filled && !excludes.Contains(x))
                    {
                        heights.Add(20 - y);
                        excludes.Add(x);
                    }
                }
            }

            //Padding the end of the list with the zeros 
            //for the columns which did not have heights
            while (heights.Count < 10)
            {
                heights.Add(0);
            }

            return heights;
        }

        //Get the heights in a list in the order that 
        //they appear on the screen from left to right.
        public List<int> getAllHeightsInOrder(Game State)
        {
            List<int> heights = new List<int>();

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y <= 20; y++)
                {
                    //If on the last line in the column, add a zero because we know that the column is empty
                    //continue to the next iteration
                    if(y == 20)
                    {
                        heights.Add(0);
                        continue;
                    }

                    //Check if the block in the column is filled
                    if (State.DeadGrid[x, y] == ShapeEnum.Filled)
                    {
                        heights.Add(20 - y);
                        break;
                    }
                }
            }
            
            return heights;
        }


        //Get the height of the highest column in the matrix
        public int getWeightedHeight(Game State)
        {

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (State.DeadGrid[x, y] == ShapeEnum.Filled)
                    {
                        return 20 - y;
                    }
                }
            }

            return 0;
        }

        //get the summ of all the heights of the columns
        public int getCumulativeHeight(Game State)
        {
            List<int> allHeights = getAllHeights(State);
            
            return allHeights.Sum();
        }

        //get the relative of the highest column vs the lowest(ie. highest - lowest)
        public int getRealtiveHeight(Game State)
        {
            List<int> heights = getAllHeights(State);

            //List is inherently sorted, just take first and last val
            return heights[0] - heights[9];
        }

        //Get all the holes in the matrix. ie. empty blocks that have filled blocks above them.
        public int getHoles(Game State)
        {
            int totalHoles = 0;
            for (int y = 1; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    //Check if there is a block that has a filled block above it. 
                    if (State.DeadGrid[x, y] == ShapeEnum.Empty && State.DeadGrid[x,y-1] == ShapeEnum.Filled)
                    {
                        //if there is, add a hole
                        totalHoles++;

                        //Must check for other blocks underneath that block, which might also now be part of bigger holes
                        for (int i = 1; i < 20-y; i++)
                        {
                            //if there it finds a filled block, break from the loop as that will be the last piece in the hole
                            if (State.DeadGrid[x, y + i] == ShapeEnum.Filled)
                            {
                                break;

                            }
                            //otherwise, keep going down and adding to holes 
                            else if (State.DeadGrid[x, y + i] == ShapeEnum.Empty)
                            {
                                totalHoles++;
                            }
                        }
                    }
                }
            }

            return totalHoles;
            
        }

        //The sum of all the absolute differences between columns
        //This returns the same as the relative height for now. 
        //Will see what needs to happen to change this.
        public int getRoughness(Game State)
        {
            int roughness = 0;
            List<int> heights = getAllHeights(State);

            for(int i = 0; i < heights.Count-1; i++)
            {
                roughness += Math.Abs(heights[i] - heights[i + 1]);
            }
            
            return roughness;
        }

        public void SyncAllStates(Game game)
        {
            this.gameState = game.Clone(game);
            this.prevGameState = game.Clone(game);
            this.StateSynced = game;
        }

    }
}
