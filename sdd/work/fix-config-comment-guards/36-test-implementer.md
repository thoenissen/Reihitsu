Role: test implementer
Commit: fd7c51a4 (tests written against the production code of ea562016, the decision-27 state; nothing under `src/` other than the five test classes below changes in this commit)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: bring the member-move tests in line with SHARED-FIX-009, SHARED-ORDER-006/007/008 as they stand after batches 3–6 (decisions 32–35), remove the tests that only exercise the per-configuration reading or the configuration limit, cover every spec example and the change doc's withholdings, and keep the #1595, #1616, and decision-16 tests unless the spec changed their outcome.

Inputs: `00-lead.md` (batches 3–6, decisions 32–35), `33-spec-author.md`, `34-skeptic.md`, `35-spec-author.md`, `sdd/changes/2026-11/member-move-conditional-groups.md` (Accepted), and SHARED-FIX-009 / SHARED-ORDER-006/007/008, RH7309-005, RH2001-009 at `fd7c51a4`.

How the expectations were set (FC-035):

- Every withholding comes from a SHARED-ORDER-008 or SHARED-FIX-009 sentence or example, or from SHARED-ORDER-007's count. The fix is refused whenever the moved or crossed span holds a conditional directive of a group that does not wrap whole units.
- Every exact output is either the spec's own output (SHARED-ORDER-008: whole members in moved trivia, attribute lists, accessors, switch section) or main's output where the spec says "as on main" or "keeps main's fix and output" (change doc *Unchanged*). Those were measured with `scripts/apply-fix.sh` on a temporary worktree at `57acf0d`, LF and CRLF alike.
- Cross-check: all five test classes were copied into the `57acf0d` worktree and run against main's production code. Every new or converted test that expects a fix passes on main. Every test that expects no fix for a change-doc withholding fails on main, which offers the fix there. The worktree and fixtures were removed afterwards.

`scripts/format.sh` was run on all five files (it reflowed one RH7309 summary). The architecture tests pass, 115 of 115. `EveryRequirementOfAnEnforcedSpecIsCovered` now passes, because SHARED-FIX-009 and SHARED-ORDER-008 have tagged tests.

## thoenissen/Reihitsu#1595

