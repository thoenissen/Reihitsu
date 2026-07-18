# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. It supports Linux cloud and local Windows environments. Use the authenticated `gh` CLI for GitHub platform operations:

1. Confirm authentication with `gh auth status`, then read the issue with `gh issue view $ARGUMENTS --json number,title,body,labels,state,url`.
2. Check for an existing claim or open draft PR. If unclaimed, create and push `codex/issue-$ARGUMENTS-<slug>` from the current `origin/main` baseline, then post an issue comment naming Codex and the branch as the owner.
3. Run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK. Do not install an SDK, modify `PATH`, or otherwise change the environment.
4. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `AGENTS.md` when nothing fits.
5. Format changed files via `Reihitsu.Cli`, make the first focused commit, push it, and open a **draft** PR immediately with `gh pr create --draft`. Use `.github/PULL_REQUEST_TEMPLATE.md` with `Closes #$ARGUMENTS`.
6. Run the full validation suite (build + all four test projects). Do not skip projects.
7. Post the draft PR URL back on the issue with `gh issue comment` if it was not included in the initial claim comment.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
