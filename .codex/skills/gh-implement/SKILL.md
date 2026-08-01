---
name: gh-implement
description: >-
  Implement a Reihitsu GitHub issue end-to-end in Codex on Linux or Windows. Trigger when the initial prompt references an issue number or URL. Use the preinstalled .NET SDK without changing the environment, claim the issue with a generic-placeholder draft PR, triage the run as routine or behavioral, and run the read-only `gh-rubber-duck` Behavior Contract before every behavioral edit. Require a code-derived defect-class enumeration and candidate sweep for bug reports, turn the accepted contract into the regression matrix, delegate to the matching repository command, self-review, synchronize `origin/main`, use at most two official `gh-preflight` attempts, run full validation once, fully rewrite the draft PR, and push one CI trigger. Use authenticated `gh` for GitHub operations.
---

# Implement GitHub Issue

You are running in Codex on **Linux cloud or local Windows**. The repository checkout, required .NET 10 SDK, and authenticated `gh` CLI are present. Before builds or tests, confirm the SDK with `scripts/prepare.sh --no-install`; do not install an SDK, modify `PATH`, or otherwise change the environment. Your job is to take a single GitHub issue from unclaimed to a validated draft PR, delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the Behavior Contract gate, the validation, and the pull request. The delegated command owns the production change and its tests.

## Run order

Follow this sequence. The gates exist because rework in this repository is caused by starting to code against an unstated contract and by discovering problems one preflight round at a time — both are far more expensive than the analysis that prevents them.

1. Read the repository instructions (`AGENTS.md`) and the GitHub issue.
2. Check issue ownership and open draft PRs.
3. Claim the issue through the existing ownership workflow when it is unclaimed.
4. Triage the run as **routine** or **behavioral**, and record which gates that decision keeps.
5. Read `.codex/skills/gh-rubber-duck/SKILL.md` completely in this parent agent (behavioral runs).
6. Gather the neutral **evidence bundle** and spawn exactly one fresh, read-only Rubber Duck subagent (behavioral runs).
7. Receive and process the Behavior Contract.
8. Resolve every `NEEDS DECISION` before editing any production or test file.
9. Freeze the **scope ledger** from the accepted contract.
10. Convert the accepted contract — including every defect-class sweep row and every delta-table boundary — or, on a routine run, the recorded contract note into the implementation plan and regression-test matrix.
11. Add all intended regression tests before production changes.
12. Implement, formatting changed paths and running focused tests as you go.
13. Run the **local self-review** and record its **admission artifact**.
14. Synchronize with current `origin/main`.
15. Decide from `gh-preflight`'s trigger list whether an audit is required, and run it on that exact synchronized tree when it is.
16. Run the complete **full validation** once.
17. Push the final non-`[skip ci]` CI trigger and finish the PR.

Claiming the issue (steps 1–3) happens before the triage and the contract gate so ownership is never lost while analysis runs. The toolchain preparation fits between the claim and the first `dotnet` command. Production and regression-test edits do not begin until step 8 permits them.

## Build environment (after the issue claim)

The required .NET 10 SDK is preinstalled in every supported Codex environment. Claim the issue first; then, before doing anything that touches `dotnet`, verify the toolchain through the repository script:

```shell
scripts/prepare.sh --no-install
```

`--no-install` turns a missing SDK into a failure instead of an installation, so the script verifies and changes nothing. The repository targets `net10.0` and there is no `global.json`; never fall back to an older SDK, install one, or modify `PATH`.

The other repository scripts — `scripts/build.sh`, `scripts/test.sh`, `scripts/format.sh`, `scripts/verify-text-only.sh` — take the same flag and resolve the SDK the same way. Use them instead of hand-written `dotnet` invocations; they are the single owner of these commands across `AGENTS.md`, `CLAUDE.md`, and every workflow skill. In the Linux cloud environment use the `.sh` variants; the `.ps1` variants are for local Windows.

If the SDK is missing or the scripts cannot run, record the failure in the already-open draft PR's `Review notes` and stop. Do not proceed with a partial validation — a green run without the SDK is meaningless.

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
| Update the draft pull request | `gh pr edit <PR> --body "<body>"` (add `--title "<title>"` for the final rewrite) |

Batch independent read-only queries in one step rather than issuing them one at a time.

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

Extract the integer issue number. If the prompt names a repository other than the current `origin`, pass it to `gh` with `--repo <owner>/<repo>`; otherwise default to the current repo.

If no issue number can be extracted with confidence, stop and ask. Do not guess.

## Read the issue

