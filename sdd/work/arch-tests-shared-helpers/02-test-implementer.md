Role: test implementer
Commit: 43c71daeb0aba01c38f995c0bc92f8c1e60595c4
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. In the `tests` lane I wrote the whole change, the deduplication included, following `00-lead.md` decisions 1–5.

## Change

- **Root lookup.** New file `tests/Reihitsu.ArchitectureTests/Helpers/RepositoryRoot.cs` with `RepositoryRoot.Find()`. It is the same walk as before, moved there unchanged.
  - All 11 private `FindRepositoryRoot` copies are deleted, and their callers call `RepositoryRoot.Find()`.
  - `ApplyFixWrapperPositionalArgumentTests.FindScript` keeps its name, but its body is now `Path.Combine(RepositoryRoot.Find(), "scripts", scriptName)`.
  - The `#region Methods` in `DocumentationCommentNormalizationDeterminismTests` held only the deleted copy, so it was empty (RH7310) and is removed.
- **bin/obj check.** New file `Helpers/BuildOutputDirectories.cs`. It has two members:
  - `IsBuildOutputDirectoryName(name)` matches `bin` or `obj` with OrdinalIgnoreCase.
  - `Contains(root, filePath)` looks only at the directory segments of the path relative to `root`.

  All 7 sites call it:
  - `SelfReferentialTrackerReferenceTests.IsExcludedFromScan(root, file)`
  - `ResourceTextLiteralTests.ScanForResourceTextLiterals`
  - `RoslynVersionOwnership.IsSkipped(root, path)`, which keeps its own every-dot-directory rule
  - `SquadNoteReferenceTests.EnumerateScannedFiles`, which keeps its `.git` prune as the new const `VersionControlDirectory`
  - `PhaseOwnershipDocumentationTests.CollectFormatterDeclarations`
  - `SpecCatalog.EnumerateProductionFiles`
  - `RequirementReferenceCatalog`

  Decision 2 holds: the dot-directory rules stay with their scans. Decision 3 holds: the tracker scan now looks only at the path relative to the scanned root.
- **Fixture.** New file `Helpers/FixtureDirectory.cs`, a rename of `TrackerReferenceFixtureDirectory`.
  - Its root comes from `Directory.CreateTempSubdirectory("ReihitsuArchitectureTest")`.
  - `WriteFile(relativePath, content)` accepts `/` or the platform separator and returns the full path.
  - The types `RoslynVersionFixtureDirectory`, `TrackerReferenceFixtureDirectory`, `SpecFixtureDirectory`, and `ApplyFixWrapperFixtureDirectoryScope` are deleted.
  - Decision 5 keeps the domain writers with their scans:
    - `WriteSpec` is now an extension in `Sdd/SpecFixtureExtensions.cs`.
    - `CreateFixture` is now a private static helper in `ApplyFixWrapperPositionalArgumentTests`.
  - The skeptic's optional scope (*Internal choices* 6) is taken: the 7 inline `try/finally` temp roots in `SquadNoteReferenceTests` and its private `WriteFile` now use `FixtureDirectory`.
- **Stale comment.** The cref on `SelfReferentialTrackerReferenceTests._generatedFileSuffixes` pointed at the deleted `FindRepositoryRoot` (FC-031). That clause is removed. The `IsExcludedFromScan` summary is rewritten for the relative basis.
- **Usings.** After the changes, `RoslynPackageVersionTests` no longer uses `System.IO` and `SquadNoteReferenceTests` no longer uses `System.Linq`. I removed both. I checked every changed file by removing each `System*` using in turn and rebuilding.
- **Remaining text matches.** After the change, `rg` for the old names in `tests/Reihitsu.ArchitectureTests` finds no declarations or crefs. It still matches in other test projects and in `FormatCommandHandler`, which are out of scope (decision 4).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Helpers.BuildOutputDirectoriesTests.FileBelowBuildOutputDirectoryIsBuildOutput` (bin, obj) | FC-003 (skip side) | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.FileDirectlyInBuildOutputDirectoryIsBuildOutput` (bin, obj) | FC-003 | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.FileOutsideBuildOutputDirectoryIsNotBuildOutput` | FC-003 (keep side) | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.BuildOutputDirectoryInOtherCasingIsBuildOutput` (Bin, BIN, Obj, OBJ) | FC-003, case rule (decision 2) | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.DirectoryNamedLikeBuildOutputIsNotBuildOutput` (binaries, objects, cabin, _obj) | FC-003, whole-segment match | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.FileNamedLikeBuildOutputIsNotBuildOutput` (bin, obj) | FC-003, directories only | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.BuildOutputDirectoryAboveRootIsIgnored` (bin, obj) | FC-003, relative basis (decision 3) | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.ForwardSlashPathIsSplitIntoDirectories` | both separators | direct call | pass |
| `Helpers.BuildOutputDirectoriesTests.IsBuildOutputDirectoryNameDecidesSingleNames` (7 rows) | name predicate used by the pruning walk | direct call | pass |
| `General.SelfReferentialTrackerReferenceTests.CommentScanReadsRootUnderBuildOutputDirectory` | regression for the absolute-path vacuous pass (decision 3) | `FixtureDirectory` + `ScanForCommentReferences` | pass |

