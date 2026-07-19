---
name: gh-implement
description: Orchestrator for implementing a Reihitsu GitHub issue end-to-end in Codex on Linux cloud or local Windows. Triggers when the initial prompt references a GitHub issue (e.g. "implement #123", "fix issue 45", or a github.com/.../issues/N URL). It uses the preinstalled .NET SDK without modifying the environment, claims the issue by opening an intent-only draft PR before implementation, delegates the change to the matching repository command playbook, updates the draft after focused commits, and runs the full validation suite. GitHub operations use the authenticated `gh` CLI.
---

# Implement GitHub Issue

You are running in Codex on **Linux cloud or local Windows**. The repository checkout, required .NET 10 SDK, and authenticated `gh` CLI are present. Before builds or tests, confirm the SDK with `dotnet --list-sdks`; do not install an SDK, modify `PATH`, or otherwise change the environment. Your job is to take a single GitHub issue from unclaimed to a validated draft PR, delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the validation, and the pull request. The delegated command owns the production change and its tests.

## Environment baseline (read first, every run)

The required .NET 10 SDK is preinstalled in both supported environments. Before builds or tests, run:

```shell
dotnet --list-sdks
```

Do not install an SDK, modify `PATH`, or otherwise change the environment. If the expected SDK is unavailable, stop and report the environment issue; do not attempt to repair the environment from this skill.

## GitHub access — `gh` CLI

Use the authenticated `gh` CLI for every GitHub platform operation. Confirm the active account before making GitHub changes:

```shell
gh auth status
```

| Purpose | Command |
|---|---|
| Confirm identity / permissions | `gh auth status` |
| Read the issue | `gh issue view <N> --json number,title,body,labels,state,url` |
| Search for related/duplicate issues | `gh issue list --search "<query>"` |
| Create the draft pull request | `gh pr create --draft` |
| Update the draft pull request | `gh pr edit <PR> --body "<body>"` |

## Parse the issue reference

The issue reference comes from the **initial user/agent prompt**, not from the branch name. Accept any of:

- `#123`
- `https://github.com/<owner>/<repo>/issues/123`
- `GH-123`, `issue 123`, `implement issue 123`

Extract the integer issue number. If the prompt names a repository other than the current `origin`, pass it to `gh` with `--repo <owner>/<repo>`; otherwise default to the current repo.

If no issue number can be extracted with confidence, stop and ask. Do not guess.

## Read the issue

Read the issue with `gh issue view <N> --json number,title,body,labels,state,url`. Capture its number, title, body, labels, state, and URL.

Use the labels and body to pick a delegate (see next section). Cache the issue URL and title — you will need them for the branch name, commit message, and PR body.

## Claim the issue with an immediate draft PR

Avoid duplicate work before editing files:

1. Inspect the issue body, comments, and linked pull requests for an existing claim or open draft PR. If another agent or person has claimed it, stop and report the existing branch or PR.
2. Create the branch from the current remote baseline, add an empty claim commit so the branch differs from `main`, and push it:

   ```shell
   git fetch origin main
   git switch -c codex/issue-<N>-<short-slug> origin/main
   git commit --allow-empty -m "Claim issue #<N>"
   git push -u origin codex/issue-<N>-<short-slug>
   ```

3. Before implementation, open a **draft** PR with `gh pr create --draft`. Use the issue title and fill every section of `.github/PULL_REQUEST_TEMPLATE.md` with the current plan:

   ```markdown
   ## Summary

   Planned: <one or two sentences describing the intended change>

   ## Why

   <explain the issue's impact or motivation>

   ## Linked issues

   Closes #<N>

   ## Review notes

   Implementation has not started. This draft reserves the issue; details will be updated after focused implementation commits.

   ## Follow-up work

   To be determined during implementation.
   ```

The linked draft PR is the ownership record. Do not post a claim comment, PR-link comment, or `in-progress` label on the issue. GitHub links the PR automatically through `Closes #<N>`.

## Delegate to the matching slash command

The orchestrator does **not** implement the change itself when a specific command fits. The commands live under `.codex/commands/` and each one has its own mandatory workflow, checklist, and validation guidance. Pick the most specific match:

| Issue signal | Delegate to | Notes |
|---|---|---|
| Formatter produces wrong output, regression in formatting | [`fix-formatter`](../../commands/fix-formatter.md) | Regression test first, then fix |
| Bug in an existing analyzer rule (`RH####` listed) | [`fix-analyzer-rule`](../../commands/fix-analyzer-rule.md) | Reproduce in test first |
| New analyzer rule requested | [`create-analyzer-rule`](../../commands/create-analyzer-rule.md) | Only ship a code fix if comprehensive |
| New or extended formatter behavior | [`extend-formatter`](../../commands/extend-formatter.md) | Match existing pipeline phases |
| Missing or stale rule doc under `documentation/rules/` | [`create-rule-doc`](../../commands/create-rule-doc.md) | Keep `helpLinkUri` in sync |
| Localized resource string add / change | [`add-resource-texts`](../../commands/add-resource-texts.md) | Update every locale |
| Issue itself is a draft to be uploaded | [`draft-issue`](../../commands/draft-issue.md) | Create the draft only; upload is a separate workflow |
| Nothing above matches | Implement inline using the rules in `AGENTS.md` | Still run the full validation below |

