Role: skeptic
Commit: 8d5f2cc
Verdict: output question

Inputs: thoenissen/Reihitsu#975 (body, triage comment), the decision in thoenissen/Reihitsu#933 (comment 6233289816), `00-lead.md`, `01-spec-author.md`, commit 8d5f2cc (change doc `sdd/changes/2026-11/dry-run-diff-header.md`, CLI-013, CLI-024 in `sdd/specs/cli/reihitsu-format.md`), CLI-006, `src/Reihitsu.Cli/DiffGenerator.cs`, `src/Reihitsu.Cli/FormatCommandHandler.cs` (`HandleChangedFileAsync`, `CollectFiles`), `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, the CLI tests that pin the header. The prompt carried no intended fix, cause, or hoped-for verdict.

Otherwise the change should **proceed** as specified: the premise holds, the lane is right, and the change is small. Two output questions are open: the spec author's question 1 (Windows separator in a full path), which I concur with, and a new question 2 (a symbolic link in the current directory), which comes from a contradiction inside CLI-024.

## Premise

Real. Caller: the CLI's `--dry-run` mode (CLI-013), any changed file. `HandleChangedFileAsync` passes the selected full path (`CollectFiles` → `GetFullPath`) to `IDiffGenerator.Generate`, and `DiffGenerator.Generate` writes `--- a/{filePath}` unconditionally (`src/Reihitsu.Cli/DiffGenerator.cs:45-46`).

Fixture `<scratch>/real/work/src/A.cs` (LF, trailing newline):

```csharp
class A
{
    int  x;
}
```

Run in `<scratch>/real/work`, `scripts/format.sh --dry-run src/A.cs` printed:

```text
Would format: /tmp/…/scratchpad/skeptic/real/work/src/A.cs
--- a//tmp/…/scratchpad/skeptic/real/work/src/A.cs
+++ b//tmp/…/scratchpad/skeptic/real/work/src/A.cs
@@ -1,4 +1,4 @@
```

The maintainer's motive ("so `git apply` and `patch -p1` can use it") checks out. I replaced the header of that real output with `--- a/src/A.cs` / `+++ b/src/A.cs` and kept the rest unchanged: the `Would format:` line, the blank line, and the summary. Then, in a copy of `work`:

- `patch -p1 --dry-run < new.patch` gave `checking file src/A.cs`, exit 0.
- `git init -q && git apply -v new.patch` at the tree root gave `Applied patch src/A.cs cleanly.` The file then read `    int x;`.
- `git apply --check` with `a/../other/B.cs` gave `error: invalid path 'src/../other/B.cs'`, exit 128. This confirms the change doc's rejected alternative.
- `git apply` run from a subdirectory reported `Checking patch src/A.cs...` both for `a/src/A.cs` and for `a/A.cs`. So I could not settle how it resolves paths from a subdirectory. See *Internal choices* 3.

The non-goal is honest, and in the common case the whole output still applies as it is.

Lane: behavior-change is right. CLI-013's example on `main` pins `--- a//work/src/A.cs`, so the change alters a specified output, which needs a change doc. The issue's "Squad lane: bug fix" and "the open-question non-goal is removed" are stale, as the spec author says.

The decision record in #933 is itself an agent-generated comment on the maintainer's account. It is the recorded answer to the question thread, and I do not dispute it.

## Mechanism and candidates

Not a bug against an unchanged requirement. Mechanism: the header writes `a/`/`b/` in front of whatever path the caller passes, and the `--dry-run` caller passes the selected full path. For any file, the header names the file the way the `Would format:` line does, behind the prefix.

Dispatch: `DiffGenerator.Generate` has two callers.

