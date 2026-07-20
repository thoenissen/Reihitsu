---
name: gh-apply-review
description: Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Codex task after another party reviewed it. Use for requests such as "apply the review", "address the review comments", "work through the PR feedback", or "fix the review findings". Build a worklist from open reviewer findings, user-authored PR hints, a pasted gh-review Copy block, and chat context; implement actionable fixes under AGENTS.md; validate and push the existing PR branch; reply to addressed threads without resolving them. This is the fix step between gh-review and gh-rereview and supports Codex on Linux cloud and local Windows with the authenticated gh CLI and preinstalled .NET SDK.
---

# Reihitsu GitHub PR Apply Review

Implement the feedback on an existing PR, validate it, push it, and reply to each addressed item. Run this in the PR author's task, normally the same task that ran `gh-implement`:

```text
gh-review -> gh-apply-review -> gh-rereview
   find             fix            re-check
reviewer task     author task     reviewer task
```

Own the implementation and branch, but not the verdict. Fix and reply; never resolve a review thread. Verification and resolution belong to `gh-rereview`.

Support Linux cloud and local Windows. Use the repository checkout, authenticated `gh` CLI, local `git`, and preinstalled .NET SDK. Follow `AGENTS.md` throughout.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR created by `gh-implement` in the current task. If several were created, use the most recent and state the chosen PR before making changes.
3. If no PR can be identified, ask the user for it. Never guess.

If the prompt names a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` PR command and use that owner/repository in API paths.

Read the PR before editing:

```shell
gh auth status
gh api user --jq .login
gh pr view <N> --json number,title,body,author,baseRefName,headRefName,headRefOid,url,comments,reviews
gh pr diff <N>
gh pr view <N> --json files,commits
gh api repos/{owner}/{repo}/pulls/<N>/comments
```

Use `gh api graphql` to read review threads with their `id`, `isResolved`, and comments. Paginate when the PR has more than 100 threads or comments. Use `gh pr view` and `gh api repos/{owner}/{repo}/issues/<N>/comments` for general PR comments.

Verify that the checkout is on the PR head branch and current head SHA before editing. If switching is required, run `git worktree list` first and do not take a branch already checked out elsewhere.

## Build the worklist

Call `gh api user --jq .login` first and compare it with the PR author so the identities are explicit. Gather and deduplicate:

1. **Reviewer findings**: unresolved inline threads and review summaries authored by accounts other than the PR author's account. Capture file, line, severity when supplied, requested change, comment database id, and thread id. Skip a resolved thread only after confirming its fix is present at the current head.
2. **User hints**: comments authored by the PR author's account, hints in the current task, and every line in a pasted `gh-review` Copy block. Treat these as first-class work items. Direct user guidance wins when it conflicts with a reviewer finding.
3. **Task context**: findings or hints supplied in chat that were never posted to GitHub.

If there is no review and no hint, stop and report that there is nothing to apply.

## Triage before editing

Classify every item:

- **fix**: actionable and unambiguous; implement it.
- **skip**: incorrect, already handled, or out of scope; record the evidence and reason.
- **needs decision**: ambiguous, contested, architecturally significant, public-API-changing, or dependency-changing. Ask the user directly and pause that item until they decide. Do not silently choose an interpretation.

## Implement

Keep changes limited to accepted review items. Group commits by concern and stage explicit paths only; never use `git add -A` blindly.

Apply the repository workflow from `AGENTS.md`:

- For analyzer or formatter bug fixes, add the reproducing regression test first and confirm it fails before changing production code.
- For formatter behavior, add the requested idempotency, CRLF, and combined-pipeline coverage when applicable.
- For code fixes, add convergence and relevant FixAll coverage. Deliver a comprehensive code fix or omit it.
- Format all changed paths before tests:

  ```shell
  dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
  ```

- Avoid unrelated cleanup. Keep broader concerns in the current PR worklist and report; never move them to a new issue.

## Commit and keep CI quiet

End every fix commit subject with `[skip ci]` so intermediate pushes do not run CI:

```text
Address review: preserve directives in parameter joins (#<PR>) [skip ci]
```

Push to the PR's existing head branch only. Do not open another PR or change draft/ready state. Retry transient push failures with bounded 2s/4s/8s/16s backoff.

After all fixes validate, push any remaining `[skip ci]` commits, create one final trigger commit without the marker, and push again:

```shell
git commit --allow-empty -m "Ready for CI (#<PR>)"
git push
```

Do not create the trigger commit when no change was applied.

## Validate

Before the first build or test, confirm the preinstalled SDK:

```shell
dotnet --list-sdks
```

Do not install an SDK or modify `PATH`. Run the full required validation from the repository root:

```shell
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

All relevant tests must pass. Fix regressions caused by the review changes in focused `[skip ci]` commits. Never silence, ignore, or delete a test to obtain a green run. If the SDK is absent or the base branch has an independent failure, stop and report the evidence.

## Reply without resolving

After the commits are pushed, reply once to every addressed inline comment using the review comment's database id:

```shell
gh api --method POST repos/{owner}/{repo}/pulls/<N>/comments \
  -F in_reply_to=<comment-id> -f body='Addressed: <change> (<sha>).'
```

Use `gh pr comment <N> --body '<message>'` for a non-line hint. Keep replies concise and in English. Do not resolve any thread; its open state is the handshake for `gh-rereview`.

## Chat output

After completion, write only this structure, rendering `_None._` under empty sections:

```markdown
## Applied
| # | Source | Location | Commit | Change |
|---|--------|----------|--------|--------|
| 1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | a1b2c3d | Preserve `#endif`; add a regression test |

## Skipped
| # | Source | Location | Reason |
|---|--------|----------|--------|
| 1 | reviewer | Reihitsu.Cli/Program.cs:120 | Pre-existing and outside this PR's scope |

## Needs decision
_None._

## Validation
- Build: green.
- Analyzer / Formatter / Core / CLI tests: green.

## Pushed
- PR #123, branch `codex/...`: two `[skip ci]` fix commits and trigger commit `Ready for CI (#123)`.
- Replied on threads #1 and #2; left them unresolved for `gh-rereview`.
```

List every item once. Give a reason for every skipped item. Move answered decisions into Applied or Skipped; list only deferred decisions under Needs decision. If validation or push failed, state the exact failure instead of claiming success. Add no preamble or closing text.

## Hard rules

- Never resolve a review thread.
- Never guess on ambiguous or significant feedback.
- Never skip the test-first, idempotency, convergence, formatting, or validation requirements in `AGENTS.md`.
- Never install an SDK or modify `PATH`.
- Never push a non-`[skip ci]` commit before validation is green.
- Never stage unrelated paths, open another PR, or change the PR's draft state.
- Never search for or create a follow-up issue. Every review item remains attached to the current PR.
- Use authenticated `gh` for GitHub operations; do not use raw unauthenticated HTTP calls.
