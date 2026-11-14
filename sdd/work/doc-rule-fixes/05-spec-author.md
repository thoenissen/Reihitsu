Role: spec author
Commit: 764e0fb
Verdict: change doc written

Correction round applying decisions 13, 14, and 15 of `00-lead.md` (findings in `04-skeptic.md`). No requirement ID added or retired; `requirement-ids.txt` unchanged. Architecture spec tests: 49 passed, 2 failed — the same two expected failures as in 01 and 03.

## thoenissen/Reihitsu#1617

### Changed requirements

- RH8203-001 — changed (decision 13): only the `<inheritdoc>` of each part's own documentation is considered, never one the other part carries; a part whose own documentation holds no `<inheritdoc>` is not reported, also when the other part carries one. The partial example gains `O` (defining part `<summary>`, implementing part `<inheritdoc/>`, member inherits nothing): only the implementing part is reported. The per-part judgment now cites SHARED-DIAG-004's second paragraph; the inheritance verdict says "by its symbol" without citing the first paragraph (decision 14).
- RH8201-001 — citation aligned (decision 14): the per-part judgment cites SHARED-DIAG-004's second paragraph; the inheritance verdict is "by its symbol" without a citation. No change in meaning.
- SHARED-DIAG-004 — second paragraph extended (decision 14, skeptic option A): the documentation comment one part of a partial member carries, as RH8201 and RH8203 judge whether it uses `<inheritdoc>`, is text one part writes independently; "own modifiers" became "own text"; a sentence says a fact about the declaration as a whole (such as whether a member inherits documentation) is still decided by the symbol. Why gains one sentence on a part's documentation comment. RH8306 and SHARED-DIAG-012 untouched.
- Change doc: Affected requirements gains SHARED-DIAG-004; What, Decisions, and Alternatives updated to match; Visible output changes gains the `O` shape (before: both parts reported, the defining part on its name; after: only the implementing part, on its `<inheritdoc/>`), for all four member kinds.

### Examples for the tests

RH8203-001 (the `O` case in the partial example), for method, property, indexer, and event; per the skeptic, test the partial event's defining part (`EventFieldDeclarationSyntax`) for both rules.

### Test tags the amendment retires or moves

None beyond 03.

## thoenissen/Reihitsu#1347

- Change doc, Decisions (decision 15): "Formatting tags are not reported at all" narrowed to nested formatting tags; a formatting tag at the top level of a comment is a top-level element and is reported as before. No spec change; RH8307-001 already leaves a top-level `<b>` as on `main`.

## Output questions

_None._

## Internal choices

- SHARED-DIAG-004 sentence on "a fact about the declaration as a whole" added so the second paragraph and the rule specs' "by its symbol" agree without citing the first paragraph. Alternative: leave the inheritance fact uncited in the shared spec.

## Follow-up candidates

_None_ new (the skeptic's symbol-XML readers candidate is in 04).
