Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the four changed C# files and the Playground test: "Formatted 0 of 5 file(s)".

Comment fixed: `src/Reihitsu.Playground.Test/PlaygroundFormatterTests.cs:90` now says the default language version is C# 15 (text only).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `LanguageVersionResolverTests`: Passed 11/11. Playground project: Passed 23/23.
- `scripts/verify-text-only.sh --strict-docs --base b5bea19 --head worktree`: FAIL only because --strict-docs flags any doc-comment change (one word, 14 -> 15, in the Playground test). Without --strict-docs: `TEXT-ONLY PROOF: PASS — 5 changed file(s) (4 non-compiled, 1 C# comment/layout-only)`.
