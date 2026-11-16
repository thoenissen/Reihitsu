Role: test implementer
Commit: 4f7c9ac (head the change was written and run on; the change and this handover are committed on top of it)
Verdict: tests written

Worklist: decision 7 in `00-lead.md` (audit finding #2, `09-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision. Tooling lane; no requirement exists for the invariant, so no test carries a `[Requirement]` tag. No production code, build file, or script touched; the #898 guard is unchanged.

## What changed

`src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs` now reads every MSBuild name through one case-insensitive rule, two private helpers:

- `IsNamed(element, name)` — element local name, `OrdinalIgnoreCase`. Used for the item types `PackageReference`/`PackageVersion` (was ordinal, shape (e)), the child metadata elements `Version`/`VersionOverride` (was ordinal, shape (f)), the property `RoslynVersion` (already case-insensitive), and its parent `PropertyGroup` (was ordinal).
- `FindAttributes(element, name)` — every attribute whose local name matches, `OrdinalIgnoreCase`. Used for `Include`, `Update`, `Version`, `VersionOverride`, and `Condition` (all were exact `XName` lookups).
- An item now passes only with **exactly one** `Version` attribute in any casing whose value is `$(RoslynVersion)`, no `VersionOverride` attribute, and no `Version`/`VersionOverride` child, all in any casing. A second `version="5.6.0"` next to `Version="$(RoslynVersion)"` (shape (g)) is therefore reported, as is a lone `version="5.6.0"`, while a lone `version="$(RoslynVersion)"` is accepted, as MSBuild reads it the same.

What stays ordinal, and why: the attribute *value* `$(RoslynVersion)` (a value, not a name the guard matches; another casing such as `$(roslynversion)` is reported — strict and loud, unchanged from `06`), the `$(` literal check, and the file-path ordering and root-path equality. Reserved names (`Include`, `Update`, `Condition`, `PropertyGroup`) are case-sensitive in MSBuild, so another casing fails evaluation; reading them case-insensitively only lets the guard report a file MSBuild already rejects, and keeps one rule for every name. No other modelling: no evaluation, no "last definition wins" ordering — two version definitions are reported regardless of which one MSBuild picks.

The class summary states the rule. `RoslynPackageVersionTests.cs` gained the eight tests below; no existing test changed.

## Tests
All in `Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests`; fixtures via `RoslynVersionFixtureDirectory`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ItemNamesInAnotherCasingNamingTheVersionPropertyAreAccepted` | FC-003 (accepted side: `packagereference` + `version="$(RoslynVersion)"`, `PACKAGEVERSION` + `VERSION="$(RoslynVersion)"`; asserts both collected, no violation) | fixture | Passes (failed before: 0 items collected) |
| `PackageReferenceItemTypeInAnotherCasingIsReported` | FC-001 / FC-017, shape (e) | `AssertSingleViolation` | Passes (failed before: 0 violations) |
| `PackageVersionItemTypeInAnotherCasingIsReported` | FC-001 / FC-017, shape (e), `packageversion` | `AssertSingleViolation` | Passes (failed before) |
| `VersionMetadataElementInAnotherCasingIsReported` | FC-001 / FC-017, shape (f) on an `Include` item | `AssertSingleViolation` | Passes (failed before) |
| `VersionMetadataElementInAnotherCasingOnUpdateItemIsReported` | FC-001 / FC-017, shape (f) on an `Update` item (in a separate props file, so the location pins the `Update` item) | `AssertSingleViolation` | Passes (failed before) |
| `VersionOverrideAttributeInAnotherCasingIsReported` | FC-003 (`versionoverride` attribute arm of the same rule) | `AssertSingleViolation` | Passes (failed before) |
| `SecondVersionAttributeInAnotherCasingIsReported` | FC-001 / FC-017, shape (g) | `AssertSingleViolation` | Passes (failed before) |
| `LoneVersionAttributeInAnotherCasingWithLiteralIsReported` | FC-003 (boundary of (g): one attribute, another casing, literal) | `AssertSingleViolation` | Passes (passed before too, as `09` states; kept as the boundary against the accepted side) |

Before the helper change, 7 of the 8 failed for the right reason (`expected count: 1, actual count: 0`, or for the accepted test, items not collected); after it, all 26 class tests pass.

## Scenarios
The guard against the real tree: each shape applied to `src/Reihitsu.Formatter/Reihitsu.Formatter.csproj` (line 13, `Microsoft.CodeAnalysis.CSharp.Workspaces` with `Version="$(RoslynVersion)"`), checked with `dotnet msbuild <csproj> -getItem:PackageReference` and with `dotnet test <ArchitectureTests.dll> --filter …EveryRoslynPackageItemUsesTheRepositoryVersionProperty`, then reverted with `git checkout`:

| Scenario | Line ending | Surface | MSBuild resolves | Guard |
|---|---|---|---|---|
| Unchanged tree | n/a | guard | 5.12.0 | passes (`Passed: 1`) |
| (e) adds `<packagereference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />` | n/a | guard | `Microsoft.CodeAnalysis.CSharp` 5.6.0 | fails: `Reihitsu.Formatter.csproj:13: packagereference Microsoft.CodeAnalysis.CSharp must state exactly …` |
| (f) `<version>5.6.0</version>` child on the `Include` item | n/a | guard | Workspaces `version` 5.6.0 | fails: `…:13: PackageReference Microsoft.CodeAnalysis.CSharp.Workspaces must state exactly …` |
| (f) `Update` item with `Version="$(RoslynVersion)"` and `<version>5.6.0</version>` | n/a | guard | Workspaces `version` 5.6.0 | fails: `…:13: PackageReference Microsoft.CodeAnalysis.CSharp.Workspaces …` |
| (g) `Version="$(RoslynVersion)" version="5.6.0"` on the item | n/a | guard | Workspaces `version` 5.6.0 | fails: `…:13: PackageReference Microsoft.CodeAnalysis.CSharp.Workspaces …` |

`git status` after the runs shows only the two test files changed.

## Not covered, and why
- The value `$(roslynversion)` in another casing: reported (ordinal value compare), loud, unchanged from `06`; not a name the guard matches, and decision 7 names names only.
- Reserved names in another casing (`include=`, `condition=`, `<propertygroup>`): MSBuild fails evaluation (MSB4232, MSB4066, MSB4067, per `09`); the guard now reads them too, which can only add a report to a file that does not build. No fixture: no shape MSBuild accepts depends on it.
- FC-002, FC-004 to FC-016, FC-018 to FC-043: no analyzer, code fix, formatter phase, trivia, or C# rewrite is touched; the change is one architecture-test helper. FC-044: the change adds no evaluation — a name comparison only.
- Line endings: no layout; the XML reader is line-ending agnostic.

## Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~RoslynPackageVersionTests"
```
On this change: 26 passed. Whole architecture suite: 180 passed, 0 failed. `scripts/format.sh` on both files: 0 of 2 changed.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
