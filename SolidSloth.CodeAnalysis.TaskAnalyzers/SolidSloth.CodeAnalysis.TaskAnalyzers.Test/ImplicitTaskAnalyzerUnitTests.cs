using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = SolidSloth.CodeAnalysis.TaskAnalyzers.Test.CSharpCodeFixVerifier<
    SolidSloth.CodeAnalysis.TaskAnalyzers.ImplicitTaskAnalyzer,
    SolidSloth.CodeAnalysis.TaskAnalyzers.ImplicitTaskCodeFixProvider>;

namespace SolidSloth.CodeAnalysis.TaskAnalyzers.Test
{
    [TestClass]
    public class ImplicitTaskAnalyzerUnitTests
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            public async Task DoNothing()
            {
                var t = Task.FromResult(1);
            }
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            public async Task DoNothing()
            {
                var t = await Task.FromResult(1);
            }
        }
    }";

            var expected = VerifyCS.Diagnostic("SSCA03E8").WithLocation(15, 17);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
