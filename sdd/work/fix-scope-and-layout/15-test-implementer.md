Role: test implementer
Commit: (this commit; base e846caa)
Verdict: tests written

Repair round 2, tests only: decisions 27, 28, 30 (auditor 13 findings 1, 2, 4), against the spec author's 14 (e846caa). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values: decision 27 from RH5203-003 as amended in e846caa (no fix offered; its new example). Decision 28 from the new RH5417-003 and RH3202-002 paragraphs and their examples, cross-checked against `main`: a detached worktree at `origin/main` (`d6a01ee`), `scripts/apply-fix.sh RH5417|RH3202` over the spec examples and `class C { void M() { } int _x; }`, `class C { int _x; void M() { } }`, under LF and CRLF. Every `main` output equals the spec example's output and the expected value of the tests. Finding 4 (comment in the gap): no requirement states it; the expected values are `main`'s output, which is also the head's, so these tests are untagged characterizations of the comment side of `SharesLineWithSiblingMember` (FC-003, FC-020).

## thoenissen/Reihitsu#1140

Covered through #1144 below (RH5417/RH3202 shared line); nothing of its own in this round.

## thoenissen/Reihitsu#1141

Nothing in this round.

## thoenissen/Reihitsu#1144

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5203…AnalyzerTests.NoFixIsOfferedWhenADelimitedCommentStandsInFrontOfTheArgumentOnItsLine` (spec example, `/* two */ 2`) | RH5203-003 | `VerifyNoCodeFixIsOffered`, LF + CRLF | red (fix offered) |
| `…NoFixIsOfferedWhenADelimitedDocumentationCommentStandsInFrontOfTheArgumentOnItsLine` (`/** two */ 2`) | RH5203-003 | same | red |
| `…NoFixIsOfferedWhenADelimitedCommentBelowACommentLineStandsInFrontOfTheArgumentOnItsLine` (`// one⏎ /* two */ 2`) | RH5203-003 | same | red |
| `…NoFixIsOfferedWhenADelimitedCommentIsWrittenDirectlyAgainstTheArgument` (`/*two*/2`) | RH5203-003 | same | red |
| `…NoFixIsOfferedWhenTheArgumentStandsBehindTheEndOfAMultiLineComment` (`/* a⏎ b */ 2`) | RH5203-003 | same | red |
| `…FixMovesADelimitedCommentLineAboveTheArgumentIntoTheAlignmentColumn` (other side: `/* two */` on its own line, argument starts its line) | RH5203-003 | `Verify`, LF + CRLF | green |
| `…FixLeavesADelimitedDocumentationCommentLineAboveTheArgumentInItsColumn` (`/** two */` on its own line stays, finding 5 / amended sentence) | RH5203-003 | `Verify`, LF + CRLF | green |
| `RH5417…AnalyzerTests.FixLeavesTheMembersOnTheLinesOfAMemberOfATypeWrittenOnOneLine` (spec example, namespace) | RH5417-003 | `Verify`, LF + CRLF | red |
| `RH5417…AnalyzerTests.FixLeavesTheMemberInFrontOnTheLineOfAMemberOfATypeWrittenOnOneLine` (`class C { int _x; void M() { } }`) | RH5417-003 | `Verify`, LF + CRLF | red |
| `RH5417…AnalyzerTests.FixKeepsACommentBetweenTheFixedMemberAndTheMemberBehindItAndDoesNotSeparateThem` | FC-003, FC-020 | `Verify`, LF + CRLF | green |
| `RH5417…AnalyzerTests.FixKeepsACommentBetweenTheMemberInFrontAndTheFixedMemberAndDoesNotSeparateThem` | FC-003, FC-020 | `Verify`, LF + CRLF | green |
| `RH3202…AnalyzerTests.FixLeavesTheMembersOnTheLinesOfAMethodOfATypeWrittenOnOneLine` (spec example, sibling in front and behind) | RH3202-002 | `Verify`, LF + CRLF | red |
| `RH3202…AnalyzerTests.FixKeepsACommentBetweenTheConvertedMethodAndTheMemberBehindItAndDoesNotSeparateThem` | FC-003, FC-020 | `Verify`, LF + CRLF | green |
| `RH3202…AnalyzerTests.FixKeepsACommentBetweenTheMemberInFrontAndTheConvertedMethodAndDoesNotSeparateThem` | FC-003, FC-020 | `Verify`, LF + CRLF | green |

