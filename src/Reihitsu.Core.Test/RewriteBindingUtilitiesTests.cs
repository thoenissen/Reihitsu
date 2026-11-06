using System;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="RewriteBindingUtilities"/>. Each replacement is built as the two consumers build
/// theirs: the code fix of RH3106 removes the empty parameter list of an anonymous method, and its analyzer replaces
/// that list by a parameter list of one <see langword="int"/> parameter, which gives the anonymous method another
/// natural type. Every criterion of the check is tested on both sides: the replacement still converts, the replaced
/// code gains no binder error, and the overload, operator, or collection initializer <c>Add</c> method around the
/// expression stays the same
/// </summary>
[TestClass]
public class RewriteBindingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that removing the parameter list of an anonymous method with a delegate type as its target binds as
    /// before and converts to that delegate type
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListWithDelegateTarget()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Action a = delegate () { };
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that the probe of another natural type binds as before under an implicitly typed local and converts to
    /// that other natural type
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForProbeUnderImplicitlyTypedLocal()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          var a = delegate () { };
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action<int>", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that the probe of another natural type does not bind as before where a delegate type of another
    /// parameter list is the target: the probe does not convert to it
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForProbeThatDoesNotConvertToDelegateTarget()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Action a = delegate () { };
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.IsNull(convertedType);
    }

    /// <summary>
    /// Verifies that removing the parameter list under an implicitly typed local does not bind as before: without the
    /// list the anonymous method has no natural type, so the local has none either
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListUnderImplicitlyTypedLocal()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          var a = delegate () { };
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a replacement that still converts to its target and binds every member around it as before, but
    /// brings a binder error into the code, does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForReplacementThatAddsBinderError()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Action a = delegate () { };
                                                      }
                                                  }
                                                  """);

        var replacement = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression("delegate { Missing(); }");

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, replacement, out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing the parameter list of an anonymous method that the expression body of a lambda under an
    /// implicitly typed local returns does not bind as before: the anonymous method still converts and every member
    /// around it stays the same, but the lambda's return type, which it inferred from the anonymous method, can no
    /// longer be inferred, and only the binder error shows that
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatLeavesLambdaReturnTypeUninferred()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          var g = () => delegate () { };
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a replacement inside code that already has a binder error binds as before when it adds no other
    /// error: only errors the original code did not have count
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListBesideExistingBinderError()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Run(delegate () { }, Missing);
                                                      }

                                                      static void Run(Action a, int b)
                                                      {
                                                      }
                                                  }
                                                  """,
                                                  expectErrors: true);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list of an argument of the only overload binds as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListWithSingleOverload()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Run(delegate () { });
                                                      }

                                                      static void Run(Action a)
                                                      {
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list of an argument that pins one of two overloads does not bind as
    /// before: without the list both overloads apply
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatMakesOverloadsAmbiguous()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Run(delegate () { });
                                                      }

                                                      static void Run(Action a)
                                                      {
                                                      }

                                                      static void Run(Action<int> a)
                                                      {
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a probe that converts and adds no error, but makes overload resolution choose another overload,
    /// does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForProbeThatChoosesAnotherOverload()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Run(delegate () { });
                                                      }

                                                      static void Run(Action a)
                                                      {
                                                      }

                                                      static void Run(Action<int> a)
                                                      {
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a probe that makes a constructor initializer choose another constructor does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForProbeThatChoosesAnotherConstructor()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      C()
                                                          : this(delegate () { })
                                                      {
                                                      }

                                                      C(Action a)
                                                      {
                                                      }

                                                      C(Action<int> a)
                                                      {
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing the parameter list of the operand of a compound assignment with a single user-defined
    /// operator binds as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListWithSingleOperator()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M(C c)
                                                      {
                                                          c += delegate () { };
                                                      }

                                                      public static C operator +(C c, Action a) => c;
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list of the operand of a compound assignment with two user-defined
    /// operators does not bind as before: without the list both operators apply
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatMakesOperatorsAmbiguous()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M(C c)
                                                      {
                                                          c += delegate () { };
                                                      }

                                                      public static C operator +(C c, Action a) => c;

                                                      public static C operator +(C c, Action<int> a) => c;
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a probe that converts and adds no error, but makes a binary operator bind another user-defined
    /// operator, does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForProbeThatChoosesAnotherOperator()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      C M(C c)
                                                      {
                                                          return c + delegate () { };
                                                      }

                                                      public static C operator +(C c, Action a) => c;

                                                      public static C operator +(C c, Action<int> a) => c;
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing the parameter list of an element of a collection initializer with a single <c>Add</c>
    /// method binds as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListWithSingleAddMethod()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;
                                                  using System.Collections;

                                                  class Coll : IEnumerable
                                                  {
                                                      public void Add(Action a)
                                                      {
                                                      }

                                                      public IEnumerator GetEnumerator() => null;

                                                      static void M()
                                                      {
                                                          var c = new Coll { delegate () { } };
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list of an element of a collection initializer with two <c>Add</c>
    /// methods does not bind as before: without the list both apply
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatMakesAddMethodsAmbiguous()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;
                                                  using System.Collections;

                                                  class Coll : IEnumerable
                                                  {
                                                      public void Add(Action a)
                                                      {
                                                      }

                                                      public void Add(Action<int> a)
                                                      {
                                                      }

                                                      public IEnumerator GetEnumerator() => null;

                                                      static void M()
                                                      {
                                                          var c = new Coll { delegate () { } };
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a probe that converts and adds no error, but makes a collection initializer element call another
    /// <c>Add</c> method, does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForProbeThatChoosesAnotherAddMethod()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;
                                                  using System.Collections;

                                                  class Coll : IEnumerable
                                                  {
                                                      public void Add(Action a)
                                                      {
                                                      }

                                                      public void Add(Action<int> a)
                                                      {
                                                      }

                                                      public IEnumerator GetEnumerator() => null;

                                                      static void M()
                                                      {
                                                          var c = new Coll { delegate () { } };
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing the parameter list of an argument whose type argument is inferred from the anonymous
    /// method does not bind as before: without the list the type argument cannot be inferred
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatBreaksTypeInference()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Run(delegate () { });
                                                      }

                                                      static void Run<T>(T value)
                                                      {
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that an anonymous method returned from a block-bodied lambda under an implicitly typed local is bound
    /// together with the declaration around the lambda, whose return type it decides: the probe converts to its own
    /// natural type there, not to the return type the original anonymous method gave the lambda
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeBindsReturnStatementTogetherWithDeclarationAroundLambda()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          var g = () => { return delegate () { }; };
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action<int>", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list binds as before in a field initializer and in an expression body,
    /// which are bound again without a statement around them
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForRemovedListInFieldInitializerAndExpressionBody()
    {
        var (semanticModel, root) = CompileRoot("""
                                                using System;

                                                class C
                                                {
                                                    Action F = delegate () { };

                                                    Action P => delegate () { };
                                                }
                                                """);

        foreach (var expression in root.DescendantNodes().OfType<AnonymousMethodExpressionSyntax>())
        {
            Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out var convertedType, CancellationToken.None), expression.Parent?.Kind().ToString());
            Assert.AreEqual("System.Action", convertedType?.ToDisplayString());
        }
    }

    /// <summary>
    /// Verifies that an expression with no statement, initializer, expression body, or constructor initializer around
    /// it below its member is not bound again and does not bind as before
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseWhenNoBindingRootSurroundsExpression()
    {
        var (semanticModel, root) = CompileRoot("""
                                                using System;

                                                [Obsolete("text")]
                                                class C
                                                {
                                                }
                                                """);

        var expression = root.DescendantNodes().OfType<LiteralExpressionSyntax>().Single();

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, SyntaxFactory.ParseExpression("\"text\""), out var convertedType, CancellationToken.None));
        Assert.IsNull(convertedType);
    }

    /// <summary>
    /// Verifies that the probe binds as before under an implicitly typed local inside an <see langword="if"/> block
    /// whose later statement calls the local: the error the probe's natural type causes in that sibling statement does
    /// not count, because the sibling statement is not on the path from the expression to its binding root
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInBlock()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M(bool b)
                                                      {
                                                          if (b)
                                                          {
                                                              var a = delegate () { };
                                                              a();
                                                          }
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action<int>", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that the probe binds as before under an implicitly typed local in the block body of a lambda whose
    /// later statement calls the local: the error in that sibling statement of the lambda's body does not count
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInLambdaBody()
    {
        var (semanticModel, expression) = Compile("""
                                                  using System;

                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          Action x = () =>
                                                                     {
                                                                         var a = delegate () { };
                                                                         a();
                                                                     };
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action<int>", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that the probe binds as before where it decides the inferred return type of a lambda under an
    /// implicitly typed local inside an <see langword="if"/> block, and a later statement calls the lambda's result:
    /// the lambda's declaration is on the path and binds without error, the sibling statement is not on the path
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsTrueForProbeReturnedByLambdaWhoseResultBreaksSiblingStatement()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M(bool b)
                                                      {
                                                          if (b)
                                                          {
                                                              var g = () => delegate () { };
                                                              g()();
                                                          }
                                                      }
                                                  }
                                                  """);

        Assert.IsTrue(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, Probe(expression), out var convertedType, CancellationToken.None));
        Assert.AreEqual("System.Action<int>", convertedType?.ToDisplayString());
    }

    /// <summary>
    /// Verifies that removing the parameter list still does not bind as before where it leaves the inferred return
    /// type of a lambda inside an <see langword="if"/> block without a type, also beside a sibling statement: the
    /// error stays on the path from the expression to its binding root
    /// </summary>
    [TestMethod]
    public void BindsAsBeforeReturnsFalseForRemovedListThatLeavesLambdaReturnTypeUninferredInBlock()
    {
        var (semanticModel, expression) = Compile("""
                                                  class C
                                                  {
                                                      void M(bool b)
                                                      {
                                                          if (b)
                                                          {
                                                              var g = () => delegate () { };
                                                              g()();
                                                          }
                                                      }
                                                  }
                                                  """);

        Assert.IsFalse(RewriteBindingUtilities.BindsAsBefore(semanticModel, expression, RemoveList(expression), out _, CancellationToken.None));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Compiles the source and gets its single anonymous method
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="expectErrors">Whether the source is expected to have compile errors</param>
    /// <returns>The semantic model and the anonymous method</returns>
    private static (SemanticModel SemanticModel, AnonymousMethodExpressionSyntax Expression) Compile(string source, bool expectErrors = false)
    {
        var (semanticModel, root) = CompileRoot(source, expectErrors);

        return (semanticModel, root.DescendantNodes().OfType<AnonymousMethodExpressionSyntax>().Single());
    }

    /// <summary>
    /// Compiles the source against the platform assemblies
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="expectErrors">Whether the source is expected to have compile errors</param>
    /// <returns>The semantic model and the root of the tree</returns>
    private static (SemanticModel SemanticModel, SyntaxNode Root) CompileRoot(string source, bool expectErrors = false)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                                                                                    .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Test", [tree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

        Assert.AreEqual(expectErrors, errors.Count > 0, string.Join(Environment.NewLine, errors));

        return (compilation.GetSemanticModel(tree), tree.GetRoot());
    }

    /// <summary>
    /// Removes the parameter list of the anonymous method, as the code fix of RH3106 does
    /// </summary>
    /// <param name="expression">Anonymous method</param>
    /// <returns>The anonymous method without parameter list</returns>
    private static AnonymousMethodExpressionSyntax RemoveList(AnonymousMethodExpressionSyntax expression)
    {
        return expression.WithParameterList(null);
    }

    /// <summary>
    /// Replaces the parameter list of the anonymous method by one <see langword="int"/> parameter, as the analyzer of
    /// RH3106 does to give it another natural type
    /// </summary>
    /// <param name="expression">Anonymous method</param>
    /// <returns>The probe</returns>
    private static AnonymousMethodExpressionSyntax Probe(AnonymousMethodExpressionSyntax expression)
    {
        return expression.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Parameter(SyntaxFactory.Identifier("probe"))
                                                                                                                          .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))))));
    }

    #endregion // Methods
}