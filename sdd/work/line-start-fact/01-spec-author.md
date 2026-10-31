Role: spec author
Commit: a89ad42
Verdict: spec written

Follow-up of the published run, answering `squad/decisions/2026-10-31-line-start-fact-follow-ups.md`. It is a clarification of existing analyzer behavior, so no change doc.

## Changed requirements

- RH6001-001 — A statement keyword glued to its parenthesis is reported — clarified: the keyword list gains `fixed`, between `catch` and `lock`. The analyzer already checks it (`SyntaxKind.FixedKeyword` in `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer`).
- `documentation/rules/RH6001.md` — unchanged. Its description already lists `fixed`, in the same position, so only the spec was behind.

## Examples for the tests

- RH6001-001 has no new example, because a `fixed` statement needs an `unsafe` context. No existing test reports `fixed(`: `VerifyEveryListedStatementKeywordIsDetectedAndFixed` does not contain it.
  - Because of that, a covering test for `fixed(…)` (reported and fixed) is missing.
  - `VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported`, which was untagged because `fixed` was not in the list, can now tag RH6001-002.

## Open questions for the maintainer

_None._
