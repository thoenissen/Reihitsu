Role: auditor
Commit: 63bc18a1d2e26be252110ffbe90c777559a5d42f
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: 63bc18a, which is the checkout. `origin/main` is 4838e9a03f0736d9e4593286d0f251f3a92fb704, matching the prompt, and it is an ancestor of the head.
- This audits the narrowing round (decision 26) as a repair delta against 729ce54, the commit `27-auditor.md` audited. It covers commits d283edd through 63bc18a.
- Production: none. `git diff 729ce54..63bc18a` touches no file under `src/` other than the RH2001 test class.
- Tests: 3 new `Verify` tests in `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, each tagged `RH2001-011`, placed inside `#region Tests`. No existing test changed.
- Spec: RH2001-011 ¶3, offered bullet 2 (the nullable sentence) and withheld bullet 3 (restricted to "a tuple type", with a back-pointer).
- Change doc `sdd/changes/2026-11/rh2001-rename-binding.md`: the inferred-name *What* bullet was split into three groups, with the nullable boundary added to the third. Six `sdd/work/…` pointers and the fixture labels `a01`/`a02` were removed. The measured SHAs were kept.
- The diff matches handovers 28, 29, and 30. Scope did not grow, and the round changed nothing outside RH2001's tests, its spec, and its change doc.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence: the test implementer ran the 3 tests against `main`'s production code (`scratchpad/r28-main.txt`, `Passed: 3`) and on the head (`r28-head.txt`). The cleaner reran them on the head (3/3) and reported a Release build with 0 warnings. I re-read the unchanged guard, `TupleTypeKeepsElementName` (provider lines 245–264), to compare it with the restated text.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 8 | closed | Three tests now pin `main`'s exact offered output for o1 (`object pair = (Value, 1);`), n2 (`(int, int)? pair = (Value, 1);`), and n1 (`(int Value, int Other)? pair = (Value, 1);` with `pair.Value.Value` read). In each case the guard sees a converted type of `object` or `Nullable<…>`. Neither is `INamedTypeSymbol { IsTupleType: true }`, so the guard returns `false` and the fix is offered. `Verify` sets `FixedCode` to an output that differs from the input, so a withheld fix fails it. Mutant M2 from `27-auditor.md` (return `true` for a named type that is not a tuple) withholds all three, so each test falsifies M2. |
| 9 | closed | Offered bullet 2 now states that a nullable tuple type is not a tuple type, also when it names the element, and gives n1 as the example. Withheld bullet 3 now reads "a tuple converted to a tuple type that supplies the name itself" and points back to bullet 2. Both match the code: `Nullable<(int Value, int Other)>` has `IsTupleType == false`. The change doc's third group carries the same boundary. n1 is also pinned by a test now. |
| 10 | closed | `rg 'sdd/work' sdd/changes/2026-11/rh2001-rename-binding.md` finds nothing. The sentences that used to point at decisions 19 and 23 still state the reason in place: the rare shapes, and "two repairs … each missed positions". The fixture labels that named work-folder fixtures are gone. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The tuple's own conversion drops the name (`TupleTypeKeepsElementName`, second clause; code unchanged) | `GetTypeInfo(tuple).ConvertedType` of the innermost tuple. It counts as keeping the name only if it is a tuple type whose element at that position carries the inferred name | RH2001-011 ¶3, offered bullet 2 ("is not a tuple type, such as `object` or `(int, int)?`; a nullable tuple type is not a tuple type, also when it names the element"), and withheld bullet 3 ("a tuple type that supplies the name") | Agree. The text's boundary is now exactly `IsTupleType`. `Nullable<ValueTuple<…>>` falls on the offered side under both, whether or not the element is named | holds; all three offered cells are now pinned |
| Deconstruction target (first clause) | Unchanged since `27-auditor.md` | RH2001-011 ¶3, offered bullet 1 | Unchanged | holds (a18 still falsifies M1) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a tuple keeps an element name | Unchanged: only the RH2001 provider (`GivesInferredMemberNameAsync`, `TupleTypeKeepsElementName`) | one owner; holds |
| Whether a rename may be applied | Unchanged. The FC-015 follow-up stays in `00-lead.md` | accepted by decisions 3, 9, and 22 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToObject` | Fix offered, with `main`'s exact output `object pair = (_value, 1);` | Withheld, or other text | `Verify` (exact `FixedCode`, batch path through the base) | holds. Green on `main` and head; M2 fails it |
| `…ConvertedToANullableUnnamedTupleType` | Offered, with `(int, int)? pair = (_value, 1);` | Withheld, or other text | `Verify` | holds. Green on `main` and head; M2 fails it |
| `…ConvertedToANullableNamedTupleType` | Offered, with `(int Value, int Other)? pair = (_value, 1);`. The read `pair.Value.Value` still compiles because the target type supplies `Value` | Withheld, or other text, or output that does not compile (the verifier compiles the fixed state) | `Verify` | holds. Green on `main` and head; M2 fails it |

## Visible output

- The three new tests assert offered output equal to `main`'s, measured on `main` at 4838e9a (`r28-main.txt`). An unchanged output puts nothing in question.
- No rule page changed. No other expected value changed.

## Failure classes dismissed

- FC-001, FC-002: no guard changed. The qualifier "tuple type" in withheld bullet 3 is now explicit, and it matches the `IsTupleType` condition of the predicate (#9).
- FC-003: both sides of the non-tuple boundary are now tested. On the offered side are `object`, the unnamed nullable, and the named nullable. On the withheld side, `(int Value, int Other) pair = (Value, 1);` was already tested.
- FC-004, FC-005, FC-019 to FC-023: no trivia, directive, or literal handling changed.
- FC-006: each new expected output compiles, because the verifier compiles the fixed state.
- FC-007 to FC-009, FC-018, FC-030: no change to the provider.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter or precheck code changed.
- FC-015, FC-016: no new copy of any policy. The sibling non-tuple targets `dynamic`, `ValueType`, and interfaces take the same `IsTupleType == false` branch as `object` (28, *Not covered*). An extra test there would pin the same branch.
- FC-017: see *Assertion adequacy*.
- FC-024 to FC-027, FC-029: no token-position, scope, or partial-type logic changed.
- FC-031: no method changed. The `<returns>` of `TupleTypeKeepsElementName` still matches the code and the restated spec. The new tests' summaries describe their inputs.
- FC-032: the cleaner's Release build (30) ran after the last `src` commit (e543db0, test-only).
- FC-033, FC-034: the three test inputs are the fixtures that `27-auditor.md` ran through apply-fix under LF and CRLF, verbatim.
- FC-035: the expected values were measured on `main`'s production code, not on a patched copy.
- FC-036: tuple element names exist at compile time only, and no anonymous member is involved.
- FC-037: the round changed no production `.cs` file, so no text-only proof is needed. The test file adds tests.

## Architecture findings

_None._

## Hints

- `27-auditor.md` (#10) said that no change doc on `main` points into `sdd/work/`. That is not accurate. `sdd/changes/2026-11/comma-behind-comment-unsupported.md:102` cites `sdd/work/comma-comment-layout/04-skeptic.md`. This defect is pre-existing and unrelated to this run, so it is not a finding here. It is a possible follow-up for that change doc: replace the pointer with the definition it refers to.
- The validator should still confirm the analyzer project's full test count in a single run (see 19 and 26).
