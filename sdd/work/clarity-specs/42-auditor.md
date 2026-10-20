Role: auditor
Commit: b4b7cb9f40e76430e773408275a07541d7017acf
Verdict: findings

## Scope

This audits only the change that `squad/decisions/2026-10-20-997-rh3001-parentheses-claim.md` allowed. It is a delta against 4f81e4d (`40-auditor.md`, which audited 2a34ee5). The change is the commits 54e32bf (decision record, `00-lead.md`) and b4b7cb9 (spec author, `41-spec-author.md`), plus the lead's edits on thoenissen/Reihitsu#1245. Head is b4b7cb9f40e76430e773408275a07541d7017acf and base `origin/main` is e84b26c9854b4014c96c30f8b3a549998a5b7396. The lead's message listed the checks to make. It carried no intended fix, suspected cause, or hoped-for verdict.

### State verified

- The checkout is the head SHA, and the tree is clean.
- `git ls-remote origin refs/heads/main` returns e84b26c, which is merged into the head.
- `scripts/lane-check.sh tooling` reports `confirmed`.
- Against e84b26c, no file under `src/` outside the test projects changed. No `.cs` file changed since 4f81e4d, so the RH3001 class result of `40-auditor.md` (34/34 on 2a34ee5) still holds for the code. `41-spec-author.md` reports 34/34 on 54e32bf.
- Since 4f81e4d, the diff holds exactly what the decision record and `41-spec-author.md` describe:
  - `sdd/specs/analyzer/RH3001.md`: one sentence of RH3001-004;
  - the decision record;
  - `00-lead.md` and `41-spec-author.md`.
- `scripts/test.sh --project architecture` on the head: 72/72 passed.

### How probes were run

- Fixes ran through `scripts/apply-fix.sh RH3001`, one negation per fixture. LF and CRLF gave the same status and text in every arm.
- Fixtures were kept outside the repository.

## Previous findings

