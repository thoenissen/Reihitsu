using System.Collections.Generic;

using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.UsingDirectives;
using Reihitsu.Formatter.Test.Helpers.Enumerations;
using Reihitsu.Formatter.Test.Regression.BlankLines;
using Reihitsu.Formatter.Test.Regression.FullPipeline;
using Reihitsu.Formatter.Test.Regression.Indentation;
using Reihitsu.Formatter.Test.Regression.LineBreaks;
using Reihitsu.Formatter.Test.Regression.UsingDirectives;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The fixtures whose final output a single phase changes today, when it runs alone, because a later phase undoes the
/// change in every pass. The list can only shrink: an entry whose reversal no longer happens fails the test that carries
/// it, so the change that removes a cause also deletes its entries
/// </summary>
public static class PhaseReversalExceptions
{
    #region Fields

    /// <summary>
    /// The known phase reversals, one entry per fixture and phase
    /// </summary>
    public static readonly IReadOnlyList<PhaseReversalException> Entries = [
                                                                               new(typeof(DelimitedDocumentationBlankLineConvergenceTests), nameof(DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass), "c258de221e30fadf", nameof(UsingDirectiveOrderingPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective),
                                                                               new(typeof(UsingDirectiveOrderingTests), nameof(UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt), "855e76f82f59bced", nameof(UsingDirectiveOrderingPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective),
                                                                               new(typeof(CommentLedFirstTokenAnchorTests), nameof(CommentLedFirstTokenAnchorTests.CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine), "d98468a6e06d824b", nameof(BlankLinePhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.BlankLineNextToBrace),
                                                                               new(typeof(CommentLedFirstTokenAnchorTests), nameof(CommentLedFirstTokenAnchorTests.NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine), "79b71c9b01485d74", nameof(BlankLinePhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.BlankLineNextToBrace),
                                                                               new(typeof(WrappedAngleBracketListTests), nameof(WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn), "0bd97eed9cf654ec", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation),
                                                                               new(typeof(ArgumentFormattingFullPipelineTests), nameof(ArgumentFormattingFullPipelineTests.FormatsMethodChainWithArguments), "c328bbe3d627fc17", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation),
                                                                               new(typeof(IndentationTests), nameof(IndentationTests.LambdaBlockBodyInArgumentListIndentsCorrectly), "bcc307cbab39d7a4", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation),
                                                                               new(typeof(IndentationTests), nameof(IndentationTests.SwitchExpressionInsideLambdaBreaksAndAligns), "c3d9f70128b65ace", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation),
                                                                               new(typeof(ListElementAttributeLayoutSinglePassTests), nameof(ListElementAttributeLayoutSinglePassTests.MinimalApiAttributedLambdaSettlesInOnePass), "d14ac64d5396f53c", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation),
                                                                               new(typeof(ParameterContinuationLineAlignmentTests), nameof(ParameterContinuationLineAlignmentTests.LambdaParameterInWrappedArgumentStaysAligned), "b8e2a139534a68aa", nameof(LineBreakPhase), PhaseReversalLineEndings.Both, PhaseReversalCauses.LineStartIndentation)
                                                                           ];

    #endregion // Fields
}