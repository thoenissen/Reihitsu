# GitHub Rubber Duck (Behavior Contract)

Derive the **Behavior Contract** for **$ARGUMENTS** before any code is written. This is a read-only analysis: it inspects, it reports, it changes nothing.

`$ARGUMENTS` accepts an issue number, an issue URL, a PR number or PR URL when the behavior question comes out of review work, or nothing at all — in which case the target is inferred from the current branch (`codex/issue-<N>-<slug>`) or from the issue linked by the current PR's `Closes #<N>`.

Examples:

- `/gh-rubber-duck 474`
- `/gh-rubber-duck https://github.com/thoenissen/Reihitsu/issues/474`
- `/gh-rubber-duck PR 586`

Invoke the `gh-rubber-duck` skill (`.codex/skills/gh-rubber-duck/SKILL.md`) and follow it exactly:

1. Resolve the target from `$ARGUMENTS` or context. If it stays ambiguous, ask **one** concise question and stop — without mutating anything.
2. Read the issue (and the PR when one is in scope) through authenticated `gh`, then gather repository evidence with `rg` and focused file reads: the analyzer's reporting condition, the formatter phase's anchor, the code fix's rewrite, the existing tests, and the rule doc under `documentation/rules/`.
3. Challenge the issue instead of paraphrasing it: name the behavioral invariant, the source of truth for layout/syntax/semantics/state, the boundary conditions, and every reasonable competing interpretation.
4. Trace the counterparts — analyzer, formatter, code fix, Fix All, `Reihitsu.Core` helpers, rule documentation, tests — and state for each whether it must change, must stay unchanged, or is not involved.
5. For every bug report, state one decidable defect mechanism, locate the switch, visitor, type hierarchy, rewriter list, or other code-defined dispatch set, and enumerate every candidate from that code rather than from the issue. Split the candidate set into its dimensions and execute the ones whose arms reach different code; a dimension may go unexecuted only with the static proof — the exact `rg` and the single shared call site — recorded in the `Dimension coverage` table, and never for LF/CRLF. Exercise one disposable temporary fixture per executed candidate through the narrowest existing entry point; for formatter behavior run the complete batch twice and record convergence. Collapse candidates that fail to reproduce for the *same* reason into one sweep row that names every candidate it covers. Never scale the sweep to how confident the issue sounds, and never return `READY` with a candidate that is neither executed nor covered by a static proof.
6. Walk the adversarial list (single-line/multi-line forms, comments before the relevant token and before both delimiters, `///` and `/* … */` comments, `#if`/`#else`/`#endif`/`#pragma`, disabled text, LF/CRLF, complex siblings, nested syntax, detached vs document-scoped formatting, formatting scopes, repeated fix application, Fix All, second-pass idempotency, syntax validity). Keep what can reach the changed code; mark the rest `N/A` with a reason.
7. Whenever the change moves a guard, predicate, or exemption, produce the **guard-delta** table (span before, span after, region losing cover, the decision depending on it, verdict) and the **predicate-boundary** table (predicate before and after, changed dimension, candidates gaining or losing classification, counterpart predicate, boundary tests). The sweep is a *before* analysis; these are the *after* analysis nothing else performs. Any region losing coverage whose dependent decision is wider than the rewrite span needs an extra guard or a `NEEDS DECISION`, every added or removed condition needs a test on both sides of its boundary, and every material qualifier from the issue — "terminal", "direct", "inside" — maps to an explicit predicate and fixture before `READY`.
8. Write the complete contract to the system temporary directory in the skill's exact artifact schema, including the two delta tables, `Defect-class enumeration`, `Dimension coverage`, and `Defect-class sweep` after the adversarial matrix. Render the delta tables as `_N/A — no guard or predicate changes._` and all three defect-class sections as `_N/A — not a bug report._` when they do not apply. Return only the compact gate result, contract counts, decisions, and absolute artifact path; later gates read that path instead of receiving pasted matrices.
9. **Stop after the contract.** Do not start implementing, do not claim the issue, do not create a branch or commit, and do not post anything to GitHub. When the gate is `NEEDS DECISION`, present the competing interpretations with concrete examples and wait; the user decides.

Read-only means no repository or GitHub mutation: no repository file edits, commits, pushes, PR changes, comments, thread resolution, or full validation suite. Disposable fixtures in a temporary directory outside the repository and focused targeted execution are allowed for the required sweep; remove the temporary directory afterwards. Confirm the preinstalled SDK with `scripts/prepare.ps1 -NoInstall` before any targeted run.

When `gh-implement` runs this skill, it hands over a neutral evidence bundle: the issue and PR **by number** — read them yourself through authenticated `gh` — plus the base and head SHAs, the merge base, the remote `main` SHA, changed files and diff, the build result, its proof that the checkout matches that head, and the user's chat clarifications quoted verbatim. Use it instead of re-deriving the same state; it carries facts only, never the parent's conclusions.

Validating the mechanism against a patched copy of the transform is legitimate evidence for this analysis and it stays inside it. Never hand that copy's output on to the implementer as expected test values: they are a snapshot of one speculative implementation, so a test asserting them can no longer falsify the implementation that produced them. Name the invariant and the helper; derive the expectation from the behavior row.

`gh-implement` runs this same skill automatically in a dedicated read-only `reihitsu-rubber-duck` custom
agent before it edits anything, so invoking `/gh-rubber-duck` first is optional — useful when the issue looks
ambiguous and you want the contract settled before the implementation run starts.
