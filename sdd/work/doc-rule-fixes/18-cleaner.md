Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 4 file(s)" for the four C# files changed since `f6cd7c7`. The comment fix was made by hand.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor 15 finding #5 (inline comment) | `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs`, partial-member branch | Comment text now says later comments are read from the part's syntax unless one of its comments holds an include element, in which case the part's whole documentation is expanded on its own. |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/verify-text-only.sh --strict-docs --base HEAD --head worktree`: TEXT-ONLY PROOF: PASS — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: none
- Focused tests not run: the only change is comment text, proven text-only.
