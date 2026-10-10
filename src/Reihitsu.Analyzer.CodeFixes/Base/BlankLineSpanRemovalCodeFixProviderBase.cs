namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for blank-line rules whose fix removes the reported diagnostic span. A
/// named specialization of <see cref="WhitespaceSpanRemovalCodeFixProviderBase"/> so blank-line rules read
/// distinctly from spacing rules while sharing its whitespace-only guard instead of duplicating it
/// </summary>
public abstract class BlankLineSpanRemovalCodeFixProviderBase : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    private protected BlankLineSpanRemovalCodeFixProviderBase(string diagnosticId, string title)
        : base(diagnosticId, title)
    {
    }

    #endregion // Constructor
}