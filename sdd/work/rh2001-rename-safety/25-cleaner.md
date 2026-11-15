Role: cleaner
Commit: see git log (parent 071e850)
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | Auditor finding #6: `<returns>` of `TupleTypeKeepsElementName` rewritten to match the code and RH2001-011 ¶3 (false for a deconstruction target, nested included, or where the tuple's own conversion gives the element no or another name; true elsewhere, where the fix is withheld). |

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- scripts/format.sh: Formatted 0 of 1 file(s).
- TEXT-ONLY PROOF: PASS — base=071e850 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only)
