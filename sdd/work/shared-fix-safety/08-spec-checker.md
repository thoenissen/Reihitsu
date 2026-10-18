Role: spec checker
Commit: 1d92c25
Verdict: findings

My prompt carried an issue, a slug, and the handovers to read, and no intended fix, suspected cause, or hoped-for verdict.

Diff checked: `git diff origin/main HEAD -- . ':(exclude)sdd/work'` (origin/main = merge base 53990ed). It touches `AGENTS.md`, `sdd/specs/requirement-ids.txt`, `sdd/specs/shared/code-fixes.md`, four files under `squad/decisions/`, and 12 files plus one new class under `src/Reihitsu.Analyzer.Test`. `scripts/lane-check.sh tooling` prints "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 7 | `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`, the preprocessor test that calls `GetCodeFixActionsAsync(..., "DEBUG")` (about line 742) | The diff changes the pre-existing assertion `Assert.HasCount(1, actions)` to `Assert.IsNotEmpty(actions)`. No requirement of this run needs it, it is not tagged with a new ID, and `06-test-implementer.md` does not mention it. A weaker assertion on an existing test is an unexplained edit outside the scope of the covering tests (FC-017). The test passes either way, so the diff does not show why it was relaxed. | test implementer: revert it, or state in a handover why `HasCount(1)` cannot hold |
| 2 | 2, 5 | `AGENTS.md` (new sentence), `sdd/specs/shared/code-fixes.md` SHARED-FIX-003 | The new sentence is the issue's text word for word. It says SHARED-FIX-003 "states what makes a fix unsafe". The sentence in front of it names dropping, reordering, and relocating comments, directives, disabled text, and literals. SHARED-FIX-003 states only changed text, a compile error, a run-time change, and `#if` names. Relocation, reordering, and disabled text are not stated anywhere (FC-004), so a fix that moves a comment is not "a bug against that spec". `05-spec-author.md` already lists this as a follow-up candidate. Not a spec error: no requirement was named for it, and the issue prescribes the sentence. The lead decides whether to ask the maintainer to widen SHARED-FIX-003 or to word the sentence differently. | lead (maintainer decision); spec author if SHARED-FIX-003 is widened |

Nits, no action required:
- SHARED-FIX-003: "The second and third condition are bounded" reads better as "conditions".
- SHARED-FIX-003 states four conditions under one `Why:`. `sdd/README.md` asks for one decision per heading. The issue prescribes one requirement, so I take it as decided.

## Checked
1. Change doc: ok. None is in the diff, as the issue's note and `00-lead.md` decide: the requirements write down the policy of `AGENTS.md` *Code-fix coverage*, and violating code is tracked as bugs. Nothing contradicts this.
2. Specs match the diff: findings 2.
   - SHARED-FIX-001 (per rule, clarified), -003, -004, -005 and the narrowed non-goals match the issue. The non-goals drop the cycle and no-op classes and keep "a fix leaves diagnostics of other rules behind".
   - Format is followed: normative sentence first, `Why:`, examples with `{|RH####:…|}` markup.
   - Every normative qualifier of SHARED-FIX-003 matches the four decisions in `squad/decisions/2026-10-18-shared-fix-003-*.md`: own reported text, new name or accessibility not counted, doc `cref`/`param` names renamed, `#endregion` comment that repeats the name renamed, layout limited to whitespace around comments and directives, field order for sequential structs and `[StructLayout(Sequential)]` types but not Explicit.
   - The `LayoutKind.Auto` exclusion is the spec author's reading of decision B ("structs, sequential by default"). The lead may want the maintainer to confirm it.
   - SHARED-DIAG-003 does not exist on origin/main, so no second wording exists today. The decision says the PR states in its notes that whichever of the two PRs merges second aligns its text.
   - Known contradiction, tracked: RH5022-003/-004 and the matching requirements of RH5024 to RH5027 describe one diagnostic on the first blank line and a fix that deletes only that line. SHARED-FIX-001 now says the whole run is corrected. `01-spec-author.md` records this, and thoenissen/Reihitsu#1076 brings those specs in line.
3. IDs: ok. SHARED-FIX-003, -004, -005 follow -002 at their sorted position. No line was removed or moved.
4. Code owners: ok. The header keeps `Code owners —` and `Uses —`, and the row in `sdd/specs/README.md` already matches. No type was renamed, added, or removed. `SpecConsistencyTests` and `SpecStructureTests` pass.
5. User documentation: findings 2 (AGENTS.md wording only). No file under `documentation/rules/` is affected and none is in the diff.
6. Open questions: ok. The only one, RH7302 renaming every substring occurrence inside the `#endregion` text, is filed as thoenissen/Reihitsu#1126 (open, `SDD open questions: sdd/specs/shared/code-fixes.md`). It matches `05-spec-author.md` and names the readings (a), (b), (c). The other items of the spec author are follow-up candidates, not questions about intended behavior: the stale non-goals in `RH2005.md` and `RH7103.md`, the RH7103 region follow-up, the RH7109 layout follow-up, and the FC-004 gap of finding 2. They are for the lead to list in the PR.
7. Coverage: findings 1.
   - `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. That includes `EveryRequirementOfAnEnforcedSpecIsCovered`.
   - `scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-001|TestCategory=SHARED-FIX-003|TestCategory=SHARED-FIX-004|TestCategory=SHARED-FIX-005"`: `Passed!  - Failed:     0, Passed:   252, Skipped:     0, Total:   252`.
   - Class `RH7103…AnalyzerTests` alone: 34 passed.
   - SHARED-FIX-001 is covered by RH5002, RH5028, RH5023 tests (LF and CRLF) and by `FixAllCorrectsEveryDiagnostic`. SHARED-FIX-003 is covered by RH4106, RH7103, RH7109, RH7302, RH3202, RH5601, RH5604, RH8107 tests. SHARED-FIX-004 is covered by `CodeFixInteractionTests` (RH2005 then RH4106, asserting the text after each step). SHARED-FIX-005 is covered by the RH4106 `_1st` test. I compared the RH3202, RH5028, RH5023, RH7302, RH5601, RH7109 and RH8107 tests with the spec blocks, and the expected texts match.
   - The triggering side of four SHARED-FIX-003 qualifiers has no test, because each is a known bug. They are listed in `05-spec-author.md` and `06-test-implementer.md` for their follow-ups. This is not a finding.
8. Nothing left behind: ok. `sdd/work/` holds only `shared-fix-safety`, which the lead deletes before publishing. No change doc or `sdd/work/coordinator` is in the diff.
