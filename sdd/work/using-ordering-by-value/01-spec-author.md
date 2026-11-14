Role: spec author
Commit: ce92404
Verdict: #1105: spec written (clarification); #1266: spec written (clarification); #1267: spec written (clarification, examples only)

No change doc: every edit writes down what an unchanged shared requirement (SHARED-DIAG-002, SHARED-USING-004, SHARED-USING-007) already demands, and the maintainer's triage of thoenissen/Reihitsu#1266 puts it in the bug-fix lane without one. My prompt carried no intended fix or suspected cause. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed (Uses rows, ledger, IDs). No requirement ID was added, so `sdd/specs/requirement-ids.txt` is unchanged. Rule pages under `documentation/rules/` are unchanged (see Internal choices).

## thoenissen/Reihitsu#1105

### Changed requirements
- SHARED-USING-004 — A group is section, kind, and root namespace — clarified: every name the spec compares or splits (directive name, alias target, static type name, alias name) is taken by its value (SHARED-DIAG-002), by analyzer and formatter alike; `System.IO`, `System . IO`, `@System.IO`, `System/* x */.IO` all have root `System` and key `System.IO`. New formatter example with `@System.IO`. `Uses` of the shared spec now lists SHARED-DIAG-002.
- SHARED-USING-007 — Alphabetical order within a group — clarified by a new example: `System.@Linq` after `System.Collections` is in order.
- RH7201-001 — clarified: neither `@` nor whitespace or comments inside the name change the root (SHARED-USING-004 added to `Uses`); example with `System . IO`, `@System.Text`, `System/* x */.Linq` after `Microsoft.Win32`, all reported (the issue's instance).
- RH7203-001 / RH7203-002 — clarified (value): reported examples `System.Linq` followed by `System . IO`, `@System.IO`, `System/* x */.IO`; not-reported example `System.Collections` then `System.@Linq`.
- RH7204-001 — clarified (value): `B = System.Text` then `A = System . IO` / `A = @System.IO` reported (the issue comment's instance).
- RH7206-001 — clarified (value): `static System . Math` / `static @System.Math` then `static System.Console` reported (the issue comment's instance).
- RH7207-001 / RH7207-002 — clarified (value): a blank line in front of `@System.IO` or `System . IO` after `System` is inside a group and reported; `System; @System.IO; System/* x */.Linq; <blank> Microsoft.Win32` is organized.

### Examples for the tests
SHARED-USING-004 (`@System.IO` example — formatter and RH7201/RH7207 fixes), SHARED-USING-007 (`System.@Linq` example), RH7201-001, RH7203-001, RH7203-002, RH7204-001, RH7206-001, RH7207-001, RH7207-002.

### Output questions
_None._

### Internal choices
- The formatter's grouping and order follow the value too, stated in SHARED-USING-004 rather than in FMT-USING. Alternative: read the shared spec's "name" as spelling for the formatter. Rejected because SHARED-USING requires analyzer and formatter to agree and the code fixes to produce the formatter's order, while SHARED-DIAG-002 binds the analyzers to the value; a spelling reading would make the fixes produce blocks the rules report, so only one reading satisfies the unchanged requirements. The issue asks the same. Visible consequence for the lead to confirm: `@System.IO`, `System . IO`, and a name with a comment inside move into the `System` group in formatter output.
- Examples with a comment inside a name are written only for analyzer reports (no formatter before/after), because the horizontal-spacing phase's treatment of a comment inside a qualified name is not this run's question; the formatter example uses `@`, which no phase rewrites.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1266

### Changed requirements
- RH7203-001 — retitled "A regular directive sorted before an earlier one of its group is reported" — clarified to SHARED-USING-007: compared with every earlier directive of its group, whatever lies between.
- RH7203-002 — clarified: not reported when the key comes before no earlier key of its group.
- RH7204-001 — retitled "An alias sorted before an earlier alias of its group is reported" — clarified likewise.
- RH7204-002 — clarified likewise.
- RH7206-001 — retitled "A static directive sorted before an earlier one of its group is reported" — clarified likewise.
- RH7206-002 — clarified likewise.
- Spec summaries of RH7203, RH7204, RH7206 reworded from "directly above it" to "an earlier directive of its group".

### Examples for the tests
RH7203-001 (`Microsoft.B; Alpha.X; Microsoft.A`, `Microsoft.B; using static System.Math; Microsoft.A`, `Microsoft.C; Microsoft.A; Microsoft.B` — both reported), RH7204-001 (`B = Microsoft…SyntaxNode; C2 = System.Text; A = Microsoft…SyntaxToken`; `C2 = System.Text; A = System.IO; B = System.Linq` — A and B reported), RH7206-001 (`…SyntaxFacts; Reihitsu.Core.CasingUtilities; …SyntaxFactory`; `System.Math; System.Console; System.Convert` — Console and Convert reported). SHARED-USING-007 already carries the RH7203 and RH7110 examples.

### Output questions
_None._ The reported spans follow from SHARED-USING-007 unchanged.

### Internal choices
- Other ordering rules checked for the same pattern (issue's "Done when"), by spec text: RH7201-001 ("follows a regular directive of another root"), RH7202-001 ("precedes a regular directive"), RH7205-001 ("follows an alias … or precedes a regular") already compare with every earlier/later directive of the section, not the adjacent one; no spec change. Whether the code matches is the skeptic's and test implementer's to check.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1267

### Changed requirements
- SHARED-USING-004 — unchanged in substance: it already states that an alias whose target is not a name forms its own group after every other group of its section, with an `int` example.
- RH7204-002 — clarified by example: `B = System.Text; A = int` is not reported (different groups).
- RH7207-001 — clarified by examples: `using B = int;` / `using P = (int X, int Y);` before `using A = System.Text;` is reported on the whole directive (RH7207-003).
- RH7207-002 — clarified by examples: `A = System.Text; <blank> B = int` (the issue's instance) and `System; A = System.Text; P = (int X, int Y)` with blank lines are organized.
- RH7207-004 — new fix example: `using B = int; <blank> using A = System.Text;` → `using A = System.Text; <blank> using B = int;`.

### Examples for the tests
SHARED-USING-004 (`int` example — formatter, idempotence on its output), RH7204-002, RH7207-001, RH7207-002, RH7207-004.

### Output questions
_None._ The output is decided by SHARED-USING-004 unchanged.

### Internal choices
- No new requirement: the issue asks for a "spec clarification", but SHARED-USING-004 already decides the case, including predefined types and tuples; I added rule-spec examples only. A section with two or more non-name aliases stays out of scope as SHARED-USING-004 already says.
- Rule page `documentation/rules/RH7207.md` is left alone: the position of a non-name alias is a corner case, not the normal case of what is reported or how to fix it.

### Follow-up candidates
_None._

## Whole run

- Files: `sdd/specs/shared/using-directive-ordering.md`, `sdd/specs/analyzer/RH7201.md`, `RH7203.md`, `RH7204.md`, `RH7206.md`, `RH7207.md`.
- Rule pages `documentation/rules/RH7201.md`–`RH7207.md` unchanged: none states "directly above", spelling, or the non-name alias position.
- Architecture SDD tests: 51 passed.
