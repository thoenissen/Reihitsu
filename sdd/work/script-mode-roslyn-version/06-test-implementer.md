Role: test implementer
Commit: 2d694ff (head the repair was written and run on; the change and this handover are committed on top of it)
Verdict: tests written

Repair round 1, worklist: audit finding #1 as decision 6 in `00-lead.md` decides it. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision. Tooling lane; no requirement exists for the invariant, so no test carries a `[Requirement]` tag. No production code touched; the #898 guard is unchanged.

## What changed

The #1780 guard no longer models MSBuild version resolution (FC-044). It checks the chosen mechanism (decision 5) syntactically, from the build files as written:

- Every `PackageReference` or `PackageVersion` item whose `Include` or `Update` (a `;` list is split) names a non-Testing `Microsoft.CodeAnalysis` / `Microsoft.CodeAnalysis.*` package states exactly `Version="$(RoslynVersion)"`, with no `VersionOverride` attribute and no `Version`/`VersionOverride` metadata element. `Remove` items are not checked.
- `RoslynVersion` is a `PropertyGroup` child exactly once across all build files; that one definition is in the root `Directory.Build.props`, has no `Condition`, sits in an unconditioned `PropertyGroup` directly under `<Project>` (so not inside `Choose`/`When`), and is a literal (non-empty, no `$(`).
- Files read: every `*.csproj`, `*.props`, `*.targets` below the repository root, skipping any directory named `bin` or `obj` or starting with a dot. On this tree that is exactly the 22 tracked build files (`git ls-files '*.csproj' '*.props' '*.targets' | wc -l` and the `find` equivalent both give 22), so the guard reads tracked files without depending on git (no `Inconclusive` path).

Removed, because they only served options decision 5 did not take or modelled evaluation: `RoslynPackageReferences.cs` (the reader: props-chain walk, property substitution, central `PackageVersion` lookup, `VersionOverride` resolution, `<unresolved>` marker) and the tests `DifferentLiteralVersionsAreReportedAsMismatch`, `SameLiteralVersionsAreNotReported`, `VersionPropertyFromDirectoryBuildPropsIsResolved`, `CentralPackageVersionIsResolved`, `TestingPackagesAreNotCollected`, `UnresolvableVersionIsReported` (their still-relevant sides reappear below as checks of the new predicate). `RoslynVersionFixtureDirectory` lost its unused `Source` property.

Files: `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs` (new), `RoslynPackageVersionTests.cs` (rewritten), `RoslynVersionFixtureDirectory.cs` (trimmed), `RoslynPackageReferences.cs` (deleted).

