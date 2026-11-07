Role: cleaner
Commit: 61708eef
Verdict: clean

## Formatted
_None._ (formatter changed 0 of 1 files)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S2583 (SonarCloud) | src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs GetFixedDocumentAsync | CodeFixContext construction, registration and first-match selection moved into private static GetFirstMatchingActionAsync returning Task<CodeAction>; the null test now tests a method result. Behavior unchanged. |

## Unresolved
_None._ Whether SonarCloud still flags it can only be seen on the next CI run.

## Checks
scripts/build.sh: Build succeeded, 0 Warning(s), 0 Error(s)
analyzer tests: Passed! - Failed: 0, Passed: 7005, Skipped: 0, Total: 7005
