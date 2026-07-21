---
name: gh-implement
description: Orchestrator for implementing a Reihitsu GitHub issue end-to-end in a Claude Code Cloud Agent environment. Triggers when the initial prompt references a GitHub issue (e.g. "implement #123", "fix issue 45", or a github.com/.../issues/N URL) and the work must be carried out from a clean cloud sandbox. It claims the issue by opening a generic-placeholder draft PR before implementation, installs the latest .NET 10 SDK, delegates the change to the matching repository slash command, updates the draft after focused commits, fully rewrites the PR title and description once the change is complete, and runs the full validation suite. GitHub operations use the GitHub MCP server; the `gh` CLI is not installed. Do NOT use locally when the SDK is already installed and the user is driving the workflow interactively.
---

# Implement GitHub Issue (Cloud Agent Orchestrator)

You are running inside a **Claude Code Cloud Agent** sandbox — a **Linux** environment, essentially identical to the one you are executing in right now. The repository checkout is present, but the build toolchain is not, and there is no `gh` CLI. Your job is to take a single GitHub issue from unclaimed to a validated draft PR, delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the validation, and the pull request. The delegated command owns the production change and its tests.

## Build environment (after the issue claim)

The cloud sandbox is **Linux** and does **not** ship with the .NET SDK. Claim the issue first; then, before doing anything that touches `dotnet`, install the latest .NET 10 SDK. The repository targets `net10.0` (see any `*.csproj`) and there is no `global.json`.

1. Probe the toolchain:

   ```bash
   dotnet --list-sdks
   ```

   If the command is missing or no `10.*` SDK is listed, install it. Do not fall back to an older SDK — the full test suite will not run on `net10.0` without it.

2. Install via the official `dotnet-install.sh` script (no admin rights required, installs into `$HOME/.dotnet`). This is a Linux environment, so use the shell script — there is no PowerShell path to consider:

   ```bash
   curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
   bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
   export PATH="$HOME/.dotnet:$PATH"
   dotnet --list-sdks
   ```

   Keep `$HOME/.dotnet` on `PATH` for every later `dotnet` invocation in the run.

3. If the script itself cannot be reached (no network egress, mirror missing, etc.), record the failure in the already-open draft PR's `Review notes` and stop. Do not proceed with a partial validation — a green run without the SDK is meaningless.

## GitHub access — MCP only, no `gh` CLI

The sandbox has **no `gh` CLI** and no direct GitHub API access. Every GitHub interaction — reading the issue, opening the PR, commenting back — goes through the **GitHub MCP server** (`mcp__github__*` tools). If those tools are not yet loaded, use `ToolSearch` (e.g. `github pull request`, `github issue`) to surface them first.

Never shell out to `gh`, `git` against the API, or `curl` the GitHub REST API by hand. Use:

| Purpose | MCP tool |
|---|---|
| Confirm identity / permissions | `mcp__github__get_me` |
| Read the issue | `mcp__github__issue_read` |
| Search for related/duplicate issues | `mcp__github__search_issues` / `mcp__github__list_issues` |
| Create the pull request (draft) | `mcp__github__create_pull_request` |
| Update the draft pull request | `mcp__github__update_pull_request` |

The local `git` CLI is still available for branch/commit/push — only the *GitHub platform* calls go through MCP.

## Keep CI silent until everything is done

`SonarCloud.yml` runs on `push` to `main` and on `pull_request` (`opened`/`synchronize`/`reopened`) against `main`. Left alone that means one CI run per push while the branch is still being claimed, implemented, and fixed up — noise that only needs to happen once, at the end. GitHub Actions skips the workflow run entirely when the triggering commit's message contains `[skip ci]`, so every commit this skill creates **except the final one** must end its subject with `[skip ci]`:

- The claim commit, every focused implementation commit, and any fix-up commit made while chasing a validation failure all get `[skip ci]`.
- The single exception is the run's last commit, pushed in "Complete the draft pull request" once validation is fully green — it must **not** contain `[skip ci]`, so it becomes the one CI run for the issue.

Don't rely on "whichever commit happens to be last" to satisfy this — "Complete the draft pull request" adds a dedicated, empty, non-skip-ci commit so the trigger is unambiguous even when validation needed no fix-up commits.

## Parse the issue reference

The issue reference comes from the **initial user/agent prompt**, not from the branch name. Accept any of:

- `#123`
- `https://github.com/<owner>/<repo>/issues/123`
- `GH-123`, `issue 123`, `implement issue 123`

