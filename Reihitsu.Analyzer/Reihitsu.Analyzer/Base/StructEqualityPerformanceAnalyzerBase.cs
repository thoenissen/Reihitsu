using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for struct equality performance analyzers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public class StructEqualityPerformanceAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Constructor

    /// <inheritdoc />
    internal StructEqualityPerformanceAnalyzerBase(string diagnosticId, DiagnosticCategory category, string tileResourceName, string messageFormatResourceName) : base(diagnosticId, category, tileResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checking of the equality members are implemented
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="type">Type</param>
    /// <returns>Are the equality members implemented?</returns>
    protected static bool AreEqualityMembersImplemented(Compilation compilation, ITypeSymbol type)
    {
        var equatableType = compilation.GetTypeByMetadataName("System.IEquatable`1")?.Construct(type);

        if (equatableType != null
            && type.Interfaces.Any(implementedInterface => SymbolEqualityComparer.Default.Equals(implementedInterface, equatableType)))
        {
            return true;
        }

        var hasOverrideOfEquals = false;
        var hasOverrideOfGetHashCode = false;

        foreach (var member in type.GetMembers())
        {
            if (member is IMethodSymbol method)
            {
                switch (method.Name)
                {
                    case "Equals"
                        when method.Parameters.Length == 1
                             && method.Parameters[0].Type.SpecialType == SpecialType.System_Object
                             && method.IsOverride:
                        {
                            hasOverrideOfEquals = true;
                        }
                        break;

                    case "GetHashCode" when method.Parameters.Length == 0
                                            && method.IsOverride:
                        {
                            hasOverrideOfGetHashCode = true;
                        }
                        break;
                }
            }
        }

        return hasOverrideOfEquals
               && hasOverrideOfGetHashCode;
    }

    #endregion // Methods
}