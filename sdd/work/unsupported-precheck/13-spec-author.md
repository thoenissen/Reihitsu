Role: spec author
Commit: 941a5ecc
Verdict: text written

Repair round 1, finding 1 of `12-spec-checker.md` (Decision 16). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the finding itself.

## Changed requirements
_None._ No requirement changes; RH7101-004, RH7101-006, and RH7101-007 stay as accepted.

Text changes:
- `documentation/rules/RH7101.md`, *How to fix it*: the sentence promising that the code fix carries the comments written on the declaration and on the individual variables over to the new fields is replaced by "The code fix performs the split for you." RH7101-007 withholds the fix for a comment inside the declaration, so the old sentence promised a carry-over the fix no longer offers. The page lists no withheld case (`create-rule-doc`); `general-notes.md` already covers a fix withheld for a comment. The Correction example is unchanged, so `RuleDocumentationExampleTests` is unaffected.
- `sdd/changes/2026-11/unsupported-precheck.md`, visible change 7 "Rule pages": now names the RH7101 page change with its before and after text instead of "None".

Other rule pages checked: every rule whose fix the change doc lists as withheld under SHARED-FIX-010 (RH2001, RH3204, RH5041, RH5111, RH5113, RH5303, RH5304, RH5305, RH5517, RH5520, RH7004, RH7105, RH7106). None promises comment handling or an unconditional fix beyond the normal case ("the code fix applies this automatically"), which `general-notes.md` qualifies for comments; RH2001's page already says the fix is offered only for shapes that convert safely. No other page changed.

## Examples for the tests
_None._

## Output questions
_None._ The rule-page wording follows the answered outputs (Q5B, Q7A); it adds no new visible behavior.

## Internal choices
- Dropped the comment sentence rather than narrowing it to "keeps the comments written in front of the declaration": that narrower promise still has exceptions (RH7101-006 withholds the fix for a documentation comment in the leading trivia), and the page should list none. Alternative: the narrowed sentence the checker suggested.

## Follow-up candidates
_None._
