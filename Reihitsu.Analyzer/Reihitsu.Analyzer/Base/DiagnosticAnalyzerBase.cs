using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Diagnostic analyzer base class
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public class DiagnosticAnalyzerBase<TAnalyzer> : DiagnosticAnalyzer
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Locking static initializing
    /// </summary>
    private static object _lock = new();

    /// <summary>
    /// Are all statics initialized?
    /// </summary>
    private static bool _isInitialized;

    /// <summary>
    /// Rule
    /// </summary>
    private static DiagnosticDescriptor _rule;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="tileResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category, string tileResourceName, string messageFormatResourceName)
    {
        lock (_lock)
        {
            if (_isInitialized == false)
            {
                var title = new LocalizableResourceString(tileResourceName, AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
                var messageFormat = new LocalizableResourceString(messageFormatResourceName, AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

                _rule = new DiagnosticDescriptor(diagnosticId, title, messageFormat, category.ToString(), DiagnosticSeverity.Warning, true);

                _isInitialized = true;
            }
        }
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
    /// <param name="locations">Locations</param>
    /// <returns>Created <see cref="Diagnostic"/> object</returns>
    protected Diagnostic CreateDiagnostic(ImmutableArray<Location> locations)
    {
        return locations.Length > 1
                   ? Diagnostic.Create(_rule, locations[1], locations.Skip(1))
                   : Diagnostic.Create(_rule, locations[0]);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <summary>
    /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <summary>
    /// Called once at session start to register actions in the analysis context.
    /// </summary>
    /// <param name="context">Context</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }

    #endregion // DiagnosticAnalyzer
}