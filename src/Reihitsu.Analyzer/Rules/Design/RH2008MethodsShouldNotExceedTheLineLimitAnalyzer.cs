using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2008: Methods should not exceed the line limit
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2008MethodsShouldNotExceedTheLineLimitAnalyzer : LineCountAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2008";

    /// <summary>
    /// Default limit of a member's body in production code
    /// </summary>
    private const int DefaultProductionLimit = 60;

    /// <summary>
    /// Default limit of a member's body in test code
    /// </summary>
    private const int DefaultTestLimit = 120;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2008MethodsShouldNotExceedTheLineLimitAnalyzer()
        : base(DiagnosticId,
               design => design.MaximumMethodLines,
               DefaultProductionLimit,
               design => design.MaximumTestMethodLines,
               DefaultTestLimit,
               [.. MemberBodyUtilities.MemberKinds])
    {
    }

    #endregion // Constructor

    #region LineCountAnalyzerBase

    /// <inheritdoc/>
    private protected override bool TryGetCountedSpan(SyntaxNode node, out TextSpan span, out Location location)
    {
        if (MemberBodyUtilities.TryGetBody(node, out var body, out var expressionBody, out var nameToken)
            && TryGetBodySpan(body, expressionBody, out span))
        {
            location = nameToken.GetLocation();

            return true;
        }

        span = default;
        location = null;

        return false;
    }

    #endregion // LineCountAnalyzerBase
}