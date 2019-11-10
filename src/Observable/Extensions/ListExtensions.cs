using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public static class ListExtensions
    {
        public static List<T> Slice<T>(this List<T> list)
        {
            return list.Slice(0);
        }

        public static List<T> Slice<T>(this List<T> list, int begin)
        {
            return list.Slice(begin, list.Count);
        }

        public static List<T> Slice<T>(this List<T> list, int begin, int end)
        {
            var count = list.Count;

            if (begin < 0)
            {
                begin += count;
            }

            if (begin > count)
            {
                return new List<T>();
            }

            if (end < 0)
            {
                end += count;
            }

            if (end > count)
            {
                end = count;
            }

            var range = end - begin;

            return range > 0 ? list.GetRange(begin, range) : new List<T>();
        }

        public static List<T> Splice<T>(this List<T> list, int start)
        {
            return list.Splice(start, list.Count + 1);
        }

        public static List<T> Splice<T>(this List<T> list, int start, int deleteCount, params T[] items)
        {
            if (Math.Abs(start) > list.Count)
            {
                start = list.Count;
            }
            else if (start < 0)
            {
                start += list.Count;
            }

            if (deleteCount > (list.Count - start))
            {
                deleteCount = (list.Count - start);
            }

            var deleted = list.GetRange(start, deleteCount);

            list.RemoveRange(start, deleteCount);

            list.InsertRange(start, items);

            return deleted;
        }
    }
}
