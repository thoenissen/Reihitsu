# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. This is a local Codex environment: the required .NET SDK and the `gh` CLI are already installed. Use the GitHub CLI for GitHub operations:

1. Read the issue with `gh issue view $ARGUMENTS --json number,title,body,labels,state,url`.
2. Use the `gh` CLI for all GitHub platform operations, including issue lookup, PR creation, and issue comments. Do not use GitHub MCP tools.
3. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `AGENTS.md` when nothing fits.
4. Branch `issue-$ARGUMENTS-<slug>`, format changed files via `Reihitsu.Cli`, commit.
5. Run the full validation suite (build + all four test projects). Do not skip projects.
6. Push and open a **draft** PR with `gh pr create --draft`, using `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
7. Post the PR URL back on the issue with `gh issue comment`.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
