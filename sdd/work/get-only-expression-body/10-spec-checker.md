Role: spec checker
Commit: 26ef624
Verdict: consistent

## thoenissen/Reihitsu#849

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

### Checked
1. ok: change doc `sdd/changes/2026-11/get-only-expression-body-rules.md` exists (new rules), is `Accepted`, has no task list, has `Visible output changes`; its `Decisions` section records the maintainer's answers (comment/directive kept list reported without fix, Q4, Q5 not declared, Q6 shared Core owner, Q7).
2. ok: RH3209-001 and SHARED-GETONLY-001 to -007 match the diff (SHARED-GETONLY-003/-005 clarification of 08 matches `DelimitedCommentTrailingTheExpressionStaysBehindIt`); format followed.
3. ok: nine IDs inserted at sorted positions in `requirement-ids.txt`; none reused or removed.
4. ok: `SpecConsistencyTests` pass; `sdd/specs/README.md` rows for RH3209, RH3210, and shared spec match owners; the AGENTS.md guard name `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective` exists.
5. ok: `RH3209.md` is short, no formatter mention, no withheld-fix note, footer present, Correction equals the fix output; Clarity range fits; `Release.ruleset` and `Reihitsu.sln` list both pages/rules.
6. ok: no open output question; all answered under `Maintainer answers`; follow-ups listed in `00-lead.md`.
7. ok: `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198"; every new requirement has a tagged test.
8. ok: every added test method carries `[Requirement]` tags matching the `05`/`07` tables, including the split CRLF test; `Examples for the tests` requirements are all tagged.
9. ok: `sdd/work/` holds only this run's folder (the lead deletes it before publishing).

Note (not a finding): a delimited comment on its own line in front of an expression-bodied getter's `;` (example B of the SHARED-GETONLY-003 negative example, 08) has no test of its own; the single-line variants are covered. Optional for the lead.

## thoenissen/Reihitsu#850

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

### Checked
1. ok: same change doc; RH3210 covered by it.
2. ok: RH3210-001 plus the shared requirements match the diff and the indexer tests.
3. ok: `RH3210-001` in the ledger at its sorted position.
4. ok: spec header owners `RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer`/`…CodeFixProvider` exist; README row present.
5. ok: `RH3210.md` same form as RH3209 with an indexer example; no formatter mention.
6. ok: as above.
7. ok: same architecture run (198 passed, 0 failed); RH3210-001 tagged.
8. ok: indexer analyzer and formatter tests tagged per the `05` table.
9. ok: as above.
