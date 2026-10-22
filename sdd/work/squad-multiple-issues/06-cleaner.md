Role: cleaner
Commit: a7b7f7b
Verdict: clean

The prompt carried no intended fix or verdict.

## thoenissen/Reihitsu#1403

### Formatted
_None._ `git diff --name-only f52df45...HEAD` lists only Markdown files, so there is no changed C# file to format.

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

### Unresolved
_None._

### Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- Focused tests: not run; no compiled file changed.
- Text-only proof: not applicable; no C# file changed.
