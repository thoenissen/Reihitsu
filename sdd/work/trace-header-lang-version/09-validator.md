Role: validator
Commit: 7a6163d
Verdict: fail

## Steps
| Step | Result | Notes |
|---|---|---|
| scripts/prepare.sh | ok | SDK 11.0.100 present at /root/.dotnet; nothing installed |
| scripts/build.sh | ok | 0 warnings, 0 errors |
| scripts/test.sh --no-build | ok | Analyzer 12064/12064; Formatter 7489/7489; Core 887/887; Cli 467 passed, 1 skipped, 0 failed (468); ArchitectureTests 198/198; Tooling 227/227; Playground 103/103 |
| line-count rows | failed | `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"`: 16 passed, 2 failed of 18 |

## Failures
Both failures are `AnalyzerReportsNoViolationsOnOwnSourceCode` in the self-hosting project, the rows for `RH2008MethodsShouldNotExceedTheLineLimitAnalyzer` and `RH2111TypesShouldNotExceedTheLineLimitAnalyzer`. Same assertion output for both:

```
Assertion failed.
Self-hosting compilation for project 'Reihitsu.Tooling' reported 4 error(s). This usually indicates missing references or invalid source:
src/Reihitsu.Tooling/CodeFixRunCommand.cs:3 [CS0234] The type or namespace name 'Formatter' does not exist in the namespace 'Reihitsu' (are you missing an assembly reference?)
src/Reihitsu.Tooling/CodeFixRunCommand.cs:325 [CS0103] The name 'LanguageVersionResolver' does not exist in the current context
src/Reihitsu.Tooling/CodeFixRunCommand.cs:498 [CS0103] The name 'LanguageVersionResolver' does not exist in the current context
src/Reihitsu.Tooling/CodeFixRunCommand.cs:499 [CS0103] The name 'LanguageVersionResolver' does not exist in the current context
```
Stack trace top frame: `Reihitsu.SelfHosting.Test.Utilities.SelfHostingSolution.AssertNoCompilationErrors` at tests/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs:line 492.

Note: the default run (`test.sh --no-build`) passed in every project. These two rows are the only failures.

## Log
/tmp/claude-0/-home-user-Reihitsu/01e33523-dd2e-50ff-a6bf-e81e7aec6721/scratchpad/test.log (default run)
/tmp/claude-0/-home-user-Reihitsu/01e33523-dd2e-50ff-a6bf-e81e7aec6721/scratchpad/linecount.log (line-count rows)
/tmp/claude-0/-home-user-Reihitsu/01e33523-dd2e-50ff-a6bf-e81e7aec6721/scratchpad/build.log (build)
