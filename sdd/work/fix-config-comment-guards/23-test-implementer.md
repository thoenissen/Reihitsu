Role: test implementer
Commit: bcf0bf4 (tests written on the production code audit 3 saw at 45c37c5; nothing under `src/` changed since except this test class)
Verdict: #1593: tests written, findings #6 and #7 reproduced, decision 25 pinned; #1595: no new tests; #1616: no new tests

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: tests for the failing members of #6 and #7 and their counterparts, and for decision 25.

Inputs: `00-lead.md` (decisions 24, 25, worklist), `22-auditor.md` (findings #6, #7, #8 with fixtures), `16-test-implementer.md`, `19-test-implementer.md`, SHARED-ORDER-007 in `sdd/specs/shared/ordering-move-safety.md`.

How the rows were checked:

- Every fixture ran with `scripts/apply-fix.sh RH7109` under LF and CRLF, on the head and on a temporary worktree at `origin/main` 57acf0d (removed afterwards). Head results match the auditor's: b2, b1s, blk1, arr1, nt1 `fixed`; j1s, j2s, far9 `no-fix-offered`; b2c `no-fix-offered`; j1n `fixed`.
- The whole RH7109 class was then run against main's production code (the head's test file copied into the main worktree). Every new "keeps the fix" test (j1s, j2s, j1n, far9, blk1c, arr1c, nt1c) passes on main, so its expected output is main's output. The new withheld counterparts (b2c, j1w, j2w) pass on main too. The five #6 tests fail on main as on the head (main offers the fix).
- Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair; both rows behave the same.
- `scripts/format.sh` changed nothing. Architecture tests pass (115).

## thoenissen/Reihitsu#1595

No new test. RH7102 reaches #6, #7 and #8 only through `OrderingDeclarationUtilities.ChangesMemberOrder` (`rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`). The read-as-text gate, the name join and the cap are decided in that shared owner before any consumer ranks positions.

## thoenissen/Reihitsu#1593

