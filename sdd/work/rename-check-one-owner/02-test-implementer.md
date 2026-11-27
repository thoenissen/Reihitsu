Role: test implementer
Commit: bcfe9ca
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict; none was used. Expected values come from SHARED-CASING-009 ("A reference that the renamer renames, or renames and qualifies with `this.`, `base.`, a type, or a namespace, does not withhold the fix") and from the renamer's own output on the holding sequences, not from patched code.

## Tests

All in `tests/Reihitsu.Analyzer.Test/Naming/`. Helper for every test: `Verify(source, fixedSource, …)` on `BatchCodeFixTestsBase` (single fix, one pass, re-analysis, and Fix All).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106PrivateFieldCasingAnalyzerTests.FixQualifiesTheFieldReferenceNextToALocalWithTheNewName` rows `Value + _value + other`, `Value + _value + 1`, `Value + _value + _value`, `Value * _value * Value` | SHARED-CASING-009 | `Verify` | **fail** (no fix offered) |
| same test, rows `Value + _value`, `_value + Value + other`, `other + Value + _value`, `Value + _value - Value` | SHARED-CASING-009 | `Verify` | pass |
| `RH4106…FixQualifiesTheFieldReferenceNextToADeconstructionVariableWithTheNewName` (issue's input 2) | SHARED-CASING-009 | `Verify` | **fail** |
| `RH4106…FixQualifiesTheFieldReferenceNextToAParameterWithTheNewName` (`int Get(int _value, int other) { return Value + _value + other; }`) | SHARED-CASING-009 | `Verify` | **fail** — a member the skeptic's matrix did not list (its row 16 is the two-operand parameter case, which holds) |
| `RH4106…FixQualifiesTheFieldReferenceInsideAnArgumentNextToALocalWithTheNewName` (row 6) | SHARED-CASING-009 | `Verify` | **fail** |
| `RH4106…FixQualifiesTheStaticFieldReferenceWithItsTypeNextToALocalWithTheNewName` row `Value + _value + other` → `Sample._value + _value + other` | SHARED-CASING-009 | `Verify` | **fail** — type-qualified member not in the skeptic's failing rows |
| same test, row `Value + _value` → `Sample._value + _value` (row 17) | SHARED-CASING-009 | `Verify` | pass |
| `RH4106…FixQualifiesTheAssignedFieldReferenceNextToALocalWithTheNewName` (row 18) | SHARED-CASING-009 | `Verify` | pass |
| `RH4106…FixAllQualifiesTheReferencesOfEveryFieldNextToLocalsWithTheNewNames` (two fields, two locals, failing sequence; FC-008) | SHARED-CASING-009 | `Verify` (single and Fix All) | **fail** |
| `RH4106…NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField` (input 1 on RH4106) | SHARED-CASING-009 | `Verify` | pass (withheld) |
| `RH4111PrivatePropertyCasingAnalyzerTests.FixQualifiesThePropertyReferenceNextToALocalWithTheNewName` row `value + Value + other` (row 7) | SHARED-CASING-009 | `Verify` | **fail** |
| same test, row `value + Value` (row 19) | SHARED-CASING-009 | `Verify` | pass |
| `RH4111…FixQualifiesTheAssignedPropertyReferenceNextToAParameterWithTheNewName` (row 19, `value = Value;`) | SHARED-CASING-009 | `Verify` | pass |
| `RH4103MethodNameCasingNameBindingTests.FixQualifiesTheCallNextToALocalFunctionWithTheNewName` row `getValue() + GetValue() + other` (row 9, conforming capturing symbol) | SHARED-CASING-009 | `Verify` | **fail** |
| same test, row `getValue() + GetValue()` (row 20) | SHARED-CASING-009 | `Verify` | pass |
| `RH4103…FixQualifiesTheCallNextToALocalDelegateWithTheNewName` (row 8) | SHARED-CASING-009 | `Verify` | **fail** |
| `RH4103…FixAllQualifiesTheCallNextToALocalFunctionAndSkipsTheRenameThatWouldCastAnArgument` — one document: `write(object)`/`Write(int)` with `box.write(1)` must stay withheld (diagnostic kept), `getValue` next to local function `GetValue` must be fixed, single fix and Fix All | SHARED-CASING-009 | `Verify` (markup allowed in fixed state, one Fix All iteration) | **fail** (the `getValue` part only; the withheld `write` part already matches) |

Every failing test fails on the issue's own difference: the expected-versus-actual diff is exactly the declaration and the qualified reference left unrenamed (no fix offered), nothing else. Observed: 14 failing rows/tests as marked, 10 holding rows/tests passing.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Input 2: RH4106 `Value` next to deconstruction local `_value`, `return Value + _value + other;` | LF (test); LF and CRLF (`scripts/apply-fix.sh`, skeptic) | code fix, single | reproduced | no fix offered vs `private int _value;` / `return this._value + _value + other;` |
| Static field, `Value + _value + other` | LF and CRLF (`scripts/apply-fix.sh RH4106`) | code fix | reproduced | no fix vs `Sample._value + _value + other` |
| Two fields with locals in one document | LF and CRLF (`apply-fix.sh`), LF (test) | single fix and Fix All | reproduced | no fix vs both qualified |
| Input 1 on RH4106 (`new { Value }`, `x.Value`) | LF and CRLF (`apply-fix.sh`), LF (test) | code fix | holds | no fix offered, as SHARED-CASING-009 (compile error) requires |
| Counterpart: RH2001 on input 2 | — | RH2001 code fix | holds, already pinned | see *Not covered* |

## Not covered, and why

- **RH2001 side of input 2**: pinned by `RH2001PrivateAutoPropertiesShouldNotBeUsedNameBindingTests` (the test whose `Split()` holds `var (_value, other) = (1, 2); return Value + _value + other;` → `this._value + _value + other`, near line 1366) and `VerifyFixQualifiesThePropertyReferenceNextToALocalNamedLikeTheField`. The skeptic reported the three-operand RH2001 shape as unpinned; it is pinned. Input 1 on RH2001 is pinned by `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName`.
- **Rewrites that must stay withheld**: pinned by `RH4103MethodNameCasingNameBindingTests.NoFixIsOfferedWhenTheRenamerWouldCastAnArgumentOfACallOfTheRenamedMethod` and `…TurnACallOfTheRenamedExtensionMethodIntoAStaticCall`, plus their Fix All siblings; not duplicated. The new mixed Fix All test adds the case where a withheld cast rewrite and a qualification-only rename share a document. I tried a qualification and a cast on the same call (`write(object)` called as `write(1)` next to a local function `Write(int)`): the renamer produced `this.Write(1)` with no cast, so no such combined input was found.
- **Namespace-qualified type rename (row 21)**: pinned by `RH4002ClassNameCasingAnalyzerTests.FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`; I found no failing type-rename sequence and wrote none.
- **Tuple inferred-name difference** (RH4106 fixed, RH2001 withheld): spec-decided difference outside this run's change; not added.
- **CRLF in tests**: the change is in the rename-safety check, which reads no line ending; the skeptic and I observed LF and CRLF identical with `scripts/apply-fix.sh` for every scenario above. No layout is involved, so no separate CRLF test.
- **Remaining rows 3/4/5 variants on other rules, and lambda-parameter capture**: the check is one generic method for every casing rule (`CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` → `CasingRenameSafetyUtilities.RewritesOtherCodeAsync`, `rg -n "RewritesOtherCodeAsync" src/`); one rule per qualifier kind (`this.` on field, property, method; type on static field) is covered.
- Failure classes: FC-015/FC-016/FC-044/FC-008 covered above. Formatter, comment/directive-in-gap, idempotency classes do not reach the change (rename semantics only, no layout). FC-036 (rename semantics) not affected: references retargeted by the Renamer are asserted in every fixed source.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixQualifies|FullyQualifiedName~FixAllQualifies|FullyQualifiedName~NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4103MethodNameCasingNameBindingTests|FullyQualifiedName~RH4106PrivateFieldCasingAnalyzerTests|FullyQualifiedName~RH4111PrivatePropertyCasingAnalyzerTests|FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedNameBindingTests"
```

## Follow-up candidates

_None._
