using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Samayas.Tools.CodeAnalyzers.Test
{
    public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
        where TCodeRefactoring : CodeRefactoringProvider, new()
    {
        public class Test : CSharpCodeRefactoringTest<TCodeRefactoring, DefaultVerifier>
        {
            public Test()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(ImmutableArray.Create(new PackageIdentity("MSTest.TestFramework", "3.10.4")));
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(ImmutableArray.Create(new PackageIdentity("System.Runtime", "9.0.0")));
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    return solution;
                });
            }
        }
    }
}
