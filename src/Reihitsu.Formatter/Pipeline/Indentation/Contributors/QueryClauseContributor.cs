using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns every clause of a query expression that starts a line — a <c>from</c>, <c>let</c>, <c>where</c>, <c>join</c>,
/// <c>orderby</c>, <c>select</c>, or <c>group</c> clause, or the <c>into</c> of a query continuation — with the first token of
/// the query expression the clause belongs to, wherever the query stands (FMT-INDENT-035). The clauses behind an <c>into</c>
/// belong to the query expression that holds the continuation. A line inside a clause that starts at another token of the
/// clause is a continuation line of that clause (FMT-INDENT-034)
/// </summary>
internal sealed class QueryClauseContributor : ILayoutContributor
{
    #region Constants

    /// <summary>
    /// The source identifier for layout entries contributed by this class
    /// </summary>
    private const string QueryClauseSource = "QueryClause";

    #endregion // Constants

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not QueryExpressionSyntax queryExpression)
        {
            return;
        }

        var column = LayoutComputer.GetAdjustedColumn(queryExpression.GetFirstToken(), model);

        for (var body = queryExpression.Body; body != null; body = body.Continuation?.Body)
        {
            foreach (var clause in body.Clauses)
            {
                LayoutComputer.SetIfFirstOnLine(clause.GetFirstToken(), column, QueryClauseSource, model);
            }

            if (body.SelectOrGroup != null)
            {
                LayoutComputer.SetIfFirstOnLine(body.SelectOrGroup.GetFirstToken(), column, QueryClauseSource, model);
            }

            if (body.Continuation != null)
            {
                LayoutComputer.SetIfFirstOnLine(body.Continuation.GetFirstToken(), column, QueryClauseSource, model);
            }
        }
    }

    #endregion // ILayoutContributor
}