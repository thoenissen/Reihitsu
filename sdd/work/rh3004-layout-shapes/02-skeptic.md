Role: skeptic
Commit: ef05d47
Verdict: proceed (after two wording amendments to RH3004-011; see *Internal choices*)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. thoenissen/Reihitsu#1607 is `agent-filed`, so its goals are proposals.

How established: `scripts/apply-fix.sh RH3004 <dir>` (LF and CRLF give the same status for every fixture below) and `scripts/format.sh <dir>` over fix results. All fixtures were written to the session scratchpad, outside the repository. Each fixture is a complete file of this shape, and only the line(s) inside `M()` are quoted:

```csharp
using System;

class C
{
    void M()
    {
        <quoted lines>
    }
}
```

(`a02` also has `using System.Threading.Tasks;`, `a07` also declares `void N(Func<int, int> f, int y) { }`, and `b01` also declares `void Foo() { }`.)

## Premise

- **Item 1 is real and reachable.** The RH3004 code fix offers a fix on the expression-body path, and nothing in it treats a multi-line comment in the parameter list as a special case. `CreateLambdaExpression` copies the parameter list's text, and `FormatNodeInDocumentAsync` lays out the lambda. The issue's own input:
  ```csharp
  Func<int, int> g = delegate (int x /* first
      second */) { return x; };
  ```
  gives `fixed (1 action(s), 1 iteration(s))`:
  ```csharp
  Func<int, int> g = (int x /* first
      second */) => x;
  ```
  Whole-document formatting of that result changes only the final newline, so there is no second diff.
- **Item 2 is not reachable.** No caller produces the shape. Both `{ /* first⏎ second */ Foo();` (b01) and `{ /* first⏎ second */ return x;` (b02, also with `};` on the same line, b03, and with `/** */`, b04) report `no-fix-offered`. The cause: a comment in the block keeps the block body (`GetExpressionBody` requires that `ContainsCommentOrDirective(block)` is false), and on the block-body path every spanning carrier withholds the fix (`WithholdsFixForLines`, RH3004-008 condition 2). RH3004-006's measuring rule therefore never sees a comment continuation. I agree with the spec author that RH3004-006 stays unchanged.
- **Item 3 is not reachable through RH3004.** A block holding `#if NEVER`/`#if INNER`/`#endif`/`#endif` in column 0 (c01) reports `no-fix-offered` (RH3004-009, `ContainsDirectiveOrDisabledText`). Outside RH3004, the two predicates did not disagree visibly either. I formatted a file with inactive nested directives that have no disabled text between them: in column 0, in columns 12 and 2 inside a method, and in member-level leading trivia. `scripts/format.sh` left every directive line as written, which matches what `SyntaxIndentationUtilities.KeepsItsColumn` keeps.

## Mechanism and candidates

_N/A._ These are specs and tests for existing behavior, and no defect was found in the code. Candidates for RH3004-011's class, derived from where a delimited comment can stand in a `ParameterListSyntax` and from the fix's two paths (`GetExpressionBody` null or not) and its registration guards. The test column applies *Coverage first*.

