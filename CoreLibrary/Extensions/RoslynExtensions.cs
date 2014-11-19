using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FileQuerier.CoreLibrary.Extensions
{
    public static class RoslynExtensions
    {
        public static SyntaxList<T> ToSyntaxList<T>(this T node) where T : SyntaxNode
        {
            return List(new System.Collections.Generic.List<T>() { node });
        }
    }
}
