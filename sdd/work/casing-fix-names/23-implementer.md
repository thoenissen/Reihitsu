Role: implementer
Commit: e1c71ec
Verdict: tests pass

Repair round 3 and structural repair 2 for auditor `21-auditor.md` findings 5 and 6, built to lead decisions 6 and 7, against the tests of `22-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decisions 6 and 7. No test was changed. `git stash` was not used.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/RecordPositionalParameterUtilities.cs` (new internal static class) | The one owner of which members the compiler declares from a record's positional parameter. `GetDeclaredProperty(recordType, parameter)` returns the property the compiler declares for the parameter, matched by its declaring syntax reference with one predicate (same tree, `reference.Span == parameter.Span`), or `null`. `GetDeclaredMembers(parameterSymbol, parameter)` yields, for a parameter of a record's parameter list whose containing symbol is a constructor, that property and the record's implicitly declared `Deconstruct` (`IsImplicitlyDeclared`, parameter count equal to the parameter list's count); for any other parameter nothing. It does not yield "any implicitly declared member", so `Equals(R)` is not asked (decision 6). | SHARED-CASING-014 bullet 2, RH4116-001 (unchanged); FC-015 (finding 6) |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` | `GetDeclaredMembers`' parameter branch yields the containing member and then `RecordPositionalParameterUtilities.GetDeclaredMembers`, replacing the private property lookup. The summary of `GetDeclaredMembers` and the remarks of `IsDictatedName` now name the property and the `Deconstruct` method the compiler declares from a positional parameter, instead of claiming the property alone (FC-031, part of finding 5). | SHARED-CASING-014 bullet 2, RH4116-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` | `GetPositionalPropertyFlagsAsync` asks `RecordPositionalParameterUtilities.GetDeclaredProperty(recordType, parameter) != null` instead of its own lookup. The span predicate moves from containment to equality, the one the auditor confirmed finds the synthesized property in both places; the code-fix tests that cover this path stay green. Its summary and remarks still describe the code and were left unchanged. | SHARED-CASING-010 rename safety (unchanged); FC-015 |

## Policy owners
- Property the compiler declares for a positional parameter: `rg -n "DeclaringSyntaxReferences" src --glob '*.cs'` — the only match on this question is `RecordPositionalParameterUtilities.GetDeclaredProperty`. `SingleLetterIdentifierUtilities.cs:65` also reads a parameter's declaring syntax, but it asks a different question (whether a parameter is a record positional parameter), not which member is declared from it.
- Members declared from a positional parameter's type: `rg -n "Deconstruct" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes --glob '*.cs'` — the only owner is `RecordPositionalParameterUtilities.GetDeclaredMembers`. RH4117 names deconstruction variables, which is unrelated.
- Dictated member: `rg -n "IsDictatedMember" src --glob '*.cs'` — the one owner is still `CasingAnalyzerBase.IsDictatedMember`, unchanged.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 12 files changed, 567 insertions(+), 74 deletions(-). This round: 3 files, a new internal type `RecordPositionalParameterUtilities` (89 lines), plus 10 insertions and 19 deletions in the two callers.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests"`: Passed, Failed: 0, Passed: 34 (including the three that failed in `22-test-implementer.md`).
- `scripts/test.sh --configuration Debug` (default set): Analyzer 12004 passed; Formatter 7130 passed; Core 884 passed; Cli 440 passed, 1 skipped; ArchitectureTests 198 passed; Tooling 204 passed; Playground 23 passed. No failures. The analyzer project holds the casing code-fix tests that cover `CasingRenameSafetyUtilities`.
- `scripts/format.sh` over the three changed files. It formatted the new file.

## Disputed tests or scope
- New internal type `RecordPositionalParameterUtilities` in `Reihitsu.Analyzer/Core`, consumed by `Reihitsu.Analyzer.CodeFixes` through the existing `InternalsVisibleTo`. Lead decision 7 asked for it, so it is reported here only as the scope note the role requires.

## Architecture findings
_None._ The axis from findings 1, 3, and 5 is now closed by the set the language defines for a positional parameter: the primary constructor (asked as the containing member), its property, and the generated `Deconstruct`. The shared owner answers both.

## Follow-up candidates
_None new._ The RH4120 candidate in `00-lead.md` can now reuse `RecordPositionalParameterUtilities.GetDeclaredProperty`. The note there that the mapping is private to RH4116 is no longer true.
