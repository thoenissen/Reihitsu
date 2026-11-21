using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Computes the cognitive complexity of a judged body (RH2009-003) and whether one of its structures with a nesting
/// increment lies in the nested part of a nesting construct other than a lambda expression or an anonymous method
/// (RH2009-004). Local functions declared in the body are left out; the bodies of lambda expressions and anonymous
/// methods count one nesting level deeper (RH2009-005). Code that a conditional directive excludes is disabled text,
/// which the walk does not visit (SHARED-DIAG-008)
/// </summary>
internal sealed class CognitiveComplexityWalker : CSharpSyntaxWalker
{
    #region Fields

    /// <summary>
    /// Semantic model of the body's syntax tree
    /// </summary>
    private readonly SemanticModel _semanticModel;

    /// <summary>
    /// The judged method or local function, normalized by <see cref="Normalize"/>, or <see langword="null"/> for a
    /// member that cannot be invoked
    /// </summary>
    private readonly IMethodSymbol _member;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Number of nesting constructs whose nested part contains the current node
    /// </summary>
    private int _nesting;

    /// <summary>
    /// Number of nesting constructs other than lambda expressions and anonymous methods whose nested part contains the
    /// current node
    /// </summary>
    private int _structuralNesting;

    /// <summary>
    /// Whether the combinators of the current pattern are counted: those of the pattern of an <c>is</c> expression are,
    /// those of a case label and of a switch expression arm are not
    /// </summary>
    private bool _countsPatternCombinators;

    /// <summary>
    /// Whether the body holds an invocation of the judged member itself
    /// </summary>
    private bool _isRecursive;

    /// <summary>
    /// Sum of the increments counted so far, recursion left out
    /// </summary>
    private int _increments;

