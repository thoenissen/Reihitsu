# Lead — script-mode-roslyn-version

- Issues: thoenissen/Reihitsu#898 (`agent-filed`), thoenissen/Reihitsu#1780 (`agent-filed`). Goals of both are proposals.
- Branch: `claude/kind-brahmagupta-vu28e9` (session-designated). PR: #1810 (draft).
- Lane: **tooling**.
  - #898: the change is the mode of `scripts/trace.sh` (tooling path `scripts/**`), optionally a guard test in `Reihitsu.ArchitectureTests` (test path).
  - #1780: the change is `*.csproj`/`*.props` (tooling paths), optionally a guard test in `Reihitsu.ArchitectureTests`.
- Roles run: test implementer (the guards are tests), implementer, cleaner, spec checker, auditor, validator.
- Roles skipped: spec author and skeptic — the tooling lane runs neither; no visible output (formatter, diagnostics, fixes, rule pages, CLI) is touched.
- Maintainer clarifications: none.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Do #898 and #1780 share a mechanism, or should the batch be split? | Keep one run | Both are repository-wide invariants over tracked build/script files whose regression guard lives in `Reihitsu.ArchitectureTests`; together they are two small tooling changes, so no handover grows long | Batch shape only | claim |
| 2 | Tests | Should the #898 guard report Inconclusive instead of failing when git is missing or refuses the tree? | Keep Inconclusive | Matches the existing precedent `ApplyFixWrapperPositionalArgumentTests` (missing bash); CI checks out with git as the owning user, so the guard runs there | Test infrastructure only | f32b2c3 |
| 3 | Tests | Should the #1780 guard include `Microsoft.CodeAnalysis.Analyzers`? | Include it | It ships on the same version line (5.12.0) and the issue names every non-Testing `Microsoft.CodeAnalysis*` reference | Build configuration only | f32b2c3 |
| 4 | Tests | Bump or guard the `#:package Microsoft.CodeAnalysis.CSharp@5.6.0` pin in `scripts/proof/verify-text-only.cs`? | Neither in this run; follow-up candidate | It is a standalone file-based script outside `src/`; whether the one-version rule covers the scripts' own pins is a separate decision with its own verification of the text-only proof | Tooling only | — |
| 5 | Implementer | #1780: the guard alone, or also one version property? | Also one version property used by every non-Testing Roslyn reference | The issue's title asks for one owner; with the guard alone a bump still edits 11 files. The guard stays as the regression check | Build configuration only; the resolved version stays 5.12.0 | 8f52e40 |
| 6 | Audit 1 | Finding #1: the #1780 guard models MSBuild version resolution and misses `Update` items, Roslyn references in props files, and a conditional `RoslynVersion` | Repair round 1 (same requirement, PR-introduced): replace the model by a cheap check of the chosen mechanism — every non-Testing Roslyn `PackageReference`/`PackageVersion` item (`Include` or `Update`) in tracked `*.csproj`/`*.props`/`*.targets` names exactly `$(RoslynVersion)`, and `RoslynVersion` is defined once, unconditionally, in the root `Directory.Build.props`; drop the paths and fixtures that only serve options not taken. Routed to the test implementer (fresh) | FC-044: a guard that models a result another tool computes is refused from the first finding; the cheap check follows decision 5 | Test only | ca374b3 |
| 7 | Audit 2 | Finding #2 repeats #1's class (guard reads MSBuild names case-sensitively; MSBuild does not) — architecture finding | Fix by the cause in this run: the guard compares every MSBuild name it reads (item types, attribute and child-element metadata names) with one case-insensitive rule, as it already does for property names and package IDs; one fixture each for (e), (f), (g); no other modelling. Routed to the test implementer (fresh); not a repair round | The cause is one comparison rule applied two ways in one file; unifying it is a cheap check, no new machinery, and it closes the class rather than the three cells. Reading MSBuild's evaluated result would add a process dependency to the architecture tests for a rare casing | Test only | 1ef7ef0 |
| 8 | Audit 3 | Finding #3 repeats #1/#2: deliberate MSBuild indirection (package ID from a property or item, a version set in a `Target`, an import with another extension, a `Directory.Build.rsp` property) escapes the guard, and the class summary claims "any other form is reported" | Refuse the class by its cause: the guard checks the build files as written and does not see deliberate MSBuild indirection; correct the class summary to state that limit. Comment-only, routed to the cleaner, proven with `scripts/verify-text-only.sh --base ec2b463`; a passing proof needs no further audit. Reading `obj/project.assets.json` is not taken | The repository uses none of these shapes, each is deliberate indirection (rare by construction), and every further check repeats the per-cell loop the architecture rule ends; reading restore output would tie the architecture tests to build artifacts of every project | Test comment only | pending |

## Follow-up candidates

- Whether the file-based scripts' own Roslyn pins (`scripts/proof/verify-text-only.cs` pins `Microsoft.CodeAnalysis.CSharp@5.6.0`, the projects use 5.12.0) fall under the one-version rule: bump and/or guard them. Named in `01-test-implementer.md`.
- S1125 warnings in the Debug compile of `PipelineTraceCommand.cs` and `FormatCommandHandler.cs` (pre-existing on `main`, files untouched by the run; Release build clean). Named in `03-cleaner.md`.

## State

- Audited SHA: —
- Last step: test implementer (f32b2c3; #898 reproduced, #1780 guard written).
- Implementer: 65b3a94 (trace.sh +x). Continued once for decision 5.
- Implementer round 2: 8f52e40.
- Cleaner: clean.
- Spec checker: consistent (2d4c863).
- Audit 1 on 2d4c863 (main eb89003): findings, 1 (05-auditor.md). Repair round 1 of 3.
- Repair 1: test implementer ca374b3 (06).
- Repair 1: cleaner clean (07), spec checker consistent (08, 4e35f30).
- Audit 2 on 61cf6e1 (main eb89003): finding #2, architecture finding (09-auditor.md, 797c0b4); decision 7.
- Decision 7 carried out: 1ef7ef0 (10).
- Cleaner clean (11), spec checker consistent (12, 0507eaf).
- Audit 3 on ec2b463 (main eb89003): finding #3, architecture finding (13-auditor.md, e8d0436); decision 8. Audited SHA for the text-only proof: ec2b463.
- Next step: cleaner (summary comment), text-only proof, validator, publish.
