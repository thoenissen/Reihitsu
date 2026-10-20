Role: spec author
Commit: d63e05b4a66e846a6def710bdd33d23a40b73178
Verdict: spec written

This is a delta to `31-spec-author.md`. It covers finding 1 of `34-auditor.md`, as `squad/decisions/2026-10-20-997-rh3001-shift-operators.md` directs. The lead's message named the tokens to exclude; that is the decision record's answer, so I took it as the scope and checked it against the code and the probes. Nothing else changed: no other requirement, no rule page, no ID, no test.

## Changed requirements

### RH3001-004 — clarified

Before: "… and whose `!` is not the next token after a `>` operator, the code fix MUST replace …"

After: "… and whose `!` is not the next token after a `>`, `>>`, or `>>>` operator, the code fix MUST replace …"

The whole-node clause and the rest of the requirement are unchanged.

**How the condition was derived.** The derivation in `31-spec-author.md` still holds. `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync` gives the new `(` the negation's leading trivia, so the `(` sits exactly where the `!` was. After a relational or shift operator the Simplifier keeps the parentheses, because `==` binds more weakly. The parser treats each `>` character of `>`, `>>`, and `>>>` as a possible end of a type argument list. So a `(` written directly after any of those three tokens can complete a generic reading when enough `<` are open in front of it.

- In the syntax tree, `>>` and `>>>` are single operator tokens (`GreaterThanGreaterThanToken`, `GreaterThanGreaterThanGreaterThanToken`). "The next token after a `>>` operator" therefore names one position.
- `=>` is not a `>` operator token and stays inside the MUST. Its fixes compile even with two `<` open (probe n4).
- `>=`, `>>=`, and `>>>=` end in `=`. They stay inside the MUST. Their fixes compile (probes p31e, a1, a2). With `<` open in an argument list, `M(G < A, B >>= !x)` and the `H < I` variants with `>>=` or `>>>=` do not compile as originals: CS0307 and CS0246. So they are not inputs.

**The 31 claim is corrected.** `31-spec-author.md` concluded from `M(G < A, B >> !x)` that the condition must name the `>` operator only. That probe has one `<` open, and two are needed for `>>`. It compiles after the fix (k1 below), but it does not prove `>>` safe.

## Probes

Fixes ran with `scripts/apply-fix.sh RH3001` on a fixture directory outside the repository. Every row has the same status and the same fixed text under LF and CRLF. Compile results come from the SDK's `csc.dll` (10.0.402) against the net10.0 reference pack (`-langversion:latest`). Every original compiles.

| # | Input (inside a method) | Fixed text | Fixed compiles | In the new MUST |
|---|---|---|---|---|
| c1 | `M(G < A, H < B >> !x)` (34-auditor counterexample 1) | `M(G < A, H < B >> (x == false))` | no: CS0307, 3× CS0246 | no |
| c2 | `M(G < A, H < I, J < B >>> !x)` (counterexample 2) | `M(G < A, H < I, J < B >>> (x == false))` | no: CS0307, 5× CS0246 | no |
| c3 | `return G < A < B >> !x;` (counterexample 3; `V` declares `<`/`>`(V, V) returning `V`) | `return G < A < B >> (x == false);` | no: CS0307, 2× CS0246 | no |
| k1 | `M(G < A, B >> !x)` (one `<`) | `M(G < A, B >> (x == false))` | yes | no (excluded but harmless) |
| k2 | `M(G < A, H < B >>> !x)` (two `<`) | `M(G < A, H < B >>> (x == false))` | yes | no (excluded but harmless) |
| n4 | `M(G < A, H < I, x => !x)` | `M(G < A, H < I, x => x == false)` | yes | yes |
| a1 | `B >>>= !x;` (user-defined `>>>`(T, bool)) | `B >>>= (x == false);` | yes | yes, see *Open questions to file* |
| a2 | `B >>= !x;` | `B >>= x == false;` | yes | yes |
| a3 | `B <<= !x;` | `B <<= x == false;` | yes | yes |
| a4 | `B &= !x;` (`bool B`) | `B &= x == false;` | yes | yes |
| p31a | `M(G < A, B > !x switch { true => 1, false => 0 })` | `M(G < A, B > (x == false) switch { … })` | no: CS0307, 2× CS0246 | no |
| p31b | `M(G < A, B > !x + t)` | `M(G < A, B > (x == false) + t)` | no: CS0307, 2× CS0246 | no |
| p31c | `M(G < A, B > !x * t)` | `M(G < A, B > (x == false) * t)` | no: CS0307, 2× CS0246 | no |
| p31d | `return b > !x switch { true => 1, false => 0 };` | `return b > (x == false) switch { … };` | yes | no (excluded but harmless) |
| p31e | `M(G < A, B >= !x)` | `M(G < A, B >= (x == false))` | yes | yes |
| p31f | `x => !x`, `x => !x switch { … }` | `x => x == false`, `x => (x == false) switch { … }` | yes | yes |

