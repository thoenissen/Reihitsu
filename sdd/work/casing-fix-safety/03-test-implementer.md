Role: test implementer
Commit: eceb7c1
Verdict: #1597: reproduced; #1598: not reproduced; #1016: reproduced; #1015: reproduced; #1084: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issues, the run's handovers, and the maintainer's #1598 instruction (settle it with a test on real linked documents), which is applied below. Expected values come from the spec examples of commit 5fed711 and the requirements they belong to, not from any patched code. All tests below were run on this head; "red" means the test fails on this head for the issue's own expected-versus-actual difference (the fix is offered and changes the code where the spec withholds it, or Fix All leaves a diagnostic the spec says it corrects). `scripts/test.sh --project analyzer` on this head: 74 failed, 2736 passed; the 74 failures are exactly the red tests below (DataRow rows counted one by one). `scripts/test.sh --project architecture`: 117 passed — `EveryRequirementOfAnEnforcedSpecIsCovered` now finds tagged tests for SHARED-CASING-011 and SHARED-CASING-012.

## thoenissen/Reihitsu#1597

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameWouldChangeTheMethodAForeachCalls` (skeptic row 1, spec example) | SHARED-CASING-011, RH4103-003 | `Verify(src, src)` | red: fix renames `getEnumerator` |
| `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodAForeachCalls` | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed, NumberOfFixAllIterations = 1)` | red |
| `RH4103…NoFixIsOfferedWhenTheRenameWouldChangeTheMethodAQueryClauseCalls` (row 2, spec example) | SHARED-CASING-011, RH4103-003 | `Verify(src, src)` | red |
| `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodAQueryClauseCalls` | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4103…NoFixIsOfferedWhenTheRenameWouldChangeTheMethodADeconstructionCalls` (row 7) | SHARED-CASING-011, RH4103-003 | `Verify(src, src)` | red |
| `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodADeconstructionCalls` | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4103…NoFixIsOfferedWhenTheRenameWouldChangeTheMethodACollectionInitializerCalls` (row 8) | SHARED-CASING-011, RH4103-003 | `Verify(src, src)` | red |
| `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodACollectionInitializerCalls` | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4103…NoFixIsOfferedWhenTheDerivedNameIsANameTheCompilerLooksUp` — 23 rows: every one of the 21 names of the list as a lone method with nothing competing (the over-withholding control), plus `get_enumerator` and `GET_ENUMERATOR` | SHARED-CASING-011, RH4103-003 | `Verify(src, src)` | red, all 23 rows |
| `RH4103…FixIsOfferedWhenTheDerivedNameIsNotANameTheCompilerLooksUp` — rows `getEnumerators`, `moveNext`, `select_all`, `adder` (other side of the list) | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed)` | green |
| `RH4103…FixIsOfferedForAMethodWhoseDerivedNameIsOutsideTheList` (spec example, RH4103 half) | SHARED-CASING-011, RH4103-003 | `Verify(src, fixed)` | green |
| `RH4104LocalFunctionNameCasingAnalyzerTests.FixIsOfferedForALocalFunctionWhoseDerivedNameIsANameTheCompilerLooksUp` (spec example, RH4104 half) | SHARED-CASING-011, RH4104-002 | `Verify(src, fixed)` | green |
| `RH4105MethodParameterCasingAnalyzerTests.NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheOldName` (row 10, spec example) | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red: fix renames `Condition`, `"Condition"` stays |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheNewName` (row 11) | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionConstantNamesTheOldName` (`const string`) | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionNamesAConstructorParameter` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionNamesAPrimaryConstructorParameter` (class) | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionNamesALocalFunctionParameter` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionNamesADelegateParameter` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…NoFixIsOfferedWhenACallerArgumentExpressionOnTheOtherPartOfAPartialMethodNamesTheParameter` (attribute on the defining part; both parts reported) | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | red |
| `RH4105…FixIsOfferedWhenACallerArgumentExpressionNamesTheParameterWithNameof` (spec example) | SHARED-CASING-012, RH4105-003 | `Verify(src, fixed)` | green |
| `RH4105…FixIsOfferedWhenACallerArgumentExpressionNamesAnotherParameter` (other side: the string is neither the old nor the new name) | SHARED-CASING-012, RH4105-003 | `Verify(src, fixed)` | green |
| `RH4105…FixAllSkipsTheParameterNamedByACallerArgumentExpression` | SHARED-CASING-012, RH4105-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheOldName` (spec example) | SHARED-CASING-012, RH4120-003 | `Verify(src, src)` | red |
| `RH4120…NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheNewName` | SHARED-CASING-012, RH4120-003 | `Verify(src, src)` | red |
| `RH4120…FixIsOfferedWhenACallerArgumentExpressionNamesTheParameterWithNameof` | SHARED-CASING-012, RH4120-003 | `Verify(src, fixed)` | green |
| `RH4120…FixAllSkipsTheParameterNamedByACallerArgumentExpression` | SHARED-CASING-012, RH4120-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Row 1 `foreach` / explicit `IEnumerable<int>` + `getEnumerator` | LF | code fix, single and Fix All | reproduced | observed: renamed to `GetEnumerator`; expected: no fix, Fix All leaves it |
| Row 2 query clause + extension `select` in `N` | LF | code fix, single and Fix All | reproduced | as above (`select` → `Select`) |
| Row 7 `deconstruct` next to extension `Deconstruct` | LF | code fix, single and Fix All | reproduced | as above |
| Row 8 `add` next to extension `Add`, collection initializer | LF | code fix, single and Fix All | reproduced | as above |
| Rows 10/11, `const`, ctor, class primary ctor, local function, delegate, partial method (RH4105); record (RH4120) | LF | code fix, single and Fix All | reproduced | observed: parameter renamed, attribute string left as written; expected: no fix |

