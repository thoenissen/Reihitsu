Role: test implementer
Commit: 25b5dfb
Verdict: reproduced

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Tests

All tests are in `src/Reihitsu.Cli.Test/EndToEnd/CliDeepNestingEndToEndTests.cs`, which is a new file.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DeeplyNestedFileDoesNotStopTheRun("chain", 6400)` | CLI-016; FC-034 (the issue's own shape) | `RunCliProcess` (CLI child process) | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("concat", 16000)` | CLI-016; FC-034 (the issue's second example) | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("concat1line", 16000)` | CLI-016; FC-027 (single-line form) | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("andchain", 16000)` | CLI-016; FC-016 | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("orpattern", 16000)` | CLI-016; FC-016 | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("elementaccess", 16000)` | CLI-016; FC-016 | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("elseif", 6400)` | CLI-016; FC-016, FC-026 (`else if` chain) | same | red, exit 134 |
| `DeeplyNestedFileDoesNotStopTheRun("memberaccess", 16000)` | CLI-016; FC-016 | same | red, exit 134 |
| `DeeplyNestedChainWithCrlfDoesNotStopTheRun` | CLI-016; FC-033 (line endings) | same | red, exit 134 |
| `DeeplyNestedChainProcessedInParallelDoesNotStopTheRun` (`--max-parallelism 4`) | CLI-016, CLI-022 | same | red, exit 134 |
| `ModeratelyNestedFileKeepsFormatting` × `chain`, `elseif`, `memberaccess`, `concat` at 2000 | none (characterization: input that formats today must keep formatting) | same | green |

### How the tests are built

Every test writes three files and runs `--check --max-parallelism <n> A.cs Deep.cs Z.cs`. `A.cs` and `Z.cs` each hold the unformatted field `int  x;`. `Deep.cs` comes from a C# port of the skeptic's generator, using the same shapes and the same text.

**Why a child process.** A stack overflow cannot be caught. An in-process `Program.Main` call, which is the convention for CLI end-to-end tests (`AGENTS.md`), would end the whole test host on `main` instead of failing one test. The tests therefore run the CLI in a child process: `dotnet <test bin>/Reihitsu.Cli.dll`. The host is taken from `DOTNET_HOST_PATH`, then from the current process when that is `dotnet`, and otherwise `dotnet`. The `Reihitsu.Cli.dll` and its `runtimeconfig.json` are already copied to the test output by the project reference. This departs from the in-process convention, and the lead may want to note it.

**What `VerifyRunContinues` asserts.** First, the exit code must be 1 or 2. If it is not, the failure message shows the first lines of stdout and stderr, which include the runtime's `Stack overflow.` / `Repeated N times: at <frame>`. After that, the outcome is one of two branches, and each is checked exactly:

- **`Deep.cs` failed** (stderr is not empty):
  - stderr is exactly one line that starts with `Error processing <Deep.cs>: `;
  - the exit code is 2;
  - stdout is exactly `Not formatted: A`, `Not formatted: Z`, a blank line, `2 of 3 file(s) need formatting.` and `1 file(s) encountered errors.`
- **`Deep.cs` formatted** (stderr is empty):
  - the exit code is 1;
  - stdout lists A, then Deep if it needs formatting, then Z, in selection order, followed by the matching `N of 3 file(s) need formatting.`

**Why there are two branches.** Lead decision 5 says the tests must not treat 6400 or 16000 as fixed thresholds. The skeptic set the condition that no input that formats today may start failing. A fix that makes a recursion iterative would format the deep file instead of failing it. CLI-016 only constrains the failing case, and CLI-013/014/017 decide the success case. Each branch asserts the complete output, so neither branch is weaker than its requirement (FC-017). The message text after `Error processing <path>: ` is the runtime's message for the exception, so it is not asserted.

**What the green guards protect.** The `ModeratelyNestedFileKeepsFormatting` rows assert the exact success output at depth 2000, where every shape formats today. They turn red for a fix that refuses or fails at a fixed depth lower than that.

## Scenarios (bug fix)

These are the observed results of `scripts/test.sh` (Release) on head 25b5dfb plus the new test file, with no production change. The frame is the one the runtime names after `Repeated N times:` in the child's stderr.

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `chain` 6400, parallelism 1 | LF | CLI (Release, child process) | reproduced | Exit 134. stdout holds only `Not formatted: A.cs` and no summary. stderr: `Stack overflow.` with `LayoutComputer.ComputeBlockIndentation` repeated 9320 times. Expected: Z.cs reported, summary, exit 1 or 2 |
| `chain` 6400, parallelism 1 | CRLF | CLI | reproduced | Exit 134, same frame (8886 repetitions) |
| `chain` 6400, parallelism 4 | LF | CLI | reproduced | Exit 134, same frame (8909 repetitions), no summary |
| `concat` 16000 | LF | CLI | reproduced | Exit 134, `ComputeBlockIndentation` (6675) |
| `concat1line` 16000 | LF | CLI | reproduced | Exit 134, `ComputeBlockIndentation` (8784) |
| `andchain` 16000 | LF | CLI | reproduced | Exit 134. The repeated frame is `ComputeBlockIndentation`; the top frame is Roslyn `GreenNode.GetFirstTerminal` |
| `orpattern` 16000 | LF | CLI | reproduced | Exit 134, `ComputeBlockIndentation` (9119) |
| `elementaccess` 16000 | LF | CLI | reproduced | Exit 134, `ComputeBlockIndentation` (12186) |
| `elseif` 6400 | LF | CLI | reproduced | Exit 134, `ControlFlowBraceTransform.GetTokenBehindIfStatement` (6026). The run takes 8–9 s |
| `memberaccess` 16000 | LF | CLI | reproduced | Exit 134, `Reihitsu.Core.FluentChain.CollectMemberAccess` (15393) |
| `chain`, `elseif`, `memberaccess`, `concat` at 2000 | LF | CLI | green | Exact success output, exit 1 |

All 10 red rows failed on the first assertion, which is the issue's own difference: the process was aborted instead of finishing the run. None failed on a compile error or a harness problem, and the test host survived every one.

## Not covered, and why

- **Code-fix and analyzer surfaces.** These are not involved per the skeptic's *Surfaces* table, which records that `apply-fix.sh RH3202` fixed the 6400 case. The analyzer path through `FluentChain` is a lead follow-up candidate. If `FluentChain` is changed, the `memberaccess` row covers its CLI path, and the analyzer behavior must stay identical; existing analyzer tests cover that.
- **Roslyn parser/lexer shapes (`generic`, `interp`).** Dropped by lead decision 3. No test is written, because a test would abort or hang on `main` and after the fix alike.
- **Masked candidates** (`AlignBinaryChain`, `AlignPatternChain`, `AlignConditionalChain`, `BreakOperatorsOntoOwnLines`, and the rest of the skeptic's table). The `andchain`, `orpattern`, `concat` and `concat1line` rows exercise binary and pattern chains at 16000. Once `ComputeBlockIndentation` no longer overflows, those rows reach the later recursions, so they are the re-run of the matrix the skeptic asked for. Shapes no row exercises:
  - conditional nesting: `ternary`, which is a syntax error at 16000;
  - raw-string nesting;
  - XML doc nesting;
  - switch-section nesting;
  - nested types: `nestedclass`, which is a syntax error at 16000.

  These are unsettled, as the skeptic recorded.
- **Debug configuration and other stack sizes (FC-033).** The child runs whatever configuration the test project was built in. `scripts/test.sh` and CI build Release, which is where the overflow reproduces. In a Debug build the 6400 rows may pass on `main`, as the skeptic observed with `format.sh`.
- **Windows exit code.** The first assertion accepts only 1 or 2, so it does not depend on 134 (SIGABRT). It holds on any platform.
- **Failure classes that cannot reach this change.** FC-001–FC-009, FC-018–FC-025, FC-028–FC-030, FC-036, FC-038–FC-044 and FC-010–FC-014 concern trivia edits, rewrites, code fixes, naming, unsupported shapes, or layout decisions. The change adds no layout decision, no fix, and no declared shape; its expected output is CLI-016's error handling or unchanged formatting. FC-031, FC-032, FC-035 and FC-037 concern the implementation and later roles. The expected values here come from CLI-016, CLI-017 and CLI-022, not from code.

## Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliDeepNestingEndToEndTests"
```

The whole class takes about 35 s on `main`. The `elseif` row takes about 9 s. `scripts/test.sh --project architecture` passed with the new file.

## Follow-up candidates

_None._ The skeptic's candidates are unchanged: the analyzer `FluentChain` path, and the dropped performance and Roslyn items.
