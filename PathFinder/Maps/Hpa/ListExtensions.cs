using System.Collections.Generic;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public static class ListExtensions
    {
        public static T[] Backwards<T>(this List<T> list)
        {
            var n = list.Count;
            T[] reverseList = new T[n];

            for (var i = 0; i < n; i++)
            {
                reverseList[n - 1 - i] = list[i];
            }

            return reverseList;
        }

        public static T[] Backwards<T>(this T[] list)
        {
            var n = list.Length;

            T[] reverseList = new T[n];

            for (var i = 0; i < n; i++)
            {
                reverseList[n - 1 - i] = list[i];
            }

            return reverseList;
        }
    }
}