## Tests
All in `Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests`; fixtures via `RoslynVersionFixtureDirectory`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` | — (tooling invariant; FC-017: asserts items were found in Core, Analyzer, CodeFixes, Formatter project files) | `RoslynVersionOwnership.Check(repositoryRoot)` | Passes |
| `ItemsNamingTheVersionPropertyAreAccepted` | FC-003 (accepted side: `Include` and `Update` naming the property, `*.Testing` literal, non-Roslyn literal, `Remove` item) | fixture | Passes |
| `LiteralVersionInProjectIsReported` | FC-003 / FC-017 (literal version, setup d) | fixture | Passes |
| `UpdateItemWithLiteralVersionIsReported` | FC-044 (auditor setup a) | fixture | Passes |
| `ReferenceInPropsFileWithLiteralVersionIsReported` | FC-044 (auditor setup b) | fixture | Passes |
| `ReferenceInTargetsFileWithLiteralVersionIsReported` | FC-003 (`.targets` arm of the file filter) | fixture | Passes |
| `CentralPackageVersionWithLiteralVersionIsReported` | FC-003 (`PackageVersion` item type) | fixture | Passes |
| `VersionOverrideAttributeIsReported` | FC-003 (`VersionOverride` attribute next to the property) | fixture | Passes |
| `VersionMetadataElementIsReported` | FC-003 (metadata element arm) | fixture | Passes |
| `RoslynPackageInIncludeListIsReported` | FC-003 (`;` list split) | fixture | Passes |
| `ConditionedVersionPropertyIsReported` | FC-044 (auditor setup c, its exact two definitions) | fixture | Passes |
| `VersionPropertyInConditionedPropertyGroupIsReported` | FC-003 (`PropertyGroup` condition arm) | fixture | Passes |
| `VersionPropertyInChooseBranchIsReported` | FC-003 (not directly under `<Project>`) | fixture | Passes |
| `RedefinitionInNestedPropsIsReported` | FC-003 (count > 1 and outside-root arms) | fixture | Passes |
| `VersionPropertyOutsideRootPropsIsReported` | FC-003 (single definition, wrong file) | fixture | Passes |
| `MissingVersionPropertyIsReported` | FC-003 (count 0) | fixture | Passes |
| `VersionPropertyNamingAnotherPropertyIsReported` | FC-003 (literal check) | fixture | Passes |
| `BuildOutputAndDotDirectoriesAreSkipped` | FC-003 (skip side: `obj`, `bin`, dot directory; asserts no items, so a broken arm surfaces) | fixture | Passes |

## Scenarios
The guard against the real tree, each mutation applied to the working tree, the repository test run on the built assembly (`dotnet test <ArchitectureTests.dll> --filter …EveryRoslynPackageItemUsesTheRepositoryVersionProperty`), then reverted with `git checkout`:

| Scenario | Line ending | Surface | Result | Observed |
|---|---|---|---|---|
| Unchanged tree (2d694ff + this change) | n/a | guard | passes | `Passed: 1` |
| (a) `Reihitsu.Formatter.csproj` adds `<PackageReference Update="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.6.0" />` | n/a | guard | fails | `src/Reihitsu.Formatter/Reihitsu.Formatter.csproj:14: PackageReference Microsoft.CodeAnalysis.CSharp.Workspaces must state exactly Version="$(RoslynVersion)" …` |
| (b) root `Directory.Build.props` adds `<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="5.6.0" />` | n/a | guard | fails | `Directory.Build.props:51: PackageReference Microsoft.CodeAnalysis.Workspaces.Common must state exactly …` |
| (c) `RoslynVersion` replaced by `Condition="'$(MSBuildProjectName)'=='Reihitsu.Formatter'"` 5.6.0 and `Condition="'$(RoslynVersion)'==''"` 5.12.0 | n/a | guard | fails | `RoslynVersion is defined 2 times: Directory.Build.props:28, Directory.Build.props:29; …:28: … under a condition; …:29: … under a condition` |
| (d) `Reihitsu.Formatter.csproj` states literal `Version="5.12.0"` | n/a | guard | fails | `src/Reihitsu.Formatter/Reihitsu.Formatter.csproj:13: PackageReference … must state exactly …` |

Note on (d): a literal equal to the current version is reported too — the check is about the owner, not the value, which is decision 6's mechanism.

## Not covered, and why
- FC-001 to FC-044 other than FC-003, FC-017, FC-044: no analyzer, code fix, formatter phase, trivia, or C# rewrite is touched; the change is one architecture test and its helper.
- Line endings: no layout involved; the XML reader is line-ending agnostic and the line numbers come from `IXmlLineInfo`.
- MSBuild property names are case-insensitive: a definition spelled `roslynversion` is still found (case-insensitive element match); a reference spelled `$(roslynversion)` is reported (ordinal compare) — strict, fails loudly, no test.
- Global properties (`-p:RoslynVersion=…`) and environment variables: not a build-file shape; outside a syntactic check of tracked files.
- An `Update` item that sets only metadata on a Roslyn package (no version) is reported under decision 6's "names exactly `$(RoslynVersion)`"; no such item exists, and adding the attribute fixes it.

## Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~RoslynPackageVersionTests"
```
On this change: 18 passed. Whole architecture suite (Release build): 172 passed, 0 failed.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
