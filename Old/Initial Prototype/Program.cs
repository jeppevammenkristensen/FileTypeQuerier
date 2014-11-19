using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApplication2.Extensions;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using System.Reflection;

namespace ConsoleApplication2
{
    public class Person
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    class Program
    {
        public string MyName { get; set; }

        static void Main(string[] args)
        {
            var res = JsonConvert.DeserializeObject<Person>("{'firstName':'Jeppe', 'lastName' : 'Kristensen'}");

            var converter = new JsonConverter();
            var parsedObject = converter.ParseJson("{ contents : [{'firstName':'Jeppe', 'lastName' : 'Kristensen'}, {'firstName':'Lene', 'lastName' : 'Vestergaard'}] }");
            
            var parser = new CsharpRoslynParser();
            parser.Parse(parsedObject);
                //System.Console.WriteLine(Converter.ConvertJson("test"));
        }
    }

    public class CsharpRoslynParser
    {
        public void Parse(ParsedCommonInformation commonInformation)
        {
            var compilationRoot = CompilationUnit()
                .WithUsings(GenerateUsings(commonInformation))
                .WithMembers(VisitNamespace(commonInformation)).NormalizeWhitespace();

            var compilation = CSharpCompilation.Create("HelloTest")

                    .AddReferences(new MetadataFileReference(
                        typeof(object).Assembly.Location), 
                        new MetadataFileReference(typeof(JsonReader).Assembly.Location), new MetadataFileReference(typeof(Enumerable).Assembly.Location))
                    .AddSyntaxTrees(new SyntaxTree[] { compilationRoot.SyntaxTree });

            using (var memoryStream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(memoryStream);

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(compilationRoot.NormalizeWhitespace().ToString());

                foreach (var res in result.Diagnostics)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(res);
                }
            }

            Console.ForegroundColor = ConsoleColor.White;

            int i = 0;
            var main = compilationRoot.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ToString() == "Run").SelectMany(x => x.DescendantNodes().OfType<BlockSyntax>()).First();
            CompilationUnitSyntax modified;            

            while (true)
            {
                Console.WriteLine("Enter code");
                var codeString = Console.ReadLine();

                modified = compilationRoot
                .ReplaceNode(main, main.AddStatements(ParseStatement(codeString)))
                .NormalizeWhitespace();


                compilation = CSharpCompilation.Create("HelloTest")
                   .AddReferences(new MetadataFileReference(
                        typeof(object).Assembly.Location),new MetadataFileReference(typeof(JsonReader).Assembly.Location), new MetadataFileReference(typeof(Enumerable).Assembly.Location))
                   .AddSyntaxTrees(new SyntaxTree[] { modified.SyntaxTree })
                   .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary));

                

                using (var stream = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(stream);

                    if (result.Success)
                    {
                        var assembly = Assembly.Load(stream.GetBuffer());
                        var type = assembly.GetType("Custom.ShowRunner");
                        var method = type.GetMethod("Run");

                        var rootType = assembly.GetType("Custom.Root");


                        method.Invoke(null, new object[] { JsonConvert.DeserializeObject( commonInformation.OriginalSource, rootType )  });
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine(modified.ToString());

                        foreach (var res in result.Diagnostics)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(res);
                        };

                        Console.ForegroundColor = ConsoleColor.White;
                    }



                }

            }
        }

        private SyntaxList<UsingDirectiveSyntax> GenerateUsings(ParsedCommonInformation commonInformation)
        {
            return List<UsingDirectiveSyntax>(
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
            return SyntaxFactory.ClassDeclaration(
             @"ShowRunner")
         .WithModifiers(
             SyntaxFactory.TokenList(
                 new[]{
                    SyntaxFactory.Token(
                        SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(
                        SyntaxKind.StaticKeyword)}))       
         .WithMembers(
             SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                 SyntaxFactory.MethodDeclaration(
                     PredefinedType(Token(SyntaxKind.VoidKeyword)),
                     Identifier(
                         @"Run"))
                 .WithModifiers(
                     TokenList(new[] {
                         Token(
                             SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)}))
                 .WithParameterList(ParameterList(SingletonSeparatedList<ParameterSyntax>(
                        Parameter(Identifier("root"))
                        .WithType(IdentifierName("Root"))
                     )))
                 .WithBody(
                     Block(
                            
                         ))));
        }

             

        private SyntaxList<MemberDeclarationSyntax> VisitClasses(ParsedCommonInformation commonInformation)
        {
            return List<MemberDeclarationSyntax>(commonInformation.DependentClasses.Select(x => x.Value).Concat(new List<CommonClass>() { commonInformation.RootClass}).Select(x => VisitClass(x, commonInformation)).Concat(new List<ClassDeclarationSyntax>() { GenerateShowRunner(commonInformation)}));            
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
                    return EnsureList(commonProperty,PredefinedType(Token(SyntaxKind.ObjectKeyword)));
                case CommonType.StringType:
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
            else
                return obj;
        }
    }

    public class CommonClass
    {
        public List<CommonProperty> Properties { get; } = new List<CommonProperty>();

        public string Id { get; set; }

        public string Name { get; set; } 

        public void AddProperties(params CommonProperty[] properties)
        {
            foreach (var item in properties.Except(Properties))
            {
                Properties.Add(item);
            }
        }
    }

    public class CommonProperty
    {

        public bool IsArray { get; set; }
        public CommonType Type { get; set; }
        public string Name { get; set; }
        public bool IsCustomType { get; internal set; }
        public string CustomTypeId { get; internal set; }

        public override bool Equals(object obj)
        {
           if (obj == null)
            {
            return false;
            }

            var parsed = obj as CommonProperty;
            if (parsed == null)
            {
                return false;
            }

            return Equals(parsed);
        }

        public bool Equals(CommonProperty p)
        {
            return string.Equals(p.Name, Name, StringComparison.InvariantCultureIgnoreCase) && p.Type == Type;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }

    public enum CommonType
    {
        Undetermined,
        StringType,
        Custom,
        Integer,
        Float,
        Boolean,
        Date,
        Bytes
    }    
}
