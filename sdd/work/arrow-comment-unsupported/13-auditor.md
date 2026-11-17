Role: auditor
Commit: d52b770
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta of thoenissen/Reihitsu#1732 (`agent-filed`), lane behavior change, against the audit of a23ec37 (`09-auditor.md`). Head d52b770084eae49e92e0b14f6b0b13389e292fff (checked out, tree clean). `origin/main` is 71a89ec49a8806e9d4eed65ba8e2c229904eb465 (`git ls-remote` agrees) and is an ancestor of the head. `scripts/lane-check.sh behavior-change`: `LANE CHECK: confirmed`.

`git diff a23ec37 d52b770 -- src` changes two test files, both in commit 43267e0, and nothing else outside `sdd/work/`. This matches 10–12. No production file, spec, change doc, or rule page changed, so scope did not grow and the rest of 09 stands unchanged.

Previous findings:
- #1 (medium, RH3202 neighbour test too weak): **closed**. The test now applies the action registered for `A` and asserts the exact result under LF and CRLF. It also checks that `B` gets no action and is still reported.

Previous hints: the `KeepsCommentTrailingArrowInIndexer` hint is resolved because the test is deleted (lead decision 8). The lambda attribute-list hint was dropped by the lead (decision 9), and the repair does not touch it.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption; 09's table stands byte-identical._

## Policy ownership
_N/A — the repair changes no policy owner; 09's table stands._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` (LF, CRLF) | (1) `B` holds the shape and gets no action. (2) The action for `A`, once invoked, converts `A`, starts `B` on its own line, and leaves `B`'s text as written, with the document's line ending. (3) `B` is still reported on the result, and `A` is not. | (1) An action is offered for `B`. (2) The action for `A` is offered but changes nothing, formats `B`, inserts a blank line, or writes a foreign line ending: `Assert.AreEqual(RemoveMarkup(expected), actual)` is an exact, line-ending-sensitive string comparison. If `A` has no action, `IsNotNull` fails. (3) `Verify(expected)` resolves to the analyzer-only `Verify(string, params DiagnosticResult[])`, which demands exactly the marked diagnostic. | `GetCodeFixActionsAsync` and `TryApplyCodeFixAtAsync`, which both register the fix for a diagnostic synthesized at `FindArrow(...)`. That span is the `ArrowExpressionClauseSyntax`, the span the analyzer reports (markup `{\|RH3202:=> /* c */ 2\|}`). The provider uses only `diagnostic.Location` (`RH3202…CodeFixProvider.cs:75`), so the synthesized diagnostic stands in for the real one. 10 reports that a mutation making the fix for `A` a no-op fails both rows. | adequate; closes #1 |
| `ExpressionBodiedTransformTests.KeepsCommentTrailingArrowInIndexer` (deleted) | — | — | — | Deletion is sound. It fed the structural phase an input the precheck refuses, which no caller passes (`AGENTS.md`, *Supported inputs*). It carried no tag. The refusal of the indexer arrow gap stays covered through public entry points: `ExpressionBodiedIndexerTests.InlineCommentAfterArrowIsUnsupported` (block comment) and the *Indexer* row of `UnsupportedExpressionBodyArrowCommentTests.CommentBehindTheArrowOfEveryExpressionBodyIsUnsupported`. The line-comment form is covered on properties, methods, and getters (`UnsupportedExpressionBodyArrowCommentTests.cs:97-101, 245-255`). The precheck dispatches on `ArrowExpressionClauseSyntax`, regardless of the owner's kind. |

Visible output (check 10): the strengthened test's expected result is demanded by unchanged requirements. RH3202-002 applies because the line begins with `A`'s first token. SHARED-FIX-007 *Shared line* requires that the changed member gets its own lines and the other member starts on the next line. SHARED-FIX-006 forbids inserting a blank line. SHARED-FIX-010 requires that `B` keeps its text, gets no fix, and stays reported. None of these specs changed in the run since base (`git diff 71a89ec d52b770` over `code-fixes.md` and `RH3202.md` is empty). Nothing is undecided.

Comments (check 8): the test's summary and the new `<param>` describe the new assertions. There is no `TODO` and no tracker reference.

## Failure classes dismissed
- FC-017: see *Assertion adequacy*. The only changed test is now as strong as SHARED-FIX-010.
- FC-001, FC-002, FC-004, FC-010, FC-012, FC-015, FC-016, FC-038, FC-042, FC-044: the repair changes no production code, guard, or spec. 09's dismissals hold unchanged.
- FC-009, FC-043: the fix asks `FindUnsupportedSyntax` about the node it formats. This was traced in 09 and is now locked by the strengthened test.
- FC-035: the expected text was derived from SHARED-FIX-007 and RH3202-002, not copied from a run (10). I checked the derivation against the spec text above.
- All other classes: unchanged from 09. The repair touches no surface they reach.

## Architecture findings
_None._

## Hints
_None._

## Follow-up candidates
_None._
