# Lead — rh3004-layout-shapes

- Issues: thoenissen/Reihitsu#1607 (`agent-filed`: goals are proposals)
- Branch: claude/affectionate-bohr-2jfs19 (session-designated)
- PR: #1984 (draft)
- Slug: rh3004-layout-shapes
- Lane: tests — the issue asks for spec examples and tests of existing RH3004 code-fix behavior; item 3 may turn out to need a production change, in which case the lane widens.
- Roles: spec author (writes the spec examples), skeptic (spec against the code), test implementer, cleaner, spec checker, validator. Skipped: implementer and auditor (tests lane has none; re-added on widening).
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Item 1 as its own requirement or inside RH3004-006 | New RH3004-011 | It has its own reason (FMT-INDENT-007 with SHARED-FIX-007) | Specifies today's output, which unchanged FMT-INDENT-007 and SHARED-FIX-007 already demand | 3187ae8 |
| 2 | Spec author | Item 2 as a rewording of RH3004-006 or an example under RH3004-008 | Example under RH3004-008 | RH3004-006 already excludes the case; the fix is withheld (RH3004-003/008) | No fix is offered today, unchanged | 3187ae8 |
| 3 | Spec author | Items 2 and 3 have no layout to specify (no fix offered) | Accept: their decided output is "no fix offered"; item 3 confirmed identical, no production change, lane stays tests | Issue is agent-filed; goals are proposals | Nothing changes; PR states the departure | 3187ae8 |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01, 3187ae8). Architecture test red until a test tagged RH3004-011 exists.
- Next: skeptic (02).
