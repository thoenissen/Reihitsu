Role: spec author
Commit: 40714ba
Verdict: spec written

This is a delta to `21-spec-author.md`. It covers findings 1 and 2 of `24-spec-checker.md`, which the lead routed to me. Both were checked against the code. The checker's suggested wording was treated as a suggestion only. Nothing else changed: no test, no rule page, no ID.

## Changed requirements

### RH3001-004 — clarified

The fix's guard now reads:

> For a negation that covers less text than the syntax node directly around it, and that is not the right operand of a `>` operator, the code fix MUST replace …

The list of excluded constructs (an argument, a tuple element) is gone. In its place is the condition the code decides on. `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.RegisterCodeFixesAsync` registers the fix only when `root.FindNode(diagnostic.Location.SourceSpan)` is a `PrefixUnaryExpressionSyntax`. `FindNode` is called without `getInnermostNodeForTie`, so it walks up through every ancestor with the same span. It therefore returns the negation only when the negation's parent covers more text. Any node made of the negation alone loses the fix, whatever its kind. The `>` exclusion (Q1 of thoenissen/Reihitsu#1245, a P1 compile error) is unchanged.

Probed with `scripts/apply-fix.sh RH3001`, under LF and CRLF with the same result:

| Input | Result |
|---|---|
| `bool[] a = [!x, y];` | reported, no fix offered |
| `var r = x is !true;` | reported, no fix offered |
| `return x switch { !true => 1, _ => 2 };` | reported, no fix offered |
| `new bool[] { !x }` | fixed |
| `[y, !x && y]` | fixed |
| `x switch { true => !y, _ => y }` | fixed |
| `case !true:` | fixed |
| `$"{!x}"` | fixed |
| `new { A = !x }` | fixed |
| `new P { A = !x }` | fixed |
| `x => !x` | fixed |
| `yield return !x;` | fixed |
| `M(x: !y)` (from round 21) | fixed |

The three no-fix inputs compile with csc. The condition sorts every row: in each fixed case the parent covers more text than the negation, and in each no-fix case it does not. The existing tagged RH3001-004 tests (`NotOperatorOn*`, `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently`) all have a parent that covers more text: a `return` statement, an `=` value, an `&&` or `==` operand.

### RH3103-003 — clarified

The last sentence, "This requirement does not cover a name qualified by an alias alone, such as `S::Nullable<int>`.", is removed. The requirement now ends after the dotted-name examples, as the other narrowed requirements of this run do.

The requirement stays true without the sentence. For `S::Nullable<int>` the analyzer reports the generic name alone: `genericName.Parent` is an `AliasQualifiedNameSyntax`, not a `QualifiedNameSyntax`, so the location is `genericName.GetLocation()`. That is the first clause, "on the generic name". The whole-name clause applies only after a `.`.

This keeps the intent of `squad/decisions/2026-10-20-997-test-findings-spec-corrections.md`, which was that the requirement must not claim the whole name for an alias-qualified name. The alias test `AliasQualifiedNullableGenericInTypeofIsReportedAndFixed` keeps its tags (RH3103-001/-004) and needs no RH3103-003 tag.

## Checks

`scripts/test.sh --project architecture`: 72/72 passed.

## Examples for the tests

No new requirement example. The test implementer adds untagged `VerifyNoCodeFixIsOffered` characterization tests for the three new inputs. The checker asked for `[!x, y]` as a minimum. Use the full inputs below.

## Open questions to file

These extend Q2 of thoenissen/Reihitsu#1245 (`SDD open questions: sdd/specs/analyzer/RH3001.md`). They are not a principle instance: no fix is offered, so nothing breaks. The open point is the guard's scope.

- **Inputs:**
  - `class C { bool[] Run(bool x, bool y) { bool[] a = [!x, y]; return a; } }` (an element of a collection expression)
  - `class C { bool Run(bool x) { var r = x is !true; return r; } }` (a constant pattern)
  - `class C { int Run(bool x) { return x switch { !true => 1, _ => 2 }; } }` (a constant pattern in a switch arm)
- **Real output:** each negation is reported, and no code fix is offered. For contrast, `new bool[] { !x }`, `[y, !x && y]`, `case !true:`, and `x switch { true => !y, _ => y }` are fixed.
- **Why it looks wrong:** no comment, directive, or binding concern is involved. As with the Q2 argument and tuple inputs, the fix is withheld only because the negation is all there is of the node around it: the collection element, or the constant pattern. In the constant-pattern cases, `!true` cannot become `true == false`, because an equality is not a pattern. The report there may have no correction. That is the competing reading for those two inputs.
- **Competing readings:**
  - offer the fix wherever the rewrite is valid;
  - keep it withheld where the negation is a whole node, and document the gap;
  - for a constant pattern: do not report, or report without a fix.
- **Touches:** RH3001-004, and RH3001-001 for the pattern cases.
