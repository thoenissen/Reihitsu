Role: cleaner
Commit: see git log (commit carrying this file)
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 2 files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Repairs (comment-only)
- Decision 13: the remarks of `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync` in `src/Reihitsu.Formatter/ReihitsuFormatter.cs`, and the summaries of `FormattingContext.FormatsRegionDirectives` and its constructor parameter in `src/Reihitsu.Formatter/Data/FormattingContext.cs`, now say that no region requirement is applied and that a tab between the tokens of a region directive still becomes four spaces (FMT-PIPE-028).
- Decision 15: the `FormatsRegionDirectives` summary now says the flag defaults to true, so every context built without the argument keeps it, and only the three node-level entry points set it to false.

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- TEXT-ONLY PROOF: PASS - base=abe7fab head=worktree - 2 changed file(s) (0 non-compiled, 2 C# comment/layout-only)
