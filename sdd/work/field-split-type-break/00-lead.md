# Lead — field-split-type-break

- Issue: thoenissen/Reihitsu#1587 — "A split field does not repeat the line break written behind the type" — label `agent-filed` (goals are proposals).
- Branch: `claude/determined-fermi-7uoq58` (session-designated). Draft PR: #1821.
- Slug: `field-split-type-break`.
- Lane: **behavior change**. Reason: the issue is an output question — what trivia the repeated type of a split field carries (FMT-STRUCT-021) is not decided by any spec today, so the change would alter formatter output and needs a change doc.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Issue context (summarized from the issue comments)

- The comment variant (`private int⏎/* c */ _x, _y;`) is now refused as unsupported by FMT-UNSUP-004 (comment inside a variable declaration); it is no longer part of the question.
- What remains: the comment-free `private int⏎        _g, _h;`. Today the split repeats the line break behind the type on every later field, and the declarator lands at the member column with no continuation indent (`private int⏎    _g;⏎    private int⏎    _h;`). A single declarator `private int⏎        _x;` becomes `private int⏎    _x;`.
- The column of such a continuation line is tracked by thoenissen/Reihitsu#953 (another issue, not part of this run).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Next: spawn spec author (01).