- `FormatCommandHandler.HandleChangedFileAsync` (via `IDiffGenerator`/`DefaultDiffGenerator`). This is the one CLI-024 changes.
- `PipelineTraceCommand.WritePhaseChange`. It passes the path as given with `\` → `/` (`PipelineTraceCommand.cs:119`). It is developer tooling and a non-goal of this spec, so it must stay. `PipelineTraceCommandTests` (lines 55-56, 191) pins its `--- a/` header.

## Breaking inputs

Failure classes that reach the change:

- FC-002: "inside" must be a real ancestry condition. Neither a string prefix nor "relative path starts with `..`" is enough.
- FC-003: test both sides of the inside/outside boundary.
- FC-015: the platform case rule already has one owner, `CollectFiles`' comparer (`OperatingSystem.IsWindows() ? OrdinalIgnoreCase : Ordinal`). See *Internal choices* 2.
- FC-033: unit tests that substitute `IFileSystem`/`IDiffGenerator` cannot see the current directory, so only an end-to-end test through `Program.Main()` exercises the real one.
- FC-017: assert whole header lines, not substrings.

Class matrix for the header. The axes are the file's position relative to the current directory (inside, directly in it, outside sibling, outside with a name-prefix match, current directory is the root), how the path is given (relative, full, through a directory), the platform (Linux, Windows, macOS), and whether the current directory involves a symbolic link:

| # | Input (run in) | Expected by CLI-024 | Existing test |
|---|---|---|---|
| 1 | `src/A.cs` (`/work`) | `--- a/src/A.cs` / `+++ b/src/A.cs` | none found |
| 2 | `/work/src/A.cs` given as a full path (`/work`) | same as 1 | none found |
| 3 | `src/` given as a directory (`/work`) | same as 1 | none found |
| 4 | `A.cs` directly in the current directory (`/work`) | `--- a/A.cs` | none found |
| 5 | `../other/B.cs` (`/work/src`) | `--- /work/other/B.cs`, no prefix | none found |
| 6 | `/work-old/A.cs` (`/work`) | outside: `--- /work-old/A.cs` | none found |
| 7 | `/work/..A.cs` (`/work`) | inside: `--- a/..A.cs` | none found |
| 8 | Linux, `/work/src/A.cs` (`/Work`) | outside | none found (needs a case-variant directory; optional) |
| 9 | Current directory `/` | inside: `--- a/work/src/A.cs` | none found (optional; low value) |
| 10 | Linux, file named `x\y.cs` in `src/` (`/work`) | `--- a/src/x\y.cs`: `\` is not a directory separator on Linux and must not become `/` | none found. Rare, but it needs no special code, only no unconditional `Replace('\\', '/')`. The trace does that replacement today. |
| 11 | Windows rows (`C:\work`; `C:\Work`; another drive) | CLI-024's Windows examples | cannot run on CI (ubuntu-only); `Path.GetRelativePath` returns the full path for another drive |
| 12 | Symbolic link in the current directory | see *Output questions* 2 | none found |

Observed for row 12 on `main`. `link` is a symbolic link to `real`. In `<scratch>/link/work` (shell `$PWD` shows `link`):

```text
$ Reihitsu.Cli --dry-run src/A.cs
Would format: /tmp/…/skeptic/real/work/src/A.cs
$ Reihitsu.Cli --dry-run /tmp/…/skeptic/link/work/src/A.cs
Would format: /tmp/…/skeptic/link/work/src/A.cs
```

The CLI's current directory is the physical one (`real`), even when the shell is in `link`. Under CLI-024 as written (links not resolved), the first call gets `--- a/src/A.cs` and the second gets `--- /…/link/work/src/A.cs`.

Test hazards for the test implementer:

- `scripts/format.sh` turns every path into a full path with the shell's logical `pwd` (`scripts/format.sh:37`). Verification through `format.sh` from a directory reached through a symbolic link therefore shows full-path headers. Verify from a physical path.
- On macOS, `Path.GetTempPath()` lies below `/var`, which links to `/private/var`. A test that sets the current directory to its temp directory and then passes a full path built from the temp-directory string would see "outside". Build expected and given full paths from `Directory.GetCurrentDirectory()` after setting it.
- Existing tests that the change touches, depending on where the implementer puts the computation: `FormatCommandHandlerTests.ExecuteAsyncDryRunCallsDiffGenerator` pins `Generate(filePath, …)` with the full path (line 966). `FormatCommandHandlerParallelTests` (line 507) pins `diff of /work/A.cs` through a substituted generator. `DiffGeneratorTests` (lines 172, 311 and following) pins `--- a/<given>`. They pin internal contracts, not visible output. Update them, do not duplicate them (*Coverage first*).
- `CliEndToEndTests` already has the `SetCurrentDirectory` pattern (lines 694-713) for an end-to-end test of rows 1-7.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FormatCommandHandler` (`--dry-run` path) | must change: the header must depend on the current directory |
| `DiffGenerator.Generate` | its unconditional `a/`/`b/` prefix cannot express the outside case, so either it or the contract its `--dry-run` caller uses changes; it must keep the trace's header |
| `IFileSystem` / `DefaultFileSystem` | possibly, if the handler needs the current directory through the abstraction; implementer's choice |
| `PipelineTraceCommand` and its tests | must stay (non-goal) |
| `Program` | not involved (the default path already is the current directory) |
| `Would format:`, `Not formatted:`, summary lines | must stay |
| Spec CLI-024 / change doc | wording changes, see *Internal choices* and *Output questions* 2 |
| Formatter, analyzers, code fixes, Fix All, `Reihitsu.Core`, rule docs, `documentation/` | not involved; no user doc shows the header (checked with `rg`) |

## Refuse option

