# Lead — raw-string-tab-alignment

- Issue: thoenissen/Reihitsu#1432 — FMT-RAW: align tab-indented multi-line raw strings. Labels: enhancement, formatter, `agent-filed` (goals are proposals).
- Slug: `raw-string-tab-alignment`
- Branch: `claude/modest-turing-gg3lyc` (session-designated). Draft PR: #1923.
- Lane: **behavior change** — the issue requests a layout for an input the spec lists as a non-goal (`sdd/specs/formatter/raw-string-alignment.md`); a new requirement and a change doc are needed.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Issue context (summarized): the issue's text names `RawStringAlignmentPhase`; per `AGENTS.md`, raw-string alignment now lives in `IndentationPhase`. The issue's one comment (agent-added, unverified) notes a possibly related case: an interpolation-hole line inside a tab-indented raw string aligned by character count rather than visually; it asks that this be checked with a test first and dropped if the prefix is already replaced by the time the hole is aligned.
- No maintainer clarifications yet.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, lane claimed.
- Next: spawn spec author.
