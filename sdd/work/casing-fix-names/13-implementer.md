Role: implementer
Commit: 96f8123
Verdict: tests pass

Repair round 2, for auditor `11-auditor.md` findings 1 and 2 and lead decision 3, against the tests of `12-test-implementer.md`. The prompt carried no intended fix or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/CasingUtilities.cs` | `ToPascalCaseWord` upper-cases a word's first character only when it is a letter (`char.IsLetter`); any other first character (letter number, combining mark, digit, connector punctuation) is kept as written. Summary updated. Reaches every PascalCase, camelCase, and `_camelCase` derivation, so every casing fix (finding 2). | SHARED-CASING-006 (unchanged, decision 3) |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` | `IsDictatedName` selects the signature of `OperatorDeclarationSyntax` (return type, parameters) and `ConversionOperatorDeclarationSyntax` (target type, parameter) besides methods, through a new private helper `GetSignatureTypes`; an event field's tuple names are dictated when any of its declarators is dictated, instead of only the first. Remarks updated. (finding 1) | SHARED-CASING-014 bullet 2, RH4116-001 |

## Policy owners
- Case of a word's first character: `rg -n "ToUpperInvariant" src/Reihitsu.Core/CasingUtilities.cs` — one site, `ToPascalCaseWord`; the three derivations all route through it (the auditor's `rg "CasingUtilities\.To" src` table).
- Dictated member: still only `CasingAnalyzerBase.IsDictatedMember`; RH4116 only selects which declaration to ask. `rg -ln "EventFieldDeclarationSyntax|ConversionOperatorDeclarationSyntax" src/Reihitsu.Analyzer/Rules/Naming src/Reihitsu.Analyzer/Base`: RH4102 (matches the declarator by span, already per declarator), RH4116, and `TypesOrganizedWithRegionsAnalyzerBase` (unrelated decision).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 12 files changed, 543 insertions(+), 74 deletions(-). Round delta: 2 files changed, 34 insertions(+), 11 deletions(-). No new type; one new private method (`GetSignatureTypes`).

## Deleted special cases
_N/A._

## Tests run
Before the change (stashed): Core `CasingUtilitiesTests` Failed 12 / Passed 132; RH4116 + `FixKeepsALetterNumberOrCombiningMark…` Failed 7 / Passed 12 — as the handover states.

After:
- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"` — Passed 144, Failed 0
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests|FullyQualifiedName~FixKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten"` — Passed 19, Failed 0
- `scripts/test.sh --configuration Debug` (default set): Analyzer 11987, Formatter 7130, Core 884, Cli 440 (+1 skipped), Architecture 198, Tooling 204, Playground 23 — all Failed 0.

`scripts/format.sh` over both files: 0 of 2 changed. Debug build: no warnings.

## Disputed tests or scope
_None._ Note: the `char.IsLetter` check also applies to the first word; an uncased first character (digit cannot start an identifier; Nl is outside SHARED-CASING-006 coverage per the test handover and is left to SHARED-CASING-008) was already unchanged or out of coverage, and the whole default set stays green.

## Architecture findings
_None._

## Follow-up candidates
_None._
