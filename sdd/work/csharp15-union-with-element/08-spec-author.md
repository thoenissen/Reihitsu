Role: spec author
Commit: 0c87802
Verdict: spec written

Repair round 1 for the spec checker's finding 1 on #1762 (`07-spec-checker.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding; the change below is re-derived from the code: `RH7101DoNotCombineFieldsCodeFixProvider` passes `fieldDeclaration.Parent` as the context of `FormatNodeInDocumentWithContextAsync`, and with FMT-STRUCT-021 / FMT-PIPE-031 now accepting a union, that parent can be a union declaration (test `VerifyCombinedStaticFieldInAUnionIsSplit`).

## thoenissen/Reihitsu#1762

- `sdd/specs/formatter/pipeline.md`, FMT-PIPE-029, last paragraph: the context the RH7101 fix passes is now "the class, struct, record, interface, or union declaration whose member it is". No other sentence of the requirement depends on the type kind.
- `sdd/changes/2026-11/union-declaration-layout.md`: `Affected requirements` adds FMT-PIPE-029 and RH7101-002 (the RH7101-002 `Why:` edit of round 01 was already in the spec, only unlisted); the *What* bullet on field splitting names the union context (FMT-PIPE-029) and the RH7101-002 wording. Status stays `Accepted`: no visible output changes; the change doc's output list already covers the RH7101 fix on a union.

No other `class, struct, record, or interface` enumeration in `sdd/specs` decides an input this run changes (`indentation.md` FMT-INDENT-017 base lists and RH3202-001 are outside the run's requirements; neither was flagged).

## thoenissen/Reihitsu#1763

Nothing to repair.

## Changed requirements
- FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — clarified (union context named).

## Examples for the tests
None new. `VerifyCombinedStaticFieldInAUnionIsSplit` (and the test after it) carry only `[Requirement("RH7101-004")]`; they also cover the FMT-PIPE-029 union context. Adding a `FMT-PIPE-029` tag is optional and the lead's call — the spec checker did not require it.

## Output questions
_None._

## Internal choices
- Named the union in FMT-PIPE-029's existing enumeration rather than rewording it to "the type declaration whose member it is"; alternative: generic wording, rejected because extension blocks would read as included (decision 2 in `00-lead.md` keeps the narrow list).

## Follow-up candidates
_None_ new.
