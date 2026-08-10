---
name: gh-preflight
description: Run a read-only local preflight of a Reihitsu pull request before external review or re-review. Use for `/gh-preflight`, `$gh-preflight`, "preflight this PR", "review before review", or as the risk-triggered final quality gate inside `gh-implement` and `gh-apply-review`, after implementation, the admission artifact, and `origin/main` synchronization but before full validation and the CI trigger. Own the audit trigger list, four gate results, repair-delta retry, and evidence-bundle and restart policy for isolated gate agents. Apply the full `gh-review` checklist, adversarial corpus, test expectations, counterpart tracing, defect-class closure, and three-axis audit of guard scope, policy ownership, and assertion adequacy without posting to GitHub or changing code. Report every confirmed finding in one pass; the parent gets one consolidated repair cycle and at most one retry.
---

# Reihitsu GitHub PR Preflight

Audit the current PR as an independent reviewer before the author declares it ready. Find the issues that would otherwise create another `gh-review` / `gh-apply-review` round.

Preflight is the **final quality gate**, not a discovery or debugging loop. The parent workflow is expected to arrive here with its Behavior Contract satisfied, its complete admission artifact recorded, and current `origin/main` already merged, so this audit confirms a finished change instead of driving it. That is also why the parent may spend at most two official attempts on it: one, plus one retry after a single consolidated repair cycle.

It is also the most expensive step in the workflow, which is why "the diff contains a compiled file" is not the trigger. An audit is spent on risk — a moved predicate, a changed rewrite, a new diagnostic — and never on a guess. This skill owns that trigger list, the gate results, the retry contract, and the evidence-bundle and restart policy; `gh-implement` and `gh-apply-review` reference them rather than restating them, so the policy has exactly one owner.

Preflight is read-only:

- Do not edit source or test files.
- Do not create commits, push, change branches, or change PR state.
- Do not post reviews, comments, replies, or thread mutations.
- Targeted tests may create normal build artifacts when they settle a specific finding.

## Position in the workflow

```text
gh-implement / gh-apply-review
  -> Behavior Contract (gh-implement) or complete review worklist (gh-apply-review)
  -> implementation + focused tests
  -> local self-review -> admission artifact complete (parent, not official)
  -> merge current origin/main, format, focused tests, push
  -> trigger list says preflight is required?
       no  -> record the skip and its proof -> full validation -> final CI-trigger commit
       yes -> gh-preflight  ................ official attempt 1
         PASS                       -> full validation (once) -> final CI-trigger commit
         PASS — non-blocking cleanup -> parent fixes the text, proves it non-behavioral,
                                        -> full validation (once) -> final CI-trigger commit
         BLOCKED — findings          -> one consolidated worklist -> one repair cycle
                    -> local self-review + admission artifact
                    -> gh-preflight  ....... official attempt 2 (retry, fresh agent, repair-delta aware)
                         PASS / cleanup -> full validation (once) -> final CI-trigger commit
                         BLOCKED        -> stop and report; no third attempt without the user
```

When a parent workflow invokes this skill, return the gate result to that workflow and let it continue. Use the strict chat output below only when `/gh-preflight` is invoked directly.

## When preflight is required

**Required** when the diff changes any of:

- a predicate, a guard, or the condition or span under which a diagnostic is reported;
- which tokens or trivia a formatter rewrite writes;
- a code-fix registration or its applicability;
- a diagnostic ID, severity, or message; public API; a dependency;
- a new rule;
- a repository script, build property, ruleset, or CI workflow the build and the workflows themselves depend on.

**Not required** when the diff only:

- edits comments, XML documentation, Markdown, skill and command files, or issue and PR templates — *including inside `.cs` files*;
- adds tests for behavior that is already correct, without touching production code.

A carve-out inside a compiled file is a claim about syntax, so it is proven mechanically rather than argued:

```powershell
scripts/verify-text-only.ps1 -NoInstall -Base <base-sha> -Head <head-sha>
```

Exit code `0` and its `TEXT-ONLY PROOF: PASS …` line are the evidence the parent records in place of the attempt. Exit code `1` means the diff carries compiled behavior, so the attempt is required. Exit code `2` is a tool failure that proves nothing — run the attempt. A line-based `grep` over the diff is not an acceptable substitute: it does not recognize every block-comment form and cannot tell a comment apart from a directive or from comment-looking text inside a string literal.

A rename is **not** a carve-out. Renaming reaches `nameof`, reflection, serialization, source generators, public API, and named arguments, so it stays behavioral unless a narrower mechanical proof establishes otherwise.

