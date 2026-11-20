Role: auditor
Commit: 3751eee36b0415a57f744991e890ec6c0cae26b1
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against 5ca3c53 (audit 1, `06-auditor.md`). Head 3751eee checked out, clean tree. `origin/main` 31d2931af8e2894fba9e66a281edd23b4de9ae6c, an ancestor of the head. `scripts/lane-check.sh bug-fix`: confirmed. The `src/` delta is what 07 and 08 describe and nothing else: the harness of `LongExpressionCostTests` (`Measure`, `AssertCostGrowsLinearly`, two constants, the class summary), the new `src/Reihitsu.Core.Test/SyntaxTokenNavigationUtilitiesTests.cs`, and a byte order mark added to `SyntaxTokenNavigationUtilities.cs` and `LongExpressionLayoutTests.cs` (diff: first line only, `ef bb bf`; all four files now start with it). No production code changed, no expected output changed, no spec touched, so the scope did not grow and this is a delta audit.

Previous findings:

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The time arm now takes alternating samples of about equal length (base formatted `GrowthFactor` times per sample, large once), the cheapest of 3 to 9 rounds per size, and counts time as grown only when both processor-time and wall-clock ratios exceed the bound. Measured on the head: idle, all 24 arms, asserted ratio (min of the two clocks) 6.4 to 9.0, memory 7.8 to 9.6; under 8 busy loops on 4 cores, highest processor ratio 11.3, highest wall ratio 10.7, highest asserted ratio 10.7, all passed (was 15.9 idle at audit 1). The bound, the sizes, and the factor are unchanged, and 07 shows the reproduction still fails on the unchanged production code (allocation ratio 45.2 and time ratio 59.9 / 60.1), so a quadratic cost still raises both clocks far above 16 |
| 2 | closed | `SyntaxTokenNavigationUtilitiesTests` (9 tests, passed) asserts helper equals Roslyn in both directions on every token of `DescendantTokens(descendIntoTrivia: true)`, covering every shape audit 1's throwaway check used: long lists (enum, initializer, arguments, fields, statements), CRLF, omitted array sizes and type arguments, end of file behind a final comment, first and last token, `#define`/`#region`/`#pragma`/`#if`/`#elif`/`#else` with disabled text, `///` and `/** */` with `cref`, missing and skipped tokens (asserted present), empty and comment-only text, a detached expression, a replaced node, and a subtree root. 07's mutation check kills each of the two fallbacks and the strict comparison |
| 3 | closed | All three files carry a BOM; the change is the first line only |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Early stop of the measuring rounds (`round >= MinimumRounds && byteRatio <= bound && min(processor, wall) <= bound`) | the cheapest sample of each size so far | the final assertions (`byteRatio <= bound`, `min(processor, wall) <= bound`) | Same quantities, same bound, same comparison; the loop stops only when both final assertions would pass, otherwise runs to `MaximumRounds` and the assertions report the last (overall cheapest) values | holds |

Production guards: none changed since audit 1; its guard-scope table stands byte-identical.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Previous/next token with text | unchanged since audit 1 (only the helper and its fallbacks on the formatter path); the repair adds a test, no owner | one owner |
| Linear-cost measurement | `AssertCostGrowsLinearly` in `LongExpressionCostTests` only | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LongExpressionCostTests` (12) | allocation and time grow about linearly, LF and CRLF | allocation ratio or both time ratios above 16 (45 to 60 before the fix, per 07's revert run) | `AssertCostGrowsLinearly` | adequate; the min of two clocks weakens nothing a quadratic cost produces, since it raises both; margins now 7 to 11 against 16 |
| `SyntaxTokenNavigationUtilitiesTests` (9) | helper equals Roslyn's default navigation on every token, structured trivia included | any token whose neighbour differs | `AssertAgreesWithRoslyn` | adequate; exhaustive per tree, compares against Roslyn rather than a hand-written expectation, and the explicit asserts confirm each fallback shape is actually present in its input |

## Failure classes dismissed
- Every production-facing class (FC-001 to FC-016, FC-018 to FC-030, FC-032 to FC-044, tab-width matrix): the repair changes no production code beyond a BOM, so audit 1's dismissals stand unchanged.
- FC-017 — checked above for both changed test classes.
- FC-031 — the rewritten summaries of the cost-test class, `Measure`, `AssertCostGrowsLinearly`, `MinimumRounds`, `MaximumRounds`, and `Min`, and the new test class's summaries describe the code; no `TODO`, no tracker number.

## Architecture findings
_None._

## Hints
- The base sample formats one parsed tree eight times while a large sample formats its tree once, so per-tree work the input tree caches (its red nodes, the `LineStartIndex` `ConditionalWeakTable` entry) is paid once per eight base formattings but once per large formatting. This makes the base slightly cheaper per formatting and biases the ratio upward, toward a spurious failure rather than a hidden regression; measured ratios (7 to 11) show the effect is small.
- The cost tests' processor-time arm measures the whole process (`Environment.CpuUsage`); that is sound only while the class stays `[DoNotParallelize]` under the assembly's method-level parallelization, and the wall-clock arm covers it otherwise because the assertion takes the smaller ratio.

## Follow-up candidates
_None_ beyond the deep-spine stack overflow already recorded in 00-lead.md.
