Role: spec author
Commit: e791b77
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist and the maintainer's recorded decision.

## Scope

Text cleanup after the passing repair audit, auditor finding 4 only (`12-auditor.md`), as the maintainer narrowed it under escalation 12 (`squad/decisions/2026-10-25-repeated-default-run-statement-and-lane-rule.md`, answer 1A): fix the one stale line in place. Consolidating the default-run statements into one owner is the approved follow-up thoenissen/Reihitsu#1470 and is not done here. Finding 5 (the test summary comment) belongs to the cleaner and is not touched.

## Changes

| File | Before | After |
|---|---|---|
| `squad/roles/auditor.md:36` (*How to verify*) | "Never run the full suite — the validator does." | "Never run the full suite — the validator runs the default set, and CI runs the self-hosting scans." |

The prohibition on the auditor stays as it was. Only the reason is corrected: the validator runs the default set (`scripts/test.sh` `all)`), which leaves out `Reihitsu.SelfHosting.Test`, and CI runs that project. The wording matches `squad/roles/validator.md:3,19` ("the default set of test projects") and the auditor's suggested change. The file is Markdown that nothing compiles and no test reads, so no text-only proof applies.

Re-enumeration of the class, done this time with a wider search than the one in `06`: `rg -i` for "full suite", "whole suite", "entire suite", "full test run", "the validator does", "validator runs|does|covers", "all tests", "every test project", "all test projects", and "runs all" (leaving out `sdd/work/` and `squad/decisions/`). It finds no other stale statement. The remaining hits match the auditor's *holds* list: `CONTRIBUTING.md:22`, `AGENTS.md:14`, `squad/README.md:19` and `:110`, `squad/roles/validator.md:19`, `scripts/test.sh:12`, and the historical `sdd/changes/2026-10/introduce-sdd.md:170`, which is left unchanged as in `06`. The search also turned up `squad/lead.md:69` ("The validator runs once on the final tree"), which says when the validator runs and makes no claim about what it covers, and `sdd/README.md:168`, which is about requirement tags and not the test run.

`scripts/lane-check.sh tooling --base 0b162f1 --head worktree --no-install` after the edit: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Changed requirements

_None._ No spec covers the test layout, the default test run, or the squad role files.

## Examples for the tests

_None._

## Open questions for the maintainer

_None._
