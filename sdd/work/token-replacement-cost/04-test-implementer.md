Role: test implementer
Commit: (this commit; tests written against 54442e3)
Verdict: tests written

My prompt named the issue, the slug, the lane, the branch, and the handovers to read, and asked for whole-file tests at the tab-replacement site `TabReplacementPhase.cs:39`, as my earlier handover (`02-test-implementer.md`) asked if that site changed. It carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

Per decision row 2, no test asserts time. Each test pins output FMT-TAB demands on a generated input of 300 members (a few thousand tokens) where the phase actually rewrites, so a rewrite that drops, misplaces, or adds an edit fails it. Every test runs under LF and CRLF.

All 11 tests pass on 54442e3 and on the phase as it was before the change (`git show 06d658a~1:src/Reihitsu.Formatter/Pipeline/TabReplacement/TabReplacementPhase.cs`, swapped in temporarily and then restored). To check that they can fail, I ran three mutations of the phase and reverted each one:
- Skipping the directive pass when no code token holds a tab: `ReplacesTheTabsInsideTheDirectivesOfALargeFileWithoutTabsInTheCode` fails.
- Leaving trailing trivia unreplaced: 7 tests fail.
- Rewriting only tokens before position 2000: 8 tests fail.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TabReplacementPhaseWholeFileTests.ReplacesEveryTabInTheWhitespaceOfTheCodeOfALargeFile` | FMT-TAB-001, FMT-TAB-003 | `TabReplacementPhase.Execute` on a parsed file | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesTheTabsInFrontOfEveryCommentLineOfALargeFileParsedWithoutDocumentationComments` | FMT-TAB-001, FMT-TAB-003 (FC-040: `DocumentationMode.None`, so `///` lines are ordinary comments and the tabs in front of them are replaced) | same | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesTheTabsOfEveryOtherMemberAndKeepsTheWhitespaceOfTheMembersBetweenThem` | FMT-TAB-001 (both sides of the `HasWhitespaceTab` filter in the callback) | same | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesTheTabsAroundAndInsideTheDirectivesOfALargeFile` | FMT-TAB-001, FMT-TAB-002, FMT-TAB-003 (the token pass and the directive pass on the same token's leading trivia; region description and disabled text keep their tabs) | same | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesTheTabsInsideTheDirectivesOfALargeFileWithoutTabsInTheCode` | FMT-TAB-002 (the `Any(HasWhitespaceTab)` is false, but the directive pass still runs) | same | pass |
| `TabReplacementPhaseWholeFileTests.KeepsALargeFileWhoseTabsAllStandInTextThatKeepsThem` | FMT-TAB-003 (no tab in code whitespace, so the shortcut returns the input) | same | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesTheTabsInFrontOfTheFirstTokenAndOnTheLastLineOfALargeFile` | FMT-TAB-001 (first token at position 0; `EndOfFileToken` leading trivia) | same | pass |
| `TabReplacementPhaseWholeFileTests.ReplacesEveryTabInTheWhitespaceOfADetachedRoot` | FMT-TAB-001 (root without a parent) | `TabReplacementPhase.Execute` on `ParseMemberDeclaration` | pass |
| `TabReplacementPhaseWholeFileTests.ThrowsWhenCancellationIsRequested` | — (cancellation, behavior only) | same | pass |
| `LargeTabIndentedFileTests.FormatsALargeTabIndentedFileLikeTheSameFileWithSpaces` | FMT-TAB-001, FMT-TAB-003 | `AssertRuleResult` (LF/CRLF, second pass, phase stability) — the CLI path | pass |
| `LargeTabIndentedFileTests.FormatsALargeTabIndentedDetachedNodeLikeTheSameNodeWithSpaces` | FMT-PIPE-023 (a target with tabs gets the output of the same target with spaces) | `ReihitsuFormatter.FormatNode` | pass |

The generated inputs place tabs in these positions:
- in front of a line's first token and in front of the first line of a documentation comment;
- between tokens, in the code of an interpolation hole, and in front of a trailing comment;
- at the end of a code line and on whitespace-only lines;
- between directive tokens, with tab indentation in front of directives.

They place tabs that must stay as written in these positions:
- inside documentation-comment text and in front of continuation `///` lines when documentation comments are parsed;
- inside interpolated-string text and inside comments;
- inside region descriptions and `#endregion` comments, and inside disabled text.

Existing tests that assert a changed output: none. The change keeps every output byte-identical, so no existing test is changed. The existing `Unit/TabReplacement/TabReplacementPhaseTests` and `Regression/TabReplacement/TabReplacementTests` stay as they are. The whole formatter project passes on this head (5935 tests).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Large tab-indented file, tab sub-phase | LF, CRLF | phase-level tests above | output as specified | no output defect; the reproduction is the measurement in `02-test-implementer.md` and `03-implementer.md` |

## Not covered, and why

- **Timing in a test**: excluded by decision row 2.
- **Cancellation in the middle of a file**: only an already-cancelled token is tested. Cancelling in the middle needs a hook or a timed token, which would make the test flaky. This is the same as in `02-test-implementer.md`.
- **Whitespace at the end of a single-line comment**: it is part of the comment text, so a tab there is kept. I found this while writing the tests: my first draft put line-end whitespace behind a trailing comment. Cleaning that whitespace is the pending non-goal in thoenissen/Reihitsu#959. The inputs avoid it instead of pinning that output.
- **`FormatNodeInDocumentAsync` with tabs**: the write-back restores the document's tabs in front of the target (FMT-PIPE-018/019). The existing `TabReplacementTests.FormatNodeInDocumentAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass` covers that path. The new rewrite is the same code path for every entry point, so a large node-in-document input adds nothing over the detached-node test.
- **`ReplaceDirectiveTriviaTabs` and `ReplaceDirectiveTabs` (`:137`/`:150`)**: these were not changed (`03-implementer.md`). They are still exercised by the directive tests above.
- **Failure classes not applicable**: the same as in `02-test-implementer.md`. FC-010 is covered by the second pass and phase stability in `AssertRuleResult`. FC-040 is covered by the `DocumentationMode.None` test.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~TabReplacementPhaseWholeFileTests|FullyQualifiedName~LargeTabIndentedFileTests"
scripts/test.sh --project formatter
```

## Follow-up candidates

_None._
