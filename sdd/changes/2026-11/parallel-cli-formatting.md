# reihitsu-format processes files in parallel

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | CLI-001, CLI-002, CLI-006, CLI-017, CLI-022 (new), CLI-023 (new) |

## What

`reihitsu-format` processes several files at the same time instead of one after another (CLI-022). The new option `--max-parallelism <count>` sets how many files it processes at the same time; without it, the CLI uses the number of logical processors (CLI-023). `1` processes one file after another.

The result does not depend on the count or on which file finishes first: standard output and standard error carry the same lines in the same order as a run that processes the files one after another in the order it selects them, the same files are written with the same content, and the summary and the exit code are unchanged (CLI-022). The confirmation of a large write run is still asked before any file is read for formatting (CLI-018). A run canceled with Ctrl+C starts no further file, completes every write that has started, names every file it finished, in selection order, and then ends with `Operation canceled.` and exit code 2. One case is exempt from the unchanged output: in write mode, when two selected paths name one physical file through a link, which of them is named `Formatted:` and the summary's count may vary between runs (CLI-006, CLI-022).

An invalid or missing count fails before any file is read, with exit code 2, like an invalid `--lang-version` (CLI-002, CLI-023).

## Why

thoenissen/Reihitsu#1795 reports `reihitsu-format --check` taking 1–3 minutes on code bases of 450–930 files in CI, about 0.12–0.34 s per file, with one processor core in use. Each file is formatted on its own, so the run scales with the cores of the machine when files are formatted at the same time. The maintainer decided on the issue that the CLI formats files in parallel, that the degree is configurable through a new option, that output, exit codes, and the summary stay as they are with results printed in file order, and that the option's name, default, and documentation are decided in this change.

## Visible output changes

### 1. A new option, `--max-parallelism <count>`

```shell
reihitsu-format --check --max-parallelism 2 src/
```

Before: `Unknown option: --max-parallelism`, the usage text on standard error, exit code 2.

After: `src/` is checked with at most two files processed at the same time; the output and the exit code are the ones `reihitsu-format --check src/` gives.

### 2. Default: as many files at the same time as the machine has logical processors

```shell
reihitsu-format --check src/
```

Before: one file after another.

After: up to `Environment.ProcessorCount` files at the same time. Output, written files, summary, and exit code are unchanged, in the same order — for example, with `A.cs` and `C.cs` unformatted and `B.cs` holding a syntax error, `reihitsu-format --check --verbose /work/A.cs /work/B.cs /work/C.cs` writes, before and after:

```text
Not formatted: /work/A.cs
Skipped (syntax errors): /work/B.cs
Not formatted: /work/C.cs

2 of 3 file(s) need formatting.
Skipped 1 file(s) with syntax errors.
```

and exits with code 1.

### 3. Invalid and missing counts

```shell
reihitsu-format --check --max-parallelism 0 src/
```

Before: `Unknown option: --max-parallelism` (and `0` taken as a path), exit code 2.

After, on standard error, followed by the usage text, exit code 2:

```text
Invalid value for --max-parallelism: '0'. Expected a whole number of 1 or more.
```

The same for `-1`, `+2`, ` 2`, `two`, `1.5`, and `99999999999`: only decimal digits, from 1 to 2147483647, are accepted, with no upper bound below that — `--max-parallelism 1000` is used as given. `reihitsu-format --check src/ --max-parallelism` writes `Missing value for --max-parallelism.` instead. `--max-parallelism 2 --max-parallelism 8` uses 8.

### 4. Usage text

The help gains, between `--lang-version` and `--help`:

```text
  --max-parallelism <count>
               Maximum number of files processed at the same time (default: the number of logical processors)
```

### 5. The CLI's README

`src/Reihitsu.Cli/README.MD` gains a row in the options table and an example (`reihitsu-format --check --max-parallelism 2 src/`).

### 6. Cancellation with Ctrl+C

Before: the file being processed is interrupted, every finished file has been named, `Operation canceled.`, exit code 2.

After: up to `<count>` files are being processed when Ctrl+C is pressed; no further file is started, every finished file is named in selection order — also when a file selected before it is unfinished — then `Operation canceled.`, exit code 2, no summary. Which files are finished depends on timing, as before.

### 7. A write interrupted by Ctrl+C

