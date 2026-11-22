Role: spec author
Commit: 41f7eee
Verdict: spec written

Follow-up stage after batch 1. The maintainer answered every question as recommended (`00-lead.md`, *Maintainer answers*). Both change docs are `Status: Accepted`. The prompt carried no intended fix, cause, or verdict to ignore.

No rule page changes. Every change narrows or widens a withheld fix, and `general-notes.md` already covers that.

`scripts/test.sh --project architecture`: 197 of 198 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered` for SHARED-ORDER-011, which the test implementer's tags close. While editing I found two stale `Uses` entries and fixed them: RH7102 now cites SHARED-FIX-003 again, and `code-fixes.md` drops SHARED-ORDER-007.

Every example whose output equals `main`'s was run with `scripts/apply-fix.sh` on 41f7eee, and each one matches: RH7103 with a group behind the spans; RH7103 with a group in the body in front, static move; RH7107 with `#if DEBUG` in a crossed `set` body; RH7109 with the whole-class wrap and no `#else`; RH7309 with `#if DEBUG` in a block that keeps its place. The skeptic had already checked the 970-3 and 970-4 outputs as `main`'s output for the same input without the pragma lines. Examples whose output changes were checked to be reported on `main`; their new "no code fix" or "fix" result is not built yet.

# thoenissen/Reihitsu#970

Change doc: `sdd/changes/2026-11/ordering-move-warning-nullable-pairs.md`, Accepted. *Decisions* records 970-1 A, 970-2 A, 970-3 B, and 970-4 B. It adds a decision that one rule covers member, accessor, and region-block moves, and one on how this issue interacts with #1668. *Visible output changes* gains two sections: a closed pair in front, and RH7309 with a pair in a block that keeps its place.

## Changed requirements

- SHARED-ORDER-011 — changed, both answers applied:
  - *Nothing in front* is now *Nothing open in front* (970-3 B). The directives of the pair's kind in front of the spans must form consecutive pairs as *Pairs* states, each keeping *One branch*. They need not lie inside one declaration. An unclosed one refuses, whatever IDs it names (970-1 A).
  - For RH7309 (970-4 B), *Inside one declaration* covers the members in any region block or in code between two blocks, whether or not the block moves. "In front" means in front of the type's first region block. This fixes the skeptic's wording finding as well.
  - New example: a closed pair in front (RH7103, fix offered).
  - The split-pair example is now stated for RH7107. A sentence notes that for member moves, SHARED-ORDER-008 already refuses the pair's conditional directives in the spans.
  - The CS1591 example's reason now reads "no `restore` closes".
- Intro paragraph of `ordering-move-safety.md` — clarified: it names SHARED-ORDER-011 next to 007 and 008 as applying to RH7309.
- RH7309-005 — changed: new example, a `#pragma warning` pair in `IComparable`, a block that keeps its place (fix offered, 970-4 B).

## Examples for the tests

- SHARED-ORDER-011, all examples:
  - pair in a crossed body (RH7103): fix;
  - lone directive, and `restore CS0219`: no fix;
  - unclosed `#pragma warning disable CS1591` in front, active and in an excluded branch: no fix;
  - closed pair in front (RH7103): fix;
  - split pair (RH7107): no fix;
  - group wholly between the pair (RH7107): fix;
  - `#nullable disable`/`restore`: fix; `#nullable disable`/`enable`: no fix.
- RH7309-005: pair in a moved block (`IDisposable`): fix. Pair in a kept block (`IComparable`): fix.
- The skeptic's candidates 1–13 in `02-skeptic.md` still stand. Under the answers, candidate 10 (pair split across `#if`) is pinned for RH7107 and RH7108; the RH7103 variant is refused by SHARED-ORDER-008. Add an RH7107 case with a closed pair in a method above the property (fix), because for accessors that pair is "in front".

## Existing tests that pin behavior that now changes

From the skeptic. Each flips from refusal to fix:

