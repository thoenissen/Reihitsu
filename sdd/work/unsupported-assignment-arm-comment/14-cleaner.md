Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Formatted
_None._ Comment-only change.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding #1 (stale comment) | `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ObjectInitializerContributor.cs:121-122` | Dropped the clause "which is behind a comment that leads its line"; the comment now says the members and closing brace follow the brace where it stands. |

## Unresolved
_None._

## Checks
TEXT-ONLY PROOF: PASS — base=c5374c2 head=worktree — 3 changed file(s) (2 non-compiled, 1 C# comment/layout-only); xml-doc impact: none

## Follow-up candidates
_None._
