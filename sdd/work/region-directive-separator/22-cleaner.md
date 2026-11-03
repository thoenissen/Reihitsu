Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 2 files. Comment-only edits (audit findings 6a and 7); the change doc was left alone.

- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs`: the class summary now says the node-level entry points apply no region requirement (FMT-PIPE-028).
- `src/Reihitsu.Formatter/ReihitsuFormatter.cs`: the `FormatNodeInDocumentWithContextAsync` remark restores "because this entry point serves code fixes; for the same reason, no region requirement is applied (FMT-PIPE-028)", with the lines re-wrapped evenly.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/verify-text-only.sh --base 3010b6a --head worktree`: TEXT-ONLY PROOF: PASS — 3 changed file(s) (1 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s).
- No tests re-run: comment-only change.
