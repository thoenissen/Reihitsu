---
name: gh-rereview
description: Re-review a Reihitsu GitHub Pull Request in the reviewer's Codex task after the author addressed a previous gh-review pass. Use for requests such as "re-review PR", "rereview", "review this PR again", "re-check PR", or "the review was addressed". Reconstruct prior findings from the reviewer's GitHub comments and chat table, run the complete gh-review methodology against the current head, classify prior findings as resolved or open, detect new findings, resolve only verified threads, and report the full checklist plus prior/new finding tables. Submit every new confirmed finding in one GitHub review and never search for or create follow-up issues. Supports Codex on Linux cloud and local Windows with the authenticated gh CLI and preinstalled .NET SDK.
---

# Reihitsu GitHub PR Re-Review

Re-review a PR that already received a `gh-review` pass. Confirm which prior findings are resolved, which remain open, and whether the fix introduced new findings.

Read `.codex/skills/gh-review/SKILL.md` completely and apply its 19-item checklist, adversarial corpus, test expectations, severity model, static-first verification discipline, comment rules, and finding-containment workflow unchanged. This skill adds reconciliation and thread handling.

Support Linux cloud and local Windows. Use the authenticated `gh` CLI and preinstalled .NET SDK. Follow `AGENTS.md`.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR most recently handled by `gh-review` or `gh-rereview` in the current task. If several qualify, state which one was selected before acting.
3. If no PR can be identified, ask the user. Never guess.

If the prompt names a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` PR command and use that owner/repository in API paths.

Confirm identity and read the current PR state:

```shell
gh auth status
gh api user --jq .login
gh pr view <N> --json number,title,body,author,baseRefName,headRefName,headRefOid,url,comments,reviews,commits,files
gh pr diff <N>
gh api repos/{owner}/{repo}/pulls/<N>/comments
```

Use `gh api graphql` to read `reviewThreads` with each thread's `id`, `isResolved`, and comments including `databaseId`, author, body, path, line, URL, and creation time. Paginate beyond 100 threads or comments.

## Rebuild the baseline

Use `gh api user --jq .login` to identify the reviewer. Reconstruct prior findings from:

1. Inline review comments and general PR comments authored by that reviewer. Capture severity, file, line, thread id, comment database id, resolved state, and demanded change.
2. The most recent `gh-review` or `gh-rereview` findings table for this PR still present in the task. Merge in its unposted hints; GitHub remains authoritative for posted findings.

If neither source contains a prior finding, stop and tell the user to run `gh-review` first. Do not manufacture a baseline.

## Re-run the complete review

Review the current head exactly as `gh-review` requires:

- Fetch metadata, diff, changed files, comments, and any linked issue.
- Inspect counterpart files even when absent from the diff; the diff is not the review boundary.
- Walk all 19 checklist items and the relevant adversarial corpus.
- Inspect commits since the prior review to focus tracing, but evaluate the complete current PR state.
- Produce a current finding set with the same severities and confidence thresholds as `gh-review`.

## Reconcile prior findings

Match each baseline finding against current code and evidence, not the author's claim or thread state:

- **resolved**: the defect and the defect class named by the finding are genuinely fixed. A moved, renamed, partially guarded, or paper-only fix is not resolved.
- **open**: the finding was not addressed or the attempted fix is incomplete. State the surviving failure precisely.

A deleted surrounding line counts as resolved only when the concern cannot recur in the replacement code. Reopen a prematurely resolved thread when the finding remains.

Anything in the current finding set without a baseline match is **new**. Apply `gh-review`'s severity and confidence rules to it.

## Act on GitHub

Reuse `gh-review`'s English-only, concise, high-confidence, deduplicated posting rules.

- For a verified resolved inline finding, post a one-line confirmation reply, then resolve the GraphQL review thread.
- For an open finding whose author attempted a fix, reply with what still fails. Leave it open; unresolve it if necessary.
- Submit every new confirmed finding in one GitHub review using the same `gh api` endpoint as `gh-review`: use inline comments for valid changed-line anchors and the review summary body for non-line findings.
- Keep systemic, pre-existing, and out-of-scope findings in that review. Never search for or create a follow-up issue, and never use a separate PR comment as the destination for a new finding.

Reply to a review comment by database id:

```shell
gh api --method POST repos/{owner}/{repo}/pulls/<N>/comments \
  -F in_reply_to=<comment-id> -f body='Verified: <concise result>.'
```

Resolve or unresolve by GraphQL thread id:

```shell
gh api graphql -f thread='<thread-id>' -f query='mutation($thread:ID!){resolveReviewThread(input:{threadId:$thread}){thread{id isResolved}}}'
gh api graphql -f thread='<thread-id>' -f query='mutation($thread:ID!){unresolveReviewThread(input:{threadId:$thread}){thread{id isResolved}}}'
```

Do not resolve a thread until the fix is verified against the current head.

## Verification

Default to static tracing because CI already builds and runs the full suite. Execute only when a specific reconciliation question changes the classification, such as convergence or idempotency.

Before execution, confirm the preinstalled SDK:

```shell
dotnet --list-sdks
```

Do not install an SDK or modify `PATH`. Run only targeted filtered tests or the formatter double-run required to settle the question. Record both executed and static verification in the output.

## Chat output

Write only the following structure. Render `_None._` under empty sections and re-post all 19 checklist items from `gh-review` against the current state.

```markdown
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

## Prior findings
| # | Severity | Location | Status | GitHub | Notes |
|---|----------|----------|--------|--------|-------|
| 1 | high | Reihitsu.Formatter/Pipeline/Foo.cs:42 | resolved | thread resolved | `#endif` is preserved; double-run clean |
| 2 | medium | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | open | replied | Parsing remains in the diagnostic method |

## New findings
| # | Severity | Location | Posted | Summary |
|---|----------|----------|--------|---------|
| 1 | high | Reihitsu.Analyzer.CodeFixes/RH3204Fix.cs:57 | yes | New guard prevents fix convergence |

## Verification
- Static tracing for the RH3204 split question; CI covers the full suite.
- Ran the targeted formatter test and confirmed the second pass is clean.

## Hints (not posted)
_None._
```

List every baseline finding once with status `resolved` or `open`. In GitHub, record `thread resolved`, `replied`, or `—`. Every New findings row is confirmed, must be part of the submitted GitHub review, and must read `yes`; never put hints in that table. Keep table cells concise and put uncertain observations only under Hints. When Hints contains entries, append the copy-ready text block required by `gh-review`; omit it when there are no hints. Add no preamble or closing text.

## Hard containment rules

- Never run `gh issue create`, `gh issue list`, or any equivalent issue-search or issue-creation operation during re-review.
- Reading an issue explicitly linked by the PR is allowed only for issue-coverage verification; do not mutate it.
- Never move, copy, redirect, omit, or demote a confirmed finding because it is systemic, pre-existing, or broader than the diff. Keep it on the current PR.
- Never post a new confirmed finding outside the submitted GitHub review; use its inline comments or summary body.
