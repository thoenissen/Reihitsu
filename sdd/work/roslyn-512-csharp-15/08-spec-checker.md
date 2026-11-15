Role: spec checker
Commit: f47c438
Verdict: consistent

The prompt carried no intended fix or hoped-for verdict to ignore.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/roslyn-512-csharp-15.md` exists (the maximum moves, so intended behavior changes), Status `Accepted`, has `Visible output changes`, no task list; `Decisions` records maintainer answers 1A, 2A, 3A, 4A.
2. Specs match the diff: ok. FMT-PIPE-008 (C# 15, reworded `Why:`) and CLI-004 (15.0 warning, 15/15.0 accepted, union example) match `LanguageVersionResolver.MaxSupportedLanguageVersion` and the tests; format kept (normative sentence first, `Why:` present, examples).
3. IDs: ok. No requirement added, retired, or renumbered; `requirement-ids.txt` untouched, both IDs present.
4. Code owners: ok. No spec header or owner changed; no type renamed, added, or removed.
5. User documentation: ok. `src/Reihitsu.Cli/README.MD` names 15.0; no rule page affected; no formatter/corner-case notes added.
6. Open questions: ok. All output questions answered in `00-lead.md`; follow-up candidates listed there.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 118, Skipped: 0, Total: 118. No new requirement; the changed ones are covered by the `[Requirement]` tests below.
8. Tags: ok. Every added test the 04 table lists against FMT-PIPE-008 / CLI-004 carries its tag (ExceedsMaximum newly tagged). The two untagged additions (`HelpNamesCSharp15AsTheVersionLatestSelects`, `UnionWithOneLineBracesKeepsThemWithoutLangVersion`) are listed with no requirement in the table. Both examples under "Examples for the tests" are tagged.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory (to be deleted by the lead before publishing).
