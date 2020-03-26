using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetris_Clone;
using TetrisBot;

namespace TetrisBot
{
    public class PopulationManager
    {
        #region Properties

        public int populationSize { get; set; }
        public List<Genome> genomes { get; set; }

        public int currentGenomeIndex = -1;
        public Genome currentGenome { get; set; }
        public int moveLimit { get; set; }

        #endregion

        #region Constructors

        public PopulationManager(int popSize = 50)
        {
            this.moveLimit = 500;
            this.genomes = new List<Genome>();

            this.populationSize = popSize;

            CreateIntialPopulation();
        }

        public PopulationManager(GameState gameState, int popSize = 50)
        {
            this.moveLimit = 500;
            this.genomes = new List<Genome>();

            //this.StateSynced = gameState;

            //loadState(StateSynced);

            this.populationSize = popSize;

            CreateIntialPopulation();

        }

        #endregion

        #region Init
        private void CreateIntialPopulation()
        {
            Random rnd = new Random();

            for (int i = 0; i < populationSize; i++)
            {
                // Creating the first gen of genomes with completely random weights.
                // Weights influence the importance a genome places on each property in the decision making proccess
                Genome newG = new Genome()
                {

                    //id = i,
                    //rowsCleared = rnd.Next(0, 1000) - 2,
                    //weightedHeight = rnd.Next(0, 1000) - 2,
                    //cumulativeHeight = rnd.Next(0, 1000) - 2,
                    //relativeHeight = rnd.Next(0, 1000) - 2,
                    //holes = rnd.Next(0, 1000) * 2,
                    //roughness = rnd.Next(0, 1000) - 2,

                    id = i,
                    rowsCleared = rnd.Next(0, 1000),
                    weightedHeight = rnd.Next(0, 1000),
                    cumulativeHeight = rnd.Next(0, 1000),
                    relativeHeight = rnd.Next(0, 1000),
                    holes = rnd.Next(0, 1000),
                    roughness = rnd.Next(0, 1000)

                };

                genomes.Add(newG);

            }
        }

        public void readyForEvaluation()
        {
            evaluateNextGenome();
        }

        #endregion

        public void evaluateNextGenome()
        {
            currentGenomeIndex++;

            if (currentGenomeIndex == populationSize)
            {
                currentGenomeIndex = 0;
                //evolve();
            }
            currentGenome = genomes[currentGenomeIndex];

            currentGenome.movesTaken = 0;
            //MakeNextMove();
        }

        public void nextMoveRequired(object sender, Game arg)
        {
            MakeNextMove(arg);
        }

        public void MakeNextMove(Game currentGameRef)
        {
            currentGenome.movesTaken++;

            

            if (currentGenome.movesTaken > moveLimit)
            {
                currentGenome.fitness = currentGameRef.TotalLinesCleared;
                evaluateNextGenome();
            }
            else
            {
                var possibleMoves = getAllPossibleMoves(currentGameRef);

                //GameState orgState = new GameState(currentGameRef);

                Move move = possibleMoves.OrderByDescending(mv => mv.rating).First();

                for (var rotations = 0; rotations < move.rotation; rotations++)
                {
                    currentGameRef.PlayerInput(PlayerInput.RotateClockwise);

                }
                if (move.translation < 0)
                {
                    for (var lefts = 0; lefts < Math.Abs(move.translation); lefts++)
                    {
                        currentGameRef.PlayerInput(PlayerInput.Left);

                    }
                }
                else if (move.translation > 0)
                {
                    for (var rights = 0; rights < move.translation; rights++)
                    {
                        currentGameRef.PlayerInput(PlayerInput.Right);
                    }
                }

                //if (inspectMoveSelection)
                //{
                //    moveAlgorithim = move.algorithim;
                //}

                //Would need to send details to the view with algorothim behavior
            }
        }

        /// <summary>
        /// Run through all the possible moves that can be made in a given situation
        /// </summary>
        /// <returns>
        /// A list of Move objects which all have corresponding ratings
        /// </returns>
        public List<Move> getAllPossibleMoves(Game currentGameRef)
        {
            //this.isGameover = false;

            List<Move> possibleMoves = new List<Move>();

            GameState orgState = new GameState(currentGameRef);

            for (int rotations = 0; rotations < 4; rotations++)
            {
                List<int> originalPos = new List<int>();

                for (int translations = -5; translations <= 5; translations++)
                {
                    loadState(orgState);

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

                        Personality alg = new Personality
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

                        if (this.isGameover == true)
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
            Result result = new Result { lose = false, moved = true, rowsCleared = 0 };

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
        public Game loadState(GameState game)
        {
            gameState = game.Clone(game);
            gameState.isActive = false;
        }

        public void saveState(Game game)
        {
            prevGameState = game.Clone(game);
            prevGameState.isActive = false;
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
                    if (y == 20)
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
                    if (State.DeadGrid[x, y] == ShapeEnum.Empty && State.DeadGrid[x, y - 1] == ShapeEnum.Filled)
                    {
                        //if there is, add a hole
                        totalHoles++;

                        //Must check for other blocks underneath that block, which might also now be part of bigger holes
                        for (int i = 1; i < 20 - y; i++)
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

            for (int i = 0; i < heights.Count - 1; i++)
            {
                roughness += Math.Abs(heights[i] - heights[i + 1]);
            }

            return roughness;
        }



    }
}
