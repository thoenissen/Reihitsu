Role: spec author
Commit: ab7af13
Verdict: spec written

Repair round 3 (lead decisions 23–25, answer 2; `22-auditor.md` finding #6, groups A and B). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions. No requirement ID was added; the code was not touched.

## Changed requirements

- RH2001-011 — The fix is withheld when the field name is already bound at a reference — changed (within this run):
  - Paragraph 3 restated as the refused class: a reference to the property that gives an anonymous-object member or a tuple element its inferred name withholds the fix, whether or not code reads it, except for a tuple element in two positions — a deconstruction target (nested included: `((Value, other), x) = source;`), and a tuple whose own conversion drops the name (no name, another name, or a non-tuple type such as `object` or `(int, int)?`; `return (Value, 1);`, `(int First, int Second) pair = …`, `Use((Value, 1))`, a deconstruction source, and a branch of `flag ? (Value, 1) : (2, 3)`, whose conditional type has no names — the offered side of the conditional qualifier, a20).
  - Withheld positions named: natural type (`var pair = (Value, 1);`), the governing tuple of a `switch` statement and expression, `is`, `==`, interpolation; a conversion on an enclosing expression (tuple nested in a tuple, also in a deconstruction source; cast; conditional branch or switch-expression arm whose branches or arms all carry the name); a target type that supplies the name.
  - Reason split: SHARED-FIX-003 only where code reads the name or for an anonymous object (`ToString()`); everywhere else a rare-class refusal (`AGENTS.md`, *Code-fix coverage*), and the tuple `switch` by the maintainer's decision. "under SHARED-FIX-003" removed from the paragraph's opening.
  - `Why:` tuple sentences rewritten accordingly.
  - New withheld example: tuple `switch` expression and `switch` statement (a01, a02). The heading of the offered tuple example now reads "a deconstruction target or the tuple's own conversion drops".
  - The two "(fourth paragraph)" pointers (¶2, ¶5) replaced by "the paragraph on a symbol named like the field below/above", since ¶3 is now several paragraphs and lists.
- RH2001-004 — coverage sentence's pointer reworded to "gives an anonymous-object member or a tuple element its inferred name, outside the two tuple positions that RH2001-011 names" — clarified.
- Change doc `sdd/changes/2026-11/rh2001-rename-binding.md` (status stays `Accepted`): the inferred-name *What* bullet and *Why* paragraph restated as above; *Visible output changes* gains the tuple `switch` expression with `main`'s output at `4838e9a` as Before (a01, from `22-auditor.md`'s `fx22-main.txt`), the statement in prose (a02), and one prose entry for the other rare tuple positions; *Decisions* gains answer 2 (A) and the rare-class refusal (decision 23); *Alternatives* gains "follow enclosing conversions and reads" and "keep the fix for the tuple switch". The "fourth paragraph" pointer there was updated too.

## Examples for the tests

RH2001-011: the new withheld tuple-`switch` examples (expression and statement). The paragraph-3 prose names every member of `22-auditor.md` groups A and B as withheld (a01–a05, a07, a08, a15, a16, b02, b03) and a18 and a20 as offered; a20 in the spec's form is `return flag ? (Value, 1) : (2, 3);` in a method returning `(int, int)` — the auditor measured the `var pair = …` form as offered on the head, so the test implementer should pin the spec's form or tell me if it differs.

## Output questions

_None._ The tuple `switch` is decided by answer 2; the rest is a rare-class refusal (decision 23).

## Internal choices

- Answer 2 is recorded in the change doc as "**A** — withhold the fix for the tuple `switch`", read from decisions 23 and 25 ("restates ¶3 … as the refused class with answer 2"). `00-lead.md`'s *Maintainer answers* section records only question 1, so the option text of question 2 is not on file; the lead should add it and correct the change doc's *Decisions* entry if A meant anything else.
- The other rare tuple positions are listed in *Visible output changes* in prose (one entry, Before = "offered, renames the reference" with `((_value, 1), 2)` from a07), not each with full Before/After blocks. Alternative: one block per position; rejected as noise for a rare-class refusal, matching how round 2 listed g5 in prose.
- The conditional and switch-expression arm qualifier ("whose branches or arms all carry the name") is stated with both sides (b02/b03 withheld, a20 offered), per FC-002; the switch-expression offered side (arms with different names) is not given as an example — it follows from the same "own conversion" rule.
- No rule page change: what is reported and how to fix it are unchanged in the normal case.

## Follow-up candidates

_None_ beyond the FC-015 unification already in `00-lead.md`. The `<returns>` doc of `TupleTypeKeepsElementName` (decision 25: cleaner) should match RH2001-011 ¶3's two offered positions; it currently says "the tuple, or a tuple it is nested in, is the target of a deconstruction, or the tuple is converted to a type whose element at that position has no name or another name", which is accurate for the code if "converted" is read as the tuple's own conversion.
