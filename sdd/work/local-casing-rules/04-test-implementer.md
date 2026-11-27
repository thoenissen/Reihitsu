Role: test implementer
Commit: 618f0b26 (tests written on this head; the commit carrying them follows it)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (RH4105, RH4115, RH4117, RH4128, SHARED-CASING-015) and the two accepted change docs, never from code. All tests are analyzer tests in `tests/Reihitsu.Analyzer.Test/Naming/`; no formatter surface is involved.

**Build note for the implementer.** `RH4128LocalConstantCasingAnalyzerTests.cs` references `Reihitsu.Analyzer.Rules.Naming.RH4128LocalConstantCasingAnalyzer` (with `DiagnosticId`) and `Reihitsu.Analyzer.CodeFixes.Rules.Naming.RH4128LocalConstantCasingCodeFixProvider` (the spec's code owners), so the analyzer test project, and with it the architecture tests, do not compile until those two types exist. The provider must return a Fix All provider (the class derives from `BatchCodeFixTestsBase`). Every other new test was run with that file moved aside.

**Validation of the RH4128 expectations.** A temporary copy of the RH4128 class, aliased to the RH4115 analyzer and fix (which rename local constants today), was run and deleted (not committed): 8 of 12 passed; the 4 failures are exactly the RH4128-specific differences — `Total`/`Max_Count`/`Name` reported by RH4115 (non-const locals), `_` reported, and `new { maxValue }` instead of `new { MaxValue = maxValue }`. So the rename results the RH4128 tests expect are the shared rename's.

## thoenissen/Reihitsu#1087

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4115…Tests.LocalNamedDiscardIsNotReported` (replaces `NoCodeFixForLetterlessLocalVariable`) | RH4115-004 | `Verify(source)` | fails (5 reported, 1 expected) |
| `RH4115…Tests.LocalNamedDiscardIsNotReportedInForUsingAndFixedStatements` | RH4115-004 | `Verify(source)` | fails (3 vs 0) |
| `RH4115…Tests.LocalConstantsAreNotChecked` | RH4115-005 | `Verify(source)` | fails (3 vs 1) |
| `RH4115…Tests.LocalConstantsAreNotCheckedInAnyContainer` | RH4115-005 | `Verify(source)` | fails (5 vs 0) |
| `RH4115…Tests.VariablesOfEveryDeconstructionFormAreNotChecked` | RH4115-002 | `Verify(source)` | fails (10 vs 0) |
| `RH4115…Tests.PatternVariablesOutsideADeconstructionAreReported` | RH4115-002 | `Verify(source)` | passes |
| `RH4115…Tests.DeconstructionVariablesInNestedAndSwitchPositionsAreNotChecked` | RH4115-002 | `Verify(source)` | fails (22 vs 0) |
| `RH4115…Tests.PatternVariablesThatReceiveNoWholeElementAreReported` | RH4115-001, RH4115-002 | `Verify(source)` | passes |
| `RH4115…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheLocal` | SHARED-CASING-015, RH4115-003 | `Verify(source, fixed)` | fails (`total` instead of `Total = total`) |
| `RH4115…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberThatToStringReads` | SHARED-CASING-015 | `Verify(source, fixed)` | fails (`new { total }`) |
| `RH4115…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberReadByItsName` | SHARED-CASING-015 | `Verify(source, fixed)` | fails (fix withheld today) |
| `RH4115…Tests.FixKeepsTheLayoutAndCommentsOfAMultiLineAnonymousObjectMember` (LF, CRLF) | SHARED-CASING-015 | `VerifyCodeFixWithEndOfLine` | fails on the member names only; own-line, trailing `//`, and `/* */` comments and the layout already match |
| `RH4115…Tests.FixKeepsTheNameOfEveryAnonymousObjectMemberNamedAfterTheLocal` | SHARED-CASING-015 | `Verify(source, fixed)` | fails (`new { total }` twice, nested) |
| `RH4115…Tests.FixWritesNoNameForAMemberNamedAfterAnotherIdentifier` | SHARED-CASING-015 | `Verify(source, fixed)` | passes (`new { text.Length, Copy = text }`) |
| `RH4117…Tests.VariablesDeclaredInsideATupleOfADeconstructionAreReported` | RH4117-001 | `Verify(source)` | fails (4 vs 8) |
| `RH4117…Tests.FixRenamesVariablesDeclaredInsideATupleOfADeconstruction` (change doc case 4) | RH4117-001, RH4117-002 | `Verify(source, fixed)` | fails (2 vs 7) |
| `RH4117…Tests.FixRenamesTheVariablesOfADeconstructionPattern` | RH4117-002, RH4117-003 | `Verify(source, fixed)` | fails (0 vs 3) |
| `RH4117…Tests.FixRenamesTheVariablesOfAVarPatternInAnIsExpressionAndASwitchStatement` (case 5) | RH4117-002, RH4117-003 | `Verify(source, fixed)` | fails (0 vs 4) |
| `RH4117…Tests.FixRenamesEveryVariableThatReceivesAWholeElementOfAPositionalPattern` (case 6, answer 1B) | RH4117-002, RH4117-003 | `Verify(source, fixed)` | fails (0 vs 5) |
| `RH4117…Tests.VariablesOfADeconstructionPatternAreReported` | RH4117-003 | `Verify(source)` | fails (0 vs 11) |
| `RH4117…Tests.PatternVariablesThatAreNoDeconstructionVariablesAreNotReported` | RH4117-003 | `Verify(source)` | fails (0 vs 1: `Whole`) |
| `RH4117…Tests.DeconstructionVariablesInNestedAndSwitchPositionsAreReported` | RH4117-001, RH4117-003 | `Verify(source)` | fails (0 vs 27) |
| `RH4117…Tests.PatternVariablesThatReceiveNoWholeElementAreNotReported` | RH4117-001, RH4117-003 | `Verify(source)` | passes |
| `RH4117…Tests.NoFixIsOfferedWhenTheNewNameOfAPatternVariableWouldCollide` | SHARED-CASING-009 | `Verify(source, source)` | fails (0 vs 1 diagnostic) |
| `RH4117…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheVariable` (range-variable-casing case 4) | SHARED-CASING-015, RH4117-002 | `Verify(source, fixed)` | fails (`new { first, second }`) |
| `RH4117…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterAPatternVariable` | SHARED-CASING-015 | `Verify(source, fixed)` | fails (0 vs 1 diagnostic) |

`RH4115…` is `RH4115LocalVariableCasingAnalyzerTests`, `RH4117…` is `RH4117DeconstructionVariableCasingAnalyzerTests`.

The two `…NestedAndSwitchPositions…` tests share one source, written once without markup (RH4115) and once with 27 RH4117 marks, so the boundary is pinned from both rules (FC-015): nested tuple `((var a, var b), var c) = …`, `(existing, var b) = …`, nested tuple in `foreach`, nested `var ((a, b), c)` pattern, nested positional pattern, typed `Point(var x, var y)`, positional inside a property pattern `{ Pair: (var k, _) }` and inside a list pattern `[(var h, _), ..]`, named subpattern `(Item1: var n, _)`, `(> 0 and int p, _)` (declaration on the right of `and`), `((var w), (int t))`, `([_, _] items, _)`, `((1, 2) matched, _)`, `case (var s, var e):`, and a switch-expression arm `var (l, u) =>`. The two `…NoWholeElement…` tests likewise share one source for the other side: `input is (var Paren)` (top-level parenthesized), list element and slice, a list element inside a positional element `([var Element, ..], _)`, `{ Pair: var Whole }`, the designation of a whole positional pattern `(var item, _) Outer`, `is var Value`, and `out var Parsed`.

### Existing tests

- Changed: `RH4115LocalVariableCasingAnalyzerTests.NoCodeFixForLetterlessLocalVariable` pinned `int _ = 42;` reported without fix; replaced by `LocalNamedDiscardIsNotReported` (RH4115-004).
- Left, still valid and green: `VerifyNoDiagnosticsForDeconstructionVariables`, `VariablesOfVarDeconstructionsAreNotChecked` (RH4115-002, `var (…)` only), `LocalVariablesOfEveryDeclarationFormAreChecked`, `DesignationsInSwitchesInitializersAndSlicePatternsAreChecked` (only shapes that stay RH4115's), `CollisionIsRefusedForSupportedLocalDeclarationForms` (`is int BadName` stays RH4115's), and every RH4117 test.
- Searched `tests/` for `const` locals not in camelCase, for `is var (`/positional subpatterns, and for anonymous objects in casing tests: no other test asserts an output this change alters.

## thoenissen/Reihitsu#1088

### Tests

All in the new `RH4128LocalConstantCasingAnalyzerTests` (`BatchCodeFixTestsBase`); none compiles before the implementer adds the types (see the build note above).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LocalConstantsThatAreNotInCamelCaseAreReported` (spec example; `Total` not reported) | RH4128-001 | `Verify(source)` | does not compile |
| `LocalConstantsInAccessorsOperatorsFinalizersAndSwitchSectionsAreReported` | RH4128-001 | `Verify(source)` | does not compile |
| `LocalConstantOfTopLevelStatementsIsReported` | RH4128-001 | `Verify(source)`, `OutputKind.ConsoleApplication` | does not compile |
| `LocalVariablesWithoutConstAreNotReported` | RH4128-001 | `Verify(source)` | does not compile |
| `LocalConstantNamedDiscardIsNotReported` | RH4128-002 | `Verify(source)` | does not compile |
| `FixRenamesLocalConstantsAndTheirReferences` (constant value, `case` label) | RH4128-003 | `Verify(source, fixed)` | does not compile |
| `FixRenamesAReferenceInAnAttributeArgumentOfALocalFunction` | RH4128-003 | `Verify(source, fixed)` | does not compile |
| `NoFixIsOfferedWhenTheNewNameWouldCollideWithALocalVariable` | SHARED-CASING-009 | `Verify(source, source)` | does not compile |
| `NoFixIsOfferedWhenTheDerivedNameIsAReservedKeyword` (`Class` → `class`) | SHARED-CASING-013 | `Verify(source, source)` | does not compile |
| `FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheConstant` | SHARED-CASING-015, RH4128-003 | `Verify(source, fixed)` | does not compile |
| `FixAllCorrectsEveryDiagnostic` (two constants of one declaration, one Fix All iteration) | RH4128-003, SHARED-CASING-010 | `GetFixAllScenario` | does not compile |
| `CodeFixProvidesFixAllProvider` (inherited) | — | base | does not compile |

## thoenissen/Reihitsu#1978

### Tests

All in `RH4105MethodParameterCasingAnalyzerTests`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RangeVariablesOfEveryQueryClauseAreReported` (spec example 1; later uses not reported) | RH4105-005 | `Verify(source)` | fails (0 vs 7) |
| `RangeVariablesInCamelCaseAreNotReported` (spec example 2) | RH4105-005 | `Verify(source)` | passes |
| `RangeVariablesInEveryEnclosingScopeAreReported` (later `from`, typed `join`, nested query, lambda, field initializer) | RH4105-005 | `Verify(source)` | fails (0 vs 5) |
| `VerbatimRangeVariableIsReportedByItsValueText` (`from @Value`) | RH4105-005 | `Verify(source)` | fails (0 vs 1) |
| `DiscardParametersRangeVariablesAndRecordParametersAreNotReported` (spec example, `from _`) | RH4105-002 | `Verify(source)` | passes |
| `FixRenamesRangeVariablesAndTheirReferencesAcrossAContinuation` (RH4105-006 example 1) | RH4105-006 | `Verify(source, fixed)` | fails (0 vs 5) |
| `FixRenamesRangeVariablesOfAJoinIntoAndASelectContinuation` (change doc case 1) | RH4105-005, RH4105-006 | `Verify(source, fixed)` | fails (0 vs 5) |
| `FixKeepsTheNamesOfAnonymousObjectMembersThatRangeVariablesName` (RH4105-006 example 2, case 2) | RH4105-006, SHARED-CASING-015 | `Verify(source, fixed)` | fails (0 vs 2) |
| `FixKeepsTheNameOfAnAnonymousObjectMemberInANestedQuery` | SHARED-CASING-015 | `Verify(source, fixed)` | fails (0 vs 1) |
| `NoFixIsOfferedWhenTheDerivedRangeVariableNameNeedsTheVerbatimPrefix` (`from Group` → `@group`) | SHARED-CASING-013 | `Verify(source, source)` | fails (0 vs 1) |
| `NoFixIsOfferedWhenTheNewRangeVariableNameWouldCollideWithALocal` (CS1931 after rename) | SHARED-CASING-009 | `Verify(source, source)` | fails (0 vs 1) |
| `RangeVariablesWithTheSameNewNameInOneQueryAreNotFixed` (`from Value … from VALUE`: single fix renames the first and is withheld for the second; Fix All leaves both) | SHARED-CASING-009, SHARED-CASING-010 | `Verify` with `BatchFixedCode` | fails (0 vs 2) |
| `NoFixIsOfferedWhenAnExcludedBranchReferencesTheRangeVariable` (`#if NEVER` clause inside the query) | SHARED-CASING-007 | `Verify(source, source)` | fails (0 vs 1) |
| `FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterALambdaParameter` (SHARED-CASING-015 example 1, RH4105 part) | SHARED-CASING-015, RH4105-003 | `Verify(source, fixed)` | fails (`new { value, … }`) |
| `FixWritesNoNameForAMemberNamedAfterAnotherIdentifier` (`new { Text.Length }`) | SHARED-CASING-015 | `Verify(source, fixed)` | passes |

## Whole run

### SHARED-CASING-015 for every fix using the shared rename (answer 4A)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberThatTakesItsNameFromTheField` (replaces `NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField`, retagged from SHARED-CASING-009) | SHARED-CASING-015 | `Verify(source, fixed)` | fails (fix withheld today) |
| `RH4106…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedByAMemberAccess` (SHARED-CASING-015 example 2: `this.Value`, named `Other = other?.Value`, read by name) | SHARED-CASING-015 | `Verify(source, fixed)` | fails (withheld) |
| `RH4106…Tests.FixKeepsTheNamesOfAnonymousObjectMembersInEveryMethod` (range-variable-casing case 4, RH4106) | SHARED-CASING-015 | `Verify(source, fixed)` | fails (withheld) |
| `RH4106…Tests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedByAConditionalAccess` (`new { other?.Value }`) | SHARED-CASING-015 | `Verify(source, fixed)` | fails (`new { other?._value }`) |
| `FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheRenamedName` in each of RH4101 (`Color.red`), RH4102 (event), RH4107, RH4108, RH4109, RH4110 (fields), RH4111 (`other.value`), RH4112, RH4113, RH4114 (properties), RH4120 (record parameter in a property initializer) | SHARED-CASING-015 | `Verify(source, fixed)` | each fails on the member name only (e.g. `new { Color.Red }` instead of `new { red = Color.Red }`) |

One test per rule, not one in total: every provider is its own `CasingCodeFixProviderBase<T>` subclass with its own node type and overrides, which `README.md` says is no single-call-site proof. RH4103, RH4104, RH4121, and the RH40xx type rules use the same base, but their names (method group, local function, type parameter, type) cannot be the inferred name of an anonymous object member, so no input reaches SHARED-CASING-015 through them.

### Failure classes walked

- FC-002, FC-003 (both sides of every condition): `_` vs `_count`/`_limit`; `const` vs non-const in both RH4115 and RH4128; each RH4117-003 position next to the RH4115 position it borders (shared sources above); a member named by the renamed reference vs one with its own name (`Item = Value`, `Other = other?.Value`) vs one named after another identifier (`Text.Length`).
- FC-015, FC-016: the deconstruction boundary is asserted from both analyzers on the same sources; SHARED-CASING-015 is asserted on every rule that uses the shared rename.
- FC-018 (references through the Renamer): every fix test has references, including across a query continuation, in a `case` label, another constant's value, and an attribute argument of a local function.
- FC-008 (Fix All): `Verify` runs Fix All on every fix test (5 range variables in one query, 5 pattern variables, two constants of one declaration with one iteration, two anonymous members in one object); same-new-name range variables under Fix All.
- FC-007 (one-pass convergence) and FC-006 (compiles): `Verify` re-analyzes the fixed code and fails on compiler errors.
- FC-023: `_`, `_count`, `_limit`, `@Value`.
- FC-027 (nesting and enclosing scopes): nested tuples, designations, and positional patterns; positional inside property and list patterns; nested query, query in a lambda and in a field initializer; constants in a constructor, anonymous method, local function, lambda, accessor, operator, finalizer, switch section, and top-level statements; nested anonymous object.
- FC-036: anonymous members (SHARED-CASING-015), with `ToString()` and a read by name as observers.
- FC-019/FC-021 (trivia next to the rewrite): SHARED-CASING-015 inserts `Name = ` in front of an expression, so the layout test carries an own-line comment, a trailing `//` comment, and a `/* */` comment, under LF and CRLF.

## Not covered, and why

- FC-004, FC-005, FC-020 (a directive in the gap): SHARED-CASING-015 joins no lines and moves no token; a directive inside the anonymous object is either active (the rewrite does not touch it) or excludes a reference, which SHARED-CASING-007 already withholds — pinned for locals by `RH4115…Tests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheLocalVariable` and for range variables by the new excluded-branch test.
- Tuple element names inferred from a renamed reference: a SHARED-CASING non-goal (a read by the old name still withholds through SHARED-CASING-009, already pinned for parameters).
- `nameof` of a range variable: not in any changed requirement; the `nameof` collision guard is pinned for parameters (`RH4105…Tests.NoFixIsOfferedWhenTheNewNameWouldChangeTheOperandOfANameof`).
- A local named `__`: no requirement states it.
- Title, message, rule pages, package README, resources of RH4105 and RH4128: pinned by `AnalyzerPackageMetadataTests`, `AnalyzerResourcesTests`, and `CodeFixResourcesTests` (coverage first; no new test).
- Single-letter overlaps (`from X`, `const int K`) are RH4124/RH4125 behavior, unchanged.
- Formatter, CLI, idempotency, FC-010 to FC-014, FC-028, FC-029, FC-038 to FC-043: no formatter phase or unsupported shape is involved.
- FC-033, FC-034: no bug reproduction in this run (behavior change and new rule).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH41"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4128LocalConstantCasingAnalyzerTests"
scripts/test.sh --project architecture
```

On this head (RH4128 class moved aside): naming filter 765 tests, 50 failing — exactly the new tests marked "fails" above, each on the specified difference; 715 pass, every existing test included. Architecture (RH4128 class moved aside): 195 passed, 3 failed — the same three as after the spec author; `EveryRequirementOfAnEnforcedSpecIsCovered` now lists only RH4128-001 to -003, which the RH4128 class covers once it compiles.

## Follow-up candidates

_None._
