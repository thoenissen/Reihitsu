Role: lead
Commit: 2ef59f5
Verdict: CI repair round 1 — bug fix lane

# Run: blank-line-rule-list

## Issues

| Issue | `agent-filed` | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1499 | yes — goals are proposals | bug fix | The issue reports an observed-versus-expected difference against an unchanged spec requirement (FMT-BLANK-008: only the do-statement `while` loses the blank line above it) and asks for a restructuring of the blank-line phase whose outputs must stay unchanged otherwise (no change doc named). |

- Slug: `blank-line-rule-list`
- Branch: `claude/clever-thompson-uc99sl` (the session's designated branch; see Decisions taken, row 1)
- Draft PR: thoenissen/Reihitsu#1573
- Parent: thoenissen/Reihitsu#1482 (package D2). Its blocker thoenissen/Reihitsu#1498 (D1) is closed, merged with thoenissen/Reihitsu#1570; the issue asks the run to check every statement about today's code against current `main` first and drop the ones an earlier package already made true or moot.
- The issue's own instruction about RH5031: a characterization test formats the `#if DEBUG` / `#region` input and runs RH5031 on the output. If RH5031 does not report it, the phase uses the Core rule; if it does, the conflict (SHARED-DIAG-010) goes to the maintainer before either side changes.

## Roles

| Role | Runs | Why |
|---|---|---|
| Spec author | deferred | Bug-fix lane: only for a clarification. The issue names `sdd/specs/formatter/blank-lines.md`'s code-owner list, which can only be written once the new owners exist; the lead spawns the spec author for it after the cleaner, or earlier if the skeptic finds a clarification. |
| Skeptic | yes | Premise check, statements against current `main`, reachability |
| Test implementer | yes | Reproduction (FMT-BLANK-008 `while`), RH5031 characterization, no-op/idempotency tests for the phase |
| Implementer | yes | |
| Cleaner | yes | |
| Spec checker | yes | |
| Auditor | yes | |
| Validator | yes | |

## Maintainer answers

Asked (batch before the test implementer): output questions 1 (region directive below a conditional directive directly after `{`) and 2 (region directive at file start behind a comment or directive), as in `01-skeptic.md`.

- Q1 — **A**: the formatter inserts the blank line between a conditional directive and a region directive directly below it, also right after `{`. The expected value of `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` changes accordingly. RH5031 unchanged.
- Q2 — **A**: at the start of a file, a region directive behind a comment or directive gets the blank line above it; only a directive on the file's first line is exempt. RH5031 unchanged.

The maintainer accepted both recommendations and told the run to continue.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `lead.md` prescribes `claude/issue-1499-<slug>`, the session prescribes `claude/clever-thompson-uc99sl` | Use the session's designated branch | The session forbids pushing to any other branch | Branch names are internal | b701d73 |
| 2 | Lane | When does the spec author run in the bug-fix lane? | After the cleaner, for the code-owner list in `blank-lines.md`; earlier only for a clarification a role reports | The code owners exist only after the implementer | Spec wording is internal | — |
| 3 | Skeptic | `while` goal (FMT-BLANK-008, Done when 2) | Drop as already met | thoenissen/Reihitsu#1541 fixed it; `BlankLineBehindDirectiveBeforeTokenTests.KeepsBlankLineBetweenDirectiveAndWhileStatement` already pins the issue's input under `FMT-BLANK-008` | The output already matches the unchanged requirement; nothing changes | — |
| 4 | Skeptic | Restructuring goal: one ordered precedence list for the whole phase (Done when 1) | Drop from this run | A prescribed solution of an `agent-filed` issue; its measurements predate #1541; the accepted change doc `sdd/changes/2026-11/blank-line-sole-owner.md` rejected a precedence ranking in favour of disjoint rule scopes; "the phase on its own output changes nothing" is already enforced by `PhaseStabilityCheck` for every fixture | Dropping it changes no output | — |
| 5 | Skeptic | Characterization input: the issue's `#if DEBUG` is inactive under the CLI and the verifiers | Characterize with an active branch (`#if !DEBUG`), keep the issue's input as the inactive guard | The literal input shows nothing (both sides silent by SHARED-BLANK-007) | Test choice only | a60c6af |
| 6 | Skeptic | Architecture finding: SHARED-BLANK-005 stated in Core (`RegionDirectiveBlankLineUtilities`) and restated in the formatter (`BlankLineEditor` exemption, first-of-kind loop), drifted | Change the structure in this run: the formatter phase decides every active region directive through one predicate shared with Core, which RH5031's analyzer and code fix also use | A narrow fix inside `BlankLineEditor` would keep the second statement of the rule; Core is the counterpart of the formatter rule the run changes, and the issue names `RegionDirectiveBlankLineUtilities` | RH5031's analyzer and fix must keep their behavior; the formatter's visible change is limited to the output questions 1–3 | 072b113 |
| 7 | Skeptic | Region directive later in a gap (behind a comment, `#pragma`, `#if`) gets no blank line (skeptic Q3) | Not asked: decided by the unchanged SHARED-BLANK-005 / FMT-BLANK-013 (the formatter inserts the blank line) | Bug fix back to an unchanged requirement; no test asserts today's output | An unchanged requirement demands the output | a60c6af, 072b113 |
| 8 | Skeptic | Documentation comment directly above `#region` (FMT-BLANK-010 vs FMT-BLANK-013; RH8302 vs RH5031) | Follow-up candidate 1 | Outside the issue's case; needs its own output decision | Not changed in this run | — |
| 9 | Implementer | New public Core member `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(bool, bool, bool)` | Accept | It is the single statement decision 6 asked for; RH5031's analyzer and fix call the unchanged `SourceText` overload | RH5031 guard tests pass unchanged | 072b113 |
| 10 | Implementer | Trailing-trivia `#endregion` path changed behavior; no caller reaches it | Keep it routed through the shared predicate; neither delete it nor test it | *Supported inputs*: an unreached case needs no test; deleting needs a proof outside the run's scope | No caller reaches the path | 072b113 |
| 11 | Implementer | SHARED-BLANK-006 (blank line after a region directive) is also stated twice (Core and `BlankLineRegionDirectiveRewriter`), with no drift found | Follow-up candidate 2, not a structural change in this run | The defect is not the run's issue's and the run does not depend on it | Not changed in this run | — |
| 12 | Implementer | Spec author for the code-owner list | Skip the spec author | No owner type was added or removed; the code-owner lists of `blank-lines.md` and `blank-line-spacing.md` stay correct | No spec text changes | — |
| 13 | Auditor (pass, hint 1) | The remark on the unreached trailing-trivia `#endregion` path promises more than the code does | Correct the comment through the cleaner, proven text-only against the audited commit 7b22e79; keep decision 10 (no deletion) | A comment finding after a passing audit goes to the cleaner; deleting the path would be a compiled change for an unreached case | Comment only | 96e4b45 |
| 14 | Auditor (hint 2) | Rewritten trailing method repeats `FindLineStartIndex`'s whitespace loop (duplication already on `main`) | Leave it | Pre-existing, unreached path, not worth a compiled change after the audit | Not changed | — |
| 15 | Auditor (hint 3) | Region directives inside initializers and switch expressions take the statement's column; `#endregion` sits four columns left of its `#region` in a switch section (same on `main`) | Follow-up candidate 3 | Pre-existing, outside the issue's case, reachable through the CLI | Not changed in this run | — |
| 16 | Auditor (hint 4) | `RegionDirectiveBlankLineUtilities.IsAdjacentToNonFormattableLine` has no caller outside tests (pre-existing) | Drop | Dead code with no user-visible effect; not a defect | Not changed | — |
| 17 | Publish | `Closes` or `Refs` for #1499 | `Refs #1499` | The issue's stated purpose — one ordered rule list for every gap — was dropped (decision 4); the region rule and characterization goals are met and the `while` goal was already met, so the maintainer decides whether the issue closes | Link choice only | — |
| 18 | CI (sonarcloud) | SonarCloud quality gate fails on new-code coverage (62.7 % < 80 %); 21 of the 23 new uncovered lines are `BlankLineTriviaBoundaryRewriter.EnsureBlankLineBeforeEndRegionsInTrailingTrivia` and its dispatch, 1 is `BlankLineEditor.cs:98` | Revisit decision 10: remove the trailing-trivia `#endregion` path instead of keeping it | The auditor's instrumented run (formatter, analyzer, CLI suites and the repository's 1843 files) is the proof of unreachability decision 10 said deletion needs; *Supported inputs* forbids writing a test for an unreached case, so removing the code is the only way to cover the change | No caller reaches the path; the formatter's output for every reachable input is unchanged | 0d4a891 |
| 19 | CI repair 1 (implementer) | `BlankLineEditor.cs:98` (skipping spaces on a whitespace-only line above a region directive) is reachable through the CLI but no test reaches it | Route a test to the test implementer | A reachable line of the run's change without a test; the output equals `main`'s, so the test pins existing behavior | Output unchanged from `main` | ac34943 |
| 20 | CI repair 1 | `lane-check.sh bug-fix` now says "no lane": the run note `squad/notes/2026-11-03-blank-line-rule-list.md`, committed at publish by `lead.md` section 8 step 4, classifies as tooling | Keep the bug-fix lane; judge the repair's lane on every path except the run note (all others classify as Test, Production, or Text) | The playbook runs the lane check before writing the note; the note is a process artifact every run adds | Lane classification is internal | — |