    /// <summary>
    /// Whether a structure with a nesting increment lies in the nested part of a nesting construct other than a lambda
    /// expression or an anonymous method
    /// </summary>
    private bool _hasNestedStructure;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="semanticModel">Semantic model of the body's syntax tree</param>
    /// <param name="member">The judged method or local function, or <see langword="null"/> for another member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private CognitiveComplexityWalker(SemanticModel semanticModel, IMethodSymbol member, CancellationToken cancellationToken)
    {
        _semanticModel = semanticModel;
        _member = member == null ? null : Normalize(member);
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Computes the cognitive complexity of a judged body
    /// </summary>
    /// <param name="body">Block body, or the expression of an expression body</param>
    /// <param name="semanticModel">Semantic model of the body's syntax tree</param>
    /// <param name="member">The judged method or local function, whose invocations count as recursion, or <see langword="null"/> for a member that cannot be invoked</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="hasNestedStructure">Whether a structure with a nesting increment lies in the nested part of a nesting construct other than a lambda expression or an anonymous method (RH2009-004)</param>
    /// <returns>Cognitive complexity</returns>
    internal static int Compute(SyntaxNode body, SemanticModel semanticModel, IMethodSymbol member, CancellationToken cancellationToken, out bool hasNestedStructure)
    {
        var walker = new CognitiveComplexityWalker(semanticModel, member, cancellationToken);

        walker.Visit(body);

        hasNestedStructure = walker._hasNestedStructure;

        return walker._increments + (walker._isRecursive ? 1 : 0);
    }

    /// <summary>
    /// Normalizes a method symbol so that every reference to one method compares equal: the defining part of a partial
    /// method, the declared form of a reduced extension method, and the original definition of a constructed method
    /// </summary>
    /// <param name="method">Method symbol</param>
    /// <returns>Normalized symbol</returns>
    private static IMethodSymbol Normalize(IMethodSymbol method)
    {
        method = method.ReducedFrom ?? method;
        method = method.OriginalDefinition;

        return method.PartialDefinitionPart ?? method;
    }

    /// <summary>
    /// Strips the parentheses around an expression
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <returns>The expression inside every pair of parentheses</returns>
    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    /// <summary>
    /// Counts the sequences of one tree of operators: the first operator in source order and each operator that differs
    /// from the operator before it
    /// </summary>
    /// <param name="operators">Operators of the tree in source order</param>
    /// <returns>Number of sequences</returns>
    private static int CountSequences(List<SyntaxKind> operators)
    {
        var sequences = 0;

        for (var index = 0; index < operators.Count; index++)
        {
            if (index == 0
                || operators[index] != operators[index - 1])
            {
                sequences++;
            }
        }

        return sequences;
    }

    /// <summary>
    /// Collects the combinators of a tree of binary patterns joined directly or through parenthesized patterns, in
    /// source order
    /// </summary>
    /// <param name="pattern">Pattern</param>
    /// <param name="operators">Collected combinators</param>
    private static void CollectCombinators(PatternSyntax pattern, List<SyntaxKind> operators)
    {
        switch (pattern)
        {
            case BinaryPatternSyntax binary:
                {
                    CollectCombinators(binary.Left, operators);
                    operators.Add(binary.Kind());
                    CollectCombinators(binary.Right, operators);
                }
                break;

            case ParenthesizedPatternSyntax parenthesized:
                {
                    CollectCombinators(parenthesized.Pattern, operators);
                }
                break;
        }
    }

    /// <summary>
    /// Determines whether a type is <c>bool</c> or <c>bool?</c>
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns><see langword="true"/> for <c>bool</c> or <c>bool?</c>; otherwise <see langword="false"/></returns>
    private static bool IsBooleanOrNullableBoolean(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        return type.SpecialType == SpecialType.System_Boolean;
    }

    /// <summary>
    /// Adds the increment of a structure with a nesting increment: 1 plus the nesting level
    /// </summary>
    private void AddNestingStructure()
    {
        _increments += 1 + _nesting;

        if (_structuralNesting > 0)
        {
            _hasNestedStructure = true;
        }
    }

    /// <summary>
    /// Visits the nested part of a nesting construct one nesting level deeper
    /// </summary>
    /// <param name="node">Node of the nested part, or <see langword="null"/></param>
    /// <param name="isLambda">Whether the construct is a lambda expression or an anonymous method</param>
    private void VisitNested(SyntaxNode node, bool isLambda = false)
    {
        if (node == null)
        {
            return;
        }

        _nesting++;

        if (isLambda == false)
        {
            _structuralNesting++;
        }

        Visit(node);

        if (isLambda == false)
        {
            _structuralNesting--;
        }

        _nesting--;
    }

    /// <summary>
    /// Visits a pattern, counting its combinators or not
    /// </summary>
    /// <param name="pattern">Pattern</param>
    /// <param name="countsCombinators">Whether the combinators of the pattern are counted</param>
    private void VisitPattern(PatternSyntax pattern, bool countsCombinators)
    {
        var previous = _countsPatternCombinators;

        _countsPatternCombinators = countsCombinators;

        Visit(pattern);

        _countsPatternCombinators = previous;
    }

    /// <summary>
    /// Determines whether a binary expression is a binary logical operator: <c>&amp;&amp;</c> and <c>||</c>, and
    /// <c>&amp;</c>, <c>|</c>, and <c>^</c> when the operator is the logical operator of <c>bool</c> or <c>bool?</c>
    /// operands
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> for a binary logical operator; otherwise <see langword="false"/></returns>
    private bool IsLogicalOperator(ExpressionSyntax expression)
    {
        switch (expression.Kind())
        {
            case SyntaxKind.LogicalAndExpression:
            case SyntaxKind.LogicalOrExpression:
                return true;

            case SyntaxKind.BitwiseAndExpression:
            case SyntaxKind.BitwiseOrExpression:
            case SyntaxKind.ExclusiveOrExpression:
                return _semanticModel.GetSymbolInfo(expression, _cancellationToken).Symbol is IMethodSymbol { MethodKind: MethodKind.BuiltinOperator } method
                       && IsBooleanOrNullableBoolean(method.ReturnType);

            default:
                return false;
        }
    }

    /// <summary>
    /// Collects the operators of a tree of binary logical operators joined directly or through parentheses, in source
    /// order
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <param name="operators">Collected operators</param>
    private void CollectLogicalOperators(ExpressionSyntax expression, List<SyntaxKind> operators)
    {
        if (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            CollectLogicalOperators(parenthesized.Expression, operators);
        }
        else if (expression is BinaryExpressionSyntax binary
                 && IsLogicalOperator(binary))
        {
            CollectLogicalOperators(binary.Left, operators);
            operators.Add(binary.Kind());
            CollectLogicalOperators(binary.Right, operators);
        }
    }

    /// <summary>
    /// Determines whether a binary logical operator is the root of its tree: no binary logical operator holds it
    /// directly or through parentheses
    /// </summary>
    /// <param name="expression">Binary logical operator</param>
    /// <returns><see langword="true"/> for the root of a tree; otherwise <see langword="false"/></returns>
    private bool IsLogicalTreeRoot(ExpressionSyntax expression)
    {
        var parent = expression.Parent;

        while (parent is ParenthesizedExpressionSyntax)
        {
            parent = parent.Parent;
        }

        return parent is not BinaryExpressionSyntax binary
               || IsLogicalOperator(binary) == false;
    }

    /// <summary>
    /// Visits an <c>if</c> statement with its <c>else</c> clauses as one nesting construct
    /// </summary>
    /// <param name="node">The <c>if</c> statement that starts the chain</param>
    private void VisitIfChain(IfStatementSyntax node)
    {
        AddNestingStructure();

        while (true)
        {
            Visit(node.Condition);
            VisitNested(node.Statement);

            if (node.Else == null)
            {
                return;
            }

            _increments++;

            if (node.Else.Statement is IfStatementSyntax elseIf)
            {
                node = elseIf;

                continue;
            }

            VisitNested(node.Else.Statement);

            return;
        }
    }

    /// <summary>
    /// Visits a conditional expression with the conditional expressions that continue its chain as one nesting construct
    /// </summary>
    /// <param name="node">The conditional expression that starts the chain</param>
    private void VisitConditionalChain(ConditionalExpressionSyntax node)
    {
        AddNestingStructure();

        while (true)
        {
            Visit(node.Condition);
            VisitNested(node.WhenTrue);

            if (StripParentheses(node.WhenFalse) is ConditionalExpressionSyntax continuation)
            {
                _increments++;
                node = continuation;

                continue;
            }

            VisitNested(node.WhenFalse);

            return;
        }
    }

    /// <summary>
    /// Visits a lambda expression or an anonymous method: its body one nesting level deeper, the rest at the level where
    /// it is written
    /// </summary>
    /// <param name="node">Lambda expression or anonymous method</param>
    private void VisitAnonymousFunction(AnonymousFunctionExpressionSyntax node)
    {
        foreach (var child in node.ChildNodes())
        {
            if (child == node.Body)
            {
                VisitNested(child, true);
            }
            else
            {
                Visit(child);
            }
        }
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override void Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        base.Visit(node);
    }

    /// <inheritdoc/>
    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        // A local function is judged on its own and does not count toward the body that holds it (RH2009-005)
    }

