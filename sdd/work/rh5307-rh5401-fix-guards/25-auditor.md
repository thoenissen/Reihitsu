Role: auditor
Commit: 7f858b70435531f8e17d1586a39695b4accaea9b
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- **State.** The checkout is the head `7f858b7` and the working tree is clean. `git fetch origin main` reads `e3cf4cdc459371c3a532a3580e6b5fc49a7cf609`, the SHA in my prompt, and that commit is an ancestor of the head (merged in `63a4fb5`). The lane check confirms behavior-change.
- **Scope of this audit.** A repair delta against `5e2f356`: the commits after the merge `63a4fb5`, which are `26dc238`, `926712f`, `eb8475c`, `9075fc0`, and `48a2736`, plus their `sdd/work` updates. Production changed in one file, the RH5401 code fix:
  - `CountBlankLinesInAttributeRegion` is deleted.
  - The new `HoldsBlankLineOrOwnLineCommentBetweenBrackets` is applied in the registration gate.
  - The summaries are reworded.

  Tests changed in one file: two tests changed, seven added, and `S` removed. The rest is RH5401-003/-004 and the change doc. This matches handovers 19–24. Scope did not grow.
- **The merge and the evidence of findings 1–4.** I re-ran the fixtures of audit 18 on the head:
  - `…/scratchpad/audit18/fx/` and `fx2/`; the output is in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit25/fx.out` and `fx2.out`.
  - Every probe that was withheld is still withheld: `a01`–`a04`, `a07`, `a11`, `a13`–`a15`, `b02`, `b03`, `b06`, `b07`.
  - Every probe that is still offered produces byte-identical diffs: `a08`, `a12`, `a16`.
  - The statuses that changed all moved from fixed to withheld: `a05`/`Q`, `a06`, `a09`, `a10`, `a17`, `b01`. Each of these holds a blank line or an own-line comment between brackets, so the change comes from the new refusal, not from the merge.
  - No probe ends in not-converged, no-progress, invalid, or line-ending drift, under LF or CRLF.
- **New probes for the new guard.** All were run with `scripts/apply-fix.sh RH5401` under LF and CRLF:
  - `…/scratchpad/audit25/c/` (`c01`–`c20`), output in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit25/c.out`
  - `…/audit25/d/` (`d01`–`d07`), output in `d.out`
  - `…/audit25/e/` (`e01`–`e04`), output in `e.out`
  - `…/audit25/f/` (`f01`–`f03`), output in `f.out`
  - `…/audit25/de/` (`d01`, `e01`–`e04`), run on `5e2f356` in a temporary worktree (since removed), output in `de5e2.out`

## Findings

