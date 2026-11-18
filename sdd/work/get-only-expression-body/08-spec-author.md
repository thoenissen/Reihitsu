Role: spec author
Commit: 706343d
Verdict: spec written

The prompt asked me to carry out lead decision 7, a wording clarification. It named no intended fix, suspected cause, or hoped-for verdict beyond that recorded decision. I worked from `00-lead.md` (decisions 7 and 9), `04-spec-author.md`, `05-test-implementer.md`, the spec, and `GetOnlyMemberExpressionBodyRewriteUtilities` as built.

## Changed requirements

- SHARED-GETONLY-003 — clarified, with no change in behavior. The list item "in front of the getter's semicolon" now names its syntax-tree owner: the leading trivia of the getter's semicolon, which is a comment on a line of its own in front of `;`. A single-line comment in the trailing trivia of the getter expression's last token is now named as withholding the fix, because it would comment out the new semicolon.
  - A new sentence says that a *delimited* comment in that trailing trivia, on the expression's own line in front of `;`, keeps neither the accessor list nor the fix. It stays behind the expression in front of the new semicolon. The same holds for the returned expression of a getter block (FMT-STRUCT-012).
  - *Why* gains one sentence.
  - Two new examples, one on each side of the condition (FC-002):
    - `{ get => _value /* c */; }` and `{ get { return _value /* d */; } }` are reported, and the fix gives `=> _value /* c */;` and `=> _value /* d */;`. This mirrors `DelimitedCommentTrailingTheExpressionStaysBehindIt`.
    - `get => _value // c` with `;` on the next line, and `get => _value` followed by `/* d */ ;` on the next line, are reported with no fix.
- SHARED-GETONLY-005 — clarified: the getter's expression keeps a delimited comment that trailed it on its line (SHARED-GETONLY-003).

Change doc `sdd/changes/2026-11/get-only-expression-body-rules.md`: unchanged. It never states that a comment between the expression and `;` withholds the fix. Its visible-change examples only show comments behind the semicolon or the brace, which this clarification leaves as they were.

Rule pages: unchanged. This is a corner case of a withheld fix.

## Decision 9 (AGENTS.md)

The `StructuralTransformPhase` row now names `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective`. It reads correctly:
- The method is public in `Reihitsu.Core`.
- The accessor-level and get-only conversions both use it, and the formatter's structural transforms run those conversions.
- `AccessorExpressionBodyTransform` no longer has a method of that name.

No edit needed.

## Examples for the tests

The new SHARED-GETONLY-003 examples:
- The positive example is already covered by `DelimitedCommentTrailingTheExpressionStaysBehindIt`. Its `[Requirement]` is SHARED-GETONLY-005, and it could also carry SHARED-GETONLY-003.
- The negative example is covered by `CommentInFrontOfTheGettersSemicolonWithholdsFix` and `SingleLineCommentInFrontOfTheReturnSemicolonWithholdsFix`. A delimited comment on its own line in front of an expression-bodied getter's `;` (`B` of the negative example) may not have a test of its own yet. The test implementer can add one if the lead wants it.

## Output questions

_None._ The behavior is what the formatter does today, and decision 7 settles it.

## Internal choices

- I left FMT-STRUCT-015's "before the getter's semicolon" unchanged. Its own qualifier, "trivia the conversion would delete or join onto the signature's line", already excludes the delimited comment on the expression's line. FMT-STRUCT-012 already states the single-line case. The formatter spec is outside this round's scope. The alternative was to mirror the clarification there.

## Follow-up candidates

_None._

## Checks

- `scripts/format.sh` on the four shapes: both positive members convert to `=> _value /* c */;` and `=> _value /* d */;`, and both negative members keep their accessor list.
- `scripts/apply-fix.sh RH3209` with each example in its own fixture: the positive example is fixed to the spec's output under LF and CRLF, and both negative members give no-fix-offered.
- `scripts/test.sh --project architecture`: 198 passed, 0 failed.
