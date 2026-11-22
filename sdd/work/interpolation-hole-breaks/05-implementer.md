Role: implementer
Commit: 027e75d
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. 36 of the 41 tests in `NonVerbatimInterpolationLineBreakTests` pass, as do the CLI and RH5417 tests. The 5 that still fail assert output that no requirement produces (see *Disputed tests or scope*). I changed no test.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/StringInterpolationUtilities.cs` | New `IsInsideSingleLineInterpolation(SyntaxNode, LanguageVersion)`: the node is, or stands at any depth inside, an interpolation of a `$"` string, and the version is below C# 11. This is the one statement of the rule | FMT-BREAK-044, RH5417-005 (decision 4) |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.cs` | The private `IsInsideSingleLineInterpolation` now passes the tree's `CSharpParseOptions.LanguageVersion` to the Core predicate. Behavior unchanged: a block is never an `InterpolationSyntax`, so "self or ancestor" equals "ancestor" here. All 61 RH5417 tests pass | RH5417-005 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/SingleLineInterpolationUtilities.cs` (new, internal static) | `RunKeepingSingleLineInterpolations`: gives each outermost covered interpolation its own annotation and runs the phase's rewriters in order. After each rewriter it puts every annotated interpolation back as it stood in the phase input, so no rewriter writes a line break into one and no later rewriter sees one. At the end it strips the annotations. Below C# 11 with no `$"` interpolation, it is the old loop | FMT-BREAK-044 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | `Execute` runs its rewriters through the helper. Class summary updated | FMT-BREAK-044 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/BlankLinePhase.cs` | Same. `BlankLineEditor.InsertBlankLineBeforeToken` writes the line break that ends the line together with the blank line when two statements share a line, so `{ var a = 1; return a; }` in the interpolation became `var a = 1;⏎⏎return a;`. That is a line break FMT-BREAK-044 forbids ("the formatter MUST NOT write a line break…"). Class summary updated | FMT-BREAK-044 |
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | `VisitSwitchStatement` returns the switch as written when the original node is inside such an interpolation. The check runs on the node before the base visit, because the visited node is detached | FMT-SWITCH-017 (decision 6) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` | No shift when the block's `{` does not start its line. Without this guard, the contributor shifted the whole statement line (the line of the `{`) toward the lambda's anchor, once per sweep, so `var s = …` was indented by about 128 columns. FMT-INDENT-028 aligns the opening brace's line, and a brace that does not start a line has none. No formatter test changed output | FMT-INDENT-028 |

## Policy owners
- `rg -n "InterpolatedStringStartToken" src --glob '*.cs'` finds `StringInterpolationUtilities.IsInsideSingleLineInterpolation` (the new owner) and `StringInterpolationUtilities.RemoveInterpolationMarkers`, which is a different policy (brace unescaping). RH5417's private copy is gone; it now calls Core.
- `rg -n "CSharp11" src --glob '*.cs'` finds the Core predicate and `CasingRenameSafetyUtilities` (unrelated: generic attributes).
- The "run the rewriters, keep the interpolations" loop has one owner, `SingleLineInterpolationUtilities`, which both phases call.

## Production diff size
Nothing was committed under `src` before this commit, so `git diff --shortstat origin/main...HEAD -- src` was empty. This commit: 6 files changed, 47 insertions(+), 31 deletions(-), plus the new file `SingleLineInterpolationUtilities.cs` (about 100 lines). New type: `SingleLineInterpolationUtilities` (internal static).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"`: Failed 5, Passed 36, Total 41
- `scripts/test.sh --configuration Debug --project formatter`: Failed 5 (the same 5), Passed 7224, Total 7229
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11886
- `scripts/test.sh --configuration Debug --project cli`: Passed 441, Skipped 1 (`CliLanguageVersionEndToEndTests.LangVersion10KeepsInterpolationOnOneLine` passes)
- `scripts/test.sh --configuration Debug --project core`: Passed 887
- `scripts/test.sh --configuration Debug --project tooling`: Passed 204
- `scripts/test.sh --configuration Debug --project architecture`: Failed 1 (`EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-SWITCH-017, the spec author's new requirement, has no tagged test yet; the test implementer has to tag one, for example `SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10`), Passed 197

## Disputed tests or scope
1. **`{return 1; }` instead of `{ return 1; }`.** This covers four tests: `IfStatementInInterpolationGetsBracesOnItsLineInCSharp10`, `LocalFunctionInInterpolationGetsBlockBodyOnItsLineInCSharp10`, `LocalFunctionConstraintInInterpolationKeepsItsLineInCSharp10`, and `LocalFunctionAttributeInInterpolationKeepsItsLineInCSharp10`. They expect a space after a `{` that a structural transform created (and in the `if` test, also a space after `}` before `return 2;`). The specs produce no such space:
   - FMT-STRUCT-026 writes no space between an inserted `{` and `return`.
   - FMT-SPACE-009 says "The space after an opening brace is not decided by this rule".
   - FMT-SPACE-002 keeps a gap no rule covers.

   Actual output, which compiles under C# 10:
   - `if (b) {return 1; }return 2;`
   - `int G() {return 1; } return G();`
   - `int G<T>() where T : class {return 1; } …`
   - `[A] int G() {return 1; } …`

   In C# 11+ these braces always get their own line, so the gap never shows. The expected output would need a new spacing rule ("one space after `{` and after `}` when a token follows on the line"). That would be a visible output change and could also change one-line initializers such as `new int[]{a}`. So either the four expected outputs are corrected to what the specs produce, or the maintainer is asked as an output question. I recommend correcting the tests: the C# 10 class is rare, and the output compiles.
2. **`UsingStatementInInterpolationGetsBracesOnItsLineInCSharp10`** expects `using (var m = new MemoryStream()) { return 1; }`. No requirement braces a `using` body: `ControlFlowBraceTransform` covers only `if`/`else` (FMT-STRUCT-001). Today's newest-version output keeps `using (var m = new MemoryStream()) return 1;` unbraced as well (checked with `scripts/format.sh` on a scratch file). The run's output keeps the input unchanged, which is what FMT-BREAK-044 and the unchanged requirements give. The expected output should be the input.
3. **Scope beyond the handovers.** The skeptic named the line-break phase and the switch-case brace phase, and decision 4 added Core and RH5417. The tests also needed:
   - the blank-line phase (it does write a line break into a one-line interpolation, against the skeptic's trace claim and the blank-line spec's own non-goal under FMT-PIPE-025);
   - the indentation phase's `LambdaAlignmentContributor` guard;
   - the new helper type `SingleLineInterpolationUtilities`, so that both phases share one loop instead of two copies.

   Production paths grew from 4 to 7 files. FMT-BREAK-044's text ("the formatter MUST NOT write a line break…") already covers the blank-line phase, but decision 3 narrowed the run to the line-break phase. The spec author may want FMT-BREAK-044, or the blank-lines non-goals, to name the blank-line phase as well.
4. **Node-level limit.** `FormatNodeInDocumentAsync` detaches its target, so a target that itself lies inside a `$"` interpolation cannot see the enclosing string. A code fix that formats such a node in a C# 10 project would still break it. Only RH5301/RH5303/RH5206 fixes reach that path, and they belong to follow-up candidate 1 in `00-lead.md` (their diagnostics should not be reported there at all). No test was written for it, per Supported inputs: no caller is shown to reach it once that follow-up is fixed.

## Architecture findings
_None_ beyond decision 4, which this commit carries out: one Core predicate, used by RH5417, the line-break phase, the blank-line phase, and the switch-case brace phase.

## Follow-up candidates
_None_ beyond those in `00-lead.md`. Follow-up 1 should note the node-level limit in item 4 above.
