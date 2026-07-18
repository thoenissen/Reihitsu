# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. You run in a **Linux** cloud sandbox. Do not use the `gh` CLI; GitHub platform operations use GitHub MCP tools:

1. Run `dotnet --list-sdks`. Install the .NET 10 SDK via the official `dotnet-install.sh` script only when no `10.*` SDK is listed, then confirm a `10.*` SDK is available.
2. Read the issue via `mcp__github__issue_read` — all GitHub access goes through the GitHub MCP server, not the `gh` CLI.
3. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `CLAUDE.md` when nothing fits.
4. Branch `issue-$ARGUMENTS-<slug>`, format changed files via `Reihitsu.Cli`, commit.
5. Run the full validation suite (build + all four test projects). Do not skip projects.
6. Push and open a **draft** PR via `mcp__github__create_pull_request` using `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
7. Post the PR URL back on the issue via `mcp__github__add_issue_comment`.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
