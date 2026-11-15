Role: spec checker
Commit: 0ccec8d
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1680

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok. No change doc; a bug fix against unchanged one-diagnostic requirements (RH8204-002, RH8304-003, RH8305-003, RH8307-003, RH8308-003, RH8309-003, RH7302-003, RH7310-003, SHARED-REGDESC-002), no requirement added, changed, or retired.
2. ok. No spec text changes for this issue; no requirement contradicts the diff. RH8305-003 and RH8307-003 already state "reported once" for a type with a primary constructor; the parameter-list case (0 reported before) now matches them.
3. ok. No new ID for this issue.
4. ok. The rename to `AnalysisContextExtensions` is reflected in `sdd/specs/shared/element-documentation.md` (header) and `sdd/specs/README.md` (SHARED-DOC row); no old name left under sdd/specs, documentation, src, squad, or .claude (grep).
5. ok. No rule page affected; output returns to the specs.
6. ok. No output question open; none written into a spec.
7. ok. Diff touches sdd/specs: `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. No new requirement.
8. ok. Every added test (35 across 12 classes) carries `[Requirement]` tags matching the test implementer's table (tag counts per class checked against the table).
9. ok for now. Only `sdd/work/primary-and-partial-constructors/` is under sdd/work; the lead deletes it before publishing.

## thoenissen/Reihitsu#1742

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok. Maintainer answer 1B recorded in 00-lead.md; SHARED-DIAG-013 writes down today's behavior (clarification, no output change), so no change doc is required, and the lead recorded the decision (6).
2. ok. SHARED-DIAG-013 in `sdd/specs/shared/diagnostics.md` has the normative sentence first, `Why:`, four examples with analyzer markup; no production change contradicts it.
3. ok. `SHARED-DIAG-013` is the next free number after SHARED-DIAG-012 and sits at its sorted position in `sdd/specs/requirement-ids.txt`; nothing reused or renumbered.
4. ok. Header owners of diagnostics.md unchanged (`DiagnosticAnalyzerBase`, `PartialMemberUtilities`) and exist (architecture test passes); README row matches.
5. ok. No rule page changed or needed (spec author internal choice 2).
6. ok. The spec author's output questions: none; the skeptic's partial-constructor output question has the answer under `Maintainer answers`; RH8017 etc. question decided by unchanged specs (decision 5); RH8102 candidate is under `Follow-up candidates`.
7. ok. Requirement SHARED-DIAG-013 is new; covered by tests tagged `[Requirement("SHARED-DIAG-013")]` in RH4122 (3), RH7302 (1), RH2004 (1). Architecture result as above. Whether SHARED-DIAG is an enforced spec was not needed: coverage exists either way.
8. ok. All four spec examples (RH4122 twice, RH7302, RH2004) are tagged on added tests.
9. ok, as for #1680.
