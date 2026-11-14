Role: cleaner
Commit: see git log
Verdict: clean

Prompt carried no intended fix; wording re-derived from the code (one fact, `FluentChainBoundary` holds only the root-parent-continues flag, read for the root itself).

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| none | `src/Reihitsu.Formatter/Data/FormattingContext.cs` | summary of `RootChainBoundary` and the `rootChainBoundary` parameter text now describe one fact, read for the root itself |

## Unresolved
_None._

## Checks
- `--strict-docs` run: FAIL, "public API documentation changed" — the tool classifies the `public` property of an `internal record` as public API documentation. Auditor finding #2 states it is not public API (no assembly is, per AGENTS.md); XML doc text only.
- Run without `--strict-docs`: see the verdict line quoted in the report.
