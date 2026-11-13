Role: test implementer
Commit: (this commit; base d734fe7)
Verdict: tests written

Repair round 3, tests only: lead decision 33 (auditor 19 finding 8, shapes a–d). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values: for the refused shapes (a)–(d), `origin/main`'s output (`d6a01ee`), derived in a detached worktree with `scripts/apply-fix.sh RH5417|RH3202 <fixtures> --max-iterations 3`, under LF and CRLF (both arms identical, 1 action, 1 iteration each); the worktree was removed afterwards. The refusal is stated by RH5417-003 and RH3202-002 ("only when the line on which the member starts begins with a member of that type … MUST NOT separate"), so the tests carry those tags. For the separated side (the member is the line's first token behind a comment that starts on that line), the expected value is SHARED-FIX-007's shared line: the member behind starts on the next line, in the column of the line (4). `scripts/format.sh` leaves that output's layout unchanged (it only drops the final newline of the scratch file).

## thoenissen/Reihitsu#1144 (shared by #1140)

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5417…AnalyzerTests.FixLeavesTheMemberOnALineThatBeginsWithTheContinuationOfASiblingsInitializer` (a) | RH5417-003 | `VerifyUnderBothLineEndings` (`Verify`, LF + CRLF) | red |
| `RH5417…AnalyzerTests.FixLeavesTheMemberOnALineThatBeginsWithAStatementOfASiblingsBody` (b) | RH5417-003 | same | red |
| `RH5417…AnalyzerTests.FixLeavesTheMemberOnALineThatBeginsWithTheClosingLineOfASiblingsVerbatimString` (c) | RH5417-003 | same | red |
| `RH5417…AnalyzerTests.FixLeavesTheMemberBehindOnALineThatBeginsWithTheTailOfAMultiLineComment` (d) | RH5417-003 | same | red |
| `RH5417…AnalyzerTests.FixSeparatesTheMemberBehindWhenTheFixedMemberBeginsItsLineBehindAComment` (other side) | RH5417-003, SHARED-FIX-007 | same | green |
| `RH3202…AnalyzerTests.FixLeavesTheMethodOnALineThatBeginsWithTheContinuationOfASiblingsInitializer` (a) | RH3202-002 | same | red |
| `RH3202…AnalyzerTests.FixLeavesTheMethodOnALineThatBeginsWithAStatementOfASiblingsBody` (b) | RH3202-002 | same | red |
| `RH3202…AnalyzerTests.FixLeavesTheMethodOnALineThatBeginsWithTheClosingLineOfASiblingsVerbatimString` (c) | RH3202-002 | same | red |
| `RH3202…AnalyzerTests.FixLeavesTheMemberBehindOnALineThatBeginsWithTheTailOfAMultiLineComment` (d) | RH3202-002 | same | red |
| `RH3202…AnalyzerTests.FixSeparatesTheMemberBehindWhenTheConvertedMethodBeginsItsLineBehindAComment` (other side) | RH3202-002, SHARED-FIX-007 | same | green |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| (a) `                     2); void M() { }` / `2); int Get() => _a;` | LF, CRLF | code fix, test | red | observed `2);⏎                     void M()⏎                     {…}` (column 21); expected `2); void M()⏎                         {⏎                         }` (`main`) |
| (b) `        A(); } void M() { }` / `A(); } int Get() => _a;` | LF, CRLF | code fix, test | red | observed `A(); }⏎        void M()⏎        {…}` (column 8); expected `A(); } void M()` with the braces under `void` (column 15) (`main`) |
| (c) `b"; void M() { }` / `b"; int Get() => 1;` | LF, CRLF | code fix, test | red | observed `b";⏎void M()⏎{⏎}` (column 0); expected `b"; void M()⏎    {⏎    }` (`main`) |
| (d) `       b */ void M() { } int _y;` / `b */ int Get() => _a; int _y;` | LF, CRLF | code fix, test | red | observed `    }⏎       int _y;` (column 7); expected `    } int _y;` (`main`) |
| `    /* c */ void M() { } int _y;` / `/* c */ int Get() => _a; int _y;` | LF, CRLF | code fix, test | green | head separates: `    }⏎    int _y;`; `main` kept `    } int _y;` |

The red tests fail on the separation difference only (`Verify`'s diff, checked for (c) both rules and (d) RH5417; the other diffs equal the head `apply-fix.sh` output above). No compile error, no unrelated layout.

## #1140, #1141, #1145, #1094, #1393, #1511, #1561

Nothing in this round.

## Whole run

Red now (8): the four refused shapes (a)–(d) of each rule. Green: every other test of the two classes, the two new separated-side tests included (93 of 101).

### Observations for the spec checker

- RH5417-003 and RH3202-002 say the line must "begin with a member"; decision 33 counts comments in front of the line's first token when their text starts on that line, and refuses a comment tail that started on an earlier line (shape d). The spec text does not say how a comment in front of the member counts. The tests follow decision 33 and are tagged with the two requirements; whether their wording should name the comment case is the spec author's.
- Shape (d) and the separated-side test pin the same member position behind a comment; only where the comment starts differs, so together they are the two sides of the comment condition (FC-003).

### Not covered, and why

- RH5513 (attribute base, auditor: shape (a) separates `[Obsolete]` at column 4): outside decision 33's refusal, which names RH5417/RH3202's shared-line helper; no test.
- The separated side for "a member in front of the fixed member begins the line" is already covered by `FixGivesTheFixedMemberAndTheMemberOnItsLineLinesOfTheirOwn` (`int _y; void P() { }`) in RH5417 and `FixPutsAMemberBehindTheExpressionBodyOnTheNextLine` in RH3202.
- An attribute list as the line's first token (`[System.Obsolete] int Get() => _a; int _b;`, auditor: holds): the first token of the member is the attribute's `[`, the same token-equality as the plain member; not added.
- A directive cannot share a member's line (auditor FC-020); no test.
- Fix All: the refused side changes nothing between members, as in round 2.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ALineThatBeginsWith|FullyQualifiedName~BeginsItsLineBehindAComment"
```