    /// <inheritdoc/>
    public override void VisitIfStatement(IfStatementSyntax node)
    {
        VisitIfChain(node);
    }

    /// <inheritdoc/>
    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        VisitConditionalChain(node);
    }

    /// <inheritdoc/>
    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        AddNestingStructure();

        Visit(node.Expression);

        foreach (var section in node.Sections)
        {
            VisitNested(section);
        }
    }

    /// <inheritdoc/>
    public override void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
    {
        VisitPattern(node.Pattern, false);
        Visit(node.WhenClause);
    }

    /// <inheritdoc/>
    public override void VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        AddNestingStructure();

        Visit(node.GoverningExpression);

        foreach (var arm in node.Arms)
        {
            VisitNested(arm);
        }
    }

    /// <inheritdoc/>
    public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
    {
        VisitPattern(node.Pattern, false);
        Visit(node.WhenClause);
        Visit(node.Expression);
    }

    /// <inheritdoc/>
    public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
    {
        Visit(node.Expression);
        VisitPattern(node.Pattern, true);
    }

    /// <inheritdoc/>
    public override void VisitBinaryPattern(BinaryPatternSyntax node)
    {
        var parent = node.Parent;

        while (parent is ParenthesizedPatternSyntax)
        {
            parent = parent.Parent;
        }

        if (_countsPatternCombinators
            && parent is not BinaryPatternSyntax)
        {
            var operators = new List<SyntaxKind>();

            CollectCombinators(node, operators);

            _increments += CountSequences(operators);
        }

        base.VisitBinaryPattern(node);
    }

    /// <inheritdoc/>
    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (IsLogicalOperator(node)
            && IsLogicalTreeRoot(node))
        {
            var operators = new List<SyntaxKind>();

            CollectLogicalOperators(node, operators);

            _increments += CountSequences(operators);
        }

        base.VisitBinaryExpression(node);
    }

    /// <inheritdoc/>
    public override void VisitForStatement(ForStatementSyntax node)
    {
        AddNestingStructure();

        Visit(node.Declaration);

        foreach (var initializer in node.Initializers)
        {
            Visit(initializer);
        }

        Visit(node.Condition);

        foreach (var incrementor in node.Incrementors)
        {
            Visit(incrementor);
        }

        VisitNested(node.Statement);
    }

    /// <inheritdoc/>
    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        AddNestingStructure();

        Visit(node.Expression);
        VisitNested(node.Statement);
    }

    /// <inheritdoc/>
    public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
    {
        AddNestingStructure();

        Visit(node.Variable);
        Visit(node.Expression);
        VisitNested(node.Statement);
    }

    /// <inheritdoc/>
    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        AddNestingStructure();

        Visit(node.Condition);
        VisitNested(node.Statement);
    }

    /// <inheritdoc/>
    public override void VisitDoStatement(DoStatementSyntax node)
    {
        AddNestingStructure();

        VisitNested(node.Statement);
        Visit(node.Condition);
    }

    /// <inheritdoc/>
    public override void VisitCatchClause(CatchClauseSyntax node)
    {
        AddNestingStructure();

        Visit(node.Declaration);
        Visit(node.Filter);
        VisitNested(node.Block);
    }

    /// <inheritdoc/>
    public override void VisitGotoStatement(GotoStatementSyntax node)
    {
        _increments++;

        base.VisitGotoStatement(node);
    }

    /// <inheritdoc/>
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (_member != null
            && _isRecursive == false
            && _semanticModel.GetSymbolInfo(node, _cancellationToken).Symbol is IMethodSymbol target
            && SymbolEqualityComparer.Default.Equals(Normalize(target), _member))
        {
            _isRecursive = true;
        }

        base.VisitInvocationExpression(node);
    }

    /// <inheritdoc/>
    public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        VisitAnonymousFunction(node);
    }

    /// <inheritdoc/>
    public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        VisitAnonymousFunction(node);
    }

    /// <inheritdoc/>
    public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        VisitAnonymousFunction(node);
    }

    #endregion // CSharpSyntaxVisitor
}