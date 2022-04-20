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
using System.Threading;

namespace Tetris_Forms_UI
{
    public partial class Form1 : Form
    {
        private Game game;
        private PopulationManager popMan;
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

            StartNewGame(initializing: true);
        }
        
        private void StartNewGame(bool initializing)
        {
            game = new Game(800, 20, 10, 4, true);

            if (initializing)
            {
                InitializePopulation();
            }

            game.readyForNextMove += this.nextMoveRequired;

            localDeadGrid = game.DeadGrid;
            game.GameOver += game_GameOver;

            game.LinesCleared += game_LinesCleared;
            game.LinesAboutToClear += game_LinesAboutToClear;

            gameTimer.Interval = 10;
            gameTimer.Start();
            
            DrawGame();
        }

        public void nextMoveRequired(object sender, Game game)
        {
            Move chosenMove = popMan.getNextMove(game);

            this.ExecuteMove(chosenMove);
        }

        public void ExecuteMove(Move move)
        {
            for (var rotations = 0; rotations < move.rotation; rotations++)
            {
                game.PlayerInput(PlayerInput.RotateClockwise);
            }
            if (move.translation < 0)
            {
                for (var lefts = 0; lefts < Math.Abs(move.translation); lefts++)
                {
                    game.PlayerInput(PlayerInput.Left);
                }
            }
            else if (move.translation > 0)
            {
                for (var rights = 0; rights < move.translation; rights++)
                {
                    game.PlayerInput(PlayerInput.Right);
                }
            }
        }

        private void InitializePopulation()
        {
            popMan = new PopulationManager(50);
            popMan.readyForEvaluation();
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
            popMan.currentGenome.fitness = this.game.TotalLinesCleared;
            popMan.evaluateNextGenome();
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

        #region Display genome info

        public void WriteInfo(Graphics g)
        {
            var brush = new SolidBrush(Color.GhostWhite);
            var font_m = new Font("Arial", 15);
            var font_s = new Font("Arial", 13);

            g.DrawString("Generation Stats:", new Font("Arial", 20), brush, 680, 30);
            g.DrawString("Population Size: " + popMan.populationSize, font_m, brush, 700, 75);
            g.DrawString("Current Generation: " + (popMan.currentGeneration + 1 ), font_m, brush, 700, 100);

            Genome generationBestPerformer = popMan.genomes.OrderByDescending(gnme => gnme.fitness).First();
            Genome bestPerformer = popMan.genomes[0];
            var bestPerformerGeneration = 0;

            if (generationBestPerformer.fitness > bestPerformer.fitness)
            {
                bestPerformer = generationBestPerformer;
                bestPerformerGeneration = popMan.currentGeneration;
            }

            g.DrawString("- Best Performer Fitness: " + bestPerformer.fitness, new Font("Arial", 12), brush, 710, 127);
            if (popMan.genomes.Where((gnme) => gnme.played == true).ToList().Count > 0)
            {
                g.DrawString("- Avg Fitness: " + popMan.genomes.Where((gnme) => gnme.played == true).Select((gnme) => gnme.fitness).Average(), new Font("Arial", 12), brush, 950, 127);
            }

            g.DrawString(" - RowsCleared :", font_s, brush, 720, 150);
            g.DrawString("Weight:  " + bestPerformer.personality.rowsCleared.weight , font_s, brush, 930, 150);
            g.DrawString("Bias:  " + bestPerformer.personality.rowsCleared.getBias(), font_s, brush, 1110, 150);

            g.DrawString(" - WeightedHeight :", font_s, brush, 720, 170);
            g.DrawString("Weight:  " + bestPerformer.personality.weightedHeight.weight , font_s, brush, 930, 170);
            g.DrawString("Bias:  " + bestPerformer.personality.weightedHeight.getBias(), font_s, brush, 1110, 170);

            g.DrawString(" - CumulativeHeight :", font_s, brush, 720, 190);
            g.DrawString("Weight:  " + bestPerformer.personality.cumulativeHeight.weight , font_s, brush, 930, 190);
            g.DrawString("Bias:  " + bestPerformer.personality.cumulativeHeight.getBias(), font_s, brush, 1110, 190);

            g.DrawString(" - RelativeHeight :", font_s, brush, 720, 210);
            g.DrawString("Weight:  " + bestPerformer.personality.relativeHeight.weight , font_s, brush, 930, 210);
            g.DrawString("Bias:  " + bestPerformer.personality.relativeHeight.getBias(), font_s, brush, 1110, 210);

            g.DrawString(" - Holes :", font_s, brush, 720, 230);
            g.DrawString("Weight:  " + bestPerformer.personality.holes.weight , font_s, brush, 930, 230);
            g.DrawString("Bias:  " + bestPerformer.personality.holes.getBias(), font_s, brush, 1110, 230);

            g.DrawString(" - Roughness :", font_s, brush, 720, 250);
            g.DrawString("Weight:  " + bestPerformer.personality.roughness.weight , font_s, brush, 930, 250);
            g.DrawString("Bias:  " + bestPerformer.personality.roughness.getBias(), font_s, brush, 1110, 250);

            g.DrawString("- Best Performer Gen: " + (bestPerformerGeneration+1), new Font("Arial", 12), brush, 710, 275);

            g.DrawString("Genome Stats:", new Font("Arial", 20), brush, 680, 300);

            g.DrawString("Current Genome: " + (popMan.currentGenomeIndex + 1), font_m, brush, 700, 350);
            g.DrawString(" - Fitness: " + game.TotalLinesCleared, font_m, brush, 720, 390);
            g.DrawString(" - Moves taken: " + popMan.currentGenome.movesTaken, font_m, brush, 720, 415);

            g.DrawString(" - RowsCleared :" , font_m, brush, 720, 450);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.rowsCleared.weight, font_m, brush, 930, 450);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.rowsCleared.getBias(), font_m, brush, 1110, 450);

