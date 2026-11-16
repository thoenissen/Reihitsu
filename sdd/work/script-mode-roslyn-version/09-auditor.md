Role: auditor
Commit: 61cf6e16bdb23677641222e295564389b6973939
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Repair delta against `2d4c863` (audit 1, `05-auditor.md`). Head `61cf6e1` (checked out, clean tree); `origin/main` and `git ls-remote origin refs/heads/main` are both `eb89003`, an ancestor of the head. The repair diff is what `06-test-implementer.md` describes: `RoslynPackageReferences.cs` deleted, `RoslynVersionOwnership.cs` added, `RoslynPackageVersionTests.cs` rewritten, `RoslynVersionFixtureDirectory.cs` trimmed; no build file, script, or production code changed since `2d4c863`, so the #898 change and the `RoslynVersion` property keep their audit-1 evidence. `scripts/lane-check.sh tooling`: confirmed. The repair stays in the test helper the finding named; scope did not grow.

## Previous findings
| # | Status | Why |
|---|---|---|
| 1 | closed | The guard no longer models MSBuild. Its members fail now: (a) `Update` item — `UpdateItemWithLiteralVersionIsReported`; (b) item in a props file — `ReferenceInPropsFileWithLiteralVersionIsReported`; (c) conditioned property — `ConditionedVersionPropertyIsReported`; each also run as a mutation of the real tree (`06`, Scenarios). The central-package, `VersionOverride`-resolution, and props-chain paths and their tests are gone. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs:91`, `:129-131` | FC-001, FC-017 | #1 | PR-introduced | The guard matches item types and version metadata names ordinally, but MSBuild matches item types and metadata names case-insensitively (reserved elements and attributes such as `PropertyGroup`, `Include`, `Condition` are case-sensitive and fail evaluation in another casing, so they stay loud). Constructed with `dotnet msbuild -getItem:PackageReference` and the guard's own `Check` (copied into a scratch program) on the same files, with the root `RoslynVersion` at `5.12.0`; each resolves `Microsoft.CodeAnalysis.CSharp` to `5.6.0` while `Check` returns 0 violations: **(e) item type in another casing** — `<packagereference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />` (skipped at `:91`, not even collected as an item; same for `packageversion`); **(f) version metadata element in another casing** — `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)"><version>5.6.0</version></PackageReference>`, and the same child under an `Update` item next to `Version="$(RoslynVersion)"` (`:131` compares `"Version"`/`"VersionOverride"` ordinally); **(g) a second version attribute in another casing** — `Version="$(RoslynVersion)" version="5.6.0"` on one item (XML allows both; MSBuild takes `5.6.0`; `:129-130` read only the exact names). Members that hold: a lone `version="5.6.0"` attribute (no `Version` attribute, so `:133` reports it); a property definition in another casing (`:95` already compares `OrdinalIgnoreCase`); a package id in another casing (`IsRoslynPackage` is case-insensitive); `include=`/`update=` in another casing (MSB4232, loud); `<propertygroup>` (MSB4067, loud); `condition=` on the property (MSB4066, loud). Every failing member is a shape nobody writes by hand, but the helper already treats the property name and package id case-insensitively, so the same decision is stated two ways in one file. | Refuse by the cause, the name comparison: match item types and metadata names (attribute and child element) with `OrdinalIgnoreCase`, as the helper already does for the property name, and add one fixture per member (e)-(g). No other modelling. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which elements are package items that state a version | Elements whose local name is ordinally `PackageReference` or `PackageVersion`, anywhere in every `*.csproj`/`*.props`/`*.targets` below the root except `bin`/`obj`/dot directories; identity from `Include` else `Update`, split on `;` | MSBuild: item types case-insensitive; `Include`/`Update`/`Remove` case-sensitive | Disagree on the item-type casing | Finding #2 (e) |
| Which metadata states another version | Attributes `Version`, `VersionOverride` and child elements `Version`, `VersionOverride`, all ordinal | MSBuild/NuGet: metadata names case-insensitive; the last definition wins | Disagree on metadata-name casing | Finding #2 (f), (g) |
| Which packages are Roslyn packages | `Microsoft.CodeAnalysis` or `Microsoft.CodeAnalysis.*`, not ending in `.Testing`, case-insensitive | Issue #1780 wording; decision 3; `Directory.Build.props:27` comment | Agree | Holds (unchanged from audit 1) |
| Where `RoslynVersion` may be defined | `PropertyGroup` children named `RoslynVersion` case-insensitively; exactly one, in the root `Directory.Build.props`, no `Condition` on it or its group, group directly under `<Project>`, literal value | Decision 6; MSBuild property names case-insensitive, `Condition` case-sensitive | Agree; `Target`-nested and `Choose`-nested groups are reported (the message says "under a condition" for a `Target` group, which is imprecise but still fails) | Holds |
| Which files are read | Every file with those extensions below the root, `bin`/`obj`/dot-segment directories skipped | Tracked build files (decision 6 says "tracked") | Agree on this tree (22 = 22); an untracked build file in a working copy is read too, which can only add a failure | Holds; see Hints |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Roslyn version of the projects | `Directory.Build.props:28` `RoslynVersion`; every non-Testing `Microsoft.CodeAnalysis*` reference in `src/**/*.csproj` names `$(RoslynVersion)` (`rg 'Microsoft.CodeAnalysis' --glob '*.csproj' --glob '*.props'`) | One owner; the `scripts/proof/verify-text-only.cs` pin stays the recorded follow-up (decision 4) |
| The guard's reader | `RoslynVersionOwnership` only; `rg RoslynPackageReferences` finds nothing | One owner |
| Case handling of MSBuild names inside the guard | `:95` (`OrdinalIgnoreCase`), `IsRoslynPackage` (`OrdinalIgnoreCase`) versus `:91`, `:129-131` (ordinal) | Two statements of one rule in one file; part of finding #2 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` | No Roslyn item in the real tree states a version other than `$(RoslynVersion)`; the property has one unconditioned root definition | A literal or second definition (mutations (a)-(d) in `06` fail); vacuity blocked by requiring items in Core, Analyzer, CodeFixes, Formatter | `RoslynVersionOwnership.Check` | Weaker than the guarantee for members (e)-(g) of finding #2 |
| `ItemsNamingTheVersionPropertyAreAccepted` | Accepted side: `Include`/`Update` naming the property, Testing and non-Roslyn literals, `Remove` | A violation, or an item count other than 3 (Testing collected, `Update` not collected, `Remove` collected) | fixture | Holds |
| `LiteralVersionInProjectIsReported`, `UpdateItemWithLiteralVersionIsReported`, `ReferenceInPropsFileWithLiteralVersionIsReported`, `ReferenceInTargetsFileWithLiteralVersionIsReported`, `CentralPackageVersionWithLiteralVersionIsReported`, `VersionOverrideAttributeIsReported`, `VersionMetadataElementIsReported`, `RoslynPackageInIncludeListIsReported` | Each item-side arm reports | No violation, a second violation, or a different file:line/type/package (exact count plus `StartsWith` on location and package) | `AssertSingleViolation` | Holds |
| `ConditionedVersionPropertyIsReported`, `VersionPropertyInConditionedPropertyGroupIsReported`, `VersionPropertyInChooseBranchIsReported`, `RedefinitionInNestedPropsIsReported`, `VersionPropertyOutsideRootPropsIsReported`, `MissingVersionPropertyIsReported`, `VersionPropertyNamingAnotherPropertyIsReported` | Each definition-side arm reports | Missing or extra violation (exact counts and full messages) | fixture | Holds |
| `BuildOutputAndDotDirectoriesAreSkipped` | `bin`, `obj`, dot directories are not read | Any item collected or violation reported | fixture | Holds |

