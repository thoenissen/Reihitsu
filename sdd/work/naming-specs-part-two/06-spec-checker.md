Role: spec checker
Commit: 6ca8237
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane claimed: tests; `scripts/lane-check.sh tests` reports "LANE CHECK: confirmed — claimed tests, diff needs tests". The diff against `origin/main` is 16 analyzer specs, `sdd/specs/shared/single-letter-names.md`, the index and ledger, 16 test files under `src/Reihitsu.Analyzer.Test/Naming/`, and this run's `sdd/work/` files. No file under `documentation/`, `sdd/changes/`, or a production project changed.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 (format: a rule a shared policy states is referenced, not restated) | `sdd/specs/analyzer/RH4121.md`, RH4121-004 and its `Uses` row | The first clause of RH4121-004, "the old name has no letter or digit, such as `_`", is the first clause of SHARED-CASING-008, which RH4121 does not list under `Uses`. Only the second clause (a derived body that is neither PascalCase nor digits only, such as `1Key`) is RH4121's own. Severity: low; the behavior stated is correct and tested. | spec author |

## Checked
1. Change doc exactly when required: ok. No requirement is added with new behavior, changed, or retired for existing behavior; every spec writes down current behavior, and the open-question cases are left out. No change doc is needed and none exists.
2. Specs match the diff: finding 1. Otherwise ok. The diff changes no production behavior; I read all 16 specs and the shared spec. Each requirement has its own heading, a normative sentence first, `Why:`, and an example (checked by script across all 17 files); analyzer markup is `{|RH####:…|}`. Coverage limits (referenced-assembly chains, record-positional names, `_`, `const`) follow the RH4102-001 and RH4103-001 precedent. Observation, not a finding: RH4122-003 lists `record`, `record class`, and `record struct` but not `readonly record struct`; the exemption in `SingleLetterIdentifierUtilities.IsRecordPrimaryConstructorParameter` keys on `RecordDeclarationSyntax`, so it covers that form too.
3. IDs: ok. The 36 new IDs plus SHARED-SINGLE-001 and SHARED-SINGLE-002 are in `requirement-ids.txt` at their sorted positions; no line removed or moved (`SpecConsistencyTests` and the ledger-sorted test pass).
4. Code owners exist: ok. `scripts/test.sh --project architecture` includes `SpecConsistencyTests` and `SpecStructureTests`, which pass. `sdd/specs/README.md` has 17 new rows with matching owners, including the two same-named-looking analyzers `RH4116TupleElementCasingAnalyzer` and `RH4118TupleElementCasingAnalyzer` (distinct types, one owner each).
5. User documentation: ok. No rule page changed. No spec requirement contradicts a page except the RH4115 page sentence "Variables declared by a deconstruction are handled by RH4117 instead", which is the filed open question thoenissen/Reihitsu#1019 Q3. The diagnostic ID range fits (RH41xx Naming); every `helpLinkUri` page exists.
6. Open questions recorded: ok. All eleven questions of `03-spec-author.md` are filed, and I read each issue on GitHub:
   - Q1–Q4: thoenissen/Reihitsu#1019 (RH4115)
   - Q5: thoenissen/Reihitsu#1020 (RH4121)
   - Q6: thoenissen/Reihitsu#1021 (RH4118)
   - Q7: thoenissen/Reihitsu#1022 (RH4124)
   - Q8: thoenissen/Reihitsu#1025, thoenissen/Reihitsu#1026, thoenissen/Reihitsu#1027 (RH4114, RH4112, RH4113)
   - Q9: thoenissen/Reihitsu#1009, a comment titled Q5
   - Q10: thoenissen/Reihitsu#1023 (RH4116)
   - Q11: thoenissen/Reihitsu#1024 (RH4122)
   All are sub-issues of thoenissen/Reihitsu#912 with the title `SDD open questions: <spec path>`. The tests pin none of these cases; the one pre-existing test that touches Q1, `NoCodeFixForLetterlessLocalVariable`, is untagged.
7. Requirements covered and specs consistent: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72" (includes `RequirementCoverageTests`; every one of the 38 new IDs has a covering `[Requirement]` test). No tracker number appears in the added test or source lines.
8. Nothing left behind: ok for now. `sdd/work/` holds only `naming-specs-part-two/`, which the lead deletes before publishing.
