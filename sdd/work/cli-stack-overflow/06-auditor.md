Role: auditor
Commit: 9f601a84da993b714a08f8865e3c12282071a36a
Verdict: pass

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Scope

Full audit. Head 9f601a84da993b714a08f8865e3c12282071a36a. `origin/main` is 9a89c85e2cdd66a0d55cdfd821c5a6fde6d5c5e2 (`git ls-remote` agrees), and it is an ancestor of the head. The working tree was clean.

The diff against `origin/main` matches the handovers:

- three production files: `LayoutComputer.ComputeBlockIndentation`, `ControlFlowBraceTransform.GetTokenBehindIfStatement`, `FluentChain.Collect`. Each gets one `RuntimeHelpers.EnsureSufficientExecutionStack()` call, a `using`, and a summary sentence;
- one new test file, `src/Reihitsu.Cli.Test/EndToEnd/CliDeepNestingEndToEndTests.cs`;
- the run's handovers.

`scripts/lane-check.sh bug-fix`: confirmed.

How I checked:

- I built `main` in Release in a temporary worktree, which I have since removed, and compared it with the head's Release CLI.
- Fixtures were generated outside the repository with a port of the skeptic's generator, plus extra shapes (below).
- I ran the new test class under `dotnet-coverage collect`, the way the SonarCloud workflow runs it.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Throw a catchable exception before `ComputeBlockIndentation` (pass 1) recurses into a child | The current thread's remaining stack, checked on entry to each level; the runtime's limit is 128 KB on 64-bit | Roslyn's `CSharpSyntaxRewriter.Visit` stack guard in the earlier phases, which uses the same runtime check and throws the same `InsufficientExecutionStackException` | Same check, same exception type, same handling by the CLI's per-file `catch (Exception)` (`FormatCommandHandler.cs:532`) | ok |
| Same, for `GetTokenBehindIfStatement` | Same check, on every level | Its mutual recursion through `GetTokenBehindInsertedCloseBrace` (`ControlFlowBraceTransform.cs:117`) always passes back through `GetTokenBehindIfStatement`, so every cycle is checked | Every cycle of the recursion passes the check | ok |
| Same, for `FluentChain.Collect` | Same check, on every level | `CollectMemberAccess` (`FluentChain.cs:473`) only calls `Collect`. `Create`, `IsOutermostChainNode` and `GetOutermostChainNode` do not recurse: the skeptic's table listed `Create` and `IsOutermostChainNode`, but neither calls itself, and `GetOutermostChainNode` is a loop | Every level of the walk is checked | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Stack check in a recursion that follows syntax nesting | `rg -n "EnsureSufficientExecutionStack\|InsufficientExecutionStack" src`: `LayoutComputer.cs:101`, `ControlFlowBraceTransform.cs:140`, `FluentChain.cs:399`. The rest of the hits are in summaries | Each call guards its own recursion, and none of them decides anything another one decides. Roslyn's rewriters follow the same pattern. Not FC-015 |
| Reporting a failed file | `FormatCommandHandler.ProcessFileAsync` `catch (Exception ex)` (`FormatCommandHandler.cs:532`), unchanged | One owner. `rg "catch \(Exception"` across Formatter, Core, Analyzer and CodeFixes found no broad catch that could swallow the new exception on the formatter or `FluentChain` path. `FormattingPipeline.cs:126` only wraps the exception, and only when a trace observer is set. `CasingCodeFixProviderBase{T}.cs:574` wraps a casing conversion and is unrelated | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DeeplyNestedFileDoesNotStopTheRun` (8 rows), `DeeplyNestedChainWithCrlfDoesNotStopTheRun` | CLI-016: a deep file does not end the run. Either it is reported as failed, with the other files, the summary and exit code 2 following, or it formats (CLI-013/014/017) | Exit 134, a missing `Z.cs` line, a missing summary, a second stderr line, or the wrong exit code for the branch taken | `VerifyRunContinues` compares the exact stdout of both branches and the exact single stderr line up to the runtime message. The message itself is the runtime's, and CLI-016 does not fix it | ok. Both branches are decided by unchanged requirements (lead decision 7). With the guard removed, the rows go red in Release, as the test implementer showed on `main` |
| `DeeplyNestedChainProcessedInParallelDoesNotStopTheRun` | CLI-016 and CLI-022 with `--max-parallelism 4` | Same as the row above, plus output in the wrong order | Same | ok |
| `ModeratelyNestedFileKeepsFormatting` (4 rows, depth 2000) | Nesting that formats today keeps formatting: exact stdout, exit 1, empty stderr | `Error processing` at depth 2000 | Direct assertions | ok. The margin is about 2× to the observed Release boundary: `chain` fails from about 4500, `elseif` from about 6500, `concat` and `memberaccess` from about 10000. The class also passed under `dotnet-coverage collect` (14/14), the way `SonarCloud.yml` runs it, so profiler instrumentation does not use up that margin |

## Failure classes dismissed

- **FC-001, FC-002, FC-003.** Each guard inspects the thread's stack, not a span of syntax. No qualifier of CLI-016 is left without a condition. The boundary is the runtime's 128 KB limit, and the tests cover both sides of it, failed and formatted.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** The change only throws. It edits no trivia, joins no gap, and moves nothing.
- **FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-036.** No rewrite is added or changed. FC-026's `else if` axis was exercised:
  - braced and unbraced ladders (`elseifnobrace`), and a ladder inside a switch section (`elseifswitch`), from 3000 to 16000;
  - every cell ended with exit 1 or 2, never 134.
- **FC-007, FC-008, FC-009, FC-018, FC-030, FC-043.** No code fix is changed. The code fixes reach the guarded methods only through the formatter, and a throw there would come only from an input that overflowed before.
- **FC-010, FC-011, FC-012.** No output changes for an input that formats, so idempotency and convergence are unaffected. The formatter, core and analyzer projects pass unchanged (implementer handover).
- **FC-013, FC-014.** No analyzer or formatter decision changes.
- **FC-015.** See *Policy ownership*.
- **FC-016.** Walked again, independently of the implementer, on the head's Release CLI. The shapes were:
  - `chain`, `concat`, `andchain`, `orpattern`, `elementaccess`, `memberaccess` and `elseif`, at their boundaries;
  - `elseifnobrace` and `elseifswitch`, from 3000 to 16000 in steps of 500 around the boundary;
  - `nullforgive` (`a!.P!.P…`), from 3000 to 16000;
  - `condaccesschain` (`a?.P?.P…`), from 3000 to 16000;
  - `coalesce` (`a ?? a ?? …`), from 2000 to 32000;
  - `notpattern` (`a is not not … null`), from 2000 to 32000;
  - `await` (`await await … a`), from 2000 to 32000;
  - `casepattern` (`case 0 or 1 or …`), from 2000 to 12000;
  - `exprbody` (an expression-bodied `c + c + …`), from 2000 to 12000;
  - `docxml` (nested XML elements in a `///` comment), from 2000 to 32000.

  None ended in a stack overflow inside Reihitsu code. The one overflow found is Roslyn's: see *Hints*.

  I also read the skeptic's remaining candidates:
  - `ExpressionBodiedTransformUtilities.RequiresAsyncExpressionBodyPreservation` and `SwitchCaseBraceRewriter.GetDirectlyDeclaredLocalNames` do not recurse along nesting. The first only dispatches on the member kind. The second does not descend into nested bodies.
  - `RewriteParseUtilities.HaveSameTokensAndStructure` is reached only from `ParsesAsWritten` in code fixes (RH3001, RH3005, RH3101, RH3105, RH3201) and never through the CLI.
