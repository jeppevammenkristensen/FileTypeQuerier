using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FileQuerier.Utilities
{
    public class TypeDumper
    {
        public TypeDumper(int maxDepth)
        {
            MaxDepth = maxDepth;
        }

        public int MaxDepth { get; }


        public void Dump(Type type, Object source)
        {
            VisitRecursive(source, 1);
        }

        public void VisitRecursive(Object source, int currentLevel)
        {
            if (currentLevel > MaxDepth)
            {
                Console.Write("->");
                return;
            }

            if (source == null)
            {
                Console.WriteLine("[Null]");
                return;
            }

            var type = source.GetType();

            if (type.IsPrimitive || type == typeof(string) ||
                    type == typeof(DateTime))
            {
                Console.Write(" \{source} ");
                return;
            }

            if (IsTypeACollection(type))
            {
                Console.Write("[");
                foreach (var obj in (IEnumerable)source)
                {
                    VisitRecursive(obj, currentLevel);
                    Console.Write(",");
                }
                Console.WriteLine("]");
            }
            else
            {
                Console.WriteLine();
                VisitClass(source, currentLevel);
            }
        }

        private void VisitClass(object source, int currentLevel)
        {
            var type = source.GetType();

            foreach (var property in type.GetProperties())
            {
               Console.Write(property.Name + ":");

                VisitRecursive(property.GetValue(source, null),currentLevel + 1);
                Console.WriteLine();
            }
        }

        //private void VisitCollectionType(PropertyInfo property, object source, int currentLevel)
        //{
        //    Console.WriteLine("{0}:[", property.Name);

        //    foreach (var obj in (IEnumerable)property.GetValue(source, null))
        //    {
        //        VisitRecursive(obj.GetType(), obj, currentLevel + 1);
        //    }
        //    Console.WriteLine("]");
        //}

        //private void VisitClassProperty(PropertyInfo property, object source, int currentLevel)
        //{
        //    Console.WriteLine("{0}", property.Name);
        //    VisitRecursive(property.PropertyType, property.GetValue(source, null), currentLevel + 1);
        //}

        //private void VisitPrimitiveType(PropertyInfo property, object source, int currentLevel)
        //{
        //    var value = property.GetValue(source, null);
        //    Console.WriteLine("\{property.Name}:\{value}");
        //}

        //Taken from http://stackoverflow.com/questions/9434825/determine-if-a-property-is-a-kind-of-array-by-reflection
        public bool IsTypeACollection(Type property)
        {
            return property.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }
    }
}