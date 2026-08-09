---
name: gh-implement
description: >-
  Implement a Reihitsu GitHub issue end-to-end in Codex on Linux or Windows. Trigger when the initial prompt references an issue number or URL. Use the preinstalled .NET SDK without changing the environment, claim the issue with a generic-placeholder draft PR, prove every bug report through a tiered reproduction-gate custom agent that writes and runs the failing regression test before any analysis — a report that does not reproduce is confirmed once at the higher tier and then ends the run with that test, a `Refs`-linked PR, and a question to the user instead of paying for a contract, implementation, and audit — triage the run as routine or behavioral, and run the read-only `gh-rubber-duck` Behavior Contract before every further behavioral edit. Require a code-derived defect-class enumeration and candidate sweep for bug reports, turn the accepted contract into the regression matrix, delegate to the matching repository command, self-review, synchronize `origin/main`, use at most two official `gh-preflight` attempts, run full validation once in its custom agent, fully rewrite the draft PR, and push one CI trigger. Use authenticated `gh` for GitHub operations.
---

# Implement GitHub Issue

You are running in Codex on **Linux cloud or local Windows**. The repository checkout, required .NET 10 SDK, and authenticated `gh` CLI are present. Before builds or tests, confirm the SDK with `scripts/prepare.ps1 -NoInstall`; do not install an SDK, modify `PATH`, or otherwise change the environment. Your job is to take a single GitHub issue from unclaimed to a validated draft PR, delegating the actual implementation to the repository's task-specific slash commands whenever one fits.

You own the environment, the issue lookup, the branch, the Behavior Contract gate, the validation, and the pull request. The delegated command owns the production change and its tests.

## Run order

Follow this sequence. The gates exist because rework in this repository is caused by starting to code against an unstated contract and by discovering problems one preflight round at a time — both are far more expensive than the analysis that prevents them.

1. Read the repository instructions (`AGENTS.md`) and the GitHub issue.
2. Check issue ownership and open draft PRs.
3. Claim the issue through the existing ownership workflow when it is unclaimed, then verify the toolchain.
4. Gather the neutral **evidence bundle** once; every isolated agent in this run consumes that same bundle.
5. On a **bug report**, run the **reproduction gate** in one `reihitsu-reproduction` subagent before any other analysis. `NO SCENARIO` and `BLOCKED` end the run here, and so does a `NOT REPRODUCED` once the escalated confirmation agrees.
6. Triage the run as **routine** or **behavioral**, and record which gates that decision keeps.
7. Read `.codex/skills/gh-rubber-duck/SKILL.md` completely in this parent agent (behavioral runs).
8. Spawn exactly one fresh, read-only `reihitsu-rubber-duck` subagent with that bundle (behavioral runs).
9. Receive and process the Behavior Contract.
10. Resolve every `NEEDS DECISION` before editing any further production or test file.
11. Freeze the **scope ledger** from the accepted contract.
12. Convert the accepted contract — including every defect-class sweep row and every delta-table boundary — or, on a routine run, the recorded contract note into the implementation plan and regression-test matrix, with the reproduction test as its **seed**, not as its whole.
13. Add all remaining intended regression tests before production changes.
14. Implement, formatting changed paths and running focused tests as you go.
15. Run the **local self-review** and record its **admission artifact**.
16. Synchronize with current `origin/main`.
17. Decide from `gh-preflight`'s trigger list whether an audit is required, and run it in a `reihitsu-preflight` subagent on that exact synchronized tree when it is.
18. Run the complete **full validation** once, in a `reihitsu-validate` subagent.
19. Push the final non-`[skip ci]` CI trigger and finish the PR.

Claiming the issue (steps 1–3) happens before the reproduction gate, the triage, and the contract gate so ownership is never lost while analysis runs. The toolchain verification fits between the claim and the first `dotnet` command — the reproduction gate is the first thing that needs it.

Production edits do not begin until step 10 permits them. The reproduction test written in step 5 is the **single** test file allowed before the contract, and it exists precisely so that a bug report which does not reproduce never consumes a contract, an implementation, or an audit at all.

## Build environment (after the issue claim)

The required .NET 10 SDK is preinstalled in every supported Codex environment. Claim the issue first; then, before doing anything that touches `dotnet`, verify the toolchain through the repository script:

```shell
scripts/prepare.ps1 -NoInstall
```

