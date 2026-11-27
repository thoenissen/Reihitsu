# Agent definitions

One thin wrapper per squad role. The role's job lives in `squad/roles/<role>.md`, which is agent-neutral; these files hold only what is specific to Claude Code: model tier, effort level, and tool allow-list. The lead is the session agent itself, started with `/squad <issue> [<issue> ...]`, so it has no wrapper.

The lead spawns a role by its `subagent_type` and passes no model argument, so re-tuning a role is a one-line change here.

| Agent | Model | Effort | Tools | Why this tier |
|---|---|---|---|---|
| `spec-author` | `opus` | `medium` | base, `Edit`, `issue_read` | Writes requirements whose `Why:` decides later edge cases, and spots open questions about intended behavior |
| `skeptic` | `opus` | `high` | base, `issue_read` | Its value is finding the reading or the input nobody else considered; a missed premise is paid for by a whole run |
| `test-implementer` | `opus` | `medium` | base, `Edit`, `issue_read` | The tests define the change; a weak assertion lets a wrong implementation through |
| `implementer` | `opus` | `medium` | base, `Edit`, `issue_read` | Changes guards and rewrites in Roslyn trivia, where the failure classes live; the handovers say what to change but not where, so it keeps the stronger model, while the tests give it a feedback loop that makes medium effort enough |
| `cleaner` | `sonnet` | `low` | base, `Edit` | Formats files and resolves warnings by form-only changes; little judgment |
| `spec-checker` | `sonnet` | `medium` | base | Compares written text with the diff against a fixed checklist |
| `auditor` | `opus` | `high` | base, `issue_read` | The last independent check, against the specs and every failure class |
| `validator` | `haiku` | `low` | base | Runs three scripts and reports; keeps thousands of output lines out of the lead's context |
| `issue-manager` | `sonnet` | `medium` | base, `issue_read`, `list_issues`, `search_issues`, `issue_write`, `add_issue_comment`, `sub_issue_write` | Searches for duplicates and writes self-contained issues from the run's candidates; keeps the search results out of the lead's context, which is the most expensive one |

## Tools

Every role restricts its tools with a `tools:` allow-list and gets only the tools its role file under `squad/roles/` makes it use. A role without an allow-list would inherit every tool of the session — in a cloud session several dozen, among them session, scheduled-task, environment, artifact, and document tools no role uses — and every definition is sent with every request of the spawn. A deny-list does not help: it still hands the role every tool it does not name, including tools added to the session later.

- **base** is `Bash`, `Read`, `Write`, `Glob`, `Grep`. Every role runs repository scripts and `git` and commits its handover through `Bash`, and writes its handover under `sdd/work/<slug>/` with `Write`; the skeptic and the auditor also write disposable fixtures outside the repository. Where a session leaves `Glob` and `Grep` out of its tool set, as a local Linux or macOS session does, the role searches through `Bash` instead.
- **`Edit`** only for the roles that change existing repository files. The skeptic, the spec checker, the auditor, the validator, and the issue manager change none: each spawn writes a new handover with `Write`, and a role that amends its own handover rewrites it with `Write`, so none needs `Edit` for it. That they write nothing but their handover is stated in their role files.
- **GitHub** tools are named one by one as `mcp__github__<tool>` (the table drops the prefix), never as `mcp__github__*`, which would also hand a role the pull-request and file-writing tools. Every role that reads the run's issues by number gets `issue_read`, which also reads their comments; the issue manager also gets the tools to search, create, extend, comment on, and attach issues. A listed MCP tool is loaded with the role's first request, so a role needs no `ToolSearch`; a listed name the session does not provide is dropped without an error.
- **No other tool**: no role file asks for `Skill`, `SendMessage`, `ToolSearch`, `WebFetch`, `NotebookEdit`, or the agent tool; the commands a role file links, such as `add-resource-texts`, are read as files. Claude Code adds `SubagentHandback` on its own where a role reports through it, whether the list names it or not.

## Main session

The lead and the slash commands run in the main session, which has no wrapper and so no allow-list: no setting the repository controls gives the main session one. Instead, the committed project settings, [`../settings.json`](../settings.json), deny the cloud session's tools that no workflow of this repository uses. A denied tool is removed from the context of every session in this repository and of every subagent it spawns, so its definition is no longer sent with each request; the `Artifact` tools alone are about 12,500 tokens.

- **Denied**: the artifact tools (`Artifact*`), `DesignSync`, the document tools (`mcp__Claude_Docs__*`), the desktop session tools (`mcp__ccd_*`), the plugin and skill suggestion tools `SearchPlugins`, `ShowOnboardingRolePicker`, `SuggestPluginInstall`, and `SuggestSkills`, and every `mcp__claude-code-remote__*` tool except those kept below. Deny rules take no exceptions, so the `claude-code-remote` tools are listed one by one.
- **Kept from `claude-code-remote`**: `subscribe_pr_activity` and `unsubscribe_pr_activity`, which the lead uses for the published pull request (`squad/lead.md`, section 9); `read_documentation`, which the cloud harness points agents to; and the environment tools `get_environment`, `update_environment`, `create_environment`, `switch_environment`, and `request_credential`, which the cloud harness points agents to when a host or a credential is blocked.
- **Everything else is kept**: the base tools, `Agent`, `SendMessage`, `ToolSearch`, `AskUserQuestion`, `Skill`, the GitHub tools, and every tool the list does not name, including tools the session gains later. The list follows the tool names of the cloud session and is updated by hand when they change; a renamed tool slips through until it is added.

The rules apply only where Claude Code reads the project settings — a single-repository cloud session or a local session in this repository; they also remove these tools from the maintainer's own sessions here.

Available effort levels depend on the model. If a level is rejected for a model, drop it from that file rather than moving the role to another tier.
