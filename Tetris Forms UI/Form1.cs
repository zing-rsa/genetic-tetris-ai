using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TetrisBot;
using Tetris_Clone;
using System.Diagnostics;

namespace Tetris_Forms_UI
{
    public partial class Form1 : Form
    {
        private Game game;
        private PopulationManager popMan;
        //private TetBrain brain;

        public static int xOffset = 10;
        public static int yOffset = 5;

        private ShapeEnum[,] localDeadGrid;
        private BufferedGraphics buffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            BufferedGraphicsContext context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            buffer = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));

            StartNewGame(initialising: true);
        }
        
        private void StartNewGame(bool initialising)
        {
            game = new Game(800, 20, 10, 4, true);

            if (initialising)
            {
                InitalisePopulation();
            }

            game.readyForNextMove += popMan.nextMoveRequired;
            //game.PieceHitBottom += doNothing;

            localDeadGrid = game.DeadGrid;
            game.GameOver += game_GameOver;

            game.LinesCleared += game_LinesCleared;
            game.LinesAboutToClear += game_LinesAboutToClear;

            gameTimer.Interval = 10;
            gameTimer.Start();
            
            DrawGame();

            if (!initialising) brain.SyncAllStates(game);

        }

        private void InitalisePopulation()
        {
            popMan = new PopulationManager();
            popMan.readyForEvaluation();
        }



        void doNothing(object sender, EventArgs e)
        {

        }

        #region Ticker(s)
        private void timer_Tick(object sender, EventArgs e)
        {
            game.TriggerGameTick();
            DrawGame();
        }

        #endregion

        #region Input and Keydowns

        private static PlayerInput MapKeypressToInput(KeyEventArgs input)
        {
            switch (input.KeyCode)
            {
                case Keys.Down:
                    return PlayerInput.Down;
                case Keys.Left:
                    return PlayerInput.Left;
                case Keys.Right:
                    return PlayerInput.Right;
                case Keys.Q:
                    return PlayerInput.RotateClockwise;
                case Keys.W:
                    return PlayerInput.RotateAntiClockwise;
                default:
                    return PlayerInput.Unknown;
            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.N:
                    if (game.isGameOver)
                    {
                        StartNewGame(false);
                    }
                    break;
                case Keys.P:
                    game.CurrentPiece = new Piece(PieceEnum.I, 5, 0);
                    break;
                case Keys.Up:
                    game.CurrentPiece.PositionY -= 1;
                    DrawGame();
                    break;
                default:
                    game.PlayerInput(MapKeypressToInput(e));
                    DrawGame();
                    break;
            }
        }

        #endregion

        #region Event Listeners

        void game_LinesAboutToClear(object sender, ShapeEnum[,] e)
        {
            localDeadGrid = e;
        }

        void game_GameOver(object sender, EventArgs e)
        {
            StartNewGame(false);
        }

        void game_LinesCleared(object sender, int[] e)
        {
            clearLines(e);
        }

        #endregion

        #region Graphics Config

        private Color GetColorByPieceType(PieceEnum pieceEnum)
        {
            switch (pieceEnum)
            {
                case PieceEnum.I:
                    return Color.Red;
                case PieceEnum.L:
                    return Color.Yellow;
                case PieceEnum.J:
                    return Color.Purple;
                case PieceEnum.T:
                    return Color.Brown;
                case PieceEnum.O:
                    return Color.Blue;
                case PieceEnum.S:
                    return Color.Green;
                case PieceEnum.Z:
                    return Color.Teal;
                default:
                    return Color.DarkGray;
            }
        }

        private string GetLineClearText(int p)
        {
            switch (p)
            {
                case 1:
                    return "Only a single? ";
                case 2:
                    return "Double. Not bad...";
                case 3:
                    return "Triple! So close!";
                case 4:
                    return "ZOMG TETRIS!";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Drawing Graphics

        private void clearLines(int[] linesToClear)
        {
            Graphics g = buffer.Graphics;
            g.Clear(Color.Black);

            DrawPiece(game, g);
            RefreshPlayArea(game, g);

            buffer.Render(Graphics.FromHwnd(this.Handle));

            localDeadGrid = game.DeadGrid;
        }

        private void DrawGame()
        {
            Graphics g = buffer.Graphics;
            g.Clear(Color.Black);
            DrawPiece(game, g);
            RefreshPlayArea(game, g);

            if (game.isGameOver)
            {
                gameTimer.Stop();

                var brush = new SolidBrush(Color.Crimson);

                g.DrawString("GAME OVER :-(", new Font("Arial", 28), brush, 80, 50);
                g.DrawString("Press 'n' for a new game", new Font("Arial", 11), brush, 480, 300);
            }

            buffer.Render(Graphics.FromHwnd(this.Handle));
        }

        private void DrawPiece(Game game, Graphics graphics)
        {
            int multiplier = 20;
            int brushSize = 1;
            int squareSize = 19;

            var brush = new SolidBrush(GetColorByPieceType(game.CurrentPiece.PieceType));
            //var brush = new TextureBrush(tex);
            var pen = new Pen(brush, brushSize);

            for (int j = 0; j < game.CurrentPiece.Shape.GetLength(1); j++)
            {
                for (int i = 0; i < game.CurrentPiece.Shape.GetLength(0); i++)
                {
                    if (game.CurrentPiece.Shape[i, j] == ShapeEnum.Active)
                    {
                        graphics.FillRectangle(brush, new Rectangle((i + game.CurrentPiece.PositionX + xOffset) * multiplier, (j + game.CurrentPiece.PositionY + yOffset) * multiplier, squareSize, squareSize));
                    }
                }
            }


            int xRightSide = 24;
            brush = new SolidBrush(GetColorByPieceType(game.NextPiece.PieceType));
            for (int j = 0; j < game.NextPiece.Shape.GetLength(1); j++)
            {
                for (int i = 0; i < game.NextPiece.Shape.GetLength(0); i++)
                {
                    if (game.NextPiece.Shape[i, j] == ShapeEnum.Active)
                    {
                        graphics.FillRectangle(brush, new Rectangle((i + xRightSide) * multiplier, (j + yOffset) * multiplier, squareSize, squareSize));
                    }
                }
            }


            brush = new SolidBrush(Color.DarkGray);
            pen = new Pen(brush, brushSize);

            for (int x = 0; x < localDeadGrid.GetLength(0); x++)
            {
                for (int y = 0; y < localDeadGrid.GetLength(1); y++)
                {
                    if (localDeadGrid[x, y] == ShapeEnum.Filled)
                    {
                        graphics.FillRectangle(brush, new Rectangle((x + xOffset) * multiplier, (y + yOffset) * multiplier, squareSize, squareSize));
                    }
                }
            }

        }
        private void RefreshPlayArea(Game game, Graphics g)
        {
            int multiplier = 20;
            int brushSize = 20;

            var brush = new SolidBrush(Color.SteelBlue);
            var pen = new Pen(brush, brushSize);
            g.DrawLine(pen, (xOffset * multiplier) - 11, yOffset * multiplier, (xOffset * multiplier) - 11, (game.Height + yOffset) * multiplier);
            g.DrawLine(pen, ((game.Width + xOffset) * multiplier) + 10, yOffset * multiplier, ((game.Width + xOffset) * multiplier) + 10, (game.Height + yOffset) * multiplier);
            g.DrawLine(pen, (xOffset * multiplier) - brushSize - 1, ((game.Height + yOffset) * multiplier) + 10, (game.Width + xOffset) * multiplier + brushSize, ((game.Height + yOffset) * multiplier) + 10);

            g.DrawString("Lines cleared: " + game.TotalLinesCleared.ToString(), new Font("Arial", 13), brush, 440, 60);
            g.DrawString("Q & W to rotate", new Font("Arial", 13), brush, 440, 80);


        }

        #endregion

    }
}
