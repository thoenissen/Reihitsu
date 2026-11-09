Role: test implementer
Commit: d3e461d
Verdict: #1666: tests written (reproduced); #1667: tests written (reproduced); #1665: tests written (reproduced)

The prompt carried no intended fix or hoped-for verdict. Expected values come from the spec examples and requirements of the spec diff `95aa3e6..d3e461d` and the maintainer's batch-1 answers (1A, 2A, 3A in `00-lead.md`). No production code was touched. All "Expected now" values were observed on `d3e461d` with only these test files changed. "red" means the test fails for the issue's own difference: the analyzer reports the diagnostic as marked, and the fix is offered where the spec withholds it. The messages are "No code fix may be offered …" or "Expected collection to be empty", and for Fix All and the two-document test, a diff that shows the move. "green" means the test passes and must stay green.

Every `[DataRow]` test runs on LF and on CRLF (`"\n"`, `"\r\n"`).

## thoenissen/Reihitsu#1666

File: `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyNoCodeFixIsOfferedForAConditionalExternInFrontOfTheFirstModifier (r1, spec example) | RH2001-010 | VerifyNoCodeFixIsOffered | red (fix offered) |
| VerifyNoCodeFixIsOfferedForAConditionalReadonlyInFrontOfTheFirstModifierInAStruct (r2, spec example) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAConditionalStaticInFrontOfTheFirstModifier (r7) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternBehindAWholeMember (spec example 2) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheMemberOfTheActiveElseBranch (r6, spec example 3) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternWhenTheSymbolIsDefined (r8, `#define DEBUG`) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAConditionalAttributeListInFrontOfTheFirstModifier (r5, output question 1, answer A) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheFirstModifierHoldingTextThatIsNotCode | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier (no nesting: a group inside the branch) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInsideAnotherGroupInFrontOfTheFirstModifier (no nesting: the group lies inside the active branch of another one) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyFixKeepsAGroupOfAWholeMemberInFrontOfTheFirstModifier (r3, spec example 5) | RH2001-004, RH2001-010 | Verify | green |
| VerifyFixKeepsAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember (counterpart of r6) | RH2001-004, RH2001-010 | Verify | green |
| VerifyFixKeepsAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment (a branch of no members) | RH2001-004, RH2001-010 | Verify | green |
| VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier (Fix All: the refused property stays, the whole-member one and a plain one are fixed) | RH2001-010, SHARED-FIX-002 | Verify, MarkupMode.Allow | red (Fix All converts `First`) |

`scripts/test.sh --project architecture` now passes 117/117: `EveryRequirementOfAnEnforcedSpecIsCovered` finds RH2001-010.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| r1 `extern` in `#if DEBUG` above `private int Value { get; }` (class) | LF, CRLF | code fix | reproduced | fix offered; expected none |
| r2 `readonly` in a struct | LF, CRLF | code fix | reproduced | fix offered; expected none |
| r1 plus whole-member and plain properties | LF | Fix All | reproduced | `First` becomes `_first`; expected `First` to stay reported |

## thoenissen/Reihitsu#1667

