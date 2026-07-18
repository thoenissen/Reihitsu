# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. You run in a **Linux** cloud sandbox with no .NET SDK and no `gh` CLI:

1. Read the issue via `mcp__github__issue_read` — all GitHub access goes through the GitHub MCP server, not the `gh` CLI.
2. First check the issue for an existing claim or open PR. If it is unclaimed, immediately create and push `claude/issue-$ARGUMENTS-<slug>`, then post an issue comment naming the branch and Claude Code as the owner.
3. Install the .NET 10 SDK via the official `dotnet-install.sh` script (the Linux sandbox does not ship with it), then confirm `dotnet --list-sdks` shows a `10.*` SDK.
4. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `CLAUDE.md` when nothing fits.
5. Format changed files via `Reihitsu.Cli`, and make the first focused commit. Push and open a **draft** PR immediately; do not wait for the complete validation suite. Use `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
6. Run the full validation suite (build + all four test projects). Do not skip projects.
7. Post the draft PR URL back on the issue via `mcp__github__add_issue_comment` if it was not included in the initial claim comment.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
