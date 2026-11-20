# Lead — rh5419-indexer-arrow

- Issue: thoenissen/Reihitsu#851 — "[ANALYZER FEATURE] Expression style get-only indexers should be single lined". Label `agent-filed`: goals are proposals.
- Slug: `rh5419-indexer-arrow`
- Branch: `claude/modest-albattani-nbx229` (session-designated). Draft PR: #1882.
- Lane: **behavior change / new rule** — the issue requests a new analyzer rule (new diagnostic + code fix). The issue itself names the new rule, so this is the issue's own deliverable, not escalation 5.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publish if follow-up candidates exist. None skipped.
- Notes from intake: the issue proposes ID RH5418, but RH5418 is already taken on `main` (EmptyUnionsShouldUseSemicolonDeclarations); the next free RH54## ID is RH5419. Dependency #848 is closed (merged via PR #854). RH5401 stays properties-only per the issue.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Skeptic | Skeptic's two follow-up candidates (`this`/`[` split; comment/#if in indexer parameter list) | Dropped | Rare input classes; the first is decided unreported by RH5108's non-goal | Not filed; formatter output unchanged | — |
| 1 | Intake | Rule ID | RH5419 instead of the issue's RH5418 | RH5418 is taken on `main`; the issue says "next free RH54## ID" | The issue's intent ("next free ID") decides it | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Spec author done (01-spec-author.md, commit 7f9df95): change doc written; open output question Q1 (wrapped parameter list: fix offered vs withheld). Lane check confirmed behavior-change.
- Skeptic done (02-skeptic.md, 3ddc02a): proceed. FC-015 risk for auditor: RH5401 attribute-list guard must not be privately copied into RH5419 provider. Follow-up candidates recommended for dropping (rare / decided by RH5108 non-goal).
- Output-question batch 1 posted to maintainer (Q1 rule as specified incl. title; Q2 wrapped parameter list A/B). WAITING for answers. Next: route answers to spec author (status Accepted), then test implementer.
