# Lead — trailing-comma-branch-end

- Issue: thoenissen/Reihitsu#1837 (`agent-filed`; goals are proposals). RH5409/RH5410/RH5411 trailing-comma fix and formatter in front of `#else`/`#elif`/`#endif` with no `#if` between the comma and the closing brace.
- Slug: `trailing-comma-branch-end`
- Branch: `claude/funny-ptolemy-pt5lxq` (session-designated). Draft PR: thoenissen/Reihitsu#1955.
- Lane: behavior-change — the issue requests changed code-fix and formatter output (a fix offered where none is today), which changes SHARED-TRAIL-002 and FMT-STRUCT-019/020 and needs a change doc.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue. The issue itself is an output question (A: offer fix + formatter removes; B: unchanged, close issue).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, prepare.sh green.
- Next: spec author (01), then skeptic (02), then output question batch.