| # | Input (inside `M()`) | Result | Existing test |
|---|---|---|---|
| a01 | `delegate (int x /* first⏎    second */) { return x; }` (issue's input, later line left of the new column) | fixed: `(int x /* first⏎    second */) => x;` | none found |
| a12 / spec | the RH3004-011 example (`second */` in column 46) and a second one with `two */` in column 12, in one document | fixed, both later lines keep their columns (2 iterations) | none found |
| a02 | `async delegate (int x /* first⏎    second */) { return await Task.FromResult(x); }` | fixed: `async (int x /* first⏎    second */) => await Task.FromResult(x);` | none found |
| a03 | `delegate ( /* first⏎            second */ int x) { return x; }` (comment in front of the first parameter) | fixed: `( /* first⏎            second */int x) => x;` — the space behind `*/` is removed, which is the formatter's FMT-SPACE-014 decision (no space after `(`); the same happens to `( /* note */ int x)` on one line | none found |
| a05 | `delegate (int x, /* first⏎            second */ int y) { return x + y; }` (behind a comma) | fixed: `(int x, /* first⏎            second */ int y) => x + y;` | none found |
| a04 | `delegate (int x /* first⏎            second */, int y) { return x + y; }` (between a parameter and the comma behind it, the comma on a later line) | **no-fix-offered**. Cause: the formatter refuses the built lambda, `unsupported syntax: a comment or directive between a list element and the comma behind it` (FMT-UNSUP-009, SHARED-FIX-010, `FindUnsupportedSyntax` at registration) | none found for this shape; `NoFixIsOfferedWhenTheAnonymousMethodBodyHoldsACommentTheFormatterRefuses` pins SHARED-FIX-010 for a body comment only |
| a06 | `/** first⏎            second */` in place of `/* */` | fixed, later line kept | none found (same mechanism as a01; optional) |
| a07 | `N(delegate (int x /* first⏎            second */) { return x; }, 1);` | fixed: `N((int x /* first⏎            second */) => x, 1);`. Formatting the result wraps `1` onto its own line, but formatting the *original* wraps it too, and `, 1` is not code the fix changes (SHARED-FIX-007), so this is not a defect | none needed |
| a08 | `delegate⏎            (int x /* first⏎            second */) { return x; }` (`delegate` alone on its line) | fixed: `= (int x /* first⏎            second */) => x;` — the parameter list moves up onto `delegate`'s line and into its column, as RH3004-011 states | none found (optional) |
| a10 | later line in column 0 (`second */)`) | fixed, kept in column 0 | none found (optional) |
| a11 | three-line comment, and a returned expression over two lines (`return x⏎                + 1;`) | fixed. The comment's lines are kept, and `+ 1` is re-indented by the formatter to column 24 (the expression body is the formatter's to lay out, RH3004-007) | none found (optional) |
| a13 | comment in the parameter list, block on lines of its own below | fixed: `(int x /* first⏎            second */) => x;` | none found (optional) |
| **a14** | `delegate (int x /* first⏎            second */⏎            ) { return x; }` (`)` on a line after the comment's last line) | fixed: `(int x /* first⏎            second */⏎                            ) => x;` — `)` goes to column 28, and `=> x` follows on the line of `)`, **not on the comment's last line**. Formatting the result changes nothing | none found |
| a09 | later line starts with `#` | no-fix-offered (RH3004-008 condition 1) | `FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath` |
| — | block-body path with such a comment | no-fix-offered (RH3004-008 condition 2) | `FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath` (+ CRLF variant) |

## Breaking inputs

No input breaks the code. a14 and a04 break the *spec text* of RH3004-011 (see *Internal choices*). Failure classes walked: comments relocated or lost (none: every later line kept, every comment kept, the comment and the parameters in the same order); line-ending variants (CRLF arm identical for every fixture); idempotency / second diff (fix results r01, r03, r12, r14 are stable under `scripts/format.sh` apart from the final newline); refused formatting (a04 withheld, as SHARED-FIX-010 requires).

Inputs the test implementer must cover, each a new test because none of them is pinned today:

1. RH3004-011 spec example (`a` with `second */` in column 46, `c` with a comment on one line). Tag RH3004-011. This clears the open architecture-test failure.
2. The issue's input a01 (later line left of the new column), or fold it into test 1 as a second statement.
3. a02 `async`: the spec states the `async ` prefix explicitly.
4. a14 `)` on a line after the comment, once the RH3004-011 wording is fixed.
5. a04 comment between a parameter and its comma: no fix offered (tag SHARED-FIX-010, and RH3004-011 if the amendment names it).
6. RH3004-008's new example (b01 and b02 in one file, no fix offered). b02 (single `return` plus comment behind `{`) is the member no existing RH3004-008 test reaches: every existing comment test has a non-`return` statement or a comment on lines of its own.
7. One CRLF variant of test 1, following the file's existing pattern (optional; the apply-fix CRLF arm matched LF).

Item 3: no new test. RH3004-009 withholds the fix for every directive, and 33 tests are tagged RH3004-009 already. A test for an inactive nested directive reads the same guard (`ContainsDirectiveOrDisabledText`) as they do.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH3004 analyzer | not involved (every fixture reports one diagnostic on `delegate`) |
| RH3004 code fix | must stay (`src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`) |
| Fix All | not involved (a12 fixes two in one document) |
| Formatter (`IndentationRewriter.IsNonRegionDirectiveTrivia`) | must stay: no visible disagreement found (item 3) |
| `Reihitsu.Core` (`SyntaxIndentationUtilities.KeepsItsColumn`) | must stay |
| Spec `sdd/specs/analyzer/RH3004.md` | must change: amend RH3004-011 as below |
| Rule page `documentation/rules/RH3004.md` | not involved |
| Tests (`tests/Reihitsu.Analyzer.Test/Clarity/RH3004*.cs`) | must change: add the tests above |

## Refuse option

- **Item 1:** the refuse option is to withhold the expression-body fix when the parameter list holds a delimited comment over several lines. Cost: a production change that removes a fix offered today, which is visible output, and nothing is gained, because today's output is correct and stable. Rejected. Declaring the shape unsupported (*a delimited comment that spans lines inside a lambda's or anonymous method's parameter list, from `(` through `)`*) would delete no special case: neither the fix nor the formatter has one for it, since FMT-INDENT-007 is the general rule. A shape that deletes nothing is not declared. It would also refuse files holding such comments, which are rare but legitimate. Not proposed.
- **Items 2 and 3:** the fix is already withheld (RH3004-008, RH3004-009), so there is nothing further to refuse.
- **Simpler option:** none. The run is already spec and tests only.

## Output questions

_None._ Every output above is decided by unchanged requirements: SHARED-FIX-007 with FMT-INDENT-007 and FMT-SPACE-014 for the fixed shapes (a14 included: the result is the formatter's layout and is stable), SHARED-FIX-010 for a04, RH3004-008 for item 2, and RH3004-009 for item 3. The spec amendments below only bring the new RH3004-011 text in line with those requirements, so they change no output.

## Internal choices

1. **RH3004-011 over-claims the position of the body (must fix).** The sentence "The expression body MUST follow on the comment's last line, behind the closing parenthesis of the parameter list and ` => `" is false for a14, where `)` stands on a line after the comment. There the result is `second */⏎                            ) => x;`, which is the formatter's own stable layout (SHARED-FIX-007). Recommendation: say that `=>` and the expression body follow the closing parenthesis on its line (RH3004-006), where the formatter places it, and that this is the comment's last line when `)` stands on that line. The alternative, keeping the sentence and limiting it to `)` on the comment's last line, leaves a14 unspecified. Neither reading changes output, because the code does the same in both.
2. **RH3004-011 is silent on the refused member (should fix).** A comment between a parameter and the comma behind it, with the comma on a later line, falls under RH3004-011's own definition ("a delimited comment in the parameter list") but gets no fix (a04, SHARED-FIX-010 / FMT-UNSUP-009). Meanwhile RH3004-003 lists RH3004-004/005/008/009/010 as the only conditions that withhold the fix, and RH3004's `Uses` does not list SHARED-FIX-010, although `NoFixIsOfferedWhenTheAnonymousMethodBodyHoldsACommentTheFormatterRefuses` is tagged with it. Recommendation: name SHARED-FIX-010 in RH3004-003's list and in `Uses`, and add one sentence to RH3004-011 for a04. Alternative: leave it to SHARED-FIX-010 alone. That is cheaper, but RH3004-003 as written would then demand a fix for a04.
3. **A `//` comment in the parameter list** (`delegate (int x // note⏎            ) { return x; }` gives `(int x // note⏎                            ) => x;`) is outside RH3004-011, which covers delimited comments over several lines only. Its output is decided by SHARED-FIX-007 and is stable. Recommendation: leave it unspecified and untested in this run. No test covers it today.

## Issue goals

- Item 1, "Specify the output and test it": keep.
- Item 2, "decide whether the requirement names the statement itself": drop. The shape is unreachable (the fix is withheld, RH3004-008). It is replaced by the RH3004-008 example and its test. No visible change.
- Item 3, "check for a second formatting diff and align the two predicates if it does": drop the alignment. RH3004 cannot reach the shape (RH3004-009), and outside RH3004 the formatter leaves inactive nested directives as written, which is what `KeepsItsColumn` keeps. The "confirmed identical" branch of "Done when" holds. The production files the issue lists (`RH3004UseLambdaSyntaxCodeFixProvider.cs`, `IndentationRewriter`) are not changed. No visible change.
- "Done when": met by the spec changes plus the tests above. Item 3's decided output is "no fix offered" (RH3004-009), which existing tests pin.

## Architecture findings

_None._ `KeepsItsColumn` (any non-region directive keeps its column) and `IsNonRegionDirectiveTrivia` (active directives only, for the formatter's column-0 rule) decide different questions. Their visible results agree in every shape tried, and RH3004 does not reach them, so they are no finding of this run.

## Follow-up candidates

_None._ The `( /* c */int x)` spacing in a03 is FMT-SPACE-014's specified behavior, not a defect.
