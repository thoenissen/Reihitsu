---
name: gh-implement
description: >-
  Orchestrator for implementing a Reihitsu GitHub issue end-to-end in a Claude Code Cloud Agent environment. Triggers when the initial prompt references a GitHub issue (e.g. "implement #123", "fix issue 45", or a github.com/.../issues/N URL) and the work must be carried out from a clean cloud sandbox. It claims the issue by opening a generic-placeholder draft PR, installs the latest .NET 10 SDK, triages the run as routine or behavioral, runs the `gh-rubber-duck` Behavior Contract gate in a dedicated read-only subagent before touching any file on every behavioral run, turns the accepted contract into the regression matrix, delegates the change to the matching repository slash command, updates the draft after focused commits, self-reviews locally, synchronizes `origin/main`, spends at most two official `gh-preflight` attempts, runs the full validation suite once, fully rewrites the PR title and description, and pushes the single CI trigger. GitHub operations use the GitHub MCP server; the `gh` CLI is not installed. Do NOT use locally when the SDK is already installed and the user is driving the workflow interactively.
---

# Implement GitHub Issue (Cloud Agent Orchestrator)

You are running inside a **Claude Code Cloud Agent** sandbox — a **Linux** environment, essentially identical to the one you are executing in right now. The repository checkout is present, but the build toolchain is not, and there is no `gh` CLI. Your job is to take a single GitHub issue from unclaimed to a validated draft PR, delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the Behavior Contract gate, the validation, and the pull request. The delegated command owns the production change and its tests.

## Run order

Follow this sequence. The gates exist because rework in this repository is caused by starting to code against an unstated contract and by discovering problems one preflight round at a time — both are far more expensive than the analysis that prevents them.

1. Read the repository instructions (`CLAUDE.md`) and the GitHub issue.
2. Check issue ownership and open draft PRs.
3. Claim the issue through the existing ownership workflow when it is unclaimed.
4. Triage the run as **routine** or **behavioral**, and record which gates that decision keeps.
5. Read `.claude/skills/gh-rubber-duck/SKILL.md` completely in this parent agent (behavioral runs).
6. Spawn exactly one fresh, read-only Rubber Duck subagent (behavioral runs).
7. Receive and process the Behavior Contract.
8. Resolve every `NEEDS DECISION` before editing any production or test file.
9. Convert the accepted contract — or, on a routine run, the recorded contract note — into the implementation plan and the regression-test matrix.
10. Add all intended regression tests before production changes.
11. Implement, formatting changed paths and running focused tests as you go.
12. Run the **local self-review**.
13. Synchronize with current `origin/main`.
14. Run the **official preflight** (`gh-preflight`) on that exact synchronized head.
15. Run the complete **full validation** once.
16. Push the final non-`[skip ci]` CI trigger and finish the PR.

Claiming the issue (steps 1–3) happens before the triage and the contract gate so ownership is never lost while analysis runs. The SDK install fits between the claim and the first `dotnet` command. Production and regression-test edits do not begin until step 8 permits them.

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

The local `git` CLI is still available for branch/commit/push — only the *GitHub platform* calls go through MCP. Batch independent read-only calls in one step rather than issuing them one at a time.

## Keep CI silent until everything is done

`SonarCloud.yml` runs on `push` to `main` and on `pull_request` (`opened`/`synchronize`/`reopened`) against `main`. Left alone that means one CI run per push while the branch is still being claimed, implemented, and fixed up — noise that only needs to happen once, at the end. GitHub Actions skips the workflow run entirely when the triggering commit's message contains `[skip ci]`, so every commit this skill creates **except the final one** must end its subject with `[skip ci]`:

- The claim commit, every focused implementation commit, the `origin/main` synchronization commit, and any fix-up commit made while working a preflight worklist or chasing a validation failure all get `[skip ci]`.
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

Use the labels and body to pick a delegate (see the routing table below). Cache the issue URL and title — you will need the title for the branch slug, and, later, the full issue context (not a copy of its title or body) to write the final PR title and body once implementation is complete.

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

## Scope triage — decide which gates this run needs

Both gates default to **on**. The escape below exists for work that is genuinely small — not for work that merely looks small at first glance, because misjudging that is exactly the failure the gates were built to catch. Classify from evidence, not from the issue's tone or length.