**Delegation rule.** When a command matches, follow that command's workflow as written. The orchestrator's job is to wrap it with the environment setup, the validation, and the PR — it does not relax or override the delegated command's own checklist (regression-test-first, single focused tests, code-fix-only-if-comprehensive, etc.).

If the issue contains two clearly separable concerns (e.g. a formatter bug *and* a new resource text), prefer two PRs over one. Open the most blocking one first and leave a `Follow-up work` note in the PR pointing at the second.

## Branch and commit

The branch already contains the empty claim commit, is pushed, and has an open draft PR. Its slug is a lower-kebab-case excerpt of the issue title (≤ 4 words).

1. Make the change via the delegated command. Stage only the files that belong to this issue. Never `git add -A` blindly — the cloud worktree may contain unrelated changes.

2. Format **the changed files** through the CLI before tests:

   ```shell
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

3. Commit with a Conventional-Commits style subject that mentions the issue, then push it:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>)
   ```

## Update the draft after focused commits

Immediately after the first focused implementation commit is pushed, update the existing draft PR with `gh pr edit`. Replace the intent-only wording with what the commits actually changed, retain `Closes #<N>`, and fill every template section. Update the body again whenever later commits materially change the summary, review notes, or follow-up work. Keep the PR draft while validation is running and implementation continues.

## Validation (always run, never skip)

Run from the repository root with the available .NET 10 SDK on `PATH`:

```shell
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

All four test projects must pass. If any fails:

1. Read the failure, decide if it is caused by your change or a pre-existing issue on `main`.
2. Fix issues caused by your change. Do not silence tests or mark them `[Ignore]`.
3. If a failure exists on `main` independent of your change, record it in the draft PR's `Review notes` with `gh pr edit` and stop. Do not continue implementation on top of a broken baseline.

Do not list the executed test commands in the PR body. CI re-runs them and the repo convention (`AGENTS.md`) is to keep the PR description concise.

## Complete the draft pull request

1. If further commits were made while validating, push the branch:

   ```shell
   git push
   ```

2. Update the existing draft PR with `gh pr edit` so its body describes the final implementation. Use `.github/PULL_REQUEST_TEMPLATE.md` as the body layout and fill every section:

   - `## Summary`
   - `## Why`
   - `## Linked issues`
   - `## Review notes`
   - `## Follow-up work`

   `Linked issues` must retain GitHub-native linking so the issue auto-closes on merge:

   ```text
   Closes #<N>
   ```

   Use this final body structure:

   ```markdown
   ## Summary

   <one or two sentences on what changed>

   ## Why

   <link the motivation to the issue, do not just restate the title>

   ## Linked issues

   Closes #<N>

   ## Review notes

   <call out any risk, behavior change, or trade-off the reviewer should know>

   ## Follow-up work

   <None, or one line per deferred item>
   ```

   Keep the PR draft. The reviewer flips it to ready when they have eyes on it.

3. Verify that the PR is still draft and that `Closes #<N>` links it to the issue. Do not post an issue comment; the linked draft PR is the ownership and status record.

## Hard rules

- **Never** mark the draft PR ready for review without running the full validation above. A green build on three of four test projects is a regression — run all four.
- **Never** open a non-draft PR. The human reviewer marks ready.
- **Never** delay the initial draft PR until implementation exists. Create the empty claim commit and intent-only draft before editing files.
- **Never** post claim, PR-link, or status comments on the issue, and do not apply an `in-progress` label. Use the linked draft PR as the ownership record.
- **Never** silence or skip a failing test to make the PR go green.
- **Never** install an SDK, modify `PATH`, or otherwise change the environment. If the preinstalled toolchain is unavailable, record the environment issue in the draft PR and stop.
- **Always** use the authenticated `gh` CLI for GitHub platform operations. Do not call the GitHub REST API with raw `curl`.
- **Never** edit files outside the scope of the issue. Out-of-scope cleanups go in a separate issue or a follow-up note.
- **Never** include a list of locally executed tests in the PR body (per `AGENTS.md`).

## Quick reference

End-state checklist for a finished run:

- [ ] Preinstalled .NET 10 SDK confirmed with `dotnet --list-sdks`
- [ ] GitHub CLI authentication confirmed with `gh auth status`
- [ ] Issue number extracted and read via `gh issue view`
- [ ] Existing claim or draft PR checked; `codex/issue-<N>-<slug>` pushed with an empty claim commit
- [ ] Intent-only draft PR opened before implementation with every template section filled and `Closes #<N>`
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `Reihitsu.Cli`
- [ ] First focused implementation commit pushed and the draft PR body updated to the actual changes
- [ ] `dotnet build` + all four `dotnet test` projects green
- [ ] Final draft PR body current; issue linked only through `Closes #<N>` with no ownership comment or label
