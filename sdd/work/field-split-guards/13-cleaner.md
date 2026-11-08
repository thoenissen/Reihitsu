Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over every changed C# file reported 0 files changed after the comment edit.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Text-only proof: `TEXT-ONLY PROOF: PASS — base=a7f868a head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: none`.
- No tests run: the change is comment-only, proven above.

## Issue #963
The stale comment in `FieldDeclarationSplitTransform.CreateSplitField` now states what the amended FMT-STRUCT-030 lets reach the first declarator's leading trivia: a comment on its own line in front of the name and, in `Parse`/`Diagnose` mode, a documentation comment behind the type on the type's line. Comment only.

## Issue #1384
No change by the cleaner.

## Whole run
Only the `CreateSplitField` comment changed (audit hint, decision 11).
