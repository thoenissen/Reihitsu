Role: test implementer
Commit: (this commit; see `git log` for "Add build-configuration test for #1880")
Verdict: tests written

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.SelfHosting.Test.Repository.BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` (replaces `RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers`) | FC-003 (both sides: item absent, item conditioned), FC-017 | MSTest over the parsed root `Directory.Build.props` / `Directory.Build.targets`; new private helper `ResolveBuildFilePath` | Red: `expected count: 1, actual count: 0` |

The test asserts that the root build files hold exactly one `AdditionalFiles` item whose `Include`, with `$(MSBuildThisFileDirectory)` expanded to the build file's directory, resolves to the root `reihitsu.json`, and that neither the item nor any enclosing element (`ItemGroup`, `Choose`/`When`, ...) carries a `Condition`, so the build passes the file in every project and every configuration, not only in Release. No tag: the test verifies tooling, no requirement.

The class summary was updated to say the build and the self-hosting scan both pass the file. The other five tests of the class pass unchanged.

## Scenarios (bug fix)
Not a bug fix. Probes run on this head with a scratch edit of `Directory.Build.props` (an unconditional `<AdditionalFiles Include="$(MSBuildThisFileDirectory)reihitsu.json" />` next to the package import), all reverted:

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| New test, item added unconditionally | n/a | self-hosting test | green | as expected |
| New test, same item inside `<ItemGroup Condition="'$(Configuration)'=='Release'">` | n/a | self-hosting test | red on `IsEmpty(conditionedItems)` | as expected |
| `dotnet build src/Reihitsu.Core -c Release --no-incremental` with the item | n/a | Release build, `binaries/` 1.5.0 analyzers | succeeded, 0 warnings, no RH0001 | the rebuilt RH0001 accepts the `design` section; the issue's blocker is lifted |
| Same build with the root limit lowered to `maximumTypeLines: 20` | n/a | Release build | 74 × `error RH2111` | the file reaches the binary analyzers and the type limit fails the build |

## Not covered, and why
- "A type over 500 lines fails the build" is not pinned by a committed test: it is a property of the Release build (the probe above shows it), and the build of every project is what CI and the validator run. A test that builds a project from inside a test would duplicate CI.
- "No RH0001 report" on the root file is covered by the Release build itself (`Release.ruleset` sets RH0001 to Error, `TreatWarningsAsErrors`), and by `RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred`, which pins the file's content.
- Failure classes: the catalog targets formatter, analyzer, and code-fix changes (trivia, convergence, idempotency, parity, precheck); none reaches an MSBuild item. FC-003 and FC-017 apply and are covered; FC-035 does not apply (expected value is the root file's path, not taken from patched code).
- Line endings: no layout involved.

## Commands
```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Repository.BuildConfigurationTests"
```

## Follow-up candidates
_None._ Notes for the implementer, inside the run's scope: the comment above the package import in `Directory.Build.props` and `scripts/README.md` line 103 ("The analyzers the solution build loads from `binaries/` do not receive the root `reihitsu.json` ... judged only by the self-hosting scan, never by `scripts/build.sh`") state the old behavior and go stale with the change.
