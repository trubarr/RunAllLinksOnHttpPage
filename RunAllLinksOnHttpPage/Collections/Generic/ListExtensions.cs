using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace RunAllLinksOnHttpPage.Collections.Generic
{
    public static class ListExtensions
    {
        private static readonly Random Rng = new Random();
        public static void QuickShuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                var k = (box[0] % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void ProperQuickShuffle<T>(this IList<T> list, Random rnd)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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