Previous findings:
- #1: closed. Every listed member is still withheld (`a01`–`a04`, `a11`, `a14`).
- #2: closed. This repair rewrote the summaries again. They describe the new code: `HoldsBlankLineOrOwnLineCommentBetweenBrackets` names its span and its exclusions, `FormatsOnOneLineKeepingBlankLines` names only the per-gap check, and the call-site comment names both conditions.
- #3: closed. The change doc's RH5401 blocks hold `L` in the offered block, `Q` (now) in the unchanged block, and `Y` (24).
- #4: closed. `a09`/`W`, `a10`, and `b01` are withheld under LF and CRLF, and `W` and `b01` each have a withheld test. A new member of the same class fails: see #5.

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs`, `HoldsBlankLineOrOwnLineCommentBetweenBrackets`, which checks blank lines and comments only. Also `sdd/specs/analyzer/RH5401.md` RH5401-004: "A directive or disabled text between the brackets does not withhold the fix by this condition", and the `Why:` claim that inside a list the formatter changes blank lines only "by dropping one … and by inserting one in front of a comment". Also the change doc's question-6 decision ("The fix's output therefore never changes a blank line among or behind the attribute lists") and its rejected alternative ("a cheaper and complete check") | FC-005, FC-020, FC-001 | #4 | PR-introduced | The refusal assumes that inside an attribute list the formatter inserts a blank line only in front of an own-line comment. The formatter also frames `#region` and `#endregion` with blank lines (FMT-BLANK-013), including when they stand between an attribute list's brackets. Directives are outside the refusal, so the fix is offered and inserts blank lines the author did not write. Maintainer answers 3, 5, and 6 rule that out ("the fix must not insert a blank line"). On `5e2f356` the whole-region total count caught every member: `de5e2.out`, all no-fix-offered. The repair removed that count and so re-exposed the class; on `main` no attributed property here got a fix. Failing members (all `fixed`, LF and CRLF): **`d01`** `[Obsolete,⏎#region R⏎ CLSCompliant(false)⏎#endregion⏎ ]⏎int E⏎    => 1;` gains four blank lines inside the list, one on each side of both directives. **`e01`**: `#region R` in front of the `[`, outside the brackets, and `#endregion` between `Obsolete,` and `CLSCompliant(false)`; it gains two blank lines inside the list. **`e02`**: both directives inside one attribute's argument list; it gains four. **`e03`**: both directives in front of the `)` of an argument list; it gains three. **`e04`**: both directives directly behind `[` and in front of `]`; it gains four. Holding members, where the output keeps the input's blank lines: `#if`/`#else`/`#endif` (`c01`, `c02`, `c11`, `d04`, `d07`), `#pragma` (`c10`, `f03`), `#nullable` (`d06`), `#warning` (`f01`), `#line` (`f02`), a blank line inside disabled text (`d03`), and `#region` between the brackets with `#endregion` behind the `]` (`d02`, withheld by the per-gap check). The shape is rare, since a region directive inside an attribute list is seldom written, and no test covers it. | Refuse by the cause, not by directive kind. For example, withhold the fix when an attribute list holds, between its brackets, a blank line, a comment that starts its own line, or a directive (every directive starts its own line). The check then no longer depends on which own-line trivia the blank-line phase frames. It withholds `c01`, `c02`, `c10`, `c11`, `d03`, `d04`, `d06`, `d07`, and `f01`–`f03` as well. All are safe today, but none of them got a fix on `main`, so this costs nothing that `main` offered. The spec author replaces the directive sentence of RH5401-004, corrects the `Why:` causal claim, and corrects the two change-doc claims. The test implementer adds withheld tests for `d01` and `e01`. The owner re-derives the check. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5401: blank line or own-line comment between the brackets (new) | For each list as written, the gap from each token, `[` through the token before `]`, to the next token: the left token's trailing trivia and the right token's leading trivia. A blank line uses `TokenGapAnalysis.Between(...).BlankLineCount > 0`. An own-line comment is a comment in the right token's leading trivia for which `StartsLineAtLeadingTriviaIndex` holds | RH5401-004: "between two tokens of the list, from its `[` through its `]`: behind the `[`, in front of the `]`, … inside the argument list", and "nothing but whitespace in front of it on its line" | Same boundaries. The `[` leading trivia and the `]` trailing trivia are outside (`J`, `c12`–`c14`, `c19` offered). A comment on a line with code is not own-line (`U`, `V`, `Y`, `c03`–`c05`, `c08`, `c15`, `c16`, `c20` offered). So is a comment behind a multi-line block comment's `*/` (`c09` offered), which matches "nothing but whitespace in front of it". A block comment that starts its line in front of code is withheld (test). The reasoning that "an own-line comment is always leading trivia" holds, because Roslyn's trailing trivia ends at the first line break | holds |
| RH5401: the new refusal vs. the formatter's blank-line insertions inside brackets (the decision the refusal protects) | Blank lines and comments only | The blank-line phase: FMT-BLANK-011 (a comment on its own line) and FMT-BLANK-013 (region directives framed by blank lines). Drops and collapses (FMT-STRUCT-028, FMT-BLANK-007/-009/-014) need an existing blank line, which the refusal catches | FMT-BLANK-011 is covered, as a superset. FMT-BLANK-013 is not covered | finding #5 |
| RH5401: per-gap check behind each list (unchanged) | Unchanged from 12/18 | RH5401-004 | Unchanged (`a07`, `a13`, `d02` withheld; `a08`, `a12`, `a16`, `c17`, `c18` offered with the input's blank lines) | holds (reused) |
| RH5401: gate order and paths | Registration: `FindUnsupportedSyntax`, then the bracket refusal on every list, then `FormatsOnOneLineKeepingBlankLines`. Fix All is `BatchFixer` and so goes through the same `RegisterCodeFixesAsync` | SHARED-FIX-010, SHARED-FIX-011 | A missing `]` cannot reach the loop, because the base class refuses a document with a syntax error before `RegisterCodeFixesCoreAsync` runs. A property without attribute lists skips the refusal (`Any` on an empty list) | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Blank-line count of a token gap | `TokenGapAnalysis.BlankLineCount` (`src/Reihitsu.Core/TokenGapAnalysis.cs`); reused | one owner |
| A comment starts its own line | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (Core). The fix reuses it, and so does the formatter's private `BlankLineTriviaBoundaryRewriter.StartsOwnLine`, which delegates to it. `RH3103…CodeFixProvider.IsOwnLineComment` is a different predicate: it also requires nothing behind the comment, and it is outside the run | one owner |
| Comment kind | `SyntaxTriviaUtilities.IsCommentTrivia` (Core; documentation comments included); reused | one owner |
| Which own-line trivia the blank-line phase frames with blank lines inside an attribute list | The blank-line phase (FMT-BLANK-011, FMT-BLANK-013). The fix's refusal and the RH5401-004 `Why:` restate part of it as "own-line comment" | second, partial statement; see *Architecture findings* and #5 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsWithheldForABlankLineInsideAnAttributeListTheFormatterDoesNotSplit` (`Q`), and the combined `VerifyFixIsWithheldForABlankLineOrACommentOnItsOwnLineBetweenTheBracketsOfAnAttributeList` (`N`, `O`, `P`, `Q`, `R`, `T`, `W`) | The diagnostic stays and no action is registered | An action is registered | `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate |
| `…ForACommentOnItsOwnLineInsideTheArgumentsOfAnAttribute` (`T`), `…BehindTheOpeningBracket…`, `…InFrontOfTheClosingBracket…`, `…ForABlockCommentThatStartsItsOwnLineInFrontOfCode…` | The own-line side of the new condition at each position: behind `[`, in front of `]`, inside the arguments, and the block comment followed by code | An action is registered. Since the total count is removed, each of these depends on the new guard alone | `VerifyNoCodeFixIsOffered` | adequate |
| `…WhenABlankLineDroppedInOneAttributeListOffsetsOneInsertedInFrontOfACommentInAnother` (`W`), `…WhenABlankLineCollapsedInOne…` (`b01`) | The offset members of #4 are withheld | An action is registered | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixKeepsTheBlankLinesAndCommentsOfAttributeListsThatTheFormatterKeeps` (`E`, `F`, `J`, `U`, `V`, `Y`), `…KeepsABlockCommentBehindCode…` (`V`), `…KeepsALineCommentBehindCodeInsideTheArguments…` (`Y`) | The offered side (FC-003): a comment on a line with code, and trivia outside the brackets. The output is exact | Any different byte, or a withheld fix | `Verify` / `VerifyCodeFixWithEndOfLine`, LF and CRLF | adequate |
| (missing) a region directive between the brackets | Withheld once #5 is fixed | A fix is offered | — | missing, part of #5 |

## Failure classes dismissed

Reused from `12-auditor.md` and `18-auditor.md`, because the repair changed only the RH5401 registration gate. The earlier reasons still hold for FC-002–FC-004, FC-006–FC-015, FC-017–FC-019, and FC-021–FC-044, apart from FC-001, FC-005, and FC-020 (finding #5) and these notes:
- FC-003: both sides of the new condition are tested (see *Assertion adequacy*).
- FC-007, FC-009: every offered probe converges in one iteration, and every withheld probe still reports the diagnostic.
- FC-016: #4's offset members are closed. The sibling shape is #5.
- FC-031: the summaries and the call-site comment describe the new code. There is no `TODO` and no tracker reference in the diff (`rg` on the added lines of `src`).
- FC-040: a `///` comment between the brackets is withheld (`d05`). `IsCommentTrivia` includes documentation comments, and a `//` comment is a comment in either documentation mode.
- FC-042: no shape is declared, and the new guard serves none.
- FC-044: the refusal is a cheap syntactic check that reads no computed result. Its stated cause, however, is a model of the blank-line phase, and that model is incomplete. See *Architecture findings*.
- Check 10: the new expected values are `J`, `V`, and `Y` (offered, the formatter's layout under maintainer answers Q1/Q2 and RH5401-003) and `U` (as on `main`, decision 13). The newly withheld members (`Q`, `T`, `W`, `b01`, and the bracket positions) are rare-class refusals. Nothing is undecided.

## Architecture findings

- **The components.** The RH5401 code fix's registration gate (`HoldsBlankLineOrOwnLineCommentBetweenBrackets`, with `KeepsBlankLinesBehindAttributeLists`) and the formatter's blank-line phase (`BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`; FMT-BLANK-011, FMT-BLANK-013).
- **The decision they share.** Whether formatting the property changes a blank line inside or around its attribute lists. The phase owns that decision. The fix's refusal names the phase's insertion causes ("a comment that starts its own line"), and RH5401-004's `Why:` states them as a complete list. That is a second, partial statement of the phase's rule.
- **The places that show it.** `d01` and `e01`–`e04`, where the phase frames a region directive between the brackets and the fix is offered.
- **The history.** This is the third member of the class in this run: #1 (a drop in a split list), #4 (count offsets), and #5 (directive framing).
- **A refusal by cause that does not depend on the phase's list.** Withhold for any trivia other than whitespace and line breaks that starts its own line between the brackets: an own-line comment or any directive. Add to that a blank line there.

## Hints

- Lone `\r` line endings are unchanged from 12 and still unverified.
- `c01`, `c02`, `d01`, `e04`: where a `]` stands alone on its line, the fix moves it one column right (`     ]`), as whole-document formatting does. This is the shape lead decision 10 dropped, and the formatter's placement (FMT-BREAK-027 leaves it unspecified). The repair did not change it.
- `d03` holds a blank line inside the disabled text between the brackets. The fix is offered and keeps the blank line, because FMT-BLANK-015 keeps it and `BlankLineCount` does not count it. RH5401-004 says a blank line between the brackets withholds the fix, and in the same requirement says disabled text does not. The reading that disabled text does not withhold matches the code. The refusal suggested in #5 (any directive) makes the question moot.

## Follow-up candidates

_None_ beyond `00-lead.md`.
