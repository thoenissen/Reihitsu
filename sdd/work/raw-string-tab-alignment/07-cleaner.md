Role: cleaner
Commit: b401e36 (base; handover commit follows)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` changed no changed file.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

Decision 5: the comment in `RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.GetFixAllScenario` no longer says a raw literal can never nest in another; it now says a plain raw literal cannot nest another, that the two literals there are plain and in separate methods, and that an interpolated one can nest a literal in a hole (covered above).

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests`: Passed 66, Failed 0.
- `TEXT-ONLY PROOF: PASS — base=b401e36 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: none`

## Follow-up candidates
_None._