            g.DrawString(" - WeightedHeight :", font_m, brush, 720, 475);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.weightedHeight.weight , font_m, brush, 930, 475);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.weightedHeight.getBias(), font_m, brush, 1110, 475);

            g.DrawString(" - CumulativeHeight :", font_m, brush, 720, 500);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.cumulativeHeight.weight , font_m, brush, 930, 500);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.cumulativeHeight.getBias(), font_m, brush, 1110, 500);

            g.DrawString(" - RelativeHeight :", font_m, brush, 720, 525);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.relativeHeight.weight , font_m, brush, 930, 525);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.relativeHeight.getBias(), font_m, brush, 1110, 525);

            g.DrawString(" - Holes :", font_m, brush, 720, 550);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.holes.weight , font_m, brush, 930, 550);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.holes.getBias(), font_m, brush, 1110, 550);

            g.DrawString(" - Roughness :", font_m, brush, 720, 575);
            g.DrawString("Weight:  " + popMan.currentGenome.personality.roughness.weight , font_m, brush, 930, 575);
            g.DrawString("Bias:  " + popMan.currentGenome.personality.roughness.getBias(), font_m, brush, 1110, 575);

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

            WriteInfo(g);

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
            var font_s = new Font("Arial", 13);
            var pen = new Pen(brush, brushSize);

            g.DrawLine(pen, (xOffset * multiplier) - 11, yOffset * multiplier, (xOffset * multiplier) - 11, (game.Height + yOffset) * multiplier);
            g.DrawLine(pen, ((game.Width + xOffset) * multiplier) + 10, yOffset * multiplier, ((game.Width + xOffset) * multiplier) + 10, (game.Height + yOffset) * multiplier);
            g.DrawLine(pen, (xOffset * multiplier) - brushSize - 1, ((game.Height + yOffset) * multiplier) + 10, (game.Width + xOffset) * multiplier + brushSize, ((game.Height + yOffset) * multiplier) + 10);

            g.DrawString("Lines cleared: " + game.TotalLinesCleared.ToString(), font_s, brush, 440, 60);
            g.DrawString("Q & W to rotate", font_s, brush, 440, 80);

        }

        #endregion

    }
}
