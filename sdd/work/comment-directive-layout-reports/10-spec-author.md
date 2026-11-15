Role: spec author
Commit: 11b773c
Verdict: spec written

Repair round 1, Finding 1 of `09-spec-checker.md`, lead decision 13. My prompt carried a requested change (repair Finding 1 as a clarification), no suspected cause or hoped-for verdict. Clarification only: no visible output changes against `main` (documentation comments were not reported there), so no change doc.

## thoenissen/Reihitsu#1386
SHARED-COMMENT-003 now states the span of a diagnostic on a `///` or `/** */` comment: from its first `///` or `/**` to the end of its text on its last line, without the line break behind it — the same span RH5204-005 gives. Matches `CommentPositionUtilities.GetCommentLocation`, which `CommentPositionAnalyzerBase` (RH5033-RH5047, RH5049) and RH5048 use. Added an RH5038 example with a `///` and a `/** */` comment in an argument list, mirroring the existing tests `VerifyTripleSlashCommentBeforeFirstArgumentIsReported` and `VerifyDelimitedDocumentationStyleCommentBeforeFirstArgumentIsReported`.

## thoenissen/Reihitsu#1147, #1152, #1387, #1594, #1655, #1736
Nothing to do in this round.

## Changed requirements
SHARED-COMMENT-003 — Each comment is reported on its own — clarified (span of a documentation-style comment; new example)

## Examples for the tests
SHARED-COMMENT-003 (new RH5038 example; already asserted by the RH5038 tests named above — the test implementer may add the `[Requirement]` tag if the lead wants it, no new behavior)

## Output questions
_None._

## Internal choices
- Left out a sentence on runs of several `///` lines: with documentation comments not parsed each line is its own comment trivia, so "one diagnostic per run" would not hold in every mode (SHARED-COMMENT-001); the clarification states only the start and end of the span. Alternative: state the run behavior per documentation mode.
- No rule page changed: where a diagnostic lies is not a change to what is reported or how to fix it.

## Follow-up candidates
_None._
