Role: spec author
Commit: 5827b95
Verdict: spec written

Repair round 1, items from `16-implementer.md` *Disputed tests or scope*. The prompt carried no intended fix or hoped-for verdict beyond those two items.

## Changed requirements
- FMT-UNSUP-009 — header *Uses* row of `sdd/specs/formatter/unsupported-syntax.md` gains FMT-STRUCT-019, which the requirement's `Why:` references — clarified (bookkeeping).
- FMT-UNSUP-009 — `Why:` wording: "the removal of a trailing comma still carries it onto the last element" became "still keeps it behind the last element", because the trivia now leads the closing brace in the tree while the text is unchanged — clarified, no behavior change.
- Change doc `sdd/changes/2026-11/comma-behind-comment-unsupported.md`: the *Checked and kept* entry now names the guard `ContainsNonFormattingTrivia(separator.LeadingTrivia)` in `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia` (the removed `GetTriviaToPreserve` is gone) and states that the comma's leading and trailing trivia now lead the closing brace, because a documentation comment cannot trail a token (FMT-PIPE-021); the text is unchanged. The *What* sentence "keeps carrying the comma's leading trivia onto the last element" became "keeps the comma's leading trivia behind the last element" for the same reason.

## Examples for the tests
_None._ No new or changed example.

## Output questions
_None._

## Internal choices
- Also reworded the two "onto the last element" sentences (FMT-UNSUP-009 `Why:`, change doc *What*) beyond the two named items, so no text states a tree placement the code no longer makes; alternative was leaving them, since they read correctly as text.

## Follow-up candidates
_None._

## Checks
`scripts/test.sh --project architecture --no-install`: Passed 118, Failed 0 (no build lock encountered).
