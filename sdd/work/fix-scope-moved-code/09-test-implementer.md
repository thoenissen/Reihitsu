Role: test implementer
Commit: a85811e
Verdict: tests written

How the statements below were established: `00-lead.md` (decisions 19–25), `05-test-implementer.md`, `07-implementer.md`, `08-spec-author.md` and the spec diff of 840e68b read. Every test below was run on a85811e (no production change): `scripts/test.sh --configuration Debug --project analyzer` over the RH3004, RH7004, RH7103, RH7109, RH7303 and RH7310 classes gives `Failed: 8, Passed: 676`. The 8 failing rows are the four new or changed tests marked "fails" below, and each fails on the spec's own difference. Where no fix is expected, the fix is offered (`Assert.IsEmpty(actions)`). For the RH7004 code line, the only diff line is `/* x */ s.Flush();` staying in column 0 instead of column 4. `--project core`: 651 passed (655 before, four tests deleted). `--project architecture`: 117 passed, requirement coverage included. Files formatted with `scripts/format.sh` (no change). Ignored input: none; my prompt carried the lead's decisions and process only.

## Whole run

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Core `OrderingDeclarationUtilitiesTests.MoveMemberBeforeMovesMemberToTheRequestedPosition`, `MoveMemberBeforeKeepsSeparatorAtItsOriginalPosition` (and the field `_movedMemberOrder`) | deleted (decision 20) | — | — |
| Core `AccessorOrderingUtilitiesTests.MoveAccessorBeforeMovesAccessorToTheRequestedPosition`, `MoveAccessorBeforeKeepsSeparatorAtItsOriginalPosition` | deleted (decision 20) | — | — |

Decision 20: the four deleted tests carried no requirement tag. What they covered is the order after the move and the separator staying at its slot (SHARED-ORDER-002). The analyzer tests of RH7102/RH7103/RH7107/RH7108/RH7109 cover both through the new helpers (12 SHARED-ORDER-002 tags in `Reihitsu.Analyzer.Test`). No retarget was needed. `MoveNodeBeforePreservingSeparators` had no direct Core test. After the deletion, `rg "MoveNodeBeforePreservingSeparators|MoveMemberBefore|MoveAccessorBefore" src --glob '*Test*/**'` finds nothing, so the implementer can delete the helpers without touching a test.

## thoenissen/Reihitsu#969

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7103 `NoCodeFixWhenADocumentationCommentBehindCodeDocumentsTheMovedDeclaration` (Parse, Diagnose); replaces `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (spec example) | SHARED-DOC-004, SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered(source, documentationMode)` | fails (fix offered) |
| RH7103 `NoCodeFixWhenADocumentationCommentBehindCodeAndAConditionalPairPrecedeTheMovedDeclaration` (Parse, Diagnose); replaces `FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration` (spec example) | SHARED-DOC-004, SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered(source, documentationMode)` | fails (fix offered) |
| RH7103 `FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed` (unchanged; the `None` side of the condition) | as tagged before | — | passes (must keep holding) |
| RH7109 `NoCodeFixWhenALeftShiftWouldCutAMovingLineOffAtColumnZero` (spec example: `// two columns` at column 2, shift -4) | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| RH7109 `LeftShiftMovesALineWithJustEnoughWhitespaceIntoColumnZero` (LF, CRLF): the other side, `// four columns` at column 4, shift -4, moves to column 0 | SHARED-ORDER-010, SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7109 `NoCodeFixWhenALeftShiftRemovesMoreColumnsThanATabAtTheStartOfAMovingLine`: a continuation line `\t+ 2;` (one character), shift -4 | SHARED-ORDER-010, SHARED-FIX-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| RH7109 `RightShiftWritesSpacesInFrontOfATabAtTheStartOfAMovingLine` (LF, CRLF): field two columns left of its slot; `\t+ 2;` becomes two spaces + `\t+ 2;` | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7109 `NoCodeFixWhenADocumentationCommentBehindCodeDocumentsTheTarget` (Parse, Diagnose) (spec example) | SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered(source, documentationMode)` | passes (must keep holding) |
| RH7109 `CrossedFieldsThatShareALineKeepItBehindTheMovedField` (LF, CRLF) (spec example) | SHARED-ORDER-009 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |

