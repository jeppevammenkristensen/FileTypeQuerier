using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FileQuerier.CoreLibrary.Compilation
{
    public class MethodBlockTransformer
    {
        private readonly Compiler _compiler = new Compiler();

        public CompilationResult Transform(CsharpRoslynCompilationInformation information, params string[] lines)
        {
            var main = information.BaseCompliationRoot.DescendantNodesAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Identifier.ToString() == "Run")
                .SelectMany(x => x.DescendantNodes().OfType<BlockSyntax>()).First();

            CompilationUnitSyntax modified = information.BaseCompliationRoot
            .ReplaceNode(main, main.AddStatements(lines.Select(x => SyntaxFactory.ParseStatement(x)).ToArray()))
            .NormalizeWhitespace();

            return _compiler.Compile(new CsharpRoslynCompilationInformation(modified, information.References));
        } 
    }
}