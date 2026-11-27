Role: implementer
Commit: 1665b05
Verdict: tests pass

## thoenissen/Reihitsu#1979

No test covers `.claude/agents/**`; the evidence is the probes below. My prompt carried no intended fix beyond the run's questions; nothing was ignored.

### Changes

| File | What changed | Requirement |
|---|---|---|
| `.claude/agents/{spec-author,test-implementer,implementer}.md` | `tools: Bash, Read, Edit, Write, Glob, Grep, mcp__github__issue_read` (no restriction before) | Issue goal 1 |
| `.claude/agents/cleaner.md` | `tools: Bash, Read, Edit, Write, Glob, Grep` (no restriction before) | Issue goal 1 |
| `.claude/agents/{skeptic,auditor}.md` | `disallowedTools: Edit, NotebookEdit` replaced by `tools: Bash, Read, Write, Glob, Grep, mcp__github__issue_read` | Issue goal 1, clarification 3 |
| `.claude/agents/spec-checker.md` | `disallowedTools` replaced by `tools: Bash, Read, Write, Glob, Grep` | Issue goal 1, clarification 3 |
| `.claude/agents/issue-manager.md` | `disallowedTools` replaced by base + `mcp__github__issue_read, list_issues, search_issues, issue_write, add_issue_comment, sub_issue_write` | Issue goal 1, question 2 |
| `.claude/agents/validator.md` | unchanged | — |
| `.claude/agents/README.md` | Table column `Denied tools` → `Tools`; new `## Tools` section stating each allow-list and why | Issue goal 3 |

### How the issue's proposed table differs (it is `agent-filed`)

