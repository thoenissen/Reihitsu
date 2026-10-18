Role: auditor
Commit: a8a399a17c8be3a9cdde6f239c43cce97e9429f3
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing had to be ignored. Nothing but this handover was edited, and GitHub was not touched.

## Scope

- Head `a8a399a17c8be3a9cdde6f239c43cce97e9429f3` is checked out with a clean tree. `git ls-remote origin refs/heads/main` gives `d83f2ae0b88a68fc626936144a7d7b1e01b861a4`, which is still an ancestor of the head.
- This is a repair delta against `96ed3201fb5c23998e8d73d0557664b7f6e5bc28` (`09-auditor.md`). The repair diff touches:
  - normative sentences, three Whys, and three `Uses` rows in RH5601, RH5602, RH5603, RH6001, RH6003, RH6004, RH6005, RH6008, RH6009, RH6010, RH6011, and RH6012;
  - one renamed test and one new test in `RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`;
  - the handovers 10–15.
- No example, ID, heading, ledger line, index row, or production file changed. The scope did not grow.
- The roles' test results in `11`–`15` are trusted; no test was re-run.
- Evidence comes from `scripts/apply-fix.sh` (LF and CRLF arms) on fixtures in the session scratchpad, and from a scratch Roslyn probe outside the repository, built against the repository's Release `Microsoft.CodeAnalysis*.dll`. In the inputs, `→` is a tab and `⏎` is a line break.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 RH5601-001 | closed | The requirement now excludes a tab that indents the second or a later `///` line, and condition 1 requires that no `/** */` stands between the last code token and the tab. Both inputs from round 1 are unclaimed and filed (#1038 Q3, Q4). The boundaries still hold. A tab-indented code line directly after a `///` block (`→void M()`) is claimed by condition 2 and is reported (`fixed`). A tab before a mid-line `///` (`int x;→/// <summary>`) is claimed by condition 1 and is reported (`fixed`). |
| 2 RH6001-002 | closed | It now claims only whitespace or a line break directly after the keyword or after a non-documentation `/* */` comment. `if /** c */(a > 0)` and `if/* a */ /** c */(a > 1)` are claimed as not reported and give `no-diagnostic`. `if/** c */ (…)` is claimed by neither requirement (#1052 Q1). |
| 3 RH6004-002 | closed | The exemption covers `#region`/`#endregion` "written without whitespace between the `#` and the keyword". That matches the analyzer's text-prefix check, and `# region` is left to #1043. |
| 4 RH6001-001, RH6005-001, RH6011-001 | closed | RH6001-001 and RH6005-001 now claim the keyword only when nothing, or only non-documentation `/* */` comments, stands before the next token, with no whitespace and no line break. Every claimed input has empty or comment-only trailing trivia, so the analyzer reports it. The spec author's check of `if/*a*//*b*/(` and `operator/*a*//*b*/-` (`14-spec-author.md`) shows that the Why holds. RH6005-002 says "directly followed". RH6011-001 claims a run after code or a non-documentation `/* */` comment, so `List /** c */ <int>` is unclaimed (#1049 Q2). `13-spec-checker.md`'s intermediate finding on this item is resolved in `708dc1b`. |
| 5 RH5603-001/-002 | closed | The comment branch reads "not on a directive line". `#pragma … // end⏎` and `#endif // end⏎` at the end of a file are unclaimed (#1040 Q3). |
| 6 RH5601-002, RH5602-001 | closed | RH5601-002 exempts "the text of an interpolated string", so a tab in a hole is no longer exempted. RH5602-001 treats every line an interpolated string spans, its holes included, as touched. That is exactly `GetStringLineIndices` (`StringLiteralExpression` and `Utf8StringLiteralExpression` spans, plus `InterpolatedStringExpressionSyntax` spans). |
| 7 RH6003/RH6008/RH6012 Why | closed | FMT-SPACE-014 is cited for a `/** */` comment the token carries, and `Uses` gains FMT-SPACE-014. Parity was verified in round 1 (`/** y */;`, `/** e */]`, `/** d */>`). |
| 8 RH6009-001, RH6010-001 | closed | The condition is now positive. The probe shows that in `new int[]/** y */{ 1 }` the `*/` is `DocumentationCommentExteriorTrivia` on a zero-width `EndOfDocumentationCommentToken`. So the character before `{` is the last character of neither a token nor a non-documentation `/* */` comment, and `/** y */{` is unclaimed. |
| 9 RH5604 tie test | closed | The test is renamed `VerifyCrLfWinsTieWhenItIsEncounteredFirst`, with a summary that matches its assertion. The new `VerifyCrLfWinsTieWhenLfIsEncounteredFirst` (`…\n` then `{\r\n`) reports the LF line and fixes it to CRLF. A "first encountered wins" rule would report and fix the other line, so the test discriminates. Its tags -001/-004/-005 are each verified by the exact span and the exact fixed text. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| — | — | — | — | — | None. | — |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5601-001, `///` continuation exclusion and `/** */` clause | Spec: indentation of `///` lines 2..n; tokens and doc comments before the tab on its line | `IsInsideCommentOrDisabledText` (top-level trivia: continuation indentation is doc trivia) + `FindToken` owner + `IsStringContentToken` | yes on both sides (continuation line not reported; first `///` line and the following code line reported) | ok |
| RH5602-001, "touched" lines | Spec: lines a string literal or an interpolated string spans | `GetStringLineIndices` | yes | ok |
| RH5603-001/-002, comment branch | Spec: `//` or `/* */` not on a directive line | Text scan of trailing `\r`/`\n` (reports directive endings too, which are now unclaimed) | yes | ok |
| RH6001-001/-002, RH6005-001/-002, separation | Spec: what stands between the keyword and the next token | `TrailingTrivia` has whitespace or EOL (a `/** */` ends trailing trivia) | yes for every claimed input; `/** */` cases unclaimed both ways | ok |
| RH6004-002, region exemption | Spec: `#region`/`#endregion` with no whitespace after `#` | `trimmed.StartsWith("#region")` / `"#endregion"` | yes | ok |
| RH6009/RH6010-001, preceding character | Spec: last character of a token or of a non-documentation `/* */` | `char.IsWhiteSpace(prev)` | spec is narrower and agrees | ok |
| RH6011-001, same-line run | Spec: after code or a non-documentation `/* */`, token before on the same line | `SameLinePrecedingWhitespaceAnalysis.GetSpan` | spec is narrower and agrees | ok |

## Policy ownership

_N/A — the repair changes no code and no policy owner. The round-1 table stands unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyCrLfWinsTieWhenItIsEncounteredFirst` (renamed) | A CRLF-first tie reports the LF line | The CRLF line reported, or none | `Verify(source)` | adequate; the summary now matches what it can show and names the discriminating test |
| `VerifyCrLfWinsTieWhenLfIsEncounteredFirst` (new) | An LF-first tie still resolves to CRLF: exact span of the LF line through its break, exact fixed text | "First encountered wins" reports `{\r\n` and writes LF | `Verify(source, fixed, …)` (analyzer, fix, re-analysis, Fix All) | adequate |

## Failure classes dismissed

- FC-001, FC-003–FC-012, FC-014–FC-016, FC-018, FC-020, FC-022–FC-030, FC-032–FC-037: the repair changes no code, rewrite, formatter phase, or guard. The reasons in `09-auditor.md` are unchanged.
- FC-002: reached. The qualifiers "second or later `///` line", "not a documentation comment", "not on a directive line", and "without whitespace after `#`" now appear in the normative sentences and match the code predicates (Guard scope).
- FC-013: reached. Every Why changed or kept by the repair holds for the inputs that stay claimed (findings 4 and 5 closed).
- FC-017, FC-031: reached by the RH5604 tie tests; closed (finding 9).
- FC-019, FC-021: reached by the comment shapes in findings 1, 2, 4, and 8; every left-out shape is filed, and every boundary input I constructed agrees with the narrowed text.

## Hints

- RH6001-001 and RH6005-001 say "with no whitespace and no line break anywhere between the keyword and" the next token. Their own examples (`if/* note */(`, `operator/* note */-`) have spaces inside the comment, so "anywhere" means outside comments. A reader could stumble on this, but the examples decide it. Not a finding.
- RH6001-002's "either directly or after a `/* */` comment" reads naturally as "directly after the keyword or directly after such a comment". The contrived `if/* a *//** c */ (` is reported and is claimed by neither requirement under that reading.
- The round-1 hints (formatter blank line in `List /** c */ <int>`, now part of #1049 Q2; the two owners of the same-line run decision) stand as follow-up candidates.

Commands: `git rev-parse HEAD`, `git ls-remote origin refs/heads/main`, `git merge-base --is-ancestor`, `git diff 96ed320 HEAD`, `scripts/apply-fix.sh RH5601|RH6001 <scratch dir>`, and the scratch Roslyn probe (`dotnet run` on a scratch project outside the repository).
