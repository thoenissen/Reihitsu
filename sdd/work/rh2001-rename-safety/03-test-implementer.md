Role: test implementer
Commit: 3e983dc
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs` (class derives from `BatchCodeFixTestsBase`, unchanged). Row IDs refer to `01-skeptic.md`. Expected values come from the RH2001-011 examples and its third paragraph (a reference to the property is renamed and qualified with `this.` only where an inner-scope symbol would otherwise answer to the name; every other reference keeps its text), never from a patched copy.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadUnqualified` (LF, CRLF) — f02 | RH2001-011; FC-033 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughThisAndThroughAReceiver` — spec example f05+t4 | RH2001-011 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughThis` — t4 | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughAReceiver` — f05 | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedPropertyIsReadThroughThis` — e2 | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughAReceiverInANestedType` — f08 | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheField` — f07 | RH2001-011 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAStaticallyImportedMemberIsNamedLikeTheField` — s16 | RH2001-011 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAMemberOfTheEnclosingTypeIsNamedLikeTheField` — e1 | RH2001-011 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenASimpleLambdaParameterIsNamedLikeTheField` — f09 | RH2001-011; FC-006 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAParenthesizedLambdaParameterWithAnInferredTypeIsNamedLikeTheField` — s01 | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAStaticMethodIsNamedLikeTheFieldOfAStaticProperty` — s12 | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnOutVarVariableOfTheSameExpressionIsNamedLikeTheField` — s06 | RH2001-011; FC-006 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnExplicitlyTypedOutVariableOfTheSameExpressionIsNamedLikeTheField` — t1 | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAPrimaryConstructorParameterIsNamedLikeTheField` — s13 | RH2001-011; FC-006 | `VerifyNoCodeFixIsOffered` | fails: action registered |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadInAPartialPartInAnotherDocument` — t4 in another `partial` part | RH2001-011; FC-024 | `Verify(source, source, …)` + second document | fails: fix renames `Value` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField` — f09 in another `partial` part | RH2001-011; FC-024 | `Verify(source, source, …)` + second document | fails: fix renames `Value` |
| `VerifyFixAllLeavesAPropertyWhoseFieldNameIsBoundAndReplacesTheOthers` — f02 next to two plain properties | RH2001-011; FC-008 | `Verify` (iterative + Fix All, `MarkupMode.Allow`) | fails: `Value` fixed to `_value + base._value` |
| `VerifyFixQualifiesThePropertyReferenceNextToAParameterNamedLikeTheFieldAndAnInheritedField` (LF, CRLF) — spec example f01 | RH2001-011; FC-033 | `Verify` with `WithEndOfLine` | passes |
| `VerifyFixIsOfferedNextToAnInheritedMethodNamedLikeTheField` — spec example f06 | RH2001-011 | `Verify` | passes |
| `VerifyFixIsOfferedNextToAnInheritedFieldThatNoReferenceNames` — f10 | RH2001-011; FC-003 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToALocalNamedLikeTheField` — f03 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToAMethodParameterNamedLikeTheField` — f04 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToAnExplicitlyTypedLambdaParameterNamedLikeTheField` — s02 | RH2001-011; FC-003 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToAnAnonymousMethodParameterNamedLikeTheField` — s03 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionParameterNamedLikeTheField` — s04 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToAPatternVariableNamedLikeTheField` — s05, t2 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToAnOutVarVariableOfAnEarlierStatement` — t3 | RH2001-011; FC-003 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToOtherInnerScopeVariablesNamedLikeTheField` — s07, s09, s08, s15 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionNamedLikeTheField` — s10 | RH2001-011 | `Verify` | passes |
| `VerifyFixQualifiesThePropertyReferenceNextToACapturedLocalNamedLikeTheField` — s11 | RH2001-011 | `Verify` | passes |
| `VerifyFixIsOfferedNextToAMemberOfANestedTypeNamedLikeTheField` — s14 | RH2001-011 | `Verify` | passes |

Each red test fails on its own expected-versus-actual difference: the `VerifyNoCodeFixIsOffered` tests at `Assert.IsEmpty(actions)` (the analyzer part passes, so the diagnostic is still reported), the `Verify` tests at "Iterative code fix application" with the fix's rename as the diff. The passing tests pin today's `main` output, which RH2001-011 paragraph 3 keeps. Run: 19 failed, 184 passed of 203 in the class.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| f02 inherited field unqualified | LF, CRLF | code fix | reproduced | action registered (fix ships `_value + base._value`) vs no action |
| f05/t4/e2/f08 inherited member via `this.`/receiver | LF | code fix | reproduced | action registered vs no action |
| f09/s01/s12 inferred lambda parameter | LF | code fix | reproduced | action registered vs no action |
| s06/t1 `out` variable in same expression | LF | code fix | reproduced | action registered vs no action |
| s13 primary-constructor parameter | LF | code fix | reproduced | action registered vs no action |
| f07/s16/e1 rewritten reference (answer B) | LF | code fix | reproduced | action registered vs no action |
| t4 and f09 in another `partial` part | LF | code fix (iterative) | reproduced | `Value` renamed vs unchanged |
| Fix All, f02 next to `Count`, `Total` | LF | Fix All | reproduced | `Value` fixed vs left reported |
| f01 issue example | LF, CRLF | code fix | not reproduced (correct, as specified) | `this._value + _value` as expected |

## Not covered, and why

- **Fix All with two properties deriving one field name** (`Value` + `value`): dropped by lead decision 4.
- **Formatter surface, formatter stability and unsupported-shape classes (FC-010–FC-014, FC-028, FC-038–FC-043):** the change adds a binding check to the code fix; no formatter phase or precheck is touched, and the fix's layout path is unchanged.
- **Trivia and directive classes (FC-004, FC-005, FC-019–FC-021):** no token gap or trivia handling changes; RH2001-007/009/010 tests stand.
- **FC-007 convergence / FC-018 renamer:** covered by `Verify`'s iterative application and re-analysis in every passing test, whose expected output retargets references through the renamer.
- **FC-009:** every withheld test asserts that no action is registered, not merely an unchanged document.
- **FC-023 verbatim identifiers (`@_value`):** not added; `@_value` is the same name to lookup as `_value`, so it reaches the same binding check as the plain local/inherited cases.
- **FC-036 (`nameof`):** unchanged by this run; RH2001-004's existing tests cover `nameof`.
- **FC-015:** architecture follow-up recorded by the lead, not a test.
- An interface member named `_value`: deliberately outside RH2001-011 (02-spec-author, internal choices); no test asserts either way.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyNoFixIsOffered|FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFix"
```
