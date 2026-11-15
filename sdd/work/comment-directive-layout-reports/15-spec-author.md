Role: spec author
Commit: 6b83f0d
Verdict: spec written

Repair round 3. My prompt named the requested changes and the lead's decisions 17–19; it carried no suspected cause or hoped-for verdict beyond those recorded decisions. Clarifications only; no change doc (decision 2 covers SHARED-INIT-001; decisions 18 and 19 record the output each change decides).

Per issue: #1147 — SHARED-INIT-001 amended, RH5201-002 confirmed unchanged. #1386 — SHARED-COMMENT-003 amended. #1152, #1387, #1594, #1655, #1736 — nothing to do.

## Changed requirements
- SHARED-COMMENT-003 — Each comment is reported on its own — clarified (decision 18): each `///` line of a run that documents nothing is a comment of its own and gets one diagnostic, from that line's `///` to the end of its text, whether or not documentation comments are parsed. A `/** */` comment stays one diagnostic, from `/**` to the end of its text on its last line, also when it spans lines. New example: RH5038 with a two-line `///` run and a two-line `/** */` in an argument list. `main` reported none of these comments (they were documentation comments to it), so the per-line count is part of the #1386 output change decision 18 decides.
- SHARED-INIT-001 — A comment in front of a token anchors its line — clarified (decision 19): on an earlier line, only the comment that starts that line (only whitespace in front of it) is checked; a further comment behind it, or behind the tail of a comment from a line above, is not checked, also when it ends on the token's line. New example (RH5301, not reported): `/* a */ /* b */` on a line of its own above `P = 1`, and `/* c */ /* d⏎*/ Q = 2` (the auditor's finding #3 shape).
- RH5201-002 — A misaligned wrapped link is reported — confirmed, not changed: its text is identical to `main` (`git diff 3b3639e -- sdd/specs/analyzer/RH5201.md` is empty), and "a line whose first character that is not whitespace" is exactly `main`'s measure that decision 17 restores. Its tension with SHARED-COMMENT-012 for a tail-led line is the class decision 17 drops.

## Examples for the tests
- SHARED-COMMENT-003: the new RH5038 example, in Parse and None (two diagnostics for the `///` run, one for the `/** */`); audit #2 also asks for the same run on RH5048.
- SHARED-INIT-001: the new RH5301 example. Both members are visible changes from `main` that decision 19 covers: `main` reports `P = 1` behind the own-line `/* a */ /* b */` (the second comment cannot stand in the expected column) and reports `Q = 2` behind `/* c */ /* d⏎*/` when `/* d` is not in the member column. Add a misaligned counterpart (first comment of the earlier line in the wrong column → reported), and RH5303/RH5304 share `IsAlignedAt`.
- RH5201-002 (decision 17, no spec change): the auditor's counterexample `var r = a.B()⏎                 .C()⏎                 /* x⏎        */       .D();` is reported as on `main`.

## Output questions
_None._

## Internal choices
- SHARED-INIT-001: the earlier-line exemption is written as its own sentence in the first bullet instead of merging both bullets into one rule for "every line"; alternative: one bullet for every line in front of the token. Kept two bullets because the closing-brace offset applies only to earlier lines.
- SHARED-COMMENT-003: the per-line rule is stated in SHARED-COMMENT-003 (where the count is), not in SHARED-COMMENT-001 (which decides what is ordinary); alternative: a sentence in SHARED-COMMENT-001.

## Follow-up candidates
_None._ (The comment-tail class of audit hints and finding #1's reverse case are dropped by decisions 17 and 20.)

Architecture tests: `scripts/test.sh --project architecture` passed, 118 of 118.
