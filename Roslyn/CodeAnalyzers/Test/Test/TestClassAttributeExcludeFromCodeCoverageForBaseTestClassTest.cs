using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Samayas.Tools.CodeAnalyzers.Analyzers.Test;

using VerifyCS = Samayas.Tools.CodeAnalyzers.Test.CSharpCodeFixVerifier<
    Samayas.Tools.CodeAnalyzers.Analyzers.Test.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzer,
    Samayas.Tools.CodeAnalyzers.CodeFixes.Test.TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProvider>;

namespace Samayas.Tools.CodeAnalyzers.TestCodeAnalyzer.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage()]
    public class TestClassAttributeExcludeFromCodeCoverageForBaseTestClassTest
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassAttributeExcludeFromCodeCoverageForBaseTestClassTest"/> class.
        /// </summary>
        public TestClassAttributeExcludeFromCodeCoverageForBaseTestClassTest()
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Test Class Attribute Exclude From Code Coverage For Base Test Class Analyze Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzeSuccess()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
using System.Diagnostics.CodeAnalysis;

namespace TestProject
{
    public class BaseTestClass
    {
    }

    [ExcludeFromCodeCoverage()]
    public class ThisIsTest : BaseTestClass
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        /// <summary>
        /// Test Class Attribute Exclude From Code Coverage For Base Test Class Analyze Failed.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzeFailed()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject
{
    public class BaseTestClass
    {
    }

    public class ThisIsTest : BaseTestClass
    {
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic()
                .WithSpan(8, 18, 8, 28) // Update these values to match your actual diagnostic location.
                .WithArguments("ThisIsTest");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        /// <summary>
        /// Test Class Attribute Exclude From Code Coverage For Base Test Class Fix Provider Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task TestClassAttributeExcludeFromCodeCoverageForBaseTestClassFixProviderSuccess()
        {
            string test = @"
namespace TestProject
{
    public class BaseTestClass
    {
    }

    public class ThisIsTest : BaseTestClass
    {
    }
}
";

            string expectedResult =
@"using System.Diagnostics.CodeAnalysis;

namespace TestProject
{
    public class BaseTestClass
    {
    }

    [ExcludeFromCodeCoverage()]
    public class ThisIsTest : BaseTestClass
    {
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic(TestClassAttributeExcludeFromCodeCoverageForBaseTestClassAnalyzer.DiagnosticId)
                .WithSpan(8, 18, 8, 28)
                .WithArguments("ThisIsTest");
            await VerifyCS.VerifyCodeFixAsync(test, expected, expectedResult);
        }
        #endregion
    }
}
