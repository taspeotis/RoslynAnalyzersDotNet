using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzersDotNet.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [UsedImplicitly]
    internal sealed class ValidateAntiForgeryTokenAttributeDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public const string DiagnosticId = "RADN0001";
        private const string Title = "A8 - Cross-Site Request Forgery (CSRF)";
        private const string MessageFormat = "{0} is vulnerable to CSRF without a ValidateAntiForgeryTokenAttribute.";
        private const string Category = "OWASP";

        private const string Description =
            "A CSRF attack forces a logged-on victim's browser to send a forged HTTP request.";

        private static readonly DiagnosticDescriptor DiagnosticDescriptor = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors;

        private static readonly ISet<string> MutableHttpVerbAttributes =
            new HashSet<string> {"HttpDeleteAttribute", "HttpPatchAttribute", "HttpPostAttribute", "HttpPutAttribute"};

        static ValidateAntiForgeryTokenAttributeDiagnosticAnalyzer()
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

            if (!MutableHttpVerbAttributes.Any(s => HasAttribute(symbol, s)))
                return;

            if (HasAttribute(symbol, "ValidateAntiForgeryTokenAttribute"))
                return;

            var location = symbol.Locations[0];
            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, location, symbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}