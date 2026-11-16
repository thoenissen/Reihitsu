Role: auditor
Commit: ec2b4632b89c1d3dd3d9b641a89a59d11936c034
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Delta audit of decision 7 (`1ef7ef0`) against audit 2 on `61cf6e1` (`09-auditor.md`). Head `ec2b463` (checked out, clean tree); `origin/main` and `git ls-remote origin refs/heads/main` are both `eb89003`, an ancestor of the head. Outside `sdd/work/`, the head tree equals `1ef7ef0`; the delta since `61cf6e1` is `RoslynVersionOwnership.cs` (one case-insensitive name rule, `IsNamed`/`FindAttributes`, exactly one `Version` attribute) and eight tests in `RoslynPackageVersionTests.cs`, as `10-test-implementer.md` describes. No build file, script, or production code changed since audit 2, so the #898 change and the `RoslynVersion` property keep their earlier evidence. `scripts/lane-check.sh tooling`: confirmed. The cleaner's build and the 26 focused tests are on this tree.

## Previous findings
| # | Status | Why |
|---|---|---|
| 1 | closed | Unchanged since audit 2. |
| 2 | closed | Every MSBuild name the guard reads now goes through one `OrdinalIgnoreCase` rule (`:92`, `:96-98`, `:119`, `:131-133`, `:170-171`). (e) `PackageReferenceItemTypeInAnotherCasingIsReported`, `PackageVersionItemTypeInAnotherCasingIsReported`; (f) `VersionMetadataElementInAnotherCasingIsReported`, `…OnUpdateItemIsReported`; (g) `SecondVersionAttributeInAnotherCasingIsReported`; the accepted side `ItemNamesInAnotherCasingNamingTheVersionPropertyAreAccepted`. Traced: each fails on `61cf6e1`'s ordinal reads and passes on the head. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs:82-84`, `:119-127`; class summary `:11-17` | FC-044, FC-001, FC-031 | #2 (and #1) | PR-introduced | The same decision as #1 and #2 — which build-file content sets the Roslyn version a project restores — fails between the guard and MSBuild on axes neither enumeration named: how an item names its package, which files define items or the property, and when an item is changed. Constructed in a scratch tree with the root `RoslynVersion` at `5.12.0`, the head's `RoslynVersionOwnership.Check` (compiled unchanged into a scratch runner) and `dotnet msbuild -getItem:PackageReference`; each MSBuild result is `Microsoft.CodeAnalysis.CSharp` `5.6.0`, each guard result is 0 violations: **(h) identity through a property** — `<PackageReference Include="$(CSharpPackage)" Version="5.6.0" />` with `CSharpPackage=Microsoft.CodeAnalysis.CSharp` (`IsRoslynPackage("$(CSharpPackage)")` is false, item not even collected); **(l) identity through an item** — `<RoslynPackage Include="Microsoft.CodeAnalysis.CSharp" /><PackageReference Include="@(RoslynPackage)" Version="5.6.0" />`, same; **(i) target-time modification** — the item states `Version="$(RoslynVersion)"`, and `<Target Name="PinRoslyn" BeforeTargets="CollectPackageReferences"><ItemGroup><PackageReference Condition="'%(Identity)' == 'Microsoft.CodeAnalysis.CSharp'" Version="5.6.0" /></ItemGroup></Target>` changes it (no `Include`/`Update`, so `:121` returns; `dotnet restore` writes `Microsoft.CodeAnalysis.CSharp/5.6.0` to `project.assets.json`); **(k) import with another extension** — `<Import Project="roslyn.msbuild" />` whose file holds `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />` (`:83` reads only `.csproj`/`.props`/`.targets`); **(m) response file** — a root `Directory.Build.rsp` holding `-p:RoslynVersion=5.6.0`, which MSBuild applies as a global property over the props definition (the guard reads no `.rsp`). Members that hold: every literal identity in a read file, in any casing, `Include` or `Update` (#1, #2); a property definition anywhere in a read file. The class summary states "any other form is reported, whatever version MSBuild would compute for it"; (h), (i), (k), (l), (m) are other forms and are not reported. Every member is deliberate indirection nobody writes by accident, and the repository uses none of them (`rg -i roslynversion` finds only the root definition and the eleven items; no `.rsp`, `.proj`, or `.msbuild` file is tracked). | The class has now produced three findings, one axis each; per *Architecture findings* refuse by the cause, not by these cells. The cause is that the guard recognizes only what is written literally in the files it reads. Either (1) read the computed result instead of, or next to, the text check — the restored `obj/project.assets.json` of each project already lists the resolved `Microsoft.CodeAnalysis*` versions and covers every member above, with no process dependency once restore has run; or (2) accept the boundary and state it: correct the class summary to say what the guard does not see (indirection through properties, items, imports of other files, targets, response files) and leave those members unchecked. A cell-by-cell refusal (`$(`/`@(` in identity, items inside `Target`, more extensions, `.rsp`) is the loop #1 and #2 already ran. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which elements are package items | Elements named `PackageReference`/`PackageVersion` in any casing, anywhere in the read files | MSBuild item types, case-insensitive | Agree (closed #2 (e)) | Holds |
| Which items name a Roslyn package | Literal `Include` else `Update`, any casing, split on `;`, `IsRoslynPackage` | MSBuild evaluates `$(…)`, `@(…)`, and target-time batching before NuGet reads the identity | Disagree on non-literal identity and identity-less target modifications | Finding #3 (h), (l), (i) |
| Which metadata states the version | Exactly one `Version` attribute in any casing equal to `$(RoslynVersion)` (ordinal value), no `VersionOverride` attribute, no `Version`/`VersionOverride` child, any casing | MSBuild metadata names case-insensitive | Agree; a value `$(roslynversion)` is reported though MSBuild resolves it — loud, strict, decision 6's form | Holds |
| Which files define items and the property | `.csproj`/`.props`/`.targets` below the root, `bin`/`obj`/dot directories skipped | MSBuild imports any extension; `Directory.Build.rsp` adds global properties | Disagree on other extensions and response files | Finding #3 (k), (m) |
| Where `RoslynVersion` may be defined | `RoslynVersion` child of `PropertyGroup`, both any casing; one definition, root file, no `Condition` (any casing) on it or its group, group directly under the root | MSBuild property names case-insensitive; `PropertyGroup`/`Condition` case-sensitive (another casing fails evaluation) | Agree; reading reserved names case-insensitively only adds a report to a file MSBuild rejects | Holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Roslyn version of the projects | `Directory.Build.props:28` `RoslynVersion`; eleven `Version="$(RoslynVersion)"` items in `src/**/*.csproj` (`rg -i roslynversion`, excluding the architecture tests) | One owner; the `scripts/proof/verify-text-only.cs` pin stays the recorded follow-up (decision 4) |
| Case handling of MSBuild names inside the guard | `IsNamed`, `FindAttributes`; `IsRoslynPackage` (package ids, `OrdinalIgnoreCase`); no remaining ordinal name read (`rg 'Attribute\(|Name.LocalName' RoslynVersionOwnership.cs`: only `IsNamed`, `FindAttributes`, and the message text at `:139`) | One rule, one statement — #2's two statements are gone |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ItemNamesInAnotherCasingNamingTheVersionPropertyAreAccepted` | Lower- and upper-case item types and `Version` attributes naming the property are collected and accepted | A violation, or an item count other than 2 (0 on `61cf6e1`) | fixture | Holds |
| `PackageReferenceItemTypeInAnotherCasingIsReported`, `PackageVersionItemTypeInAnotherCasingIsReported` | An item type in another casing is read and its literal reported | 0 violations (as on `61cf6e1`), or another file:line/type/package | `AssertSingleViolation` (exact count, `StartsWith` location, type as written, package) | Holds |
| `VersionMetadataElementInAnotherCasingIsReported`, `…OnUpdateItemIsReported` | A `<version>` child next to the property is reported, on `Include` and on `Update` items | 0 violations; for the `Update` test the location `src/B/B.props:1` pins the `Update` item, not the `Include` one | `AssertSingleViolation` | Holds |
| `VersionOverrideAttributeInAnotherCasingIsReported` | A `versionoverride` attribute is reported | 0 violations | `AssertSingleViolation` | Holds |
| `SecondVersionAttributeInAnotherCasingIsReported` | Two version attributes in different casings are reported | 0 violations (on `61cf6e1` the exact `Version` passed) | `AssertSingleViolation` | Holds |
| `LoneVersionAttributeInAnotherCasingWithLiteralIsReported` | Boundary of (g): one attribute, another casing, literal | 0 violations | `AssertSingleViolation` | Holds (passes on both trees by design, against the accepted side) |
| `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` | No Roslyn version on the real tree outside the property | — | `RoslynVersionOwnership.Check` | Weaker than its summary for finding #3's members |

The definition-side switches to `IsNamed(…, "PropertyGroup")` and `FindAttributes(…, "Condition")` have no own fixture; their other casings fail MSBuild evaluation (MSB4067, MSB4066), so no accepted build file depends on them. Not a finding.

## Failure classes dismissed
- FC-002, FC-003: no requirement qualifier; the accepted and reported sides of the casing rule each have a fixture, and the lone-attribute boundary is tested.
- FC-015, FC-016: one name rule, one reader; sibling item types as in audit 2 (`PackageDownload`, `GlobalPackageReference`).
- FC-017: holds for the delta; the real-tree test's gap is finding #3.
- FC-031: `IsNamed`, `FindAttributes`, and the changed lines carry accurate summaries; the class summary's "any other form is reported" is part of finding #3. No `TODO`, no tracker number (`rg 'TODO|#[0-9]+'` on both files: empty).
- FC-032: the cleaner's build and focused run are on `8e82a75`, whose tree equals the head outside `sdd/work/`.
- FC-033-FC-035, FC-037: hand-written fixtures; expected values are location prefixes, not taken from patched code; no comment-only claim.
- FC-004-FC-014, FC-018-FC-030, FC-036, FC-038-FC-043: no analyzer, code fix, formatter phase, trivia, or C# rewrite is touched.

## Architecture findings
Finding #3 repeats the class of #1 and #2: the guard and MSBuild evaluation share one decision — which build-file content sets the Roslyn version a project restores — and the guard answers it by reading files as written. #1 found it on item form and placement, #2 on name casing, #3 on indirection (properties, items, imports of other extensions, targets, response files). Decision 7 closed the casing axis by its cause; the remaining axes share a different cause, the guard's literal reading itself, which no cheap name or syntax check removes without listing cells. Ending the loop takes either the computed result (`obj/project.assets.json`) or an explicit, documented boundary; the lead decides which.

## Hints
- Unchanged from audit 2: the guard reads untracked build files in a working copy; a `RoslynVersion` definition inside a `Target`'s `PropertyGroup` is reported as "defined under a condition".

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
