using System;
using System.Threading;

namespace Beyond14
{
    public static class ThreadStaticRandom
    {
        private static int seed = Environment.TickCount;

        private static ThreadLocal<Random> Random { get; } = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
            return Random.Value.Next();
        }
        public static int Next(int max)
        {
            return Random.Value.Next(max);
        }
        public static int Next(int min, int max)
        {
            return Random.Value.Next(min, max);
        }
        public static double NextDouble()
        {
            return Random.Value.NextDouble();
        }
        public static double NextDouble(double max)
        {
            return Random.Value.NextDouble() * max;
        }
        public static double NextDouble(double min, double max)
        {
            return Random.Value.NextDouble() * (max - min) + min;
        }
    }
}