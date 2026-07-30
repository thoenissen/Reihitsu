---
name: gh-rubber-duck
description: Read-only Behavior Contract analysis ("rubber duck") for a Reihitsu GitHub issue or pull request, run before any code is written. Use for `/gh-rubber-duck`, `$gh-rubber-duck`, "rubber duck issue 474", "what exactly should this issue change", "pin the expected behavior down first", and as the mandatory pre-implementation gate that `gh-implement` runs in a dedicated read-only subagent. Turns an issue plus every clarification into explicit user-visible examples, a behavior contract table, anchor and trivia rules, an analyzer/formatter/code-fix/test counterpart map, an adversarial boundary matrix, non-goals, and the decisions that must be settled before implementation starts. Challenges assumptions instead of paraphrasing the issue. Never edits files, commits, pushes, opens or updates a PR, posts comments, claims an issue, resolves threads, or runs the full validation suite.
---

# Reihitsu Behavior Contract (Rubber Duck)

Produce a **Behavior Contract**: an explicit, example-backed statement of what the change must do, before a single test or production line is written.

The expensive failure mode in this repository is not a wrong fix — it is a *correct fix for the wrong contract*. A formatter issue that reads unambiguously at first glance usually hides a decision (which token is the anchor? what happens when the key spans lines? does the analyzer have to agree?), and that decision only surfaces after implementation, preflight, and review have already run. Surfacing it here costs one read-only pass; surfacing it later costs a full implement/preflight/review cycle.

Your job is to **argue with the issue**, not to summarize it. A contract that only restates the issue text has found nothing.

## Read-only guarantees

This workflow inspects and reports. It never mutates anything:

- No edits to repository files, including tests and documentation.
- No commits, branches, stashes, or pushes.
- No PR creation or update, no issue claim, no labels.
- No GitHub comments, replies, reviews, or thread resolution.
- No invocation of an implementation skill or command.
- No full solution validation (`dotnet build` of the solution, full test projects).

Allowed: reading issue and PR data, `git log` / `git show` / `git diff` on existing history, reading source, tests, and documentation, and `rg` searches. Running one narrowly filtered existing test to settle a factual question about current behavior is acceptable; running the suite is not.

Because the analysis is read-only, it is safe to run at any time, including while another agent owns the issue.

## Resolve the target

Accept, in this order:

1. An explicit issue number, `#number`, or issue URL from the prompt or `$ARGUMENTS`.
2. An explicit PR number or PR URL (`PR 586`, a `/pull/586` URL) when the behavior question comes from review work.
3. An unambiguous issue inferred from context: the issue linked by `Closes/Fixes/Resolves #N` in the current PR body, or the issue number embedded in the current branch name (`codex/issue-<N>-<slug>`).

If the target is still ambiguous — no number, several candidates, or a branch that does not map to exactly one issue — ask **one** concise question naming the candidates and stop. Do not guess, and do not mutate anything while asking.

For a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` command.

## Gather evidence

Batch the independent read-only queries instead of running them one at a time:

```shell
gh issue view <N> --json number,title,body,labels,state,url,comments
gh pr view <PR> --json number,title,body,baseRefName,headRefName,headRefOid,url,files   # only when a PR is in scope
gh pr diff <PR>                                                                          # only when a PR is in scope
```

Then work from the local checkout:

- `rg` for the rule ID, phase name, helper, or message text named by the issue. Prefer one targeted `rg` over opening candidate files speculatively.
- Read only the regions that decide behavior — the analyzer's reporting condition, the formatter phase's anchor computation, the code fix's rewrite, the existing tests for the same construct.
- Read `documentation/rules/RH####.md` when a rule is in scope; it is part of the contract.
- Use `git log -- <path>` or `git show <sha>` when the issue refers to a regression, so the previous behavior is evidence rather than assumption.

Read the conversation too. Clarifications the user typed in chat are part of the contract and frequently contradict the issue body; when they do, the user wins and the contract must say so.

## Build the contract

Determine and state explicitly:

- **The behavioral invariant** — the one sentence that must stay true after the change, phrased so a test can falsify it.
- **The source of truth** — which token, trivia, delimiter, node, or existing helper decides layout, syntax, semantics, or state. Name it.
- **Boundary conditions** — where the invariant stops applying.
- **Ambiguous interpretations** — every reading of the issue that a competent implementer could pick, with the behavior each one produces.
- **Counterpart parity** — which of analyzer, formatter, and code fix must agree, and in which direction.
- **Fix All and convergence** — whether one application silences the diagnostic and whether multiple diagnostics in one document can be fixed together.
- **Idempotency** — whether a second formatter pass over the output must be a no-op.
- **Trivia and syntax variants** — the comment, directive, and syntax shapes that reach the changed code.
- **Non-goals** — behavior the issue does *not* ask for, so the implementer does not broaden it.

For a formatter or analyzer issue, trace the counterpart behavior across every surface — analyzer, formatter phase, code fix, Fix All, shared utilities in `Reihitsu.Core`, rule documentation, and tests — and say for each whether it must change, must stay unchanged, or is not involved. "Not involved" is a finding too; it tells the implementer where not to spend time.

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

## Required output schema

Return exactly these sections, in this order, rendering `_None._` under any that is empty. Keep it short enough to be read in one pass — no dumped source files, no verbatim reproduction of the issue.

````markdown
## Gate
READY

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
- **Behavior contract** `Owner` names the responsible surface: the analyzer, the formatter phase, the code fix, a shared helper, or the rule doc. One row per verifiable scenario; the IDs are what the implementer's regression matrix and the local self-review reference later.
- **Adversarial matrix** carries one row per shape kept from the list above, plus `N/A` rows with their reason for the shapes that were considered and dismissed.
- **Decisions needed** is `_None._` for `READY`. Otherwise, per decision: the competing interpretations, a concrete example of each (input and the differing output), and a recommended choice with the reason it fits the repository's existing behavior.
- **Implementation handoff** lists the red tests that should exist before production code, matching the repository's test-first rule for analyzer and formatter bug fixes, and names focused `--filter` commands rather than the full suite.

## When `gh-implement` invokes this skill

`gh-implement` reads this file in the parent agent and then runs the analysis in one fresh, read-only subagent that has no access to the parent's proposed solution. Two consequences:

- Report the contract you can defend from the issue and the repository, not the one the parent seems to want. Independence is the whole point of the isolation.
- Return the schema above verbatim. The parent turns the `Behavior contract` and `Adversarial matrix` rows directly into its regression matrix and its local self-review checklist, so unstable headings break the hand-off.

When the parent later sends new evidence (a user clarification, a fact discovered mid-implementation), amend the same contract: restate the gate, mark which rows changed, and keep the unchanged rows stable so the parent can diff them.

## Execution economy

The analysis is cheap only if it stays cheap:

- Use `rg` to locate behavior; do not read whole projects to find one phase.
- Batch the independent GitHub reads in one step.
- Read a large file once and work from that content; do not re-open it per question.
- Quote the few lines that carry the evidence, not the file.
- Skip narration of unchanged state.

## Hard rules

- Never edit, commit, push, or open/update a PR, and never post to GitHub in any form.
- Never claim an issue or resolve a review thread — ownership belongs to `gh-implement`, resolution to `gh-rereview`.
- Never start implementation, and never call an implementation skill or command.
- Never run the full validation suite; focused, filtered runs only, and only to settle a factual question.
- Never report `READY` while a material interpretation question is open.
- Never omit the schema sections; render `_None._` instead.
- Never pad the adversarial matrix with shapes that cannot reach the changed code — mark them `N/A`.
- English only, per `AGENTS.md`.