Read the issue with `gh issue view <N> --json number,title,body,labels,state,url`. Capture its number, title, body, labels, state, and URL.

Use the labels and body to pick a delegate (see the routing table below). Cache the issue URL and title — you will need the title for the branch slug, and, later, the full issue context (not a copy of its title or body) to write the final PR title and body once implementation is complete.

## Claim the issue with an immediate draft PR

Avoid duplicate work before editing files:

1. Inspect the issue body, comments, and linked pull requests for an existing claim or open draft PR. If another agent or person has claimed it, stop and report the existing branch or PR.
2. Create the branch from the current remote baseline, add an empty claim commit so the branch differs from `main`, and push it:

   ```shell
   git fetch origin main
   git switch -c codex/issue-<N>-<short-slug> origin/main
   git commit --allow-empty -m "Claim issue #<N> [skip ci]"
   git push -u origin codex/issue-<N>-<short-slug>
   ```

3. Before implementation, open a **draft** PR with `gh pr create --draft`. Both the title and the body are a **generic placeholder** at this point — do not paraphrase or copy the issue's title or body into either one. The only issue-specific content allowed anywhere in the claim PR is the issue number, and the body's `Closes #<N>` link is mandatory:

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

**Mechanical veto first.** Look at the file set before judging anything. If the diff touches any of these, the run is **behavioral** — no further judgment needed:

- `Reihitsu.Analyzer/**`, `Reihitsu.Analyzer.CodeFixes/**`, `Reihitsu.Formatter/**`, `Reihitsu.Core/**`, `Reihitsu.Cli/**`;
- any test project (`Reihitsu.*.Test/**`, `Reihitsu.ArchitectureTests/**`);
- `*.csproj`, `Reihitsu.sln`, `Directory.Build.props`, `*.ruleset`, `.editorconfig`;
- `scripts/**` and CI workflow files — these are not compiled, but they are executable behavior;
- a mixed diff that contains any of the above next to documentation.

Touching a `.cs` file is not by itself a reason to audit when only its comments changed. The veto therefore has one mechanical escape, and it is a proof rather than a judgment call:

```bash
scripts/verify-text-only.sh --no-install --base <base-sha> --head <head-sha>
```

Exit code `0` proves that every changed line is comment, documentation, or layout trivia, that no token, directive, disabled-text region, or literal moved, and that both versions parse. A diff the proof accepts may still be routine even though it contains `.cs` files; record the `TEXT-ONLY PROOF: PASS …` line as the evidence. Exit code `1` restores the veto, and exit code `2` is a tool failure that proves nothing — treat it as `1`.

A rename never qualifies: it reaches `nameof`, reflection, serialization, source generators, public API, and named arguments. Everything else that is neither C# nor a known non-compiled text path stays behavioral.

Passing the veto is necessary, not sufficient. A run is **routine** only when, in addition, every one of these holds after reading the issue and doing one quick `rg` pass over the surfaces it names:

1. No behavior of an analyzer, formatter phase, code fix, Fix All, or `Reihitsu.Core` helper changes — diagnostics and formatter output stay byte-identical.
2. The issue admits exactly one reading; there is nothing a reviewer could reasonably interpret differently.
3. The change is confined to a small set of files you can list before you start.
4. No new rule, no diagnostic ID / severity / message change, no public API change, no dependency change.
5. It is not an analyzer or formatter bug report — those always get the contract, because the defect class *is* the question.

`documentation/rules/RH####.md` deserves a second look: fixing its wording is routine, changing the behavior it documents is not — the doc is part of the rule contract and `AnalyzerPackageMetadataTests` compares it against the shipped analyzers.

Typical routine work: rule-doc wording, repository instructions, workflow skill and command files, a comment typo, a test added for behavior that is already correct.

Everything else is **behavioral** and runs both gates exactly as written below.

| Gate | Routine run | Behavioral run |
|---|---|---|
| Rubber Duck subagent | Optional — replace it with a **contract note** (see below) | Mandatory |
| Official preflight | Per `gh-preflight`'s trigger list; a proven text-only diff records the skip with its proof line | Per the same trigger list — in practice always, with the 1 + 1 budget |
| Local self-review + admission artifact | Mandatory, in its routine form (below) | Mandatory |
| Test-first for bug fixes | N/A — a routine run changes no behavior | Mandatory |
| Full validation | Skipped only when the diff contains **no** compiled file at all | Mandatory |

