Role: test implementer
Commit: the commit that adds this handover (parent 1976629)
Verdict: tests written

Repair round 1: audit findings 1 and 2 (`11-auditor.md`, `00-lead.md` decisions 12 and 13). My prompt carried no intended fix or hoped-for verdict. Only test files and this handover changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3209 `DelimitedCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix` (spec example `B`) | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | Pass |
| RH3209 `SingleLineCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix` | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | Pass |
| RH3209 `DelimitedCommentOnALineOfItsOwnInFrontOfTheReturnSemicolonWithholdsFix` (block variant) | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | Pass |
| RH3210 `DelimitedCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix` | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | Pass |
| RH3210 `DelimitedCommentOnALineOfItsOwnInFrontOfTheReturnSemicolonWithholdsFix` | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | Pass |
| RH3209 `FixPutsAMemberBehindTheConvertedMemberOnTheNextLine` (LF, CRLF; auditor a; blank line above stays) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair |
| RH3209 `FixPutsTheConvertedMemberOnTheLineAfterAMemberInFrontOfIt` (LF, CRLF; auditor b, block getter) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair |
| RH3209 `FixPutsAMultiLineConvertedMemberOnTheLineAfterAMemberInFrontOfIt` (LF, CRLF; sibling in front, multi-line list) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair |
| RH3209 `FixPutsAMemberBehindTheClosingBraceOnTheNextLineWhenTheSemicolonCarriesAComment` (LF, CRLF; auditor d) | SHARED-FIX-007, SHARED-GETONLY-005 | `VerifyCodeFixWithEndOfLine` | Fail until repair (today `B` lands in column 0) |
| RH3209 `FixPutsAMemberBehindADelimitedCommentBehindTheConvertedMemberOnTheNextLine` (LF, CRLF; auditor e) | SHARED-FIX-007, SHARED-GETONLY-005 | `VerifyCodeFixWithEndOfLine` | Fail until repair — see *Not covered* note 1 |
| RH3209 `FixPutsTheConvertedMemberBehindADelimitedCommentOnTheNextLine` (LF, CRLF; mirror of e) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair — see note 1 |
| RH3209 `FixAllPutsTwoConvertedMembersOfOneLineOnLinesOfTheirOwn` (LF, CRLF; iterative and Fix All) | SHARED-FIX-007 | `Verify` + `ConvertEndOfLine` | Fail until repair |
| RH3210 `FixPutsAMemberBehindTheClosingBraceOnTheNextLine` (LF, CRLF; auditor c) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair |
| RH3210 `FixPutsTheConvertedIndexerOnTheLineAfterAMemberInFrontOfIt` (LF, CRLF) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | Fail until repair |
| RH3210 `FixAllPutsTwoConvertedIndexersOfOneLineOnLinesOfTheirOwn` (LF, CRLF; auditor f, iterative and Fix All) | SHARED-FIX-007 | `Verify` + `ConvertEndOfLine` | Fail until repair |

Observed on this head: both classes 100 tests, 80 pass, 20 fail — exactly the 10 shared-line tests above times two line endings. Each fails on the shared line itself (iterative step, expected versus actual), e.g. `public int this[int i] => i; public int this[long l] => (int)l;` on one line, or `public int B => 2;` written in column 0 behind `public int A => 1; // a`. No other existing test changed or failed.

Expected values: SHARED-FIX-007 *Shared line* (own line for the changed member, the other member on the next line, no blank line inserted per SHARED-FIX-006, as the RH5417 example shows) and SHARED-GETONLY-005 for the converted member (comment behind the closing brace or the getter's semicolon follows the new semicolon). For the two comment-gap tests the formatter's output on this head was checked (`scripts/format.sh --dry-run`): `public int A => 1; /* c */` / `public int B => 2;`, and `public int B => 2; /* c */` / `public int A => 1;`.

## Scenarios (bug fix)
_Not applicable (behavior change, repair round)._

## Not covered, and why
1. **Comment in the gap to the sibling — needs a lead decision.** The two tests with `/* c */` between the converted member and its sibling (auditor e and its mirror) assert what SHARED-FIX-007 and the formatter both give: the members are separated, the comment stays behind the first. The sibling path that decision 12 reuses, `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`, does not do this: `SharesLineWithSiblingMember` only breaks a gap that holds nothing but whitespace, and RH3202's untagged test `FixKeepsACommentBetweenTheConvertedMethodAndTheMemberBehindItAndDoesNotSeparateThem` asserts `} /* c */ private int _x;` kept on one line. So these two tests will stay red if the fix only adopts that wrapper. Either the lead drops/rewrites them (and the RH3202 family's gap becomes a follow-up against SHARED-FIX-007), or the repair covers the comment gap. I did not weaken them on my own (FC-017).
2. **Type written on one line** (`class C { private int _v; public int A { get => _v; } }`) — no test. SHARED-FIX-007's *Shared line* bullet as written has no exception for it; the exception (line does not begin with a member) is stated only in RH3202-002 and RH5417-003, not in SHARED-GETONLY or RH3209/RH3210. The auditor's "no break, as SHARED-FIX-007 states" is not what that bullet says. A test either way would pin an output no spec of this run decides; a question for the lead / spec author. Same for the other RH3202-002 non-member line starts (sibling initializer continuation, statement of a sibling's body, tail of a multi-line literal or comment).
3. Finding 2 tests pass on this head, as the auditor observed. I could not watch them fail without touching production code; each puts the comment only in the leading trivia of the getter's (or `return`'s) semicolon, which is the seam the auditor named, with an expression the FMT-UNSUP checks do not refuse.
4. Directive in the shared-line gap: not added. A directive forces a line break of its own, so no sibling can share the line across it.
5. FC catalog for the rest: unchanged from 05-test-implementer; the repair touches only the line of the fixed member (SHARED-FIX-007), covered on both sides (sibling in front and behind, single-line and multi-line list, comment behind semicolon), both rules, both line endings, iterative and Fix All. The "member with its line to itself" side is the existing suite.

## Commands
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests|FullyQualifiedName~RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests"

## Follow-up candidates
- RH3202 family (`FormatMemberOnLinesOfItsOwnAsync`) keeps a member and its sibling on one line when a comment sits in the gap (`FixKeepsACommentBetweenTheConvertedMethodAndTheMemberBehindItAndDoesNotSeparateThem`, `…MemberInFrontAndTheConvertedMethod…`), while SHARED-FIX-007 *Shared line* states no such exception and the formatter separates them — only if the lead keeps note 1's tests as written.
