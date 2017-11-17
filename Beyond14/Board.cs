namespace Beyond14
{
    public struct Board
    {
        public UInt128 Field { get; }
        public ushort NextTile { get; }
        public ushort AfterNextTile { get; }

        public Board(UInt128 field, ushort nextTile, ushort afterNextTile)
        {
            Field = field;
            NextTile = nextTile;
            AfterNextTile = afterNextTile;
        }

        public UInt128 PlaceTile(short x, short y)
        {
            return PlaceTile(x, y, NextTile);
        }

        private UInt128 PlaceTile(short x, short y, ushort tile)
        {
            int index = y * 4 + x;
            UInt128 result = Field | ((UInt128)tile << (index * GameHelper.MaxPreCalcTileBits));
            return result;
        }
    }
}