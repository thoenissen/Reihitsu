Role: auditor
Commit: 4fc5068f47739ddc751614c6c67806e39152a947
Verdict: findings

## Scope

This audits only the targeted RH3001-004 change that `squad/decisions/2026-10-20-997-rh3001-targeted-fix.md` allowed. It is a delta against 167d9c0 (`30-auditor.md`, which audited fb3a564). Head is 4fc5068f47739ddc751614c6c67806e39152a947 and base `origin/main` is e84b26c9854b4014c96c30f8b3a549998a5b7396. The lead's message listed checks and candidate operators to try. It carried no intended fix, suspected cause, or hoped-for verdict.

### State verified

- The checkout is the head SHA, and the tree is clean.
- `git ls-remote origin refs/heads/main` returns e84b26c, which is merged into the head.
- `scripts/lane-check.sh tooling` reports `confirmed`.
- Against e84b26c, no file under `src/` outside the test projects changed.
- Since 167d9c0, the diff holds exactly what handovers 31–33 and the decision record describe:
  - `sdd/specs/analyzer/RH3001.md`: one sentence of RH3001-004;
  - one test added to `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`;
  - the decision record;
  - `00-lead.md` and handovers 31–33.
- `scripts/test.sh --project architecture` on the head: 72/72 passed.
- The RH3001 test class on the head: 31/31 passed.

### How probes were run

- Fixes ran through `scripts/apply-fix.sh RH3001`. Every probe below was `fixed (1 action, 1 iteration)` under LF and CRLF, with the same text in both.
- Compile results come from the SDK's `csc.dll` (10.0.402) against the net10.0 reference pack, on both the original and the fixed text.
- Fixtures were kept outside the repository.

## Previous findings

