Role: spec checker
Commit: 18bd1c4191bb60da15cd2c31c38c59eb3b5b687f
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane claimed: tooling (widened from tests). `scripts/lane-check.sh tooling --head worktree`: "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

Observation, not a finding: `sdd/specs/analyzer/RH5601.md` RH5601-001 leaves a tab next to a string or character literal unreported, which conflicts with the SHARED-DIAG-003 bullet "the whitespace before and after a literal is code". `00-lead.md` and `03-skeptic.md` list RH5601 among the rules tracked in thoenissen/Reihitsu#1071, #1074 and #1075, and `04-spec-author.md` keeps the case out of the spec's examples. No test in this run asserts it. The lead may want to confirm the RH5601 case is named in one of those issues.

## Checked
1. Change doc: ok. The diff adds no change doc; both requirements write down behavior that already exists (maintainer decision, `00-lead.md`), so this is a clarification. No retired, renamed or removed rule, phase or surface.
2. Specs match the diff: ok. The diff changes no production code; `git diff origin/main -- src` has only added lines (test methods and `[Requirement]` tags). Both requirements have the normative sentence first, `Why:`, and diagnostics-only examples with `{|RH####:…|}` markup. The literal-value, comment-subject and fix-clause decisions are reflected: no fix clause, new non-goal pointing to `code-fixes.md` (file exists), exemption paragraph for comment, directive and literal subjects.
3. IDs: ok. SHARED-DIAG-002 and SHARED-DIAG-003 are the next free numbers after SHARED-DIAG-001, inserted in sorted order before SHARED-DOC-001 (`LC_ALL=C sort -c` passes); no ID reused, renumbered, or deleted.
4. Code owners: ok. The header owner `DiagnosticAnalyzerBase` is unchanged and exists; `sdd/specs/README.md` line 25 lists `shared/diagnostics.md` with `SHARED-DIAG` and `DiagnosticAnalyzerBase`. No paths or line numbers.
5. User documentation: ok. `documentation/rules/` is untouched. The pages of the rules the examples name (RH2003, RH3201, RH5023, RH5202, RH5601, RH6002, RH6003, RH8101) exist; the RH5601 page already says tabs in literal and comment content are left alone, consistent with the new examples.
6. Open questions: ok. `04-spec-author.md` lists none. The using-directive case from `01-spec-author.md` is filed as thoenissen/Reihitsu#1105 (`squad/decisions/2026-10-18-using-rules-name-by-value-filing.md`); the lead lists #1105 under follow-up work in the PR.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72". SHARED-DIAG-002 is covered by tests in `RH8101…AnalyzerTests` and `RH2003…AnalyzerTests`; SHARED-DIAG-003 by tests in the RH5601, RH5023, RH6002, RH6003, RH3201 and RH5202 analyzer test classes (13 new tests, 16 existing tests tagged). Every example block of the spec has a test in the diff. No source comment or name in the diff carries a `TODO` or a tracker number.
8. Nothing left behind: ok for now. The branch holds `sdd/work/name-value-literal-content/` only (the lead deletes it before publishing); `sdd/work/coordinator/` is untouched. The three `squad/decisions/` records are intended to stay.
