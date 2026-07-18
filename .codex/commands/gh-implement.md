# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. The Codex cloud environment is a **Linux** sandbox. Do not assume the .NET SDK or the `gh` CLI are installed; probe them first and use MCP GitHub tools for GitHub API operations:

1. Run `dotnet --list-sdks`. If no `10.*` SDK is listed, install the .NET 10 SDK via the official `dotnet-install.sh` script, then confirm `dotnet --list-sdks` shows a `10.*` SDK.
2. Read the issue via `mcp__github__issue_read` when GitHub MCP tools are available. All GitHub platform access goes through the GitHub MCP server, not the `gh` CLI or direct REST calls.
3. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `AGENTS.md` when nothing fits.
4. Branch `issue-$ARGUMENTS-<slug>`, format changed files via `Reihitsu.Cli`, commit.
5. Run the full validation suite (build + all four test projects). Do not skip projects.
6. Push and open a **draft** PR through the available PR creation tool using `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
7. Post the PR URL back on the issue via `mcp__github__add_issue_comment` when GitHub MCP tools are available.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