**Uncertain → ask the user.** When a diff fits neither list cleanly, ask the user and let them decide. Preflight is the single most expensive step in the workflow and is not spent on a guess — in either direction.

Skipping preflight never implies skipping the full validation. A diff that contains any compiled file still gets the build and all test projects; only a diff with no compiled file at all skips validation too. Test runtime costs wall-clock and almost no tokens, so it is never the thing to economize.

## Calling conditions

Once the trigger list says an audit is required, a parent should invoke it only when all of the following hold. Preflighting earlier burns one of the two attempts on a state that was going to change anyway:

- the intended implementation or review repair cycle is complete and committed;
- the parent's local self-review has run, its findings are fixed, and its admission artifact is complete — a missing row there is a cheap local gap that must not become an expensive audit finding;
- current `origin/main` is merged into the branch and any conflict resolution is formatted and focused-tested;
- the head is pushed and the local checkout matches it;
- `git ls-remote origin refs/heads/main` was run **in the spawning step** and still names the `origin/main` the branch was merged with;
- `scripts/build.ps1 -NoInstall` is green on that exact head.

The last two preconditions exist for measured reasons. A remote-tracking ref proves only when the parent last fetched, and a reviewer spawned against a stale one can run a complete audit and then return `BLOCKED — state mismatch` having produced no gate result — a second of `ls-remote` against minutes of audit. A short build removes an entire class of expensive reasoning from the audit: this repository dogfoods its own analyzers with `TreatWarningsAsErrors=true`, so without that fact a reviewer has to argue from source about whether a merged analyzer change and new documentation can still compile together. Both results belong in the bundle as facts.

## Reviewer isolation and the evidence bundle

When `gh-implement` or `gh-apply-review` invokes preflight and a subagent facility is available, run the audit
in exactly one fresh `reihitsu-preflight` custom agent with no inherited turns or author transcript. Its model, reasoning effort,
and instructions come from `.codex/agents/reihitsu-preflight.toml`; pass no overrides. The parent remains the
only writer and consumes the subagent's gate report.

The subagent receives the repository root, this skill path, and one **immutable evidence bundle** the parent gathered once:

- the issue and PR **by number** (`thoenissen/Reihitsu#<N>`), to be read through authenticated `gh issue view` and `gh pr view` — not pasted in;
- the base and head SHAs, the merge base, and the remote `main` SHA with the time it was read;
- the changed-file list and the diff;
- the parent's proof that the local checkout matches the PR head;
- the `scripts/build.ps1 -NoInstall` result on that head;
- the parent's focused-test results;
- the parent's checklist-applicability list below.

That bundle is neutral fact-gathering. It contains no author conclusion, no suspected finding, and no intended fix, so consuming it preserves independence while removing the unreliable GitHub reconstruction each isolated agent would otherwise repeat. If the bundle disagrees with the repository — a head SHA that is not the checkout, a diff that does not match — return `BLOCKED — state mismatch` rather than auditing a tree nobody is reviewing.

Documents are referenced rather than inlined because isolation is about the parent's *reasoning*, not about documents one authenticated `gh` call retrieves. Fetch them yourself; a reference also cannot go stale the way a paste can.

### Which bundle facts to verify, and which to trust

"Verify anything your audit depends on" is right in principle and, applied flatly, means every reviewer re-runs the suites the parent had just run and reported. Split the facts by cost and by what a wrong one costs:

| Fact | Treatment | Why |
|---|---|---|
| Head SHA, checkout match, remote `main` | **Always verify** | Seconds to check, and a wrong one means the whole audit reviewed a tree nobody is merging |
| Changed-file list and diff | **Always verify** against the checkout | Same cost, and it is the object under review |
| `scripts/build.ps1 -NoInstall` result | **Trust**, unless a finding turns on compilability | The parent ran it on this head, and the full validation re-runs it |
| Focused test results | **Trust and spot-check** | Comparatively expensive, and the parent's full validation re-runs them regardless. Re-run one when a specific finding turns on it — that is the targeted execution this skill already allows |
| Checklist applicability | **Confirm, do not adopt** | See below |

A spot-check is a targeted run that settles a suspicion, not a routine re-run of the parent's suite.

### Checklist applicability comes with the bundle

Six or seven of the 19 checklist items are typically `N/A` for a given diff — security, error handling, performance, parts of SOLID, coupling — and re-deriving that independently in every agent, in every attempt, pays for the same conclusion three times. The parent derives applicability once from the diff's shape and states it in the bundle, per item, with the one-clause reason.

