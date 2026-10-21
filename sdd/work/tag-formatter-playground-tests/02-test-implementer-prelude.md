Role: test implementer (prelude)
Commit: 293b104
Verdict: tests written — attribute declarations only, no test tagged

## Change

| File | Content |
|---|---|
| `src/Reihitsu.Formatter.Test/Helpers/RequirementAttribute.cs` | `RequirementAttribute` in namespace `Reihitsu.Formatter.Test.Helpers`, next to `FormatterTestsBase` |
| `src/Reihitsu.Playground.Test/RequirementAttribute.cs` | `RequirementAttribute` in namespace `Reihitsu.Playground.Test`, the project's only namespace |

Both are byte-for-byte copies of `src/Reihitsu.Cli.Test/Helpers/RequirementAttribute.cs` except for the namespace line: `TestCategoryBaseAttribute`, `AllowMultiple = true`, method target, `TestCategories = [id]`. All three existing declarations are identical apart from the namespace, and both projects use MSTest 4.5.1 like them. A test file under `Formatter.Test/Unit`, `Regression`, `Idempotency`, or `Integration` needs `using Reihitsu.Formatter.Test.Helpers;` to use the short `[Requirement]` form; Playground tests need nothing.

## Discovery by `Reihitsu.ArchitectureTests`

No registration is needed, and none was made:

- `RequirementReferenceCatalog.ReadReferences` parses the C# sources (not the assemblies) of every directory under `src/` whose name ends in `.Test` or `Tests`, skipping `bin/` and `obj/`. `Reihitsu.Formatter.Test` and `Reihitsu.Playground.Test` both match.
- It matches attributes by simple name `Requirement` / `RequirementAttribute`, with or without a qualifier, so the attribute's namespace is irrelevant. The ID must be the single string-literal argument, and the attribute must sit on a method that also carries `[TestMethod]`.
- `Assemblies.cs` loads `Reihitsu.Formatter.Test` (not Playground.Test) for ArchUnit rules; that list plays no part in requirement coverage.
- `EnforcedSpecPaths` is unchanged (`analyzer/`, `cli/`, `shared/code-fixes.md`, `shared/diagnostics.md`), as instructed.

Probe, reverted before commit: I tagged `PlaygroundFormatterTests.FormatReformatsMisindentedInput` with `[Requirement("PLAY-001")]` and `RegionNamingRewriterTests.CapitalizesRegionDescription` with `[Reihitsu.Formatter.Test.Helpers.Requirement("FMT-PROBE-999")]`. Results:

- `TestCategory=PLAY-001` ran 1 test in Playground.Test, and `TestCategory=FMT-PROBE-999` ran 1 test in Formatter.Test.
- `EveryRequirementReferenceNamesAnActiveRequirement` failed with exactly one finding, `src/Reihitsu.Formatter.Test/Unit/RegionFormatting/RegionNamingRewriterTests.cs:30: FMT-PROBE-999 heads no requirement in any spec`, and reported nothing for PLAY-001.

So tags in both projects are discovered and checked. After the revert, all 13 `RequirementCoverageTests` pass.

## Note for the lead

`sdd/README.md` (*Tests and requirements*) still says the attribute is declared in `Reihitsu.Analyzer.Test`, `Reihitsu.Cli.Test`, and `Reihitsu.Core.Test`, and its "Enforced today" list will go stale once `formatter/` and `playground/` are enforced. The lead's notes say other squads edit `sdd/README.md` in parallel, so I left the file alone. Updating that sentence is the lead's call; the same paragraph already says another project declares the attribute "when its tests are first tagged".

## Tests

None added or tagged in this stage.

## Not covered, and why

The failure-class walk does not apply: this stage changes no behavior and adds no test.

## Commands

```shell
scripts/build.sh                                   # Release, 0 warnings, 0 errors
scripts/test.sh --project playground               # 3 passed
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"   # 13 passed
scripts/test.sh --project formatter --filter "TestCategory=<ID>"     # runs every formatter test of one requirement once tagged
scripts/test.sh --project playground --filter "TestCategory=<ID>"
```
