using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base analyzer for attribute placement rules by <see cref="AttributeTargets"/>
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type</typeparam>
public abstract class TargetAttributePlacementAnalyzerBase<TAnalyzer> : AttributeTargetRuleAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// Default placement policy
    /// </summary>
    private readonly TargetAttributePlacementMode _defaultPlacementMode;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="target">Target analyzed by this rule</param>
    /// <param name="defaultPlacementMode">Default placement mode</param>
    protected TargetAttributePlacementAnalyzerBase(string diagnosticId,
                                                   string titleResourceName,
                                                   string messageFormatResourceName,
                                                   AttributeTargets target,
                                                   TargetAttributePlacementMode defaultPlacementMode)
        : base(diagnosticId, titleResourceName, messageFormatResourceName, target)
    {
        _defaultPlacementMode = defaultPlacementMode;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Resolves the expected placement mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected placement mode</returns>
    protected virtual TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        return _defaultPlacementMode;
    }

    /// <summary>
    /// Analyzes an attribute list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAttributeList(SyntaxNodeAnalysisContext context)
    {
        var attributeList = (AttributeListSyntax)context.Node;

        if (AttributeTargetUtilities.TryResolveTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false
            || AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
        {
            return;
        }

        var closeLine = attributeList.GetLocation().GetLineSpan().EndLinePosition.Line;
        var nextLine = tokenAfter.GetLocation().GetLineSpan().StartLinePosition.Line;
        var expectedMode = ResolvePlacementMode(attributeList);
        var hasViolation = expectedMode == TargetAttributePlacementMode.SeparateLine
                               ? closeLine == nextLine
                               : closeLine != nextLine;

        if (hasViolation)
        {
            context.ReportDiagnostic(CreateDiagnostic(attributeList.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAttributeList, Microsoft.CodeAnalysis.CSharp.SyntaxKind.AttributeList);
    }

    #endregion // DiagnosticAnalyzer
}