A run is **routine** only when every one of these holds after reading the issue and doing one quick `rg` pass over the surfaces it names:

1. No behavior of an analyzer, formatter phase, code fix, Fix All, or `Reihitsu.Core` helper changes — diagnostics and formatter output stay byte-identical.
2. The issue admits exactly one reading; there is nothing a reviewer could reasonably interpret differently.
3. The change is confined to a small set of files you can list before you start.
4. No new rule, no diagnostic ID / severity / message change, no public API change, no dependency change.
5. It is not an analyzer or formatter bug report — those always get the contract, because the defect class *is* the question.

Typical routine work: rule-doc wording, repository instructions, workflow skill and command files, a comment typo, a test added for behavior that is already correct.

Everything else is **behavioral** and runs both gates exactly as written below.

| Gate | Routine run | Behavioral run |
|---|---|---|
| Rubber Duck subagent | Optional — replace it with a **contract note** (see below) | Mandatory |
| Official preflight | Optional **only when the diff contains no production code**; anything that compiles keeps at least attempt 1 | Mandatory, 1 + 1 budget |
| Local self-review | Mandatory | Mandatory |
| Test-first for bug fixes | Mandatory | Mandatory |
| Full validation | Mandatory | Mandatory |

**Contract note.** On a routine run, write three or four lines in chat before editing: the expected behavior, the files you will touch, and what must not change. It costs seconds, it is the artifact the local self-review walks instead of contract rows, and writing it is often what exposes that the run was not routine after all.

**Trip-wires.** The classification is provisional. Stop and run the full Behavior Contract gate before any further edit as soon as one of these appears:

- the change starts touching production behavior after all;
- a second reasonable interpretation of the issue surfaces;
- the file list grows beyond the one you wrote down;
- a test shows the current behavior is not what the issue assumed.

From that moment the run is behavioral: the official preflight is required again and the earlier routine classification is void.

**Record it.** The final report names the classification, which gates ran, which were skipped, and the one-line reason. A skipped gate is a judgment you own — it is never silent.

## Behavior Contract gate (behavioral runs — before any edit)

The claim is in place and nothing has been edited. Before the first test or production change, obtain a **Behavior Contract** from the `gh-rubber-duck` workflow. On a routine run this section is replaced by the contract note above — until a trip-wire fires, at which point it applies in full.

### 1. Read the skill in this agent

Read `.claude/skills/gh-rubber-duck/SKILL.md` completely yourself. You need the output schema to consume the contract, to spot a malformed one, and to send follow-up evidence later.

### 2. Spawn exactly one read-only Rubber Duck subagent

Run the analysis in **one fresh subagent** with no access to your reasoning. Isolation is the point: an analysis primed with your intended fix will confirm it instead of challenging it. The subagent prompt must contain, and must contain nothing more:

- the repository root;
- the issue number or URL;
- the PR number when one already exists;
- the user's relevant clarifications from this conversation, quoted rather than summarized;
- the path `.claude/skills/gh-rubber-duck/SKILL.md`, with the instruction to read and follow it completely;
- a strict read-only instruction (no edits, commits, pushes, PR changes, GitHub comments, or full validation);
- a request for the exact required output schema.

Do **not** include your own proposed solution, suspected root cause, planned diff, or preferred interpretation.

Use **one** Rubber Duck subagent per implementation run. When the user later clarifies the same contract, continue that same subagent with a follow-up message carrying the new evidence and ask for an amended contract — do not spawn a second one. Spawn a replacement only when the original agent is unavailable or the issue scope changes materially (a different defect, a different rule).

If subagents are unavailable in the current environment, perform the analysis yourself by following `gh-rubber-duck` before any edit, and record the resulting contract in this chat. The gate still applies; only the isolation is lost.

### 3. Handle the gate

**`READY`**

- Show the user a concise version of the user-visible examples and the important invariants — enough to catch a wrong contract in one glance, not the full report.
- Fold the contract into the implementation plan and the regression matrix.
- Continue automatically. Do not pause for approval unless the user explicitly asked to approve before implementation. If the user has already approved these examples or said "go" after seeing them, do not ask again.

**`NEEDS DECISION`**

- Show only the concrete unresolved decisions: the competing interpretations, one example each, and the recommendation. `AskUserQuestion` is the right tool when the options are discrete.
- Wait for the user's answer.
- Do not edit any production or test file while waiting.

