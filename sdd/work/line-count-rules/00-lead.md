# Lead — line-count-rules

- Issues: thoenissen/Reihitsu#353 (agent-filed) — report methods whose line count exceeds a limit; thoenissen/Reihitsu#354 (agent-filed) — report types whose line count exceeds a limit. Both carry `agent-filed`: goals, target behavior, and prescribed solutions are proposals.
- Slug: `line-count-rules`
- Branch: `claude/squad-353-354-pq7nz1` (designated by the session)
- PR: thoenissen/Reihitsu#1860 (draft claim)
- Lane: behavior change / new rule — both issues request a new analyzer rule (new diagnostic, new configuration option, new rule page); no existing behavior is reported as wrong. They share a mechanism (counting the lines of a declaration's body against a configured limit) and both sit in the Design (`RH2###`) range, so one run.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt (`/squad #353 #354`).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | State that the line count follows the text as laid out (formatting can raise it) | Accept; spec author adds one sentence to SHARED-LINES-001 `Why:` | Prevents later roles from raising FC-013/FC-014 disagreements | Wording only | pending |
| 2 | Skeptic | Define a line as the compiler's line split (CRLF/LF/CR count alike, FC-033) | Accept; spec author states it in SHARED-LINES-001 | Removes an ambiguity | Same count for all line endings is the only sensible reading; no output changes beyond it | pending |
| 3 | Skeptic | One owner for the counting (FC-015) | Accept; implementer builds one shared counter that both rules use, and it becomes the code owner of `shared/line-count.md` | Avoids private copies | Internal structure | pending |

## Follow-up candidates

- A setting written twice in `reihitsu.json` is not reported by RH0001; the later value wins (02-skeptic.md, found by reading code, low value).

## Audited SHA

(none yet)

## Open worklist / next step

- Done: 01-spec-author (eaa17de), 02-skeptic (b7f1ce2).
- Now: output-question batch 1 (Q1–Q8 from 01/02 handovers) posted to the maintainer; waiting.
- Next: route answers to the spec author (amend specs, add decisions 1–2, set change doc Accepted), then the test implementer.
