Role: test implementer
Commit: 04f3cbd (tests written on the production code the audit saw at cf4d7c6; nothing under `src/` except tests changed since)
Verdict: #1593: tests written, both findings reproduced; #1595: no new tests; #1616: no new tests

Repair round 1, items 1 and 2 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (Worklist), `09-auditor.md` (#1593 findings #1 and #2), `05-test-implementer.md`. The auditor's fixtures were not kept, so each row was rebuilt from its description and run with `scripts/apply-fix.sh` under LF and CRLF, on the head and on a temporary worktree at `origin/main` 57acf0d (removed afterwards). Expected outputs of the "keeps the fix" tests are main's output for the same input, confirmed by running these test methods green on main. Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair; both rows behave the same. `scripts/format.sh` changed none of the four files; `scripts/test.sh --project architecture` passes (115).

## thoenissen/Reihitsu#1595

No new test. Finding #1 reaches RH7102 through the shared guard; its row d1 is under #1593 below, in the RH7102 test class.

## thoenissen/Reihitsu#1593

### Tests

Finding #1 — a split whose excluded part lies in front of the declaration's first token:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (a1: `#if DEBUG private static #else private #endif int _b = Log("b");` crossed by `_c`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyThroughAWholeGroupInFrontOfItsFirstToken` (a2: `#if DEBUG static #endif` in front of `private int _b = Log("b");`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedConstantIsAStaticReadonlyFieldOnlyInAGroupInFrontOfItsFirstToken` (a4: `#if DEBUG private static readonly #else private const #endif int K = 1;`, `_c = Log(K)` moved across it) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenTheMovedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (e1: moved `_c` is `static readonly` in DEBUG, crosses `private static int _s = Log("s")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupBehindItsFirstModifier` (a3, counterpart: `#if` behind `private`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main offers a fix here; the head already withholds it) |
| RH7103 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (c1) | RH7103-007, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7102 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (d1: `K` carries an excluded `private static int _x = Log("x");` across the split `_b`) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7309 `NoCodeFixWhenAFieldOfTheMovedBlockIsStaticOnlyInAGroupInFrontOfItsFirstToken` (b1: `Fields` block moved across `private static int _a = Log("a")` between the blocks) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: one action registered |

Finding #2 — an excluded `=` that is no initializer (main fixed, head withholds):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 `FixMovesAFieldWithAnExcludedAttributeWithANamedArgumentInItsLeadingTrivia` (o1: `#if DEBUG [Obsolete("x", DiagnosticId = "y")] #endif` in front of `_b`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix applied (output equals input) |
| RH7109 `FixMovesAFieldWithAnExcludedAttributeWithANamedArgumentBehindAnActiveAttribute` (o2: behind active `[NonSerialized]`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix applied |
| RH7109 `FixMovesAFieldWithAnExcludedAttributeInItsLeadingTrivia` (o3, counterpart: `[Obsolete("x")]`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7103 `FixMovesAStaticMethodWithAnExcludedAttributeWithANamedArgument` (g1: `#if DEBUG [DllImport("a", EntryPoint = "b")] #else [DllImport("a")] #endif private static extern void M();` across `private int _b = 2`) | RH7103-007, SHARED-ORDER-007 | `Verify(source, fixed)` | red: no fix applied |
| RH7103 `FixMovesAStaticPropertyWithAnAssignmentInAnExcludedAccessor` (g2: static property `Value` with excluded `set { _v = value; }`, moved in front of instance property `Instance` across `public static int Counter = Log("c")`) | RH7103-007, SHARED-ORDER-007 | `Verify(source, fixed)` | red: no fix applied |
| RH7103 `FixMovesAStaticPropertyWithAnExcludedAccessor` (g3, counterpart: `set { Log("v"); }`) | RH7103-007, SHARED-ORDER-007 | `Verify(source, fixed)` | green |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| a1, a2, a4, e1 | LF, CRLF | RH7109 single fix | reproduced | one action (move reorders DEBUG static initializers; main and head alike) vs none |
| c1 / d1 / b1 | LF, CRLF | RH7103 / RH7102 / RH7309 single fix | reproduced | one action vs none |
| a3 | LF, CRLF | RH7109 | holds on head | no fix (main: fixed) |
| o1, o2, g1, g2 | LF, CRLF | RH7109 / RH7103 single fix | reproduced | no action on head vs main's moved output |
| o3, g3 | LF, CRLF | RH7109 / RH7103 | holds | fixed, equals main |

Shapes that differ from the auditor's description, so the rows reach the fix: g1 adds an active `#else [DllImport("a")]` and an instance method `Run` first (RH7103 compares members of one kind and accessibility; a lone field in front of the method gave no diagnostic); g2 moves the property across an instance *property* with a `public static` field between them, so `Counter` is in another accessibility group and not itself reported (otherwise the verifier fixes `Counter` first and the test would not isolate `Value`'s action). Members are separated by blank lines in g1–g3 so the expected output carries no separator the "member-moving fixes format the whole type" follow-up may change.

### Not covered, and why

- b2 and b3 (RH7309 split behind the first modifier; struct `#if !DEBUG static #endif private int _b;`): the auditor reports both as holding; the RH7109 counterpart a3 covers "group behind the first modifier" through the same `OrderingDeclarationUtilities.ChangesMemberOrder` owner (`rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`, `RegionBlockLayout.ChangesMemberOrder`).
- #2 on RH7102 and RH7309: same `GetOrderedMembers` owner as RH7109/RH7103, per the same `rg`; the RH7109 and RH7103 rows exercise both code paths the auditor names (excluded text inside a declaration's leading trivia / span, and the "outside every member" parse).
- The excluded `static` at nesting depth (auditor's h1): not confirmed by the auditor; no row reproduces it on its own.
- Expected outputs avoid continuation lines after `#endif` (decision 15).
- Formatter counterpart: the formatter does not reorder members (P11).

## thoenissen/Reihitsu#1616

No new test; the audit has no finding for it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
scripts/test.sh --project architecture
```

Result now: 22 rows (11 tests) red as listed, 254 passed.
