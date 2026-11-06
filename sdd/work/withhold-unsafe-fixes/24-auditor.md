Role: auditor
Commit: 69687d9
Verdict: findings (#1136: pass; #1134: findings; #1135: findings; #1262: pass; #1332: pass; #1336: pass; #1340: pass)

Inputs: the seven issues as named in `00-lead.md`, `00-lead.md` (maintainer answers 1–5, decisions 12–15), the earlier audits `10-auditor.md` and `18-auditor.md`, handovers `19`–`23`, the change doc `sdd/changes/2026-10/withhold-unsafe-fixes.md`, the specs the round changes (SHARED-ORDER-007, RH7103-007) and the ones its code reads (SHARED-FIX-003 *Run time* and *Build configurations*, SHARED-ORDER-002/-003, RH7109-006, RH7309-006, RH7004-004), and the diff `7eecbae..69687d9`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Observations come from `scripts/apply-fix.sh <ID>` on fixtures in the session scratchpad (LF and CRLF, `DEBUG` not defined), on this branch and, where marked "main", on a temporary worktree of `a59a003` (removed afterwards). No test was re-run: no finding turns on a test result.

## Scope

Delta audit of decision 12's structural change and repair round 2, against `7eecbae`. Head `69687d9` (checked out, clean). `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073` (`git ls-remote`), an ancestor of the head. The diff matches handovers `19`–`23`: 6 production files (`OrderingDeclarationUtilities`, `Reihitsu.Core.csproj`, the RH7103, RH7109 and RH7309 providers, `RegionBlockLayout`), 13 tests in three test classes, SHARED-ORDER-007, RH7103-007, and the change doc. The change stays inside the components the run already changes; scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed. Evidence of `18-auditor.md` that the round did not touch is reused.

### Status of the earlier findings

| # | Status | Evidence |
|---|---|---|
| 9 | closed | RH7103 asks `ChangesMemberOrder(typeDeclaration, typeSymbol, …)`; fixture h is tested (withheld). The class was enumerated again on RH7103 along the set axis (instance layout fields, static initializers) and the type axis: static method across an instance method with an excluded instance field in its trivia, no instance field crossed (m1: offered, as SHARED-ORDER-007 states); static auto-property across an instance auto-property (m2: withheld); `[StructLayout(Sequential)]` class (m3: withheld); `LayoutKind.Auto` struct (m4: offered); `record struct` (m5: withheld); excluded static initializer moving across a crossed static initializer (m6: withheld); excluded instance field-like event (m7: withheld); partial struct with `Auto` on another part (m8: offered). All hold |
| 10 | closed | SHARED-ORDER-007 now states that members of excluded code in the moved declaration's leading trivia are moved members and cross what it crosses, for initializers and for the layout guard; change-doc case 10's wrong sentence is replaced and fixtures a, c, h are listed with their `main` outputs. Fixtures a and c are tested on the withheld side. The offered side of one condition is not pinned: #12 |
| 11 | closed | The five tests cover g1, g3, g4, g7, g8 as suggested, each with `VerifyNoCodeFixIsOffered` or the full output |
| Architecture finding of `18-auditor.md` | closed | `rg -n "ChangesInitializerExecutionOrder\|ChangesMemberOrder\|FieldOrderSetsLayout\|fieldOrderSetsLayout" src`: the flag and `FieldOrderSetsLayout` occur only inside `OrderingDeclarationUtilities`, as private members; the two public overloads take the `INamedTypeSymbol`; production callers are RH7103, RH7109 (single move) and `RegionBlockLayout` → RH7309 (rank). `ChangesInitializerExecutionOrder` is `internal` and called only by `Reihitsu.Core.Test`. No code fix can choose the sets any more |

## thoenissen/Reihitsu#1136

Pass. #11 is closed. g4's expected output is `main`'s LF output (observed on `main`: the same diff). See *Hints* for a pre-existing CRLF drift in that output.

## thoenissen/Reihitsu#1134

Findings: #12 (RH7109 side). #10 is closed for the withheld sides. Holds: n1 and n2 (struct, `static readonly _r` with an excluded instance field in its trivia, crossing only a static field): offered, as SHARED-ORDER-007 states.

## thoenissen/Reihitsu#1135

Findings: #12 (RH7103 side), #13. #9 is closed (table above).

## thoenissen/Reihitsu#1262

Pass. `RegionBlockLayout.ChangesMemberOrder` forwards the symbol; RH7309's verdicts are unchanged (the round's handovers report the RH7309 tests green, and the rank overload's body is unchanged).

## thoenissen/Reihitsu#1332

Pass. No change in the round.

## thoenissen/Reihitsu#1336

Pass. No change in the round.

## thoenissen/Reihitsu#1340

Pass. No change in the round.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 12 | medium | `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests`, `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests` against SHARED-ORDER-007 ("… reorders instance fields when excluded code in its leading trivia declares one **and the move crosses an instance field**") | FC-017, FC-003 | #10 | same requirement | The layout counting of excluded code in the moved trivia has three conditions: an excluded instance field in the moved trivia, a layout type, and a crossed instance field. The new tests flip the first two (`…OfAClass`, `FixMovesAStaticFieldOfAStructAcrossAnInstanceField`) but every test that carries the excluded field also crosses an instance field. A guard that withholds for "layout type and excluded instance field in the moved trivia", ignoring what is crossed, passes every test and withholds fixes SHARED-ORDER-007 offers. The code is right today: RH7103 m1 (`struct S { public void M() { }` ⏎ `#if DEBUG` ⏎ `private int _y;` ⏎ `#endif` ⏎ `public static void N() { } }`: offered, `N` with the `#if` block moves in front of `M`) and RH7109 n1/n2 (`struct S { private int _x; private static int _s;` ⏎ `#if DEBUG` ⏎ `private int _y;` ⏎ `#endif` ⏎ `private static readonly int _r; }`: offered). For initializers the matching side is pinned (`FixCarriesAnInitializedFieldOfExcludedCode…AcrossAFieldWithoutInitializer`). This is a member of #10's class (moved-trivia members, stated and pinned) that #10's enumeration did not name: the offered side of the crossing condition for the layout set | Test implementer: one RH7109 test for n1 (the shape stays inside one field group, so its output is the RH7109-005 move) and, optionally, one RH7103 test for m1; both expected outputs are the plain move of RH7103-005/RH7109-005, which `main` also produces. No production change |
| 13 | low | `OrderingDeclarationUtilities.HasInitializer`, `OrderingDeclarationUtilities.IsLayoutField` (`src/Reihitsu.Core/OrderingDeclarationUtilities.cs:312`, `:333`) | FC-015 | – | PR-introduced | Both are `public`, but no code outside `OrderingDeclarationUtilities` calls them (`rg "HasInitializer\b\|IsLayoutField" src`). `HasInitializer` was `private` on `main`; the run made it `public` (decision 5) and added `IsLayoutField` as `public` for consumers that repair 1 removed. Decision 15 narrowed `FieldOrderSetsLayout` and `ChangesInitializerExecutionOrder` so that no code fix can rebuild the member-order comparison, yet these two building blocks of that comparison stay open to every assembly. No wrong output | Make both `private`, as `FieldOrderSetsLayout` is; no test calls them |

Severity notes. #12 leaves a stated condition of a newly withheld fix unpinned; it changes no output. It repeats #10's class only in test coverage: the code has one owner of the decision, and m1, n1, n2 hold, so I see no structural cause (*Architecture findings*). #13 is internal API hygiene.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which sets count for a fix (initializers per context, layout fields) | `ChangesMemberOrder(type, symbol, …)`: both initializer sets always; layout set when `FieldOrderSetsLayout(symbol)` (struct without `Auto`/`Explicit`, class with `StructLayout(Sequential)`, every part) | SHARED-FIX-003 *Run time*, *Build configurations*; SHARED-ORDER-007 | yes, for RH7103, RH7109, RH7309 alike; no caller chooses | ok (#9 closed) |
| RH7103 symbol missing | `semanticModel?.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol` | RH7109's identical guard | yes; the base always fetches the document's semantic model, so no caller reaches the `null` branch | ok |
| Single-move ranks (unchanged body) | `< target.FullSpan.Start` → 0, moved full span → 1, between → 2, behind → 3 | SHARED-ORDER-007 bullets (crossed span; moved leading trivia) | yes; the spec text now states the rank-1 members as moved | ok (#10 closed); offered side of the layout crossing untested (#12) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member order (initializers, layout fields), counting excluded members | `OrderingDeclarationUtilities.ChangesMemberOrder` (public symbol overloads → private flag overloads); `ChangesInitializerExecutionOrder` internal, test callers only; consumers RH7103, RH7109, `RegionBlockLayout` → RH7309, all passing the symbol | one owner; its building blocks `HasInitializer`/`IsLayoutField` are still public (#13) |
| Layout type | `FieldOrderSetsLayout` (private), `IsStructLayoutAttribute`; `rg "StructLayout\|LayoutKind" src` outside tests: only `OrderingDeclarationUtilities` | ok |
| Whether a member-moving fix asks the owner | the overrides of `IsMoveSafe` in RH7103 and RH7109 (identical); `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` checks directives only; RH7102 does not ask | unchanged by the round; RH7102 is outside the run (hint) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7103 `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct` | layout guard counts excluded fields that move with a static member | an action is registered | `VerifyNoCodeFixIsOffered` | ok; observed failing before `cd3fa06` (19-test-implementer) |
| RH7103 `FixCarriesAnInstanceFieldOfExcludedCode…OfAClass`, `FixMovesAStaticFieldOfAStructAcrossAnInstanceField` | the guard needs a layout type, and an excluded instance field | no action, or another output | `Verify` | ok; the crossing condition is not flipped (#12) |
| RH7109 `NoCodeFixWhenExcludedCode…CarryAnInitializerAcrossAnInitializedField` / `FixCarriesAnInitializedFieldOfExcludedCode…AcrossAFieldWithoutInitializer` | moved-trivia initializer crosses a crossed initializer, both sides | action / no action | `VerifyNoCodeFixIsOffered` / `Verify` | ok |
| RH7109 `NoCodeFixWhenExcludedCode…InstanceFieldsOfAStruct`, `FixCarriesAnInstanceFieldOfExcludedCode…OfAClass`, `FixMovesAStaticFieldOfAStructAcrossAnInstanceField` | as the RH7103 trio | as above | as above | ok; crossing condition not flipped (#12) |
| RH7004 four `NoCodeFixWhenTheNewBlockWouldSplit…` (g1, g7, g3, g8) | pairing per warning code and per `#nullable` setting | an action is registered | `VerifyNoCodeFixIsOffered` | ok; the offered side of each pairing is pinned by g4 and the existing (c)–(e) |
| RH7004 `FixIsOfferedForNullableDirectivesOfDifferentSettingsOnBothSidesOfTheNewBlock` (g4) | different `#nullable` settings do not pair | no action, other output | `Verify` (LF) | ok for the pairing; LF only, the CRLF output drifts on `main` and here (hint) |

### Visible output is decided

Every expected value the round adds is decided. The withheld fixes (RH7103 h, RH7109 a and c, RH7004 g1/g3/g7/g8) are demanded by the approved SHARED-FIX-003 *Build configurations* bullet and the maintainer's #1136 clarification (decisions 11–13), and case 10 of the change doc lists a, c, h with their `main` outputs. The offered outputs (RH7103 and RH7109 class and plain-struct counterparts, RH7109 `_a` without initializer, RH7004 g4) are the plain moves of RH7103-005/RH7109-005 or RH7004-004's block, which `main` also produces (g4 observed on `main`). No rule page changed.

### Failure classes dismissed

- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-032: no formatter file changed.
- FC-018, FC-036: the renamed parameters (`fieldOrderSetsLayout` → `typeSymbol`) have no other spelling left; `rg` finds the flag only in the private overloads.
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-022, FC-023, FC-025, FC-026, FC-029: the round changes no trivia handling, literal, identifier, expression-body, or repeated-token logic; the move itself (`MoveMemberBefore`) is unchanged.
- FC-007, FC-008: every new `Verify` runs the iterations and Fix All of the base.
- FC-009: no new action that changes nothing.
- FC-024: RH7103 now resolves the type through the symbol, so partial parts count (m8).
- FC-030: whole-type formatting by RH7103/RH7109 stays with thoenissen/Reihitsu#969 (decision 9); the RH7109 test with an output uses a block-bodied `Log`.
- FC-031: every changed summary and `<param>` checked (`ChangesMemberOrder` four overloads, `ChangesInitializerExecutionOrder`, `RegionBlockLayout.ChangesMemberOrder`); each describes the new signatures and cross-references resolve (Release build 0 warnings).
- FC-033, FC-034: the guards read members, initializers, and the symbol, not line structure; the withheld tests assert that no action exists, which no line ending changes. The fixtures are the reported shapes themselves.
- FC-035: 19-test-implementer observed the RH7103 layout test failing on the unchanged code.
- FC-037: no text-only claim.
- FC-001, FC-002, FC-003, FC-006, FC-015, FC-016, FC-017: walked; the failing members are #12 and #13.

### Architecture findings

_None._ The structural change closes the architecture finding of `18-auditor.md`: the member-order owner decides its sets from the type symbol, and no caller passes the flag or reaches a layout-free check. #12 repeats #10's class only on the test side; the code it concerns has one owner and gives the specified output for every member traced (m1, n1, n2), so I name no structural cause for it. The one consumer that still does not ask the owner, RH7102, is outside the run and changes no output of it (hint).

### Hints

- RH7004 under CRLF (pre-existing, unrelated to the round): when a comment or a directive stands in front of the first following statement, the fix writes one LF line ending into a CRLF document. Observed identically on `main` and on the branch for g4, on `main` for g7 (withheld on the branch), and on both for `#pragma warning disable CS0618` directly behind the using declaration (h1: CRLF=12, LF=1), and an own-line `// note` there (h2: CRLF=11, LF=2). It widens the follow-up candidate from `10-auditor.md` (`#nullable enable` among the following statements). `19-test-implementer.md` states that g4's CRLF behavior is covered by other tests; it is not, and g4 drifts under CRLF.
- `ChangesInitializerExecutionOrder` is production code that only `Reihitsu.Core.Test` calls (decision 15). Its four Core tests exercise a wrapper no code fix uses; the production overloads are covered through the analyzer tests only.
- RH7103's and RH7109's `IsMoveSafe` overrides are now identical. RH7102 is the only `TypeMemberOrderingCodeFixProviderBase` consumer without the owner call (follow-up candidate in `00-lead.md`); once that is decided, the call belongs in the base.
- `InternalsVisibleTo` is declared as a csproj item in `Reihitsu.Core`, while `Reihitsu.Cli` and `Reihitsu.Formatter` declare it in `Properties/AssemblyInfo.cs`. `Reihitsu.Core` has no `Properties` folder, so this is a style difference only.

### Follow-up candidates

- RH7004 fix: LF written into a CRLF document when a comment or a directive stands in front of the first following statement (hint above; extends the existing candidate).
