---
name: gh-rubber-duck
description: Read-only Behavior Contract analysis ("rubber duck") for a Reihitsu GitHub issue or pull request, run before any code is written. Use for `/gh-rubber-duck`, "rubber duck issue 474", "what exactly should this issue change", "pin the expected behavior down first", and as the mandatory pre-implementation gate that `gh-implement` runs in a dedicated read-only subagent. Turns an issue plus every clarification into explicit user-visible examples, a behavior contract table, anchor and trivia rules, an analyzer/formatter/code-fix/test counterpart map, an adversarial boundary matrix, a guard-delta and predicate-boundary analysis for every changed guard, predicate, or exemption, and, for bug reports, a code-derived defect-class enumeration with an executable candidate sweep. Challenges assumptions instead of paraphrasing the issue. Never edits repository files, commits, pushes, opens or updates a PR, posts comments, claims an issue, resolves threads, or runs the full validation suite. Runs in a Linux Claude Code Cloud Agent through the GitHub MCP server.
---

# Reihitsu Behavior Contract (Rubber Duck)

Produce a **Behavior Contract**: an explicit, example-backed statement of what the change must do, before a single test or production line is written.

The expensive failure mode in this repository is not a wrong fix — it is a *correct fix for the wrong contract*. A formatter issue that reads unambiguously at first glance usually hides a decision (which token is the anchor? what happens when the key spans lines? does the analyzer have to agree?), and that decision only surfaces after implementation, preflight, and review have already run. Surfacing it here costs one read-only pass; surfacing it later costs a full implement/preflight/review cycle.

Your job is to **argue with the issue**, not to summarize it. A contract that only restates the issue text has found nothing.

Weight the failure modes that historically surface in review, not only the ones the issue text describes. In this repository the recurring one is **a predicate whose inspected span differs from its decision span**: a guard is narrowed to fix one symptom, and a region it no longer covers still decides something wider. Behavior rows cannot express that — a contract can be fully green while the change is broken — which is why the delta analysis below is part of the contract rather than something preflight discovers later.

## Read-only guarantees

This workflow inspects and reports. It never mutates repository or GitHub state:

- No edits to repository files, including tests and documentation.
- No commits, branches, stashes, or pushes.
- No PR creation or update, no issue claim, no labels.
- No GitHub comments, replies, reviews, or thread resolution.
- No invocation of an implementation skill or command.
- No full solution validation (`dotnet build` of the solution, full test projects).

Allowed: reading issue and PR data, `git log` / `git show` / `git diff` on existing history, reading source, tests, and documentation, and `rg` searches. Running one narrowly filtered existing test to settle a factual question about current behavior is acceptable; running the suite is not.

For the required bug-report sweep, create disposable fixtures only in a temporary directory outside the repository. Exercise them through the narrowest existing public or test entry point, record the results, and remove the temporary directory afterwards. Normal build artifacts from that targeted execution are allowed. Never add a fixture or harness to the repository from this read-only workflow. This Linux sandbox ships no .NET SDK: when the sweep needs execution, run `scripts/prepare.sh`, which probes the toolchain and installs .NET 10 only when it is missing. If it cannot install (no network egress), the sweep is incomplete — report `BLOCKED` rather than a `READY` contract with unexecuted candidates.

Because the analysis mutates no repository or GitHub state, it is safe to run at any time, including while another agent owns the issue.

## Resolve the target

Accept, in this order:

1. An explicit issue number, `#number`, or issue URL from the prompt or `$ARGUMENTS`.
2. An explicit PR number or PR URL (`PR 586`, a `/pull/586` URL) when the behavior question comes from review work.
3. An unambiguous issue inferred from context: the issue linked by `Closes/Fixes/Resolves #N` in the current PR body, or the issue number embedded in the current branch name (`claude/issue-<N>-<slug>`).

If the target is still ambiguous — no number, several candidates, or a branch that does not map to exactly one issue — ask **one** concise question naming the candidates (`AskUserQuestion` is the right tool) and stop. Do not guess, and do not mutate anything while asking.

