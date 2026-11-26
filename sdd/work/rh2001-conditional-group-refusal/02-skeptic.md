Role: skeptic
Commit: d638d710a10e10c4015ebf642edb10558d507037
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I treat the maintainer's answer, option A in the issue comment of 2026-11-22 and recorded in `00-lead.md`, as binding.

## Premise

The premise is real. The caller is the RH2001 code fix, and the IDE and the `apply-fix` runner reach it. `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.TryCreateFieldDeclaration` (lines 441-446) calls `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` on two tokens: `node.Modifiers[0]`, and, for a property without an initializer, `node.GetLastToken().GetNextToken()`. That method parses each excluded branch through `ConditionalGroupUnitUtilities.WrapsWholeMembers`, then `GetExcludedText`, then `ReadsAsWholeMembers`, then `ParseInTypeBody`. Nothing else calls `ConditionalGroupsWrapWholeMembers` (`rg` over `src/` and `tests/`). Within `ConditionalGroupUnitUtilities`, only that chain calls `WrapsWholeMembers`, `GetExcludedText`, `ReadsAsWholeMembers`, and `ParseInTypeBody`. No test calls them directly. It is the last parse of excluded text in the repository: `ParseInTypeBody` is the only `SyntaxFactory.Parse*` call in `src/Reihitsu.Core`, `src/Reihitsu.Analyzer`, and `src/Reihitsu.Analyzer.CodeFixes`, and `ExcludedCodeUtilities` reads names only. The issue's claim holds.

The issue says "nobody ran the fix". I ran it, with these fixtures in a scratch directory outside the repository:

`scripts/apply-fix.sh RH2001 <fixture-dir>` gave 10 fixtures × LF/CRLF, with 18 fixed and 2 no-fix-offered (f10 under both endings). Every arm reached a supported end state in one iteration.

| Fixture | Input (abridged) | `main` today | After option A |
|---|---|---|---|
| f1 (issue) | `#if DEBUG` / `private int _trace;` / `#endif` / `static private int Value { get; }` / `int Read() => Value;` | fixed: `private static readonly int _value;`, `Read() => _value` | withheld |
| f2 | `#if !DEBUG` / `private int _trace;` / `#endif` / `private int Value { get; }` | fixed: `private readonly int _value;` | withheld |
| f3 | `private int Value { get; }` / `#if DEBUG` / `private int _trace;` / `#endif` / `int Read() => Value;` | fixed | withheld |
| f4 | `#define NET` … `#if NET` / `private int Value { get; }` / `#endif` / `int Read() => Value;` | fixed: `#if NET` / `private readonly int _value;` / `#endif` | withheld |
| f5 | `#define NET` … `int Read() => Value;` / `#if NET` / `private static int Value { get; } = 5;` / `#endif` / `}` | fixed: `private static readonly int _value = 5;` | withheld |
| f6 | `int Read() => Value;` / `private int Value { get; }` / `#if DEBUG` / `private int _trace;` / `#endif` / `}` | fixed | withheld |
| f7 | `#if DEBUG` / `private int _trace;` / `#endif` / `private int _other;` / (blank) / `private int Value { get; }` | fixed | fixed, unchanged |
| f8 | `#region Values` / `private int Value { get; }` / `#endregion` | fixed | fixed, unchanged |
| f9 | `#define NET` / `#if NET` / `class C { private int Value { get; } … }` / `#endif` | fixed | fixed, unchanged |
| f10 | `#define NET` … `#if NET` / `int Value { get; }` / `#endif` (no modifier) | **no fix offered** (RH2001-009) | no fix offered, unchanged |

Full f4 input (f5 has the same shape, with an initializer and `static`):

```csharp
#define NET
class C
{
#if NET
    private int Value { get; }
#endif

    int Read() => Value;
}
```

f1, f2, f3, f7, and f9 agree with the spec author's runs and with the change doc's *Before* and *Unchanged* blocks.

### One visible case the change doc does not list: a group around the property itself

f4 and f5 are a group whose `#if` stands in front of the property's first modifier and whose `#endif` stands behind it. The group holds the property, not "whole members next to the property". Today the fix is offered there, because the group has no excluded branch. Under the new RH2001-010 it is withheld, because the `#if` is in the first modifier's leading trivia. This is a visible output change. Neither the issue's example nor the change doc's *Visible output changes* shows it. A private member wrapped in its own `#if NET…` is something multi-targeting code does write.

I do not raise it as an output question, because there is only one reading. The maintainer's answer says "any `#if` group directly in front of the property", and option A says "whatever the group holds". The `#if` of f4/f5 stands exactly there. The same shape without a modifier (f10) is already refused today by RH2001-009, so option A makes the two forms agree. **The lead should add f4/f5 to the change doc's *Visible output changes*,** as a third bullet under section 1 or a section of its own, and should have a test pin it (see *Breaking inputs*). If the lead judges that the maintainer did not see this shape when answering, it is an output question with these options: A, withheld (f4 as is, reported, no fix); B, fixed as today. I recommend A, because it is the literal answer and it matches RH2001-009.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug.

