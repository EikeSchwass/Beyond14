using System.Collections.Generic;

namespace Beyond14
{
    public static class TileMergerDatabase
    {
        private const long CROSS_MASK = 0b11111_00000_11111_00000_11111_00000_11111_00000_11111;
        private const long PLUS_MASK = 0b00000_11111_00000_11111_00000_11111_00000_11111_00000_11111;

        private const short MAX_CACHED_TILE = 21;

        public static bool Initialized { get; private set; }

        private static Dictionary<long, MergeResult> PlusResolve { get; } = new Dictionary<long, MergeResult>();
        private static Dictionary<long, MergeResult> CrossResolve { get; } = new Dictionary<long, MergeResult>();

        public static List<long> GetAreaPossibilities(bool plus)
        {
            var results = new List<long>(33554432);
            var bits = GameHelper.MaxPreCalcTileBits;
            int si = plus ? bits : 0;
            int sj = bits * 2 + (plus ? bits : 0);
            int sk = bits * 4;
            int sl = bits * 5 + (!plus ? bits : 0);
            int sm = bits * 7 + (!plus ? bits : 0);
            for (long i = 0; i < GameHelper.MaxPreCalcTile; i++)
            {
                for (long j = 0; j < GameHelper.MaxPreCalcTile; j++)
                {
                    for (long k = 0; k < GameHelper.MaxPreCalcTile; k++)
                    {
                        for (long l = 0; l < GameHelper.MaxPreCalcTile; l++)
                        {
                            for (long m = 0; m < GameHelper.MaxPreCalcTile; m++)
                            {
                                long result = 0;
                                result |= i << sm;
                                result |= j << sl;
                                result |= k << sk;
                                result |= l << sj;
                                result |= m << si;
                                results.Add(result);
                            }
                        }
                    }
                }
            }
            return results;
        }

        /*public static MergeResult GetPlaceResult(long area)
        {
            return GetPlaceResultRecursive(new MergeResult(area, 0, 0, 0), 0);
        }

        /*public static void Initialize()
        {
            if (Initialized)
                throw new InvalidOperationException($"{nameof(Initialize)} was called already");

            var plusList = GetAreaPossibilities(true);
            var crossList = GetAreaPossibilities(false);

            foreach (var area in plusList)
            {
                if (GameHelper.GetMaxTileInArea((UInt128)area) > MAX_CACHED_TILE)
                    continue;
                var mr = CalculateMergeResult(area);
                PlusResolve.Add(area, mr);
            }
            foreach (var area in crossList)
            {
                if (GameHelper.GetMaxTileInArea((UInt128)area) > MAX_CACHED_TILE)
                    continue;
                var mr = CalculateMergeResult(area);
                CrossResolve.Add(area, mr);
            }

            Initialized = true;
        }

        private static MergeResult CalculateMergeResult(long area)
        {
            short[,] field = new short[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    field[i, j] = GameHelper.GetTileInArea((UInt128)area, i, j, 3);
                }
            }
            short center = field[1, 1];
            short tilesMerged = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1)
                        continue;
                    if (field[i, j] == field[1, 1])
                    {
                        center = (short)(field[i, j] + 1);
                        field[i, j] = 0;
                        tilesMerged++;
                    }
                }
            }
            var resultField = GameHelper.GetAreaFromArray(field, GameHelper.MaxPreCalcTileBits);
            var mergeResult = new MergeResult((long)resultField, 0, center, tilesMerged);
            return mergeResult;
        }


        private static MergeResult GetPlaceResultRecursive(MergeResult prevMergeResult, short level)
        {
            var plus = prevMergeResult.Field & PLUS_MASK;
            var cross = prevMergeResult.Field & CROSS_MASK;

            var plusResolve = PlusResolve[plus];
            var crossResolve = CrossResolve[cross];
            var resultField = plusResolve.Field | crossResolve.Field;
            if (resultField == prevMergeResult.Field)
                return prevMergeResult;
            var totalMerged = (short)(plusResolve.NumberOfTilesMerged + crossResolve.NumberOfTilesMerged);
            var nextMergeResult = new MergeResult(resultField, 0, plusResolve.MergedTileResult, totalMerged);

            return GetPlaceResultRecursive(nextMergeResult, (short)(level + 1));
        }*/
    }
}