Role: spec author
Commit: 4343bbf
Verdict: spec written

Repair round 1, audit finding #1 (`08-auditor.md`), carrying out the lead's decision 4 (`00-lead.md`): the input class is refused as a rare class. The prompt carried the lead's decision on the class, which is the input of this round, but no intended fix, suspected cause, or hoped-for verdict beyond it; the mechanism is left to the implementer, and the spec names none.

## Changed requirements

- SHARED-DOC-019 — A union whose documentation holds a parameter element is not judged — added (`sdd/specs/shared/element-documentation.md`; ID inserted in `sdd/specs/requirement-ids.txt`). A rule that uses it reports nothing for a union whose documentation comment (SHARED-DOC-004, or a later one under SHARED-DOC-006) holds a `param` or `paramref` element at any depth — not the union, no element of its documentation, no type parameter — and does not fail with an exception on it, in every documentation mode the rule runs in. Nested declarations in the union body are judged on their own. Not covered: an element that only `<include>` content brings in; a part of a partial union.
- RH8035-001, RH8036-001, RH8108-001, RH8203-001 — clarified: SHARED-DOC-019 added to `Uses`, and the "nothing is reported when SHARED-DOC-008 applies" sentence extended by the union that SHARED-DOC-019 leaves unjudged, stated as a hand-correction decision. Rule outputs otherwise unchanged.
- Change doc `sdd/changes/2026-11/union-naming-documentation.md` — `Affected requirements` names SHARED-DOC-019; a bullet in *What*; a new *Decisions* section "A union whose documentation holds a parameter element is not judged" (input, before and after: not reported on `main`, not reported now); one rejected alternative (judge such a union from its syntax alone). `Status` stays `Accepted`: no visible output change is added or altered relative to `main`.

Rule pages: unchanged. The refusal is a corner case the normal user never meets, and the shared behavior already lives in `general-notes.md`; RH8035, RH8036, RH8108, and RH8203 pages keep saying what is reported in the normal case.

Verification: the four C# blocks of SHARED-DOC-019 (markup stripped, one namespace each) compile with the .NET 11 SDK (`net11.0`, `LangVersion 15`, no documentation file): Build succeeded, no errors or warnings. With `GenerateDocumentationFile` the compiler's CS1569 is expected for them, as the requirement's *Why* states.

## Examples for the tests

SHARED-DOC-019: one test per affected rule under the default documentation mode (`Diagnose`), with `CompilerDiagnostics.None` so CS1569 does not mask the result, as the auditor's counterexamples did:

- RH8035 — `/// <param name="Circle">` on `Shape` without `<summary>`: not reported, no AD0001.
- RH8036 — `/// <remarks>See <paramref name="x"/></remarks>` on a private nested `Inner`: not reported.
- RH8108 — generic `Result<T>` with `<summary>` and `<param name="x">`, no `<typeparam>`: not reported.
- RH8203 — `<inheritdoc/>` with `<param>`, and `<inheritdoc/>` with `<paramref>` nested in `<summary>`: not reported.
- RH8108 — the nested-method example: the union's `T` not reported, the method's `TItem` reported.

The other side of the qualifier is each rule's existing -001 union example (no `param`/`paramref`: reported). Mode `Parse` is in the class too ("every documentation mode"); one `Parse` test for RH8203 pins that the refusal does not depend on the mode.

## Output questions

_None._ On `main` no rule judges a union, so the refused inputs are not reported before or after; the maintainer's answers 1A–5A are untouched for every union without such an element.

## Internal choices

- One shared requirement (SHARED-DOC-019) referenced from four specs, instead of restating the refusal in each rule spec. Alternative: a sentence per rule spec; rejected because a rule a shared spec states is referenced, not restated.
- The refusal is mode-independent (also under `Parse`, where the expander does not throw), so it can be a cheap syntactic check of the comment. Alternative: refuse only under `Diagnose`; rejected because it would tie the rule's output to the documentation mode for this input.
- The refused class covers every `param`/`paramref` element, with or without a `name` attribute, written in lower case without a prefix. Alternative: only elements with a `name` attribute (the exact crash trigger); rejected as a finer guard for a class that is rare either way.
- RH8030 and RH8109 to RH8111 do not use SHARED-DOC-019: they decide on syntax only and do not fail (auditor's class enumeration), so they keep judging such a union. Alternative: refuse uniformly in all union-checking rules; rejected because it gives away safe ground.

## Follow-up candidates

_None._ Unverified: whether `param` content brought in only by `<include>` also makes the compiler's expansion throw; the auditor did not probe it, and SHARED-DOC-019 leaves it uncovered. If the test implementer can construct it cheaply, a probe settles it; a crash there would be a new member of the same class for the lead to decide.