| # (40-auditor) | Status | Evidence |
|---|---|---|
| 1 RH3001-004, parentheses sentence | closed | **The spec.** The negative claim is gone, and with it the assignment-operator exemption. RH3001-004 now ends: "When the comparison binds differently from the negation inside its parent, the code fix MUST put it in parentheses. An operand that is already parenthesized keeps its parentheses." The spec has no other "MUST NOT add", excluded-case, or open-question wording. **The filing.** The positions are filed on #1245 as "Q3, continued (other positions)". The table and the contrast list match `40-auditor.md`. |
| 2 #1245 input 6 | closed | Input 6 of "Q1, continued (shift operators)" is now complete text. It is identical to counterexample 2 of `34-auditor.md`, which compiles and gives CS0307 and 5× CS0246 after the fix, as filed. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Analyzer.Test/Clarity/RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`, `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently` (tagged RH3001-004): its summary at line 507 and its name | FC-031 | PR-introduced (e6caef6) | **What the summary says.** "Verifies the fix adds parentheses only where the comparison would bind differently than the negation, …". The name says the same. **Why that is false.** Read generally, it is the claim this round dropped from RH3001-004, because it does not hold: #1245 Q3 and its two continuations list the positions with unneeded parentheses. Two untagged tests in the same class pin exactly such output: `B >>>= (x == false);` and `B ??= (x == false);`. **What the test verifies.** Its assertions are right, and its tag still fits. It checks parentheses for an `==` operand, none for an `&&` operand, and the operand's own parentheses kept. Only its description claims more than the test and the requirement establish. `41-spec-author.md` noted the name for the test implementer's lane, and the change left it. | **Summary.** State it for the test's own inputs: parentheses for the right operand of `==`, none for an `&&` operand, and the operand's own parentheses kept. **Name.** Consider dropping "Only" from it. **Not text cleanup.** This is a doc comment on a public test method, which `verify-text-only.sh --strict-docs` counts as public API documentation. So it cannot be cleared as `pass, text cleanup`. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3001-004: the replacement keeps the operand as written | Spec: "replace `!x` with `x == false`" | `ApplyCodeFixAsync`: `Simplifier.Annotation` on the whole replacement, so the Simplifier could reduce inside the operand | **Agree.** No name, cast, escaping, `this.`, or extension-method reduction happens inside the operand. `!this.x` → `this.x == false`, and these keep their operand unchanged: `!System.String.IsNullOrEmpty(s)`, `!global::System.Boolean.Parse("true")`, `!(bool)x`, `!@b`, `!C.SP`, `!System.Linq.Enumerable.Any(xs)`. | ok |
| RH3001-004: the operand's own parentheses are kept | Spec: "An operand that is already parenthesized keeps its parentheses" | Simplifier (parenthesized-expression reduction) | **Agree.** `!(x)` → `(x) == false`; `!((x))` → `((x)) == false`; `!(this.x)` → `(this.x) == false`; `!(o is bool)` → `(o is bool) == false`; `!(b) && y` → `(b) == false && y`; `!(x) ? y : x` → `(x) == false ? y : x`; and `b == !(x)` → `b == ((x) == false)`. | ok |
| RH3001-004: parentheses where the binding differs | Spec: "When the comparison binds differently … MUST put it in parentheses" | Simplifier | **Agree. No counterexample.** `b != !x` → `b != (x == false)`, and the set of `40-auditor.md` holds: `y == !x`, `!x is true`, `(object)!x`, `!x switch { … }`, `!x with { }`, `p + !x`, and the left operand of `>>>` (`35-spec-author.md`). **Not probed:** an inner negation of `!!x` fixed first, because the runner always fixes the outer diagnostic first. The cast case covers a negation as the operand of a unary-level construct. | ok |
| RH3001-004: where the fix is offered and compiles | Spec: whole-node clause and the `>`, `>>`, `>>>` token clause | `FindNode`; the type-argument scan | Unchanged since `40-auditor.md`; that evidence still applies to the same code | ok |
| RH3001-004 and RH3001-006 | -004's MUST covers every negation in its scope; -006 withholds the fix across a directive | `HasSafeOperandTrivia` | Unchanged. `sdd/specs/shared/code-fixes.md` has a rule spec state both what its fix produces and when it is withheld, so the two read together | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where RH3001's fix parenthesizes the comparison | Code: only `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync` (`Simplifier.Annotation`), unchanged since `40-auditor.md`. Spec: RH3001-004 now states only the positive claim. | One owner in code. The spec's description of it is now true. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The five RH3001-004-tagged tests | **`NotOperatorOnLiteral`, `NotOperatorOnField`, `NotOperatorOnProperty`, `NotOperatorOnMethodCall`:** the replacement. **`FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently`:** the positive parentheses claim (`b == (a == false)`) and the kept operand parentheses (`(a && b) == false`). | Any other fixed text | `Verify(source, fixed)` | **The tags fit.** Every tagged test verifies a claim RH3001-004 still makes. **The extra line.** The `&&` line of the parentheses test pins output beyond the requirement, which a test may do. **One gap.** That test's summary and name overclaim (finding 1). **The example.** RH3001-004's example is that test's input and output, so it is the real output. |
| The six untagged RH3001 pins of earlier rounds | Unchanged | — | — | Still correctly untagged. Four lie outside clause 1: three after `>`, one after `>>`. The `>>>=` and `??=` pins are now covered by no claim of RH3001-004. |

## Failure classes dismissed

- **FC-001–FC-003, FC-005, FC-009, FC-016, FC-018, FC-028, FC-029, FC-036.** Not reachable: no predicate, guard, fix, or rename changed. The change only removed a claim.
- **FC-004, FC-019–FC-021.** No rewrite changed, and no trivia claim changed.
- **FC-006.** The change makes no compile claim. Clause 1 is unchanged since `40-auditor.md`.
- **FC-007, FC-008, FC-010–FC-014, FC-030.** No fix, Fix All, formatter, or analyzer code changed.
- **FC-015.** Recorded under *Policy ownership*.
- **FC-017.** No test changed. The existing tests assert exact output.
- **FC-022–FC-027.** The change adds no requirement and no input class. The operand probes above cover verbatim (`@b`), qualified, `global::`, cast, and parenthesized operands for the remaining claims.
- **FC-032.** The architecture results were re-run on the head. No compiled file changed since the RH3001 class last ran.
- **FC-033–FC-035, FC-037.** No test or `.cs` comment changed in this change.

## Hints

- **The decision record.** `squad/decisions/2026-10-20-997-rh3001-parentheses-claim.md` follows the format. `00-lead.md` now records the earlier assignment-operator narrowing (47b1fad), which closes the hint of `40-auditor.md`.
- **Leftover.** `sdd/work/clarity-specs/` is the only leftover, and the lead deletes it before publishing.