No outcome changed. The RH7102 tests whose excluded code is whole members in front of the constant follow SHARED-ORDER-007's count, which batch 3 keeps unchanged. They keep their tags `RH7102-006` and `SHARED-ORDER-007`. Two tests now refuse under SHARED-ORDER-008 as well, so their tags and summaries changed:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7102…NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations` (retagged; `J`'s modifiers split) | RH7102-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7102…NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (retagged; `_b`'s modifiers split) | RH7102-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7102…NoCodeFixWhenAnInitializedFieldOfAnIfFalseBlockInFrontOfTheConstantWouldCrossAnInitializedField` (unchanged) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | **red on head**: one action is registered. The test was already red at `fd7c51a4` before this commit, because head's per-assignment parse gives `#if false` no build. Under SHARED-ORDER-007/SHARED-FIX-009, every excluded whole member counts as present, `#if false` included, so no fix |

Decision 16 (`ConstantWhoseModifiersAnIfSplits…IsNotReported`, two tests) and the RH7102 Fix All test are unchanged and green.

## thoenissen/Reihitsu#1593

Files:

- `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs`

Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair, and both rows behave alike.

### Removed tests, and why

The removed tests are of two kinds.

**Fix-side counterparts of a per-configuration condition (decision 32).** Each one expected a fix that the per-configuration reading offered for a member split by `#if`. Under SHARED-ORDER-008 the same input refuses. Its NoCodeFix partner, which is kept, already covers the same directive place.

RH7109:
- `FixMovesAFieldAcrossAFieldThatNoBranchOpeningBehindThePreviousDeclaratorMakesStatic`
- `…NoElseBranchOpeningBehind…`
- `…NoBranchSpanningThePreviousDeclarator…`
- `FixMovesAnInstanceFieldAcrossAFieldThatEveryBranchSpanningThePreviousDeclaratorMakesStatic`
- `…IsStaticBehindAGroupThatOpensBehindThePreviousDeclarator`
- `…AStaticFieldWithAnExcludedGroupInFrontOfItsFirstToken` (excluded `volatile`)
- `…StaticInEveryBranchWithAReadonlyModifierOnlyInOne`
- `…StaticInEveryBranchWithADifferentAccessibility`
- `…ADeclarationThatIsAConstantInEveryBranch`
- `…StaticInEveryBranchBehindItsFirstModifier`
- `…StaticInEveryBranchOfAnElifGroup`
- `FixMovesAFieldThatIsStaticInEveryBranchAcrossAnInstanceInitializedField`
- `FixIsOfferedWhenAnExcludedInitializerOfACrossedFieldIsAStaticLambda`
- `FixIsOfferedWhenAnExcludedArgumentOfACrossedFieldsInitializerIsAStaticLambda`
- `FixIsOfferedWhenACrossedDeclarationIsSplitIntoAStaticFieldAndAnInstanceInitializedField`
- `FixIsOfferedAcrossAPropertyWhoseExcludedBranchEndsTheExpressionBodyWithoutDeclaringAMember` (the expression-body place is kept by the converted test below)
- tc2 `NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneAroundTheMovedField` (tc1 has the same shape and place and is the spec example)
- nt3 `FixMovesAFieldAcrossANestedTypeThatExcludedBranchesOpenAndCloseInFrontOfIt`
- nt5 `FixIsOfferedAcrossANestedTypeThatExcludedBranchesOpenAroundAMemberInFrontOfAnotherMember`
- sg1, sg1i, cr1 (`FixIsOfferedWhenGroupsWith…` ×2, `FixIsOfferedWhenTheMovedAndACrossedFieldAreStaticInBuildsThatExcludeEachOther`)
- `NoCodeFixWhenGroupsWithNegatedConditionsSplitACrossedDeclarationIntoAnInstanceInitializedField`: it differed from sg1t only in its condition, which the guard no longer reads (SHARED-FIX-009)

RH7309:
- `CodeFixMovesABlockOfAStructAcrossAnInstanceFieldWhenEveryBranchDeclaresItsFieldStatic`

**Configuration-limit tests only.** These are `FixMovesAFieldAcrossEightConditionalGroupsInFrontOfACrossedField`, `FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove`, and cap2 `NoCodeFixWhenSeparatedConditionalGroupsInTheCrossedSpanTogetherExceedTheConfigurationLimit`. SHARED-ORDER-008 states "has no limit on the number of configurations", and that sentence is covered once by the converted nine-group tests below.

### Tests: changed outcome (assertion changed, each traced)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7109…NoCodeFixWhenAnIfSplitsTheModifiersOfACrossedDeclaration` (was `FixIsOfferedWhenACrossedDeclarationIsAStaticReadonlyFieldInOneBranchAndAConstantInTheOther`, decision 22; now the SHARED-ORDER-008 `K` example verbatim, change doc) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…NoCodeFixWhenAGroupStandsInTheExpressionBodyOfACrossedProperty` (was `FixMovesAFieldAcrossAPropertyWhoseExpressionBodyHoldsAnExcludedArgument`; SHARED-ORDER-008 first refusal bullet "its expression body", change doc's closing sentence) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…FixMovesAFieldAcrossAFieldWithNineConditionalAttributeLists` (was `NoCodeFixWhenNineConditionalAttributeLists…ExceedTheConfigurationLimit`, decision 31; batch 5 2A and "no limit") | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | **red**: no action |
| `RH7109…FixMovesAFieldAcrossNineGroupsOfWholeMembers` (was `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit`; batch 3 "groups of whole members keep main's reading", "no limit") | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | **red**: no action |
| `RH7309…NoCodeFixWhenAnIfSplitsAMemberInsideTheMovedBlock` (was `CodeFixMovesABlockWithAMemberSplitAcrossConfigurationsWhenNothingInitializedIsCrossed`; now the #1593 issue input, the SHARED-ORDER-008 example and change doc verbatim) | SHARED-ORDER-008, RH7309-005 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…NoCodeFixWhenAGroupStandsInTheCollectionInitializerOfACrossedField` (was the nine-entry limit test; reduced to one group, outcome unchanged, now refused by place) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action (head's limit no longer applies with one group) |

### Tests: new, one per spec example or change-doc case

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7109…NoCodeFixWhenAnIfCompletesTheInitializerOfACrossedField` (SHARED-FIX-009 example, change doc) | SHARED-FIX-009, SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…FixMovesTheFieldOfTheSharedFixExampleWithoutTheConditionalGroup` (its "without the group" side) | SHARED-FIX-009, RH7109-005 | `Verify(source, fixed)` | green |
| `RH7109…NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldHoldsAModifier` (excluded `static`, spec and change doc) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…NoCodeFixWhenAConditionalGroupIsNestedInTheMovedFieldsLeadingTrivia` (`FEATURE`/`DEBUG`, spec and change doc) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…NoCodeFixWhenAGroupOfStatementsStandsInALambdaOfACrossedFieldInitializer` (spec, change doc, batch 4 note) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldHoldsTextThatIsNotCode` (`#if false` prose, change doc) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7109…FixMovesAFieldAcrossACrossedNestedTypeWithAGroupOfWholeMembers` (spec example, batch 5 1A) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output (a blank line follows `_c`; this is main's whole-type formatting, the follow-up in `00-lead.md`) | green |
| `RH7109…FixMovesAFieldWithAGroupOfWholeAttributeListsInEveryBranch` (spec example, batch 5 2A, spec output) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)` | green |
| `RH7109…FixMovesAFieldAcrossAFieldWithAGroupOfWholeAttributeLists` (spec: "the same holds … in front of a crossed member"; skeptic's `attr_crossed`) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | green |
| `RH7109…NoCodeFixWhenWholeMembersInBranchesThatExcludeEachOtherHoldACrossedInitializer` (new SHARED-ORDER-007 `_c`/`_e` example) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| `RH7109…FixMovesAFieldAcrossAGroupWhoseExcludedBranchHoldsOnlyAComment` (comment-only branch = no units; change doc *Branches that hold only comments*) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | green |
| `RH7109…FixMovesAFieldWithAGroupWhoseActiveElseBranchHoldsOnlyAComment` (`#else // nothing in release`, 34-skeptic) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | green |
| `RH7109…FixMovesAFieldWithAGroupWhoseExcludedBranchHoldsARegionAroundWholeMembers` (`#region` in a branch; change doc) | SHARED-ORDER-008, RH7109-006 | `Verify(source, fixed)`, main's output | green |
| `RH7103…FixMovesAMemberWithAGroupOfWholeMembersInItsLeadingTrivia` (`Run`/`Trace`/`Create`, spec output) | RH7103-007, SHARED-ORDER-008 | `Verify(source, fixed)` | green |
| `RH7103…FixMovesAMemberAcrossAPropertyWithAGroupOfWholeAccessors` (spec output, batch 5 3A) | RH7103-007, SHARED-ORDER-008 | `Verify(source, fixed)` | green |
| `RH7103…FixMovesAMemberAcrossAMethodWithAGroupOfWholeStatementsInASwitchSection` (spec output, batch 4, decision 33) | RH7103-007, SHARED-ORDER-008 | `Verify(source, fixed)` | green |
| `RH7103…NoCodeFixWhenAConditionalArgumentStandsInAStatementOfACrossedMethod` (spec and change doc) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7103…NoCodeFixWhenAGroupInACrossedAccessorBodyUsesTheFieldKeyword` (spec `field` example for RH7103, change doc) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **red**: one action |
| `RH7103…FixMovesAMemberAcrossAnAccessorWhoseGroupOfWholeStatementsDoesNotUseTheFieldKeyword` (its `return 1;` side) | RH7103-007, SHARED-ORDER-008 | `Verify(source, fixed)`, main's output | green |
| `RH7309…NoCodeFixWhenAGroupInAnAccessorBodyOfAStructUsesTheFieldKeyword` (spec `field` example for RH7309; a SHARED-FIX-003 bug fix, not in the change doc) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7309…NoCodeFixWhenAConditionalAccessorWithoutABodyGivesAPropertyOfAStructABackingField` (SHARED-ORDER-007 example verbatim) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | green |
| `RH7309…CodeFixMovesABlockOfAStructAcrossAPropertyWhoseConditionalAccessorHasABody` (its `get => 0; #if DEBUG set { } #endif` side) | RH7309-006, SHARED-ORDER-007 | `Verify(source, fixed)`, main's output | green |

Spec examples covered by kept tests instead of new ones:

- tc1 is the type-closing example: `NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneDeclaringAStaticInitializedField`.
- nt2 is the "opens a nested type" example: `NoCodeFixWhenAnExcludedBranchOpensANestedTypeAroundTheMovedField`.
- Both carry SHARED-ORDER-008 now.
- The first SHARED-ORDER-006 example and the second RH7309-005 example stay as they were on main.

### Tests: kept, outcome unchanged, retagged and re-summarized

The tags, and the summaries that described a per-build reading, were changed (FC-031). Names stay, except for the two renames noted below.

- Tagged `SHARED-ORDER-008` plus the rule's withholding requirement (`RH7109-006`, `RH7103-007`, or `RH7309-005`), replacing `SHARED-ORDER-007` where 007 was the stated reason.
- RH7109, refused by place: the initializer split, moved or crossed (2 tests); the property initializer behind an accessor list; modifiers in front of or behind the first token, moved or crossed (5); a branch that opens in the previous initializer, behind the previous declarator (if, else, elif), or spans the previous declarator or initializer together with the modifiers (7); the same shapes for an instance field (2); split modifiers with `const`/`readonly` (2); an inner group between modifiers; `NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantInTheActiveBranch`; two groups that close the active bracket; a nested group of whole members; branches that close a method block, an expression body (2), or a nested type; a split crossed declaration; nt4; sg1t, renamed `NoCodeFixWhenTwoGroupsSplitTheModifiersAndTheInitializerOfACrossedDeclaration`; and cr1's counterpart, renamed `NoCodeFixWhenGroupsBehindTheFirstModifierSplitTheMovedAndACrossedField`.
- RH7109, kept: three excluded-attribute tests (whole attribute lists); `FixMovesAFieldAcrossAMethodWhoseBlockHoldsAnExcludedStatement` (whole statements); `FixMovesAFieldAcrossANestedTypeWhoseBodyHoldsAnExcludedStaticInitializedField` (batch 5 1A); `FixMovesAFieldWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheType` ("a conditional directive outside both spans neither refuses the move nor is read").
- RH7109, tagged `SHARED-ORDER-007`, `RH7109-006`: the four j1s/j2s tests (two whole members of one name in branches that exclude each other, both counted) and the counterpart of the group behind the closing brace.
- RH7103: `NoCodeFixWhenTheMovedStaticFieldHasAnInitializerOnlyInAnotherConfiguration` (SHARED-DIAG-008 kept, plus 008); two refusals by place; the excluded-attribute method and two excluded-accessor properties (whole attribute lists or accessors without `field`).
- RH7309: seven refusals by place, now `RH7309-005` and `SHARED-ORDER-008`; acc1 and its all-bodied counterpart now cite SHARED-ORDER-007's bodiless-accessor count (`RH7309-006`, `SHARED-ORDER-007`).

All of these are green on head.

### Result on head

`scripts/test.sh --project analyzer` over the five classes: 28 rows fail (14 tests), 481 pass. The red tests are the 13 #1593 tests marked **red** above, plus the RH7102 `#if false` test under #1595. Each fails on its own expected-versus-actual difference: a NoCodeFix test fails with "Expected collection to be empty", meaning one action is registered; a fix test fails on the iterative code fix, meaning no move was made. None fails on a compile error or a diagnostic mismatch.

## thoenissen/Reihitsu#1616

All RH2001 tests are unchanged. SHARED-FIX-009's last paragraph exempts RH2001-009's fix, so it changes no RH2001 outcome. One test was added for that scope sentence:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001…VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` (skeptic's `if_static`) | SHARED-FIX-009 | `Verify` + `GetCodeFixActionsAsync`, `Assert.HasCount(1, actions)` | green (main: green) |

Only "offered" is asserted, not the output. Main writes `readonly` in front of the `#if` group, and head writes it behind the group. RH2001-009 says that "A directive at that place is not covered by this requirement". So an exact output would assert an unspecified placement.

## Not covered, and why

- **FC-003 (both sides).** Every SHARED-ORDER-008 unit list has a kept side and a refused side: members (whole members / prose, `static`, nested), attribute lists (kept / `static` text), accessors (without `field` / with `field` or bodiless in a layout type), and statements (method body and switch section / lambda in an initializer, conditional argument, `field` in an accessor body). Each place a directive can stand inside a declaration (modifiers, initializer, expression body, collection initializer, argument list) has a refused test.
- **Parameter list, constructor initializer, base list.** The change doc names these places but gives no example. The guard's "anything not classified refuses" rule covers them in one code path with the expression-body and argument cases. No test was added for each; the spec checker can ask for one.
- **FC-007, FC-008.** The existing RH7102 Fix All test still covers a withheld member next to a fixed one. No new Fix All test was added: no new withholding can co-occur with a fix in one document in a way the existing test does not already show.
- **FC-010, FC-012, FC-028, FC-030** (formatter phases): no formatter code is in the change.
- **FC-019, FC-020, FC-021** (comment or directive in the gap): directives are the subject of every test here. Comment-only branches and a `#region` in a branch are covered.
- **FC-033.** Every test runs under LF and CRLF.
- The other failure classes (literals, identifiers, renames, expression-bodied kinds) do not reach a guard that decides only from directive places.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"
scripts/test.sh --project architecture
```
