Role: spec checker
Commit: 3293cb0e5930dfd1ee53cde3657649cbe209ee0a
Verdict: consistent

Second check, after the second repair round (`11-auditor.md` findings 1 to 7, repaired in `12`, `13`, `14`). Inputs: `00-lead.md`, `11` to `14`, thoenissen/Reihitsu#1331 (body and both comments), `git diff origin/main...HEAD`, and `git diff 37795fd HEAD` for the repair. No prompt-carried fix or verdict was present. I changed no file other than this handover.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None. | - |

## Checked
1. Change doc: ok. The diff adds no requirement with new behavior and none is retired. It states decided principles (P25 to P31) in the shared specs, a clarification lane with no production code. Both new IDs (SHARED-DIAG-009, SHARED-DIAG-010) were planned in `00-lead.md` from the issue. No change doc is required.
2. Specs match the diff: ok. The repair changes only text and tests.
   - SHARED-DIAG-004 paragraph 2 is limited to what one part writes independently: the accessibility of one part of a partial type (RH7104) and modifier order (RH7105, RH7106). That is the P28 reach in the issue, "Covers RH7104, RH7105, RH7106".
   - A new paragraph keeps the accessibility of a partial member under the first paragraph (P15, thoenissen/Reihitsu#1287). The Why no longer claims every per-part correction compiles.
   - The RH2004 example is recaptioned without a symbol rationale. A new partial-property example states decided behavior that does not conform today. By the 1260 no-conforming-shape precedent it stays without a pinning test, and the tracking issue is thoenissen/Reihitsu#1287.
   - SHARED-DOC-004's fix clause is scoped: a fix moving neither the declaration nor the code leaves the comment where it stands, except RH8303. A fix moving the documented declaration carries the comment (SHARED-ORDER-002). A fix moving the code the comment stands behind is not covered. The new RH7103 example matches the observed behavior in `13`.
   - SHARED-DOC-008 is retitled, and its body is unchanged. The RH7103 non-goal now points at SHARED-FIX-006.
   - Format: one requirement per heading, normative sentence first, `Why:` present, and the examples carry analyzer markup.
   - The issue's second comment is applied: SHARED-FIX-003 names SHARED-DIAG-008, SHARED-DIAG-010 and SHARED-JOIN-001, and the `sdd/README.md` P11 closing sentence names P31.
   - No tracker number appears in the added spec lines.
3. IDs: ok. `requirement-ids.txt` gains SHARED-DIAG-009 and SHARED-DIAG-010 at their sorted position. The repair touched no ID, and nothing is renumbered or deleted.
4. Code owners: ok. The repair adds no new spec header owner. `Uses` of `shared/element-documentation.md` gains SHARED-ORDER-002. `scripts/test.sh --project architecture` (includes `SpecConsistencyTests`): "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" at head 3293cb0e5930dfd1ee53cde3657649cbe209ee0a.
5. User documentation: ok. No rule page under `documentation/rules/` changed and none describes a changed behavior. The repair needs no rule page.
6. Open questions: ok. `12-spec-author.md` lists none. The two found earlier are filed by the lead as thoenissen/Reihitsu#1407 and thoenissen/Reihitsu#1408 (see `00-lead.md`).
7. Requirements covered and specs consistent: ok.
   - The architecture suite passes (line quoted under check 4).
   - Every new or amended active requirement has a covering `[Requirement]` test in the diff.
   - The new SHARED-DOC-004 moved-declaration sentence is covered by `RH7103…FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (LF and CRLF, tagged SHARED-DOC-004 and SHARED-ORDER-002).
   - The new RH2004 partial-property example is deliberately unpinned (thoenissen/Reihitsu#1287).
   - Repair tests: the three RH7107/RH7108 summaries are reworded as characterizations of today's behavior, assertions unchanged. The duplicated `Format` helper moved to `AnalyzerTestsBase<TAnalyzer, TCodeFix>`. `VerifySpecificationFile` gained an `onConfigure` overload. The cleaner reports a warning-free Release build.
8. Nothing left behind: ok. `sdd/work/` holds only `specs-principles-p25-p31`, which the lead deletes before publishing. `scripts/lane-check.sh tooling` prints "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

## Notes for the lead (not findings)
- The RH2004 test `VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic` keeps its `SHARED-DIAG-004` tag (decision `2026-10-21-1260-diag004-weak-coverage.md`). It is weak coverage, since RH2004-002 makes it pass. The test implementer and spec author both left the choice to the lead. Dropping it costs no coverage: RH2102 and RH7104 tests still cover SHARED-DIAG-004.
- The three RH7107/RH7108 single-line accessor tests are not named in thoenissen/Reihitsu#1335's instance list. The lead may add them there.
- The auditor's hints stand for the run report: the SHARED-DIAG-009 bullet-1 test cannot fail today (real coverage comes with thoenissen/Reihitsu#1334), and the RH7103 whole-type formatting also moves an unrelated behind-code `///` comment (candidate note for thoenissen/Reihitsu#969).
