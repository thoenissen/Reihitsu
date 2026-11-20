Role: spec author
Commit: cfa3d8d
Verdict: spec written

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answer 1A recorded in `00-lead.md`.

Clarification only: the spec now writes down behavior that already exists and that the maintainer chose to keep (answer A); no change doc is required (`sdd/README.md`, *When a change doc is required*: a clarification). Nothing visible changes. Rule page `documentation/rules/RH5604.md` not touched: what is reported and how to fix it is unchanged for the user in the normal case.

- Finding 1 (per answer A), `sdd/specs/analyzer/RH5604.md`:
  - RH5604-001: the trigger is now "the line breaks of a file are not all the same" instead of "`\r\n` and `\n` and both occur"; the predominant ending is `\r\n` when it occurs at least as often as all other line breaks together, `\n` otherwise (FMT-PIPE-007); it holds for every terminator FMT-EOL-001 names. New example: a lone `\r` between tokens, at a directive's end, and inside `///` in an LF file, each line reported; one sentence for U+0085/U+2028/U+2029.
  - RH5604-005: "In a file the analyzer reports", rewriting every differing line break "whatever line terminator it is". New example: lone `\r` between tokens, fixed to `\n`.
  - Intro: the `␍` convention gains "a `␍` with code behind it on the same line is a lone `\r`".
- Finding 2: `sdd/specs/formatter/line-endings.md` intro restored to `origin/main` (the implementation sentence and the restated code-owner rule dropped; no behavioral content was lost). `sdd/specs/analyzer/RH5604.md` intro: the implementation sentence replaced by the behavioral one — the fix converts the file whatever else it holds, also a file the formatter leaves unchanged for a syntax error or an unsupported shape (already demanded by RH5604-005, which has no exception).
- Checked: `Uses` unchanged and exact (FMT-EOL-001 was already listed); no code owner changed. `scripts/test.sh --project architecture`: 198/198. `scripts/test.sh --project analyzer --filter RH5604`: 47/47 on the unchanged code, so the examples agree with today's behavior (the new examples mirror the characterization fixtures; `apply-fix.sh` normalizes fixtures to LF/CRLF and cannot run mixed-ending examples).

## Changed requirements
- RH5604-001 — Lines that differ from the predominant line ending are reported — clarified (every line terminator)
- RH5604-005 — The fix converts every line ending of the file — clarified (every line terminator)

## Examples for the tests
Tags for the characterization tests 07-auditor finding 1 lists (`src/Reihitsu.Analyzer.Test/Formatting/RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`); no new test is needed:
- `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` — `RH5604-001`, `RH5604-005`
- `VerifyForeignTerminatorAtTheEndOfADirectiveIsFixedToThePredominantLineEnding` — `RH5604-001`, `RH5604-005`
- `VerifyForeignTerminatorInsideADocumentationCommentIsFixedToThePredominantLineEnding` — `RH5604-001`, `RH5604-005`

## Output questions
_None._

## Internal choices
- RH5604-002 left as is ("all `\r\n` or all `\n`"). Today a file whose every line break is the same other terminator (for example all lone `\r`) is not reported either (the analyzer reports only when more than one kind occurs); writing that down was not asked, no test pins it, and the rule's non-goal ("the rule only makes a file consistent with itself") already covers it. Alternative: extend RH5604-002 to every terminator and add a characterization test.
- No example for a file whose `\r` lines outnumber its `\r\n` lines with no `\n` at all (predominant `\n`, so every line is reported); it follows from the RH5604-001 text and FMT-PIPE-007's own example. Alternative: add an example and a test.

## Follow-up candidates
_None_ new.