`-NoInstall` turns a missing SDK into a failure instead of an installation, so the script verifies and changes nothing. The repository targets `net10.0` and there is no `global.json`; never fall back to an older SDK, install one, or modify `PATH`.

The other repository scripts — `scripts/build.ps1`, `scripts/test.ps1`, `scripts/format.ps1`, `scripts/verify-text-only.ps1` — take the same switch and resolve the SDK the same way. Use them instead of hand-written `dotnet` invocations; they are the single owner of these commands across `AGENTS.md`, `CLAUDE.md`, and every workflow skill. In the Linux cloud environment use the `.sh` variants; the `.ps1` variants are for local Windows.

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

## Subagent tiers live in `.codex/agents/`

Every gate below is spawned by its custom agent name, never by an ad-hoc prompt with a model argument:

| Stage | Custom agent |
|---|---|
| Reproduction gate, and its escalated confirmation | `reihitsu-reproduction` |
| Behavior Contract gate | `reihitsu-rubber-duck` |
| Official preflight, attempt 1 and retry | `reihitsu-preflight` |
| Full validation | `reihitsu-validate` |

The model, reasoning effort, instructions, and tool restrictions come from the corresponding TOML file under
`.codex/agents/`, which is their single owner; `.codex/agents/README.md` records the current assignment and
reasoning. Do not pass `model` or `reasoning_effort` when spawning a normal gate. The one exception is the
escalated reproduction confirmation, which deliberately overrides both upward.

Do not re-tune a stage in this skill. Change its custom agent file so the assignment stays inspectable in one
place. The orchestrator remains the session agent and owns the `Never` rules, scope ledger, and audit budget.

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

## Gather the evidence bundle once

Every isolated agent in this run — the reproduction gate now, the Rubber Duck after it, the preflight at the end — consumes the same neutral **evidence bundle**, so the facts are collected once instead of being reconstructed unreliably per agent:

- the issue JSON and every linked clarification;
- the PR metadata and body, when a PR already exists;
- the base and head SHAs, and the merge base;
- the changed-file list and diff, when there is one;
- your proof that the local checkout matches that head.

The bundle is fact-gathering. It never carries your proposed solution, suspected root cause, planned diff, or preferred interpretation — that is what makes handing it to three different agents safe.

## Reproduction gate (bug reports — before everything else)

A bug report's most expensive failure mode is not a wrong fix. It is a full contract, implementation, and audit spent on a defect that the repository does not actually have, or that the reported example does not actually trigger. The reproduction gate settles that question first, with a test, for the price of one focused test run.

### When it applies

Run it when the issue is a **bug report**: it names an observed output and a differing expected output, carries a bug label, or routes to [`fix-formatter`](../../commands/fix-formatter.md) or [`fix-analyzer-rule`](../../commands/fix-analyzer-rule.md) in the routing table. Skip it for a new rule, new or extended formatter behavior, rule docs, resource texts, issue drafts, and every other feature request — there is nothing to reproduce, and an "expected to fail" test written from a feature request only encodes your guess at the design.

The gate needs no triage decision first: a bug report is behavioral by definition (triage criterion 5), so the two never disagree.

### Why it runs before the contract

[`fix-formatter`](../../commands/fix-formatter.md) and [`fix-analyzer-rule`](../../commands/fix-analyzer-rule.md) already require a failing regression test before production code, and `AGENTS.md` requires it repository-wide. The gate therefore adds no work to the reproducing path — it moves work that has to happen anyway to the front and into a subagent, and it turns the reproduction into an *input* to the Behavior Contract instead of something the contract has to assume from prose. On the non-reproducing path it removes the entire remainder of the workflow.

### The subagent

Spawn **one** fresh custom agent named `reihitsu-reproduction` with no inherited conversation turns. Its
normal model and reasoning effort come from `.codex/agents/reihitsu-reproduction.toml`; pass no overrides. It
is the only writer of the reproduction test, while the parent stays the only writer of commits, pushes, and
GitHub state.

Its prompt contains the evidence bundle, plus:

- the repository root;
- the issue's reported scenario quoted verbatim — input, expected output, observed output, and the surface it names;
- the user's relevant clarifications from this conversation, quoted rather than summarized;
- the "Use the existing test infrastructure" table from "Convert the contract into a regression matrix" below, so the test lands on the right helper rather than a hand-rolled one;
- a strict instruction: it may add and run **test** files and nothing else — no production file, no commit, no push, no PR or GitHub change, no full validation;
- the required report schema below.

