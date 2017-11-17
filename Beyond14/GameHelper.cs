using System.Collections.Generic;

namespace Beyond14
{
    public static class GameHelper
    {
        public const int MaxPreCalcTile = 32;
        public const int MaxPreCalcTileBits = 5;
        public static readonly UInt128 TileMask = 0b11111;

        public static short GetTileInArea(UInt128 area, int x, int y, int areaWidth)
        {
            if (x < 0 || x >= areaWidth || y < 0 || y >= areaWidth)
                return 0;
            int index = y * areaWidth + x;
            area = area >> (index * MaxPreCalcTileBits);
            short result = (short)(area & TileMask);
            return result;
        }

        public static ushort GetMaxTileInArea(UInt128 area)
        {
            ushort max = 0;
            for (int i = 0; i < 80; i += MaxPreCalcTileBits)
            {
                ushort current = (ushort)((area >> i) & TileMask);
                if (current > max)
                    max = current;
            }
            return max;
        }


        public static UInt128 GetAreaFromArray(short[,] area, int bitsPerField)
        {
            UInt128 result = 0;
            for (int i = 0; i < area.GetLength(0); i++)
            {
                for (int j = 0; j < area.GetLength(1); j++)
                {
                    long value = area[i, j];
                    int index = j * area.GetLength(0) + i;
                    result |= (UInt128)value << (index * bitsPerField);
                }
            }
            return result;
        }

        public static T SelectRandomly<T>(this IList<T> list)
        {
            int index = ThreadStaticRandom.Next(0, list.Count);
            return list[index];
        }

        public static short GetMinTileInArea(UInt128 area)
        {
            short min = short.MaxValue;
            for (int i = 0; i < 80; i += MaxPreCalcTileBits)
            {
                short current = (short)((area >> i) & TileMask);
                if (current < min)
                    min = current;
            }
            return min;
        }

        public static short GetEmptyTileCount(UInt128 area)
        {
            short count = 0;
            for (int i = 0; i < 80; i += MaxPreCalcTileBits)
            {
                short current = (short)((area >> i) & TileMask);
                if (current == 0)
                    count++;
            }
            return count;
        }
    }
}