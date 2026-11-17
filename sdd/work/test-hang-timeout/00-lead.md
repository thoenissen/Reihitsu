# Lead — test-hang-timeout

- Issue: thoenissen/Reihitsu#1850 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed. Milestone 1.4.0.
- Slug: `test-hang-timeout`. Branch: `claude/squad-1850-kebylz` (session-designated). Draft PR: #1854.
- Lane: **tooling** — the issue asks for a change to `scripts/test.sh`, `scripts/README.md`, and `.github/workflows/*.yml`, all tooling paths in the lane-check table.
- Roles run: implementer → cleaner → spec checker → auditor → validator.
- Roles skipped: spec author and skeptic (not in the tooling lane); test implementer — the tooling lane runs it only when the tool has tests, and `scripts/test.sh` and the workflows have no test project (`Reihitsu.Tooling.Test` covers the fixture runner only). The issue's "shown by temporarily adding such a test" is a demonstration the implementer performs and reports in its handover; the temporary test is not committed.
- Maintainer clarification: triage comment (2026-11-17) plans the issue for 1.4.0 as its own tooling run.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Test implementer in the tooling lane | Skipped; the implementer demonstrates the hang guard with a temporary, uncommitted blocking test and records the commands and results | No test project covers `scripts/test.sh` or the workflows | Tooling only | — |
| 2 | Claim | `sdd/work/precheck-comma-linear-scan/` left on `main` by an earlier PR | Superseded by row 5 | — | — | — |
| 3 | Implementer | Follow-up: `release.yml`, `deploy-pages.yml`, `pr-labels.yml` have no `timeout-minutes` | Dropped | None of them runs tests, so the issue's failure (a hanging test) cannot occur there | Tooling only | — |
| 4 | Implementer | Follow-up: a caller passing its own `--blame-hang-timeout` to `test.sh` doubles the option | Dropped | No caller does this (`AGENTS.md`, *Supported inputs*) | Tooling only | — |
| 6 | Auditor | Text finding in `scripts/README.md:86` (hang-timeout wording for parallel projects) | Routed to the spec author, proven with verify-text-only; no second audit | `lead.md` section 7 | Text only | — |
| 5 | Implementer | `WorkingDocumentsDirectoryContainsNoFiles` fails on `main` because PR #1840 left `sdd/work/precheck-comma-linear-scan/` | Adjacent fix: the lead deletes that directory in the publish commit together with this run's `sdd/work/` | A failure on `main` that this PR's CI would otherwise inherit; a delete-only commit of `sdd/work/` is the lead's to write; that PR is merged | Text only, no surface | — |

## Follow-up candidates

None yet.

## Audited SHA

b08b083 (against main dc6d942)

## Next step

Implementer done (01, d6bb4d8, tests pass). Cleaner done (02, e1d0230, clean). Spec checker done (03, 9643278, consistent). Merged origin/main dc6d942 (no overlap). Auditor done (04, 059c8a6, pass with one text finding in scripts/README.md:86). Audited SHA: b08b083. Spec author text fix done (05, 8922862); TEXT-ONLY PROOF PASS b08b083..8922862. Next: validator, publish.
