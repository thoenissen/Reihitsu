Role: skeptic
Commit: 987ba16
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict; its pointer to check shared state in the formatter and CLI is a question, and is answered below. Inputs: thoenissen/Reihitsu#1795 and the maintainer's decision comment on it, `00-lead.md`, `01-spec-author.md`, `sdd/changes/2026-11/parallel-cli-formatting.md`, the spec diff of `sdd/specs/cli/reihitsu-format.md` (`git diff origin/main`), `src/Reihitsu.Cli/Program.cs`, `FormatCommandHandler.cs`, `Abstractions/Default*.cs`, `Diff/LcsComputer.cs`, the CLI test helpers, and every static field in `src/Reihitsu.Formatter`, `src/Reihitsu.Core`, `src/Reihitsu.Cli`.

## Premise

Real. `FormatCommandHandler.ProcessFilesAsync` formats the files one after another in a `foreach` with `await` on each file. Measured with the Release build on this branch's base (4 logical processors):

```shell
time dotnet src/Reihitsu.Cli/bin/Release/net11.0/Reihitsu.Cli.dll --check src/Reihitsu.Formatter src/Reihitsu.Analyzer
```

```text
0 of 550 file(s) need formatting.
real 0m15.979s   user 0m21.216s   sys 0m0.322s
```

CPU time is about 1.3 times wall time: one core does the formatting, the rest is JIT and GC. A caller reaches the case: every `reihitsu-format` run through the CLI (`Program.Main` → `FormatCommandHandler.ExecuteAsync`). The issue has no `agent-filed` label; its goal (faster with output, exit codes, and summary unchanged) and the maintainer's decision comment are binding, and the spec diff follows both.

Shared state that concurrent formatting could break — checked by listing every `static` field in the three projects:

- `Reihitsu.Formatter`: `LineStartIndex._indexes` is a `ConditionalWeakTable<SyntaxTree, LineStartIndex>` (thread-safe; keyed per tree, and each file has its own tree; the instance's two dictionaries are filled only in the constructor and only read afterwards); `SpacingPolicy._rules` is a `static readonly ISpacingRule[]` whose rule classes have no instance fields; two `static readonly SyntaxTrivia _singleSpace` and one `ImmutableArray<string>` are immutable. No non-readonly static field, no `[ThreadStatic]`, `ThreadLocal`, pool, or lock exists in the formatter or in `Reihitsu.Core`.
- `Reihitsu.Cli`: `FormatCommandHandler._utf8BomEncoding` (immutable `UTF8Encoding`), `DiffGenerator`/`LcsComputer` (static, stateless), `DefaultFileSystem`, `DefaultSourceFormatter`, `DefaultDiffGenerator` (no fields). `DefaultConsoleOutput` calls `Console.WriteLine`/`Console.Error.WriteLine` directly, so lines written from several workers would interleave in completion order — the ordering in CLI-022 cannot come from the console abstraction as it is.
- The counters in `ProcessFilesAsync` are locals incremented in the loop; they are shared state once the loop runs concurrently.

No formatter or Core change is needed for thread safety.

## Mechanism and candidates

_N/A_ — a behavior change, not a bug.

## Breaking inputs

**Class 1 — two selected paths name one physical file (aliases).** CLI-006 deduplicates by full path only (`CollectFiles`, `HashSet<string>` with `Ordinal` on Linux and macOS, `OrdinalIgnoreCase` on Windows). `Directory.EnumerateFiles(..., AllDirectories)` follows a symlinked directory. Fixture (`w/real/A.cs` = `class A\n{\n    int  x;\n}\n`, `w/link -> real`, `w/B.cs` a hard link to `w/real/A.cs`):

```shell
scripts/format.sh --check --verbose <tmp>/w
```

```text
Not formatted: <tmp>/w/B.cs
Not formatted: <tmp>/w/real/A.cs
Not formatted: <tmp>/w/link/A.cs

3 of 3 file(s) need formatting.
```

```shell
scripts/format.sh --force --verbose <tmp>/w
```

```text
Formatted: <tmp>/w/B.cs
Unchanged: <tmp>/w/real/A.cs
Unchanged: <tmp>/w/link/A.cs

Formatted 1 of 3 file(s).
```

Today the second and third path read the file the first one wrote. Processed in parallel, each of the three can read the original before another write lands, so write mode names one to three of them `Formatted:` and the summary says `Formatted 1 of 3` to `Formatted 3 of 3`, by timing. That breaks CLI-022's MUST ("the same lines … the same summary") for this input. The class matrix:

| Member | `--check` | `--dry-run` | write mode |
|---|---|---|---|
| symlinked directory below a given directory (reproduced above) | same as today (no write) | same as today (no write) | timing-dependent lines and count |
| file symlink below a given directory | same | same | timing-dependent |
| hard link (reproduced above) | same | same | timing-dependent |
| one file given under two spellings on a case-insensitive file system with an `Ordinal` comparer (macOS) | same | same | timing-dependent |

Content is not at risk on Linux: a console program running `File.WriteAllTextAsync` of the same 2,000,000-character text to one path from four tasks, 50 rounds, ended with no exception and the full text every round (`errors=0`, no `CORRUPT` line). Windows was not run: `File.WriteAllTextAsync` opens for writing while sharing only reads, so a concurrent second open there may fail with a sharing violation and print `Error processing …` with exit code 2. That is unverified and must not be stated as fact. The class is rare (links inside a source tree), affects write mode only, and leaves `--check` — the issue's CI case — unchanged. It is output question 6 below.

**Class 2 — a canceled write leaves a truncated file** (the spec author's question 5). Reproduced on Linux with the API the CLI calls: `File.WriteAllTextAsync(path, <50,000,000 characters>, UTF-8, token)`, token canceled after 5 ms → `OperationCanceledException`, file length `5406720`. `DefaultFileSystem.WriteAllTextAsync` forwards the run's token unchanged (`HandleChangedFileAsync` → `IFileSystem.WriteAllTextAsync`), so Ctrl+C during a write can truncate a user's source file **on `main` today**. Parallel processing multiplies the window from one write to up to `<count>` writes. This is not an output question (below): no one can pick "may truncate a file". It is a pre-existing defect the change newly widens, so it reaches the run (`squad/README.md`, *Architecture findings*, scope paragraph), and it is a safety-rail case (escalation 2) if the run does not repair it.

**Class 3 — per-file resource use multiplied by the degree.** `LcsComputer.ComputeTable` allocates `int[n+1, m+1]` per changed file in `--dry-run` (a 5,000-line file ≈ 100 MB, 10,000 lines ≈ 400 MB). With `<count>` large changed files diffed at once, an `OutOfMemoryException` is caught by `ProcessFileAsync`'s `catch (Exception)` and printed as `Error processing …`, exit 2 — a result that depends on the degree and on timing. Rare (multi-thousand-line files that need formatting, in `--dry-run`); the spec already names memory a non-goal. No change in this run recommended; follow-up candidate below.

**Candidate inputs the test implementer must cover** (failure classes FC-017, FC-033, FC-003):

- Out-of-order completion: the first selected file finishes last (a fake `IFileSystem`/`ISourceFormatter` that holds file 1 until file 3 has finished). A fake that completes in selection order lets an implementation that prints in completion order pass (FC-033).
- The name-based generated skip: `Skipped (generated): <path>` for a `*.g.cs` / `*.Designer.cs` path is written by `ProcessFilesAsync` itself, outside `ProcessFileAsync`. A `--verbose` run with `A.cs`, `X.g.cs`, `C.cs` must print the three lines in selection order — an ordering that covers only `ProcessFileAsync`'s lines misses this one.
- Cross-stream order: `A.cs` (stdout `Not formatted:`), `B.cs` undecodable or throwing in the formatter, or `u.cs` unsupported (stderr), `C.cs` (stdout). `CapturedConsoleOutput` (`src/Reihitsu.Cli.Test/Helpers/CapturedConsoleOutput.cs`) keeps two separate lists and cannot show the order across streams; CLI-022 needs a capture that records one interleaved sequence. Today no single file writes to both streams, so "a file's lines keep their order across both streams" has no input that exercises it; state it, don't build a test around it.
- Concurrency bound: `--max-parallelism 1` never has two files in progress; `--max-parallelism 2` reaches two and never three (FC-003: both sides of the boundary), counted by a fake that records in-flight calls.
- A failing file (formatter throws) between two others: both others are processed and named, exit 2.
- Cancellation: file 1 in progress, file 2 finished → `file 2`'s line, then `Operation canceled.`, exit 2, no summary; no file started after the cancel.
- CLI-018: no `ReadFileAsync` call before the confirmation is answered.
- Parsing: `0`, `-1`, `two`, `1.5`, a trailing `--max-parallelism`, a repeated option, the option after `--` (a path then, by CLI-001), and `--max-parallelism --check` (the value is `--check` and is invalid, as `--lang-version --check` is today — the parser consumes the next argument as the value).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Program.ParseArguments`, `PrintUsage`, `ParseResult` | must change (option, errors, usage text) |
| `FormatCommandHandler` (`ProcessFilesAsync`, counters, output order, cancellation) | must change |
| `IConsoleOutput` / `DefaultConsoleOutput` | must stay as the sink; lines must reach it in selection order, which the console itself does not give |
| `IFileSystem` / `DefaultFileSystem` | must stay, except as the lead decides for class 2 (the canceled write) |
| `Reihitsu.Formatter`, `Reihitsu.Core` | not involved — no shared mutable state (see *Premise*) |
| Analyzers, code fixes, Fix All, rule docs | not involved |
| `src/Reihitsu.Cli/README.MD`, spec, change doc | must change (written) |
| CLI tests (unit, integration, end-to-end), `CapturedConsoleOutput` | must change: an interleaved capture for cross-stream order; existing tests must still pass when the default degree is above 1 |
| `scripts/trace.sh` / `PipelineTraceCommand` | not involved (spec non-goal) |

## Refuse option

The issue as a whole cannot be refused: the maintainer decided parallel processing, so "no change" is off the table. A narrower variant — parallel `--check` and `--dry-run`, write mode one after another — would sidestep class 1 entirely and halve class 2, but departs from the issue's goal ("both `--check` and normal formatting") and from the maintainer's decision; it is not recommended and not asked.

For class 1 (aliases), the refuse option is to carve the class out of CLI-022's equality: the file's content is still right, but which alias is named and the count may vary between runs. Cost: one sentence in the spec, no production code. The alternative — detecting that two paths name one file — needs a symlink-resolving pass for links and a file-identity (inode / file-index) check for hard links, which .NET does not expose cross-platform; that is the machinery *Special cases do not grow the source* rules out for a rare class. Declaring a shape unsupported does not apply: no formatter shape is involved.

For class 2, refusing is not an option (non-destructiveness comes first).

## Output questions

**On the drafted questions.**

- Q1 (name), Q2 (default), Q4 (Ctrl+C with several files in progress): complete, options right, recommendations sound.
- Q3 (documentation): the proposed README sentence "The output, its order, and the exit code are the same for every count" holds only if Q6 is answered B or C; with Q6 A it is still fine for users — aliases are a corner case the README need not list — but the lead should know the sentence is not literally true then.
- Q5 (interrupted write) is not an output question. Its premise reproduces (class 2), it exists on `main`, and option B ("a canceled write may leave a truncated file") is an output that violates non-destructiveness, so no maintainer choice is needed. Recommendation: the lead treats it as a defect the run's change widens — repaired in this run if it meets *Scope* (small, in `DefaultFileSystem`/`FormatCommandHandler`, testable with a fake that records the token passed to the write), otherwise escalation 2. CLI-022's cancellation paragraph then has to say that a file whose write has started is finished, and named.
- Missing: visible output change 3 of the change doc (invalid and missing counts), and two behaviors the spec author filed under *Internal choices* although they are CLI behavior: no upper bound on the count, and the last of several counts wins. Every change the change doc lists is in question until answered (`squad/README.md`, *What reaches the maintainer*). Asked as Q7 below.
- Missing: aliases in write mode (class 1). Asked as Q6 below.

**6 · reihitsu-format · one file reached under several paths, in write mode**

Input: a directory `w` with `real/A.cs` holding `int  x;`, `link` a symbolic link to `real`, and `B.cs` a hard link to `real/A.cs`.

```shell
reihitsu-format --force --verbose w
```

Before (`main`):
```text
Formatted: w/B.cs
Unchanged: w/real/A.cs
Unchanged: w/link/A.cs

Formatted 1 of 3 file(s).
```

After, A (recommended): between one and three of the paths are named `Formatted:` and the others `Unchanged:`, and the summary reads `Formatted 1 of 3` to `Formatted 3 of 3` — it varies between runs; the file's content is the formatted text either way. `--check` and `--dry-run` print what they print today.

After, B: each physical file is processed once, under the first path that selects it:
```text
Formatted: w/B.cs

Formatted 1 of 1 file(s).
```
(also changes `--check` and `--dry-run` to `1 of 1`.)

After, C: exactly today's output, in parallel runs too.

Why A: links inside a source tree are rare, the case only arises when writing, and B and C both need the tool to recognize that two paths are one file, which hard links make costly to do on every platform.

**7 · reihitsu-format · which `--max-parallelism` values are accepted**

Input:
```shell
reihitsu-format --check --max-parallelism <value> src/
```

Before (`main`): `Unknown option: --max-parallelism`, usage text, exit code 2, for every value.

After, A (recommended): a value of decimal digits only, from `1` up to the largest 32-bit integer, is accepted — `1`, `4`, `64`, `1000` (a count above the processors or files is used as given). `0`, `-1`, `+2`, ` 2`, `two`, `1.5`, `99999999999` give

```text
Invalid value for --max-parallelism: '<value>'. Expected a whole number of 1 or more.
```

with the usage text and exit code 2; a trailing `--max-parallelism` gives `Missing value for --max-parallelism.`; `--max-parallelism 2 --max-parallelism 8` uses 8.

After, B: as A, but a count above the number of logical processors is an error: `Invalid value for --max-parallelism: '64'. Expected a whole number from 1 to 4.`

Why A: it mirrors `--lang-version` (last value wins, same message shape), and an oversized count only costs the user threads, not a wrong result.

## Internal choices

- The spec author's ordering rule stated once in CLI-022 against "one after another in selection order", instead of amending CLI-011 to CLI-016: agree.
- "No summary on cancellation" as a statement of today's behavior: agree (the `OperationCanceledException` passes `PrintSummary` in `ExecuteAsync`).
- CLI-006's "A file selected more than once — given directly and through a directory — MUST be processed once" reads as "the same physical file", but the code deduplicates by full path (class 1 shows the same file processed three times on `main`). Recommendation: reword it to "the same full path" in this run — it states today's behavior and changes no output — whatever Q6's answer.
- The error-message shape, the upper bound, and repetition are not internal; moved to Q7.

## Issue goals

_N/A_ — the issue is not `agent-filed`. Its goals and the maintainer's decision are met by the spec diff; the issue's other two starting points (profiling, a version benchmark) are outside the decision and already a follow-up candidate.

## Architecture findings

_None._

## Follow-up candidates

- Profile the per-file formatter pipeline and benchmark versions on a fixed corpus (already in `00-lead.md`).
- `--dry-run` diff memory: `LcsComputer.ComputeTable` allocates `(n+1)·(m+1)` integers per changed file (400 MB for a 10,000-line file), and parallel processing multiplies it by the degree. Pre-existing; no output question; a linear-space diff would remove it.
- If Q6 is answered A: whether CLI-006 should process one physical file once (Q6's option B) as a change of its own — an output question in that follow-up.
- Windows behavior of two concurrent writes to one file (class 1, not run): if a sharing violation is confirmed there, it belongs to the Q6 follow-up.