For completeness, the dispatch is `TryCreateFieldDeclaration`. Which tokens' leading trivia the conditional-directive decision reads:

- `node.Modifiers[0]`, under RH2001-010 when `Modifiers.Count > 0`.
- `node.Modifiers[1..]` and `node.Type.GetFirstToken()`, under RH2001-009 (`HasConditionalDirectiveBetweenModifiers`). This includes the type of a property without a modifier, which is its first token.
- `node.GetLastToken().GetNextToken()` when `node.Initializer == null`, under RH2001-010. That token is the next member's first token or the type body's `}`.
- Every other token inside the declaration, under RH2001-007 (`KeepsEveryCommentAndDirective`).

A property with an attribute list never reaches these checks (`IsSupportedPropertyShape` rejects `AttributeLists.Count > 0`). So the property's first token is always the first modifier or the type, and none of the property's own leading trivia escapes RH2001-009/010. No other candidate exists.

## Breaking inputs

Failure classes that reach this change:

- **FC-044.** This change ends an instance of the class: the guard modelled which excluded branches are harmless. The replacement must be a trivia-kind test of the two tokens and must read no excluded text.
- **FC-020 / FC-005.** Every conditional kind (`#if`, `#elif`, `#else`, `#endif`, including `#if false`) withholds. Non-conditional directives (`#region`, `#pragma`) must keep the fix.
- **FC-003.** Both sides of each boundary need a test. For the token, a directive in the first modifier's leading trivia is withheld, and one in an earlier member's leading trivia is kept. Behind the property, a property without an initializer is withheld, and one with an initializer is kept. For the directive kind, a conditional directive is withheld, and `#region`/`#pragma` is kept.
- **FC-015.** The conditional-kind test already exists as a private copy in RH2001 (`HasConditionalDirectiveBetweenModifiers`, line ~501) and in `SyntaxTriviaUtilities.IsConditionalDirective` (private, line 673), plus `ConditionalGroupUnitUtilities.IsConditionalDirective`. The new check should not become a fourth copy.
- **FC-008.** In Fix All, two neighbouring properties with a group between them must both be skipped. The decision reads only trivia that the neighbour's fix keeps, so I expect no staleness, but the existing Fix All tests should stay green.
- **FC-031.** The comments describe the parse in these places: the `TryCreateFieldDeclaration` remarks ("unless its group wraps whole members"), the `ConditionalGroupsWrapWholeMembers` summary, the `ConditionalGroupUnitUtilities` type summary, and the XML summaries of **kept** refusal tests that state the parse as their reason. Examples: `VerifyNoCodeFixIsOfferedForAConditionalModifierBehindAPropertyWithoutAnInitializer` ("the branch does not read as whole members"), `VerifyNoCodeFixIsOfferedForAnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember` ("every excluded branch of the group is read"), and the attribute-list test ("the branch is no whole member"). The spec author says these tests "stay as they are". Their verdicts do, but their summaries go stale and need rewording.

Candidate inputs for the test implementer:

