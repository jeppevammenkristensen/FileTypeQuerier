using System;
using System.Collections.Generic;
using System.IO;
using FileQuerier.Utilities;
using NUnit.Framework;

namespace UnitTests.Utilities
{
    public class Person
    {
        public Name Name { get; set; }
        public string BirthTown { get; set; }
        public DateTime Birthday { get; set; }
        public List<string> Titles { get; set; }

        public Name[] Aliases { get; set; }
    }

    public class Name
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }


    [TestFixture]
    public class ExtensionsTester
    {
        [Test]
        public void Dump_CanDumpGeneralExample()
        {
            var generateTestData = TestData();
            var oldWriter = Console.Out;
            string result = string.Empty;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                generateTestData.Dump();
                result = writer.ToString();
            }

            Console.SetOut(oldWriter);
            Console.WriteLine(result);

        }

        private Person TestData()
        {
            return new Person
            {
                BirthTown = "Aalborg",
                Aliases = new[]
                {
                    new Name()
                    {
                        FirstName = "El",
                        LastName = "Boberino"
                    },
                    new Name()
                    {
                        FirstName = "Bill",
                        LastName = "Mac Laughalot"
                    }
                },
                Name = new Name()
                {
                    FirstName = "Jeppe",
                    LastName = "Kristensen"
                },
                Birthday = new DateTime(1979, 10, 12),
                Titles = new List<string>() { "Sir", "Master Jedi" }
            };
        }
    }
}