Files: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`, `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs`

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass (m01, spec example) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAnActiveConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass (spec: "when `NET` is defined") | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | green (the symbol already shows the layout) |
| RH7109 NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialStruct (m02) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfAnotherDeclarationOfAPartialClass (m09 `[System.Serializable]`, output question 2, answer A) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfAnotherDeclarationOfAPartialClass (another declaration *behind* the reordered one; a directive behind its first token) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 FixMovesAFieldOfAPartialClassWithoutAConditionalDirectiveInAnyDeclaration (spec counterpart) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAStaticFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalGroup | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7103 FixMovesAStaticFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute (static across instance) | SHARED-ORDER-008, RH7103-007 | Verify | green |
| RH7109 FixMovesAFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalGroupInItsBody (spec: the body of another declaration is not read) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldOfATypeWhoseEnclosingTypeHoldsAConditionalGroup (spec: an enclosing type does not count) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfADeclarationOfAPartialClassInAnotherDocument (A3, two documents) | SHARED-ORDER-008, RH7109-006 | Verify with a second source; fixed equals test | red (fix applied) |
| RH7109 FixMovesAFieldOfAPartialClassWhoseDeclarationInAnotherDocumentHoldsNoConditionalDirective (A3 counterpart; the other part has an unconditional `[System.Serializable]`) | SHARED-ORDER-008, RH7109-006 | Verify with a second source | green |
| RH7109 NoCodeFixWhenAnExcludedBranchInFrontOfThePartialClassDeclaresAnotherDeclarationOfItWithAnAttributeList (A5 / m06, spec example) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 FixMovesAFieldWhenAnExcludedDeclarationOfThePartialClassInFrontOfItHasNoAttributeList (A5 counterpart: no attribute list) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldWhenAnExcludedPartialDeclarationOfAnotherNameWithAnAttributeListStandsInFrontOfIt (A5 counterpart: name) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldWhenAnExcludedPartialDeclarationOfTheNameWithAnotherArityStandsInFrontOfIt (A5 counterpart: type parameter count) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldWhenAnExcludedPartialDeclarationOfTheNameIsNestedInAnotherType (A5 counterpart: outermost level of the branch) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 NoCodeFixWhenAnExcludedBranchOfTheGroupAroundTheFirstTokenOfAnotherDeclarationOfAPartialClassHoldsAnAttributeList (A4) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7309 NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass (m01 shape for the region reorder) | RH7309-006, SHARED-ORDER-008 | VerifyNoCodeFixIsOffered | red |
| RH7109 FixAllSkipsTheFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute (Fix All over a refused partial class and a plain class) | SHARED-ORDER-008, SHARED-FIX-002 | Verify, MarkupMode.Allow | red (Fix All moves `C._c`) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| m01 (issue input), same document | LF, CRLF | code fix | reproduced | fix offered; expected none |
| m01, other declaration in another document | LF, CRLF | code fix | reproduced | `_c` moved; expected unchanged |
| m01, RH7309 region reorder | LF, CRLF | code fix (counterpart consumer) | reproduced | fix offered; expected none |
| m01 plus a plain class | LF | Fix All | reproduced | the partial class is reordered; expected only `D` |

## thoenissen/Reihitsu#1665

Files: RH7109, RH7103, RH7102, RH7309 test classes as above.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 NoCodeFixWhenAGroupInFrontOfTheSpansClosesTheTypeAndOpensAStruct (m04, spec example) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAGroupInTheLeadingTriviaOfTheTargetClosesTheTypeAndOpensAStruct (m13, the other side of the crossed-span boundary, FC-003) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | green |
| RH7103 FixMovesAStaticFieldBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct (s1, spec example) | SHARED-ORDER-008, RH7103-007 | Verify | green |
| RH7102 FixMovesAConstantBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct (Decision #7: RH7102 keeps its fix) | SHARED-ORDER-008, RH7102-006 | Verify | green |
| RH7309 NoCodeFixWhenAGroupInFrontOfTheMovedBlocksClosesTheTypeAndOpensAStruct (m04 for the region reorder: the group stands between an uncrossed `Constants` block and the crossed `IDisposable` block) | RH7309-006, SHARED-ORDER-008 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAGroupInFrontOfTheSpansHoldsAModifierOfAMemberInFrontOfThem (m08, spec example, output question 3, answer A) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAGroupAroundTheSpansHasAnExcludedBranchThatIsNoWholeMember (`#if` in front, `#else`/`#endif` behind; the spans lie in the active branch) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 FixMovesAFieldWhenAGroupAroundTheSpansHasAnExcludedBranchOfAWholeMember (counterpart) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldBehindAGroupOfWholeMembersInFrontOfTheSpans (m11, spec example) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldBehindAMethodWithAGroupOfWholeStatementsInFrontOfTheSpans (spec sentence) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldBehindAGroupOfWholeAttributeListsInFrontOfTheSpans | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldBehindANestedTypeWithAGroupOfWholeMembersInFrontOfTheSpans (the *Place* bullet's new "or in front of them") | SHARED-ORDER-008, RH7109-006 | Verify | green |
| RH7109 FixMovesAFieldInFrontOfAGroupBehindTheSpansThatHoldsAModifier (a directive behind the spans is not read) | SHARED-ORDER-008, RH7109-006 | Verify | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| m04 (issue input) | LF, CRLF | RH7109 code fix | reproduced | fix offered; expected none |
| m04 shape before the moved region blocks | LF, CRLF | RH7309 code fix (counterpart consumer) | reproduced | fix offered; expected none |
| s1 static move; const move | LF, CRLF | RH7103, RH7102 code fix | fix kept, as specified | — |