## Failure classes dismissed
- FC-002, FC-003: no requirement qualifier; both sides of each predicate arm have a fixture (table above).
- FC-015, FC-016: one reader; sibling item types checked — `PackageDownload` restores without referencing, so it states no compile version; `GlobalPackageReference` exists only under central package management, where every `PackageReference` carrying `Version` is NU1008 and the guard already demands `Version` on each, so adopting it fails loudly.
- FC-044: closed for finding #1 — the helper no longer evaluates properties, imports, or conditions; it reads the build files as written. Finding #2 is a name-matching disagreement, not a model.
- FC-004-FC-014, FC-018-FC-030, FC-036, FC-038-FC-043: no analyzer, code fix, formatter phase, trivia, or C# rewrite is touched by the repair.
- FC-031: summaries of `RoslynVersionOwnership`, its methods, the test class, and the fixture directory describe the new code; no `TODO`, no tracker number (`rg 'TODO|#[0-9]+'` on the three files: empty).
- FC-032: the cleaner's build and the focused architecture runs are on `a5374a2`, whose tree equals the head's outside `sdd/work/`.
- FC-033-FC-035, FC-037: fixtures are hand-written; no comment-only claim.

## Architecture findings
Finding #2 repeats the class of #1: the same decision — which build-file content states the Roslyn version a project gets — failing between the same components, the guard and MSBuild evaluation, in a new input shape. The cause this time is a cheap one, the name comparison, so refusing by that cause (case-insensitive item-type and metadata-name matching) closes the class without modelling; the failing shapes are rare, hand-written casings.

## Hints
- The guard reads every build file below the root, not only tracked ones; an untracked or half-written `*.csproj`/`*.props` in a working copy outside `bin`/`obj` makes the test fail or throw locally. CI checks out only tracked files, so it is not a CI risk.
- A `RoslynVersion` definition inside a `Target`'s `PropertyGroup` in the root file is reported as "defined under a condition"; it fails as it should, but the message names the wrong reason.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
