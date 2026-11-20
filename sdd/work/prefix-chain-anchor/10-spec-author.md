Role: spec author
Commit: eadf630c6cf0e639b2a06fcf816ac19019e80a5b
Verdict: change doc written

Repair round 1, auditor finding #2 (`08-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding.

## Changed requirements

_None._ Only the change doc `sdd/changes/2026-11/prefix-chain-anchor.md` changed:

- Visible change 7 (*Repository source*) no longer claims repository files are reformatted. It now says no file is reformatted to the new anchor: the `binaries/` analyzers that build the repository keep the old anchor until rebuilt, so the four repository chains with two or more links in front of the first wrapped link were rewritten, with a new local or a new private method, to a prefix of at most one link, keeping the same calls in the same order (source: `06-cleaner.md`, *Warnings resolved*).
- *Decisions → Maintainer answers*, first bullet: the reference "visible changes 1, 3, 4, 5, 6, and 7" drops 7, since visible change 7 no longer describes a realignment that answer 1A decided.

No spec, rule page, requirement ID, `Uses` row, or code owner changed, so no architecture-test run was needed.

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

- Kept the repository note as visible change 7 rather than moving it out of *Visible output changes*; the alternative (moving it under *Decisions*) would renumber nothing but loses the cross-reference the auditor named.

## Follow-up candidates

_None._
