using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Integration.Rules.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.BlankLines.BlankLineBeforeStatementRule"/>
/// </summary>
[TestClass]
public class BlankLineBeforeStatementRuleTests
{
    #region Constants

    private const string TestData = """
        internal class BlankLineBeforeStatementTestData
        {
            public void TryStatement()
            {
                var x = 1;
                try
                {
                }
                catch
                {
                }
            }

            public void IfStatement()
            {
                var x = 1;
                if (x == 1)
                {
                }
            }

            public void WhileStatement()
            {
                var x = 1;
                while (x > 0)
                {
                    x--;
                }
            }

            public void DoStatement()
            {
                var x = 1;
                do
                {
                    x--;
                }
                while (x > 0);
            }

            public void UsingStatement()
            {
                var x = 1;
                using (var stream = new System.IO.MemoryStream())
                {
                }
            }

            public void ForeachStatement()
            {
                var x = 1;
                foreach (var item in new int[0])
                {
                }
            }

            public void ForStatement()
            {
                var x = 1;
                for (var i = 0; i < 10; i++)
                {
                }
            }

            public void ReturnStatement()
            {
                var x = 1;
                return;
            }

            public void GotoStatement()
            {
                var x = 1;
                goto end;
                end:
                return;
            }

            public void BreakStatement()
            {
                while (true)
                {
                    var x = 1;
                    break;
                }
            }

            public void ContinueStatement()
            {
                while (true)
                {
                    var x = 1;
                    continue;
                }
            }

            public void ThrowStatement()
            {
                var x = 1;
                throw new System.Exception();
            }

            public void SwitchStatement()
            {
                var x = 1;
                switch (x)
                {
                    case 1:
                        break;
                }
            }

            public void CheckedStatement()
            {
                var x = 1;
                checked
                {
                    x++;
                }
            }

            public void UncheckedStatement()
            {
                var x = 1;
                unchecked
                {
                    x++;
                }
            }

            public unsafe void FixedStatement()
            {
                var arr = new int[10];
                fixed (int* p = arr)
                {
                }
            }

            public void LockStatement()
            {
                var obj = new object();
                lock (obj)
                {
                }
            }

            public System.Collections.Generic.IEnumerable<int> YieldReturnStatement()
            {
                var x = 1;
                yield return x;
            }

            // --- Cases that should NOT be modified ---

            public void FirstInBlock()
            {
                try
                {
                }
                catch
                {
                }
            }

            public void ElseIf()
            {
                var x = 1;

                if (x == 1)
                {
                }
                else if (x == 2)
                {
                }
            }

            public System.Collections.Generic.IEnumerable<int> ConsecutiveYieldReturn()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }

            public void AlreadyHasBlankLine()
            {
                var x = 1;

                if (x == 1)
                {
                }
            }

            public void PrecededByComment()
            {
                var x = 1;
                // This is a comment
                if (x == 1)
                {
                }
            }

            public void PrecededByBlockComment()
            {
                var x = 1;
                /* This is a block comment */
                if (x == 1)
                {
                }
            }
        }
        """;

    private const string ResultData = """
        internal class BlankLineBeforeStatementTestData
        {
            public void TryStatement()
            {
                var x = 1;

                try
                {
                }
                catch
                {
                }
            }

            public void IfStatement()
            {
                var x = 1;

                if (x == 1)
                {
                }
            }

            public void WhileStatement()
            {
                var x = 1;

                while (x > 0)
                {
                    x--;
                }
            }

            public void DoStatement()
            {
                var x = 1;

                do
                {
                    x--;
                }
                while (x > 0);
            }

            public void UsingStatement()
            {
                var x = 1;

                using (var stream = new System.IO.MemoryStream())
                {
                }
            }

            public void ForeachStatement()
            {
                var x = 1;

                foreach (var item in new int[0])
                {
                }
            }

            public void ForStatement()
            {
                var x = 1;

                for (var i = 0; i < 10; i++)
                {
                }
            }

            public void ReturnStatement()
            {
                var x = 1;

                return;
            }

            public void GotoStatement()
            {
                var x = 1;

                goto end;
                end:
                return;
            }

            public void BreakStatement()
            {
                while (true)
                {
                    var x = 1;

                    break;
                }
            }

            public void ContinueStatement()
            {
                while (true)
                {
                    var x = 1;

                    continue;
                }
            }

            public void ThrowStatement()
            {
                var x = 1;

                throw new System.Exception();
            }

            public void SwitchStatement()
            {
                var x = 1;

                switch (x)
                {
                    case 1:
                        break;
                }
            }

            public void CheckedStatement()
            {
                var x = 1;

                checked
                {
                    x++;
                }
            }

            public void UncheckedStatement()
            {
                var x = 1;

                unchecked
                {
                    x++;
                }
            }

            public unsafe void FixedStatement()
            {
                var arr = new int[10];

                fixed (int* p = arr)
                {
                }
            }

            public void LockStatement()
            {
                var obj = new object();

                lock (obj)
                {
                }
            }

            public System.Collections.Generic.IEnumerable<int> YieldReturnStatement()
            {
                var x = 1;

                yield return x;
            }

            // --- Cases that should NOT be modified ---

            public void FirstInBlock()
            {
                try
                {
                }
                catch
                {
                }
            }

            public void ElseIf()
            {
                var x = 1;

                if (x == 1)
                {
                }
                else if (x == 2)
                {
                }
            }

            public System.Collections.Generic.IEnumerable<int> ConsecutiveYieldReturn()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }

            public void AlreadyHasBlankLine()
            {
                var x = 1;

                if (x == 1)
                {
                }
            }

            public void PrecededByComment()
            {
                var x = 1;

                // This is a comment
                if (x == 1)
                {
                }
            }

            public void PrecededByBlockComment()
            {
                var x = 1;

                /* This is a block comment */
                if (x == 1)
                {
                }
            }
        }
        """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the formatter inserts blank lines before statements where required.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLinesBeforeStatements()
    {
        // Arrange
        var input = TestData;
        var expected = ResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}