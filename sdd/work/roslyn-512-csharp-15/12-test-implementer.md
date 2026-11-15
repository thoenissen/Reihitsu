Role: test implementer
Commit: d4c7b90
Verdict: tests written

Repair round 1 (00-lead answer 5A, decision 6; auditor 09 finding #1). The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Input and expected outputs are taken verbatim from the change doc `sdd/changes/2026-11/roslyn-512-csharp-15.md`, entry "CLI, a top-level statement that starts with a local named `union`" (FC-035). Production code is unchanged; both tests pass on the head, as expected for already-decided behavior.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Cli.Test/EndToEnd/CliLanguageVersionEndToEndTests.TopLevelStatementStartingWithUnionLocalIsSkippedWithoutLangVersion`: change-doc input, no `--lang-version`; exit 0, empty stderr, exact summary `Formatted 0 of 1 file(s).` / `Skipped 1 file(s) with syntax errors.`, file unchanged | CLI-004 | `Program.Main` + `ConsoleCapture` | green |
| `…TopLevelStatementStartingWithUnionLocalIsFormattedUnderLangVersion14`: same input under `--lang-version 14`; exit 0, empty stderr, exact summary, file equals the change doc's "before" text (`1,2` -> `1, 2`) | CLI-004 | `Program.Main` + `ConsoleCapture` | green |

Tagged like the earlier type-name tests (`[Requirement("CLI-004")]`).

Watched for the right reason: with the arguments swapped (the skip test run under `--lang-version 14`, the format test run without it), both failed on the summary-line assertion; the swap was reverted before commit.

## Not covered, and why
- **The other failing statement starts** (`union = …;`, `union++;`, `union[0] = …;`, `union?.M();`, `union(…);`, `union<int>(…);`) and the holding positions (block or method body, `closed`). All reach one call site, the parse with the CLI's version followed by the syntax-error check in `src/Reihitsu.Cli/FormatCommandHandler.cs` (same proof as in 04-test-implementer); which starts break is Roslyn's parser behavior, not Reihitsu's. The change doc's single example is pinned.
- **CRLF variant.** The outcome is a parse decision before any phase runs; the `--lang-version 14` output only changes a space inside one line. The CRLF union tests in this class already cover line endings for the CLI path.
- **Second-pass idempotency of the C# 14 output.** Not part of answer 5A; the C# 14 formatter path is unchanged by this run.
- **Failure classes.** As in 04-test-implementer: no guard, rewrite, code fix, rule, or precheck shape changes; FC-017 is addressed by exact output and file-content assertions.

## Commands
```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliLanguageVersionEndToEndTests"
```
