using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Clone
{
    public class Game
    {
        #region Properties

        public Piece piece;
        public int gameTick;
        public ShapeEnum[,] deadGrid;
        public bool isTicking;
        public bool isGameOver;

        public event EventHandler GameOver;
        public event EventHandler PieceHitBottom;
        public event EventHandler NewGameStarted;

        public event EventHandler<ShapeEnum[,]> LinesAboutToClear;
        public event EventHandler<int[]> LinesCleared;
        public int GameTick
        {
            get { return gameTick; }
            set { gameTick = value; }
        }
        public int StartXPosition { get; set; }
        public int StartYPosition { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Piece CurrentPiece
        {
            get { return piece; }
            set { piece = value; }
        }
        public ShapeEnum[,] DeadGrid
        {
            get
            {
                return deadGrid;
            }
            set
            {
                deadGrid = value;
            }
        }

        private Piece nextPiece;
        public Piece NextPiece
        {
            get { return nextPiece; }
            set { nextPiece = value; }
        }
        

        private int totalLinesCleared;
        public int TotalLinesCleared
        {
            get { return totalLinesCleared; }
            private set { totalLinesCleared = value; }
        }

        public bool isActive { get; set; }

        #endregion Properties
        [JsonConstructor]
        public Game(int tickInterval, int height, int width, int startXPosition, bool isActive)
        {
            isGameOver = false;
            isTicking = false;
            gameTick = tickInterval;
            totalLinesCleared = 0;

            this.isActive = isActive;
            this.StartXPosition = startXPosition;
            this.StartYPosition = 0;
            this.CurrentPiece =  AddNewPiece();
            this.NextPiece = AddNewPiece();
            this.Height = height;
            this.Width = width;

            InitialiseDeadGrid();

        }

        public void PlayerInput(PlayerInput input)
        {
            //if (isTicking)
            //{
            //    return;
            //}

            switch (input)
            {
                case Tetris_Clone.PlayerInput.Down:
                    TriggerGameTick(true);
                    break;
                case Tetris_Clone.PlayerInput.Left:
                    if (piece.PositionX > 0 )
                    {
                        piece.PositionX -= 1;
                        if (CheckForDeactivations())
                        {
                            piece.PositionX += 1;
                        }
                    }                    
                    break;
                case Tetris_Clone.PlayerInput.Right:
                    if (piece.RightMostX < this.Width)
                    {
                        piece.PositionX += 1;
                        if (CheckForDeactivations())
                        {
                            piece.PositionX -= 1;
                        }
                    }                    
                    break;
                case Tetris_Clone.PlayerInput.RotateAntiClockwise:
                    piece.RotateAntiClockwise();
                    CorrectPiecePositionAfterRotation();
                    break;
                case Tetris_Clone.PlayerInput.RotateClockwise:
                    piece.RotateClockwise();
                    CorrectPiecePositionAfterRotation();
                    break;
                default:
                    break;
            }
        }

        public void TriggerGameTick(bool goingDown = false)
        {
            if (isGameOver)
            {
                return;
            }

            isTicking = true;
            piece.PositionY += 1;

            if (CheckForDeactivations())//Checks for pieces that have reached the "floor"
            {
                //atbottom event


                //Need to add functionality that will check if a piece has hit the ground, 
                //so that the Ai can be triggered to make its next move
                //Events? The problem is that the event has not been initalised before 
                //the first time it needs to be called

                //Try passing the method to be called to the contructor or something like that

                //PieceReachedBottom.Invoke(this, new EventArgs());
                
                piece.PositionY -= 1;
                piece.Deactivate();
                AddPieceToDeadGrid(piece);
                ClearLinesFromDeadGrid();
                piece = nextPiece;
                nextPiece = AddNewPiece();


                //NewPeiceAdded
                if (isActive)
                {
                    // Check that this game isntance is in fact the one that is 
                    //the one being used to play the game, and not the one the AI is using
                    //to test moves
                    PieceHitBottom.Invoke(this, new EventArgs());
                }

                if (CheckForDeactivations() && isActive)
                {
                    isGameOver = true;
                    GameOver.Invoke(this, new EventArgs());
                }
            }
            isTicking = false;
        }
        public bool CheckForDeactivations()
        {
            if (piece.BottomMostY > this.Height)
            {
                return true;
            }

            for (int x = 0; x < piece.Shape.GetLength(0); x++)
            {
                for (int y = 0; y < piece.Shape.GetLength(1); y++)
                {
                    if (piece.Shape[x,y] != ShapeEnum.Empty &&
                        deadGrid[piece.PositionX + x, piece.PositionY + y] != ShapeEnum.Empty)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private void ClearLinesFromDeadGrid()
        {
            var oldGrid = (ShapeEnum[,])deadGrid.Clone();
            List<int> yLinesToClear = new List<int>();

            for (int y = 0; y < deadGrid.GetLength(1); y++)
            {
                bool fullLine = true;
                for (int x = 0; x < deadGrid.GetLength(0); x++)
                {
                    if (deadGrid[x, y] == ShapeEnum.Empty)
                    {
                        fullLine = false;                        
                        break;
                    }
                }
                if (fullLine)
                {
                    yLinesToClear.Add(y);

                    for (int ytoClear = y; ytoClear > 0; ytoClear--)
                    {
                        for (int x = 0; x < deadGrid.GetLength(0); x++)
                        {
                            deadGrid[x, ytoClear] = deadGrid[x, ytoClear - 1];
                        }
                    }
                }
            }

            if (yLinesToClear.Any())
            {
                totalLinesCleared += yLinesToClear.Count();
                LinesAboutToClear.Invoke(this, oldGrid);
                LinesCleared.Invoke(this, yLinesToClear.ToArray());
            }
            

        }
        private void AddPieceToDeadGrid(Piece piece)
        {
            for (int x = 0; x < piece.Shape.GetLength(0); x++)
            {
                for (int y = 0; y < piece.Shape.GetLength(1); y++)
                {
                    if (piece.Shape[x, y] != ShapeEnum.Empty)
                    {
                        deadGrid[piece.PositionX + x, piece.PositionY + y] = ShapeEnum.Filled;
                    }
                }
            }
        }
        private Piece AddNewPiece()
        {
            Random rand = new Random();
            var randomNumber = rand.Next(0, 7);
            return new Piece((PieceEnum)randomNumber, this.StartXPosition, this.StartYPosition);
        }
        private void CorrectPiecePositionAfterRotation()
        {
            if (piece.RightMostX > this.Width)
            {
                piece.PositionX -= piece.RightMostX - this.Width;
            }
        }
        private void InitialiseDeadGrid()
        {
            //Building the grid out of a 2d array of ShapeEnums which can be active or inactive
            deadGrid = new ShapeEnum[this.Width, this.Height];

            //Populating the grid with inactive blocks
            for (int x = 0; x < deadGrid.GetLength(0); x++)
            {
                for (int y = 0; y < deadGrid.GetLength(1); y++)
                {
                    deadGrid[x, y] = ShapeEnum.Empty;

                }
            }
        }


        public Game Clone(Game source, bool isActive = false)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(Game);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var newState = JsonConvert.DeserializeObject<Game>(JsonConvert.SerializeObject(source), deserializeSettings);
            newState.isActive = isActive;
            newState.GameOver = source.GameOver;
            newState.LinesAboutToClear = source.LinesAboutToClear;
            newState.LinesCleared = source.LinesCleared;
            return newState;
        }

    }
}
