Role: spec author
Commit: ac1b3c1 (production code unchanged from d63eab4)
Verdict: spec written

Follow-up round: carries the maintainer's answers (1A 2A 3A 4B 5A) and lead decisions 4 and 5 into the specs and the change doc, and sets the change doc `sdd/changes/2026-11/rh5201-first-call-behind-broken-root-line.md` to `Status: Accepted`. My prompt carried the changes asked for as lead decisions, which I checked against the specs rather than taking as given; no verdict was hoped for.

## Changed requirements

- SHARED-CHAIN-005 — changed (this round, answer 4B): the line break between a prefix link's operator tokens counts only when no comment, directive, or disabled text keeps it there; the exclusions name the held one (RH5201 does not count it as a split; the formatter refuses every such input as unsupported, so formatter output is untouched).
- RH5201-006 — changed (answer 4B): the held inner line break of a prefix link's operator does not count, "counted as RH5201-003 counts it, as if it were not there"; a new negative example `g` (`c.Next? // keep` ⏎ `.Next.Self();`) "is not reported".
- RH5201-002 — clarified (decision 4): a link behind the closing `*/` of a delimited comment that starts its own line (only whitespace between the line break and the comment, and between `*/` and the operator) is reported whatever column the comment's last line starts in, also as a prefix link or a link of a chain without a call; a link behind the `*/` of a comment that starts behind code is judged as a link behind code. New example `a`/`b`/`d` reported, `e` not reported, with the fix results.
- RH5201-001 — clarified: its "MUST NOT be reported for not starting a line" for prefix links and call-less chains now names the two exceptions (RH5201-006, RH5201-002). Needed: without it, -001 contradicted the SHARED-CHAIN-005 exception for a call-less chain (skeptic's G2), so the spec text needed more than the change doc.
- RH5201 `Uses` — SHARED-COMMENT-012 added (decision 5).
- Change doc — `Status: Accepted`; *Affected requirements* lists the above; *What* states the adjacent fix; *Visible output changes* gains item 6 (link behind an own-line `*/` in the anchor column: later call, call-less chain, prefix link — the F11 shape, with the first call reported too) and item 7 now lists `g` (held split in a prefix operator) as not reported before and after; the rule page item is now 8; *Decisions* gains "Maintainer answers" (all five, 4B stated as not reported) and "Report a link behind an own-line `*/` in the anchor column".
- Rule page unchanged this round: answer 5A keeps the drafted wording; the adjacent fix is a corner case, not the normal flow.

## Examples for the tests

- RH5201-002 — the new `*/` example (`a`, `b`, `d` reported, `e` not) and its fix results; run under LF and CRLF. `d` holds two diagnostics (prefix `.Next` by -002, `.Self` by -006), so it also covers RH5201-006 and FC-008.
- RH5201-006 — the new negative example `g`.
- Unchanged from 01: RH5201-006's other examples, SHARED-CHAIN-005's formatter example.

Checked on the branch head with `scripts/apply-fix.sh RH5201` (LF and CRLF): `a`, `b`, `d`, `e` of the new -002 example and `g` give `no-diagnostic` today; `scripts/format.sh` gives exactly the fix results shown for `a`, `b`, `d` (inputs carry the blank line in front of the comment, so the blank-line phase changes nothing there) and leaves `e` unchanged; `g` is refused by the CLI. `scripts/test.sh --project architecture` fails only with `RH5201-006: no test covers it`, as expected until tests are tagged.

## Output questions

_None._

## Internal choices

- The adjacent fix is stated in RH5201-002 (column/position rule) with a matching exception clause in RH5201-001, rather than as a new requirement: it restates for RH5201 what SHARED-CHAIN-005 and SHARED-COMMENT-012 already require. Alternative: a new RH5201-007.
- SHARED-CHAIN-005 names RH5201's reading of the held split without citing RH5201-003 or FMT-UNSUP-015 by ID, since the `Uses` check would otherwise require those IDs in the shared spec's `Uses` row.

## Follow-up candidates

_None._
