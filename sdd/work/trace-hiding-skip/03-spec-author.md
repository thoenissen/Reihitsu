Role: spec author
Commit: 38dbd68
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the requested edits, which come from `02-skeptic.md` and `00-lead.md` (Decisions taken, row 4).

Edits to `sdd/changes/2026-11/trace-hiding-skip.md`:

- *Visible output changes*: the precheck's refusal for a carrier file that also holds an unsupported shape was printed to standard error "on every pass"; now "once, with the wrong reason" (skeptic's `carrier_unsup.cs`: the trace stops with `Stable after pass 1`).
- *Why*: "Reproduced on `main` (e2a1f5f)" relabelled as the run's start commit e2a1f5f, which has no code difference from `main` (merge base eb89003).
- `Status: Accepted`: the doc lists no visible output change, and no output question is open.

## Changed requirements
_None._ The trace has no spec.

## Examples for the tests
None from a spec; the seeds in `02-skeptic.md` (*Breaking inputs*) and the change doc's example stand.

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None_ beyond the one already in `00-lead.md`.
