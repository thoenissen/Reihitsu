Role: Lead
Commit: e71aff7
Verdict: lane claimed — tests

# Run: thoenissen/Reihitsu#1138

- Issue: thoenissen/Reihitsu#1138 — record the fix-scope and fix-layout principles in `sdd/specs/shared/code-fixes.md`.
- Slug: `fix-scope-and-layout`
- Branch: `claude/ecstatic-cerf-pwolo0`. The session can push only to this branch, so the run does not use the playbook's `claude/issue-<N>-<slug>` name. Draft PR: thoenissen/Reihitsu#1357, based on `main`.
- Lane: **Specs and tests for existing behavior** (`tests`). The run adds requirements to the enforced spec `shared/code-fixes.md`, so each new requirement needs a covering test in this PR. The issue calls this the docs/spec lane, but under `squad/README.md` an enforced spec that gains requirements widens docs to tests.
- Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production code may change), auditor (not part of the tests lane).

## Maintainer's clarifications for this run (summarized)

- Scope: in `sdd/specs/shared/code-fixes.md`, write the new SHARED-FIX-006 (P9 fix scope, plus P14a and P14d), SHARED-FIX-007 (P10 fix layout, plus P14b and P14c) and SHARED-FIX-008 (P24 line ending). Also amend SHARED-FIX-002 (the Fix All clarification with the RH7103 example) and SHARED-FIX-003 (the P23 sentence under "The diagnostic is still reported.", and replace its RH3202 layout example with "two blank lines above a get-only method stay two blank lines"). The `Why:` of FMT-PIPE-010 in `sdd/specs/formatter/pipeline.md` may reference SHARED-FIX-006 instead of restating it. Nothing else. The issue body says where each P14 item goes.
- The principle texts in the issue body are decided under thoenissen/Reihitsu#912. Write them as stated and do not reopen them.
- Where code does not yet meet a principle, the requirement still states the principle. Its covering tests pin only shapes that already conform. The non-conforming shapes are tracked by follow-up bugs (thoenissen/Reihitsu#1140, #1141, #1142, #1143, #1144, #1263, #1269, #1283, #1284, #1335, #1342). Write no open question and no characterization test that contradicts the requirement for them.
- If a new requirement has no conforming shape to test at all, report it as blocking. The lead escalates before it is written.
- Do not write P26: it narrows SHARED-FIX-007 for moved code and belongs to a later run (thoenissen/Reihitsu#1331).
- IDs: this run may mint only SHARED-FIX-006, SHARED-FIX-007 and SHARED-FIX-008. Register them in `sdd/specs/requirement-ids.txt`. Do not retire, move or renumber an existing ID.
- Do not touch the files other squads are working on in parallel: `shared/comment-placement.md`, `shared/diagnostics.md`, `type-regions.md`, `using-directive-ordering.md`, `element-documentation.md`, `sdd/README.md`, the `Uses` lines of RH51xx/RH52xx specs, and Formatter.Test/Playground.Test tags.
- No production code and no behavior change. Fix no bug, even an obvious one.
- Open questions go to a "SDD open questions" issue under `sdd/README.md`, never into the spec. A case a decided principle covers is not an open question. Create no other issues.
