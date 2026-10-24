Role: spec checker
Commit: 28d9f39
Verdict: #1329: findings (F1); #1389: findings (F1); #1241: findings (F3); #1344: findings (F2); #1242: consistent

All three findings are small and none blocks a behavior. The inputs were `00-lead.md`, `04-spec-author.md`, `06-spec-author.md`, `05-test-implementer.md`, `07-implementer.md`, `08-cleaner.md`, the five issues with their comments, and `git diff f3d514d...HEAD`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Whole-run facts the sections below refer to:
- `scripts/test.sh --project architecture` on 28d9f39: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84` (includes `SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`, `RuleDocumentationConventionTests`).
- `scripts/lane-check.sh bug-fix`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- `ls sdd/work`: only `doc-comment-element-safety`.
- Ledger `sdd/specs/requirement-ids.txt`: three insertions only (`FMT-DOC-013`, `RH8309-005`, `SHARED-DOC-018`), each at its sorted position and each the next free number; no line removed or moved.
- Test tags: I compared every test named in the `Tests` tables of `05-test-implementer.md` with its `[Requirement]` attributes in the sources (script over `src/*Test*`). Zero mismatches. The 21 retagged RH8107/RH8202 tests carry `SHARED-DOC-018` (8 + 13).

## Findings

| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| F1 | 2 | `sdd/specs/formatter/documentation-comments.md`, FMT-DOC-011, the new byte-order-mark prose example | The example formats a text whose class is empty (`class C`, `{`, `}`) and states the result keeps `class C`, `{`, `}`. Through the full pipeline an empty class becomes `class C;` (the test implementer observed this, `05-test-implementer.md` "Note for the spec checker"), so the example is not the formatter's output, and an example is a seed for tests. The test `RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark` already deviates from the spec by adding a field `int A;`. Every other example of the file uses a member. | spec author |
| F2 | 2 | `sdd/specs/analyzer/RH8309.md`, RH8309-005 versus `RH8309…AnalyzerTests.VerifyNoCodeFixForEntityReferenceBetweenTheElements` | The test is tagged `RH8309-005` and asserts that a `/// &amp;` line between the elements withholds the fix. RH8309-005 names "text with a character that is not white space", an XML comment, a CDATA section, and a processing instruction, but not a character or entity reference. `04-spec-author.md` lists the case only as a "candidate". RH8304-002 does name the reference form (`&amp;`) explicitly. Either the requirement should name the reference, or the claim is not specified. Low severity. | spec author |
| F3 | 4 | `sdd/specs/formatter/documentation-comments.md` header, `DocumentationCommentUtilities` | The implementer's note in `07-implementer.md` (#1329, "Policy owners") stands open: `HasOwnEndTag` and `ContainsElementWithoutOwnEndTag` in `DocumentationCommentUtilities` implement the definition of SHARED-DOC-017, yet the type is a code owner of the FMT-DOC spec, while SHARED-DOC lists `DocumentationCommentCodeFixUtilities`, which calls it. A type is an owner of at most one spec and the architecture tests pass, so this is a judgement for the spec author, who has not recorded one. | spec author |

## thoenissen/Reihitsu#1329

### Findings
F1 (FMT-DOC-011 example, reached through the BOM text), none specific to the formatter fix itself.

### Checked
1. Change doc: none present. FMT-DOC-013 is a new requirement with new behavior, and FMT-DOC-003, FMT-DOC-004 and RH8301/RH8304/RH8305/RH8307 change as well. By `sdd/README.md` that normally calls for a change doc. `00-lead.md` records the maintainer's decision to run this as a bug fix with no change doc (answers 1C and the clarifications), and the lane check passes. Not a finding; the lead should state this in the PR.
2. Specs match the diff: FMT-DOC-013 (new), FMT-DOC-004, FMT-DOC-005/006/008/009 ("does not apply"), RH8301-002, RH8305-002, RH8307-002 state exactly what `IsLeftAsWritten` and the phase guard do (own end tag compared ordinal at any depth; stranded and no-end-tag comments skipped; relocation unchanged). Format of the new requirements is right (normative sentence, `Why:`, examples). Both issue shapes are FMT-DOC-013's first example.
3. IDs: `FMT-DOC-013` is next free and sorted. ok.
4. Owners: all named types exist (`SpecConsistencyTests` passed). `sdd/specs/README.md` unchanged for the formatter spec and consistent. See F3.
5. User documentation: `RH8301.md`, `RH8305.md`, `RH8307.md` gain `## Exceptions` that match RH8301-002, RH8305-002, RH8307-002, mention no formatter, repeat nothing from `general-notes.md`, and keep the footer. Corrections unchanged and still equal to the fixes' output.
6. Open questions: the spec author's questions 1 to 6 and the round-3 questions are all answered in `00-lead.md` (maintainer answers); none appears in a spec. ok.
7. Coverage: `FMT-DOC-013` is covered by `DocumentationCommentWithoutOwnEndTagTests` (18 tagged tests). Architecture result quoted above.
8. Tags: every test of the `#1329` table carries the tags the table lists. Every requirement the spec author named for the tests is tagged on at least one new test (FMT-DOC-013, FMT-DOC-004, FMT-DOC-005, FMT-DOC-001, RH8301-002, RH8305-002, RH8307-002). ok.
9. Nothing left behind: only this run's `sdd/work/`. ok.

## thoenissen/Reihitsu#1389

### Findings
F1.

### Checked
1. Change doc: see #1329; FMT-DOC-003 and FMT-CLEAN-009 are changed requirements. The maintainer's recorded decision (answers 2 and the skeptic's 4A) stands.
2. Specs match the diff: FMT-DOC-003 (text left byte for byte, owning-token list is examples), FMT-CLEAN-009 (BOM kept before a line break, white space after it removed, blank lines removed per FMT-BLANK-007), and FMT-DOC-011 (added lines start with the indentation only) match `CleanupPhase`, `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines`, `GetContinuationPrefix`, and the phase guard. `formatter/blank-lines.md` needs no change, as the spec author argued: FMT-BLANK-007 does not decide the BOM. F1 applies to the FMT-DOC-011 example.
3. IDs: no new ID. `FMT-CLEAN-009` and `FMT-DOC-003` keep theirs. ok.
4. Owners: `CleanupPhase` stays in the cleanup spec; `BlankLineTokenCleanupRewriter` is not named by a spec the diff touches. `cleanup.md` `Uses` gains FMT-BLANK-007 and passes the structure test. ok.
5. User documentation: no rule page is affected by the formatter part (the analyzers that go silent are listed under #1329). ok.
6. Open questions: the BOM question (skeptic Q) was answered 4A and is written as a clarification; the 'stranded text' question was answered (2, A). None is recorded only in the PR. ok.
7. Coverage: FMT-CLEAN-009 and FMT-DOC-003 were already covered; the new tests add more. Architecture result quoted above.
8. Tags: `ByteOrderMarkTests`, `CleanupPhaseTests`, and `StrandedDocumentationCommentTextTests` match the table. The spec author's "Examples for the tests" requirements (FMT-DOC-003, FMT-CLEAN-009, FMT-DOC-011) are each tagged. ok.
9. ok.

## thoenissen/Reihitsu#1241

### Findings
F3 (ownership note, applies because `HasOwnEndTag` is the SHARED-DOC-017 implementation).

### Checked
1. Change doc: SHARED-DOC-017 gains behavior (exact name comparison, RH8204 and RH8309 added to the withheld list), and RH8204-002/003 widen their coverage. Same maintainer decision as above; no finding.
2. Specs match the diff: SHARED-DOC-017 (RH8107, RH8202, RH8204 guard via `CanRewriteElement`/`HasOwnEndTag`; RH8309 withheld when any top-level element lacks its own end tag), RH8107-007, RH8202-004, RH8204-002, RH8204-003, RH8309-004 agree with `CanRewriteElement`, the RH8204 provider guard, and `CanReorderElements`. RH8305/RH8307 are correctly dropped from the withheld list, since those rules no longer report there. The SHARED-DOC-017 `Uses` (FMT-DOC-013) is present.
3. IDs: no new ID for this issue. ok.
4. Owners: `DocumentationCommentCodeFixUtilities` is listed once, under SHARED-DOC. The helper's `CanRemoveElement` rename is not named by any spec. See F3.
5. User documentation: `RH8107.md`, `RH8202.md`, `RH8204.md` each carry one sentence about the withheld fix; they cover never closed, closed by another element's end tag, and for RH8107 a differently spelled end tag. They do not mention the formatter and match RH8107-007, RH8202-004, and RH8204-003. ok.
6. Open questions: skeptic Q on exact versus case-insensitive names was answered 5A and is stated in SHARED-DOC-017. ok.
7. Coverage: SHARED-DOC-017 covered by the new tests of RH8107, RH8202, RH8204, RH8309. Architecture result quoted above.
8. Tags: all tests of the `#1241` table match. The three Fix All tests repaired in `145086e` only changed the harness mode; their tags are unchanged. ok.
9. ok.

## thoenissen/Reihitsu#1344

### Findings
F2 (RH8309-005 and the character-reference test). F1 does not apply.

### Checked
1. Change doc: RH8304-002 (stops reporting) and RH8309-005 (new) are intended-behavior changes; the maintainer's decision for the run stands (see #1329).
2. Specs match the diff: RH8304-004 (indentation only is inserted; code, delimited comment, BOM in front are not repeated; coverage widened as maintainer answer 3A) matches `GetContinuationPrefix`; RH8304-002 matches the early return on `IsLeftAsWritten`; RH8309-005 (text, XML comment, CDATA, processing instruction at the top level, before, between and after) matches `CanReorderElements`, apart from F2. RH8309-004's coverage now says "between, before, and after". RH8304 has no rule-page note, as decided in answer 3A.
3. IDs: `RH8309-005` is next free, sorted. ok.
4. Owners: unchanged for RH8304 and RH8309. ok.
5. User documentation: `RH8304.md` gains the same `## Exceptions` as the other layout rules; `RH8309.md` gains the withhold sentence. Both are short, formatter-free, and consistent with the specs; the RH8304 Correction example still equals the fix output. ok.
6. Open questions: the RH8304 stranded-comment question and the RH8308 question were answered in the round-3 section of `00-lead.md`, and the specs follow them (RH8304-002 joins the silent rules; RH8308 untouched). ok.
7. Coverage: `RH8309-005` covered by the ten new tests; architecture result quoted above.
8. Tags: all tests of the `#1344` table match, including the RH8304 tests under RH8304-002 and RH8304-004. The six `GetContinuationPrefix` tests are untagged on purpose (the table lists `—`). ok.
9. ok.

## thoenissen/Reihitsu#1242

### Findings
None.

### Checked
1. Change doc: none needed for the owner move itself; the new `SHARED-DOC-018` writes down behavior that already exists (RH8107-006), so it is a clarification. ok.
2. Specs match the diff: SHARED-DOC-018 carries the RH8107-006 text and example; RH8107-006 now points to it with its own `Why:`; RH8202-003 points to it. RH8107 and RH8202 `Uses` list SHARED-DOC-017/018, no longer RH8107-006/007. No code behavior changed for this issue. ok.
3. IDs: `SHARED-DOC-018` is next free and sorted. `RH8107-006` is kept, not retired: the issue text spoke of retiring the old IDs, but the maintainer's recorded decision was only the owner move, and the requirement still has its own normative sentence. I treat this as consistent with the decision; the lead may want to say so in the PR.
4. Owners: `DocumentationCommentCodeFixUtilities` moved from `analyzer/RH8107.md` to `shared/element-documentation.md`; the index rows in `sdd/specs/README.md` match (`SpecStructureTests` passed). RH8304 and RH8308 use other methods of the same type (`MoveContentToNextDocumentationLineAsync`, `IsHorizontalWhitespace`), whose behavior no SHARED-DOC requirement specifies, so there is no requirement ID for their `Uses`. ok.
5. User documentation: no rule page is affected by the move. ok.
6. Open questions: none. ok.
7. Coverage: `SHARED-DOC-018` covered by the 21 retagged tests and `VerifySpecificationExampleRemovesReturnsElementWithTheLinesItOwns`. Architecture result quoted above.
8. Tags: the retagged tests carry `SHARED-DOC-018` next to their old tag and the table says so. ok.
9. ok.

## Whole run

Notes for the lead, none of them a finding against a role:
- **Change doc.** The run adds FMT-DOC-013 and RH8309-005, changes FMT-DOC-003, FMT-CLEAN-009, and SHARED-DOC-017, and makes RH8301, RH8304, RH8305, and RH8307 stop reporting in some comments. `sdd/README.md` calls a change doc for added or changed intended behavior. The maintainer's clarification in `00-lead.md` says no change doc, and the lane check passes; the PR description should say that this was the maintainer's decision.
- **Scope.** `07-implementer.md` reports six new public static methods on `DocumentationCommentUtilities` (Reihitsu.Core) beyond the approved `GetContinuationPrefix` change, and asks the lead to confirm them against escalation 5.
- **Follow-up candidates** (from `03-skeptic.md`, `04-spec-author.md`, `07-implementer.md`) all still need the maintainer's approval of the text before any issue exists: the RH8309 fix formats the whole member (SHARED-FIX-006); the stale comment in `ReorderElements` about unknown elements; own-line comments that document nothing are still normalized; the duplicate "source character" test in `HasOwningLeadingSourceOnSameLine`; `scripts/trace.sh` mode 644; the superseded `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md`.
- **Before publishing**, the lead deletes `sdd/work/doc-comment-element-safety/`.
