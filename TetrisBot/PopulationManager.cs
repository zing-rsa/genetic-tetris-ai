using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public double mutationRate { get; set; }
        List<int> biases = new List<int>() { -1, 1 };



        public List<Genome> genomes { get; set; }
        public int currentGenomeIndex = -1;
        public Genome currentGenome { get; set; }

        public int currentGeneration { get; set; }

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
                    personality = new Personality
                    {
                        rowsCleared = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)]),
                        weightedHeight = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)]),
                        cumulativeHeight = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)]),
                        relativeHeight = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)]),
                        holes = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)]),
                        roughness = new Bias(rnd.Next(0, 1000), biases[rnd.Next(0, 2)])
                    }
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

            if (currentGenomeIndex == this.populationSize)
            {
                currentGenomeIndex = 0;
                evolve();
            }
            currentGenome = genomes[currentGenomeIndex];

            currentGenome.movesTaken = 0;
            //MakeNextMove();
        }

        public void evolve()
        {
            var evolutionChance = 0.5;
            Random rand = new Random();

            List<Genome> nextGeneration = new List<Genome>();

            var topTen = genomes.OrderByDescending((g) => g.fitness).Skip(genomes.Count-10).Take(10).ToList();

            

            foreach (Genome g in topTen)
            {
                nextGeneration.Add(g);

                for (int i = 0; i < 4; i++)
                {
                    Genome child = new Genome()
                    {
                        personality = new Personality()
                        {
                            rowsCleared = new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.rowsCleared.weight * mutationRate : g.personality.rowsCleared.weight, g.personality.rowsCleared.bias),
                            weightedHeight= new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.weightedHeight.weight * mutationRate : g.personality.weightedHeight.weight, g.personality.weightedHeight.bias),
                            cumulativeHeight= new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.cumulativeHeight.weight * mutationRate : g.personality.cumulativeHeight.weight, g.personality.cumulativeHeight.bias),
                            relativeHeight= new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.relativeHeight.weight * mutationRate : g.personality.relativeHeight.weight, g.personality.relativeHeight.bias),
                            holes= new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.holes.weight * mutationRate : g.personality.holes.weight, g.personality.holes.bias),
                            roughness= new Bias(rand.Next(1, Convert.ToInt32(1.0 / evolutionChance) + 1) == 1 ? g.personality.roughness.weight * mutationRate : g.personality.roughness.weight, g.personality.roughness.bias)
                        }
                    };
                    nextGeneration.Add(child);
                }
            }

            this.currentGeneration++;

            this.genomes = nextGeneration;
        }

        public void assignFitness(int fitness)
        {
            this.currentGenome.fitness = fitness;
        }

        public Move getNextMove_Seed(Game ref_CurrentGame, Genome seedGenome)
        {
            currentGenome = seedGenome;
            currentGenome.movesTaken++;

            //if (currentGenome.movesTaken > moveLimit)
            //{
            //    currentGenome.fitness = ref_CurrentGame.TotalLinesCleared;
            //    evaluateNextGenome();
            //}
            //else
            //{
            var possibleMoves = getAllPossibleMoves(ref_CurrentGame);

            //GameState orgState = new GameState(currentGameRef);

            return getHighestRatedMove(possibleMoves);

            //if (inspectMoveSelection)
            //{
            //    moveAlgorithim = move.algorithim;
            //}

            //Would need to send details to the view with algorothim behavior
            //}
        }

        public Move getNextMove(Game ref_CurrentGame)
        {
            currentGenome.movesTaken++;

            //if (currentGenome.movesTaken > moveLimit)
            //{
            //    currentGenome.fitness = ref_CurrentGame.TotalLinesCleared;
            //    evaluateNextGenome();
            //}
            //else
            //{
            var possibleMoves = getAllPossibleMoves(ref_CurrentGame);

                //GameState orgState = new GameState(currentGameRef);

            return getHighestRatedMove(possibleMoves);

                //if (inspectMoveSelection)
                //{
                //    moveAlgorithim = move.algorithim;
                //}

                //Would need to send details to the view with algorothim behavior
            //}
        }

        public Move getHighestRatedMove(List<Move> possibleMoves)
        {
            return possibleMoves.OrderByDescending(mv => mv.rating).First();
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

            for (int rotations = 0; rotations < 4; rotations++)
            {
                List<int> originalPos = new List<int>();

                for (int translations = -5; translations <= 5; translations++)
                {
                    Game clonedState = new Game(currentGameRef);

                    //rotate the shape
                    for (int i = 0; i < rotations; i++)
                    {
                        clonedState.CurrentPiece.RotateClockwise();
                    }

                    //move the shape
                    if (translations < 0)
                    {
                        for (int i = 0; i < Math.Abs(translations); i++)
                        {
                            clonedState.PlayerInput(PlayerInput.Left);
                        }

                    }
                    else if (translations > 0)
                    {
                        for (int i = 0; i < translations; i++)
                        {
                            clonedState.PlayerInput(PlayerInput.Right);
                        }
                    }

                    if (!originalPos.Contains(clonedState.CurrentPiece.PositionX))
                    {
                        Result MoveDownResult = moveToBottom(clonedState);

                        //GameStats gs = new GameStats
                        //{
                        //    rowsCleared = MoveDownResult.rowsCleared * currentGenome.personality.rowsClearedWeight,
                        //    weightedHeight = getWeightedHeight(clonedState) * currentGenome.personality.weightedHeight,
                        //    cumulativeHeight = getCumulativeHeight(clonedState) * currentGenome.personality.cumulativeHeight,
                        //    relativeHeight = getRealtiveHeight(clonedState) * currentGenome.personality.relativeHeight,
                        //    holes = getHoles(clonedState) * currentGenome.personality.holesWeight,
                        //    roughness = getRoughness(clonedState) * currentGenome.personality.roughnessWeight
                        //};

                        GameStats gs = new GameStats
                        {
                            rowsCleared = MoveDownResult.rowsCleared,
                            weightedHeight = getWeightedHeight(clonedState),
                            cumulativeHeight = getCumulativeHeight(clonedState),
                            relativeHeight = getRealtiveHeight(clonedState),
                            holes = getHoles(clonedState),
                            roughness = getRoughness(clonedState) 
                        };

                        int rating = 0;

                        rating += gs.rowsCleared * Convert.ToInt32(currentGenome.personality.rowsCleared.weight * currentGenome.personality.rowsCleared.bias);
                        rating += gs.weightedHeight * Convert.ToInt32(currentGenome.personality.weightedHeight.weight * currentGenome.personality.weightedHeight.bias);
                        rating += gs.cumulativeHeight * Convert.ToInt32(currentGenome.personality.cumulativeHeight.weight * currentGenome.personality.cumulativeHeight.bias);
                        rating += gs.relativeHeight * Convert.ToInt32(currentGenome.personality.relativeHeight.weight * currentGenome.personality.relativeHeight.bias);
                        rating += gs.holes * Convert.ToInt32(currentGenome.personality.holes.weight * currentGenome.personality.holes.bias);
                        rating += gs.roughness * Convert.ToInt32(currentGenome.personality.roughness.weight * currentGenome.personality.roughness.bias);

                        //if (MoveDownResult.lose)
                        //{
                        //    rating = 0;
                        //}

                        possibleMoves.Add(new Move()
                        {
                            rotation = rotations,
                            translation = translations,
                            rating = rating,
                            gameStats = gs
                        });
                    }
                }
            }

            //loadState(StateSynced);//////////////////////////////////////////////////////////

            return possibleMoves;
        }

        public Result moveToBottom(Game game)
        {
            //problem is here
            Result result = new Result { lose = false, rowsCleared = 0 };

            while (!game.CheckForDeactivations())
            {
                game.PlayerInput(PlayerInput.Down);
            }
            result.lose = !game.softGameOver;
            result.rowsCleared = game.TotalLinesCleared;

            return result;

        }

        /// <summary>
        /// Creates a deep copy of a Game object
        /// </summary>
        //public Game loadState(GameState game)
        //{
        //    gameState = game.Clone(game);
        //    gameState.isActive = false;
        //}

        //public void saveState(Game game)
        //{
        //    prevGameState = game.Clone(game);
        //    prevGameState.isActive = false;
        //}

        //public Game cloneState(Game orgGame)
        //{
        //    Game newState = new Game();

            
        //}

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


        public int getFilledRatio(Game State)
        {
            var countFilled = 0;
            var countHoles = this.getHoles(State);

            for (int y = 1; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (State.DeadGrid[x, y] == ShapeEnum.Filled)
                    {
                        countFilled++;
                    }
                }
            }

            countHoles = countHoles == 0 ? 1 : countHoles;

            return countFilled / countHoles;
        }
    }
}
