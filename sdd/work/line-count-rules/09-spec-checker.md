Role: spec checker
Commit: cb05f16
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. One finding, a cosmetic order in `sdd/specs/README.md`; nothing else does not match.

## thoenissen/Reihitsu#353

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/README.md`, row `shared/line-count.md` | The row sits after `shared/line-join-rules.md`; the table is ordered by file name, so `line-count.md` comes before `line-join-rules.md`. No test enforces the order. | spec author |

### Checked
Change doc (check 1): required (new rules, new setting section, new RH0001 behavior), `Accepted`, has `Visible output changes` 1–8, no task list; every visible change has its maintainer answer under `Decisions` (1A, 2B, 3A–8A, 9B, 10A, 11A, 12D–16A; change 7, RH8402, is part of 1A as recorded in `00-lead.md`).
Specs (2): RH2008-001 to -005, SHARED-LINES-001 to -003, RH0001-004 (changed) and RH0001-007 (new) match the code read (`LineCountAnalyzerBase`, `RH2008…Analyzer`, `ConfigurationManager.ParseDesign`/`ParseLineLimit`): defaults 60/100, spans between braces or `=>` plus expression, location tokens, strict whole-number validation, a null setting falls back to its default, either test property `true` case-insensitively. Format kept (one requirement per heading, `Why:`, examples with `{|RH####:…|}`).
IDs (3): ok. All 15 new IDs and RH0001-007 sit at their sorted positions in `requirement-ids.txt`; none reused or removed.
Owners (4): `LineCountAnalyzerBase` and both analyzers exist; `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. Finding 1 for the row order; the rows' owners match.
User documentation (5): `documentation/rules/RH2008.md` and `RH2111.md` are short, name no formatter, carry the footer, repeat nothing from `general-notes.md`, have no withheld-fix or corner-case notes; the violation examples count right (RH2008: 4 > 3; RH2111: 5 > 4) and each correction is at or below its limit; `RuleDocumentationExampleOptOuts` carries both with the reason in the handover. Package README lists both rules under Design with the right columns; `Reihitsu.sln` lists both pages; IDs 2008 and 2111 fit the Design range; `helpLinkUri` pages exist. `RH0001.md` needs no change. `buildTransitive/Reihitsu.Analyzer.targets` declares both `CompilerVisibleProperty` items. Rulesets: `Release.ruleset` and `Debug.ruleset` set RH2008 and RH2111 to `None` (answer 11A), and the self-hosting scans leave both out with a test for each direction.
Open questions (6): ok. The only open output question of the spec author and the skeptic (12) is answered 12D, with 13A–16A; no question is written into a spec or only in the PR. Follow-up candidates sit in `00-lead.md` (repository's own limits, compiler-visible properties in `Directory.Build.props`, a setting written twice).
Requirements covered (7): the architecture run above covers `EveryRequirementOfAnEnforcedSpecIsCovered`; every new active requirement has a `[Requirement]` test.
Tags (8): every test of the `Tests` table carries the requirement it is listed against; where the table lists a secondary requirement in parentheses (for example `MembersOfEveryTypeKindAreJudged`, `AccessorsAreNotSummed`, `ParameterListOfPositionalRecordIsNotCounted`) the test carries only the primary one, which is not a finding because the secondary one is covered by other tests. Every requirement under `Examples for the tests` (SHARED-LINES-002, SHARED-LINES-003, RH2008-001, RH0001-007) is tagged on tests the diff adds; the new RH8402 tests carry RH8402-001.
Nothing left behind (9): ok. `sdd/work/` holds only `line-count-rules`, which the lead deletes before publishing.

## thoenissen/Reihitsu#354

### Findings
None.

### Checked
Same as above for RH2111-001 to -006 against `RH2111TypesShouldNotExceedTheLineLimitAnalyzer` (class, struct, interface, record, record struct, union with a brace body; identifier location; defaults 500/1000). `documentation/rules/RH2111.md` ok. Tags: RH2111-001 to -006 each tagged on at least one new test.

## Whole run

- Verdict findings: only finding 1 (row order in `sdd/specs/README.md`), owner spec author.
- Note, not a finding: the test implementer's handover names commit 4c142e9 while its tests are in 1198769.
