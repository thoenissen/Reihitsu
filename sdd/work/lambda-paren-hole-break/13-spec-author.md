Role: spec author
Commit: 3663caf
Verdict: spec written

Repair round 1, for lead decision 10 (auditor finding #1). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 10 itself, which is the lead's recorded decision and not a hope; I worked from 00-lead.md, 09, 12, the change doc, and the specs.

## Changed requirements

- **FMT-INDENT-031 — An anonymous method in a raw-string hole aligns like a block lambda — changed.**
  - New paragraph defining *the hole*: the innermost hole of a multi-line interpolated raw string literal that contains the anonymous method. A regular, verbatim, or single-line raw interpolated string inside it does not end it, so an anonymous method in a hole of such a nested string aligns under its anchor, as the block lambda in the same position does. A nested multi-line raw literal does end it.
  - Exception paragraph: "the same hole" now reads "between the anonymous method and the hole", and it holds whether or not a nested regular, verbatim, or single-line raw string lies between the body and the anonymous method.
  - The "as before" sentence is narrowed: the scope depth applies outside interpolation holes and in a hole of a regular, verbatim, or single-line raw interpolated string *that no hole of a multi-line interpolated raw string literal encloses*.
  - Examples added: the nested case (regular, verbatim, single-line raw, plus the block-lambda counterpart, in a moved literal; the aligned literal gives the same output); a single-line raw literal not enclosed by a multi-line hole, left unchanged; and an anonymous method in a regular string inside a block lambda's body in the hole, left unchanged.
- **FMT-RAW-007 — unchanged.** It already counts FMT-INDENT-031 among the alignments and speaks of lines starting inside a hole, which covers lines inside a nested string's hole.
- **Change doc** `sdd/changes/2026-11/lambda-paren-hole-break.md`:
  - *What*: the FMT-INDENT-031 bullet covers the nested strings; "Outside holes nothing changes" now says outside holes of multi-line interpolated raw string literals.
  - New entry 8 with input, before, and after. The before output was taken at 3663caf (the run's code leaves the case unaligned, as `main` does); the doc says so.
  - *Unchanged*: the single-line bullet is narrowed to strings no multi-line raw hole encloses, and a bullet added for the nested string inside a lambda body.
  - Maintainer answer 1's line now names entries 1–4 and 8. Status stays `Accepted`: entry 8 is covered by lead decision 10 (answer 1 and the general clarification).

Evidence (scratch fixtures, `scripts/format.sh --force` at 3663caf):
- Block-lambda counterparts in the nested regular, verbatim, and single-line raw position align at `(` in columns 28, 29, 30 — the columns of `delegate` in the new example.
- Anonymous methods in the same positions end in column 13 (moved literal) and column 0 (aligned literal) today, confirming the finding.
- `var c = $"""in {M(delegate { return 1; })}""";` and the nested-in-lambda-body exception example are fixed points today.
- The "after" output of entry 8 is not yet produced by any code; it is derived from the requirement and the counterpart columns.

## Examples for the tests

FMT-INDENT-031: the new nested-string example (moved and aligned literal, regular, verbatim, single-line raw), the unenclosed single-line raw example, and the nested-string-in-lambda-body exception example.

## Output questions

_None._ The only visible change is entry 8, which lead decision 10 covers. No other visible output change follows from my edits.

## Internal choices

- The nested multi-line raw literal ends the walk (its own hole is the hole). This is what the existing test `AnonymousMethodInNestedLiteralInsideBlockLambdaOfOuterHoleIsAligned` and the registration in `LayoutComputer.RegisterRawStringHoleLines` already do; stated for completeness, no output change. Alternative: leave it unstated.
- I did not add a `static` or `async` variant to the nested example; the anchor rule is position-independent and stated once above. Alternative: a fourth nested line `Z {$"""in {M(static delegate …)}"""}` (block under `delegate`, column 37 in an aligned literal).

## Follow-up candidates

_None_ beyond the lead's list in 00-lead.md.
