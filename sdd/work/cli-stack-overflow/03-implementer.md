Role: implementer
Commit: 07e4012
Verdict: tests pass

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | `ComputeBlockIndentation` calls `RuntimeHelpers.EnsureSufficientExecutionStack()` on every recursion level; summary states it | CLI-016 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | `GetTokenBehindIfStatement` does the same (its mutual recursion through `GetTokenBehindInsertedCloseBrace` passes through it); summary states it | CLI-016 |
| `src/Reihitsu.Core/FluentChain.cs` | `Collect` does the same; it is on every level of the `Collect`/`CollectMemberAccess` mutual recursion; summary states it | CLI-016 |

The check turns the uncatchable stack overflow into the catchable `InsufficientExecutionStackException` that Roslyn's own guarded rewriters already throw, so the CLI's existing per-file `catch (Exception)` reports the file (`Error processing <path>: Insufficient stack ...`), continues, and exits with code 2. The CLI is unchanged, as the skeptic's *Surfaces* table requires. No input that formats today fails: the check throws only when the thread is already near the end of its stack, where the process died before; the 2000-depth rows stay green, and the full formatter, core, analyzer, CLI and architecture projects pass.

Not chosen: making the three recursions iterative. It would format some deep files instead of failing them, but each recursion is one node of a deep spine that Roslyn's guarded rewriters in the same pipeline already refuse a little deeper, so the gain is a shifted threshold for a rare input class, at the cost of three rewritten walks (`AGENTS.md`, *Special cases do not grow the source*).

## Policy owners

`rg -n "EnsureSufficientExecutionStack|InsufficientExecutionStack" src` — before the change: no hit. After: the three methods above. No existing owner of a stack-depth policy; Roslyn's `CSharpSyntaxRewriter.Visit` guard is the model followed.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (working tree, before the commit): `3 files changed, 21 insertions(+), 5 deletions(-)`. No new type, no new public member, no new dependency. Three production paths, the three the lead's decision 1 names.

## Deleted special cases

_N/A._

## Tests run

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliDeepNestingEndToEndTests"
```
`Passed!  - Failed: 0, Passed: 14, Skipped: 0, Total: 14, Duration: 33 s`

- `scripts/test.sh --project cli`: `Passed: 421, Failed: 0`
- `scripts/test.sh --project core`: `Passed: 747, Failed: 0`
- `scripts/test.sh --project formatter`: `Passed: 6405, Failed: 0`
- `scripts/test.sh --project analyzer`: `Passed: 11416, Failed: 0`
- `scripts/test.sh --project architecture`: `Passed: 198, Failed: 0`
- `scripts/build.sh` (Release): 0 warnings, 0 errors. `scripts/format.sh` over the three files: `Formatted 0 of 3 file(s).`

**Matrix re-run (the skeptic's request for the masked candidates).** Release CLI, `--check --max-parallelism 1`, one generated file per cell with the skeptic's generator (plus a `ternary` shape `c ? i⏎: …`), in a scratch directory outside the repository. Every cell ended with exit 1 (formatted) or exit 2 (`Error processing ...: Insufficient stack ...`, summary printed); none ended in `Stack overflow`:

| Shape | Depths run | Result |
|---|---|---|
| `chain` | 3000, 4000, 5000, 6000, 6400 (also `--max-parallelism 4`) | exit 1 up to 4000, exit 2 from 5000 |
| `concat` | 8000–8800 (step 200), 9000–14000, 16000 | exit 1 up to 8800, exit 2 from 9000 |
| `andchain` | 8000–14000, 16000 | exit 1 up to 10000, exit 2 from 10200 |
| `orpattern` | 8000–14000, 16000 | exit 1 or exit 2 between 10200 and 10800, exit 2 above |
| `elementaccess`, `memberaccess` | 6000–14000, 16000 (`memberaccess` also `--max-parallelism 4`) | exit 1 up to 10000, exit 2 from 12000 |
| `elseif` | 6000–12000 | exit 1 at 6000, exit 2 from 7000 |
| `ternary` | 4000, 8000–13000 | exit 1; 14000 is a syntax error and skipped |

The boundary cells are where the masked candidates (`AlignBinaryChain`, `AlignPatternChain`, `AlignConditionalChain`, `BreakOperatorsOntoOwnLines`) would run on the deepest tree pass 1 accepts; none overflowed. Those methods are therefore not changed.

## Disputed tests or scope

_None._ The boundary between exit 1 and exit 2 is not exact (the `orpattern` rows alternate between 10200 and 10800); that is the stack-dependence lead decision 5 already accepts, and the tests take either branch.

## Architecture findings

_None._

## Follow-up candidates

- The skeptic's unexercised self-recursive members (`RawStringLineShifts.GetDistance`, `DocCommentElementNormalizer.TryGetFirstMeaningfulContentPosition`, `SwitchCaseBraceRewriter.GetDirectlyDeclaredLocalNames`, `ExpressionBodiedTransformUtilities.RequiresAsyncExpressionBodyPreservation`, `RewriteParseUtilities.HaveSameTokensAndStructure`, the declaration-nesting members in `Reihitsu.Core`) remain unsettled: no shape the CLI parses reached an overflow in them, and the guarded `ComputeBlockIndentation` now walks every node of the same trees first. Likely drop as a rare input class unless a reproduction appears.
- The analyzer path through `FluentChain.Create` (lead follow-up candidate) is now covered: an extreme chain throws `InsufficientExecutionStackException` in `Collect`, which the analyzer host reports as `AD0001` for that analyzer instead of losing the process. Not demonstrated with a test; the analyzer tests pass unchanged.
