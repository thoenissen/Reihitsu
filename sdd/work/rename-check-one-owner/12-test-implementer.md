Role: test implementer
Commit: 2f832c2
Verdict: tests written

Repair round 2, auditor finding #2 (`11-auditor.md`), lead decision 6. The prompt carried no intended fix, suspected cause, or hoped-for verdict. It did carry a conditional step for the implementer: drop the cref arm if no input reaches it. That step is not needed. The harness reaches the arm (see shape (d) below).

Expected values come from SHARED-CASING-009: "A reference that the renamer renames, or renames and qualifies with `this.`, `base.`, a type, or a namespace, does not withhold the fix". Each fixed text is the renamer output that the auditor probed. None comes from patched code.

## Tests

Helper for every test: `Verify(source, fixedSource)` on `BatchCodeFixTestsBase`. It covers the single fix, one pass, re-analysis, and Fix All.

| Test | Requirement / failure class | Helper | Expected now (head) | On `main` (src at `origin/main`) | Arm removed (probe) |
|---|---|---|---|---|---|
| (a) `RH4103MethodNameCasingNameBindingTests.FixQualifiesTheCallWithBaseNextToALocalFunctionWithTheNewName`: `base.GetValue()` | SHARED-CASING-009 | `Verify` | pass | fail (no fix offered) | fail |
| (b) `RH4002ClassNameCasingAnalyzerTests.FixQualifiesAReferenceWithGlobalThatANestedTypeOfTheNewNameWouldCapture`: `global::Order _value;` | SHARED-CASING-009 | `Verify` | pass | pass | fail |
| (c) `RH4103…FixQualifiesTheCallWithTheNestedBaseTypeAndItsContainingTypeNextToALocalFunctionWithTheNewName`: `Outer.Base.GetValue()` | SHARED-CASING-009 | `Verify` with `CodeActionValidationMode.None` | pass | pass | fail |
| (c) `RH4103…FixQualifiesTheCallWithTheBaseTypeAndItsNamespaceNextToALocalFunctionWithTheNewName`: `A.B.Base.GetValue()` | SHARED-CASING-009 | `Verify` with `CodeActionValidationMode.None` | pass | pass | fail |
| (c) `RH4106PrivateFieldCasingAnalyzerTests.FixQualifiesTheStaticFieldReferenceWithItsTypeAndNamespaceWhenANestedTypeHidesTheTypeName`: `N.Sample._value` | SHARED-CASING-009 | `Verify` with `CodeActionValidationMode.None` | pass | fail (no fix offered) | fail |
| (c) neighbor `RH4106…FixQualifiesTheStaticFieldReferenceWithGlobalAndItsTypeWhenANestedTypeHidesTheTypeName`: `global::Sample._value` | SHARED-CASING-009 | `Verify` | pass | fail (no fix offered) | pass (the left side is an alias-qualified name, which is a `TypeSyntax`, so no removed arm applies) |
| (d) `RH4103…FixRenamesTheDocumentationReferenceNextToANestedTypeWithTheNewName`: `<see cref="getValue"/>` becomes `<see cref="Sample.GetValue"/>` | SHARED-CASING-009 | `Verify` | pass | pass | fail |

All 7 pass on the head. On `main`, rows (a) and the two RH4106 rows get no fix. That is the issue's own behavior, and the head now fixes them.

**Shape (d) is reached.** The harness renames crefs. Inside `class Sample { static getValue(); class Nested { class GetValue {} /// <see cref="getValue"/> } }`, the renamer writes `Sample.GetValue` into the cref, and the fix is offered. The probe removed the `QualifiedCrefSyntax` arm. The test then failed on exactly that difference: no fix, and the declaration and the cref stayed as written. The cref arm is live, so the implementer has nothing to drop.

**Probe.** I ran it in a temporary detached worktree in the scratchpad, which I have since removed. The working tree's production code was never touched. The probe made three changes in `CasingRenameSafetyUtilities`:
- It removed the `AliasQualifiedNameSyntax` arm.
- It removed the `QualifiedCrefSyntax` arm.
- It narrowed the accepted qualifier to `ThisExpressionSyntax or (TypeSyntax and not QualifiedNameSyntax)`. That drops `base` and multi-part qualified names.

Six of the new tests failed, each on the no-fix difference. The existing `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` (`N.Order`) still passed, because its left side is a simple name. Each arm has its own test, so a change that drops or narrows one arm fails.

**Harness note on (c).** By default the harness compares the syntax structure of the fix result with the result of parsing the fixed text. For multi-part qualifiers in an expression context, the renamer builds a `QualifiedName` node, while the same text parses as a chain of `SimpleMemberAccessExpression` nodes. So three (c) tests fail the structure check even though the text matches. This confirms the auditor's statement that these shapes are accepted only because the renamer builds a `QualifiedNameSyntax`.

Those three tests pass `test => test.CodeActionValidationMode = CodeActionValidationMode.None`, with a comment saying why. The fixed text is still compared in full. Only the node-kind comparison is skipped. No other existing test needed this.

## Scenarios (bug fix)

_Not applicable._ This is a repair round for a test gap, not a reproduction. Line endings: the check reads no line ending, and the round-1 probes gave LF and CRLF the same result. No layout is involved.

## Not covered, and why

- **`M.Order.Count` (RH4002, expression context):** this is the same shape as the other (c) rows, through the same `QualifiedName`-left-side acceptance in `FindQualifiedNameToken`. The RH4103 and RH4106 rows already pin that path, and the qualifier shape does not depend on which casing rule renames. `RewritesOtherCodeAsync` has one caller, `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`. That is unchanged from `07-test-implementer.md`.
- **`this.` qualifier:** already pinned by the existing RH4106 `FixQualifiesTheAssignedFieldReferenceNextToALocalWithTheNewName` and its neighbors.
- **Failure classes:** FC-016 and FC-017 are covered by one test per arm, and the probe confirms each one. Formatter, layout, gap, and line-ending classes do not reach the rename check.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixQualifiesTheCallWithBase|FullyQualifiedName~FixQualifiesTheCallWithTheNestedBaseType|FullyQualifiedName~FixQualifiesTheCallWithTheBaseTypeAndItsNamespace|FullyQualifiedName~FixRenamesTheDocumentationReferenceNextToANestedType|FullyQualifiedName~FixQualifiesAReferenceWithGlobal|FullyQualifiedName~WhenANestedTypeHidesTheTypeName"
```

## Follow-up candidates

_None._ The auditor's hint that `scripts/apply-fix.sh` does not rename crefs was not investigated further. It is pre-existing tooling, and the analyzer harness now covers the cref arm.