Preflight and validation are separate decisions and must not be collapsed. A comment-only change inside `.cs` can skip the audit, because the syntax-aware proof shows there is nothing to audit — but it still gets the build and all four test projects, because the build is the last thing that would catch a malformed comment or a changed documentation artifact, and test runtime costs wall-clock rather than tokens. Only a diff with no compiled file anywhere skips validation as well. Record every skip and its reason in the report.

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

Read `.codex/skills/gh-rubber-duck/SKILL.md` completely yourself. You need the output schema to consume the contract, to spot a malformed one, and to send follow-up evidence later.

### 2. Spawn exactly one read-only Rubber Duck subagent

Run the analysis in **one fresh subagent** with no access to your reasoning. Isolation is the point: an analysis primed with your intended fix will confirm it instead of challenging it.

Gather the neutral **evidence bundle** once, before spawning, and hand it over verbatim. Every isolated gate agent in this run — Rubber Duck now, preflight later — consumes the same bundle, so the facts are collected once instead of being reconstructed unreliably per agent:

- the issue JSON and every linked clarification;
- the PR metadata and body, when a PR already exists;
- the base and head SHAs, and the merge base;
- the changed-file list and diff, when there is one;
- your proof that the local checkout matches that head.

The subagent prompt must contain that bundle plus, and nothing more:

- the repository root;
- the user's relevant clarifications from this conversation, quoted rather than summarized;
- the path `.codex/skills/gh-rubber-duck/SKILL.md`, with the instruction to read and follow it completely;
- a strict read-only instruction (no edits, commits, pushes, PR changes, GitHub comments, or full validation);
- a request for the exact required output schema.

Do **not** include your own proposed solution, suspected root cause, planned diff, or preferred interpretation. The bundle is fact-gathering; anything that carries your conclusions defeats the isolation the gate exists for.

Use **one** Rubber Duck subagent per implementation run. When the user later clarifies the same contract, continue that same subagent with a follow-up message carrying the new evidence and ask for an amended contract — do not spawn a second one. Spawn a replacement only when the original agent is unavailable or the issue scope changes materially (a different defect, a different rule).

Apply `gh-preflight`'s bounded restart policy here too: one start, at most one restart after the agent errors, exits without a contract, or goes 15 minutes without tool activity, then the local fallback below. A start that returned no contract counts as a **process start**, not as a gate result, and the final report states both.

If subagents are unavailable in the current environment, perform the analysis yourself by following `gh-rubber-duck` before any edit, and record the resulting contract in this chat. The gate still applies; only the isolation is lost.

### 3. Handle the gate

**`READY`**

- For a bug report, reject a malformed `READY` result that lacks a decidable `Defect-class enumeration` or a completed `Defect-class sweep` row for every code-derived candidate. Send the schema defect back to the same Rubber Duck subagent; do not begin implementation.
- When the change moves a guard, predicate, or exemption, reject a `READY` result whose `Guard-delta table` or `Predicate-boundary table` is missing, empty without the explicit `_N/A_` text, carries a region that loses coverage without a verdict, or names a boundary with a test on only one side. Those rows are the *after* analysis; the sweep only covers the *before*.
- Show the user a concise version of the user-visible examples and the important invariants — enough to catch a wrong contract in one glance, not the full report.
- Fold the contract into the implementation plan and the regression matrix.
- Continue automatically. Do not pause for approval unless the user explicitly asked to approve before implementation. If the user has already approved these examples or said "go" after seeing them, do not ask again.

**`NEEDS DECISION`**

- Show only the concrete unresolved decisions: the competing interpretations, one example each, and the recommendation.
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

## Freeze the scope ledger

At contract acceptance — before the first test or production edit — record the **scope ledger**. It is what makes a later "should this still be in this PR?" decidable instead of a matter of taste, and it is the same discipline `gh-apply-review` already applies to review findings:

- the accepted defect mechanism or requirement boundary, phrased so membership is decidable;
- the behavior rows of the accepted contract;
- the initial production owners the contract names;
- the intended production and test file set;
- the shipped diagnostics, public APIs, and dependencies that must stay unchanged.

Classify every later discovery — from your own implementation, from the local self-review, from a preflight finding — as exactly one of:

- **same mechanism/requirement** — stays in this PR;
- **PR-introduced** — a defect this diff created; stays in this PR even when its mechanism differs;
- **new mechanism or pre-existing** — leaves the PR as a follow-up draft, regardless of severity.

A **scope checkpoint** is mandatory when a change or repair:

- touches a new diagnostic or behavior owner;
- changes a shared base or utility that has other consumers;
- changes public API or dependencies;
- introduces repository-wide "canonical" policy ownership;
- materially enlarges the compiled production file set;
- more than doubles the originally intended production paths.

