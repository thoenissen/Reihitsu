Role: auditor
Commit: 8943764f4f8bc580f9ad8a6dff4a653bb5dc9f2d
Verdict: #1642: pass; #1635: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from the issues' numbers, `00-lead.md`, handovers 01–10, the specs, the change doc, and the diff.

## thoenissen/Reihitsu#1642

### Scope

Head 8943764, base `origin/main` 8a26419 (merged, `git merge-base --is-ancestor` true). Full audit. The diff for this issue is the new test class `src/Reihitsu.Formatter.Test/Regression/Spacing/MultiLineCommentGapSpacingTests.cs` and nothing else (decision 2).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ FMT-SPACE-001's last sentence and FMT-SPACE-013 read as 01-skeptic quotes them, and they demand the output the tests pin. Each multi-line case has its single-line twin in the same fixture.

### Guard scope

_N/A — no production change for this issue._

### Policy ownership

_N/A — no policy changed._

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `MultiLineCommentGapSpacingTests` (5 tests: the qualified-name dot in a parameter type and in a constraint, `<` of a type argument list, a binary operand, an inserted space) | A line break inside a multi-line comment is not a line break of the gap. The rule's decision applies behind the comment exactly as it does behind the single-line twin | Any difference between the multi-line case and its twin, a change on the second pass, or a change under CRLF | `AssertRuleResult`: exact output, LF and CRLF, second pass, phase stability | adequate |

### Failure classes dismissed

Every class except FC-017, FC-033, and FC-034 needs a production change to reach this issue, and there is none.
- FC-017: the helper is exact and runs both line endings and a second pass.
- FC-033: the single-line twin in the same fixture keeps the scenario from being normalized away.
- FC-034: the issue's shapes are used verbatim, including the two spaces in front of the operand.

### Architecture findings

_None._

### Hints

_None._

## thoenissen/Reihitsu#1635

### Scope

Head 8943764, base 8a26419 (merged). Full audit. What I checked:
- The diff is what handovers 04–10 describe: one production file (`LineBreakListRewriter.cs`), the tests 04 and 06 list, the FMT-BREAK-011 and FMT-BREAK-012 changes, the non-goal, and the change doc.
- `scripts/lane-check.sh behavior-change` reports "confirmed".

