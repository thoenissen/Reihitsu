---
name: gh-review
description: Review a GitHub Pull Request for the Reihitsu repository. Triggers on "review PR", "review pull request", "check PR #", "PR review", or any prompt that supplies a pull request ID or URL and implies a code review. Works in both local Claude Code and the Claude Code Cloud Agent — uses the `gh` CLI exclusively, no MCP dependency. Focus areas: the Reihitsu invariants (trivia/directive preservation, semantics and compilability of rewrites, fix convergence, formatter idempotency and termination, analyzer/formatter/fix parity, defect-class closure), SOLID violations (especially SRP / concern leakage), duplicated logic that could reuse existing helpers, correctness bugs, security, tests, and repo conventions. Verifies suspicions by execution where a .NET SDK is available. Posts only high-confidence findings as inline GitHub review comments and reports a single Markdown table (preceded by a checklist) back in chat. No praise, no chit-chat, no LGTM.
---

# Reihitsu GitHub PR Review

You review a GitHub Pull Request and report findings. **Output is strict** — only a checklist, a findings table, and a verification block in chat, plus inline GitHub review comments for confirmed findings. Nothing else.

## Inputs

The PR identifier comes from the invoking prompt or `$ARGUMENTS`:

- `123`
- `#123`
- `https://github.com/<owner>/<repo>/pull/123`

If no PR id can be extracted, stop and ask. Do not guess.

## Tooling

Use the `gh` CLI for everything GitHub-related — it works locally and in the cloud agent and does not require any MCP server:

```bash
gh pr view <N> --json number,title,body,baseRefName,headRefName,author,url
gh pr diff <N>
gh pr view <N> --json files
gh api repos/<owner>/<repo>/pulls/<N>/comments
gh issue view <linked-N>          # parse "Closes #N" / "Fixes #N" / "Resolves #N" from PR body
```

Skip the linked-issue fetch if no `Closes/Fixes/Resolves #N` is present in the PR body.

**The diff is not the review boundary.** Reihitsu defects live in interactions: between an analyzer and its formatter twin, between a fix base and its consumers, between one pipeline phase and the next. Whenever a checklist item below names a counterpart file, open and read it even though it is absent from the diff.

## Review checklist

Walk every item. For each, mark one of:

- `[x]` checked and applicable
- `[ ]` N/A (with one short reason, e.g. `N/A (no inheritance touched)`)

Items 2–7 are the **Reihitsu invariants**. They exist because the 1.0-RC reviews traced nearly every Critical to one of them. A confirmed violation of items 2–5 is always **high**.

