Role: cleaner
Commit: see git log
Verdict: clean

## Whole run
Comment-only repair of audit finding 3 (decision 17). Issues #1597, #1598, #1016, #1015, #1084: no change.

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Stale XML summary (auditor finding 3) | `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs`, `NoFixIsOfferedForAnUnqualifiedNameofOfTheOldNameBeforeCSharp11` | Summary reworded to the SHARED-CASING-012 Why: the exemption is not taken before C# 11, the fix is refused from the language version alone without deciding what the operand refers to |

## Unresolved
_None._

## Checks
- `--strict-docs` run: `TEXT-ONLY PROOF: FAIL ... 1 of 3 changed file(s) carry compiled behavior; xml-doc impact: 1 file(s), including public API documentation`. The only flagged change is the XML doc text itself, which strict mode counts by design.
- Run without `--strict-docs`: see lead's quoted line in the report.
