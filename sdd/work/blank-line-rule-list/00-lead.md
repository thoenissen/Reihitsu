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

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `lead.md` prescribes `claude/issue-1499-<slug>`, the session prescribes `claude/clever-thompson-uc99sl` | Use the session's designated branch | The session forbids pushing to any other branch | Branch names are internal | b701d73 |
| 2 | Lane | When does the spec author run in the bug-fix lane? | After the cleaner, for the code-owner list in `blank-lines.md`; earlier only for a clarification a role reports | The code owners exist only after the implementer | Spec wording is internal | — |

## Follow-up candidates

None yet.
