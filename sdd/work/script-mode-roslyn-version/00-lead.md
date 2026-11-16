# Lead — script-mode-roslyn-version

- Issues: thoenissen/Reihitsu#898 (`agent-filed`), thoenissen/Reihitsu#1780 (`agent-filed`). Goals of both are proposals.
- Branch: `claude/kind-brahmagupta-vu28e9` (session-designated). PR: #1810 (draft).
- Lane: **tooling**.
  - #898: the change is the mode of `scripts/trace.sh` (tooling path `scripts/**`), optionally a guard test in `Reihitsu.ArchitectureTests` (test path).
  - #1780: the change is `*.csproj`/`*.props` (tooling paths), optionally a guard test in `Reihitsu.ArchitectureTests`.
- Roles run: test implementer (the guards are tests), implementer, cleaner, spec checker, auditor, validator.
- Roles skipped: spec author and skeptic — the tooling lane runs neither; no visible output (formatter, diagnostics, fixes, rule pages, CLI) is touched.
- Maintainer clarifications: none.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Do #898 and #1780 share a mechanism, or should the batch be split? | Keep one run | Both are repository-wide invariants over tracked build/script files whose regression guard lives in `Reihitsu.ArchitectureTests`; together they are two small tooling changes, so no handover grows long | Batch shape only | claim |

## Follow-up candidates

None yet.

## State

- Audited SHA: —
- Last step: claim, prepare.
- Next step: test implementer.