Evidence comes from `scripts/format.sh` (two passes, LF and CRLF) and `scripts/apply-fix.sh` for RH5101, RH5102, RH5105–RH5109, and RH6002. The runs were on the head, and the same fixtures were also run on an `origin/main` worktree. All fixtures were in the scratchpad and are deleted.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/formatter/line-breaks.md` FMT-BREAK-012, second bullet ("A list whose elements all start on one line … is **single-line**, also when a line break stands behind its opening parenthesis or bracket. Its elements MUST stay on that line"), against `LineBreakListRewriter.HasElementSplitSignal` (`LineBreakTriviaUtilities.HasLeadingEndOfLine(element.GetFirstToken())`, applied to the first element) and the `EnsureSeparatorsHaveEndOfLine` summary ("once two elements start on different lines …") | FC-002, FC-003 | – | PR-introduced | **The changed requirement does not hold when a comment, directive, or disabled text keeps the first element off the opening line.** `CollapseFirstElementToSameLine` correctly refuses that join (FMT-BREAK-001). The first element then still starts its line, and `HasElementSplitSignal` reads that as a split signal. So a list whose elements all start on one line is split, while the new text requires it to stay on one line. Counterexample: `Foo( // c⏎    1, 2);` gives `Foo( // c⏎    1,⏎    2);`. **Failing members** (list kind × holder): argument list of an invocation and of `new C(...)`, attribute argument list, method parameter list, parenthesized lambda parameter list, and indexer parameter list, each with a `//` comment behind the opening delimiter; an invocation with a `//` comment on its own line in front of the first element; `/* c */ 1, 2` on the line below `(`; a `#pragma` line in front of the first element; and disabled text (`#if X⏎0,⏎#endif`) in front of it. Every one is split, stable on a second pass, and **identical to `main`'s output**: the old text, "wrapped after its opening parenthesis", demanded the split. **Holding member**: a multi-line comment across the gap with no end-of-line trivia (`Foo( /* a⏎ b */ 1, 2)`) stays on one line, as SHARED-BLANK-008 counts it. **The code's output is the defensible one.** RH5102 reports the shape the new text asks for (`Foo( // c⏎1, 2);` → `no-fix-offered`, LF and CRLF), and it does not report the split output (`no-diagnostic`). Following the text would leave formatter output that RH5102 reports with no fix (SHARED-DIAG-010). No test pins the refused side of the opener join (FC-003). The tests cover only the join that succeeds | Spec author: state in FMT-BREAK-012 (and in the change doc's *Elements decide* decision) that a first element that a comment, a directive, or disabled text keeps off the line of the opening delimiter starts its own line, and that this makes the list multi-line, one element per line. That is today's output and `main`'s. Test implementer: one test of that case, with a comment held behind `(` and a directive or disabled-text twin. No production change; only the `EnsureSeparatorsHaveEndOfLine` summary should name the case. The output stays as on `main`. It is a rare class (RH5035 and RH5038 report the comment), so the lead decides whether the maintainer's clarification ("single-line when all elements are on the same line") covers it. Leaving the list as written instead would keep an RH5102 report that its fix cannot clear |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A comma is held, so it is not moved (`CollapseSeparatorsToPreviousElementLine`) | Runs only when `TokenGapUtilities.HasLineBreakBetween(previous element's last token, comma)` holds. The check itself is `WouldJoinAcrossUnjoinableTrivia` over the previous token's trailing trivia and the comma's leading trivia: any comment, directive, or disabled text in that gap | `EnsureSeparatorsHaveEndOfLine`'s `HasLeadingEndOfLine(separator)` (`IsFirstOnLine` → `AreSeparatedByEndOfLine` over the same two trivia lists) | Same gap and same trivia. After the move, only held commas still start their line. Neither predicate counts a line break inside a multi-line comment (`Foo(1 /* a⏎ b */, 2,⏎3)` keeps nothing; tested). A directive and a `///` comment end their line for both (`Foo(1 /// stray⏎, 2, 3)` → held, `2,⏎3`; observed) | agree |
| The list is multi-line (`EnsureSeparatorsHaveEndOfLine`) | `IsMultiLine(node)` over the whole list including its delimiters, or `HasElementSplitSignal` (the first token of each element, element spans), and then a line break at some separator or element start | FMT-BREAK-012's definition (elements start on different lines, or an element spans lines) | Disagree on the first element held off the opening line (finding 1). Agree on a line break only in front of `)` (`Foo(1, 2⏎)` stays, `R` in my fixtures) and only behind `(` (joined) | finding 1 |
| Held comma alone on its line keeps the break behind it | `HasLeadingEndOfLine(separator)` checked before `HasTrailingEndOfLine(separator)` | FMT-BREAK-012 `P` clause | `int a // keep⏎,⏎int b` unchanged (tests `CommaAloneOnItsLineStaysAligned`, `CommaAfterDirectiveHeldFirstParameterStaysAligned`) | agree |
| Indexer opener signal (`isWrappedAfterOpener`) | deleted | — | No copy remains (`rg isWrappedAfterOpener src` empty) | agree |
| Formatter output vs RH5102-001 (SHARED-DIAG-010) | — | RH5102 analyzer | RH5101, RH5102, RH5107, RH5109, and RH6002 report nothing on the formatter's output of all my fixtures (held `//` and block comments, a raw-string element, a lambda element, `new(...)`, named and `out`/`ref`/`in` arguments, an attribute on a parameter, an interpolation hole, a switch arm), LF and CRLF | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A comma in a multi-line argument, attribute argument, or parameter list ends the previous element's line | `LineBreakListRewriter.CollapseSeparatorsToPreviousElementLine`, called by all four visitors (`rg -n "CollapseSeparatorsToPrevious" src`). `GenericConstraintLineBreakRewriter` handles constraint clauses: a different construct with its own requirement (FMT-BREAK-037) and the same guard. The RH5107 code fix builds its own parameter-list layout by design (pre-existing; its disagreement on a comment behind a leading comma is already a follow-up in `00-lead.md`) | one formatter owner; ok |
| A comma held by a comment or directive | `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` (Core), reached through `LineBreakTriviaUtilities`. It is shared with the analyzers' join guards | single owner |
| Whether a list is multi-line | `EnsureSeparatorsHaveEndOfLine` + `HasElementSplitSignal` (formatter). The analyzers keep their own report conditions under their specs (RH5101-001, RH5102-001, RH5109-001; P11) | one formatter owner; its definition diverges from the changed text (finding 1). The RH5109 W1 disagreement is pre-existing and already a follow-up |
| Leading-comma alignment of held commas | `ListElementAligner` (indentation), unchanged | ok |
| `ListElementInteriorJoinRewriter.IsOwnedGap` | Leaves every gap at a comma to the list rewriters | no second owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ListWrapByElementsTests` (31 tests: the issue's input verbatim, entries 1–9, the per-owner arguments, held and moved comments, SHARED-BLANK-008 counting, a directive with and without `X`) | Exact FMT-BREAK-012 output, a fixed point, every phase stable | Any byte of difference, a second-pass change, or a CRLF drift | `AssertRuleResult` with the preprocessor symbol where the build matters | adequate |
| `IndexerParameterListLayoutTests.IndexerParameterListWrappedOnlyBehindOpeningBracketIsJoined`, `LineBreakRequirementTests.WrappedListsBecomeOneElementPerLine` | W1 join (entry 6; first example verbatim) | A split behind `[` / `(` | `AssertRuleResult` | adequate |
| `ParameterContinuationLineAlignmentTests` (3 retagged or updated) | Held comma stays, alone if written alone; alignment under the first parameter | The comma joined or moved, or a column change | `AssertRuleResult` | adequate |
| RH5102 `FixMovesLeadingComma…` (3) and RH5101 `FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` | The delegated fix converges to the formatter's layout (entries 10, 11); Fix All on two lists | The fix result equals its input (`main`), a second iteration changes it, or Fix All differs | `VerifyCodeFixWithEndOfLine` (LF and CRLF, iterative, Fix All) / `Verify` with two diagnostics | adequate |
| RH5102 `VerifyCommentHeldLeadingCommaWithOneArgumentPerLineIsNotReported` | The formatter's held-comma output is not reported (SHARED-DIAG-010) | Any diagnostic | `Verify(source)` | adequate |
| _missing_ | The first element held off the opening line (finding 1) | — | — | finding 1 |

Visible output decided (check 10). Every changed expected value is covered:
- Entries 1–5 are the maintainer's answers 1C and 2, and the clarification.
- Entry 6 (the indexer W1 join) is covered by the clarification (decision 8).
- The held parts of entries 7 and 8, and the block-comment-held test, are a rare-class refusal (leaving a construct as written). Their other parts follow answer C.
- Entry 9 (a comment behind a moved comma) is demanded by unchanged FMT-BREAK-001. Its second sentence keeps a comment that trailed the token in front of an inserted break, here the comma, at the end of that line.
- Entries 10 and 11 follow through RH5101-003 and RH5102-003.
- #1642's values are demanded by unchanged FMT-SPACE-001 and FMT-SPACE-013.

No rule page changed.

### Failure classes dismissed

- FC-001: covered by finding 1. The other guards are in the table above.
- FC-004, FC-005, FC-019, FC-020, FC-021: traced, and constructed under both line endings, for these holders. None loses or relocates trivia beyond what FMT-BREAK-001 and FMT-BREAK-012 state:
  - a `//` comment, a block comment, and a `///` comment in front of a comma;
  - a comment in the comma's leading trivia;
  - a `#pragma` directive and an `#if` directive, including an inactive branch;
  - a comment behind a moved comma;
  - a blank line in front of a leading comma, which goes with the join, as FMT-BREAK-030 allows.
- FC-006: moving a comma between the same two elements changes no meaning.
- FC-007, FC-008, FC-009: `apply-fix` reports `fixed` for RH5102 on leading-comma inputs (LF and CRLF). It reports `no-fix-offered` only where a comment is in the list (RH5102-004), and those lists are ones the formatter changes. Fix All is covered by a test.
- FC-010, FC-012: a second pass leaves every fixture unchanged, and the tests run the phase stability check.
- FC-011: the loop runs to `SeparatorCount`, which a replacement does not change.
- FC-013: see the guard-scope table. The RH5109 W1 disagreement is pre-existing and already a follow-up. The indexer W1 input is not reported by RH5109.
- FC-014: no new rule.
- FC-015, FC-016: one owner. Tuples, collection expressions, and bracketed and angle-bracket lists are outside FMT-BREAK-012 by the spec.
- FC-017: see the assertion table.
- FC-018, FC-023, FC-024, FC-026, FC-029, FC-036: no rename, identifier, partial, token re-resolution, or key/value layout is involved.
- FC-022: a raw-string element moves its comma behind the closing quotes, and the result is stable.
- FC-025: expression-bodied members whose bodies hold such lists format and stay stable (`Q`, `R`, `S` in my fixtures).
- FC-027: checked on a nested list, a lambda block element, a switch-expression arm, an interpolation hole, an attribute on a parameter, `new(...)`, and named and `out`/`ref`/`in` arguments.
- FC-028: the fixes reach these lists through the document entry point. No caller formats them detached.
- FC-030: the fix tests show only the list changing.
- FC-031: the changed summaries describe the new code, except for the case in finding 1.
- FC-032: the implementer ran the full analyzer project after the compiled change, and the validator runs it again.
- FC-033: both line endings are used, and the `X` symbol is passed where the build decides.
- FC-034: the issue's input is tested verbatim.
- FC-035: per handover 04, the expected values come from the spec and the change doc.
- FC-037: no text-only claim is made.
- FC-038 to FC-043: no shape is declared, and the precheck is untouched. A comma move inside an argument list carries no comment into the span of FMT-UNSUP-004 or FMT-UNSUP-008.

Cost of special cases (check 11):
- The one new condition (`HasLeadingEndOfLine(separator)`) serves only the held-comma rare class. It is the cheap refusal itself: leave the comma as written. It is not machinery.
- The run deletes the indexer-only opener signal.

### Architecture findings

_None._ Finding 1 is a disagreement between the changed text and one predicate of the single owner, not two components owning the same decision.

### Hints

- Pre-existing, unrelated, and identical on `main`: a trailing comma that starts its own line in an object initializer, `new D(1, 2) { X = 3⏎, };`, is formatted to `X = 3⏎        ,⏎                };`. The comma is left alone on its line at the statement's column, not the initializer's (FMT-BREAK-021 and FMT-INDENT). This is a candidate follow-up for the lead.

## Whole run

- State:
  - the checkout is at 8943764;
  - `git ls-remote origin refs/heads/main` gives 8a26419, which is merged;
  - `scripts/lane-check.sh behavior-change` reports "confirmed".
- No test re-run was needed. The cleaner's build and the roles' focused and full runs stand. The finding turns on the CLI's output and `apply-fix`, not on a test.
- Commands I ran:
  - `scripts/format.sh` on the head and on an `origin/main` worktree, over 6 scratch fixtures (two passes, LF and CRLF);
  - `scripts/apply-fix.sh` for RH5101, RH5102, RH5105–RH5109, and RH6002.
- The worktree and all fixtures were removed afterwards.
