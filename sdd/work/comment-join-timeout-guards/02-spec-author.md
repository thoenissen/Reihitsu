Role: spec author
Commit: d0dd5d5
Verdict: text written

The prompt named the comment-only change from `00-lead.md` decisions 1–3; that is the lead's recorded decision, not a fix I was asked to confirm, so I followed it. I added no spec text and no change doc: no intended behavior changes.

## Changed requirements

_None._ Comment-only edits:

- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/CommentJoinTests.cs` — the `<summary>` of `SingleLineParameterAttributeFollowedByCommentDoesNotLoopForever` and `SingleLineParameterAttributeFollowedByDirectiveDoesNotLoopForever` now state that the timeout is cooperative, so it stops a reintroduced loop only because the placement loop checks the cancellation token on every pass.
- `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs` — a line comment above `_cancellationToken.ThrowIfCancellationRequested()` in `ApplyPlacement`: checked on every pass, because the non-termination guards in `CommentJoinTests` use a cooperative timeout, which stops a loop that never ends only through this check.

No tracker numbers. `scripts/format.sh` on both files: `Formatted 0 of 2 file(s).`

Proof (`scripts/verify-text-only.sh --base d0dd5d5 --head worktree`):

```
TEXT-ONLY PROOF: PASS — base=d0dd5d5 head=worktree — 2 changed file(s) (0 non-compiled, 2 C# comment/layout-only); xml-doc impact: 1 file(s)
```

`scripts/lane-check.sh docs --head worktree`: `LANE CHECK: confirmed — claimed docs, diff needs docs` (the run's claimed tests lane is wider and also covers it).

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

- Named the guards' class (`CommentJoinTests`) in the production comment so a reader can find the dependents; alternative was a generic "the non-termination guards", which gives no lead.

## Follow-up candidates

_None_ beyond the hang-timeout tooling candidate already in `00-lead.md`.
