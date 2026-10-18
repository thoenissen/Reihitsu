Role: spec checker
Commit: 0639f87
Verdict: consistent

Repair-round re-check after `07-auditor.md` (findings 1 to 4), `08-spec-author.md`, `09-test-implementer.md`, and `10-cleaner.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. The tree was clean at `0639f87`. Diff since my first check (`bd63778..HEAD`): one spec file (`sdd/specs/shared/element-documentation.md`, three coverage sentences and one `Why:`), the BOM of `InMemoryXmlReferenceResolver.cs`, two test classes (RH8001, RH8013), and the work docs. The ledger and the index are unchanged.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc exactly when required: ok. The repair only narrows coverage sentences (it records which inputs the specs do not claim); no requirement gained, changed, or lost behavior, and `sdd/changes/` is untouched.
2. Specs match the diff: ok.
   - SHARED-DOC-002 now excludes "a partial type declared in several parts", matching SHARED-DOC-001 and SHARED-DOC-005 and closing audit finding 1; its `Why:` states the partial-type exception.
   - SHARED-DOC-005 adds "without a namespace prefix, and in no XML namespace" (SHARED-DOC-006 narrows with it, since it covers "what SHARED-DOC-005 covers").
   - SHARED-DOC-007 now also requires an included file whose tag names are lower case, prefix-free, and in no namespace, closing audit finding 2.
   - No example changed, and no active test asserts an excluded input: the only `partial` in the R01 to R19 tests are the single-part `VerifyNoDiagnosticForPartialClassWithSummary` (RH8001) and `VerifyNoDiagnosticForPartialRecordWithSummary` (RH8007), both still covered by the coverage sentences.
3. IDs: ok. No ID added, retired, or renumbered; `requirement-ids.txt` and `sdd/specs/README.md` have no diff since `bd63778`.
4. Code owners exist: ok. No owner changed.
5. User documentation: ok. The rule pages are untouched and still consistent with the specs; no code fix exists, so there is no `### Correction` to compare.
6. Open questions: ok. thoenissen/Reihitsu#1037 (updated 2026-10-18 13:51 UTC) contains every addition of `08-spec-author.md`:
   - Q2 gained the undeclared-prefix shape (`<x:summary>`, CS1570);
   - "Q3, continued" covers the group of a modifier-less part of a partial type, touching SHARED-DOC-001, 002, 005, 006, 007 and RH8001 to RH8010, RH8017, RH8018;
   - Q5 is retitled "…and regardless of XML namespace" and covers included `<Summary>`/`<INHERITDOC/>` and the namespaced summaries, with readings extended.
   Every excluded case in the three coverage sentences maps to a question: separated comments (Q1), malformed XML (Q2), partial types and constructors (Q3, Q3 continued), interface static constructor (Q4), case and namespace (Q5).
7. Requirements covered, specs consistent: ok. `scripts/test.sh --project architecture --configuration Debug`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. The repair adds no active requirement, so no new covering test is required. The RH8001 and RH8013 malformed-XML tests still carry no `[Requirement]` tag; I did not re-run the analyzer project (the cleaner reports 287 passed and the test implementer 305 passed on this tree).
8. Nothing left behind: ok for now. `sdd/work/` holds only `element-documentation-specs/`; the lead deletes it before publishing. Open note for the lead: `00-lead.md` still says "Roles skipped: … auditor" in its *Lane* section, written before the lane widened; it is deleted with the folder.
