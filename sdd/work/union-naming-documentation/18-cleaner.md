Role: cleaner
Commit: see git log (this handover's commit)
Verdict: clean

## Formatted
None by the formatter (0 of 29 changed C# files).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S4136 (error under TreatWarningsAsErrors): overloads of `GetExpandedDocumentation` not adjacent | `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs` | Moved `CanExpandDocumentation` below the enum overload; no code changed |

## Unresolved
None.

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Focused analyzer tests (RH80*, RH4012): see result below in commit message run.