Extract the integer issue number. If the prompt names a repository other than the current `origin`, use that owner/repo when calling the GitHub MCP tools; otherwise default to the current repo.

If no issue number can be extracted with confidence, stop and ask. Do not guess.

## Read the issue

Read the issue with `mcp__github__issue_read` (owner, repo, issue number). Capture its number, title, body, labels, state, and URL.

Use the labels and body to pick a delegate (see next section). Cache the issue URL and title — you will need the title for the branch slug, and, later, the full issue context (not a copy of its title or body) to write the final PR title and body once implementation is complete.

## Claim the issue with an immediate draft PR

Avoid duplicate work before installing dependencies or editing files:

1. Inspect the issue body, comments, and linked pull requests for an existing claim or an open draft PR. If another agent or person has claimed it, stop and report the existing branch or PR; do not create a competing branch.
2. Create the branch from the current remote baseline, add an empty claim commit so the branch differs from `main`, and push it:

   ```bash
   git fetch origin main
   git checkout -b claude/issue-<N>-<short-slug> origin/main
   git commit --allow-empty -m "Claim issue #<N> [skip ci]"
   git push -u origin claude/issue-<N>-<short-slug>
   ```

3. Before implementation, open a **draft** PR with `mcp__github__create_pull_request` and set `draft` to `true`. Both the title and the body are a **generic placeholder** at this point — do not paraphrase or copy the issue's title or body into either one. The only issue-specific content allowed anywhere in the claim PR is the issue number, and the body's `Closes #<N>` link is mandatory:

   - **Title**: `Claim: issue #<N>` — never the issue's own title.
   - **Body**: fill every section of `.github/PULL_REQUEST_TEMPLATE.md` with static, generic wording, verbatim:

   ```markdown
   ## Summary

   Placeholder — implementation has not started yet.

   ## Why

   Not documented yet; this draft only reserves the issue.

   ## Linked issues

   Closes #<N>

   ## Review notes

   Generic placeholder draft. Title and description will be fully rewritten once implementation is complete — not ready for review.

   ## Follow-up work

   Not yet determined.
   ```

The linked draft PR is the ownership record. Do not post a claim comment, PR-link comment, or `in-progress` label on the issue. GitHub links the PR automatically through `Closes #<N>`.

## Delegate to the matching slash command

The orchestrator does **not** implement the change itself when a specific command fits. The commands live under `.claude/commands/` and each one has its own mandatory workflow, checklist, and validation guidance. Pick the most specific match:

| Issue signal | Delegate to | Notes |
|---|---|---|
| Formatter produces wrong output, regression in formatting | [`fix-formatter`](../../commands/fix-formatter.md) | Regression test first, then fix |
| Bug in an existing analyzer rule (`RH####` listed) | [`fix-analyzer-rule`](../../commands/fix-analyzer-rule.md) | Reproduce in test first |
| New analyzer rule requested | [`create-analyzer-rule`](../../commands/create-analyzer-rule.md) | Only ship a code fix if comprehensive |
| New or extended formatter behavior | [`extend-formatter`](../../commands/extend-formatter.md) | Match existing pipeline phases |
| Missing or stale rule doc under `documentation/rules/` | [`create-rule-doc`](../../commands/create-rule-doc.md) | Keep `helpLinkUri` in sync |
| Localized resource string add / change | [`add-resource-texts`](../../commands/add-resource-texts.md) | Update every locale |
| Issue itself is a draft to be uploaded | [`draft-issue`](../../commands/draft-issue.md) | Validates against `upload-issues.ps1` |
| Nothing above matches | Implement inline using the rules in `CLAUDE.md` | Still run the full validation below |

**Delegation rule.** When a command matches, follow that command's workflow as written. The orchestrator's job is to wrap it with the environment setup, the validation, and the PR — it does not relax or override the delegated command's own checklist (regression-test-first, single focused tests, code-fix-only-if-comprehensive, etc.).

If the issue contains two clearly separable concerns (e.g. a formatter bug *and* a new resource text), prefer two PRs over one. Open the most blocking one first and leave a `Follow-up work` note in the PR pointing at the second.

## Branch and commit

The branch already contains the empty claim commit, is pushed, and has an open draft PR. Its slug is a lower-kebab-case excerpt of the issue title (≤ 4 words).

1. Make the change via the delegated command. Stage only the files that belong to this issue. Never `git add -A` blindly — the cloud sandbox may contain unrelated SDK install artifacts.

2. Format **the changed files** through the CLI before tests:

   ```bash
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

3. Commit with a Conventional-Commits style subject that mentions the issue and ends with `[skip ci]` (see "Keep CI silent until everything is done"), then push it:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>) [skip ci]
   ```

