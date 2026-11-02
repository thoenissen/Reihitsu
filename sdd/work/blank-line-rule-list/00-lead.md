Role: lead
Commit: b701d73
Verdict: lane claimed — bug fix

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

Asked (batch before the test implementer): output questions 1 (region directive below a conditional directive directly after `{`) and 2 (region directive at file start behind a comment or directive), as in `01-skeptic.md`. Awaiting answers.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `lead.md` prescribes `claude/issue-1499-<slug>`, the session prescribes `claude/clever-thompson-uc99sl` | Use the session's designated branch | The session forbids pushing to any other branch | Branch names are internal | b701d73 |
| 2 | Lane | When does the spec author run in the bug-fix lane? | After the cleaner, for the code-owner list in `blank-lines.md`; earlier only for a clarification a role reports | The code owners exist only after the implementer | Spec wording is internal | — |
| 3 | Skeptic | `while` goal (FMT-BLANK-008, Done when 2) | Drop as already met | thoenissen/Reihitsu#1541 fixed it; `BlankLineBehindDirectiveBeforeTokenTests.KeepsBlankLineBetweenDirectiveAndWhileStatement` already pins the issue's input under `FMT-BLANK-008` | The output already matches the unchanged requirement; nothing changes | — |
| 4 | Skeptic | Restructuring goal: one ordered precedence list for the whole phase (Done when 1) | Drop from this run | A prescribed solution of an `agent-filed` issue; its measurements predate #1541; the accepted change doc `sdd/changes/2026-11/blank-line-sole-owner.md` rejected a precedence ranking in favour of disjoint rule scopes; "the phase on its own output changes nothing" is already enforced by `PhaseStabilityCheck` for every fixture | Dropping it changes no output | — |
| 5 | Skeptic | Characterization input: the issue's `#if DEBUG` is inactive under the CLI and the verifiers | Characterize with an active branch (`#if !DEBUG`), keep the issue's input as the inactive guard | The literal input shows nothing (both sides silent by SHARED-BLANK-007) | Test choice only | — |
| 6 | Skeptic | Architecture finding: SHARED-BLANK-005 stated in Core (`RegionDirectiveBlankLineUtilities`) and restated in the formatter (`BlankLineEditor` exemption, first-of-kind loop), drifted | Change the structure in this run: the formatter phase decides every active region directive through one predicate shared with Core, which RH5031's analyzer and code fix also use | A narrow fix inside `BlankLineEditor` would keep the second statement of the rule; Core is the counterpart of the formatter rule the run changes, and the issue names `RegionDirectiveBlankLineUtilities` | RH5031's analyzer and fix must keep their behavior; the formatter's visible change is limited to the output questions 1–3 | — |
| 7 | Skeptic | Region directive later in a gap (behind a comment, `#pragma`, `#if`) gets no blank line (skeptic Q3) | Not asked: decided by the unchanged SHARED-BLANK-005 / FMT-BLANK-013 (the formatter inserts the blank line) | Bug fix back to an unchanged requirement; no test asserts today's output | An unchanged requirement demands the output | — |
| 8 | Skeptic | Documentation comment directly above `#region` (FMT-BLANK-010 vs FMT-BLANK-013; RH8302 vs RH5031) | Follow-up candidate 1 | Outside the issue's case; needs its own output decision | Not changed in this run | — |

## Follow-up candidates

1. Documentation comment directly above a region directive: RH8302 and RH5031 contradict each other, and the formatter's output is decided only by its rewriter order (FMT-BLANK-010 vs FMT-BLANK-013). Source: `01-skeptic.md`, follow-up candidate 1 (carries the output question in form).
