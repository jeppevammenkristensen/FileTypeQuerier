using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FileQuerier.CoreLibrary.Compilation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;

namespace FileQuerier.CoreLibrary.Runners
{
    public class Runner
    {
        public void Run(CompilationResult compilationResult, ParsedCommonInformation parsedInformation)
        {
            ValidateInput(compilationResult);

            var byteArray = compilationResult.MemoryStream.GetBuffer();
            var assembly = Assembly.Load(byteArray);
            var type = assembly.GetType("Custom.ShowRunner");
            var method = type.GetMethod("Run");
            var rootType = assembly.GetType("Custom.Root");
            method.Invoke(null,new object[] {JsonConvert.DeserializeObject(parsedInformation.OriginalSource, rootType)});
                
            

            //var compilation = CSharpCompilation.Create("HelloTest")

                //        .AddReferences(new MetadataReference(
                //            typeof(object).Assembly.Location),
                //            new MetadataReference(typeof(JsonReader).Assembly.Location), new MetadataReference(typeof(Enumerable).Assembly.Location))
                //        .AddSyntaxTrees(new SyntaxTree[] { compilationRoot.SyntaxTree });

                //using (var memoryStream = new MemoryStream())
                //{
                //    EmitResult result = compilation.Emit(memoryStream);

                //    Console.ForegroundColor = ConsoleColor.Green;

                //    Console.WriteLine(compilationRoot.NormalizeWhitespace().ToString());

                //    foreach (var res in result.Diagnostics)
                //    {
                //        Console.ForegroundColor = ConsoleColor.Red;
                //        Console.WriteLine(res);
                //    }
                //}

                //Console.ForegroundColor = ConsoleColor.White;

                //int i = 0;
                //var main = compilationRoot.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ToString() == "Run").SelectMany(x => x.DescendantNodes().OfType<BlockSyntax>()).First();
                //CompilationUnitSyntax modified;

                //while (true)
                //{
                //    Console.WriteLine("Enter code");
                //    var codeString = Console.ReadLine();

                //    modified = compilationRoot
                //    .ReplaceNode(main, main.AddStatements(ParseStatement(codeString)))
                //    .NormalizeWhitespace();


                //    //compilation = CSharpCompilation.Create("HelloTest")
                //    //   .AddReferences(MetadataReference.CreateFromAssembly(typeof(object).Assembly), 
                //    //       MetadataReference.CreateFromAssembly(typeof(JsonReader).Assembly),MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly))
                //    //   .AddSyntaxTrees( modified.SyntaxTree)
                //    //   .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));



                //   



                //    }

                //}
            }
        /// <summary>
        /// Validates the input and throws and exception if it did not build
        /// </summary>
        /// <param name="compilationResult"></param>
        private void ValidateInput(CompilationResult compilationResult)
        {
            if (!compilationResult.WasSuccesful)
                throw new InvalidOperationException(
                    "The code did not compile has \{compilationResult.Errors.Count} errors <newline> \{compilationResult.CodeAsString}");
        }
    }
}