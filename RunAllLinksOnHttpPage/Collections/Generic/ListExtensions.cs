using System;
using System.Collections.Generic;

namespace RunAllLinksOnHttpPage.Collections.Generic
{
    public static class ListExtensions
    {
        private static readonly Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool EndWithPart<T>(this IList<T> list, string part)
        {
            foreach (var item in list)
            {
                if (item.ToString().EndsWith(part, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}