Role: auditor
Commit: 8a4daf2
Verdict: pass, text cleanup

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

## thoenissen/Reihitsu#1808

### Scope
Full audit. The checkout is at `8a4daf25c9e905690a786eaedf62fb2675cf4d9f`. `origin/main` and `git ls-remote origin refs/heads/main` are both `a971d8944cdc449278472f6ad85e403c9d48cd70`, and HEAD contains that commit (`git merge-base --is-ancestor`). The diff for this issue is `scripts/proof/verify-text-only.cs` (`IsNonCompiledPath`, the `_behavioralPrefixes` summary) and `scripts/README.md`, *The text-only proof*. That is what `02-implementer.md` describes.

Against the issue and decisions 1, 5 and 6:
- Done-when, re-checked on the audited head in a scratch worktree with `scripts/verify-text-only.sh --base HEAD --head worktree`:
  - A prose edit in `scripts/README.md` plus an added `scripts/proof/NOTES.md` (intent-to-add) gives `PASS — 2 changed file(s) (2 non-compiled, …)`.
  - Adding `scripts/build.sh` to that change gives `FAIL — 1 of 3`. Only `scripts/build.sh` is behavioral.
  - An edit in `.github/workflows/build.yml` gives `FAIL — 1 of 3`.
  - The README states the rule.
- `Verify` (`verify-text-only.cs:364`) handles a rename or copy first, then sends every non-C# path to `IsNonCompiledPath`, then handles C# files.
  - `.md` is now decided first, case-insensitively, so `scripts/**/*.md` and `.github/workflows/**/*.md` are text (decisions 1 and 6).
  - Every other path under the behavioral prefixes is still behavioral.
  - C# under `scripts/` never reaches `IsNonCompiledPath`, so nothing changes for it.
- README sentence on renames and adds (decision 5): a renamed file is behavioral at `:368`, before the extension is checked. An added or deleted C# file is behavioral at `:382`–`:386`. An added or deleted non-C# file goes through `IsNonCompiledPath`. The sentence matches the code.
- Interaction with the lane check (`src/Reihitsu.Tooling/LaneClassifier.cs:234`):
  - The proof's verdict only turns `src/**/*.cs` into text. `scripts/**`, Markdown included, stays tooling, as the README says.
  - One consequence: a diff of `scripts/*.md` plus a comment-only C# change under `src/` now passes the proof, so its C# counts as text and the lane is tooling. Before, the lane was bug-fix or wider. The lane check classifies more precisely, and no lane is narrowed below what the paths need. This is not a finding.
- No tool reads a Markdown file under `scripts/` as data:
  - `rg` over `src/Reihitsu.Tooling`, `scripts/*.sh`, `scripts/proof`, `.github/workflows` finds only comments that name `scripts/README.md`.
  - `LaneClassifierTests` hard-codes `scripts/README.md` as tooling. That is unchanged and agrees with the README.
  - `git ls-files 'scripts/**/*.md' '.github/workflows/*'` lists `scripts/README.md` and five `.yml` workflows.

### Findings
_None for this issue._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Markdown is text for the proof | The whole repository-relative path, `.md` suffix, `OrdinalIgnoreCase` (`verify-text-only.cs:601`) | `LaneClassifier` `.md` checks (`IsRootMarkdownText`, `ClassifySourcePath`): a different decision, the lane category, which decision 1 keeps separate on purpose | They differ by design, and the README says so ("This decides only the proof's verdict") | ok |
| Behavioral prefix | Path start, `scripts/`, `.github/workflows/`, now after the `.md` check | none other | — | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which non-C# paths the proof treats as text | `scripts/proof/verify-text-only.cs` `IsNonCompiledPath`, `_nonCompiledPrefixes`, `_behavioralPrefixes`. Prose statement: only `scripts/README.md:103`. `rg "behavioral regardless\|regardless of extension\|non-compiled text"` finds no other statement in `squad/`, `sdd/`, `AGENTS.md` or `CLAUDE.md`. `squad/notes/2026-11-16-one-statement-docs.md:56` records the old behavior as a run observation (a note, not a rule). | one owner |

