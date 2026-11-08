Role: spec author
Commit: 6491678
Verdict: spec written

Repair round 1. This amendment follows the maintainer's answer to output question 2 (`00-lead.md`, *Maintainer answers*), which was **B**. The case is a `///` or `/** */` comment behind the type on the type's line, in documentation mode `Parse` or `Diagnose`. Roslyn puts it in the first declarator's leading trivia. Answer B: the declaration is split as on `main`, the comment stays in the first field only, and the RH7101 fix is offered. This resolves auditor finding #1 (`09-auditor.md`) on the spec side. The guard's code behavior does not change. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Outputs were measured on `6491678` and are used verbatim in the examples:

- `scripts/format.sh`: one pass, then a second pass that reports `Formatted 0 of 1`.
- `scripts/apply-fix.sh RH7101`: LF and CRLF both give the same output as the formatter.

## thoenissen/Reihitsu#963

### Changed requirements

- FMT-STRUCT-030 (clarified):
  - The third bullet now lists the two kinds of comment that belong to the first declarator and do not block the split:
    - a comment that starts on a later line than the type's last token (answer A);
    - in `Parse`/`Diagnose`, a `///` or `/** */` comment behind the type on the type's line, also when the first declarator starts the next line (answer B).
  - The bullet now states the other side of the boundary explicitly. An ordinary `//` or `/* */` comment behind the type on the type's line blocks in every mode. So does a `///` or `/** */` comment there in mode `None`.
  - `Why:` gains one sentence for the doc-style case.
  - New example (`Parse`) with `private int /** c */ _first, _second;` and `private int /// <summary>c</summary>⏎        _g, _h;`. It shows the measured output, and a sentence below it says that neither declaration is split in `None`. The example says the comment's place inside the first field is not this requirement's decision.
- FMT-STRUCT-022 (clarified): the first-declarator clause now refers to "a comment that belongs to the first declarator as FMT-STRUCT-030 delimits it". It no longer refers only to the own-line case.
- FMT-PIPE-031 (clarified):
  - The comment condition names both first-declarator kinds as not counting.
  - The `Parse` example gains `private int /** c */ _g, _h;`. The method returns true for it in `Parse` and false in `None`.
- RH7101-007 (clarified):
  - The non-withholding sentence names both first-declarator kinds. It also states that the comment withholds the fix in mode `None`.
  - `Why:` gains one sentence.
  - New example (`Parse`): `private int /** c */ _first, {|RH7101:_second|};` with the fix output.

### Examples for the tests

- FMT-STRUCT-030: the new `Parse` example, with both declarations. Its `None` counterpart: neither declaration is split.
- FMT-PIPE-031: `_g, _h` → true in `Parse`, false in `None`.
- RH7101-007: the new `Parse` example (fix offered, under LF and CRLF). The `None` counterpart: reported, and no fix offered.

## thoenissen/Reihitsu#1384

No change: a doc-style comment in the first declarator's leading trivia is not a documentation comment of the declaration (SHARED-DOC-004), as before.

## Output questions

_None._

## Internal choices

- The case is stated in user terms: comment kind, line, and documentation mode, with the compiler's attachment given as the reason. The spec does not name trivia owners. The alternative was to define the boundary by trivia ownership. Line-and-mode wording matches the existing bullets.
- The example's output keeps the blank line and the moved comment inside the first field. A sentence says that this placement is not FMT-STRUCT-030's decision. The layout belongs to the documentation-comment phase, which is in the lead's existing follow-up candidate. The alternative was to leave the output out. That would leave the test seed incomplete, and the maintainer chose B from these outputs.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
