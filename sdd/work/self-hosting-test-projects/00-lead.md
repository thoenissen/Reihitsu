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

## Escalations

- Escalation 12 (auditor architecture finding, default-run coverage restated in eight places): maintainer chose A — fix the three stale lines narrowly, consolidation as a follow-up. `squad/decisions/2026-10-25-default-test-run-restated-in-several-places.md`.
- Auditor finding 1 (analyzer scan wall-clock not lower): maintainer accepted it — the goal is the default local run, which no longer runs the scans; no repair. `squad/decisions/2026-10-25-self-hosting-analyzer-scan-wall-clock.md`.

## Repair round 1

- Auditor finding 2 (three stale text lines, PR-introduced) → spec author (docs owner in repairs, although the tooling lane skipped it initially).
- Auditor finding 3 (FC-017, assertion that cannot fail, PR-introduced) → test implementer.
- Escalation 10 (lane check: root `CONTRIBUTING.md` unclassified, "no lane"): maintainer chose B — keep the edit; the lane classifier treats `CONTRIBUTING.md` as text in this run, covered by a `Reihitsu.Tooling.Test` test. `squad/decisions/2026-10-25-lane-check-contributing-md-unclassified.md`. Lane stays tooling. Added to repair round 1: test implementer (test), then implementer (classifier).
- Escalation 12 (audit of repair round 1): 1A — fix the stale `squad/roles/auditor.md` line now (spec author); 2A — lane-rule duplication and `CODE_OF_CONDUCT.md`/`SECURITY.md` become a follow-up; both follow-up issues approved. `squad/decisions/2026-10-25-repeated-default-run-statement-and-lane-rule.md`.

## After the repair audit (passed with text findings)

- Audit 12 finding 4 → spec author; finding 5 (test summary comment) → cleaner. Proof: `scripts/verify-text-only.sh --strict-docs --base 5459c5d --head worktree`.
