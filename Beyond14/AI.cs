using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Math;

namespace Beyond14
{
    public abstract class AI
    {
        public Task<Move> CalculateMoveAsync(Board board, Action<Board> debugBoard)
        {
            return Task.Factory.StartNew(() => CalculateNextMove(board, debugBoard));
        }

        protected abstract Move CalculateNextMove(Board board, Action<Board> debugBoard);

        protected List<Move> GetAllowedMoves(Board board)
        {
            var result = new List<Move>(16);
            for (short i = 0; i < 4; i++)
            {
                for (short j = 0; j < 4; j++)
                {
                    if (GameHelper.GetTileInArea(board.Field, i, j, 4) == 0)
                        result.Add(new Move(i, j));
                }
            }
            return result;
        }

        public List<ushort> GetPossibleNextTiles(Board board)
        {
            ushort min = 1;
            ushort max = 0;
            ushort maxTile = GameHelper.GetMaxTileInArea(board.Field);
            if (maxTile >= 5)
            {
                max = (ushort)(Ceiling(maxTile * 0.7) + 0.001);
            }
            else
            {
                max = 2;
            }
            if (maxTile - 3 >= 9)
            {
                ushort minInQueue = board.NextTile < board.AfterNextTile ? board.NextTile : board.AfterNextTile;
                ushort num3 = (ushort)Max(2, (int)(Ceiling(maxTile * 0.7 - 7) + 0.001));
                if (minInQueue < num3)
                    max = Max(max, minInQueue);
                else
                {
                    max = (ushort)(num3 + 8);
                    min = num3;
                }
            }
            var result = new List<ushort>(max - min);
            for (ushort i = min; i <= max; i++)
            {
                result.Add(i);
            }
            return result;
        }

        public UInt128 MergeTiles(UInt128 nextField, short x, short y)
        {
            bool dummy;
            do
            {
                nextField = MergeTiles(nextField, x, y, out dummy);
            }
            while (dummy);
            return nextField;
        }

        private UInt128 MergeTiles(UInt128 nextField, short x, short y, out bool merged)
        {
            bool m = false;
            UInt128 result = 0;
            int index = y * 4 + x;
            ushort center = (ushort)((nextField >> (index * 5)) & GameHelper.TileMask);
            for (short i = 0; i < 16; i++)
            {
                if (i == index)
                {
                    result |= (UInt128)center << (index * 5);
                    continue;
                }

                short currentX = (short)(i % 4);
                short currentY = (short)(i / 4);
                if (Abs(currentX - x) > 1 || Abs(currentY - y) > 1)
                {
                    result |= nextField & (GameHelper.TileMask << (i * 5));
                    continue;
                }
                ushort current = (ushort)((nextField >> (i * 5)) & GameHelper.TileMask);
                if (current == center)
                {
                    m = true;
                }
                else
                {
                    result |= nextField & (GameHelper.TileMask << (i * 5));
                }
            }
            if (m)
            {
                result = result + ((UInt128)1 << (index * 5));
            }
            merged = m;
            return result;
        }
    }
}