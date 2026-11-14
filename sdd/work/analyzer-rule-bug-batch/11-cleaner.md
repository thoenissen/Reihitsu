Role: cleaner
Commit: see git log (commit "cleaner: IsInsideLiteralText position doc")
Verdict: clean

## Formatted
_None._ Comment text only.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding #1 | src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs, `IsInsideLiteralText` `<param name="position">` | "Position of the tab" became "Position to classify: the position of a tab, or the start of a whitespace run" |

## Unresolved
_None._

## Checks
- `--strict-docs` run: `TEXT-ONLY PROOF: FAIL — base=d75cd95 head=worktree — 1 of 2 changed file(s) carry compiled behavior; xml-doc impact: 1 file(s), including public API documentation`. By design strict-docs flags any public API documentation change; the only change is the param doc text.
- Without strict-docs: `TEXT-ONLY PROOF: PASS — base=d75cd95 head=worktree — 2 changed file(s) (1 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), including public API documentation` (1504 tokens identical, changed trivia: documentation).
- No build or tests run (comment only).