At that checkpoint, stop and present three explicit choices: narrow or revert the change, approve an expanded contract and scope, or create and link a follow-up issue. Do not pick one silently — an expanding repair is exactly how a bounded fix turns into an unreviewed refactor.

For follow-up work, use the same mechanism as `gh-apply-review`'s follow-up preservation and publication steps: stable `F<n>` IDs, an English copy-ready draft written from the matching template in `.codex/commands/draft-issue.md`, the complete draft in the final chat response, and the same text cached under the ignored `plans/issues/pr-<PR>/`. Create the GitHub issue only after the user explicitly approves that draft ID and content, and never invoke `scripts/upload-issues.ps1`.

## Convert the contract into a regression matrix

Before production code changes, turn the accepted contract — or the routine run's contract note — into the complete test plan. A single narrow test is not acceptable when the contract identifies a broader defect class — that is precisely the gap that produces another review round.

The matrix must cover, for the surfaces the contract names:

- every known defect variant from the issue and the contract's adversarial matrix;
- every candidate from the contract's defect-class enumeration, including non-reproducing candidates that prove the boundary and every reproducing in-scope candidate from the sweep;
- a test on **each** side of every boundary named in the contract's guard-delta and predicate-boundary tables — "no trivia at all" and "trivia on one line" are two different tests, and shipping only the first is how a narrowed guard looks covered while its real boundary is untested;
- one test per region the guard-delta table shows losing coverage, asserting the decision that depends on that region still holds;
- stable valid examples that must **not** change (the anti-regression side);
- misaligned or invalid examples that must change;
- code-fix convergence (one pass silences the diagnostic and raises no new RH diagnostic);
- Fix All across several diagnostics in one document, where the fix supports it;
- formatter second-pass idempotency;
- analyzer / formatter / code-fix parity in both directions;
- the trivia and directive cases the contract flagged as relevant (comments before the token and before both delimiters, `#if`/`#endif`/`#pragma`, disabled text);
- LF **and** CRLF coverage whenever layout is affected.

Per `AGENTS.md`, an analyzer or formatter bug must be reproduced by a failing test before production code changes. Add the intended regression tests first, watch them fail for the right reason, then implement. Analyzer tests stay many small focused tests rather than one large multi-case test.

Track the contract row IDs (`B1`, `B2`, …) in your working plan and in the final report, never in the repository. Test names and test bodies have to stand on their own long after the contract is gone, so `KeepsSingleLineValueBesideKey` is the deliverable and `B1` is scaffolding — do not write contract IDs into test code, comments, or any tracked file.
### Use the existing test infrastructure

The repository already owns most of this coverage; write new helpers only when none of these fits. Naming the right helper matters — `VerifyFormatterFix` proves the formatter clears the diagnostic, but it checks neither a second pass nor CRLF, so a layout change verified with it looks green while the invariant is untested.

| Concern | Use |
|---|---|
| Analyzer diagnostic + code fix, one pass | `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Verify(source, fixedSource, …)` — the verifier re-analyzes the fixed state, which is the convergence check |
| Code fix applied by hand / several diagnostics | `ApplyCodeFixAsync`, `GetCodeFixActionsAsync` on the same base |
| Analyzer ↔ formatter parity | `Analyzer.Test/Base/FormatterTestsBase<TAnalyzer>.VerifyFormatterFix` |
| Parity **plus** second pass **plus** LF/CRLF | `VerifyFormatterFixAndIdempotency` — required whenever the change affects layout |
| Analyzer-clean code must stay untouched | `VerifyFormatterStability` |
| Formatter phase behavior, per line ending | `Formatter.Test/Helpers/FormatterTestsBase.AssertRuleResult(input, expected, endOfLine)` with `_lineEndings` |
| Repo-wide formatter stability | `Formatter.Test/Idempotency/SelfHostingTests` already covers every source file — do not duplicate it per rule |
| Rule doc, README row, code-fix and formatter columns | `Analyzer.Test/SelfHosting/AnalyzerPackageMetadataTests` enforces these; keep the doc and README in sync instead of adding a test |

Record which helper each new test uses; the local self-review and the run report reference it.

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

**Delegation rule.** When a command matches, follow that command's workflow as written. The orchestrator's job is to wrap it with the environment setup, the Behavior Contract, the validation, and the PR — it does not relax or override the delegated command's own checklist (regression-test-first, single focused tests, code-fix-only-if-comprehensive, etc.). The accepted contract is an input to the delegated command, not a replacement for it.

