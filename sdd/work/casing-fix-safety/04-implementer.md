Role: implementer
Commit: 16bcd60 (statements derived from the working tree on top of it, committed with this handover)
Verdict: #1597: test disputed; #1598: nothing to do; #1016: test disputed; #1015: tests pass; #1084: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the run's handovers and decisions. Every single-fix red test of #1597, #1016 and #1084 and all three #1015 tests now pass. Eleven tests fail; none for a production reason I can find (see *Disputed tests or scope*): ten new Fix All tests whose harness drops the diagnostics their own fixed code keeps, and one pre-existing RH4103 test that asserts a rename SHARED-CASING-011 now withholds.

## thoenissen/Reihitsu#1597

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `IsCompilerLookupMethodRename`: `PrepareRenameAsync` withholds a rename whose declared symbol is a method that is not a local function and whose new name is one of the 21 names of the list (a constant `is ... or ...` pattern). Runs for single fix and Fix All, since both go through `RenameWithoutConflictAsync` | SHARED-CASING-011, RH4103-003 |
| same | `HasCallerArgumentExpressionNaming` + `IsNameofArgument`: after the rename, `RenameWithoutConflictAsync` withholds it when a parameter list that declares a parameter of the cascade (method, constructor, local function, delegate `Invoke`, indexer, primary constructor; both parts of a partial method, since both parameters are in the cascade) carries `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute` whose constant argument is an old name of the cascade or the new name, and whose argument syntax is not `nameof(...)`. Uses the existing `renamedNames` (old names + identifier) | SHARED-CASING-012, RH4105-003, RH4120-003 |

### Tests run
All single-fix tests of the handover pass (`NoFixIsOffered…`, `FixIsOffered…`, the 23 list rows, the CallerArgumentExpression rows). The six #1597 Fix All tests (`FixAllSkips…` in RH4103 ×4, RH4105, RH4120) fail only on the fixed-state diagnostic count; with the fixed-state markup kept (see *Disputed*) they pass. `FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees` fails (see *Disputed*).

## thoenissen/Reihitsu#1598

Nothing to do: the five RH4109 characterization tests stay green (`scripts/test.sh --project analyzer --filter "…Naming.RH4102|…Naming.RH4109"`: 36 passed).

