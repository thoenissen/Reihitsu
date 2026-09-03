using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7108EventAccessorsMustFollowOrderAnalyzer"/> and <see cref="RH7108EventAccessorsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7108EventAccessorsMustFollowOrderAnalyzerTests : BatchCodeFixTestsBase<RH7108EventAccessorsMustFollowOrderAnalyzer, RH7108EventAccessorsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying event accessors are reported and fixed when add appears after remove
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EventAccessorsAreReportedAndFixedWhenAddAppearsAfterRemove()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        {|#0:add|}
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     public event EventHandler Changed
                                     {
                                         add
                                         {
                                             _changed += value;
                                         }

                                         remove
                                         {
                                             _changed -= value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7108MessageFormat));
    }

    /// <summary>
    /// Verifying no code fix is offered when the move would separate a preprocessor directive from its partner,
    /// with the conditional opened around the target accessor and closed before the moved accessor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                #if DEBUG
                                        remove
                                        {
                                            _changed -= value;
                                        }
                                #endif
                                        add
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.AddAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying no code fix is offered when a preprocessor directive sits between the accessor attribute list and the
    /// accessor keyword, since the directive attaches to a later token and moving the accessor would split the pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectiveFollowsAccessorAttributeList()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        [Obsolete]
                                #if DEBUG
                                        add
                                        {
                                            _changed += value;
                                        }
                                #endif
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.AddAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    private EventHandler _closed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        {|#0:add|}
                                        {
                                            _changed += value;
                                        }
                                    }

                                    public event EventHandler Closed
                                    {
                                        remove
                                        {
                                            _closed -= value;
                                        }

                                        {|#1:add|}
                                        {
                                            _closed += value;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     private EventHandler _closed;

                                     public event EventHandler Changed
                                     {
                                         add
                                         {
                                             _changed += value;
                                         }

                                         remove
                                         {
                                             _changed -= value;
                                         }
                                     }

                                     public event EventHandler Closed
                                     {
                                         add
                                         {
                                             _closed += value;
                                         }

                                         remove
                                         {
                                             _closed -= value;
                                         }
                                     }
                                 }
                                 """;

        // Two misordered accessor lists inside one type, so both rewrites are merged into the same type
        // declaration and each accessor move has to stay confined to its own event
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7108MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}