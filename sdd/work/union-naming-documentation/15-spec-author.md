Role: spec author
Commit: 1a84049
Verdict: spec written

Architecture-finding refusal (not a repair round): carries out the lead's decision 6 (`00-lead.md`) for audit finding #2 (`14-auditor.md`), including the auditor's wording hint (the expansion throws only under `Diagnose`). The prompt carried the lead's decision on the class, which is the input of this step, and no intended fix, suspected cause, or hoped-for verdict beyond it; the spec names no mechanism.

## Changed requirements

- SHARED-DOC-019 — A union whose documentation holds a parameter element is not judged — changed (`sdd/specs/shared/element-documentation.md`):
  - (a) A *parameter element* is defined: an element whose tag name, without a namespace prefix it may carry, is `param` or `paramref` in any letter case (`<param>`, `<Param>`, `<PARAMREF/>`, `<x:param/>`). The coverage sentence "with the tag name in lower case and without a namespace prefix" is removed.
  - (b) New paragraph: independently of which elements the documentation holds, the rule MUST NOT fail with an exception on any union because the compiler's expansion of the union's documentation fails; when it fails, the rule reports nothing for that union (for a partial union, for none of its parts). This also covers content brought in only by `<include>`, which the previous text excluded.
  - *Why* states that the expansion throws when documentation comments are diagnosed (`Diagnose`, the default) and not under `Parse`, that the refusal of a parameter element is nevertheless mode-independent, and that the rules do not rely on predicting which elements the compiler binds.
  - New example: tag names `<Param>`, `<PARAMREF/>`, `<pAram>` for RH8035, RH8108, RH8203.
- RH8035-001, RH8036-001, RH8108-001, RH8203-001 — clarified: the "left unjudged" clause reads "holds a `param` or `paramref` element in any letter case, or whose documentation the compiler cannot expand". `Uses` rows unchanged.
- Change doc `sdd/changes/2026-11/union-naming-documentation.md` — *What* bullet widened; the *Decisions* section "A union whose documentation holds a parameter element is not judged" gains a paragraph on the second audit and decision 6 (any letter case; failed expansion leaves the union unjudged; `Diagnose`-only throw, mode-independent refusal); one rejected alternative (add each spelling per audit finding without the safety net). `Status` stays `Accepted`: no visible output change against `main` is added or altered (no rule judges a union on `main`).

Rule pages: unchanged, as decision 6 states — the refusal is a corner case and shared behavior lives in `general-notes.md`.

Verification: the new SHARED-DOC-019 example compiles (.NET 11, `net11.0`, `LangVersion 15`, no documentation file): Build succeeded, 0 warnings, 0 errors. `scripts/test.sh --project architecture` → Passed: 118, Failed: 0.

## Examples for the tests

SHARED-DOC-019, new example "a tag name in another letter case", each under the default mode (`Diagnose`) with `CompilerDiagnostics.None`, not reported and no AD0001:

- RH8035 — `/// <Param name="Circle">` on `Shape` without `<summary>`.
- RH8036 — the same shape on a private nested union (`<Param>` or `<ParamRef/>`), traced by the auditor as failing through the same path.
- RH8203 — `<inheritdoc/>` with `<PARAMREF name="x"/>` nested in `<summary>`; and the auditor's `<inheritdoc/>` + `<Param name="Circle">` counterexample.
- RH8108 — `Result<T>` with `<pAram name="x">`.
- Prefixed `<x:param name="x"/>` is in the class by the definition; the auditor probed it as already holding (RH8203).

Part (b) has no example of its own: every expansion failure known today is a parameter element, which (a) already refuses. Whether `param` content brought in only by `<include>` reaches the expansion and makes it throw is unverified (the analyzer test harness may not resolve the file); if the test implementer can construct it cheaply, it is a test for (b), otherwise (b) is covered by review of the code that calls the expansion.

## Output questions

_None._ The widened class is not reported, which is `main`'s output too; unions without a parameter element and with an expandable documentation are judged as answers 1A–5A decided.

## Internal choices

- The definition includes a prefixed tag name (`<x:param/>`), which matches what the code already refuses (`14-auditor.md`, *Hold*). Alternative: exclude prefixes; rejected because it would make the spec narrower than the code and the class rule ask for one comparison on the local name.
- Part (b) is stated as a rule-level requirement ("does not fail; reports nothing"), not as a mechanism; how the failure is caught is the implementer's. Alternative: name the expansion call or an exception type; rejected as an implementation detail.
- The refusal of a parameter element stays mode-independent (unchanged from `09-spec-author.md`); only (b) depends on the expansion actually failing, which happens under `Diagnose` only.

## Follow-up candidates

_None._
