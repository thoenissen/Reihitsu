Role: lead
Commit: 74fa119
Verdict: CI repair (repair round 3 of 3)

# Run: thoenissen/Reihitsu#1489 — CI repair

PR thoenissen/Reihitsu#1520, head `74fa11985eb2ea4d98d5ddb81e1c7b12345ee6dc`. Lane: behavior change. The earlier run's handovers were deleted at publish; they are in git history before `74fa119`.

## CI finding

Check `sonarcloud` (job 124609642446) failed in `Reihitsu.SelfHosting.Test`: 340 of 3689 tests fail, all with the same self-hosting compilation error for project `Reihitsu.Analyzer.Test`:

```
src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs:145 [CS0012] The type 'FluentChainBoundary' is defined in an assembly that is not referenced. You must add a reference to assembly 'Reihitsu.Core, ...'.
```

The same CS0012 is reported at six more places: `RH5021…FormatterTests.cs:210,229`, `RH5029…FormatterTests.cs:154`, `RH5030…FormatterTests.cs:572`, `RH5111…FormatterTests.cs:118`, and `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs:2281`. `build-and-test`, CodeQL, and `submit-nuget` are green. `FluentChainBoundary` is a Core type this PR added (round 1, commit 75ee131); the failure is PR-caused.

## Maintainer clarifications

Only end-user-visible output reaches the maintainer; the formatter's public methods are not a public API (`AGENTS.md`, *Supported inputs*).
