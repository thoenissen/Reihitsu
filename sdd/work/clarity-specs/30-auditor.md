Role: auditor
Commit: fb3a5640e4b00bdd7005cc2a6f19431284c0eb39
Verdict: findings

## Scope

This audits only the second repair round, as a delta against 2e0bffa (`20-auditor.md`, which audited afd241d). Head is fb3a5640e4b00bdd7005cc2a6f19431284c0eb39 and base `origin/main` is e84b26c9854b4014c96c30f8b3a549998a5b7396. The lead's message carried the task and the checks to make; it carried no intended fix, suspected cause, or hoped-for verdict.

### State verified

- The checkout is the head SHA, and the tree is clean.
- `git ls-remote origin refs/heads/main` returns e84b26c, which is merged into the head.
- `scripts/lane-check.sh tooling` reports `confirmed`.

### What the round changed

Read from the round's own commits (c685b96 to fb3a564), not counting the merge:

- **Specs.** Six specs changed: RH3001, RH3002, RH3005, RH3101, RH3103, RH3105.
- **Tests.** Six test classes under `src/Reihitsu.Analyzer.Test/Clarity/` (RH3001, RH3002, RH3005, RH3101, RH3103, RH3105).
  - The only `[Requirement]` lines added are `RH3002-001` and `RH3002-007`, on the five new RH3002 position tests.
  - The 21 new characterization tests are untagged.
  - The only removed test lines are the two summary lines of finding 7.
- **Records.** One decision record, plus the lead's and the roles' handovers 21–29.

### Merge c2d778e

The merge touched this run's paths only in the ledger and the index.

- Against e84b26c, the ledger adds exactly this run's 78 IDs and removes none. It is sorted (`LC_ALL=C sort -c`) and holds no duplicate.
- The index adds this run's 25 rows and changes the RH3202 row.
- `scripts/test.sh --project architecture` on the head: 72/72 passed.

### No production change

Against e84b26c, the diff holds only:

- `documentation/rules/RH3201.md`;
- `sdd/specs`;
- `sdd/work`;
- `squad/decisions`;
- 23 files under `src/Reihitsu.Analyzer.Test`.

The repair commits touch no other `src/` path.

### Issues read back from GitHub

- thoenissen/Reihitsu#1245 exists with Q1 (P1, two inputs) and Q2 (five inputs).
- #1245 carries the "Q2, continued" comment with the three further inputs.
- Each of these issues carries its comment: #1231 (Q2), #1227 (Q2), #1233 (Q2), #1229 (Q3).
- Their inputs, fixed texts, and error codes match `21-spec-author.md` and `25-spec-author.md`.

### How probes were run

- Fixes ran through `scripts/apply-fix.sh` (LF and CRLF, same result in both).
- Compile results come from the SDK's `csc.dll` against the net10.0 reference pack, on the original and the fixed text.

## Previous findings

