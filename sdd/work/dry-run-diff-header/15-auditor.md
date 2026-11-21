Role: auditor
Commit: d55232d
Verdict: pass

## Scope

Repair delta: changes from `5ea38fc` (the first audit) to head `d55232d361c17d51c010b05cff183b5e8d7dffa5` on `claude/serene-sagan-dkz5bo`. Base `origin/main` is `cfa445f48989c7c75731845255f90c9a0f89cff6`, which `git ls-remote origin refs/heads/main` also returns, and it is an ancestor of the head. I checked that the checkout is the head SHA. `scripts/lane-check.sh behavior-change` returned confirmed.

The repair diff is what handovers 11–14 describe. `DefaultDiffGenerator` gets a new private `TryGetCurrentDirectory`, and `Generate` uses it. `CliDryRunDiffHeaderEndToEndTests` gets two Linux tests and the helpers `AssertWholeOutput` and `RunInDeletedDirectory`. The rest of the diff is the run's handovers. The repair changed no spec, change doc, or `requirement-ids.txt`, and its scope did not grow, so this is a delta audit. Evidence from `10-auditor.md` for the code the repair left unchanged is reused: `TryGetPathInside`, `PathComparison`, `DiffGenerator`, `FormatCommandHandler`, and the first 14 tests.

Issue: thoenissen/Reihitsu#975. The prompt named only the scope and the inputs. It carried no intended fix, suspected cause, or hoped-for verdict.

### Previous findings

- #1 (high, FC-016: `--dry-run` with a full path exits 2 when the current directory cannot be read): **closed**. The listed members are a full path to a file and a full path to a directory under `--dry-run`. Both reached `Directory.GetCurrentDirectory()` only through `IDiffGenerator.Generate` (`FormatCommandHandler.cs:610`). That call now goes through `TryGetCurrentDirectory`, and an unreadable directory gives the full-path header. Each member has a test that asserts exit code 1, an empty standard error, and the whole standard output. The cleaner ran these tests green after the last compiled change (13-cleaner.md: 15 pass, 1 skipped), and `94fd7ae..d55232d` changes no `.cs` file. The members that held in the first audit are unaffected: `--check` and the write mode do not call the generator. Relative paths and Windows are not PR-introduced or not reachable, as before.

## Findings

_N/A — no finding._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The current directory is unreadable, so no file is inside it | The exception from `Directory.GetCurrentDirectory()`, filtered to `IOException` (including `FileNotFoundException` for a deleted directory and `PathTooLongException`) or `UnauthorizedAccessException` (EACCES) | CLI-024: the current directory is "the working directory the CLI process reports". The "any other file" bullet covers a file when no directory is reported. `Program.cs:58` reads the current directory only when no path is given, which is a different decision unchanged from `main` | On Unix, .NET maps every `getcwd` errno through `Interop.GetExceptionForIoErrno` to these types, so every failure is covered and no unrelated exception type is swallowed. Only a successfully read directory reaches `TryGetPathInside`, and that guard is unchanged | ok |
| File is inside the current directory | Unchanged from `10-auditor.md` | Unchanged | Unchanged | ok (reused) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Reading the current directory for the header | `rg -n "GetCurrentDirectory" src/Reihitsu.Cli` finds `DefaultDiffGenerator.TryGetCurrentDirectory` (the header) and `Program.cs:58` (the default path when no path is given, from `main`, and a different decision) | one owner, ok |
| Inside or outside the current directory | `DefaultDiffGenerator.TryGetPathInside` only (unchanged) | ok |
| Diff header text | `DiffGenerator.GenerateWithNames` only. The only other generator call is the trace command's `DiffGenerator.Generate` (`PipelineTraceCommand.cs:336`), which the change doc lists as a non-goal | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FullPathToAFileWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader` | With an unreadable current directory, `--dry-run <full path to file>` prints the diff with the full-path header, writes nothing to standard error, and exits 1 (CLI-013, CLI-024) | Exit code 2; `Error processing …` on standard error; an `a/`/`a//` header; a missing diff or summary line | `RunInDeletedDirectory` asserts the precondition that `GetCurrentDirectory` throws an `IOException`, so the test cannot pass when the directory is still readable. It restores the previous directory in `finally`. `AssertWholeOutput` compares every line of standard output | adequate. It reproduced red before the fix (exit 2) |
| `FullPathToADirectoryWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader` | Same as above, for a file selected through a directory given by its full path | Same as above | Same as above | adequate. It reproduced red before the fix |
| Isolation of the two tests | Changing and deleting the process's current directory does not affect other tests | Another test that reads the current directory while it is deleted | The class has `[DoNotParallelize]`. MSTest runs those methods serially after the parallel ones (assembly `Parallelize` at method level), as the existing `RunIn` helper already relies on | ok |

## Visible output is decided

The two new expected values are the `--- <full path>` and `+++ <full path>` header, exit code 1, and the unchanged diff and summary. CLI-024 demands them, and the run does not change it: the process reports no working directory, so no file is inside one, and "Any other file" applies. Only one reading is possible: the alternative is `main`'s error and exit code 2, which CLI-013 forbids. No rule page changed.

## Failure classes dismissed

- FC-001/FC-002: the new guard filters exceptions and has no span boundary. The ancestry guard is unchanged.
- FC-003: both sides are tested. A readable directory is covered by the 14 earlier tests, and an unreadable one by the 2 new tests.
- FC-015: one owner (see *Policy ownership*).
- FC-016: the repair adds no dependency. It removes the hard dependency that finding #1 named. The sibling trace header is still a follow-up candidate.
- FC-017: whole-output assertions plus a precondition assertion.
- FC-031: the summary of `TryGetCurrentDirectory`, the inline comment in `Generate`, and the class summary describe the new code. There is no `TODO`, no commented-out code, and no tracker number.
- FC-032: the build and the focused tests (13-cleaner.md) ran after the last compiled change (`dbdffb6`). Later commits touch only `sdd/work/`.
- FC-033: the tests run through `Program.Main` with the real `DefaultDiffGenerator`.
- FC-034: each test is the counterexample of finding #1.
- FC-035: the expected values come from CLI-024 and CLI-013, not from the implementation.
- FC-037: no text-only claim.
- FC-004–FC-014, FC-018–FC-030, FC-036, FC-038–FC-044: these are formatter, analyzer, code-fix, rename, and precheck classes. The repair touches only how the CLI reads the current directory for the `--dry-run` header.

## Architecture findings

_None._

## Hints

- The summary of `TryGetCurrentDirectory` says an unreadable current directory "contains no file the CLI can name". That holds exactly for a deleted directory. For the untraced EACCES case, a full path could still pass through the directory. The outcome there, a full-path header, is still what CLI-024 gives when the process reports no directory, so no behavior is wrong. This is not a finding.

## Follow-up candidates

_None_ beyond the `scripts/trace.sh` header already listed in `00-lead.md`.
