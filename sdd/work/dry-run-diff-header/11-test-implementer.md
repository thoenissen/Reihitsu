Role: test implementer
Commit: d064980 (tests written on this head; the commit that adds this handover follows it)
Verdict: reproduced

Repair round 2, for finding 1 of `10-auditor.md` (FC-016): `--dry-run` with a full path exits 2 when the process's current directory has been deleted. Inputs: `00-lead.md`, `04-test-implementer.md`, `10-auditor.md`, CLI-013, CLI-017, and CLI-024 in `sdd/specs/cli/reihitsu-format.md`. The prompt carried no intended fix, cause, or hoped-for verdict.

Expected values are derived from the spec, not from the auditor's suggested change. A deleted directory contains no file, so no existing file's full path passes through it, and CLI-024's "any other file" bullet applies: `--- <full path>` and `+++ <full path>`, as the `Would format:` line prints it. CLI-013 and CLI-017 then require the diff, the summary, and exit code 1, and CLI-016's `Error processing` must not appear, so standard error is empty.

Both tests run through `Program.Main` in one new helper, `RunInDeletedDirectory`, which creates a directory, makes it current, deletes it, asserts the precondition that `Directory.GetCurrentDirectory()` now throws an `IOException`, and restores the previous current directory afterwards. Both compare the whole standard output line by line (FC-017).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FullPathToAFileWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader` (Linux only) | CLI-013, CLI-024 | `RunInDeletedDirectory` + `AssertWholeOutput` | fails: exit code 2, expected 1 |
| `FullPathToADirectoryWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader` (Linux only) | CLI-013, CLI-024 | same | fails: exit code 2, expected 1 |

Both are in `src/Reihitsu.Cli.Test/EndToEnd/CliDryRunDiffHeaderEndToEndTests.cs`. The precondition held in both runs, so each fails on the exit code, the first assertion behind it. The auditor observed the rest of the difference: `Error processing <path>: Unable to find the specified file.` on standard error, and no diff.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Current directory deleted, `--dry-run <full path to file>` | LF | CLI end to end | reproduced | exit code 2, expected 1 |
| Current directory deleted, `--dry-run <full path to directory>` | LF | CLI end to end | reproduced | exit code 2, expected 1 |

## Not covered, and why

- The file and the directory arms reach the same call site, `IDiffGenerator.Generate` from `HandleChangedFileAsync`. Both are kept because the auditor named both as failing and each costs one small test.
- `--check` and write mode with a deleted current directory: they do not call the diff generator, and the auditor observed them unchanged from `main`. A relative path cannot be resolved on `main` either, so it is not PR-introduced.
- Windows does not let a process's current directory be deleted, so the tests are `[OSCondition(Linux)]`. macOS with an unreadable parent (`EACCES`) is untraced and not testable portably. `rmdir` of the current directory also works on macOS, but whether .NET's `getcwd` fails there is not verified, so the tests are not enabled for it.
- Line endings: the header is written from the path alone, as stated in `04-test-implementer.md`.
- The formatter, analyzer, code-fix, rename, and precheck failure classes do not apply, as stated in `04-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliDryRunDiffHeaderEndToEndTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~WhileTheCurrentDirectoryIsDeleted"
```

Results on this head:

- CLI project: 438 pass, 1 skipped (the Windows test), and the 2 new tests fail.
- Architecture project: 198 pass, including requirement coverage.

## Follow-up candidates

_None._