The reviewer **confirms** that list; it does not adopt it. An item the parent marked `N/A` that the diff can in fact reach is itself a finding, and reporting it costs one line. Independence survives because the reviewer retains the verdict — what it loses is the obligation to rediscover six obvious negatives from scratch. Every item still appears in the report with its status, so a silently dropped item stays impossible.

The retry attempt gets its own fresh `reihitsu-preflight` custom agent on the exact new head — never a
continuation of the first one, which would carry its earlier conclusions into a review that is supposed to be
independent — plus the repair-delta inputs below.

If subagents are unavailable, perform the audit locally from GitHub and filesystem evidence. A direct `/gh-preflight` invocation already acts as the reviewer and does not need another agent.

### Bounded restart policy

An isolated reviewer that never returns a verdict must not silently consume the workflow's audit budget, and must not spin either:

1. one agent start;
2. at most one restart when the agent errors, returns without a verdict, or the parent's own wait passes roughly 15 minutes without a result — report which of the three it was rather than inferring activity you cannot observe inside another agent;
3. then the local read-only fallback above, performed by the parent.

A start that produced no verdict costs a **process start**, not an official attempt. The parent reports both numbers separately.

### Attempt 1 and the repair-delta retry

**Attempt 1** is a complete, independent audit of the whole change.

**The retry** is repair-delta-aware. The parent adds to the bundle:

- the previous independent report;
- the previously audited SHA;
- the repaired SHA;
- the repair diff.

Verify every previous finding, the complete repair delta, every guard and predicate the repair moved, the counterparts those reach, and the boundary tests the repair added. Evidence for a decision or a test that is byte-identical to attempt 1 may be reused instead of re-derived — that reuse is the entire saving, and it is what keeps a retry from re-auditing hundreds of untouched tests row by row.

**The retry reports the delta, not a second full report.** Reusing evidence and then re-emitting the complete 19-item checklist and three-axis tables with most rows marked "reused" stops the saving at derivation and never lets it reach the output. A retry returns the previous findings with their status, the rows the repair actually moved, and the verdict — and nothing that was neither re-derived nor changed. Use the retry schema under "Direct chat output"; the full schema belongs to attempt 1.

Reporting less is not reporting selectively: **every** previous finding is named with its status, so a silently dropped one remains impossible, and any checklist item or axis row the repair touched appears in full. A retry that finds a new defect outside the repair delta reports it like any other finding and says where it came from.

Incremental retry mode becomes **invalid** when the repair expands into an unrelated production surface, changes the accepted contract, or materially enlarges the file set. Then audit the change in full and say in the report that scope grew: that situation needs a new scope decision from the parent, not a silent second implementation review.

The previous report is reviewer output, not author reasoning, so receiving it does not break isolation. Author conclusions, suspected findings, and intended fixes stay excluded in both attempts.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR created or updated by the current author task.
3. Otherwise, use the PR associated with the current branch when `gh pr view` identifies exactly one.
4. If no PR can be identified, stop. Never guess.

