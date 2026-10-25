Role: spec author
Commit: c2fbbf8
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist and the maintainer's recorded decision.

## Scope

Repair round 1, auditor finding 2 only (`05-auditor.md`), narrowed by the maintainer's answer to escalation 12 (`squad/decisions/2026-10-25-default-test-run-restated-in-several-places.md`, option A): correct the three stale statements of the default test run in place; no consolidation into one owner in this run.

## Changes

| File | Before | After |
|---|---|---|
| `CONTRIBUTING.md:22` | `scripts/test.sh      # run all test projects` | `scripts/test.sh      # run all test projects except the self-hosting scans, which CI runs` |
| `squad/README.md:19` (roles table, validator row) | "Builds the solution and runs all tests" | "Builds the solution and runs every test project except the self-hosting scans, which CI runs" |
| `.claude/agents/validator.md:3` (frontmatter `description`) | "builds the solution and runs every test project, and reports." | "builds the solution, runs every test project except the self-hosting scans (which CI runs), and reports." |

The wording follows the already-corrected `AGENTS.md:14` comment and `squad/roles/validator.md:19`. None of the three files is compiled, read by a test, or rule metadata, so no text-only proof applies.

A repository-wide `rg` for "all tests", "every test project", "all test projects", and "runs all" (excluding `sdd/work/` and `squad/decisions/`) finds no other statement of the default run left stale. `sdd/changes/2026-10/introduce-sdd.md:170` still says the validator "runs every test project"; it is a historical change doc that records the state when SDD was introduced, so it is left unchanged.

`scripts/lane-check.sh tooling` after the edits: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Changed requirements

_None._ No spec describes the test layout or the default test run.

## Examples for the tests

_None._

## Open questions for the maintainer

_None._
