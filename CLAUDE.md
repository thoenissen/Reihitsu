@AGENTS.md

# Reihitsu — Claude-specific notes

Everything above is imported from `AGENTS.md`, the single source of repository instructions. This file adds only what applies to Claude Code.

## Environment

Claude runs in a Linux cloud sandbox, so let the repository scripts install .NET 10 into `$HOME/.dotnet` when no `10.*` SDK is present — the sandbox is fresh on every session, so `--no-install` does not apply here. GitHub is reached through the GitHub MCP server (`mcp__github__*`), not the `gh` CLI.

## The squad

Issues are implemented by the squad described in [`squad/README.md`](squad/README.md). Start a run with `/squad <issue>`, or with `/squad <issue> <issue> ...` for one run with one pull request over several issues: the session agent becomes the lead and follows [`squad/lead.md`](squad/lead.md).

The roles are defined as subagents under `.claude/agents/`, one thin wrapper per role that sets model, effort, and tools and points at `squad/roles/<role>.md`. Spawn a role by its `subagent_type` and pass no model argument, so re-tuning a role stays a one-line frontmatter change; [`.claude/agents/README.md`](.claude/agents/README.md) records the current assignment.

When the lead watches the published pull request's CI, it subscribes to the PR's activity instead of polling, and handles each CI event as `squad/lead.md` describes.

## Custom slash commands

| Command | Purpose |
|---|---|
| `/squad` | Run the squad on one or more GitHub issues, as its lead |
| `/create-rule-doc` | Write or update rule markdown under `documentation/rules/`; it owns the rule-page format |
| `/add-resource-texts` | Add or update localized resource strings |
