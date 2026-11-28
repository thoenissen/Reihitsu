# Lead — apply-fix-compile-check

- Issue: thoenissen/Reihitsu#1977 — `agent-filed` (goals are proposals). Labels: cli, maintenance.
- Slug: `apply-fix-compile-check`
- Branch: `claude/determined-mccarthy-dpombd`; draft PR thoenissen/Reihitsu#1993
- Lane: **tooling** — the change is to `scripts/apply-fix.sh` and the code-fix runner in `src/Reihitsu.Tooling` (`FixtureRunner`), with tests in `tests/Reihitsu.Tooling.Test`; no analyzer, formatter, or CLI output changes.
- Roles run: test implementer (the tool has tests), implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic (the tooling lane runs neither; no spec under `sdd/specs/` covers the fixture runner).
- Clarifications (issue comment by the maintainer's account): the gap in `FixtureRunner.HasSyntaxErrors` holds — it reads only parse diagnostics, while CS8967 comes from binding — but the issue's example no longer reproduces since thoenissen/Reihitsu#1976 (RH5301 no longer reports inside a non-verbatim hole under language version 10). The work is a safety net; the test needs a different trigger, such as a stub fix or a hand-made fix result in `Reihitsu.Tooling.Test`.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Test implementer | How to tell a new compile error from one the fixture already had | Compare errors as a set of (ID, message): a fix result is invalid only when it carries an error whose ID and message the original did not have; repeats of an existing error do not count | A count- or ID-only comparison flags realistic fixtures, e.g. a field split of an unknown type repeats CS0246, or misses a new CS0246 for another name | Tooling only; no analyzer, formatter, or CLI output changes | de445df |
| 2 | Auditor (05, repair round 1) | Rename fixes change the message of an error the fixture already had, so the (ID, message) set flags them `invalid-result` (finding #1) | Revise decision 1: a result is invalid when, for some error ID, its number of distinct messages exceeds the fixture's (an ID the fixture lacks counts from zero). Add a rename test; correct the stale `FixtureRunnerTests` summary (finding #2) | Keeps every existing new-error and repeat case while a rename that keeps an error's count no longer counts as new | Tooling only | adb59e6, 62bc4d2 |

## Follow-up candidates

None yet.

## State

- Audited SHA: 7847c6ca90171381a6ac9aab9f31406dbbf2fcd4 (base 707d4b8); verdict findings (05-auditor.md).
- Repair round 1 worklist: #1 (PR-introduced) → test implementer adds rename test, then implementer; #2 (comment in test file) → test implementer.
- Repair done: 06-test-implementer (adb59e6), 07-implementer (62bc4d2). Next: cleaner, spec checker, auditor on the repair, validator.
