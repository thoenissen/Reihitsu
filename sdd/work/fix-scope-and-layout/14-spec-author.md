Role: spec author
Commit: cca0321
Verdict: spec written

Repair round 2, decisions 27, 28, 30 (auditor 13, findings 1, 2, 5, 6). Spec text only; no code, test, change doc, or rule page touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions. `SpecStructureTests` and `SpecConsistencyTests` pass on the working tree (26/26). No new requirement ID: every change amends an existing requirement, so the test implementer's tags stay valid.

## Changed requirements

- RH5203-003 — The fix moves the argument into the alignment column — changed (decision 27, finding 1): when anything but whitespace stands in front of the reported argument on its line, the fix MUST NOT be offered; the diagnostic is still reported. New negative example (`/* two */ {|RH5203:2|}`). Why extended (RH5038 class, rare).
- RH5203-003 — same requirement — clarified (decision 30, finding 5): the comment lines the fix moves are `//` and `/* */`; a documentation comment (`///`, `/** */`) there stays in its column. Checked against `RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.ApplyCodeFixAsync`: it moves only `SingleLineCommentTrivia` and `MultiLineCommentTrivia`; for a documentation comment the pending whitespace is written back unchanged.
- RH5417-003 — The fix makes the braces of a member or accessor body start their own lines — changed (decision 28, finding 2): the SHARED-FIX-007 shared-line separation applies only when the line on which the member starts begins with a member of the same type (the member itself or one in front of it); otherwise the fix MUST NOT separate the members on its lines and lays out the body as before. Example from the auditor's fixture, output as on `main` (d6a01ee, `apply-fix.sh`). `Uses` gains SHARED-FIX-007.
- RH3202-002 — The fix keeps the separators around the method — changed (decision 28): the same condition for the converted method; example from the auditor's fixture, output as on `main`. `Uses` gains SHARED-FIX-007.
- SHARED-ATTR-008 — The separate-line fix breaks the line after the list — clarified (finding 6): the indentation unit is named "scope level" (each enclosing namespace, type, enum, block, switch statement, or accessor list, and one more inside a switch section's statements), and the coverage sentence counts the same scope levels for the enclosing brace.
- SHARED-ATTR-021 — The separate-line fix is withheld when the enclosing brace is off its depth column — clarified (finding 6): counts the scope levels SHARED-ATTR-008 counts, a switch section's statements included, so a block among a section's statements is at the column of those statements. Matches `HasUnusableEnclosingScope` (`ComputeBaseIndentLevel(ancestor) * IndentSize`, which counts `SwitchSectionSyntax`) and the existing test `VerifyCodeFixIndentsInsideBlockOfSwitchSection`.

## Examples for the tests

- RH5203-003: the new negative example (comment in front of the argument on its line: reported, no fix). The auditor's other members (`/** a */ 2`, a `/* a */ 2` line below a `// two` line) fall under the same sentence.
- RH5417-003, RH3202-002: the new "type written on one line" examples (output as on `main`).
- SHARED-ATTR-008/021: no new example; the existing switch-section test is the case the wording now covers.

## Per issue

- #1140: RH5417-003 and RH3202-002 above (shared-line instances narrowed per decision 28).
- #1141: nothing to do in this round.
- #1144: RH5203-003 above; RH5417-003 and RH3202-002 above.
- #1145: nothing to do in this round.
- #1094: SHARED-ATTR-008 and SHARED-ATTR-021 wording above.
- #1393: nothing to do in this round.
- #1511: nothing to do in this round.
- #1561: nothing to do in this round.

## Output questions

_None._ Each refused class keeps `main`'s output (decisions 27, 28), and the RH5203 documentation-comment class keeps its column, as the code does today (decision 30).

## Internal choices

- Decision 28 is recorded in the rule specs RH5417-003 and RH3202-002, not in SHARED-FIX-007: the narrowing belongs to the helper these two fixes share, while the attribute-placement fixes (SHARED-ATTR-008) still separate in the same shape at the depth column. Alternative: an exception sentence in SHARED-FIX-007's *Shared line* bullet naming the two rules.
- Decisions 27 and 28 amend existing requirements instead of adding new IDs (as RH5205-004 did), so tests written in parallel need no new tag. Alternative: RH5203-004 and RH5417-006/RH3202-003 as separate "no fix" requirements.
- The decision-28 condition is worded on "the line on which the member starts", matching the helper's existing use of the member's first line; the implementer should key the check there.

## Follow-up candidates

_None_ beyond those already in `00-lead.md` (decision 29 covers finding 3).
