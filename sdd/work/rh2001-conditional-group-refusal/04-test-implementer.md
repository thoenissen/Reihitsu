Role: test implementer
Commit: fc26903 (tests written and run against this head; production code unchanged)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I followed the maintainer's answer A and the lead's decisions 1–3 in `00-lead.md`.

All tests are in `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.cs`. Each single-property test runs under LF and CRLF (`DataRow`). On this head, 26 cases fail (13 tests × 2 line endings), plus 2 Fix All tests. Every failure is `Assert.IsEmpty(actions)` (a fix is offered where none is expected), or, for Fix All, the one property the new rule withholds is fixed. The two new keep-the-fix tests pass. The other 142 cases of the class pass.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInFrontOfModifiersTheFixWouldReorder` (was `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier`; now the spec's first example, `static private`, the issue's shape; retagged from RH2001-004/009) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInFrontOfTheFirstModifier` (was `VerifyFixKeeps…`, flipped) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAnEndifAloneInFrontOfTheFirstModifier` (new; spec example 2, `#if !DEBUG`) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAnIfFalseGroupOfAWholeMemberInFrontOfTheFirstModifier` (new; skeptic cell 15) | RH2001-010; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupThatWrapsThePropertyWithoutAnInitializer` (new; spec example 3, class `C`; f4) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupThatWrapsThePropertyWithAnInitializer` (new; spec example 3, class `D`; f5; only the `#if` decides) | RH2001-010; FC-003 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixKeepsAGroupInFrontOfAnEarlierMember` (new; spec example, output from the spec; f7) | RH2001-010; FC-003 | `Verify` | passes |
| `VerifyFixKeepsAGroupThatWrapsTheWholeType` (new; spec's last example, output from the spec; f9) | RH2001-010; FC-003 | `Verify` | passes |
| `VerifyNoCodeFixIsOfferedForAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember` (flipped) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment` (flipped) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer` (flipped; spec example `C` of the behind case; f3) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberBehindTheLastPropertyInFrontOfTheClosingBrace` (new; f6, the `}` token) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberBehindAPropertyWithoutAModifier` (new; "with or without a modifier" side of the behind check) | RH2001-010; FC-003 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile` (flipped) | RH2001-010 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixAllSkipsThePropertiesBehindWhichAConditionalGroupStands` (was `VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands`; rewritten) | RH2001-010, SHARED-FIX-002; FC-008 | `Verify` (Fix All, markup allowed in fixed state) | fails: `Third`, behind a whole-member group, is fixed |
| `VerifyFixAllSkipsThePropertiesBehindConditionalGroupsInFrontOfTheFirstModifier` (was `VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier`; rewritten) | RH2001-010, SHARED-FIX-002; FC-008 | `Verify` (Fix All) | fails: `Second`, behind a whole-member group's `#endif`, is fixed |

Both Fix All tests depended on the old parse. Their `Second`/`Third` properties stood next to a whole-member group and expected a fix. The handovers said both tests "stay as they are", but they could not stay as written. I rewrote each so that it still fixes two properties and skips two. A plain member (`_alpha`, `_beta`) separates each group from the next property, so that property's leading trivia holds no conditional directive (skeptic cell 16, the neighbour shape).

FC-031: I reworded the summaries of seven kept refusal tests that gave the old parse as their reason ("the branch is no whole member", "every excluded branch of the group is read", "does not read as whole members", "the group is nested"). Their sources, tags, and verdicts are unchanged: `VerifyNoCodeFixIsOfferedForAConditionalStaticInFrontOfTheFirstModifier`, `…AnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember`, `…AConditionalAttributeListInFrontOfTheFirstModifier`, `…ANestedGroupOfAWholeMemberInFrontOfTheFirstModifier`, `…AGroupBehindThePropertyThatHoldsAnotherGroupInItsBranch`, `…AConditionalModifierBehindAPropertyWithoutAnInitializer`, `…ANestedGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer`.

## Not covered, and why

These values are already pinned and get no new test:
- Group behind a property with an initializer keeps the fix: `VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer`.
- `#region` / `#pragma` in front of the first modifier keeps the fix (FC-005, the non-conditional side): `VerifyFixKeepsARegionDirectiveAboveOnceInFrontOfTheReorderedModifiers`, `VerifyFixKeepsAPragmaDirectiveAboveOnceInFrontOfTheReorderedModifiers`.
- No modifier, group in front of the type (RH2001-009, f10): `VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers` and its siblings.
- Excluded initializer behind a property, also in front of `}` and without a modifier: the three `VerifyNoCodeFixIsOfferedForAnExcludedInitializer…` tests.
- `#elif` / `#else` in the decisive trivia: `VerifyNoCodeFixIsOfferedForAnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember` and the `#else` tests.

Static argument against kind-isolated `#elif`/`#else` tests: a token's leading trivia cannot hold an `#elif` or `#else` without also holding that group's `#if` or `#endif`. A disabled branch always runs up to the next directive, so the trivia that holds the `#elif`/`#else` also holds the directive that opens or closes the disabled text around it. A predicate that misses `#elif`/`#else` but sees `#if`/`#endif` therefore cannot be told apart by any input. `#if` alone decides in `…WrapsThePropertyWithAnInitializer`, and `#endif` alone decides in `…AnEndifAloneInFrontOfTheFirstModifier`.

Failure classes that do not apply:
- Formatter classes (FC-010–014, FC-028, FC-038–043): no formatter phase or shape changes, and no shape is declared unsupported.
- Rename classes (FC-018, FC-036): rename behavior is unchanged.
- Literal and identifier classes (FC-022–026, FC-029): the decision reads only directive trivia of two tokens.
- FC-007/FC-009: the change only withholds; the kept-fix outputs are verified for one-pass convergence by `Verify`.
- FC-015/FC-042/FC-044 are implementation concerns for the implementer and auditor; FC-044 is what the change removes. The tests pin the outcome whatever the predicate's owner.
- RH2001-004's last spec example (`#if NEVER` / `private int _value;` / `#endif` above the property) is already withheld (`VerifyNoFixIsOfferedWhenExcludedCodeDeclaresTheNewName` in the analyzer tests), and its verdict does not change.
- No other test project exercises the RH2001 fix with a conditional group. I ran `rg` for `#if`, `#elif`, `#else`, and `#endif` over the three RH2001 test files, and for `RH2001` across `tests/`. The analyzer-test conditional cases are excluded-name refusals that stay refused; `CodeFixRunCommandTests.ExecuteAsyncReportsRH2001WithoutDocumentRename` has no directive.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001"
```

## Follow-up candidates

_None._