- **GitHub read for five roles.** The issue's table gives only the issue manager GitHub tools. But the lead's prompt names issues by number only (`squad/lead.md` section 4), and the spec author (`roles/spec-author.md`, Inputs: "through the GitHub MCP server"), skeptic, test implementer, and auditor ("Every issue of the run by number") read them; in the tooling lane the implementer works from the issue too (this run's prompt). With the issue's table those five roles would lose access to the issue they work on. They get `mcp__github__issue_read` only (it also reads comments). Cleaner, spec checker, and validator read no issue (their Inputs sections), so they get none.
- **No `ToolSearch`.** Not needed: an MCP tool named in `tools:` loads upfront when `ToolSearch` is not in the list (probe 1).
- **`NotebookEdit`, `Skill`, `SendMessage`, `WebFetch`, agent tool:** no role file uses them. The commands a role file links (`add-resource-texts`, `create-rule-doc`) are links read as files, not `Skill` calls.

### Clarification 3 — do skeptic, spec checker, auditor need `Edit`?

No. Each spawn writes a new numbered handover (`squad/README.md`: `<nn>` counts up); the three are "always fresh" (`squad/lead.md` section 4 table), so they never amend an earlier handover; a role that rewrites its own new handover does so with `Write` after `Read`. Their role files forbid editing any other file (skeptic *Must not*, spec-checker *Must not*, auditor *Must not*). The validator and issue manager likewise edit no existing repository file.

### Question 2 — how a subagent with an allow-list reaches GitHub MCP tools

Sources: code.claude.com/docs/en/sub-agents (`tools` accepts exact `mcp__<server>__<tool>` names and `mcp__<server>__*`; unresolved entries are dropped silently unless nothing resolves), docs/en/mcp ("If you deny `ToolSearch` or leave it out of `--tools`, MCP tools load upfront"), docs/en/tools-reference (`SubagentHandback` is added even when `tools` omits it).

Probe 1 (verified): a nested `claude -p` in this container (Claude Code 2.1.332, no MCP servers of its own) with a local stub stdio MCP server named `github` (10 fake tools, contacts nothing) spawned:
- `issue-manager` with `ToolSearch` in the list: the GitHub tools were deferred, loaded via `ToolSearch`, and `issue_read` returned the stub result; `merge_pull_request` was neither listed nor loadable.
- the same list without `ToolSearch`: the six listed GitHub tools were in the tool list upfront, `issue_read` was called directly; `merge_pull_request` unavailable.
Both work; I chose exact names without `ToolSearch` (one step fewer, no deferral dependency). `mcp__github__*` was rejected: it would hand the issue manager `merge_pull_request`, `push_files`, `create_or_update_file`, against its *Must not*.

Not verified: the real cloud GitHub server (I created nothing on GitHub and cannot spawn subagents in the cloud session). Note: this session's GitHub server offers no `search_issues` (absent from its deferred list); the listed name is dropped silently there, and the issue manager searches with `list_issues`. The lead's issue-manager spawn in this run is the first real check.

### Question 3 — restricting the main session (lead, commands) from the repository

Mechanism: `permissions.deny` in the committed `.claude/settings.json`. A bare tool name or a tool-name glob (`mcp__*`, `mcp__<server>__*`) "removes the tool from Claude's context entirely" (docs/en/permissions); a committed project settings file is read by a single-repository cloud session (docs/en/settings, *Settings in cloud sessions*), not by cloud threads in a project or multi-repository sessions. No settings key gives the main session an allow-list: `--tools` is a CLI flag the repository does not control, and the `agent` setting runs the session as a custom agent, which replaces the Claude Code system prompt.

Probe 2 (verified): nested session with `--settings '{"permissions":{"deny":["Artifact","ArtifactComments","ArtifactData","mcp__github__merge_pull_request"]}}'`: the init tool list lacked all four; an unrestricted subagent reported none of them either (deny rules are inherited by subagents). Main-session first context 55,784 → 42,812 tokens, unrestricted subagent 44,063 → 31,450: the `Artifact` tools alone are about 12.5k tokens.

What the repository's workflows need in the main session: `Agent`, `SendMessage` and `ToolSearch` (CLAUDE.md), `AskUserQuestion`, base tools and `Edit`, the GitHub MCP tools (lead, `/coordinator`, `/triage`, `/bump-version`, `/generate-release-notes`), and `mcp__claude-code-remote__subscribe_pr_activity`/`unsubscribe_pr_activity` (lead section 9) — so the `claude-code-remote` server cannot be denied as a glob, only tool by tool.

**Not implemented.** The rule would remove tools from every session in this repository, not only from squad and command workflows: the maintainer's own sessions here would lose Artifacts, scheduled tasks, Docs, session tools, and whatever else is denied, and the cloud harness itself directs agents to some of these tools (`read_documentation`, environment tools when a host is blocked). Which of them the maintainer's own sessions may lose is the maintainer's choice, not something the repository's workflows decide; and a deny-list must be kept up to date by hand as the harness adds tools. Candidate list, if the maintainer wants it: `Artifact`, `ArtifactComments`, `ArtifactData`, `DesignSync`, `mcp__Claude_Docs__*`, `mcp__ccd_session__*`, `mcp__ccd_session_mgmt__*`, `mcp__ccd_window__*`, `ShowOnboardingRolePicker`, `SuggestPluginInstall`, `SuggestSkills`, `SearchPlugins`, and the `mcp__claude-code-remote__*` tools except `subscribe_pr_activity`, `unsubscribe_pr_activity`, `read_documentation`. Bare names that do not exist locally (e.g. `Artifact`) produce a startup warning in local sessions; names containing `_` are exempt.

### Tests run

Measurement (question 4): one nested `claude -p` session spawned every role once in the foreground with the prompt "Do not read any file, do not run any command, do not call any tool. Reply with the single word DONE.", plus a baseline `implementer-unrestricted` (the implementer wrapper without `tools:`). `scripts/usage.sh --session c4c7a599-a683-4f52-a3fd-46d1e274f430 --json`, `first_context_tokens`:

| Spawn | first_context_tokens |
|---|---|
| spec-author | 20,855 |
| skeptic | 20,508 |
| test-implementer | 20,857 |
| implementer | 20,854 |
| cleaner | 20,674 |
| spec-checker | 20,327 |
| auditor | 20,506 |
| validator | 20,342 |
| issue-manager | 20,973 |
| implementer-unrestricted (baseline) | 44,063 |

Limits: the nested session has no cloud tools (no `claude-code-remote`, Docs, real GitHub server) and no `Glob`/`Grep` (left out on Linux by default and dropped silently from the lists), and the stub GitHub tools are far smaller than the real ones. So the restricted numbers understate cloud by a few thousand tokens (the issue measured the validator at 23,166 in the cloud) and the baseline understates the cloud baseline (82,987 for this implementer spawn, `scripts/usage.sh --json` on the lead's session). Every role except the issue manager is now independent of the session's pool apart from `mcp__github__issue_read`.

Probe the lead can run in the cloud: project agents reload live, so every spawn after commit `1665b05` in this session uses the new lists. After the cleaner, spec checker, auditor, validator, and issue manager have run, `scripts/usage.sh --json | jq -c '.spawns[] | {agent, first_context_tokens}'` gives their cloud numbers; the issue's target is about 25k apart from the issue manager's GitHub tools. For the roles this run does not spawn, spawn each once with the probe prompt above.

`scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Policy owners
`rg -n -i "disallowedTools|Denied tools" --glob '!sdd/work/**' .` → no other owner; the tool policy lives only in `.claude/agents/*.md` and `.claude/agents/README.md`.

## Production diff size
No production code (`src/` untouched); 9 files under `.claude/agents/`, no new type.

## Deleted special cases
_N/A._

## Disputed tests or scope
_None._ (Main-session restriction not implemented; see question 3.)

## Architecture findings
_None._

## Follow-up candidates
- **Main-session tool restriction (maintainer's choice).** Whether to commit `permissions.deny` rules to `.claude/settings.json` that remove the tools listed under question 3 from every session in this repository; saves about 12.5k tokens per request for the `Artifact` tools alone, in the lead and in every subagent, at the cost of those tools in the maintainer's own sessions here.
- **`roles/implementer.md` Inputs** names no issue, though in the tooling lane (no spec author) the implementer works from the issue it reads through `issue_read`; one clause would make the allow-list's reason visible in the role file.
