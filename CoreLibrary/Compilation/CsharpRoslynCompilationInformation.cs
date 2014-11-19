using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FileQuerier.CoreLibrary.Compilation
{
    public class CsharpRoslynCompilationInformation
    {
        public CompilationUnitSyntax BaseCompliationRoot { get;  }
        public MetadataReference[] References { get; }

        public CsharpRoslynCompilationInformation(CompilationUnitSyntax compilationRoot, MetadataReference[] references)
        {
            BaseCompliationRoot = compilationRoot;
            References = references;
        }
    }
}