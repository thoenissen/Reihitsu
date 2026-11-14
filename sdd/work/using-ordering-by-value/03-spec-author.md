Role: spec author
Commit: f411dc0
Verdict: #1105: spec written (amendment); #1266: spec written (amendment, one example); #1267: spec written (checked, no change)

Amendment to `01-spec-author.md`, written from the maintainer's answer to Q1 and Decisions 4 and 5 in `00-lead.md`, and from the Internal choices in `02-skeptic.md`. No requirement ID was added and no rule page changed. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed.

## thoenissen/Reihitsu#1105

### Changed requirements
- SHARED-USING-004 — clarified, to record the maintainer's answer to Q1. The value is now "as the compiler interprets it": `@` dropped, Unicode escapes in identifiers decoded, and whitespace and comments "anywhere between the name's tokens — between its identifiers and dots, around the `::` of a `global::` qualifier, and inside a type-argument list" (Decision 5 replaces "between its parts"). The list of equal names now includes `System.IO` and `global :: System.IO`, and `Comparer< int >` has the key `Comparer<int>`. Why updated. New example (Q1): `using Microsoft.Win32; using System.IO; using System;` → `using System;⏎using System.IO;⏎⏎using Microsoft.Win32;`. The formatter keeps the escape's spelling.
- SHARED-USING-007 — clarified by a new example: `using static …Comparer<byte>; using static …Comparer< int >;` is in order.
- RH7201-001 — clarified. The requirement text now names Unicode escapes and trivia around `::`. New examples: `global :: System.Xml` after `Microsoft.Win32` is reported, and the Q1 input `Microsoft.Win32; System.IO; System` reports `System.IO`.
- RH7206-001 — clarified by an example: `static …Comparer< int >` then `static …Comparer<byte>` reports `Comparer<byte>`.
- RH7207-001 — clarified by an example: `System; ⏎ Microsoft.Win32; ⏎ System.IO` is reported.
- RH7207-002 — the organized example now also holds `System.Text` and `global :: System.Xml` in the `System` group.

### Examples for the tests
SHARED-USING-004 (Unicode-escape formatter example), SHARED-USING-007 (`Comparer` example, a formatter idempotence seed), RH7201-001, RH7206-001, RH7207-001, RH7207-002.

### Output questions
_None._ Q1 is answered; the examples above follow the answer.

### Internal choices
- `global :: System.IO` and `Comparer< int >` appear only in analyzer examples and in the "in order" shared example, not as a formatter before/after. The horizontal-spacing phase normalizes their spelling in whole-document output; SHARED-USING-004's wording and FMT-PIPE-004 already decide the formatter's result. Alternative: write formatter before/after blocks that include the spacing phase's output.
- SHARED-DIAG-002 itself (analyzer-wide) is not changed to mention Unicode escapes; SHARED-USING-004 states it for the using policy. Alternative: amend SHARED-DIAG-002, which is outside the run's issues.

### Follow-up candidates
- SHARED-DIAG-002 names `@`, whitespace, and comments but not Unicode escapes, while RH4001 and now SHARED-USING-004 decode them. Whether every name comparison under SHARED-DIAG-002 decodes escapes is a spec-wording question outside this run.

## thoenissen/Reihitsu#1266

### Changed requirements
- RH7204-001 — clarified by an example from the skeptic's class enumeration: in `B = System.Text; P = int; A = System.IO`, `A` is reported, with a non-name alias in between.

### Examples for the tests
RH7204-001 (new example).

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1267

### Changed requirements
None in this amendment. Decision 4 was checked: every example in the run's spec diff has at most one alias whose target is not a name per block. They are `using A = int` (RH7204-002), `using P = int` (RH7204-001), `int` or a tuple alone (RH7207-001, -002, -004), and the existing `int` example in SHARED-USING-004. No requirement text decides the order between two such aliases, and SHARED-USING-004 keeps its scope note "at most one alias whose target is not a name".

### Examples for the tests
_None new._

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None._

## Whole run
- Files: `sdd/specs/shared/using-directive-ordering.md`, `sdd/specs/analyzer/RH7201.md`, `RH7204.md`, `RH7206.md`, `RH7207.md`.
- Architecture SDD tests: 51 passed.