## thoenissen/Reihitsu#1598

### Tests

All in `RH4109PublicFieldCasingAnalyzerTests` (the issue's `public int key` is RH4109). The helper `ApplyFixInMultiTargetSolutionAsync` builds an `AdhocWorkspace` with one project per target (`Lib(net9.0)` without symbols, `Lib(net8.0)` with `NET8_0`); a file shared by both targets is added to each project with the same `filePath`, so Roslyn treats the copies as linked documents, as the IDE does for a multi-targeted project. It runs RH4109 in the initiating project, registers the fix for the diagnostic in that project's copy of `A.cs`, applies the first action (or Fix All in document scope), and compiles every project before and after.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoFixIsOfferedWhenALinkedDocumentReferencesTheFieldOnlyInTheOtherTargetsBuild` (candidate 1, the issue's own shape, fix started from net9.0) | SHARED-CASING-007, SHARED-FIX-003 | `ApplyFixInMultiTargetSolutionAsync` | green: no action registered |
| `FixStartedFromTheTargetWithTheActiveReferenceBreaksNoTargetsBuild` (candidate 1 started from net8.0) | SHARED-CASING-007, SHARED-FIX-003 | same; `AssertWithheldOrRenamedEverywhere` | green: observed no action registered |
| `FixBreaksNoTargetsBuildWhenAFileOfOnlyTheOtherTargetReferencesTheField` (candidate 2: `B.cs` compiled only in net8.0, active reference) | SHARED-CASING-007, SHARED-FIX-003 | same | green: observed the fix renames both copies of `A.cs` and `B.cs` to `Key`, no compile error in either project |
| `FixBreaksNoBuildWhenAProjectReferencingOnlyTheOtherTargetUsesTheField` (candidate 3: `Consumer` references only net8.0) | SHARED-CASING-007, SHARED-FIX-003 | same | green: observed the fix renames `A.cs` (both copies) and `Consumer/C.cs`, no compile error |
| `FixAllLeavesTheFieldWhenALinkedDocumentReferencesItOnlyInTheOtherTargetsBuild` (candidate 1, Fix All path) | SHARED-CASING-007, SHARED-FIX-003 | same, `fixAll: true` | green: observed a Fix All action that changes no document (treated as withheld) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input, linked `A.cs`/`B.cs`, start in the target without `NET8_0` | LF | code fix | not reproduced | observed: no fix offered; expected: no fix offered, or a rename that breaks no target |
| Same, start in the target with `NET8_0` | LF | code fix | not reproduced | observed: no fix offered |
| Same, Fix All | LF | Fix All (document) | not reproduced | observed: no document changed |
| File only in the other target, active reference | LF | code fix | not reproduced | observed: renamed everywhere, compiles in both targets |
| Project referencing only the other target | LF | code fix | not reproduced | observed: renamed everywhere, compiles in all projects |

Not reproduced in any of the skeptic's three candidates, on the single-fix and the Fix All path. The tests stay as characterization tests; the PR should link #1598 with `Refs`, and closing it is the maintainer's call (escalation 4).

## thoenissen/Reihitsu#1016

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106PrivateFieldCasingAnalyzerTests.NoFixIsOfferedForAnyFieldWhoseDerivedNameTheRuleStillReports` (spec example, all five fields) | SHARED-CASING-008, RH4106-006 | `Verify(src, src)` | red: `__1st` → `_1st` |
| `RH4106…NoFixIsOfferedWhenTheRuleStillReportsTheDerivedName` — rows `__1st`, `_1_value`, `注文`, `注文_明細`, `ϒ_x`, `_ϒ_x`, `ϒx` (digit after the prefix; caseless leading and after `_`; no lowercase mapping leading and after `_`) | SHARED-CASING-008, RH4106-006 | `Verify(src, src)` | red, all 7 rows |
| `RH4106…FixIsOfferedWhenTheRuleAcceptsTheDerivedNameOfAnUncoveredOldName` (spec example `ß_x` → `_ßX`) | SHARED-CASING-008, RH4106-004 | `Verify(src, fixed)` | green |
| `RH4106…FixAllSkipsTheFieldsWhoseDerivedNamesTheRuleStillReports` | SHARED-CASING-008, RH4106-006 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4002ClassNameCasingAnalyzerTests.NoFixIsOfferedWhenTheRuleStillReportsTheDerivedNameOfACaselessName` (spec example, RH4002 half) | SHARED-CASING-008, RH4002-002 | `Verify(src, src)` | red: `_注文` → `注文` |
| `RH4002…NoFixIsOfferedWhenTheRuleStillReportsTheDerivedName` — rows `_注文`, `注文_明細`, `_ßx`, `ß_x`, `_ŉx` | SHARED-CASING-008, RH4002-002 | `Verify(src, src)` | red, all 5 rows |
| `RH4002…FixAllSkipsTheClassWhoseDerivedNameTheRuleStillReports` | SHARED-CASING-008, RH4002-002 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4105MethodParameterCasingAnalyzerTests.NoFixIsOfferedWhenTheRuleStillReportsTheDerivedName` — rows `_ϒx` (spec example, RH4105 half), `_注文`, `ϒ_x` | SHARED-CASING-008, RH4105-003 | `Verify(src, src)` | red, all 3 rows |
| `RH4105…FixIsOfferedWhenTheRuleAcceptsTheDerivedNameOfAnUncoveredOldName` (`_ŉx` → `ŉx`) | SHARED-CASING-008, RH4105-003 | `Verify(src, fixed)` | green |
| `RH4105…FixAllSkipsTheParameterWhoseDerivedNameTheRuleStillReports` | SHARED-CASING-008, RH4105-003 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4005InterfaceNameCasingAnalyzerTests.NoFixIsOfferedForAnyInterfaceWhoseDerivedNameTheRuleStillReports` (RH4005-002 spec example: `_`, `__`, `_2x`, `注文`) | RH4005-002, SHARED-CASING-008 | `Verify(src, src)` | red |
| `RH4005…NoFixIsOfferedWhenNoUsableNameCanBeDerived` — rows `_2x`, `注文`, `ßx` (and the #1084 rows below) | RH4005-002, SHARED-CASING-008 | `Verify(src, src)` | red: `_2x` → `I2x`, `注文` → `I注文`, `ßx` → `Ißx` |
| `RH4121TypeParameterNameCasingAnalyzerTests.FixIsOfferedWhenTheRuleAcceptsTheDerivedNameOfADigitName` (`_1` → `T1`) | SHARED-CASING-008, RH4121-003 | `Verify(src, fixed)` | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `_camelCase` (RH4106): digit, caseless, no lowercase mapping, leading and after `_` | LF | code fix, single and Fix All | reproduced | observed: renamed to a name RH4106 still reports; expected: no fix |
| PascalCase (RH4002): caseless, `ß`, `ŉ` | LF | code fix, single and Fix All | reproduced | as above |
| camelCase (RH4105): caseless, `ϒ` | LF | code fix, single and Fix All | reproduced | as above |
| `I` prefix (RH4005): digit, caseless, `ß` | LF | code fix, single and Fix All | reproduced | observed: `I2x`, `I注文`, `Ißx`; expected: no fix |
| `T` prefix (RH4121) `_1`; `ß_x` (RH4106); `_ŉx` (RH4105) | LF | code fix | holds | controls, green |

## thoenissen/Reihitsu#1015

### Tests

All in `RH4102EventNameCasingAnalyzerTests`. The helper `VerifyFixAllFromEveryDiagnosticAsync` finds every `CodeFixProvider` of the code-fix assembly whose `FixableDiagnosticIds` holds RH4102 (by reflection, so the test does not depend on whether the implementer keeps two providers or merges them), and for every diagnostic of the document, every provider that registers an action for it, and each of the Document, Project, and Solution scopes, runs that provider's Fix All started from that diagnostic once and compares the text exactly; it then checks the result compiles, and, where SHARED-FIX-002 asks for it, that applying the single fixes one after another gives the same text.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixAllRenamesAFieldLikeEventAndAnEventWithAccessorsTogether` (the issue's input) | SHARED-FIX-002, RH4102-003 | `VerifyFixAllFromEveryDiagnosticAsync` | red: started from `dataChanged`, `saved` stays |
| `FixAllRenamesSeveralFieldLikeEventDeclaratorsAndAnEventWithAccessorsTogether` (`event EventHandler opened, closed;` plus a custom event, with subscriptions) | SHARED-FIX-002, RH4102-003 | same | red: started from `saved`, `opened, closed` stay |
| `FixAllSkipsAFieldLikeEventAndAnEventWithAccessorsThatWouldCollide` (`dataChanged` and `DATA_CHANGED` with accessors; `saved` renamed) | SHARED-CASING-010, RH4102-003 | same, without the single-fix comparison (SHARED-CASING-010 says the results differ) | red: started from `dataChanged`, it is renamed |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input, Fix All started from `dataChanged` | LF | Fix All, Document scope | reproduced | observed: `saved` left; expected: both renamed |
| Two declarators + custom event, Fix All from `saved` | LF | Fix All, Document scope | reproduced | observed: `opened, closed` left |
| Mixed collision group | LF | Fix All | reproduced | observed: `dataChanged` renamed; expected: both members of the group skipped |

## thoenissen/Reihitsu#1084

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4005…NoFixIsOfferedForAnyInterfaceWhoseDerivedNameTheRuleStillReports` (shared with #1016) | RH4005-002, SHARED-CASING-008 | `Verify(src, src)` | red: `_` → `I_` |
| `RH4005…NoFixIsOfferedWhenNoUsableNameCanBeDerived` — rows `_`, `__`, `___` | RH4005-002, SHARED-CASING-008 | `Verify(src, src)` | red: `I_`, `I__`, `I___` |
| `RH4005…FixAllSkipsTheInterfacesWithoutAUsableDerivedName` (`_`, `_2x`, `iDocumentWriter`) | RH4005-002, SHARED-CASING-008 | `Verify(src, fixed, 1 Fix All iteration)` | red |
| `RH4005…FixIsOfferedWhenAnUnderscoreIsFollowedByALetter` (`_reader` → `IReader`, other side of the letterless condition) | RH4005-002, SHARED-CASING-008 | `Verify(src, fixed)` | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `interface _`, `__`, `___` | LF | code fix, single and Fix All | reproduced | observed: `I_` (and `II` in the Fix All iteration of the mixed test); expected: no fix |

## Whole run

### Not covered, and why

- **CRLF variants.** No test here involves layout: every fix is a Renamer rename whose output differs from the input only in identifiers, and no requirement in play states anything about line endings. One LF test per scenario.
- **Counterpart surface (formatter).** The formatter does not rename identifiers; none of the five issues has a formatter side.
- **RH4005 `I` → `II` control (skeptic, #1084).** Not added: it is #1451's territory, and a test fixing today's `II` would have to be rewritten there. The other side of the letterless condition is covered by `_reader` → `IReader` instead.
- **Unbuilt list members of SHARED-CASING-011 with a competing binding** (`GetAwaiter`, `GetAsyncEnumerator`, `Dispose`, `DisposeAsync`, `GetPinnableReference`, `AppendLiteral`, `AppendFormatted`, the other query names). Each name of the list is covered by the lone-method DataRow, which reaches the same withholding check as the four binding-change tests; the condition in SHARED-CASING-011 is the derived name alone ("whether or not the rename would change which method a construct calls"), so a competing binding is not an arm of the condition. The four rows the skeptic required (1, 2, 7, 8) are built with their competing bindings.
- **SHARED-CASING-012 on a partial method where the constant names the other part's spelling.** The partial test covers the attribute on the other part with the same spelling; a part spelled differently is a further member the spec author flagged for the implementer and auditor (02-spec-author.md, choice 4) and was not built.
- **FC-004/005/019/020/021 (comments and directives in a gap), FC-010–014, FC-022, FC-025/026, FC-028/029, FC-038–043.** The fixes rename identifiers through the Renamer; they join no lines, move no trivia, and do not format. The #1598 tests are the one place where a directive matters, and they use the issue's `#if`/`#else` shape.
- **FC-018 (Renamer retargeting).** Every positive control renames references (`return _ŉx`, `Value * 2`, subscriptions in the RH4102 tests, `foreach` / query / initializer uses that stay as written) and is checked by the verifier's compile step.
- **FC-033.** Applied: #1598 uses real linked documents (same `filePath` in two projects) instead of a single-project `#if`; Fix All is exercised in every issue; every reproduced family has a Fix All test whose document also holds a second, fixable diagnostic, so the Fix All path cannot pass by having nothing to do.
- **FC-009.** The #1598 Fix All test counts a Fix All action that changes no document as withheld rather than as an offered fix; the single-fix tests assert on the registered actions.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4103|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4104|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4106|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4002|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4005|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4121"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4102"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4109"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"
```
