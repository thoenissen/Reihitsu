Role: spec author
Commit: 9df75e0
Verdict: spec written

This revision applies the maintainer's answers recorded in `00-lead.md` (Escalations) and in `squad/decisions/2026-10-18-shared-diag-003-comment-subject-rules.md` and `squad/decisions/2026-10-18-using-rules-name-by-value-filing.md`. Everything in `01-spec-author.md` still holds, except its Q1 and its open question, which are now settled as described below.

## Changed requirements

- SHARED-DIAG-003 — Only the text of a literal or a comment is content — clarified. The exemption paragraph now uses the general wording the maintainer chose: "This requirement restricts only rules that check code. A rule whose subject is the text of a comment or of a directive, such as RH3201, RH7301, or a documentation rule (RH8###), checks that text as its subject."
- SHARED-DIAG-003 — new example, so the exemption has an example on each side (FC-002):
  - The rule-subject side: RH3201 reports a `//` comment with no text, located on `//`. I confirmed this passes today with a throwaway test at `9245af9`.
  - The code-rule side: the existing RH6002 example, which does not report ` ,` inside a `//` comment.
- SHARED-DIAG-002 — unchanged.
- The using-directive case (RH7201, RH7203, RH7207) never appeared in the spec and still does not. It is tracked as bug thoenissen/Reihitsu#1105.

Checks: `scripts/test.sh --project architecture`, filtered to `SpecConsistencyTests`, `SpecStructureTests`, and `RequirementCoverageTests`, gives 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, and only because SHARED-DIAG-002 and SHARED-DIAG-003 have no covering tests yet. The test implementer adds those tests.

## Examples for the tests

- SHARED-DIAG-002: the RH8101 block and the RH3103 block.
- SHARED-DIAG-003:
  - the RH6002 fix pair;
  - the RH6003 fix pair;
  - the RH5023/RH6021 negative block;
  - the new RH3201 block.

Every example passes today. As before, the issue's own examples appear in the spec only as prose with no rule named. They break the requirements today and are tracked in #1071, #1074, #1075, and #1105. Do not turn them into tests in this run.

## Open questions to file

_None._ The maintainer decided the using-directive case from `01-spec-author.md` is covered by thoenissen/Reihitsu#1105. No open-questions issue is filed.
