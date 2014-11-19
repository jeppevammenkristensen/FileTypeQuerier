using System;
using System.Collections.Generic;
using System.Linq;
using FileQuerier.CoreLibrary.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Newtonsoft.Json;

namespace FileQuerier.CoreLibrary.Compilation
{
    public class CsharpRoslynSyntaxTreeGenerator
    {
        public CsharpRoslynCompilationInformation Parse(ParsedCommonInformation commonInformation)
        {
            var compilationRoot = CompilationUnit()
                .WithUsings(GenerateUsings(commonInformation))
                .WithMembers(VisitNamespace(commonInformation)).NormalizeWhitespace();

            var references = new[]
            {
                MetadataReference.CreateFromAssembly(typeof (object).Assembly),
                MetadataReference.CreateFromAssembly(typeof (JsonReader).Assembly),
                MetadataReference.CreateFromAssembly(typeof (Enumerable).Assembly)
            };

            return new CsharpRoslynCompilationInformation(compilationRoot, references);

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

        private SyntaxList<UsingDirectiveSyntax> GenerateUsings(ParsedCommonInformation commonInformation)
        {
            return List(
                new[] {
                    UsingDirective(IdentifierName(@"System")),
                    UsingDirective(QualifiedName(QualifiedName(IdentifierName(@"System"), IdentifierName(@"Collections")), IdentifierName("Generic"))),
                    UsingDirective(QualifiedName(IdentifierName(@"Newtonsoft"), IdentifierName(@"Json"))),
                    UsingDirective(QualifiedName(IdentifierName(@"System"), IdentifierName(@"Linq")))
           });
        }

        private SyntaxList<MemberDeclarationSyntax> VisitNamespace(ParsedCommonInformation commonInformation)
        {
            return NamespaceDeclaration(IdentifierName("Custom"))
                    .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                    .WithMembers(List(VisitClasses(commonInformation)
                   )).WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken)).ToSyntaxList<MemberDeclarationSyntax>();
        }

        private ClassDeclarationSyntax GenerateShowRunner(ParsedCommonInformation commonInformation)
        {
            return ClassDeclaration(
             @"ShowRunner")
         .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),Token(SyntaxKind.StaticKeyword)))
         .WithMembers(
             SingletonList<MemberDeclarationSyntax>(
                 MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),Identifier(@"Run"))
                 .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                 .WithParameterList(ParameterList(SingletonSeparatedList<ParameterSyntax>(
                        Parameter(Identifier("root")).WithType(IdentifierName("Root"))
                     )))
                 .WithBody(
                     Block(

                         ))));
        }



        private SyntaxList<MemberDeclarationSyntax> VisitClasses(ParsedCommonInformation commonInformation)
        {
            return List<MemberDeclarationSyntax>(commonInformation.DependentClasses.Select(x => x.Value).Concat(new List<CommonClass>() { commonInformation.RootClass }).Select(x => VisitClass(x, commonInformation)).Concat(new List<ClassDeclarationSyntax>() { GenerateShowRunner(commonInformation) }));
        }

        private ClassDeclarationSyntax VisitClass(CommonClass commonClass, ParsedCommonInformation commonInformation)
        {
            return ClassDeclaration(commonClass.Name)
                 .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
                 .WithMembers(List<MemberDeclarationSyntax>(commonClass.Properties.Select(x => VisitProperty(x))));
        }

        private PropertyDeclarationSyntax VisitProperty(CommonProperty commonProperty)
        {
            return PropertyDeclaration(GetTypeSyntax(commonProperty), commonProperty.Name)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                 .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                 .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        private TypeSyntax GetTypeSyntax(CommonProperty commonProperty)
        {
            switch (commonProperty.Type)
            {
                case CommonType.Undetermined:
                    return EnsureList(commonProperty, PredefinedType(Token(SyntaxKind.ObjectKeyword)));
                case CommonType.String:
                    return EnsureList(commonProperty, PredefinedType(Token(SyntaxKind.StringKeyword)));
                case CommonType.Custom:
                    return EnsureList(commonProperty, IdentifierName(commonProperty.CustomTypeId));
                case CommonType.Integer:
                    return EnsureList(commonProperty, PredefinedType(Token(SyntaxKind.IntKeyword)));
                case CommonType.Float:
                    return EnsureList(commonProperty, PredefinedType(Token(SyntaxKind.DecimalKeyword)));
                case CommonType.Boolean:
                    return EnsureList(commonProperty, PredefinedType(Token(SyntaxKind.BoolKeyword)));
                case CommonType.Date:
                    return EnsureList(commonProperty, IdentifierName("DateTime"));
                case CommonType.Bytes:
                    return EnsureList(commonProperty, IdentifierName("??"));
                default:
                    throw new InvalidOperationException("Blaaaaah");
                    break;
            }
        }

        private TypeSyntax EnsureList(CommonProperty commonProperty, TypeSyntax obj)
        {
            if (commonProperty.IsArray)
                return GenericName(Identifier(@"List"))
                            .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(obj)));
            return obj;
        }
    }
}
