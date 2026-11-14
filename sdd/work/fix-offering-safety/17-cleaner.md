Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ The formatter changed 0 of the 11 files changed by repair round 2.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| MSTEST0068 (error in Release) | `src/Reihitsu.ArchitectureTests/Analyzer/FixAllProviderCreationTests.cs:57` | `CollectionAssert.AreEquivalent` replaced by `Assert.AreSequenceEqual`, expected list written in the helper's ordinal order (base, then `DocumentOrderFixAllProvider`); same two expected types, same assertion meaning |

## Unresolved
_None._ (The S1125 lines printed by the formatter's own build of `FormatCommandHandler.cs` are not part of the Release solution build, which reports 0 warnings.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~FixAllProviderCreationTests"`: Passed 1/1.
