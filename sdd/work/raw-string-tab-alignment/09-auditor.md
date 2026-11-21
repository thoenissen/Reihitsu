Role: auditor
Commit: 1a0127edc904eeb29b1dddc96838f1b27d0ebea4
Verdict: pass, text cleanup

## Scope

Full audit. Head `1a0127edc904eeb29b1dddc96838f1b27d0ebea4` (checkout matches, clean tree). Base `origin/main` = `cfa445f48989c7c75731845255f90c9a0f89cff6` (`git ls-remote` agrees); it is an ancestor of head, merged as `6655052`. The diff is `cfa445f...1a0127e`: 28 paths. They are three production files (`RawStringLiteralUtilities.cs`, `RawStringLineShifts.cs`, the RH5202 code fix), eight test files, the specs, the change doc, `AGENTS.md`, and `sdd/work/`. This matches `04`–`08`. `scripts/lane-check.sh behavior-change` reports `confirmed`.

Issue: thoenissen/Reihitsu#1432 (`agent-filed`). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (maintainer answers 1A–4A, decisions 1–6), handovers `01`–`08`, the specs, the change doc, and the code.

Fixtures were run outside the repository, in the scratchpad.

- **`scripts/format.sh`, two passes, LF and CRLF.** The fixtures were:
  - closing whitespace `→·→` with content `→·→x`, `→·→→y`, and a whitespace-only `→` line;
  - an interpolated tab literal with a wrapped argument in a hole, a multi-line hole holding a comment, and own text after `}`;
  - a nested tab literal inside a block lambda in a hole of an outer literal that shifts right;
  - a tab literal as an attribute argument and in a switch-expression arm.

  Every output matched FMT-RAW-010: `y` kept its tab behind the width, the `→` line became 14 spaces (answer 4A), and the wrapped argument kept its column. The second pass changed 0 of 8 files, and LF equalled CRLF.
