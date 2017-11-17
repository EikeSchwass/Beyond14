namespace Beyond14
{
    public struct Move
    {
        public short X { get; }
        public short Y { get; }

        public Move(short x, short y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Move other)
        {
            return X == other.X && Y == other.Y;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Move && Equals((Move)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Move left, Move right)
        {
            return !left.Equals(right);
        }
        public override string ToString()
        {
            return $"{X}|{Y}";
        }
    }
}