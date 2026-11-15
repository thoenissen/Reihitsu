Role: spec author
Commit: cc45461
Verdict: spec written

Repair round 1, for D25 and D26 (11-auditor findings #1 and #2). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

## Changed requirements

- RH5113-004 — No fix is offered in a file with a syntax error or for a declaration the formatter refuses — added (D26). It records `main`'s withholding as a decision: the fix moves only the semicolon now, so SHARED-FIX-010/011 no longer cover it; it stays withheld in a document whose tree holds an error and for a declaration `ReihitsuFormatter.FindUnsupportedSyntax` refuses. Its `Why` gives the reason. ID added to `sdd/specs/requirement-ids.txt`. RH5113's `Uses` gains FMT-PIPE-005, FMT-UNSUP-001, FMT-UNSUP-004, FMT-UNSUP-007.
- Change doc `sdd/changes/2026-11/fix-layout-scope.md`: RH5113-004 added to *Affected requirements*; new *Decisions* subsection "RH5113 keeps its withholding". No visible output change, so the doc's acceptance is not affected.
- Rule page `documentation/rules/RH5113.md`: unchanged (a withheld fix in rare inputs; `general-notes.md` covers diagnostics without a fix).

D25: no spec change. SHARED-FIX-007 *Shared line* already demands the outputs the routing produces, as for D8's members, whose specs carry no statement of their own either. See *Internal choices* for the helper's two rare classes.

## Examples for the tests

- RH5113-004: three examples.
  - A syntax error elsewhere in the file: reported, no fix. It is offered once the error is gone. The existing `NoFixIsOfferedInAFileWithASyntaxError` covers this; retag it from SHARED-FIX-011 to RH5113-004 and correct its summary.
  - `int /* c */ _a⏎    ;`: reported, no fix (FMT-UNSUP-004). This needs a new test.
  - The same field without the comment: the fix is offered and gives `int _a;`.
- Architecture: `EveryRequirementOfAnEnforcedSpecIsCovered` fails on the head of this commit, and only for RH5113-004, until those tests are tagged. `UsesListsExactlyTheReferencedRequirements` passes.
- D25 members (RH2001, RH5110 both sides, RH5412/RH5413 behind, RH5111 property target both sides) are tagged SHARED-FIX-007. The auditor's inputs in 11-auditor finding #1 are the seeds. The expected output follows the *Shared line* bullet and the RH5417 example there.

## Output questions

_None._ FU7 (offer the RH5113 fix in those inputs) stays a follow-up candidate with its output question, as D26 decided.

## Internal choices

- RH5113-004 does not name SHARED-FIX-010 or SHARED-FIX-011. Listing them would put them in `Uses`, as if RH5113 used them. Its `Why` describes them in words instead. The alternative was to name them and list them in `Uses`.
- D25 and the helper's rare classes: no shared statement in this run. This is my recommendation.
  - `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` leaves a member on its shared line in two cases:
    - the line does not begin with a member of the same list;
    - the gap to the sibling holds anything but whitespace (D18).
  - Only RH3202-002 and RH5417-003 state this. The specs of D8's callers (RH3203, RH3205, RH3207, RH5113, RH5401, RH5404, RH5408) say nothing, and neither do those of D25's callers. SHARED-FIX-007 *Shared line* states no such exception.
  - The qualifier cannot move into SHARED-FIX-007 now. The attribute base's private before-break (D23, the second owner) breaks without it, so a shared MUST NOT would contradict RH5511/RH5513/RH5517.
  - Alternative: restate the qualifier in each caller's spec. I rejected it as nine restatements of one rule.
  - Recommendation: carry the statement into FU5. Once the in-front break has one owner, SHARED-FIX-007 gains the qualifier once, and RH3202-002 and RH5417-003 reference it.
- Should the implementer withhold any D25 fix for a class beyond the helper's own two, the lead routes that class back to me. AGENTS.md, *Code-fix coverage*, requires the gap in the rule's spec. I wrote nothing ahead of the implementation.

## Follow-up candidates

- FU5 extension, not a new candidate: when the in-front break gets one owner, SHARED-FIX-007 *Shared line* states the "line begins with a member" and "whitespace-only gap" exceptions once, and RH3202-002 and RH5417-003 reference them. Spec wording only, apart from the RH5511/RH5513/RH5517 output question FU5 already carries.
