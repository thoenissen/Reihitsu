# Lead — role tool allow-lists

- Issue: thoenissen/Reihitsu#1979 — `agent-filed` (goals are proposals).
- Slug: `role-tool-allow-lists`
- Branch: `claude/sleepy-lamport-eq940r` (session-designated); draft PR thoenissen/Reihitsu#1980.
- Lane: `tooling` — the change touches `.claude/agents/**` (and possibly `.claude/settings.json`, `squad/**`), all tooling paths in the lane-check table.
- Roles run: implementer → cleaner → spec-checker → auditor → validator.
- Roles skipped: spec author and skeptic (the tooling lane has neither); test implementer (no test covers the agent definitions or Claude Code settings — `rg` over `tests/`, `src/` finds none apart from the lane classifier's path table).

## Maintainer clarifications (summarized)

1. The maintainer asks whether the main session's (the lead's) tools can be restricted as well.
2. The maintainer is not sure the allow-lists the issue proposes are correct; they are to be verified, not copied.
3. The maintainer asks whether skeptic, spec checker, and auditor need `Edit` for their handovers under `sdd/work/<slug>/`.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Skip the test implementer? | Skip | Tooling lane runs it only when the tool has tests; no test covers `.claude/agents/**` or settings | Tooling only | — |
| 2 | Start | Take the main session's tool restriction (clarification 1) into the run? | Yes, as an investigated part: the implementer establishes whether a repository-level setting can remove tool definitions from the main session's context without removing a tool any repository workflow (lead, coordinator, triage, other commands) uses, and implements it only if it can and the lead's and commands' needs stay met; otherwise it reports the mechanism and limits and the run states it | The maintainer asked; same mechanism and motivation as the issue | Tooling only; no end-user surface | — |
| 3 | Start | Read-only roles and `Edit` (clarification 3) | Verified by the implementer against the role files: each role must keep every tool its role file's steps need, including amending its own handover | Decided on evidence, not assumed | Tooling only | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: implementer (01-implementer.md).
