Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Decision 11 carried out as a comment-only change. The comments no longer restate what the carry keeps; they point to `BlockWrapTriviaUtilities` and name only why a directive blocks the wrap (discarded, or moved onto a line it does not own).

## Formatted
_None._ The formatter changed 0 of 2 files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Changed: `SwitchCaseBraceRewriter.cs` (`SectionCarriesDirectives` summary, `AddBraces` comment) and `ControlFlowBraceTransform.cs` (`CanWrap` summary).
- `TEXT-ONLY PROOF: PASS — base=8e6bb0b head=worktree — 3 changed file(s) (1 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s), no public API documentation`
