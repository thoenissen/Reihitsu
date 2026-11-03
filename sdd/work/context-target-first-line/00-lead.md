Role: lead
Commit: 21c2ff4ba9e1b1846c39452468e2802c28c19ec8
Verdict: run started

# Run: context-target-first-line

## Issues

- thoenissen/Reihitsu#1501 — "FMT-PIPE-017: the context entry point's target keeps the phases' first line". Labels: `enhancement`, `formatter`, `agent-filed` (goals, "Done when", target behavior, and prescribed solution are proposals). Sub-issue E1 of thoenissen/Reihitsu#1482. Blockers thoenissen/Reihitsu#1487 (B2) and thoenissen/Reihitsu#1489 (B4) are closed (merged in PRs #1510 and #1520).
- The issue was written against an older `main`; every statement about today's code is checked against current `main` (base above) before it is used.

## Lane

Behavior change. The issue asks for changed formatter behavior of `FormatNodeInDocumentWithContextAsync` and changed requirements FMT-PIPE-017/FMT-PIPE-018, and asks for a change doc.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when there are follow-up candidates. No role is skipped.

## Maintainer's clarifications (summarized)

- The node-level formatting methods (`FormatNodeInDocumentWithContextAsync` and its siblings) are not a public API. Only the cases the analyzer code fixes actually reach have to be covered — inputs that only a direct call of the method would produce are out of scope (AGENTS.md, *Supported inputs*). Every role names the code fix (and the node it formats / the context it passes) that reaches each case it specifies, tests, or reports.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/stoic-bohr-xb3ycb` instead of `claude/issue-1501-<slug>` | The session's environment pins the development branch; pushing elsewhere is not permitted | Branch names are internal | claim commit |

## Follow-up candidates

None yet.
