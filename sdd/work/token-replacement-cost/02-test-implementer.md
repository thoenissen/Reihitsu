Role: test implementer
Commit: (this commit; tests written against 6a97c6c)
Verdict: reproduced (by measurement); tests written

My prompt named the issue, the slug, the lane, the branch, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict.

## Reproduction

Per *Decisions taken* rows 1 and 2, the reproduction is a measurement, not a test: no wall-clock or ratio assertion is committed. I re-ran the skeptic's probe (`01-skeptic.md`, *Probe source*) on this head, Release build (`scripts/build.sh`), variant `plain`, best of 3, ms:

| N | Tokens | Identity `ReplaceTokens` (all tokens) | Indentation phase | `LayoutComputer.Compute` alone | Cleanup phase | Total |
|---|---|---|---|---|---|---|
| 400 | 2409 | 47.1 | 80.2 | 30.4 | 51.7 | 292.2 |
| 800 | 4809 | 179.3 / 180.6 | 270.4 / 279.0 | 35.4 / 33.2 | 196.3 / 202.3 | 613.6 / 699.4 |
| 1600 | 9609 | 825.5 / 771.5 | 1171.2 / 1142.7 | 121.0 / 123.2 | 878.5 / 992.5 | 2499.2 / 2609.7 |

Observed: cleanup and the replacement share of indentation grow about 4x per doubling of the token count. Expected (narrowed goal, row 1): the whole-tree replacement in both phases grows about linearly. The validator re-runs the same probe after the fix for the *after* column.

Command: `dotnet run probe.cs -- /home/user/Reihitsu/src/Reihitsu.Cli/bin/Release/net11.0 plain`, probe source copied verbatim from `01-skeptic.md`.

## Tests

All tests pass on 6a97c6c: they pin today's output where the two phases actually rewrite (row 2), so a replacement that drops, misplaces, or adds an edit fails them. Every test runs under LF and CRLF. Expected values come from the named requirements; the one untagged test characterizes unspecified output.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `IndentationRewriterWholeFileTests.IndentsEveryMemberAndDocumentationCommentLineOfALargeFile` | FMT-INDENT-002, FMT-INDENT-008 | `LayoutComputer.Compute` + `IndentationRewriter.Apply`, 300 generated members | pass |
| `IndentationRewriterWholeFileTests.IndentsEveryMemberAndCommentLineOfALargeFileParsedWithoutDocumentationComments` | FMT-INDENT-002, FMT-INDENT-006 (FC-040: `DocumentationMode.None`) | same | pass |
| `IndentationRewriterWholeFileTests.PlacesDirectivesRegionsDisabledTextAndCommentsWhileIndentingTheCodeAroundThem` | FMT-INDENT-007, FMT-INDENT-009, FMT-INDENT-010, FMT-INDENT-011 (FC-019/FC-020) | same | pass |
| `IndentationRewriterWholeFileTests.MovesRawStringLinesUnderTheOpeningQuotesBehindADocumentationComment` | FMT-RAW-001, FMT-RAW-002, FMT-RAW-003 (FC-022, `placedColumns`) | same | pass |
| `IndentationRewriterWholeFileTests.PlacesTheLinesAroundAMissingToken` | FMT-INDENT-002 (missing token) | same | pass |
| `IndentationRewriterWholeFileTests.PlacesTheFirstTokenOfTheFile` | FMT-INDENT-002 (first token, position 0) | same | pass |
| `IndentationRewriterWholeFileTests.PlacesTheLastLineInFrontOfTheEndOfTheFile` | — (characterization: whitespace-only lines in the `EndOfFileToken` leading trivia; no spec states it) | same | pass |
| `CleanupPhaseWholeFileTests.RemovesWhitespaceInFrontOfEveryLineBreakOfALargeFile` | FMT-CLEAN-001, FMT-CLEAN-005 | `CleanupPhase.Execute`, 300 generated members | pass |
| `CleanupPhaseWholeFileTests.RemovesWhitespaceThatEndsALargeFileWithoutAFinalLineBreak` | FMT-CLEAN-001, FMT-CLEAN-005 | same | pass |
| `CleanupPhaseWholeFileTests.RemovesWhitespaceInFrontOfTheLineBreakOfTheFirstLine` | FMT-CLEAN-001 (`FullSpan.Start == 0`) | same | pass |
| `CleanupPhaseWholeFileTests.KeepsDisabledTextAndLiteralsWhileCleaningTheCodeAroundThem` | FMT-CLEAN-001, FMT-CLEAN-004 (FC-019/FC-020/FC-022) | same | pass |
| `CleanupPhaseWholeFileTests.CleansADetachedRoot` | FMT-CLEAN-001, FMT-CLEAN-008 (`GetNextToken` returns None) | same | pass |
| `CleanupPhaseWholeFileTests.ThrowsWhenCancellationIsRequested` | — (cancellation, behavior only) | same | pass |
| `LargeFileFullPipelineTests.IndentsAndCleansEveryLineOfALargeFile` | FMT-INDENT-002, FMT-INDENT-008, FMT-CLEAN-001, FMT-CLEAN-005 | `AssertRuleResult` (LF/CRLF, second pass, phase stability) — the CLI path | pass |
| `LargeFileFullPipelineTests.IndentsAndCleansEveryLineOfADetachedNode` | FMT-INDENT-002, FMT-CLEAN-001, FMT-CLEAN-008 (FC-028) | `ReihitsuFormatter.FormatNode` | pass |
| `LargeFileFullPipelineTests.IndentsAndCleansEveryLineOfANodeInItsDocument` | FMT-INDENT-002, FMT-CLEAN-001 (FC-028) | `ReihitsuFormatter.FormatNodeInDocumentAsync` | pass |