| # | Input | Expected after | Existing test |
|---|---|---|---|
| 1 | f1 (issue example) | reported, no fix | `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier`, `VerifyFixKeepsAGroupOfAWholeMemberInFrontOfTheFirstModifier` (flip to no fix) |
| 2 | f2 (`#endif` alone in front of the first modifier, active `#if` branch) | reported, no fix | none found |
| 3 | excluded `#if` branch + active `#else` member, `#endif` in front of the first modifier | reported, no fix | `VerifyFixKeepsAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember` (flip) |
| 4 | excluded branch holding only a comment | reported, no fix | `VerifyFixKeepsAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment` (flip) |
| 5 | f3 (whole member behind a property without an initializer) | reported, no fix | `VerifyFixKeepsAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer` (flip) |
| 6 | f6 (whole-member group in front of the type body's `}`) | reported, no fix | none found (`VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindTheLastPropertyInFrontOfTheClosingBrace` covers the `}` token, but with a group that is refused today) |
| 7 | f4 (group around the property, no initializer) | reported, no fix | none found |
| 8 | f5 (group around a property with an initializer, multi-targeting shape) | reported, no fix: `#if` in front of the first modifier; the behind-check does not apply | none found |
| 9 | inner group in front of the first modifier inside an outer group that wraps the file | reported, no fix | `VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile` (flip) |
| 10 | f7 (group in front of an earlier member) | fixed, output as in the spec | none found |
| 11 | f9 (group wrapping the whole type) | fixed, output as in the spec | none found |
| 12 | f8 / `#pragma` in front of the first modifier | fixed | `VerifyFixKeepsARegionDirectiveAboveOnceInFrontOfTheReorderedModifiers`, `VerifyFixKeepsAPragmaDirectiveAboveOnceInFrontOfTheReorderedModifiers` |
| 13 | group behind a property with an initializer | fixed | `VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer` |
| 14 | f10 (no modifier, group around the property) | no fix (RH2001-009) | `VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers` covers the no-modifier whole-member case |
| 15 | `#if false` / `private int _x;` / `#endif` in front of the first modifier | reported, no fix | none found (optional, FC-020) |
| 16 | Fix All over two properties with a whole-member group between them, plus a third plain property | the first two skipped, the third fixed | `VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier` covers skipping, not this neighbour shape (optional) |

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer` | must stay: the diagnostic is reported as today |
| Code fix `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.TryCreateFieldDeclaration` | must change: the two RH2001-010 calls become a test for a conditional directive in the leading trivia of the same two tokens. Its remarks must be updated (FC-031) |
| Fix All | must stay: it goes through the same guard |
| `Reihitsu.Core` `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` | must change: delete it. It has no caller other than RH2001 |
| `Reihitsu.Core` `ConditionalGroupUnitUtilities` | must change: delete `ParseInTypeBody`, `WrapsWholeMembers`, `GetExcludedText`, and `ReadsAsWholeMembers`. Keep `IsConditionalDirective`, `GetConditionalDirectives`, and `IsWithoutBody`, which `OrderingDeclarationUtilities` (394, 519, 605) and `OrderingMoveSafety` (436) still use. Update the type summary |
| Formatter | not involved: no phase reads this decision, and no formatter output changes |
| Rule doc `documentation/rules/RH2001.md` | must stay: a withheld fix is covered by `general-notes.md` |
| Specs | RH2001-004/009/010 and SHARED-FIX-009 already changed in `5a13874`. `SHARED-ORDER-008` is not involved (it is consistent) |
| Tests | must change: the six flipped tests, the new seeds above, and rewording of stale summaries on kept refusal tests |

## Refuse option

The requested change is the refuse option: the fix is withheld for every conditional group in the two places. Compared with keeping the parse (option B, already rejected by the maintainer), it is a net deletion: about 80 lines of Core helpers and one public Core method go, no type or mechanism is added, and the replacement is a cheap trivia-kind check. The input class, a conditional group next to a private auto-property that RH2001 reports, is rare. The one exception is f4/f5, a group around the property itself, which multi-targeting code does write. The maintainer's "any" covers it, and RH2001-009 already refuses its no-modifier form (see *Premise*).

Declaring a shape unsupported is no option here. The decision lives in a code fix, and no formatter phase holds a guard for a conditional group in front of a member's first token. A declared shape "conditional directive in front of a property's first modifier" would delete no formatter special case and would refuse whole files that are common code: every `#if` around a member. A shape that deletes nothing is not declared (`AGENTS.md`, *Special cases do not grow the source*).

Simpler options: none beyond the change itself. Narrowing further, for example to refuse only in front of the first modifier and not behind the property, would keep the hazard RH2001-010's *Why* names: an excluded `= 5;` behind a property without an initializer.

## Output questions

_None._ The binding answer settles the run's question. I read the group around the property itself (f4/f5) as covered by its wording; see *Premise* for the fallback question in case the lead judges otherwise.

## Internal choices

- **Where the check lives.** It can be a test in the RH2001 fix itself, next to `HasConditionalDirectiveBetweenModifiers`, which already performs the same trivia-kind test for RH2001-009, or a Core helper. Nothing visible changes either way. I recommend the RH2001 fix, reusing one conditional-kind predicate rather than adding another copy (FC-015). A RH2001-only decision in `OrderingDeclarationUtilities` was the old misplacement.
- **`ConditionalGroupUnitUtilities` name.** After the deletion the type no longer decides "units". Renaming it is optional and internal. I recommend leaving the name, to keep the diff on the issue.
- **SHARED-FIX-009's general bullet** still lists "whole member declarations, whole statements, whole attribute lists or accessors". After this change no fix reads those from excluded text, only names. The spec author chose to keep the bullet. That is acceptable, and nothing visible changes. The spec checker may note that the bullet now describes no current reader except names.

## Issue goals

- **"Affected" helper names are stale.** The issue names `WrapsWholeUnitsOf` and "the type-body arm of `ReadsAsWholeUnits`". In the code they are `ConditionalGroupUnitUtilities.WrapsWholeMembers` and `ReadsAsWholeMembers`, and `ReadsAsWholeMembers` has a single arm, so the whole method goes, not "an arm". The "Done when" deletion should be read against the actual names listed under *Surfaces*. This changes no visible output.
- **"Tests cover the refusal for a group of whole members"** is kept, and should be widened to the cells in *Breaking inputs*, in particular f4/f5/f6 and the kept-fix boundaries f7/f9. No visible change beyond the decision.
- The other goals stand.

## Architecture findings

_None._ The change removes an FC-044 model rather than adding one. The ordering fixes already decide by the same "where the directive stands" criterion (SHARED-ORDER-008).

## Follow-up candidates

_None._
