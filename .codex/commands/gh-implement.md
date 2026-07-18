# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. It supports Linux cloud and local Windows environments. Use the authenticated `gh` CLI for GitHub platform operations:

1. Run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK. Do not install an SDK, modify `PATH`, or otherwise change the environment.
2. Confirm authentication with `gh auth status`, then read the issue with `gh issue view $ARGUMENTS --json number,title,body,labels,state,url`.
3. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `AGENTS.md` when nothing fits.
4. Branch `issue-$ARGUMENTS-<slug>`, format changed files via `Reihitsu.Cli`, commit.
5. Run the full validation suite (build + all four test projects). Do not skip projects.
6. Push and open a **draft** PR with `gh pr create --draft`, using `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
7. Post the PR URL back on the issue with `gh issue comment`.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