If the issue contains two clearly separable concerns (e.g. a formatter bug *and* a new resource text), prefer two PRs over one. Open the most blocking one first and leave a `Follow-up work` note in the PR pointing at the second.

## Branch and commit

The branch already contains the empty claim commit, is pushed, and has an open draft PR. Its slug is a lower-kebab-case excerpt of the issue title (≤ 4 words).

1. Make the change via the delegated command. Stage only the files that belong to this issue. Never `git add -A` blindly — the cloud worktree may contain unrelated changes.

2. Format **the changed files** through the CLI before tests:

   ```shell
   scripts/format.sh --no-install <changed-path-1> [<changed-path-2> ...]
   ```

3. Run the focused tests for the changed rule or phase — not the suite:

   ```shell
   scripts/test.sh --no-install --project analyzer --filter "FullyQualifiedName~RH3204"
   ```

4. Commit with a Conventional-Commits style subject that mentions the issue and ends with `[skip ci]` (see "Keep CI silent until everything is done"), then push it:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>) [skip ci]
   ```

## Update the draft after focused commits

Immediately after the first focused implementation commit is pushed, update the existing draft PR's **body** with `gh pr edit`. Replace the generic placeholder wording with what the commits actually changed, retain `Closes #<N>`, and fill every template section. Update the body again whenever later commits materially change the summary, review notes, or follow-up work. Leave the placeholder **title** (`Claim: issue #<N>`) as-is for now — the mandatory full title rewrite happens once in "Complete the draft pull request", from the finished change, not incrementally. Keep the PR draft while validation is running and implementation continues.

## Local self-review and the admission artifact

The official preflight is a final quality gate, not a discovery loop, and you only get two attempts at it (see the budget below). Spend the cheap check first: walk your own change locally, in this agent, with no extra agent and no full suite.

Check, concretely:

- **every Behavior Contract row** — for each `B<n>`, name the test or code path that satisfies it; on a routine run, walk the contract note the same way;
- **counterpart parity** — formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable;
- **defect-class closure** — grep for sibling shapes and private copies of the policy you changed; a guard that covers only the reported example is not closure;
- **sweep closure** — every code-derived candidate still has the expected result after implementation, and every reproducing in-scope row maps to a regression test;
- **boundary closure** — every row of the guard-delta and predicate-boundary tables holds against the code you actually wrote, with a test on each side;
- **convergence** — the code fix silences its own diagnostic in one pass and raises no new RH diagnostic;
- **idempotency** — a second formatter pass over the output is a no-op, on LF and CRLF;
- **comments and directives** — the trivia shapes the contract marked relevant survive at sensible positions, or the edit is refused;
- **comment and documentation consistency** — for **every method whose body changed**, re-read its XML summary and its inline comments and confirm they still describe the code they sit next to. A comment that documents the previous behavior is a defect in the same diff that changed it, and it is the single most common thing an audit returns once everything else is right;
- **documentation** — `documentation/rules/RH####.md` matches the shipped behavior when a rule changed;
- **changed-path formatting** — every changed C# path went through `scripts/format.sh --no-install`;
- **focused tests** — the tests for the changed rule/phase pass at the current working tree.

Fix what you find now. This is not an official preflight, does not consume a preflight attempt, and is not reported as one.

### The admission artifact

A self-review whose result is a paragraph of prose ("ownership looked consistent, no sibling copy remained") is not checkable, and it is how three policy owners survive into an audit. Before preflight may start, record a compact, falsifiable artifact — in chat and your working plan, never in a tracked file:

| Row | Content |
|---|---|
| Requirement qualifiers | each qualifier from the issue and its exact owner and predicate |
| Changed predicates | each one, plus the test on **each** side of its boundary |
| Policy owners | the exact `rg` invocation and its result for every changed policy |
| Contract and sweep rows | each row and the regression test that covers it |
| New tests | each test's invariant, the observation that would falsify it, and the helper it uses |
| Comments and documentation | each changed method whose summary and inline comments were re-read |

A missing row blocks admission to preflight. That is the whole point: an incomplete row is a cheap local gap now and an expensive audit finding twenty minutes later.

On a routine run the artifact keeps the rows that apply to text — referenced paths and links resolve, YAML frontmatter parses, `git diff --check` is clean, nothing contradicts `AGENTS.md`/`AGENTS.md` or a neighboring skill — and marks the rest N/A.

## Synchronize with `origin/main` before the official gate

The audited tree must be the tree that will merge. Synchronizing after a passing preflight invalidates the audit, and preflighting a known-stale or known-conflicting branch wastes an attempt.

