# Lead — formatter-fix-agreement

- Issues: thoenissen/Reihitsu#1529 (`agent-filed`), thoenissen/Reihitsu#1543 (`agent-filed`). Both goals are proposals.
- Slug: `formatter-fix-agreement`
- Branch: `claude/squad-1529-1543-wokrft` (the session's designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1697 (draft)
- Lane: behavior change
  - #1529: an open output question on which layout the RH5111/RH6023 fixes and whole-document formatting share for a wrapped member of an assigned conditional access; whichever reading is chosen changes the formatter's or the fixes' visible output, so a spec change and likely a change doc.
  - #1543: an open question whether the blank-line phase removes blank lines between an own-line comment and further braces (and chained keywords, per the issue's comment), or whether RH5024/RH5025/RH5026 exempt them; either reading changes formatter output or diagnostics and amends FMT-BLANK-020.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

- Q1 (#1529, RH5111 wrapped member behind a conditional access): **B** — the RH5111 fix adopts whole-document formatting's layout (`.C` under the `?` column as the formatter places it; element binding joined `a?[0].C = 5;`; nested case column 9). Formatter, RH5201, RH5112 unchanged. Options D (withhold) and E (declare unsupported — escalation 10) not chosen: the shape is **not** declared unsupported. This supersedes the earlier answer recorded in `sdd/changes/2026-10/node-root-detached.md`.
- Q2 (#1543, blank line between own-line comment and brace/chained keyword): **A** — the formatter removes the blank line for every brace and for `else`/`catch`/`finally`/do-`while`; RH5024–RH5027 unchanged.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Spec | #1529 issue goals: RH6023 test and "record C in the formatter spec" | Drop both | RH6023 fix only touches operator gaps (SHARED-OPSPACE-004), already pinned; C not chosen | No output changes from dropping them | — |
| 3 | Spec | #1543 issue goals: issue's own input (now refused by FMT-UNSUP-004) and complex element initializer | Replace input with assignment/initializer-interior forms; keep existing element-initializer coverage; extend round trips to RH5026/RH5027 | Skeptic findings | Test selection is internal | — |
| 4 | Spec | Edit merged `node-root-detached.md` for the superseded answer? | No; supersession recorded in the new change doc only (spec author's objection taken) | `sdd/README.md`: merged change docs are history | Doc history only | b8924f4 |
| 5 | Spec | Accept both change docs | Status set to Accepted | Every visible output change has the maintainer's answer | — | (this commit) |
| 6 | Audit 1 | Finding #1: the conditional-access-branch fact (`FluentChainBoundary.RootIsConditionalAccessBranch`, its branch in `FluentChain.IsConditionalAccessBranch`, FMT-PIPE-024 row) is dead after the change | Delete it in this run (repair round 1): spec row → spec author, code → implementer | PR-introduced, small, deletes code; a follow-up would cost a run of its own | No caller reaches the fact any more, so no output changes | (repair) |
| 7 | Repair 1 | Disputed test `ReihitsuFormatterDetachedRootTests.ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` asserts output for a root no caller passes any more | Delete the test (test implementer) | AGENTS.md *Supported inputs*: no caller reaches it; changed FMT-PIPE-024 leaves the root undecided; its other cases are covered by FMT-PIPE-023 and RH5111-006 tests | Only a direct-call input; no caller produces it | (repair) |
| 8 | Audit 2 | Comment fix for finding #2 fails `verify-text-only.sh --strict-docs` (public property of an internal record counts as public API doc); plain proof passes with identical tokens | Treat as repair round 2 as `lead.md` section 7 says: spec checker + auditor on the comment-only delta | Follow the playbook rather than override the proof | Comment-only; token stream identical | 7129a6a |
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1529-1543-wokrft` instead of `claude/issues-1529-1543-<slug>` | The session environment permits pushes only to the designated branch | Branch names are internal | claim commit |

## Follow-up candidates

- Comment in front of the closing `}` of a multi-line interpolation hole stays in column 12 while the hole's other lines move to column 8 (outside #1543; carries an open question, readings A move with the hole's other lines / B keep; refined by `02-skeptic.md`: in a regular interpolated string the formatter moves the comment to column 12 even from column 8) — `01-spec-author.md`, `02-skeptic.md`.

## Worklist / next step

- Done: claim, draft PR #1697, `scripts/prepare.sh` (SDK 10.0.404 installed).
- Done: spec author (`01-spec-author.md`, a49e965): two draft change docs, two output questions (#1529 rec. B, #1543 rec. A).
- Done: skeptic (`02-skeptic.md`, 7cfa03f): #1529 rec. B, adds the column-9 input and option D (withhold fix); names escalation 10 (declare the null-conditional assignment with a wrapped target unsupported), not recommended. #1543 rec. A; change-doc corrections (oscillation claim, field array initializer `}`, nested array `{`, RH5027 for do-while).
- Answers received (1B, 2A).
- Done: spec author amendment (`03-spec-author.md`, b8924f4): RH5111-006, FMT-BLANK-020 widened, change docs finalized and accepted.
- Done: test implementer (`04-test-implementer.md`, 4e241e5): failing tests exactly on changed outputs; rewritten existing asserts (3 old RH5111 conditional-access tests, 2 FMT-BLANK kept-blank-line tests) are covered by answers 1B/2A.
- Done: implementer (`05-implementer.md`, 0c30c38): +17/−64 production; withheld rare class: RH5111 fix withheld when the receiver before `?` holds an unsupported shape (FMT-UNSUP-004) — for PR Review notes.
- Done: cleaner (`06-cleaner.md`, c821499): clean, Release 0 warnings.
- Done: spec checker (`07-spec-checker.md`, be1cc6d): consistent, no findings. origin/main unchanged (b34c0f1); build green.
- Done: audit 1 (`08-auditor.md`, e60b17f) on 01672a1, base b34c0f1: one low finding (dead fact, #1529); #1543 clean.
- Repair round 1 worklist: (a) spec author removes FMT-PIPE-024 "Conditional-access branch" row (and any other spec text naming the fact); (b) implementer deletes the flag and boundary branch; keep `RootContinuesChain`.
- Done: spec author repair (`09`, 2be1c17); implementer repair (`10`, 3940f29) — one disputed test, decision 7.
- Done: test implementer repair (`11`, ce39258): test deleted, coverage confirmed.
- Done: cleaner (`12`, eaab5a9): clean.
- Done: spec checker (`13`, 5bf6f9a): consistent.
- Done: audit 2 (`14`, a2d888a) on 79610f4 (audited SHA): pass; one comment-only finding #2 (`FormattingContext.RootChainBoundary` summary, :152-158).
- Done: cleaner (`15`, 7129a6a): comment fixed; strict proof FAIL, plain proof PASS → repair round 2 (decision 8).
- Done: spec checker (`16`, 51071ef), audit 3 (`17`, c9b1c5e): pass. **Audited SHA: 51071efcb7884ba8fe0528b266d654eaece242f6.** Repair rounds used: 2.
- Next: validator → publish.