There is no fix or formatter construct to withhold. The only "refuse" is no change: keep today's header and revert CLI-013's example. That costs nothing in code but contradicts the maintainer's decision in #933, so it is not an option. Declaring a shape unsupported does not apply, because this is CLI output, not a syntax shape. The requested change is small: one ancestry check, one relative path, and a separator replacement on Windows only, with no new type expected. Within it, the symbolic-link case (*Output questions* 2) is a rare class, and its cheap answer is the fallback the spec already writes: no link resolution, full-path header.

## Output questions

**1 · `reihitsu-format --dry-run`: the diff header of a file outside the current directory, on Windows.** This is the spec author's question. I concur that it is open: the decision in #933 says "forward slashes on every platform, after the `a/` and `b/` prefixes", which reads as a statement about the relative path only. I also concur with recommendation A, the full path as `Would format:` prints it. Linux is unaffected.

**2 · `reihitsu-format --dry-run`: the diff header when the current directory is reached through a symbolic link**

CLI-024 says both "symbolic links are not resolved" and "The header depends on where the file is, not on how its path was given". These contradict each other here, and the two readings print different headers.

Input: `/home/me/proj` is a symbolic link to `/data/proj`. The user runs `cd /home/me/proj` and then `reihitsu-format --dry-run /home/me/proj/src/A.cs`, with `src/A.cs` unformatted. The CLI's current directory is the physical `/data/proj` (observed above).

Before (`main`):

```text
Would format: /home/me/proj/src/A.cs
--- a//home/me/proj/src/A.cs
+++ b//home/me/proj/src/A.cs
```

After, A (recommended): the path is compared as given, without resolving links, and falls back to the full path:

```text
Would format: /home/me/proj/src/A.cs
--- /home/me/proj/src/A.cs
+++ /home/me/proj/src/A.cs
```

After, B: links are resolved, so the file counts as inside:

```text
Would format: /home/me/proj/src/A.cs
--- a/src/A.cs
+++ b/src/A.cs
```

With a relative path (`reihitsu-format --dry-run src/A.cs`), both options print `--- a/src/A.cs`.

Why A: the case is rare, A's fallback still names the right file, and B needs link resolution for every path, which a rare case does not justify (*Special cases do not grow the source*).

Under A, the spec sentence "The header depends on where the file is, not on how its path was given" must be narrowed. A possible wording: "The header is the same whether the file's full path is given directly, relative to the current directory, or through a directory". Whichever option is picked, the sentence and the "symbolic links are not resolved" clause must agree. The change doc should list the case with the chosen output.

## Internal choices

1. **What "the current directory" is.** It is the directory the process reports (`Directory.GetCurrentDirectory()`), which on Linux and macOS is the physical path, not the shell's `$PWD`. Recommend that CLI-024 say "the working directory the CLI process reports". This follows from *Output questions* 2 A, and it changes nothing visible beyond that question.
2. **Case rule owner (FC-015).** CLI-024 says the platform's case sensitivity "as CLI-006 compares them". CLI-006's code compares case-insensitively on Windows only and case-sensitively on macOS (`FormatCommandHandler.cs:731`). `Path.GetRelativePath` applies the BCL's own case rule, which I could not run on macOS and do not claim. Recommendation: CLI-024's wording should name the rule outright, "case-insensitively on Windows, case-sensitively elsewhere, as CLI-006 compares paths", so that the header follows the one existing rule rather than a second, BCL-internal one. The only visible effect is on macOS, for a full path typed with a different case than the current directory. That is rare, and the spec's reference to CLI-006 already decides it.
3. **The `Why:` of CLI-024 and of the change doc.** They claim that `git apply` "look[s] for the rest below the directory [it] run[s] in". I verified this for `patch -p1` and for `git apply` at the top of the work tree, but not for `git apply` from a subdirectory. Recommend stating the reason for `patch -p1` and for `git apply` at the top of a work tree, and claiming nothing more. This is wording only.

## Issue goals

- "Squad lane: bug fix": drop. The lane is behavior-change (CLI-013's example pins today's header). Not visible.
- "the open-question non-goal is removed": stale. No such non-goal exists on `main`, and the spec author added a different, honest non-goal. Not visible.
- "Add an end-to-end test through `Program.Main()`": keep. It is the only test shape that sees the real current directory (FC-033).
- "The `Would format:` line is unchanged": keep. CLI-024 states it.
- `DiffGeneratorTests` "covers only relative paths": true but not a goal. That test class pins the generator, not the header rule.

## Architecture findings

_None._ The trace and `--dry-run` share `DiffGenerator.Generate` legitimately, with different display paths, and CLI-024 keeps the trace out of scope. See *Internal choices* 2 for the one place a second copy of the case rule could appear.

## Follow-up candidates

- `scripts/trace.sh <full path>` prints `--- a//…` for the same reason: the trace passes the path as given behind `a/`, so a full path gives `a//…`. It is developer tooling, a non-goal of this spec, and no end-user output, so it is low value. Name it as a tooling follow-up only if the lead wants the two headers consistent.
