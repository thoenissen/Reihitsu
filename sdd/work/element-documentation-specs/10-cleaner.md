Role: cleaner
Commit: 1bf0fdd
Verdict: clean

Repair round. Finding 4 of `07-auditor.md` is fixed; the files changed by `09-test-implementer.md` were re-cleaned.

## Formatted
- `src/Reihitsu.Analyzer.Test/Base/InMemoryXmlReferenceResolver.cs`: added the UTF-8 BOM (`ef bb bf`, once) that `.editorconfig` requires for `[*.cs]`. No other byte changed.
- `scripts/format.sh` on that file and the two files of the repair (RH8001 and RH8013 test classes): `Formatted 0 of 3 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH80"`: `Passed! Failed: 0, Passed: 287, Skipped: 0, Total: 287`.
- Text-only proof: not applicable.
