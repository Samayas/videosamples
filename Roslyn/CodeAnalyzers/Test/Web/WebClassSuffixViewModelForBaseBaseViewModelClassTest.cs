using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Samayas.Tools.CodeAnalyzers.Analyzers.Web;

using VerifyCS = Samayas.Tools.CodeAnalyzers.Test.CSharpCodeFixVerifier<
    Samayas.Tools.CodeAnalyzers.Analyzers.Web.WebClassSuffixViewModelForBaseBaseViewModelClassAnalyzer,
    Samayas.Tools.CodeAnalyzers.CodeFixes.Web.WebClassSuffixViewModelForBaseBaseViewModelClassFixProvider>;

namespace Samayas.Tools.CodeAnalyzers.TestCodeAnalyzer.Test.Web
{
    [TestClass]
    [ExcludeFromCodeCoverage()]

    public class WebClassViewModelTestForBaseWebClassTest
    {
        public WebClassViewModelTestForBaseWebClassTest()
        {
        }

        [TestMethod()]
        public async Task WebClassViewModelTestForBaseWebClassAnalyzeSuccess()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject
{
    public class BaseViewModel
    {
    }

    public sealed class MyViewModel : BaseViewModel
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject
{
    public class BaseViewModel
    {
    }

    public class SubViewModel : BaseViewModel
    {
    }

    public sealed class My2ViewModel : SubViewModel
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod()]
        public async Task WebClassViewModelTestForBaseWebClassAnalyzeFailed()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject
{
    public class BaseViewModel
    {
    }

    public sealed class ThisIs : BaseViewModel
    {
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic()
                .WithSpan(startLine: 8, 25, 8, 31) // Update these values to match your actual diagnostic location.
                .WithArguments("ThisIs");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod()]
        public async Task WebClassViewModelTestForBaseWebClassFixProvider()
        {
            string test = @"
namespace TestProject
{
    public class BaseViewModel
    {
    }

    public class My2View : BaseViewModel
    {
    }
}
";

            string expectedResult = @"
namespace TestProject
{
    public class BaseViewModel
    {
    }

    public class My2ViewViewModel : BaseViewModel
    {
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic(WebClassSuffixViewModelForBaseBaseViewModelClassAnalyzer.DiagnosticId)
                .WithSpan(8, 18, 8, 25)
                .WithArguments("My2View");
            await VerifyCS.VerifyCodeFixAsync(test, expected, expectedResult);
        }
    }
}
