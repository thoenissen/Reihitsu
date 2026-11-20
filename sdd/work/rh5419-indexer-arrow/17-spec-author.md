Role: spec author
Commit: c6485e8
Verdict: spec written

Repair round 3: auditor finding #4 in `16-auditor.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- RH5419-004 — The fix is withheld where the indexer cannot be put on one line — clarified. The one-line clause now states its cause as the governing condition ("would still span several lines, whatever keeps a line break there") and gives the causes as a non-exhaustive list ("Causes include …"), which now names a line break in front of `this` that the formatter keeps, such as one between a modifier, the type, an explicit interface name, and `this`. The opposite side is stated next to it: a bare line break between `this` and the `[` is joined (FMT-BREAK-011) and keeps the fix, unlike one in front of `this` (FC-002). `Why:` names the new cause. The withheld example gains `int⏎ this[decimal a,⏎ decimal b] => 1;` (auditor probe B2). No behavior change: the code already withholds it (auditor B1, B2, B4 under LF and CRLF).
- Change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`: *What* names the cause; new decision "A line break the formatter keeps in front of `this` withholds the fix" with an input/after block. Status stays Accepted (rare class withheld, nothing visible in question; decision 6).

No requirement ID, `Uses` row, or code owner added or removed, so the architecture tests are not affected; per the prompt no build or test was run. The new example was not run through `scripts/apply-fix.sh` (no builds in the shared tree); the auditor observed its exact shape (B2) withheld with `apply-fix.sh` on 84a0f1e. Rule page unchanged: a withheld fix for a corner case.

## Examples for the tests

RH5419-004, withheld example: the new `decimal` indexer (a withheld test under LF and CRLF, as finding #4 asks; `VerifySpecificationExampleOfWithheldFixes` gains it).

## Output questions

_None._

## Internal choices

- Marked the list of causes as examples and kept the formatted-text condition as the rule, instead of making the list exhaustive, so a further cause the formatter keeps does not need another spec repair (finding #4 offered both).

## Follow-up candidates

_None._
