Role: spec author
Commit: a873151
Verdict: spec written

Architecture-finding repair (decision 10 of `00-lead.md`, auditor finding #4 in `13-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming decision 10, which this handover follows. Lane bug fix (clarification only): no change doc, no new requirement ID, `requirement-ids.txt` and `sdd/specs/README.md` unchanged. Rule pages unchanged: the change only widens a withheld fix, which `general-notes.md` ("A diagnostic without a code fix") covers.

## Changed requirements

- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified:
  - The interface paragraph is replaced. The fix is withheld when a renamed method, property, or event would get the name that a member of an interface already has, where the interface is implemented by the declaring type or by any type in the solution derived from it (for an interface as declaring type, also by its implementing types and inheriting interfaces); "implements" covers listed and inherited interfaces. It holds whether or not the mapping would change, whatever implements the member today (inherited member, default implementation, base type that re-lists the interface), and without any reference.
  - The counter-statement "when the type does not list the interface itself … does not withhold" is dropped. The other side is now: no member of those interfaces has the new name — the type is outside every implementing hierarchy, or its interfaces have no such member. Renamed fields, parameters, locals, types, and type parameters are explicitly outside this paragraph.
  - Why: the interface sentence gains the reason for refusing by name (the mapping depends on every derived type in the solution, default implementations, and re-listed interfaces; comparing all mappings is not a cheap check; the class is rare). The "uncommon" sentence now reads "the name of a member of an interface implemented in its type's hierarchy".
  - Examples: the a3 example's last sentence now says the `class Derived : Base` variant is withheld too. Two examples added: b1 (derived type lists the interface; b2, b4, property, event named in "The same holds") and a control (unrelated `Job : System.IDisposable` with `process()`, fixed to `Process`).

## Examples for the tests

SHARED-CASING-009, the a3 example (its `class Derived : Base` variant flips), the two new examples. Run with `scripts/apply-fix.sh RH4103` on HEAD (a873151), LF and CRLF identical:

| Member | HEAD | Required |
|---|---|---|
| b1 (`Base.process`, `Derived : Base, IProcessor`, `Root.Process`) | fixed | no fix |
| control (`Job : System.IDisposable`, `process()`, unrelated `Worker : IProcessor`) | fixed | stays fixed |

b2, b4, b5 (RH4114), b6 (RH4102) not run here; `13-auditor.md` reports them fixed on df00d3b. The existing controls `FixRenamesAMethodThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface` (RH4103) and `FixRenamesAPropertyThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface` (RH4114) now contradict the spec and must assert no fix, as decision 10 states.

## Output questions

_None._ The change widens a withheld fix for a rare class under decision 10 and the maintainer's answer A.

## Internal choices

- The name match ignores member kind (any interface member with the new name withholds), as decision 10 states "a member of any interface"; alternative: match only members of the same kind.
- When the declaring type is an interface, its implementing types and inheriting interfaces count as derived, so the paragraph has one reading for every declaring type; alternative: leave interfaces as declaring types to R2. If the implementer finds this costs more than a cheap check, the lead may narrow it to classes and structs.
- Fields are left out (a field cannot implement an interface member), matching decision 10's "method, property or event".

## Follow-up candidates

_None._