| # (20-auditor) | Status | Evidence |
|---|---|---|
| 1 RH3103-002 / RH3103-001 | closed | RH3103-002 now covers only the unqualified argument of `nameof`. RH3103-001's false sentence is gone. RH3103-003 is scoped to "a position of RH3103-001" and no longer names the alias-only form. The dotted `nameof` forms are filed in #1231 Q2 and pinned by two untagged analyzer-only tests (`Dotted…` and `GlobalDotted…AsNameofArgumentIsReportedOnTheGenericName`), which never apply the throwing fix. |
| 2 RH3005-004 | closed | The new condition: "the largest expression that contains the comparison … holds no `<` or `>` operator besides the comparison's own". The fix adds one `<` or `>` token, and a `<`/`>` belonging to a type argument list closes itself. So a generic reading needs a second operator inside that expression. In-scope probes with type-argument brackets compile: `M(G < Max, F<int>(7))` and `M(G < Max, F<List<int>>(7))`. The three inputs are in #1227 Q2 and pinned by untagged `ApplyCodeFixAsync` exact-text tests. |
| 3 RH3001-004 | **open** | The narrowing is incomplete. See finding 1 below. The #1245 Q1 inputs are filed and pinned, and Q2 with its continuation is filed and pinned by eight untagged `VerifyNoCodeFixIsOffered` tests. The "covers less text than the syntax node directly around it" clause matches `FindNode` without `getInnermostNodeForTie`: it climbs through ancestors of equal full width, and parent and child have equal full width exactly when they cover the same tokens. |
| 4 RH3101-004 / RH3105-004 | closed | The condition "the largest expression that contains the access does not hold both a `<` operator and a `>` operator" covers the hazard: the fix adds no `<` or `>` token. In-scope probes compile: `M(A < B, F<int>(7))` and `M(A < B, F<List<int>>(7))`. The cases are in #1229 Q3 and #1233 Q2, pinned by six untagged exact-text tests. |
| 5 RH3002-001 positions | closed | Five tagged tests (RH3002-001, RH3002-007, `Verify` with exact fixed text) cover an object-creation argument, an element-access argument, a field and a property `=` value, a local-function body, and the left side of an assignment. With `ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition`, every position RH3002-001 lists now has a tagged test. |
| 6 RH3002-001 wording | closed | Now reads "directly inside another parenthesized expression". |
| 7 RH3103 test summary | closed | Now reads "qualified with dots (also behind `global::`)"; tags and assertion unchanged. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/analyzer/RH3001.md`, RH3001-004 ("… and that is not the right operand of a `>` operator") | FC-002, FC-006, FC-016 | same requirement (finding 3 of `20-auditor.md`, not closed) | **What is unsafe.** The fix writes a `(` directly after the token in front of the negation. That breaks the code whenever that token is a `>`, which happens whenever the negation *starts* the right operand of `>`. The new condition excludes only a negation that *is* that operand. **The counterexample needs no user-defined operator.** This input compiles:<br>`class C { int G; int A; int B; void M(bool x, bool y) { } void Run(bool x) { M(G < A, B > !x switch { true => 1, false => 0 }); } }`<br>The negation is the governing expression of the `switch` expression, so its parent covers more text, and it is not itself the right operand of `>`. That puts it inside the narrowed MUST. The fix writes `M(G < A, B > (x == false) switch { true => 1, false => 0 });`, which parses as `G<A, B>(x == false)`. csc reports `CS0307: The field 'C.G' cannot be used with type arguments` and `CS0246` for `A` and `B`. **A second instance.** With a user-defined `operator +(bool, T)` and `operator >(V, T)`, `M(G < A, B > !x + t);` becomes `M(G < A, B > (x == false) + t);`, with the same errors. **Not filed or tested.** Neither input is in #1245 Q1, and no test pins it. | Make the condition cover every negation that a `>` token directly precedes. One way is to state it by that token rather than by the operand role. Another is to reuse the largest-expression condition of RH3005-004 and RH3101-004. The owner re-derives the wording. Add both inputs to #1245 Q1 (P1, compile error) and pin at least the `switch` input with an untagged `ApplyCodeFixAsync` exact-text test. |

## Guard scope

Each repaired requirement's new scope compared with the code that decides it.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3001-004: where the fix is offered | Spec: "covers less text than the syntax node directly around it" | `RegisterCodeFixesAsync`: `root.FindNode(diagnostic span)` must be the `PrefixUnaryExpressionSyntax` (tie-break climbs while `FullWidth` is equal) | Agree. The eight Q2 pins and the contrast inputs of `25-spec-author.md` sort correctly. | ok |
| RH3001-004: where the fix is safe | Spec: not the right operand of `>` | The hazard is the `(` the Simplifier keeps right after a `>` token | Disagree when the negation starts the right operand (as a `switch` governing expression or the left operand of a tighter binary operator) | Finding 1 |
| RH3005-004 | Spec: no other `<`/`>` operator in the largest containing expression | The fix adds one `<` or `>` token; type-argument brackets close themselves | Agree (probed with type-argument brackets; `>=` and `>>` were compiled clean by `24-spec-checker.md`) | ok |
| RH3101-004, RH3105-004 | Spec: not both a `<` and a `>` operator in the largest containing expression | Removing `this.` or `base.` adds no token. A generic reading needs an unclosed `<` before the name and a `>` after it. | Agree | ok |
| RH3103-002/-003 | Spec: unqualified `nameof` argument; diagnostic span in RH3103-001 positions | `ShouldSkip` (walks qualified-name parents only); location is the parent `QualifiedNameSyntax` when the generic name is its `Right` | Agree. In the listed positions a dotted name is a `QualifiedNameSyntax`. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A rewrite must not create a generic-name reading | Code: still only the RH3002 analyzer (no production change in this lane). Spec: each of RH3001-004, RH3005-004, RH3101-004, and RH3105-004 now scopes its own MUST. | The code side stays a follow-up candidate, as `21-spec-author.md` lists it. The spec scopes are correct except RH3001-004 (finding 1). |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Five new RH3002 position tests | Each listed position is reported over the pair and fixed to the bare content | A missing report, a wrong span, or other output | `Verify(source, fixed)` | Adequate; tagged RH3002-001 and RH3002-007, both of which they verify |
| 11 RH3005 / RH3001-Q1 / RH3101 / RH3105 characterization tests | The exact current fixed text | Any other text | `Assert.AreEqual(expected, await ApplyCodeFixAsync(...))`, whole document, no normalization | Adequate as characterization. Untagged, correctly: the inputs lie outside the narrowed MUSTs. |
| Eight RH3001 Q2 / "Q2, continued" tests | Reported on `!`, and no action for any diagnostic | An action offered, or a missing report | `VerifyNoCodeFixIsOffered` | Adequate; untagged |
| Two RH3103 dotted `nameof` tests | Reported on `Nullable<int>` alone | A wider span, or no report | Analyzer-only `Verify(source)`; the throwing fix is never applied | Adequate. `22-test-implementer.md` records a mutation that made the assertion fail. |
| Existing tags against the narrowed text (RH3001-004, RH3005-004, RH3101-004, RH3105-004, RH3103-002/-003) | Each tagged input lies inside the new condition | A tagged input in the excluded region | — | Checked by `22-test-implementer.md`. I re-checked RH3005-004 and RH3101/RH3105-004: no other `<`/`>` operator in their bodies (RH3005) and none at all (RH3101/RH3105). ok |

## Failure classes dismissed

- **FC-001, FC-005, FC-009, FC-018, FC-028, FC-029, FC-036.** Not reachable: no production predicate, fix, or rename changed.
- **FC-004, FC-019–FC-021.** No rewrite changed, and the round adds no trivia claim.
- **FC-007, FC-008, FC-010–FC-014.** No fix, Fix All, formatter, or analyzer code changed.
- **FC-015.** Recorded under *Policy ownership*.
- **FC-017.** Every new test asserts exact output or the full no-fix contract (see *Assertion adequacy*).
- **FC-022–FC-025, FC-030.** The round adds no requirement about literals, identifiers, member kinds, or fix scope.
- **FC-026.** Not reachable: no edit re-resolves tokens.
- **FC-027.** It reaches RH3001-004 (finding 1). The other narrowed scopes hold in arguments, array initializers, and chains (probed).
- **FC-031.** The changed summary in RH3103 matches the test. The new tests' summaries describe what they pin.
- **FC-032.** No production file changed in the round or the merge. I re-ran the architecture tests on the head after the merge.
- **FC-033–FC-035.** The characterization tests copy the filed inputs and the real output. Exact-text tests do not normalize line endings.
- **FC-037.** Not reachable: no comment-only claim about a `.cs` file.

## Hints

- **Similar shapes for the owner to sort with the same condition** (not probed): RH3001's other leftmost positions inside a `>` right operand, such as the left operand of a user-defined `*`, `<<`, or `..`.
- **Wording inside the scope for the alias-only `nameof` form.** `nameof(S::Nullable<int>)` does not compile (CS8083, per `21-spec-author.md`), so dropping it from RH3103-002 leaves no valid input unstated.
- **Leftover.** `sdd/work/clarity-specs/` is the only leftover, and the lead deletes it before publishing.
