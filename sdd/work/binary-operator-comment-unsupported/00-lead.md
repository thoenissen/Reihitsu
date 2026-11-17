# Lead — binary-operator-comment-unsupported

- Issue: thoenissen/Reihitsu#1726 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter. Milestone 1.4.0.
- Slug: `binary-operator-comment-unsupported`
- Branch: `claude/admiring-heisenberg-tpv91a` (session-designated). Draft PR: #1831.
- Base at claim: `a971d8944cdc449278472f6ad85e403c9d48cd70`
- Lane: **behavior change** — the issue declares a new unsupported shape (new FMT-UNSUP requirement, precheck change, visible formatter/CLI/code-fix output change); this needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

- Escalation 10 (declare the shape unsupported), answered before this run and carried by the issue text: declare a comment or directive between a binary operator and its right operand unsupported, in a change of its own (this run). Comments above the operator's line stay supported; only the position behind the operator is refused. Same decision as for the conditional operator (FMT-UNSUP-008).

- Batch 1 (8 output questions, before test implementer): maintainer answered A to all: 1 declare the shape in this change; 2 same-line block comments included; 3 directives/disabled text included; 4 RH5302 fix withheld chain-wide; 5 other formatting fixes withheld (RH3202 example etc.); 6 `is` + constant name refused (spec states it syntactically); 7 RH2001 fix withheld for operator comment in initializer; 8 RH7101 fix withheld type-wide. Asked back about "follow-up for B": the maintainer meant question 8 — the RH7101 fix should not format the whole type (so a refused shape elsewhere in the type does not withhold it); handle in a follow-up, not this run.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Retire RH5302-005 (issue goal) or rewrite in place | Rewrite in place (RH5308-005 precedent) | Keeps existing tagged tests homed; skeptic agrees | Requirement wording only | c920452 |
| 2 | spec author | Reason text where FMT-UNSUP-004/-010 overlap | Keep their reason; new shape ranks after them | Existing CLI messages unchanged | No existing message changes | c920452 |
| 3 | skeptic | indentation.md non-goal naming binary right operand | Spec author drops that example in the amendment | Unreachable after the change | Spec text only | pending |
| 4 | skeptic | Escalation-10 answer only in agent-filed issue text | Ask the batch as confirmation of the declaration itself | Not traceable in repo | — (asked) | — |

## Follow-up candidates

- Update #953: its example and members `1 + // c⏎2`, `a && // c⏎b` become refused; list pattern continuations (`o is⏎not null`, `o is⏎int i`) explicitly (02-skeptic).
- RH7101 spec silent about FMT-UNSUP-008/-009 in `Uses` (02-skeptic) — fold into RH7101 amendment if Q8 answered A, else follow-up.

- RH7101 fix formats the whole type, so any unsupported shape anywhere in the type withholds the fix for every combined field; the maintainer wants it to stop formatting the whole type (maintainer answer to batch 1, question 8). Carries the answer; not in this run.

## Audited SHA

(none yet)

## Open worklist / next step

- Spec author done (01, c920452): FMT-UNSUP-011, change doc Draft. Skeptic done (02, 8ab8f07): proceed narrower. Batch 1 answered (all A). Next after answers: spec author amendment (RH2001-007, RH7101-007/Uses, is-constant sentence, change 6 RH7101 example, rarity sentence, non-goal), then test implementer.
