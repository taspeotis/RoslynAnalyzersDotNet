using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzersDotNet.DiagnosticAnalyzers
{
    // This diagnostic has opinions - AllowAnonymous on a controller is bad.
    // (TODO: Check that AllowAnonymous on a controller is permitted and works.)
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [UsedImplicitly]
    internal sealed class AuthorizeAttributeDiagnosticAnalyzerDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public const string DiagnosticId = "RADN0002";

        private const string Title = "A7 – Missing Function Level Access Control";
        private const string MessageFormat = "{0} has neither AllowAnonymousAttribute nor AuthorizeAttribute.";
        private const string Category = "OWASP";

        private const string Description =
            "Attackers will be able to forge requests in order to access functionality without proper authorization.";

        private static readonly DiagnosticDescriptor DiagnosticDescriptor = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors;

        static AuthorizeAttributeDiagnosticAnalyzerDiagnosticAnalyzer()
        {
            DiagnosticDescriptors = ImmutableArray.Create(DiagnosticDescriptor);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticDescriptors;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;
            var methodSymbol = (IMethodSymbol) symbol;

            if (!IsActionMethod(methodSymbol))
                return;

            if (HasAttribute(symbol, "AllowAnonymousAttribute") || HasAttribute(symbol, "AuthorizeAttribute"))
                return;

            for (var baseType = symbol.ContainingType; baseType != null; baseType = baseType.BaseType)
            {
                if (HasAttribute(baseType, "AuthorizeAttribute"))
                    return;
            }
            
            var location = symbol.Locations[0];
            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, location, symbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}