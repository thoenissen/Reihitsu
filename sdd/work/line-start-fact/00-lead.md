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
