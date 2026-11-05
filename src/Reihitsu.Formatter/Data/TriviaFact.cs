using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The kind and text of one trivia, as a plain value that holds no trivia of a tree (FMT-PIPE-024)
/// </summary>
internal readonly struct TriviaFact
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">The kind of the trivia</param>
    /// <param name="text">The full text of the trivia</param>
    public TriviaFact(SyntaxKind kind, string text)
    {
        Kind = kind;
        Text = text;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The kind of the trivia
    /// </summary>
    public SyntaxKind Kind { get; }

    /// <summary>
    /// The full text of the trivia
    /// </summary>
    public string Text { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Captures the kind and text of the specified trivia
    /// </summary>
    /// <param name="trivia">The trivia</param>
    /// <returns>The plain value</returns>
    public static TriviaFact From(SyntaxTrivia trivia)
    {
        return new TriviaFact(trivia.Kind(), trivia.ToFullString());
    }

    /// <summary>
    /// Creates a trivia of no tree with this kind and text. A comment, whitespace, or line break is created as it is; a
    /// structured trivia is parsed from its text
    /// </summary>
    /// <returns>The trivia, as many as parsing the text yields</returns>
    public SyntaxTriviaList ToTrivia()
    {
        switch (Kind)
        {
            case SyntaxKind.WhitespaceTrivia:
            case SyntaxKind.EndOfLineTrivia:
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
            case SyntaxKind.DisabledTextTrivia:
                {
                    return SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(Kind, Text));
                }

            default:
                {
                    return SyntaxFactory.ParseTrailingTrivia(Text);
                }
        }
    }

    #endregion // Methods
}