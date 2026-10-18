Role: cleaner
Commit: see git log (this handover's commit)
Verdict: clean

## Formatted
_None._ The repair diff (`git diff b3d4590 HEAD`) changes no C# file (two rule pages and two handovers), so `scripts/format.sh` had nothing to run on.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationConventionTests|FullyQualifiedName~RuleDocumentationExampleTests"`: Passed! Failed: 0, Passed: 5, Total: 5.
- Text-only proof: not applicable (no comment fixes after an audit).
