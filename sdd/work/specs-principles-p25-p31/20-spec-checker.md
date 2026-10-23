Role: spec checker
Commit: f3a47b452abc10c9d41c4a66cffe9656d378bccf
Verdict: findings

Third check, after the third repair round (`16-auditor.md` findings 8 and 9, repaired in `17`, `18`, `19`). Inputs: `00-lead.md`, `16` to `19`, `15-spec-checker.md`, thoenissen/Reihitsu#1331 (body and both comments), the lead's question comment on thoenissen/Reihitsu#1407, `git diff origin/main...HEAD`, and `git diff 323a540 HEAD` for the repair. My prompt carried no intended fix or hoped-for verdict. I changed no file other than this handover.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 (Uses lines) | `sdd/specs/analyzer/RH7105.md` and `sdd/specs/analyzer/RH7106.md`, header `Uses` | SHARED-DIAG-004 paragraph 2 names RH7105 and RH7106 as the rules that judge each part on its own (P28, "Covers RH7104, RH7105, RH7106"), and the repair adds a test for RH7106 tagged `SHARED-DIAG-004`. Neither rule spec lists SHARED-DIAG-004 in `Uses`; only RH7104 does. `00-lead.md` asks for the `Uses` lines of the affected rule specs to be updated. Neither spec states per-part judgement on a partial declaration anywhere (`rg -i partial` finds only the modifier order in RH7105). Not caused by the third round: the gap exists since the first round (`01-spec-author.md` line 42 lists the three rules) and was not raised before. `SpecConsistencyTests` does not enforce it, so the suite is green. Low severity. | spec author |

## Checked
1. Change doc: ok. The repair rescopes two requirements of a clarification run; it adds, changes, or retires no intended behavior. SHARED-DOC-004's move clause now narrows toward what the code does, and the not-covered shapes are filed as questions, not decided. No change doc is required.
2. Specs match the diff: finding 1; otherwise ok.
   - SHARED-DOC-004 paragraph 2: the MUST is limited to a fix that moves the documented declaration and not the code the comment stands behind, in documentation mode `Parse` or `Diagnose`, with no preprocessor directive between comment and declaration. Mode `None` and the directive shape are stated as not covered, as are moves of the code in front. This follows `16-auditor.md` finding 8. The Why explains both limits. The RH7103 example caption names `Parse` or `Diagnose`, and a new `#if DEBUG` / `#endif` block is captioned as not covered. The covered claim for `Diagnose` is not exercised by a test, but the tree's trivia in `Diagnose` equals `Parse`; I note it and do not count it a finding.
   - SHARED-DIAG-004 paragraph 3 and the Why: now say the parts of a partial member state the same accessibility, restricted to which accessibility a part states or whether it states one (first paragraph). The keyword order stays with paragraph 2. The new RH7106 partial-property example sits on the paragraph 2 side. Its markup is `{|RH7106:protected|}` and its fixed output matches the test, which passes. The RH2004 example is recaptioned and keeps its verdict. This follows `16-auditor.md` finding 9.
   - Format: one requirement per heading, normative sentence first, `Why:` present, the new examples are code blocks with analyzer markup. The new text carries no tracker number.
   - The issue's second comment (SHARED-FIX-003, `sdd/README.md` P11 closing sentence) was applied in earlier rounds and is untouched here.
3. IDs: ok. `requirement-ids.txt` differs from `origin/main` by SHARED-DIAG-009 and SHARED-DIAG-010 at their sorted position, as planned in `00-lead.md`. The repair touched no ID; nothing is reused, renumbered, or deleted.
4. Code owners: ok. The repair adds no spec header owner. `scripts/test.sh --project architecture` (includes `SpecConsistencyTests`) at head f3a47b4: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`.
5. User documentation: ok. The diff touches no rule page under `documentation/rules/`, and none describes the rescoped behavior.
6. Open questions: ok. The two not-covered SHARED-DOC-004 shapes are on thoenissen/Reihitsu#1407 as Q2 (documentation mode `None`) and Q3 (directive between comment and declaration). The comment's inputs, observed outputs, counter-observation, and competing readings match `17-spec-author.md`. Q1 of the earlier round and thoenissen/Reihitsu#1408 are unchanged.
7. Requirements covered and specs consistent: ok.
   - The architecture suite passes (line quoted under check 4).
   - SHARED-DIAG-004 stays covered by RH2102, RH7104, and the new `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.PartOfAPartialPropertyIsReportedAndFixedOnItsOwn` (tags `RH7106-001`, `RH7106-004`, `SHARED-DIAG-004`; both RH7106 IDs exist in the spec). It pins the paragraph 2 side only; the paragraph 3 side is covered by the RH2004 partial-property tests.
   - SHARED-DOC-004's covered side is pinned by the RH7103 test (LF and CRLF); its summary now says mode `Parse` and no directive in between. The not-covered shapes have no test, which is correct since they state no requirement.
   - Cleaner: 0 warnings, RH7103 and RH7106 classes 56 of 56 passed (`19-cleaner.md`). I did not re-run them.
8. Nothing left behind: ok. `sdd/work/` holds only `specs-principles-p25-p31`, which the lead deletes before publishing. `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
