Role: auditor
Commit: e97a7bf
Verdict: findings

## Scope

Full audit of head `e97a7bfb832ad16678e27a6dcc9898a088b26893` on `claude/serene-sagan-dkz5bo` against base `origin/main` `cfa445f48989c7c75731845255f90c9a0f89cff6`. Checked: the checkout is the head SHA, `git ls-remote origin refs/heads/main` returns `cfa445f…`, and that commit is an ancestor of the head (merged in 9e14b7b). The diff matches the handovers: production `DefaultDiffGenerator`, `DiffGenerator` (`GenerateWithNames`), `IDiffGenerator` (doc only), `FormatCommandHandler` (comparer extraction), the new `PathComparison`; the test `CliDryRunDiffHeaderEndToEndTests` (14 methods); the change doc; CLI-013, CLI-024, the code owners, and `requirement-ids.txt`. `scripts/lane-check.sh behavior-change`: confirmed.

Issue: thoenissen/Reihitsu#975. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Method: static tracing of `TryGetPathInside`, `HandleChangedFileAsync`, `CollectFiles`, `IsInBuildOutputDirectory`, and `PipelineTraceCommand`'s use of `DiffGenerator.Generate`. I also ran the head's Release CLI (`src/Reihitsu.Cli/bin/Release/net11.0/Reihitsu.Cli.dll`, which contains `GenerateWithNames`) on fixtures in the scratchpad, and ran a CLI built from `cfa445f` in a scratch worktree, which I removed afterwards. Probes that held: `work//src/A.cs`, `work/./src/A.cs`, `../work/src/A.cs`, `work/sub/../src/A.cs`, `./src`, `src/A.cs src/` (deduplicated), and `.` from `work/src` all print `--- a/src/A.cs` (or `a/A.cs`). The current directory `/` prints `--- a/tmp/…/A.cs`, and `../other/B.cs` prints the full path without a prefix.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Cli/Abstractions/DefaultDiffGenerator.cs:46` (`Generate` → `Directory.GetCurrentDirectory()`) | FC-016 (a new dependency that the existing paths of the `--dry-run` flow did not have) | – | PR-introduced | `--dry-run` now reads the process's current directory for every changed file. When the current directory cannot be read, `GetCurrentDirectory` throws, and the file counts as an error. Counterexample on Linux: `mkdir g && cd g && rmdir ../g`, then `reihitsu-format --dry-run /…/work/src/A.cs` with `A.cs` unformatted. **Head:** prints `Would format: /…/work/src/A.cs`, then `Error processing /…/work/src/A.cs: Unable to find the specified file.`, `0 of 1 file(s) would be formatted.`, `1 file(s) encountered errors.`, exit code 2. **`main` (cfa445f), same input:** prints the diff, exit code 1, as CLI-013 requires. Class members (axes: how the file is given × mode × platform). Fails: a full path to a file with `--dry-run`, and a full path to a directory with `--dry-run` (`--dry-run /…/work/src`: exit 2). Holds: `--check` with a full path (exit 1, unchanged, because it does not call the diff generator), and the write mode (it does not call the diff generator). Not PR-introduced: a relative path, which `GetFullPath` already cannot resolve on `main`. Not reachable: Windows, which does not let a process's current directory be deleted. Untraced: macOS with an unreadable parent directory, where `getcwd` can fail with `EACCES`; it is probably the same failure. | Treat a current directory that cannot be read as having no file inside it. The file then gets the full-path header without prefixes, which is what CLI-024's "any other file" bullet already gives, so no visible output is in question. Add a Linux end-to-end test that deletes the current directory before `Program.Main` and runs `--dry-run <full path>`. It should assert exit code 1, an empty standard error, and the `Would format:` and `---`/`+++` lines. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| File is inside the current directory (CLI-024) | `filePath[..prefix.Length]`, where `prefix` is the process's current directory plus one `Path.DirectorySeparatorChar` unless `Path.EndsInDirectorySeparator`; requires `filePath.Length > prefix.Length` | `CollectFiles`' deduplication (`HashSet` with `PathComparison.Comparer` over the full path that `GetFullPath` produces) and the `Would format:` path (the same `filePath`) | Same comparer, same normalized full path (`GetFullPath` collapses `//`, `.`, and `..`, as probed), and an ancestry condition at a separator boundary. `work-old` is outside, `..A.cs` is inside, and a root directory is handled without a double separator | ok |
| Relative-path separator | `Replace(Path.DirectorySeparatorChar, '/')` only | CLI-024: "every directory separator written as `/`" | On Windows, `GetFullPath` already turns `/` into `\`, so every separator is `\` before the replacement. On Linux and macOS a `\` is part of a name and is kept, which `OnLinuxABackslashInAFileNameIsKeptInTheRelativeHeader` checks | ok |
| Trace header keeps `a/<path>` (non-goal) | `DiffGenerator.Generate` → `GenerateWithNames($"a/{filePath}", …)` | `PipelineTraceCommand.cs:336`; `PipelineTraceCommandTests` pins `--- a/` and `+++ b/` | Unchanged | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Platform case rule for paths | `rg -n "IsWindows\(\)\|OrdinalIgnoreCase\|GetRelativePath" src/Reihitsu.Cli` → `PathComparison.Comparer` (sole owner), used by `CollectFiles` (and through it `IsInBuildOutputDirectory`) and by `DefaultDiffGenerator.TryGetPathInside`. The other `OrdinalIgnoreCase` hits are the `.cs` extension, the y/yes answer, and the generated-file suffixes. `IsInBuildOutputDirectory`'s `Path.GetRelativePath(selectionRoot, filePath)` runs only on files enumerated below that root, so the BCL case rule cannot decide anything different there | one owner, ok |
| Diff header text (`---`/`+++`) | `DiffGenerator.GenerateWithNames` only; the `a/`/`b/` prefix only in `DiffGenerator.Generate` | ok |
| Inside or outside the current directory | `DefaultDiffGenerator.TryGetPathInside` only. `Reihitsu.Tooling/CodeFixRunCommand.cs:159` computes a display path with `GetRelativePath`; that is another tool and another decision | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SpecExampleRelativePathInsideTheCurrentDirectoryPrintsTheRelativeHeader` | CLI-013 example output with the CLI-024 header | Any line of standard output differs: header, hunk, marker, or summary; or standard error is not empty | `Assert.AreSequenceEqual` over every line | adequate |
| `FullPathInside…`, `DirectoryInside…`, `FileDirectlyIn…`, `FileNameStartingWithTwoDots…`, `SpecExampleFileOutside…`, `FileInSiblingDirectory…`, `OnLinuxADirectoryDifferingOnlyInCase…`, `OnLinuxABackslash…`, `FullPathThroughASymbolicLink…`, `RelativePathInACurrentDirectoryReachedThroughASymbolicLink…` | The `Would format:` line and both header lines for each case of CLI-024 | Any of the first three output lines differs, including a prefix on an outside file or a missing prefix on an inside file | `AssertHeader` compares the first three whole lines; expected paths are built from the directory the process reports (the macOS `/var` hazard) | adequate. The symbolic-link test also asserts its precondition (the process reports the physical directory) |
| `InsideAndOutsideFilesInOneRunGetTheirOwnHeaders` | Each file of one run gets its own header decision | A wrong header for either file, or the wrong order | Lines filtered by the prefixes `Would format: `, `--- `, and `+++ `. A content line starting with `-- ` would also match, but the fixture has none | adequate |
| `FileBelowTheRootDirectoryIsInsideWhenTheRootIsTheCurrentDirectory` | A root current directory makes every file inside, without a double separator | `a//…` (the precondition plus the header) or an outside header | The expected relative path is derived by substring and replacement. That restates CLI-024's definition and is not copied from the implementation (FC-035): `a//` and outside both fail it | adequate |
| `OnWindowsADirectoryDifferingOnlyInCaseIsTheSameDirectory` | Windows: case-insensitive inside, `/` in the relative path | A full-path header, or `\` in the relative path | `AssertHeader` | adequate, but never runs in CI (ubuntu only). The handover states this |
| Missing | `--dry-run` with a current directory that cannot be read keeps exit code 1 and prints the diff (finding 1) | — | — | missing; part of finding 1 |

## Visible output is decided

Every expected value the diff adds is covered:

- The inside header `a/<relative path>` with `/`, and the outside header (full path, no prefix): the maintainer's answer in thoenissen/Reihitsu#933 (`reihitsu-format.md` Q1), recorded under `Decisions` in the change doc.
- Windows outside keeps `\`: batch 1, Q1 A.
- Symbolic links not resolved: batch 1, Q2 A.
- `..A.cs` inside, `work-old` outside, the root directory, Linux `Work` vs `work`, and a Linux `\` in a file name: each follows physically from the Q1 answer. There is only one reading in each case: `/Work` and `/work` are different directories on Linux, and `\` is a name character there.
- The CLI-013 example output: the Q1 answer for the header; every other line is unchanged.

No rule page changed.

## Failure classes dismissed

- FC-001: no guard was narrowed. The inside predicate is new and inspects the whole prefix the decision needs.
- FC-002: "inside" is an explicit ancestry condition at a separator boundary (`work-old` vs `..A.cs`, both tested).
- FC-003: both sides of every condition are tested: inside/outside, prefix vs ancestry, case on Linux and on Windows, the link given as a full path vs as a relative path, and given as relative, full, or a directory.
- FC-015: one case-rule owner (see *Policy ownership*).
- FC-016: the sibling header (`scripts/trace.sh`/`PipelineTraceCommand`) is a non-goal and is already a follow-up candidate. The new dependency on the current directory is finding 1.
- FC-017: whole-line assertions (see *Assertion adequacy*).
- FC-031: the summaries of `DefaultDiffGenerator`, `TryGetPathInside`, `DiffGenerator.Generate`/`GenerateWithNames`, the `IDiffGenerator.Generate` parameter, `PathComparison`, and the `CollectFiles` comment all describe the new code.
- FC-032: the build and test results are from after the last compiled change. The merge of `main` touched no CLI file.
- FC-033: the end-to-end tests run through `Program.Main`, which wires `DefaultDiffGenerator` (`Program.cs:312`) with the real current directory. They do not substitute the generator.
- FC-034: the first test is the issue's own scenario (`--dry-run src/A.cs` run in `work`).
- FC-035: expected values come from CLI-024 and the change-doc examples.
- FC-037: no text-only claim.
- FC-004–FC-014, FC-018–FC-030, FC-036, FC-038–FC-044: formatter, analyzer, code-fix, rename, and precheck classes. The change touches only how the CLI names a file in the `--dry-run` diff header. No formatter phase, code fix, or unsupported shape is involved.

## Architecture findings

_None._ Finding 1 is a local missing fallback, not two components owning one decision.

## Hints

- macOS: CLI-024 and `PathComparison` compare case-sensitively, while the default macOS file system is case-insensitive. A full path typed in a different case than the one `getcwd` reports gets the full-path header. CLI-024 decides this and the lead recorded it (decision 2), and no test asserts it. I mention it only so that a macOS report of a "missing" `a/` prefix is recognized as specified behavior.
- `DefaultDiffGenerator` reads the process's current directory directly rather than through `IFileSystem`, as `Program` does for its default path. So unit tests with a substituted file system cannot reach the decision; only the end-to-end tests do. This is acceptable for the current tests and is not a finding.

## Follow-up candidates

_None_ beyond the `scripts/trace.sh` header already listed in `00-lead.md`.
