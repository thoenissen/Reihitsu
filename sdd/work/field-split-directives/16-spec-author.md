Role: spec author
Commit: 1071f27
Verdict: text written

Repair round 2, decision 4 in `00-lead.md`, on member D2 of `15-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Text only; no requirement ID added or removed; no behavior changes (the code and `main` already give FMT-UNSUP-009's reason for D2).

## Changed requirements
- FMT-UNSUP-010 — Directives inside a field declaration with several declarators are unsupported — clarified: the tie paragraph now says the declarator and the comma can belong to any variable declaration inside the field declaration — the field's own, or a local declaration inside a value, with the D2 pragma input as the example — and both give FMT-UNSUP-009's reason. "Every other directive" is spelled out by cause: a directive in front of a comma that separates no declarators (argument list, tuple type), one between a conditional operator and its branch (FMT-UNSUP-008), and every directive no other requirement refuses.
- FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported — clarified: the first tie bullet ("A comma between two declarators of the declaration") adds that the reason and location stay this requirement's also where the declaration is a local declaration inside a value of the field, such as one in a lambda body.

## Examples for the tests
FMT-UNSUP-010 tie paragraph — its new local-in-lambda example is the pragma row of `DirectiveInFrontOfADeclaratorCommaOfALocalInAValueKeepsTheReasonOfAListSeparator` (15, member D2); no new test is needed.

## Output questions
_None._

## Internal choices
- The added FMT-UNSUP-008 mention in the "every other directive" sentence names the tie the D3–D6/N2–N4 tests check; alternative: leave "every other directive" unqualified, which decision 4 asked to make unambiguous.

## Follow-up candidates
_None._

`scripts/test.sh --project architecture`: 153 passed.
