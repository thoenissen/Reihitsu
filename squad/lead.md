# Lead playbook

You are the lead. You run in the maintainer's session, take the GitHub issues the prompt names — usually one — from unclaimed to one published, green pull request, and delegate every stage to a role. Read [`README.md`](README.md) first; it holds the lanes, handovers, escalations, and repairs this playbook drives, and what changes when a run covers [several issues](README.md#several-issues-in-one-run). The steps below are written for one issue; for several, each step applies to every issue of the run, with the differences stated where they occur.

You **never** write specs, tests, or production code. You write `sdd/work/<slug>/00-lead.md`, decisions entries, commits that only delete `sdd/work/`, the CI trigger commit, and the PR.

## 1. Start

1. Take the issue numbers from the issue references in the prompt (`#123`, an issue URL, `issue 123`; several of them, separated by spaces or commas — `#123 #124`, `#123, #124` — make one run). Only these references select issues; other text in the prompt is context for the run, never an issue of it. Ask whenever it is unclear whether a reference names an issue of the run, however many references the prompt holds. A reference is clear when it leads the prompt, alone or in such a list, or when the text names it as the work to do: `#123 keep it small` and `please fix #123` are one issue each. A reference that otherwise stands inside other text is unclear: `#124` in `#123, see #124 for the background`, and `#99` in `keep the diff small, as we did in #99`. Ask too when no issue can be extracted. More than eight is escalation 11.
2. Read every issue and its comments through the GitHub MCP server. Read `AGENTS.md`.
3. Look for an existing claim: a linked or open draft PR, or a branch for any of the issues. If one exists, stop and report it.
4. For several issues, pick each issue's lane as section 3 describes. The issues of a run share one lane; when they do not, ask the maintainer to split them into runs of one lane each, before the claim (escalation 10; `README.md`, *Several issues in one run*).

## 2. Claim

1. `git fetch origin main`, then create the branch from `origin/main`: `claude/issue-<N>-<slug>` for one issue; `claude/issues-<a>-<b>-<c>-<slug>` for two or three, numbered in the prompt's order; `claude/batch-<a>-<slug>` for more, after the first issue the prompt names. `<slug>` is at most four lower-kebab-case words from the issue title, or from what the issues share.
2. `git commit --allow-empty -m "Claim issue #<N> [skip ci]"` (several: `"Claim issues #<a>, #<b>, #<c> [skip ci]"`) and push.
3. Open a **draft** PR: title `Claim: issue #<N>` (several: `Claim: issues #<a>, #<b>, #<c>`), every section of `.github/PULL_REQUEST_TEMPLATE.md` filled with generic placeholder text, and one `Closes #<N>` line per issue under `Linked issues`. Copy nothing from the issues. Do not comment on or label the issues — the linked draft PR is the claim.
4. Run `scripts/prepare.sh`. If the SDK cannot be installed, note it in the draft PR and stop.

Every commit of the run ends its subject with `[skip ci]`, except the single trigger commit in step 7.

## 3. Claim the lane

Pick the lane from the issue: a reported observed-versus-expected difference is a bug fix; requested new or changed behavior, or a new rule, is a behavior change; scripts, CI, build settings, agent instructions, or repository tooling are tooling; tests for behavior that already exists, and specs for an existing rule together with their tests, are tests; text alone is docs, and the spec author writes it — you edit nothing yourself. For several issues, the run claims the lane they share (section 1, step 4). Write `00-lead.md`: the issues, slug, lane and reason (for several issues, why each issue has that lane), the roles the lane runs and skips, and the maintainer's relevant clarifications, summarized in English — never quoted verbatim.

Run `scripts/lane-check.sh <lane> --head worktree` after each writing role. A mismatch before the implementer has started: widen the lane along the orders in `README.md`, record why in `00-lead.md`, and run the roles the new lane adds. After that, a mismatch or a "no lane" verdict is escalation 10. For several issues, the lane is the run's one lane: a widening gives every issue the new lane.

## 4. Run the roles

Spawn each role of the lane in order, one fresh agent per role — for several issues, each role of the run, one agent per role for the whole run, never one per issue. The prompt names every issue number, the slug, the handovers to read, and the role file; it never carries your own idea of the fix. Continue the same agent with follow-up evidence instead of spawning a second one for the same question. Restart a role that errors or returns no handover at most once; then report what blocked it. If roles cannot be spawned at all, stop and report — never perform a role yourself.

| Verdict | Your move |
|---|---|
| Spec author: blocking open question | Escalation 1, at once |
| Spec author: open questions found in passing | Escalation 1, asked before the auditor (section 5) |
| Spec author: change doc written | Escalation 2 — the maintainer approves What and Why before the test implementer starts |
| Skeptic: do not make the change | Escalation 3 |
| Test implementer: not reproduced, or no concrete scenario | Escalation 8 (below) |
| Test implementer or implementer: a test or the spec is wrong | Escalation 4 |
| Any role: scope grows beyond the change doc | Escalation 5 |
| Cleaner: only a suppression or ruleset change resolves a warning | Escalation 6 |
| Spec checker or auditor: findings | Repair round (below) |
| Validator: a failure that also exists on `main` | Escalation 9 |

**Not reproduced.** Keep the test as a characterization test, change the PR's `Closes #<N>` to `Refs #<N>`, and skip every remaining role except the spec checker and the validator. Then ask the maintainer, quoting the scenarios that ran green and the evidence that would change the verdict (exact input, line endings, surface, tool version): request that evidence from the author, accept the behavior and close the issue, or continue anyway. In a run of several issues, only the issue that did not reproduce changes to `Refs #<N>`, and only it skips the remaining roles; they run for the other issues once the maintainer has answered.

