Role: spec checker
Commit: 924eaa0
Verdict: #951: consistent; #1133: consistent; #1146: consistent

Inputs: `00-lead.md` through `05-cleaner.md`, `git diff origin/main...HEAD` (spec, documentation, source, and test files), `sdd/README.md` (*Keeping specs true*), `.claude/commands/create-rule-doc.md`, and `sdd/specs/shared/diagnostics.md` / `code-fixes.md` (read only). The prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing except this handover.

Results quoted below were run on `924eaa0`:

- `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 84, Skipped: 0, Total: 84`.
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~RawStringAlignment|FullyQualifiedName~Indentation"`: `Passed: 658, Total: 658`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5104|FullyQualifiedName~RH5601|FullyQualifiedName~RH5602|FullyQualifiedName~RH5202"`: `Passed: 125, Total: 125`.
- `scripts/lane-check.sh bug-fix`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- `LC_ALL=C sort -c sdd/specs/requirement-ids.txt`: sorted.

## thoenissen/Reihitsu#951

### Findings
None.

### Checked
1. Change doc: none present, none required on the claimed lane. The diff adds FMT-RAW-007, FMT-RAW-008, FMT-RAW-009 and clarifies FMT-RAW-006, FMT-INDENT-011, FMT-INDENT-013. They write down the intended result of FMT-PIPE-004 and FMT-RAW-001 for hole lines and nested literals, the gap a bug fix exposed. The maintainer's answers 1B, 2A, and escalation 3 (A) in `00-lead.md` decided the content. 2A fixes a case beyond the issue's text (verbatim string or comment nested in a hole); `00-lead.md` says the PR states that.
2. Specs match the diff. Each behavior in `RawStringAlignmentPhase` (per-line shift table, outermost-first visit, `StartsInsideNestedText`) and in the layout code (`LayoutModel` hole-line registration, `CommentIndentationContributor`, `IndentationRewriter`) is stated by FMT-RAW-007 to FMT-RAW-009, FMT-INDENT-011, or FMT-INDENT-013. The two unspecified cases (a line an earlier phase breaks inside a hole; a hole line indented less than a leftward shift, clamped to column 0 as before) are named in FMT-RAW-007's coverage sentence as the Format rules require. Format is followed: normative sentence first, `Why:`, examples, no non-goal that holds an open question (the tab non-goal points to thoenissen/Reihitsu#1432).
3. IDs: FMT-RAW-007, FMT-RAW-008, FMT-RAW-009 are the next free numbers and sit in sorted position in the ledger; no line removed or moved.
4. Code owners: no type added, renamed, or removed (`rg` over the production diff finds no new type declaration); owners of `raw-string-alignment.md` and `indentation.md` are unchanged; `sdd/specs/README.md` untouched and correct. `Uses` of both specs matches (SpecStructureTests passed).
5. User documentation: no rule page is affected (the formatter has none for this behavior).
6. Open questions: the spec author lists none for #951; escalation answers are carried out in the spec (not written as open questions).
7. Coverage: FMT-RAW-007, FMT-RAW-008, FMT-RAW-009 each have covering `[Requirement]` tests; architecture result above.
8. Tags: every test the test implementer's table lists against a requirement in `RawStringInterpolationHoleTests` and the RH5202 formatter-parity file carries exactly the tags the table names. Every requirement under `Examples for the tests` (FMT-RAW-007, FMT-RAW-008, FMT-RAW-009, FMT-INDENT-011) is tagged on at least one added test. `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` is deliberately untagged (characterization of an unspecified case); not a finding.
9. No `sdd/work/` other than this run's.

## thoenissen/Reihitsu#1133

### Findings
None.

### Checked
1. Change doc: none required. RH5601-002 and RH5602-002 are clarified to SHARED-DIAG-003 (maintainer decision J4, escalation 1 A and 3A in `00-lead.md`); RH5601-001 and RH5602-001 are back to their pre-run wording, with the remaining gap held as a follow-up candidate.
2. Specs match the diff. `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken(token, position)` exempts only the text of the `InterpolatedMultiLineRawStringStartToken` and `InterpolatedRawStringEndToken` tokens; the trivia around them and a hole's tabs stay reported. That is what RH5601-002 (whitespace after the opening quotes, content-line and closing-line indentation, interpolated or not) and RH5601-001 (hole-line tab, `"""→;`, `→$"""`) state. RH5602 has no code change, matching its clarification-only spec text. `Uses` of RH5601 gains SHARED-DIAG-003, which the spec references outside its header.
3. IDs: none added.
4. Code owners: unchanged and existing.
5. User documentation: `documentation/rules/RH5601.md` carries the new clause, stays short, mentions no formatter, repeats nothing from `general-notes.md`, and keeps the footer; `RH5602.md` is net unchanged. Category/ID range unchanged, help page exists.
6. Open questions: none from the spec author. The RH5601/RH5602 versus SHARED-DIAG-003 gap is a follow-up candidate whose text the lead still has to show the maintainer (`00-lead.md`, escalation 1); nothing about it is written into a spec.
7. Coverage: no new ID, so no new coverage requirement; the added tests cover RH5601-001, RH5601-002, RH5602-002.
8. Tags: all 14 added tests carry the tags the table lists (RH5601-001, RH5601-002 with SHARED-DIAG-003 or SHARED-FIX-003, RH5602-002); each requirement under `Examples for the tests` is tagged.
9. Nothing left behind.

## thoenissen/Reihitsu#1146

### Findings
None.

### Checked
1. Change doc: none required on the claimed lane. RH5104-006 is new and brings the fix in line with SHARED-FIX-003. RH5104-005 is reworded, and RH5104-002 clarified, on the maintainer's recorded decisions J1 (SHARED-FIX-006 over RH5104-005) and the in-scope Q4. Both change what the old requirement text said: RH5104-005 required a blank line, and RH5104-002 left `/* a */ /* b */` reported. The lead's lane claim and the maintainer's "no change doc" instruction in `00-lead.md` rest on those answers; I took them as given and did not re-judge them.
2. Specs match the diff. `IsInlineComment` now asks whether a token overlaps a spanned line (RH5104-001/002); `RequiresBlankLineBeforeRelocatedComment` is removed (RH5104-005, SHARED-FIX-006); `StartsInsideLiteralOrComment` withholds the action for a line beginning inside a non-whitespace trivia, a token, or an interpolated string that began earlier, including a hole and its opening brace (RH5104-006, RH5104-004's condition). The RH5104-006 example (seven withheld forms, the `@"…"` line-start comment that is moved) agrees with the code paths and the added tests. Spec format followed (normative sentence, `Why:`, `{|RH5104:…|}` markup, fixed block shown).
3. IDs: RH5104-006 is the next free number, sorted after RH5104-005.
4. Code owners: unchanged and existing; `Uses` of RH5104 lists SHARED-COMMENT-002, SHARED-FIX-003, SHARED-FIX-006, all referenced outside the header.
5. User documentation: `documentation/rules/RH5104.md` gains one `## Exceptions` bullet for the withheld fix, as `create-rule-doc` allows for a deliberately withheld fix; short, no formatter mention, not a restatement of the general comment limitation. Its `### Correction` is unchanged and still equals the fix output (the examples already have blank lines above the moved comments, so dropping the insertion does not alter them).
6. Open questions: none from the spec author. The follow-ups it lists (reordering for `M(); /* a */ // b` and `/* a */ /* b⏎c */ M();`; the superseded Q6 of thoenissen/Reihitsu#1433) are outside this run and not written into the spec; the lead still has to show the maintainer the issue text for them.
7. Coverage: RH5104-006 has covering tests (13 tests); architecture result above.
8. Tags: all added RH5104 tests carry the tags the table lists, including the renamed `VerifyRelocatedCommentGetsNoBlankLineWhenPreviousLineIsCode` (RH5104-005) and the formatter-parity test (RH5104-002). Every requirement under `Examples for the tests` (RH5104-002, RH5104-005, RH5104-006) is tagged on at least one added test.
9. Nothing left behind.

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None | — |

### Checked
- 1: ok — no change doc, none required on the bug-fix lane, resting on the maintainer's recorded answers (J1, J4, Q4, 1B, 2A, 3A, 4B, escalation 3 A).
- 2: ok.
- 3: ok (four new IDs, sorted, none removed).
- 4: ok (no owner changed; `SpecConsistencyTests` and `SpecStructureTests` passed).
- 5: ok (RH5104 and RH5601 pages; RH5602 unchanged).
- 6: ok. For the lead, items the spec author and skeptic name that still need the maintainer before the PR: the text of the follow-up issue for the RH5601/RH5602 gap against SHARED-DIAG-003, the RH5104 comment-reordering follow-up, the update to thoenissen/Reihitsu#1433 (Q6 superseded by J1), and the run-report note that R1 narrowed 1B's wording to constructs inside the hole.
- 7: ok — `Passed! - Failed: 0, Passed: 84, Skipped: 0, Total: 84`.
- 8: ok.
- 9: ok — `sdd/work/` holds only `raw-string-literal-content/`, which the lead deletes before publishing.

# Repair round 1

Role: spec checker
Commit: 71b17d2
Verdict: #951: consistent; #1133: consistent; #1146: consistent

Inputs: the diff `ea03170..71b17d2` and the full diff against `origin/main`, `00-lead.md` (Repair round 1, escalation 4), `01-spec-author.md`, `03-test-implementer.md` and `04-implementer.md` (Repair round 1), `07-auditor.md`. The prompt carried no intended fix or hoped-for verdict. I changed nothing except this file.

Results on `71b17d2`:

- `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 84, Skipped: 0, Total: 84`.
- Formatter filter `RawStringAlignment|Indentation|SelfHosting`: `Passed: 669, Total: 669`.
- Analyzer filter `RH5104|RH5601|RH5602|RH5202`: `Passed: 129, Total: 129`.
- `scripts/lane-check.sh bug-fix`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- `LC_ALL=C sort -c sdd/specs/requirement-ids.txt`: sorted.

## thoenissen/Reihitsu#951

### Findings
None.

### Checked
1. Change doc: still none required. The repair adds no requirement and changes none for #951: `raw-string-alignment.md` changes only its `Code owners` row. The behavior the implementer changed (anchors measured after the raw-string shift, tab columns, `///` lines and constraint clauses in holes, the argument anchored behind a raw literal's closing quotes outside any hole) is stated by FMT-RAW-007 conditions 1 and 2, FMT-RAW-009 (`/* */` only, so dropping `///` from the frozen lines agrees with it), FMT-INDENT-012, FMT-INDENT-017, FMT-INDENT-029, FMT-INDENT-030 and FMT-CLEAN-002, none of which changed. It is the bug-fix case of bringing code to the spec.
2. Specs match the diff: ok. `RawStringLineShifts` is the one owner of the per-line shift that FMT-RAW-001, -007, -008, -009 describe; no requirement contradicts it.
3. IDs: ok; no #951 ID added.
4. Code owners: the diff adds two production types, `RawStringLineShifts` (class) and `RawStringLiteral` (readonly record struct), in `Reihitsu.Formatter.Pipeline.RawStringAlignment.Utilities`. Both are in the `Code owners` row of `sdd/specs/formatter/raw-string-alignment.md` and in its row of `sdd/specs/README.md`, the two rows are identical, and neither type is an owner of another spec. `indentation.md` needs no owner or `Uses` change (it already lists FMT-RAW-001 and FMT-RAW-007, the only FMT-RAW IDs it references). `SpecConsistencyTests` and `SpecStructureTests` pass.
5. User documentation: no rule page affected.
6. Open questions: none from the spec author. The implementer lists one unverified follow-up candidate, auditor hint 3 (FC-028: a fix that formats a node inside a hole registers only the holes inside that node); it is for the lead to route and is written into no spec. The RH5202 fix's own copy of the shift (`RealignContent`) is already the approved follow-up thoenissen/Reihitsu#1458.
7. Coverage: no new #951 ID; FMT-RAW-007, -008, -009 stay covered. Architecture result above.
8. Tags: the eight added formatter tests carry the tags `03-test-implementer.md` names. In `RawStringInterpolationHoleTests`: FMT-RAW-007 on all seven, plus FMT-RAW-008 on `ArgumentAnchoredOnTheClosingLineOfANestedLiteralKeepsItsAlignment`, FMT-RAW-009 on `ArgumentAnchoredOnTheContinuationLineOfAVerbatimStringKeepsItsAlignment`, FMT-INDENT-017 on both constraint-clause tests. In `RawStringAlignmentPipelineTests`: FMT-INDENT-029 and FMT-INDENT-030 on the chain-link test, FMT-INDENT-012 and FMT-INDENT-030 on the wrapped-argument test.
9. No `sdd/work/` other than this run's (`raw-string-literal-content/`, now with `07-auditor.md`).

## thoenissen/Reihitsu#1133

### Findings
None.

### Checked
1 to 9: ok, unchanged. The repair touched no RH5601/RH5602 file; `ea03170..71b17d2` contains none.

## thoenissen/Reihitsu#1146

### Findings
None.

### Checked
1. Change doc: still none required. RH5104-007 is a new requirement for the comment-reordering gap. It follows the maintainer's answer A (escalation 4 in `00-lead.md`) and SHARED-FIX-003, which the bug-fix lane brings the code to. The behavior it states is the fix being withheld, as RH5104-006 is.
2. Specs match the diff: ok. `HasCommentBeforeOnLine` withholds the action when a comment trivia lies wholly between the line start and the reported comment, which is what RH5104-007 states and what RH5104-004's new condition refers to. The RH5104-007 example (`M(); /* a */ // b` and `/* c */ /* d⏎ e */ M();`, then the state after the first fix) matches the test `VerifyFixAllKeepsTheOrderOfCommentsThatShareALine` and its siblings. Format followed: normative sentence with SHARED-FIX-003, `Why:`, markup, a result block.
3. IDs: RH5104-007 is the next free number and sits after RH5104-006 in the ledger; nothing removed.
4. Code owners: unchanged. `Uses` of RH5104 (SHARED-COMMENT-002, SHARED-FIX-003, SHARED-FIX-006) still matches its references.
5. User documentation: `documentation/rules/RH5104.md` is unchanged. That is right: a fix withheld only because of a comment is the general limitation in `general-notes.md` (`AGENTS.md`, *Code-fix coverage*), so it is not repeated on the rule page.
6. Open questions: the earlier follow-up candidate (comment reordering) is resolved by RH5104-007. Still for the lead: the text of the follow-up for thoenissen/Reihitsu#1433 (Q6 superseded by J1) and of the RH5601/RH5602 SHARED-DIAG-003 gap, as in the first round.
7. Coverage: RH5104-007 has four covering tests; architecture result above.
8. Tags: `VerifyCodeFixIsOfferedOnlyForTheFirstCommentOfALine`, `VerifyNoCodeFixIsOfferedForACommentBehindAnUnreportedCommentOnItsLine`, `VerifyFixAllKeepsTheOrderOfCommentsThatShareALine`, and `VerifyCommentIsMovedOnceTheCommentBeforeItHasLeftItsLine` each carry `[Requirement("RH5104-007")]`, as the table lists. Both shapes of the RH5104-007 example are tagged on added tests.
9. Nothing left behind.

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None | — |

### Checked
1 to 9: ok, as stated per issue. Items for the lead before the PR: the follow-up text for the RH5601/RH5602 gap, the update to thoenissen/Reihitsu#1433, the auditor hint 3 candidate, and the run-report notes already listed in the first section; `sdd/work/` is deleted before publishing.
