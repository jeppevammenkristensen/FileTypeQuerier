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

namespace ConsoleApplication2
{
    class Program
    {
        public string MyName { get; set; }

        static void Main(string[] args)
        {               
            var converter = new JsonConverter();
            var parsedObject = converter.ParseJson("{'persons':[{ 'age' : 5 },{ 'age' : 10, nameDetails:{'firstName':'Jeppe', 'lastName':'Kristensen', 'yearsActive':[3,5,7]}}]}");
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
                .WithMembers(VisitNamespace(commonInformation));

            var compilation = CSharpCompilation.Create("HelloTest")

                    .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location), new MetadataFileReference(typeof(JsonReader).Assembly.Location))
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
             
        }

        private SyntaxList<UsingDirectiveSyntax> GenerateUsings(ParsedCommonInformation commonInformation)
        {
            return List<UsingDirectiveSyntax>(
                new[] {
                    UsingDirective(QualifiedName(QualifiedName(IdentifierName(@"System"), IdentifierName(@"Collections")), IdentifierName("Generic"))),
                    UsingDirective(QualifiedName(IdentifierName(@"Newtonsoft"), IdentifierName(@"Json")))
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
                         @"Main"))
                 .WithModifiers(
                     TokenList(new[] {
                         Token(
                             SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)}))
                 .WithBody(
                     Block(
                            LocalDeclarationStatement(
                                VariableDeclaration(IdentifierName("var"))
                                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        VariableDeclarator("roots")
                                        .WithInitializer(EqualsValueClause(
                                            InvocationExpression(
                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("JsonConvert"), GenericName(Identifier("DeserializeObject")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(commonInformation.RootClass.Name))))
                                           ))                                           
                                           .WithArgumentList(
                                                ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(                                                    
                                                        LiteralExpression(SyntaxKind.StringLiteralExpression,Literal(TriviaList(), "\"\{commonInformation.OriginalSource}}\"", "\"\{commonInformation.OriginalSource}}\"", TriviaList()))
                                                    )
                                                )
                                            )
                                        )
                                      )
                                   )         
                                )
                              )
                            )
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