The 13 inputs of `25-spec-author.md` were re-run with the same results. `[!x, y]`, `x is !true`, and `x switch { !true => 1, … }` report the negation and offer no fix. The ten others are fixed, and every fixed text compiles: `new bool[] { !x }`, `[y, !x && y]`, `true => !y`, `case !true:`, `$"{!x}"`, `new { A = !x }`, `new P { A = !x }`, `x => !x`, `yield return !x`, and `M(x: !y)`. None of the ten has a `>`, `>>`, or `>>>` token directly before the `!`. They follow `{`, `,`, `=>`, `case`, `=`, `return`, or `:`.

**Tests.** The five RH3001-004-tagged tests stay inside the condition. They are `NotOperatorOnLiteral`, `NotOperatorOnField`, `NotOperatorOnProperty`, `NotOperatorOnMethodCall`, and `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently`, and their `!` follows `return`, `=`, or `==`. The three untagged characterization tests stay outside it, because their `!` follows `>`: `FixOfNegationAfterGreaterThanInAnArgumentWritesAGenericReading`, `…InAComparisonChainWritesAGenericReading`, and `FixOfNegationStartingASwitchAfterGreaterThanInAnArgumentWritesAGenericReading`. A test pinning c1 is outside the condition and needs no `[Requirement]`.

As the lead asked, the spec names neither the excluded case nor an open question. So FC-002's example for the excluded side stays out of the spec, as it did for the previous two wordings.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed. The working tree also held the test implementer's uncommitted change to `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`, which I did not touch or commit.

## Examples for the tests

No new requirement example.

## Open questions to file

**`>>>=` gets parentheses it does not need.** This was found in passing and is not caused by this round's change. It belongs to thoenissen/Reihitsu#1245 (`SDD open questions: sdd/specs/analyzer/RH3001.md`).

- **Input:**
  ```csharp
  class T { public static T operator >>>(T a, bool b) => a; }
  class C { T B; void Run(bool x) { B >>>= !x; } }
  ```
- **Real output:** `B >>>= (x == false);` (LF and CRLF, one iteration). It compiles.
- **Contrast:** `B >>= !x;` gives `B >>= x == false;`. `B <<= !x;` and `B &= !x;` also get no parentheses.
- **Why it looks wrong:** an assignment binds more weakly than `==`, so `x == false` binds the same with or without the parentheses. RH3001-004 says the fix "MUST NOT add any" parentheses in that case. The `!` follows `>>>=`, not `>>>`, so the input is inside the MUST, and the real output contradicts that sentence. The behavior is harmless; the spec claim is false for this one input.
- **Competing readings:**
  - fix the code so `>>>=` behaves like `>>=`;
  - narrow the parenthesization sentence so it does not claim `>>>=`.
- **Touches:** RH3001-004 (its parentheses sentence).
- **For the lead:** this round left the input inside the MUST, because the decision record names only `>>` and `>>>`. Whether the spec has to change before publishing is the lead's call.

## Decisions needed

_None._