Do **not** hand it a suspected root cause, a candidate fix, or a guess at which guard is wrong. Its report likewise states observations only — file, test name, helper, command, expected versus actual — and never a diagnosis, because the Rubber Duck reads that report next and an analysis primed with a theory confirms it instead of challenging it.

### The four outcomes

**`REPRODUCED`** — the test fails, and it fails on the asserted symptom.

"Fails for the right reason" is a requirement, not a formality: a compile error, a missing helper, an ambiguous overload, or an assertion tripping over unrelated layout is an agent defect, not a reproduction. The failure message must name the same expected-versus-actual difference the issue describes. When it does not, send the defect back to the same subagent once; a second miss is `BLOCKED`.

Then:

1. Commit the red test with `[skip ci]` and push it. A red test on the branch is safe — no CI runs until the final trigger commit, and the PR stays draft.
2. Add the test's file, name, helper, command, and failure output to the evidence bundle as an **observed fact**.
3. Continue with the triage and the full workflow, unchanged.

The reproduction test is a **seed**, never the regression matrix. The contract's defect-class enumeration and sweep still decide the real coverage, and they may well rename, move, split, or broaden the seed — a guard that covers only the reported example is exactly what the sweep exists to catch.

**`NOT REPRODUCED`** — the literal example is green, every variant of the fixed fan-out below is green, **and
the escalated confirmation agrees**.

A single green test is weak evidence of absence. By far the most common cause is that the harness normalized the scenario away, so the literal example alone never settles it. Before reporting `NOT REPRODUCED`, the subagent runs this fixed, small fan-out — no enumeration, no dispatch-code analysis, no sweep, because that is the expensive analysis this gate exists to avoid:

- the reported example on **LF and CRLF**;
- the **counterpart** of the reported surface — the formatter when the issue reports an analyzer, the analyzer when it reports formatter output;
- the **code-fix path** when the issue mentions a fix, a "Fix All", or an IDE quick action;
- the **nearest sibling shape** the reported example implies — the same construct one nesting level deeper, or with the trivia the example omitted.

Green on all of them is not yet the verdict. The other outcomes are cheap to reverse: `REPRODUCED` is tested
by every later stage and `BLOCKED` costs one restart. `NOT REPRODUCED` ends the run, downgrades the issue
link, and hands the user a question, so a wrong negative is expensive and quiet. Escalate it exactly once
before acting:

1. Start one fresh `reihitsu-reproduction` custom agent with no inherited turns and explicit
   `model: gpt-5.6-sol` and `reasoning_effort: xhigh` overrides. In runtimes that expose `fork_turns`, set it
   to `none`; a full-history fork cannot carry a model override.
2. Hand it the same evidence bundle plus the first run's complete report: test file and name, helper,
   commands, scenarios, and raw results. Ask it to audit those facts against the repository and return its
   own verdict rather than agree with the first run.
3. The escalated verdict decides. This costs a second **process start**, never a second official gate result.

If the environment cannot override a custom agent for one spawn, perform the confirmation in the parent
instead of acting on an unconfirmed negative. An escalated `REPRODUCED`, `NO SCENARIO`, or `BLOCKED` replaces
the first verdict and follows that outcome's path. A confirmed `NOT REPRODUCED` ends the run:

1. Keep the passing test as a **characterization test** — it records what the repository does today at exactly the scenario the issue described, which is the artifact that makes the next attempt cheap. Commit and push it with `[skip ci]`.
2. **Downgrade the issue link.** The PR body's `Linked issues` section becomes `Refs #<N>`, never `Closes #<N>`. A merge must not close a bug report nobody understood yet — this is the one place in the workflow where the `Closes` link is wrong.
3. Skip the Rubber Duck, the delegated command, and the preflight. The diff adds tests for behavior that is already correct without touching production code, which is an explicit *not required* entry in `gh-preflight`'s trigger list; record that as the reason.
4. Run the **full validation** once anyway — the diff contains a compiled file, and test runtime costs wall-clock rather than tokens.
5. Rewrite the PR title and body from the actual content (a characterization test, not a fix), push the single CI-trigger commit, and keep the PR draft.
6. Report to the user: every scenario that ran green with its helper, the exact commands, and — the part that matters — **what evidence would change the verdict**: the exact input, the line endings, the surface, and the tool version the reporter used.
7. Ask how to proceed, with these choices: request that specific evidence from the issue author; accept the current behavior as correct and close the issue as not-reproducible; or continue into the full contract gate anyway because the user has reason to believe the harness, not the report, is wrong.

