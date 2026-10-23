Role: test implementer
Commit: 26eca7a
Verdict: tests written

Second repair round, findings 4 and 7 of `11-auditor.md` (routing in `00-lead.md`, decision `squad/decisions/2026-10-23-1331-second-repair-round.md`).

Inputs:
- `00-lead.md`, `04-test-implementer.md`, `08-test-implementer.md`, `11-auditor.md`;
- SHARED-FIX-006 and SHARED-FIX-007 in `sdd/specs/shared/code-fixes.md`, SHARED-ORDER-002;
- RH7107-004 and RH7108-004;
- thoenissen/Reihitsu#1335.

I re-derived both changes from these sources and used the auditor's suggestions as candidates only. The coordinator's message carried no intended fix or verdict.

No assertion, test input, or expected output changed. No tag changed. I touched no file under `sdd/specs/`. The spec author's uncommitted spec edits in the working tree are not part of my commits.

Commits:
- `765cdcf`: test summaries, finding 4.
- `26eca7a`: helpers, finding 7.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7108…Tests.SingleLineEventAccessorsAreFixedOntoSeparateLines` (summary only) | FC-031. The summary now says the test pins today's behavior. The decided behavior is in SHARED-FIX-007 (moved code: the moved `add` keeps its braces and line breaks, and its separators follow SHARED-ORDER-002) and in SHARED-FIX-006 (the event that holds the accessors is not formatted). The change belongs to an existing follow-up. | `Verify(source, fixed)` | pass, untagged |
| `RH7107…Tests.SingleLineExpressionBodiedAccessorsAreFixedOntoSeparateLines` (summary only) | FC-031. Same treatment. The separators around the moved `get` stay (SHARED-FIX-007 → SHARED-ORDER-002), and the property is not formatted (SHARED-FIX-006). | `Verify` | pass, untagged |
| `RH7107…Tests.MultiLineAutoIndexerAccessorsAreFixedOntoSingleLine` (summary only) | FC-031. Same treatment: the line breaks around the moved `get` stay, and the indexer is not formatted. | `Verify` | pass, untagged |
| `RH7107…Tests.BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` (summary only; the auditor did not list it) | FC-031. Its old summary called the bracing of the `set` accessor's `if` body a precondition of the move ("rewrites the accessor being moved"). The new summary keeps the test's subject (the blank line). It marks the bracing as today's behavior, which SHARED-FIX-006 and SHARED-FIX-007 decide against. thoenissen/Reihitsu#1335 names this test under "Tests that change" (instance #1326 F1). | `Verify` | pass, untagged |
| `RH8307…Tests.VerifyHalfWrappedElementThatTheFormatterChangesIsReportedAndFixed` (helper only) | FC-015. The private `Format` is removed, and the test now calls the new protected `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Format`, which has the same body. The usings that only the private helper needed are dropped. | `Format` + `Verify` | pass |
| `RH7309…Tests.MovedRegionBlocksKeepTheirLayout` (helper only) | FC-015. Same change as RH8307. | `Format` + `Verify` | pass |
| `AnalyzerTestsBase<TAnalyzer, TCodeFix>.AssertFormattingChangesNothing` (helper only) | FC-015. It now calls `Format` and keeps its assertion and message. The four test classes that use it were run: RH7103, RH5417, RH7310, and RH7309 through the shared base. | — | pass |
| `RH8402…Tests.FileNotStartingWithHeaderWhenDocumentationModeIsParse` (helper only) | FC-015. `VerifySpecificationFile` gains an overload that takes an `onConfigure`, applied after the checked file, the configuration, and `SkipSuppressionCheck` are set. That is the same order as the inlined body it replaces. The existing overload delegates with `null`. The test passes `test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject)`, with the same source, configuration, and expected diagnostic as before. | `VerifySpecificationFile` | pass |

## Scenarios (bug fix)

_N/A._ This is a repair round on existing tests.

## Not covered, and why

- **Follow-up for the three single-line accessor tests.** thoenissen/Reihitsu#1335 covers RH7107-004, RH7108-004, and SHARED-FIX-006/007 in its scope ("Organization fixes change only the node they rewrite or move", P26). Its instance list, however, names only `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` and the bracing shapes. It does not name the single-line expansion or the collapse that these three tests pin. The lead may want to add them to #1335 as instances. The source names no tracker number.
- **Other move-fix siblings.** I swept the summaries of the RH7102–RH7110, RH7309, and RH7004 test classes for layout claims.
  - `RH7103…MultipleBlankLinesAboveTheMovedMemberCollapseToOneAfterReordering` is about the separator in front of the moved member. It is tagged RH7103-006, and SHARED-ORDER-002 was not changed by this run, so it is out of scope.
  - `RH7004…VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` keeps the moved statements as written. The blank line it inserts in front of `using (` is the SHARED-FIX-006 blank-line case (P14a, #1335 A4). The new moved-code bullet does not decide it, and its summary does not claim it.
  - The brace fixes RH5405–RH5407 pin SHARED-BRACE-002, which is a spec requirement. The lead routed that conflict to thoenissen/Reihitsu#1408, so those tests are not characterization tests and are not reworded here.
- **Other `FormatSyntaxTree` call sites in the analyzer tests.** The calls in `RH5111…Tests` and in `Formatter/Formatting/*FormatterTests` existed before this run. They also have a different shape: an async root, the `TestContext` token, or two passes. None of them duplicates the removed helper, so they stay.
- **New tests.** None. Both findings are about comments and helpers.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7107PropertyAccessors|FullyQualifiedName~RH7108EventAccessors|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH8307TextAfter|FullyQualifiedName~RH8402File|FullyQualifiedName~RH7103Static"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessor|FullyQualifiedName~RH7310EmptyRegions"
scripts/test.sh --project architecture
scripts/verify-text-only.sh --base 7795f5f --head 765cdcf
```

Results:
- First analyzer filter: 141/141 passed.
- Second analyzer filter: 65/65 passed.
- Architecture: 72/72 passed.
- `scripts/format.sh` over the 6 changed test files: 0 files changed.
- Text-only proof for the summary commit: `TEXT-ONLY PROOF: PASS — base=7795f5f head=765cdcf — 2 changed file(s) (0 non-compiled, 2 C# comment/layout-only)`.