**`BLOCKED`**

- Report exactly what could not be inspected and what would unblock it.
- Stop. Make no implementation changes.

### 4. When the contract changes mid-implementation

A newly discovered fact that materially changes the contract (a second syntax shape, a counterpart that must move too, an anchor that is not where the issue implied) is a contract change, not a detail:

1. Stop editing production code.
2. Send the new evidence to the existing Rubber Duck subagent.
3. Take the amended contract and update the plan and regression matrix from it.
4. Ask the user only when the amended result is `NEEDS DECISION`.

Never silently broaden the implemented behavior beyond the accepted contract.

## Convert the contract into a regression matrix

Before production code changes, turn the accepted contract — or the routine run's contract note — into the complete test plan. A single narrow test is not acceptable when the contract identifies a broader defect class — that is precisely the gap that produces another review round.

The matrix must cover, for the surfaces the contract names:

- every known defect variant from the issue and the contract's adversarial matrix;
- stable valid examples that must **not** change (the anti-regression side);
- misaligned or invalid examples that must change;
- code-fix convergence (one pass silences the diagnostic and raises no new RH diagnostic);
- Fix All across several diagnostics in one document, where the fix supports it;
- formatter second-pass idempotency;
- analyzer / formatter / code-fix parity in both directions;
- the trivia and directive cases the contract flagged as relevant (comments before the token and before both delimiters, `#if`/`#endif`/`#pragma`, disabled text);
- LF **and** CRLF coverage whenever layout is affected.

Per `CLAUDE.md`, an analyzer or formatter bug must be reproduced by a failing test before production code changes. Add the intended regression tests first, watch them fail for the right reason, then implement. Analyzer tests stay many small focused tests rather than one large multi-case test.

Keep the contract row IDs (`B1`, `B2`, …) next to the tests you write. The local self-review walks that mapping later.

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

**Delegation rule.** When a command matches, follow that command's workflow as written. The orchestrator's job is to wrap it with the environment setup, the Behavior Contract, the validation, and the PR — it does not relax or override the delegated command's own checklist (regression-test-first, single focused tests, code-fix-only-if-comprehensive, etc.). The accepted contract is an input to the delegated command, not a replacement for it.

If the issue contains two clearly separable concerns (e.g. a formatter bug *and* a new resource text), prefer two PRs over one. Open the most blocking one first and leave a `Follow-up work` note in the PR pointing at the second.

## Branch and commit

The branch already contains the empty claim commit, is pushed, and has an open draft PR. Its slug is a lower-kebab-case excerpt of the issue title (≤ 4 words).

1. Make the change via the delegated command. Stage only the files that belong to this issue. Never `git add -A` blindly — the cloud sandbox may contain unrelated SDK install artifacts.

2. Format **the changed files** through the CLI before tests:

   ```bash
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

3. Run the focused tests for the changed rule or phase — not the suite:

   ```bash
   dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --filter "FullyQualifiedName~RH3204"
   ```

4. Commit with a Conventional-Commits style subject that mentions the issue and ends with `[skip ci]` (see "Keep CI silent until everything is done"), then push it:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>) [skip ci]
   ```

## Update the draft after focused commits

Immediately after the first focused implementation commit is pushed, update the existing draft PR's **body** with `mcp__github__update_pull_request`. Replace the generic placeholder wording with what the commits actually changed, retain `Closes #<N>`, and fill every template section. Update the body again whenever later commits materially change the summary, review notes, or follow-up work. Leave the placeholder **title** (`Claim: issue #<N>`) as-is for now — the mandatory full title rewrite happens once in "Complete the draft pull request", from the finished change, not incrementally. Keep the PR draft while validation is running and implementation continues.

## Local self-review (before the official preflight)

The official preflight is a final quality gate, not a discovery loop, and you only get two attempts at it (see the budget below). Spend the cheap check first: walk your own change locally, in this agent, with no extra agent and no full suite.

Check, concretely:

