Role: spec checker
Commit: (this commit; see `git log -1 -- sdd/work/comma-comment-layout/11-spec-checker.md`)
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked against `git diff origin/main...HEAD` at 7929a0e. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118` (includes `SpecConsistencyTests` and `EveryRequirementOfAnEnforcedSpecIsCovered`).

## thoenissen/Reihitsu#1737

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

### Checked
1. Change doc `rh5107-comment-behind-leading-comma.md`: required (RH5107-004 is new behavior, a fix is withheld), has `Visible output changes`, no task list. Status is `Draft`; the lead sets `Accepted` before the PR is marked ready. The withheld fix is recorded under `Decisions` as a rare-class decision that is not an output question (lead decision 5, README *What reaches the maintainer*).
2. RH5107.md: RH5107-004 follows the format (normative sentence, `Why:`, examples with `{|RH5107:…|}`), RH5107-003 points to it, a non-goal and `Uses` updated. The code (`RH5107…CodeFixProvider` registers no fix when the comma's trailing trivia holds a comment) matches, including the multi-line-comment and later-line cases.
3. `RH5107-004` is in `requirement-ids.txt` at its sorted position, nothing removed or renumbered.
4. Code owners of RH5107 unchanged and exist; architecture project passes.
5. `documentation/rules/RH5107.md` needs no change (no comment or withheld-fix note; `general-notes.md` covers it) and has the footer.
6. No output question in this issue's handovers is open; the follow-up candidate (fix delegates to the formatter) is under `Follow-up candidates` in `00-lead.md`.
7. RH5107-004 is covered (architecture run above).
8. All eight new RH5107 tests carry `[Requirement("RH5107-004")]`, matching the `Tests` table; the spec examples P, Q, R, T are tagged on tests.
9. See *Whole run*.

## thoenissen/Reihitsu#1738

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

### Checked
1. Change doc `initializer-leading-comma.md`: required; `Visible output changes` present; decisions record batch 1 Q1 (A with arrays moving the comma) and Q2 (not declared), and the escalation-10 answers. Status `Draft`; the lead sets `Accepted` before ready.
2. FMT-BREAK-038 (added), FMT-BREAK-021/022 (pointers), FMT-INDENT-033 (retired in place, heading and ledger line kept, body names the reason), `indentation.md` non-goal and `Uses` updated. No spec still states the retired held-comma behavior (grep for held/kept-off wording finds only unrelated text). The code (`LineBreakInitializerRewriter` calls `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` for initializer, anonymous, collection expression, list pattern, property pattern clause) matches FMT-BREAK-038.
3. `FMT-BREAK-038` and `FMT-INDENT-033` are in the ledger at sorted positions; FMT-INDENT-033 kept although retired.
4. Moved helper `CollapseSeparatorsToPreviousElementLine` is not named by any spec header; owners unchanged and exist.
5. No rule page affected (RH5301/RH5303/RH5304 pages describe the diagnostic and fix, no contradiction).
6. The three follow-up candidates (switch arm, positional pattern, base list commas) are under `Follow-up candidates` in `00-lead.md`; no output question is open.
7. FMT-BREAK-038 is covered.
8. `InitializerLeadingCommaTests` (11 tests) and the RH5301/RH5303/RH5304 fix-path tests carry `[Requirement("FMT-BREAK-038")]` plus their rule requirement, as in the `Tests` table. Spec examples (moving commas, arrays `e`, `f`, `y`) are tagged.
9. See *Whole run*.

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None blocking. Two notes below. | — |

Notes (not findings):
- Change docs `comma-behind-comment-unsupported.md`, `initializer-leading-comma.md`, `rh5107-comment-behind-leading-comma.md` are all `Status: Draft`; the lead sets `Accepted` before the PR is marked ready (check 1).
- The changed examples of FMT-BREAK-011 and FMT-BREAK-037 only lost cases (held-comma lines). The diff adds or newly tags no test for them, but the existing tagged tests (`ConstraintClauseLineBreakTests`, `LineBreakRequirementTests`) still cover the remaining example text; I did not find an assertion that fails against the reduced example. The tuple tests `TupleLeadingCommaStaysAligned` and `TupleTypeLeadingCommaIsAlignedWithTheElements` carry no tag, as before; they verify no requirement the diff adds, so that is not a finding.

### Checked
1. Change doc exactly when required: three docs, one per behavior change (declaration, initializer layout, RH5107 fix); each has `Visible output changes`; the declaration's doc has `Special cases` with `Deleted` and `Checked and kept`, its visible changes answered by escalation 10 (Q4 A, Q5 A), and `Depends on` links resolve. Statuses see notes.
2. FMT-UNSUP-009 (added; reasons, location, declarator tie rule, examples both sides) matches `PrecheckPhase.FindTriviaInFrontOfComma` and the candidate order (declarator commas keep FMT-UNSUP-004's reason, others FMT-UNSUP-009's). FMT-BREAK-011/012/037, FMT-INDENT-032, FMT-STRUCT-027, RH5110-003 changed as the diff deletes the guards (`WouldJoinAcrossUnjoinableTrivia`, held-separator operands, constraint comma alignment, `TrailingCommaRemovalUtilities` leading-trivia branch). `AGENTS.md` PrecheckPhase row names the third shape.
3. Ledger: `FMT-BREAK-038`, `FMT-INDENT-033`, `FMT-UNSUP-009`, `RH5107-004` added at sorted positions; no deletion or renumbering.
4. Code owners: architecture project passes (118/118); `unsupported-syntax.md` owners unchanged (`PrecheckPhase` exists); `Uses` lines updated (FMT-UNSUP-009 and SHARED-FIX-010 in RH5110, FMT-BREAK-038 in indentation). No owner renamed, so `sdd/specs/README.md` needs no change.
5. User documentation: no `documentation/` file changed or needed; RH5110 loses its fix only for the refused shape, covered by `general-notes.md`.
6. Open questions routed: all output questions have answers under `Maintainer answers` in `00-lead.md`; none is written into a spec as open; follow-ups are listed.
7. Architecture result quoted above; every new active requirement (RH5107-004, FMT-BREAK-038, FMT-UNSUP-009) has a covering `[Requirement]` test.
8. `UnsupportedListSeparatorCommentTests` (all tests), `CliHiddenCodeEndToEndTests` (2), RH5110 (2 new, 1 rewritten), RH5301/RH5303 withheld-fix tests carry the tags the `Tests` table names, including FMT-UNSUP-002/003/004/005/007 and SHARED-FIX-010 where listed; renamed tests keep their requirement tags.
9. `sdd/work/` holds only `comma-comment-layout`.
