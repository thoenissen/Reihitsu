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

    /// <summary>
    /// Supported diagnostics, cached so the driver's repeated access does not allocate a fresh array
    /// </summary>
    private readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category)
        : this(diagnosticId, category, true)
    {
    }

    /// <summary>
    /// Constructor. The title and the message format of the rule are the resources <c>&lt;ID&gt;Title</c> and
    /// <c>&lt;ID&gt;MessageFormat</c> of <see cref="AnalyzerResources"/>, named by the diagnostic ID
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="isEnabledByDefault">Whether the rule is enabled by default</param>
    internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category, bool isEnabledByDefault)
    {
        var title = new LocalizableResourceString(diagnosticId + "Title", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
        var messageFormat = new LocalizableResourceString(diagnosticId + "MessageFormat", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

        _rule = new DiagnosticDescriptor(diagnosticId, title, messageFormat, category.ToString(), DiagnosticSeverity.Warning, isEnabledByDefault, helpLinkUri: $"https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/{diagnosticId}.md");
        _supportedDiagnostics = [_rule];
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Title of the rule in the current UI culture
    /// </summary>
    public string Title => _rule.Title.ToString();

    /// <summary>
    /// Message format of the rule in the current UI culture
    /// </summary>
    public string MessageFormat => _rule.MessageFormat.ToString();

    #endregion // Properties

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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }

    #endregion // DiagnosticAnalyzer
}