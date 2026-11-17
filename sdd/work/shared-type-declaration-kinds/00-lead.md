# Lead — shared-type-declaration-kinds

- Issue: thoenissen/Reihitsu#1788 — "Analyzers, Core, formatter: the type-declaration kinds a rule selects come from one shared set". Label `agent-filed`: its goals, options, and "Done when" are proposals.
- Slug: `shared-type-declaration-kinds`
- Branch: `claude/squad-1788-96lw0x` (session-designated). Draft PR: thoenissen/Reihitsu#1841.
- Lane: **bug fix**. The issue text says "Lane: tooling", but every path it names is production (`src/Reihitsu.Analyzer`, `src/Reihitsu.Core`, the formatter) or test (`src/Reihitsu.ArchitectureTests`); the lane check classifies production without a change doc as bug fix. The issue states that no output changes, so no change doc is expected. Either of the issue's options fits this lane (an architecture test alone would need only the tests lane, which bug fix is wider than).
- Roles run: skeptic (premise, reachability, which option), test implementer, implementer, cleaner, spec checker, auditor, validator. Spec author skipped unless a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane, given the issue says tooling | Bug fix | Named paths are production/test, not tooling; no output change planned | Lane choice is internal | claim |

## Follow-up candidates

None yet.

## Worklist / next step

- Next: spawn skeptic.
