---
name: gh-review
description: Review a GitHub Pull Request for the Reihitsu repository. Triggers on "review PR", "review pull request", "check PR #", "PR review", or any prompt that supplies a pull request ID or URL and implies a code review. Works in both local Claude Code and the Claude Code Cloud Agent — uses the `gh` CLI exclusively, no MCP dependency. Focus areas: SOLID violations (especially SRP / concern leakage when a class is extended with a job that belongs elsewhere), duplicated logic that could reuse existing helpers, correctness bugs, security, tests, and Reihitsu-specific repo conventions. Posts only high-confidence findings as inline GitHub review comments and reports a single Markdown table (preceded by a checklist) back in chat. No praise, no chit-chat, no LGTM.
---

# Reihitsu GitHub PR Review

You review a GitHub Pull Request and report findings. **Output is strict** — only a checklist and a findings table in chat, plus inline GitHub review comments for confirmed findings. Nothing else.

## Inputs

The PR identifier comes from the invoking prompt or `$ARGUMENTS`:

- `123`
- `#123`
- `https://github.com/<owner>/<repo>/pull/123`

If no PR id can be extracted, stop and ask. Do not guess.

## Tooling

Use the `gh` CLI for everything — it works locally and in the cloud agent and does not require any MCP server:

```bash
gh pr view <N> --json number,title,body,baseRefName,headRefName,author,url
gh pr diff <N>
gh pr view <N> --json files
gh api repos/<owner>/<repo>/pulls/<N>/comments
gh issue view <linked-N>          # parse "Closes #N" / "Fixes #N" / "Resolves #N" from PR body
```

Skip the linked-issue fetch if no `Closes/Fixes/Resolves #N` is present in the PR body.

## Review checklist

Walk every item. For each, mark one of:

- `[x]` checked and applicable
- `[ ]` N/A (with one short reason, e.g. `N/A (no inheritance touched)`)

| # | Item | What to look for |
|---|------|------------------|
| 1 | **Correctness** | Logic errors, off-by-one, edge cases (null / empty / max / negative), resource leaks, concurrency |
| 2 | **SRP / concern leakage** | A class touched by this PR gains a responsibility that belongs to another class. Method named for one job that quietly does two. New code added to a class whose name no longer describes it. **Priority item.** |
| 3 | **Duplication** | New logic that re-implements something already present elsewhere in the diff context or repository. Copy-paste of a block from another file. **Priority item.** |
| 4 | **Other SOLID** | OCP (modifying stable code instead of extending), LSP (subtype breaks parent contract), ISP (interface forces unrelated members), DIP (high-level concrete dependency) |
| 5 | **Coupling / cohesion** | Unnecessary cross-module dependency introduced; related members scattered across files |
| 6 | **Security** | Missing input validation at trust boundaries, injection (SQL/command/path), exposed secrets, missing authorization |
| 7 | **Error handling** | Swallowed exceptions, missing failure modes, unclear error messages, broad `catch` without rethrow |
| 8 | **Tests** | New behavior covered. For analyzer or formatter **bug fixes** the repo requires a regression test **before** the production change (see `CLAUDE.md`). Analyzer tests should be many small focused tests, not one large multi-case test. |
| 9 | **Performance** | Only obvious issues — hot-path allocations in tight loops, O(n²) over user-sized collections, unnecessary repeated IO. Do not nitpick. |
| 10 | **Repo conventions** | Diagnostic ID in correct range (`RH0###` Analyzer, `RH1###` Performance, … `RH8###` Documentation). `helpLinkUri` matches the actual rule doc under `documentation/rules/`. Code fixes delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` / `FormatNode`. Formatter still leaves syntax-invalid and generated code untouched. New analyzer rule ships a comprehensive code fix or no fix at all. |
| 11 | **Naming & docs** | Names align with surrounding code. Public API XML docs added/updated. Rule doc under `documentation/rules/RH####.md` exists and matches the rule if a rule was added or renamed. |
| 12 | **Scope discipline** | No out-of-scope edits. No commented-out code. No `TODO` left without an issue link. |
| 13 | **Issue coverage** | If the PR links an issue (`Closes #N`), every requirement listed in the issue is addressed by the diff. Flag missing requirements explicitly. |

## Severity model

- **high** — bug, security issue, broken contract, missing required regression test, unaddressed issue requirement
- **medium** — design issue (SRP leak, duplication of existing helper), missing error handling, repo-convention violation
- **low** — naming, minor doc gap, dead branch
- **hint** — uncertain observation that needs human judgment; **do not post**, only list in chat

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

- **English only.** Concise. 1–2 sentences.
- State the problem and the fix. No softening (`maybe`, `perhaps`, `could potentially`).
- No praise, no greeting, no signature.
- Before posting, fetch existing comments and **skip any finding already raised**.

## What to write back in chat

**Only** the following block, nothing else. No preamble, no closing summary, no "Done."

````markdown
## Checklist
- [x] Correctness
- [x] SRP / concern leakage
- [ ] Other SOLID — N/A (no inheritance touched)
- [x] Duplication
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
| 1 | high   | Reihitsu.Formatter/Pipeline/Foo.cs:42 | yes | Duplicates BarHelper.Render — reuse instead of re-implementing |
| 2 | medium | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | yes | Method parses input and writes diagnostic; split parsing into helper |
| 3 | hint   | Reihitsu.Cli/Program.cs:120 | no | Possible SRP issue (see Hints) |

## Hints (not posted)
**#3** — `ProcessData` reads JSON and writes a file. Borderline SRP — depends on whether `Process` is established vocabulary in this module. Worth a reviewer judgement call rather than a posted comment.
````

Rules for the chat block:

- If a section would be empty, still render its heading and write `_None._` underneath it.
- The `Posted` column reads `yes` or `no`. `no` only for `hint` rows.
- Keep `Summary` cells short — one sentence. Long prose for `hint` rows goes under **Hints**, never in the table.
- After the block, write **nothing**. No "let me know if…" footer.

## Silence rule

If, after a thorough review, there are **no findings of any severity** (including hints), still post the Checklist block, then `## Findings` with `_None._` underneath. Do not post any GitHub comments. Do not write any other prose.
