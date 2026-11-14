Role: spec author
Commit: 26344a1
Verdict: spec written

Wording-only follow-up to the *Hints* section of `16-auditor.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond that hint.

- `sdd/specs/formatter/line-breaks.md` FMT-BREAK-012, first bullet: "with the first element on the line of the opening parenthesis or bracket" now ends with ", unless a comment, a directive, or disabled text holds it off that line (third bullet)". The specific-over-general reading the third bullet already stated is now explicit where the general rule is written. No requirement's meaning changes; no ID added; no example, change doc, or rule page touched.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.

## Changed requirements
- FMT-BREAK-012 — A list is on one line or one element per line — clarified (cross-reference from the first bullet to the held-first-element bullet; wording only)

## Examples for the tests
_None._ No example changed.

## Output questions
_None._

## Internal choices
- Referred to the third bullet by position ("third bullet") rather than restating its qualifier, since the bullet lies three lines below in the same requirement. Alternative: repeat the full "held" condition in the first bullet.

## Follow-up candidates
_None._