## Gather evidence

The sandbox has no `gh` CLI. Read GitHub through the MCP server; surface the tools with `ToolSearch` if they are not loaded. Every call below is read-only:

| Purpose | MCP tool |
|---|---|
| Issue body, labels, state, comments | `mcp__github__issue_read` |
| PR metadata and head SHA (only when a PR is in scope) | `mcp__github__pull_request_read` (get) |
| PR diff / changed files (only when a PR is in scope) | `mcp__github__pull_request_read` (get_diff / get_files) |

Batch the independent reads instead of running them one at a time. Then work from the local checkout:

- `rg` for the rule ID, phase name, helper, or message text named by the issue. Prefer one targeted `rg` over opening candidate files speculatively.
- Read only the regions that decide behavior — the analyzer's reporting condition, the formatter phase's anchor computation, the code fix's rewrite, the existing tests for the same construct.
- Read `documentation/rules/RH####.md` when a rule is in scope; it is part of the contract.
- Use `git log -- <path>` or `git show <sha>` when the issue refers to a regression, so the previous behavior is evidence rather than assumption.

**Pin the baseline before reading anything.** Record the exact commit your evidence comes from and report it under `Evidence baseline`. When a PR is in scope, either stand on its head SHA or read files through that commit; never mix a stale checkout with fresh PR metadata, because a contract derived from code nobody is reviewing is worse than no contract. For an issue with no PR, the baseline is current `origin/main`.

Read the conversation too. Clarifications the user typed in chat are part of the contract and frequently contradict the issue body; when they do, the user wins and the contract must say so.

## Build the contract

Determine and state explicitly:

- **The behavioral invariant** — the one sentence that must stay true after the change, phrased so a test can falsify it.
- **The defect mechanism** — for a bug report, one sentence whose membership is decidable for any candidate construct. Describe why the failure occurs, not the examples that happened to expose it.
- **The source of truth** — which token, trivia, delimiter, node, or existing helper decides layout, syntax, semantics, or state. Name it.
- **Boundary conditions** — where the invariant stops applying.
- **Ambiguous interpretations** — every reading of the issue that a competent implementer could pick, with the behavior each one produces.
- **Counterpart parity** — which of analyzer, formatter, and code fix must agree, and in which direction.
- **Fix All and convergence** — whether one application silences the diagnostic and whether multiple diagnostics in one document can be fixed together.
- **Idempotency** — whether a second formatter pass over the output must be a no-op.
- **Trivia and syntax variants** — the comment, directive, and syntax shapes that reach the changed code.
- **Non-goals** — behavior the issue does *not* ask for, so the implementer does not broaden it.

For a formatter or analyzer issue, trace the counterpart behavior across every surface — analyzer, formatter phase, code fix, Fix All, shared utilities in `Reihitsu.Core`, rule documentation, and tests — and say for each whether it must change, must stay unchanged, or is not involved. "Not involved" is a finding too; it tells the implementer where not to spend time.

## Defect-class enumeration and sweep

This section is mandatory for every bug report. A symptom list such as "indexers and events" is not a defect class.

1. State the **mechanism** in one sentence so membership can be decided without judgment, for example: "A brace is normalized on a node that does not contain the token preceding it."
2. Locate the code that dispatches that mechanism: the fixed switch, visitor registration, rewriter list, syntax type hierarchy, or equivalent closed set.
3. Enumerate every arm, registration, or concrete type from that code. Cite the defining path and symbol. Do not derive the candidate list from the issue text.
4. Split the candidate set into its **dimensions** — the trivia slot, the container arm, the line ending,
   whichever the mechanism actually varies over — and decide per dimension whether its arms reach *different
   code*. Execute the dimensions that do. For a dimension whose arms provably share one code path, state the
   static proof instead of running it (see below).
5. Create one minimal disposable fixture per executed candidate and exercise them through the narrowest
   existing entry point. For formatter behavior, run the formatter over the complete fixture batch, then run
   it a second time and compare the first- and second-pass outputs. For analyzer or code-fix behavior, use the
   narrowest existing harness that can analyze or fix the temporary fixture and re-analyze the result.
