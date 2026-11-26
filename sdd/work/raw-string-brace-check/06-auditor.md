Role: auditor
Commit: a5dd6d4
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue and the handovers.

## Scope

Full audit of head a5dd6d4e89202203a091517e903b95be629bdee4 against `origin/main` eed7195c0b6fb583ab4271173b7a4ad022851c5f (`git ls-remote origin refs/heads/main` returns the same SHA; it is an ancestor of the head, so `main` is merged). The checkout is the head. The diff is what 03-implementer.md describes: one doc-comment change in `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` (`IsHoleOpeningLine`), one predicate change plus `using Reihitsu.Core;` in `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/SingleSpaceSpacingRule.cs` (`RequiresSpaceBeforeBrace`), and the run's handovers under `sdd/work/raw-string-brace-check/`. No test, spec, change doc, or rule page changed.

`scripts/lane-check.sh bug-fix`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`. Its text-only proof marks `SingleSpaceSpacingRule.cs` as the one file with compiled behavior and `RawStringLineShifts.cs` as xml-doc impact only, which matches decisions 1 and 2.

Issue #1722 ("Done when": the Core check is used for the brace test, or a comment there states why it does not; the formatter's tests pass unchanged): addressed by the second branch. The comment's claim holds in the code. `CollectEdits` (`RawStringLineShifts.cs:226-244`) writes the moved indentation of a line that starts with a hole's opening brace as literal text, into the previous `InterpolatedStringTextToken` or as a new leading text element (`InsertLeadingText`). A line that starts with a closing brace takes the ordinary path, `ApplyEdits` → `ApplyLeadingTriviaEdit`, which replaces the brace's leading whitespace trivia. `HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` (`tests/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs:240`) pins that difference. The goal "one statement per policy" is met through the adjacent fix: no private copy of the interpolation-brace check remains (see *Policy ownership*).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace`: a `{`/`}` token gets one space in front unless it is an interpolation brace (FMT-SPACE-009) | the token `next` alone: kind `OpenBraceToken`/`CloseBraceToken`, then `IsInterpolationBrace` (its parent, and identity with the parent's open or close brace) | RH6009 / RH6010 analyzers, `FormattingTextAnalysisUtilities.IsInterpolationBrace` (the same helper now) | Identical for every token the kind guard admits. `InterpolationSyntax`'s only token members are `OpenBraceToken` and `CloseBraceToken`, and `$$` braces (`{{`) are still of those kinds. The old `Parent is not InterpolationSyntax` and the new call therefore agree on all brace tokens, and formatter and analyzers now share one predicate | ok |
| `RawStringLineShifts.IsHoleOpeningLine` (unchanged code; doc only) | the line-start token: parent `InterpolationSyntax`, identity with `OpenBraceToken`, `SpanStart == line.Start` | `IsInterpolationBrace` is a different decision (open or close). The closing-brace side goes through `ApplyLeadingTriviaEdit` | Different questions on purpose. The comment now states the reason | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Is a token a brace of an interpolation | `rg -n "is (not )?InterpolationSyntax\|as InterpolationSyntax\|SyntaxKind\.Interpolation\b\|IsInterpolationBrace" src --type cs`: owner `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs:345` (`IsInterpolationBrace`); callers RH5025 (`:68`), RH6009 (`:51`), RH6010 (`:51`), `SingleSpaceSpacingRule.cs:42`. `RawStringLineShifts.cs:278` (`as InterpolationSyntax`, open-brace-only, a different decision, documented). `RH5202...CodeFixProvider.cs:122` (`content is InterpolationSyntax`, a node-level content check, not a brace token). `rg "Interpolat"` over `Pipeline/HorizontalSpacing` and `Pipeline/BlankLines` finds no other brace exemption | one owner — ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| _(no test added or changed)_ `HorizontalSpacingRequirementTests.OneSpaceBeforeBracesAndAroundDeclarationColons` (FMT-SPACE-009) | one space before ordinary braces, none before interpolation braces | `new int[]{ a }` or `$"{a }"`-style output. 02-test-implementer.md shows by mutation that each side of the rewritten predicate fails this test | `AssertRuleResult`, exact output, LF and CRLF | adequate |
| `HorizontalSpacingRewriterTests.HandlesStringInterpolation` | interpolation braces keep the author's spacing | `$"Hello  {name }!"` (mutation shown in 02) | `AssertHorizontalSpacing`, exact output | adequate |
| `RawStringMovedLineStartTests.HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` and the opening-brace cases | open-brace-only detection in `IsHoleOpeningLine` | literal text inserted in front of `a`, `}` left in column 0 (01-skeptic.md, mutation) | exact formatter output | adequate. The code is unchanged and the doc comment is proven text-only |

The adjacent fix's "it gets a test" condition (`squad/README.md`, *Scope*) is met by existing tests that are shown by mutation to pin both sides of the predicate (*Coverage first*).

## Failure classes dismissed
- FC-001, FC-002, FC-003: the only changed predicate is equivalent to the old one on every token its kind guard admits. The span (`next` token) and its boundary are unchanged, and the mutation runs in 02 pin both sides.
- FC-015, FC-016: checked above. One owner remains, and the one remaining `InterpolationSyntax` brace test is a different decision with its reason stated at the site.
- FC-004, FC-005, FC-019, FC-020, FC-021: no trivia handling, join, or gap edit changed.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-027, FC-029, FC-036: no rewrite changed. `RawStringLineShifts` (FC-022) carries a comment-only change, proven text-only by `verify-text-only.sh` (lane check above).
- FC-028: `SingleSpaceSpacingRule` is called the same way by every entry point. The predicate reads only the token and its parent.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix touched.
- FC-010, FC-011, FC-012: output unchanged, so idempotency and phase interplay are unchanged.
- FC-013, FC-014: formatter and RH6009/RH6010 now share the exemption, so parity is strengthened. No new rule.
- FC-038 to FC-042: no unsupported shape declared, checked, or guarded.
- FC-044: no computed result is modelled.
- FC-017, FC-033, FC-034, FC-035: no test added or changed. The existing tests' strength is covered above.
- FC-031: `RequiresSpaceBeforeBrace`'s summary and the class summary ("outside string interpolation") still describe the code. `IsHoleOpeningLine`'s new sentences match `CollectEdits`/`ApplyLeadingTriviaEdit`.
- FC-032: no project result is reused across the compiled change. The implementer ran the formatter project after the change (7286 passed).
- FC-037: text-only proven with `verify-text-only.sh`, not grep.
- Tab-width matrix: no tab handling or column computation changed.

## Architecture findings
_None._

## Hints
_None._

## Follow-up candidates
_None_ beyond the `IsOwningConstruct` duplicate already in `00-lead.md`.