- **every Behavior Contract row** — for each `B<n>`, name the test or code path that satisfies it; on a routine run, walk the contract note the same way;
- **counterpart parity** — formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable;
- **defect-class closure** — grep for sibling shapes and private copies of the policy you changed; a guard that covers only the reported example is not closure;
- **convergence** — the code fix silences its own diagnostic in one pass and raises no new RH diagnostic;
- **idempotency** — a second formatter pass over the output is a no-op, on LF and CRLF;
- **comments and directives** — the trivia shapes the contract marked relevant survive at sensible positions, or the edit is refused;
- **documentation** — `documentation/rules/RH####.md` matches the shipped behavior when a rule changed;
- **changed-path formatting** — every changed C# path went through `Reihitsu.Cli`;
- **focused tests** — the tests for the changed rule/phase pass at the current working tree.

Fix what you find now. This is not an official preflight, does not consume a preflight attempt, and is not reported as one.

## Synchronize with `origin/main` before the official gate

The audited head must be the head that will merge. Synchronizing after a passing preflight invalidates it, and preflighting a known-stale or known-conflicting branch wastes an attempt.

1. Fetch the current base:

   ```bash
   git fetch origin main
   ```

2. Check worktree and branch state — `git status --short` must be clean of unintended changes, and the branch must be the PR head.
3. Merge current `origin/main` into the working branch when the branch is behind.
4. Resolve conflicts so that **both** the branch behavior and the `main` behavior survive. A conflict resolution that drops one side is a defect, not a merge detail.
5. Run `Reihitsu.Cli` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Run the official preflight against that exact head.

If `origin/main` moves again **after** a passing preflight: do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required (does the new `main` touch anything this PR touches?). If it is, say plainly that merging again changes the audited head, and follow the user's explicit direction — including their decision to rely on CI without spending another preflight attempt.

## Official preflight gate — hard 1 + 1 budget

`gh-preflight` is the final, independent quality gate. Read `.claude/skills/gh-preflight/SKILL.md` completely and apply it as an internal gate, read-only, on the pushed and synchronized head. Do not post its findings through GitHub MCP. Run it in a fresh, independent read-only subagent when subagents are available, exactly as that skill's reviewer-isolation section requires.

A routine run whose diff contains **no production code** — documentation, repository instructions, workflow files — may skip the official preflight entirely and go straight to full validation, with the skip and its reason recorded in the final report. Every other run, routine or not, spends at least attempt 1.

The budget is fixed:

1. **Attempt 1** runs automatically once implementation is complete, the local self-review is done, `main` is synchronized, and the head is pushed.
2. On `PASS`, continue to full validation.
3. On `BLOCKED — findings`, collect **every** finding into **one** consolidated worklist. Do not start fixing before the worklist is complete, and do not run a preflight in between.
4. Fix the complete worklist in **one** repair cycle: close each finding's full defect class, format the changed paths, run the focused tests, redo the local self-review, then commit and push with `[skip ci]` and update the PR body when needed.
5. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent, read-only subagent against the exact new head.
6. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head and rerun; a state mismatch is a setup error, not a review result, so it does not consume an attempt.

Ask the user before expanding scope for an architecturally significant, public-API-changing, dependency-changing, contested, or unrelated pre-existing finding. Do not run the full validation suite until an attempt returns `PASS`.

A tracked-file change made after a passing preflight leaves the final head unaudited. If an attempt is still unspent, use it on the new head. If the budget is exhausted, do not start another official preflight: run the local self-review and the focused tests over the change, and state in the final report that the final head carries post-preflight changes.

The final report must state how many official preflight attempts were used, the result of each, and whether the budget was exhausted.

## Full validation — run it once

Focused, filtered tests run throughout implementation. The complete suite runs **once**, after implementation is complete, `main` is synchronized, the official preflight has passed, and the worktree matches the audited head. Do not rerun the whole solution after each small fix.

```bash
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --no-build --verbosity minimal
```

