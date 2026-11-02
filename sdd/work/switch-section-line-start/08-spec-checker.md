Role: spec checker
Commit: be0d6bc
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

## Checked
1. Change doc: ok. `sdd/changes/2026-11/switch-section-line-start.md` is required (three requirements added, two changed), is `Accepted`, has `Visible output changes` (entries 1 to 6) and no task list. Every entry has the maintainer's answer under `Decisions` / `Maintainer answers` and in `00-lead.md` (answers 1 to 7).
2. Specs match the diff: ok. FMT-BREAK-033/034/035 and the FMT-BREAK-006 scope, FMT-BLANK-004 (blank line in the first run), and the scope line in `switch-case-braces.md` describe the diff. Format (one heading, normative sentence, `Why:`, examples) holds. Note, not a finding: the narrow `SwitchCaseBraceRewriter` change (no doubled line break behind a braced section, lead decisions 5, 6, 12) is a bug fix under the unchanged FMT-PIPE-004 / FMT-BLANK-004, so no requirement or change-doc text is needed.
3. IDs: ok. FMT-BREAK-033 to 035 are the next free numbers, at their sorted position in `sdd/specs/requirement-ids.txt`; nothing reused, renumbered, or deleted.
4. Code owners: ok. The touched production types (`BlankLineBreakSpacingRewriter`, `BlankLineEditor`, `LineBreakBlockRewriter`, `TokenGapNormalizer`, `SwitchCaseBraceRewriter`) are listed in the owners of `sdd/specs/README.md`; no type was renamed, added, or removed. `SpecConsistencyTests` pass inside the architecture run.
5. User documentation: ok. No rule page is touched, and the diff changes no rule behavior (change doc, "Analyzers are unchanged"); the RH5417 / RH5403 pages contain no switch example that the new layout contradicts.
6. Open questions: ok. 03-spec-author raises none; the skeptic's output questions are answered in `00-lead.md`; follow-up candidates 1 to 7 are listed there, none written into a spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. FMT-BREAK-033, 034, 035 each have covering `[Requirement]` tests.
8. Tags: ok. Every test in 04's and 06's `Tests` tables carries the listed tags (including the RH5417 / RH5403 fix tests and the renamed `MovesDelimitedDocumentationWithStatementBehindCaseLabel`); the spec author's `Examples for the tests` (FMT-BREAK-033 third example, FMT-BREAK-034 and -035 extended examples, entry 6) are tagged on tests the diff adds.
9. Nothing left behind: ok. Only `sdd/work/switch-section-line-start/` exists under `sdd/work/`; the lead deletes it before publishing.
