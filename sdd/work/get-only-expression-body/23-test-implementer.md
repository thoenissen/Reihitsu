Role: test implementer
Commit: the commit that adds this handover (parent 4040015, the spec author's `Restate SHARED-GETONLY-008 comment gap by its cause`, committed while I worked; the test run below was on c1301ff plus that spec edit in the working tree, and touches no spec file)
Verdict: tests written

Decision 18 (`00-lead.md`), auditor findings 4 and 5 (`21-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict. Only the two analyzer test files and this handover changed. Tags: SHARED-GETONLY-008, as the prompt directed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3209 `DocumentationCommentBetweenTheMemberAndAMemberBehindItOnTheSameLineWithholdsFix` (`/** c */`; Parse, Diagnose, None x LF, CRLF) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` + `ConvertEndOfLine` | Parse/Diagnose: fail until change (fix offered); None: pass |
| RH3209 `DocumentationCommentBetweenTheMemberAndAMemberInFrontOfItOnTheSameLineWithholdsFix` (same matrix) | SHARED-GETONLY-008 | same | Parse/Diagnose: fail until change; None: pass |
| RH3209 `MultiLineCommentBetweenTheMemberAndAMemberBehindItWithholdsFix` (`/* a` / `b */`, LF, CRLF) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` (Parse) + `ConvertEndOfLine` | Fail until change (fix offered) |
| RH3209 `MultiLineCommentBetweenTheMemberAndAMemberInFrontOfItWithholdsFix` (LF, CRLF) | SHARED-GETONLY-008 | same | Fail until change |
| RH3209 `MultiLineCommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix` (LF, CRLF; other side of the condition: a real end of line after the multi-line comment) | SHARED-GETONLY-005, SHARED-GETONLY-008 | `VerifyCodeFixWithEndOfLine` | Pass |
| RH3210 `DocumentationCommentBetweenTheIndexerAndAMemberBehindItOnTheSameLineWithholdsFix` (Parse, Diagnose, None x LF, CRLF) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` | Parse/Diagnose: fail until change; None: pass |
| RH3210 `DocumentationCommentBetweenTheIndexerAndAMemberInFrontOfItOnTheSameLineWithholdsFix` (same matrix) | SHARED-GETONLY-008 | same | Parse/Diagnose: fail until change; None: pass |
| RH3210 `MultiLineCommentBetweenTheIndexerAndAMemberBehindItWithholdsFix` (LF, CRLF) | SHARED-GETONLY-008 | `VerifyNoCodeFixIsOffered` (Parse) | Fail until change |
| RH3210 `MultiLineCommentBetweenTheIndexerAndAMemberInFrontOfItWithholdsFix` (LF, CRLF) | SHARED-GETONLY-008 | same | Fail until change |

Observed on this head: 148 tests, 124 pass, 24 fail. Every failure is `No code fix may be offered for the diagnostic at …` (the fix is offered): 8 Parse/Diagnose documentation-comment cases and 4 multi-line-comment cases per rule. The 8 `DocumentationMode.None` cases (where `/** c */` lexes as an ordinary delimited comment) and the 2 keep-fix cases pass.

Finding 5's multi-line inputs are the two `MultiLineComment…WithholdsFix` inputs per rule: the auditor observed them fixed today, equal to the formatter. Decision 18 refuses them, so these tests assert the withholding instead of pinning the current fix output; they flip from green-equivalent behavior to the decided behavior and pin the boundary finding 5 says is untested.

No existing test asserts an output decision 18 changes: `rg "/\* a|/\*\* c" src/Reihitsu.Analyzer.Test/Formatting/RH32(09|10)*` found none before this change; the existing `/* c */` gap tests stay withheld.

## Scenarios (bug fix)
_Not applicable (architecture finding, behavior decided)._

## Not covered, and why
- Mirror of the keep-fix test (multi-line comment then end of line, sibling in front): the member then starts on its own line, the suite's ordinary case.
- RH3210 keep-fix multi-line variant: one shared base (`GetOnlyMemberExpressionBodyCodeFixProviderBase.CanConvert`), RH3209 covers the condition's other side; RH3210 has the original `/* c */` pair.
- CR-only `EndOfLineTrivia` (finding 5 aside): the test base converts only LF/CRLF; a CR-only document is no supported line ending of the test helpers.
- Directive or disabled text in the gap: always ends its line, cannot share one (unchanged from 15).
- Fix All with a withheld member next to a fixed one: the guard decides per diagnostic on the current document (SHARED-FIX-002, auditor), and the Fix All tests of 12 stay.
- FC catalog: unchanged from `12-test-implementer.md`; FC-040 (documentation mode) is now covered by the three-mode rows.

## Commands
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests|FullyQualifiedName~RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests"

## Follow-up candidates
_None new._ The RH3202 family's comment-gap behavior (documentation-comment cell included) is already in `00-lead.md`.
