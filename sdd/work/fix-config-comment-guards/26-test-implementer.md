Role: test implementer
Commit: f5a8ad4 (tests written against the production code of 6b91450; nothing under `src/` changed since except this test class)
Verdict: #1593: tests written; #1595: no new tests; #1616: no new tests

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: the worklist's Next line in `00-lead.md` (decisions 24–26), `25-spec-author.md`, `24-implementer.md` (*Disputed tests or scope*), and `23-test-implementer.md`.

How the rows were checked:

- Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair; both rows behave the same.
- The whole RH7109 class passes on the head: 184 rows.
- The new and re-tagged tests were also run against `origin/main` 57acf0d's production code (the head's test file copied into a temporary worktree, removed afterwards). The fixtures also ran through `scripts/apply-fix.sh RH7109` on main, under LF and CRLF. Results are in the table's last column.
- `scripts/format.sh` changed nothing.

## thoenissen/Reihitsu#1595

No new test. RH7102 reaches the per-configuration reading only through `OrderingDeclarationUtilities.ChangesMemberOrder`. `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`, and both call that one owner, as `23-test-implementer.md` states.

## thoenissen/Reihitsu#1593

File: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`. The change adds 4 new tests and puts requirement tags on 2 existing tests. No assertion changed.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixIsOfferedWhenACrossedDeclarationIsSplitIntoAStaticFieldAndAnInstanceInitializedField` (SHARED-ORDER-007's genuinely split `_b` example, verbatim) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` (no expected text: the lone `;` continuation line is decision 15's shape, as `25-spec-author.md` says) | green (main: green) |
| `NoCodeFixWhenACrossedDeclarationSplitIntoAStaticFieldAndAnInstanceInitializedFieldIsCrossedByAnInstanceInitializedField` (the example's counterpart, `private readonly int _c = Log("c");`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: green) |
| `NoCodeFixWhenABalancedExcludedBranchEndsAnExpressionBodyAndDeclaresAStaticInitializedField` (`24-implementer.md`'s new withholding: `public int P => 1` / `#if DEBUG ; static int _z = Log("z"); int Q => 2 #endif ;`, crossed by a static initialized `_c`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: red, one action registered; main's fix moves `_c` in front of `_t`, and in the `DEBUG` build that puts `Log("c")` before `Log("z")`) |
| `FixIsOfferedAcrossAPropertyWhoseExcludedBranchEndsTheExpressionBodyWithoutDeclaringAMember` (counterpart: `public int P =>` / `#if DEBUG 1; #else 2; #endif`, balanced, ends the member, declares nothing; a `private int _e;` follows so that `#endif` stays in the crossed span and is not in `_c`'s leading trivia) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` (see *Not covered* for why the expected text is not pinned) | green (main: green) |
| `FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove` (far9, existing; tags added) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main: green) |
| `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit` (over-cap, existing; tags added) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: red, main offers the fix) |

The two new expression-body tests are the two sides of a condition that `24-implementer.md` named: a bracket-balanced branch whose `#if` the configuration parse puts at member level. One side declares a member of the crossed context; the other declares none (FC-003). Before this change, b2 and b1s covered that side only through another group. These tests pin it directly.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| split `_b`, static `_c` | LF, CRLF | RH7109 single fix | holds | fix offered on head and main |
| split `_b`, instance `_c` | LF, CRLF | RH7109 | holds | no action on head and main |
| balanced branch ends `P`, declares static `_z` | LF, CRLF | RH7109 single fix (`apply-fix.sh` on main) | head withholds | main: fixed; head: no action, as SHARED-FIX-003 requires |
| balanced branch ends `P`, declares nothing | LF, CRLF | RH7109 single fix (`apply-fix.sh` on main and head) | holds | fixed on both, with the same output |

### Not covered, and why

- **Exact output of the expression-body counterpart.** Main and the head produce the same output. The `2;` continuation line goes to column 4, and whole-document formatting does the same (`scripts/format.sh --dry-run`), so this is decision 15's class. In the same output, the blank line between `_t` and `P` disappears, and a blank line appears in front of `#endif`, inside the `#else` branch:

  ```
      private static int _t;
      public int P =>
  #if DEBUG
          1;
  #else
      2;

  #endif
      private int _e;
  ```

  This output is the same on main and is not caused by this run. Pinning it would lock in a layout that SHARED-ORDER-001/002 do not describe. So the test asserts only that the fix is offered. Reported below as a follow-up candidate.
- **A move whose moved member lies inside the over-cap region.** Unchanged from `23-test-implementer.md`: `MoveRangeContainsDirectives` withholds it before the cap applies.
- **Other consumers (RH7102, RH7103, RH7309).** They use the same shared owner; see the `rg` under #1595.
- **Formatter counterpart.** The formatter does not reorder members (P11).
- **Failure classes.**
  - FC-003: each new condition is tested on both sides (split member: a static moved member vs an instance moved member; balanced member-level branch: declares a member of the context vs declares none).
  - FC-033: LF and CRLF rows.
  - FC-035: no expected output was taken from patched code; both new fix cases assert only that the fix is offered.
  - FC-030: decision 15's continuation shapes are deliberately not pinned.
  - FC-004/005/019/021: the guard edits no comment or directive.
  - FC-007/008: no new action path. `VerifyCodeFixIsOffered` re-analyzes the result.
  - FC-018/022–029: no rename, literal, or formatter phase is involved.

### Follow-up candidates

- **A member move relocates a blank line into an `#else` branch.** RH7109's fix (on main as well) moves `_c` across `public int P =>` / `#if DEBUG 1; #else 2; #endif` / `private int _e;`. The blank line in front of `P` then reappears in front of `#endif`, inside the active branch. It may belong to the existing "member-moving fixes format the whole type" follow-up (decision 15); the lead decides.

## thoenissen/Reihitsu#1616

No new test. Nothing in decisions 24–26 touches it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~SplitIntoAStaticField|FullyQualifiedName~BalancedExcludedBranchEnds|FullyQualifiedName~WhoseExcludedBranchEndsTheExpressionBody|FullyQualifiedName~ExceedTheConfigurationLimit"
```
