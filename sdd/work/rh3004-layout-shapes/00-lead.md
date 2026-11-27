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
| 4 | Skeptic | RH3004-011 wording claims the body follows on the comment's last line; false when `)` stands on a later line | Amend: `=>` and body follow `)` on the line where `)` stands | Spec must match today's output, which SHARED-FIX-007 decides | Wording of today's stable output | 8657581 |
| 5 | Skeptic | Comment between a parameter and its comma on a later line gets no fix (FMT-UNSUP-009 via SHARED-FIX-010), unstated | Add SHARED-FIX-010 to RH3004-003's withholding list and `Uses`, and one sentence in RH3004-011 | Already today's behavior; an existing test is tagged SHARED-FIX-010 | No fix offered today, unchanged | 8657581 |
| 6 | Skeptic | Issue goal "align the predicates" (item 3) | Dropped: unreachable through RH3004 and the visible results agree | agent-filed goal | No output changes | — |
| 7 | Lead | Continue spec author or spawn fresh for the amendment | Fresh | Spec author's context large and cache cold | — | — |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01, 3187ae8). Architecture test red until a test tagged RH3004-011 exists.
- Done: skeptic (02, 137b857): proceed, no output questions, two spec amendments.
- Done: spec author amendment (03, 8657581). No output questions, no change doc: no batch stop.
- Done: test implementer (04, 7cad0b5): 15 tests, all green on unchanged code.
- Done: cleaner (05, 2a9584e): clean, 0 warnings.
- Done: spec checker (06, aa2c9ad): consistent, no findings.
- Next: validator (07), then publish.
