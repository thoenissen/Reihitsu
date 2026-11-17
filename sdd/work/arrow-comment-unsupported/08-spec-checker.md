Role: spec checker
Commit: 5b9b9d5
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. Change doc: ok. `sdd/changes/2026-11/expression-body-arrow-comment-unsupported.md` exists for the added FMT-UNSUP-012, is `Accepted`, has `Visible output changes` 1-10 and no task list; its `Decisions` section records the maintainer's batch-1 answers (declaration, accessors and local functions, directives, one line, reason text, code fixes), matching `00-lead.md`.
2. Specs match the diff: ok. FMT-UNSUP-012 (new), FMT-STRUCT-006/007/015/024/026, SHARED-BODY-001, RH5401-004, RH7101-007 and the `AGENTS.md` PrecheckPhase row match the four production changes (precheck check, join refusal behind the arrow deleted, converter cases deleted, get-only seam check deleted). FMT-BREAK-026 states no comment rule, so needs no edit. Format (one heading, normative sentence, `Why:`, examples, markup) holds.
3. IDs: ok. FMT-UNSUP-012 is the next free number, inserted in sorted position in `requirement-ids.txt`; nothing reused, renumbered, or removed.
4. Code owners: ok. No type added, renamed, or removed; `Uses` rows updated in unsupported-syntax, structural-transforms, shared/expression-body-conversion, RH5401, RH7101; `sdd/specs/README.md` needs no change.
5. User documentation: ok. No rule page touched; the withheld fixes fall under `general-notes.md`.
6. Open questions: ok. All six batch-1 questions are answered in `00-lead.md`; the in-front-of-the-arrow shape is under Follow-up candidates; no handover carries an open output question.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 197, Skipped: 0, Total: 197. FMT-UNSUP-012 is covered by 38 tagged tests, the new CLI test included.
8. Tags: ok. Every table row of 04-test-implementer.md carries its tag in the diff (FMT-UNSUP-012, -001, -003, -005, -010, RH7101-007, SHARED-BODY-001, SHARED-FIX-010; the renamed RH5401-004, FMT-STRUCT-006 and FMT-STRUCT-024 example tests keep their tags). Every requirement under `Examples for the tests` is tagged on at least one test.
9. Nothing left behind: ok. `sdd/work/` holds only `arrow-comment-unsupported`, which the lead deletes before publishing.
