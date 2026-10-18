Role: test implementer
Commit: 1909e47 (spec repair in `05-spec-author.md`; analyzer, code-fix, and production sources unchanged)
Verdict: tests written

Repair round for the RH5103-003 and RH5104-004 wording changes of `05-spec-author.md`. The coordinator's prompt asked for a re-check of every test tagged RH5103-003 or RH5104-004. It carried no intended fix or verdict.

## Re-check

### RH5103-003

I read the input of every test tagged RH5103-003 against the new text.

The two tags the spec author named fall outside the requirement now. I confirmed this from their inputs:

| Test | Label column | Required by scope depth | Result |
|---|---|---|---|
| `SwitchSectionChainInsideObjectInitializerFixAllKeepsLabelRelativeIndentation` | 58 | 16 | Line not indented as its scope depth requires; tag removed |
| `SwitchSectionChainWithoutAnchorScopeUsesCanonicalIndentationRegardlessOfLabelsOwnColumn` | 20 | 12 | Line not indented as its scope depth requires; tag removed |

Both tests carried no other tag, so they are untagged now. They stay as characterization tests.

The remaining tags match the new text:

- **Predecessor starts its line:** the moved statement keeps the line's indentation.
  - `VerifyMultipleStatementsOnOneLineAreDetectedAndFixed`
  - `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine`
  - `ThreeStatementChainFixAllKeepsBlockIndentation`
  - `TabIndentedBlockChainFixAllPreservesExactIndentation`
- **Switch-section line indented as its depth requires:** the moved statement goes to the section's statement column.
  - Line starts with a comment: `VerifySplitStatementKeepsSiblingColumnWhenPrecedingLineStartsWithComment` and `FixAllKeepsSiblingColumnWhenPrecedingLineStartsWithComment`.
  - Line starts with another section's label: `VerifySplitStatementCompensatesWhenLineStartsWithAnotherSectionsLabel`.
  - Comment in front of the own label: `VerifySplitStatementCompensatesWhenCommentPrecedesTheLabelOnItsLine`.
  - Line starts with another section's statement: `VerifySplitStatementDoesNotCompensateWhenLineStartsWithAnEarlierSiblingSectionsStatement`.
  - Own label line: `VerifyStatementBehindStatementOnCaseLabelLineIsFixedToSectionIndentation`.
- These shapes were already covered, but only inside an anchor scope (a lambda in an object initializer). The new spec example is a plain method, so I added it verbatim as a test.
- `VerifySplitStatementOverIndentsWhenLabelSpansMultipleLinesAndStatementSharesItsContinuationLine` (#1096) and `VerifySplitStatementDoesNotCompensateWhenSwitchLabelIsNotFirstOnItsLine` were untagged already. Both stay untagged: their inputs are the excluded multi-line label, and the excluded label on the brace line.

### RH5104-004

All nine tags match the new text. The comments that code follows on their last line keep the whitespace in front of them:

- `VerifyInlineBlockCommentInExpressionIsDetectedAndFixed` gives `if (left == right)`.
- `VerifyLeadingBlockCommentInFrontOfCodeIsDetectedAndFixed` keeps the line's indentation.
- `VerifySingleLineInterpolatedStringHoleCommentIsStillDetectedAndFixed` has no whitespace on either side of the comment.

The comments at the end of their line lose the whitespace in front of them:

- `VerifyTrailingSingleLineCommentIsDetectedAndFixed`
- `VerifyTrailingMultiLineCommentIsDetectedAndFixed`
- `VerifyTrailingCommentBehindFieldIsDetectedAndFixed`
- the CRLF test

`VerifyLineBreakCarryingCommentRemovalDoesNotJoinStatements` covers the multi-line clause.

I added a test for the new example's shape, a comment directly in front of `)`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5103…Tests.VerifyStatementsInSwitchSectionsAreFixedToTheSectionStatementColumn`: the RH5103-003 spec example verbatim (statement line, `case 1:` line, own-line predecessor under `case 2:`, `/* three */ case 3:`, `case 4: break; case 5:`, `break; case 7:`), 6 diagnostics, fixed one by one and with Fix All | RH5103-001, RH5103-003 | `Verify(source, fixed, diag×6)` | pass |
| `RH5104…Tests.VerifyBlockCommentInFrontOfClosingParenthesisKeepsTheWhitespaceInFrontOfIt`: `Method(value /* value */);` becomes `/* value */` / `Method(value );` | RH5104-001, RH5104-004 | `Verify(source, fixed, diag)` | pass |
| Removed tag RH5103-003 from the two misindented-label tests above | RH5103-003 scope | — | pass (untagged) |

- After the change, 11 tests carry RH5103-003 and 9 carry RH5104-004.
- `scripts/format.sh` over both files formatted 0 of 2 files.

## Not covered, and why

- **The "otherwise … the whitespace behind it kept" half of RH5104-004.** The spec author's probe for it is `N(a); /* d */` with trailing spaces. A test would have to carry trailing whitespace inside a raw-string literal of the test source, so I did not add one. The half where code follows the comment is now covered.
- **The excluded RH5103-003 shapes** have no output assertion, as the spec states: a multi-line label (#1096), a label on the opening brace's line, and a line not indented as its depth requires (#1116 Q4). Their existing tests stay untagged characterization tests.
- **The SHARED-FIX-002 question of `05-spec-author.md`** is for the lead to file. No test change.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5103|FullyQualifiedName~RH5104"
scripts/test.sh --project architecture
```

Results:

- RH5103/RH5104 filter: 50 passed, 0 failed.
- Architecture: 72 passed, 0 failed, including `RequirementCoverageTests`.