1. Fetch the current base:

   ```shell
   git fetch origin main
   ```

2. Check worktree and branch state — `git status --short` must be clean of unintended changes, and the branch must be the PR head.
3. Merge current `origin/main` into the working branch when the branch is behind.
4. Resolve conflicts so that **both** the branch behavior and the `main` behavior survive. A conflict resolution that drops one side is a defect, not a merge detail.
5. Run `scripts/format.sh --no-install --no-install` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Take the preflight decision against that exact head.

If `origin/main` moves again **after** a passing preflight: do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required (does the new `main` touch anything this PR touches?). If it is, say plainly that merging again changes the audited tree, and follow the user's explicit direction — including their decision to rely on CI without spending another preflight attempt.

## Official preflight gate — risk-triggered, hard 1 + 1 budget

`gh-preflight` is the final, independent quality gate. Read `.codex/skills/gh-preflight/SKILL.md` completely and apply it as an internal gate, read-only, on the pushed and synchronized head. Do not post its findings to GitHub. Run it in a fresh, independent read-only subagent when subagents are available, exactly as that skill's reviewer-isolation section requires, and hand it the same evidence bundle the Rubber Duck received.

**First decide whether an audit is required at all.** That decision belongs to `gh-preflight`'s trigger list, not to this file: an audit is required when the diff changes a predicate, guard, or report condition; which tokens or trivia a rewrite writes; a code-fix registration or applicability; a diagnostic ID, severity, or message; public API; a dependency; a repository script, build property, ruleset, or CI workflow; or adds a rule. It is not required for a diff that only edits comments, documentation, Markdown, skill and command files, or templates — including inside `.cs` — or that only adds tests for behavior that is already correct. Prove the comment-only case with `scripts/verify-text-only.sh --no-install` and record the proof line; ask the user when the diff fits neither list. A skipped audit never skips the full validation.

The budget is fixed:

1. **Attempt 1** runs automatically once implementation is complete, the local self-review and admission artifact are complete, `main` is synchronized, and the head is pushed.
2. On `PASS`, continue to full validation.
3. On `PASS — non-blocking cleanup`, apply the listed comment and documentation fixes, prove they changed nothing compiled with `scripts/verify-text-only.sh --no-install --base <audited-sha> --head worktree`, and continue to full validation. This costs no attempt. If the proof rejects the cleanup, the audit no longer covers the tree — treat it as a repair cycle instead.
4. On `BLOCKED — findings`, collect **every** finding into **one** consolidated worklist. Do not start fixing before the worklist is complete, and do not run a preflight in between.
5. Fix the complete worklist in **one** repair cycle: close each finding's full defect class, format the changed paths, run the focused tests, redo the local self-review and admission artifact, then commit and push with `[skip ci]` and update the PR body when needed.
6. **Re-audit the repair, not just the finding.** Preflight's `Required change` column is a suggestion, not a specification — implementing it literally is what turns one finding into the next round's finding. Re-run the guard-delta and predicate-boundary tables against the guard *as repaired*, and add a test on each side of every boundary the repair moved.
7. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent, read-only subagent against the exact new head, carrying the repair-delta inputs that skill defines: the previous report, the previously audited SHA, the repaired SHA, and the repair diff.
8. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head and rerun; a state mismatch is a setup error, not a review result, so it does not consume an attempt. Neither does a reviewer agent that returned no verdict — that costs a process start, and the bounded restart policy applies.

Classify every finding against the frozen scope ledger before fixing it, and take the scope checkpoint when the repair hits one of its triggers. Do not run the full validation suite until an attempt returns a passing result.

A tracked-file change made after a passing preflight means the audited tree is no longer the tree that will merge:

- the change is proven text-only by `scripts/verify-text-only.sh --no-install` → note it and its proof line in the report and continue;
- it touches compiled behavior and an attempt is unspent → spend the retry on the new tree;
- it touches compiled behavior and the budget is exhausted → stop and report. The user decides whether to ship a tree that no audit covered; you do not decide it silently.

The final report must state the preflight decision and its reason, how many official attempts were used, how many reviewer process starts they took, the result of each, and whether the budget was exhausted.

## Full validation — run it once

Focused, filtered tests run throughout implementation. The complete suite runs **once**, after implementation is complete, `main` is synchronized, the preflight decision is settled, and the worktree matches the audited tree. Do not rerun the whole solution after each small fix.

