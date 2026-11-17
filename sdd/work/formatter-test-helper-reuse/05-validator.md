Role: validator
Commit: 7b1b8fe97cd34d4030393126ed3a4221a81195af
Verdict: pass

## thoenissen/Reihitsu#1847
Covered by the whole-run results below.

## thoenissen/Reihitsu#1848
Covered by the whole-run results below.

## Whole run

## Steps
| Step | Result | Notes |
|---|---|---|
| scripts/prepare.sh | ok | .NET 11.0.100 already present at /root/.dotnet |
| scripts/build.sh | ok | 0 warnings, 0 errors |
| scripts/test.sh --no-build | ok | Analyzer 10639, Formatter 5746, Core 738, Cli 385, ArchitectureTests 198, Tooling 133, Playground 23; all passed, 0 failed, 0 skipped |

## Failures
None.

## Log
/root/validator-formatter-test-helper-reuse/test.log (build log: /root/validator-formatter-test-helper-reuse/build.log)
