Role: spec author
Commit: 2463a98
Verdict: spec written

Amendment for the maintainer's answer 6 (A): a body below its header keeps every line in its column, whatever its indentation.

## thoenissen/Reihitsu#1416

### Changed requirements

- SHARED-BRACE-002 — The fix wraps the body in braces — clarified: the "below its `if` or `else`" bullet no longer limits "keeps every line as written" to a body one level (four columns) right of the braces; a misindented body, and every continuation line of it, keeps its column. New example: `if (a)` with `M(a, b);` in column 10 → braces in column 8, body stays in column 10; the multi-line sentence covers `M(a,` at 10 with `b);` at 14.
- Nothing else carried the one-level restriction: SHARED-BRACE-003 and SHARED-FIX-007's brace-fix sentence ("a body below it keeps its lines") already read without it. Rule pages unchanged.

### Examples for the tests

SHARED-BRACE-002, the new misindented-body example (RH5405), and its multi-line variant (RH5406), LF and CRLF.

### Output questions

_None._

## thoenissen/Reihitsu#1388, #1627, #1142

No change since 03-spec-author.md.

## Internal choices

_None._

## Follow-up candidates

_None._
