// ReSharper disable once RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Beyond14.ExpectiMax;
using Beyond14.MonteCarlo;

namespace Beyond14
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Dictionary<int, Brush> Colors { get; } = new Dictionary<int, Brush>();
        private Stack<Board> BoardHistory { get; } = new Stack<Board>();
        private TextBlock[,] TextBlocks { get; } = new TextBlock[4, 4];
        private Board CurrentBoard => BoardHistory.Any() ? BoardHistory.Peek() : InitBoard();
        private Move? LastMove { get; set; }

        private AI AI { get; }

        public MainWindow()
        {
            InitializeComponent();
            BoardHistory.Push(InitBoard());
            AI = new UCT();
        }

        private void DebugBoard(Board board)
        {
            Thread.Sleep(50);
            Dispatcher.Invoke(() => { RenderBoard(board); });
        }

        private void ExecuteMove(Move move)
        {
            var field = CurrentBoard.PlaceTile(move.X, move.Y);
            field = AI.MergeTiles(field, move.X, move.Y);
            var randomTile = AI.GetPossibleNextTiles(CurrentBoard).SelectRandomly();
            BoardHistory.Push(new Board(field, CurrentBoard.AfterNextTile, randomTile));
            RenderBoard(CurrentBoard);
        }

        private Board InitBoard()
        {
            return new Board(0, 14, 14);
            var shorts = new short[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var next = (short)ThreadStaticRandom.Next(16, 19);
                    shorts[i, j] = (short)(ThreadStaticRandom.NextDouble(1) < 0.2 ? next : 0);
                }
            }
            var field = GameHelper.GetAreaFromArray(shorts, 5);
            return new Board(field, (ushort)ThreadStaticRandom.Next(17, 18), (ushort)ThreadStaticRandom.Next(17, 18));
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Colors.Add(1, (Brush)FindResource("TileColor1"));
            Colors.Add(2, (Brush)FindResource("TileColor2"));
            Colors.Add(3, (Brush)FindResource("TileColor3"));
            Colors.Add(4, (Brush)FindResource("TileColor4"));
            Colors.Add(5, (Brush)FindResource("TileColor5"));
            Colors.Add(6, (Brush)FindResource("TileColor6"));
            Colors.Add(7, (Brush)FindResource("TileColor7"));

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var textBlock = new TextBlock { Text = "0" };
                    PlayGrid.Children.Add(textBlock);
                    Grid.SetColumn(textBlock, i);
                    Grid.SetRow(textBlock, j);
                    TextBlocks[i, j] = textBlock;
                }
            }
            RenderBoard(CurrentBoard);
            //TileMergerDatabase.Initialize();
        }

        private void Rectangle_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var rectangle = (Rectangle)sender;
            int col = Grid.GetColumn(rectangle);
            int row = Grid.GetRow(rectangle);
            ExecuteMove(new Move((short)col, (short)row));
        }

        private void RenderBoard(Board board)
        {
            NextTile.Text = board.NextTile + "";
            AfterNextTile.Text = board.AfterNextTile + "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var tile = GameHelper.GetTileInArea(board.Field, i, j, 4);
                    var rectangle = PlayGrid.Children.OfType<Rectangle>().First(r => { return Grid.GetColumn(r) == i && Grid.GetRow(r) == j; });
                    rectangle.Fill = tile == 0 ? (Brush)FindResource("BackgroundColorBrush") : Colors[(tile - 1) % 7 + 1];
                    TextBlocks[i, j].Text = tile > 0 ? tile + "" : "";
                }
            }
            NextTileRectangle.Fill = Colors[(board.NextTile - 1) % 7 + 1];
            AfterNextTileRectangle.Fill = Colors[(board.AfterNextTile - 1) % 7 + 1];
        }
        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            while (GameHelper.GetEmptyTileCount(CurrentBoard.Field) > 0)
            {
                var stopwatch = Stopwatch.StartNew();
                var move = await AI.CalculateMoveAsync(CurrentBoard, LastMove);
                ExecuteMove(move);
                LastMove = move;
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds < 100)
                    await Task.Delay((int)(100 - stopwatch.ElapsedMilliseconds));
            }
        }
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UndoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (BoardHistory.Any())
                BoardHistory.Pop();
            RenderBoard(CurrentBoard);
        }
    }
}