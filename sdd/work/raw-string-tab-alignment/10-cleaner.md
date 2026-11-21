Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ Only two `<param name="edit">` texts changed.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Audit finding 1 (FC-031) | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`, `ApplyLeadingTriviaEdit` and `MoveLineInsideTrivia` | Both `edit` parameter docs now name the fourth part, as `ReplaceIndentation` does |

## Unresolved
_None._

## Checks
TEXT-ONLY PROOF: PASS — base=1a0127e head=worktree — 3 changed file(s) (2 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)

## Follow-up candidates
_None._