The project has no spec, so no test carries a requirement tag.

Each new test fails for the right reason under a mutant of `BuildOutputDirectories.Contains`. Every mutant was reverted, and the file matches the committed version byte for byte.
- **Split the absolute path instead of the relative one.** `CommentScanReadsRootUnderBuildOutputDirectory` fails on `Assert.HasCount(1, findings)` with 0 findings. Both `BuildOutputDirectoryAboveRootIsIgnored` rows fail.
- **Ordinal instead of OrdinalIgnoreCase, plus the file-name segment included.** 8 tests fail: the 4 casing rows, the 2 file-name rows, and the `Bin`/`OBJ` name rows.

Full run: `Passed: 247` (the baseline was 221, plus these 26). The full run also passes with `TMPDIR` under an `obj` directory, which is the skeptic's reproduction command. Before the change, that command made `CommentScanExcludesBuildOutputAndGeneratedFiles` fail. It now passes along with the other 246.

## Scenarios (bug fix)
_N/A._ This is not a bug-fix lane. The adjacent defect from decision 3 has its regression test above, and the `TMPDIR`-under-`obj` run now passes.

## Not covered, and why
- **Root lookup and fixture lifecycle.** Every repository-wide test (root) and every fixture self-test (fixture) already pins them, and all 247 pass. No new test is needed (skeptic, *Candidate inputs*).
- **Each scan's own bin/obj exclusion.** It stays pinned by the existing tests the skeptic lists:
  - `CommentScanExcludesBuildOutputAndGeneratedFiles`
  - `ScanExcludesBuildOutputDirectories`
  - the two Roslyn `…BuildOutputAndDotDirectoriesAreSkipped` tests
  - `ScanIgnoresReadmePlaceholdersNotesAndBuildOutput`
  - `CodeOwnerCheckIgnoresTestProjectsAndBuildOutput`
  - `GuardCheckReportsNameDeclaredOnlyInBuildOutput`
  - `CoverageCheckCountsOnlyTestMethodsInTestProjects`
- **A per-site case-variant test.** All 7 sites call the one `BuildOutputDirectories` member (`rg -n "BuildOutputDirectories\.(Contains|IsBuildOutputDirectoryName)\(" tests/Reihitsu.ArchitectureTests --glob '!**/Helpers/**'` gives 7 call sites in 7 files: 6 call `Contains`, the squad-note walk calls `IsBuildOutputDirectoryName`). The case rule is therefore pinned once, at the shared check, as the skeptic recommended.
- **The `.git` prune of the squad-note scan.** No test pinned it before (skeptic), and this change does not alter it: it only moves the literal into a const. Writing a `.git` fixture is outside the issue.
- **Formatter, code-fix, trivia, and unsupported-shape failure classes, line endings, idempotency.** None can reach this change, which is test-project code only.

## Commands
```shell
scripts/test.sh --project architecture
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Helpers.BuildOutputDirectoriesTests|FullyQualifiedName~SelfReferentialTrackerReferenceTests.CommentScanReadsRootUnderBuildOutputDirectory"
```

## Follow-up candidates
_None._ The root lookups in other test projects were dropped by decision 4.