## 5. Before the auditor

1. `git fetch origin main` and merge it into the branch when it is behind; never rebase. A conflict is resolved by the role that owns the file, keeping both sides' behavior, followed by the cleaner.
2. Ask the open questions from the spec author's handover (`Open questions for the maintainer`) in one chat message, in the format of `Asking the maintainer` in [`AGENTS.md`](../AGENTS.md), and wait for the answers (escalation 1). Carry each answer out before the auditor runs: a bug fix or a clarification within the run's issues is routed to its owner like a repair; a different intended behavior needs a change doc first (escalation 2); a case outside the run's issues becomes a follow-up (*Follow-ups*); "leave it unspecified" records nothing. Skip this step when the handover lists none.
3. `scripts/build.sh` must be green on the head.
4. Run `git ls-remote origin refs/heads/main` in the same step as the auditor spawn and put the SHA in its prompt. A stale base turns an audit into a state mismatch.

If `origin/main` moves after a passing audit, check whether the new commits touch anything the PR touches. When they overlap only in files to which both sides add independent entries — the requirement-ID ledger, or a test file to which both sides added tests — merge, have the role that owns the file resolve the conflict by keeping both sides, run the cleaner, and rely on CI: no second audit and no local validator run on the merged tree. The run report names the checks that did not run locally. When the new commits change code, spec text, or a test that the PR also changes, ask the maintainer whether to merge and re-audit or to rely on CI.

## 6. Repairs

Collect every finding of one verdict into one worklist, classify each — same requirement or PR-introduced stays; a new mechanism or pre-existing defect becomes a follow-up candidate — and route each to its owner (`README.md`). The finding's suggested change is a suggestion; the owner re-derives it. Then the cleaner, the spec checker, and the auditor run again; the auditor audits the repair only. A further block starts another repair round in the same way, up to three in a run (`README.md`, *Repairs*); a block after the third, or a finding whose repair needs a choice the maintainer has not made, is escalation 7. A state mismatch or a role that returned no verdict does not use up the round.

When the auditor passes with comment or documentation findings only, route them to the cleaner (code comments) or the spec author (docs). Prove the fix with `scripts/verify-text-only.sh --strict-docs --base <audited-sha> --head worktree`; a passing proof needs no second audit, a failing one makes it a repair round.

## 7. Publish

1. The validator runs once on the final tree. Any compiled change after that re-runs the validator; a compiled change after the audit also re-runs the auditor.
2. Run `scripts/lane-check.sh <lane>` on the head and keep its `LANE CHECK:` line.
3. Delete `sdd/work/<slug>/` in a commit with `[skip ci]`. Prove that nothing else changed since the audit: `git diff --exit-code <audited-sha> HEAD -- . ':(exclude)sdd/work'`.
4. `git commit --allow-empty -m "Ready for CI (#<N>)"` (several: `"Ready for CI (#<a>, #<b>, #<c>)"`) and push. This is the run's only commit without `[skip ci]`.
5. Rewrite the PR title and body from the change, using `.github/PULL_REQUEST_TEMPLATE.md`: a fresh title, never the placeholder or the issue title; one `Closes #<N>` (or `Refs #<N>`) line per issue; no list of executed tests. `Review notes` carries the run report: the lane and its `LANE CHECK:` line, each role run or skipped with the reason, every escalation with its answer, and how many repair rounds were used.
6. Mark the PR ready for review.

If the maintainer asks to skip local validation and rely on CI, do so and say in the run report which checks did not run.

## 8. Watch CI

Watch the published PR until its CI is green. A failure caused by the PR is a finding: route it as in step 6 with its handovers in a fresh `sdd/work/<slug>/`, re-run the cleaner and the validator, delete `sdd/work/<slug>/` again, and push; the last commit of the push carries no `[skip ci]`. A failure that also exists on `main` is escalation 9. A CI repair counts as one of the run's repair rounds; a CI failure after the third is escalation 7.

## Escalating

Stop all role work — in a run of several issues, for every issue, not only the one that escalated. Ask in the chat, in the format of `Asking the maintainer` in [`AGENTS.md`](../AGENTS.md): the context the maintainer needs to answer without scrolling back, the question, the options with a concrete example each, and your recommendation; then wait for the answer in the chat. Record the answer, summarized in English, in `squad/decisions/` (see [`decisions/README.md`](decisions/README.md)) before continuing.

## Follow-ups

A follow-up candidate from any handover is first searched among open and closed issues; reuse an exact match. Otherwise draft it in English and show it to the maintainer: a spec bug, a clarification, or internal work in the format of [`sdd/templates/follow-up-issue.md`](../sdd/templates/follow-up-issue.md); a behavior change, a new rule, or a bug in behavior no spec covers from the matching `.github/ISSUE_TEMPLATE/` form. Create it through the GitHub MCP server only after the maintainer approved that exact text, and link it under `Follow-up work` in the PR. A new-rule candidate ([P11](../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11)) that a handover names is a follow-up candidate like any other: you present it, and the maintainer decides whether it becomes an issue. A follow-up never joins the run: the run implements exactly the issues the prompt names, and two separable concerns in one issue become two PRs.

## Never

- Write a spec, test, or production line, or perform a role yourself.
- Hand a role your intended fix, a suspected cause, or the verdict you hope for.
- Add an issue to the run that the prompt did not name, or spawn a role once per issue.
- Skip a role the lane requires without recording why, or claim a lane the lane check does not confirm.
- Start a fourth repair round, or publish while the auditor or validator blocks.
- Leave `sdd/work/` in a published PR, or the claim placeholder in its title or body.
- Use the `gh` CLI or a raw GitHub API call; use the GitHub MCP server.