Existing tests that assert a changed output: none. The change keeps every output byte-identical, so no existing test is changed. The whole formatter project passes on this head (5923 tests).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Synthetic class, N = 400/800/1600 documented properties | LF | whole pipeline in process (probe) | reproduced | cleanup and the indentation replacement grow ~4x per doubling; expected ~2x |
| Same inputs, output | LF, CRLF | phase-level and pipeline tests above | output as specified | no output defect; none was reported |

The skeptic's CLI timing and the `tabs`/`mixed`/`docnospace`/`array` variants were not re-run; they are in `01-skeptic.md`.

## Not covered, and why

- **Timing in a test** — excluded by decision row 2 (flaky; a whole-phase ratio would fail after a correct fix).
- **Cancellation in the middle of a file** — only an up-front cancelled token is tested (the existing `IndentationRewriterTests.ApplyThrowsWhenCancellationIsRequested`, and the new cleanup test). Cancelling after a given number of tokens needs a hook into the phase or a timed token, which would be flaky.
- **Whitespace at the end of a directive line, a comment, or a documentation comment line** — the cleanup spec lists these as non-goals with decided changes pending (thoenissen/Reihitsu#959, #960). Pinning today's output would assert a known bug; the inputs above contain directives and documentation comments without that whitespace.
- **Byte order mark** — covered by the existing FMT-CLEAN-009 tests in `CleanupPhaseTests`; the `startsText` path is also exercised by `RemovesWhitespaceInFrontOfTheLineBreakOfTheFirstLine`.
- **Line break in the next token's leading trivia** — covered by the existing `CleanupPhaseTests.RemovesTrailingWhitespaceBeforeLineBreakInFollowingTokenLeadingTrivia`; it only arises from an earlier phase's tree, so a whole-file parse cannot produce it.
- **Sibling sites (row 3)** — tab replacement, line-ending normalization, and documentation-comment relocation keep their existing tests (`Regression/TabReplacement`, `ReihitsuFormatterLineEnding*Tests`, `Unit/DocumentationComments`). If the implementer changes them, the lead sends me back for whole-file tests there.
- **Failure classes not applicable**: FC-001/002/015/044 (no guard or policy changes), FC-004/005/021 (no line join or token move), FC-006/007/008/009/018/030/036 (no code fix or rename), FC-011/012 (no loop or neighbour interaction changes), FC-013/014 (no analyzer), FC-023 to FC-027/FC-029 (no syntax-specific decision changes), FC-038/039/041 to FC-043 (no shape declared unsupported), FC-037 (not a text-only change). FC-010 is covered by the second pass and phase stability in `AssertRuleResult`. FC-031 is the implementer's and the auditor's to check.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~CleanupPhaseWholeFileTests|FullyQualifiedName~IndentationRewriterWholeFileTests|FullyQualifiedName~LargeFileFullPipelineTests"
scripts/test.sh --project formatter
```

## Follow-up candidates

_None_ beyond those in `00-lead.md`.
