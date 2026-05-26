using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Common analyzer base for attribute-target rules
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type</typeparam>
public abstract class AttributeTargetRuleAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// Target analyzed by this rule
    /// </summary>
    private readonly AttributeTargets _target;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="target">Target analyzed by this rule</param>
    protected AttributeTargetRuleAnalyzerBase(string diagnosticId,
                                              string titleResourceName,
                                              string messageFormatResourceName,
                                              AttributeTargets target)
        : base(diagnosticId, DiagnosticCategory.Formatting, titleResourceName, messageFormatResourceName)
    {
        _target = target;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the attribute list is in scope for this analyzer
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when the attribute list should be analyzed</returns>
    protected virtual bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return target == _target;
    }

    #endregion // Methods
}