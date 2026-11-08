Role: test implementer
Commit: 9828fc6 (tests written against the production code of 439f105, which audit 4 saw; nothing under `src/` changed since except these two test classes)
Verdict: #1593: tests written, findings #9 and #10 reproduced, #11 pinned, the new limit example (cap2) reproduced; one conflict between existing tests and decision 28 reported; #1595: no new tests; #1616: no new tests

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: tests for the failing members of #9 and #10 and their counterparts, for #11 (acc1), and a list of existing tests that rest on a branch combination no symbol assignment selects.

Inputs: `00-lead.md` (decisions 27 to 29, the worklist's Next line), `29-auditor.md` (#9, #10, #11 with fixtures), `23-test-implementer.md`, `26-test-implementer.md`, and SHARED-ORDER-007 as the spec author restated it in parallel (413cfcf, `30-spec-author.md`).

How the rows were checked:

- Every fixture ran with `scripts/apply-fix.sh RH7109` (acc1 and its counterpart with `RH7309`) under LF and CRLF, on the head and on a temporary worktree at `origin/main` 57acf0d. The worktree and the fixtures were removed afterwards.
- The new tests were then run against main's production code (the head's two test files copied into the main worktree). Each test that keeps a fix passes on main, so its expectation is main's outcome (FC-035). The one exact output (nt3) is main's output byte for byte.
- Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair; both rows behave the same.
- `scripts/format.sh` changed nothing. Architecture tests pass (115).

## thoenissen/Reihitsu#1595

No new test. RH7102 reaches #9 and #10 only through `OrderingDeclarationUtilities.ChangesMemberOrder`: `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`, and both call that one owner (`23-test-implementer.md`, `26-test-implementer.md`).

## thoenissen/Reihitsu#1593

Files:

- `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`: 15 new tests.
- `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs`: 2 new tests.

