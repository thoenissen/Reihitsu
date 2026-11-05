Role: cleaner
Commit: see git log
Verdict: clean

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes

| File | What changed |
|---|---|
| `src/Reihitsu.ArchitectureTests/Analyzer/CodeFixFormatterAccessTests.cs` | Lines 59 and 75: `CollectionAssert.Contains`/`DoesNotContain` became `Assert.Contains`/`DoesNotContain` (MSTEST0068), same assertion, same input, arguments reordered for the new API. |

`scripts/format.sh` over the 17 changed files: formatted 0 of 17.
`scripts/build.sh` (Release): 0 Warning(s), 0 Error(s).

## Tests run

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~CodeFixFormatterAccessTests"
```
Passed (see run output below in lead's CI).
