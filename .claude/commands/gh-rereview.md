# GitHub PR Re-Review

Re-review GitHub Pull Request **#$ARGUMENTS** after the author has addressed a previous `gh-review` pass.

`$ARGUMENTS` is **optional**. When it is empty, resolve the PR from the current chat: reuse the PR that `gh-review` (or an earlier `gh-rereview`) already ran on in this session. Only ask for a number when no PR can be found in the chat context.

Invoke the `gh-rereview` skill and follow it exactly. You run in a **Linux** cloud sandbox with no `gh` CLI — all GitHub access goes through the GitHub MCP server (`mcp__github__*`):

1. Rebuild the **prior finding set**: read the review comments the reviewer identity (`mcp__github__get_me`) posted on this PR (`mcp__github__pull_request_read` → get_review_comments / get_comments), plus any findings table still in this chat. That set is the baseline.
2. Re-run the full `gh-review` pass on the **current** PR state (metadata, diff, changed files, counterpart files). The diff is not the review boundary — open counterpart files even when they are not in the diff.
3. **Reconcile** each prior finding against the current evidence: `resolved` (verified fixed, not merely moved or resolved-on-paper), `follow-up #<N>`, or `open` (not addressed or inadequately addressed). A follow-up qualifies only when the author linked an existing issue from the thread or PR body, the issue captures the same mechanism and acceptance boundary, the split represents new behavior/a different pre-existing mechanism/a shipped-surface change, and the PR did not introduce the defect. A chat draft or local path is still open. Anything in the current pass with no prior match is a **new** finding.
4. On GitHub: reply to and resolve the thread for each verified-`resolved` finding and each verified durable follow-up handoff, reply on still-`open` threads stating what remains, and submit every new confirmed finding in one GitHub review — inline when anchored, in that review's summary body otherwise.
5. Never search for or create a GitHub issue. You may read only an exact issue URL already linked by the author to verify a follow-up handoff. New systemic, pre-existing, and out-of-scope findings stay in the current review until the author workflow classifies them; do not demote them to hints because of scope.
6. Report back in chat using **only** the re-review block: the same 19-item Checklist, a Prior findings table (with Status), a New findings table, Verification, and Hints. Nothing else.

If `$ARGUMENTS` is empty, take the PR from the chat context (step 1 below). Only if that is empty *and* no PR is identifiable from the chat, stop and ask for the PR number. A non-empty `$ARGUMENTS` that is not a positive integer is an error — ask rather than guess.
