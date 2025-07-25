using System;
using System.Collections.Generic;

namespace MergeCSProjectsToNuspec.Library.ExtensionMethods
{
        public static class ListExtensionMethods
        {
            public static void Merge<T>(this IList<T> target, params IList<T>[] lists)
            {
                foreach (IList<T> list in lists)
                {
                    foreach (T item in list)
                    {
                        target.Add(item);
                    }
                }
            }
        }
}