- **FC-017.** See *Assertion adequacy*.
- **FC-027.** Single-line and multi-line forms are both covered (`concat`/`concat1line`), and nesting is the axis of the class.
- **FC-028, FC-029.** No layout decision changes.
- **FC-031.** All three changed summaries describe the new check. The `GetTokenBehindIfStatement` summary says it "recurses once per `else if`", which matches the code: the `GetTokenBehindInsertedCloseBrace` path re-enters only for an `if` that is a non-wrappable body. There is no `TODO`, no commented-out code, and no tracker reference in the diff.
- **FC-032.** The implementer's project runs were taken on the compiled change. The cleaner changed nothing.
- **FC-033.** The reproduction depends on the stack and the build configuration. Lead decisions 5 and 7 accept this: the tests accept either outcome. CI (`build.yml`, `SonarCloud.yml`) runs Release on `ubuntu-latest`, where it reproduces.
- **FC-034.** The issue's own shape (`chain` 6400) and its second example (`concat` 16000) are rows of the test.
- **FC-035.** The expected values come from CLI-016, CLI-017 and CLI-022, not from the patched code.
- **FC-037.** No comment-only change is claimed.
- **FC-038 to FC-042, FC-044.** No unsupported shape is declared or touched, and no guard models a result another component computes. The check reads the runtime's own stack limit.

## Architecture findings
_None._ Each guarded recursion is owned by its own component. No decision is shared, predicted, or stated twice. This agrees with the skeptic.

## Hints

1. **Guard margin.** `EnsureSufficientExecutionStack` throws once less than 128 KB of stack is left, not once the stack is exhausted. So in principle the change also fails some inputs that used to finish within the last 128 KB. The implementer's sentence "the check throws only when the thread is already near the end of its stack, where the process died before" is therefore imprecise.

   The band is lost in run-to-run noise. Near the boundary, the same input gives different results from one run to the next on `main` and on the head alike. I saw:
   - on `main`, `chain` 4500 overflow, 4650 formatted, 4750 overflow;
   - on `main`, `andchain` 11000 formatted once, then exit 134 four times;
   - on the head, `orpattern` 10000 gave exit 1, 1, then 2.

   This is the stack dependence that lead decisions 5 and 7 already accept, and the input class is rare. It is not a finding, but the PR should not claim that no input that formatted before can now fail. The same noise means that for an input at the boundary, CLI-022's "same result whatever the degree" depends on the thread. That is pre-existing: `main` crashes there.

2. **Roslyn's documentation-comment parser.** It overflows on the CLI path for `///` text with 20000 or more nested XML elements: `DocumentationCommentParser.ParseXmlElement`, repeated 15394 times, inside `FormatCommandHandler.ReadFileAsync`, exit 134. This is one more member of lead decision 3's class (Roslyn parser and lexer overflows before Reihitsu code runs, dropped). It is not new to this run and no Reihitsu guard reaches it.

3. **Test convention.** `AGENTS.md` (*Key conventions*) says that CLI end-to-end tests call `Program.Main()` directly. `CliDeepNestingEndToEndTests` runs the CLI as a child process instead. Lead decision 6 decided this with a sound reason: a stack overflow cannot be caught in process. The convention text has no exception for it, though, so a later reviewer may flag the test again. The lead may want to add a one-clause exception to that line ("…except tests that need process isolation, such as a test against an uncatchable crash") in this run's text, or file it as a follow-up.

4. **Analyzer and code-fix hosts.** `FluentChain.Collect` and the formatter phases now throw `InsufficientExecutionStackException` in IDE and compiler hosts too: the analyzer reports `AD0001`, and the code-fix service drops the action. This only happens for inputs that used to overflow, or that lie in the band from hint 1. That is an improvement on terminating the host, and nothing visible is in question for inputs that format today.

## Follow-up candidates
_None._ Hint 3 is a text amendment the lead may take into the run or drop.