## thoenissen/Reihitsu#1016

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs` | New `internal bool IsValidName(string name)`: the rule's own casing predicate, which `CheckCasing` now calls too | SHARED-CASING-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | The constructor takes the rule's `CasingAnalyzerBase` instance instead of the diagnostic ID (the ID is read from its descriptor). `TryGetFixedIdentifier` additionally requires `_analyzer.IsValidName(identifier)`, so single fix and Fix All candidates are withheld when the rule would still report the derived name | SHARED-CASING-008, SHARED-FIX-001 (lead decision 4) |
| all 26 casing providers under `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/` | `base(RHxxxAnalyzer.DiagnosticId, …)` → `base(new RHxxxAnalyzer(), …)` | same |

### Tests run
All single-fix tests pass (RH4106 rows, RH4002 rows, RH4105 rows, RH4005 rows, controls `ß_x`, `_ŉx`, `_1`). The Fix All tests (RH4106, RH4002, RH4105) fail only on the fixed-state diagnostic count; see *Disputed*.

## thoenissen/Reihitsu#1015

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4102EventNameCasingCodeFixProvider.cs` | Now `CasingCodeFixProviderBase<SyntaxNode>` with `GetIdentifier` switching on `VariableDeclaratorSyntax` / `EventDeclarationSyntax` (the RH4115 pattern), so one provider and one Fix All cover both event kinds, including the SHARED-CASING-010 collision group | SHARED-FIX-002, RH4102-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4102CustomEventNameCasingCodeFixProvider.cs` | Reduced to `internal sealed class … : RH4102EventNameCasingCodeFixProvider;` without `[ExportCodeFixProvider]`, so the IDE does not see two providers for one diagnostic. Kept only because existing tests name it (see *Disputed*) | — |

### Tests run
`FixAllRenamesAFieldLikeEventAndAnEventWithAccessorsTogether`, `FixAllRenamesSeveralFieldLikeEventDeclaratorsAndAnEventWithAccessorsTogether`, `FixAllSkipsAFieldLikeEventAndAnEventWithAccessorsThatWouldCollide`: pass; all RH4102 tests pass.

## thoenissen/Reihitsu#1084

### Changes
Covered by the #1016 check: `_` → `I_`, `__` → `I__`, `_2x` → `I2x`, `注文` → `I注文`, `ßx` → `Ißx` are rejected by `RH4005InterfaceNameCasingAnalyzer`'s predicate, so no fix. `RH4005InterfaceNameCasingCodeFixProvider.OnTransformIdentifier` is unchanged.

### Tests run
`NoFixIsOfferedWhenNoUsableNameCanBeDerived` (all rows), `NoFixIsOfferedForAnyInterfaceWhoseDerivedNameTheRuleStillReports`, `FixIsOfferedWhenAnUnderscoreIsFollowedByALetter`: pass. `FixAllSkipsTheInterfacesWithoutAUsableDerivedName` fails only on the fixed-state diagnostic count; see *Disputed*.

## Whole run

### Policy owners
- Rule acceptance of a name: `rg -n "IsValidName|_casingValidation" src --type cs` → only `CasingAnalyzerBase` (field + `IsValidName`) and its one consumer in `CasingCodeFixProviderBase<T>.TryGetFixedIdentifier`. The fix now reads the analyzer's predicate instead of restating it; no provider passes a predicate of its own.
- Compiler-looked-up method names: `rg -n '"GetEnumerator"' src --type cs` (non-test) → only `IsCompilerLookupMethodRename`.
- `CallerArgumentExpression`: `rg -n "CallerArgumentExpression" src --type cs` (non-test) → only `HasCallerArgumentExpressionNaming`.
- `nameof` detection: `rg -n '"nameof"' src --type cs` → `RH3103UseShorthandForNullableTypesAnalyzer` (analyzer-private, different context) and the new `IsNameofArgument`; no shared helper exists.
- Rename-safety checks: all in `CasingCodeFixProviderBase<T>` (`PrepareRenameAsync` / `RenameWithoutConflictAsync`), extended in place; no parallel path.

### Production diff size
`git diff --shortstat -- src ':(exclude,glob)src/*Test*/**'` before committing: 28 files changed, 174 insertions(+), 82 deletions(-). No new type; one type shrinks (`RH4102CustomEventNameCasingCodeFixProvider`). Of the 28 files, 26 are the one-line constructor change of each casing provider. New members: `CasingAnalyzerBase.IsValidName` (internal), `CasingCodeFixProviderBase<T>.IsCompilerLookupMethodRename`, `IsNameofArgument`, `HasCallerArgumentExpressionNaming` (private); the protected base constructor's first parameter changed from `string diagnosticId` to `CasingAnalyzerBase analyzer`.

### Deleted special cases
_N/A._

### Tests run
- `scripts/build.sh --configuration Debug`: succeeded.
- `scripts/test.sh --configuration Debug --project analyzer`: `Failed: 11, Passed: 7829, Total: 7840` — exactly the 11 tests listed under *Disputed*.
- Same, with the ten Fix All tests temporarily given `FixedState.MarkupHandling = MarkupMode.Allow` and `BatchFixedState.MarkupHandling = MarkupMode.Allow` (reverted with `git checkout`, not committed), `--filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: `Failed: 1, Passed: 643` — only `FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees`.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed: 117, Failed: 0`.
- `scripts/format.sh --force` over the changed files: two files formatted (the new `RH4102Custom…` file's final newline, one alignment).

### Disputed tests or scope

1. **Ten new Fix All tests cannot pass with their harness** (test implementer's, commit 1c84020): `RH4002…FixAllSkipsTheClassWhoseDerivedNameTheRuleStillReports`, `RH4005…FixAllSkipsTheInterfacesWithoutAUsableDerivedName`, `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodA{Foreach,QueryClause,Deconstruction,CollectionInitializer}Calls`, `RH4105…FixAllSkipsTheParameterNamedByACallerArgumentExpression`, `RH4105…FixAllSkipsTheParameterWhoseDerivedNameTheRuleStillReports`, `RH4106…FixAllSkipsTheFieldsWhoseDerivedNamesTheRuleStillReports`, `RH4120…FixAllSkipsTheParameterNamedByACallerArgumentExpression`.
   - What they assert: `Verify(src, fixed, static test => test.NumberOfFixAllIterations = 1)`, where `fixed` keeps the skipped declarations with markup such as `{|RH4106:__1st|}`.
   - Why they fail: `Microsoft.CodeAnalysis.Testing` 1.1.4 defaults `FixedState.MarkupHandling` (and `BatchFixedState`) to `MarkupMode.IgnoreFixable`, which drops markup diagnostics whose ID is in the provider's `FixableDiagnosticIds`. The expected fixed-state list is therefore empty, while the analyzer still reports the skipped names — exactly what the spec demands. Failure text: `Context: Diagnostics of fixed state / Mismatch between number of diagnostics returned, expected "0" actual "3"`. The fixed *text* already matches.
   - Evidence that the production code meets the spec: with `MarkupMode.Allow` set on both states (as `RH4103MethodNameCasingAnalyzerTests` line ~728, `RH4107…` line ~265 already do for the same situation), all ten pass. No production change can make them pass as written: it would have to stop reporting the skipped names.
   - Needed change (test implementer): set `test.FixedState.MarkupHandling = MarkupMode.Allow` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow` in each. No asserted output changes.
