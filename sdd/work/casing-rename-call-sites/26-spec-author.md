Role: spec author
Commit: 03edc5c
Verdict: spec written

Repair round 3, decisions 14 and 15 of `00-lead.md` (auditor findings 7 and 8 in `25-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the decisions, which this handover follows. Lane bug fix (clarification only): no change doc, no new requirement ID, `requirement-ids.txt` and `sdd/specs/README.md` unchanged. Rule pages unchanged: decision 14 restores `main`'s output for type renames, and decision 15 records a withheld fix for a rare class, which `general-notes.md` ("A diagnostic without a code fix") already covers.

## Changed requirements

- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified:
  - Interface paragraph (decision 14): the two closing sentences (local function excluded; list of excluded kinds) are replaced by an allow-list. The paragraph checks only the cascade members that can implement an interface member by their name: properties, events, and ordinary methods ("methods declared with a name of their own"). Every other symbol of the cascade is named as not checked: constructor, static constructor, finalizer, operator, conversion operator, accessor, explicit interface implementation of a method, local function, field, parameter, local variable, type, type parameter. It states that a renamed class, struct, record, or record struct is therefore not withheld although its cascade holds its constructors, and that a local function is not withheld.
  - Interface paragraph (decision 15): a symbol that is not checked itself is still withheld when its cascade holds a property, event, or ordinary method that the paragraph withholds, for example a positional record parameter through the property the compiler declares for it. This replaces the old "A renamed … parameter … is not withheld by this paragraph".
  - Why: the local-function sentence is replaced by the reason for the allow-list — only a property, event, or ordinary method can start to implement an interface member by its name; an accessor does so only through its property or event, which the cascade holds; an explicit implementation's interface member is in the cascade, so its hierarchy (which holds the implementing type) is checked anyway; the allow-list keeps the check independent of what the renamer cascades to; a positional record parameter is withheld like its property.
  - Examples added after the RH4104 local-function example:
    - RH4002 (t1): `public class processor : IHasProcessor` with `IHasProcessor.Processor`, before and after, fix offered (`Processor`). "The same holds" names the derived-type variant (t2) and RH4003, RH4010, RH4011 (t5, t4, t7).
    - RH4120 (finding 8): `public record Person(string name) : INamed;` with `INamed.Name => "default"`, no code fix offered, with the run-time difference (`"default"` today, `"x"` after); control: fixed when `INamed` has no member `Name`.

## Examples for the tests

SHARED-CASING-009, the two new examples:

| Member | Rule | HEAD 27789ab (per `25-auditor.md`) | Required |
|---|---|---|---|
| t1 (own type) | RH4002 | no fix | fixed, exact output as in the example |
| t2 (derived type lists the interface), or t5/t4/t7 | RH4002 / RH4003 / RH4010 / RH4011 | no fix | fixed |
| `Person(string name) : INamed` | RH4120 | no fix | no fix (`Verify(src, src)`), tags SHARED-CASING-009 and SHARED-FIX-003 |

I did not run the fixtures myself; the HEAD column is the auditor's.

## Output questions

_None._ Decision 14 restores `main`'s output; decision 15 keeps a withheld fix for a rare class under unchanged SHARED-FIX-003.

## Internal choices

- "Ordinary method" is written out as "methods declared with a name of their own", and an explicit interface implementation of a method is listed among the kinds not checked (it is not an ordinary method), with the reason in Why that the implemented interface member's hierarchy covers its type; alternative: leave explicit implementations unmentioned. The implementer's allow-list of `MethodKind.Ordinary` matches this.
- Properties and events are checked whatever their kind (explicit implementations included), since decision 14 names them without a further filter; alternative: none considered.

## Follow-up candidates

_None._
