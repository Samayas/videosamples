using System;
using System.Collections.Generic;
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

using Samayas.Tools.CodeAnalyzers.Analyzers.Test;

namespace Samayas.Tools.CodeAnalyzers.CodeFixes.Test
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProvider)), Shared]
	public class TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProvider : CodeFixProvider
	{
		public TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProvider()
		{
		}

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			[DebuggerStepThrough()]
			get { return ImmutableArray.Create(TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzer.DiagnosticId); }
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
					title: CodeFixResources.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProviderTitle,
					c => AddAttributeAsync(context.Document, root, classDeclaration, "ExcludeFromCodeCoverage", "System.Diagnostics.CodeAnalysis", c),
					equivalenceKey: CodeFixResources.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProviderKey),
				diagnostic);
		}

        private static Task<Document> AddAttributeAsync(Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration, string attributeName, string usingNamespace, CancellationToken cancellationToken)
        {
            AttributeSyntax attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName), SyntaxFactory.AttributeArgumentList());
            AttributeListSyntax newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));

            List<SyntaxTrivia> allDocTrivias = new List<SyntaxTrivia>();
            List<SyntaxTrivia> otherTrivias = new List<SyntaxTrivia>();

            // Check if existing attribute lists have documentation comments
            bool existingAttributesHaveDocs = classDeclaration.AttributeLists.Any(attrList =>
                attrList.GetLeadingTrivia().Any(trivia =>
                    trivia.HasStructure && trivia.GetStructure() is DocumentationCommentTriviaSyntax));

            if (existingAttributesHaveDocs)
            {
                // Collect docs ONLY from existing attribute lists (not from class)
                foreach (AttributeListSyntax existingList in classDeclaration.AttributeLists)
                {
                    foreach (SyntaxTrivia trivia in existingList.GetLeadingTrivia())
                    {
                        bool isDoc = trivia.HasStructure && trivia.GetStructure() is DocumentationCommentTriviaSyntax;
                        if (isDoc)
                        {
                            allDocTrivias.Add(trivia);
                        }
                    }
                }

                // Collect only non-doc trivia from class declaration
                foreach (SyntaxTrivia syntaxTrivia in classDeclaration.GetLeadingTrivia())
                {
                    bool isDoc = syntaxTrivia.HasStructure && syntaxTrivia.GetStructure() is DocumentationCommentTriviaSyntax;
                    if (!isDoc)
                    {
                        otherTrivias.Add(syntaxTrivia);
                    }
                }
            }
            else
            {
                // No existing attributes have docs, so collect ONLY from class declaration
                foreach (SyntaxTrivia syntaxTrivia in classDeclaration.GetLeadingTrivia())
                {
                    bool isDoc = syntaxTrivia.HasStructure && syntaxTrivia.GetStructure() is DocumentationCommentTriviaSyntax;
                    if (isDoc)
                    {
                        allDocTrivias.Add(syntaxTrivia);
                    }
                    else
                    {
                        otherTrivias.Add(syntaxTrivia);
                    }
                }
            }

            // Merge existing lists + new list (work on a List<AttributeListSyntax>).
            List<AttributeListSyntax> cleanedExistingLists = classDeclaration.AttributeLists
              .Select(list => list.WithLeadingTrivia(SyntaxFactory.TriviaList()))
              .ToList();

            // Add the new attribute list
            cleanedExistingLists.Add(newAttributeList);

            // Helper to get comparable attribute name (rightmost identifier).
            Func<AttributeSyntax, string> attrKey = a =>
            {
                NameSyntax nameSyntax = a.Name;
                IdentifierNameSyntax identifierNameSyntax = nameSyntax as IdentifierNameSyntax;
                if (identifierNameSyntax != null)
                {
                    return identifierNameSyntax.Identifier.ValueText;
                }

                QualifiedNameSyntax qualifiedNameSyntax = a.Name as QualifiedNameSyntax;
                if (qualifiedNameSyntax != null)
                {
                    IdentifierNameSyntax rightId = qualifiedNameSyntax.Right as IdentifierNameSyntax;
                    if (rightId != null)
                    {
                        return rightId.Identifier.ValueText;
                    }
                }

                return nameSyntax.ToString();
            };

            // Sort attributes within each AttributeListSyntax.
            List<AttributeListSyntax> normalizedLists = new List<AttributeListSyntax>();
            foreach (AttributeListSyntax list in cleanedExistingLists)
            {
                List<AttributeSyntax> attributeSyntaxList = list.Attributes.OrderBy(a => attrKey(a), StringComparer.OrdinalIgnoreCase).ToList();
                SeparatedSyntaxList<AttributeSyntax> sortedSeparated = SyntaxFactory.SeparatedList(attributeSyntaxList);
                AttributeListSyntax listSorted = list.WithAttributes(sortedSeparated);
                normalizedLists.Add(listSorted);
            }

            // Sort the attribute lists by their first attribute name.
            Func<AttributeListSyntax, string> listKey = list => list.Attributes.Count == 0 ? string.Empty : attrKey(list.Attributes[0]);

            List<AttributeListSyntax> sortedLists = normalizedLists
                .OrderBy(list => listKey(list), StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Move XML docs to the first AttributeListSyntax.
            if (sortedLists.Count > 0 && allDocTrivias.Count > 0)
            {
                AttributeListSyntax firstList = sortedLists[0];
                SyntaxTriviaList newLeading = SyntaxFactory.TriviaList(allDocTrivias).Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
                sortedLists[0] = firstList.WithLeadingTrivia(newLeading);
            }

            // Clean class leading trivia (docs moved) and apply sorted lists.
            ClassDeclarationSyntax classWithCleanLeading = classDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(otherTrivias));
            SyntaxList<AttributeListSyntax> finalLists = SyntaxFactory.List(sortedLists);
            ClassDeclarationSyntax newClassDecl = classWithCleanLeading.WithAttributeLists(finalLists);

            // Replace node and return updated document.
            SyntaxNode newRoot = root.ReplaceNode(classDeclaration, newClassDecl);

            // Add using statement if specified and not already present
            if (!string.IsNullOrWhiteSpace(usingNamespace))
            {
                CompilationUnitSyntax compilationUnit = newRoot as CompilationUnitSyntax;
                if (compilationUnit != null)
                {
                    // Check if the using directive already exists
                    bool usingExists = compilationUnit.Usings.Any(u =>
                        u.Name?.ToString().Equals(usingNamespace, StringComparison.OrdinalIgnoreCase) == true);

                    if (!usingExists)
                    {
                        // Create the new using directive
                        UsingDirectiveSyntax newUsing = SyntaxFactory.UsingDirective(
                            SyntaxFactory.ParseName(usingNamespace))
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                        // Add to existing usings and sort them
                        SyntaxList<UsingDirectiveSyntax> newUsings = compilationUnit.Usings.Add(newUsing);

                        // Sort usings alphabetically
                        UsingDirectiveSyntax[] sortedUsings = newUsings
                            .OrderBy(u => u.Name?.ToString(), StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        // Update the compilation unit with sorted usings
                        newRoot = compilationUnit.WithUsings(SyntaxFactory.List(sortedUsings));
                    }
                }
            }

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

    }
}