Role: lead
Commit: 78af33e
Verdict: lane claimed — tests (specs and tests for existing behavior)

# Run

- Issue: thoenissen/Reihitsu#992 — SDD analyzer specs for RH5409–RH5417 (trailing commas, empty type declarations). Parent thoenissen/Reihitsu#899; question tracker thoenissen/Reihitsu#912.
- Slug: `rh5409-rh5417-specs`
- Branch: `claude/pensive-cerf-chrp89` (the session's designated branch, used instead of `claude/issue-992-<slug>` because the session may push only there). Draft PR: thoenissen/Reihitsu#1164, based on `main`.
- Lane: tests ("Specs and tests for existing behavior"). Reason: the issue asks for specs of existing rules plus tagged covering tests; no production change is allowed.
- Roles: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator. Skipped: implementer (no production change in this lane), auditor (lane table skips it).

# Maintainer clarifications (summarized)

- Scope: one spec per rule RH5409–RH5417 under `sdd/specs/analyzer/`. A shared spec for trailing commas or for empty type declarations only when more than one spec relies on the same requirement.
- `TrailingCommaRemovalUtilities` and `EmptyTypeDeclarationSemicolonTransform` (and the other structural transforms) are owned by `sdd/specs/formatter/structural-transforms.md`. Reference its FMT-STRUCT IDs under `Uses`; never list those types as code owners of a new spec.
- No existing follow-up issue names these rules. Thoroughly check whether any fix that delegates to the formatter produces a no-op when the formatter refuses: that is a further instance of thoenissen/Reihitsu#1150 and gets a one-line comment by the lead on that issue, not a new question.
- Other squads run concurrently on RH8101–RH8204, RH5301–RH5408, RH7201–RH7311, RH0002–RH2109, RH3001–RH3208: do not touch their rules, specs, or shared files. Only text conflicts in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` are expected (keep both sides).
- Decided principles from thoenissen/Reihitsu#912 (a case one of them decides is not an open question; a new instance is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)" with the violated requirement):
  - P1 a fix that could break code is withheld (SHARED-FIX-003 and its readings in `squad/decisions/`).
  - P2 names dictated from outside are not reported; P3 names compared by value (SHARED-DIAG-002).
  - P5 only the text of a literal or comment is content (SHARED-DIAG-003); P6 a `/** */` that documents nothing is a block comment.
  - P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
  - P8/P8b/P8c documentation is what the compiler produces / binds / renders.
  - P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application.
  - P11 the formatter may do more than the analyzers: a construct the rule does not name is not a question (run-report new-rule candidate only); a gap inside the construct the rule names is filed marked "Decided by principle P11 (#912)".
  - P12 a line-leading comment is the line's first element; P13 every ordinary comment kind is treated alike.
  - P4 any other analyzer/formatter disagreement is an open question.
- Decided but not implemented behavior stays out of the spec entirely; the test pinning current behavior is a characterization test.
- Must not: change production code or behavior; fix any bug; write an open question into a spec; retire/move/renumber an existing requirement ID; claim a shared utility another spec owns; leave `sdd/work/` in the PR. No issues other than "SDD open questions: <spec path>" (sub-issue of #912) without maintainer approval of the exact text.
- Done: spec per rule, IDs registered in `sdd/specs/requirement-ids.txt` and listed in `sdd/specs/README.md`, every requirement covered by tagged tests, rule pages corrected only where wrong, open questions filed and linked, CI green, PR `Closes #992`.
