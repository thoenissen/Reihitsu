# Lead — attached-part-line-break

- Issue: thoenissen/Reihitsu#892 — "Report a line break in front of an attached part of a member-access chain". Labels: enhancement, analyzer, `agent-filed` (goals are proposals).
- Slug: `attached-part-line-break`
- Branch: `claude/trusting-euler-pkbd73` (session-designated). Draft PR: thoenissen/Reihitsu#1892.
- Lane: **behavior change / new rule** — the issue requests a new analyzer diagnostic (or an extension of RH5201) with a code fix; a new or changed diagnostic is new behavior and needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.

## Context from the issue thread (summarized)

An agent comment on the issue (2026-11-18) reports that, since the formatter change for thoenissen/Reihitsu#895, the formatter joins `[`, `?[` and a trailing `!` written on the line below their element everywhere, not only in chains with a link (FMT-BREAK-040; no join across a comment, directive, or disabled text). Argument lists were already joined everywhere. So the issue's scope restriction "chains with a member access only, except `?[`" no longer matches the formatter, and the formatter owner is now the attached-part gap in the line-break phase, not `ChainLineBreakRewriter.NormalizeChain`. The argument list's `(` outside chains is the subject of a separate issue ("New rule: the opening parenthesis of an argument list is on the line of the code in front of it (counterpart of RH5105)"). Whichever rule is built must state the scope per part. This comment is itself agent-written, so it is a hypothesis like the issue.

## Maintainer answers

- Batch 1 (Q1–Q5, before the test implementer): all as recommended — 1A (`[`, `?[`, `!` everywhere; no argument lists, those stay with #1827), 2A (comment/directive in the gap: reported, no fix), 3A (fix lays out the whole chain when the part is in a chain with links), 4A (build the rule; Warning, enabled, Fix All, nothing reported in a file with syntax errors), 5A (outside a chain the fix deletes only the gap). **Change to Q4:** make two rules instead of one — one for the index (`[` and `?[`), one for the null-forgiving `!`. The maintainer also asked whether rules for a wrapped `?` and for a wrapped `(` already exist (answered in chat: `?`/`?[` gaps are covered by the new index rule, `?.` is a normal chain link; argument-list `(` has no rule, only RH5105 for declaration parameter lists, and #1827 proposes one).
- Rule identity: A — RH5115 "Index access must stay on the line of its expression" (reports `[` and `?[`, both gaps of `?[`); RH5116 "Null-forgiving operator must stay on the line of its expression" (reports `!`). Both Warning, enabled, Fix All, same fix behavior per 2A/3A/5A.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic (02) | Architecture finding FC-015: the analyzer needs the attached-part gap predicate, which lives only in the formatter's internal `AttachedPartGapUtilities` | Move the predicate (its `[`/`?[`/`!` part at least) into `Reihitsu.Core` in this run; formatter precheck, line-break phase and RH5115 analyzer all use the one copy. No private copy in the analyzer | A private copy would be a second statement of the rule (the narrow fix adds a copy); the move is ~117 lines, no new logic, within the analyzer and its formatter counterpart, below the escalation-9 bound | Pure move; formatter output unchanged, guarded by existing formatter tests | — |
| 2 | Skeptic (02) | Issue goal "use `FluentChain.AttachedParts`" and the stale formatter-owner reference | Replace with the moved predicate; the fix decides chain layout vs. plain join by `FluentChain.Create(...).Links.Count > 0` | `FluentChain` folds the `!` of `x!?[0]` differently from the join | Internal | — |
| 3 | Spec author (03) | Keep the RH5116 page sentence that a `!` directly in front of `.` or `?.` is not reported? | Keep | It states the rule's scope, as RH5105's page states what is outside its rule; it is not a list of corner cases or withheld fixes | Scope decided by answers 1A/4A; the sentence describes it | d24a646 |

## Follow-up candidates

- Comment on thoenissen/Reihitsu#1827 (01-spec-author.md, 02-skeptic.md): this run leaves every argument list's `(` to it if 1A stands; its proposal to withhold the fix for a comment in the gap predates FMT-UNSUP-014, and SHARED-DIAG-010 now asks for a report without a fix.
- Query clause layout (02-skeptic.md): `from v in arr⏎select v` puts `select` at the statement's column; no spec requirement decides it — an agent-filed issue carrying the output question drafted in 02-skeptic.md.
- (superseded) Comment on thoenissen/Reihitsu#1827 that this run leaves every argument list's `(` to it, if output question 1A stands (01-spec-author.md).

## State

- Last step finished: skeptic (02, f987f50): proceed; Q5 added; architecture finding decided (#1).
- Spec author 03 done (d24a646): two rules RH5115/RH5116 specified, change doc Accepted; lane check confirmed behavior-change.
- Test implementer 04 done (4043635): 112 tests, fail for the right reason against stubs; test project needs the four types.
- Next: implementer (05), including decision #1 (move the predicate to Core).
- Next after answers: spec author (fresh) — apply answers, Q5 sentence in RH5115-004, set change doc Accepted; also the decided move of the predicate to Core goes to the implementer (spec author updates code owners if needed). Then test implementer.
