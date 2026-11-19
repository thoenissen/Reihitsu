Role: spec checker
Commit: 79d4ebd
Verdict: findings

Repair round 1. Diff checked: `git diff origin/main...HEAD`, and the repair delta `48888c2..79d4ebd` (decisions 12 to 17; handovers 11 to 17). My prompt carried no intended fix or hoped-for verdict.

## thoenissen/Reihitsu#849

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/shared/get-only-expression-bodies.md` header (`Code owners`) and its row in `sdd/specs/README.md` | `13-spec-author.md` left this open for me: the repair put the signature-kept splice that SHARED-GETONLY-005 states into `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync` (`16-implementer.md`, af77361), and SHARED-GETONLY-008 paragraph 2 is implemented by the new `BlankLineCodeFixUtilities.SharesLineWithSiblingMemberAcrossComment` (called from `GetOnlyMemberExpressionBodyCodeFixProviderBase.CanConvert`). Neither type is a SHARED-GETONLY owner; 13 said an existing SHARED-BODY owner that hosts the splice is to be added to the header and the row. Low severity. Decide for `BlankLineCodeFixUtilities` too (it has no spec owner row for SHARED-FIX-007 either, so adding it is the spec author's call). | spec author |

### Checked
1. ok: change doc `get-only-expression-body-rules.md` stays `Accepted`, `Affected requirements` now reads SHARED-GETONLY-001 to -008, no task list, `Visible output changes` present. SHARED-GETONLY-008 withholds a fix for a rare class and applies the existing RH3202-002/RH5417-003 exception; per decisions 14 and 15 nothing visible is in question, and no new maintainer answer is required.
2. ok: SHARED-GETONLY-008 follows the format (one heading, normative sentence first, `Why:`, examples, `{|RH3209:…|}` markup). It matches the diff: `SharesLineWithSiblingMemberAcrossComment` is the cheap trivia check of paragraph 2, and the shared-line separation goes through `FormatMemberOnLinesOfItsOwnAsync` with its line-start exception. RH3209-001 and RH3210-001 `Uses` and prose name -008. No requirement contradicts the diff. Spec examples 1 to 4 are each tested.
3. ok: `SHARED-GETONLY-008` inserted after `-007` in `requirement-ids.txt` (sorted position); no ID reused, renumbered, or deleted.
4. finding 1. The rest ok: `SpecConsistencyTests` pass (all three types in the header exist); the `sdd/specs/README.md` row matches the header.
5. ok: no rule page changed in the repair; `RH3209.md`/`RH3210.md` carry no withheld-fix or shared-line note (right for -008). Diagnostic range and `helpLinkUri` pages unchanged and present.
6. ok: no output question in `12` to `17`; decisions 14 and 15 are recorded in `00-lead.md`; follow-up candidates (RH3202 comment-gap, trailing-whitespace copies) are under `Follow-up candidates`; nothing is written into the spec as a question.
7. ok: `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198". SHARED-GETONLY-008 is tagged on tests of both rules.
8. ok: every test added in the repair carries `[Requirement]` tags that match the `12` and `15` tables (SHARED-GETONLY-003, SHARED-GETONLY-005, SHARED-GETONLY-008, SHARED-FIX-007), including the split LF/CRLF cases; all four `Examples for the tests` of `14-spec-author.md` are tagged on a test (example 1 `VerifySpecificationExampleOfMembersSharingTheirLine`, 2 `VerifySpecificationExampleOfAMemberOfATypeWrittenOnOneLine`, 3 the comment-gap tests, 4 `CommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix`). The finding-2 tests (SHARED-GETONLY-003, own-line comment) now exist for both rules; the earlier optional note of `10` is closed.
9. ok: `sdd/work/` holds only `get-only-expression-body` (the lead deletes it before publishing). The `squad/notes/2026-11-18-fluent-chain-context-docs.md` in the delta comes from the merge of main, not from this run.

## thoenissen/Reihitsu#850

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | same header and row as #849 finding 1 | The shared spec's owners serve RH3210 too; same finding, not a separate one | spec author |

### Checked
1. ok: same change doc; the repair changes no RH3210 behavior beyond SHARED-GETONLY-008 (same base classes).
2. ok: RH3210-001 `Uses` and prose name -008; matches the indexer tests (`FixPutsAMemberBehindTheClosingBraceOnTheNextLine`, `FixPutsTheConvertedIndexerOnTheLineAfterAMemberInFrontOfIt`, `FixAllPutsTwoConvertedIndexersOfOneLineOnLinesOfTheirOwn`, `CommentBetweenTheIndexerAndAMemberBehindItOnTheSameLineWithholdsFix`, `FixLeavesTheMembersOnTheLineOfAnIndexerOfATypeWrittenOnOneLine`).
3. ok: ledger as above.
4. ok except the shared header (finding 1): the RH3210 spec's own owners exist; its README row unchanged.
5. ok: `RH3210.md` unchanged and conformant.
6. ok: as above.
7. ok: same architecture run (198 passed, 0 failed).
8. ok: indexer tests tagged as listed above.
9. ok: as above.
