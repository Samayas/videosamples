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
using Samayas.Tools.CodeAnalyzers.Analyzers.Data;

namespace Samayas.Tools.CodeAnalyzers.CodeFixes.Data
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DataMethodEmptyConstructorForBaseEntityBaseClassFixProvider)), Shared]
    public class DataMethodEmptyConstructorForBaseEntityBaseClassFixProvider : CodeFixProvider
    {
        public DataMethodEmptyConstructorForBaseEntityBaseClassFixProvider()
        {
        }
 
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            [DebuggerStepThrough()]
            get { return ImmutableArray.Create(DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzer.DiagnosticId); }
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
            ClassDeclarationSyntax classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                return;
            }

            context.RegisterCodeFix(
                  CodeAction.Create(
                      title: CodeFixResources.DataMethodEmptyConstructorForBaseEntityBaseClassFixProviderTitle,
                      createChangedDocument: c => AddParameterlessConstructorAsync(context.Document, classDeclaration, c),
                      equivalenceKey: CodeFixResources.DataMethodEmptyConstructorForBaseEntityBaseClassFixProviderKey),
                  diagnostic);
        }
        private async Task<Document> AddParameterlessConstructorAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            ConstructorDeclarationSyntax constructor = SyntaxFactory.ConstructorDeclaration(
                attributeLists: SyntaxFactory.List<AttributeListSyntax>(),
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                identifier: SyntaxFactory.Identifier(classDeclaration.Identifier.Text),
                parameterList: SyntaxFactory.ParameterList(),
                initializer: null,
                body: SyntaxFactory.Block(),
                expressionBody: null);

            SyntaxList<MemberDeclarationSyntax> newMembers = classDeclaration.Members.Insert(0, constructor);
            ClassDeclarationSyntax newClassDecl = classDeclaration.WithMembers(SyntaxFactory.List(newMembers));

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(classDeclaration, newClassDecl);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
