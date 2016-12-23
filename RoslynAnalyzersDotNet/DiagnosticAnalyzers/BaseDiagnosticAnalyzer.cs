using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzersDotNet.DiagnosticAnalyzers
{
    internal abstract class BaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected static bool IsActionMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                return false;

            if (methodSymbol.IsGenericMethod)
                return false;

            if (methodSymbol.IsStatic)
                return false;

            if (methodSymbol.MethodKind != MethodKind.Ordinary)
                return false;

            INamedTypeSymbol baseType;

            for (baseType = methodSymbol.ContainingType; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType.Name == "Controller" && baseType.ContainingNamespace.ToDisplayString() == "System.Web.Mvc")
                    break;
            }

            if (baseType == null)
                return false;

            if (HasAttribute(methodSymbol, "ChildActionOnlyAttribute"))
                return false;

            if (HasAttribute(methodSymbol, "NonActionAttribute"))
                return false;

            return true;
        }

        protected static bool HasAttribute(ITypeSymbol typeSymbol, string attributeClassName)
        {
            for (var baseType = typeSymbol; baseType != null; baseType = baseType.BaseType)
            {
                // Does GetAttributes return inherited attributes? Probably not.
                // TODO: Check this, and whether we need to navigate the heirarchy.
                if (baseType.GetAttributes().Any(ad => IsAttribute(ad, attributeClassName)))
                    return true;
            }

            return false;
        }

        // TODO: Extension methods would probably be fine
        protected static bool HasAttribute(ISymbol symbol, string attributeClassName)
        {
            return symbol.GetAttributes().Any(ad => IsAttribute(ad, attributeClassName));
        }

        private static bool IsAttribute(AttributeData attributeData, string attributeClassName)
        {
            for (var baseType = attributeData.AttributeClass; baseType != null; baseType = baseType.BaseType)
                if (baseType.Name == attributeClassName)
                    return true;

            return false;
        }
    }
}