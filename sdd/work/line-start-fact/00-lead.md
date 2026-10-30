# Lead — line-start-fact

- Issue: thoenissen/Reihitsu#1487 (formatter restructuring package B2 of thoenissen/Reihitsu#1482)
- Slug: `line-start-fact`
- Branch: `claude/great-archimedes-xwz86i` — the session that runs the squad is bound to this branch and may not push to another, so the playbook's `claude/issue-<N>-<slug>` name is not used.
- Draft PR: thoenissen/Reihitsu#1510
- Blocker thoenissen/Reihitsu#1485 (B1) is closed, merged with thoenissen/Reihitsu#1508; `main` at 802afbf.

## Lane

Behavior change. The issue requests changed formatter behavior (FMT-INDENT-002 places the line behind a `///` written behind code; FMT-SPACE-015 is retired; FMT-DOC-003's second example changes) and states that a change doc under `sdd/changes/` is required. Two of its cases (FMT-INDENT-015 zero-width tokens, FMT-INDENT-006 comment interiors) are bugs against existing requirements and ride in the same run.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Clarifications from the maintainer

None so far; the issue has no comments. The issue asks the run to first check every statement about today's code against current `main` and drop what an earlier package already made true or moot.

## Escalations

- Escalation 2 — change doc approved as written, analyzer consequences included (`squad/decisions/2026-10-30-line-start-fact-change-doc.md`).
- Escalation 12 — skeptic's architecture finding: option B. The line-break rewriters for conditionals, conditional-access chains, and do-while, and the line-terminator count over disabled text, are routed through the shared line-start fact and terminator predicate in this run, with tests (`squad/decisions/2026-10-30-line-start-fact-architecture-finding.md`).

## Scope baseline (escalation 5)

The issue's Proposed Behavior and Done when — including its restructuring items: the shared facts in `Pipeline/Core`, the neutral-helper moves, the `ConfigurableStructuralTransforms` move to `Data/`, the `ReihitsuFormatterHelpers` split, and the one-contributor-per-line test — plus the components added under escalation 12.
