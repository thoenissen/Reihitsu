Role: implementer
Commit: (this commit; parent 62a32c6)
Verdict: tests pass

Repair round 1, decisions 12 and 14 in `00-lead.md`, auditor finding #1 and hint (`11-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `RegisterRelocation` takes the language version and registers no relocation for a documentation comment whose owning token stands in an interpolation for which `StringInterpolationUtilities.IsInsideSingleLineInterpolation` is true; the comment stays where written. The check runs only after the trivia is known to be a documentation comment written behind code | FMT-BREAK-044 (documentation-comment part), FMT-DOC-001 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | `VisitIfStatement` reads the Core predicate on the original node; `WrapWithBraces` gets `forbidsLineBreak` and returns the statement unbraced when it is set and either brace's FMT-STRUCT-026 separator would be a line break. A body that needs no separator is still braced (`w`, the `if` body of the `else` case) | FMT-BREAK-044 (structural part), FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/SingleLineInterpolationUtilities.cs` | `Annotate` returns the root unwalked when every interpolation can hold a line break (decision 14) | FMT-BREAK-044 (no output change) |
| `src/Reihitsu.Core/StringInterpolationUtilities.cs` | New `CanInterpolationsHoldLineBreaks(LanguageVersion)` states the C# 11 boundary once; `IsInsideSingleLineInterpolation` and `Annotate`'s early return both call it | FMT-BREAK-044 |

## Policy owners
- `rg -n "IsInsideSingleLineInterpolation|CSharp11\b" src --type cs`: the boundary is stated only in `StringInterpolationUtilities.CanInterpolationsHoldLineBreaks`; the hole predicate only in `IsInsideSingleLineInterpolation`. Callers: RH5417, `SingleLineInterpolationUtilities`, `SwitchCaseBraceRewriter`, and now `DocumentationCommentFormattingPhase` and `ControlFlowBraceTransform`. `CasingRenameSafetyUtilities:736` is an unrelated C# 11 check (generic attributes).
- FMT-STRUCT-026 separators: still decided only in `WrapWithBraces`; the refusal reads the two trivia lists it already computes, no second copy.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this round: 7 files changed, 148 insertions(+), 31 deletions(-). Delta of this round: 4 files changed, 46 insertions(+), 8 deletions(-). No new type; one new public member (below).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"`: Passed 62 of 62 (the 11 failing tests of `13-test-implementer.md` now pass).
- `scripts/test.sh --configuration Debug` (default run, before the Core extraction): Analyzer 11886, Formatter 7250, Core 887, Cli 441 (+1 skipped), Architecture 198, Tooling 204, Playground 23 — all passed.
- After the Core extraction: formatter 7250, core 887, architecture 198, analyzer `~RH5417` 67 — all passed.
- `scripts/format.sh` on the four files: 0 of 4 changed.

## Disputed tests or scope
- New public member `StringInterpolationUtilities.CanInterpolationsHoldLineBreaks` in `Reihitsu.Core`. Decision 14's early return needs the C# 11 boundary; writing `>= LanguageVersion.CSharp11` in `Annotate` would be a second statement of it (FC-015), so the boundary moved into one Core method. Lead decides; the alternative is a literal check in `Annotate`.
- The structural refusal is scoped to the `if`/`else` bodies this transform braces. For a nested `if (a) if (b) return 1; /** d */ return 2;` in a C# 10 hole, the outer body is refused (its `}` would need a line break) while the inner body's next-token reading still assumes the outer brace, so the inner body is braced with `}` directly followed by `/** d */`. That output holds no line break and Roslyn never puts a documentation comment in trailing trivia, so it compiles and reparses the same; not tested, not modelled (rare class, decision 12).

## Architecture findings
_None._

## Follow-up candidates
_None._
