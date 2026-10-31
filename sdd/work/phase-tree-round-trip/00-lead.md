Role: lead
Commit: 57f0f45
Verdict: run started

# Run: thoenissen/Reihitsu#1488

- Issue: thoenissen/Reihitsu#1488 — every formatter phase returns a tree that round-trips and keeps foreign annotations (package B3 of thoenissen/Reihitsu#1482).
- Slug: `phase-tree-round-trip`
- Branch: `claude/vibrant-lamport-dcba1q` (the session's designated branch; used instead of the playbook's `claude/issue-1488-<slug>` name because the session may push only to that branch).
- Draft PR: thoenissen/Reihitsu#1513
- Blocker thoenissen/Reihitsu#1487 (B2) is closed, merged through thoenissen/Reihitsu#1510.

## Lane

Behavior change. The issue requests new and changed specified behavior — a new FMT-PIPE requirement, a changed FMT-STRUCT non-goal, a new FMT-BREAK residue statement — and states that a change doc is required. It also contains bug-shaped parts (the context entry point returning the document unchanged for a target inside a moving raw-string literal), which fall under the same change doc.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications

None so far. The issue asks the run first to check every statement about current code against current `main`, since it was written against `735d0dba`; a statement already made true or moot is noted and dropped, and a difference that changes the target goes to the maintainer.