## Update the draft after focused commits

Immediately after the first focused implementation commit is pushed, update the existing draft PR's **body** with `mcp__github__update_pull_request`. Replace the generic placeholder wording with what the commits actually changed, retain `Closes #<N>`, and fill every template section. Update the body again whenever later commits materially change the summary, review notes, or follow-up work. Leave the placeholder **title** (`Claim: issue #<N>`) as-is for now — the mandatory full title rewrite happens once in "Complete the draft pull request", from the finished change, not incrementally. Keep the PR draft while validation is running and implementation continues.

## Validation (always run, never skip)

Run from the repo root with the just-installed SDK on `PATH`:

```bash
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

All four test projects must pass. If any fails:

1. Read the failure, decide if it is caused by your change or a pre-existing issue on `main`.
2. Fix issues caused by your change and commit with `[skip ci]` in the subject before pushing. Do not silence tests or mark them `[Ignore]`.
3. If a failure exists on `main` independent of your change, record it in the draft PR's `Review notes` with `mcp__github__update_pull_request` and stop. Do not continue implementation on top of a broken baseline.

Do not list the executed test commands in the PR body. CI re-runs them and the repo convention (`CLAUDE.md`) is to keep the PR description concise.

## Complete the draft pull request

1. Push any remaining validation fix-up commits, then add the run's single non-skip-ci commit so the push triggers the one CI run for this issue:

   ```bash
   git push
   git commit --allow-empty -m "Ready for CI (#<N>)"
   git push
   ```

   This is the only commit in the run that must not contain `[skip ci]`.

2. Update the existing draft PR with `mcp__github__update_pull_request`, passing both `title` and `body` in the same call. This is the mandatory full rewrite — not an edit of the claim-time placeholder:

   - **Title**: write a fresh, descriptive title from what the commits actually did. Never carry over the claim-time placeholder (`Claim: issue #<N>`), and never reuse the issue's own title verbatim.
   - **Body**: use `.github/PULL_REQUEST_TEMPLATE.md` as the layout and fill every section from the real change — not the issue's wording, and not the claim-time placeholder text:

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
- **Never** open a non-draft PR from the cloud agent. The human reviewer marks ready.
- **Never** delay the initial draft PR until implementation exists. Create the empty claim commit and generic-placeholder draft before installing dependencies or editing files.
- **Never** copy or paraphrase the issue's title or body into the claim-time draft PR. Title and body are the fixed generic placeholders; the only issue-specific content is the issue number and the `Closes #<N>` link.
- **Never** post claim, PR-link, or status comments on the issue, and do not apply an `in-progress` label. Use the linked draft PR as the ownership record.
- **Never** silence or skip a failing test to make the PR go green.
- **Never** finish a run leaving the claim-time placeholder title or wording in place — "Complete the draft pull request" must rewrite both the title and every body section from the actual change.
- **Never** push a commit without `[skip ci]` before validation is green — the empty trigger commit in "Complete the draft pull request" is the only exception.
- **Never** modify `global.json` or the `TargetFramework` to dodge an SDK install — install the SDK via `dotnet-install.sh` instead.
- **Never** reach for the `gh` CLI or a raw GitHub API call — it is not available. Use the GitHub MCP server (`mcp__github__*`).
- **Never** edit files outside the scope of the issue. Out-of-scope cleanups go in a separate issue or a follow-up note.
- **Never** include a list of locally executed tests in the PR body (per `CLAUDE.md`).

## Quick reference

End-state checklist for a finished run:

- [ ] .NET 10 SDK installed via `dotnet-install.sh` and on `PATH`
- [ ] Issue number extracted and read via `mcp__github__issue_read`
- [ ] Existing claim or draft PR checked; `claude/issue-<N>-<slug>` pushed with an empty claim commit
- [ ] Generic-placeholder draft PR opened before implementation (title `Claim: issue #<N>`, every template section filled with static generic text, `Closes #<N>`) — nothing paraphrased from the issue
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `Reihitsu.Cli`
- [ ] First focused implementation commit pushed and the draft PR body updated to the actual changes
- [ ] `dotnet build` + all four `dotnet test` projects green
- [ ] Every commit up to that point contains `[skip ci]`; the final non-skip-ci trigger commit was pushed to run CI once
- [ ] Final draft PR **title and body fully rewritten** from the actual change — no claim-time placeholder or issue-verbatim wording left; issue linked only through `Closes #<N>` with no ownership comment or label
