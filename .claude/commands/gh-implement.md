# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. You run in a **Linux** cloud sandbox with no .NET SDK and no `gh` CLI:

1. Read the issue via `mcp__github__issue_read` — all GitHub access goes through the GitHub MCP server, not the `gh` CLI.
2. First check the issue for an existing claim or open PR. If it is unclaimed, create `claude/issue-$ARGUMENTS-<slug>` from the current `origin/main`, add an empty `Claim issue #$ARGUMENTS [skip ci]` commit, push it, and immediately open a **draft** PR. Fill the template with the planned work and `Closes #$ARGUMENTS`; do not comment on or label the issue because the linked draft PR is the ownership marker.
3. Install the .NET 10 SDK via the official `dotnet-install.sh` script (the Linux sandbox does not ship with it), then confirm `dotnet --list-sdks` shows a `10.*` SDK.
4. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `CLAUDE.md` when nothing fits.
5. Format changed files via `Reihitsu.Cli`, make the first focused implementation commit with `[skip ci]` in the subject, and push it. Immediately update the existing draft PR body from the plan to the actual changes while retaining `Closes #$ARGUMENTS`.
6. Run the full validation suite (build + all four test projects). Do not skip projects. Commit any fixes with `[skip ci]` too.
7. Push any validation fix commits, then push one final commit *without* `[skip ci]` (an empty `Ready for CI (#$ARGUMENTS)` commit works) so CI runs exactly once, now that everything is finished. Update the draft PR body again if its summary, review notes, or follow-up work changed. Keep the PR draft for the human reviewer.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