- `tests/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs` `MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective`
- `tests/Reihitsu.Core.Test/AccessorOrderingUtilitiesTests.cs` `MoveRangeContainsDirectivesReturnsTrueWhenCrossedAccessorBodyContainsDirective`
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7107PropertyAccessorsMustFollowOrderAnalyzerTests.cs` `BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix`

The tests the skeptic listed as refusing neighbours stay green. Among them, `VerifyNoCodeFixWhenRegionContainsPragmaWarningDirective` and `VerifyNoCodeFixWhenRegionContainsNullableDirective` (RH7309 trivia tests) need a check: if their directive is a closed pair inside one member, they now flip.

## Output questions

_None._

## Internal choices

- "Nothing open in front" reuses *Pairs* and *One branch* over all text in front of the spans and does not require the pairs to lie inside one declaration. A pair around an earlier type is accepted as well. The alternative, pairs in front only inside one declaration, gives no safety: either way a closed pair leaves the project's setting behind it.
- The split-pair example moved from RH7103 to RH7107, as the skeptic recommended.

## Follow-up candidates

_None._

# thoenissen/Reihitsu#1668

Change doc: `sdd/changes/2026-11/ordering-move-refuses-conditional-groups.md`, Accepted. *Decisions* records 1668-1 B and 1668-2 A, and states how "in front of the class declaration" is read and what the exception checks. *Visible output changes* gains the skeptic's group-before-class case, the `#else` wrap, RH7102, and RH7309.

## Changed requirements

- SHARED-ORDER-008 — changed and retitled "A conditional directive in the moved or the crossed span refuses a member move".
  - **Spans.** Every `#if`, `#elif`, `#else`, or `#endif` in the moved or crossed span refuses RH7102, RH7103, RH7109, and RH7309, whatever its group holds. RH7309's spans are as on `main`.
  - **Layout** (1668-2 A). Three places make the type count as one whose field order sets its layout:
    - a conditional directive in a declaration of the type, behind its first token, up to `{` or `;` (unchanged);
    - one in the leading trivia of a declaration's first token. Exception: an `#if`…`#endif` with no `#elif` or `#else`, whose `#if` stands there and whose `#endif` stands in the leading trivia of the token behind the declaration's last token;
    - one in the reordered body in front of the spans.

    The first two places apply to every declaration of a `partial` type, as on `main`. The whole-unit test, the `field` and backing-field clauses, and the excluded-member parse are gone. The decided gap for `partial` declarations written only by excluded code is kept.
- SHARED-ORDER-007 — changed:
  - no member of excluded code is counted;
  - the excluded-code paragraphs and examples are removed;
  - the backing-field rule is kept;
  - RH7102 is never refused by it.
- SHARED-ORDER-006 — changed. The conditional-pair example and the *Why* are now for RH7107, an `#if DEBUG` in a crossed `set` body, per the skeptic's *Why* finding. A sentence states that a member move keeps only region pairs and SHARED-ORDER-011 pairs. The `#if false` leading-trivia example is gone; SHARED-ORDER-008 names that case as refused.
- RH7102-006 — changed. A conditional directive anywhere in the spans withholds the fix. The three excluded-code examples are replaced by one: a group in front of the constant, no fix.
- RH7103-007 — changed. The excluded-code sentence now reads that a static member reorders no instance field. The struct example's reason is now SHARED-ORDER-008.
- RH7309-005 — changed. A conditional directive in a moved block, or in code between blocks that a moved block crosses, withholds the fix, also a balanced pair. The former "balanced `#if` in `IDisposable`" example flips to no fix. A new example keeps the fix for the same pair in `IComparable`, a block that keeps its place.
- SHARED-FIX-009 — clarified: its excluded-members bullet and example now point to RH2001-010, not SHARED-ORDER-007.

## Examples for the tests

- SHARED-ORDER-008:
  - whole statements in a crossed body (RH7103): no fix;
  - whole members in the moved member's leading trivia: no fix;
  - the same with `#if false`, whole attribute lists, whole accessors, and members of a crossed nested type: no fix;
  - group behind the spans: fix;
  - conditional attribute list on the type (RH7109): no fix; the RH7103 static move: fix;
  - whole type of another name in front, `#endif` on `C`'s first token (RH7109): no fix;
  - whole-class wrap with no `#else` (RH7109): fix; with `#else`: no fix;
  - the two `partial` examples: no fix; the `partial` example without directives: fix;
  - group in the body in front (RH7109): no fix, with `DEBUG` defined or not; the RH7103 static move across it: fix.
