Role: implementer
Commit: acc2f4c
Verdict: tests pass

Structural change for decision 15 (thoenissen/Reihitsu#1611): the shift applied to a line of a multi-line raw string is stated once, in `Reihitsu.Core`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 15 itself. No test file was changed; no visible output changes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RawStringLiteralUtilities.cs` | New public `GetLineShiftEdit(leadingWhitespaceLength, shift)` next to `GetLeadingWhitespaceLength`: returns `(RemovedLength, InsertedSpaces)`. Right shift inserts `shift` spaces; left shift removes `Math.Min(-shift, leadingWhitespaceLength)` characters, whatever whitespace they are. `using System;` added for `Math` | RH5202-003, SHARED-FIX-007, FMT-RAW-002, decision 15 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `ReplaceIndentation` applies the edit from `GetLineShiftEdit` (remove, then insert at the line's offset) instead of its own insert/remove rule; summary updated | FMT-RAW-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | `ApplyCodeFixAsync` builds each `TextChange` from `GetLineShiftEdit` instead of its own `delta > 0` / `Math.Min` branches; summary updated. A line gets a change only when the edit removes or inserts something, as before (`delta == 0` already returns early) | RH5202-003 |

## Policy owners
- `rg -n "Math\.Min\(-" src --type cs` → only `RawStringLiteralUtilities.GetLineShiftEdit`. No private copy of the left-shift rule remains.
- `rg -n "GetLineShiftEdit\(" src --type cs` → Core owner, `RawStringLineShifts.ReplaceIndentation`, RH5202 fix `ApplyCodeFixAsync`.
- `rg -n "GetLeadingWhitespaceLength\(" src --type cs` → unchanged owners from 06 plus the new fix call site (the measure feeding the edit).
- Not routed through the helper: in `RawStringLineShifts.ApplyEdits`, a line of an interpolation hole whose indentation is whitespace trivia is rewritten as a whole (`NewLength` spaces). That trivia holds only spaces after the preparation phase, so the result equals the shift rule; it sets formatter-owned code indentation, not literal text. I left it as it was (06 recorded it the same way). The lead may decide whether it should also go through the helper.

## Tests run
- `scripts/build.sh --configuration Debug`: 0 errors; no warnings in the three changed files.
- `scripts/format.sh` on the three files: formatted 0 of 3.
- `scripts/test.sh --configuration Debug --project core`: Passed 660, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 4692, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7350, Failed 0.

## Disputed tests or scope
- New public member in `Reihitsu.Core` (`RawStringLiteralUtilities.GetLineShiftEdit`) with two consumers, as decision 15 asks. It has no direct test in `Reihitsu.Core.Test`; both consumers' tests cover it. A direct `RawStringLiteralUtilitiesTests` case (right shift, left shift within and beyond the whitespace) is the test implementer's if the lead wants it.

## Architecture findings
_None new._ Decision 15 is carried out: one shift rule, two callers.

## Follow-up candidates
_None._
