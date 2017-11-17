namespace Beyond14
{
    public struct MergeResult
    {
        public long Field { get; }
        public int Points { get; }
        public short MergedTileResult { get; }
        public short NumberOfTilesMerged { get; }

        public MergeResult(long field, int points, short mergedTileResult, short numberOfTilesMerged)
        {
            Field = field;
            Points = points;
            MergedTileResult = mergedTileResult;
            NumberOfTilesMerged = numberOfTilesMerged;
        }
    }
}