## Follow-up candidates

1. Documentation comment directly above a region directive: RH8302 and RH5031 contradict each other, and the formatter's output is decided only by its rewriter order (FMT-BLANK-010 vs FMT-BLANK-013). Source: `01-skeptic.md`, follow-up candidate 1 (carries the output question in form).
2. SHARED-BLANK-006 is stated twice: Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineAfter` and the formatter's `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives`; no drift found yet. Source: `03-implementer.md`.
3. Region directives inside collection initializers and switch expressions end up at the statement's column rather than the elements' column, and in a switch section `#endregion` sits four columns left of its `#region`; identical on `main`. Source: `06-auditor.md`, *Hints*, 3.

## CI repair round 1

- PR thoenissen/Reihitsu#1573 is published; head 2ef59f5. Every check is green except `sonarcloud`: quality gate `new_coverage` 62.7 % against a threshold of 80 % (all other conditions OK, no new issues, no hotspots).
- SonarCloud's uncovered new lines: `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` lines 301–345 and 377 (21 lines), `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` line 98 (1 line).
- The audited commit of the original run was 7b22e79; its handovers were deleted before publishing and are in the branch history (`git show 2231fe0:sdd/work/blank-line-rule-list/06-auditor.md` for the auditor's instrumented reachability proof, `…/03-implementer.md`, `…/02-test-implementer.md`).
- Roles of this round: implementer → test implementer (decision 19) → cleaner → spec checker → auditor (repair only) → validator.
