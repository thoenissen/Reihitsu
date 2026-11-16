Role: spec author
Commit: 668ca35
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1795 and the maintainer's decision comment on it (parallel processing, a new option for the degree, output/exit codes/summary unchanged with results in file order, name/default/documentation asked in the run), `00-lead.md`, `sdd/specs/cli/reihitsu-format.md`, `src/Reihitsu.Cli/Program.cs`, `FormatCommandHandler.cs`, `README.MD`.

Files written:

- `sdd/changes/2026-11/parallel-cli-formatting.md` — change doc, `Status: Draft`, six visible output changes.
- `sdd/specs/cli/reihitsu-format.md` — CLI-001, CLI-002 changed; CLI-022, CLI-023 added; three non-goals added.
- `sdd/specs/requirement-ids.txt` — CLI-022, CLI-023.
- `src/Reihitsu.Cli/README.MD` — options-table row and one example for `--max-parallelism`.

## Changed requirements

- CLI-001 — Arguments are options and paths — changed: `--max-parallelism <count>` added to the accepted options.
- CLI-002 — Invalid arguments fail before any file is read — changed: two new errors, `Invalid value for --max-parallelism: '<value>'. Expected a whole number of 1 or more.` and `Missing value for --max-parallelism.`
- CLI-022 — Files are processed in parallel, and the result is the same as one after another — added. Same lines on both streams in the same order as one-after-another processing in selection order (each file's lines after every earlier file's and before every later file's, a file's lines keeping their order across both streams), same files written, same summary and exit code; a failing/undecodable/refused file does not affect others; the CLI-018 confirmation still comes before any file is read; on Ctrl+C no further file starts, every finished file's lines are written in selection order, then `Operation canceled.`, exit 2, no summary.
- CLI-023 — `--max-parallelism` sets how many files are processed at the same time — added. At most `<count>` files at once; `1` is one after another; default `Environment.ProcessorCount`; counts larger than files or processors accepted; repeated option: the last valid count; usage text lines quoted.

`scripts/test.sh --project architecture`: 152 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` names CLI-022 and CLI-023 as uncovered. That is expected until the test implementer tags tests `[Requirement("CLI-022")]` / `[Requirement("CLI-023")]`; no other spec-structure test failed.

Checked against today's CLI (`scripts/format.sh --check --verbose A.cs B.cs C.cs`, A/C with `int  x;`, B with a missing `;`): output and exit code 1 match the CLI-022 example exactly. The CLI-023 `0` example's argument error is new behavior (today: `Unknown option: --max-parallelism`).

## Examples for the tests

- CLI-022 — the three-file `--check --verbose` example (order must hold when a later file finishes first, so a test needs a formatter/file system fake that completes files out of order); the example with an unsupported `u.cs` between two unformatted files (the `error:` line on standard error between the two `Not formatted:` lines; exit 2); the cancellation paragraph (no summary, finished files named in order, `Operation canceled.`, exit 2); the CLI-018 confirmation still before any read.
- CLI-023 — `--max-parallelism 2` equals the default run's output; `0`, `-1`, `two`, `1.5` give the invalid-value error with exit 2; a trailing `--max-parallelism` gives the missing-value error; the usage text lines; `1` processes at most one file at a time; the default equals `Environment.ProcessorCount`.
- CLI-001, CLI-002 — the parser accepts the option anywhere among paths; the error messages above.

## Output questions

**1 · reihitsu-format · name of the new option**

Input:
```shell
reihitsu-format --check <name> 4 src/
```

Before (`main`): `Unknown option: <name>`, usage text, exit code 2.

After, A (recommended): `--max-parallelism 4` — checks `src/` with at most four files at the same time.

After, B: `--jobs 4`, also `-j 4` — same behavior.

After, C: `--parallel 4` — same behavior.

Why A: it says that the count is an upper bound, follows .NET's own term (`MaxDegreeOfParallelism`), and keeps the tool's long-option-only style (`-h` is its only short option).

**2 · reihitsu-format · default degree of parallelism**

Input:
```shell
reihitsu-format --check src/
```

Before (`main`): one file after another.

After, A (recommended): as many files at the same time as the machine has logical processors; same output, summary, exit code.

After, B: one file after another unless `--max-parallelism` is given.

Why A: the issue asks the existing CI commands to get faster with output unchanged; B leaves every existing invocation as slow as today.

**3 · reihitsu-format · documentation of the option**

Input: `reihitsu-format --help`, and the options table of the CLI's README.

Before (`main`): no entry.

After, A (recommended) — help:
```text
  --max-parallelism <count>
               Maximum number of files processed at the same time (default: the number of logical processors)