New private helpers: `RH5203…VerifyNoCodeFixIsOfferedUnderBothLineEndings`, `RH3202…VerifyUnderBothLineEndings` (same shape as the existing RH5417/RH5203 helpers).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `M(1,⏎           /* a */ 2);` and the four siblings above | LF, CRLF | code fix (`apply-fix.sh RH5203`) | head: `not-converged (10)`; `main`: `no-progress (2)` | head appends alignment spaces behind the comment on every application (`/* a */` + ~100 spaces + `2`); expected: no fix offered |
| RH5417 spec example | LF, CRLF | code fix, test | red | observed `}⏎    int _x; }` (separated, `int _x;` at the type's column 4); expected `} int _x; }` (`main`) |
| `class C { int _x; void M() { } }` | LF, CRLF | code fix, test | red | observed `class C { int _x;⏎void M()⏎{⏎} }` (column 0); expected `class C { int _x; void M()⏎                  {⏎                  } }` (`main`) |
| RH3202 spec example | LF, CRLF | code fix, test | red | observed `int _value;⏎    public int GetValue()⏎    {…}⏎    private int _x; }`; expected the spec example (`main`) |
| Comment in the gap, RH5417 both sides, RH3202 both sides | LF, CRLF | code fix | green | head equals `main`: comment kept in place, members not separated |

## thoenissen/Reihitsu#1145

Nothing in this round.

## thoenissen/Reihitsu#1094

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5513…AnalyzerTests.FixKeepsACommentBetweenAFieldAndTheAttributeListAndDoesNotSeparateThem` (`int _a; /* c */ [Obsolete] void M()`) | FC-003, FC-020 | `Verify`, LF + CRLF | green |

Observed (head and `main` alike): `int _a; /* c */ [Obsolete]⏎    void M()`: the list stays behind the comment, only the method moves (RH5513-004). The whitespace-only side is the existing `FixPutsTheAttributeListBehindAFieldOnALineOfItsOwn`.

## thoenissen/Reihitsu#1393

Nothing in this round.

## thoenissen/Reihitsu#1511

Nothing in this round.

## thoenissen/Reihitsu#1561

Nothing in this round.

## Whole run

Red now (8, all for the requirement's own difference, none for a compile error or unrelated layout):
- RH5203: the five `NoFixIsOffered…` tests (`Assert.IsEmpty(actions)`: a fix is offered). Decision 27.
- RH5417: `FixLeavesTheMembersOnTheLinesOfAMemberOfATypeWrittenOnOneLine`, `FixLeavesTheMemberInFrontOnTheLineOfAMemberOfATypeWrittenOnOneLine`; RH3202: `FixLeavesTheMembersOnTheLinesOfAMethodOfATypeWrittenOnOneLine`. Decision 28; diffs above.

Every other test in the four classes passes (138 of 146).

### Not covered, and why

- Finding 3 (sibling behind the attribute-placement member) is a follow-up (decision 29); no test.
- Finding 6 (SHARED-ATTR-021 wording, switch section): spec text only; the existing `RH5513…VerifyCodeFixIndentsInsideBlockOfSwitchSection` already covers the switch-section column the amended wording now states.
- A directive in the shared-line gap: a directive cannot share a member's line (auditor FC-020), so no test.
- A one-line type whose line begins with a sibling of an outer list (nested one-line types): rarer than the decision-28 class and decided by the same condition (the member's line begins with a member of the same list); not added.
- Fix All for the decision-28 class: a type written on one line with two reported members would put both on one line again; the existing `GetFixAllScenario` covers the separated side, and the refused side changes nothing between members, so no batch interaction beyond `main`'s.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~TypeWrittenOnOneLine|FullyQualifiedName~NoFixIsOfferedWhen"
```

## Amendment (lead decision 32)

`RH5417…FixLeavesTheMemberInFrontOnTheLineOfAMemberOfATypeWrittenOnOneLine`: the input `class C { int _x; void M() { } }` held no line break, so its CRLF half converted nothing and compared the formatter's environment newline with a CRLF expectation. The input and the expected output now start with `using System;` and a blank line, so both halves carry their line ending. What the test asserts about the members is unchanged: `int _x;` and `void M()` stay on the type's line, and the braces are laid out from the member's column. The RH5417 class passes on fa0083d plus this change (52 of 52).