**`NO SCENARIO`** — the issue carries no scenario concrete enough to write a test against.

Commit nothing. Stop and ask the user for a concrete input, the expected and the observed output, and the surface. This is precisely the issue shape on which the contract gate burns the most tokens on guesswork, so asking here is the cheapest possible move — never route an underspecified bug report into the Rubber Duck to have it come back `NEEDS DECISION`.

**`BLOCKED`** — the toolchain or the harness prevented an answer.

Apply the same bounded restart policy as the other gates: one start, at most one restart, then report what could not be executed and what would unblock it. A blocked gate is not a `NOT REPRODUCED` and must never be reported as one.

### The report schema

The subagent returns exactly this, and nothing else. Unstable headings break the hand-off into the evidence bundle, and prose invites the diagnosis this gate must not carry:

```markdown
## Outcome

REPRODUCED | NOT REPRODUCED | NO SCENARIO | BLOCKED

## Test

| Field | Value |
|---|---|
| File | `Reihitsu.<Project>.Test/…` |
| Test name | `…` |
| Helper | `…` |
| Command | `scripts/test.ps1 -NoInstall -Project … -Filter …` |

## Scenarios

| # | Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|---|

## Raw result

<the failing assertion's own output, or the passing run's summary line>

## Missing evidence

<what the issue would need for a decidable scenario — `_None._` when the gate answered>
```

`Scenarios` carries one row for the literal example on `REPRODUCED`, and one row per fan-out variant on `NOT REPRODUCED`. `Observed vs expected` states what the run produced against what the issue said it should produce — a fact, not a cause.

### Recording it

The gate result, its outcome, the scenarios that ran, and the helper each used belong in the final report.
`NOT REPRODUCED` additionally records both process starts, both model/effort pairs, the escalated verdict, the
`Refs #<N>` downgrade, and every gate skipped with that reason.

## Scope triage — decide which gates this run needs

Both gates default to **on**. The escape below exists for work that is genuinely small — not for work that merely looks small at first glance, because misjudging that is exactly the failure the gates were built to catch. Classify from evidence, not from the issue's tone or length.

**Mechanical veto first.** Look at the file set before judging anything. If the diff touches any of these, the run is **behavioral** — no further judgment needed:

- `Reihitsu.Analyzer/**`, `Reihitsu.Analyzer.CodeFixes/**`, `Reihitsu.Formatter/**`, `Reihitsu.Core/**`, `Reihitsu.Cli/**`;
- any test project (`Reihitsu.*.Test/**`, `Reihitsu.ArchitectureTests/**`);
- `*.csproj`, `Reihitsu.sln`, `Directory.Build.props`, `*.ruleset`, `.editorconfig`;
- `scripts/**` and CI workflow files — these are not compiled, but they are executable behavior;
- a mixed diff that contains any of the above next to documentation.

Touching a `.cs` file is not by itself a reason to audit when only its comments changed. The veto therefore has one mechanical escape — but it is a proof about a diff, and at triage time no diff exists yet.

So the veto's escape is **claimed** here and **proven** later. At triage, state that you intend a comment-only `.cs` change and which files it will touch. Before the preflight decision, when the change is committed, run the proof against the real diff:

```powershell
scripts/verify-text-only.ps1 -NoInstall -Base <base-sha> -Head <head-sha>
```

Exit code `0` proves that every changed line is comment, documentation, or layout trivia, that no token, directive, disabled-text region, or literal moved, and that both versions parse; record the `TEXT-ONLY PROOF: PASS …` line as the evidence. Exit code `1` restores the veto, and exit code `2` is a tool failure that proves nothing — treat it as `1`. Running the proof on an empty diff proves nothing either: it reports `no changed files` and exits `0`, which is why the claim is not evidence until the diff exists.

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
| Reproduction gate | N/A — a routine run is never a bug report | Mandatory on a bug report, N/A otherwise |
| Rubber Duck subagent | Optional — replace it with a **contract note** (see below) | Mandatory — unless the reproduction gate ended the run |
| Official preflight | Per `gh-preflight`'s trigger list; a proven text-only diff records the skip with its proof line | Per the same trigger list — in practice always, with the 1 + 1 budget; a `NOT REPRODUCED` run skips it as "tests for already-correct behavior" |
| Local self-review + admission artifact | Mandatory, in its routine form (below) | Mandatory |
| Test-first for bug fixes | N/A — a routine run changes no behavior | Satisfied by the reproduction gate, then extended by the contract's regression matrix |
| Full validation | Skipped only when the diff contains **no** compiled file at all | Mandatory — including on a `NOT REPRODUCED` run |

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