6. Record whether each candidate reproduces and converges. Decide `In scope` from the mechanism, not from
   severity, convenience, or whether the issue named it.

### Execute what varies, prove what provably shares a path

A sweep that runs every arm of every dimension multiplies fixtures against itself, and the product is mostly
confirmation of something the dispatch code already states. When four `Visit*` overrides all call the same
`SplitFields(node.Members)`, one `rg` establishes that statically, and six container fixtures add nothing that
reading the dispatch did not already prove.

So: **enumerate every dimension, execute only the dimensions whose arms reach different code.**

A dimension may go unexecuted only with a **static proof** in the contract — the exact `rg` invocation and the
shared call site or symbol it lands on. That is the difference between "this dimension is uniform, here is why"
and an analysis nobody performed, and it is the whole reason the proof is mandatory rather than encouraged.
Anything short of a single shared call site — a shared *base class* with an override, a switch that dispatches
to different helpers, a conditional inside the shared method — is not a proof, and the dimension gets executed.

Line endings are never provable this way: LF and CRLF differ in the trivia the code reads, not only in the
text, so they are executed whenever layout is in scope.

If the dispatch set cannot be enumerated or a candidate cannot be exercised from the inspected baseline, the sweep is incomplete. Report `BLOCKED` when evidence is unavailable, or `NEEDS DECISION` when completing the class exposes materially different intended behavior. Never paper over an untested candidate with `N/A`.

For a feature request or other non-bug task, render both required defect-class sections as `_N/A — not a bug report._` so downstream consumers can distinguish an intentional omission from a malformed contract.

## Predicate and span delta

The defect-class sweep is a **before** analysis: it asks which constructs reproduce the bug today. Nothing else in the contract asks the **after** question — once the guard or predicate has moved, is every region the decision depends on still covered? That gap is what produces blocking preflight findings on changes whose behavior rows are all satisfied.

This section is mandatory whenever the change adds, removes, narrows, or widens a guard, predicate, or exemption. It is `gh-preflight`'s Guard-scope axis, moved to the front of the workflow.

### Guard-delta table — for span defects

When the mechanism is "a predicate inspects the wrong span", record one row per guard:

| Guard | Span before | Span after | Region losing cover | Decision depending on it | Verdict |
|---|---|---|---|---|---|
| Formatter collapse | `AccessorList.FullSpan` | `AccessorList.Span` | `}` trailing trivia | Rewrite touches `}` *leading* trivia only | OK |
| RH5408 analyzer | `AccessorList.FullSpan` | `AccessorList.Span` | `}` trailing trivia | Reported span is `signatureStart..Span.End`, so it includes the `}`→`=` gap | **unguarded → needs an extra guard** |

**Gate rule.** Any region losing coverage whose dependent decision is *wider* than the rewrite span is either an additional guard or a `NEEDS DECISION`. It is never an accepted leftover.

### Predicate-boundary table — for every other changed predicate

The same failure occurs on predicates that have nothing to do with spans: an exemption reduced to immediate ownership while the real requirement also demanded a position, a qualifier from the issue that never became an explicit condition. Record one row per changed decision:

| Decision | Predicate before | Predicate after | Changed dimension | Candidates gaining/losing classification | Counterpart predicate | Boundary tests |
|---|---|---|---|---|---|---|
| Switch-break spacing exemption | direct section owner | direct section owner **and final sibling** | sibling position | direct non-terminal break loses exemption | `SwitchCaseBraceRewriter` already uses final-statement semantics | terminal direct stays exempt; non-terminal direct reports |

**Gate rule.** Every added or removed predicate condition needs a test on **both** sides of its boundary. Every material qualifier in the issue — "terminal", "direct", "inside", "trailing" — maps to an explicit predicate and an explicit fixture before `READY`, or it is a `NEEDS DECISION`.

For a span defect the guard-delta table is the specialized form of this analysis; produce both when both apply, and name the counterpart predicate in each case so the implementer inherits the comparison instead of rediscovering it.

