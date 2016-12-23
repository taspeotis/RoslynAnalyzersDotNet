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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AuthorizeAttributeCodeFixProvider))]
    [Shared]
    [UsedImplicitly]
    internal sealed class AuthorizeAttributeCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly ImmutableArray<string> DiagnosticIds =
            ImmutableArray.Create(AuthorizeAttributeDiagnosticAnalyzerDiagnosticAnalyzer.DiagnosticId);

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

            context.RegisterCodeFix(GetAddAllowAnonymousCodeAction(document, declaration), diagnostic);
            context.RegisterCodeFix(GetAddAuthorizeCodeAction(document, declaration), diagnostic);
        }

        private static CodeAction GetAddAllowAnonymousCodeAction(Document document, MethodDeclarationSyntax declaration)
        {
            const string title = "Add [AllowAnonymous]";

            Func<CancellationToken, Task<Document>> createChangedDocument =
                ct => AddAllowAnonymousAttributeAsync(document, declaration, ct);

            return CodeAction.Create(title, createChangedDocument, title);
        }

        private static CodeAction GetAddAuthorizeCodeAction(Document document, MethodDeclarationSyntax declaration)
        {
            const string title = "Add [Authorize]";

            Func<CancellationToken, Task<Document>> createChangedDocument =
                ct => AddAuthorizeAsync(document, declaration, ct);

            return CodeAction.Create(title, createChangedDocument, title);
        }

        private static async Task<Document> AddAllowAnonymousAttributeAsync(
            Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            // This seems redundant given we already have the root in RegisterCodeFixesAsync.
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            return document.WithSyntaxRoot(root.ReplaceNode(
                methodDeclaration, AddAttribute(methodDeclaration, "AllowAnonymous")));
        }

        private static async Task<Document> AddAuthorizeAsync(
            Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            // This seems redundant given we already have the root in RegisterCodeFixesAsync.
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            return document.WithSyntaxRoot(root.ReplaceNode(
                methodDeclaration, AddAttribute(methodDeclaration, "Authorize")));
        }
    }
}