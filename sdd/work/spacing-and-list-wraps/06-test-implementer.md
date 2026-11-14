Role: test implementer
Commit: c053836
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out decisions 16 and 17 of `00-lead.md` on the test side. Expected values come from FMT-BREAK-012 as of a8ba6f3 and the spec author's concurrent wording of the `#if X` example (the example now states that `X` is defined, and that without `X` the list stays as written), and from change doc entry 7 `O`; none was taken from the output of c119a8a.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ListWrapByElementsTests.DirectiveHeldArgumentCommaStaysWhileArgumentsInFrontAreSplit` (decision 16: parses with `X` defined) | FMT-BREAK-012; FC-019, FC-020 | `AssertRuleResult` with `WithPreprocessorSymbols("X")` | pass |
| `ListWrapByElementsTests.SpecificationExampleHeldCommasStayAndCommentsMoveWithTheirComma` (decision 16: parses with `X` defined; the example's `P` method, added to the spec in a8ba6f3, added so the test stays verbatim) | FMT-BREAK-012 | `AssertRuleResult` with `WithPreprocessorSymbols("X")` | pass |
| `ListWrapByElementsTests.ArgumentCommaInInactiveBranchLeavesSingleLineListUnchanged` (new: the other side of decision 16 — without `X`, the parsed list `1, 2` starts on one line and stays as written) | FMT-BREAK-012; FC-003, FC-019 | `AssertRuleResult` (input unchanged) | pass |
| `ParameterContinuationLineAlignmentTests.IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter` (decision 17: expected `int first,⏎int second // keep⏎, int third`; summary reworded) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `ParameterContinuationLineAlignmentTests.CommaAloneOnItsLineStaysAligned` (tag only; the lone held comma clause of FMT-BREAK-012, decision 14) | FMT-BREAK-012 | `AssertRuleResult` (input unchanged) | pass |
| `ParameterContinuationLineAlignmentTests.CommaAfterDirectiveHeldFirstParameterStaysAligned` (tag only; the lone held comma clause, held by `#endif`) | FMT-BREAK-012 | `AssertRuleResult` (input unchanged) | pass |

The tag fits the convention: both tests assert exactly the sentence FMT-BREAK-012 now states for a held comma alone on its line (its line break and the element behind it stay as written), so they are derived from that requirement, not incidental to it. The indexer test is tagged for the same reason: its expected output is now the requirement's per-gap split.

The two `X` tests failed on c119a8a without the symbol for the reason 05-implementer gives (the list parsed as single-line); with `X` they pass. The inactive-branch test passes on c119a8a, as the definition demands.

## Not covered, and why

- Unchanged from 04-test-implementer. No new condition was added to production code in this round.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ListWrapByElementsTests|FullyQualifiedName~IndexerParameterListLayoutTests|FullyQualifiedName~LineBreakRequirementTests|FullyQualifiedName~ParameterContinuationLineAlignmentTests|FullyQualifiedName~MultiLineCommentGapSpacingTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests|FullyQualifiedName~RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests"
```

Results on c119a8a plus these test changes (Debug): the focused formatter run passed 102 of 102, the focused analyzer run 55 of 55, the full formatter project 4972 of 4972, and architecture 118 of 118.
