Role: spec author
Commit: 464fefa
Verdict: spec written

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named decisions 6–8 of `00-lead.md`, which this handover follows. Lane bug fix (clarification only): no change doc, no new requirement ID, `requirement-ids.txt` and `sdd/specs/README.md` unchanged. Rule pages unchanged: every addition is a withheld fix, which `general-notes.md` ("A diagnostic without a code fix") covers.

## Coverage check of the text from `02-spec-author.md`

- **Decision 8 (a1, a2)** — not covered. The text said only that a cascade reference "is renamed with it and is not such a reference", which reads as allowing any rewrite of it. Added (see below).
- **Decision 7 (a3)** — not covered. Both bullets of the second paragraph are about a *reference* whose binding changes; in a3 no source reference changes binding (`((IProcessor)x).Process()` binds to `IProcessor.Process` before and after), only the interface mapping does. Added (see below).
- **Decision 6 (c4, c6, c7, d1)** — covered in substance: the field `value` and the class `N.TItem` are symbols outside the cascade, and the renamer rewrites their references (type qualification, `this.`, `nameof` qualification), which the *Rewritten* bullet withholds. The text never said that a reference is matched to the cascade by what it refers to rather than by spelling, which is the exact point the implementation got wrong; added one sentence and an example so the boundary is explicit.

## Changed requirements

- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified:
  - The "cascade reference" sentence now states that the symbol a reference refers to decides, not its spelling, with the c4 (`value` field vs. the other part's parameter `value`) and d1 (class `TItem` vs. the other part's type parameter `TItem`, left as it is by SHARED-CASING-007) cases.
  - New normative paragraph: no fix when the renamer would rewrite a reference to a cascade symbol beyond renaming it and qualifying its name (extension call turned into a static call, cast argument). Other side: a reference renamed, or renamed and qualified with `this.`/`base.`/type/namespace, does not withhold (the existing RH4002 test `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`).
  - New normative paragraph: no fix when the rename would change which member implements an interface member — the renamed method, property, or event would implicitly implement a member of an interface its type lists among its own base types, implemented today by another (e.g. inherited) member. Other side: when the type does not list the interface itself, the current implementer keeps implementing it and this does not withhold.
  - The closing sentence now reads "no code other than the declarations of the rename cascade, the names of its references, and the qualifiers in front of those names".
  - Why gains the reasons for the three points, and the "uncommon" sentence names the re-implementation case.
  - Four examples added after the hiding example.

## Examples for the tests

SHARED-CASING-009, the four new examples. Each was run with `scripts/apply-fix.sh` on `main` (6ab3d75, temporary worktree, removed) and on HEAD, under LF and CRLF, with identical results:

| Member | Example | `main` | HEAD (464fefa) | Required |
|---|---|---|---|---|
| c4 | RH4105, partial `Get(int value)` / `Get(int Value)`, static field `value` | fixed, `return total + Counter.value;` | (not run on HEAD; auditor: fixed) | no fix |
| c6, c7 | same, instance field / `nameof(value)` (named in "The same holds") | auditor: `this.value` / `nameof(Counter.value)` | auditor: fixed | no fix |
| d1 | RH4121, `Count<TItem>()` / `Count<item>()`, class `N.TItem`, `TItem.Size()` (named in the c4 example) | fixed, `N.TItem.Size()` | (auditor: fixed) | no fix |
| a1 | RH4103, `toList` in `N`, `values.toList()` in `N.Inner` with `using System.Linq;` | fixed, `Extensions.ToList(values)` | no fix | no fix |
| a2 | RH4103, `write(object)` next to `Write(int)`, `box.write(1)` | fixed, `box.Write((object)1)` | no fix | no fix |
| a3 | RH4103, `Derived : Base, IProcessor` with `process()`, `Base.Process()` implements `IProcessor` | fixed | fixed | no fix |
| a3 control | `WorkerDerived : WorkerBase` (interface not re-listed), `work()` | fixed | fixed | stays fixed |

The control for a1/a2 (a qualified cascade reference stays fixed) is the existing RH4002 test named above; the spec cites it in the a2 example.

## Output questions

_None._ All additions withhold a fix for a rare class under decisions 6–8 and the maintainer's answer A.

## Internal choices

- The new classes are stated as two further MUST NOT paragraphs of SHARED-CASING-009 rather than a new requirement (consistent with decision 4); alternative: SHARED-CASING-013.
- The interface case is stated by its effect (which member implements an interface member), with "its type lists the interface among its own base types" as the explicit qualifier and the not-listing side as the counterpart; alternative: a closed list of hiding shapes.
- A `dynamic` receiver, reflection, and serialization (calls by name that no static reference shows) are not written into the requirement; every casing rename changes them, and the auditor dismissed them as outside the issue.

## Follow-up candidates

_None._
