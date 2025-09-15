using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Samayas.Tools.CodeAnalyzers.Analyzers.Data
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DataClassSuffixEntityForBaseEntityBaseClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SAMDATA01";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Resources.DataClassSuffixEntityForBaseEntityBaseClassAnalyzerTitle, Resources.DataClassSuffixEntityForBaseEntityBaseClassAnalyzerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Resources.DataClassSuffixEntityForBaseEntityBaseClassAnalyzerDescription);

        public DataClassSuffixEntityForBaseEntityBaseClassAnalyzer()
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

            if (!ImplementsConfigurationInterface(namedTypeSymbol))
            {
                return;
            }

            if (!namedTypeSymbol.Name.EndsWith("Entity", StringComparison.OrdinalIgnoreCase))
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool ImplementsConfigurationInterface(INamedTypeSymbol symbol)
        {
            INamedTypeSymbol current = symbol.BaseType;
            while (current != null)
            {
                if (current.Name == "EntityBase" && current.TypeArguments.Length == 1)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
