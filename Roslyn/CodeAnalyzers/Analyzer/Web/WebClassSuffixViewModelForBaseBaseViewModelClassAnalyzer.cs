using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Samayas.Tools.CodeAnalyzers.Analyzers.Web
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WebClassSuffixViewModelForBaseBaseViewModelClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SAMWEB01";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Resources.WebClassSuffixForBaseViewModelClassAnalyzerTitle, Resources.WebClassSuffixForBaseViewModelClassAnalyzerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Resources.WebClassSuffixForBaseViewModelClassAnalyzerDescription);

        public WebClassSuffixViewModelForBaseBaseViewModelClassAnalyzer()
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            [DebuggerStepThrough()]
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (namedTypeSymbol.TypeKind != TypeKind.Class)
            {
                return;
            }

            if (!InheritsFromBaseViewModel(namedTypeSymbol))
            {
                return;
            }

            if (!namedTypeSymbol.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool InheritsFromBaseViewModel(INamedTypeSymbol symbol)
        {
            INamedTypeSymbol baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name.Equals("BaseViewModel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
