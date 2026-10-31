Role: auditor
Commit: ba5a99bfb626a88c8d66dc71dff63109adfe354d
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `12-auditor.md`, `17-auditor.md`, handovers 18 to 22, the decision `squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md`, the specs, and the diff.

## Scope

- **Head:** ba5a99b, checked out, clean tree.
- **Base:** `origin/main` = 802afbf, confirmed with `git ls-remote`. It is an ancestor of the head.
- **What I audited:** the structural change chosen under escalation 12, 21befc2..ba5a99b, against the previous audit (`17-auditor.md`, of 21befc2). It is not a repair round.
- **Production diff:** one condition in `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.OnSyntaxTree`.
- **Spec diff:** RH6001-002 and the change-doc addition.
- **Test diff:** 18 analyzer tests and 1 formatter test.
- **Diff against the handovers:** it matches 18 to 22, with nothing beyond them.
- **Lane:** `scripts/lane-check.sh behavior-change` confirmed it.
- **Results I trusted:** the cleaner's build and the implementer's test runs. I ran no test project.
- **Probes:** fixtures in the session scratchpad, each under LF and CRLF. I ran `scripts/format.sh` twice (stable: `Formatted 0 of 16` on the second pass), then `scripts/apply-fix.sh RH6001` on the output. I also ran a `git worktree` of 802afbf to get the behavior from before the change. The shapes:
  - `if/// x`, `while/// x`, `if /// x`, `if//// x`;
  - `if/* c *//// x` / `(c)`;
  - `if/** d */(c)`;
  - `catch/// x` / `(System.Exception)`;
  - `if/// x` followed by `#if DEBUG` / `(c)` / `#else` / `(!c)` / `#endif`.
- **Enumeration check:** I repeated the test implementer's search independently (see *Enumeration completeness*).

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Closed in `17-auditor.md`. This change does not touch `CollapseTokenToSameLine`. |
| 2 | closed | Closed in `17-auditor.md`. RH6005 and `AdjacentTokenSpacingUtilities` are unchanged. |
| 3 | closed | Closed in `17-auditor.md`. Unchanged. |
| 4 | closed | Every failing member that #4 listed now holds: `if`, `while`, the `while` of `do`, `for`, `foreach`, `fixed`, `lock`, `switch`, `using`, each as `keyword/// x` / `(…)`. The decision goes through `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(keyword, openParen)`. Each member has a test; the implementer's run gives 29 passed, 0 failed. My probes of `if` and `while` give `no-diagnostic` under LF and CRLF, and so does the directive member. The holding members `return`, `throw`, and `catch` are still relocated by the documentation-comment phase; I probed `catch` and got `no-diagnostic`. |

## Enumeration completeness

The change was to enumerate every analyzer, code fix, and Core helper that decides "a line break separates two tokens" by trivia kind. I checked that enumeration (`18-test-implementer.md`) along two axes.

**Sites.**
- I ran `rg -n "EndOfLineTrivia"` over `src/Reihitsu.Analyzer`, `src/Reihitsu.Analyzer.CodeFixes`, and `src/Reihitsu.Core`. It found the 28 sites the handover lists, plus five it does not list:
  - `SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex` and `FindLastSignificantTriviaIndex` (`:151`, `:173`). Callers: `OrderingMoveSafety` and RH7207. They split leading trivia into gap and payload; they decide nothing about line breaks.
  - `SyntaxTriviaUtilities.IsPrecededByDirective` and `IsFollowedByDirective` (`:285`, `:302`). They skip trivia to find a directive; they decide nothing about line breaks.
  - `LineEndingUtilities.DetectEndOfLine:31`. It counts line-ending styles; it decides nothing about line breaks.

  None of these is a member.
- `TokenGapAnalysis:185,223` is the shared fact itself.
- I ran `rg` for trivia-presence checks (`TrailingTrivia.Any(`, `.All(`, `Count == 0`, `HasLeading/TrailingTrivia`), for `WhitespaceTrivia` decision sites, and for `SingleLineCommentTrivia` kind checks. They found no further member.
- I also ran `rg` for gap checks by `EndOfLineTrivia` kind in `src/Reihitsu.Formatter`. It found three, and each holds, because a `///` never sits in trailing trivia:
  - `EndOfLineWhitespace.StripBeforeFollowingLineBreak` reads the first trivia of the following leading trivia, and a `///` there means the whitespace is not at the end of the line;
  - `AccessorExpressionBodyTransform:107` and `ExpressionBodyToBlockConverter:66` read trailing trivia only.

The site enumeration is complete.

