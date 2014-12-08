using System;
using System.IO;
using FileQuerier.CoreLibrary.Compilation;
using FileQuerier.CoreLibrary.Compilation.Csharp;
using FileQuerier.CoreLibrary.JSON;
using FileQuerier.CoreLibrary.Runners;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new JsonToCommonFormatConverter();
            var parsed = converter.ParseJson(File.ReadAllText("FileSample.json"));
            var generator = new CsharpRoslynCompilationInformationGenerator();
            var compilationInformation = generator.Parse(parsed);
            var compiler = new Compiler();
            var transformer = new MethodBlockTransformer();
            var runner = new Runner();
            CompilationResult result;
            using (result = compiler.Compile(compilationInformation))
            {
                OutputCompilationValues(result, true);
            }

            while (true)
            {
                Console.WriteLine("Write a code line");
                Console.WriteLine("You can use Each (for instance (...Each(x => x.name.Dump())");
                Console.WriteLine("Or you can use Dump on objects");
                Console.Write(">");
                using (var compilation = transformer.Transform(compilationInformation, Console.ReadLine()))
                {
                    OutputCompilationValues(compilation, false);
                    if (compilation.SuccesfulCompilation)
                    {
                        try
                        {
                            runner.Run(compilation, parsed);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }

        private static void OutputCompilationValues(CompilationResult result, bool verboseSuccess = true)
        {
            if (result.SuccesfulCompilation)
            {
                if (!verboseSuccess)
                    return;

                Console.WriteLine("Success");

                Console.Write(result.CodeAsString);
            }
            else
            {
                Console.Write(result.CodeAsString);

                foreach (var diagnostic in result.Warnings)
                {
                    Console.WriteLine(diagnostic);
                }

                foreach (var diagnostic in result.Errors)
                {
                    Console.WriteLine(diagnostic);
                }
            }
        }
    }
}
