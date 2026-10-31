Role: auditor
Commit: f57c5d73fcdfbc2a78f7c529815a38e2e5d0d5dc
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `23-auditor.md` (and the earlier audits `12-auditor.md` and `17-auditor.md`), handovers 24 to 26, `sdd/specs/analyzer/RH6001.md`, SHARED-BLANK-008, and the diff.

## Scope

- **Head:** f57c5d7, checked out, clean tree.
- **Base:** `origin/main` = 802afbf (the SHA in my prompt; `git fetch` gives the same). It is an ancestor of the head.
- **What I audited:** repair round 2, ba5a99b..f57c5d7, as a delta against `23-auditor.md` (of ba5a99b).
- **Diff:** tests only — six analyzer tests in `src/Reihitsu.Analyzer.Test/Formatting/RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.cs` and one formatter test in `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.cs`, plus handovers 23 to 26 and two lines in `00-lead.md`. No production code, spec, or change doc changed. Scope did not grow.
- **Diff against the handovers:** it matches `24-test-implementer.md` (seven tests), `25-cleaner.md` (no edits), and `26-spec-checker.md` (no edits).
- **Results I trusted:** the cleaner's build (0 warnings) and the focused run `RH6001` (36 passed), and the test implementer's run against the analyzer from 802afbf (the three new `///` rows and the new `VerifyFormatter` row fail; the `//` controls pass).
- **Probes:** I formatted the six test inputs with `scripts/format.sh --force`, twice, from fixtures in the session scratchpad. Every input is kept apart from the final line break, except `if// x` / `/* c */(c)`, which gets the blank line its control test carries. The second pass gave `Formatted 0 of 6`. So every "this is the formatter's output for it" in the new summaries is true.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Closed in `17-auditor.md`. The repair touches no production code. |
| 2 | closed | Closed in `17-auditor.md`. Unchanged. |
| 3 | closed | Closed in `17-auditor.md`. Unchanged. |
| 4 | closed | Closed in `23-auditor.md`. Unchanged. |
| 5 | closed | `VerifyKeywordFollowedByBlockCommentAndStrayDocumentationCommentIsNotReported` pins `if/* c *//// x` / `(c)` with `Verify` (no diagnostic). `VerifyFormatterOutputWithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported` pins the same shape through `VerifyFormatter`. Both carry `RH6001-002`. Both fail against the 802afbf analyzer (test implementer's run). The repair also adds two block comments before the `///`, and a block comment behind the `///` in front of `(`. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | low | `RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.cs`, the XML summaries of `VerifyKeywordFollowedByBlockCommentAndStrayDocumentationCommentIsNotReported`, `…BlockCommentAndSingleLineComment…`, `…TwoBlockCommentsAndStrayDocumentationComment…`, and `…TwoBlockCommentsAndSingleLineComment…` | – (wording) | – | PR-introduced | **The summaries name the wrong subject.**<br>- They read "Verifies that a block comment written directly behind the keyword, followed by a documentation comment, …, is not reported". The comment is never what RH6001 reports; the keyword is. The rows from round 1 make the keyword the subject (`<c>if</c> directly followed by … is not reported`).<br>- The two-comment summaries also say "two block comments … is not reported".<br>- The two summaries for the comment in front of `(` (`…StrayDocumentationCommentAndBlockCommentInFrontOfParenthesis…`, `…SingleLineCommentAndBlockCommentInFrontOfParenthesis…`) make the keyword the subject and hold.<br><br>This is text-only. It changes no test and no assertion. | Make the keyword the subject, as in the round-1 rows. For example: "Verifies that <c>if</c> followed by a block comment and a documentation comment, which ends the line in front of the <c>(</c>, is not reported; …". |

`pass, text cleanup`: #6 is confined to XML summaries of test methods. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. `scripts/verify-text-only.sh` can prove the edit.

## Guard scope

_N/A — the repair changes no guard, predicate, or exemption._ The guard from `23-auditor.md` (RH6001 `OnSyntaxTree`: whitespace in the keyword's trailing trivia, or `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(keyword, openParen)`) is byte-identical.

Against that guard, the new rows pin two boundaries:
- **Non-empty trailing trivia of the keyword.** Rows 1 and 2 put `/* */` there, so a guard that skipped only when the keyword's trailing trivia is empty would report them.
- **A line break that is not the last trivia before `(`.** Row 3 (`if/// x` / `/* c */(c)`) puts `/* c */` after the `///` in the leading trivia of `(`, so a guard that looked only at the trivia next to `(` would report it.

A guard that looked only at the first leading trivia of `(` is not separated from the real guard by any reachable input. A `/* */` or whitespace on the keyword's line goes into the keyword's trailing trivia, so the `///` is always the first leading trivia of `(` when it is on that line. No row is missing for it.

## Policy ownership

_N/A — no policy moved in the repair._ The owner recorded in `23-auditor.md` (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, deferral 3A for `AttributeTargetUtilities.StartsLine`) is unchanged.

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyKeywordFollowedByBlockCommentAndStrayDocumentationCommentIsNotReported` (`if/* c *//// x` / `(c)`) | RH6001-002: a line break after a `/* */` comment separates the keyword, also when a `///` carries it | Any RH6001 diagnostic | `Verify(source)`: no diagnostics, LF and CRLF through the base | adequate; fails on the 802afbf analyzer |
| `VerifyKeywordFollowedByTwoBlockCommentsAndStrayDocumentationCommentIsNotReported` | Same, for several `/* */` comments (RH6001-001 says "one or more") | Any RH6001 diagnostic | `Verify(source)` | adequate; fails on the 802afbf analyzer |
| `VerifyKeywordFollowedByStrayDocumentationCommentAndBlockCommentInFrontOfParenthesisIsNotReported` (`if/// x` / `/* c */(c)`) | RH6001-002: a line break anywhere before `(` separates the keyword, even when `(` does not start its line | Any RH6001 diagnostic | `Verify(source)` | adequate; fails on the 802afbf analyzer |
| The three `//` controls | The `//` counterpart gives the same verdict, on formatter output (my probe confirms the inputs are formatter output) | Any RH6001 diagnostic | `Verify(source)` | adequate as controls |
| `VerifyFormatterOutputWithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported` | FC-013: the formatter keeps `if/* c *//// x` / `(c)`, and RH6001 does not report it | The formatter changes it, or RH6001 reports the input, pass 1, or pass 2 | `VerifyFormatter`: LF and CRLF, two passes, analyzer after each | adequate; fails on the 802afbf analyzer |

## Failure classes dismissed

- **FC-001, FC-002, FC-015, FC-016:** no guard and no owner changed in the repair (see *Guard scope*, *Policy ownership*). The sibling shapes of #5's axis (one or several `/* */` before the `///`, a `/* */` after it) now each have a row.
- **FC-003:** both sides of the boundary are tested. The new `///` rows are not reported. `VerifyCommentAloneDoesNotSeparateKeywordAndFixInsertsSpaceBeforeComment` (`if/* note */(a > 1)`, unchanged) is still reported, so a `/* */` with no line break still counts as glued.
- **FC-004 to FC-012, FC-018 to FC-030, FC-036:** no production code, rewrite, fix, or formatter phase changed.
- **FC-013:** the formatter keeps all three `///` shapes (probe), and RH6001 does not report them (tests). The `/** */` disagreement stays a follow-up candidate (`00-lead.md`).
- **FC-014:** no new rule.
- **FC-017:** see *Assertion adequacy*.
- **FC-031:** no production method changed. The test summaries are covered by #6.
- **FC-032:** the RH6001 results were gathered on the head after the last compiled change of the run (ba5a99b's production diff). The repair compiles only tests.
- **FC-033:** `Verify` and `VerifyFormatter` run under LF and CRLF. The inputs carry the trivia of the shape itself; nothing is normalized away.
- **FC-034:** row 1 is #5's shape verbatim.
- **FC-035:** the expected verdict comes from RH6001-002 and the `//` controls. The rows fail on the pre-change analyzer.
- **FC-037:** no text-only claim was made.
- **Tab matrix:** no column is computed.

## Architecture findings

_None._

## Hints

- **`if/** d *//// x` / `(c)`:** the test implementer leaves this shape untested because "RH6001-001 already excludes a gap that holds a documentation comment".
  - That reason is about the report condition. The shape has a `///` that ends the line, so RH6001-002 decides it, and `AreSeparatedByEndOfLine` skips it whatever `/** d */` is.
  - It holds in code. It reaches the same `/** */` gap as the existing follow-up candidate (`if/** d */(c)`), so it fits there and not in this run.
