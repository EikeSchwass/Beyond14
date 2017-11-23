using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MCTS;
using static System.Math;

namespace Beyond14.MonteCarlo
{
    public class UCT : AI
    {
        private long MoveDuration { get; }
        private PropUTC<Board, Move, Board> PropUTC { get; set; }

        public UCT(long moveDuration = 2000)
        {
            MoveDuration = moveDuration;
        }

        protected override Move CalculateNextMove(Board board, Move? lastMove)
        {
            if (lastMove != null && false)
            {
                try
                {
                    PropUTC.MoveRoot(board);
                }
                catch (InvalidOperationException)
                {
                    PropUTC = new PropUTC<Board, Move, Board>(board, AllowedMoves, PossibleNextStates, IsFiniteState, EvaluateOutcome);
                    Debug.WriteLine("Had to reset tree", "Error");
                }
            }
            else
            {
                PropUTC = new PropUTC<Board, Move, Board>(board, AllowedMoves, PossibleNextStates, IsFiniteState, EvaluateOutcome);
            }
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var stopwatch = Stopwatch.StartNew();
            var a = GameHelper.GetArrayFromArea(board.Field).Enumerate().OrderByDescending(s => s).ToArray();
            var emptyTileCount = GameHelper.GetEmptyTileCount(board.Field);
            double md = emptyTileCount >= 15 ? 10 : a.First() + a.Skip(1).First() * 1.0 / a.First();
            if (a.First() < 18)
                md = 15;

            md = Sigmoid(md);
            while (stopwatch.ElapsedMilliseconds <= md)
            {
                PropUTC.ImproveTree(token);
            }
            stopwatch.Stop();
            return PropUTC.GetCurrentBestMove();
        }

        private double Sigmoid(double x)
        {
            return 100 + 3000 / (1 + Pow(E, -1 * (x - 18.5)));
        }

        private KeyValuePair<Move, Board>[] AllowedMoves(Board state)
        {
            var allowedMoves = GetAllowedMoves(state);
            var result = new KeyValuePair<Move, Board>[allowedMoves.Count];
            for (int i = 0; i < allowedMoves.Count; i++)
            {
                var allowedMove = allowedMoves[i];
                var nextField = state.PlaceTile(allowedMove.X, allowedMove.Y);
                nextField = MergeTiles(nextField, allowedMove.X, allowedMove.Y);
                var board = new Board(nextField, state.AfterNextTile, 0);
                result[i] = new KeyValuePair<Move, Board>(allowedMove, board);
            }
            return result;
        }

        private double EvaluateOutcome(Board state)
        {
            return ExpectiMax.ExpectiMaxAI.Heuristik(state);
            List<ushort> addedTiles = new List<ushort>();
            double sum = 0;
            for (int i = 0; i < 80; i += GameHelper.MaxPreCalcTileBits)
            {
                ushort current = (ushort)((state.Field >> i) & GameHelper.TileMask);
                if (current == 0)
                {
                    sum += 32 / 24.0;
                }
                else if (addedTiles.Contains(current))
                    sum += current / 32.0;
                else
                {
                    addedTiles.Add(current);
                    sum += current / 24.0;
                }
            }
            sum = sum / 16.0;
            sum += GameHelper.GetMaxTileInArea(state.Field) / 8.0;
            return sum / 4;
        }

        private bool IsFiniteState(Board state)
        {
            return GameHelper.GetEmptyTileCount(state.Field) == 0;
        }

        private KeyValuePair<Board, double>[] PossibleNextStates(Board prepropabilitystate)
        {
            var possibleNextTiles = GetPossibleNextTiles(prepropabilitystate);
            var result = new KeyValuePair<Board, double>[possibleNextTiles.Count];
            for (int i = 0; i < possibleNextTiles.Count; i++)
            {
                result[i] = new KeyValuePair<Board, double>(new Board(prepropabilitystate.Field, prepropabilitystate.NextTile, possibleNextTiles[i]), 1.0 / result.Length);
            }
            return result;
        }
    }
}