## Adversarial considerations

Behavior contracts fail on shapes the author never pictured. Walk this list, keep the shapes that can actually reach the changed code, and mark the rest `N/A` with a one-clause reason. Do not pad the matrix with irrelevant rows:

- single-line and multi-line forms of the construct;
- comments before the relevant token;
- comments before the opening and before the closing delimiter;
- single-line documentation comments (`///`);
- multi-line comments (`/* … */`), including one that ends mid-line;
- directives: `#if`, `#else`, `#elif`, `#endif`, `#pragma`, `#region`;
- disabled text (`#if false` bodies);
- LF and CRLF line endings;
- ordinary and complex sibling elements (a plain identifier next to a lambda, query, or nested initializer);
- nested syntax of the same kind;
- detached-node formatting (`FormatNode`) versus document-scoped formatting (`FormatNodeInDocumentAsync`);
- the enclosing formatting scope: statement, equals-value clause, arrow expression, argument, attribute, initializer;
- repeated code-fix application on already-fixed code;
- Fix All across several diagnostics in one document;
- formatter second-pass idempotency;
- syntax validity and semantic preservation of any rewrite.

For each kept shape, state the expected result, not merely "must be handled".

## Gate

Decide one of:

- **`READY`** — the issue plus repository evidence support exactly one coherent implementation contract.
- **`NEEDS DECISION`** — two or more reasonable interpretations would materially change user-visible behavior. Cosmetic wording choices do not qualify; a different output for the same input does.
- **`BLOCKED`** — the issue, repository, or GitHub context cannot be inspected well enough to write a contract (issue unreadable, referenced code missing, contradictory requirements with no evidence to settle them).

A single unresolved semantic decision is enough for `NEEDS DECISION`. Do not pick a favorite and report `READY`.

For a bug report, `READY` additionally requires a complete code-derived enumeration, a dimension-coverage row per dimension, and one recorded sweep result per candidate — where a candidate whose dimension was not executed is covered by that dimension's static proof, and a group of candidates that fail to reproduce for one shared reason may be covered by one collapsed row naming them all. A green reported example is not enough.

Whenever a guard, predicate, or exemption changes, `READY` additionally requires the matching delta table with a verdict per row, no region left uncovered under the guard-delta gate rule, and a named boundary test on each side of every changed predicate condition. An uncovered region or an unmapped issue qualifier is a `NEEDS DECISION`, not a note for the implementer.

## Required output schema

Return exactly these sections, in this order, rendering `_None._` under any that is empty. Keep it short enough to be read in one pass — no dumped source files, no verbatim reproduction of the issue.

````markdown
## Gate
READY

## Evidence baseline
`thoenissen/Reihitsu` @ `<sha>` — PR #<N> head, or `origin/main` when the target is an issue

## Requirement summary
<one short paragraph: the requested outcome, in behavioral terms>

## User-visible examples
<the smallest inputs and outputs that let a human verify the behavior>

## Behavior contract
| ID | Scenario | Expected behavior | Owner |
|----|----------|-------------------|-------|
| B1 | Single-line key, single-line value | Value stays on the key's line, one space after `:` | Formatter — `Pipeline/HorizontalSpacing` |

## Anchor and trivia rules
<the exact token / trivia / delimiter / node that decides positioning or behavior>

## Counterpart map
| Concern | Analyzer | Formatter | Code fix/Fix All | Tests |
|---------|----------|-----------|------------------|-------|

## Adversarial matrix
| Case | Relevant | Expected result |
|------|----------|-----------------|

## Guard-delta table
| Guard | Span before | Span after | Region losing cover | Decision depending on it | Verdict |
|-------|-------------|------------|---------------------|--------------------------|---------|

## Predicate-boundary table
| Decision | Predicate before | Predicate after | Changed dimension | Candidates gaining/losing classification | Counterpart predicate | Boundary tests |
|----------|------------------|-----------------|-------------------|------------------------------------------|-----------------------|----------------|

