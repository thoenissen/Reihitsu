Role: auditor
Commit: 4645c9447a7ab8f4ab8a62155fab337bd85f06b0
Verdict: pass

My prompt named the audit's scope and carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audits repair round 2 (`00-lead.md` row 26): `git diff dfd93f89 4645c944`, against findings #7 and #8 and the hint of `28-auditor.md`, with handovers 29–32. It is a delta audit, not a full one.

- Head `4645c944` is checked out, and the tree is clean.
- Base `origin/main` is `df4f828d`, as my prompt states (`git ls-remote`). The branch contains it (`git merge-base --is-ancestor`).
- The diff holds what 29–32 describe, and nothing else:
  - two `ReihitsuFormatterFieldSplitTests` (g02, and g02 with two tabs) and two `RH7101DoNotCombineFieldsAnalyzerTests` (the same two inputs through the fix);
  - in `ReihitsuFormatter.cs`, `StartsLineInLayout` calls `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`, and the remarks of `FormatSplitFieldInContext` are reflowed (the cleaner's text-only proof passed);
  - row 26 in `00-lead.md` and handovers 29–32.
- No spec, change doc, or rule page changed. The scope did not grow.
- I relied on the cleaner's Release build and the roles' focused results, except for the mutation below, on which the closure of #7 turns.

How I checked:

- I computed the columns of every new expected value from the test source. g02: `private` in column 13, `1` and the wrapped `+` in column 30. Tab variant: `private` in column 9 (each tab one column, answer 3b), `1` and `+` in column 26. The expected values follow FMT-PIPE-017 and FMT-PIPE-029, not the code.
- I repeated 28's mutation on the head in a scratch worktree: the condition of the second `FormatContext` call made always false. `ReihitsuFormatterFieldSplitTests`: 2 failed, 45 passed; the 2 are exactly the new tests, with `+ 2` in column 25 instead of 30 (and 26). `RH7101DoNotCombineFieldsAnalyzerTests`: 2 failed, 80 passed; the 2 are exactly the new tests. This matches 29's watched failures.
- I ran `scripts/apply-fix.sh RH7101 --max-iterations 3` on the head over five further fixtures that take the second layout, under LF and CRLF. All 10 arms ended `fixed` with one action and one iteration, and LF and CRLF gave the same diff:
  - h1: g02 plus a member behind the target on its last line. The second layout's context still moves `_q` onto its own line, so the read of the behind gap after the second layout holds.
  - h2: g02 plus a spanning `/* */` in the behind gap. `_q` stays behind `*/`, as in the formatter.
  - h3: g02 in a nested struct, with a wrapped argument list. The wrapped `2` stands under `1` in column 32, laid out from `private`'s document column 13.
  - h4: g02 with a lambda block initializer. The block stands 23 columns behind `private`, as `scripts/format.sh` places it behind a `private` in column 8.
  - h5: a space, a tab, and a space in front of `private` (column 10). `+ 2` stands in column 27.
- The worktree and the fixtures were in the scratchpad and are removed.

Status of earlier findings:

| # | Status | Evidence |
|---|---|---|
| 1–5 | closed (unchanged) | Not in this diff; the evidence of 20 and 28 stands |
| 6 | closed (unchanged) | No production change in this diff touches the shared-line read; `IsDocumentationCommentTrivia` tests the same two kinds as the inline copy it replaces |
| 7 | closed | Four tests now cover the second layout: g02 and the tab variant, at the entry point and through the fix, with exact text under LF and CRLF. The mutation that drops the second layout fails exactly these four. The tab variant also tells the specified column (26) from a tab at the preparation phase's width (32) and from no second layout (25) |
| 8 | closed | `StartsLineInLayout` (`ReihitsuFormatter.cs:631`) calls `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`. `rg` finds no inline `IsKind(SyntaxKind.(SingleLine\|MultiLine)DocumentationCommentTrivia)` left in `ReihitsuFormatter.cs` |
| Hint of 28 (remarks width) | closed | `ReihitsuFormatter.cs:449-454` reflowed to the surrounding width; text-only proof PASS (31) |
| Hint of 14/20/28 (`FormatNodeInDocumentAsync` `keepsLeadingTrivia`) | unchanged | Not in this diff, still not verified for an input a caller reaches |

**Repeats.** This audit has no new finding, so no finding repeats the class of a finding of 14, 20, or 28.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | none | – |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A trivia in the formatted member's leading trivia is a documentation comment (`StartsLineInLayout`) | One leading trivia of the member's first token in the formatted context | `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`, now called rather than restated; the documentation-comment phase moves both `///` and `/** */` | Same kinds, same span as at 02126b5f | holds |

The diff changes no other predicate, guard, or exemption. The precondition, the read of the layout, and the choice of the final layout are byte-identical to 02126b5f, and 28's rows for them stand.

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A trivia is a documentation comment | `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (Core), now used by `StartsLineInLayout`. No copy left in `ReihitsuFormatter.cs`. Pre-existing inline copies in `RH8201InheritdocShouldBeUsedCodeFixProvider.cs:87` (one kind only, a different decision) and `RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider.cs:93-94` are outside the run and unchanged from `main` | one owner in the run's code; see *Hints* |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterFieldSplitTests.WrappedLineOfAFirstFieldBehindACommentThatSpansLinesIsLaidOutFromTheTargetsColumn` (FMT-PIPE-029, -017) | A first field that keeps the target's line behind a spanning `/* */` keeps the document column 13 of `private`, and its wrapped line is laid out from it (`+` in column 30); `_y` at the member column, no blank line | `+ 2` in column 25 (no second layout), the gap behind `*/` changed, or a blank line | `AssertSplitsTarget`: whole text, LF and CRLF, second pass | adequate; fails under the mutation |
| `…AndTabsCountsEachTabAsOneColumn` (FMT-PIPE-029, -017, answer 3b) | Same, with two tabs: `private` in column 9, `+` in column 26 | Column 25 (no second layout) or 32 (tabs counted as replaced) | same | adequate; fails under the mutation and separates all three readings |
| `RH7101…FixLaysOutTheWrappedLineOfAFirstFieldBehindACommentThatSpansLinesFromTheDeclarationsColumn` (RH7101-004) | Exact fix output for g02, one action | Any other text or a second action | `Verify` + `AssertSingleFixUnderBothLineEndings` | adequate; fails under the mutation. The formatter-equality helper does not apply: the fix keeps the six spaces behind `*/` (FMT-PIPE-018), which document-level formatting collapses |
| `RH7101…FixCountsEachTabInFrontOfAFirstFieldBehindACommentThatSpansLinesAsOneColumn` (RH7101-004) | Same for the tab variant, via `ExpandWhitespaceMarkers` | Same | same | adequate; fails under the mutation |

## Failure classes dismissed

- **FC-001, FC-002, FC-003**: The only changed predicate is #8's, with identical kinds. The second-layout branch is now tested on its observable side; on the other side (one space or one tab behind `*/`) both layouts give the same text, so no test can observe it (29, 28).
- **FC-004, FC-005, FC-019, FC-020, FC-021**: No trivia write-back changed. In h1–h5 every comment stayed on its token, and the spacing in front of `private` was kept as in the document.
- **FC-006, FC-022–FC-027, FC-029, FC-036**: No rewrite semantics added.
- **FC-007, FC-008, FC-009, FC-010, FC-011, FC-012**: h1–h5 converged in one iteration. The new entry-point tests assert a stable second pass.
- **FC-013, FC-016**: The class of #6 is unaffected (no change to the read of the layout). The class of #7 is the branch that picks the final layout; its members, the front gap alone (g02, h3, h4, h5) and the front gap with a behind gap (h1, h2), hold.
- **FC-014, FC-018, FC-028, FC-030**: Not reached.
- **FC-015**: #8 closed; no new copy.
- **FC-017**: #7 closed; the four new tests are as strong as the guarantee.
- **FC-031**: The reflowed remarks keep their wording. `StartsLineInLayout`'s summary still describes the code. The new test summaries describe what they assert. No `TODO`, no commented-out code, no tracker number.
- **FC-032**: The mutation runs used tools built from the mutated worktree; `apply-fix.sh` and `format.sh` used tools built from the head.
- **FC-033, FC-034, FC-035**: LF and CRLF covered; expected values from FMT-PIPE-017/029 and answer 3b, cross-checked against `scripts/format.sh` by 29 and by my column count; the inputs are the auditor's own g02 shape.
- **FC-037**: The cleaner's change is text-only (proof PASS); #8's change is not text-only and is covered by the existing tests of the shared-line read.
- **Matrix *Tab width across formatter phases***: The tab variant pins the column decision on this path (answer 3b); h5 adds mixed spaces and a tab.

**Visible output (check 10).** The four new expected values are decided by FMT-PIPE-017, FMT-PIPE-029, and answers 3a/3b, which the run does not change (row 26). No rule page changed. Nothing is undecided.

## Architecture findings

_None._

## Hints

- `RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider.cs:93-94` restates `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` inline. It is pre-existing, unchanged from `main`, and outside the run; the two agree today.
- The hint of 14, 20, and 28 about `FormatNodeInDocumentAsync`'s `keepsLeadingTrivia` (`ReihitsuFormatter.cs:186`) is unchanged and still not verified for an input a caller reaches.