2. **`RH4103MethodNameCasingAnalyzerTests.FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees`** (pre-existing, from #1596, tagged SHARED-CASING-009) asserts that the extension method `select` is renamed to `Select`. SHARED-CASING-011 (added in 5fed711) withholds the fix for every non-local method whose derived name is `Select`, "whether or not the rename would change which method a construct calls"; its examples say the same for a lone `getEnumerator`. Lead decision 3 / 6 accepted this over-withholding. The test and the spec now disagree; the spec wins, so the test's expected output must become "no fix offered" (`Verify(src, src)`), or its input must use a name outside the list if its purpose (SHARED-CASING-009: no ambiguity across documents) is to stay covered by a rename. Visible output: a fix `main` offers is withheld — already covered by decision 6 (rare class), so I see no new output question.
3. **`RH4102CustomEventNameCasingCodeFixProvider` is kept only for tests.** `RH4102EventNameCasingAnalyzerTests.VerifyCustomEvent` and `VerifyCustomEventWithReference` instantiate it by name. It is now a non-exported subclass of the merged provider; the Release build reports `S2094` (empty class) on it. Recommendation: the test implementer switches those two helpers to `RH4102EventNameCasingCodeFixProvider`, and the type is then deleted (a follow-up commit of mine, one file). Until then the cleaner should not suppress S2094.
4. **Scope of SHARED-CASING-012 check.** It runs for every rename whose cascade contains a parameter, not only when the declared symbol is a parameter: a property rename that cascades to a record's primary-constructor parameter (RH4111–RH4114 on a positional property) is withheld too when a `CallerArgumentExpression` names it. SHARED-CASING-012 says "for a parameter"; SHARED-FIX-003 (*Run time*, sentence added in 5fed711) withholds the property case anyway, so I read it as within the spec. Not covered by a test.
5. **Shape.** The base constructor's signature change touches all 26 casing providers (one line each). It is mechanical and needed so the fix asks the analyzer's own predicate (lead decision 4) instead of restating it per provider.

### Architecture findings

_None new._ Lead decision 4's finding (acceptability of the derived name decided in several places) is resolved structurally: the fix consults `CasingAnalyzerBase.IsValidName`, the predicate the analyzer reports with. The converters' "return the input unchanged" behavior and `RH4005…OnTransformIdentifier` stay as they are; they no longer decide acceptance.

### Follow-up candidates

_None_ beyond the skeptic's renamer call-site rewrite (already in `00-lead.md`).
