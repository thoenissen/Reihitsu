# Run: using-block-directive-report

- Issue: thoenissen/Reihitsu#1836 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer. Milestone 1.6.0.
- Branch: `claude/fervent-mccarthy-vxx8ss` (session-designated). Draft PR: #1968.
- Lane: **behavior-change** — the issue requests changed behavior (RH7201–RH7207 reporting in a using block that holds a preprocessor directive, with no fix), which needs a change doc and a spec rewrite of SHARED-USING-009.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager if follow-up candidates exist. None skipped.
- Maintainer clarifications: none yet. The issue itself is an output question (A: report without a fix; B: keep current silence), carried no recommendation.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Reorder a using block whose every directive stands before its first using (e.g. a `#nullable enable` header) in fix and formatter — changes formatter output; from 01-spec-author.md.

## Worklist / next step

- Done: spec author 01 (change doc written, 1db90d1; 4 output questions).
- Next: skeptic (02), then output-question batch.
