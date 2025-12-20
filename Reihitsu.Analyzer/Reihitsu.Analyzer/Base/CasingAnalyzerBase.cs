using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for casing analyzers.
/// </summary>
/// <typeparam name="T">The type of the derived class</typeparam>
public abstract class CasingAnalyzerBase<T> : DiagnosticAnalyzerBase<T>
    where T : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// The syntax kind to analyze
    /// </summary>
    private readonly SyntaxKind _type;

    /// <summary>
    /// The function to validate the casing
    /// </summary>
    private readonly Func<string, bool> _casingValidation;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">The diagnostic ID</param>
    /// <param name="category">The diagnostic category</param>
    /// <param name="titleResourceName">The resource name for the title of the diagnostic</param>
    /// <param name="messageFormatResourceName">The resource name for the message format of the diagnostic</param>
    /// <param name="type">The syntax kind to analyze</param>
    /// <param name="casingValidation">The function to validate the casing</param>
    private protected CasingAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind type, Func<string, bool> casingValidation)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName)
    {
        _type = type;
        _casingValidation = casingValidation;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the node.
    /// </summary>
    /// <param name="context">The syntax node analysis context</param>
    private void OnAnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        foreach (var (name, location) in GetLocations(context.Node))
        {
            CheckCasing(name, location, context);
        }
    }

    /// <summary>
    /// Checks the casing of the given name and reports a diagnostic if it's not valid
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="location">The location of the name in the source code</param>
    /// <param name="context">The syntax node analysis context</param>
    private void CheckCasing(string name, Location location, SyntaxNodeAnalysisContext context)
    {
        if (_casingValidation(name) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    /// <summary>
    /// Gets the locations of the names to check.
    /// </summary>
    /// <param name="node">Syntax node</param>
    /// <returns>The locations of the names to check</returns>
    protected abstract IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node);

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAnalyzeNode, _type);
    }

    #endregion // DiagnosticAnalyzer
}