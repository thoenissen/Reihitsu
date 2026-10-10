using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Defines the direct parameter-list parents normalized by the formatter
/// </summary>
internal static class ParameterListParentPolicy
{
    #region Methods

    /// <summary>
    /// Determines whether the parameter list has an opening parenthesis with an intrinsic declaration anchor
    /// </summary>
    /// <param name="parameterList">Parameter list to inspect</param>
    /// <returns><see langword="true"/> when the opening parenthesis is covered; otherwise, <see langword="false"/></returns>
    internal static bool IsOpeningParenthesisCovered(ParameterListSyntax parameterList)
    {
        return parameterList.Parent is not ParenthesizedLambdaExpressionSyntax && IsClosingParenthesisCovered(parameterList);
    }

    /// <summary>
    /// Determines whether the parameter list belongs directly to a syntax shape whose closing parenthesis is normalized by the formatter
    /// </summary>
    /// <param name="parameterList">Parameter list to inspect</param>
    /// <returns><see langword="true"/> when the closing parenthesis is covered; otherwise, <see langword="false"/></returns>
    internal static bool IsClosingParenthesisCovered(ParameterListSyntax parameterList)
    {
        return parameterList.Parent is MethodDeclarationSyntax
                                    or ConstructorDeclarationSyntax
                                    or DestructorDeclarationSyntax
                                    or LocalFunctionStatementSyntax
                                    or OperatorDeclarationSyntax
                                    or ConversionOperatorDeclarationSyntax
                                    or DelegateDeclarationSyntax
                                    or RecordDeclarationSyntax
                                    or ClassDeclarationSyntax
                                    or StructDeclarationSyntax
                                    or InterfaceDeclarationSyntax
                                    or ExtensionBlockDeclarationSyntax
                                    or ParenthesizedLambdaExpressionSyntax
                                    or AnonymousMethodExpressionSyntax;
    }

    #endregion // Methods
}