File: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`, 15 new tests, and the over-cap test's summary re-scoped.

Fixtures are the auditor's, verbatim (each ends with `Run` and `Log`; the diagnostic is RH7109 on `_c`, which the fix moves in front of `_t`). The block, expression-body and nested-type counterparts are new: the same shapes with every branch balanced inside the method block, expression body, or nested type.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenAnExcludedBranchThatClosesTheActiveBracketHoldsAGroupDeclaringAStaticInitializedField` (b2, #6, bracket rule) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| `NoCodeFixWhenAGroupBesideAGroupThatClosesTheActiveBracketDeclaresAStaticInitializedField` (b1s, #6, bracket rule) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| `NoCodeFixWhenTheNestedGroupDeclaringAStaticInitializedFieldIsAtMemberLevelInEveryBuild` (b2c, counterpart) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: withheld as well) |
| `NoCodeFixWhenAnExcludedBranchClosesAMethodBlockAndDeclaresAStaticInitializedField` (blk1, #6, block rule) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| `FixMovesAFieldAcrossAMethodWhoseBlockHoldsAnExcludedStatement` (blk1c, counterpart) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `NoCodeFixWhenAnExcludedBranchClosesAnExpressionBodyAndDeclaresAStaticInitializedField` (arr1, #6, expression-body rule) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| `FixMovesAFieldAcrossAPropertyWhoseExpressionBodyHoldsAnExcludedArgument` (arr1c, counterpart) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `NoCodeFixWhenAnExcludedBranchClosesANestedTypeAndDeclaresAStaticInitializedField` (nt1, #6, nested-type rule) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| `FixMovesAFieldAcrossANestedTypeWhoseBodyHoldsAnExcludedStaticInitializedField` (nt1c, counterpart: the static initialized field belongs to the nested type in every build) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `FixMovesAStaticFieldAcrossTwoDeclarationsOfOneNameOfWhichNoneIsAStaticInitializedField` (j1s, #7) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix (output equals input) |
| `FixMovesAnInstanceFieldAcrossTwoDeclarationsOfOneNameOfWhichNoneIsAnInstanceInitializedField` (j2s, #7) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix (output equals input) |
| `FixMovesAStaticFieldAcrossTwoDeclarationsOfDifferentNamesOfWhichNoneIsAStaticInitializedField` (j1n, counterpart: distinct names) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `NoCodeFixWhenOneOfTwoDeclarationsOfOneNameIsAStaticInitializedField` (j2w: j2s's declarations crossed by a static initialized `_c`; the `DEBUG` build crosses `static _b = Log("b")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: withheld as well) |
| `NoCodeFixWhenOneOfTwoDeclarationsOfOneNameIsAnInstanceInitializedField` (j1w: j1s's declarations crossed by an instance initialized `_c`; the other build crosses `_b = Log("b")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: withheld as well) |
| `FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove` (far9, decision 25: over-cap region after the move, neither crossed nor moved) | none: decision 25 rests on the maintainer's standing answer, no requirement | `Verify(source, fixed)` | red: no fix (output equals input) |
| `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit` (existing, summary re-scoped) | none, as before | `VerifyNoCodeFixIsOffered` | green |

Result now: 8 tests (16 rows) red as listed, for the stated reason; the other 160 rows of the class pass.

On decision 25's re-scope: the existing over-cap test already has the shape decision 25 asks for. Its nine groups lie between `_a` and `_b`, so the move of `_c` in front of `_a` crosses every member of the region. Only its summary changed ("when the move crosses their members"); source and assertion are unchanged. It and far9 are the two sides of "the move touches the over-cap region" (FC-003).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| b2, b1s, blk1, arr1, nt1 (#6) | LF, CRLF | RH7109 single fix | reproduced | one action (head and main) vs none |
| j1s, j2s (#7) | LF, CRLF | RH7109 single fix | reproduced | no action vs main's moved output |
| far9 (#8 / decision 25) | LF, CRLF | RH7109 single fix | reproduced | no action vs main's moved output |
| b2c, j1w, j2w | LF, CRLF | RH7109 | holds | no action, head and main |
| j1n, blk1c, arr1c, nt1c | LF, CRLF | RH7109 | holds | main's output, head and main |

### Not covered, and why

- **The union of a genuinely split declaration (auditor's hint).** Decision 24 says each configuration is judged on its own. For one declaration split as `#if DEBUG private static int _b #else private int _b = Log("b") #endif ;` crossed by a static initialized field, that reading keeps the fix (no build has a static initializer), while SHARED-ORDER-007's split-member paragraph ("the member counts with what any configuration contributes"; "counts in both contexts") withholds it. No test is written for either outcome: it would lock in one side of a conflict between decision 24 and the spec text. The lead decides whether the spec sentence follows decision 24 (spec author) before a test pins it.
- **A move whose moved member lies inside the over-cap region.** The moved member's leading trivia then holds the groups' directives, and `MoveRangeContainsDirectives` withholds on main and head alike; the cap does not decide it.
- **Other consumers (RH7102, RH7103, RH7309).** Same shared owner, per the `rg` under #1595.
- **Formatter counterpart.** The formatter does not reorder members (P11).
- **Failure classes:** FC-001/FC-016 are the findings themselves; FC-003 each new condition has both sides (bracket/block/expression body/nested type: excluded branch closes it vs balanced inside it; same name vs distinct name, and a build that does vs does not cross an initializer of the context; over-cap region touched vs away); FC-033/035 LF and CRLF rows, expected outputs from main; FC-030 decision 15's continuation shapes avoided (every fixed output only moves `_c` to the top of the body); FC-004/005/019/021 the guard edits no comment or directive; FC-007/008 no new action path, Fix All runs inside `Verify`; FC-018/022–029 no rename, literal, or formatter phase involved.

## thoenissen/Reihitsu#1616

No new test. Audit 3 has nothing for it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
scripts/test.sh --project architecture
```
