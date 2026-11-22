Role: spec author
Commit: 80fb2fe
Verdict: spec written

Repair round for finding 1 of `25-spec-checker.md` (lead decision 9). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Decision: `RecordPositionalParameterUtilities` is an owner of SHARED-CASING. It decides which members the compiler declares from a record positional parameter — the property and the generated `Deconstruct` — and that set is what SHARED-CASING-014 bullet 2 asks when it names "the types of its parameters" of a dictated member (RH4116 calls `GetDeclaredMembers`), and what the rename-safety checks of the casing fix ask (`CasingRenameSafetyUtilities` calls `GetDeclaredProperty`). Both callers are already SHARED-CASING behavior, so the type implements behavior the spec states and is not a general-purpose helper. It is listed only in SHARED-CASING, not in RH4116: it serves the shared spec's dictated-name rule and the shared code fix, so one spec owns it. No other spec lists it.

Edit: `RecordPositionalParameterUtilities` added after `CasingRenameSafetyUtilities` in the `Code owners` row of `sdd/specs/shared/casing.md` and in the SHARED-CASING row of `sdd/specs/README.md`. No requirement text changed.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.

## Changed requirements
_None._ (code-owner lists only)

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Listed `RecordPositionalParameterUtilities` as a SHARED-CASING code owner rather than leaving it out as a general utility; alternative: list it under RH4116, rejected because the code fix's rename safety uses it as well.

## Follow-up candidates
_None._