For a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` command.

Read current state without mutating GitHub:

```shell
gh auth status
gh pr view <N> --json number,title,body,author,baseRefName,headRefName,headRefOid,url,files,commits
gh pr diff <N>
git status --short
git rev-parse HEAD
git log --oneline origin/<base-branch>..HEAD
```

Use `gh issue view` only for an issue linked by `Closes`, `Fixes`, or `Resolves` in the PR body.

The gate requires the local checkout to match the PR head SHA and all intended scoped changes to be committed and pushed. If it does not, return `BLOCKED — state mismatch` with the exact mismatch. Do not switch branches or repair the state from this skill.

What the gate certifies is the **tree**, not the commit id. The parent may add a content-free CI-trigger commit afterwards; that keeps the audit valid as long as `git diff --exit-code <audited-sha> HEAD` prints nothing. Any change with content invalidates the audit.

## Apply the review methodology

Read `.codex/skills/gh-review/SKILL.md` completely. Apply its complete methodology to the current PR:

- all 19 checklist items;
- the relevant adversarial input corpus;
- test expectations for every changed concern;
- severity and confidence rules;
- counterpart tracing beyond the diff;
- static-first verification.

Override the review skill's GitHub-posting, existing-comment deduplication, and output rules with this skill. Review the current code independently; do not fetch prior review comments merely to learn what another reviewer found.

**Corpus breadth stays flat, deliberately.** The three-axis tables scale with the change — one row per changed decision — and it is tempting to scale the adversarial corpus and counterpart tracing the same way, so a one-decision diff gets a narrow corpus. Do not. A narrow corpus on a small diff is precisely the reasoning that misses the boundary finding this gate exists to catch, and the evidence for the change would have to come from runs where a scaled-down corpus demonstrably lost nothing — which is data this workflow does not have yet. Revisit it when the per-gate metrics from a dozen runs exist; until then, breadth is not the knob.

Limit blocking findings to defects caused by the PR, missing issue requirements, incomplete tests required by the change, and pre-existing behavior that the changed code newly depends on or exposes. Record unrelated pre-existing concerns as hints rather than expanding the PR. Give each confirmed scope hint a defect mechanism and `new mechanism` relation so the parent workflow can preserve it against its scope ledger without treating it as a blocking repair.

Complete the entire checklist and relevant adversarial corpus before returning. Report every confirmed finding in one pass; never stop after the first.

Start the checklist from the parent's applicability list rather than from nothing: for each item it marked `N/A`, confirm the diff genuinely cannot reach it and record the item as `N/A (confirmed)`, or overturn it and audit the item. Every one of the 19 items appears in the report either way.

## Three axes that must be answered explicitly

Build the three tables in the required report before deciding the gate. Cover every predicate, guard, or policy the PR adds or changes and every test the PR adds or materially changes. Empty prose such as "looks consistent" is not evidence.

### Guard scope

For each changed decision, state the exact inspected span: full trivia, node interior, an explicit `TextSpan`, or another precisely named range. Name the existing counterpart predicate used for the same decision and compare their spans and boundary semantics. If no counterpart exists, say so and justify why the decision is genuinely one-sided. A mismatch is a finding even when every test passes.

When the Behavior Contract carries a guard-delta or predicate-boundary table, audit it rather than trusting it: verify that every region it lists as losing coverage really is covered by the decision that depends on it, and that each boundary it names has a test on both sides. A region the contract dismissed is exactly where the expensive findings live.

### Policy ownership

Use `rg` to enumerate every place that now owns the changed policy, including private copies outside the diff. Record paths and symbols rather than a count alone. More than one owner is a finding unless the PR centralizes them or explicitly justifies why the owners represent different policies and proves their parity.

### Assertion adequacy

For every new or materially changed test, name the invariant and the observation that would falsify it. Confirm the assertion is at least as strong as the behavior the change guarantees. Balanced token counts do not prove exact output; one pass does not prove convergence; LF-only output does not prove line-ending stability; a helper that skips the second pass does not prove idempotency. A weaker assertion is a finding even when the test is green.

Do not collapse these axes into the ordinary checklist. Their value is that the report makes each answer reviewable rather than leaving it implicit in the reviewer's reasoning.

## Prove defect-class closure

For every bug fix or review fix:

1. State the general defect class, not only the reported counterexample.
2. Search for sibling syntax shapes and private copies of the same policy.
3. Trace wrappers, aliases, nested executable scopes, repeated tokens, and target re-resolution when relevant.
4. Verify the regression test reproduces the actual failure shape.
5. Verify the tests use the helper that actually proves the claim — a layout change verified with `VerifyFormatterFix` checks neither the second pass nor CRLF, so it is not idempotency coverage; `VerifyFormatterFixAndIdempotency` is. The same holds for a code fix asserted without re-analysis and for a formatter phase tested on LF only.
6. Verify the relevant matrix:
   - token or trivia changes: comments, directives, and disabled text;
   - formatter changes: LF, CRLF, second-pass idempotency, and neighboring phases;
   - code fixes: one-pass convergence, multiple-diagnostic Fix All, and target identity after earlier edits;
   - analyzer / formatter / fix changes: both directions of counterpart parity;
   - naming fixes: Roslyn Renamer reference retargeting.

A narrow guard for one example is not closure when sibling shapes retain the same hazard.

## Verification

Default to static tracing. Run only targeted tests or formatter double-runs that resolve a concrete suspicion. Before execution, confirm the preinstalled SDK with `scripts/prepare.ps1 -NoInstall`, then use `scripts/test.ps1 -NoInstall -Project <name> -Filter <expression>`. Do not install an SDK or modify `PATH`. Do not run the full solution test suite; the parent workflow owns full validation after this gate.

## Gate decision

- `PASS`: no confirmed `high`, `medium`, or `low` finding remains.
- `PASS — non-blocking cleanup`: every remaining confirmed finding is confined to comments or documentation and **cannot** affect compiled behavior, diagnostics, public API documentation, generated artifacts, or required rule metadata. List them exactly as findings; the parent fixes that text without spending another attempt.
- `BLOCKED — findings`: at least one confirmed finding remains that does not satisfy the cleanup condition.
- `BLOCKED — state mismatch`: the checkout, PR head, or intended committed scope cannot be reviewed reliably.
- Hints do not block the gate.

The cleanup result exists because risk-based admission alone does not solve the problem it was built for: an audit can be genuinely required by a moved predicate and still come back carrying nothing but "a comment no longer describes the code it sits next to". That is worth fixing and is not worth a second audit. It is not a discount for a finding that is merely *small* — one doubt about whether a finding can reach compiled behavior makes it `BLOCKED — findings`.

Report every confirmed finding in the one gate report. The parent has a single repair cycle to work from it, so a finding withheld for a "next round" never gets one.

Both passing results also require a complete three-axis audit. Missing rows, unnamed owners, an unexamined changed test, or an assertion without a concrete falsifier is a blocking finding rather than an incomplete note.

The `Required change` column is a **suggestion, not a specification**. It is written from the reviewer's read of one finding, and implementing it literally is how an over-broad repair creates the next round's finding. The parent owns the repair and must re-derive its scope.

When invoked by `gh-implement` or `gh-apply-review`, the parent owns the budget:

1. It merges all confirmed findings into one consolidated worklist and fixes every in-scope item — including the complete defect class — in a single repair cycle, without invoking preflight in between.
2. It re-runs the contract's guard-delta and predicate-boundary tables **against its own repair**, not only against the original change, and adds a test on each side of any boundary the repair moved.
3. It formats changed paths, runs focused tests, redoes its local self-review and admission artifact, commits with `[skip ci]`, pushes, and updates the PR body when needed.
4. It applies its own scope policy: both parents classify against a frozen scope ledger, ask before expanding the accepted contract, and preserve confirmed out-of-scope work as an unapproved follow-up draft.
5. It spends the **preflight retry** — one fresh, independent, read-only run against the exact new head, with the repair-delta inputs above. `BLOCKED — state mismatch` does not consume an attempt; neither does an agent start that returned no verdict. Reconcile and rerun.
6. On `PASS — non-blocking cleanup` it applies the listed text fixes and proves them non-behavioral with `scripts/verify-text-only.ps1 -NoInstall -StrictDocs -Base <audited-sha> -Head worktree`, then continues to full validation without another attempt. `-StrictDocs` is required here rather than optional: this result excludes public API documentation, and a plain exit code 0 does not establish that, because a documentation edit is exactly what a comment-only change is otherwise allowed to be. A proven text-only cleanup does not invalidate the audit; anything the proof rejects does.
7. It proceeds to full validation only after a passing result, and it stops and reports if the retry blocks. A third official attempt requires explicit user direction.

## Direct chat output

The schema below is **attempt 1's**, and the one a direct `/gh-preflight` invocation uses. The retry has its own, smaller schema further down.

For direct `/gh-preflight` invocations, write only:

```markdown
## Gate
PASS

