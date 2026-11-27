Role: spec author
Commit: fc985a3
Verdict: spec written

## Changed requirements

None. No requirement is added, changed, retired, or clarified: the change is a rename of the code owner only, and RH4118-001 already states what the analyzer does. No change doc (no intended behavior changes), and `documentation/rules/RH4118.md` is untouched (nothing reported or fixed changes for the user).

Spec edits:

- `sdd/specs/analyzer/RH4118.md` — `Code owners`: `RH4118TupleElementCasingAnalyzer` -> `RH4118TupleArgumentNameCasingAnalyzer`.
- `sdd/specs/README.md` — the RH4118 row of the code-owner map, same rename.

## New type names for the implementer

| Old | New | File |
|---|---|---|
| `RH4118TupleElementCasingAnalyzer` | `RH4118TupleArgumentNameCasingAnalyzer` | `src/Reihitsu.Analyzer/Rules/Naming/RH4118TupleArgumentNameCasingAnalyzer.cs` (class, constructor) |
| `RH4118TupleElementCasingAnalyzerTests` | `RH4118TupleArgumentNameCasingAnalyzerTests` | `tests/Reihitsu.Analyzer.Test/Naming/RH4118TupleArgumentNameCasingAnalyzerTests.cs` (class, `<see cref>`, base type argument, the two `DiagnosticId` references) |

There is no RH4118 code-fix provider (`Code fix: None` in the spec; no `RH4118*CodeFixProvider` under `src/Reihitsu.Analyzer.CodeFixes/`), so nothing else is renamed. The `RH4118Title` resource entries in `AnalyzerResources.resx` and `CodeFixResources.resx`/`.cs` are keyed on the rule ID, not on the type name, and stay as they are; every rule has such an entry in `CodeFixResources`, with or without a provider.

Evidence for the name:

- What the code checks: `GetLocations` visits `TupleExpressionSyntax.Arguments` and reports `argument.NameColon.Name.Identifier`, the name of a tuple *argument* (`ArgumentSyntax` with a `NameColon`). It never looks at a tuple type's `TupleElementSyntax`.
- The mismatch: `RH4116TupleElementCasingAnalyzer` is the rule that checks `TupleElementSyntax` names; RH4118 carries the same suffix for a different construct.
- Conventions: Naming analyzers are `RH41xx<Construct>CasingAnalyzer` or `RH41xx<Construct>NameCasingAnalyzer` (`RH4102EventNameCasingAnalyzer`, `RH4103MethodNameCasingAnalyzer`, `RH4121TypeParameterNameCasingAnalyzer`). `TupleArgumentName` follows the second form and matches both the Roslyn node (`ArgumentSyntax` in a `TupleExpressionSyntax`) and the diagnostic title and the class's own summary, "Named tuple argument names should be in PascalCase". This is the name the issue proposes; the evidence confirms it, so it is kept.

Architecture tests after the spec edit (`scripts/test.sh --project architecture`): 196 passed, 2 failed, and both are failures that the rename in the next stage clears:

- `SpecConsistencyTests.EveryCodeOwnerNamesAnExistingType` — `RH4118TupleArgumentNameCasingAnalyzer` is not declared yet.
- `SpecStructureTests.AnalyzerSpecHeaderMatchesItsRule` — `RH4118TupleElementCasingAnalyzer` implements the rule but is no longer a code owner.

## Examples for the tests

None new. The existing RH4118-001 tests move with the class rename, and their assertions stay unchanged.

## Output questions

_None._ Type names are internal; the diagnostic ID, title, message, help link, and rule page are unchanged.

## Internal choices

- The spec title "Tuple expression element casing" and the wording of RH4118-001 ("element name written in a tuple expression") stay. They describe the construct correctly in C# terms and do not name the type. The alternative was to retitle the spec "Tuple argument name casing", but that churns text the issue does not ask about.

## Follow-up candidates

_None._

## Notes

The prompt carried no intended fix or verdict beyond the issue's own proposed name, which the evidence above confirms on its own.
