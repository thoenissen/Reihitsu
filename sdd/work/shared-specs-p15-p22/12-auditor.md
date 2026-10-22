Role: auditor
Commit: fa90251075e1944ac8fe7d271117e213a8fec115
Verdict: pass, text cleanup

## Scope

Repair delta against `8bb9cb4` (the commit of `07-auditor.md`). Head is `fa90251`. `origin/main` is `d70f487` (#1357, SHARED-FIX-006 to -008): `git ls-remote` returns it, and it is an ancestor of the head. The repair commits are `b4ea8d9..adb1258`; `fa90251` merges main into them.

- Repair content (`git diff 8bb9cb4 adb1258`):
  - the SHARED-DIAG-008 sentence about `#region`/`#endregion`/`#pragma`, its `Why:`, and the `Uses` row of `diagnostics.md`;
  - `<summary>` text of eight tests in five files;
  - decision `2026-10-22-1260-diag008-auditor-findings.md`;
  - handovers 08–11 and the worklist in `00-lead.md`.
- That matches the worklist R1–R3 and handovers 08–11. The scope did not grow, so this is a delta audit only.
- `scripts/lane-check.sh tooling` on the merged head gives `confirmed`.

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond what the recorded decision states.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | All seven summaries in RH1001 (×3), RH7407 (×2), RH7410, and RH7105 now use the decided wording. Each names the requirement its pinned behavior contradicts: SHARED-DIAG-005, -006, -004, -008. I re-read each against its test body. `scripts/verify-text-only.sh --base 8bb9cb4 --head adb1258` gives `TEXT-ONLY PROOF: PASS`: 5 C# files, documentation trivia only, tokens, literals, directives, and disabled text identical. |
| 2 | closed | (a) Filed as thoenissen/Reihitsu#1378 (RH5102) and thoenissen/Reihitsu#1379 (RH5109), per the decision. Both bodies name the tagged test and are consistent with the narrowed text: conditional directives stop the report, and `#region`/`#pragma` keep it, as RH5102-004 and RH5109-004 already do. (b) SHARED-DIAG-008 is narrowed. See Guard scope. |
| 3 | closed | The RH7305 summary now states only the input and the observation ("the type is not reported"). The ownership mechanism the code does not implement is gone. |

### Merge of main

- No conflict with this run's specs.
  - Main changed `requirement-ids.txt`, `shared/code-fixes.md`, `formatter/pipeline.md`, and 10 test files, plus a new helper `AssertFormattingChangesNothing` in the code-fix test base.
  - The only path both sides touched is `requirement-ids.txt`. The merged ledger holds SHARED-DIAG-004…008, SHARED-DOC-017, and SHARED-FIX-006…008 in order.
  - `git diff adb1258 fa90251` equals main's 13 files exactly, so the merge resolution added nothing.
- `scripts/test.sh --project architecture` filtered to RequirementCoverageTests, SpecConsistencyTests, and SpecStructureTests, run on the merged head: `Passed: 39, Failed: 0`. This run is after the merge; handover 11 ran before it.
- SHARED-DIAG-004 cites SHARED-FIX-003, which main reworded. The citation still fits: "a compile error that the code did not have" covers a one-part change of a partial declaration.
- Main's new tests add no `#if`, `partial`, or several-declarator shapes (`rg` over the added lines), so none of them pins a shape this run's requirements decide.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | low | `squad/decisions/2026-10-22-1260-diag008-auditor-findings.md`, row `Escalation` | — (repository convention) | PR-introduced (repair) | The entry gives escalation `7`. In `squad/README.md`, 7 means "the auditor still blocks after one repair round". This question was put after the first audit, before any repair round, and it asked whether a spec changes mid-run. That is escalation 4, the number the parallel entry `2026-10-22-1260-diag008-spec-checker-findings.md` uses. The decisions are mined by escalation kind, so the wrong number would miscount this answer. | Set `Escalation` to `4` before the PR is published. |

`pass, text cleanup`: finding 1 is confined to a decisions entry, which is documentation. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-DIAG-008: `#region`/`#endregion`/`#pragma` in the judged span "MUST NOT by itself keep the construct from being reported; a rule's own requirement may still leave such a shape unreported" | judged span | SHARED-JOIN-001; RH5408-003 (tagged test `VerifyRegionAroundDeclarationSemicolonAutoPropertyIsNotFlagged`); RH5307-001; SHARED-TRAIL-002 (stronger: MUST NOT keep the comma unreported); the RH5412 body (no own exclusion: reported) | all agree. The three clauses of 07 finding 2(b) now fall under the rule's-own-requirement clause. TRAIL-002 and RH5412 keep the report, which the narrowed text permits. | holds |
| SHARED-DIAG-008 reading B, now prefixed "For a construct that is reported" | the part of the span holding the directive | RH5409 fix (keeps the directive and its line), RH5412 fix (withheld), SHARED-MODORDER-003 (withheld) | agree. The trailing "the diagnostic is still reported" is redundant and not contradictory (as 11 notes) | holds |
| SHARED-DIAG-008: conditional directive in the judged span | judged span | unchanged since 07 | unchanged; RH5102/RH5109 now tracked (#1378, #1379) | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| `#region`/`#pragma` in a judged span | SHARED-DIAG-008 (general: no reason by itself); SHARED-JOIN-001, RH5408-003, RH5307-001, SHARED-USING-009 (rule-own exclusions); SHARED-TRAIL-002, RH5102-004, RH5109-004, SHARED-MODORDER-003 (report, fix withheld) | one general owner. Every other clause is now an allowed rule-own refinement, so they no longer contradict each other |
| `Uses` of `diagnostics.md` | `SHARED-FIX-003, SHARED-JOIN-001, SHARED-TRAIL-002`. Matches the IDs referenced outside the non-goals (`SpecStructureTests` passes on the merged head) | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH1001 `VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported`, `VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported`, `VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported`; RH7407 `VerifyOnlyFirstDeclaratorOfOverrideEventNamesOverrideRegion`, `VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOnceAtFirstDeclarator`; RH7410 `VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart`; RH7105 `MisorderedModifiersWithDirectiveAreReportedWithoutFix` | unchanged characterization assertions; only the summary changed | — | unchanged | each summary describes the test's assertion and names the contradicted requirement (text-only proof PASS) |
| RH7305 `NestedEnumInARegionIsOneMemberWhateverRegionsItHasAmongItsValues` | the spec example is not reported | — | `Verify` | summary now matches what the test observes; it still cannot fail, as declared in 03, 04, and 09 |

## Failure classes dismissed

- FC-001, FC-002: the narrowed DIAG-008 sentence adds a rule-own exemption that the maintainer decided. Its counterparts agree (Guard scope). No code predicate changed.
- FC-003, FC-005, FC-020: conditional directives versus `#region`/`#pragma` keep their separate sentences. The tagged tests on both sides are unchanged (RH5409, RH5412, RH7106, RH7003).
- FC-015, FC-016: see Policy ownership. The sibling summaries of 07 finding 1 are closed. The remaining "open question" summaries were swept in 09 and 11, and I agree with their reasons.
- FC-017, FC-031: summaries were re-read against the bodies (Assertion adequacy).
- FC-032: no compiled file of this run changed in the repair. The merge brings main's compiled test changes, which the validator covers. `10-cleaner.md` built the pre-merge tree.
- FC-037: the text-only claim rests on `scripts/verify-text-only.sh`, not on `grep`.
- FC-004, FC-006 to FC-014, FC-018 to FC-030, FC-033 to FC-036: the repair changes no rewrite, fix, formatter, or test-input code.

## Hints

- thoenissen/Reihitsu#1374 (and the `#region`/`#pragma` part of thoenissen/Reihitsu#1376) argue from the earlier DIAG-008 text ("a `#region`, `#endregion` or `#pragma` does not stop it"). Under the narrowed text, SHARED-USING-009 and SHARED-ATTR-011/-019/-020 are rule-own requirements that may leave such a shape unreported. Those issues no longer follow from the spec's wording, only from P22 itself. A note on them would keep the tracker consistent; it needs no tree change.
- Main's SHARED-FIX-003 now says "a rule reports the construct it names for every input, also for an input where the fix is withheld". In context it means that withholding a fix never suppresses a report. Read on its own, "for every input" overreaches against SHARED-DIAG-008's not-reported cases and SHARED-JOIN-001. `shared/code-fixes.md` is outside this run's scope.
- The hints of `07-auditor.md` that the repair did not touch stand as written.