The claim is in place, and nothing has been edited beyond the reproduction gate's seed test. Before the first production change or any further test, obtain a **Behavior Contract** from the `gh-rubber-duck` workflow. On a routine run this section is replaced by the contract note above — until a trip-wire fires, at which point it applies in full. A run the reproduction gate ended never reaches this section at all.

### 1. Read the skill in this agent

Read `.codex/skills/gh-rubber-duck/SKILL.md` completely yourself. You need the output schema to consume the contract, to spot a malformed one, and to send follow-up evidence later.

### 2. Spawn exactly one read-only Rubber Duck subagent

Run the analysis in **one fresh `reihitsu-rubber-duck` custom agent** with no inherited conversation turns
and no access to your reasoning.
Isolation is the point: an analysis primed with your intended fix will confirm it instead of challenging it.
Its model and reasoning effort come from `.codex/agents/reihitsu-rubber-duck.toml`; pass no overrides.

Hand over the **evidence bundle** you already gathered, verbatim. On a bug report it now also carries the reproduction gate's observed facts — the seed test's file, name, helper, command, and failure output — which is what lets the sweep start from a confirmed executable reproduction instead of from the issue's prose. Those are observations; the gate's report contains no diagnosis, and you add none.

The subagent prompt must contain that bundle plus, and nothing more:

- the repository root;
- the user's relevant clarifications from this conversation, quoted rather than summarized;
- the path `.codex/skills/gh-rubber-duck/SKILL.md`, with the instruction to read and follow it completely;
- a strict read-only instruction (no edits, commits, pushes, PR changes, GitHub comments, or full validation);
- a request for the exact required output schema.

Do **not** include your own proposed solution, suspected root cause, planned diff, or preferred interpretation. The bundle is fact-gathering; anything that carries your conclusions defeats the isolation the gate exists for.

Use **one** Rubber Duck subagent per implementation run. When the user later clarifies the same contract, continue that same subagent with a follow-up message carrying the new evidence and ask for an amended contract — do not spawn a second one. Spawn a replacement only when the original agent is unavailable or the issue scope changes materially (a different defect, a different rule).

Apply `gh-preflight`'s bounded restart policy here too: one start, at most one restart when the agent errors, returns without a contract, or your own wait passes roughly 15 minutes, then the local fallback below. A start that returned no contract counts as a **process start**, not as a gate result, and the final report states both.

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

Per `AGENTS.md`, an analyzer or formatter bug must be reproduced by a failing test before production code changes. On a bug report the reproduction gate already committed that test — it is the matrix's **seed row**, and the contract decides what the matrix adds around it. Treat the seed as revisable: when the sweep shows it sits on the wrong helper, asserts too narrowly, or belongs on a sibling surface, move it. Shipping the seed unchanged as the whole matrix is the same defect as shipping a guard that covers only the reported example.

