Role: Lead
Commit: 78af33ed1a0c6b1f5963b445ff80e21d5f80a896
Verdict: lane claimed

# Run: thoenissen/Reihitsu#991

- Issue: thoenissen/Reihitsu#991 — SDD analyzer specs for RH5301–RH5308 and RH5401–RH5408 (16 rules). Parent thoenissen/Reihitsu#899; open-question tracker thoenissen/Reihitsu#912.
- Slug: `rh5301-rh5408-specs`
- Branch: `claude/awesome-fermat-tuy84y` (the session's designated branch replaces the playbook's `claude/issue-<N>-<slug>` name; the session forbids pushing elsewhere). Base: `main`, no stacked PRs.
- Claim: draft PR thoenissen/Reihitsu#1163.
- Lane: **Specs and tests for existing behavior** (tests). Reason: the issue asks for specs of existing rules plus the covering, tagged tests; no production code changes. The maintainer named this lane.
- Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

## Maintainer's instructions for this run (summarized)

- Read first: `AGENTS.md` ("Code-fix coverage", "Issue intake"), `sdd/README.md` (esp. "Open questions"), `sdd/specs/shared/code-fixes.md`, `sdd/specs/shared/diagnostics.md`, the newest entries in `squad/decisions/`, and thoenissen/Reihitsu#991 with its comments. Also read thoenissen/Reihitsu#1138, #1140, #1144, #1150, `sdd/specs/shared/line-join-rules.md`, `sdd/specs/shared/token-gap-fixes.md`.
- One spec per rule under `sdd/specs/analyzer/RH####.md`. A shared spec for the statement-braces fix (RH5405–RH5407, `StatementBracesCodeFixProviderBase`) only if more than one spec relies on the same requirement. Shared specs in general only where more than one spec relies on the behavior.
- `SyntaxNodeUtilities` is also used by the concurrent batch thoenissen/Reihitsu#997: neither batch claims it as code owner.
- Specs state current behavior only. Decided but not yet implemented behavior stays out of the spec entirely (no requirement, no non-goal, no note); the test pinning current behavior is a characterization test.
- Decided principles from #912 (a case they decide is not an open question; a new instance is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)"):
  - P1 a fix that could break code is withheld (SHARED-FIX-003 and recorded readings in `squad/decisions/`).
  - P2 names dictated from outside are not reported; P3 names compared by value (SHARED-DIAG-002).
  - P5 only the text of a literal or comment is content (SHARED-DIAG-003); P6 a `/** */` documenting nothing is a block comment.
  - P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
  - P8/P8b/P8c documentation rules (not expected to matter here).
  - P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138).
  - P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (run-report new-rule candidate only). A gap inside the construct the rule names is the rule's own gap, filed marked "Decided by principle P11 (#912)".
  - P12 a comment at the start of a line is that line's first element; P13 every ordinary comment kind is treated alike (both #1139).
  - P4 any other analyzer/formatter disagreement → normal open question.
- Existing follow-ups to route instances to (the lead comments one line on the issue, no new question):
  - #1150: `StatementBracesCodeFixProviderBase` and the RH53xx/RH54xx providers withhold the fix when the formatter returns the document unchanged (P7).
  - #1140 (P9 fix scope) / #1144 (P10 fix layout) — or, for a new instance, "Decided by principle P9/P10 (#912)".
- Open questions: one issue per spec titled "SDD open questions: <spec path>", sub-issue of #912, linked from the PR. Never written into a spec.
- Rule pages under `documentation/rules/` corrected only where wrong.
- Must not: change production code or behavior; fix any bug; retire/move/renumber an existing requirement ID without escalating; claim ownership of a shared utility another batch/spec owns; put `sdd/work/` in the PR. No issues other than "SDD open questions" ones without maintainer approval of the exact text.
- Concurrent squads (do not touch their rules, specs or shared files): #984 (RH8101–RH8204), #992 (RH5409–RH5417), #994 (RH7201–RH7311), #996 (RH0002–RH2109), #997 (RH3001–RH3208). Expected text conflicts only in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md`; keep both sides.
- Done: a spec for every rule, IDs registered, every requirement covered by tagged tests, CI green, PR "Closes #991".
