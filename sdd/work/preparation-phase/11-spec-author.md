Role: spec author
Commit: 805ce53
Verdict: text written

Repair round 1, auditor finding 4 (`09-auditor.md`).

## Changed requirements

_None._ This is a text fix outside the specs.

- `AGENTS.md`, ownership-table row `PreparationPhase/LineEndings`: "line breaks inside literals, comments, and disabled text are never touched" now reads "line breaks inside literals, comments that are not documentation comments, and disabled text are never touched".
  - It now matches FMT-PIPE-019 and the FMT-EOL-003 rule that documentation-comment newlines are normalized.
  - Row count and phase names are unchanged, so `PhaseOwnershipDocumentationTests` is unaffected.
- No code, test, or spec file touched.

## Examples for the tests

_None._

## Open questions for the maintainer

_None._
