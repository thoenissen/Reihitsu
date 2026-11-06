Role: spec checker
Commit: 4914c182
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No `sdd/changes/` file in the diff; the lane is bug fix, the maintainer's triage comments and Decision 2/5 rule out a change doc, and no visible output change is listed. RH3103-006 is retired in place (Status line), not deleted.
2. Specs match the diff: ok. RH3001-007, RH3004-005/-006, RH3005-007/-008, RH3101-005..-007, RH3103-007, RH3104-005/-006, RH3105-005/-006, RH3106-002/-003, RH3201-006/-007, RH5417-005 and SHARED-PARENS-003 each have a heading, normative sentence, `Why:` and markup examples. RH3106-003 matches maintainer answers 2 and 4 (the `Run(object)` case); RH3201-007 matches answer 5. No open question is written into a spec.
3. IDs: ok. All 19 new IDs are in `requirement-ids.txt` at sorted positions, no ID reused, renumbered, or removed.
4. Code owners: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115" (includes SpecConsistencyTests and `UsesListsExactlyTheReferencedRequirements`). Owner lines of the touched specs are unchanged and exist; `sdd/specs/README.md` is untouched and needs no change (no spec added or removed, owners unchanged). The new helpers (`RewriteParseUtilities`, `EmptyParenthesesCodeFixProviderBase<TNode>`) are not spec owners.
5. User documentation: ok. RH3101.md (Derived `internal`, `virtual` dropped, per answer 3) matches RH3101 spec; RH3103.md drops the nameof note, as the report no longer exists there and withheld-fix notes do not belong on a page. No other page needs a change (RH3106 page example still holds); no formatter mention added.
6. Open questions: ok. Handovers 01-03 list no open output question; Q1-Q3 and answers 4-5 are recorded under `Maintainer answers` in `00-lead.md`. Follow-up candidates are listed there, none only in the PR.
7. Coverage: ok. Every one of the 19 new requirement IDs has at least one `[Requirement]` test in the diff (counted by tag in the diff, e.g. RH3104-005 and RH3103-007 many times; each ID at least once).
8. Tags: ok. Every requirement tagged by a test the diff adds appears in `04-test-implementer.md`; every requirement named under `Examples for the tests` (RH3001-007, RH3005-008, RH3101-005, RH3105-005, RH3004-003/-005/-006, RH3106-001/-002/-003, RH3201-006) is tagged on at least one test in the diff. No test still carries the retired RH3103-006 (the tag occurrences are only on removed lines).
9. Nothing left behind: ok for now. `sdd/work/` holds only `withhold-unsafe-fixes`; the lead deletes it before publishing.
