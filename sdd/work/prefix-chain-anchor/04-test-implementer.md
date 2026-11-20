Role: test implementer
Commit: f27d246
Verdict: tests written

Issue thoenissen/Reihitsu#896, lane behavior change. My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-CHAIN-007, FMT-INDENT-029, RH5201-001/-002, RH5112-003, RH5111-006 and the change doc's visible changes; every unchanged line is `main`'s output (`scripts/format.sh` on a fixture), every changed column is counted by hand from the spec's anchor rule. Case `q` is tested only through the anchor (lead decision 3): a `GetAnchorLink` unit row and RH5201 silence on the 3A layout; no formatter-output test pins the `*/.Next` join.

Discovery aid, not a source of values (FC-035): to find every existing test that pins today's anchor, I ran the five relevant test projects in a throw-away worktree under the scratchpad with a minimal `GetAnchorLink` patch ("last link in front of the first link that starts a line"), then removed the worktree. The checked-out tree's production code was never touched. Only 11 existing tests failed there (listed below); the hand-derived expected values of all new and updated tests then passed against that patch, which cross-checks the counting.

## Tests

New and updated tests. "Expected now" = on `f27d246` (unchanged production code).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Core `FluentChainTests.GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink` (23 rows; new) | SHARED-CHAIN-007; FC-003 (one vs two links in front; comment own-line vs behind code), FC-020, FC-027 | `FluentChain.Create` + `GetAnchorLink` | 17 rows fail (index 1 expected, `FirstLink` returned); the 6 rows with expected index 0 pass |
| Core `FluentChainTests.GetAnchorLinkReturnsFirstInvokedLinkBeforeFirstWrappedLink` (updated: `x.Items.Count` row moved to the test above; rows `g` and `x.Next.Self().Next` added) | SHARED-CHAIN-007 | same | pass |
| Formatter `PrefixChainAnchorTests.PrefixOfPropertyReadsAlignsToTheLastOfThemWithOrWithoutCall` (FMT-INDENT-029 example verbatim) | FMT-INDENT-029, SHARED-CHAIN-007 | `AssertRuleResult` (LF, CRLF, 2nd pass, phase stability) | fail |
| `PrefixChainAnchorTests.WrappedLinksAlignToTheAnchorOfTheSharedChainExample` (SHARED-CHAIN-007 example verbatim) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.WrappedCallBehindTwoPropertyReadsAlignsUnderTheLastOfThem` (visible change 1) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.ChainWithoutCallAlignsUnderTheLastLinkInFrontOfItsFirstWrappedLink` (visible change 2, `c`, `j`) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.LaterPrefixWrapsConditionalLinksAndJoinedFirstLinkFollowTheSameAnchor` (visible change 3, `d`, `g`, `h`) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.OneLinkOrAFirstCallInFrontOfTheFirstWrappedLinkKeepsItsAnchor` (unchanged side: `x.Next` ⏎ `.Next.Next`, `x?[0, 1].Next`, `x.Next.Self()` ⏎ `.Next`, skeptic p13) | SHARED-CHAIN-007; FC-003 | `AssertRuleResult` | pass |
| `PrefixChainAnchorTests.LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindEveryOperatorKind` (`!.`, `?.`, `System.String.Empty`) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindARootThatSpansLines` (multi-line invocation, initializer rebase) | SHARED-CHAIN-007; FC-027 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindARawStringRoot` | SHARED-CHAIN-007; FC-022 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.DirectiveInFrontOfTheFirstWrappedLinkKeepsTheLastPrefixLinkAsAnchor` (skeptic p12) | SHARED-CHAIN-007; FC-019, FC-020 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.LinkBehindCommentThatStartsItsOwnLineStartsALineForTheAnchor` (skeptic candidate 10, SHARED-CHAIN-005 `r` with a prefix) | SHARED-CHAIN-007; FC-003, FC-019, FC-021 | `AssertRuleResult` | pass |
| `PrefixChainAnchorTests.PrefixChainInsideLambdaBodyAndArgumentAlignsUnderTheLastPrefixLink` | SHARED-CHAIN-007; FC-027 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.PrefixAnchorBehindTabAndDelimitedDocumentationCommentAlignsInOnePass` (tab matrix, skeptic candidate 9) | SHARED-CHAIN-007; tab matrix, FC-010 | `AssertRuleResult` | fail |
| `PrefixChainAnchorTests.FormatNodeAlignsPrefixAnchorBehindTabAndDelimitedDocumentationCommentInOnePass` | SHARED-CHAIN-007; FC-028, tab matrix | `ReihitsuFormatter.FormatNode`, LF+CRLF | fail |
| `PrefixChainAnchorTests.FormatNodeInDocumentAlignsPrefixAnchorBehindTabAndDelimitedDocumentationCommentInOnePass` | SHARED-CHAIN-007; FC-028, tab matrix | `FormatNodeInDocumentAsync`, LF+CRLF | fail |
| Formatter `ChainLinkLayoutTests.WrappedCallAfterPropertyPrefixStaysUnderPrefix` (updated: `.ToString()` under `.Count`; tag added) | SHARED-CHAIN-007 | `AssertRuleResult` | fail |
| Formatter `ConditionalAccessWrapBetweenOperatorTokensTests.MemberAccessOnlyChainStartsConditionalLinkWithQuestionMark` (updated: `?.D` under `.C`) | — (FMT-BREAK test; column only incidental) | `AssertRuleResult` | fail |
| Analyzer `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyExampleOfLaterCallSharingALineIsReported` (RH5201-001 example verbatim) | RH5201-001, SHARED-CHAIN-007 | `Verify(source, diagnostics)` | fail (`d` reported on main) |
| `RH5201…AnalyzerTests.VerifyLinkWrappedBehindPrefixIsReportedUnlessUnderTheLastPrefixLink` (RH5201-002 new example verbatim; fix of `a` and `d` in one doc, Fix All) | RH5201-002, RH5201-005, SHARED-CHAIN-007; FC-007, FC-008 | `Verify(source, fixed, diagnostics)` | fail |
| `RH5201…AnalyzerTests.VerifyLinkWrappedBehindPrefixIsFixedUnderTheLastPrefixLinkWithCrLf` | RH5201-002, RH5201-005, SHARED-CHAIN-007; FC-010 | `VerifyCodeFixWithEndOfLine("\r\n")` | fail |
| `RH5201…AnalyzerTests.VerifyLinkBehindCommentThatStartsBehindCodeIsTheAnchor` (case `q`, decision 3: 3A layout silent LF+CRLF; `.Self()` under the first `.Next` reported) | RH5201-002, SHARED-CHAIN-007; FC-003, FC-021 | `Verify(source[, diagnostics])` | fail (main reports `.Self()` at (10,15) on the 3A layout) |
| `RH5201…AnalyzerTests.VerifyPrefixLinksSharingALineAreNotReported` (updated: `.Self()` under the second `.Next`; SHARED-CHAIN-007 tag added) | RH5201-001, SHARED-CHAIN-005, SHARED-CHAIN-007 | `Verify(source)` | fail |
| `RH5201…AnalyzerTests.VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument` (updated to the RH5111-006 example columns) | RH5111-006 | `Verify(source)` LF+CRLF | fail |
| `RH5112…AnalyzerTests.VerifyFixAlignsLaterLinkUnderTheLastPrefixLink` (RH5112-003 second example verbatim, LF and CRLF rows) | RH5112-003, SHARED-CHAIN-007; FC-007, FC-010 | `VerifyCodeFixWithEndOfLine` | fail (both rows) |
| `RH5112…AnalyzerTests.VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument` (updated columns) | RH5111-006 | `Verify(source)` | pass (RH5112 does not judge columns) |
| `RH5111…ConditionalAccessTests.VerifyFixAlignsAWrappedMemberBehindANestedConditionalAccessUnderTheAnchor` (updated: `.C` under the second `?`) | RH5111-006 | `VerifyUnderBothLineEndings` | fail |
| `RH5111…ConditionalAccessTests.VerifyFixMovesAWrappedMemberUnderTheLastBindingOfANestedConditionalAccessIntoTheAnchorColumn` (updated) | RH5111-006 | `VerifyUnderBothLineEndings` | fail |
| `RH5111…ConditionalAccessTests.VerifyFixAlignsAWrappedMemberBehindAReceiverWithAMemberAccessUnderTheAnchor` (updated: `.C` under `?` of `a.B?.B`) | RH5111-006 | `VerifyUnderBothLineEndings` | fail |
| `RH5111…ConditionalAccessTests.VerifyFixAllLaysOutEveryAssignmentBehindAConditionalAccess` (updated, RH5111-006 first example) | RH5111-006; FC-008 | `Verify` with 6 diagnostics, LF+CRLF | fail |
| `RH5201FluentChainLinkFormatterTests.VerifyMisalignedCallAfterPropertyPrefixIsReported` (updated fixed source: under `.Count`; tag added) | SHARED-CHAIN-007; FC-013 | `VerifyFormatter(source, fixed, diag)` | fail |
| `RH5201FluentChainLinkFormatterTests.VerifyMisalignedLinkOfCallLessChainIsReportedAndAlignedUnderTheLastLink` | SHARED-CHAIN-007; FC-013 | `VerifyFormatter(source, fixed, diag)` | fail |
| `RH5201FluentChainLinkFormatterTests.VerifyLinksAlignedUnderTheLastPrefixLinkStayUnchanged` | SHARED-CHAIN-007; FC-013 | `VerifyFormatter(source)` | fail |
| `RH5112FluentChainLinkFormatterTests.VerifyWrappedLinkOfCompoundNullConditionalAssignmentAfterPrefixIsNotReported` (updated: `.C` under `?` of `x.Y?.B`; tag added) | SHARED-CHAIN-007; FC-013 | `VerifyFormatter(source)` | fail |

Totals on `f27d246` with the focused filter below: Core 17 rows fail / 39 pass, Formatter 15 fail / 130 pass, Analyzer 16 fail / 366 pass. Every failure is the old anchor (the chain's first link) where the specs now demand the last link in front of the first wrapped link; I read the failure messages for the formatter tests, the q test, and the RH5111-006 layout test, and each differs only in that column. `scripts/test.sh --project architecture`: 198 passed (requirement tags valid).

Existing tests checked and left unchanged because they keep their output: the RH6023 tests the spec author named (they passed against the discovery patch), `ChainLinkLayoutTests.NullForgivingConditionalLinkAfterPrefixStaysAligned` (one link in front), every other test of `MethodChainAlignmentTests`, `ChainLinkBehindMultiLineCommentTests`, the CLI and Playground suites (passed against the discovery patch).

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- Case `q` as a full formatter-output test: withheld per lead decision 3 (would pin the `*/.Next` join that departs from FMT-BREAK-019).
- Self-hosting (`Reihitsu.SelfHosting.Test`): the repository sources the spec author lists will be reformatted by the implementer; the existing self-hosting suite covers that in CI, not duplicated.
- FC-004/FC-005: the change moves no trivia and joins no lines (which links start a line is unchanged); comment and directive gaps are still covered by the directive, own-line-comment, behind-code-comment and tab/`/** */` tests.
- FC-006, FC-018, FC-023–FC-025, FC-029, FC-036, FC-038–FC-043: no rewrite of semantics, no rename, no declared shape in this change.
- FC-011/FC-012: no loop or phase pair changes; the anchor is read only by the indentation phase.
- FC-031 (stale `GetAnchorLink` summary) is a review item for the implementer, not testable.
- SHARED-CHAIN-009 mid-line chain start is not re-tested separately: the anchor choice is the same `GetAnchorLink` call for every entry point (`rg -n "GetAnchorLink" src --glob '!*Test*'` → `MethodChainAlignmentContributor.GetAnchorColumn` and `RH5201MethodChainsShouldBeAlignedAnalyzer.GetAnchorColumn`); the node-level entry points are covered by the tab tests and the RH5201/RH5112/RH5111 fix tests.

## Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~FluentChainTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~PrefixChainAnchorTests|FullyQualifiedName~ChainLinkLayoutTests|FullyQualifiedName~ConditionalAccessWrapBetweenOperatorTokensTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201|FullyQualifiedName~RH5112|FullyQualifiedName~RH5111"
```

## Follow-up candidates

_None._
