Role: spec author
Commit: f4689af
Verdict: spec written

Repair round 1. It covers Decisions #12, #13, #14, and #15 in `00-lead.md`, from audit 08 findings #1 to #4. My prompt carried no intended fix or verdict beyond those decisions.

No requirement ID was added. RH2001-010 is extended instead of getting a sibling, so `sdd/specs/requirement-ids.txt` is unchanged. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 of 51 pass. No rule page changes, because every change is a withheld fix or a kept fix.

## thoenissen/Reihitsu#1666

### Changed requirements

- RH2001-010 — now titled "The fix is withheld for a conditional group in front of the first modifier or behind the property that does not wrap whole members" — changed:
  - Decision #12: the no-nesting condition is now "No group inside it": no other conditional group lies inside one of the group's branches. The half that refused a group lying inside another group's branch is dropped. A new paragraph and the `Why:` state that a group inside another group's branch is decided by its own branches. The enclosing group is read only through its own directive in front of the first modifier or behind the property.
  - Decision #13: a second MUST NOT. For a property **without an initializer**, with or without a modifier, a directive of a conditional group in the leading trivia of the token behind the property's last token withholds the fix when the group does not wrap whole members. That token is the next member's first token or the type body's closing brace. The test is the same two conditions on the same parse. A property with an initializer ends in `;`, so a directive behind it does not withhold the fix by this requirement (FC-002, both sides have examples). "An initializer" is added to the list of branch contents that do not read as whole members. The active branch of a group behind the property is not parsed.
- RH2001-004 — clarified: its coverage sentence points to RH2001-010 for a group behind a property without an initializer too.
- SHARED-FIX-009 — clarified: the RH2001 mention names the excluded branches "in front of a property's first modifier or behind the property".

### Examples for the tests

The new RH2001-010 examples, all appended at the end of the section:
- Initializer `= 5;` in an excluded branch behind a property without an initializer (audit finding #2 input): no code fix is offered.
- A group of a whole member behind a property without an initializer: fix kept, output in the spec. Measured on head with `scripts/apply-fix.sh RH2001`, LF and CRLF: fixed.
- `#if DEBUG static #endif` behind a property **with** an initializer, in front of `_other`: fix kept, output in the spec. Measured the same way: fixed.
- A file wrapped in `#if NET` around a group of a whole member in front of the first modifier (audit finding #1 input): fix kept, output in the spec. That output is `main`'s, as the auditor measured on `main`.
- `#if !RELEASE` around `_first`, the group `#if DEBUG … #endif`, and the property, with its `#endif` behind the property: no code fix is offered, because the enclosing group's `#endif` stands at the token behind the property and that group holds another group. The text also names the nested group in front (`#if DEBUG` around `#if TRACE`): no code fix.

Effect on existing tests (facts for the test implementer, not a prescription):
- `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInsideAnotherGroupInFrontOfTheFirstModifier` has the same input as the last example. Its expected value (no fix) still holds, but the reason is now the group behind the property, not the "lies inside" half. Its name and summary describe the dropped reason.
- `VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier` is unchanged. It is the "No group inside it" condition.

### Output questions

_None._ Decision #12 keeps `main`'s output. Decision #13's harmful input is withheld because unchanged SHARED-FIX-003 demands it. The remaining new withholdings are rare classes, listed in the change doc.

### Internal choices

- **The enclosing-group variant stays withheld.** This is the class-body variant of audit finding #1: `#if !RELEASE` with its `#endif` behind the property. Decision #13's "same shared reading" puts the no-group-inside condition on the group behind the property, so this variant is still withheld, now through the directive behind the property. On `main` it was fixed. The file-wrapped variant, which is the common shape, is kept. I list the class-body variant as a rare-class withholding.
  - Alternative: for both positions, refuse only a group that lies inside an *excluded* branch. The partial-reading risk exists only there. That would keep `main`'s output for this variant too, but it needs a third mode of the shared helper's nesting check. The lead decides.
- **The behind check applies only to a property without an initializer.** This is one cheap check (`Initializer` is null), and with it the group behind a property that ends in `;` keeps `main`'s fix.
  - Alternative: apply the check to every property. That withholds more fixes that change no build.
- **RH2001-010 is extended rather than given a sibling RH2001-011.** Both positions use one test, the tests are already tagged RH2001-010, and the ledger stays unchanged.
  - Alternative: RH2001-011 for the position behind the property.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1667

### Changed requirements

- SHARED-ORDER-008 — changed (Decision #15), in three places:
  - The layout sentence: "in front of its body's opening brace — or, for a declaration that ends in `;` instead of a body, in front of that `;`".
  - "a declaration of the type" now names one that ends in `;`, such as `public partial class C;`.
  - The first list item: "up to and including the opening brace, or, for a declaration that ends in `;` instead of a body, up to and including that `;`".

### Examples for the tests

- SHARED-ORDER-008, a new example placed in front of the A5 example: the audit finding #4 input (`#if NET [StructLayout(LayoutKind.Sequential)] #endif public partial class C;` plus a declaration with the fields). No fix for RH7109.
- The text also names the same case for a `partial` struct, record, and record struct declared with `;`, and for a directive behind the first token (base list, in front of the `;`). The control without a directive is the existing example (fix kept).

### Output questions

_None._ The withholding is demanded by unchanged SHARED-FIX-003.

### Internal choices

_None._

### Follow-up candidates

_None new._ A6 stays as recorded.

## thoenissen/Reihitsu#1665

Nothing to do in this round. Findings #5 and #6 are the implementer's (Decision #16).

## Whole run

Change doc `sdd/changes/2026-11/conditional-group-fix-refusals.md`. Status stays `Accepted`.

- *What*:
  - The RH2001 bullet names the position behind the property and the dropped nesting half.
  - The partial bullet names declarations that end in `;`.
- *Why*: names the `= 5;` input and the `;`-bodied partial among the inputs that SHARED-FIX-003 demands be withheld.
- *Visible output changes*: one sentence points to the new section. The three maintainer-answered cases are unchanged. The "kept" line adds two cases: the file-wrapped group, and groups behind the property that keep the fix.
- New section `## Rare-class withholdings`, outside *Visible output changes* (Decision #14). It lists the auditor's four classes, each decided by the rare-class rule and not by a maintainer answer:
  - `static` in front of the first modifier;
  - text that is not code;
  - a group that holds another group in front of the first modifier;
  - a conditional base list on another `partial` declaration.

  It adds the two classes this round's reading creates:
  - an enclosing group with a nested group whose directive stands behind the property;
  - a non-member branch behind a property without an initializer, such as a modifier or an attribute list of the next member.
- *Decisions*, "RH2001 tells whole members …": states why the "lies inside" half is not applied, and the check behind the property.