```shell
reihitsu-format --force big/
```

Before: Ctrl+C while a large file is written cancels the write between chunks and can leave the file truncated (reproduced with the API the CLI calls: a 50,000,000-character write canceled after 5 ms left 5,406,720 bytes).

After: a write that has started completes; the file holds the whole formatted text and is named like every other finished file (CLI-022, CLI-017).

### 8. One file reached under several paths, in write mode

A directory `w` with `real/A.cs` holding `int  x;`, `link` a symbolic link to `real`, and `B.cs` a hard link to `real/A.cs`:

```shell
reihitsu-format --force --verbose w
```

Before:

```text
Formatted: w/B.cs
Unchanged: w/real/A.cs
Unchanged: w/link/A.cs

Formatted 1 of 3 file(s).
```

After: between one and three of the paths are named `Formatted:` and the others `Unchanged:`, and the summary reads `Formatted 1 of 3` to `Formatted 3 of 3`, varying between runs; the file holds the formatted text either way. `--check` and `--dry-run` print what they print today. CLI-006 now states that it deduplicates by full path, which is what it does on `main`.

## Decisions

### The result is independent of the degree of parallelism

Output is buffered per file and written in selection order, so a run with any count reads exactly like a run with `1`. This keeps CI logs and diffs of logs stable, and it is what the maintainer's decision on the issue asks for.

### The option is named `--max-parallelism <count>`

Maintainer's answer (option A of three: `--max-parallelism`, `--jobs`/`-j`, `--parallel`). The name says that the count is an upper bound, follows .NET's `MaxDegreeOfParallelism`, and keeps the tool's long-option-only style (CLI-001, CLI-023).

### The default is the number of logical processors

Maintainer's answer (option A; the alternative was one file after another unless the option is given). Existing CI commands get faster without being changed (CLI-023).

### Documentation: help lines, a full README row, and an example

Maintainer's answer (option A; the alternative was the bare help sentence in the README with no example). The README row says that `1` processes one file after another, that output, its order, and the exit code are the same for every count, and that an invalid value fails with exit code 2, and the README gains `reihitsu-format --check --max-parallelism 2 src/`. The sentence on unchanged output does not mention the link case below; it is a corner case the README need not list.

### Ctrl+C names every finished file, in selection order

Maintainer's answer (option A; the alternative named only the finished files in front of the first unfinished one, so a rewritten file could go unnamed). No file is changed without the user being told (CLI-022).

### A started write completes on Ctrl+C

Not asked: the only alternative — a canceled write may leave a truncated file — loses user code, which the tool must never do. The defect exists on `main` with one write in flight; parallel processing widens it to up to `<count>` writes, so it is repaired in this change. A file whose write has started counts as finished and is named (CLI-022, CLI-017).

### Several paths to one physical file may report differently in write mode

Maintainer's answer (option A; the alternatives were processing each physical file once, which also changes `--check` and `--dry-run` to count it once, and keeping today's output exactly). Both alternatives need the tool to recognize that two paths name one file, which hard links make costly on every platform, for a rare input; the content is right either way and `--check` is unchanged. CLI-006 is reworded to "the same full path", which is what it deduplicates by today; processing one physical file once is left to a change of its own.

### Accepted counts: decimal digits, 1 to 2147483647, the last one wins

Maintainer's answer (option A; the alternative capped the count at the number of logical processors). A count above the processors or files only costs threads, not a wrong result. The messages and the last-value-wins rule mirror `--lang-version` (CLI-002, CLI-023).

## Alternatives considered

- **Profiling and speeding up the per-file pipeline** (the issue's second starting point) — not part of this change; it changes no visible output and is independent of parallel processing.
- **Printing each file's lines as soon as it finishes** — rejected: the order would depend on timing, so two runs on the same tree would print different logs; the maintainer asked for file order.
- **Parallel processing without an option** — rejected by the maintainer's decision: the degree has to be configurable.
- **Deduplicating one physical file reached by several paths** — rejected for this change by the maintainer's answer; it needs a file-identity check .NET does not offer on every platform.
- **Capping `--max-parallelism` at the number of logical processors** — rejected by the maintainer's answer.
- **Splitting the check across CI jobs or checking only changed files** — the workarounds the issue names; they leave full runs slow and add scripting to every consumer.
