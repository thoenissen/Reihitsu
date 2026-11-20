Role: cleaner
Commit: c85a543 (plus this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 50 changed C# files: "Formatted 0 of 50 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Missing UTF-8 byte order mark (audit finding 3, `.editorconfig` `charset = utf-8-bom`) | `SyntaxTokenNavigationUtilities.cs`, `LongExpressionCostTests.cs`, `LongExpressionLayoutTests.cs` | BOM added; the new Core test file already had one |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- Core `SyntaxTokenNavigationUtilitiesTests`: Passed 9/9. Formatter `LongExpression*`: Passed 21/21.
- Text-only proof: see the commit message of this run's last check (BOM-only change on three files).

## Follow-up candidates
_None._ (301 existing `src` files lack a BOM; out of scope.)