## thoenissen/Reihitsu#1335

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7004 `CodeLineWithAColumnZeroCommentInFrontOfItMovesWithTheStatements` (LF, CRLF) (spec example) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails (line kept in column 0) |
| RH7004 `LineOfOnlyCommentsStartingWithADelimitedCommentInColumnZeroKeepsItsColumn` (LF, CRLF): the other side, `/* x */ // s.Close();` holds no code and keeps column 0 | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7004 `NoCodeFixForCodeInFrontOfTheUsingDeclarationOnItsLine` (spec example, `int a = 0; using …`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (must keep holding) |
| RH7004 `NoCodeFixForTheOpeningBraceOfTheBlockInFrontOfTheUsingDeclarationOnItsLine` (`{ using …`, named in the requirement) | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (must keep holding) |
| RH7004 `NoCodeFixWhenTheLastMovedStatementEndsOnTheLineOfTheClosingBrace` (spec example) | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (must keep holding) |
| RH7004 `NoFixIsOfferedForACommentInFrontOfTheSemicolon`: retagged SHARED-FIX-010 → RH7004-007, and its summary now gives the RH7004-007 reason instead of the formatter's refusal (decision 21). Body unchanged | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (must keep holding) |
| RH7004 `NoFixIsOfferedForADirectiveInFrontOfTheSemicolon` (`#pragma warning disable` in the leading trivia of `;`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (must keep holding) |

Retagging the semicolon test leaves SHARED-FIX-010 with other covering tests: the architecture coverage test passes.

## thoenissen/Reihitsu#1263

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7303 `CodeFixKeepsTheBlankLineInFrontOfTheRemovedRegionWhereTheRemovalJoinsTwoBlankLines` (LF, CRLF). The blank line in front of `#region` holds 2 spaces and the one behind holds 8; the 2-space line stays | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7310 `CodeFixKeepsTheBlankLineInFrontOfTheRemovedRegionWhereTheRemovalJoinsBlankLines` (LF, CRLF). Blank lines of 2, 8 and 6 spaces around `#region`/`#endregion`; only the 2-space line stays | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |

The existing RH7303 `CodeFixRemovesOneOfTwoWhitespaceOnlyBlankLinesTheRemovalLeavesDoubled` still accepts either line. I left it as it is, because the implementer has already worked against it. The new test pins the rule.

## thoenissen/Reihitsu#1151

No test changes. Decisions 19–25 do not touch RH7302.

## thoenissen/Reihitsu#1604

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3004 `FixMovesALaterStatementLineThatStartsWithAColumnZeroCommentByTheShift`: a later statement line `/* note */ Console.WriteLine(x + 1);` in column 0 moves by the shift (+16) | RH3004-006, SHARED-FIX-007 | `Verify` | passes (must keep holding) |

Decision 19: the RH3004 tests already cover these line classes:
- a comment line in column 0 stays (`FixKeepsAColumnZeroCommentLineInColumnZero`, `FixKeepsAColumnZeroCommentLineAfterASharedOpeningBraceLineInColumnZero`);
- a comment line in another column moves (`FixMovesACommentLineInAnotherColumnByTheMeasuredShift`);
- the *first* statement line with a column-0 comment moves (`FixMovesAFirstStatementLineThatStartsWithAColumnZeroComment…`);
- `///` lines (`FixMovesAMultiLineDocumentationCommentWithTheStatements`).

The untested class was a *later* code line with a column-0 comment in front of it. That line is where the RH3004 rule and the a85811e helper disagree, so the new test locks RH3004's output before its private shifter is replaced. Directive lines do not arise for RH3004 (RH3004-009). Every existing RH3004 test is unchanged.

## Not covered, and why

- Tab counting for an RH7004 or accessor move: SHARED-FIX-007 states one rule for member/accessor moves and RH7004, and decision 19 makes `SyntaxIndentationUtilities.TryShiftMovedLines` its one owner (callers `OrderingMoveSafety`, RH7004 per 07). RH7004's shift is always +4, a right shift, and a right shift only writes spaces, so no refusal can arise there. The RH7109 tab tests cover both directions through the shared helper. I did not test tabs for RH7107/RH7108 separately: they reach the same `TryCreateWholeLineMove` → `TryShiftMovedLines` path.
- Fix All: none of the changed behaviors adds a co-occurring instance. The existing `GetFixAllScenario` overrides stay.
- Formatter/CLI counterpart: no formatter behavior changes in this round.
- Failure classes for a declared unsupported shape (FC-038 to FC-043): this round declares no shape unsupported.
- The RH7004 semicolon refusal for a comment in the *trailing* trivia of the token in front of `;` (`new MemoryStream() /* c */;`): RH7004-007 names only the semicolon's leading trivia, and no spec example gives that output, so I did not invent an expected result.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004|FullyQualifiedName~RH7103|FullyQualifiedName~RH7109|FullyQualifiedName~RH7303|FullyQualifiedName~RH7310|FullyQualifiedName~RH3004"
scripts/test.sh --configuration Debug --project core
scripts/test.sh --configuration Debug --project architecture
```
