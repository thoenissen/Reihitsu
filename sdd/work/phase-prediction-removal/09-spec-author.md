Role: spec author
Commit: 7f7a584
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It told me to apply the maintainer's answer to escalations 5 and 12 (`00-lead.md`, last section; `squad/decisions/2026-10-25-phase-prediction-removal-node-level-shift.md`), option A, and to set the change doc to `Accepted`. This round does both. Fixture outputs below were observed on 7f7a584 with `scripts/apply-fix.sh` on fixtures in the scratchpad; columns are 0-based.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` is now **`Status: Accepted`**. The maintainer approved it as of `06-spec-author.md` (`00-lead.md`, "Escalation 2 (second amendment)"). This round only records the decided gap; What and Why describe the same change as approved:
- **Affected requirements**: + RH5303-006, RH5304-005.
- **What**, RH5303/RH5304 bullet: one sentence saying that their code fixes are withheld when the brace stands behind the last line of a multi-line comment.
- **Inventory row** `ReihitsuFormatterHelpers.ComputeTokenColumn` / `AdjustNodeIndentation`: it also does not move a line that starts inside a comment. It stays "Not changed here", and the two fixes are withheld where that would misplace an element.
- **Decisions**, under "The RH5202 code fix and the node-level blank-line facts stay out of scope": a paragraph explains the node-level step, the shape it misplaces, why the fixes are withheld, and that restructuring the step is left to its own change, with a pointer to thoenissen/Reihitsu#1382.
- **Alternatives**: "Change the node-level column step in this change" is recorded as rejected (option B).

## Changed requirements

- RH5303-006 — The fix is withheld for a complex element behind the last line of a multi-line comment — **added**.
- RH5304-005 — The fix is withheld for a complex element behind the last line of a multi-line comment — **added**.

Both say the same thing. The code fix MUST NOT be offered for a diagnostic when the code it formats contains a complex element initializer whose opening brace stands on the last line of a multi-line comment, behind the comment. That code is the reported object creation, or the object creation of the reported assignment, together with the declaration value, statement, or expression body around it. The diagnostic stays reported. The Why says the gap is deliberate, not an oversight.

The other side of the qualifier (FC-002) is stated too: a brace behind a one-line comment, or on its own line after a multi-line comment that ended above, is not covered, and the fix is offered.

**Scope of the guard.** It covers the whole code the fix formats, not only a diagnostic located on that element. A diagnostic elsewhere in the same creation, such as RH5303-001 on misaligned outer braces, formats the element too and would misplace it. That would break SHARED-FIX-007 and SHARED-FIX-001, which every fix must meet. A guard limited to diagnostics on the element is therefore not an open reading: the shared specs already rule it out. The maintainer's answer gives no condition on where the formatted target starts, so the guard applies whether the target starts at the beginning of its line or in the middle.

Header `Uses` rows of both specs: + SHARED-COMMENT-012, SHARED-FIX-001, SHARED-FIX-007. The ledger has the two new IDs at their sorted positions.

State of `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd|FullyQualifiedName~PhaseOwnership|FullyQualifiedName~RuleDocumentation"` on the working tree: 51 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, because RH5303-006 and RH5304-005 have no test yet. That is pending work for the test implementer.

**No rule page changes.** A withheld fix is covered by `documentation/rules/general-notes.md` ("A diagnostic without a code fix"), as `AGENTS.md` *Code-fix coverage* states.

## Examples for the tests

- **RH5303-006**:
  - Reported, no code fix offered: the brace stands behind `b */` at column 28, with the expressions at 24 and `}` at 20.
  - Fix offered (the other side): the comment `/* a` / `b */` ends on the line above, the brace starts its line at 20, and `2` at 22 is reported. The fix moves `2` to 24. Observed with `apply-fix.sh RH5303`: `fixed (1 action(s), 1 iteration(s))`.
- **RH5304-005**: reported, no code fix offered. `D = {` / `/* a` / `b */ {` with the brace at 36 and the expressions at 32 and `}` at 28 (measured from the comment). On 7f7a584 the fix moves them to 46/42 and does not converge (`no-progress`). This is the shape of the RH5304 failure in `08-implementer.md`.
- **Tests to be replaced**, per the decision. They assert node-level output that the change doc excludes:
  - `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed` and the RH5304 test of the same name. Replace them with "detected, no code fix offered" tests tagged RH5303-006 / RH5304-005 (plus SHARED-INIT-002, RH5303-003/RH5304-002, and RH5303-004/RH5304-003, which still hold for the report).
  - `CommentLedFirstTokenAnchorTests.FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment` and `…WhenTheInitializerBraceMoves`. No requirement now specifies the node-level output for this shape, so the tests derived from RH5303-006/RH5304-005 replace them. The document-level half of the second test (`AssertRuleResult`) still holds and passes.
- Add one test where the guard covers a diagnostic that is not on the element: RH5303-001 misaligned outer braces around a creation that contains the shape, no code fix offered. This follows from the requirement's "the code it formats contains".

## Open questions for the maintainer

_None._ The follow-up candidate is presented when the PR is ready, as the decision states: restructure the node-level column steps (`ComputeTokenColumn` / `AdjustNodeIndentation`), together with the skeptic's follow-ups 1 and 2 and the finding in `08-implementer.md`. It is recorded in the change doc's Decisions as left to its own change; it is not recorded as an issue.
