Role: auditor
Commit: 5b79d152a854bf779fed97278e0b5fb66f1dbea8
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `5b79d152a854bf779fed97278e0b5fb66f1dbea8` (checked out, clean tree). Base `origin/main` `eed7195c0b6fb583ab4271173b7a4ad022851c5f`, an ancestor of the head, so it is merged. The diff (`git diff eed7195c..HEAD`) matches the handovers. It touches 4 production files: the RH2001 fix, `ConditionalGroupUnitUtilities`, `OrderingDeclarationUtilities`, and `SyntaxTriviaUtilities`. It also touches 1 test file, the RH2001 spec, SHARED-FIX-009, the change doc, and `sdd/work/`. `scripts/lane-check.sh behavior-change` confirms the lane.

The cleaner reported a green build, and the implementer and cleaner reported green focused test runs. I did not re-run those tests. I checked the evidence below with `scripts/apply-fix.sh RH2001` over fixtures in the scratchpad, outside the repository.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.cs` (behind-the-property tests) | FC-003, FC-017 | – | same requirement | RH2001-010 says a directive in the leading trivia of the token behind a property without an initializer withholds the fix, "whether it is the group's `#if` or only its `#endif`". Every test of the behind side has both the `#if` and the `#endif` in that one trivia. That includes `…BehindAPropertyWithoutAnInitializer`, `…BehindTheLastPropertyInFrontOfTheClosingBrace`, `…BehindAPropertyWithoutAModifier`, the excluded-initializer and conditional-modifier tests, and the Fix All `Third`. So a behind check that only recognized `#if`, or only `#endif`, would pass the whole class. Two members are unpinned, and both are visible changes against `main`. **(a) `#endif` alone behind:** `#define NET` / `#if NET` / `private int _a;` / (blank) / `private int B { get; }` / `#endif` / (blank) / `int Read() => _a + B;`. `main` fixes this: the group has no excluded branch, so the old parse accepted it. Head offers no fix (apply-fix, LF and CRLF). This is the multi-targeting shape of a group around several members with the property last. **(b) `#if` alone behind:** `#define NET` / `private int B { get; }` / `#if NET` / `private int _x;` / (blank) / `private int _y;` / `#endif`. `main` fixes this, head offers no fix. The front side holds: `#endif` alone (`…AnEndifAloneInFrontOfTheFirstModifier`) and `#if` alone (`…WrapsThePropertyWithAnInitializer`) are each pinned. | Test implementer: add two no-fix tests for the behind token, one with only `#endif` in its trivia (a) and one with only `#if` in it (b), under LF and CRLF, tagged RH2001-010. For the other side of the boundary, a property in the middle of the same group keeps the fix: `#if NET` / `_a` / `B` / `_d` / `#endif`. Head fixes `B` to `private readonly int _b;` and leaves the group in place. It is already covered in kind by `VerifyFixKeepsAGroupThatWrapsTheWholeType`, so it is optional |
| 2 | low | `sdd/changes/2026-11/rh2001-conditional-group-refusal.md`, *Visible output changes* | – | – | same requirement | The change doc does not list two shapes that change visibly. Neither is an undecided output, because maintainer answer A and RH2001-010's boundary text cover both (the same reading as lead decision 1). (a) Shape (a) of finding 1: a group whose `#if` stands in front of an earlier member and whose `#endif` alone stands behind a property without an initializer. The *Decisions* section names only the mirror of this shape, with the `#endif` in front of the first modifier. (b) A group whose active `#else` branch is the property itself: `#if NET` / `private int _a;` / `#else` / `private int B { get; }` / `#endif` / `private int _d;`, without `NET`. `main` fixes it: its one excluded branch is a whole member. Head offers no fix. Section 1 lists only an active `#else` branch that holds another member, and section 3 lists only `#if` around the property | Spec author: add both shapes to *Visible output changes*. Shape (a) fits section 2 or 3, and shape (b) fits section 3 |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001-010, front: conditional directive in front of the first modifier | `node.Modifiers[0].LeadingTrivia`, only when `Modifiers.Count > 0`. This is every trivia from the token in front of the property up to the first modifier | RH2001-009 `HasConditionalDirectiveBetweenModifiers`: `Modifiers[1..]` leading trivia plus `node.Type.GetLeadingTrivia()`, which is the first token when there is no modifier | The two spans are contiguous and disjoint, and together they cover the leading trivia of every token from the first token through the type. `IsSupportedPropertyShape` rejects attribute lists, so no leading trivia of the property escapes both checks | holds |
| RH2001-010, behind: conditional directive behind a property without an initializer | `node.GetLastToken().GetNextToken().LeadingTrivia`, only when `node.Initializer == null`. That token is the next member's first token, or the type body's `}` | The spec text names the same token. With an initializer the side is not read, and the spec says so | Agree, with and without a modifier | holds (tests: finding 1) |
| Directive kind | `SyntaxTriviaUtilities.IsConditionalDirective`: `#if`, `#elif`, `#else`, `#endif` | RH2001-004's "directives other than `#if`, `#elif`, `#else`, and `#endif`" keep the fix | Same four kinds. `#region`/`#endregion`/`#pragma`/`#nullable` in front keep the fix (existing tests, and apply-fix on `#nullable` above the property). `#endregion` behind keeps the fix (the test with `#endregion // Properties`) | holds |
| Interior directives | RH2001-007 `KeepsEveryCommentAndDirective` | – | Unchanged | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Kind test "is a conditional directive" on trivia | `SyntaxTriviaUtilities.IsConditionalDirective` (now `public`, the shared owner); RH2001 fix (both checks call it; its inline copy is deleted); `ConditionalGroupUnitUtilities.IsConditionalDirective(DirectiveTriviaSyntax)` (internal, node form, existed before the run); `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.WouldSeparateDirective` line 197 (inline type test, existed before the run, outside the run); `DirectivePairSplitScan` switch cases and `ReihitsuFormatterHelpers` (different decisions) | The run removed one copy and added none. The copies that remain existed before the run and lie outside it (*Follow-up candidates*) |
| Which conditional directives withhold the RH2001 fix | `HasConditionalDirectiveBetweenModifiers` (RH2001-009) and `HasConditionalDirectiveNextToTheProperty` (RH2001-010), both private to the RH2001 fix. `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` is deleted, and no other owner exists | one owner |
| Reading excluded text as code | `rg "SyntaxFactory.Parse(CompilationUnit\|SyntaxTree\|MemberDeclaration\|Statement)" src` finds nothing. `ParseInTypeBody`, `WrapsWholeMembers`, `GetExcludedText`, and `ReadsAsWholeMembers` are deleted, and nothing in `src/`, `tests/`, `sdd/specs/`, or `documentation/` refers to them | gone, FC-044 instance removed |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 13 `VerifyNoCodeFixIsOffered…` tests, new or flipped. They cover groups in front of the first modifier (`static private` reorder, plain, `#endif` alone, `#if false`, excluded `#if` with active `#else`, comment-only branch, inside a group that wraps the file, `#if` around the property with and without an initializer) and behind (whole member, in front of `}`, without a modifier) | RH2001 is reported at the marked span, and no action is registered | The fix registers an action, or the diagnostic is missing or moved | `VerifyNoCodeFixIsOffered`: an exact diagnostic check plus `Assert.IsEmpty(actions)` on the unmarked source, under LF and CRLF | adequate per test. The behind side's kind axis is open (finding 1) |
| `VerifyFixKeepsAGroupInFrontOfAnEarlierMember`, `VerifyFixKeepsAGroupThatWrapsTheWholeType` | The fix is offered, and the exact output matches the spec example | Any byte of the output differs, or the fix does not converge | `Verify` (full fixed source, LF and CRLF) | adequate. The expected outputs come from the spec, and `main` produces them unchanged (01-spec-author's apply-fix run) |
| `VerifyFixAllSkipsThePropertiesBehindWhichAConditionalGroupStands`, `VerifyFixAllSkipsThePropertiesBehindConditionalGroupsInFrontOfTheFirstModifier` | Fix All fixes exactly the properties next to which no conditional directive stands, skips the others, and keeps their diagnostics | A skipped property is converted, or a fixable one is not | `Verify` with `MarkupMode.Allow` on fixed and batch-fixed state | adequate. Each test fixes two properties and skips two (SHARED-FIX-002) |
| 7 kept refusal tests with reworded summaries | unchanged verdicts | – | unchanged | adequate. The summaries no longer cite the parse (FC-031) |

Visible output (check 10): every flipped expected value, both Fix All results, and the two new expected outputs follow from maintainer answer A. Answer A is recorded under `Maintainer answers` in `00-lead.md` and in the change doc's `Decisions`. The kept outputs are listed under *Unchanged* in the change doc and match `main`. The rule page is unchanged. Nothing visible is undecided.

## Failure classes dismissed

- FC-001, FC-002: the inspected spans match the spec's tokens exactly (*Guard scope*).
- FC-004, FC-019, FC-021: the change only withholds. The trivia handling of the rewrite is unchanged.
- FC-005, FC-020: conditional and non-conditional directives are separated on both tokens, and `#if false` is pinned. `#elif` and `#else` cannot stand in a token's leading trivia without that group's `#if` or `#endif` in the same trivia, so a mutation that misses them is indistinguishable (04-test-implementer's argument; I checked it).
- FC-006, FC-022–FC-027, FC-029: the rewrite, literals, identifiers, and nesting forms are unchanged. The decision reads only directive trivia of two tokens.
- FC-007, FC-009: the kept-fix tests verify convergence. A withheld fix registers no action.
- FC-008: `BatchFixer` computes every fix against the original document. The trivia the decision reads is kept by a neighbour's fix, and both Fix All tests cover a neighbour group.
- FC-010–FC-014, FC-028, FC-038–FC-043: no formatter phase, precheck, or unsupported shape is touched, and no shape is declared.
- FC-015, FC-016: see *Policy ownership*. No sibling fix parses excluded text any more.
- FC-018, FC-036: the rename behavior is unchanged.
- FC-030: the change adds no formatting. A defect that existed before the run shows up near it (*Follow-up candidates*).
- FC-031: the `TryCreateFieldDeclaration` remarks, the new method's summary, the `ConditionalGroupUnitUtilities` type summary, the `SyntaxTriviaUtilities.IsConditionalDirective` summary, and the test summaries all describe the new code. `rg -i "whole members" src` finds nothing.
- FC-032, FC-035: the expected values come from the spec, and the spec author checked them against `main`.
- FC-033, FC-034: the issue's exact shape (`static private` behind a whole-member group) is the first flipped test, under both line endings.
- FC-037: no text-only claim.
- FC-044: the change removes the instance and adds no model.
- Check 11 (cost of special cases): the production change is a net deletion (32 lines added, 99 deleted in the main commit, and a further net −10 in the cleanup). No special case is added.

## Architecture findings

_None._

## Hints

- The maintainer may notice an asymmetry, which follows literally from answer A and RH2001-010. In `#if NET` / `private int A { get; }` / `private int B { get; }` / `private int D { get; }` / `#endif`, `A` (the `#if` is in front of its first modifier) and `D` (the `#endif` is behind it) get no fix, while `B` is fixed. The apply-fix runner stops at the first diagnostic, so it shows `A` directly. `D` is shown by shape (a) of finding 1, and `B` by the `_a`/`B`/`_d` fixture. The lead may want to state this in the PR's review notes, since a group around several members is common in multi-targeting code.
- SHARED-FIX-009's first bullet still allows a fix to read "whole member declarations, whole statements, whole attribute lists or accessors" from excluded text, but no fix reads anything but names any more. The skeptic and the spec author noted this and kept the bullet. It is harmless, but it describes no current reader.

## Follow-up candidates

- **RH2001 fix: an indented `#nullable` behind the converted property is moved and gets a foreign line ending.** Input, CRLF: `class C` / `{` / `    private int A { get; }` / `    #nullable enable` / `    private int _b;` / … `int Read() => A + _b;`. The fix outdents `#nullable enable` to column 0 and writes one LF into the CRLF document (apply-fix: `[line-ending-drift]`, CRLF=7, LF=1). Under LF, only the outdent happens. The directive sits in the leading trivia of the next member, outside the replaced node (FC-030). The same input with `#region`/`#endregion` behind, or with `#nullable` in front of the property, shows neither effect. This existed before the run: for this input the fix's decision path is the same on `main` and on the head, because no conditional directive is present and every other line of the fix is unchanged. I have not established the structural cause.
- **Conditional-directive kind test, remaining copies (FC-015).** `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.WouldSeparateDirective` (line 197) states the four-kind test inline. `ConditionalGroupUnitUtilities.IsConditionalDirective(DirectiveTriviaSyntax)` states it again for directive nodes. Both could use `SyntaxTriviaUtilities.IsConditionalDirective`, which is now public. This is a small internal cleanup outside the run's components.
