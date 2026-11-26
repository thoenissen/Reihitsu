# The pipeline trace names the file in its diff header as `--dry-run` does

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — the pipeline trace (`scripts/trace.sh`) is a repository tool that no spec covers; the CLI spec excludes it as a non-goal |

## What

The `---` and `+++` lines of every phase diff the pipeline trace prints name the traced file the way CLI-024 decides for the diff of `reihitsu-format --dry-run`, applied to the full path the trace's `Tracing <full path> (maximum passes: <n>)` line prints and to the current directory of the trace process:

- a file inside the current directory — that directory is one of the directories its full path passes through — is named `--- a/<relative path>` and `+++ b/<relative path>`, the full path relative to the current directory with `/` as the directory separator;
- any other file is named `--- <full path>` and `+++ <full path>`, without the `a/` and `b/` prefixes.

Paths are compared as CLI-024 compares them: as written, without resolving symbolic links, case-insensitively on Windows and case-sensitively elsewhere. The header is the same whether the file is given by its full path or by a relative path, with or without `.` and `..` segments.

Today the header is `a/` and `b/` followed by the path exactly as the caller gave it, with `\` replaced by `/`. Every other line of the trace — the `Tracing` line, the skip lines, the pass and phase headers, the hunks, the line-ending counts, the summary lines — and every exit code is unchanged. `scripts/README.md` (*Formatter pipeline trace*) states the header.

## Why

The trace reproduces, phase by phase, the diff `reihitsu-format --dry-run` prints for a whole pass, and both use the same diff generator. A full path gives a header with a doubled slash, `--- a//work/src/A.cs`, which is the header CLI-024 replaced for `--dry-run`: the `a/` and `b/` prefixes are the git convention for a path relative to the root of the tree a patch applies to, and a full path behind them names no file `patch -p1` or `git apply` can find. A path given with `.` or `..` segments keeps them in the header (`--- a/./src/A.cs`, `--- a/../src/A.cs`), so the same file gets different headers depending on how it was named.

Reproduced on the run's start commit b8b7365, run in a directory (shortened here to `/work`) that holds `src/A.cs` with the content `class C{void M(){}}`: `scripts/trace.sh /work/src/A.cs` prints `--- a//work/src/A.cs` and `+++ b//work/src/A.cs`; `scripts/trace.sh src/A.cs` prints `--- a/src/A.cs`; `scripts/trace.sh ./src/A.cs` prints `--- a/./src/A.cs`; run in `/work/src`, `scripts/trace.sh ../src/A.cs` prints `--- a/../src/A.cs`.

## Visible output changes

None — no output an end user sees changes. The trace is repository tooling (`scripts/trace.sh`), not part of `reihitsu-format`, the analyzers, or the code fixes. Its own output changes as follows; in each case only the two header lines of every phase diff change.

### 1. A full path inside the current directory

Run in `/work`:

```shell
scripts/trace.sh /work/src/A.cs
```

Before:

```text
== Pass 1: LineBreakPhase ==
--- a//work/src/A.cs
+++ b//work/src/A.cs
```

After:

```text
== Pass 1: LineBreakPhase ==
--- a/src/A.cs
+++ b/src/A.cs
```

### 2. A relative path inside the current directory

Run in `/work`, `scripts/trace.sh src/A.cs` prints `--- a/src/A.cs` and `+++ b/src/A.cs`, before and after. `scripts/trace.sh ./src/A.cs` printed `--- a/./src/A.cs` and now prints `--- a/src/A.cs`. Run in `/work/src`, `scripts/trace.sh ../src/A.cs` printed `--- a/../src/A.cs` and now prints `--- a/A.cs`, because the file lies in the current directory.

### 3. A file outside the current directory

Run in `/work/src`, with `/work/other/B.cs` unformatted:

```shell
scripts/trace.sh ../other/B.cs
```

Before:

```text
--- a/../other/B.cs
+++ b/../other/B.cs
```

After:

```text
--- /work/other/B.cs
+++ /work/other/B.cs
```

Run in `/work`, `scripts/trace.sh /tmp/B.cs` printed `--- a//tmp/B.cs` and now prints `--- /tmp/B.cs`.

## Decisions

### The trace follows CLI-024 instead of keeping its own header

The issue offered two outcomes: change the header, or keep `a/<path as given>` and record that as a decision. The header is changed, because the trace exists to show what `reihitsu-format` does to a file, and a header that differs from the `--dry-run` header for the same file — and that is broken in the same way the `--dry-run` header was before CLI-024 — serves no reader. Recording today's header as intended would document a defect that CLI-024 already fixed for the other caller of the same diff generator.

### One statement of the rule

The trace names the file by the same rule, and through the same code, as `--dry-run`, so the two headers cannot drift apart; the rule is stated once, in CLI-024. The trace adds no second statement of when a file is inside the current directory.

### The issue's goals this change does not carry out

Two goals of the issue are dropped. Its alternative outcome, a statement in the trace command's spec that the trace keeps `a/<path>`, has no place: the trace has no spec (below), and the change takes the other outcome. Its constraint that a change to full paths leave the header of relative paths as it is rests on a wrong premise: the existing test it names passes a full path in a temporary directory, not a relative path, and asserts only that the header starts with `a/`. A relative path without `.` or `..` segments keeps its header (example 2); a relative path with them changes, and the existing test is updated, because its full path lies outside the test process's current directory (example 3). The goal that a test covers the header for a full path is kept.

### No spec

The trace has no spec: `sdd/specs/cli/reihitsu-format.md` excludes it as a non-goal, and it is repository tooling. CLI-024 stays a requirement of `reihitsu-format` only and does not name the trace. `scripts/README.md` is the trace's documentation and states the header; a test in `Reihitsu.Cli.Test` covers it for a full path inside the current directory, a file outside it, and a relative path.

## Alternatives considered

- **Keep `a/<path as given>` and record it as a decision** — rejected: see *Decisions*; the header would stay broken for a full path, and the same file would get different headers depending on how it was named.
- **Drop only the doubled slash** (`--- a/work/src/A.cs`) — rejected: the path behind `a/` would still be a full path without its root, which names no file relative to any directory a patch tool runs in, and it would be a third header rule next to CLI-024 and today's.
- **Always print the full path without prefixes** — rejected: the trace would differ from `--dry-run` for every file inside the current directory, the common case, for which `--- a/src/A.cs` is the header today.
