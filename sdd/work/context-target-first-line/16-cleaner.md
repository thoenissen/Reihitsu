Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on `LayoutComputer.cs`: "Formatted 0 of 1 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

The summary of `LayoutComputer.UseTargetPosition` now says the entry point writes back only the target, with its own leading trivia, and keeps everything in front of the target as it stands in the document.

## Unresolved
_None._

## Checks
TEXT-ONLY PROOF: PASS — base=7be0173 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), no public API documentation

`--strict-docs` passed, so no run without it was needed. No build or tests were run: the change is comment-only.
