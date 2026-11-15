Role: spec author
Commit: a10d3e34c0e5edb9261727571ee28be1731fabdb
Verdict: change doc written

The prompt carried no intended fix or hoped-for verdict beyond the maintainer's recorded answer B, which is binding.

Files:
- `sdd/changes/2026-11/rh2001-rename-binding.md` (Status: Draft; lists the rewritten class f02, f07, s16, e1 as visible output changes, records answer B under *Decisions*; the run-time class is named as a bug fix against SHARED-FIX-003 and not listed).
- `sdd/specs/analyzer/RH2001.md`: RH2001-011 added; RH2001-004 coverage paragraph points to it; `SHARED-CASING-009` added to *Uses*.
- `sdd/specs/requirement-ids.txt`: `RH2001-011`.

## Changed requirements

- RH2001-004 — The fix replaces the property with a private field — changed (coverage: a field name already bound at a reference, or a renamed property reference that would bind to another symbol, is outside it; RH2001-011 withholds).
- RH2001-011 — The fix is withheld when the field name is already bound at a reference — added. Paragraph 1: the "rewritten" and "left as written" cases of SHARED-CASING-009 for references to symbols other than the property, in any document including another `partial` part; only those paragraphs of SHARED-CASING-009 apply. Paragraph 2: a renamed property reference that would bind to an implicitly typed lambda parameter, an `out` variable of the same expression, or a primary-constructor parameter (SHARED-FIX-003). Paragraph 3: the qualifier's other side, cases that keep the fix.

## Examples for the tests

RH2001-011, every example (withheld: f02 inherited unqualified; f05/t4 combined `this._value` + `other._value`; f07 extension method; f09 lambda, s06 `out var`, s13 primary constructor; offered: f01 parameter with `this.` qualification, f06 inherited method). The 01-skeptic tables give the further rows (e2, f08, s01, s12, t1, s16, e1 withheld; f03, f04, s02, s05, t3, s10, f10 offered) and FC-024 asks for one cross-document `partial` case; the test implementer derives those from paragraphs 1–3. Note f01 and f06 results carry CS0108 (warning), which SHARED-FIX-003 does not count.

## Output questions

_None._ Answer B covers the rewrite class; the run-time class is demanded by unchanged SHARED-FIX-003; the offered cases keep `main`'s output.

## Internal choices

- New requirement RH2001-011 instead of widening RH2001-006 (alternative: rewrite RH2001-006 to cover inherited and inner-scope names). Keeps RH2001-006's existing tests and wording intact.
- Refer to SHARED-CASING-009's binding paragraphs only, not the whole requirement (alternative: adopt SHARED-CASING-009 entirely). Its interface-member paragraph would withhold a fix for an interface member named `_value`, a visible change nobody was asked about; its compile-error paragraph duplicates SHARED-FIX-003.
- The spec keeps the `this.`-qualified inner-scope cases (f01, f03, f04, s02, s05, t3, s10) offered, as on `main`. The skeptic's "cheap" alternative of checking every visible name would withhold them; that is a visible change outside answer B, so an implementation that cannot keep them offered with a cheap check is an escalation, not a free choice.
- Rule page `documentation/rules/RH2001.md` unchanged: a withheld fix in a corner case, covered by `general-notes.md`.

## Follow-up candidates

_None_ beyond the lead's recorded FC-015 follow-up (one owner for the rename-safety decision).
