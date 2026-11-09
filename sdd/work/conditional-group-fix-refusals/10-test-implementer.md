Role: test implementer
Commit: 3fd9b4d
Verdict: tests written

Repair round 1 (Decisions #12, #13, #15, #17 in `00-lead.md`; audit 08 findings #1, #2, #4). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the findings. Expected values come from the RH2001-010 and SHARED-ORDER-008 examples and normative sentences of the spec diff `ce0b3eb..3fd9b4d` and from `main`'s output where the spec keeps it. No production code was touched. "Expected now" was observed on `3fd9b4d` with only these test files changed. "red" means the test fails on the finding's own difference: "No code fix may be offered for the diagnostic at …" (fix offered where the spec withholds it), or, for the kept fixes and Fix All, a diff that shows the property left unchanged or converted. Every `[DataRow]` test runs on LF and CRLF.

## thoenissen/Reihitsu#1666

File: `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutOne (spec example, finding #2 input) | RH2001-010 | VerifyNoCodeFixIsOffered | red (fix offered) |
| VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindTheLastPropertyInFrontOfTheClosingBrace (token behind = the type body's closing brace) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutAModifier ("with or without a modifier") | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForAConditionalModifierBehindAPropertyWithoutAnInitializer (change doc rare class: a modifier of the next member) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer ("No group inside it", behind position) | RH2001-010 | VerifyNoCodeFixIsOffered | red |
| VerifyFixKeepsAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer (spec example) | RH2001-004, RH2001-010 | Verify | green |
| VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer (spec example; other side of the initializer condition, FC-003) | RH2001-004, RH2001-010 | Verify | green |
| VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile (spec example, finding #1 input) | RH2001-004, RH2001-010 | Verify | red (no fix offered; expected `main`'s output) |
| VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands (Fix All: `Second` behind a whole-member group and plain `Third` fixed, `First` with `= 5;` behind stays) | RH2001-010, SHARED-FIX-002 | Verify, MarkupMode.Allow | red (Fix All converts `First`) |
| VerifyNoCodeFixIsOfferedForAGroupBehindThePropertyThatHoldsAnotherGroupInItsBranch (**renamed**, was `…ForAGroupOfAWholeMemberInsideAnotherGroupInFrontOfTheFirstModifier`; spec's last example) | RH2001-010 | VerifyNoCodeFixIsOffered | green (unchanged assertion) |

The renamed test: the spec author's 09 noted that its name and summary described the dropped "lies inside another group" half. Only the name and `<summary>` changed; the input and the assertion (no fix) are as before. It is green on head through the dropped half and must stay green through the new behind-the-property reading.

`VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier` is unchanged; it is the "No group inside it" condition in front of the first modifier.

Fixture note for the implementer: in the first draft of the Fix All test, the `#if X … #endif` group stood in front of `Second`'s first modifier, which RH2001-010 already refuses (its excluded branch `= 5;` is not whole members). The test now places that group in front of a method, so only `First` is withheld.

### Scenarios (repair)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| finding #2 `= 5;` in `#if X` behind `private int Value { get; }` | LF, CRLF | code fix | reproduced | fix offered; expected none |
| the same, Fix All with two fixable properties | LF | Fix All | reproduced | `First` converted; expected to stay |
| finding #1, file wrapped in `#if NET` | LF, CRLF | code fix | reproduced | no fix offered; expected `main`'s field |

## thoenissen/Reihitsu#1667

File: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClassThatEndsInASemicolon (spec example, finding #4 input) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialStructThatEndsInASemicolon (`#if !X [StructLayout(Auto)]`, as a struct is sequential by default) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialRecordThatEndsInASemicolon | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialRecordStructThatEndsInASemicolon | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfAnotherDeclarationOfAPartialClassThatEndsInASemicolon (directive behind the first token, `#endif` in front of the `;`) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | red |
| FixMovesAFieldOfAPartialClassWhoseOtherDeclarationEndsInASemicolonWithoutAConditionalDirective (counterpart: `;` declaration without a directive) | SHARED-ORDER-008, RH7109-006 | Verify | green |
| NoCodeFixWhenAnExcludedBranchInFrontOfThePartialClassDeclaresAnotherDeclarationOfItWithAnAttributeListThatEndsInASemicolon (A5 reading with a `;` declaration in the branch) | SHARED-ORDER-008, RH7109-006 | VerifyNoCodeFixIsOffered | green |

## thoenissen/Reihitsu#1665

No tests this round: findings #5 (check order) and #6 (`<returns>` text) change no behavior. #5 keeps every result; the existing RH7102/RH7103/RH7109 front-of-spans tests cover both outcomes of the reordered check.

## Not covered, and why

- **RH7102, RH7103, RH7309 for the `;` declaration.** All four consumers reach the declaration reading through one call: `rg -n "AnyDeclarationHoldsConditionalDirective|ChangesMemberOrder" src --glob '!*Test*/**'` finds `AnyDeclarationHoldsConditionalDirective` only inside the two public `OrderingDeclarationUtilities.ChangesMemberOrder` overloads, called by `TypeMemberOrderingCodeFixProviderBase` (RH7102/RH7103/RH7109) and `RegionBlockLayout` (RH7309). One consumer (RH7109) tests it; the m01 shape is already tested for RH7309 and RH7103.
- **`;` declaration in another document.** The document dimension is the existing two-document test; the `;` reading is per declaration (`DeclaringSyntaxReferences`), independent of the document.
- **Fix All for the `;` declaration.** `FixAllSkipsTheFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute` covers the refusal path in Fix All; the new reading is inside the same per-diagnostic guard.
- **RH2001 behind position with `#elif`/`#else` branches.** The behind reading uses the same shared helper as the front reading, whose branch handling is tested by the existing `#else`/`#elif` RH2001-010 tests.
- **Formatter failure classes (FC-010 to FC-014, FC-028, FC-030, FC-038 to FC-043).** The formatter is not touched and no shape is declared unsupported.
- **Comment/directive gap cases (token-gap rule).** No rule touching token gaps changes; the directive cases are the tests themselves.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
scripts/test.sh --project architecture
```

On `3fd9b4d` with these tests: analyzer filter 23 failed (the red tests above, LF and CRLF counted separately), 423 passed; architecture 117/117.
