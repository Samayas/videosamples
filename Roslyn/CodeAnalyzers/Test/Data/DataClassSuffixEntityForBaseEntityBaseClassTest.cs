using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samayas.Tools.CodeAnalyzers.Analyzers.Data;
using VerifyCS = Samayas.Tools.CodeAnalyzers.Test.CSharpCodeFixVerifier<
    Samayas.Tools.CodeAnalyzers.Analyzers.Data.DataClassSuffixEntityForBaseEntityBaseClassAnalyzer,
    Samayas.Tools.CodeAnalyzers.CodeFixes.Data.DataClassSuffixEntityForBaseEntityBaseClassFixProvider>;

namespace Samayas.Tools.CodeAnalyzers.TestCodeAnalyzer.Test.Data
{
    [TestClass]
    [ExcludeFromCodeCoverage()]

    public class DataClassSuffixEntityForBaseEntityBaseClassTest
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassSuffixEntityForBaseEntityBaseClassTest"/> class.
        /// </summary>
        public DataClassSuffixEntityForBaseEntityBaseClassTest()
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Data Class Suffix Entity For Base Entity Base Class Analyze Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataClassSuffixEntityForBaseEntityBaseClassAnalyzerSuccess()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject1
{
    public class EntityBase<TPrimaryKey>
    {
    }

    public class MyEntity : EntityBase<int>
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        /// <summary>
        /// Data Class Suffix Entity For Base Entity Base Class Analyze Failed.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataClassSuffixContextForBaseDbContextClassAnalyzerFailed()
        {
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);

            test = @"
namespace TestProject1
{
    public class EntityBase<TPrimaryKey>
    {
    }

    public class MyEntdity : EntityBase<int>
    {
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic()
                .WithSpan(8, 18, 8, 27) // Update these values to match your actual diagnostic location.
                .WithArguments("MyEntdity");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        /// <summary>
        /// Test Class Suffix Entity For Base Entity Base Class Fix Provider Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataClassSuffixEntityForBaseEntityBaseClassFixProviderSuccess()
        {
            {
                string test = @"
namespace TestProject1
{
    public class EntityBase<TPrimaryKey>
    {
    }

    public class My : EntityBase<int>
    {
    }
}
";

                string expectedResult = @"
namespace TestProject1
{
    public class EntityBase<TPrimaryKey>
    {
    }

    public class MyEntity : EntityBase<int>
    {
    }
}
";

                DiagnosticResult expected = VerifyCS.Diagnostic(DataClassSuffixEntityForBaseEntityBaseClassAnalyzer.DiagnosticId)
                    .WithSpan(8, 18, 8, 20) // Update these values to match your actual diagnostic location.
                    .WithArguments("My");
                await VerifyCS.VerifyCodeFixAsync(test, expected, expectedResult);
            }
        }
        #endregion
    }
}
