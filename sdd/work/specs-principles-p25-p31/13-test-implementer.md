Role: test implementer
Commit: 7098757
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
- `7098757`: the new RH7103 test for the SHARED-DOC-004 example that the spec author added in `9fee74f`.

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

## Re-check after the spec author's `9fee74f`

The coordinator asked for this after `12-spec-author.md`. I re-derived it from the amended SHARED-DIAG-004 and SHARED-DOC-004 text, not from the spec author's recommendations.

### Tests tagged SHARED-DIAG-004

- `RH2102…Tests.VerifyPartialClassDeclaredInTwoPartsIsReportedOnce`: first paragraph, unchanged. It still covers.
- `RH7104…Tests.PartThatOmitsTheAccessibilityIsReportedAndFixedAloneWhenAnotherPartDeclaresIt`: the rescoped second paragraph names RH7104 and the accessibility of one part of a partial type explicitly. It still covers.
- `RH2004…Tests.VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic`: **I kept the tag.**
  - The test pins exactly the outcome the recaptioned SHARED-DIAG-004 example states: RH2004 reports neither part of the two-part type.
  - The tag was set by the maintainer's answer `squad/decisions/2026-10-21-1260-diag004-weak-coverage.md`. The recaption keeps the example inside SHARED-DIAG-004, so the premise of that answer still holds.
  - Dropping the tag would reverse a recorded decision. That belongs to the lead, not to a repair round.
  - What the tag cannot do: no SHARED-DIAG-004 sentence makes this test fail. RH2004 is silent because of RH2004-002. The amended second paragraph names RH7104 as the per-part rule and does not require RH2004 to be silent. The test is weak coverage, as the 1260 decision already recorded.
  - If the lead decides to drop it, the change is one line: remove `[Requirement("SHARED-DIAG-004")]`. SHARED-DIAG-004 stays covered by the RH2102 and RH7104 tests.
  - Its summary ("partial types with accessibility declared on another part are owned by RH7104") matches the new caption, so it is unchanged.
- The untagged RH2004 characterization tests `VerifyPartialMemberPartsAreReported` and `VerifyOneFixApplicationChangesOnePartOfPartialProperty` say that SHARED-DIAG-004 judges a partial member by its symbol, reports it at most once, and withholds the fix. The new partial-member paragraph states exactly that, so their summaries are right again and stay unchanged.
- The new RH2004 partial-property example gets no test. It is decided behavior that does not conform today (thoenissen/Reihitsu#1287), which follows the precedent `2026-10-21-1260-razor-content-no-conforming-shape.md`. The two characterization tests above pin today's opposite and are already reworded.

### Tests tagged SHARED-DOC-004

There are 21 tagged tests (RH8001, RH8013, RH8019, RH8020, RH8026, RH8030, RH8031, RH8304, RH8308).
- 19 of them are about which comments belong to a declaration. That text is unchanged.
- `RH8304…VerifyElementsOnSameLineInCommentBehindCodeAreDetectedAndFixed` and `RH8308…VerifyTextAfterClosingTagInCommentBehindCodeIsDetectedAndFixed` are fixes that move neither the declaration nor the code in front of it. The scoped clause ("moves neither … MUST leave the comment behind that code") still applies to them. They still cover.

### New test for the SHARED-DOC-004 RH7103 example

The example states a new MUST: a fix that moves the declaration the comment documents carries the comment with it. No test covered this side of the clause (FC-003), so I added one.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (new, `[DataRow("\n")]` and `[DataRow("\r\n")]`) | SHARED-DOC-004, the moved-declaration sentence and its RH7103 example. Also SHARED-ORDER-002. FC-003, FC-033 | `Verify(source)` for the report on `Create`, then `ApplyCodeFixAsync` on the plain source in each line ending. The result is parsed with `DocumentationMode.Parse`, and structural assertions follow. | pass, both rows |

The structural assertions:
- `Create` comes before `Run`.
- `Create`'s leading trivia holds exactly one `///` comment, whose `<summary>` text is `Creates`.
- No comment remains inside `Run`.
- `Creates` occurs once in the result.

Two things are deliberately not asserted:
- Where the comment starts, which the requirement leaves uncovered.
- The layout of the comment's lines and of the rest of the type. The RH7103 fix formats the whole type, which is the bug thoenissen/Reihitsu#969 (SHARED-FIX-006).

What I saw today, under both line endings, without pinning it: the comment travels and starts its own line above `Create`. The whole-type formatting also expands it to a multi-line `<summary>`, and there is no blank line between `Create` and `Run`. The input had none either.

My first draft asserted the one-line `<summary>Creates</summary>` and failed on exactly that expansion. That failure is the #969 layout, not a SHARED-DOC-004 defect, so the assertion now compares only the summary text.

Mode `None` is not pinned. There the behind-code `///` may belong to the brace's trivia rather than to `Create` (`12-spec-author.md`).

## Scenarios (bug fix)

_N/A._ This is a repair round on existing tests.

## Not covered, and why

- **Follow-up for the three single-line accessor tests.** thoenissen/Reihitsu#1335 covers RH7107-004, RH7108-004, and SHARED-FIX-006/007 in its scope ("Organization fixes change only the node they rewrite or move", P26). Its instance list, however, names only `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` and the bracing shapes. It does not name the single-line expansion or the collapse that these three tests pin. The lead may want to add them to #1335 as instances. The source names no tracker number.
- **Other move-fix siblings.** I swept the summaries of the RH7102–RH7110, RH7309, and RH7004 test classes for layout claims.
  - `RH7103…MultipleBlankLinesAboveTheMovedMemberCollapseToOneAfterReordering` is about the separator in front of the moved member. It is tagged RH7103-006, and SHARED-ORDER-002 was not changed by this run, so it is out of scope.
  - `RH7004…VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` keeps the moved statements as written. The blank line it inserts in front of `using (` is the SHARED-FIX-006 blank-line case (P14a, #1335 A4). The new moved-code bullet does not decide it, and its summary does not claim it.
  - The brace fixes RH5405–RH5407 pin SHARED-BRACE-002, which is a spec requirement. The lead routed that conflict to thoenissen/Reihitsu#1408, so those tests are not characterization tests and are not reworded here.
- **Other `FormatSyntaxTree` call sites in the analyzer tests.** The calls in `RH5111…Tests` and in `Formatter/Formatting/*FormatterTests` existed before this run. They also have a different shape: an async root, the `TestContext` token, or two passes. None of them duplicates the removed helper, so they stay.
- **New tests for findings 4 and 7.** None; both findings are about comments and helpers. The coordinator's follow-up added one test (see above).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7107PropertyAccessors|FullyQualifiedName~RH7108EventAccessors|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH8307TextAfter|FullyQualifiedName~RH8402File|FullyQualifiedName~RH7103Static"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessor|FullyQualifiedName~RH7310EmptyRegions"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-DIAG-004|TestCategory=SHARED-DOC-004"
scripts/test.sh --project architecture
scripts/verify-text-only.sh --base 7795f5f --head 765cdcf
```

Results:
- First analyzer filter: 141/141 passed before the new test. After it, the RH7103 class passes 42/42.
- Requirement-category filter on `7098757`: 26/26 passed (24 tagged tests, plus the two data rows of the new test).
- Second analyzer filter: 65/65 passed.
- Architecture: 72/72 passed, also on `7098757`.
- `scripts/format.sh` over the 7 changed test files: 0 files changed.
- Text-only proof for the summary commit: `TEXT-ONLY PROOF: PASS — base=7795f5f head=765cdcf — 2 changed file(s) (0 non-compiled, 2 C# comment/layout-only)`.
