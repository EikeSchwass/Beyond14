using System;
using System.Collections.Generic;
using System.Linq;

namespace MCTS
{
    public static class LinqExtensions
    {
        public static T MaxElement<T>(this IEnumerable<T> source, Func<T, double> selector)
        {
            List<T> bestElements = new List<T>();
            double bestRating = double.MinValue;

            foreach (var element in source)
            {
                double rating = selector(element);
                if (Math.Abs(rating - bestRating) < 0.0001)
                {
                    bestElements.Add(element);
                }
                else if (rating > bestRating)
                {
                    bestRating = rating;
                    bestElements.Clear();
                    bestElements.Add(element);
                }
            }
            var random = ThreadStaticRandom.Next(0, bestElements.Count);
            return bestElements[random];
        }
        public static T MinElement<T>(this IEnumerable<T> source, Func<T, double> selector)
        {
            T best = default(T);
            double bestRating = double.MaxValue;

            foreach (var element in source)
            {
                double rating = selector(element);
                if (rating <= bestRating)
                {
                    best = element;
                    bestRating = rating;
                }
            }
            return best;
        }
        public static T RandomlySelect<T>(this IEnumerable<KeyValuePair<T, double>> source)
        {
            var list = source.ToList();
            double sum = 0;
            foreach (var kvp in list)
            {
                sum += kvp.Value;
            }
            var random = ThreadStaticRandom.NextDouble(0, sum);
            sum = 0;
            foreach (var kvp in list)
            {
                sum += kvp.Value;
                if (sum >= random)
                    return kvp.Key;
            }
            return list.Last().Key;
        }
    }
}