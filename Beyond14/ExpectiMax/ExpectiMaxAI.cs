using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Beyond14.ExpectiMax
{
    public class ExpectiMaxAI : AI
    {
        private long LastDuration { get; set; } = 0;
        private int Depth { get; set; } = 2;

        protected override Move CalculateNextMove(Board board, Action<Board> debugBoard)
        {
            var emptyTileCount = 16 - GameHelper.GetEmptyTileCount(board.Field);
            Depth = (int)Math.Sqrt(emptyTileCount);

            if (Depth < 2)
                Depth = 2;
            if (Depth > 6)
                Depth = 6;

            var allowedMoves = GetAllowedMoves(board);
            object locker = new object();
            Move bestMove = allowedMoves.First();
            double bestRating = 0;

            Parallel.ForEach(allowedMoves,
                             allowedMove =>
                             {
                                 var nextField = board.PlaceTile(allowedMove.X, allowedMove.Y);
                                 nextField = MergeTiles(nextField, allowedMove.X, allowedMove.Y);
                                 var possibleNextTiles = GetPossibleNextTiles(board);
                                 var average = possibleNextTiles.Average(s =>
                                                                         {
                                                                             var nextBoard = new Board(nextField, board.AfterNextTile, s);
                                                                             return CalculateNextMoveRecursive(nextBoard, 1, Depth);
                                                                         });
                                 lock (locker)
                                 {
                                     if (average > bestRating)
                                     {
                                         bestRating = average;
                                         bestMove = allowedMove;
                                     }
                                 }
                             });
            return bestMove;
        }

        private double CalculateNextMoveRecursive(Board board, int depth, int maxDepth)
        {
            if (depth >= maxDepth || GameHelper.GetMinTileInArea(board.Field) > 0)
                return Heuristik(board);
            var allowedMoves = GetAllowedMoves(board);
            var bestRating = 0d;
            foreach (var allowedMove in allowedMoves)
            {
                var nextField = board.PlaceTile(allowedMove.X, allowedMove.Y);
                nextField = MergeTiles(nextField, allowedMove.X, allowedMove.Y);
                var possibleNextTiles = GetPossibleNextTiles(board);
                double average = 0;
                foreach (var s in possibleNextTiles)
                {
                    var nextBoard = new Board(nextField, board.AfterNextTile, s);
                    average += CalculateNextMoveRecursive(nextBoard, depth + 1, maxDepth);
                }
                average /= possibleNextTiles.Count;
                if (average > bestRating)
                    bestRating = average;
            }
            return bestRating;
        }

        private IEnumerable<T> Enumerate<T>(T[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    yield return array[i, j];
                }
            }
        }
        private static short[] GetNeighbours(short[,] array, int x, int y)
        {
            short[] neighbours = {
                SaveAccess(array, x - 1, y - 1),
                SaveAccess(array, x, y - 1),
                SaveAccess(array, x + 1, y - 1),
                SaveAccess(array, x - 1, y),
                SaveAccess(array, x + 1, y),
                SaveAccess(array, x - 1, y + 1),
                SaveAccess(array, x, y + 1),
                SaveAccess(array, x + 1, y + 1)
            };
            Array.Sort(neighbours);
            return neighbours;
        }


        public static double Heuristik(Board board)
        {
            return GameHelper.GetEmptyTileCount(board.Field) / 32.0+ GameHelper.GetMaxTileInArea(board.Field) / 128.0;
            double rating = 0;
            List<short> tiles = new List<short>();
            ushort highestTile = GameHelper.GetMaxTileInArea(board.Field);
            short[,] field = new short[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    short current = GameHelper.GetTileInArea(board.Field, i, j, 4);
                    field[i, j] = current;
                    current *= 2;
                    if (!tiles.Contains(current))
                    {
                        current *= 2;
                        tiles.Add(current);
                    }
                    rating += current * current * current;
                }
            }
            double highestTileOnEdgeRating = highestTile * 8;
            double emptyRating = 0;
            int emptyCount = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (field[i, j] == highestTile)
                    {
                        var neighbours = GetNeighbours(field, i, j);
                        int outsides = 0;
                        for (int k = 0; k < neighbours.Length; k++)
                        {
                            if (neighbours[k] > -1)
                                break;
                            outsides++;
                        }
                        highestTileOnEdgeRating /= outsides + 1;
                    }
                    if (field[i, j] == 0)
                    {
                        emptyCount++;
                        double ratingBase = 24;
                        var neighbours = GetNeighbours(field, i, j);
                        for (int k = 1; k < neighbours.Length; k++)
                        {
                            if (neighbours[k - 1] == 0)
                                ratingBase += neighbours[k] * 2;
                            else if (neighbours[k - 1] == -1)
                                ratingBase += neighbours[k] * 1.25;
                            else if (neighbours[k] == neighbours[k - 1] + 1)
                                ratingBase += neighbours[k] * 1.5;
                            else if (neighbours[k] == neighbours[k - 1])
                                ratingBase += neighbours[k];
                            else
                                ratingBase += 1.0 / (neighbours[k] - neighbours[k - 1]);
                        }
                        emptyRating += ratingBase;
                    }
                }
            }

            rating = rating / 512 + emptyRating + highestTileOnEdgeRating * 10;
            rating *= highestTile / 16.0;
            rating = rating * (emptyCount + 1);
            var heuristik = rating / 8192;
            if (heuristik < min)
            {
                min = heuristik;
                Debug.WriteLine($"{min:F4}-{max:F4}");
            }
            if (heuristik > max)
            {
                max = heuristik;
                Debug.WriteLine($"{min:F4}-{max:F4}");
            }
            return heuristik;
        }

        private static double min = double.MaxValue;
        private static double max = double.MinValue;

        private static short SaveAccess(short[,] array, int x, int y)
        {
            if (x < 0 || y < 0 || x >= array.GetLength(0) || y >= array.GetLength(1))
                return -1;
            return array[x, y];
        }
    }
}