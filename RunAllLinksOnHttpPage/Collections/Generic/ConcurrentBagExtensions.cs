using System;
using System.Collections.Concurrent;

namespace RunAllLinksOnHttpPage.Collections.Generic
{
    public static class ConcurrentBagExtensions
    {
        public static bool EndWithPart<T>(this ConcurrentBag<T> list, string part)
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
