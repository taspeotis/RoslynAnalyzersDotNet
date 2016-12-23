using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynAnalyzersDotNet.CodeFixProviders
{
    internal abstract class BaseCodeFixProvider : CodeFixProvider
    {
        // TODO: Fancy name sorting, insert attribute alphabetically
        // http://stackoverflow.com/a/37598072/242520
        protected static SyntaxNode AddAttribute(MethodDeclarationSyntax methodDeclaration, string attributeName)
        {
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
            var singletonSeparatedList = SyntaxFactory.SingletonSeparatedList(attribute);
            var attributeList = SyntaxFactory.AttributeList(singletonSeparatedList);
            var attributeLists = methodDeclaration.AttributeLists.Add(attributeList);

            return methodDeclaration.WithAttributeLists(attributeLists);
        }
    }
}