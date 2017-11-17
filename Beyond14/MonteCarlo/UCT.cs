using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MCTS;

namespace Beyond14.MonteCarlo
{
    public class UCT : AI
    {
        private long MoveDuration { get; }

        public UCT(long moveDuration = 3072)
        {
            MoveDuration = moveDuration;
        }


        protected override Move CalculateNextMove(Board board, Action<Board> debugBoard)
        {
            var propUTC = new PropUTC<Board, Move, Board>(board, AllowedMoves, PossibleNextStates, IsFiniteState, EvaluateOutcome, debugBoard);
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds <= MoveDuration)
            {
                propUTC.ImproveTree(token);
            }
            stopwatch.Stop();
            return propUTC.GetCurrentBestMove();
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