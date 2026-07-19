# GitHub PR Apply Review

Work through the review on GitHub Pull Request **#$ARGUMENTS**: implement the reviewer's findings and any extra hints the user posted, then push.

`$ARGUMENTS` is **optional**. When it is empty, resolve the PR from the current chat — the one `gh-review` / `gh-rereview` already ran on in this session. Only ask for a number when no PR can be found in the chat context.

Invoke the `gh-apply-review` skill and follow it exactly. You run in a **Linux** cloud sandbox with no `gh` CLI — all GitHub access goes through the GitHub MCP server (`mcp__github__*`), while local `git` handles branch/commit/push:

1. Collect the **worklist**: the reviewer's open inline + general findings on the PR (`mcp__github__pull_request_read` → get_review_comments / get_comments via the `get_me` identity), the extra hints the user posted as PR comments or passed in this chat (including a `gh-review` **Copy block**), and any `hint` rows still in the chat. Reconcile against already-resolved threads so you do not redo settled work.
2. For each actionable item, implement the fix on the PR branch following the repository workflow in `CLAUDE.md` (regression/repro test first for analyzer/formatter bug fixes; format the changed files with `Reihitsu.Cli` before tests; code fix only if comprehensive).
3. Ambiguous, contested, or architecturally significant findings → do **not** guess. Use `AskUserQuestion` to confirm the intended resolution before editing.
4. Install the .NET 10 SDK via `dotnet-install.sh`, then build and run the four test projects. Keep CI quiet: every commit except the final trigger commit ends its subject with `[skip ci]`.
5. Reply once per addressed thread stating what changed (`Addressed: … (<sha>)`). Do **not** resolve threads — verification and resolution belong to `gh-rereview`.
6. Report back in chat with the compact worklist block defined in the skill: Applied / Skipped (with reason) / Needs decision, plus the validation result and the pushed commits. Nothing else.

A non-empty `$ARGUMENTS` that is not a positive integer is an error — ask rather than guess.
