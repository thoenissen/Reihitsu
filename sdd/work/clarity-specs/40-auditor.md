Role: auditor
Commit: 2a34ee5f29ecf77b828c0d2f39b810c230063efc
Verdict: findings

## Scope

This audits only the shift-operator change that `squad/decisions/2026-10-20-997-rh3001-shift-operators.md` allowed. It is a delta against 4fd82ad (`34-auditor.md`, which audited 4fc5068). The change covers the commits d63e05b to 2a34ee5: spec author 76a363d and 47b1fad, test implementer f8c19c5, 5fb61fb, and f16d7a0, and cleaner 39. Head is 2a34ee5f29ecf77b828c0d2f39b810c230063efc and base `origin/main` is e84b26c9854b4014c96c30f8b3a549998a5b7396. The lead's message listed the checks to make. It carried no intended fix, suspected cause, or hoped-for verdict.

### State verified

- The checkout is the head SHA, and the tree is clean.
- `git ls-remote origin refs/heads/main` returns e84b26c, which is merged into the head.
- `scripts/lane-check.sh tooling` reports `confirmed`.
- Against e84b26c, no file under `src/` outside the test projects changed.
- Since 4fd82ad, the diff holds exactly what handovers 35–39 and the decision record describe:
  - `sdd/specs/analyzer/RH3001.md`: both changed clauses of RH3001-004;
  - three tests added to `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`;
  - the decision record;
  - `00-lead.md` and handovers 35–39.
- `scripts/test.sh --project architecture` on the head: 72/72 passed.
- The RH3001 test class on the head: 34/34 passed.

### How probes were run

- Fixes ran through `scripts/apply-fix.sh RH3001`, with one negation per fixture wherever a fixture could stop early. LF and CRLF gave the same status and text in every arm.
- Compile results come from the SDK's `csc.dll` (10.0.402) against the net10.0 reference pack, on both the original and the fixed text.
- Fixtures were kept outside the repository.

## Previous findings

