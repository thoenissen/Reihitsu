Role: test implementer
Commit: a716919 (tests written on the production code of 234e32a; nothing under `src/` except the RH7109 test class changed)
Verdict: #1593: tests written (pins of decision 23, all green on the head); #1595: no new tests; #1616: no new tests

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: the pins of decision 23 and the new SHARED-ORDER-007 example from `18-spec-author.md`.

Inputs: `00-lead.md` (decision 23), `17-implementer.md` (*Disputed tests or scope*), `18-spec-author.md`, `16-test-implementer.md`, SHARED-ORDER-007 in `sdd/specs/shared/ordering-move-safety.md`.

## thoenissen/Reihitsu#1595

No new test. RH7102 reaches the guard only through `OrderingDeclarationUtilities.ChangesMemberOrder` (the `rg` in `16-test-implementer.md` still holds); the pinned behaviors live in that shared owner.

## thoenissen/Reihitsu#1593

File: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`, 8 tests, each a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair.

How the rows were checked:

- Every fixture ran with `scripts/apply-fix.sh RH7109` under LF and CRLF, on the head and on a temporary worktree at `origin/main` 57acf0d (removed afterwards).
- The whole test class was then run against main's production code (the head's test file copied into the main worktree). Of the new tests, only the over-cap test fails there (main offers one action); the other seven pass on main as on the head. The expected outputs of the "keeps the fix" tests are main's outputs.
- Each pin was checked to bite, by a temporary mutation of `OrderingDeclarationUtilities.cs` in a separate worktree at HEAD (removed afterwards; the branch's production code is untouched):
  - `WritesOnlyInsideBrackets` returning `false`: the attribute-list and collection-initializer tests fail (no fix), nothing else.
  - `MaximumConfigurationCount` 512: the over-cap test fails (one action). 128: the eight-group test fails (no fix).
  - Withholding when a region ends at the body end (the first draft `17-implementer.md` describes): the closing-brace test fails (no fix).
  - A `const` configuration counted like any other (`isConst = false` in `TryAddOrderedMembersOfRegion`): the `K` example fails ("Expected value to not be null"); its counterpart stays green, as it should.
- `scripts/format.sh` changed nothing. Architecture tests pass (115).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixIsOfferedWhenACrossedDeclarationIsAStaticReadonlyFieldInOneBranchAndAConstantInTheOther` (the new SHARED-ORDER-007 `K` example, `internal static readonly` / `private const`) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` | green (main: offered as well) |
| `NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantInTheActiveBranch` (the example's counterpart, `private readonly` in place of `private const`) | SHARED-ORDER-007, RH7109-006 | `Verify` + `GetCodeFixActionsAsync` on `_b`, asserted empty | green (main: withheld as well) |
| `FixMovesAFieldAcrossAFieldWithNineConditionalAttributeLists` (nine `#if An [Attr] #endif` groups in front of a crossed `private static int _b = Log("b")`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `FixMovesAFieldAcrossAFieldWhoseCollectionInitializerHasNineConditionalEntries` (crossed static `List<int>` initializer with nine `#if An n, #endif` entries) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `FixMovesAFieldAcrossEightConditionalGroupsInFrontOfACrossedField` (eight groups, each a field, in `_b`'s leading trivia: 256 configurations, at the cap) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit` (the same with nine groups: 512 configurations) | none: characterization of the safety bound of decision 23, which no requirement decides | `VerifyNoCodeFixIsOffered` | green; red on main (one action) |
| `FixMovesAFieldWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheType` (`#if DEBUG … } #else } #endif`, nothing initialized crossed) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green (main's output) |
| `NoCodeFixWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheTypeAndAnInitializedFieldIsCrossed` (same body, `_b = Log("b")` moved across `_a = Log("a")`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main: withheld as well) |

Notes on the shapes:

- **`K` example** asserts no output text, as `18-spec-author.md` asks: the `int K = 1;` continuation line after `#endif` is re-indented by the whole-type formatting (decision 15).
- **The counterpart** reports `K` too (in the active build it is a `readonly` field behind `_a`), so `VerifyNoCodeFixIsOffered`, which asserts no action for any diagnostic, does not fit: `K`'s own move crosses nothing initialized and is offered. The test asserts the exact two diagnostics and that `_b`'s diagnostic gets no action.
- **The collection initializer** is written with its braces in the column the whole-type formatting gives them (34). With the braces at column 4, main's output re-aligns them, which is the open layout follow-up of decision 15, not what this pin decides.
- **The over-cap fixture** puts the nine groups in front of a crossed field without an initializer, so the only reason to withhold is the cap. The eight-group test is the other side of the same condition (FC-003).

## Scenarios (bug fix)

None: these are pins of accepted behavior, not reproductions.

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `K` example | LF, CRLF | RH7109 single fix | holds | offered, on main and head |
| `K` counterpart | LF, CRLF | RH7109, `_b` diagnostic | holds | no action for `_b`, on main and head |
| attribute lists, collection initializer, 8 groups, closing brace | LF, CRLF | RH7109 | holds | main's output, on main and head |
| 9 groups | LF, CRLF | RH7109 | differs from main by decision 23 | head: no fix; main: fixed |

## Not covered, and why

- **Other consumers (RH7102, RH7103, RH7309).** The bracket rule, the cap, and the unclosed-group bound are in `OrderingDeclarationUtilities.TryGetOrderedMembers`/`GetConditionalGroups`/`GetConfigurations`, reached by every consumer through `ChangesMemberOrder` (`rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`). The decision is made in the shared owner before a consumer ranks positions, so one consumer pins it.
- **An `#if` in front of the body (two headers, each with `{`).** Withheld on main and head by `MoveRangeContainsDirectives` (`17-implementer.md` probe), so the member-order guard does not decide it.
- **The cap reached through nesting or `#elif` branches rather than side-by-side groups.** Same `MaximumConfigurationCount` checks in both `GetConfigurations` overloads; the side-by-side pair pins the bound itself. Decision 23 asks for one over-cap region.
- **The bracket rule for an accessor list** (excluded on purpose, `LiesInsideBrackets`): the `field`-keyword case is a recorded follow-up candidate, unchanged from main.
- **Failure classes:** FC-003 both sides of the cap, the const condition, and the initialized-crossing condition; FC-033/035 LF and CRLF rows, expected outputs from main; FC-030 the continuation and initializer-brace shapes are avoided or asserted without layout; FC-004/005/019/021 the guard handles no comments; FC-007/008 no new action path, Fix All runs inside `Verify`; FC-018/022–029 no rename, literal, or formatter phase is involved.

## thoenissen/Reihitsu#1616

No new test. Decision 23 has nothing for it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
scripts/test.sh --project architecture
```
