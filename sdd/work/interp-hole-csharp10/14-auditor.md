Role: auditor
Commit: fa2ba0f829803d18c2367f09bc5a15b15060a798
Verdict: pass

My prompt did not carry an intended fix, a suspected cause, or a verdict anyone hoped for.

## Scope

Repair round 1 only. This is the delta audit against `369a7ae15a94c660921e6060f354e54d5cb1bfcc`, the commit of the first audit (08).

- **Head.** `fa2ba0f` is checked out and the tree is clean.
- **Base.** `origin/main` is `0feb51810751b245fec4c51eb6fd207d0f824f75`. A fetch confirmed it, and it is an ancestor of the head.
- **Repair diff (`369a7ae..HEAD`).** It matches handovers 09–13:
  - `sdd/specs/analyzer/RH3004.md`: RH3004-010 is amended, with new examples.
  - `RH3004UseLambdaSyntaxCodeFixProvider.WithholdsFixForLines`: the hole check moves ahead of the `keepsBlockBody` branch, and the summary is rewritten.
  - `RH3004UseLambdaSyntaxAnalyzerTests`: three no-fix tests, the C# 11 test and the `$@"` test, and one Fix All test.
  - The run's work docs.
- **Scope growth.** None. No other production file, spec, rule page, or script changed.
- **Lane.** `scripts/lane-check.sh bug-fix` confirmed the bug-fix lane.

Evidence from 08 that the repair did not touch is reused as is: the analyzer guards, the Core predicate, RH3208, the test helpers, and the other fixes that call the formatter.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | **Static trace.** `WithholdsFixForLines` now returns `true` from `StringInterpolationUtilities.IsInsideSingleLineInterpolation(anonymousMethodExpression)` before it computes `keepsBlockBody`, so both paths are withheld by position alone. `RegisterCodeFixesCoreAsync` is the only place that registers the action. Fix All (`DocumentOrderFixAllProvider`) goes through the same per-diagnostic registration. Outside a C# ≤ 10 `$"` hole the predicate is `false`, so the remaining checks run exactly as before.<br>**Execution.** I ran `scripts/apply-fix.sh RH3004 <scratch> --lang-version 10` on 30 fixtures × LF/CRLF. Each fixture is `var s = <string>;` with the hole `G(delegate (int p) { return <expr>; })`. The ten expressions are every failing and holding member of #1: `p`, `new P { A = p }.A`, `new { A = p }.A`, `new List<int> { p, 1 }[0]`, `(r with { A = p }).A`, `p switch { 1 => 2, _ => 3 }`, `F(() => { var a = p; return a; })`, `F(delegate { return p; })`, `new[] { p, 1 }[0]`, and `p > 0 ? p : 1`. Each expression ran in three string kinds:<br>• `$"`: no fix offered for 20 of 20.<br>• `$@"` nested in a `$"` hole: no fix offered for 20 of 20.<br>• `$@"` outside every `$"` hole: fixed for 20 of 20, which RH3004-010 demands.<br>The run reported 0 not-converged and 0 line-ending drift. The C# 11 side is covered by `ExpressionBodyInsideANonVerbatimInterpolationIsFixedFromCSharp11` and by the existing C# 11 block-path test. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding in the repair._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3004-010 (`WithholdsFixForLines`, now first) | The anonymous method, ancestors and self, read through `IsInsideSingleLineInterpolation(SyntaxNode)` at the tree's effective `LanguageVersion` | The node the formatter formats on both paths: the lambda that replaces the anonymous method at the same position (`ApplyCodeFixAsync` → `FormatNodeInDocumentAsync`). The block path's `MoveStatementsAsync` acts on that same lambda. The formatter's own gate is the same Core predicate (`SingleLineInterpolationUtilities`, FMT-BREAK-044) | The two agree. The guard now covers both paths, so the gap from finding #1 is gone. It does not model which expressions the formatter breaks (FC-044). An anonymous method that only *contains* a `$"` string is not withheld, because the check walks ancestors, and this case is tested | holds |
| The rest of `WithholdsFixForLines` (spanning carriers, line endings, tab) | Unchanged | Unchanged | It behaves the same outside a hole. Inside a hole the method returned `true` on the block path before the change as well, so moving the check removes no withholding | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| "No line break inside a `$"` hole below C# 11" | `rg -n "IsInsideSingleLineInterpolation" src` finds one owner, `Reihitsu.Core.StringInterpolationUtilities`. RH3004 calls the existing `(SyntaxNode)` overload at one site. No caller or copy was added since 08 | single owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ExpressionBodyInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` | The expression path in a `$"` hole at C# 10 is reported and offers no fix | A registered action, or the diagnostic missing | `VerifyNoCodeFixIsOffered(source, LanguageVersion)`, which is byte-identical to 08 and was judged adequate there. The test failed on the pre-repair provider (10) | adequate |
| `ExpressionBodyWithAnObjectInitializerInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` | The same invariant for a brace construct, the failing member of #1 | Same | Same; failed before the repair (10) | adequate. One cell per side is enough, because the requirement decides by position. A test per construct would pin the formatter's layout (FC-044); the scratch run above covers the other members |
| `ExpressionBodyInsideANonVerbatimInterpolationBehindAVerbatimOneOffersNoFixBeforeCSharp11` | The nesting axis: `$@"` inside a `$"` hole | Same | Same; failed before the repair (10) | adequate |
| `FixAllLeavesTheExpressionBodyInsideANonVerbatimInterpolationBeforeCSharp11` | Fix All leaves the hole diagnostic in place and fixes `N` | A text mismatch | `Verify` with C# 10 and `MarkupHandling.Allow` on the fixed and batch states. It failed before the repair with an iterative-application mismatch (10) | adequate |
| `ExpressionBodyInsideANonVerbatimInterpolationIsFixedFromCSharp11`, `ExpressionBodyInsideAVerbatimInterpolationIsFixedBeforeCSharp11` | The other side of the boundary, in language version and in string kind | A missing or different fix | `Verify` with the language-version transform | adequate (FC-003). Both outputs fit on one line and equal `main` |

