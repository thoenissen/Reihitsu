Role: spec checker
Commit: 80c743f
Verdict: consistent

Re-check of the repair round for finding 1 of `08-spec-checker.md`, on head 80c743f (specs in 8f02d0f, tests in c1964af). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed no file other than this handover. I reviewed `git diff 349dc02..HEAD` (two specs, two test files, three handovers) and the two comments on thoenissen/Reihitsu#1093.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
- **The #1093 case is absent from every spec.** The case is a one-line property, indexer, or event whose accessor list SHARED-ATTR-006 does not exempt.
  - SHARED-ATTR-013 now covers only lists "not attached to an accessor" and sends accessor lists to SHARED-ATTR-014.
  - SHARED-ATTR-014 reports a multi-attribute accessor list only when the owner spans more than one line, and its example lost the `int Q { [return: A, B] get => 1; }` line.
  - RH5531-001 cites SHARED-ATTR-013 and SHARED-ATTR-014.
  - RH5531-006 withholds only for SHARED-ATTR-019, and its example lost `int Q { [A] /* c */ {|RH5531:[B]|} get; set; }`.
  - No other spec of the 16 shows the case: RH5528, RH5518, RH5502, RH5504, RH5516, and RH5520 have only exempt or multi-line accessor examples (grep over `get =>` and one-line `Q`).
  - The other #1093 instances (`param:`/`typevar:` on a method or accessor, `[return: A] [return: B] get;`, RH5531 merging on a non-exempt one-line property) remain unwritten, as in my first review.
- **The case is absent from every tagged test.** I scanned every `[Requirement]`-tagged test of the 16 classes for an accessor list on one line that holds an attribute list together with `=>`, `{ }`, `/*` or `//`. One match: RH5528 `VerifyNoDiagnosticForInlineAutoPropertyWithOwnAttributeListAndTrailingComment`, whose comment follows the accessor list (`int Q { [return: A, B] get; set; } = 1; // c`). That is still the exempt case of SHARED-ATTR-006, so it may stay tagged.
  - RH5528 `…OnlyWhenTheAccessorListSpansSeveralLines` (tags RH5528-001, SHARED-ATTR-014) now holds exempt `P` and multi-line `R` only.
  - RH5528 `…WhenCommentSitsBetweenTheAccessors` and RH5531 `…WhenCommentSitsBetweenOneLineAccessorLists` are untagged characterization tests.
  - The existing untagged tests (`…OnSingleLinePropertyAccessorWithExpressionBody` of RH5518 and RH5528, and the others listed in `08`) are still untagged.
- **The addendum on #1093** names SHARED-ATTR-013, SHARED-ATTR-014, and RH5531-006. These are exactly the three requirements the repair narrowed to leave the case out.
- **Coverage kept.** `scripts/test.sh --project architecture --no-install`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72" (includes `RequirementCoverageTests`, `SpecConsistencyTests`, `SpecStructureTests`).
  - RH5531-006 is still covered by `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent` (multi-line `[First, /* keep */ Second]`, tags RH5531-006 and SHARED-ATTR-019).
  - RH5528-001 and SHARED-ATTR-014 keep several tagged tests.
  - SHARED-ATTR-020 stays covered by the RH5524 and RH5529 classes. RH5531 no longer lists it under `Uses`, and `SpecStructureTests` passes, so `Uses` matches the references.
  - RH5531's two retagged tests carry SHARED-ATTR-014 in place of or next to SHARED-ATTR-013, as the narrowed SHARED-ATTR-013 requires.
- **Nothing passed earlier is broken.** `scripts/test.sh --project analyzer --no-install --filter "FullyQualifiedName~ShapeRulesAnalyzerTests"`: "Passed!  - Failed:     0, Passed:   200, Skipped:     0, Total:   200". No ledger, code-owner, index, rule-page, or production change in the repair diff (checks 1, 3, 4, 5 of `08` unchanged). The #1109 and #1110 content is still absent from the specs and from tagged tests, since the repair touched neither. `sdd/work/` holds only `attribute-list-shape/`, which the lead deletes before the PR is marked ready.
