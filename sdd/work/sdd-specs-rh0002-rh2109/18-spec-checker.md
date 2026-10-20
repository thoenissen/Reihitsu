Role: spec checker
Commit: c22e86c8cbba6f7b39778dd771beaa00b58fb2c7
Verdict: consistent

Scope: the audit repair only, `git diff 53efd05 HEAD -- . ':(exclude)sdd/work'` (8 paths: `sdd/specs/README.md`, RH1001, RH1002, RH2001, `shared/struct-equality.md`, the decisions entry, and the RH1001 and RH1002 test classes). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing except this file.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Observations (not findings)
- SHARED-EQUAL-002 says an argument counts as not custom "exactly when" it has one of the listed forms, and the list names "a cast that is not a user-defined conversion". The code also unwraps an `as` conversion: a scratch build (outside the repository, deleted afterwards) reported RH1002 for `items.Distinct(EqualityComparer<S>.Default as IEqualityComparer<S>)` and `items.Distinct(null as IEqualityComparer<S>)`, so those two count as not custom. Reading "cast" to include `as` makes the spec true, so I raise no finding. If the spec author wants the list literal, one clause covers it. No test depends on it.

## Checked
1. Change doc: ok. The repair is clarification only (no requirement added, changed in behavior, or retired), so no change doc is needed.
2. Specs match the diff: ok.
   - RH1001-002: the new coverage sentence (only `new` creations, no collection expressions) agrees with the RH1001 Q3 text and `VerifyCollectionExpressionArgumentIsNotReportedWhereTargetTypedCreationIs`.
   - RH1001-003: the form list is exhaustive, with the `throw` and `with(…)` coverage sentence. It matches `HasExplicitEqualityComparerInitializer`, the RH1001 Q1 and Q2 texts, and the characterization tests for both.
   - RH1002-001: the invocation-only coverage sentence agrees with RH1002 Q2 and `VerifyQueryExpressionClausesAreNotReported`.
   - SHARED-EQUAL-002: "exactly when" list, the `Default ?? x` form, "every other argument is custom" (a local holding `null` included), and the `throw` coverage sentence match `EqualityComparerArgumentUtilities` and the struct-equality Q4 text, whose RH1002 and RH1001 inputs are pinned by two characterization tests.
   - Format: the normative sentence comes first, `Why:` is present, and the markup is `{|RH####:…|}`.
3. IDs: ok. `git diff` shows no change to `sdd/specs/requirement-ids.txt`, and no requirement was added or renumbered.
4. Code owners: ok. `PropertySymbolExtensions` is the only production file that implements RH2001's auto-property decision. Its single consumer is `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer` (`symbol.IsAutoProperty()`). `grep` over `src` and `sdd` finds it in no other spec. The RH5047 and RH5408 analyzers and the formatter have their own syntax-level `IsAutoPropertyAccessorList`. It appears in the RH2001 header and the `sdd/specs/README.md` row, and `SpecConsistencyTests` passes.
5. User documentation: ok. No rule page changed, and none is affected, since the repair only clarifies. RH1001's page says the exemption applies "only if every one of those creations supplies a comparer", consistent with RH1001-003.
6. Open questions: ok. 15 lists the full set of issue texts, one per spec path, for the lead to file. The new or changed ones are:
   - the new `SDD open questions: sdd/specs/analyzer/RH1001.md` issue (Q1 to Q3);
   - RH1002 Q2;
   - struct-equality Q4.
   Each "Touches" line names the requirement whose coverage sentence leaves the case out, and each input equals the input of its characterization test (compared by hand). RH1001-005 and RH1002-005, which Q3 and Q2 cite, exist.
7. Coverage: ok.
   - `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72".
   - The two tagged tests (`VerifyOnlyInitialValuesOfRecognizedCreationFormsExemptTheDeclaredType` for RH1001-003, `VerifyFormOfArgumentDecidesWhetherComparerIsCustom` for SHARED-EQUAL-002 and RH1002-001) contain the spec examples verbatim. I compared each spec block with its test raw string after normalizing the markup, and the text and the markup count match (15 and 11).
   - Both sides of each named form are covered:
     - parentheses, `!`, `??`, conditional, `switch`, null-conditional, a user-defined cast, `default`, and the local holding `null`, in SHARED-EQUAL-002;
     - creation, cast, conditional (also with a constant condition), `switch`, several declarators, user-defined cast, `??`, variable, `default`, collection expression, and `throw` next to a creation without a comparer, in RH1001-003.
   - The characterization tests carry no `[Requirement]` tag.
8. Nothing left behind: ok. The working tree was clean, and `sdd/work/` holds only this run's directory. The lead deletes it before publishing.

Extra checks:
- The decisions entry `squad/decisions/2026-10-20-996-auditor-block-extra-repair.md` is a faithful record. The format follows `squad/decisions/README.md`, escalation 7 ("the auditor still blocks after one repair round") fits, and the date is right. The question summarizes auditor findings 1 to 3 and hints 1 and 2 of 14 accurately, the options are listed, the answer matches the lead's instruction, and the reason is "None given".
- `scripts/lane-check.sh tooling`: "LANE CHECK: confirmed — claimed tooling, diff needs tooling".
