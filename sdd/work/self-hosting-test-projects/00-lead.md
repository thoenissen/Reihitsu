Role: lead
Commit: (claim) on branch claude/wizardly-lovelace-c79su6
Verdict: lane claimed

## Run

- Issue: thoenissen/Reihitsu#1467 — move the self-hosting scans into their own test projects that CI runs and the local default run skips; one test row per analyzer.
- Slug: `self-hosting-test-projects`
- Branch: `claude/wizardly-lovelace-c79su6` (the session harness mandates this branch name instead of `claude/issue-1467-<slug>`).
- Draft PR (claim): thoenissen/Reihitsu#1468

## Lane

**Tooling.** The issue changes test infrastructure: test-project layout, `Reihitsu.sln`, `scripts/test.sh` and `scripts/README.md`, `.github/workflows/`, and `squad/roles/`. No shipped source under `src/` (analyzer, code fixes, formatter, core, CLI) changes behavior, and no spec describes the test layout. The issue itself names the lane `tooling`.

## Roles

| Role | Runs | Reason |
|---|---|---|
| Spec author | skipped | Tooling lane; no shipped behavior or spec changes |
| Skeptic | skipped | Tooling lane |
| Test implementer | runs | The tool has tests: `src/Reihitsu.ArchitectureTests/General/TestScriptProjectSelectionTests.cs` guards the project selection of `scripts/test.sh`. Owns test code: the moved and split self-hosting scans and the new test projects, plus tests guarding the new selection |
| Implementer | runs | Makes those tests pass: scripts, CI workflows, solution wiring, squad role docs |
| Cleaner | runs | |
| Spec checker | runs | |
| Auditor | runs | |
| Validator | runs | |

## Ownership note

Test code (the scans being moved, their new projects) is test-implementer territory; the implementer must not change what those tests assert. The implementer owns `scripts/`, `.github/workflows/`, `Reihitsu.sln` wiring that the scripts and CI depend on, and the `squad/roles/` text the issue names.

## Maintainer clarifications

None so far.
