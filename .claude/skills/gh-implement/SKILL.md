---
name: gh-implement
description: Orchestrator for implementing a Reihitsu GitHub issue end-to-end in a Claude Code Cloud Agent environment. Triggers when the initial prompt references a GitHub issue (e.g. "implement #123", "fix issue 45", or a github.com/.../issues/N URL) and the work must be carried out from a clean cloud sandbox. The sandbox is always a Linux environment without the .NET SDK, so it installs the latest .NET 10 SDK via the official dotnet-install.sh script first, parses the issue, delegates the actual implementation to the matching repository slash command (fix-formatter, fix-analyzer-rule, create-analyzer-rule, extend-formatter, create-rule-doc, add-resource-texts, draft-issue), then runs the full validation suite and opens a draft pull request using PULL_REQUEST_TEMPLATE.md. All GitHub interaction goes through the GitHub MCP server — the `gh` CLI is not installed. Do NOT use locally when the SDK is already installed and the user is driving the workflow interactively.
---

# Implement GitHub Issue (Cloud Agent Orchestrator)

You are running inside a **Claude Code Cloud Agent** sandbox — a **Linux** environment, essentially identical to the one you are executing in right now. The repository checkout is present, but the build toolchain is not, and there is no `gh` CLI. Your job is to take a single GitHub issue from "assigned" to "draft PR open", delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the validation, and the pull request. The delegated command owns the production change and its tests.

## Environment baseline (read first, every run)

The cloud sandbox is **Linux** and does **not** ship with the .NET SDK. The repository targets `net10.0` (see any `*.csproj`) and there is no `global.json`, so install the latest .NET 10 SDK before doing anything that touches `dotnet`.

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

3. If the script itself cannot be reached (no network egress, mirror missing, etc.), **stop and report the failure on the issue thread** (via the GitHub MCP server, see below). Do not proceed with a partial validation — a green run without the SDK is meaningless.

## GitHub access — MCP only, no `gh` CLI

The sandbox has **no `gh` CLI** and no direct GitHub API access. Every GitHub interaction — reading the issue, opening the PR, commenting back — goes through the **GitHub MCP server** (`mcp__github__*` tools). If those tools are not yet loaded, use `ToolSearch` (e.g. `github pull request`, `github issue`) to surface them first.

Never shell out to `gh`, `git` against the API, or `curl` the GitHub REST API by hand. Use:

| Purpose | MCP tool |
|---|---|
| Confirm identity / permissions | `mcp__github__get_me` |
| Read the issue | `mcp__github__issue_read` |
| Search for related/duplicate issues | `mcp__github__search_issues` / `mcp__github__list_issues` |
| Create the pull request (draft) | `mcp__github__create_pull_request` |
| Comment the PR URL back on the issue | `mcp__github__add_issue_comment` |

The local `git` CLI is still available for branch/commit/push — only the *GitHub platform* calls go through MCP.

## Parse the issue reference

The issue reference comes from the **initial user/agent prompt**, not from the branch name. Accept any of:

- `#123`
- `https://github.com/<owner>/<repo>/issues/123`
- `GH-123`, `issue 123`, `implement issue 123`

Extract the integer issue number. If the prompt names a repository other than the current `origin`, use that owner/repo when calling the GitHub MCP tools; otherwise default to the current repo.

If no issue number can be extracted with confidence, stop and ask. Do not guess.

## Read the issue

Read the issue with `mcp__github__issue_read` (owner, repo, issue number). Capture its number, title, body, labels, state, and URL.

Use the labels and body to pick a delegate (see next section). Cache the issue URL and title — you will need them for the branch name, commit message, and PR body.

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

1. Create a branch from `main`:

   ```bash
   git checkout main
   git pull --ff-only
   git checkout -b issue-<N>-<short-slug>
   ```

   The slug is a lower-kebab-case excerpt of the issue title (≤ 4 words).

2. Make the change via the delegated command. Stage only the files that belong to this issue. Never `git add -A` blindly — the cloud sandbox may contain unrelated SDK install artifacts.

3. Format **the changed files** through the CLI before tests:

   ```bash
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

4. Commit with a Conventional-Commits style subject that mentions the issue:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>)
   ```

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
2. Fix issues caused by your change. Do not silence tests or mark them `[Ignore]`.
3. If a failure exists on `main` independent of your change, stop and report it on the issue thread (via `mcp__github__add_issue_comment`) — do not open the PR on top of a broken baseline.

Do not list the executed test commands in the PR body. CI re-runs them and the repo convention (`CLAUDE.md`) is to keep the PR description concise.

## Push and open the draft pull request

1. Push the branch:

   ```bash
   git push -u origin issue-<N>-<short-slug>
   ```

2. Open a **draft** PR with `mcp__github__create_pull_request` (set its `draft` flag to `true`). Use the repository's `PULL_REQUEST_TEMPLATE.md` as the body layout. The template lives at `.github/PULL_REQUEST_TEMPLATE.md` and has these sections:

   - `## Summary`
   - `## Why`
   - `## Linked issues`
   - `## Review notes`
   - `## Follow-up work`

   Fill every section. `Linked issues` must use GitHub-native linking so the issue auto-closes on merge:

   ```text
   Closes #<N>
   ```

   Body skeleton to pass to `create_pull_request`:

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

   The PR **must** be created as draft. The reviewer flips it to ready when they have eyes on it.

3. Post a short comment back on the issue with the PR URL (via `mcp__github__add_issue_comment`) so the assignee thread stays linked.

## Hard rules

- **Never** commit without running the full validation above. A green build on three of four test projects is a regression — run all four.
- **Never** open a non-draft PR from the cloud agent. The human reviewer marks ready.
- **Never** silence or skip a failing test to make the PR go green.
- **Never** modify `global.json` or the `TargetFramework` to dodge an SDK install — install the SDK via `dotnet-install.sh` instead.
- **Never** reach for the `gh` CLI or a raw GitHub API call — it is not available. Use the GitHub MCP server (`mcp__github__*`).
- **Never** edit files outside the scope of the issue. Out-of-scope cleanups go in a separate issue or a follow-up note.
- **Never** include a list of locally executed tests in the PR body (per `CLAUDE.md`).

## Quick reference

End-state checklist for a finished run:

- [ ] .NET 10 SDK installed via `dotnet-install.sh` and on `PATH`
- [ ] Issue number extracted and read via `mcp__github__issue_read`
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `Reihitsu.Cli`
- [ ] `dotnet build` + all four `dotnet test` projects green
- [ ] Branch pushed
- [ ] Draft PR opened via `mcp__github__create_pull_request` with `PULL_REQUEST_TEMPLATE.md` fully filled and `Closes #<N>`
- [ ] PR URL posted back on the issue via `mcp__github__add_issue_comment`
