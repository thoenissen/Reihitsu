using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Diagnostic analyzer base class
/// </summary>
public class DiagnosticAnalyzerBase : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Rule
    /// </summary>
    private readonly DiagnosticDescriptor _rule;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName)
        : this(diagnosticId, category, titleResourceName, messageFormatResourceName, true)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    /// <param name="isEnabledByDefault">Whether the rule is enabled by default</param>
    internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, bool isEnabledByDefault)
    {
        var title = new LocalizableResourceString(titleResourceName, AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
        var messageFormat = new LocalizableResourceString(messageFormatResourceName, AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

        _rule = new DiagnosticDescriptor(diagnosticId, title, messageFormat, category.ToString(), DiagnosticSeverity.Warning, isEnabledByDefault, helpLinkUri: $"https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/{diagnosticId}.md");
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Create diagnostic
    /// </summary>
    /// <param name="location">Location</param>
    /// <returns>Created <see cref="Diagnostic"/> object</returns>
    protected Diagnostic CreateDiagnostic(Location location)
    {
        return Diagnostic.Create(_rule, location);
    }

    /// <summary>
    /// Create diagnostic
    /// </summary>
    /// <param name="location">Location</param>
    /// <param name="messageArgs">Message arguments</param>
    /// <returns>Created <see cref="Diagnostic"/> object</returns>
    protected Diagnostic CreateDiagnostic(Location location, params object[] messageArgs)
    {
        return Diagnostic.Create(_rule, location, messageArgs);
    }

    /// <summary>
    /// Create diagnostic
    /// </summary>
    /// <param name="locations">Locations</param>
    /// <returns>Created <see cref="Diagnostic"/> object</returns>
    protected Diagnostic CreateDiagnostic(ImmutableArray<Location> locations)
    {
        return locations.Length > 1
                   ? Diagnostic.Create(_rule, locations[0], locations.Skip(1))
                   : Diagnostic.Create(_rule, locations[0]);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }

    #endregion // DiagnosticAnalyzer
}