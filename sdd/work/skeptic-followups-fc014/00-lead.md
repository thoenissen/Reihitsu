Role: lead
Commit: c857b2d97e7d4822cc9adebd0388b4953de5ebd7
Verdict: run started

## Run

- Issues, in prompt order: thoenissen/Reihitsu#1423, thoenissen/Reihitsu#1424.
- Slug: `skeptic-followups-fc014`.
- Branch: `claude/determined-ramanujan-rm3ow3` (the session's mandated branch, not `claude/issues-1423-1424-<slug>`). Draft PR: thoenissen/Reihitsu#1425, based on `main`.

## Lane

Tooling, for both issues.

- #1423 changes agent instructions (`squad/roles/skeptic.md`, handover template) — tooling.
- #1424 changes agent instructions (`squad/failure-classes.md`, `squad/roles/skeptic.md` check 3) — tooling.

Roles run: implementer (writes the instruction text), cleaner, spec checker, auditor, validator.
Roles skipped: spec author and skeptic (the tooling lane does not run them); test implementer (no test covers these Markdown files).

## Maintainer's clarifications (summarized)

- #1423: add a `## Follow-up candidates` section, with `_None._` when empty, to the handover template in `squad/roles/skeptic.md`.
- #1424: narrow FC-014 in `squad/failure-classes.md`, and the FC-014 sentence of check 3 in `squad/roles/skeptic.md`, to layout concerns; cite SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md`) as the reason an order or content rule whose concern no formatter phase owns is intended and not this failure class. The FC-013 wording that thoenissen/Reihitsu#1421 just wrote stays as it is.
- Do not change other failure classes or other skeptic checks; no code, spec, or test changes; nothing under `sdd/` other than this run's `sdd/work/` handovers, which are deleted before publishing.
- A parallel run (thoenissen/Reihitsu#998) touches `RequirementCoverageTests`, `sdd/README.md`, and `squad/roles/test-implementer.md`; do not touch those files.
- No issue is created without the maintainer's approval of its exact text.
