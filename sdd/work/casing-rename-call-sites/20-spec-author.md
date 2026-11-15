Role: spec author
Commit: 7aad1ae
Verdict: spec written

Repair round 2, decisions 12 and 13 of `00-lead.md` (auditor findings 5 and 6 in `19-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the decisions, which this handover follows. Lane bug fix (clarification only): no change doc, no new requirement ID, `requirement-ids.txt` and `sdd/specs/README.md` unchanged. Rule pages unchanged: decision 12 only widens a withheld fix (covered by `general-notes.md`, "A diagnostic without a code fix"), and decision 13 restores `main`'s output.

## Changed requirements

- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified:
  - Interface paragraph (decision 12): the subject is now "a method, property, or event of the rename cascade (SHARED-CASING-007)", and the hierarchy is that of the type declaring that cascade member. A sentence states it holds for every cascade method, property, and event, not only the reported one, for example another implementer of the same interface member, the overridden base member, and the implemented interface member (worded as examples, since SHARED-CASING-007 leaves the cascade to the renamer). The other side now reads: no member of these interfaces has the new name for any cascade method, property, or event.
  - Interface paragraph (decision 13): a local function is not a member of a type and cannot implement an interface member, so it is not withheld by this paragraph, whatever interfaces its enclosing type implements; "local function" is added to the list of excluded kinds.
  - Why: "every type that inherits the renamed member" becomes "every type that inherits a member of the rename cascade"; one sentence explains why the other cascade members' hierarchies count (as for every other check of the rename); the rarity and "uncommon" sentences now say "the hierarchy of a type that declares a member of its rename cascade"; one sentence says a local function belongs to no interface mapping, so its fix stays offered.
  - Examples added, after the `Job` control: e1 (RH4103, `A`/`B` implementing `IProcessor.process`, `B` also implementing `IOther` with default `Process`; property/event variants named), e3 (RH4103, `Derived.process` overriding `Base.process`, `Sibling : Base, IOther`; property/event variants named), l2 (RH4104, local function `process` in `Worker : IProcessor`, before/after showing the fix; l1 `dispose` in an `IDisposable` type named in "The same holds").

## Examples for the tests

SHARED-CASING-009, the three new examples:

| Member | Rule | HEAD 07b4f2f (per `19-auditor.md`) | Required |
|---|---|---|---|
| e1 (fix on `A.process`; the example marks all three methods) | RH4103 | fixed | no fix |
| e3 (fix on `Derived.process`; also `Base.process`) | RH4103 | fixed | no fix |
| p1/p3 or v1 (property or event variant) | RH4114 / RH4102 | fixed | no fix |
| l2 (and l1) | RH4104 | no fix | fixed, exact output as in the example |

Note for the test implementer: in e1 the interface method `IProcessor.process` and `B.process` are reported too; the spec example marks every reported span. I did not run the fixtures myself; the HEAD column is the auditor's.

## Output questions

_None._ Decision 12 widens a withheld fix for a rare class under answer A; decision 13 restores `main`'s output.

## Internal choices

- The cascade relations in the paragraph are written as examples ("for example also for ...") rather than a closed list, because SHARED-CASING-007 defines the cascade as the renamer's own determination; alternative: an exhaustive list of relations.
- Local functions are excluded in the interface paragraph only; the other paragraphs of SHARED-CASING-009 (collisions, rebinding) still apply to them; alternative: none considered.

## Follow-up candidates

_None._
