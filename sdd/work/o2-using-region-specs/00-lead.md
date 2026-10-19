Role: Lead
Commit: 78af33e (origin/main at claim)
Verdict: Lane claimed — specs and tests for existing behavior

# Run

- Issue: thoenissen/Reihitsu#994 (SDD analyzer specs: RH7201–RH7311, using directives and regions). Parent thoenissen/Reihitsu#899; question tracker thoenissen/Reihitsu#912.
- Slug: `o2-using-region-specs`. Branch: `claude/optimistic-turing-vxue9i` (the session's designated branch, used instead of `claude/issue-994-<slug>`). Draft claim PR: thoenissen/Reihitsu#1165, based on `main`.
- Rules: RH7201–RH7207, RH7301–RH7311, RH7305A (19 rules), one spec each at `sdd/specs/analyzer/RH####.md`.

# Lane

Specs and tests for existing behavior: the run writes specs for existing rules and tags or adds the tests that cover their requirements. No production code changes.

Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

# Maintainer clarifications (summarized)

- Reuse `sdd/specs/shared/using-directive-ordering.md` (SHARED-USING) for the using rules; reference FMT-USING / FMT-REGION IDs where the formatter owns the policy. A shared spec for region descriptions (or for `TypesOrganizedWithRegionsAnalyzerBase`) only if more than one spec relies on the same requirement.
- `RegionDirectiveUtilities` is owned by `formatter/region-formatting.md` and `OrderingDeclarationUtilities` by `shared/ordering-move-safety.md`: reference them, never claim them as code owners. Add no requirement to `shared/ordering-move-safety.md` — that needs escalation to the maintainer.
- Already filed, not to be filed again: #964, #965, #966, #1083. Further instances of:
  - #1105 (RH7201/RH7203/RH7207 compare names by spelling, principle P3) → report to the lead; the lead comments on #1105.
  - #1151 (RH7302 renames the `#endregion` comment), #1135 (RH7309 changes static initializer order, SHARED-ORDER-007), #1143 (RH7310 needs two Fix All passes) → current behavior, not a requirement; report further instances to the lead.
- Thread #995 (member grouping by regions) runs after this batch; do not touch it.
- Decided principles from #912 (P1–P3, P5–P13) decide their cases; such a case is not an open question. A new instance of a principle is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)". P4 (other analyzer/formatter disagreements) stays a normal open question. Under P11 a construct the rule does not name is a new-rule candidate for the run report, not an issue; a gap inside the construct the rule names is filed marked P11.
- Decided-but-not-implemented behavior stays out of the spec entirely (no requirement, non-goal, or note); the test pinning current behavior is a characterization test.
- Open questions: one `SDD open questions: <spec path>` issue per spec, sub-issue of #912 — collected in handovers, created by the lead. No other issues without maintainer approval of the exact text.
- Must not: change production code or behavior, fix any bug, write an open question into a spec, retire/move/renumber an existing requirement ID without escalating, claim a shared utility another spec owns, put `sdd/work/` into the PR.
- Parallel squads own RH8101–RH8204 (#984), RH5301–RH5417 (#991, #992), RH0002–RH3208 (#996, #997): do not touch their rules, specs, or shared files. Expected conflicts only in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` (keep both sides).
- Done: a spec per rule, IDs registered, every requirement covered by tagged tests, rule pages corrected only where wrong, open questions filed and linked, CI green, PR `Closes #994`.
