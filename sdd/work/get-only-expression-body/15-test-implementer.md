Role: test implementer
Commit: the commit that adds this handover (parent d2c3711; spec text of 14-spec-author was uncommitted in the working tree when I ran)
Verdict: tests written

Repair round 1, decisions 14 and 15 (`00-lead.md`). My prompt carried no intended fix or hoped-for verdict. Only the two test files and this handover changed. Requirement tags follow `14-spec-author.md`: SHARED-GETONLY-008 (added in parallel), not the provisional SHARED-FIX-007 / SHARED-GETONLY-003 my prompt offered as fallback.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3209 `CommentBetweenTheMemberAndAMemberBehindItOnTheSameLineWithholdsFix` (LF, CRLF; replaces `FixPutsAMemberBehindADelimitedCommentBehindTheConvertedMemberOnTheNextLine`, auditor e) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` + `ConvertEndOfLine` | Fail until repair (fix offered) |
| RH3209 `CommentBetweenTheMemberAndAMemberInFrontOfItOnTheSameLineWithholdsFix` (LF, CRLF; replaces `FixPutsTheConvertedMemberBehindADelimitedCommentOnTheNextLine`, mirror of e) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` + `ConvertEndOfLine` | Fail until repair (fix offered) |
| RH3209 `CommentBetweenTheMemberAndAMemberBehindItInATypeWrittenOnOneLineWithholdsFix` (LF, CRLF; "whether or not the line begins with a member") | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` + `ConvertEndOfLine` | Fail until repair (fix offered) |
| RH3210 `CommentBetweenTheIndexerAndAMemberBehindItOnTheSameLineWithholdsFix` (LF, CRLF) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` + `ConvertEndOfLine` | Fail until repair (fix offered) |
| RH3209 `CommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix` (LF, CRLF; spec example 4, other side of the comment-gap condition) | SHARED-GETONLY-005, SHARED-GETONLY-008 | `VerifyCodeFixWithEndOfLine` | Pass |
| RH3209 `VerifySpecificationExampleOfMembersSharingTheirLine` (LF, CRLF; spec example 1, iterative and Fix All) | SHARED-FIX-007, SHARED-GETONLY-008 | `Verify` + `ConvertEndOfLine` | Fail until repair (shared line kept) |
| RH3209 `VerifySpecificationExampleOfAMemberOfATypeWrittenOnOneLine` (LF, CRLF; spec example 2, member in front only) | SHARED-GETONLY-008 | `VerifyCodeFixWithEndOfLine` | Pass |
| RH3209 `FixLeavesTheMembersOnTheLineOfAMemberOfATypeWrittenOnOneLine` (LF, CRLF; members in front and behind) | SHARED-GETONLY-008 | `VerifyCodeFixWithEndOfLine` | Pass |
| RH3210 `FixLeavesTheMembersOnTheLineOfAnIndexerOfATypeWrittenOnOneLine` (LF, CRLF; members in front and behind) | SHARED-GETONLY-008 | `VerifyCodeFixWithEndOfLine` | Pass |

Retagged, assertions unchanged: the shared-line tests of `12-test-implementer.md` that remain (RH3209 `FixPutsAMemberBehindTheConvertedMemberOnTheNextLine`, `FixPutsTheConvertedMemberOnTheLineAfterAMemberInFrontOfIt`, `FixPutsAMultiLineConvertedMemberOnTheLineAfterAMemberInFrontOfIt`, `FixPutsAMemberBehindTheClosingBraceOnTheNextLineWhenTheSemicolonCarriesAComment`, `FixAllPutsTwoConvertedMembersOfOneLineOnLinesOfTheirOwn`; RH3210 `FixPutsAMemberBehindTheClosingBraceOnTheNextLine`, `FixPutsTheConvertedIndexerOnTheLineAfterAMemberInFrontOfIt`, `FixAllPutsTwoConvertedIndexersOfOneLineOnLinesOfTheirOwn`) now carry SHARED-GETONLY-008 next to their existing tags, since its first paragraph is the requirement they verify for these rules. The implementer has not yet worked against them in this round.

Observed on this head: 114 tests, 88 pass, 26 fail. The failures are the 16 shared-line test cases of 12 that remain, the 8 comment-gap cases above (each fails on `No code fix may be offered for the diagnostic at …`), and the 2 cases of spec example 1 (shared line kept). The four one-line-type cases and the two "sibling on the next line" cases pass, as the spec author observed with `scripts/apply-fix.sh`.

## Scenarios (bug fix)
_Not applicable (behavior change, repair round)._

## Not covered, and why
- Other non-member line starts of RH3202-002 (sibling's wrapped initializer continuation, statement inside a sibling's body, tail of a multi-line literal or comment): SHARED-GETONLY-008 reaches them only through the helper RH3202-002 / RH5417-003 already test (decision 15: the fix reuses that helper); one one-line-type test per rule shows the get-only fix goes through it. If the repair adds its own line-start check instead of reusing the helper, these need tests.
- Comment gap with a line break in it (`/* c */` then sibling on the next line in front of the member): the "behind" direction is covered (spec example 4); the mirror is the existing suite's ordinary own-line case.
- Single-line comment or directive in the gap: cannot occur on a shared line (spec: a single-line comment ends its line, a directive stands on its own line).
- RH3210 one-line type with a comment gap: same condition as RH3209's, one shared base (`GetOnlyMemberExpressionBodyCodeFixProviderBase`); RH3210 has the plain comment-gap test.
- FC catalog: unchanged from `12-test-implementer.md`.

## Commands
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests|FullyQualifiedName~RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests"

## Follow-up candidates
_None new._ The RH3202 family's comment-gap behavior is already in `00-lead.md`.
