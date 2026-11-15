Role: spec author
Commit: 68612fc
Verdict: spec written

Repair round 1 (lead decisions 15–18; `10-auditor.md` findings #1–#3). The prompt carried no intended fix or hoped-for verdict beyond the lead's recorded decisions. No visible output is decided here: #1 is a fix against unchanged SHARED-FIX-003 (decision 15), #2 keeps `main`'s output (decision 16), #3 follows RH2001-011 paragraph 1 (decision 17).

## Changed requirements

- RH2001-011 — The fix is withheld when the field name is already bound at a reference — changed (within this run):
  - Paragraph 1: a `cref` in a documentation comment is named as a reference like any other (finding #3), with `<see cref="_value"/>` to an inherited member as the withheld case.
  - New paragraph 3 (finding #1, decision 15): the fix is withheld, under SHARED-FIX-003, when a reference to the property (`Value`, `this.Value`, `other.Value`) is a member of an anonymous-object creation without `Name =` or an element of a tuple expression without `Name:`, whether or not any code reads it. Both sides of the qualifier are stated: an explicit name (`new { Value = Value }`, `(Value: Value, 1)`) keeps the fix.
  - Former paragraph 3, now 4 (finding #2, decision 16): a member named like the field reached through the property's own value (`Value._value` → `_value._value`) keeps the fix, as on `main`. Paragraph 2's back-reference now says "fourth paragraph".
  - `Why:` gains one sentence on why an inferred name changes the anonymous or tuple type.
  - New examples at the end: the withheld cref; the withheld inferred anonymous member (`new { Value }` read as `pair.Value`), tuple element (`(Value, 1)` read as `pair.Value`), and `new { Value }.ToString()`; the offered explicit names (`new { Value = _value }`, `(Value: _value, 1)`); the offered receiver `Outer`/`Inner` case (`return _value._value;`, the auditor's measurement on `main`).
- RH2001-004 — coverage sentence: adds the inferred-name class to the cases RH2001-011 states — clarified (pointer only).

Change doc `sdd/changes/2026-11/rh2001-rename-binding.md`: *What* adds the cref to the "left as written" bullet, a new bullet for the inferred-name class under SHARED-FIX-003 with its explicit-name counterpart, and the receiver case to "The rename itself is unchanged". *Why* states that the inferred-name case is a fix against unchanged SHARED-FIX-003 and not a new visible-output decision, so *Visible output changes* is not extended. Status stays Accepted.

## Examples for the tests

- RH2001-011: the cref example (withheld), the three inferred-name examples (withheld; the auditor's members (a), (e), (f)), the explicit-name example (offered), and the `Outer`/`Inner` receiver example (offered, `main` output). The expected outputs of the offered examples come from the auditor's measurements on `main` 4838e9a; the test implementer confirms them.

## Output questions

_None._ Every added line states `main`'s output or a withholding demanded by unchanged SHARED-FIX-003 / RH2001-011.

## Internal choices

- The inferred-name condition is stated syntactically (no `Name =` / no `Name:`) and independent of whether the member is read, as decision 15 says. The alternative, withholding only when the member is read by name, would need a data-flow check and still miss the `ToString()` and type-identity cases.
- The receiver case is stated as offered (decision 16's first option) rather than as a withheld rare class. If the implementer cannot narrow the comparison cheaply, the lead's fallback applies and this sentence and its example flip to withheld; that is the spec author's to rewrite then.

## Follow-up candidates

_None_ beyond the unification follow-up already in `00-lead.md`.

Staged only `sdd/specs/analyzer/RH2001.md`, `sdd/changes/2026-11/rh2001-rename-binding.md`, and this handover; the parallel test implementer's test file is left unstaged.
