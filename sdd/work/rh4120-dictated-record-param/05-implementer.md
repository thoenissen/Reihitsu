Role: implementer
Commit: 28a5103
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzer.cs` | Overrides `IsDictatedName`: the parameter's name is dictated when `IsDictatedMember` holds for the member the parameter stands for | SHARED-CASING-014 (record-parameter bullet, boundary sentence), RH4120-001 |
| `src/Reihitsu.Analyzer/Core/RecordPositionalParameterUtilities.cs` | New `GetMember(recordType, parameter)`: searches the record and its base types outward and returns the first property or field of the parameter's name — the compiler-declared property (a member of the record, so found first), else the declared or inherited member the parameter matches; class summary extended | SHARED-CASING-014 (2B: "the property the compiler declares … or … that member") |

No change to the code fix: RH4120-003 is unchanged, and the fix for a reported parameter next to an exempted one already passes (`OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed`).

## Policy owners
- "dictated": `rg -n "IsDictated" src` — one owner, `CasingAnalyzerBase.IsDictatedMember`; reused, not copied. Callers: RH4102, RH4103, RH4104, RH4112–RH4114, RH4116, and now RH4120.
- Positional parameter to member: `rg -n "GetMembers\(parameter\.Identifier|BaseType\)" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` — the one owner is `RecordPositionalParameterUtilities` (`GetDeclaredProperty`, `GetDeclaredMembers`, now `GetMember`); no private copy elsewhere. `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync` uses `GetDeclaredProperty` and is unchanged.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 43 insertions(+), 1 deletion(-). No new type; one new internal method on an existing utility class.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests"` — Passed: 38, Failed: 0 (12 failing before).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 11901, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0.

## Disputed tests or scope
_None._ Note for the auditor (FC-044): the compiler exposes no symbol for "the existing member a positional parameter matches", so `GetMember` reads it by name lookup with hiding (record, then base types). Whether a matching member exists at all is still the compiler's result: when the compiler declares a property, that property is a member of the record and is found first, which also covers an inaccessible base member the compiler skips. A field or a non-property member can never be dictated (`IsDictatedMember` returns false), so a mismatch there only keeps today's diagnostic.

## Architecture findings
_None._

## Follow-up candidates
_None._
