# Lead — script-build-flags

- Issue: thoenissen/Reihitsu#1828 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed.
- Slug: script-build-flags
- Branch: claude/squad-1828-nb8qws (session-designated)
- Draft PR: #1834
- Lane: tooling — the issue changes only flags (and possibly comments) in `scripts/trace.sh`, `scripts/apply-fix.sh`, `scripts/lane-check.sh`, which the lane-check table classifies as tooling.
- Roles run: implementer → cleaner → spec checker → auditor → validator.
- Roles skipped: spec author, skeptic (not in tooling lane); test implementer — the flags under question are wrapper-script build arguments with no test project covering them; the evidence the issue asks for is a clean-tree script run, which the implementer records in its handover.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Run the test implementer? | Skip | No test covers wrapper-script build flags; evidence is a clean-tree run recorded in the handover | Scripts are repository tooling, not a user surface | claim |
| 2 | Implementer | Follow-up: global `NoWarn` replaces project `NoWarn` | Drop | No script passes `NoWarn` any more, so no caller reaches it | Tooling only | 0be6478 |

## Follow-up candidates

None yet.

## State

- Last step: implementer 01 (tests pass, all five flags removed, 0be6478); lane check confirmed tooling.
- Next: cleaner (02).
