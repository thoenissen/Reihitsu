using System.Collections.Generic;

using Reihitsu.Formatter.Test.Helpers.Enumerations;
using Reihitsu.Formatter.Test.Regression.BlankLines;
using Reihitsu.Formatter.Test.Regression.FullPipeline;
using Reihitsu.Formatter.Test.Regression.Indentation;
using Reihitsu.Formatter.Test.Regression.LineBreaks;
using Reihitsu.Formatter.Test.Regression.UsingDirectives;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The exception list of the phase-level stability check: the fixtures whose final output a single phase changes today,
/// when it runs alone, because a later phase undoes the change in every pass. The list can only shrink: an entry whose
/// reversal no longer happens fails the test that carries it, so the change that removes a cause also deletes its entries
/// </summary>
public static class PhaseReversalExceptionList
{
    #region Fields

    /// <summary>
    /// The known phase reversals, one entry per fixture and phase, each with the output the phase returns
    /// </summary>
    public static readonly IReadOnlyList<PhaseReversalEntry> Entries = [
                                                                           new(typeof(DelimitedDocumentationBlankLineConvergenceTests), nameof(DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass), "c258de221e30fadf", PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective, "9631d3fcb3a10b34", PhaseReversalLineEndings.Both),
                                                                           new(typeof(UsingDirectiveOrderingTests), nameof(UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt), "855e76f82f59bced", PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective, "4048e372d9c31c30", PhaseReversalLineEndings.Both),
                                                                           new(typeof(CommentLedFirstTokenAnchorTests), nameof(CommentLedFirstTokenAnchorTests.CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine), "d98468a6e06d824b", PhaseReversalCauses.BlankLineNextToBrace, "052e3d7440412572", PhaseReversalLineEndings.Both),
                                                                           new(typeof(CommentLedFirstTokenAnchorTests), nameof(CommentLedFirstTokenAnchorTests.NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine), "79b71c9b01485d74", PhaseReversalCauses.BlankLineNextToBrace, "d52681f567f0de42", PhaseReversalLineEndings.Both),
                                                                           new(typeof(ArgumentFormattingFullPipelineTests), nameof(ArgumentFormattingFullPipelineTests.FormatsMethodChainWithArguments), "c328bbe3d627fc17", PhaseReversalCauses.LineStartIndentation, "fd65937e92e8ef79", PhaseReversalLineEndings.Both),
                                                                           new(typeof(IndentationTests), nameof(IndentationTests.LambdaBlockBodyInArgumentListIndentsCorrectly), "bcc307cbab39d7a4", PhaseReversalCauses.LineStartIndentation, "ef1fbcc3d5a16c25", PhaseReversalLineEndings.Both),
                                                                           new(typeof(IndentationTests), nameof(IndentationTests.SwitchExpressionInsideLambdaBreaksAndAligns), "c3d9f70128b65ace", PhaseReversalCauses.LineStartIndentation, "3385ba8051c36fd0", PhaseReversalLineEndings.Both),
                                                                           new(typeof(ListElementAttributeLayoutSinglePassTests), nameof(ListElementAttributeLayoutSinglePassTests.MinimalApiAttributedLambdaSettlesInOnePass), "d14ac64d5396f53c", PhaseReversalCauses.LineStartIndentation, "70f9c8b554a3af87", PhaseReversalLineEndings.Both),
                                                                           new(typeof(ParameterContinuationLineAlignmentTests), nameof(ParameterContinuationLineAlignmentTests.LambdaParameterInWrappedArgumentStaysAligned), "b8e2a139534a68aa", PhaseReversalCauses.LineStartIndentation, "a63cb7720189fa7e", PhaseReversalLineEndings.Both)
                                                                       ];

    #endregion // Fields
}