**Visible output (check 10).** The C# 10 `$"` expression-path fix that `main` offers today is now withheld. That is a withheld fix for a rare input class, and README *What reaches the maintainer* says it puts nothing in question. Lead decision 14 records it, and the PR should name the class under `Review notes`, as 09 notes. No other expected value changed.

## Failure classes dismissed

The dismissals of 08 stand unchanged for everything the repair did not move. For the repair:

- **FC-001, FC-002, FC-027, FC-028, FC-044:** traced above. The guard is placed by cause and does not predict the formatter's layout.
- **FC-003:** both boundaries are tested, C# 10 against C# 11 and `$"` against `$@"`.
- **FC-007, FC-008, FC-009:** a fix that is offered is unchanged. A withheld fix registers no action, and Fix All is tested.
- **FC-016:** the requirement is rewritten to match the code, and the spec checker (13) confirmed it.
- **FC-017:** see *Assertion adequacy*.
- **FC-031:** the summary of `WithholdsFixForLines` now names the hole check on both paths and its cause. The inline comment in `ApplyCodeFixAsync` is still accurate.
- **FC-004 to FC-006, FC-010 to FC-015, FC-018 to FC-026, FC-029, FC-030, FC-032 to FC-043:** the repair adds no rewrite, rule, formatter change, rename, text-only claim, or unsupported shape, so none of these can reach it.

The repair adds no other fix that calls the formatter. Of the 24 that 08 dismissed, the expression-level layout fixes RH5302, RH5304, and RH5308 report only forms that already span several lines, so they cannot fire inside a C# ≤ 10 `$"` hole. RH5305 and RH5306 need C# 12 and C# 11.

## Architecture findings

_None._ The FC-028 structure from 08 (a node-level entry point that detaches its target) is now refused by cause in both affected fixes, RH3004 and RH3208. No other member of the class surfaced.

## Hints

_None._

## Follow-up candidates

_None_ beyond the lead's existing `scripts/apply-fix.sh` candidate.
