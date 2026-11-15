Role: spec author
Commit: e9ba813
Verdict: spec written

The prompt named the gap (`00-lead.md` decision 11, `07-implementer.md` *Disputed tests or scope*, item 2) and the lead's decision to write the behavior the code and tests already have; it carried no other intended fix or hoped-for verdict. The statement was checked against `PrecheckPhase.FindUnsupportedSyntax` (FMT-UNSUP-004's reason wins only when the reported comma's parent is a `VariableDeclarationSyntax`) and against `UnsupportedListSeparatorCommentTests` (`DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment`; the *Tuple type* and *Function pointer parameters* rows assert FMT-UNSUP-009's reason). No build or test was run, as instructed; no file under `src/` was touched.

## Changed requirements

- FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported — clarified (not yet shipped): the paragraph on declarator commas now states, as two conditions with an example on each side, which reason and location the formatter gives when a comment in front of a comma inside a variable declaration's span refuses the input under both FMT-UNSUP-004 and FMT-UNSUP-009. A comma between two declarators gives FMT-UNSUP-004's reason and location (a directive or disabled text there stays FMT-UNSUP-009's only); every other comma in the span — tuple type, function pointer type, type argument list, a field's attribute argument list — gives FMT-UNSUP-009's. A sentence notes that a comma inside an initializer value raises no tie (FMT-UNSUP-004 excepts the value's inside). The `Why:` sentence on the tie was reworded to match. Two examples added: `int a = 1 // c⏎, b = 2;` (FMT-UNSUP-004's reason) and `(int A // c⏎, int B) t = default;` (FMT-UNSUP-009's reason).

Change doc `sdd/changes/2026-11/comma-behind-comment-unsupported.md`, decision *FMT-UNSUP-004 keeps its reason*: a paragraph added that limits it to declarator commas, gives FMT-UNSUP-009's reason for every other comma in the span, and records the rejected alternative (FMT-UNSUP-004's reason for every comma in the span). `Status: Draft` unchanged.

## Examples for the tests

- FMT-UNSUP-009: the two new examples. Both are already covered: `DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment` (declarator side) and the *Tuple type* row of the statement-list test (other side); the *Function pointer parameters* row covers the function-pointer case named in the bullet. No new test is required. An attribute-argument-list comma in a field (named in the bullet, same code path: its comma's parent is not a `VariableDeclarationSyntax`) has no dedicated tie test; the test implementer may add a row such as `[A(1 // c⏎, 2)] private int _f;` if the lead wants every named case pinned.

## Output questions

_None._ The reason text is chosen per the lead's decision 11; both texts are existing reasons.

## Internal choices

- Named the attribute argument list of a field among the non-declarator commas, because the code gives it FMT-UNSUP-009's reason by the same parent test. Alternative: name only the type commas the tests cover.

## Follow-up candidates

_None._
