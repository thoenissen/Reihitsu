# GitHub Rubber Duck (Behavior Contract)

Derive the **Behavior Contract** for **$ARGUMENTS** before any code is written. This is a read-only analysis: it inspects, it reports, it changes nothing.

`$ARGUMENTS` accepts an issue number, an issue URL, a PR number or PR URL when the behavior question comes out of review work, or nothing at all — in which case the target is inferred from the current branch (`claude/issue-<N>-<slug>`) or from the issue linked by the current PR's `Closes #<N>`.

Examples:

- `/gh-rubber-duck 474`
- `/gh-rubber-duck https://github.com/thoenissen/Reihitsu/issues/474`
- `/gh-rubber-duck PR 586`

Invoke the `gh-rubber-duck` skill (`.claude/skills/gh-rubber-duck/SKILL.md`) and follow it exactly. You run in a **Linux** cloud sandbox with no `gh` CLI — all GitHub access goes through the GitHub MCP server (`mcp__github__*`):

1. Resolve the target from `$ARGUMENTS` or context. If it stays ambiguous, ask **one** concise question (`AskUserQuestion`) and stop — without mutating anything.
2. Read the issue with `mcp__github__issue_read` (and the PR with `mcp__github__pull_request_read` when one is in scope), then gather repository evidence with `rg` and focused file reads: the analyzer's reporting condition, the formatter phase's anchor, the code fix's rewrite, the existing tests, and the rule doc under `documentation/rules/`.
3. Challenge the issue instead of paraphrasing it: name the behavioral invariant, the source of truth for layout/syntax/semantics/state, the boundary conditions, and every reasonable competing interpretation.
4. Trace the counterparts — analyzer, formatter, code fix, Fix All, `Reihitsu.Core` helpers, rule documentation, tests — and state for each whether it must change, must stay unchanged, or is not involved.
5. For every bug report, state one decidable defect mechanism, locate the switch, visitor, type hierarchy, rewriter list, or other code-defined dispatch set, and enumerate every candidate from that code rather than from the issue. Exercise one disposable temporary fixture per candidate through the narrowest existing entry point; for formatter behavior run the complete batch twice and record convergence. Never return `READY` with a missing or unexecuted candidate.
6. Walk the adversarial list (single-line/multi-line forms, comments before the relevant token and before both delimiters, `///` and `/* … */` comments, `#if`/`#else`/`#endif`/`#pragma`, disabled text, LF/CRLF, complex siblings, nested syntax, detached vs document-scoped formatting, formatting scopes, repeated fix application, Fix All, second-pass idempotency, syntax validity). Keep what can reach the changed code; mark the rest `N/A` with a reason.
7. Return the complete contract in the skill's exact schema, including `Defect-class enumeration` and `Defect-class sweep` after the adversarial matrix. Render both as `_N/A — not a bug report._` only for non-bug work.
8. **Stop after the contract.** Do not start implementing, do not claim the issue, do not create a branch or commit, and do not post anything through GitHub MCP. When the gate is `NEEDS DECISION`, present the competing interpretations with concrete examples and wait; the user decides.

Read-only means no repository or GitHub mutation: no repository file edits, commits, pushes, PR changes, comments, thread resolution, or full validation suite. Disposable fixtures in a temporary directory outside the repository and focused targeted execution are allowed for the required sweep; remove the temporary directory afterwards. The Linux sandbox ships no .NET SDK, so install .NET 10 through the `dotnet-install.sh` procedure in `.claude/skills/gh-review/SKILL.md` when the sweep needs execution and `dotnet --list-sdks` shows no `10.*`.

`gh-implement` runs this same skill automatically in a dedicated read-only subagent before it edits anything, so invoking `/gh-rubber-duck` first is optional — useful when the issue looks ambiguous and you want the contract settled before the implementation run starts.
