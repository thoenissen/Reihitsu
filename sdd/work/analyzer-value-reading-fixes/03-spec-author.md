Role: spec author
Commit: 9730262
Verdict: spec written

Amendment after the skeptic (`02-skeptic.md`) and the maintainer's answers Q1–Q3 (all A) and decisions 4 and 5 in `00-lead.md`. Bug-fix lane: clarifications only, no change doc, no new requirement IDs, no rule page changed (the normal case of every rule is unchanged).

## thoenissen/Reihitsu#1071

### Changed requirements

- RH4001-002 — clarified: the type name is compared by its value with every Unicode escape sequence replaced (Q1); new example: `class Abc` is not reported in `Abc.cs`, is reported in `Invoice.cs`.
- RH4001-003 — clarified: type name and type-parameter names are compared as RH4001-002 compares the type name.
- RH4001-005 — clarified: the fix renames `Invoice.cs` declaring `class Abc` to `Abc.cs`.
- RH4001-006 — clarified: the taken-name check uses the name RH4001-005 derives; `Invoice.cs` with `class @Order` next to `Order.cs` in the same folder gets no fix (skeptic's breaking input).
- RH4009-002 — clarified: the dash clause restating SHARED-DIAG-002 is replaced by the reference plus "with every Unicode escape sequence replaced" (decision 5, Q1); the "entry is compared as written" sentence is kept; new example: list `[ "A.B" ]`, `namespace A.B { }` is not reported. The coverage sentence about a file without content after its root object is removed (Q2, see #974).

### Examples for the tests

RH4001-002 (escape example), RH4001-005 (escape rename), RH4001-006 (`@Order` with `Order.cs` present), RH4009-002 (escape example, and the entry-as-written sentence, which decision 5 asks to test).

### Output questions

_None._

### Internal choices

- The Unicode-escape statement is written into RH4001 and RH4009 only, as Q1 was asked for them; SHARED-DIAG-002 is not widened (a shared change is outside this run's issues).

### Follow-up candidates

- Whether SHARED-DIAG-002 should name Unicode escapes for every rule, as Q1 decided for RH4001 and RH4009. Spec wording only, no visible output of this run; the lead decides whether it is worth an issue.

## thoenissen/Reihitsu#1268

Nothing amended; 02-skeptic raised no spec gap for RH2003.

## thoenissen/Reihitsu#1289

### Changed requirements

- RH2109-001 — clarified (Q3): the normative sentence now says a `@code` is a directive only when it starts a line after optional white space and is not text; a `@code` behind other characters on its line, and a `@code` SHARED-DIAG-003 makes text, MUST NOT be reported. The former coverage sentence is folded into it. New example: `<div>@code { int X; }</div>` is not reported, an indented `@code {` inside `<div>` on its own line is.

### Examples for the tests

RH2109-001 (the new `<div>` example) and SHARED-DIAG-003's last example.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None from this role._ Note for the lead: decision 7 leaves C# comments and literals in `@{ }`, control-flow, and expression hosts reported. SHARED-DIAG-003 (unchanged) and RH2109-001 still demand that a `@code` line inside them is not reported, so that remainder is a known gap against the spec, to be carried by the follow-up decision 7 names; I did not narrow the spec to the implementation. A mid-line `@code` in those hosts is already not reported under Q3, so the gap is only a `@code {` that starts its own line inside a multi-line C# comment or literal outside a `@code`/`@functions` block.

## thoenissen/Reihitsu#974

### Changed requirements

- RH0001-003 — clarified (decision 4, Q2, skeptic's non-object root): trailing content is "anything other than JSON white space — space, tab, line feed, and carriage return — after the root value, whatever kind of value the root is"; new example sentences: `{ "formatting": 1 } junk` reports only invalid JSON syntax at the `j` (not the unknown section), `[ ] junk` reports only invalid JSON syntax at the `j` (not that the root must be an object).
- RH4009-001 — clarified (Q2): the rule reports nothing when the file is empty, white space only, or not valid JSON (RH0001-003), including content after its root value; the "before its root value ends" qualifier is gone; new example: `{ "naming": { "allowedNamespaceDeclarations": [ "A" ] } } junk` is not reported.
- RH4009-002 — the coverage sentence "covers a configuration file that has no content after its root object" is removed.

### Examples for the tests

RH0001-003 (all trailing-content examples, `{ "formatting": 1 } junk`, `[ ] junk`, trailing tabs and line breaks valid), RH4009-001 (the `junk` example).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._
