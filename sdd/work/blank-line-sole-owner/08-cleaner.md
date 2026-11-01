Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ The formatter changed no file.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

Comment fix (lead decision 14): the XML comment of `BlankLineOnlyAfterTrailingDirectiveBeforeElseIsRemoved` in `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineBeforeIfDirectiveAfterStatementTests.cs` described `TokenGapNormalizer` node-scoped calls; it now says the line-break phase writes line breaks only and keeps blank lines, so `BlankLineTokenCleanupRewriter` is the sole owner. Other comments in the run's changed source files were checked; none are stale.

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s).
Text-only proof (no strict-docs): see verdict line below. `--strict-docs` reports the file because the change is to the XML doc of a public test method, which is expected for a doc-comment edit; no token changed.
TEXT-ONLY PROOF: PASS — base=6f38281 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), including public API documentation
