Role: test implementer
Commit: a37fab68 (base; this handover's commit adds only the test changes below)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: decisions 46–48 and audit 6's #17–#21. Expected values come from those decisions, from `main`'s output (checked with `scripts/apply-fix.sh` on a temporary worktree of `origin/main` 3578f98e, LF and CRLF, removed afterwards), and from the spec author's working-tree edits to SHARED-ORDER-007/-008 and RH2001-009. I read those edits before committing: the partial-property example of SHARED-ORDER-007 is the input of my `…DefiningDeclaration…` test, and RH2001-009 now withholds the fix for every `#if`/`#elif`/`#else`/`#endif` behind the first modifier or in front of the type. Every tag below matches that text.

Every new or changed test has an LF and a CRLF row, and both rows behave alike. `scripts/format.sh` changed nothing in the four files. The architecture tests pass (115 of 115).

Result on the head (a37fab68 production code): the RH7102, RH7103, RH7109, RH7309 and RH2001 classes give **18 failing rows out of 575**. These are the 9 tests marked **red** below.
- The `VerifyNoCodeFixIsOffered` tests fail with "Expected collection to be empty … No code fix may be offered", which means the head registers the fix.
- The `Verify` tests fail on "Iterative code fix application … did not match", and the actual text is the unchanged input, which means the head withholds the fix.

None fails on a compile error or a diagnostic mismatch.

## thoenissen/Reihitsu#1595

No test added or changed. Decisions 46 and 47 reach RH7102 through the shared `IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder` path (`rg -n "ChangesMemberOrder\(" src -g '!*Test*'`), which the RH7103 and RH7109 tests below cover.

## thoenissen/Reihitsu#1593

### Decision 46 (#17): partial-property shapes in a partial struct, RH7309

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7309…NoCodeFixWhenTheImplementingDeclarationOfAPartialPropertyOfAStructHasAnAccessorWithoutABodyAndUsesTheFieldKeyword` (#17 member 7: `get => field; set;` in the region, defining declaration in the other part) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | **red**: the head fixes it (partial exemption in `HasBackingField`) |
| `RH7309…NoCodeFixWhenTheImplementingDeclarationOfAPartialPropertyOfAStructHasAnAccessorWithoutABody` (#17 member 8: `get => 0; set;`) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | **red**, same reason |
| `RH7309…NoCodeFixWhenTheDefiningDeclarationOfAPartialPropertyOfAStructStandsInTheCrossedBlock` (decision 46, "no `partial` exception": defining `{ get; set; }` in the region, implementing `get => 0; set { }` in the other part; the new SHARED-ORDER-007 example) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | **red**. The head and `main` fix it. Decision 46 withholds it on purpose (over-counting) |
| `RH7309…CodeFixMovesABlockOfAStructAcrossTheImplementingDeclarationOfAPartialPropertyWhoseAccessorsAllHaveABody` (other side of the condition: implementing `get => 0; set { }` in the region) | RH7309-006, SHARED-ORDER-007 | `Verify` (`main`'s output) | green |

### Decision 46 (#20): active half of *No backing field from a branch*, RH7103 class

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7103…NoCodeFixWhenAnActiveGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody` (the auditor's input: `get => 0; #if !DEBUG set; #endif` in a class, crossed by a static member) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green. In a class no layout count masks the clause, so this test kills the auditor's mutant (`branchAccessors` without the active accessors). The auditor showed that the mutant fixes this input. I did not run the mutant again |

### Decision 47 (#18): a conditional header only makes the type count as layout-sensitive

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7103…FixMovesAStaticMethodOfAClassWithAConditionalAttributeList` (`#if DEBUG [System.Serializable] #endif`, static `Create` moved in front of `Run`; the input of the former SHARED-ORDER-008 RH7103 example) | RH7103-007, SHARED-ORDER-008 | `Verify` (`main`'s output) | **red**: the head withholds every move |
| `RH7103…FixMovesAStaticMethodOfAClassWithAConditionalEntryInItsBaseList` (`: System.IDisposable #if NET , System.IAsyncDisposable #endif`) | RH7103-007, SHARED-ORDER-008 | `Verify` (`main`'s output) | **red**, same reason |
| `RH7109…FixMovesAStaticFieldOfAClassWhoseAttributeListsHoldAConditionalGroup` (`#if X [System.Obsolete] #endif`, `private static int _a; private static readonly int _c;`) | SHARED-ORDER-008, RH7109-006 | `Verify` (`main`'s output) | **red**, same reason |
| `RH7109…FixMovesAStaticFieldOfAClassWhoseBaseListHoldsAConditionalGroup` (conditional `: System.IDisposable`, static fields) | SHARED-ORDER-008, RH7109-006 | `Verify` (`main`'s output) | **red**, same reason |

Re-pointed, outcome unchanged. Only the summary changed: "refuses every member move of the type" became "makes the type count as one whose field order sets its layout; moving instance field `_c` in front of instance field `_a` would reorder it". The tags stayed.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7109…NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfTheType` | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| `RH7109…NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfTheType` | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| `RH7109…NoCodeFixWhenAConditionalGroupDecidesTheKindOfTheType` | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| `RH7109…NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfTheType` | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| `RH7309…NoCodeFixWhenAConditionalDirectiveStandsInTheAttributeListsOfTheType` | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |

### For the lead: the auditor's #18 claim about the two RH7109 tests does not hold

Audit 6 #18 says the RH7109 `[System.Obsolete]` and base-list tests "become `Verify` with `main`'s output" under the narrowed check. They do not. Both move the *instance* field `_c` in front of the *instance* field `_a`. Under decision 47's form, `(fieldOrderSetsLayout || header directive) && ChangesOrder(IsLayoutField)`, that move is still refused. So their outcome stays "no fix", and decision 47's spec text agrees with that: the spec author's new RH7109 `[System.Serializable]` example with instance fields is "no code fix is offered".

The plain-class fixes that decision 47 restores are moves that reorder no instance field. The four new `Fix…` tests above pin them. They match the two auditor inputs on RH7103, plus static-field counterparts on RH7109. With these, each side of the new condition has a test:
- header directive and instance-field reorder → refused;
- header directive and no instance-field reorder → fixed;
- no header directive and instance-field reorder in a class → fixed, by the existing `FixMovesAFieldOfATypeWithAnAttributeListOutsideAnyGroup`.

## thoenissen/Reihitsu#1616

Decision 48: every `#if`/`#elif`/`#else`/`#endif` behind the first modifier or in front of the type withholds the fix, whatever its branches hold.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` (renamed from `VerifyFixIsOfferedFor…`; decision 36's kept shape `private #if DEBUG static #endif int`, withdrawn by decision 48; `Verify` → `VerifyNoCodeFixIsOffered`; `SHARED-FIX-009` tag dropped, since the amendment it pinned is reverted) | RH2001-009 | `VerifyNoCodeFixIsOffered` | **red**: the head keeps the fix (`static` is in the keyword set). This test pins the type arm: with `DEBUG` undefined, every directive lies in the type's leading trivia |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalGroupWithADirectiveBetweenTwoModifiers` (new: `#if !DEBUG private #else internal #endif static int`; `#else`/`#endif` stand in the leading trivia of `static`, the second modifier) | RH2001-009 | `VerifyNoCodeFixIsOffered` | **red**: the head fixes it (every branch holds an accessibility keyword). This test pins the modifier arm |
| `RH2001…VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier` (new: a whole `#if DEBUG private int _debug; #endif` group above `private static int Value { get; }` keeps the fix, and the group stays above the field; the other side of RH2001-009's position condition, which leaves the directives above the first token to RH2001-004) | RH2001-004, RH2001-009 | `Verify` | green |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalExternBetweenTheModifiersAndTheType` | RH2001-009 | `VerifyNoCodeFixIsOffered` | green (summary reworded; the hazard is now covered by the general refusal) |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalReadonlyBetweenTheModifiersAndTheType` | RH2001-009 | `VerifyNoCodeFixIsOffered` | green (summary reworded) |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalUnsafeBetweenTheModifiersAndTheType` | RH2001-009 | `VerifyNoCodeFixIsOffered` | green (summary reworded) |
| `RH2001…VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder` (existing, `#pragma`) | RH2001-009 | `Verify` | green, unchanged. It is the other side of the directive-kind condition: a non-conditional directive keeps the fix |

Dropped: `RH2001…VerifyFixIsOfferedForAConditionalNewBetweenTheModifiersAndTheType`. It existed only to pin that `new` belongs to the removed keyword set. Under decision 48 its input withholds the fix exactly as the `static` test does, with no condition of its own.

Kept, although they now only repeat the general refusal: the `extern`, `readonly`, and `unsafe` tests. They pin the hazards that RH2001-009's Why names (`extern`, a second `readonly`) and the auditor's suggestion for #19. The lead may drop `unsafe` if it wants one test per hazard named in the spec.

#21 is moot under decision 48: no branch reading and no comment scan remain to be tested.

## Scenarios (bug fix)

_Not applicable_: this round pins decisions and spec examples of a behavior change.

## Not covered, and why

- **The RH2001-009 example with several properties (`First`, `Second`, …) as one test.** Its `static` shape is the re-pointed decision-36 test, and the `extern`/`readonly`/`unsafe` shapes are their own tests. I did not copy the multi-property form while the spec author is still editing it. The spec checker can ask for it once the example is settled.
- **`#elif` alone, and a group fully between two modifiers.** The refusal tests trivia kind and position only. The `#else` test and the type-arm test reach both arms (leading trivia of `Modifiers[1..]`, and of the type).
- **A non-conditional directive in front of the type.** RH2001-009 leaves it out of the new refusal. The `#pragma`-between-modifiers test covers the directive-kind side, and RH2001-007/-004 tests cover the rest.
- **Decisions 46/47 through RH7102.** These go through the same shared call as RH7103/RH7109; see #1595.
- **FC-010 to FC-014 (formatter).** No formatter code is involved.
- **FC-007/FC-008 (Fix All).** Every `Verify` test runs Fix All. The new withholdings come from a property's or a type's shape, so they cannot co-occur with a fix in a way the existing Fix All scenarios do not already show.
- **FC-033.** Every new and changed test has LF and CRLF rows.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"
scripts/test.sh --project architecture
```
