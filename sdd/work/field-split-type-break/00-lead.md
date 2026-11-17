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

1. Output question 1 (field split, line break written inside the head of a multi-declarator field; options A keep as written / B later fields one-line head / D all split fields one-line head): **A** — every split field repeats the head as written, line breaks included (today's output, written into the spec).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Output questions | Keep option C (join type/declarator break on all fields) in the question? | No; C becomes follow-up candidate 1 | Skeptic measured the break is kept on every declaration kind; a field-only join is a different construct outside this issue | C's output is not decided here; it goes to a follow-up that carries its own output question | — |
| 3 | After answer | Lane after answer A | Narrow from behavior change to tests: no change doc, no production change | A is today's output; a behavior-change claim requires a change doc | Output unchanged from main; the maintainer answered it | — |
| 4 | After answer | Continue spec author agent or fresh | Fresh | Its cache is cold and its context exceeds ~50k growth | Internal | — |
| 2 | Output questions | Issue goal "a test for this input" names the comment variant, now refused (FMT-UNSUP-004) | Test the comment-free input instead | The comment input never reaches the split | The refused input's output (unchanged + reported) is not touched | — |

## Follow-up candidates

1. Type-to-name line break is kept on every declaration head except parameters (FMT-BREAK-015); whether a general declaration-head join is wanted (02-skeptic, option C generalized). Carries an output question.
2. Property accessor list expands when the type is followed by a line break: `public int⏎    P { get; set; }` → accessor list broken onto separate lines; FMT-BREAK-024 reads two ways (02-skeptic). Carries an output question.

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01, commit 1ea832b) — verdict open question: what the repeated head of a split field carries when a line break is written inside it (A keep as written = today, B later fields one-line head, C join type/declarator break everywhere — C would be its own change). No spec edit yet.
- Done: skeptic (02, commit 50c6bb7) — verdict output question; drops C from this run (fields would become the odd one out; type/name break kept on every declaration but parameters), adds D (join head on every split field incl. first), recommends A. Declaring unsupported is not an option (deletes nothing). If A: lane narrows to tests.
- Maintainer answered A.
- Lane narrowed to **tests** (decision 3): roles now spec author (clarification) → test implementer → cleaner → spec checker → validator. Implementer and auditor skipped (no production change).
- Done: spec author (03, commit b91639c) — FMT-STRUCT-021 clarified with a new example (break behind long generic type, between modifiers repeated; break behind a `,` not repeated). Lane check: confirmed tests.
- Done: test implementer (04, commit bf7e0d2) — 8 new formatter tests + tag on existing, 2 RH7101 fix tests, 1 parity test; all green on unchanged code. Lane check: confirmed tests.
- Running: cleaner (05).
- Next: cleaner → spec checker → validator → issue manager (2 follow-up candidates) → publish.
