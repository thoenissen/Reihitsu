# Lead

- Issue: thoenissen/Reihitsu#984 (parent thoenissen/Reihitsu#899, question tracker thoenissen/Reihitsu#912)
- Slug: `parameter-return-inheritdoc-docs`
- Branch: `claude/charming-ramanujan-87ngle` (the session's designated branch, used instead of `claude/issue-984-<slug>` because the session may push only there), based on `origin/main` 78af33e
- Claim: draft PR thoenissen/Reihitsu#1162
- Lane: **Specs and tests for existing behavior** — the issue asks for specs of existing rules and their covering tests; no production change.
- Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator. Skipped: implementer (no production code in this lane), auditor (not part of this lane).

## Batch

RH8101–RH8106, RH8108–RH8111, RH8201–RH8204 (14 rules). One spec per rule under `sdd/specs/analyzer/`.

## Maintainer clarifications (summarized)

- Reuse `sdd/specs/shared/element-documentation.md` (SHARED-DOC). New SHARED-DOC IDs continue after the highest existing one (currently SHARED-DOC-013). thoenissen/Reihitsu#985 extends the same spec later, not in parallel.
- Also read: `sdd/specs/shared/element-documentation.md`, `sdd/specs/analyzer/RH8107.md`, thoenissen/Reihitsu#1091, #1153, #1132, `squad/decisions/2026-10-18-shared-doc-coverage-and-extension-block.md`.
- thoenissen/Reihitsu#1153 (P8b, P8c) already decides how `name` is matched (case, namespace, white space, escapes, unnamed parameters) and what counts as text inside `<param>`, `<returns>`, `<typeparam>`. A case of that kind is an instance of #1153 (the lead posts a one-line comment there), not a new question. P8 shapes (`<inheritdoc>` case or namespace, dropped comments, merged partial parts) are instances of #1091 in the same way.
- thoenissen/Reihitsu#1132: check whether RH8101/RH8108 and RH8102/RH8109 match `name` the same way. Where they read it through SHARED-DOC-012, reference that ID under `Uses`. A difference between them is an open question.
- Moving a requirement out of `analyzer/RH8107.md` (for example the one owning `DocumentationCommentCodeFixUtilities`) retires its ID and must be escalated to the maintainer first. `XmlDocumentationElementOrderingUtilities` (used by RH8204, and by RH8309 in #985) is not claimed for a shared spec in this run; it is referenced from RH8204 only.
- Concurrent squads run on other rule ranges (#991, #992, #994, #996, #997). Do not touch their rules, specs, or shared files. Only text conflicts in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` are expected; they are resolved by keeping both sides.
- Decided principles from #912 (P1–P3, P5–P13) decide their cases; such a case is not an open question. A new instance of a principle is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)". P4 (other analyzer/formatter disagreement) stays an open question. Under P11, a construct the rule does not name is a new-rule candidate for the run report only (no issue); a gap inside the construct the rule names is filed marked P11.
- Decided but not yet implemented behavior stays out of the spec entirely; the test pinning current behavior is a characterization test.
- Must not: change production code or behavior; fix any bug; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalation; claim a shared utility another spec or batch owns without escalation; leave `sdd/work/` in the PR. No issues other than "SDD open questions: <spec path>" issues (one per spec, sub-issue of #912) without the maintainer approving the exact text.

## Open-question issues of touched specs

No open "SDD open questions" issue exists for `shared/element-documentation.md` (#1037, #1130 closed) or `analyzer/RH8107.md`.

## Lane widened

After the decision record `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md` was committed, `scripts/lane-check.sh tests --head worktree` reported `mismatch — claimed tests, diff needs tooling` (the record lives under `squad/`). This is before the implementer stage, so the lane widens tests → tooling, as the RH8020–RH8034 batch did. Roles now: spec author and skeptic (done), test implementer, implementer only if a test fails against the unchanged code (production code stays untouched either way), cleaner, spec checker, auditor (added by the tooling lane), validator.

## Escalation answered

RH8202 keeps RH8107-006 and RH8107-007 under `Uses`; nothing moves or retires (decision record above).
