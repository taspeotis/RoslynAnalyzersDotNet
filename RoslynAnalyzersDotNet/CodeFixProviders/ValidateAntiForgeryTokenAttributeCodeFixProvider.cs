using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynAnalyzersDotNet.DiagnosticAnalyzers;

namespace RoslynAnalyzersDotNet.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValidateAntiForgeryTokenAttributeCodeFixProvider))]
    [Shared]
    [UsedImplicitly]
    internal sealed class ValidateAntiForgeryTokenAttributeCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly ImmutableArray<string> DiagnosticIds =
            ImmutableArray.Create(ValidateAntiForgeryTokenAttributeDiagnosticAnalyzer.DiagnosticId);

        public override ImmutableArray<string> FixableDiagnosticIds => DiagnosticIds;

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var nodes = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf();
            var declaration = nodes.OfType<MethodDeclarationSyntax>().FirstOrDefault();

            const string title = "Add [ValidateAntiForgeryToken]";

            Func<CancellationToken, Task<Document>> createChangedDocument =
                ct => AddValidateAntiForgeryTokenAsync(document, declaration, ct);

            var codeAction = CodeAction.Create(title, createChangedDocument, title);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> AddValidateAntiForgeryTokenAsync(
            Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            // This seems redundant given we already have the root in RegisterCodeFixesAsync.
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            return document.WithSyntaxRoot(root.ReplaceNode(
                methodDeclaration, AddAttribute(methodDeclaration, "ValidateAntiForgeryToken")));
        }
    }
}