- SHARED-ORDER-006: the RH7107 `#if DEBUG` crossed-body example: fix, unchanged from `main`.
- RH7102-006: the new group-in-front example: no fix.
- RH7309-005:
  - `#if DEBUG` in moved `IDisposable`: no fix;
  - `#if DEBUG` in kept `IComparable`: fix;
  - a group in code between blocks that a moved block crosses: no fix (not in the spec; see below).
- The skeptic's 1668 candidates 1–5 in `02-skeptic.md`. Also a Fix All scenario (FC-008) where one move has a group in its span and the other does not.

## Existing tests that pin behavior that now changes

Review every test tagged SHARED-ORDER-007 or SHARED-ORDER-008. Every one that asserts a fix with a conditional group in the spans, or with a group in front of the type or the spans for an instance-field move, flips to no fix. No-fix tests stay, with their reason updated. Tests that only tell kept groups apart are deleted, as the issue's *Done when* asks. Those are the tests on the `field` keyword, bodiless accessors in a group, excluded-member initializers or fields moved along, and excluded `partial` declarations with attributes. Files, with tag counts (SHARED-ORDER-006/007/008):

- `tests/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsExcludedCodeTests.cs` (0/16/67)
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsTypeDeclarationGroupTests.cs` (0/0/59)
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs` (3/8/27). It holds these, which flip:
  - `CodeFixTravelsWithABalancedDisabledDirectiveBlockAboveTheMovedMember`
  - `CodeFixIsOfferedWhenCrossedMemberBodyContainsBalancedConditional`
  - `CodeFixIsOfferedWhenCrossedMemberBodyContainsActiveBalancedConditional`
- `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderInitializationOrderTests.cs` (0/7/22)
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.cs` (0/17/3). Every `FixCarries…OfExcludedCode…` and `FixCrossesAnInstanceFieldOfExcludedCodeInAStruct` flips to no fix. `FixMovesAConstantBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct` stays: it moves a constant, which reorders no instance field.
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs` (0/1/0): review.
- `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs`, tagged RH7309-005:
  - `VerifyCodeFixReordersRegionsContainingBalancedConditionalDirectives` flips (the old spec example);
  - `VerifyCodeFixReordersRegionsWithBalancedConditionalDirectivesBetweenThem` flips (a group in code between blocks that `Methods` crosses);
  - `BalancedConditionalPairInsideABlockDoesNotWithholdTheFix`: check which block holds the pair;
  - `VerifyCodeFixReordersRegionsWrappedByTheSameConditionalDirective` stays a fix: the group's directives lie in front of and behind the spans, and the move reorders no instance field.

These stay green:

- the RH7107 and RH7108 conditional tests;
- `AccessorOrderingUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsBalancedConditional`;
- RH2001-010's tests;
- the RH7103/RH7107/RH7108 tags of SHARED-ORDER-006, apart from the three RH7103 tests above. The RH7107 and RH7108 ones keep the tag, since SHARED-ORDER-006's example is now an accessor example.

## Output questions

_None._ Every case written is covered by 1668-1 B or 1668-2 A.

## Internal choices

- "In front of the class declaration" (1668-2 A) means the leading trivia of the declaration's first token, the place `main` already read, applied to every declaration of a `partial` type as `main`'s layout clause was. A group elsewhere in front, such as around an earlier type whose `#endif` does not reach `C`'s first token, is not read, as on `main`. The alternative, all text in front of the declaration in the document, would withhold fixes for groups that cannot attach anything to `C`. The 1668-2 question and the skeptic's neighbour example both put the `#endif` on the class's first token. If the lead reads the answer more widely, the third and fourth layout examples are unaffected; only the "not read" sentence changes.
- The exception is stated structurally: an `#if` and `#endif` only, the `#if` in the first token's leading trivia, the `#endif` in the leading trivia of the token behind the declaration's last token.
- SHARED-ORDER-008 keeps its ID and changes its title and text, rather than being retired and replaced. Its subject, which conditional directives refuse a member move, is unchanged.
- No new requirement and no new ID.

## Follow-up candidates

- RH2001-010 (`OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers`) is now the only code fix that parses excluded branches as members. The same criterion would refuse any conditional group there. Input: a private auto-property with `#if DEBUG` around a whole member directly in front of its first modifier. On `main` RH2001's fix is offered; under the criterion, no fix would be offered. This is an output question outside the run, to be filed `agent-filed`, as recorded in `00-lead.md`.
