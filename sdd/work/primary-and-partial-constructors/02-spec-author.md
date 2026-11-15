Role: spec author
Commit: 9781203
Verdict: #1680: no spec change; #1742: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1680

Nothing to write. Every confirmed member (RH8204, RH8304, RH8308, RH8309, RH7302, RH7306, RH7307, RH7310) breaks an unchanged one-diagnostic requirement (RH8204-002, RH8304-003, RH8308-003, RH8309-003, RH7302-003, RH7310-003, SHARED-REGDESC-002), and the edge the shared guard must keep — a `#region` inside a primary constructor's parameter list, reported once — is already demanded by RH7302-001 ("every such `#region` is checked"). The bug is brought back to these specs; no gap needs a clarification.

### Changed requirements

_None._

### Examples for the tests

The requirements above; the skeptic's fixtures (01-skeptic.md, #1680) are the inputs.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1742

Records maintainer answer 1 (B, with the clarification that a diagnostic identical on both parts is reported once, on the implementing part) in `sdd/specs/shared/diagnostics.md`.

### Changed requirements

- SHARED-DIAG-013 — The defining part of a partial constructor is judged through its implementing part — added (clarification of today's behavior). Ledger updated.

Scope of the requirement, as written:
- covered: rules that judge elements one at a time as the compiler hands them — the defining part, its attribute lists, parameter list, parameters, parameter types, default values, and the documentation comment and directives in front of it are not reported; a diagnostic both parts carry alike is reported once on the implementing part; a violation only the defining part writes is not reported;
- not covered (judge the defining part as their specs say): once-per-constructor rules reaching it from the symbol or containing type (SHARED-DIAG-012, e.g. RH2004), the per-part modifier rules RH7105/RH7106, whole-file text rules (line/token, e.g. RH5031, RH6002), and partial methods, properties, indexers, events.

### Examples for the tests

SHARED-DIAG-013: three examples — RH4122 reports `a` once on the implementing part; RH7302 does not report `#region lower` in front of the defining part; RH2004 reports the defining part `partial {|RH2004:C|}(int value);` when neither part states an accessibility (RH2004 already has tests of this shape with `P`; tag them or add one with the spec's input).

### Output questions

_None._ The answer decides every visible case.

### Internal choices

1. Change doc: not written. The answer keeps today's output, and the requirement writes down existing behavior, which `sdd/README.md` (*When a change doc is required*, *Open questions*: "a clarification adds the current behavior as a requirement") classifies as a clarification. Alternative: treat it as a changed requirement because it narrows what rule specs such as RH7302-001 ("every `#region`") and SHARED-DIAG-006 demand, and write a change doc — that would move the run's lane to behavior change. Recommend no change doc; the lead decides.
2. Placement: one shared requirement in `diagnostics.md`, which applies to every rule, instead of an exception in each of ~35 rule specs. Alternative: per-rule non-goals — rejected as 35 copies of one driver fact. No rule spec and no rule page changed (a narrowed report in a corner case, `spec-author.md` step 4).
3. Wording names no driver API; it describes which rules are covered by how they reach the element ("one at a time, as the compiler hands them") because that is what decides the output. `Uses` unchanged: the requirement names only SHARED-DIAG-012 of its own spec and rule IDs, no other spec's requirement ID.

### Follow-up candidates

None new; the lead already holds the RH8102 candidate from 01-skeptic.md.

## Whole run

Files: `sdd/specs/shared/diagnostics.md`, `sdd/specs/requirement-ids.txt`, this handover.
