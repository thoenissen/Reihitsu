Role: auditor
Commit: 494c075ed347bc1510fea203375dd875b03f0774
Verdict: pass

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope
Repair delta `b0522fd..494c075`, head 494c075, base `origin/main` 1defac1. I checked that the checkout is 494c075, that `git ls-remote origin refs/heads/main` gives 1defac1, and that 1defac1 is an ancestor of the head. `scripts/lane-check.sh behavior-change` reports `confirmed`. The repair diff matches handovers 10–14 and nothing else. It touches:
- the change doc: the *What* sentence, a new *Visible output changes* entry for a top-level `union` local, a Playground/trace entry, and a new *Decisions* section for answer 5A;
- two new CLI end-to-end tests with two constants in `CliLanguageVersionEndToEndTests.cs`;
- the comment in `PlaygroundFormatterTests.cs:90-91`;
- work docs.

No production source and no spec changed, so the scope did not grow and a full audit is not needed. The evidence from audit 09 that this round reuses is the behaviour of the Release CLI on the failing and holding statement starts, and `closed` in every position. It is unchanged, because no production code changed after b0522fd.

Status of the previous findings:
- **#1 — closed.** The change doc now lists the top-level `union` local case with its failing starts and holding positions, and an example whose input, before, and after match my counterexample from 09. Maintainer answer 5A is recorded under `Maintainer answers` in `00-lead.md` and under the change doc's *Decisions* section. Decision 6 pins the outcome in the two new tests.
- **#2 — closed.** The comment now reads "which only C# 14 and later parse without an error". That is accurate for C# 15. The change is comment-only.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption; production code is byte-identical to b0522fd (see 09)._

## Policy ownership
_N/A — the repair changes no policy; the owners recorded in 09 (`LanguageVersionResolver.MaxSupportedLanguageVersion`, the 11 Roslyn `PackageReference`s) are untouched._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CliLanguageVersionEndToEndTests.TopLevelStatementStartingWithUnionLocalIsSkippedWithoutLangVersion` | Answer 5A / CLI-004: under the default C# 15, a file whose top-level statement starts with a local named `union` is skipped with syntax errors and left unchanged | the file is formatted or changed, the summary differs, or stderr is not empty, a non-zero exit code | `Program.Main` + `ConsoleCapture` (`Run`), `TemporaryDirectoryFixture` | adequate. It asserts the exact stdout, including `Skipped 1 file(s) with syntax errors.`, the exit code, an empty stderr, and the exact file content. It has the same shape as `TypeNamedUnionIsSkippedWithoutLangVersion`. The input is verbatim the change doc's example (FC-035). |
| `…TopLevelStatementStartingWithUnionLocalIsFormattedUnderLangVersion14` | Answer 5A: `--lang-version 14` restores the old result | a skip, a different output, or stderr | same | adequate. It asserts the exact verbose stdout and the exact file content, `1,2` → `1, 2`, and the expected text equals the change doc's "before". The test implementer saw both tests fail when the arguments were swapped, so the summary assertion tells the two outcomes apart. |

Visible output is decided (check 10). Both expected values are covered by answer 5A in `00-lead.md` and in the change doc's *Decisions*. The `--lang-version 14` text is the unchanged C# 14 path, already covered by 3A's "restores the old result". No rule page changed.

The new constants' summaries are accurate (check 8). `TopLevelUnionLocalSource` says "C# 15 reads [it] as the start of a union declaration", and `TopLevelUnionLocalFormatted` says "under C# 14". The test summaries describe what the tests assert. No `TODO` and no tracker number appears in source.

Change-doc claims checked against the code. The Playground reports syntax errors and leaves the input unchanged (`PlaygroundFormatter.cs:25,52-53`, `Home.razor.cs:72`). The trace tool parses with the default options (`PipelineTraceCommand.cs:145`) and prints its syntax-errors message (`:201-205`). Both agree with the new "Playground and pipeline trace tool" entry. The failing and holding statement starts in the new entry match the members I probed in 09 on Roslyn 5.12, and nothing in the repair changed them.

## Failure classes dismissed
- FC-001 to FC-016 and FC-018 to FC-030: the repair changes no production code, guard, rewrite, code fix, rule, phase, or loop. Audit 09's dismissals stand byte-identically.
- FC-017: see *Assertion adequacy*. Both new tests assert exact output and exact file content.
- FC-031: finding #2 is closed. The new test and constant summaries are accurate.
- FC-032 to FC-034: no implementation result is relied on beyond the tests' own runs. Cleaner 13 reported 16/16 green for the class on fe06956. The tests are unchanged since d4c7b90, and I re-ran nothing because no finding turns on it.
- FC-035: the expected values are copied verbatim from the change doc, which was written from runs on the Release CLI.
- FC-036: no rewrite changed.
- FC-037: the Playground edit is a comment-only change inside a `///` summary, visible in the diff.
- FC-038 to FC-043: no shape is declared, the precheck is unchanged, and no code fix changed.
- Class matrix "Tab width across formatter phases": no column computation changed.

## Architecture findings
_None._ The repair adds no mechanism. The decision it covers is the CLI's parse with the resolved version, which has one owner (`FormatCommandHandler`'s parse followed by the syntax-error check).

## Hints
- **Uneven wrap in the Playground summary.** `PlaygroundFormatterTests.cs:91` is now 133 characters, while the line above is 120. The repository has no line-length rule for comments (the same file has 234-character lines), so this is cosmetic only.
- **`union<int>(…)` in the change doc.** The new *Visible output changes* entry lists `union<int>(…);`, which answer 5A's own example list does not name. The answer's subject, "a local named `union`" at the start of a top-level statement, covers it, and 09 confirmed that it fails the same way. No action is needed unless the lead wants the two lists word-for-word identical.