```
README row: "Maximum number of files processed at the same time (default: the number of logical processors). Use `1` to process one file after another. The output, its order, and the exit code are the same for every count. A value that is not a whole number of `1` or more fails with exit code `2`." plus the example `reihitsu-format --check --max-parallelism 2 src/` ("for example on a shared build machine").

After, B — the same help lines; README row only "Maximum number of files processed at the same time (default: the number of logical processors).", no example.

Why A: the sentence on unchanged output answers the first question a CI user has about turning parallelism on, and `1` is the documented way back to today's behavior.

**4 · reihitsu-format · Ctrl+C while several files are in progress**

Input: Ctrl+C during `reihitsu-format --force big/` while files later in selection order are already finished and an earlier one is still in progress.

Before (`main`): only one file is ever in progress; every finished file is named, then `Operation canceled.`, exit 2, no summary.

After, A (recommended): no further file is started; every finished file is named, in selection order, also when an earlier file was unfinished; then `Operation canceled.`, exit 2, no summary.

After, B: only the finished files in front of the first unfinished one are named; then `Operation canceled.`, exit 2.

Why A: with B a file can be rewritten without the user being told; A keeps today's property that every changed file is named. CLI-022 is written with A.

**5 · reihitsu-format · a write interrupted by Ctrl+C**

Input: Ctrl+C while `reihitsu-format --force big/` writes a large file.

Before (`main`): `FormatCommandHandler.HandleChangedFileAsync` passes the run's cancellation token to `IFileSystem.WriteAllTextAsync`, which `DefaultFileSystem` forwards to `File.WriteAllTextAsync`; that opens the file truncated and writes it in chunks, checking the token between them. Established by reading the code, not reproduced: a cancellation between chunks can leave the file truncated. With one file in progress the window is one write; with up to `<count>` files in progress it is up to `<count>` writes.

After, A (recommended): a write that has started is completed before the run ends; the file holds the whole formatted text.

After, B: unchanged — a canceled write may leave a truncated file.

Why A: a truncated file loses user code, which the tool must never do; the parallel change widens an existing window. The specs say nothing about it until answered. If the lead treats this as outside the run (it exists on `main`), it becomes a follow-up candidate in this form; it is a safety-rail case either way (escalation 2 territory: alters user code).

## Internal choices

- Error messages for an invalid or missing count mirror the `--lang-version` messages; alternative: a range message such as `must be between 1 and N`.
- No upper bound for the count; a count above the processor or file count is accepted. Alternative: cap at `Environment.ProcessorCount` with a warning.
- A repeated option uses the last valid count, as `--lang-version` does in the parser today. Alternative: reject repetition.
- The ordering rule is stated once in CLI-022 against "one after another in selection order", rather than amending CLI-011 to CLI-016 one by one.
- Cancellation stays in CLI-017; CLI-022 adds only what parallelism changes about it. "No summary on cancellation" states today's behavior (the exception passes `PrintSummary`).
- Non-goals added: directory enumeration order is not sorted; speed, core count, and memory are not specified; no parallelism inside one file.
- The repository README (`README.MD`) is not changed: it only shows `reihitsu-format src/`.
- The code-owner list of the CLI spec is unchanged; no new type is named, since none exists yet. If the implementation adds one that owns CLI-022, the spec checker should see it added to `Code owners` and `sdd/specs/README.md`.

## Follow-up candidates

- The issue's other starting points — profiling the per-file pipeline for repeated work, and a benchmark against a fixed corpus for a regression between 1.2.0 and 1.3.0 — are not asked for by the maintainer's decision and change no visible output. Candidate follow-up: "Profile reihitsu-format's per-file formatting time on a fixed corpus", tooling/performance, no output question.
- Output question 5, if the lead rules it outside the run.
