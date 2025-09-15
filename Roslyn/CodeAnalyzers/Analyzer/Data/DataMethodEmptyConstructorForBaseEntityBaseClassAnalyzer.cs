using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Samayas.Tools.CodeAnalyzers.Analyzers.Data
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SAMDATA02";
        private const string Category = "Design";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Resources.DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzerTitle, Resources.DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Resources.DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzerDescription);
     
        public DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzer()
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

            bool hasParameterlessConstructor = namedTypeSymbol.InstanceConstructors.Any(c => c.Parameters.IsEmpty);
            if (!hasParameterlessConstructor)
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