**Shapes inside RH6001.** RH6001-001 names "one or more `/* */` comments" between the keyword and `(`. This axis is: nothing, or `/* */` comments, then `///`. The handover leaves the shape with a comment unchecked (*Not covered*). I checked it: it was a failing member of #4's class before this change. It holds now, but it has no test (finding #5).

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.cs` (missing row) | FC-003, FC-017, FC-034 | – | same requirement | **One failing member of #4's class has no test: a `/* */` comment between the keyword and the `///`.**<br><br>Input: `if/* c *//// x` / `(c)`.<br>- The formatter keeps it, on 802afbf and on the head. It is stable over two passes, and the LF and CRLF outputs are identical.<br>- With the RH6001 from 802afbf (unchanged up to 21befc2), `apply-fix.sh RH6001` reports this formatter output and fixes it to `if /* c *//// x` (1 iteration, LF and CRLF).<br>- On the head it gives `no-diagnostic`, because the `///` is in the leading trivia of `(` and `AreSeparatedByEndOfLine` sees it.<br><br>So the code closes this member, but nothing pins it. Every new test has an empty gap in front of the `///`. A guard that inspects only the first leading trivia of `(`, or only the case where the keyword's trailing trivia is empty, would pass all 19 new tests and fail this member.<br><br>The decision's option B requires a reproduction of each failing member on formatter output with a `///`. The test implementer listed this shape as not checked.<br><br>Holding members (no test needed):<br>- `if /* c */ /// x`: whitespace in the trailing trivia; skipped before and after the change;<br>- `if//// x`: a `//` comment in the trailing trivia, followed by an end-of-line trivia;<br>- `if/// x` followed by an `#if`/`#else` directive before `(`: `no-diagnostic`. | Add a `[Requirement("RH6001-002")]` test with `if/* c *//// x` / `(c)` (formatter output) and expect no diagnostic. Optionally add a `VerifyFormatter` row for the same shape. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH6001: keyword separated from `(` | Keyword trailing trivia (`WhitespaceTrivia`), and the trailing trivia of the keyword plus the leading trivia of `(` through `AreSeparatedByEndOfLine` → `EndsLine` | Formatter same-line gap (FMT-SPACE-001) through the same fact; RH6005 and RH6021 through the same fact | Yes. The new skip is exactly the old skip, plus a line-ending trivia in the leading trivia of `(`. A directive or disabled text can stand there only after a line break, so the only new exclusion is a `///` comment, with or without `/* */` in front of it. RH6001-002 as amended covers exactly that ("a line break before its `(`, either directly or after a `/* */` comment"). A multi-line comment's interior is still not a line break, so `if/* a`/`b */(c)` is reported as before. | holds |
| RH6001 fix applies | The reported keyword's span end | The analyzer | The fix has no predicate of its own; it inserts a space behind any reported keyword | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A line break separates two tokens | `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (owner), used by RH6001, RH6002, RH6005, and RH6021, and by the formatter's same-line gap. `AttributeTargetUtilities.StartsLine:355` remains, deferred by decision 3A. No other kind-based copy in Analyzer, CodeFixes, or Core (see *Enumeration completeness*) | one owner, plus the recorded deferral |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `Verify{If,While,DoWhile,For,ForEach,Lock,Switch,Using}KeywordFollowedByStrayDocumentationCommentIsNotReported` | RH6001-002: a `///` behind the keyword puts `(` on its own line, so the keyword is not reported | Any RH6001 diagnostic | `Verify(source)`: no diagnostics | Adequate for the shape with an empty gap. The shape with a `/* */` comment is missing (#5) |
| `VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported` | Same, for `fixed` | Any diagnostic | `Verify` with `AllowUnsafe` | Adequate. It is untagged, because RH6001-001 does not list `fixed` (spec checker 22, lead-owned) |
| Nine `…FollowedBySingleLineCommentIsNotReported` controls | The `//` counterpart gives the same verdict | A diagnostic | `Verify(source)` | adequate |
| `RH6001…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported` | FC-013: the formatter keeps `if/// x` / `(c)`, and RH6001 does not report it | The formatter changes it, or RH6001 reports the input, pass 1, or pass 2 | `VerifyFormatter`: LF and CRLF, two passes, analyzer after each | adequate |

## Failure classes dismissed

- **FC-001, FC-002:** see *Guard scope*. The qualifier "the `(` starts its line" is the explicit `AreSeparatedByEndOfLine` condition.
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the change only removes reports. The fix is unchanged, and it inserts one space without touching trivia. The directive probe gives `no-diagnostic`.
- **FC-006, FC-022 to FC-026, FC-029, FC-036:** there is no rewrite, rename, or identifier logic in the change.
- **FC-007, FC-008, FC-009:** the fix provider is unchanged, and no new shape is reported. The existing fix tests use `Verify` (iterative and Fix All).
- **FC-010, FC-011, FC-012, FC-028, FC-030:** no formatter or code-fix code changed. The probes were stable on the second `format.sh` pass.
- **FC-013:** RH6001 now agrees with the formatter on every `///` member probed. The `/** */` disagreement is pre-existing and not a line-break decision (*Hints*).
- **FC-014:** no new rule.
- **FC-015, FC-016:** see *Policy ownership* and *Enumeration completeness*. The only sibling shape found is #5, which holds in code.
- **FC-017:** see *Assertion adequacy*.
- **FC-027:** the `do … while` member and the directive member were probed or tested.
- **FC-031:** the new inline comment in `OnSyntaxTree` describes the new condition. No summary changed meaning.
- **FC-032:** my probes ran on the head build that the scripts produced. The pre-change comparison ran on a separate 802afbf worktree, which I have removed.
- **FC-033:**
  - The tests run under LF and CRLF through the base.
  - The probes were separate LF and CRLF fixtures, each run through both arms of `apply-fix.sh`.
- **FC-035:** the expected verdict comes from RH6001-002 and the `//` controls, not from the patched code.
- **FC-037:** no text-only claim was made.
- **Tab matrix:** no column is computed.

## Architecture findings

_None._ The class of #2 and #4 is now one owner, apart from the deferral recorded by decision 3A. Finding #5 is a missing test for a member that holds, not a member that fails.

## Hints

- **`if/** d */(c)`** (a multi-line documentation comment, directly between the keyword and `(`):
  - The formatter keeps it, on 802afbf and on the head.
  - RH6001 reports it and fixes it to `if /** d */(c)`, under LF and CRLF.
  - Roslyn lexes `/** */` into the leading trivia of `(`, and the formatter's same-line gap leaves it.
  - RH6001-001 reports only ordinary `/* */` comments, and RH6001-002 does not exclude this shape, so the spec leaves it undecided.
  - This predates the run and involves no line break. It is a follow-up candidate (FC-013, spec gap), not a finding of this run.
- **`fixed`:** the mismatch between RH6001-001 and the analyzer is already with the lead (spec checker 22).
