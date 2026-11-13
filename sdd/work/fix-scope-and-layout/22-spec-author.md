Role: spec author
Commit: 1144034
Verdict: spec written

Repair round 3, decision 33 (auditor 19, finding 8; test implementer 20's wording gap). Spec text only: RH5417-003 and RH3202-002. No code, test, change doc, or rule page touched; no new requirement ID, so the tags of test implementer 20 stay valid. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision. `SpecStructureTests`/`SpecConsistencyTests` (architecture, filter `Spec`) pass on the working tree, 37/37.

## Changed requirements

- RH5417-003 — The fix makes the braces of a member or accessor body start their own lines — clarified (decision 33): "the line begins with a member" is now defined. The line's first token starts the line — no part of an earlier token (such as a multi-line string literal) reaches into it, and only whitespace and comments whose text starts on that line stand in front of it — and that token is the first token of the fixed member or of a member in front of it in the same member list (for a member with attributes, the `[` of its first attribute list). The tail of a comment that started on an earlier line does not count. The refused list now names the shapes of finding 8: a type header, a sibling's wrapped-initializer continuation, a statement or closing brace inside a sibling's body, a sibling's multi-line literal tail, a multi-line comment tail. *Why* reworded to cover them.
- RH3202-002 — The fix keeps the separators around the method — clarified the same way.

## Examples for the tests

Three new examples in each requirement, taken from test implementer 20's fixtures and expected outputs (`main`'s output for the refused side):

- comment that starts on the member's line (separated side): `FixSeparatesTheMemberBehindWhenTheFixedMemberBeginsItsLineBehindAComment` / `…TheConvertedMethodBeginsItsLineBehindAComment`;
- tail of a comment started on the line above (refused, shape d): `FixLeavesTheMemberBehindOnALineThatBeginsWithTheTailOfAMultiLineComment` (both rules);
- continuation of a sibling's wrapped initializer (refused, shape a): `FixLeavesTheMemberOnALineThatBeginsWithTheContinuationOfASiblingsInitializer` / `FixLeavesTheMethodOn…`.

The first two are the two sides of the comment qualifier (FC-002). Shapes (b) and (c) are named in the normative sentence; their tests already exist, no spec example added.

## Output questions

_None._ The refused class keeps `main`'s output (decision 33).

## Internal choices

- The definition is written into both rule requirements, not into SHARED-FIX-007, as in round 2 (14-spec-author): the narrowing belongs to the helper these two fixes share. Alternative: a shared definition in SHARED-FIX-007.
- No cross-reference to SHARED-STMT-004 (the statement counterpart with the same check): `UsesListsExactlyTheReferencedRequirements` would require it in `Uses`, which would claim a dependency the rules do not have. Alternative: list it in `Uses`.
- The comment kinds are "comments" without distinguishing documentation comments, matching `StartsLineBehindComments` (`SyntaxTriviaUtilities.IsCommentTrivia`) which the decision names as the check to reuse.

## Follow-up candidates

_None_ beyond those in `00-lead.md` (RH5513 attribute base still separating shape (a) at the depth column is recorded by test implementer 20 as outside decision 33).
