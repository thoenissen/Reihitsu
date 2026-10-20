Role: spec author
Commit: 54e32bf
Verdict: spec written

This is a delta to `37-spec-author.md`. It covers finding 1 of `40-auditor.md`, as `squad/decisions/2026-10-20-997-rh3001-parentheses-claim.md` directs: drop the negative parentheses claim. Nothing else changed: no other requirement, no example, no rule page, no ID, no test.

## Changed requirements

### RH3001-004 — clarified

Before: "When the comparison binds differently from the negation inside its parent, the code fix MUST put it in parentheses; otherwise, unless the negation is the right operand of an assignment operator other than `=`, it MUST NOT add any."

After: "When the comparison binds differently from the negation inside its parent, the code fix MUST put it in parentheses."

The requirement no longer says where the fix adds no parentheses. That also removes the assignment-operator exemption that `37-spec-author.md` added. The guard clause, the replacement with `x == false`, and the sentence about the operand's own parentheses are unchanged.

## What still fits

**The positive claim.** I re-checked it against the probes of rounds 31, 35, and 37. Every input where `==` binds differently from the negation gets parentheses:
- `b == !a`;
- `!x is true`;
- `B >>> !x`;
- the leftmost operand of `switch`, `+`, or `*` (`x => (x == false) switch { … }`).

No input of those rounds binds differently and lacks parentheses.

**The example.** It still holds, and it is the real output: `var x = a == false && b;`, `var y = b == (a == false);`, `return (a && b) == false;`. The first line shows a position without parentheses. It is the real output, but the requirement no longer claims it.

**The Why line.** It is unchanged, and its two statements are still true C#:
- `b == !a` needs the parentheses to keep its meaning;
- `!a && b` reads the same without them.

It explains why parentheses are needed. It does not promise that they are left out.

**The five RH3001-004-tagged tests still fit:**
- `NotOperatorOnLiteral`, `NotOperatorOnField`, `NotOperatorOnProperty`, and `NotOperatorOnMethodCall` test the replacement. Each asserts the real output, `return x == false;`.
- `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently` asserts parentheses for `b == !a`, which is the positive claim. It also asserts the operand's own parentheses for `!(a && b)`. Its `&&` line asserts real output that the requirement no longer claims. That is allowed, since a test may pin more than its requirement states.
- **Naming note for the test implementer's lane, not changed here.** The word "Only" in that test's name now says more than RH3001-004 does.

The two untagged assignment pins stay untagged, which is correct: `FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses` and `FixOfNegationAfterNullCoalescingAssignmentAddsParentheses`.

`documentation/rules/RH3001.md` says nothing about parentheses, so it needs no change.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3001NotOperatorShouldNotBeUsedAnalyzerTests"`: 34/34 passed.

## Examples for the tests

No new requirement example.

## Open questions to file

_None._ The decision record has the lead file the positions from `40-auditor.md` on #1245 Q3, together with the `>>>=` and `??=` inputs from 35 and 37.

## Decisions needed

_None._
