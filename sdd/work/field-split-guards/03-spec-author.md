Role: spec author
Commit: 567da89
Verdict: spec written

Amendment after the maintainer's answer to output question 1 (`00-lead.md`, *Maintainer answers*; `02-skeptic.md`): **A** — a comment on its own line in front of the first declarator's name is in that declarator's leading trivia, does not block the split, stays in front of the first field only, and the RH7101 fix is offered.

## thoenissen/Reihitsu#963

### Changed requirements

- FMT-STRUCT-030 — retitled "A documented field, or one with a comment in the part every field repeats, is not split" — clarified:
  - the blocking bullet now ends at "behind the type on the type's line" (was "between the type and the first declarator");
  - a new third bullet names the first declarator's slot on the split side: a comment that starts on a later line than the type's last token, in front of the first declarator's name, belongs to the first declarator and does not block; `///`/`/** */` there is treated the same way (not a documentation comment of the declaration, SHARED-DOC-004); a comment behind the type on the type's line still blocks when the first declarator starts the next line;
  - `Why:` gains the sentence that this slot is copied nowhere, so FMT-STRUCT-024 does not refuse the split;
  - the blocking example gains `private int // c⏎        _e, _f;`; a new split-side example is the skeptic's measured case (input `private int⏎        /* c */ _first, _second;`, output `private int⏎    /* c */ _first;⏎    private int⏎    _second;`, which the maintainer accepted as today's output).
- FMT-STRUCT-022 — clarified: the normative sentence states that a comment in front of the first declarator's name on a line of its own stays in front of the first field's name; the pointer to FMT-STRUCT-030 says "as FMT-STRUCT-030 delimits it".
- FMT-PIPE-031 — clarified: the comment condition refers to FMT-STRUCT-030's delimitation and says an own-line comment in front of the first declarator's name does not count.
- RH7101-007 — retitled "The fix is withheld for a comment in the part every field repeats" — clarified with the same slot boundary; the withheld example gains `private int // c⏎        _e, {|RH7101:_f|};`; a new example shows the fix offered for the own-line comment, with the same output as the formatter example.

### Examples for the tests

- FMT-STRUCT-030: the extended blocking example (`_e, _f`) and the new own-line example.
- RH7101-007: the extended withheld example and the new own-line example (fix offered). Existing `RH7101-004`-tagged tests for this slot stay valid.

## thoenissen/Reihitsu#1384

No change from `01-spec-author.md`, apart from this: a `///`/`/** */` comment in the first declarator's own leading trivia is now named explicitly as not blocking (FMT-STRUCT-030, RH7101-007).

## Output questions

_None._

## Internal choices

- Retitled FMT-STRUCT-030 and RH7101-007, because "in front of its first declarator" now reads opposite to the answer — alternative: keep the titles. Titles are not IDs; no reference quotes them.
- Drew the boundary at the line, "behind the type on the type's line" against "on a later line than the type's last token", rather than naming trivia owners. This is the user-visible form of Roslyn's trailing-trivia/leading-trivia boundary — alternative: name the trivia slots in the spec.

## Follow-up candidates

_None._
