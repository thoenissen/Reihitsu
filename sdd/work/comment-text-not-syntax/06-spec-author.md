Role: spec author
Commit: b027f9e
Verdict: spec written (spec metadata repair, decision 10 of `00-lead.md`)

My prompt named only the decision to carry out and the failing test; it carried no intended fix or verdict beyond that.

## Changed requirements

- No requirement changed. `sdd/specs/shared/diagnostics.md` header: `Uses` row gains `RH2109-004` at its sorted position (between `FMT-UNSUP-008` and `RH5308-005`), because SHARED-DIAG-003's pointer sentence (from `04-spec-author.md`, decision 8) references RH2109-004 outside the non-goals.
- `scripts/test.sh --project architecture`: Passed 118, Failed 0 (`UsesListsExactlyTheReferencedRequirements` included).

## Examples for the tests

_None_ — no example changed.

## Output questions

_None._

## Internal choices

_None._ (Alternative considered and not taken: rewording SHARED-DIAG-003 to drop the ID reference; it would weaken the pointer decision 8 asked for.)

## Follow-up candidates

_None_ new.
