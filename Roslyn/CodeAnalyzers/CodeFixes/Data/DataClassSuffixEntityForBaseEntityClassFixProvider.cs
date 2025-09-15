using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Samayas.Tools.CodeAnalyzers.Analyzers.Data;

namespace Samayas.Tools.CodeAnalyzers.CodeFixes.Data
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DataClassSuffixEntityForBaseEntityBaseClassFixProvider)), Shared]
    public class DataClassSuffixEntityForBaseEntityBaseClassFixProvider : CodeFixProvider
    {

        public DataClassSuffixEntityForBaseEntityBaseClassFixProvider()
        {
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            [DebuggerStepThrough()]
            get { return ImmutableArray.Create(DataClassSuffixEntityForBaseEntityBaseClassAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics.First();
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            TypeDeclarationSyntax typeDeclarationSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (typeDeclarationSyntax == null)
            {
                return;
            }

            context.RegisterCodeFix(
                  CodeAction.Create(
                      title: CodeFixResources.DataClassSuffixEntityForBaseEntityClassFixProviderTitle,
                      createChangedSolution: c => RenameClass(context.Document, typeDeclarationSyntax, c),
                      equivalenceKey: CodeFixResources.DataClassSuffixEntityForBaseEntityClassFixProviderKey),
                  diagnostic);
        }

        private async Task<Solution> RenameClass(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);

            string newName = typeSymbol.Name + "Entity";

            Solution solution = document.Project.Solution;
            SymbolRenameOptions renameOptions = new SymbolRenameOptions(RenameOverloads: false, RenameInStrings: true, RenameInComments: true, RenameFile: false);

            return await Renamer.RenameSymbolAsync(solution, typeSymbol, renameOptions, newName, cancellationToken).ConfigureAwait(false);
        }
    }
}
