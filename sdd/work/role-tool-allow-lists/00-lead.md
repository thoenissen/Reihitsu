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

1. Restricting the main session (clarification 1). Options asked: A — committed `.claude/settings.json` `permissions.deny` for `Artifact*`, `DesignSync`, `mcp__Claude_Docs__*`, `mcp__ccd_*`, the plugin/skill suggestion tools, and every `mcp__claude-code-remote__*` tool except `subscribe_pr_activity`, `unsubscribe_pr_activity`, `read_documentation`; B — as A, but also keep the environment tools `get_environment`, `update_environment`, `create_environment`, `switch_environment`, `request_credential`; C — no restriction, follow-up issue. **Answer: B.**

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Skip the test implementer? | Skip | Tooling lane runs it only when the tool has tests; no test covers `.claude/agents/**` or settings | Tooling only | — |
| 2 | Start | Take the main session's tool restriction (clarification 1) into the run? | Yes, as an investigated part: the implementer establishes whether a repository-level setting can remove tool definitions from the main session's context without removing a tool any repository workflow (lead, coordinator, triage, other commands) uses, and implements it only if it can and the lead's and commands' needs stay met; otherwise it reports the mechanism and limits and the run states it | The maintainer asked; same mechanism and motivation as the issue | Tooling only; no end-user surface | — |
| 3 | Start | Read-only roles and `Edit` (clarification 3) | Verified by the implementer against the role files: each role must keep every tool its role file's steps need, including amending its own handover | Decided on evidence, not assumed | Tooling only | — |
| 4 | Implementer 02 | `list_environments` and `SuggestConnectors`/`ListConnectors`/`SearchMcpRegistry`/`ListPlugins`/`ListSkills`/`SearchSkills` — deny or keep? | As implemented: `list_environments` denied, the others kept | Answer B names exactly the tools kept and denied; neither group is named, and the implementer followed the asked list literally | Tooling only | adb377d |

## Follow-up candidates

- Inputs of `squad/roles/implementer.md`: name the issue as an input in the tooling lane (01-implementer).

## Worklist / next step

- Done: implementer (01-implementer.md, commits 1665b05, 617a956; lane check confirmed tooling).
- Maintainer answered B (main-session deny list).
- Done: implementer 02 (adb377d, 7625038).
- Done: cleaner (34f452b, clean).
- Done: spec checker (6da6468, consistent).
- Done: auditor (05-auditor.md, bbb783b, pass). Audited SHA: c2d6deb5b37b5e921e81633db4900c5f6382da6a against origin/main 4bbb3ceb.
- Lead observation on auditor hint 3: in this cloud session, once `.claude/settings.json` was written, the harness withdrew the denied `mcp__Claude_Docs__*` and `mcp__claude-code-remote__*` tools mid-session with the reason "Denied by a permission rule" — the cloud honours the deny list for its built-in servers.
- Next: validator (06-validator.md), then issue manager for the follow-up candidates.
