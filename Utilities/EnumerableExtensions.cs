using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FileQuerier.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Each<T>(this IEnumerable<T> source,Action<T> processor)
        {
            foreach (var result in source)
            {
                processor(result);
            }

            return source;
        }

        public static T Dump<T>(this T source, int depth = 2)
        {
            var typeDumper = new TypeDumper(depth);
            typeDumper.VisitRecursive(source,0);
            return source;
        }
    }
}