Only a diff that contains **no compiled file at all** skips this section — see "Scope triage". A skipped preflight does not skip validation: a comment-only change inside `.cs` still builds and runs every test project, because the build is what catches a malformed comment or a changed documentation artifact, and test runtime costs wall-clock rather than tokens.

```bash
scripts/build.sh --no-install
scripts/test.sh --no-install --no-build
```

`scripts/test.sh --no-install --no-install` runs all four test projects in order; `--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All four test projects must pass. If any fails:

1. Read the failure, decide if it is caused by your change or a pre-existing issue on `main`.
2. Fix issues caused by your change and commit with `[skip ci]` in the subject before pushing. Do not silence tests or mark them `[Ignore]`. **A change to any compiled file invalidates the build, every project result gathered before it, and the preflight** — those green runs proved the previous tree. Re-run the build and all four test projects on the repaired tree; in this repository a formatter fix really can flip analyzer results, because `Reihitsu.Analyzer.CodeFixes` depends on the formatter and the analyzer tests drive it through `FormatterTestsBase<TAnalyzer>`.
3. If a failure exists on `main` independent of your change, record it in the draft PR's `Review notes` with `gh pr edit` and stop. Do not continue implementation on top of a broken baseline.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and state in the final report exactly which local checks ran and which did not.

Do not list the executed test commands in the PR body. CI re-runs them and the repo convention (`AGENTS.md`) is to keep the PR description concise.

## Complete the draft pull request

1. Push any remaining validation fix-up commits, then add the run's single non-skip-ci commit so the push triggers the one CI run for this issue:

   ```shell
   git push
   git commit --allow-empty -m "Ready for CI (#<N>)"
   git push
   ```

   This is the only commit in the run that must not contain `[skip ci]`.
   The trigger commit is empty, so it carries the audited tree under a new SHA. Prove that before pushing it:

   ```shell
   git diff --exit-code <audited-sha> HEAD
   ```

   It must print nothing. If it prints a diff, the audit no longer covers what you are about to push — reconcile instead of pushing.

2. Update the existing draft PR with `gh pr edit <PR> --title "<title>" --body "<body>"`, rewriting both in the same call. This is the mandatory full rewrite — not an edit of the claim-time placeholder:

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

   - the scope classification (routine or behavioral), which gates it kept or skipped, and why — including the preflight trigger decision and, when the audit was skipped, the `TEXT-ONLY PROOF: PASS …` line that justified it;
   - the run metrics, so the workflow itself can be evaluated after a dozen runs, keeping these apart rather than reporting one number: **official gate verdict attempts**, **reviewer process starts**, state mismatches, tool or environment failures, and elapsed time per gate — plus contract gate result, number of contract rows, defect-class candidates and reproductions, delta-table rows, tests added, findings on the first official preflight, and whether the retry was needed;
   - **scope growth**: the frozen ledger, every discovery classified against it, each scope checkpoint and the choice the user made, and how the production file set changed from the intended one across repair cycles;
   - a coverage table so the reader can check the change without reading the diff:

     | Contract row | Test | Helper |
     |---|---|---|
     | B1 | `RH5111…Tests.KeyAndValueStayOnOneLine` | `VerifyFormatterFixAndIdempotency` |

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
- **Never** claim a comment-only carve-out from reading the diff. Run `scripts/verify-text-only.sh --no-install --no-install` and quote its proof line, or treat the diff as behavioral.
- **Never** invoke preflight before the admission artifact is complete, and never let a missing artifact row become an audit finding.
- **Never** implement a preflight `Required change` literally without re-deriving its scope against the delta tables — that is how one finding becomes two rounds.
- **Never** let a repair grow past a scope-checkpoint trigger without putting the three choices to the user.
- **Never** hand the Rubber Duck subagent your proposed solution, suspected cause, or planned diff — the analysis must stay independent.
- **Never** spawn a second Rubber Duck subagent for the same contract; send follow-up evidence to the existing one.
- **Never** start a third official preflight automatically. The budget is one attempt plus one retry; after that, report and stop.
- **Never** split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- **Never** run the final preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- **Never** start full validation or push the final CI-trigger commit while an audit is required and has not returned `PASS` or `PASS — non-blocking cleanup` for the current PR tree. A recorded, proven skip from the trigger list is the only way past it, and it still leaves the full validation in place unless the diff contains no compiled file at all. If the budget is exhausted without a passing result, stop and report; that is not a licence to proceed.
- **Never** claim that preflight or validation covered the final *commit* when only the tree matches. Say tree, and prove it with `git diff --exit-code <audited-sha> HEAD`.
- **Never** treat an earlier green project result as still valid after a compiled file changed — build and all four projects have to be green on one and the same final tree.
- **Never** write contract row IDs into test code or any tracked file; the mapping lives in the plan and the report.
- **Never** mark the draft PR ready for review without running the full validation above. A green build on three of four test projects is a regression — run all four.
- **Never** open a non-draft PR. The human reviewer marks ready.
- **Never** delay the initial draft PR until implementation exists. Create the empty claim commit and generic-placeholder draft before editing files.
- **Never** copy or paraphrase the issue's title or body into the claim-time draft PR. Title and body are the fixed generic placeholders; the only issue-specific content is the issue number and the `Closes #<N>` link.
- **Never** post claim, PR-link, or status comments on the issue, and do not apply an `in-progress` label. Use the linked draft PR as the ownership record.
- **Never** silence or skip a failing test to make the PR go green.
- **Never** ship a single narrow test when the contract identifies a broader defect class.
- **Never** accept a bug-report contract as `READY` when its code-derived enumeration or candidate sweep is missing or incomplete.
- **Never** finish a run leaving the claim-time placeholder title or wording in place — "Complete the draft pull request" must rewrite both the title and every body section from the actual change.
- **Never** push a commit without `[skip ci]` before validation is green — the empty trigger commit in "Complete the draft pull request" is the only exception.
- **Never** install an SDK, modify `PATH`, or otherwise change the environment. If the preinstalled toolchain is unavailable, record the environment issue in the draft PR and stop.
- **Always** use the authenticated `gh` CLI for GitHub platform operations. Do not call the GitHub REST API with raw `curl`.
- **Never** edit files outside the scope of the issue. Out-of-scope cleanups go in a separate issue or a follow-up note.
- **Never** include a list of locally executed tests in the PR body (per `AGENTS.md`).

