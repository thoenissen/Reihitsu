Role: Lead
Commit: 78af33e
Verdict: run started

# Run: thoenissen/Reihitsu#997 — SDD analyzer specs RH3001–RH3208 (clarity)

- Issue: thoenissen/Reihitsu#997 (parent thoenissen/Reihitsu#899, question tracker thoenissen/Reihitsu#912)
- Slug: `clarity-specs`
- Branch: `claude/focused-fermi-dqr6b4` (the session's designated branch; the playbook's `claude/issue-<N>-<slug>` naming is not used because this session may push only to its designated branch). Based on `origin/main` 78af33e.
- Claim: draft PR thoenissen/Reihitsu#1167.

## Lane

Specs and tests for existing behavior (`tests`): the issue asks for specs of existing rules plus covering tests; no production code may change.

Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

## Scope

Rules: RH3001–RH3007, RH3101–RH3107, RH3201, RH3203–RH3208 (21 rules). One spec per rule under `sdd/specs/analyzer/`.

## Maintainer's clarifications (summarized)

- Shared specs only where more than one spec relies on the same requirement: (a) the expression-body → block behavior (may take `ExpressionBodyRewriteUtilities` out of RH3202's code owners), (b) the empty-parentheses behavior (RH3106/RH3107, `EmptyParenthesesAnalyzerBase<TNode>`).
- Moving an existing requirement out of `sdd/specs/analyzer/RH3202.md` retires its ID: the lead escalates to the maintainer before that happens. A role that finds it necessary reports it and stops on that point.
- Ownership of `BlankLineCodeFixUtilities` (used by RH3208 and RH5030) is the open question thoenissen/Reihitsu#1067: escalate before claiming it in any spec.
- thoenissen/Reihitsu#1143 covers the second Fix All pass of RH3001, RH3002, RH3004, RH3102: that is current behavior, not a requirement; tests keep it as is.
- thoenissen/Reihitsu#963 (comment after `=>` glued to the new `{`) is a known formatter bug: do not file it again; describe current behavior only where it is not that defect.
- RH3201 uses only SHARED-DOC (no other shared spec).
- `SyntaxNodeUtilities` is also used by batch thoenissen/Reihitsu#991: neither batch claims it as a code owner.
- Other squads run in parallel (thoenissen/Reihitsu#984, #991, #992, #994, #996): do not touch their rules, specs, or shared files. Text conflicts in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` are resolved by keeping both sides.
- Decided principles of thoenissen/Reihitsu#912 (P1–P3, P5–P13): a case one of them decides is not an open question. A new instance of a principle is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)". P11: a construct the rule does not name is not a question (run-report mention as new-rule candidate only); a gap inside the construct the rule names is filed marked P11. P4 (other analyzer/formatter disagreements) is a normal open question.
- Decided but not implemented behavior stays out of the spec entirely (no requirement, non-goal, or note); the test pinning current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue, not a new question.
- Open questions: one `SDD open questions: <spec path>` issue per spec, sub-issue of thoenissen/Reihitsu#912, filed by the lead and linked from the PR. Never written into a spec.
- Must not: change production code or behavior; fix any bug; retire/move/renumber an existing requirement ID without escalation; claim a shared utility another batch or spec owns without escalation; create issues other than "SDD open questions" issues without the maintainer's approval of the exact text.
- Done: a spec per rule, IDs registered, every requirement covered by tagged tests, rule pages corrected only where wrong, open questions filed, CI green, PR `Closes #997`.