`--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All four test projects must pass. If any fails:

1. Read the failure, decide if it is caused by your change or a pre-existing issue on `main`.
2. Fix issues caused by your change and commit with `[skip ci]` in the subject before pushing. Do not silence tests or mark them `[Ignore]`. Rerun the focused tests for the fix plus the project that failed — not the whole suite.
3. If a failure exists on `main` independent of your change, record it in the draft PR's `Review notes` with `mcp__github__update_pull_request` and stop. Do not continue implementation on top of a broken baseline.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and state in the final report exactly which local checks ran and which did not.

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

4. Report back in chat, stating at least:

   - the scope classification (routine or behavioral) and which gates it kept or skipped, with the reason;
   - the Behavior Contract gate result — or the contract note — and any decision the user settled;
   - the regression matrix rows that were added;
   - official preflight attempts used (0, 1, or 2), the result of each, and whether the budget was exhausted;
   - the `origin/main` synchronization and the full-validation result;
   - anything deliberately left out of scope.

## Execution economy

Tokens and elapsed time are part of the deliverable. Without discipline this workflow re-reads the same files, re-runs passing tests, and prints thousands of warning lines nobody reads:

- Use `rg` for discovery instead of opening candidate files one by one.
- Batch independent read-only GitHub queries.
- Read a large file once and work from that content; do not reload it per question.
- Use focused `--filter` runs during implementation; keep the suite for the single full validation.
- Do not rerun a passing focused test unless the head changed code it covers.
- Keep build and test verbosity minimal.
- Capture very large command output to a file and report a concise summary.
- On failure, show the actionable error and the relevant log tail — not the full log.
- Do not narrate unchanged state between steps.
- Reuse the same Rubber Duck subagent for follow-up clarification instead of spawning another.

None of this may reduce correctness or hide a failing result. When output is trimmed, say what was trimmed.

## Hard rules

- **Never** edit a production or test file on a behavioral run before the Behavior Contract gate returns `READY` (or before the user has resolved a `NEEDS DECISION`). On a routine run the recorded contract note takes its place.
- **Never** classify a run routine to dodge the gates. The five criteria are all-or-nothing, a trip-wire voids the classification, and the report has to name the reason.
- **Never** hand the Rubber Duck subagent your proposed solution, suspected cause, or planned diff — the analysis must stay independent.
- **Never** spawn a second Rubber Duck subagent for the same contract; send follow-up evidence to the existing one.
- **Never** start a third official preflight automatically. The budget is one attempt plus one retry; after that, report and stop.
- **Never** split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- **Never** run the final preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- **Never** start full validation or push the final CI-trigger commit until `gh-preflight` returns `PASS` for the current PR head — the only exception is a routine run with no production code in the diff, which records the skip. If the budget is exhausted without a `PASS`, stop and report; that is not a licence to proceed.
- **Never** mark the draft PR ready for review without running the full validation above. A green build on three of four test projects is a regression — run all four.
- **Never** open a non-draft PR from the cloud agent. The human reviewer marks ready.
- **Never** delay the initial draft PR until implementation exists. Create the empty claim commit and generic-placeholder draft before installing dependencies or editing files.
- **Never** copy or paraphrase the issue's title or body into the claim-time draft PR. Title and body are the fixed generic placeholders; the only issue-specific content is the issue number and the `Closes #<N>` link.
- **Never** post claim, PR-link, or status comments on the issue, and do not apply an `in-progress` label. Use the linked draft PR as the ownership record.
- **Never** silence or skip a failing test to make the PR go green.
- **Never** ship a single narrow test when the contract identifies a broader defect class.
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
- [ ] Run triaged routine or behavioral against the five criteria, and the decision recorded
- [ ] Behavioral run: `gh-rubber-duck/SKILL.md` read in this agent and exactly one read-only Rubber Duck subagent spawned before any edit — routine run: contract note written before any edit
- [ ] Behavior Contract accepted (`READY`, or `NEEDS DECISION` resolved by the user) and shown to the user in short form
- [ ] Regression matrix derived from the contract or contract note; red tests added before production changes
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `Reihitsu.Cli`, focused tests green
- [ ] First focused implementation commit pushed and the draft PR body updated to the actual changes
- [ ] Local self-review completed against every contract row
- [ ] Current `origin/main` merged, conflicts formatted and focused-tested, synchronized head pushed with `[skip ci]`
- [ ] Official preflight `PASS` on that exact head, within the 1 + 1 budget — or a recorded skip on a routine run with no production code
- [ ] `dotnet build` + all four `dotnet test` projects green — run once
- [ ] Every commit up to that point contains `[skip ci]`; the final non-skip-ci trigger commit was pushed to run CI once
- [ ] Final draft PR **title and body fully rewritten** from the actual change — no claim-time placeholder or issue-verbatim wording left; issue linked only through `Closes #<N>` with no ownership comment or label
- [ ] Final chat report states the scope classification with its reason, the contract gate result, preflight attempts used, and whether the budget was exhausted