## Quick reference

End-state checklist for a finished run:

- [ ] Preinstalled .NET 10 SDK confirmed with `scripts/prepare.sh --no-install`
- [ ] GitHub CLI authentication confirmed with `gh auth status`
- [ ] Issue number extracted and read via `gh issue view`
- [ ] Existing claim or draft PR checked; `codex/issue-<N>-<slug>` pushed with an empty claim commit
- [ ] Generic-placeholder draft PR opened before implementation (title `Claim: issue #<N>`, every template section filled with static generic text, `Closes #<N>`) — nothing paraphrased from the issue
- [ ] Run triaged routine or behavioral against the five criteria, and the decision recorded — with the `TEXT-ONLY PROOF` line when a `.cs` diff was carved out
- [ ] Behavioral run: `gh-rubber-duck/SKILL.md` read in this agent, evidence bundle gathered, and exactly one read-only Rubber Duck subagent spawned before any edit — routine run: contract note written before any edit
- [ ] Behavior Contract accepted (`READY`, or `NEEDS DECISION` resolved by the user) and shown to the user in short form
- [ ] Bug-report contract contains a complete code-derived defect-class enumeration and sweep; non-bug contracts mark both sections N/A
- [ ] Guard-delta and predicate-boundary tables present with a verdict per row, or explicitly `_N/A_`
- [ ] Scope ledger frozen at contract acceptance; every later discovery classified against it
- [ ] Regression matrix derived from the contract, including every sweep candidate and a test on both sides of every named boundary; red tests added before production changes
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `scripts/format.sh --no-install`, focused tests green
- [ ] First focused implementation commit pushed and the draft PR body updated to the actual changes
- [ ] Local self-review completed against every contract row, including comment and documentation consistency for every changed method
- [ ] Admission artifact complete — no missing row — before any audit starts
- [ ] Current `origin/main` merged, conflicts formatted and focused-tested, synchronized head pushed with `[skip ci]`
- [ ] Preflight decision taken from the trigger list; when required, a passing result on that exact tree within the 1 + 1 budget; when skipped, the proof line recorded
- [ ] `scripts/build.sh --no-install` + `scripts/test.sh --no-install` green on the final tree — run once, or recorded as skipped for a diff with no compiled file
- [ ] Trigger commit proven content-free with `git diff --exit-code <audited-sha> HEAD`
- [ ] Every commit up to that point contains `[skip ci]`; the final non-skip-ci trigger commit was pushed to run CI once
- [ ] Final draft PR **title and body fully rewritten** from the actual change — no claim-time placeholder or issue-verbatim wording left; issue linked only through `Closes #<N>` with no ownership comment or label
- [ ] Final report states the scope classification with its reason, the contract gate result, official attempts and reviewer process starts separately, scope growth across repair cycles, and whether the budget was exhausted
