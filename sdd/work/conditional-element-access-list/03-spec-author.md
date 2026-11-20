Role: spec author
Commit: 85d74731
Verdict: change doc written

Follow-up round: the maintainer's answers to output questions 1 and 2 are recorded, the change doc is `Status: Accepted`, and lead decision 2 is applied. The spec was already written on 1A and 2A; checking it against the answers recorded in `00-lead.md` found nothing that did not match, so no requirement's behavior changed in this round. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Change doc: `sdd/changes/2026-11/conditional-element-access-list.md` — `Status: Accepted`; `Decisions` replaces the open *Output questions* paragraph with three sections: *Join the list in the formatter (maintainer, question 1A)*, *Report the list on its brackets (maintainer, question 2A)*, and *Which lists hold an index is stated once*. Under `Alternatives considered`, the narrowing of FMT-BREAK-014 and the location on the whole conditional access are now marked rejected by the maintainer.

## thoenissen/Reihitsu#1877

### Changed requirements

- FMT-BREAK-014 — Indexer arguments and angle-bracket lists are joined — unchanged in this round (changed in round 01; matches answer 1A). It is now the one place that defines which bracketed argument lists hold an index.

### Examples for the tests

FMT-BREAK-014 (second example), as in `01-spec-author.md`. Per answer 1A, the regression test `ConditionalElementAccessKeepsWrappedArgumentsWhenNestedIndexerCollapses` expects `a?[b[1, 2], 3]` (change doc, *Join the list in the formatter*).

### Output questions

_None._ Question 1 answered: 1A.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Changed requirements

- RH5307-001 — A wrapped element access is reported — clarified (lead decision 2): its normative sentence reports "a bracketed argument list that holds an index, as FMT-BREAK-014 defines it" instead of restating the three parent kinds; the sentence that the parent must not change the report now lists `[1,⏎2] = 3` in an initializer beside `m[`, `d![`, and `c?[`, since "the three" no longer has an antecedent. No reported input changes; FMT-BREAK-014 was already in the spec's `Uses` row.
- RH5307-003, RH5307-004 — unchanged in this round; they match answer 2A (located from `[` through `]`; `e?[0,⏎1][1,⏎2]` with two diagnostics).
- `documentation/rules/RH5307.md` — unchanged in this round; the exclusion sentence was already removed in round 01, as answer 2A requires.

### Examples for the tests

RH5307-001 (the `!`/`?` example and the comment-between-brackets negative), RH5307-003 (via RH5307-001), RH5307-004 (the last example), as in `01-spec-author.md`. No example changed in this round.

### Output questions

_None._ Question 2 answered: 2A.

### Internal choices

- RH5307-001 refers to FMT-BREAK-014 by ID only and does not repeat the three forms as an illustration in the normative sentence; the forms still appear in the sentence that the parent does not change the report and in the examples. Alternative: keep a parenthetical list of the three forms, which would restate the definition.

### Follow-up candidates

_None._

## Whole run

- No requirement IDs, `Uses` rows, or code owners added or removed; `scripts/test.sh --project architecture`: 198 passed.
- Files changed in this round: `sdd/changes/2026-11/conditional-element-access-list.md`, `sdd/specs/analyzer/RH5307.md`, this handover.
