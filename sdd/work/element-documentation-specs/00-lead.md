Role: Lead
Commit: d83f2ae0b88a68fc626936144a7d7b1e01b861a4
Verdict: lane claimed — specs and tests for existing behavior

# Run: thoenissen/Reihitsu#982

- Issue: thoenissen/Reihitsu#982 — analyzer specs for RH8001–RH8019 (element documentation, part 1), one batch of the backfill under thoenissen/Reihitsu#899.
- Slug: `element-documentation-specs`
- Branch: `claude/dazzling-meitner-iphch4` (the session's designated branch; used instead of `claude/issue-982-<slug>` because the session may push only to it). Claim PR: thoenissen/Reihitsu#1035 (draft).
- Base: `origin/main` at `d83f2ae0b88a68fc626936144a7d7b1e01b861a4`.

## Lane

Specs and tests for existing behavior (lane-check name: `tests`). The issue asks for specs of existing rules plus `[Requirement]` tags on existing tests and focused tests only where a requirement has no covering test. No production code.

Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane per `squad/README.md`).

## Open-question issues on touched specs

None open for `analyzer/RH8107.md` or any element-documentation shared spec at the claim (checked the sub-issues of thoenissen/Reihitsu#912).

## Maintainer clarifications (summarized)

- Read as models: `sdd/specs/shared/casing.md` with `sdd/specs/analyzer/RH4106.md` (a shared spec with member rules), and `sdd/specs/analyzer/RH8107.md`, which today lists the documentation helpers as code owners.
- Create one shared element-documentation spec under `sdd/specs/shared/` covering: which elements are documentable, the accessibility split, what counts as documented, and documentation mode. `SplitElementDocumentationAnalyzerBase` is one of its code owners.
- Code ownership follows the behavior a type implements, not who calls it. The helpers `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `DocumentationModeAnalysisContextExtensions`, `DocumentationCommentCodeFixUtilities` move from RH8107 to a shared spec when that shared spec specifies their behavior; RH8107 then references the shared IDs under `Uses`.
- If moving an existing RH8107 requirement would retire its ID, stop and ask the maintainer first. Do not change any existing requirement of another spec without asking.
- Design the shared spec so the later documentation batches (thoenissen/Reihitsu#983, #984, #985) can reuse it. Do not specify rules of those batches.
- Never write current-but-defective behavior into a spec; such output is filed as an open question (sub-issue of thoenissen/Reihitsu#912) per `sdd/README.md`. Filing those is pre-approved; no other issue may be created without approval.
- Other squads (thoenissen/Reihitsu#979, #980, #986) also edit `sdd/specs/README.md` and `sdd/specs/requirement-ids.txt`; when main moves, merge it and keep both sides' rows.
- Done: 19 rule specs, the shared spec, existing tests tagged with `[Requirement]`, focused tests only for uncovered requirements, every example verified against a test or by running it, open questions filed, CI green, PR based on main without `sdd/work/`.

## Lane widened: tests → tooling

After the spec author, the lead recorded the maintainer's answer on the RH8107 reference clauses in `squad/decisions/2026-10-18-rh8107-shared-doc-reference-clauses.md` (answer: keep the clauses and `Uses`). `scripts/lane-check.sh tests --head worktree` then reported `mismatch — claimed tests, diff needs tooling`, because `squad/` is a tooling path. The implementer had not started, so the lane widens along tests → tooling. The run still has no production change; the spec author's work remains in scope as the tests-lane part of the diff.

Roles now: spec author (done) → skeptic → test implementer → implementer (only if a handed-over test fails against current code; otherwise skipped, since there is no production change to make) → cleaner → spec checker → auditor → validator.

## Escalations and filings so far

- RH8107 reference clauses: maintainer kept them (`squad/decisions/2026-10-18-rh8107-shared-doc-reference-clauses.md`); after the skeptic narrowed SHARED-DOC-004, the maintainer restored RH8107-001 to its `main` text and kept only SHARED-DOC-008 under `Uses` (`squad/decisions/2026-10-18-rh8107-001-keeps-original-text.md`).
- The five open questions of `03-spec-author.md` are filed as thoenissen/Reihitsu#1037 (`SDD open questions: sdd/specs/shared/element-documentation.md`, sub-issue of thoenissen/Reihitsu#912).
