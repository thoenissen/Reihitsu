Role: spec author
Commit: ebac800
Verdict: spec written (amendments per decisions 5–8 of `00-lead.md`, from `03-skeptic.md`)

My prompt named the decisions to carry out and carried no other intended fix or verdict; wording is my own, re-derived from `03-skeptic.md` and the code.

## thoenissen/Reihitsu#966

### Changed requirements

- SHARED-USING-009 — Blocks with preprocessor directives are not reordered — example amended (decision 5): the B1 example's continuation line `.Text;` now stands at column 0, the formatter's stable shape, so "is left unchanged and not reported" holds for formatter output = input. No wording change.

### Examples for the tests

- SHARED-USING-009: B1 as amended; the `# Header` example unchanged. Matrix A gains A9 from `03-skeptic.md` (decided by SHARED-USING-009, SHARED-FIX-012, FMT-PIPE-033; no spec change needed).

## thoenissen/Reihitsu#1289

### Changed requirements

- RH2109-004 — A line inside a multi-line C# comment or literal is read like markup — added (decision 7): the paragraphs, `Why:`, and examples that `02-spec-author.md` put into RH2109-001 moved into their own requirement; ID added to `sdd/specs/requirement-ids.txt`.
- RH2109-001 — changed: its first paragraph now says "RH2109-004 decides instead" for a line inside a multi-line C# comment or literal; its own `Why:` and examples are as on `main`.
- RH2109-004 converse bullet (decision 6): a `@*` is closed only by `*@` and a `<!--` only by `-->`; when that closer does not follow, the comment runs to the end of the file, and a closer of the other kind does not end it. New third example pins the pairing (`@*` on line 3, `<!-- -->` on line 6, `@code` on line 7 not reported; only line 1 reported).
- SHARED-DIAG-003 — pointer sentence reworded (decision 8): "RH2109 is an exception to this requirement for a line inside a multi-line C# comment or literal of a Razor file: it reads such a line like markup, as RH2109-004 states."
- Change doc `sdd/changes/2026-11/rh2109-csharp-lines-read-as-markup.md`: affected requirements now RH2109-001, RH2109-004, SHARED-DIAG-003; *What* uses the same closer-pairing wording and names RH2109-004; one alternative added (recording the class inside RH2109-001, rejected for its different reason). Still `Status: Draft`.

### Examples for the tests

- RH2109-004: all three examples (lines 1, 3, 7 reported; `@*` hides the rest of the file; `-->` does not close `@*`). The retagged characterization tests and the new converse tests tag RH2109-004, not RH2109-001 (decision 7).

## Output questions

_None._

## Internal choices

_None_ beyond decisions 5–8.

## Follow-up candidates

Unchanged from `02-spec-author.md` (mid-line opener of a real HTML or Razor comment).
