Role: cleaner
Commit: 6b81bd1 (comment fix committed on top)
Verdict: clean

## Formatted
_None._ Comment-only change to `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/DeclaredNameSpacingRule.cs`: the summary now names fields and event fields (with local, `for`, `fixed`, `using` declarations) as covered, and states that method, local function and delegate return types, property and indexer types, and the type of an event with accessors are not decided by the rule. Matches FMT-SPACE-021. The inline comments carried no claim about these types, so none changed.

## Warnings resolved
_None._

## Unresolved
_None._ (format.sh's own CLI build printed S1125 at FormatCommandHandler.cs:452, a file this run did not change; the Release build is warning-free.)

## Checks
- `scripts/format.sh`: Formatted 0 of 1 file(s).
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- TEXT-ONLY PROOF: PASS — base=6b81bd1 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only)
