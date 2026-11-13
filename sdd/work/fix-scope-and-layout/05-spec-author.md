Role: spec author
Commit: 6863ef8
Verdict: spec written

Repair after decision 20: the brace fixes RH5405–RH5407 and RH5403 go back to `main`'s behavior. My prompt carried no intended fix or verdict beyond the decision itself.

## thoenissen/Reihitsu#1140

- RH5403-003 — The fix lays out the statement — restored to `origin/main`'s text (the change to "lays out the reported block", the second example, the intro sentence, and the `Uses` entries SHARED-FIX-006/SHARED-FIX-007 are withdrawn).
- SHARED-BRACE-002 — The fix wraps the body in braces — restored to `origin/main`'s text (the SHARED-FIX-006 sentence, the changed Why, the last example, and the `Uses` entry SHARED-FIX-006 are withdrawn).
- `git diff origin/main` showed that every change of this run in `sdd/specs/analyzer/RH5403.md` and `sdd/specs/shared/statement-braces.md` belonged to these fixes, so both files were restored whole; neither file now differs from `origin/main`. No requirement ID was added for them, so `requirement-ids.txt` is unchanged.
- References checked: no other spec this run changes refers to the reverted text. RH5103-005's Why names RH5403 only as the rule that reports a one-line statement block (RH5403-001, unchanged on `main`), so it stays. `sdd/specs/shared/code-fixes.md` (RH5405 CRLF example) is not changed by the run.

## thoenissen/Reihitsu#1141
Nothing to do.

## thoenissen/Reihitsu#1144
The `else if` brace column is left to #1416 (decision 20); no spec text of this run states it any longer. Nothing else to do.

## thoenissen/Reihitsu#1145
Nothing to do.

## thoenissen/Reihitsu#1094
Nothing to do.

## thoenissen/Reihitsu#1393
Nothing to do.

## thoenissen/Reihitsu#1511
Nothing to do.

## thoenissen/Reihitsu#1561
Nothing to do.

## Changed requirements
- RH5403-003 — The fix lays out the statement — restored to `main`
- SHARED-BRACE-002 — The fix wraps the body in braces — restored to `main`

## Examples for the tests
None new. The tests tagged RH5403-003 and SHARED-BRACE-002 follow `main`'s examples again; the examples listed for them in 02-spec-author.md (RH5403-003 second example, SHARED-BRACE-002 last example) no longer exist.

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
