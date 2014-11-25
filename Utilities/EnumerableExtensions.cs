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
            RecursiveDump(typeof(T), source,1, depth);
            return source;
        }

        private static void RecursiveDump(Type type, object source, int currentLevel, int depth)
        {
            if (currentLevel > depth)
                return;
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(source, null);
                    Console.WriteLine("\{property.Name}:\{value}");
                }
                else
                {
                    Console.WriteLine("{0}", property.Name);

                    try
                    {
                        RecursiveDump(property.PropertyType, property.GetValue(source, null), currentLevel + 1,
                         depth);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed (x x)");
                    }
                }
            }

        }
    }
}
