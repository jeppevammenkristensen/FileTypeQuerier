using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace FileQuerier.CoreLibrary.Compilation.Csharp
{
    public class CompilationResult : IDisposable
    {
        public MemoryStream MemoryStream { get; }
        public string CodeAsString { get; set; }

        public EmitResult CompiledResult { get; }

        public CompilationResult(EmitResult result, MemoryStream memoryStream)
        {
            MemoryStream = memoryStream;
            SuccesfulCompilation = result.Success;
            if (!SuccesfulCompilation)
            {
                Warnings = result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Warning).ToList();
                Errors = result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            }

            CompiledResult = result;
        }

        public List<Diagnostic> Errors { get; set; }

        public List<Diagnostic> Warnings { get; set; }

        public bool SuccesfulCompilation { get; }

        public CompilationResult WithCodeAsString(string codeAsString)
        {
            CodeAsString = codeAsString;
            return this;
        }

        public void Dispose()
        {
            MemoryStream.Dispose();
        }
    }

    public class Compiler
    {
        public CompilationResult Compile(CsharpRoslynCompilationInformation information)
        {
            var compilation = CSharpCompilation.Create("HelloTest")
                .AddReferences(information.References)
                .AddSyntaxTrees(information.BaseCompliationRoot.SyntaxTree)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var memoryStream = new MemoryStream();
            EmitResult emitResult;
            try
            {
                emitResult = compilation.Emit(memoryStream);
            }
            catch (Exception)
            {
                memoryStream.Dispose();
                throw;
            }

            return new CompilationResult(emitResult, memoryStream)
                                .WithCodeAsString(information.BaseCompliationRoot.ToString());

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



            //    using (var stream = new MemoryStream())
            //    {
            //        EmitResult result = compilation.Emit(stream);

            //        if (result.Success)
            //        {
            //            var assembly = Assembly.Load(stream.GetBuffer());
            //            var type = assembly.GetType("Custom.ShowRunner");
            //            var method = type.GetMethod("Run");

            //            var rootType = assembly.GetType("Custom.Root");


            //            method.Invoke(null, new object[] { JsonConvert.DeserializeObject(commonInformation.OriginalSource, rootType) });
            //        }
            //        else
            //        {
            //            Console.ForegroundColor = ConsoleColor.Green;

            //            Console.WriteLine(modified.ToString());

            //            foreach (var res in result.Diagnostics)
            //            {
            //                Console.ForegroundColor = ConsoleColor.Red;
            //                Console.WriteLine(res);
            //            };

            //            Console.ForegroundColor = ConsoleColor.White;
            //        }



            //    }

            //}
        }
    }
}