- **`scripts/apply-fix.sh RH5202`.** The first fixture above gave the same bytes as the formatter. A tab-indented outer literal with a nested literal whose closing whitespace holds the outer's tabs gave `no-fix-offered` (RH5202-008), and so did a nested tab literal inside a lambda in a hole.
- **A throwaway Fix All probe**, run in two detached scratch worktrees that were removed afterwards. The input was an outer literal and a nested literal, both reported, both shifting right, with no tab. It is described under FC-008 below.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:345` (`ApplyLeadingTriviaEdit`) and `:400` (`MoveLineInsideTrivia`) | FC-031 | – | PR-introduced | The edit tuple of both methods gained a fourth element, `SpacesWidth`. Their `<param name="edit">` text still lists only three parts: "the position the line starts at, the number of characters of indentation it has, and the column it has after the move". `MoveLineInsideTrivia` passes the whole edit to `ReplaceIndentation`, which applies `SpacesWidth`, and that method's doc (`:428`) names the fourth part. Documentation only: the text cannot reach compiled behavior, diagnostics, or rule metadata (`<param>` text is not emitted into anything a test or user reads), so it qualifies for `pass, text cleanup`. | Name the fourth part, as `:428` does. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Trigger: is the stripped indentation written as spaces (FMT-RAW-010, RH5202-007) | The leading whitespace (`char.IsWhiteSpace`) of the literal's closing line: `RawStringLiteralUtilities.HasTabInLeadingWhitespace`, called through `GetStrippedIndentationSpacesWidth` | The formatter's `RawStringLineShifts.GetSpacesWidth` (closing line of the indented tree) and the fix's `ApplyCodeFixAsync` (closing line of the document). Both use the same helper on the same raw text, because FMT-TAB-003 keeps literal tabs until the indentation phase | Same span, same helper | holds |
| Width written as spaces | The closing whitespace length after the move/shift, clamped at 0 | Formatter: `getColumnAfterMove(closingLine, closingWsLen)`. Fix: `closingWsLen + delta` (the analyzer's measure, `GetClosingLineShift`). Before this run, FMT-RAW-001 and RH5202-001 already made the two equal | Same measure | holds |
| Which lines get the width | Formatter: lines whose start token is a raw text, interpolated text, or end token of a multi-line raw literal, with `token.SpanStart <= lineStart`. Each such line gets the width of that literal (innermost owner) | Fix: every moved line not inside one of the literal's own `Interpolation` spans (`StartsInOwnHole`). Nested raw lines get 0 there, and RH5202-008 withholds the fix whenever a nested literal's closing whitespace holds a tab | On every input where the fix is offered, both give the outer width on the outer's text lines and 0 on hole lines. The literal's span after its opening line is text tokens, interpolations, and the end token, so the two partitions are complements. Nested raw lines are 0 in both once RH5202-008 holds | holds |
| RH5202-008 withholding (`HasTabIndentedNestedRawString`) | Every `DescendantNodes()` multi-line raw literal of the interpolated literal, at any depth (inside lambdas and non-raw strings too); the leading whitespace of its closing line | The formatter writes a nested literal's stripped indentation exactly when that same span holds a tab. The nested closing line is non-empty and always a `MovesLine` line, so `CollectEdits` always edits it | Same span. The guard refuses exactly the inputs where the shift-only result would differ from the formatter's, and no safe input is refused | holds |
| `CollectEdits` skip condition | `newSpaces == indentation && spacesWidth == 0` | Fix: `shiftedIndentation != indentation` | The formatter may issue a no-op edit where the fix issues none (an aligned literal whose within-width whitespace is already spaces, or a nested opening at column 0). `ReplaceIndentation` then rewrites identical text, and the value and annotations are kept | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Shift and space-rewrite of one raw-string line | `RawStringLiteralUtilities.GetShiftedIndentation`. Consumers: `RawStringLineShifts.ReplaceIndentation` and `RH5202…CodeFixProvider.ApplyCodeFixAsync`. `GetLineShiftEdit` is removed, and `rg GetLineShiftEdit` finds no reference outside `sdd/work/` | one owner (decision 2 met) |
| Trigger "tab in closing whitespace" | `RawStringLiteralUtilities.HasTabInLeadingWhitespace`, used by `GetStrippedIndentationSpacesWidth` and by the fix's `HasTabIndentedNestedRawString`. `rg "IndexOf('\t'|Contains('\t')|== '\t'"` outside tests turns up `SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine` (RH3004-006/RH7004-007, a different decision: code lines a fix re-indents), the tab-replacement phase, and same-line spacing checks. None of them decides raw-string text | one owner |
| Width of the stripped indentation | `RawStringLiteralUtilities.GetStrippedIndentationSpacesWidth` (both callers) | one owner |
| Which lines move | `RawStringLineUtilities.MovesLine` (formatter and fix), unchanged | one owner |
| Line belongs to the literal's own text | `RawStringLineShifts.GetSpacesWidth` (token-based, innermost literal) and the fix's `StartsInOwnHole` (span-based, existing before this run). These serve different scopes: the fix decides only "own text vs. own hole" and is withheld for nested tab literals by RH5202-008 (spec, decision 1). Parity is shown under *Guard scope* row 3 | two statements, parity proven, justified by RH5202-008. Not a finding |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RawStringTabIndentationTests` (26 tests) | Exact FMT-RAW-010 output for each condition side and kind | Any differing character, a second pass that changes the output, or a single phase that changes it, under LF and under CRLF | `AssertRuleResult` (exact output, LF and CRLF, second pass, `PhaseStabilityCheck`). Fixtures use placeholders that `Expand` maps to `\t`, NBSP, VT, and FF | adequate |
| `RawStringClosingWhitespaceMeasureTests` (2 changed) | FMT-RAW-001 four-tab example and the aligned-literal example | Same as above | `AssertRuleResult` | adequate |
| `ReihitsuFormatterNodeColumnTests` (2 new) | Node-level entry points write spaces (FC-028) | Tabs kept, or a column moved | `AssertFormatsTarget` (LF, CRLF, second pass); `FormatNode` exact text under LF and CRLF | adequate. `FormatNode` has no second pass, but idempotency is pinned by the whole-document tests on the same text |
| `RH5202…TabIndentationTests` (11) | RH5202-007 fix output; analyzer unchanged; fix equals formatter | A differing fixed text under LF or CRLF; a diagnostic appearing on an aligned literal; a difference from `Format(plainSource)` | `VerifyCodeFixWithEndOfLine` ×2, `Verify`, `VerifyCodeFixMatchesFormatter`, `BatchCodeFixTestsBase` Fix All scenario | adequate |
| `RH5202…HoleTests` (8 new, 1 iteration change) | RH5202-008 withholds at every depth, kind, and direction; the other side is offered; the two-step sequence converges | An action registered for the outer diagnostic; a nested result with tabs; a Fix All iteration count other than exactly 2 | `VerifyNoCodeFixIsOffered` (asserts that the diagnostic is reported and that no action is registered for any diagnostic), `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered`, `TryApplyCodeFixAtAsync`, `Verify` with an exact `NumberOfFixAllIterations = 2` | adequate. The left-shift test is not also caught by `ChangesValueOfNestedRawString`: nested closing width 13 ≥ shift 5, so only the tab guard withholds |
| `RH5202…FormatterTests`, `RH5601…FormatterTests` (3 changed) | Formatter output with diagnostics before and after | A tab left in the stripped width; a diagnostic on the output | `VerifyFormatter` | adequate |
| `RH3202…AnalyzerTests.VerifyFixWritesTheStrippedTabsOfARawStringAsSpaces` | Change 8: a formatting fix picks up FMT-RAW-010 | A tab left behind | `Verify` LF and CRLF (incremental and Fix All) | adequate |

