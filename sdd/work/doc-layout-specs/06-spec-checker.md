Role: spec checker
Commit: 448ee23
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Nothing in the inputs asked for one.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The diff adds no `sdd/changes/` file. Every requirement writes down current behavior, and the two rule-page edits correct text. None is a change of intended behavior.
2. Specs match the diff: ok. The 49 requirements of RH8301-RH8309, RH8401, RH8402 and RH8501 follow the format (one per heading, normative sentence first, `Why:`, examples, `{|RH####:…|}` markup). No requirement contradicts the code. All 255 tests of these rules pass on unchanged production code, and the tests derive from the spec examples. The `This requirement covers …` sentences follow the pattern the existing analyzer specs use. I found no open question in a spec: a grep for question, principle, issue numbers, "bug", and "characterization" finds nothing, and the spec text has no `Q<n>`, `P<n>`, or `#<n>` marker.
3. IDs: ok. The ledger diff is 49 added lines after `RH8204-003` in sorted position and removes none. Headings per spec equal ledger lines per spec for all twelve specs. No reserved ID is minted: grep for SHARED-DOC-017, SHARED-DIAG-004 and above, SHARED-USING-007, SHARED-TYPEREGION-002/006, and SHARED-FIX-006 finds none in the new specs. `sdd/specs/shared/`, `sdd/templates/`, and `src/` outside test projects are untouched.
4. Code owners: ok. `scripts/test.sh --project architecture` includes `SpecConsistencyTests` and `SpecStructureTests`, and passes. I checked the header rows against the code myself:
   - Every analyzer and code-fix provider named exists under `src/Reihitsu.Analyzer` or `src/Reihitsu.Analyzer.CodeFixes`.
   - Category is `Documentation` for all twelve rules.
   - Severity is `Warning`. `DiagnosticAnalyzerBase` fixes it, and no rule passes `isEnabledByDefault: false`, so "Enabled by default" is `Yes`.
   - "Code fix" is `Fix All` for the nine rules whose provider returns `WellKnownFixAllProviders.BatchFixer` (RH8301-RH8305, RH8307-RH8309, RH8401, RH8402), and `None` for RH8306 and RH8501, which have no provider.
   - The only owner beyond analyzer and provider is `XmlDocumentationElementOrderingUtilities` in RH8309, which the lead allowed. None of `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationCommentCodeFixUtilities`, `CopyrightHeaderTemplateResolver`, `FormattingTextAnalysisUtilities`, or `DocumentationCommentUtilities` is claimed.
   - `sdd/specs/README.md` has twelve new rows whose owners match the headers.
   - Observation, not a finding: RH8204 and `DirectDocumentationSyntaxChecker` also call `XmlDocumentationElementOrderingUtilities.GetTagName`. RH8309 specifies the order and the rank, not `GetTagName`, and RH8204 does not name the type, as the lead required.
5. User documentation: ok.
   - `documentation/rules/RH8309.md` drops "in declaration order" and says elements of one kind keep their written order. This matches RH8309-002 and RH8309-004. Its Correction equals the fix output: summary, `<param a>`, `<param b>`, then returns.
   - `documentation/rules/RH8501.md` says "not empty or consist only of white space", which matches RH8501-002 (`null` is also reported).
   - Neither page mentions the formatter. Both end with the `general-notes.md` footer, and `RuleDocumentationConventionTests` passes as part of the architecture run.
   - The ID range RH83xx/RH84xx/RH85xx fits the Documentation category. The help-link pages for all twelve rules exist. Every rule ID the specs cross-reference (RH0001, RH5020, RH5023, RH5031, RH8022-RH8025, RH8030, RH8031, RH8102, RH8109) has a page.
   - I did not check the other ten pages against their specs beyond the convention tests. The spec author reports they needed no change.
6. Open questions: ok. `03-spec-author-repair.md` lists all of them under "Open questions to file", one section per spec path, for all twelve specs, and says the section replaces the one in `01`. The lead files them as `SDD open questions: <spec path>`; no issue exists yet, which the handover states. None is recorded only in a PR.
7. Coverage: ok.
   - `scripts/test.sh --project architecture` result line: `Passed!  - Failed: 0, Passed: 72, Skipped: 0, Total: 72`. It includes `RequirementCoverageTests`.
   - Every new active requirement has a covering `[Requirement]` test. Tags on every changed test file are consistent with the specs.
   - The tests the handovers say must stay untagged are untagged: RH8401 `VerifyNoDiagnosticsWhenDocumentationModeIsNone`, RH8303 `VerifyDocumentationAfterSwitchLabelProducesDiagnostic`, RH8402 `MismatchedConfiguredHeader` and `MismatchedBlockCommentHeader`, and RH8305 `VerifyNoDiagnosticsWhenDocumentationModeIsNone`.
   - No retired or unknown ID is tagged.
   - `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH830|FullyQualifiedName~RH8401|FullyQualifiedName~RH8402|FullyQualifiedName~RH8501"`: `Passed! Failed: 0, Passed: 255, Skipped: 0, Total: 255`.
8. Nothing left behind: ok. `sdd/work/` holds only `doc-layout-specs`, which the lead deletes before publishing. The working tree was clean before this handover.
- No production code changed. `git diff origin/main..HEAD -- src` touches only test projects (twelve files under `src/Reihitsu.Analyzer.Test`). The only removed line in them is a BOM `using` line that was re-added together with three more usings. `scripts/lane-check.sh tests` reports `LANE CHECK: confirmed — claimed tests, diff needs tests`.
