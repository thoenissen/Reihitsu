Role: skeptic
Commit: dfcde82
Verdict: proceed narrower

## Premise

Real, and it is a naming mismatch, not a behavior defect.

- `src/Reihitsu.Analyzer/Rules/Naming/RH4118TupleElementCasingAnalyzer.cs`: `GetLocations` walks `TupleExpressionSyntax.Arguments` and reports `argument.NameColon.Name.Identifier`. That is the name of a tuple *argument* (an `ArgumentSyntax` with a `NameColon`). It never reads `TupleElementSyntax`.
- `RH4116TupleElementCasingAnalyzer` checks `TupleElementSyntax` names in tuple types. Two different constructs share one suffix.
- A scan of every analyzer file name with its `RH####` prefix removed (`find src -name 'RH[0-9]*Analyzer.cs' -printf '%f\n' | sed -E 's/^RH[0-9]+A?//' | sort | uniq -d`) finds exactly one duplicate, `TupleElementCasingAnalyzer.cs`. RH4116/RH4118 is the only case of this kind in the repository.
- Supported inputs: this issue does not report a behavior, so there is no input whose reachability needs checking. The type name is internal (`AGENTS.md`, *Supported inputs*: no assembly is a public API). Users see the rule by ID and title: the ruleset, `.editorconfig` severities, the help link (`documentation/rules/RH4118.md`, built from the ID), the package README row, and the resource keys `RH4118Title`/`RH4118MessageFormat`. None of them contains the type name. The type name shows up only in an `AD0001` crash message, which is not specified output.
- The proposed name matches the rule's title, "Named tuple argument names should be in PascalCase" (rule page heading, package README row, the class's own `<summary>`). It also follows the `RH41xx<Construct>NameCasingAnalyzer` form (`RH4121TypeParameterNameCasingAnalyzer`).

## Mechanism and candidates

_N/A._ No behavior defect. The cause is that RH4118 was created by copying RH4116 and kept RH4116's construct suffix.

## Breaking inputs

_N/A._ No behavior changes, so no failure class reaches the change. The six existing RH4118-001 tests in `tests/Reihitsu.Analyzer.Test/Naming/RH4118TupleElementCasingAnalyzerTests.cs` already pin the unchanged behavior and move with the rename without assertion changes. Two architecture tests pin the code-owner mapping: `SpecConsistencyTests.EveryCodeOwnerNamesAnExistingType` and `SpecStructureTests.AnalyzerSpecHeaderMatchesItsRule`. According to the spec author's run, both fail now and the type rename clears them. No new test is needed (`README.md`, *Coverage first*).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer `RH4118TupleElementCasingAnalyzer` (class, constructor, file name) | Must change (rename only). `DiagnosticId`, the syntax kind, the predicate, and `GetLocations` must stay byte-for-byte |
| `RH4116TupleElementCasingAnalyzer` | Must stay. Its name is correct, and renaming it is outside the issue |
| Code-fix provider | Not involved. None exists (`Code fix: None`, and there is no `RH4118*` under `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/`) |
| Fix All | Not involved |
| `CodeFixResources` / `AnalyzerResources` `RH4118*` entries | Must stay. They are keyed by rule ID |
| `Reihitsu.Core`, formatter | Not involved |
| Rule page `documentation/rules/RH4118.md`, package README row, `Release.ruleset`, `Reihitsu.sln` | Must stay. They reference the rule by ID only (checked with `rg -n RH4118` over each) |
| Test class `RH4118TupleElementCasingAnalyzerTests` (class, file name, `<see cref>`, base type argument, two `DiagnosticId` references) | Must change (rename only) |
| Specs `analyzer/RH4118.md` and `README.md` code-owner row | Changed by the spec author. The diff in `dfcde82` is correct and minimal |
| Checked-in `binaries/Reihitsu.Analyzer.dll` (loaded for self-hosting by `Directory.Build.props:54`) | Must stay. It keeps the old type name until the next version bump, which is harmless because self-hosting and the ruleset key on the ID. This run must not rebuild binaries |

## Refuse option

The refuse option is to make no change. It costs nothing, but it leaves the only duplicated construct suffix among the analyzers in place, where it misleads any reader who looks for the RH4118 owner or tells the two rules apart. The requested change is two file renames and about seven identifier edits, with no new type, no new mechanism, and no visible output. It is cheaper to make than to argue about. Declaring a shape unsupported does not apply, because no formatter phase or code fix is involved. No simpler option exists.

## Output questions

_None._ Nothing an end user sees changes: the diagnostic ID, title, message, help link, severity, rule page, and package README all stay the same.

## Internal choices

- **Test method names.** The RH4118 test methods say `TupleExpressionElement…` (for example, `VerifyDiagnosticsForTupleExpressionElementWrongCasing`). That phrase matches the spec title and RH4118-001's wording, and it does not collide with RH4116. Recommend leaving them as they are, because renaming them only churns the diff. The other option is to rename them to `…TupleArgumentName…` for symmetry with the class, which has no consequence beyond the diff.
- **Spec title "Tuple expression element casing."** I agree with the spec author: keep it. It describes the construct correctly and does not name the type.

## Issue goals

- "Rename … along with its code-fix provider": drop it. RH4118 has no code-fix provider. This does not change visible output.
- "Rename the test class if it carries the same mismatch": keep it. The test class does carry the mismatch.
- "Update the code owners in both specs": done in `dfcde82`. Keep it.
- "The spec checks pass, and no behavior changed": keep it. The implementer should change no line of `RH4118…Analyzer.cs` other than the type name and the constructor name.

## Architecture findings

_None._

## Follow-up candidates

_None._

The prompt carried no intended fix or hoped-for verdict beyond the issue's own proposed name. I checked that name independently above.
