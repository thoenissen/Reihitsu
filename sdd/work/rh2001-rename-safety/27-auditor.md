Role: auditor
Commit: 729ce54917a052754bb784951928a1522bf4b955
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: 729ce54. `origin/main` is 4838e9a03f0736d9e4593286d0f251f3a92fb704. That matches the prompt and is an ancestor of the head.
- This is the repair delta against df58320, the commit `22-auditor.md` audited. It covers commits e7043ea through 729ce54, the last repair round.
- Production: `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` changes only in the `<returns>` doc of `TupleTypeKeepsElementName` (+3/−2). The cleaner's text-only proof passed (25).
- Tests: 8 new tests in `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`. No existing test changed.
- Spec and change doc: RH2001-004 (coverage pointer); RH2001-011 (¶3 restated as the refused class, "fourth paragraph" pointers reworded, `Why`, new tuple-`switch` example, renamed example heading); `rh2001-rename-binding.md` (inferred-name bullet, `Why`, visible output changes, two decisions, two alternatives).
- The diff matches handovers 23–26. The repair stayed within RH2001's fix, its spec, and its tests, so scope did not grow.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence:
  - `scripts/apply-fix.sh RH2001` on the head over `scratchpad/fx27` and `fx27n` (n1, n2, o1, s1, s2, d1). Output is in `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/fx27-head.txt`.
  - Two mutants of the provider, each built in a temporary worktree that I have since removed:
    - M1 deletes the nested-target `while` loop. I ran `NestedInADeconstructionTarget|DeconstructionTarget|DeconstructingConstructor` against it (`scratchpad/wt27-test.txt`).
    - M2 returns `true` when the converted type is a named type that is not a tuple. I ran the whole RH2001 test class against it, plus apply-fix over `fx27n` (`scratchpad/wt27b-test.txt`).

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 6 | closed | Decision 23 and answer 2 refuse the class. RH2001-011 ¶3 now keeps the fix offered in two positions only: a deconstruction target (nested included) and a tuple whose own conversion drops the name. It names every member of groups A and B as withheld (a01–a05, a07, a08, a15, a16, b02, b03). The code already decides exactly that, and I found no position where code and spec disagree (see *Guard scope*). The reason is split correctly: SHARED-FIX-003 applies where code reads the name or for an anonymous object; elsewhere it is a rare-class refusal, and the tuple `switch` is answer 2. The `<returns>` doc now matches the code. a01, a02, a07, a15, and b02 are pinned by `VerifyNoCodeFixIsOffered`, and each is red on `main` (24). |
| 7 | closed | (i) a18 is pinned with `main`'s exact output. M1 fails it (`Failed: 1, Passed: 1`), so the test falsifies deletion of the loop. (ii) c01 is pinned with `main`'s exact output, `other._value + _value`. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | medium | `RH2001…AnalyzerTests.cs`; RH2001-011 ¶3, offered bullet 2 ("or is not a tuple type, such as `object` or `(int, int)?`") | FC-003, FC-017 | #7. This is another offered-side boundary of the same guard that the spec states and no test pins; #7's enumeration did not name it. | same requirement | This round's ¶3 states that a tuple whose own conversion goes to a type that is not a tuple type keeps the fix: `object` and `(int, int)?`. The head offers it, with `main`'s output: o1 `object pair = (Value, 1);` → `(_value, 1)`; n2 `(int, int)? pair = (Value, 1);` → `(_value, 1)`. No test has such a target. M2 withholds o1, n2, and n1 (`(int Value, int Other)? pair = (Value, 1);`), and all 249 RH2001 tests stay green (`Passed: 249`). Holding: every other offered example of ¶3 (`return (Value, 1);`, `(int First, int Second)`, `Use((Value, 1))`, deconstruction source, a20, a18, t1) has a `Verify` test. | Add `Verify` tests with `main`'s exact output, tagged RH2001-011, for `object pair = (Value, 1);` and `(int, int)? pair = (Value, 1);`. |
| 9 | low | RH2001-011 ¶3, withheld bullet 3 ("a tuple converted to a type that supplies the name itself") against offered bullet 2 ("or is not a tuple type, such as … `(int, int)?`") | FC-002 | – | same requirement | The spec names only one side of the nullable boundary. n1, `(int Value, int Other)? pair = (Value, 1); return pair.Value.Value;`, is offered on the head and `main`, with output `(_value, 1)`, which compiles. The offered bullet covers n1 because a nullable is "not a tuple type". A reader of withheld bullet 3 can read the same input as "a type that supplies the name", and so expect it to be withheld. The spec's structure resolves this ("two positions only", "among them"), but no example shows which side n1 is on. Nothing observable changes either way. | Text only. In offered bullet 2, say that a nullable tuple type counts as not a tuple type even when it names the element, for example `(int Value, int Other)? pair = (Value, 1);`. Or restrict bullet 3 to "a tuple type that supplies the name". |
| 10 | low | `sdd/changes/2026-11/rh2001-rename-binding.md` lines 32, 136, 184, 222, 240, 248 | — | – | PR-introduced | The change doc is a durable document. It points six times into `sdd/work/rh2001-rename-safety/` (`01-skeptic.md`, `05-test-implementer.md`, `16-auditor.md`, `22-auditor.md`, `00-lead.md` decisions 19 and 23), and that directory is deleted before the PR is marked ready (`sdd/README.md`, *Temporary working docs*). Every pointer dangles once the PR merges. This round adds three of the six (184, 222, 248). The others are earlier rounds' and were not flagged before. No change doc on `main` points into `sdd/work/` for evidence. | Text only. Keep the measured SHAs (`measured with scripts/apply-fix.sh RH2001 on main at 4838e9a`) and drop the `sdd/work/…` paths. For the two decision pointers, state the reason in the change doc itself, which it already does, and drop the reference. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Deconstruction target, nested included (`TupleTypeKeepsElementName`, first clause) | Unchanged since `22-auditor.md`: the walk up through `ArgumentSyntax`/`TupleExpressionSyntax`, then `AssignmentExpressionSyntax.Left == outermostTuple` | RH2001-011 ¶3, offered bullet 1 | Agree: t1, t5, a13, a18, d1 (`(this.Value, x) = pair;`) offered | holds; a18 falsifies M1 |
| The tuple's own conversion drops the name (second clause) | `GetTypeInfo(tuple).ConvertedType` of the innermost tuple: a tuple type with the inferred name at that position, otherwise offered | RH2001-011 ¶3, offered bullet 2 and the withheld bullets, now stated by the tuple's own conversion | Agree on every member I traced or ran. a20 and s1 (conditional, switch arm with differently named branches) are offered. b02 and s2 (all branches or arms carry the name) are withheld. Nested, cast, and natural type are withheld. `object` and nullable are offered | holds. The nullable-named side is ambiguous in the text (#9). The non-tuple side is untested (#8) |
| Anonymous object vs. tuple arm | Unchanged | ¶3 "an anonymous-object member, which is never converted" | Agree | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a tuple keeps an element name | Unchanged since `22-auditor.md`: only the RH2001 provider (`TupleElements`, `IsTupleType`, `TryGetInferredMemberName`) | one owner; holds |
| Whether a rename may be applied | Unchanged. The FC-015 follow-up stays in `00-lead.md` | accepted by decisions 3, 9, 22 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| a01, a02, a07, a15, b02 (`VerifyNoFixIsOffered…`) | Diagnostic reported, no action registered | An action is registered | `VerifyNoCodeFixIsOffered` (`Assert.IsEmpty(actions)`). Each is red on `main` (24) | holds |
| a18 `…NestedInADeconstructionTarget` | Offered, with `main`'s exact output | Withheld, or other text | `Verify`, exact text. M1 fails it | holds |
| c01 `…OwnConstructionOfItsGenericType` | Offered, with `main`'s exact output | Withheld (for example by a stricter symbol comparison) | `Verify`, exact text | holds |
| a20 `…WithDifferentlyNamedBranches` | Offered, with `main`'s exact output | Withheld | `Verify`, exact text | holds; it is the offered side of b02 |
| Missing | — | — | — | The non-tuple-type conversion (#8) |

## Visible output

- a01, a02: withheld, covered by answer 2 (`00-lead.md`, *Maintainer answers*, and the change doc's *Decisions*).
- a07, a15, b02, and the other group A/B positions: withheld as a rare-class refusal (decision 23). The change doc lists them under *Visible output changes*.
- a18, c01, a20: offered with `main`'s output.
- No rule page changed.

## Failure classes dismissed

- FC-001: the inspected span was traced above. This round changed no guard.
- FC-004, FC-005, FC-019 to FC-023: the round changed no trivia handling or literal handling, and no production behavior.
- FC-006: every offered fixture output compiles (o1, n1, n2, s1, d1, and the new tests' results). The withheld group compiles on `main` but is refused by rule.
- FC-007 to FC-009: the guard is per diagnostic and unchanged. t1 still covers Fix All.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter phase, precheck, or formatter call changed.
- FC-015: no new copy of any policy.
- FC-016: I re-walked the sibling positions of a tuple under the restated rule: natural type, `switch`, `is`, `==`, interpolation, nested, cast, conditional, switch arm (both sides), named target, `object`, nullable, generic inference, lambda, and deconstruction target and source. Only the gaps in #8 and #9 remain.
- FC-018: still `Renamer.RenameSymbolAsync`.
- FC-024: partial parts go through the unchanged `GetReferringDocuments`.
- FC-025 to FC-027, FC-029, FC-030: no token-position or scope logic changed.
- FC-031: the `<returns>` of `TupleTypeKeepsElementName` matches the code. The summaries of `GivesInferredMemberNameAsync` and `TupleTypeKeepsElementName` ("keeps that name in its type where it is used") still read correctly as the tuple's own type (see *Hints*). There is no `TODO`, no tracker number, and no `sdd/work` path in source.
- FC-032: the cleaner's Release build (25) is after the last `src` commit, and that commit is text-only.
- FC-033, FC-034: apply-fix gave the same results under LF and CRLF for every fixture.
- FC-035: the offered expected values were measured on `main` (24). The test inputs match the spec examples.
- FC-036: tuple element names are compile-time only. Anonymous members stay withheld.
- FC-037: the text-only claim for the provider rests on the cleaner's `verify-text-only` PASS.

## Architecture findings

#8 repeats #7 on a member #7 did not name: an offered-side boundary of `TupleTypeKeepsElementName` that RH2001-011 ¶3 states and no test pins. It is a coverage gap with no structural cause in the code. The guard is one owner, and the code matches the spec. The lead decides under `README.md`, *Architecture findings*. Under a refusal of the class, two `Verify` tests close it, and nothing has to be reasoned further.

## Hints

- #9 and #10 are text only and cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. If #8 is the only reason for another round, both can ride along with it.
- The change doc's inferred-name bullet reads "decided by the maintainer for the governing tuple of a `switch` (…): a tuple whose names no code reads, …". There, the list after the colon can read as an expansion of the `switch` case rather than of "everywhere else". It is a readability point, not a contradiction.
- The validator should still confirm the analyzer project's full test count in one run (see 19 and 26).