| # | Item | What to look for |
|---|------|------------------|
| 1 | **Correctness** | Logic errors, off-by-one, edge cases (null / empty / max / negative), resource leaks, concurrency |
| 2 | **Trivia preservation** | Any edit that joins lines, collapses a token gap, or moves/reorders/deletes tokens or members: enumerate every trivia kind that can live in every affected gap — end-of-line comments, block comments, doc comments, `#if`/`#else`/`#elif`/`#endif`, `#pragma`, `#region`/`#endregion`, disabled text. Each must survive at a sensible position **or the edit must be refused**. A guard that covers only comments does not cover directives. Silent deletion or relocation into a string/another construct is a bug, not a style issue. **Priority item.** |
| 3 | **Semantics & compilability** | A rewrite (fix or formatter transform) must compile and preserve runtime behavior for all input shapes, not just the ones in the PR's tests: target-typed expressions (`FormattableString`, overload selection), user-defined operators, string literal contents, language-version gates, `partial` types, explicit interface implementations, file-local types, modifier-order rules of the C# grammar. When flagging, name the compiler error (`CSxxxx`) or the behavior change concretely. |
| 4 | **Fix convergence** | Applying a code fix must silence its own diagnostic in one pass and must not raise other RH diagnostics in the edited span. Check FixAll: overlapping spans, positions stale after the first application. A registered code action that cannot change the document must not be offered. |
| 5 | **Idempotency & termination** | A formatter phase run on its own output must be a no-op — including on CRLF input. Any loop-until-stable needs an obviously decreasing measure; in particular, a guard that *refuses* an edit must not be reported upstream as "changed" (hang risk). Watch for oscillation between neighboring phases (insert ↔ collapse). |
| 6 | **Analyzer/formatter/fix parity** | For a touched analyzer, formatter phase, or fix: name its counterparts and check both directions — formatter output must not be flagged by the analyzer, and analyzer-clean code must be formatter-stable. A new analyzer whose concern no formatter phase owns creates permanent diagnostics for CLI users: flag it. Shared policy must come from one place (usually `Reihitsu.Core`), not a private copy. |
| 7 | **Defect-class closure** | When the PR fixes a bug: state the general defect class, then grep for sibling shapes with the same hazard and for surviving private copies of any helper this PR consolidates. A fix that closes only the reported instance while the class stays open is an incomplete fix — say so. |
| 8 | **SRP / concern leakage** | A class touched by this PR gains a responsibility that belongs to another class. Method named for one job that quietly does two. New code added to a class whose name no longer describes it. **Priority item.** |
| 9 | **Duplication** | New logic that re-implements something already present elsewhere in the diff context or repository. Copy-paste of a block from another file. **Priority item.** |
| 10 | **Other SOLID** | OCP (modifying stable code instead of extending), LSP (subtype breaks parent contract), ISP (interface forces unrelated members), DIP (high-level concrete dependency) |
| 11 | **Coupling / cohesion** | Unnecessary cross-module dependency introduced; related members scattered across files |
| 12 | **Security** | Missing input validation at trust boundaries, injection (SQL/command/path), exposed secrets, missing authorization |
| 13 | **Error handling** | Swallowed exceptions, missing failure modes, unclear error messages, broad `catch` without rethrow |
| 14 | **Tests** | See "Test expectations" below. For analyzer or formatter **bug fixes** the repo requires a regression test **before** the production change (see `CLAUDE.md`). Analyzer tests should be many small focused tests, not one large multi-case test. |
| 15 | **Performance** | Only obvious issues — hot-path allocations in tight loops, O(n²) over user-sized collections, unnecessary repeated IO, per-node `GetText()`/`ToString()` materialization. Do not nitpick. |
| 16 | **Repo conventions** | Diagnostic ID in correct range (`RH0###` Analyzer, `RH1###` Performance, … `RH8###` Documentation). `helpLinkUri` matches the actual rule doc under `documentation/rules/`. Code fixes delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` / `FormatNode` — but check the delegation scope is tight (formatting a whole member/type to fix one token drags unrelated edits and inherited formatter defects into the fix). Formatter still leaves syntax-invalid and generated code untouched. New analyzer rule ships a comprehensive code fix or no fix at all. |
| 17 | **Naming & docs** | Names align with surrounding code. Public API XML docs added/updated. Rule doc under `documentation/rules/RH####.md` exists and matches the rule if a rule was added or renamed. |
| 18 | **Scope discipline** | No out-of-scope edits. No commented-out code. No `TODO` left without an issue link. |
| 19 | **Issue coverage** | If the PR links an issue (`Closes #N`), every requirement listed in the issue is addressed by the diff. Flag missing requirements explicitly. |

## Adversarial input corpus

The recurring blind spot is code shapes the author did not imagine. For every analyzer, formatter, or fix change, pick the relevant subset of these shapes and trace the changed code against them (or run them, see Verification). If a shape is relevant and neither handled nor tested, that is a finding:

- `#if` / `#else` / `#endif`, `#pragma`, `#region` in every gap the edit touches (between modifiers, parameters, members, accessors, attribute lists, base-list entries) — including fully inactive (`#if false`) blocks
- End-of-line comments on the line being joined/moved; comments between modifiers; comments on the header line of a construct being deleted or rewritten
- Raw string literals and **interpolated** raw string literals; verbatim strings; comment-lookalike content inside string literals
- CRLF line endings throughout the file
- Verbatim identifiers (`@class`), letterless/digit-only identifiers (`T1`, `_`, unicode)
- `partial` types and members; explicit interface implementations; file-local (`file`) types; static interface members
- All seven expression-bodied member kinds (method, property, indexer, ctor, dtor, operator, local function) with and without `async`, `Task<T>`/`ValueTask<T>`
- Multiple identical tokens on one line (token re-resolution after edits); nested and else-if chains for brace edits

## Test expectations

What checklist item 14 concretely demands, by change type:

- **Analyzer/formatter bug fix** — the regression test exists and precedes the fix commit; the test input reproduces the reported shape, not a simplified cousin.
- **New or changed formatter behavior** — an idempotency assertion for the changed construct (format twice, second pass is a no-op) and a CRLF variant. If the phase interacts with a neighbor (blank lines ↔ line breaks), a combined-pipeline test.
- **New or changed code fix** — a convergence test (fix output re-analyzed: diagnostic gone, no new RH diagnostics) and, where multiple instances can co-occur in one file, a FixAll test.
- **Any rule touching token gaps or line joins** — at least one test case with a comment and one with a preprocessor directive in the affected gap.
- **Casing/naming fixes** — a Renamer-path test (references actually retargeted), not only a syntax-level rename assertion.

Missing tests from this list are findings (severity per the model below), not hints.

## Verification

Do not settle for reading the diff when the claim is checkable:

- If a .NET SDK is available, run the touched test project(s) with a `--filter` for the affected rules (examples in `CLAUDE.md`), and for formatter changes run `dotnet run --project Reihitsu.Cli -- <path>` **twice** over a file exercising the change — the second run must report no changes.
- A **high**-severity finding should carry a concrete counterexample (a short code snippet plus what goes wrong) in the review comment. Constructing the counterexample is how a suspicion earns "high confidence" — do not discard invariant suspicions merely because they are not obvious from the diff.
- If execution is not possible in the environment, say so in the Verification block and state what was verified by static tracing instead.

## Severity model

- **high** — bug, security issue, broken contract, violation of invariants 2–5 (trivia/directive loss, non-compiling or behavior-changing rewrite, non-convergent fix, non-termination/idempotency break), missing required regression test, unaddressed issue requirement
- **medium** — design issue (SRP leak, duplication of existing helper), parity divergence without confirmed user-visible corruption, missing test from "Test expectations", missing error handling, repo-convention violation
- **low** — naming, minor doc gap, dead branch
- **hint** — uncertain observation that needs human judgment; **do not post**, only list in chat

## Systemic suspicions → follow-up issues

A suspicion whose scope exceeds the PR (policy drift across assemblies, a stale copy elsewhere, a parity question about an untouched counterpart, a defect class with more call sites than the diff) must not die with the review:

1. Search for an existing issue first: `gh issue list --search "<keywords>"`.
2. If none exists, file one: `gh issue create --title "<short>" --body "<origin PR, suspicion, affected files>"`.
3. Reference the issue number in the **Hints** section of the chat block.

Never silently drop a cross-cutting suspicion, and never block the PR on it.

## What to post as a GitHub review comment

Post only **high-confidence findings** (`high`, `medium`, `low`). For each:

- Tied to a specific line → inline review comment via `gh api`:

  ```bash
  gh api -X POST repos/<owner>/<repo>/pulls/<N>/comments \
    -f body="<short message>" \
    -f commit_id="<head sha>" \
    -f path="<file>" \
    -F line=<line> \
    -f side="RIGHT"
  ```

- Not tied to a line (e.g. missing issue requirement) → general PR comment:

  ```bash
  gh pr comment <N> --body "<short message>"
  ```

Comment body rules:

- **English only.** Concise. 1–2 sentences; a `high` finding may add a minimal counterexample snippet.
- State the problem and the fix. No softening (`maybe`, `perhaps`, `could potentially`).
- No praise, no greeting, no signature.
- Before posting, fetch existing comments and **skip any finding already raised**.

## What to write back in chat

**Only** the following block, nothing else. No preamble, no closing summary, no "Done."

````markdown
## Checklist
- [x] Correctness
- [x] Trivia preservation
- [x] Semantics & compilability
- [ ] Fix convergence — N/A (no code fix touched)
- [x] Idempotency & termination
- [x] Analyzer/formatter/fix parity
- [x] Defect-class closure
- [x] SRP / concern leakage
- [x] Duplication
- [ ] Other SOLID — N/A (no inheritance touched)
- [x] Coupling / cohesion
- [ ] Security — N/A (no boundary code touched)
- [x] Error handling
- [x] Tests
- [ ] Performance — N/A
- [x] Repo conventions
- [x] Naming & docs
- [x] Scope discipline
- [x] Issue coverage

## Findings
| # | Severity | Location | Posted | Summary |
|---|----------|----------|--------|---------|
| 1 | high   | Reihitsu.Formatter/Pipeline/Foo.cs:42 | yes | Line join deletes `#endif` between parameters — output does not compile (CS1027) |
| 2 | medium | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | yes | Method parses input and writes diagnostic; split parsing into helper |
| 3 | hint   | Reihitsu.Cli/Program.cs:120 | no | Possible SRP issue (see Hints) |

## Verification
- Ran `Reihitsu.Formatter.Test` with filter `RH5xxx` — pass; double-run over fixture: second pass clean.
- No SDK available: convergence of the RH3204 fix verified by static tracing only.

## Hints (not posted)
**#3** — `ProcessData` reads JSON and writes a file. Borderline SRP — depends on whether `Process` is established vocabulary in this module. Worth a reviewer judgement call rather than a posted comment. Filed #412 for the cross-assembly policy question.
````

Rules for the chat block:

- If a section would be empty, still render its heading and write `_None._` underneath it.
- The `Posted` column reads `yes` or `no`. `no` only for `hint` rows.
- Keep `Summary` cells short — one sentence. Long prose for `hint` rows goes under **Hints**, never in the table.
- **Verification** lists what was executed and what could only be traced statically. One line per item.
- After the block, write **nothing**. No "let me know if…" footer.

## Silence rule

If, after a thorough review, there are **no findings of any severity** (including hints), still post the Checklist block, then `## Findings` with `_None._` underneath (Verification block still applies). Do not post any GitHub comments. Do not write any other prose.