### Assertion adequacy
_N/A — the proof has no tests. `01-test-implementer.md` confirms this with `rg`, and decision 3 accepts it. The verdicts were checked by hand by the implementer and again by this audit, as described above._

## thoenissen/Reihitsu#1817

### Scope
Full audit of `scripts/proof/verify-text-only.cs:1`, `scripts/README.md` (the paragraph on the proof's file-based app), `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs` and `src/Reihitsu.ArchitectureTests/General/RoslynPackageVersionTests.cs`. These match `02-implementer.md` and `03-test-implementer.md`.

Against the issue and decisions 2 and 4:
- The pin `@5.12.0` equals `RoslynVersion` (`Directory.Build.props:37`).
- The guard fails on drift. The implementer reports a mutation back to 5.6.0 failing the repository test. The fixture tests cover another version, a floating version, a prerelease suffix, no version, and the compare-with-defined-version case.
- The proof builds and runs with 5.12.0: both of this audit's worktree runs above built it.
- Before and after verdicts on a text-only and a non-text-only C# change are in `02-implementer.md` (scenarios C and D).
- `rg` finds no other Roslyn version pin outside build files and this directive. `apply-fix.cs`, `lane-check.cs` and `trace.cs` carry `#:project` and `#:property` only, so Roslyn reaches them through guarded project files.
- 6 of 1938 tracked `.cs` files contain `#:`, so the repository walk parses only those 6. Each file is read once, and the cost is negligible.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.ArchitectureTests/General/RoslynPackageVersionTests.cs:38` | FC-031 | – | PR-introduced | `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` now also asserts that the guard reads the `#:package` directive of `scripts/proof/verify-text-only.cs` (`:57`), and that its violation list covers directives. Its summary still says "Verifies that the repository's build files name the Roslyn version only through the one property". The file-based program is not a build file. | Reword the summary to cover the repository's build files and file-based programs, as the class summary already does. Comment-only. |

Why `pass, text cleanup` holds for #1: the change is to an XML summary of a test method. Nothing ships it, and it cannot reach compiled behavior, diagnostics or rule metadata. Prove it with `scripts/verify-text-only.sh --base 8a4daf2 --head worktree`.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which `#:` directives are read | The leading trivia of `GetFirstToken(includeZeroWidth: true)`, so the EOF token when the file has no code. Only `IgnoredDirectiveTriviaSyntax`. | The .NET SDK honors `#:` directives only before the first C# token, and Roslyn reports a later one as an error. | Same span. A late directive, or `#:`-looking text in a literal, is not read (`PackageDirectiveTextAfterTheFirstTokenIsNotRead`). | ok |
| Pre-filter | `text.Contains("#:")`, ordinal | The parse above | It is a superset: every file with an ignored directive contains `#:` | ok |
| Roslyn package | `IsRoslynPackage(package)` on the identifier before the first `@` | The same `IsRoslynPackage` used for build items (`CheckPackageItem`) | One predicate, shared | ok |
| Version equality | The text after the first `@`, ordinal, against the trimmed root `RoslynVersion` value; `null` when there is no `@` | Build items: an exact `Version="$(RoslynVersion)"` text, ordinal | Both are exact-literal comparisons. The directive cannot name the property, so it compares with the property's literal value. | ok |
| Comparison is skipped | Not exactly one root definition | `CheckDefinitions` reports 0, more than 1, or non-root definitions | Every skipped case is reported there. The 0-definition case is covered by the existing test at `:415`, which would throw without the skip. | ok |
| Skipped directories | `IsSkipped`, unchanged, applied before the extension split | Same as for build files | One predicate | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| One Roslyn version | `RoslynVersionOwnership` alone (`rg "IsRoslynPackage\|RoslynVersion" src --glob '*.cs'`). The directive check reuses `IsRoslynPackage`, `IsSkipped` and `VersionPropertyName`. Prose: `scripts/README.md` paragraph on the proof, and the `RoslynVersion` comment in `Directory.Build.props:36` (incomplete, not contradictory; dropped by decision 9). | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` (extended) | The real proof's pin is read and equals `RoslynVersion` | The item is missing, or there is a violation (the implementer's 5.6.0 mutation) | `Assert.Contains` on files, `Assert.IsEmpty` | adequate |
| `PackageDirectiveAtTheRepositoryVersionIsAccepted` | An exact pin is recorded with the bare id and not reported | No item, an id that still holds `@…`, or a violation | `Contains` the exact tuple, `HasCount(1)` | adequate. It also catches a missing `@` split, which the `StartsWith` prefix of the report tests could not catch alone. |
| `PackageDirectiveWithAnotherVersionIsReported`, `…WithFloatingVersion…`, `…WithLongerVersion…`, `…WithoutVersion…`, `…InAnotherCasing…` | Every non-exact version is reported once, at its line | 0 or 2 violations, or the wrong location prefix | `AssertSingleViolation` (exact count, prefix) | adequate |
| `PackageDirectiveIsComparedWithTheDefinedVersion` | The comparison is against the defined value, not a constant | A constant 5.12.0 would report `A.cs`, not `B.cs` | `AssertSingleViolation` | adequate |
| `EveryPackageDirectiveOfAProgramIsCheckedOnItsLine` | Every pin is checked, and the line is counted behind a shebang and other directives | Only the first pin checked, or a wrong line | `AssertSingleViolation` with `:5`, `HasCount(2, items)` | adequate |
| `PackageDirectivesOutsideTheRoslynVersionLineAreAccepted`, `PackageDirectiveWithoutVersionOutsideTheRoslynVersionLineIsAccepted` | `*.Testing`, look-alike and foreign ids are neither read nor reported, with or without a version | Any item or violation | `IsEmpty` on both lists | adequate |
| `PackageDirectiveTextAfterTheFirstTokenIsNotRead` | `#:`-looking text after the first token is not a directive | Any item or violation | `IsEmpty` on both lists | adequate |
| `PackageDirectivesInBuildOutputAndDotDirectoriesAreSkipped` | `bin`, `obj` and dot directories are skipped for `.cs` files | Any item or violation | `IsEmpty` on both lists | adequate |

## Whole run

### Scope
Head `8a4daf2`, base `a971d89`, full audit. `scripts/lane-check.sh tooling --base a971d89… --head HEAD` gives `LANE CHECK: confirmed — claimed tooling, diff needs tooling`. No path is outside the two issues and `sdd/work/text-only-proof-scripts/`. Visible output (check 10): no test of a visible surface is added or changed, and no rule page or CLI behavior is touched. Special cases (check 11): none. No `TODO`, no commented-out code, and no tracker numbers in source.

### Failure classes dismissed
- FC-001, FC-002: these are traced above under *Guard scope*. The spans agree for both decisions.
- FC-003: both sides of each boundary are tested for the guard: accepted and reported, Roslyn and non-Roslyn, before and after the first token, versioned and version-less. For the proof, both sides were checked by hand (Markdown and script under `scripts/`; Markdown and `.yml` under workflows).
- FC-015: one owner each (*Policy ownership*).
- FC-016: no sibling Roslyn pin exists. The three other file-based programs pin no package.
- FC-044: the guard reads the directive Roslyn returns. It does not model what NuGet resolves.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-043: no formatter phase, analyzer, code fix, rewrite, rename or unsupported shape is touched. The changed code reads text and writes nothing.
- FC-017: see *Assertion adequacy*. Every assertion is as strong as the guarantee.
- FC-031: finding #1. Every other changed summary and inline comment (`Check`, `ReadPackageDirectives`, `CheckPackageDirectives`, the class summaries, `IsNonCompiledPath`, `_behavioralPrefixes`) describes the new code.
- FC-032: no compiled production change. The cleaner's Release build and the focused architecture runs are after the last code commit (`6931294`).
- FC-033, FC-034, FC-035: no bug reproduction. The expected values are the lead's decisions, not output copied from the implementation.
- FC-037: the proof itself is what changed. Its verdicts were checked by running it, not with `grep`.

### Architecture findings
_None._

### Hints
- `00-lead.md` worklist names the cleaner commit `b5426cd`, while `04-cleaner.md` says it worked at `6d060d1`. This is run state only. The spec checker noted it too.

### Follow-up candidates
_None._