| # (30-auditor) | Status | Evidence |
|---|---|---|
| 1 RH3001-004 | **open** | **What the restatement closes.** It now excludes every negation whose `!` comes directly after the `>` *operator*. That covers the `switch`, `+`, and `*` shapes. **What it leaves open.** The same hazard comes from the shift tokens `>>` and `>>>`. A negation after either of them is still inside the MUST. See finding 1. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/analyzer/RH3001.md`, RH3001-004 ("whose `!` is not the next token after a `>` operator") | FC-002, FC-016, FC-006 | same requirement (finding 1 of `30-auditor.md`, not closed) | **What is unsafe.** The fix writes `(` exactly where the `!` was. Roslyn lexes every `>` on its own and merges `> >` and `> > >` into a shift operator only afterwards. So when it scans for a type argument list, each `>` of a `>>` or `>>>` can close one. A `(` directly after `>>` or `>>>` therefore completes a generic reading whenever enough `<` are open in front of it: two for `>>`, three for `>>>`. **The new condition misses it.** `>>` and `>>>` are shift operators, not the `>` operator. So their negations stay inside the narrowed MUST. **Counterexample 1** (compiles):<br>`class V { public static bool operator <(V a, W b) => true; public static bool operator >(V a, W b) => true; } class W { } class T { public static W operator >>(T a, bool b) => new W(); } class C { int G; int A; V H; T B; void M(bool p, bool q) { } void Run(bool x) { M(G < A, H < B >> !x); } }`<br>The fix writes `M(G < A, H < B >> (x == false));`, which parses as `G<A, H<B>>(x == false)`. csc reports CS0307 ("The field 'C.G' cannot be used with type arguments") and CS0246 for `A`, `H<>`, and `B`. **Counterexample 2** (compiles): `>>>` with `int G, A, H, I; V J; T B` (`T` declares `operator >>>(T, bool)` returning `W`) and `M(bool, bool, bool)`, called as `M(G < A, H < I, J < B >>> !x);`. It is fixed to `M(G < A, H < I, J < B >>> (x == false));`, which reads `G<A, H<I, J<B>>>(x == false)`: CS0307 and 5× CS0246. **Counterexample 3** (a chain, compiles): `return G < A < B >> !x;`, with `V` declaring `<`/`>`(V, V) and `T` declaring `>>`(T, bool) returning `V`. It is fixed to `return G < A < B >> (x == false);`: CS0307 and 2× CS0246. **Contrast.** Two inputs compile after the fix because fewer `<` are open: `M(G < A, B >> !x)` (one `<` before `>>`) and `M(G < A, H < B >>> !x)` (two before `>>>`). The `>>` probe in `31-spec-author.md` is the one-`<` form, so it compiled and does not support the claim drawn from it, "the condition must name the `>` operator only". **Every case needs a user-defined shift operator with a `bool` right operand** (C# 11 or later). **Not filed or tested.** None of the three inputs is in #1245 Q1, and no test pins them. | Exclude every token that ends in `>` and can close a type argument list: `>`, `>>`, and `>>>`. One wording is "whose `!` does not directly follow a `>`, `>>`, or `>>>` token". `=>` must stay inside the MUST: it cannot close a type argument list, and its fixes compile (`x => !x`, and `M(G < A, x => !x switch { … })` below). The owner re-derives the wording. Add counterexamples 1 and 2 to #1245 Q1 as P1 instances (compile error), and pin at least counterexample 1 with an untagged `ApplyCodeFixAsync` exact-text test. |
| 2 | low | thoenissen/Reihitsu#1245, comment "Q1, continued", input 4 | — | same requirement | **The filed input cannot be compiled as written.** Input 4 is given only as "Same shape with user-defined `+` and `>` operators: `M(G < A, B > !x + t)`", with no declarations. Inputs 1–3 of Q1 are complete. I reproduced it with these declarations: `class T { public static T operator +(bool a, T b) => b; } class V { public static bool operator >(V a, T b) => true; public static bool operator <(V a, T b) => true; } class C { int G; int A; V B; void M(bool x, bool y) { } void Run(bool x, T t) { M(G < A, B > !x + t); } }`. The original compiles, and the fixed `M(G < A, B > (x == false) + t);` gives CS0307 and 2× CS0246, as filed. | Edit the comment so input 4 carries its declarations, like inputs 1–3. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3001-004: where the fix is safe | Spec: the token in front of `!` is not the `>` operator | `ApplyCodeFixAsync`: the new `(` takes the negation's leading trivia, so it follows the token that was in front of `!`. The Simplifier keeps it whenever that token is a relational or shift operator, because `==` binds more weakly. The parser's type-argument scan treats every `>` character of `>`, `>>`, `>>>` as a possible closer. | Agree for `>`. Disagree for `>>` and `>>>`. | Finding 1 |
| RH3001-004: other tokens in front of `!` | Spec: inside the MUST | Same counterpart | **Agree.** Probed; the fixed text compiled where the original did: `>=` (nested after two `<`), `<<` with a later `>`, `<` with a later `> (`, `?` and `:` of a conditional (also after `is string` and `is bool`), `on`/`equals` of a join, `when (`, a cast `(G<int>)`, the left operand of a user-defined `>`, and `=>` (`M(G < A, x => !x switch { … })`). `..` cannot be reached: `a[!x..]` (CS0029) and `[.. !x]` (CS9212) do not compile. The ten fixed inputs of `25-spec-author.md` follow `{`, `,`, `=>`, `case`, `=`, `return`, or `:`. | ok |
| RH3001-004: where the fix is offered | Spec: "covers less text than the syntax node directly around it" | `RegisterCodeFixesAsync`: `FindNode` without `getInnermostNodeForTie` | Unchanged since `30-auditor.md`; that evidence is byte-identical | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A rewrite must not create a generic-name reading | **Code.** Unchanged since `30-auditor.md`. No production file changed in this round, so the owners found there still hold: the RH3002 analyzer in code, and no guard in the RH3001 provider. **Spec.** RH3001-004 now scopes its MUST by the token in front of `!`. RH3005-004, RH3101-004, and RH3105-004 keep the largest-expression condition. | **Different conditions.** The spec scopes still differ in form, and the round justifies that. `31-spec-author.md` shows why: the largest-expression condition does not reach `M(G < A, B > !x switch { … })`, because the `<` sits in another argument. **Remaining gap.** The token condition is incomplete (finding 1). The code side stays a follow-up candidate. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixOfNegationStartingASwitchAfterGreaterThanInAnArgumentWritesAGenericReading` | The fix of `M(G < A, B > !x switch { true => 1, false => 0 });` writes exactly `M(G < A, B > (x == false) switch { true => 1, false => 0 });` | Any other text. `32-test-implementer.md` records a mutation to `(!x) switch` that failed the test. | `Assert.AreEqual(expected, await ApplyCodeFixAsync(source))`. The helper applies the first action for the one diagnostic and returns the raw document text, with no normalization. | **Adequate.** It carries no `[Requirement]`, correctly, since the input is outside the restated MUST. **Real output.** `scripts/apply-fix.sh` gives the same text under LF and CRLF. **Real input.** The original compiles with csc, and the input is identical to input 3 of #1245 "Q1, continued". **Summary.** It describes what the test pins. |
| The five RH3001-004-tagged tests (`NotOperatorOnLiteral`, `…OnField`, `…OnProperty`, `…OnMethodCall`, `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently`) | Each tagged input lies inside the restated condition | A tagged `!` directly after a `>` | — | ok. Every `!` follows `return`, `=`, or `==`. The three untagged Q1 tests have their `!` after `>`, outside the condition. |

## Failure classes dismissed

- **FC-001, FC-005, FC-009, FC-018, FC-028, FC-029, FC-036.** Not reachable: no production predicate, fix, or rename changed.
- **FC-003.** The spec change adds no tested boundary. Tests on both sides already exist: five tagged inside, and three untagged Q1 tests outside.
- **FC-004, FC-019–FC-021.** No rewrite changed. The restated sentence makes no trivia claim, and RH3001-005/-006 are untouched.
- **FC-007, FC-008, FC-010–FC-014, FC-030.** No fix, Fix All, formatter, or analyzer code changed. Every probe converged in one iteration.
- **FC-015.** Recorded under *Policy ownership*.
- **FC-017.** The new test asserts exact whole-document text (see *Assertion adequacy*).
- **FC-022–FC-026.** The change adds no requirement about literals, identifiers, member kinds, or token re-resolution.
- **FC-027.** It reaches RH3001-004. I probed arguments, comparison chains, lambdas, conditionals, query clauses, catch filters, and casts. The only failures are finding 1.
- **FC-031.** The one changed source member is the new test. Its summary matches what it pins.
- **FC-032.** The architecture results and the RH3001 class results were re-run on the head.
- **FC-033–FC-035.** The test copies the filed input and the real output. It runs the code-fix path, and the exact-text assertion does not normalize line endings.
- **FC-037.** Not reachable: no comment-only claim about a `.cs` file.

## Hints

- **The `*` input from `31-spec-author.md` is not filed.** It is `M(G < A, B > !x * t)`. It is the same mechanism as the filed `+` input, and the spec author left adding it to the lead.
- **`return b > !x switch { … }` with no `<` before it** is now outside the MUST although its fix compiles. Keeping a harmless input out of a MUST makes no false claim, so this is not a finding.
- **Leftover.** `sdd/work/clarity-specs/` is the only leftover, and the lead deletes it before publishing.
