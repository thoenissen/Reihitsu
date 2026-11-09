Role: spec author
Commit: c258219
Verdict: spec written

This is an amendment for the lead's Decisions #5, #7, and #8. My prompt carried no intended fix or verdict beyond those decisions.

## Changed requirements

- SHARED-ORDER-008 — Only conditional groups of whole units keep a member move — changed (Decision #5, A5 / m06):
  - Item two of the layout list now also counts a group in front of a declaration's first token when its excluded branch reads as whole members and one of them is another declaration of the type. "Another declaration of the type" means a type declaration at the outermost level of the branch with the type's name, the same type parameter count, the `partial` modifier, and an attribute list.
  - The exemption sentence ("a whole type declared in front of the reordered one") now reads "a whole type of another name", plus the exception. The test it describes adds the name, type-parameter-count, modifier, and attribute-list check on the parse that already exists.
  - "every other declaration of it" now reads "every other declaration of it that the analyzed build compiles".
  - The `Why:` paragraph names the exception.
  - New example (m06), placed before the example of a group in front of the spans. Its counterparts keep the fix: the same branch without the attribute, and a declaration of another name.
  - Decision #8 (A6): one short paragraph states that a `partial` declaration that only excluded code writes anywhere else is not read, and that the move stays. It names the reason: no cheap check exists, and the only blanket refusal would hit common code. It records this as a decided gap against SHARED-FIX-003. Before this change, the spec read as if A6 were covered: "every other declaration" with no build qualifier, plus "The attributes of every declaration … count" in the `Why:`.

Nothing else repeats the exemption: the RH7102, RH7103, RH7109, RH7309, and SHARED-FIX-009 texts refer to SHARED-ORDER-008 as a whole. No rule page changes.

Change doc `sdd/changes/2026-11/conditional-group-fix-refusals.md`:
- The *What* bullet on partial types gains the A5 reading.
- *Why* lists m06 among the issue inputs. It is withheld because SHARED-FIX-003 demands it, so it is not under *Visible output changes*.
- The "unchanged and kept" line gains an excluded partial declaration without an attribute list.
- *Decisions* gains the A5 paragraph and an A6 section.
- *Alternatives* gains two rejected options: refusing any excluded partial declaration, and counting every partial type as layout-relevant.

Decision #7: the change doc has no section that says how the result differs from the issues. It already shows #1665's narrowing (RH7103 static move kept) and #1666's (the blanket refusal is rejected). So I added nothing. The PR description is the place for the narrowed goals.

`scripts/test.sh --project architecture --filter Sdd`: 50 of 51 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which wants a test for RH2001-010 and is unchanged from 01.

## Examples for the tests

- SHARED-ORDER-008, the new m06 example: no fix for RH7109, LF and CRLF. Counterparts that keep the fix: the same branch without `[StructLayout(...)]`, and with `public partial class D`.

## Output questions

_None._ The A5 reading keeps `main`'s output for every input it does not have to refuse.

## Internal choices

- The A5 check counts only an excluded `partial` declaration that has an attribute list. The alternative was to count every excluded `partial` declaration of the type, which is how Decision #5 reads if taken literally. That would also withhold fixes `main` offers for a declaration without attributes, which cannot give a class a layout. That is a visible change, and it would need an output question. The lead should confirm this reading.
- The match is on the type's name, its type parameter count, and `partial`, at the outermost level of the branch's parse. The alternative was to match any type declaration in the branch, which would refuse whole types of other names (`main` fixes those).

## Follow-up candidates

_None new._ A6 stays as recorded in `00-lead.md`. The spec now records it as a decided gap, so the follow-up issue can cite SHARED-ORDER-008 for it.
