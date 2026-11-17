Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the two changed test files and the helper: 0 of 3 changed.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- Doc-comment repair (`00-lead.md` decision 3, FC-031): `FormatChainAsync` and `GetContextNode` in `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs` no longer say "positional context"; they say the statement or member starting the chain's line is passed to the formatter as context, the chain keeps its column as a fact (FMT-PIPE-017), and the same context scopes the `IsFormattingRefused` check.
- `scripts/verify-text-only.sh --base HEAD(9ebf722) --head worktree`: `TEXT-ONLY PROOF: PASS — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only)`; changed trivia: documentation.
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Focused tests (RH5201 + RH5112 analyzer test classes): Passed 140, Failed 0. (The test implementer counted 141; the count here is from the same filter.)

## Follow-up candidates
_None._
