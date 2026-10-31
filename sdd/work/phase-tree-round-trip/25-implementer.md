Role: implementer
Commit: c8b4e86
Verdict: tests pass

Repair round 3, scope audit finding #11 (`23-auditor.md`) only. Finding #10 (`ApplyEdits` summary) is the cleaner's and is untouched. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`, `IsHoleOpeningLine` | Removed the conjunct `interpolation.Parent is InterpolatedStringExpressionSyntax`. Roslyn places an `InterpolationSyntax` only in the `Contents` of an `InterpolatedStringExpressionSyntax`, so the conjunct always held; the caller's cast `(InterpolatedStringExpressionSyntax)interpolation.Parent` relies on the same fact. | FC-031 (finding #11); no behavior change |
| same method, XML summary | The summary no longer claims the predicate checks for a raw string. It now says the predicate tests whether a line starts with a hole's `{`, does not check raw, and why that suffices: only moving lines reach it, and `StartsInsideNestedText` gives a line that starts at a non-raw string's hole (text of a non-raw string ends at the line start) no shift. | FC-031 (finding #11) |

I took both of the auditor's suggestions (drop the conjunct, and state the reach in the summary) rather than one: dropping the conjunct alone would leave the summary claiming a raw check that does not exist.

## Policy owners
`rg -n "InterpolatedStringTextToken|OpenBraceToken|InterpolationSyntax" src/Reihitsu.Formatter/Pipeline/Indentation` — the hole-line indentation decision has one owner, `RawStringLineShifts` (unchanged from `23-auditor.md`, Policy ownership). No policy was moved or copied.

## Tests run
```shell
scripts/format.sh src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs
# Formatted 0 of 1 file(s).
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests"
# Passed!  - Failed: 0, Passed: 29, Skipped: 0, Total: 29
scripts/test.sh --configuration Debug --project formatter
# Passed!  - Failed: 0, Passed: 3402, Skipped: 0, Total: 3402
scripts/build.sh
# 0 Warning(s), 0 Error(s)
```

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
