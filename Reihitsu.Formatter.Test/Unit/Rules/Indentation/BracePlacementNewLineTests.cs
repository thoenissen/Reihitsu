using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Tests that capture the current formatter behavior for placing braces on new lines.
/// </summary>
[TestClass]
public class BracePlacementNewLineTests
{
    #region Methods

    /// <summary>
    /// Verifies brace placement for block-scoped namespaces.
    /// </summary>
    [TestMethod]
    public void BlockScopedNamespaceBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            namespace Sample{
            class C{
            }
            }
            """;

        const string expected = """
            namespace Sample
            {
                class C
                {
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for class and method declarations.
    /// </summary>
    [TestMethod]
    public void TypeAndMethodBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){}
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for constructor declarations.
    /// </summary>
    [TestMethod]
    public void ConstructorBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            C(){}
            }
            """;

        const string expected = """
            class C
            {
                C()
                {
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for property accessors.
    /// </summary>
    [TestMethod]
    public void PropertyAccessorBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            int P{
            get{}
            set{}
            }
            }
            """;

        const string expected = """
            class C
            {
                int P
                {
                    get
                    {
                    }
                    set
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for if and else blocks.
    /// </summary>
    [TestMethod]
    public void IfElseBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(bool condition){
            if (condition){}
            else{}
            }
            }
            """;

        const string expected = """
            class C
            {
                void M(bool condition)
                {
                    if (condition)
                    {
                    }
                    else
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for for-loops.
    /// </summary>
    [TestMethod]
    public void ForLoopBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            for (var i = 0; i < 1; i++){}
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    for (var i = 0; i < 1; i++)
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for foreach loops.
    /// </summary>
    [TestMethod]
    public void ForeachLoopBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            var items = new[] { 1 };
            foreach (var item in items){}
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var items = new[] { 1 };

                    foreach (var item in items)
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for while and do-while blocks.
    /// </summary>
    [TestMethod]
    public void WhileAndDoWhileBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            while (true){}
            do{}
            while (false);
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                    }

                    do
                    {
                    }
                    while (false);
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for try, catch, and finally blocks.
    /// </summary>
    [TestMethod]
    public void TryCatchFinallyBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            try{}
            catch{}
            finally{}
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    try
                    {
                    }
                    catch
                    {
                    }
                    finally
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for switch statement blocks.
    /// </summary>
    [TestMethod]
    public void SwitchStatementBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(int x){
            switch (x){
            case 1:{}
            default:{}
            }
            }
            }
            """;

        const string expected = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                        {
                        }
                        default:
                        {
                        }
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for object initializer blocks.
    /// </summary>
    [TestMethod]
    public void ObjectInitializerBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class Holder{
            public int Value { get; set; }
            }

            class C{
            void M(){
            var holder = new Holder(){ Value = 1 };
            }
            }
            """;

        const string expected = """
            class Holder
            {
                public int Value { get; set; }
            }

            class C
            {
                void M()
                {
                    var holder = new Holder()
                                 {
                                     Value = 1
                                 };
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for anonymous object initializer blocks.
    /// </summary>
    [TestMethod]
    public void AnonymousObjectInitializerBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            var value = new{ A = 1 };
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var value = new
                                {
                                    A = 1
                                };
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement for lambda block bodies.
    /// </summary>
    [TestMethod]
    public void LambdaBlockBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C{
            void M(){
            var value = Execute(() =>{});
            }

            int Execute(System.Func<int> callback){
            return callback();
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var value = Execute(() =>
                    {
                    });
                }

                int Execute(System.Func<int> callback)
                {
                    return callback();
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies brace placement when the entire input is written as a single minimized line.
    /// </summary>
    [TestMethod]
    public void FullyMinimizedSingleLineInputBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """class C{void M(){if (true){}}}""";

        const string expected = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    }
                }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that auto property braces remain on a single line.
    /// </summary>
    [TestMethod]
    public void AutoPropertyBracesRemainOnSingleLine()
    {
        // Arrange
        const string input = """
            class C{public int Prop { get; set; }}
            """;

        const string expected = """
            class C
            {
                public int Prop { get; set; }
            }
            """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that collection initializer braces in a method chain are moved to dedicated lines.
    /// </summary>
    [TestMethod]
    public void CollectionInitializerInMethodChainBracesAreMovedToNewLines()
    {
        // Arrange
        const string input = """
            class C
            {
                public void MultiLineChainMisaligned()
                {
                    var result = new System.Collections.Generic.List<int> { 1, 2, 3 }
                            .Where(x => x > 0)
                                .Select(x => x * 2)
                            .ToList();
                }
            }
            """;

        const string expected = """
        class C
        {
            public void MultiLineChainMisaligned()
            {
                var result = new System.Collections.Generic.List<int>
                             {
                                 1,
                                 2,
                                 3
                             }.Where(x => x > 0)
                              .Select(x => x * 2)
                              .ToList();
            }
        }
        """;

        // Act
        var actual = ApplyFormatter(input);

        // Assert
        Assert.AreEqual(expected.Replace("\r\n", "\n").TrimEnd('\n'), actual.Replace("\r\n", "\n"));
    }

    /// <summary>
    /// Applies the formatter to input code and returns the formatted output.
    /// </summary>
    /// <param name="input">Input source code.</param>
    /// <returns>Formatted source code.</returns>
    private static string ApplyFormatter(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree);

        return formattedTree.GetRoot().ToFullString();
    }

    #endregion // Methods
}