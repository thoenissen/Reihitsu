Role: auditor
Commit: 5ca3c53213a63b5e28ea4c50f59330220d2b6afb
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 5ca3c53 (checked out, clean tree). `origin/main` 31d2931af8e2894fba9e66a281edd23b4de9ae6c, an ancestor of the head (`git merge-base --is-ancestor`), and the branch point of the run. `scripts/lane-check.sh bug-fix`: confirmed. The diff is what 02 and 03 describe: two new test classes (unchanged since 5304634, written before the production change), the `TernaryLineBreakRewriter` change, the `CleanupPhase` change, the new `Reihitsu.Core.SyntaxTokenNavigationUtilities`, and mechanical call swaps; every removed navigation call was the default form `GetPreviousToken()` / `GetNextToken()` (67) except the one `GetNextToken(includeZeroWidth: false)` in `AttributeTargetUtilities`, and every added line outside the two rewritten methods is a helper call or a `using` (one BOM change, finding #3). No path outside `src/` and `sdd/work/long-expression-cost/`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Regression/FullPipeline/LongExpressionCostTests.cs:309-347` (`AssertCostGrowsLinearly`, time assertion) | test reliability (no catalog ID; the time arm of the reproduction) | – | PR-introduced | The time-ratio assertion runs at its bound on an idle machine, so it will fail spuriously under CI load. Measured on the head, seven runs: `ClassMembersGrowLinearly` LF ratio 15.9 (bound 16); `BlockStatementsGrowLinearly` CRLF 13.7, 12.6, 12.1, 12.2, 12.9, 15.0; `EnumMembersGrowLinearly` LF 11.9 once. The code is linear (CLI `--check` on the head: statements 8000/16000/32000 1.54/2.44/3.54 s LF, 1.50/2.40/3.55 s CRLF; fields 8000/16000/32000 1.87/3.01/4.19 s), so the excess is measurement noise, and the harness is biased toward it: the large input is timed as the minimum of at most 3 runs of about 250 ms, the base as the minimum of 5 runs of about 20 ms after a warm-up, so a scheduler or GC pause lands in the long runs far more often and the minimum of fewer samples filters it less. Every allocation ratio is 7.7 to 9.5, far from the bound | Make the time arm robust to noise without an absolute budget: the owner re-derives it (for example, equal sample counts for both sizes, a collection before each measurement, more large-input samples, or a larger base so milliseconds of noise do not dominate). Keep the allocation arm as is |
| 2 | medium | `src/Reihitsu.Core/SyntaxTokenNavigationUtilities.cs` (new public type; no test in `src/Reihitsu.Core.Test/`) | FC-017 (missing direct test of a guarantee) | – | new mechanism | The helper's whole contract is "returns exactly what Roslyn's default `GetPreviousToken()` / `GetNextToken()` return", and it is now called by 30 formatter files and 13 `Reihitsu.Core` helpers shared with the analyzers. No test pins it; its two fallback branches (a zero-width candidate that carries trivia, such as `EndOfFileToken` behind a final comment; no ancestor reaching past the token, i.e. the first or last token of a tree, of a detached node, or of a directive or documentation comment) are reached only incidentally by formatter output tests. A later change to it that differs on one of those shapes would only show if some formatter test happens to hold that shape. 28 of 41 Core utility files have a test class (e.g. `SyntaxTokenPositionUtilitiesTests`). No defect today: a throwaway program outside the repository compared helper and Roslyn on every token (`DescendantTokens(descendIntoTrivia: true)`) of all 2002 `src/**/*.cs` files, five extra inputs (omitted array sizes, `typeof(Dictionary<,>)`, `#if`/`#else`, `#region`, `///` comments, a final comment before end of file, missing tokens, skipped tokens, CRLF, empty and comment-only text) and a detached rewritten conditional: 2,075,404 tokens, 0 mismatches | Add a Core test class that asserts helper and Roslyn agree, in both directions, on every token of trees covering those shapes, the structured-trivia tokens included |
| 3 | low | `src/Reihitsu.Core/SyntaxTokenNavigationUtilities.cs`, `src/Reihitsu.Formatter.Test/Regression/FullPipeline/LongExpressionCostTests.cs`, `src/Reihitsu.Formatter.Test/Regression/FullPipeline/LongExpressionLayoutTests.cs` | repository convention | – | PR-introduced | The three new files are written without a byte order mark, while `.editorconfig` sets `charset = utf-8-bom` for `[*.cs]` (the diff also adds one to `RegionDirectiveBlankLineUtilities.cs`, which is the convention). No build or test enforces it; 301 of 2002 existing `src/**/*.cs` files lack it too | Write the three files with a BOM (cleaner) |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| `BreakOperatorsOntoOwnLines` skips a branch conditional (`HasNestedConditional(nested)`) | the nested conditional's two direct branch slots | `NormalizeTernaryOperatorPosition` at the nested conditional's own bottom-up visit: breaks when `HasNestedConditional || IsMultiLine` | The skip predicate is the first disjunct of the counterpart, so every skipped conditional was broken by its own visit; a conditional that is not skipped is broken here as before. The re-application the old code did was a no-op: after one application `?` and `:` carry no trailing line break and `HasLeadingEndOfLine` (previous-token based) is true, and the outer conditional's own edits touch only its condition's last token, the branches' first tokens and the true branch's last token, none of which decides the nested `?`/`:`. `VisitConditionalExpression` has no early exit, and no other visit override stops the descent | holds |
| `NormalizeTernaryOperatorPosition` reorder (`HasNestedConditional` before `IsMultiLine`) | same | – | both side-effect free; `&&` of the negations is commutative | holds |
| `CleanupPhase.CleanTrailingTrivia`: zero-width-inclusive lookup only when the helper returns `None` | the next token with text, then the next token of any width | old `GetNextToken(includeZeroWidth: true)` | When a token with text follows, the old lookup returned that token or a zero-width token in front of it (never `None`, never `EndOfFileToken`), so neither arm ran in either version; when none follows, both run the same call | holds |
| Helper: climb to the first ancestor whose full span reaches past the token, `FindToken` there | full span of the ancestor, position `fullStart - 1` / `fullEnd` | Roslyn's default navigation (skips zero-width, skipped tokens, directives, documentation comments) | Full spans of tokens with non-empty full span tile the ancestor, so `FindToken` returns the adjacent such token; tokens with empty full span are zero-width and skipped by Roslyn too; a zero-width candidate with trivia and the no-ancestor case fall back to Roslyn; `FindToken` with `findInsideTrivia: false` does not enter structured trivia, as Roslyn's default does not; the `EndOfFileToken` quirk of `FindToken` needs position equal to the full end, which the strict comparison excludes. Confirmed empirically (finding #2's check, 0 mismatches) | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Previous/next token with text (default navigation) on the formatter path | `rg -n "\.(GetPreviousToken\|GetNextToken)\(" src/Reihitsu.Formatter src/Reihitsu.Core`: only the helper's own fallbacks remain; every other call goes through `SyntaxTokenNavigationUtilities` | one owner; analyzer projects keep calling Roslyn directly, which the helper reproduces exactly, so no drift |
| Zero-width-inclusive navigation | `TokenLocator.TryGetPreviousToken` (`TokenLocator.cs:55,61`), `DocumentationCommentFormattingPhase.cs:292,296`, `ControlFlowBraceTransform.cs:122,142`, `CleanupPhase.cs:108` | a different policy, left on Roslyn deliberately (03); measured below that none of them makes a long list quadratic |
| Position-based token lookup | `rg -n "FindToken\(" src/Reihitsu.Formatter src/Reihitsu.Core`: line lookups in `RawStringLineShifts`, `RawStringLineUtilities`, `RegionDirectiveBlankLineUtilities`, `SyntaxIndentationUtilities`, `OrderingDeclarationUtilities`, `DocumentationCommentFormattingPhase`, `TokenLocator.GetCurrentToken` | none is a neighbour lookup; no private copy |
| Breaking nested conditionals | `TernaryLineBreakRewriter` only | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LongExpressionCostTests` conditional-chain tests (4) and the parenthesized boundary | allocation and time grow about linearly with depth, LF and CRLF | quadratic allocation (ratio 44 before the fix) or time | `AssertCostGrowsLinearly` | allocation arm strong and deterministic (9.5 / 7.8 now); time arm see finding #1 |
| `LongExpressionCostTests` list tests (7) | time grows about linearly with child count, LF and CRLF; allocation guards against trading time for memory | time ratio 41 to 64 before the fix | same | time arm is the only signal for defect B and runs at its bound (finding #1) |
| `LongExpressionLayoutTests` (9) | today's exact output for the cost shapes with comments and `#if` near the rewritten tokens; LF, CRLF, second pass, phase stability | any changed character | `AssertRuleResult` | adequate; written and passing before the production change (5304634 precedes 3e94566, file untouched since), so not taken from patched code (FC-035) |
| (none) | `SyntaxTokenNavigationUtilities` equals Roslyn's default navigation | a token where they differ | – | missing (finding #2) |

## Checks not covered above

1. **Specs.** No requirement states a cost; FMT-BREAK-018 holds for the four tagged layout tests (green). Issue thoenissen/Reihitsu#1803: chain and concatenation rows dropped as met (decision 1); re-measured on the head, still linear (chain 1600/3200 links 1.25/1.99 s, concatenation 4000/8000 terms 0.93/1.43 s). Ternary rows linear (one per line 1600/3200/6400: 0.80/1.64/2.69 s; true branch 3200/6400: 1.52/2.79 s). Lists linear (array 4000/8000/16000/32000: 0.50/0.73/1.60/2.12 s; enums 4000 to 32000: 0.53/0.97/1.49/2.59 s; arguments 4000/8000: 0.59/0.92 s). "Slowest phase identified" is in 01-skeptic.md per defect. Output identity: 03's 6019-file comparison plus the equivalence check of finding #2.
2. **Sibling shapes (FC-016).** The Roslyn calls left in place, measured for long inputs that reach them: a class of N fields each with a `///` comment (2000/4000/8000/16000: 1.11/1.98/3.78/5.80 s) and a block of N unbraced `if` statements, on two lines and on one (2000/4000/8000: 1.92/2.63/3.60 s; 1.95/2.68/3.82 s): no quadratic growth.
3. **Scope.** Swaps in analyzer-only Core helpers (`DeclarationModifierUtilities`, `AdjacentTokenSpacingUtilities`) go beyond the formatter path; decision 5 keeps them, and their results are identical (finding #2's check). No other out-of-scope edit.
4. **Comments (FC-031).** The rewritten summaries of `NormalizeTernaryOperatorPosition`, `BreakOperatorsOntoOwnLines`, `HasNestedConditional`, the `CleanupPhase` comment, and the helper's summaries describe the new code. No `TODO`, no tracker number in source.
5. **Visible output.** No expected value of an existing test changed; the new layout tests pin `main`'s output (green before the fix), so no output is in question and no maintainer answer is needed.
6. **Cost of special cases.** No refusal, guard, or rare-class special case is added; the new type is the issue's own fix (decision 5). No declared shape is involved.
7. **Code quality.** The ternary change also bounds the recursion of `BreakOperatorsOntoOwnLines` to two levels (it was the chain depth). Public XML documentation present on the new type.

## Failure classes dismissed
- FC-001, FC-002, FC-003 — guards traced above; the only added predicates are equivalence-preserving.
- FC-004, FC-005, FC-019, FC-020, FC-021 — no trivia is read or written differently; output byte-identical; the layout tests hold comments and `#if` next to the rewritten tokens.
- FC-006, FC-022 to FC-026, FC-029, FC-036 — no rewrite shape, literal, identifier, or member kind is changed.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043 — no code fix changed; fixes reach the helper only through the formatter, whose results are unchanged.
- FC-010, FC-011, FC-012 — output unchanged, so idempotency and convergence are as on `main`; no loop added.
- FC-013, FC-014 — no analyzer or rule changed.
- FC-015 — see Policy ownership.
- FC-027 — the nesting axes (true branch, false branch, one line, one per line, field initializer, parenthesized boundary) are covered by tests and measurements.
- FC-028, FC-041 — the helper is position-based within any tree; checked on a detached rewritten node.
- FC-032 — the analyzer suite ran after the last Core swap (03); the validator reruns the default set.
- FC-033, FC-034 — the reproductions use the reported shapes at reporting sizes (02).
- FC-035 — tests predate the production change.
- FC-037 — no text-only claim.
- FC-038, FC-039, FC-040, FC-042 — no unsupported shape declared or touched; the precheck's change is two call swaps.
- FC-044 — nothing modelled; the helper reads Roslyn's own tree positions.
- Tab-width matrix — no tab handling touched.

## Architecture findings
_None._

## Hints
- The cost tests format through `ReihitsuFormatter.FormatSyntaxTree` and measure allocation with `GC.GetAllocatedBytesForCurrentThread`; this is exact only while the formatter stays single-threaded, which holds today.

## Follow-up candidates
_None_ beyond the deep-spine stack overflow already recorded in 00-lead.md.