## Scope
- PR #123 at `<head-sha>`; local checkout matches.
- Audit mode: full — or `repair delta against <previously-audited-sha>`, naming what was reused.
- Reviewed changed files, linked issue requirements, and named counterpart files.

## Checklist
<all 19 gh-review checklist items>

## Three-axis audit

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|----------|----------------|-----------------------|-----------|---------|

### Policy ownership
| Policy | Owners found by `rg` | Justification for multiple owners | Verdict |
|--------|----------------------|-----------------------------------|---------|

### Assertion adequacy
| Test | Invariant | Falsifying observation | Assertion/helper | Verdict |
|------|-----------|------------------------|------------------|---------|

## Findings
_None._

## Verification
- Static tracing only; no targeted execution needed.

## Gate metrics
- Elapsed: `<duration, or "not measured">`; audit mode: `<full | repair delta>`; agent starts: `<n>`.

## Hints
_None._
```

For findings, set the gate to `BLOCKED — findings` — or to `PASS — non-blocking cleanup` when every one of them satisfies the cleanup condition — and use:

```markdown
| # | Severity | Location | Defect class | Scope relation | Required change |
|---|----------|----------|--------------|----------------|-----------------|
| 1 | high | Reihitsu.Formatter/Pipeline/Foo.cs:42 | Cross-scope label relocation | same mechanism/requirement | Model executable scopes and add the nested-scope regression |
```

Keep every confirmed finding in the table exactly once. Provide a concrete counterexample for each high finding. Do not add a preamble or closing text.

When the gate is `PASS — non-blocking cleanup`, add a `Cleanup-safe` column stating, per finding, why it cannot reach compiled behavior, diagnostics, public API documentation, generated artifacts, or rule metadata. A finding without that justification belongs under `BLOCKED — findings`.

In `Scope relation`, state `same mechanism/requirement`, `PR-introduced`, or `new mechanism`. This is evidence for the parent workflow's scope decision; preflight itself neither fixes nor drafts follow-up work. In each three-axis subsection, render an explicit `_N/A — no applicable changed …._` line when there is no row so absence is distinguishable from an omitted audit.

When Hints is non-empty, use `| Location | Defect class | Scope relation | Evidence |`. Keep uncertain observations clearly marked as uncertain; use `new mechanism` for a confirmed concern that is non-blocking only because it is unrelated and pre-existing.

### Retry report schema

The repair-delta retry returns this instead. It is deliberately smaller: what the retry re-derived is the repair, so what it reports is the repair.

```markdown
## Gate
PASS

