Role: spec author
Commit: b7c2c17
Verdict: spec written

This is a delta to `25-spec-author.md`. It covers finding 1 of `30-auditor.md`, the open RH3001-004 finding, as `squad/decisions/2026-10-20-997-rh3001-targeted-fix.md` directs. The lead's message named two candidate conditions. I treated them as options to check, not as the answer, and derived the condition from the code. Nothing else changed: no other requirement, no rule page, no ID, no test.

## Changed requirements

### RH3001-004 — clarified

Before: "… and that is not the right operand of a `>` operator, the code fix MUST replace …"

After: "… and whose `!` is not the next token after a `>` operator, the code fix MUST replace …"

The whole-node clause ("covers less text than the syntax node directly around it") is unchanged.

**How the condition was derived.** `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync` replaces the negation with a `ParenthesizedExpression` that carries `Simplifier.Annotation`. Its `(` takes the negation's leading trivia, so the `(` sits exactly where the `!` was. The token in front of it is therefore the token that was in front of the `!`.

The Simplifier keeps the parentheses whenever `==` binds more weakly than the negation's parent. This holds in every case where a `>` precedes the `!`:

- when the negation is the right operand of `>`;
- when the negation is the leftmost operand of a construct that binds more tightly than `>` and stands in that right operand, such as a `switch` expression, `+`, or `*`.

So the fix writes a `(` directly after `>` exactly when the `!` comes directly after a `>` operator. Together with an earlier `<`, that `(` produces the generic reading.

**Options rejected.**

- *The largest-expression condition of RH3005-004, RH3101-004, and RH3105-004* ("does not hold both a `<` and a `>` operator") does not cover the hazard. In `M(G < A, B > !x switch { … })`, the largest expression that contains the negation is the second argument, `B > !x switch { … }`. That argument holds no `<`, so the negation would stay inside the MUST.
- *"Holds no `>` operator"* would exclude safe inputs such as `a > b && !c`. It also does not describe what the fix does.

**Probes** with `scripts/apply-fix.sh RH3001`, LF and CRLF, same result in both. Compile results come from the SDK's `csc.dll` against the net10.0 reference pack.

| Input (inside a method) | Fixed text | Fixed text compiles | In the new MUST |
|---|---|---|---|
| `M(G < A, B > !x switch { true => 1, false => 0 })` (int `G`, `A`, `B`) | `M(G < A, B > (x == false) switch { … })` | no: CS0307, 2× CS0246 | no |
| `M(G < A, B > !x + t)` (user-defined `+(bool, T)`, `>`/`<`(V, T)) | `M(G < A, B > (x == false) + t)` | no: CS0307, 2× CS0246 | no |
| `M(G < A, B > !x * t)` (same with `*`) | `M(G < A, B > (x == false) * t)` | no: CS0307, 2× CS0246 | no |
| `return b > !x switch { true => 1, false => 0 };` (no `<`) | `return b > (x == false) switch { … };` | yes | no (excluded but harmless; left uncovered) |
| `M(G < A, B >> !x)` (user-defined `>>`(V, bool)) | `M(G < A, B >> (x == false))` | yes | yes |
| `M(G < A, B >= !x)` (user-defined `>=`(V, bool)) | `M(G < A, B >= (x == false))` | yes | yes |
| `x => !x`, `x => !x switch { … }` | `x => x == false`, `x => (x == false) switch { … }` | yes | yes |

The probes confirm the condition must name the `>` operator only. `>>`, `>=`, and `=>` are different tokens, and their fixes compile.

**The condition still matches everything checked before.**

- I re-ran all 13 inputs of the `25-spec-author.md` probe table under LF and CRLF. The results are unchanged: the three whole-node inputs get no fix and the ten others are fixed.
- None of the ten fixed inputs has a `>` operator directly before the `!`. `x => !x` and `true => !y` are preceded by `=>`, and `M(x: !y)` by `:`.
- The five tagged RH3001-004 tests are inside the condition: `NotOperatorOnLiteral`, `NotOperatorOnField`, `NotOperatorOnProperty`, `NotOperatorOnMethodCall`, and `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently`. Their `!` follows `return`, `=`, or `==`.
- The two untagged Q1 characterization tests stay outside it: `FixOfNegationAfterGreaterThanInAnArgumentWritesAGenericReading` and `…InAComparisonChainWritesAGenericReading`. In both, the `!` follows `>`.

As the lead asked, the spec names neither the excluded case nor an open question. So FC-002's example for the excluded side stays out of the spec, as it already was for the previous wording.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed. The working tree also held the test implementer's uncommitted change to `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`.

## Examples for the tests

No new requirement example. Under the decision record, the `switch` input above is pinned by an untagged characterization test. That is the test implementer's work in parallel.

## Open questions to file

_None._ The lead has already filed both inputs, the `switch` input and the user-defined `+` input, as #1245 "Q1, continued". The `*` input and the related shapes the auditor hinted at (`<<`, `..`, not probed) are further instances of the same mechanism. Their `!` comes directly after a `>` operator, so they are in the excluded region by the same token condition and need no new question. The lead may add them to that comment.
