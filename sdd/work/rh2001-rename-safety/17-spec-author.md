Role: spec author
Commit: 3593829
Verdict: spec written

Repair round 2 (lead decisions 19 and 20; `16-auditor.md` findings #4 and #5). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions. No requirement ID was added.

## Changed requirements

- RH2001-011 — The fix is withheld when the field name is already bound at a reference — changed (within this run):
  - Paragraph 3 (decision 20): the inferred-name withholding now applies only where the inferred name is part of a type: every anonymous-object member without `Name =` (never converted), and a tuple element without `Name:` whose tuple, where it is used, has a type with an element of that name at that position (its natural type, `var pair = (Value, 1);`). Named as keeping the fix: the target of a deconstruction (`(Value, Name) = (value, name);`, `(Value, other) = pair;`, nested tuples included), the source of a deconstruction (`(value, count) = (Value, 1);`, `var (value, count) = (Value, 1);`), and a conversion to a type whose element has no name or another name (`return (Value, 1);` to `(int, int)`, `(int First, int Second) pair = (Value, 1);`, `Use((Value, 1))` with a `(int, int)` parameter).
  - New fifth paragraph (decision 19): the fix is withheld for two rare shapes: a reference to the property through another construction of its generic containing type (`other.Value` with `Sample<int> other`, `new Sample<TOther> { Value = Value }`), with `Sample<T> other` named as keeping the fix; and a member named like the field reached through the property's own value whose receiver the renamer also qualifies because an inner-scope symbol is named like the field (`var _value = 1; return Value._value + _value;`), with the same receiver without such a symbol named as keeping the fix. Paragraph 4 was checked: it no longer claims that q1 keeps the fix (its receiver clause says "changes only by the rename", and paragraph 5 now states the qualified receiver), so its text is unchanged.
  - `Why:` corrected for tuples (natural type keeps the name; deconstruction and conversion drop it) and given one sentence on the rare shapes (rare; RH4115 reports the local).
  - Examples: offered t1 (deconstructing constructor, `main`'s output with `readonly` fields) and t2 (`return (Value, 1);`), inserted before the receiver example; withheld g2, g5, and q1 at the end of the section.
- RH2001-004 — coverage paragraph's pointer to RH2001-011 extended to the narrowed inferred-name case and the two rare shapes — clarified.
- Change doc `sdd/changes/2026-11/rh2001-rename-binding.md` aligned: the inferred-name bullet and *Why* are narrowed to tuples in their natural type; a bullet, a *Why* paragraph, two *Visible output changes* entries (g2 with g5 in prose, q1, before = `main`'s output from `16-auditor.md`), a decision, and two rejected alternatives (compare `OriginalDefinition`; withhold every inferred tuple name) were added. Status left `Accepted`.

## Examples for the tests

RH2001-011: the two new offered tuple examples (t1, t2) and the two new withheld examples (g2 and g5; q1). The paragraph-3 prose also names t3, t4, t5, t6 and `var (value, count) = (Value, 1);` as offered, with `main`'s output.

## Output questions

_None._ Decision 19 is a refusal of a rare class; decision 20 restores `main`'s output.

## Internal choices

- A tuple converted to a type that has an element of the inferred name at that position, such as `(int Value, int) pair = (Value, 1);`, stays withheld: paragraph 3 keys on "a type with an element of that name", following decision 20's "not converted to a type without the name" and the auditor's converted-type check. The alternative (offer it, because the name then comes from the target) is equally rare; the implementer's check decides, and the spec should follow if it differs.
- The rare-shape refusal is listed under *Visible output changes* in the change doc, marked as a rare-class refusal under `AGENTS.md`, *Code-fix coverage*, rather than as a maintainer answer; the doc stays `Accepted` on decision 19. Alternative: omit it from that section, as the inferred-name bug fix is omitted.
- The auditor's traced `foreach ((Value, var x) in …)` is not named: a `foreach` must declare its iteration variables (CS8186), so it is not valid input.
- The reserved-name hint (`(Rest, 1)`) is not specified; a converted-type check offers it as on `main`, and the spec's "type with an element of that name" wording already gives that result.
- No rule page change: what is reported and how to fix it are unchanged in the normal case.

## Follow-up candidates

_None_ beyond the FC-015 unification already recorded in `00-lead.md`.
