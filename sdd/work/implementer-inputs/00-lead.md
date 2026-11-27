# Run: implementer-inputs

- Issue: thoenissen/Reihitsu#1981 (`agent-filed`; goals are proposals)
- Slug: implementer-inputs
- Branch: claude/tender-fermi-k96rs1 (designated by the session); PR: draft, claim placeholder
- Lane: tooling (widened from docs, see Decisions) — the issue's one file, `squad/roles/implementer.md`, is classified tooling by the lane check.
- Roles run: implementer, cleaner, spec checker, auditor, validator. Skipped: spec author (ran once, found the mismatch, wrote no change), skeptic, test implementer (the tool has no tests).

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Docs lane cannot hold `squad/roles/implementer.md` | Widen to tooling along docs → tests → tooling | `scripts/lane-check.sh docs --head worktree` reported mismatch, diff needs tooling (01-spec-author.md) | Role-file text for agents; no formatter, diagnostic, code-fix, rule-page or CLI output | this commit |

## Follow-up candidates

None.

## Open worklist / next step

Spawn the implementer (tooling lane, issue by number only), then cleaner, spec checker, auditor, validator.