Add the remaining intended regression tests before production code, watch them fail for the right reason, then implement. Analyzer tests stay many small focused tests rather than one large multi-case test.

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
   scripts/format.ps1 -NoInstall <changed-path-1> [<changed-path-2> ...]
   ```

3. Run the focused tests for the changed rule or phase — not the suite:

   ```shell
   scripts/test.ps1 -NoInstall -Project analyzer -Filter "FullyQualifiedName~RH3204"
   ```

4. Commit with a Conventional-Commits style subject that mentions the issue and ends with `[skip ci]` (see "Keep CI silent until everything is done"), then push it:

   ```text
   Fix RH3204 code fix for interpolated strings (#<N>) [skip ci]
   ```

## Update the draft after focused commits

Immediately after the first focused implementation commit is pushed, update the existing draft PR's **body** with `gh pr edit`. Replace the generic placeholder wording with what the commits actually changed, retain `Closes #<N>` — `Refs #<N>` on a `NOT REPRODUCED` run — and fill every template section. Update the body again whenever later commits materially change the summary, review notes, or follow-up work. Leave the placeholder **title** (`Claim: issue #<N>`) as-is for now — the mandatory full title rewrite happens once in "Complete the draft pull request", from the finished change, not incrementally. Keep the PR draft while validation is running and implementation continues.

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
- **changed-path formatting** — every changed C# path went through `scripts/format.ps1 -NoInstall`;
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
5. Run `scripts/format.ps1 -NoInstall` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Take the preflight decision against that exact head.

If `origin/main` moves again **after** a passing preflight: do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required (does the new `main` touch anything this PR touches?). If it is, say plainly that merging again changes the audited tree, and follow the user's explicit direction — including their decision to rely on CI without spending another preflight attempt.

## Official preflight gate — risk-triggered, hard 1 + 1 budget

`gh-preflight` is the final, independent quality gate. Read `.codex/skills/gh-preflight/SKILL.md` completely
and apply it as an internal gate, read-only, on the pushed and synchronized head. Do not post its findings to
GitHub. Run it in a fresh, independent `reihitsu-preflight` custom agent with no inherited conversation turns
when subagents are available,
exactly as that skill's reviewer-isolation section requires, and hand it the same evidence bundle the Rubber
Duck received. Its model and reasoning effort come from `.codex/agents/reihitsu-preflight.toml`; pass no
overrides, and give the retry its own fresh instance rather than continuing attempt 1.

**First decide whether an audit is required at all.** That decision belongs to `gh-preflight`'s trigger list, not to this file: an audit is required when the diff changes a predicate, guard, or report condition; which tokens or trivia a rewrite writes; a code-fix registration or applicability; a diagnostic ID, severity, or message; public API; a dependency; a repository script, build property, ruleset, or CI workflow; or adds a rule. It is not required for a diff that only edits comments, documentation, Markdown, skill and command files, or templates — including inside `.cs` — or that only adds tests for behavior that is already correct. Prove the comment-only case with `scripts/verify-text-only.ps1 -NoInstall` and record the proof line; ask the user when the diff fits neither list. A skipped audit never skips the full validation.

The budget is fixed:

1. **Attempt 1** runs automatically once implementation is complete, the local self-review and admission artifact are complete, `main` is synchronized, and the head is pushed.
2. On `PASS`, continue to full validation.
3. On `PASS — non-blocking cleanup`, apply the listed comment and documentation fixes, prove they changed nothing compiled *and no public API documentation* with `scripts/verify-text-only.ps1 -NoInstall -StrictDocs -Base <audited-sha> -Head worktree`, and continue to full validation. This costs no attempt. If the proof rejects the cleanup, the audit no longer covers the tree — treat it as a repair cycle instead.
4. On `BLOCKED — findings`, collect **every** finding into **one** consolidated worklist. Do not start fixing before the worklist is complete, and do not run a preflight in between.
5. Fix the complete worklist in **one** repair cycle: close each finding's full defect class, format the changed paths, run the focused tests, redo the local self-review and admission artifact, then commit and push with `[skip ci]` and update the PR body when needed.
6. **Re-audit the repair, not just the finding.** Preflight's `Required change` column is a suggestion, not a specification — implementing it literally is what turns one finding into the next round's finding. Re-run the guard-delta and predicate-boundary tables against the guard *as repaired*, and add a test on each side of every boundary the repair moved.
7. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent
   `reihitsu-preflight` custom agent against the exact new head, carrying the repair-delta inputs that skill
   defines: the previous report, the previously audited SHA, the repaired SHA, and the repair diff.
8. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head and rerun; a state mismatch is a setup error, not a review result, so it does not consume an attempt. Neither does a reviewer agent that returned no verdict — that costs a process start, and the bounded restart policy applies.

Classify every finding against the frozen scope ledger before fixing it, and take the scope checkpoint when the repair hits one of its triggers. Do not run the full validation suite until an attempt returns a passing result.

A tracked-file change made after a passing preflight means the audited tree is no longer the tree that will merge:

- the change is proven text-only by `scripts/verify-text-only.ps1 -NoInstall` → note it and its proof line in the report and continue;
- it touches compiled behavior and an attempt is unspent → spend the retry on the new tree;
- it touches compiled behavior and the budget is exhausted → stop and report. The user decides whether to ship a tree that no audit covered; you do not decide it silently.

The final report must state the preflight decision and its reason, how many official attempts were used, how many reviewer process starts they took, the result of each, and whether the budget was exhausted.

## Full validation — run it once

Focused, filtered tests run throughout implementation. The complete suite runs **once**, after implementation is complete, `main` is synchronized, the preflight decision is settled, and the worktree matches the audited tree. Do not rerun the whole solution after each small fix.

Only a diff that contains **no compiled file at all** skips this section — see "Scope triage". A skipped preflight does not skip validation: a comment-only change inside `.cs` still builds and runs every test project, because the build is what catches a malformed comment or a changed documentation artifact, and test runtime costs wall-clock rather than tokens.

```powershell
scripts/build.ps1 -NoInstall
scripts/test.ps1 -NoInstall -NoBuild
```

Run these commands through one fresh `reihitsu-validate` custom agent with no inherited conversation turns
when subagents are available. Pass the exact tree to validate and whether any file changed since the Release
build. It returns pass/fail per step, failing
assertions, and the raw-log path, which keeps thousands of output lines out of the orchestrator context. Its
model, effort, and edit-denial hook come from `.codex/agents/reihitsu-validate.toml`; pass no overrides. It
fixes and diagnoses nothing. Without subagents, run the commands in the parent and capture their output to a
temporary file instead of the transcript.

`scripts/test.ps1 -NoInstall` runs all four test projects in order; `-NoBuild` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All four test projects must pass. If any fails:

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

   The single exception is a `NOT REPRODUCED` run, whose PR contains a characterization test and no fix. It uses `Refs #<N>` instead, so merging it does not close a bug report that was never explained.

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

   - the reproduction gate outcome (`REPRODUCED`, `NOT REPRODUCED`, `NO SCENARIO`, `BLOCKED`, or N/A for a non-bug issue), the scenarios it ran with the helper each used, and — on `NOT REPRODUCED` — the `Refs #<N>` downgrade, every gate skipped with that as the reason, and the evidence that would change the verdict;
   - the scope classification (routine or behavioral), which gates it kept or skipped, and why — including the preflight trigger decision and, when the audit was skipped, the `TEXT-ONLY PROOF: PASS …` line or the "tests for already-correct behavior" entry that justified it;
   - the run metrics, so the workflow itself can be evaluated after a dozen runs, keeping these apart rather than reporting one number: **official gate verdict attempts**, **reviewer process starts**, state mismatches, tool or environment failures, and elapsed time — plus token cost per gate where the environment reports it, **and the effective model and reasoning effort of every gate** — plus the reproduction gate outcome, whether it was escalated and what the escalation returned, contract gate result, number of contract rows, defect-class candidates and reproductions, delta-table rows, tests added, findings on the first official preflight, and whether the retry was needed;
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

- Settle a bug report's reproduction with one focused test run before spending a contract, an implementation, and an audit on it. The gate is the cheapest step in the workflow and the only one that can remove the three most expensive ones outright.
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

- **Never** edit a production or test file on a behavioral run before the Behavior Contract gate returns `READY` (or before the user has resolved a `NEEDS DECISION`). On a routine run the recorded contract note takes its place, and the reproduction gate's single seed test is the one file that legitimately precedes the contract.
- **Never** enter the Rubber Duck, a delegated command, or an audit on a bug report before the reproduction gate returned `REPRODUCED`. That gate is the cheapest step in the workflow and it is what stops the three most expensive ones from being spent on a defect that is not there.
- **Never** report `NOT REPRODUCED` from the literal example alone, or from a green run whose test never executed, compiled, or asserted anything. The fixed fan-out — LF and CRLF, the counterpart surface, the code-fix path, the nearest sibling shape — is the minimum, and a `BLOCKED` gate is never reported as a non-reproduction.
- **Never** act on `NOT REPRODUCED` — end the run, downgrade the link, or ask the user — before the single escalated confirmation agrees. The other outcomes are reversible by the stages that follow; this one is not.
- **Never** spawn a normal gate with an ad-hoc prompt or inline model/effort values instead of its custom agent from `.codex/agents/`. The escalated reproduction confirmation is the one deliberate override, and it only moves the tier upward.
- **Never** let the validation agent edit, commit, diagnose, or repair. It runs the canonical scripts and reports; every decision stays with the parent.
- **Never** accept a red seed test as a reproduction when it fails on a compile error, a missing helper, or unrelated layout. The failure has to name the issue's own expected-versus-actual difference.
- **Never** leave `Closes #<N>` in the PR of a `NOT REPRODUCED` run — a merge would close an unexplained bug report. Use `Refs #<N>` and say in `Review notes` that the test characterizes current behavior.
- **Never** hand the reproduction subagent a suspected root cause or a candidate fix, and never let its report carry a diagnosis into the Rubber Duck — only observed facts.
- **Never** ship the reproduction gate's seed test as the whole regression matrix when the contract names a broader defect class.
- **Never** classify a run routine to dodge the gates. The five criteria are all-or-nothing, a trip-wire voids the classification, and the report has to name the reason.
- **Never** claim a comment-only carve-out from reading the diff. Run `scripts/verify-text-only.ps1 -NoInstall` and quote its proof line, or treat the diff as behavioral.
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

- [ ] Preinstalled .NET 10 SDK confirmed with `scripts/prepare.ps1 -NoInstall`
- [ ] GitHub CLI authentication confirmed with `gh auth status`
- [ ] Issue number extracted and read via `gh issue view`
- [ ] Existing claim or draft PR checked; `codex/issue-<N>-<slug>` pushed with an empty claim commit
- [ ] Generic-placeholder draft PR opened before implementation (title `Claim: issue #<N>`, every template section filled with static generic text, `Closes #<N>`) — nothing paraphrased from the issue
- [ ] Evidence bundle gathered once, carrying no proposed solution or suspected cause
- [ ] Bug report: reproduction gate run in one `reihitsu-reproduction` custom agent before any other analysis, its outcome recorded, and the seed test committed with `[skip ci]` — non-bug issue: gate recorded N/A
- [ ] `NOT REPRODUCED` run: fixed fan-out executed (LF/CRLF, counterpart surface, code-fix path, sibling shape), the verdict confirmed by the single escalated run, characterization test committed, PR linked with `Refs #<N>` and **not** `Closes #<N>`, contract and preflight skipped with that reason, full validation still run, and the user asked how to proceed
- [ ] Run triaged routine or behavioral against the five criteria, and the decision recorded — a claimed `.cs` carve-out proven with `TEXT-ONLY PROOF` against the real diff before the preflight decision, never against an empty one
- [ ] Behavioral run: `gh-rubber-duck/SKILL.md` read in this agent and exactly one read-only `reihitsu-rubber-duck` custom agent spawned with that bundle before any further edit — routine run: contract note written before any edit
- [ ] Behavior Contract accepted (`READY`, or `NEEDS DECISION` resolved by the user) and shown to the user in short form
- [ ] Bug-report contract contains a complete code-derived defect-class enumeration and sweep; non-bug contracts mark both sections N/A
- [ ] Guard-delta and predicate-boundary tables present with a verdict per row, or explicitly `_N/A_`
- [ ] Scope ledger frozen at contract acceptance; every later discovery classified against it
- [ ] Regression matrix derived from the contract with the reproduction seed as one row rather than the whole, including every sweep candidate and a test on both sides of every named boundary; red tests added before production changes
- [ ] Delegated command (or inline plan) selected from the routing table
- [ ] Change made, files formatted via `scripts/format.ps1 -NoInstall`, focused tests green
- [ ] First focused implementation commit pushed and the draft PR body updated to the actual changes
- [ ] Local self-review completed against every contract row, including comment and documentation consistency for every changed method
- [ ] Admission artifact complete — no missing row — before any audit starts
- [ ] Current `origin/main` merged, conflicts formatted and focused-tested, synchronized head pushed with `[skip ci]`
- [ ] Preflight decision taken from the trigger list; when required, a passing result from a `reihitsu-preflight` custom agent on that exact tree within the 1 + 1 budget; when skipped, the proof line recorded
- [ ] `scripts/build.ps1 -NoInstall` + `scripts/test.ps1 -NoInstall` green on the final tree — run once through `reihitsu-validate`, or recorded as skipped for a diff with no compiled file
- [ ] Every normal gate spawned by its custom agent name with no model/effort override, and the effective model and effort recorded in the final report
- [ ] Trigger commit proven content-free with `git diff --exit-code <audited-sha> HEAD`
- [ ] Every commit up to that point contains `[skip ci]`; the final non-skip-ci trigger commit was pushed to run CI once
- [ ] Final draft PR **title and body fully rewritten** from the actual change — no claim-time placeholder or issue-verbatim wording left; issue linked only through `Closes #<N>` with no ownership comment or label
- [ ] Final report states the reproduction gate outcome, the scope classification with its reason, the contract gate result, official attempts and reviewer process starts separately, scope growth across repair cycles, and whether the budget was exhausted
