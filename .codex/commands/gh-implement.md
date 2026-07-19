# GitHub Issue Implementation

Implement GitHub issue **#$ARGUMENTS** end-to-end.

Invoke the `gh-implement` skill and follow its workflow exactly. It supports Linux cloud and local Windows environments. Use the authenticated `gh` CLI for GitHub platform operations:

1. Confirm authentication with `gh auth status`, then read the issue with `gh issue view $ARGUMENTS --json number,title,body,labels,state,url`.
2. Check for an existing claim or open draft PR. If unclaimed, create `codex/issue-$ARGUMENTS-<slug>` from the current `origin/main`, add an empty `Claim issue #$ARGUMENTS` commit, push it, and immediately open a **draft** PR. Fill the template with the planned work and `Closes #$ARGUMENTS`; do not comment on or label the issue because the linked draft PR is the ownership marker.
3. Run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK. Do not install an SDK, modify `PATH`, or otherwise change the environment.
4. Pick the matching delegate from the skill's routing table (`fix-formatter`, `fix-analyzer-rule`, `create-analyzer-rule`, `extend-formatter`, `create-rule-doc`, `add-resource-texts`, `draft-issue`) or implement inline per `AGENTS.md` when nothing fits.
5. Format changed files via `Reihitsu.Cli`, make the first focused implementation commit, and push it. Immediately update the existing draft PR body from the plan to the actual changes while retaining `Closes #$ARGUMENTS`.
6. Run the full validation suite (build + all four test projects). Do not skip projects.
7. Push any validation fixes and update the draft PR body again if its summary, review notes, or follow-up work changed. Keep the PR draft for the human reviewer.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the issue number instead of guessing.