| # (34-auditor) | Status | Evidence |
|---|---|---|
| 1 RH3001-004, `>>` and `>>>` | closed | **The spec.** Clause 1 now reads "whose `!` is not the next token after a `>`, `>>`, or `>>>` operator". All three counterexamples are outside the MUST. **The test.** Counterexample 1 is pinned by the untagged `FixOfNegationAfterShiftRightInAnArgumentWritesAGenericReading`. **The filing.** Counterexamples 1 and 2 are filed as inputs 5 and 6 in #1245 "Q1, continued (shift operators)". `=>` stays inside the MUST. Clause 1 holds for every other token probed (see *Guard scope*). |
| 2 #1245 input 4 | closed | The "Q1, continued" comment now carries the full declarations. They are the same text I reproduced in `34-auditor.md`: the original compiles, and the fixed text gives CS0307 and 2× CS0246. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/analyzer/RH3001.md`, RH3001-004, the parentheses sentence ("otherwise, unless the negation is the right operand of an assignment operator other than `=`, it MUST NOT add any") | FC-016, FC-027 | same requirement (the sentence this round narrowed) | **What the narrowing missed.** `47b1fad` exempted only the assignment operators. Other positions get the same unneeded parentheses. In each of them the negation is the whole expression of a construct that is not an assignment operator. The negation is inside the MUST: its parent covers more text, and the `!` follows `=`, `:`, `case`, `select`, `group`, `by`, or `orderby`. `==` binds the same there as `!`, so the sentence says "MUST NOT add any". **The fix adds them, though.** Each original and each fixed text compiles: **named attribute argument:** `[A(P = !true)]` → `[A(P = (true == false))]`; also on a parameter attribute, `void M([A(P = !true)] int p)`. **named attribute argument, colon form:** `[A(b: !true)]` → `[A(b: (true == false))]`. **`goto case`:** `goto case !true;` → `goto case (true == false);`. **query `select`:** `from v in xs select !v` → `select (v == false)`; also `select !v into w`. **query `group`:** `group !v by v` → `group (v == false) by v`, and `group v by !v` → `group v by (v == false)`. **query `orderby` with a direction:** `orderby !v descending` → `orderby (v == false) descending`, and `ascending` gets the same. **Contrast.** In those same positions, a negation that is an `&&` operand gets none: `[A(P = true == false && true)]`, `goto case true == false && true;`, `select v == false && y`. These whole-expression positions also get none: `where`, `let`, `join … on … equals`, `case … when`, a catch filter, named arguments of invocations, object creations, indexers, and constructor initializers. **The cause.** The parentheses come from the Simplifier, which `ApplyCodeFixAsync` asks to decide by `Simplifier.Annotation`. It keeps them in the positions above, as it does after `>>>=` and `??=`. `37-spec-author.md` re-probed every assignment operator but no other position. | The owner re-derives the wording. Two shapes: state the "MUST NOT add any" claim only for the positions where it holds, or keep only the positive "MUST put it in parentheses" claim and leave the absence unclaimed. Add the inputs above to #1245 Q3, which holds the same question for `>>>=` and `??=`. If the lead wants pins, untagged exact-text tests like the two assignment pins of this round. |
| 2 | low | thoenissen/Reihitsu#1245, comment "Q1, continued (shift operators)", input 6 | — | same requirement | **The filed input cannot be compiled as written.** It is described ("Same pattern with `>>>`: `int G, A, H, I; V J; T B` where `T` declares …"), not given as text. Inputs 1–5 are complete. Put together with `V` and `W` from input 5, it is the counterexample 2 of `34-auditor.md`. That original compiles, and the fixed text gives CS0307 and 5× CS0246, as filed. This is the same gap as finding 2 of `34-auditor.md`. | Give input 6 as complete text, like inputs 1–5. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3001-004 clause 1: where the fix compiles | Spec: the token in front of `!` is none of `>`, `>>`, `>>>` | `ApplyCodeFixAsync` writes the `(` where the `!` was. The type-argument scan treats each `>` character of `>`, `>>`, `>>>` as a possible closer. | **Agree.** The three counterexamples of `34-auditor.md` are now excluded. These tokens in front of `!` give fixed text that compiles: `>=`; `>>=` and `>>>=` (also `>>>=` with one `<` open in the same argument list); `??=` with a `<` open; `=>`; `?` after `is List<int>` and after `is int`; `<<`; `<`; `:`; `case`; `when`; `select`; `group`; `by`; `orderby`; `where`; `let`; `on`; `equals`; `=`; `,`; `(`; `{`; `return`. **Not inputs:** `>>>=` with three `<` open, and a relational pattern `is > !K`, because neither original compiles (CS0307/CS0246 and CS8781). Neither are `..` and spread (`34-auditor.md`). | ok |
| RH3001-004 clause 2: where parentheses are required | Spec: the comparison binds differently inside the parent | Simplifier, through `Simplifier.Annotation` | **Agree.** Each of these keeps its parentheses: `y == !x`, `!x is true`, `(object)!x`, `!x switch { … }`, `!x with { }`, `p + !x`, and the left operand of `>>>` (`35-spec-author.md`). `(!x).ToString()` gives `(x == false).ToString()`, with no doubled parentheses. | ok |
| RH3001-004 clause 2: where parentheses are forbidden | Spec: everything else, except the right operand of an assignment operator other than `=` | Simplifier | Disagree in attribute arguments, `goto case`, and query `select`, `group`, and `orderby … ascending/descending`. **The claim's own examples agree.** `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `<<=`, `>>=`, `&=`, `|=`, `^=` get none, and `>>>=` and `??=` get parentheses. Those are inside the exemption, and Q3 and "Q3, continued" describe them correctly. | Finding 1 |
| RH3001-004: whole-node clause | Spec: "covers less text than the syntax node directly around it" | `FindNode` without `getInnermostNodeForTie` | Unchanged since `30-auditor.md`. `orderby !v` and `orderby v, !v` get no fix: an `Ordering` holding only the negation has the same span. With `descending` or `ascending`, the fix is offered. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where RH3001's fix parenthesizes the comparison | `rg "Simplifier\.Annotation" src`: only `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync`, which hands the decision to Roslyn's Simplifier. Spec: RH3001-004 only. | **One owner in code.** The spec's description of that owner is incomplete (finding 1). |
| A rewrite must not create a generic-name reading | **Code.** Unchanged, since no production file changed. **Spec.** RH3001-004 now scopes by `>`, `>>`, `>>>`. RH3005-004, RH3101-004, and RH3105-004 are unchanged. | **The difference is justified** by `31-spec-author.md`, as before. **No code guard yet.** It remains a follow-up candidate. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixOfNegationAfterShiftRightInAnArgumentWritesAGenericReading` | The fix writes exactly `M(G < A, H < B >> (x == false));` | Any other text. `36-test-implementer.md` records a mutation that failed the test. | `Assert.AreEqual(expected, await ApplyCodeFixAsync(source))`: raw whole-document text, no normalization | **Adequate.** It is untagged, correctly, since the input is outside clause 1. **Real.** The input is counterexample 1 of `34-auditor.md` laid out over several lines, and the output matches `scripts/apply-fix.sh` under LF and CRLF. The original compiles. |
| `FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses` | The fix writes exactly `B >>>= (x == false);` | Any other text, for example without the parentheses. A mutation failed the test (`38-test-implementer.md`). | same | **Adequate.** It is untagged. The output now lies in the clause-2 exemption, so the test pins behavior the spec leaves open. **Real.** The original and the fixed text both compile (re-checked), and the output matches `scripts/apply-fix.sh`. The summary is accurate. |
| `FixOfNegationAfterNullCoalescingAssignmentAddsParentheses` | The fix writes exactly `B ??= (x == false);` | same | same | **Adequate.** It is untagged and accurate, on the same grounds as the `>>>=` test. |
| The five RH3001-004-tagged tests | Each tagged input is inside both clauses | A tagged `!` after `>`, `>>`, or `>>>`. A tagged negation that is the right operand of an assignment other than `=`. A tagged position where the fix keeps unneeded parentheses. | — | ok. Every `!` follows `return`, `=` (a declarator), or `==`, and none is in a position of finding 1. |

## Failure classes dismissed

- **FC-001, FC-005, FC-009, FC-018, FC-028, FC-029, FC-036.** Not reachable: no production predicate, fix, or rename changed.
- **FC-002.** Clause 1 now names every token whose characters can close a type argument list (see *Guard scope*).
- **FC-003.** Both sides of clause 1 are tested: five tagged tests inside, and four untagged pins outside (three after `>`, one after `>>`).
- **FC-004, FC-019–FC-021.** No rewrite changed. The changed clauses make no trivia claim.
- **FC-006.** It reaches clause 1. Every reachable input inside the MUST compiled after the fix (see *Guard scope*).
- **FC-007, FC-008, FC-010–FC-014, FC-030.** No fix, Fix All, formatter, or analyzer code changed. Every probe converged: one iteration per negation, and `!!x` meaning-preserving in two.
- **FC-015.** Recorded under *Policy ownership*.
- **FC-017.** The three new tests assert exact whole-document text.
- **FC-022–FC-026.** The change adds no requirement about literals, identifiers, member kinds, or token re-resolution.
- **FC-031.** The only changed source members are the three new tests. Their summaries describe what they pin.
- **FC-032.** The architecture results and the RH3001 class results were re-run on the head.
- **FC-033–FC-035.** The tests copy the filed or probed inputs and the real output. They run the code-fix path, and the exact-text assertion does not normalize line endings.
- **FC-037.** Not reachable: no comment-only claim about a `.cs` file.

## Hints

- **The decision record names less than the round did.** `squad/decisions/2026-10-20-997-rh3001-shift-operators.md` answers only the `>>`/`>>>` exclusion, the pin, and the filings. It does not name the narrowing of the parentheses sentence in 47b1fad. `00-lead.md` records only the shift exclusion. Whether that narrowing needs its own record is the lead's call.
- **The Q3 contrast list holds.** "Q3, continued" lists `=`, `+=`, `<<=`, `>>=`, `&=`, `|=`, `^=` "and the other compound assignments" as getting no parentheses. `-=`, `*=`, `/=`, and `%=` also get none, so that matches.
- **Leftover.** `sdd/work/clarity-specs/` is the only leftover, and the lead deletes it before publishing.
