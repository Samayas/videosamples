using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samayas.Tools.CodeAnalyzers.Analyzers.Data;
using VerifyCS = Samayas.Tools.CodeAnalyzers.Test.CSharpCodeFixVerifier<
    Samayas.Tools.CodeAnalyzers.Analyzers.Data.DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzer,
    Samayas.Tools.CodeAnalyzers.CodeFixes.Data.DataMethodEmptyConstructorForBaseEntityBaseClassFixProvider>;

namespace Samayas.Tools.CodeAnalyzers.TestCodeAnalyzer.Test.Data
{
    [TestClass]
    public class DataMethodEmptyConstructorForBaseEntityBaseClassTest
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMethodEmptyConstructorForBaseEntityBaseClassTest"/> class.
        /// </summary>
        public DataMethodEmptyConstructorForBaseEntityBaseClassTest()
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Data Method Empty Constructor For Base Entity Base Class Analyze Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzerSuccess()
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
        /// Data Method Empty Constructor For Base Entity Base Class Analyze Failed.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzerFailed()
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
        public MyEntity(int a)
        {
        }
    }
}
";

            DiagnosticResult expected = VerifyCS.Diagnostic()
                  .WithSpan(8, 18, 8, 26) // Update these values to match your actual diagnostic location.
                  .WithArguments("MyEntity");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        /// <summary>
        /// Test Class Method Empty Constructor For Base Entity Base Class Fix Provider Success.
        /// </summary>
        /// <returns>Task <see cref="Task"/>.</returns>
        [TestMethod()]
        public async Task DataMethodEmptyConstructorForBaseEntityBaseClassFixProviderSuccess()
        {
            {
                string test = @"
namespace TestProject1
{
    public class EntityBase<TPrimaryKey>
    {
    }

    public class MyEntity : EntityBase<int>
    {
        public MyEntity(int a)
        {
        }
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
        public MyEntity()
        {
        }

        public MyEntity(int a)
        {
        }
    }
}
";

                DiagnosticResult expected = VerifyCS.Diagnostic(DataMethodEmptyConstructorForBaseEntityBaseClassAnalyzer.DiagnosticId)
                    .WithSpan(8, 18, 8, 26) // Update these values to match your actual diagnostic location.
                  .WithArguments("MyEntity");
                await VerifyCS.VerifyCodeFixAsync(test, expected, expectedResult);
            }
        }
        #endregion
    }
}