No existing test changed.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneDeclaringAStaticInitializedField` (tc1, #9, the auditor's fixture and the spec example) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered (main: red too) |
| `NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneAroundTheMovedField` (tc2: tc1 with `_z` uninitialized, so only membership decides; in `DEBUG` the moved `_c` is `D`'s) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered (main: red too) |
| `NoCodeFixWhenAnExcludedBranchOpensANestedTypeAroundTheMovedField` (nt2, #9) | SHARED-ORDER-001, SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered (main: red too) |
| `NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldClosesANestedTypeThatAnotherGroupOpens` (nt4, new #9 member, see *Scenarios*) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered (main: red too) |
| `FixMovesAFieldAcrossANestedTypeThatExcludedBranchesOpenAndCloseInFrontOfIt` (nt3, counterpart: the nested type that the groups open and close holds a static initialized `_z` and closes in front of `_d`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `FixIsOfferedAcrossANestedTypeThatExcludedBranchesOpenAroundAMemberInFrontOfAnotherMember` (nt5, counterpart of nt4: `private int _e;` between the closing group and `_c`) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` (see *Not covered*) | green (main: green) |
| `FixIsOfferedWhenGroupsWithNegatedConditionsSplitACrossedDeclarationIntoAStaticFieldAndAnInstanceInitializedField` (sg1, #10) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` (decision 15's lone `;` line) | red: no action (main: green) |
| `NoCodeFixWhenGroupsWithIndependentConditionsSplitACrossedDeclarationIntoAStaticInitializedField` (sg1t, counterpart: `#if TRACE`; the `DEBUG`+`TRACE` build crosses `static _b = Log("b")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: red, main offers the fix) |
| `FixIsOfferedWhenGroupsWithTheSameConditionSplitACrossedDeclarationIntoAStaticInitializedFieldAndAnInstanceField` (sg1i, #10) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` | red: no action (main: green) |
| `NoCodeFixWhenGroupsWithNegatedConditionsSplitACrossedDeclarationIntoAnInstanceInitializedField` (sg1i's counterpart with `#if !DEBUG`: the build without `DEBUG` crosses `_b = Log("b")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: green) |
| `FixIsOfferedWhenTheMovedAndACrossedFieldAreStaticInBuildsThatExcludeEachOther` (cr1, #10) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` | red: no action (main: green) |
| `NoCodeFixWhenTheMovedAndACrossedFieldAreStaticInTheSameBuild` (cr1's counterpart: both split by `#if DEBUG`, so both builds cross an initializer of the moved member's context) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: green) |
| `NoCodeFixWhenSeparatedConditionalGroupsInTheCrossedSpanTogetherExceedTheConfigurationLimit` (cap2, the spec's new limit sentence) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered (main: red too) |
| `NoCodeFixWhenAPropertyOfAStructHasAnAutoAccessorOnlyInOneBranchOfItsAccessorList` (acc1, #11, RH7309) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | green (main: red, main offers the fix). Pins the accessor-list exemption: the auditor's mutation turns it red |
| `CodeFixMovesABlockOfAStructAcrossAPropertyWhoseAccessorListBranchesAreAllBodied` (acc1's counterpart: `get => 1;` / `get => 0;`) | RH7309-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green (main's output) |

Result now: 8 tests (16 rows) red as listed, for the stated reason. The other rows of both classes pass (279).

The spec's example of a nested type around the moved field (nt2's shape) and the type-closing example (tc1, verbatim) are covered by the tests above. tc1 and tc2 are the two sides of "the crossed type-closing branch declares an initializer of the moved member's context". Both are withheld, because in `DEBUG` the moved field is not a member of `C`.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| tc1, tc2, nt2 (#9) | LF, CRLF | RH7109 single fix | reproduced | one action, head and main, vs none |
| nt4: `#if DEBUG private class N { #endif` / `_d;` / `#if DEBUG } #endif` / `_c` (new #9 member) | LF, CRLF | RH7109 single fix | reproduced | head and main move `#if DEBUG } #endif`, which is `_c`'s leading trivia, to the top of `C`. In the `DEBUG` build the moved `}` then closes `C` itself. Expected: no action. The new SHARED-ORDER-007 paragraph covers it: in `DEBUG`, `N` reaches across the start of the moved declaration |
| sg1, sg1i, cr1 (#10) | LF, CRLF | RH7109 single fix | reproduced | no action vs main's fix |
| sg1t | LF, CRLF | RH7109 | holds on head | no action on head; main offers the fix, which is wrong in the `DEBUG`+`TRACE` build |
| cap2 (decision 28) | LF, CRLF | RH7109 single fix | reproduced | one action vs none. The withholding is decision 28, which is covered by the standing answer |
| acc1 (#11) | LF, CRLF | RH7309 single fix | holds on head | no action; main offers the fix |
| nt3, nt5, sg1i's and cr1's counterparts, acc1c | LF, CRLF | RH7109 / RH7309 | holds | same outcome on head and main |

The auditor described nt3 as `no-fix-offered` on head and main. The shape built here from the auditor's description (`_z` inside the nested type, closed right behind it by `#if DEBUG } #endif`, then `_d`, `_c` and `Log` at member level) is `fixed` on both, with the same output. In every build `_z` is `N`'s and `_c` is `C`'s, so a fix is correct. The test pins main's output.

### Existing tests that rest on a combination no symbol assignment selects

None. Method:

- A script listed every test in the RH7102, RH7103, RH7109 and RH7309 classes, and in `OrderingDeclarationUtilitiesTests`, whose source holds two or more `#if` groups or an `#elif`.
- Only a test that withholds a fix can rest on such a combination, because a combination that no build compiles can only refuse a move. Among those tests, the ones whose groups name independent symbols (`DEBUG`/`TRACE`, `A1`…`A9`) select every combination by some assignment.
- That left three tests whose groups share a symbol. Each withholds because of a build that a real assignment selects:
  - `RH7102…NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations`: in `DEBUG`, `J` is `readonly int J = 1`, and the excluded `_c = Log("c")` moves with `K` across it.
  - `RH7102…NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken`: in `DEBUG`, `_b` is `static … = Log("b")`, and the excluded static `_x = Log("x")` moves with `K` across it.
  - `RH7109…NoCodeFixWhenAGroupBesideAGroupThatClosesTheActiveBracketDeclaresAStaticInitializedField`: in `DEBUG`+`TRACE`, `_a = Run(1 );` closes and `_z = Log("z")` stands at member level.

### Existing tests that conflict with decision 28 (reported, not changed)

Decision 28 and the restated limit count the symbols of every directive in the moved and crossed spans: more than eight refuses the move. Two existing tests, which decision 23 pinned to keep main's fix, have nine symbols in the crossed span:

- `FixMovesAFieldAcrossAFieldWithNineConditionalAttributeLists`: `#if A1` … `#if A9`, each holding an attribute list of the crossed field.
- `FixMovesAFieldAcrossAFieldWhoseCollectionInitializerHasNineConditionalEntries`: `#if A1` … `#if A9`, each holding an entry of the crossed field's collection initializer.

Under the spec text as written (413cfcf), both moves are refused, and each refusal withholds a fix that main offers. Both tests still pass on the head. The lead decides between two options: the limit leaves out directives that write only inside brackets, as the head's reading does today, and the spec states it; or the two tests change to `VerifyNoCodeFixIsOffered` under the standing answer.

### Not covered, and why

- **Exact output of nt5.** Main and the head produce the same output. The fix also moves `_d`, which lies inside the excluded nested type, from column 8 to column 4. That is whole-type formatting of the active tree (decision 15's follow-up). So the test asserts only that the fix is offered.
- **Exact outputs of sg1, sg1i, cr1.** These are decision 15's continuation shapes (a lone `;` line and modifiers on separate lines), as the auditor says. Only the offer is asserted. The decision under test is whether the fix is offered.
- **The spec author's residual** (`30-spec-author.md`: a group that lies wholly outside both spans and names only symbols the spans do not name). No test is written: the spec deliberately leaves it unblessed, and no input was found where it changes an outcome.
- **Other consumers (RH7102, RH7103).** They use the same shared owner; see the `rg` under #1595. RH7309 has acc1 because the accessor-list exemption was found through that rule.
- **Formatter counterpart.** The formatter does not reorder members (P11).
- **Failure classes:**
  - FC-001, FC-016: these are the findings themselves.
  - FC-003: each condition is tested on both sides:
    - a type-closing branch with and without a crossed initializer;
    - a nested type that closes in front of the moved declaration, inside its leading trivia, or behind it;
    - negated conditions against the same or independent conditions;
    - auto-accessor against bodied accessors;
    - eight symbols (existing test) against ten.
  - FC-017: every withholding uses `VerifyNoCodeFixIsOffered`.
  - FC-033: LF and CRLF rows.
  - FC-035: expected outputs come from main.
  - FC-030: decision 15's shapes are not pinned.
  - FC-004, FC-005, FC-019, FC-021: the guard edits no comment or directive.
  - FC-007, FC-008: no new action path. `Verify` and `VerifyCodeFixIsOffered` re-analyze the result.
  - FC-018, FC-022 to FC-029: no rename, literal, or formatter phase is involved.

## thoenissen/Reihitsu#1616

No new test. Nothing in decisions 27 to 29 touches it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ExcludedBranchClosesTheTypeAndOpens|FullyQualifiedName~OpensANestedTypeAroundTheMovedField|FullyQualifiedName~InFrontOfTheMovedFieldClosesANestedType|FullyQualifiedName~ExcludedBranchesOpen|FullyQualifiedName~GroupsWith|FullyQualifiedName~InBuildsThatExcludeEachOther|FullyQualifiedName~StaticInTheSameBuild|FullyQualifiedName~SeparatedConditionalGroups|FullyQualifiedName~AutoAccessorOnlyInOneBranch|FullyQualifiedName~AccessorListBranchesAreAllBodied"
```
