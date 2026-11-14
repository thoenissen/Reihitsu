@AGENTS.md

# Reihitsu — Claude-specific notes

Everything above is imported from `AGENTS.md`, the single source of repository instructions. This file adds only what applies to Claude Code.

## Environment

Claude runs in a Linux cloud sandbox, so let the repository scripts install .NET 11 into `$HOME/.dotnet` when no `11.*` SDK is present — the sandbox is fresh on every session, so `--no-install` does not apply here. GitHub is reached through the GitHub MCP server (`mcp__github__*`), not the `gh` CLI.

## Asking the maintainer

`AskUserQuestion` is the question dialog that `Asking the maintainer` in `AGENTS.md` restricts: use it only for a short question whose options fit in their labels and need no code or background. Every other question — every squad escalation and every output question included — is a chat message in the format `AGENTS.md` describes, so its C# blocks render with highlighting, and you end your turn after posting it.

## The squad

Issues are implemented by the squad described in [`squad/README.md`](squad/README.md). Start a run with `/squad <issue>`, or with `/squad <issue> <issue> ...` for one run with one pull request over several issues: the session agent becomes the lead and follows [`squad/lead.md`](squad/lead.md).

The roles are defined as subagents under `.claude/agents/`, one thin wrapper per role that sets model, effort, and tools and points at `squad/roles/<role>.md`. Continue an agent with `SendMessage` (a deferred tool: load it with `ToolSearch` first) where `squad/lead.md` says to continue; otherwise spawn a role by its `subagent_type` and pass no model argument, so re-tuning a role stays a one-line frontmatter change; [`.claude/agents/README.md`](.claude/agents/README.md) records the current assignment.

The lead manages the pull request; the `issue-manager` agent manages issues, so in a squad run only it writes to the issue tracker, once before the PR is published.

When the lead watches the published pull request's CI, it subscribes to the PR's activity instead of polling, and handles each CI event as `squad/lead.md` describes.

## Compact instructions

These are a hint to the summarizer, not a guarantee; the safeguard is `sdd/work/<slug>/00-lead.md`. When the conversation is compacted, keep: the squad run's slug, lane, issue numbers, branch, and PR number; the audited SHA; the path `sdd/work/<slug>/00-lead.md` with the rule that it holds the maintainer's answers, the decisions taken, the follow-up candidates, and the open worklist; and the last step finished and the next one. Drop handover contents, tool output, and role transcripts: all of it is in files. After a compaction, re-read `00-lead.md` and the latest handovers before acting.

## Custom slash commands

| Command | Purpose |
|---|---|
| `/squad` | Run the squad on one or more GitHub issues, as its lead |
| `/triage` | Assess open issues without a milestone and assign milestones and labels after the maintainer confirms |
| `/create-rule-doc` | Write or update rule markdown under `documentation/rules/`; it owns the rule-page format |
| `/add-resource-texts` | Add or update localized resource strings |
| `/generate-release-notes` | Write user-facing release notes for a release tag compared with its predecessor, as one copyable Markdown block |
