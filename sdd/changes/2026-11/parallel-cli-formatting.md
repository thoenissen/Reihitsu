# reihitsu-format processes files in parallel

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | CLI-001, CLI-002, CLI-022 (new), CLI-023 (new) |

## What

`reihitsu-format` processes several files at the same time instead of one after another (CLI-022). The new option `--max-parallelism <count>` sets how many files it processes at the same time; without it, the CLI uses the number of logical processors (CLI-023). `1` processes one file after another.

The result does not depend on the count or on which file finishes first: standard output and standard error carry the same lines in the same order as a run that processes the files one after another in the order it selects them, the same files are written with the same content, and the summary and the exit code are unchanged (CLI-022). The confirmation of a large write run is still asked before any file is read for formatting (CLI-018). A run canceled with Ctrl+C starts no further file, names every file it finished, in selection order, and then ends with `Operation canceled.` and exit code 2, as today.

An invalid or missing count fails before any file is read, with exit code 2, like an invalid `--lang-version` (CLI-002).

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

`reihitsu-format --check src/ --max-parallelism` writes `Missing value for --max-parallelism.` instead.

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

After: up to `<count>` files are being processed when Ctrl+C is pressed; no further file is started, every finished file is named in selection order, then `Operation canceled.`, exit code 2. Which files are finished depends on timing, as before.

## Decisions

### The result is independent of the degree of parallelism

Output is buffered per file and written in selection order, so a run with any count reads exactly like a run with `1`. This keeps CI logs and diffs of logs stable, and it is what the maintainer's decision on the issue asks for.

### The option's name, default, and documentation

Open: asked as output questions in this run. The specs are written with the recommendation — `--max-parallelism <count>`, defaulting to the number of logical processors — and change with the answer.

## Alternatives considered

- **Profiling and speeding up the per-file pipeline** (the issue's second starting point) — not part of this change; it changes no visible output and is independent of parallel processing.
- **Printing each file's lines as soon as it finishes** — rejected: the order would depend on timing, so two runs on the same tree would print different logs; the maintainer asked for file order.
- **Parallel processing without an option** — rejected by the maintainer's decision: the degree has to be configurable.
- **Splitting the check across CI jobs or checking only changed files** — the workarounds the issue names; they leave full runs slow and add scripting to every consumer.
