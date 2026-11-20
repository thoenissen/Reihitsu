Role: spec checker
Commit: 0f29897d
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing but this handover.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/conditional-element-access-list.md:142` (visible output change 3) | Reads "After (location per the open output question in the run's handover; recommended: the element binding, ...)". The change doc is `Accepted` and its `Decisions` records question 2A, so the question is no longer open and the sentence is stale. It should state the answered location (the element binding, from `[` through `]`) | spec author |

## Checked

### thoenissen/Reihitsu#1877
- 1 change doc: ok, one doc covers both issues; required (FMT-BREAK-014 changes what is joined); `Accepted`; has `Visible output changes`; no task list; answer 1A under `Decisions` and `Maintainer answers` in `00-lead.md`. Stale wording in finding 1 touches only #1878's change.
- 2 specs match diff: ok. FMT-BREAK-014 now names the three parent kinds (`ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`, `ImplicitElementAccessSyntax`) and the fixed-size buffer list stays undecided, matching `SyntaxNodeUtilities.IsIndexArgumentList`; the format (normative sentence, `Why:`, examples) is kept.
- 3 IDs: ok, none added, retired, or renumbered; `requirement-ids.txt` unchanged.
- 4 code owners: ok, no spec header changed; `SyntaxNodeUtilities` is named in no spec, and no new type was added. `SpecConsistencyTests` is part of the architecture run below.
- 5 user documentation: ok, no rule page for #1877.
- 6 open questions: ok, question 1 answered 1A in `00-lead.md`; no open questions outside the run; follow-up candidates none.
- 7 coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. No requirement is new; FMT-BREAK-014 has covering tagged tests in the diff.
- 8 tags: ok. Every test of the `04` and `06` tables carries its tag, including the `[Requirement]` on the flipped `ConditionalElementAccessJoinsWrappedArgumentsWhenNestedIndexerCollapses` and the two Core tests; the spec examples for FMT-BREAK-014 are covered. `FixedSizeBufferSizeListStaysWrapped` is deliberately untagged (verifies no requirement).
- 9 nothing left behind: ok for now, `sdd/work/` holds only this run's directory (the lead deletes it before publishing).

### thoenissen/Reihitsu#1878
- 1 change doc: finding 1. Otherwise ok (answer 2A recorded, no task list, `Accepted`).
- 2 specs match diff: ok. RH5307-001 (refers to FMT-BREAK-014 by ID, per decision 2), RH5307-003 (element binding located `[` through `]`), RH5307-004 (new example with two diagnostics for `e?[0,⏎1][1,⏎2]`), intro and non-goals match the analyzer (`SyntaxNodeUtilities.IsIndexArgumentList`), the code fix, and the removal of `IsIndexerArgumentList`. Examples carry `{|RH5307:...|}` markup; every requirement has `Why:`.
- 3 IDs: ok.
- 4 code owners: ok, RH5307's header owners (analyzer and code fix provider) still exist; `Uses` (FMT-BREAK-001, FMT-BREAK-014, SHARED-FIX-006) unchanged and sufficient; `sdd/specs/README.md` unchanged and correct.
- 5 rule page: ok. `documentation/rules/RH5307.md` drops the exclusion and names `target?[...]`; it is short, names no formatter, has no withheld-fix or corner-case note, keeps the footer; `### Correction` equals the fix's output; the ID range (RH5xxx Layout) fits and the page exists.
- 6 open questions: ok, question 2 answered 2A.
- 7 coverage: ok (architecture result above). RH5307-001, -003, -004 each have covering tests in the diff.
- 8 tags: ok. RH5307 tests carry -001/-002/-003/-004 as the table lists; every requirement the spec author named under `Examples for the tests` (RH5307-001, -003, -004) is tagged on at least one new test.
- 9 nothing left behind: ok, as above.
