Role: cleaner
Commit: see `git log -1` on the branch (the commit that adds this file)
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 26 changed C# files: "Formatted 0 of 26 file(s)." (Without `--force` the script only asks for confirmation on a non-interactive input.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh` (Release): Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project formatter --filter ReihitsuFormatterDocumentationElementTests`: passed (see run output).
- Text-only proof: not applicable, no comment fixes.
