using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0107: Code analysis suppression must have justification
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0107CodeAnalysisSuppressionMustHaveJustificationAnalyzer : DiagnosticAnalyzerBase<RH0107CodeAnalysisSuppressionMustHaveJustificationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0107";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0107CodeAnalysisSuppressionMustHaveJustificationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0107Title), nameof(AnalyzerResources.RH0107MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the attribute resolves to <see cref="System.Diagnostics.CodeAnalysis.SuppressMessageAttribute"/>
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="attributeSyntax">Attribute syntax</param>
    /// <returns><see langword="true"/> if the attribute is a suppression attribute</returns>
    private static bool IsSuppressMessageAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken);
        var constructorSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        return constructorSymbol?.ContainingType.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute";
    }

    /// <summary>
    /// Try to get the justification argument
    /// </summary>
    /// <param name="attributeSyntax">Attribute syntax</param>
    /// <param name="justificationArgument">Justification argument</param>
    /// <returns><see langword="true"/> if the argument exists</returns>
    private static bool TryGetJustificationArgument(AttributeSyntax attributeSyntax, out AttributeArgumentSyntax justificationArgument)
    {
        foreach (var attributeArgument in attributeSyntax.ArgumentList?.Arguments ?? [])
        {
            if (attributeArgument.NameEquals?.Name.Identifier.ValueText == "Justification")
            {
                justificationArgument = attributeArgument;

                return true;
            }
        }

        justificationArgument = null;

        return false;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.Attribute"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attributeSyntax)
        {
            return;
        }

        if (IsSuppressMessageAttribute(context, attributeSyntax) == false)
        {
            return;
        }

        if (TryGetJustificationArgument(attributeSyntax, out var justificationArgument) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(attributeSyntax.Name.GetLocation()));

            return;
        }

        var justificationValue = context.SemanticModel.GetConstantValue(justificationArgument.Expression, context.CancellationToken);

        if (justificationValue.HasValue == false
            || justificationValue.Value is not string justification
            || string.IsNullOrWhiteSpace(justification))
        {
            context.ReportDiagnostic(CreateDiagnostic(attributeSyntax.Name.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAttribute, SyntaxKind.Attribute);
    }

    #endregion // DiagnosticAnalyzer
}