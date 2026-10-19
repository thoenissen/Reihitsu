Role: Lead
Commit: 78af33e
Verdict: lane claimed — Specs and tests for existing behavior

# Run

- Issue: thoenissen/Reihitsu#996 (SDD analyzer specs: RH0002–RH2109). Parent thoenissen/Reihitsu#899, question tracker thoenissen/Reihitsu#912.
- Slug: `sdd-specs-rh0002-rh2109`.
- Branch: `claude/quirky-lamport-mb0j4d` (the session's designated branch, used instead of `claude/issue-996-<slug>` because the session environment only permits pushing to it). Draft PR: thoenissen/Reihitsu#1166, based on `main`.
- Lane: **Specs and tests for existing behavior** — the run writes specs for existing rules and tags/adds covering tests; no production change.
- Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production code in this lane), auditor (the lane table does not run it).

# Rules of this batch

RH0002, RH1001, RH1002, RH2001, RH2002, RH2003, RH2004, RH2006, RH2007, RH2101, RH2102, RH2103, RH2104, RH2105, RH2106, RH2107, RH2108, RH2109 (18 rules). One spec per rule at `sdd/specs/analyzer/RH####.md`.

# Maintainer clarifications (summarized; they override the issue text)

1. Shared specs only where more than one spec relies on the same requirement. A shared struct-equality spec for RH1001/RH1002 (`StructEqualityPerformanceAnalyzerBase`) is written only if both specs rely on it; same test for the nested-type rules.
2. Read also: the casing note on #996 (from the run of #978), `sdd/specs/shared/casing.md`, `sdd/specs/analyzer/RH2005.md`, thoenissen/Reihitsu#971, thoenissen/Reihitsu#967, thoenissen/Reihitsu#1137.
3. RH2001's spec lists the shared casing requirement IDs it relies on under `Uses` and does not restate how the created field name is derived. thoenissen/Reihitsu#971 (leading acronym run) also affects the field name RH2001's fix creates: that is a further instance of an existing follow-up — the lead comments on #971; it is not a new open question and is not written into the spec.
4. `DeclarationModifierUtilities` is owned by `analyzer/RH2005.md` (and thoenissen/Reihitsu#967 concerns it): reference it, do not claim it as a code owner. `ModifierOrderingUtilities` is also used by a later batch (thoenissen/Reihitsu#993): do not claim it in this run.
5. SHARED-FIX-003 (`sdd/specs/shared/code-fixes.md`) now decides cases that earlier specs wrote as non-goals (thoenissen/Reihitsu#1137). Do not write such cases as non-goals.
6. Other squads run at the same time (#984, #991, #992, #994, #997). Do not touch their rules, specs, or shared files. Expect text conflicts only in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md`; resolve by keeping both sides.
7. Decided principles (#912). A case one of them decides is not an open question:
   - P1 a fix that could break code is withheld (SHARED-FIX-003, incl. the readings recorded in `squad/decisions/`).
   - P2 names dictated from outside are not reported. P3 names are compared by value (SHARED-DIAG-002).
   - P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
   - P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
   - P8 documentation is what the compiler produces; P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text.
   - P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application.
   - P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (list it in the handover as a new-rule candidate, no issue). A gap inside the construct the rule names is the rule's own gap: filed marked "Decided by principle P11 (#912)".
   - P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike.
   - P4 any other analyzer/formatter disagreement is decided case by case → open question.
8. Decided but not implemented: the spec states current behavior only. Behavior decided but not implemented stays out of the spec (no requirement, no non-goal, no note); the test pinning current behavior is a characterization test. A further instance of an existing follow-up issue is reported in the handover (the lead comments on that issue) — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".
9. Done means: a spec for every rule, requirement IDs registered in `sdd/specs/requirement-ids.txt`, every spec listed in `sdd/specs/README.md`, every requirement covered by tagged tests, rule pages corrected only where they are wrong, open questions filed per `sdd/README.md` ("SDD open questions: <spec path>", one issue per spec, sub-issue of #912) — the spec author lists them in its handover and the lead files them.
10. Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID; claim ownership of a shared utility another batch or spec owns; put `sdd/work/` into the PR.
