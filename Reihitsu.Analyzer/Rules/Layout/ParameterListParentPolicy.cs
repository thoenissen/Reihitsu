using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Defines the direct parameter-list parents normalized by the formatter
/// </summary>
internal static class ParameterListParentPolicy
{
    #region Methods

    /// <summary>
    /// Determines whether the parameter list belongs directly to a syntax shape normalized by the formatter
    /// </summary>
    /// <param name="parameterList">Parameter list to inspect</param>
    /// <returns><see langword="true"/> when the direct parent is covered; otherwise, <see langword="false"/></returns>
    internal static bool IsCovered(ParameterListSyntax parameterList)
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