**Visible output is decided (check 10).** Every added or changed expected value is one of three things:

- the formatter, RH5202 fix, or RH3202 fix output of the FMT-RAW-010 / RH5202-007 rule, which answers 1A, 2A, 3A, and 4A cover (recorded in the change doc's `Decisions` and in `00-lead.md`);
- a rare-class withholding (RH5202-008, decision 1);
- an output demanded by an unchanged requirement (FMT-RAW-009, `ContinuationLine…KeepsItsTab`; FMT-RAW-002, `ClosingWhitespaceWithoutATab…`).

No rule page changed. Nothing is undecided.

## Failure classes dismissed

- FC-001, FC-002, FC-003: traced under *Guard scope*. Both sides of each condition are tested: tab / no tab, within / behind the width, whitespace-only shorter / longer, the three left-shift boundaries, own text / nested, and withheld / offered.
- FC-004, FC-005, FC-019, FC-020, FC-021: nothing joins, collapses, or reorders. Only the characters within the stripped width of raw-literal text change, one for one. Hole trivia, comment continuations, and disabled text are left alone (`MovesLine`, width 0 in trivia), as `ContinuationLineOfACommentInAHoleKeepsItsTab` confirms.
- FC-006: the value is kept. The compiler requires every content line to start with the closing whitespace, or to be a whitespace-only prefix of it; the skeptic compiled this (`02`). One space per character within that width touches only stripped characters. The fix and formatter fixtures parsed after the rewrite.
- FC-007: the fix silences its own diagnostic in one application, because the analyzer's measure is unchanged and the width is kept. No other RH rule reports the spaces: RH5602 skips literal text (`IsInsideLiteralText`), and RH5601-002 does not look at literal text.
- FC-008: the outer + nested Fix All with a tab needs 2 iterations, pinned exactly (decision 6). For the no-tab case where both literals are reported and the shift is right, the HEAD fix now emits replace edits over the indentation span, where `main` emitted zero-length inserts. The probe (outer `$"""` and nested `"""`, both reported, `NumberOfFixAllIterations = 1`) failed with "Expected '1' iterations but found '2'" on both `cfa445f` and `1a0127e`. The batch merge already conflicted on `main`, the result converges in two passes on both, and so the change introduces no regression.
- FC-009: the fix is registered only for a reported literal with `delta != 0`, which always changes text. Where it would not match the formatter, it is withheld.
- FC-010, FC-012: the CRLF fixtures and tests are equal to LF. `PhaseStabilityCheck` passes. Cleanup does not touch literal text, so the 14-space whitespace-only line stays.
- FC-011: no loop is involved.
- FC-013: the analyzer is unchanged and finds the output aligned. The analyzer's silence on an aligned literal whose stripped indentation holds tabs is a P11 gap, already a follow-up candidate in `00-lead.md`.
- FC-014: not a new rule.
- FC-015, FC-016: see *Policy ownership*. The sibling raw-string consumer `RH5601…CodeFixProvider` deliberately leaves literal text alone (RH5601-002, unchanged). No other code fix rewrites raw-literal text (`rg RawString src/Reihitsu.Analyzer.CodeFixes`).
- FC-017: see *Assertion adequacy*.
- FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no renames, identifiers, members, or paired layout are involved. Edits are positional per line, computed on one snapshot.
- FC-022, FC-027: plain, `u8`, `$`, and `$$` literals are tested. Nesting is tested at one and two levels, inside a block lambda in a hole (my fixture), and in a hole of a regular `$"…"`. Enclosing scopes checked: statement, equals-value, expression body (RH3202), attribute argument, and switch arm (my fixture, stable).
- FC-028: both node-level entry points are tested. The matrix cell "tab in front of a node-level target" keeps its existing meaning (counted as one column, FMT-PIPE-017), and this run changes no column.
- Matrix *Tab width across formatter phases*: entry points (document, node-level), shift (right, none, left by fewer / exactly / more than the spaces in front of the tabs), tab in front of the line start or the opening quotes of a nested raw string, and anchoring (a wrapped argument, a block lambda, the nested literal's lines). All are covered by tests or fixtures, and every cell holds exact output, not only stability.
- FC-030: the RH5202 fix rewrites only its literal's lines.
- FC-031: finding #1. The other changed summaries (`RawStringLineShifts` class, `MoveLines`, `CollectEdits`, `ReplaceIndentation`, `GetSpacesWidth`, the fix's `ApplyCodeFixAsync` and `HasTabIndentedNestedRawString`, and the three Core helpers) describe the new code.
- FC-032: the implementer's suite runs came before decision 6, the comment-only cleaner change, and the build-config-only merge. The cleaner and the lead rebuilt Release afterwards, and the validator runs the default set next.
- FC-033, FC-034, FC-035: the issue's input and the issue comment's own shape are tested (`RightShift…`, `HoleLineAlignedToATokenOnATabIndentedContentLine…`, `BlockLambdaInAHole…`). The test implementer verified that each test was red before the implementation.
- FC-037: no text-only proof was claimed except the cleaner's comment change, proven by `verify-text-only.sh`.
- FC-038 to FC-043: no unsupported shape is declared or touched. The precheck is not involved.
- FC-044: RH5202-008 is a syntactic check on closing-line whitespace and models no computed result.

**Cost of special cases (check 11).** `GetSpacesWidth` serves common tab-indented code, not a rare class. RH5202-008 is itself the refusal for the rare class, as a cheap check. No guard serves a declared shape.

**Scope (check 9).** Every path is within the run (lane check confirmed). No `TODO` or tracker number appears in the added source.

## Architecture findings
_None._

## Hints

- RH5202-008 is decided as a rare class (decision 1). Note its practical reach: in tab-indented code, every multi-line raw string nested in a hole of an interpolated raw string normally carries the outer's tabs in front of its closing quotes, so the outer fix is withheld for that whole nesting class whenever the user indents with tabs, not only for an unusual mix. Observed with `apply-fix.sh` on an outer `→→`-indented literal whose nested closing line is `→→` plus eight spaces: `no-fix-offered`. Rarity still rests on nested multi-line raw strings in holes being rare. This is not a finding.
- Fix All over an outer and a nested literal that are both reported takes two passes on `main` and on head alike (probe under FC-008). This is pre-existing and converges; recorded so a later reader does not attribute it to this run.

## Follow-up candidates
_None_ new. The P11 candidate in `00-lead.md` stands. RH5202 does not report an aligned literal whose stripped indentation holds tabs. The structural cause is that the analyzer's measure judges only the closing column, while the formatter now also judges the spelling of the stripped indentation.