## Scope
- PR #123 at `<repaired-sha>`, repair delta against `<previously-audited-sha>`; local checkout matches.
- Repair touched: `<files>`; reused unchanged evidence for: `<what>`.

## Previous findings
| # | Previous finding | Status | Evidence |
|---|------------------|--------|----------|
| 1 | Cross-scope label relocation | closed | Guard now models executable scopes; nested-scope regression added and run |
| 2 | Comment describes previous behavior | closed | Summary rewritten at `Foo.cs:31` |

## Delta rows
<only the checklist items and three-axis rows the repair moved, in the attempt-1 table formats;
 `_None._` when the repair moved none>

## New findings
_None._

## Verification
- <targeted runs that settled the repair, or "static tracing only">

## Gate metrics
- Elapsed: `<duration>`; audit mode: repair delta; agent starts: `<n>`.
```

Rules for it:

- **Previous findings** carries every finding from the previous report, with `closed`, `open`, or `superseded` and one clause of evidence. Omitting one is never an option, whatever its status.
- **Delta rows** carries only what the repair moved. Do not re-emit a checklist item or an axis row that the repair did not touch, and do not list reused rows one by one — the `Scope` line already names what was reused.
- **New findings** uses attempt 1's findings table and states, per finding, whether it came from the repair delta or from outside it.
- If the repair expanded into an unrelated production surface, changed the accepted contract, or materially enlarged the file set, incremental mode is invalid: audit in full, use attempt 1's schema, and say scope grew.

## Hard rules

- Never edit tracked files or mutate GitHub or repository history.
- Never mark plain `PASS` with a confirmed finding; `PASS — non-blocking cleanup` is the only passing result that carries findings, and only ones proven unable to reach compiled behavior, diagnostics, public API documentation, generated artifacts, or rule metadata.
- Never mark either passing result without explicit guard-scope, policy-ownership, and assertion-adequacy rows for every applicable changed decision and test.
- Never accept a line-based `grep` as the proof for a comment-only carve-out — the syntax-aware proof is the evidence.
- Never treat a rename as behavior-preserving without a narrower mechanical proof.
- Never turn a retry into a silent full re-review after the repair grew: say scope expanded and audit it as a new scope decision.
- Never review only the diff when a counterpart or pipeline neighbor is relevant.
- Never accept a test-only or paper fix when the defect class remains open.
- Never run the full validation suite from preflight.
- Never hold a confirmed finding back for a later round; the parent has one consolidated repair cycle and one retry.
- Never omit a confirmed unrelated pre-existing concern; record it once under Hints with its mechanism and scope relation.
- Never act as the parent's discovery loop — it arrives with its local self-review done and `origin/main` merged.
- Never adopt the parent's checklist-applicability list without confirming it; an `N/A` the diff can reach is a finding, not a shortcut you inherited.
- Never re-run the parent's focused suites wholesale to re-establish a bundle fact. Verify SHAs, the checkout, and the diff; spot-check a test only when a specific finding turns on it.
- Never audit a tree whose remote base moved after the parent read it — that is a `BLOCKED — state mismatch`, and the parent's `ls-remote` check in the spawning step exists to make it rare.
- Never return attempt 1's full report from a repair-delta retry, and never drop a previous finding from the retry's status table — reporting less is by delta, never by selection.
- Never create or search for follow-up issues.
