Role: spec author
Commit: 54e4b2d
Verdict: spec written

Follow-up round: carries the maintainer's answer to Q8 ("a") into RH5201-002 and the change doc. My prompt asked for this change only; it carried no intended fix or hoped-for verdict.

## Changed requirements

- RH5201-002 — clarified (Q8 A): the condition on the own-line comment is now "stands below the token in front of the link — a line break stands between that token and the comment, also when other comments stand in front of the comment on its first line (`/* w */ /* x` ⏎ `*/ .Self()`)", replacing "only whitespace stands between the line break in front of the comment and the comment". The *Why* adds that a comment in front of the comment on its first line does not give the link a line of its own. The `*/` example gains `g` (reported on the `.` of the last `.Self`), with fix result `/* w */ /* x` ⏎ `*/` ⏎ `.Self();`. Qualifier examples (FC-002): `a` and `g` (line break between the token in front of the link and the comment, with or without a comment in front), both reported; `e` (comment starts behind code on that token's line), not reported.
- SHARED-CHAIN-005 — unchanged. Its exception already reads "stands below the token in front of the link — a line break stands between that token and the comment" (07 pass), which covers `g`. SHARED-CHAIN-007 counts the line break in front of `/* w */`, so it covers `g` too.
- Change doc `sdd/changes/2026-11/rh5201-first-call-behind-broken-root-line.md`: *Visible output changes* item 6 gains `g` with input and fix result; *Decisions* → *Maintainer answers* records the Q8 answer with its output.
- Rule page: unchanged (corner case).

Checked on the worktree: example `g` as a fixture goes through `scripts/apply-fix.sh RH5201` under LF and CRLF: fixed in one action, giving exactly the spec's fix result. `scripts/format.sh` gives the same `*/` ⏎ `.Self();`. `scripts/test.sh --project architecture` passed (198/198). `scripts/lane-check.sh behavior-change --head worktree` confirmed the lane.

## Examples for the tests

- RH5201-002 — new case `g` (`/* w */ /* x` ⏎ `*/ .Self()`, the comment's last line in the anchor column): reported on the `.` of the last `.Self`, fix `*/` ⏎ `.Self();`, under LF and CRLF. Case `f` (Q7) is still listed as needing a test in `07-spec-author.md`.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None._
