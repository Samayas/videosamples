using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Samayas.Tools.CodeAnalyzers.Analyzers.Test
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SAMTEST01";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Resources.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzerTitle, Resources.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Resources.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzerDescription);

        public TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzer()
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
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax classDeclaration = context.Node as ClassDeclarationSyntax;
            if (classDeclaration == null)
            {
                return;
            }

            SemanticModel model = context.SemanticModel;
            if (model == null)
            {
                return;
            }

            INamedTypeSymbol classSymbol = model.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
            if (classSymbol == null)
            {
                return;
            }

            if (!InheritsFromBaseTestClass(classSymbol))
            {
                return;
            }

            if (HasExcludeFromCodeCoverageAttribute(classSymbol))
            {
                return;
            }

            Diagnostic diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Has Exclude From Code Coverage Attribute.
        /// </summary>
        /// <param name="classSymbol">Named Type Symbol <see cref="INamedTypeSymbol"/>.</param>
        /// <returns>True / False.</returns>
        private static bool HasExcludeFromCodeCoverageAttribute(INamedTypeSymbol classSymbol)
        {
            // Prefer semantic attribute comparison. 
            foreach (AttributeData attribute in classSymbol.GetAttributes())
            {
                INamedTypeSymbol attributeClass = attribute.AttributeClass;
                if (attributeClass is null)
                {
                    continue;
                }

                // Checks both full name and short name to be robust.
                if (attributeClass.Name == "ExcludeFromCodeCoverageAttribute" || attributeClass.ToDisplayString() == "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool InheritsFromBaseTestClass(INamedTypeSymbol classSymbol)
        {
            return classSymbol.BaseType?.Name == "BaseTestClass";
        }
    }
}