## Defect-class enumeration
- Mechanism: …
- Dispatch source: `<path>` — `<symbol>`
- Candidates: …

## Dimension coverage
| Dimension | Arms | Executed | Static proof if not executed |
|-----------|------|----------|------------------------------|

## Defect-class sweep
| Candidate | Fixture | Reproduces | Converges | In scope | Reason if excluded |
|-----------|---------|------------|-----------|----------|--------------------|

## Non-goals
- <behavior intentionally outside this issue>

## Decisions needed
_None._

## Implementation handoff
- Production paths: …
- Test paths: …
- Red tests to add first: …
- Focused validation commands: …
- Risks to re-check: …
````

Section rules:

- **User-visible examples** show input *and* expected output whenever formatting changes are involved. For key/value or paired-element formatting, cover all four combinations explicitly: single-line key + single-line value, multi-line key + single-line value, single-line key + multi-line value, multi-line key + multi-line value. If a combination cannot occur, say why instead of omitting it.
- **Evidence baseline** names the commit every statement below was derived from, so a later reader can check whether the contract still applies.
- **Behavior contract** `Owner` names the responsible surface: the analyzer, the formatter phase, the code fix, a shared helper, or the rule doc. One row per verifiable scenario; the IDs are what the implementer's regression matrix and the local self-review reference later.
- **Adversarial matrix** carries one row per shape kept from the list above, plus `N/A` rows with their reason for the shapes that were considered and dismissed.
- **Guard-delta table** and **Predicate-boundary table** carry one row per changed guard and per changed decision. When the change moves no guard, predicate, or exemption, render the explicit text `_N/A — no guard or predicate changes._` under each; an empty table is indistinguishable from an analysis nobody performed. The `Verdict` column states `OK` or names the additional guard the change needs; `Boundary tests` names the two fixtures that will sit on either side of the moved condition.
- **Defect-class enumeration** names the decidable mechanism, the code location that defines the candidate set, and every candidate found there. For non-bug work, use the explicit `_N/A` text above.
- **Dimension coverage** carries one row per dimension the candidate set varies over. `Executed` is `yes` or `no`; a `no` row must name the exact `rg` invocation and the shared call site that proves the arms reach one code path, and `_None._` in that column with `Executed = no` is a malformed contract, not a shortcut. For non-bug work, use the same explicit `_N/A` text as the sections below.
- **Defect-class sweep** contains one row per enumerated candidate. `Fixture` identifies the disposable input, `Reproduces` records the baseline failure, `Converges` records re-analysis or the formatter's second pass, and every `In scope = no` row explains why the candidate does not satisfy the mechanism. For non-bug work, use the explicit `_N/A` text above.
- **Collapse the negative rows.** When several candidates do not reproduce *for the same reason* — read by the same other call site, guarded by the same predicate — write **one** row per reason rather than one per candidate, and name every candidate it covers in the `Candidate` cell. The closure argument is carried by the reason, not by the repetition, and twenty-five rows of "does not reproduce — read by some other call site" say exactly what five do. A collapsed row that does not enumerate its candidates is not a collapse; it is a gap.
- **Decisions needed** is `_None._` for `READY`. Otherwise, per decision: the competing interpretations, a concrete example of each (input and the differing output), and a recommended choice with the reason it fits the repository's existing behavior.
- **Never hand over expected outputs produced by a candidate fix.** Validating the mechanism by patching a copy of the transform and running fixtures through it is legitimate evidence for *this* analysis, and it stays inside it. What must not cross into the handoff is the patched copy's output as the implementer's expected test values: those are a snapshot of one speculative implementation, so a test asserting them can no longer falsify the implementation that produced them, and it silently becomes a test of the code instead of the contract. Name the invariant and the helper; let the implementer derive the expectation from the behavior row.
- **Implementation handoff** lists the red tests that should exist before production code — including one on each side of every boundary named in the delta tables — matching the repository's test-first rule for analyzer and formatter bug fixes, and names focused `--filter` commands rather than the full suite. Name the existing test helper each red test should use — `VerifyFormatterFixAndIdempotency` for layout changes (second pass plus LF/CRLF), `VerifyFormatterFix` for plain analyzer/formatter parity, `VerifyFormatterStability` for code that must stay untouched, `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Verify` for a code fix and its convergence, `AssertRuleResult(input, expected, endOfLine)` for formatter phases. Pointing at the wrong helper is how an invariant ends up looking covered while it is not.

