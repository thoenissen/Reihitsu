# Lead — RH2001 anonymous-object member name

- Issue: thoenissen/Reihitsu#1990 — `agent-filed` (goals are proposals; the output decision below is the maintainer's and binding).
- Slug: `rh2001-anonymous-member-name`
- Branch: `claude/great-galileo-pf6zoo` (session-designated); draft PR thoenissen/Reihitsu#1997.
- Base: `origin/main` 13b0ed2.
- Lane: **behavior-change** — the run changes what the RH2001 code fix produces (a fix offered where it is withheld today), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.

## Maintainer clarifications (summarized)

The maintainer answered the issue's output question in an issue comment: option B. RH2001's fix is offered where a reference to the property gives an anonymous-object member its inferred name, and the member keeps its name explicitly (`new { Value }` → `new { Value = _value }`, property becomes `private int _value;`). The run also resolves the spec conflict that SHARED-CASING-015, read literally, covers RH2001. Tuple elements stay as RH2001-011 states them (issue text).

## Maintainer answers

1. Issue output question (A/B): **B** — given in the issue comment before the run.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## Audited SHA

—

## Open worklist / next step

- Next: spec author (change doc + spec diff).
