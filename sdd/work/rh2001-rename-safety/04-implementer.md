Role: implementer
Commit: 2d663bf
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. 198 of the 203 tests in the RH2001 class pass, and so do all other analyzer tests. The 5 remaining tests (below) assert that the fix is withheld for inputs where, measured on this branch and on unchanged `main`, the renamer qualifies the renamed reference correctly or leaves a reference whose binding does not change. Their fix output compiles and behaves the same at run time. Making them pass would need a syntactic withhold that predicts renamer failures that do not occur, and that would withhold correct fixes `main` offers today. I did not build it.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | New registration guard `RenameChangesBindingAsync`. It runs the rename on an annotated copy. Then, in every document that can refer to the property (`ExcludedCodeUtilities.GetReferringDocuments`), it compares the sorted descriptions of every `IdentifierNameSyntax` spelled like the field name that does not bind to the property, before and after the rename. A description is the bound symbols (or candidates), the candidate reason, and the written qualifier (member-access receiver, qualified-name left side, `?` for a member binding). A difference withholds the fix. New helpers: `GetOtherSymbolReferencesAsync`, and `GetAnnotatedPropertySymbolAsync`, which now also serves `RenamePropertyToFieldNameAsync` (no behavior change there). `ApplyCodeFixAsync` is unchanged. | RH2001-011 paragraphs 1 and 3; paragraph 2 where the renamer actually misbinds (s06, t1, s13) |

What the comparison catches, by test:
- f02, s16, e1: the qualifier changes (`base`, `Holder`, `Outer`).
- t4, f05, e2, f08, and t4 in another `partial` part: a reference to the inherited member is left as written, binds to the renamed property, and drops out of the list.
- s06, t1, s13: a renamed property reference binds to the local or parameter and adds an entry.
- Fix All f02: covered through registration.
- The `this.`/`Sample.`-qualified renamed references bind to the property, are not compared, and stay offered (f01, f03, f04, s02–s05, s07–s11, s14, t2, t3).
- f06 and f10 are offered.

## Policy owners
- `rg -n "Renamer.RenameSymbolAsync|ConflictAnnotation|RewritesOtherCodeAsync|CountReferenceLocationsAsync" src --glob '!*Test*'`:
  - `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`, with `RewritesOtherCodeAsync`, `CountReferenceLocationsAsync`, and `HasConflictAnnotationsAsync` (all private);
  - RH2001's `RenamePropertyToFieldNameAsync` and the new `RenameChangesBindingAsync`.
- Reuse attempt. I first made `RewritesOtherCodeAsync` and `CountReferenceLocationsAsync` `internal` and called them from RH2001. I reverted it, for two reasons:
  - `RewritesOtherCodeAsync` refuses `VerifyFixQualifiesThePropertyReferenceNextToOtherInnerScopeVariablesNamedLikeTheField`, whose output is correct. In `return Value + _value + other;`, the tree diff aligns `Value + ` → `this.` plus an insertion of `_value + ` behind the local `_value`. The insertion touches a token that is not an old name, so the check reports a rewrite.
  - The count check misses t4 in another `partial` document, because that document is not among the changed documents.
- So the rename-safety decision now has a second, RH2001-specific implementation. This is FC-015, which the lead accepted under decision 3 with a follow-up.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 141 insertions(+), 7 deletions(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"`: Failed: 5, Passed: 198, Total: 203.
- `scripts/test.sh --configuration Debug --project analyzer`: Failed: 5, Passed: 9882, Total: 9887. The failures are the same 5 tests.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.

## Disputed tests or scope

**Disputed tests.** In each of these, the fix output keeps every binding. The tests assert `no action`:

| Test | Row | Fix output (apply-fix on unchanged `main`, `git stash`, and on this branch, LF and CRLF) |
|---|---|---|
| `VerifyNoFixIsOfferedWhenASimpleLambdaParameterIsNamedLikeTheField` | f09 | `_value => this._value + _value` |
| `VerifyNoFixIsOfferedWhenAParenthesizedLambdaParameterWithAnInferredTypeIsNamedLikeTheField` | s01 | `(_value) => this._value + _value` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAStaticMethodIsNamedLikeTheFieldOfAStaticProperty` | s12 | `_value => Sample._value + _value` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField` | f09, other part | `this._value + _value` in the other document (registration-time rename result) |
| `VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheField` | f07 | `return _value + this._value();` (`this._value()` is left as written and still binds to `Extensions._value`, candidate reason `None`) |

- **Run-time evidence.** I compiled and ran the before and after code of f07, f09, and s12 with the .NET 11 SDK. Both give `11 101 101`: no compile error and no run-time change.
- **The table these tests came from.** `01-skeptic.md` reports `_value => _value + _value` (101 → 200) for f09/s01/s12 and `Extensions._value(this) + _value()` for f07. Neither reproduces with the test inputs.
- **The spec reading.**
  - RH2001-011 paragraph 2 withholds when a renamed reference "would bind to a symbol other than the field". Here it does not.
  - Paragraph 1's rewritten/left-as-written cases do not occur for f07: nothing is rewritten, and the reference does not rebind.
  - The paragraph-2 list ("a parameter of an implicitly typed lambda") and the change doc's extension-method example describe a renamer result this toolchain does not produce.
- **Why I did not make them pass.** Withholding these would be a visible change from `main` that neither SHARED-FIX-003 nor answer B demands, because `main` offers a correct fix today. The only way to pass them would be a syntactic refusal that predicts renamer behavior, such as "an implicitly typed lambda parameter named like the field encloses a property reference". I did not add it.
- **Options for the lead.**
  - Correct the tests and the spec examples or list: f09/s01/s12 become "offered, qualified with `this.`/the type", and f07 becomes "offered, `_value + this._value()`".
  - Or ask the maintainer whether to withhold anyway.
- **The other members still misbind**, and the guard withholds them: s06, t1, s13 (tests pass).

**Scope.**
- The guard runs a rename, and builds semantic models of every referring document twice, at registration for each diagnostic. That is the same cost profile as the casing fixes' `HasRenameConflictAsync`.
- Production diff: about 134 net lines in one file, no new type, no new public member.

## Architecture findings
- **Components.** `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and RH2001's `RenameChangesBindingAsync`.
- **Shared decision.** Whether a rename keeps every other reference's binding (SHARED-CASING-009 binding paragraphs).
- **Input where they disagree.** `VerifyFixQualifiesThePropertyReferenceNextToOtherInnerScopeVariablesNamedLikeTheField`, with a deconstruction local `_value` read as `Value + _value + other`. `RewritesOtherCodeAsync`, the casing owner, reports a rewrite because of the text-diff alignment. RH2001's comparison by binding finds none.
- **Status.** This extends the FC-015 finding of `01-skeptic.md`. The lead already decided it under decision 3: narrow fix plus a follow-up.

## Follow-up candidates
- **False refusal in the casing check.** `CasingCodeFixProviderBase<T>.RewritesOtherCodeAsync` withholds a correct rename when the tree diff aligns a qualified renamed reference with a neighbouring identifier (`Value + _value + other` → `this._value + _value + other`). It is a withheld fix, not unsafe output. I did not run it against a casing rule. Fold it into the existing FC-015 unification follow-up.
