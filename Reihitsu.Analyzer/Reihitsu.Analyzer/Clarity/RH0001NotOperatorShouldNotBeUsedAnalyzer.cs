using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Clarity
{
    /// <summary>
    /// RH0001: The logical operator ! should not be used for clarity.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RH0001NotOperatorShouldNotBeUsedAnalyzer : DiagnosticAnalyzer
    {
        #region Constants

        /// <summary>
        /// Diagnostic ID
        /// </summary>
        public const string DiagnosticId = "RH0001";

        /// <summary>
        /// Category
        /// </summary>
        private const string Category = "Clarity";

        #endregion // Constants

        #region Fields

        /// <summary>
        /// Title
        /// </summary>
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RH0001_Title), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// Title
        /// </summary>
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RH0001_MessageFormat), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// Rule
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
        /// </summary>
        /// <param name="context">Context</param>
        private void OnLogicalNotExpressionSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PrefixUnaryExpressionSyntax node)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.OperatorToken.GetLocation()));
            }
        }

        #endregion // Methods

        #region DiagnosticAnalyzer

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        /// <summary>
        /// Called once at session start to register actions in the analysis context.
        /// </summary>
        /// <param name="context">Context</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(OnLogicalNotExpressionSyntaxNode, SyntaxKind.LogicalNotExpression);
        }

        #endregion // DiagnosticAnalyzer
    }
}