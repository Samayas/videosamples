using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Samayas.Tools.CodeAnalyzers.Test
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            public Test()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(ImmutableArray.Create(new PackageIdentity("System.Runtime", "9.0.0")));
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(ImmutableArray.Create(new PackageIdentity("MSTest.TestFramework", "3.10.4")));
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