## Whole run

### Decision #3 (adjacent fix in the first-token reading)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 NoCodeFixWhenAnExcludedBranchInFrontOfAnotherTypeOfTheGroupAroundTheFirstTokenHoldsAnAttributeList (m05) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 NoCodeFixWhenAnExcludedBranchInFrontOfAnExcludedElifBranchOfTheGroupAroundTheFirstTokenHoldsAnAttributeList (n3) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| RH7109 FixMovesAFieldOfATypeBehindANestedGroupOfAWholeTypeInFrontOfIt (guard, see below) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| The A4 test under #1667 also covers Decision #3 on another declaration of a partial type. | | | |

**FixMovesAFieldOfATypeBehindANestedGroupOfAWholeTypeInFrontOfIt.** The input is `#if X #if Y public class D { } #endif #endif` in front of `public class C`. The fix is offered on `main`. SHARED-ORDER-008's first-token item names only "an excluded branch that does not read as whole members" and has no nesting clause. RH2001-010 does have one. Decision #6 asks for one shared Core helper. If that helper applies RH2001-010's no-nesting clause to the first-token reading, this test turns red. That would be a withheld fix that neither the change doc nor an output question lists. The test locks in the spec as written.

### Not covered, and why

- **r4** (the excluded branch holds the property itself, so the rename check refuses). Existing tests cover it: `VerifyNoCodeFixIsOffered…` at the `#if NEVER` examples (RH2001-004 coverage, lines about 1066–1200 of the RH2001 test file). They stay green.
- **A property without a modifier**: RH2001-009 covers it, with existing tests such as `VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers`.
- **A6** (m07, m10, other-document excluded declarations): a decided gap (Decision #8). The output stays as on `main`, so no test is written. The run's change could still move it, and a test would lock in a known safety-rail gap.
- **An excluded `[StructLayout] public class C` without `partial` in front of `public partial class C`**: a compile error in the build that includes it. The `partial` condition is covered on its other side by the m06 test only. A rare shape.
- **Declaring a shape unsupported (FC-038 to FC-043)**: not part of this run (change doc, *Alternatives*). No formatter phase is touched.
- **FC-004, FC-019, FC-021, FC-022, FC-023, FC-025, FC-026, FC-029**: the run only adds refusals and changes no rewrite. The kept fixes are covered by the counterpart tests, which compare exact output.
- **FC-007 (convergence)**: `Verify` re-runs the analyzer on the fixed source for every fix-kept test. **FC-008 (Fix All)**: covered for RH2001 and RH7109. RH7309 and RH7103 run through the same `ChangesMemberOrder` guard per diagnostic. The shared call sites are `rg -n "ChangesMemberOrder" src/Reihitsu.Analyzer.CodeFixes`: `TypeMemberOrderingCodeFixProviderBase.cs:80` (RH7102, RH7103, RH7109) and `RegionBlockLayout.cs:71` (RH7309). A Fix All test per consumer was not added.
- **FC-010 to FC-014, FC-028, FC-030**: formatter-only classes. The formatter is not involved (02-skeptic.md *Surfaces*).
- **FC-018, FC-036**: the RH2001 rename path is unchanged. Its tests exist.
- **FC-024 (partial)**: covered by the #1667 tests, two documents included.
- **FC-033**: LF and CRLF are run for every scenario. The code-fix path and the counterpart consumers (RH7309 for #1665 and #1667; RH7103/RH7102 kept) are covered.

### Architecture findings

None new. The guard test above concerns how Decision #6's shared helper is scoped. It is not a structural finding.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
scripts/test.sh --project architecture
```

Results on `d3e461d` plus these tests:
- RH2001: 23 failed / 127 passed. Every failure is a red test listed above.
- Ordering and RH7309 set: 29 failed / 509 passed. Every failure is a red test listed above.
- Architecture: 117/117 passed.
