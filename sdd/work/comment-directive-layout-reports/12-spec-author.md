Role: spec author
Commit: b597ae3
Verdict: spec written

Repair round 2, Finding 1 of `11-spec-checker.md`, lead decision 10 (shared specs do not depend on rule specs). The prompt gave only the change to make; it carried no suspected cause or hoped-for verdict.

## thoenissen/Reihitsu#1386
I removed the clause "— the same span RH5204-005 gives a documentation comment" from the Why of SHARED-COMMENT-003 in `sdd/specs/shared/comment-placement.md`. The span stays described in words in the normative paragraph, and the Uses row is unchanged. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:   118, Skipped:     0, Total:   118".

## thoenissen/Reihitsu#1147, #1152, #1387, #1594, #1655, #1736
Nothing to do in this round.

## Changed requirements
SHARED-COMMENT-003 — Each comment is reported on its own — clarified (Why wording only)

## Examples for the tests
_None new._

## Output questions
_None._

## Internal choices
- Removed the rule-ID reference instead of adding RH5204-005 to Uses, per lead decision 10.

## Follow-up candidates
_None._