## When `gh-implement` invokes this skill

`gh-implement` reads this file in the parent agent and then runs the analysis in one fresh, read-only subagent of type `reihitsu-rubber-duck` — defined in `.claude/agents/`, which owns its model tier and effort level — with no access to the parent's proposed solution. Three consequences:

- Report the contract you can defend from the issue and the repository, not the one the parent seems to want. Independence is the whole point of the isolation.
- Return the schema above verbatim. The parent turns the `Behavior contract`, `Adversarial matrix`, `Guard-delta table`, `Predicate-boundary table`, and `Defect-class sweep` rows directly into its regression matrix and local self-review checklist, so unstable headings break the hand-off.
- The parent hands over an **evidence bundle**: the issue JSON and linked clarifications, PR metadata and body when a PR exists, base and head SHAs, the merge base, the changed-file list and diff, and its proof that the local checkout matches that head. That bundle is neutral fact-gathering, not author reasoning — use it instead of re-deriving the same state from GitHub, and treat a missing or self-contradicting bundle as a reason to gather the evidence yourself rather than to proceed on assumption. It never contains conclusions, suspected findings, or intended fixes; if it does, ignore them and say so in the report.

When the parent later sends new evidence (a user clarification, a fact discovered mid-implementation), amend the same contract: restate the gate, mark which rows changed, and keep the unchanged rows stable so the parent can diff them.

## Execution economy

The analysis is cheap only if it stays cheap:

- Use `rg` to locate behavior; do not read whole projects to find one phase.
- Batch the independent GitHub reads in one step.
- Batch all disposable sweep fixtures into one targeted invocation per pass instead of launching one process per candidate.
- Read a large file once and work from that content; do not re-open it per question.
- Quote the few lines that carry the evidence, not the file.
- Skip narration of unchanged state.

## Hard rules

- Never edit, commit, push, or open/update a PR, and never post to GitHub in any form.
- Never claim an issue or resolve a review thread — ownership belongs to `gh-implement`, resolution to `gh-rereview`.
- Never start implementation, and never call an implementation skill or command.
- Never run the full validation suite; focused, filtered runs only, and only to settle a factual question or to execute the mandatory bug-report sweep. Prepare the toolchain with `scripts/prepare.sh` only when the sweep needs it; if it cannot install, report `BLOCKED` instead of an unproven contract.
- Never report `READY` while a material interpretation question is open.
- Never report `READY` for a bug report without a decidable mechanism, a code-derived complete candidate enumeration, and a completed sweep row for every candidate.
- Never leave a dimension unexecuted without the static proof — the exact `rg` and the shared call site — that its arms reach one code path. A shared base class, a switch to different helpers, or a conditional inside the shared method is not that proof.
- Never scale the sweep to how confidently the issue names the mechanism. The enumeration comes from the dispatch code, and an issue that sounds precise is still a hypothesis; the saving comes from not executing dimensions that provably share a path, never from trusting the report.
- Never collapse sweep rows that fail to reproduce for *different* reasons, and never collapse one without naming every candidate it covers.
- Never report `READY` for a change that moves a guard, predicate, or exemption without the matching delta table, a verdict per row, and a named boundary test on each side of every changed condition.
- Never leave a region that loses guard coverage unresolved because every behavior row is green — behavior rows cannot express span algebra, which is exactly why the delta tables exist.
- Never omit the schema sections; render `_None._` instead.
- Never pad the adversarial matrix with shapes that cannot reach the changed code — mark them `N/A`.
- Never reach for the `gh` CLI or a raw GitHub API call — use the GitHub MCP server.
- English only, per `CLAUDE.md`.
