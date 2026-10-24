Role: spec checker
Commit: 9431ee8
Verdict: #1329: consistent; #1389: findings (F1); #1241: consistent; #1344: consistent; #1242: consistent

Repair round 2 re-check on the files changed since `9161d75`: eight rule pages, `sdd/specs/formatter/documentation-comments.md`, the formatter and Core production files, and the test files. Inputs: `13-auditor.md`, `14-spec-author.md`, "Repair round 2" in `05-test-implementer.md`, `15-implementer.md`, `16-cleaner.md`, and `git diff 9161d75..HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| F1 | 2 | `sdd/specs/formatter/cleanup.md`, FMT-CLEAN-009 versus `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` (`startsText`) and the tests `ByteOrderMarkCharacterOnBlankLineBefore{Else,Finally,OpeningBrace,ClosingBrace,Statement}IsRemoved` and `CleanupPhaseTests.RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText` | This round makes the code keep a byte order mark only at the start of the text; elsewhere it is removed with its blank line. FMT-CLEAN-009 says only that the mark is kept at the start of the text. The behavior elsewhere is not stated, yet eight tests assert it and carry `FMT-CLEAN-009`. It is also not uniform: the auditor's hint says the off-limits Indentation phase already keeps a mark that starts the white space of a line elsewhere on base, so the spec author has to check whether a plain clarification fits ("a `U+FEFF` that does not start the text is white space and goes with its blank line") or whether the mixed behavior is an open question for the maintainer (`sdd/README.md`, *Open questions*). Until then the tests tag a requirement that does not state their claim. Low severity; behavior is unchanged from base. | spec author |

## Closed findings of the auditor that touch the specs
- **F4 (#1329): closed.** FMT-DOC-013's third normative sentence and its coverage sentence now name FMT-DOC-002 next to FMT-DOC-001, with a before/after example (`/* c */ /// <summary>S` becomes `/* c */` and `/// <summary>S` on separate lines). It agrees with FMT-DOC-002's text ("keeping the earlier comment on its original line"). Two tests, `CommentWithoutEndTagSeparatedFromEarlierBlockCommentIsLeftAsWritten` and `CommentWithoutEndTagBehindBannerAtStartOfFileIsLeftAsWritten`, carry `FMT-DOC-013` and `FMT-DOC-002`, as the repair table lists.
- **F5 (#1241, #1329, #1344): closed.** All eight rule pages (RH8107, RH8202, RH8204, RH8301, RH8304, RH8305, RH8307, RH8309) use one wording, "closed by an end tag with a different name, including one that differs only in case", and the pages match SHARED-DOC-017, FMT-DOC-013 and the layout-rule specs. No formatter mention; `RuleDocumentationConventionTests` pass.

## Checked (full checklist on the changed files)
1. Change doc: unchanged from `09-spec-checker.md`; this round clarifies FMT-DOC-013 and rule pages and repairs code toward FMT-CLEAN-009 and FMT-PIPE-004. The maintainer's no-change-doc decision stands. ok.
2. Specs match the diff: FMT-DOC-013 matches the guard, which runs after relocation, so a comment that FMT-DOC-002 separates is moved and left as written. The `U+FEFF` joined with the following indentation (BOM kept only at `token.FullSpan.Start == 0`, one white-space trivia) is consistent with FMT-CLEAN-009's example and its "same holds when white space follows". Requirement format intact. F1.
3. IDs: no ID added, changed or removed since `9161d75`; ledger untouched. ok.
4. Owners: no spec header changed; no type added, renamed or removed. ok.
5. User documentation: see F5 above. ok.
6. Open questions: none from the spec author this round. F1 may turn into one; the spec author decides. ok pending F1.
7. Coverage: `scripts/test.sh --project architecture` on 9431ee8: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7`. No requirement was added. ok.
8. Tags: I compared every test of the tables in `05-test-implementer.md` (including "Repair round 2") with its `[Requirement]` attributes: zero mismatches. The new tests carry `FMT-CLEAN-009` with `FMT-PIPE-004` / `FMT-DOC-011` / `FMT-CLEAN-001` as listed, and the two F4 tests carry `FMT-DOC-013` and `FMT-DOC-002`. The F3 change from the invisible `U+FEFF` character to `﻿` in Core and tests changes no tag. ok (the FMT-CLEAN-009 tag on the mid-text tests is F1).
9. Nothing left behind: `ls sdd/work` shows only `doc-comment-element-safety`; `scripts/lane-check.sh bug-fix` confirmed the lane; the worktree is clean. The lead deletes the run folder before publishing. ok.
