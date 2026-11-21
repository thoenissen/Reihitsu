Role: spec author
Commit: 5d2bac9
Verdict: change doc written

Inputs: thoenissen/Reihitsu#975 (body and the triage comment), the maintainer's decision in thoenissen/Reihitsu#933 (comment 6233289816, `reihitsu-format.md` Q1), `00-lead.md`, `sdd/specs/cli/reihitsu-format.md`. The prompt carried no intended fix or verdict. The issue's "Squad lane: bug fix" and "the open-question non-goal is removed" are stale: the lane is behavior-change (CLI-013's example pins today's header), and the spec on `main` holds no such non-goal any more, so nothing was removed.

Premise reproduced on 5d2bac9: run in a scratch directory `…/work`, `scripts/format.sh --dry-run src/A.cs` (with `src/A.cs` holding `int  x;`) printed `--- a//tmp/…/work/src/A.cs` and `+++ b//tmp/…/work/src/A.cs`, exit code 1. Caller: the CLI's `--dry-run` mode, any file.

Files written:

- `sdd/changes/2026-11/dry-run-diff-header.md` — change doc, `Status: Draft`.
- `sdd/specs/cli/reihitsu-format.md` — intro sentence on example paths (header paths are not full paths; examples run in `/work`), a new non-goal (the `--dry-run` output as a whole is not promised to be an applicable patch), CLI-013 refers to CLI-024 and its example shows `--- a/src/A.cs` / `+++ b/src/A.cs`, new CLI-024.
- `sdd/specs/requirement-ids.txt` — `CLI-024`.

Code owners and `Uses` unchanged (`FormatCommandHandler` and `DiffGenerator` are already owners of this spec).

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: `CLI-024: no test covers it`. Expected until the test implementer adds the covering test.

## Changed requirements

- CLI-013 — `--dry-run` shows the diff without writing — changed (normative sentence refers to CLI-024 for the header; example header is now relative).
- CLI-024 — The `--dry-run` diff header names the file relative to the current directory — added.

## Examples for the tests

- CLI-024 — the inside case (`src/A.cs` run in `/work` → `--- a/src/A.cs`), the same header when given by full path and through a directory, the outside case (`../other/B.cs` run in `/work/src` → `--- /work/other/B.cs`), and the boundary cases: `/work-old/A.cs` run in `/work` is outside; `/work/..A.cs` is inside (`--- a/..A.cs`); on Linux, `/work/src/A.cs` run in `/Work` is outside. The Windows examples (forward slashes in the relative path; `C:\Work` vs `C:\work` inside) can only run on Windows. Per the issue, an end-to-end test through `Program.Main()` (`[DoNotParallelize]`, current directory set to the temp directory) is the natural shape; `DiffGeneratorTests` covers relative paths only.
- CLI-013 — its example now shows the relative header.

## Output questions

**1 · `reihitsu-format --dry-run` — the diff header of a file outside the current directory, on Windows**

The maintainer's decision fixes the relative path's separator (`/`) but not a full path's. It decides only what Windows prints; on Linux both readings are identical.

Input: run in `C:\work`, `reihitsu-format --dry-run D:\other\B.cs`, with `B.cs` unformatted.

Before (`main`):

```text
Would format: D:\other\B.cs
--- a/D:\other\B.cs
+++ b/D:\other\B.cs
```

After, A (recommended) — the full path as the `Would format:` line prints it:

```text
Would format: D:\other\B.cs
--- D:\other\B.cs
+++ D:\other\B.cs
```

After, B — the full path with `/`:

```text
Would format: D:\other\B.cs
--- D:/other/B.cs
+++ D:/other/B.cs
```

Why A: without the prefix, the header serves a reader, not `git apply`, and A keeps it identical to the `Would format:` line just above it.

Requirement touched: CLI-024 (written on recommendation A: "with the full path the `Would format:` line prints"). Same question for a file on the same drive outside the current directory (run in `C:\work\src`, file `C:\work\other\B.cs`). Does not block writing the Linux tests; blocks only a Windows-only test of this case.

## Internal choices

- The header got its own requirement, CLI-024, rather than a longer CLI-013; alternative: extend CLI-013's normative sentence. Separate decision, separate `Why:`.
- "Inside the current directory" is defined by path ancestry on the selected full path (CLI-006), with the platform's case sensitivity and no symbolic-link resolution; alternatives: resolve links, or compare with a string prefix (which would put `/work-old/A.cs` inside `/work`). Keeps the header consistent with the `Would format:` line and with CLI-006's comparison.
- The current directory is the working directory the CLI runs in; the spec intro now states that examples run in `/work`.
- New non-goal: the whole `--dry-run` output is not promised to be an applicable patch (it interleaves `Would format:` lines and a summary); only the header's file name is decided. Alternative: no non-goal, which invites scope growth toward patch compatibility.
- The pipeline trace (`scripts/trace.sh`) also calls `DiffGenerator.Generate` with its own display path; the trace stays a non-goal of this spec, and its header is not specified by this change.

## Follow-up candidates

_None._
