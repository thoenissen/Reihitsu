# GitHub PR Re-Review

Re-review GitHub Pull Request **#$ARGUMENTS** after the author has addressed a previous `gh-review` pass.

Invoke the `gh-rereview` skill and follow it exactly. You run in a **Linux** cloud sandbox with no `gh` CLI — all GitHub access goes through the GitHub MCP server (`mcp__github__*`):

1. Rebuild the **prior finding set**: read the review comments the reviewer identity (`mcp__github__get_me`) posted on this PR (`mcp__github__pull_request_read` → get_review_comments / get_comments), plus any findings table still in this chat. That set is the baseline.
2. Re-run the full `gh-review` pass on the **current** PR state (metadata, diff, changed files, counterpart files). The diff is not the review boundary — open counterpart files even when they are not in the diff.
3. **Reconcile** each prior finding against the current code: `resolved` (verified fixed, not merely moved or resolved-on-paper), or `open` (not addressed or inadequately addressed). Anything in the current pass with no prior match is a **new** finding.
4. On GitHub: reply to and resolve the thread for each verified-`resolved` finding, reply on still-`open` threads stating what remains, and post `new` findings as fresh inline comments — same posting rules as `gh-review` (high-confidence only, English, concise, no praise, no duplicates).
5. Report back in chat using **only** the re-review block: the same 19-item Checklist, a Prior findings table (with Status), a New findings table, Verification, and Hints. Nothing else.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.
