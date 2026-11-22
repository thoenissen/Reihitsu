# Lead — list-pattern-columns

- Branch: `claude/squad-tickets-review-w5g0uk` (session-designated)
- PR: thoenissen/Reihitsu#1933 (draft)
- Base at claim: `75cc39f` (origin/main)
- Lane: **behavior-change**

## Issues

| Issue | agent-filed | Own lane | Subject |
|---|---|---|---|
| thoenissen/Reihitsu#1633 | yes | behavior-change | Base list wrapped behind its colon (column of the base types; comma-first shape in comment) |
| thoenissen/Reihitsu#1634 | yes | behavior-change | Comment-held line inside an angle-bracket list's first element or a qualified name |
| thoenissen/Reihitsu#1647 | yes | behavior-change | Comment in front of a list pattern's closing `]` |
| thoenissen/Reihitsu#1685 | yes | behavior-change | Wrapped `when` condition of a `case` label |
| thoenissen/Reihitsu#1542 | yes | behavior-change | Content of a wrapped parenthesized or positional pattern |
| thoenissen/Reihitsu#1929 | yes | bug-fix (FMT-INDENT-026, possible wording clarification) | Property-pattern brace behind `and`/`not` in a switch arm |

Lane reason: five issues ask for new formatter layout decided by output questions (change doc required). #1929 alone would be a bug fix against FMT-INDENT-026; it was placed in this run by the milestone coordinator because it shares `PatternAnchor` and FMT-INDENT-026 with #1542, and the behavior-change lane admits a bug-fix diff (lanes widen bug fix → behavior change). Recorded as decision 1.

All roles of the lane run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

Issue comments relevant to the run (summarized):
- #1633: the merged change #1920 (FMT-INDENT-034 continuation indent, FMT-BREAK-041 joins) joins a base list's `:` onto the type line; the issue's no-comment "Before" may no longer occur; restate against current behavior. Also a comma-first base-list shape (`class K : IA⏎, IB`) was proposed to be decided together.
- #1634: after #1920 the "Before" changed: unowned continuation lines now sit one level deeper than their construct's line; an alignment this issue chooses takes precedence over FMT-INDENT-034.
- #1542: further members (`is not⏎(...)`, `is C⏎(1)`, `{ X:⏎1 }`, `(1⏎or 2)`, `(int⏎or long)`, case-label form); comma-first positional pattern (`(1⏎, 2)`); after #1920 parenthesized-pattern content sits four columns right of `(` by a maintainer answer there; positional patterns still undecided.
- #1929: coordinator note: FMT-INDENT-026 already requires anchoring past `and`/`or`/`not`, so no output question is expected.

## Maintainer answers

- Batch 1 (Q1, Q3, Q6, Q7, Q7b, Q8 as in 03-spec-author.md): maintainer answered "as recommended" → 1A, 3A, 6A, 7A, 7bA, 8A.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | #1929 is a bug fix while the other issues are behavior changes; split the run? | Keep all six in one behavior-change run | The maintainer named all six in the prompt; the coordinator batched #1929 here for the shared `PatternAnchor`/FMT-INDENT-026 mechanism; the behavior-change lane admits a bug-fix diff | Lane choice is internal | claim |
| 2 | Spec author | #1929 turned out to carry an output question (case-label anchor reading); #1685 already met on main | Accept: #1929 gets a change doc and Q8; #1685 gets only a spec example + characterization test | Spec author's handover shows two readings with different `case` output; main already gives #1685's reading A | #1929's visible part goes to the maintainer as Q8; #1685 changes no output | 26aff07 |
| 3 | Skeptic | Comment-held first base type (Q2), comment-held first element of a bracket list (Q4), line inside a qualified name (Q5): new columns? | Refuse the rare class: keep today's output (FMT-INDENT-034 continuation indent); drop FMT-INDENT-036/037/038 and Q2/Q4/Q5; the spec records that these lines keep the continuation indent | Every such input already violates RH5104 / RH5043 / RH5044 / RH5046, so it is rare by definition (AGENTS.md, Code-fix coverage); FMT-INDENT-038 would also move comment-free qualified names in casts, usings, etc., outside the issues' scope | Output stays as on main | (spec-author 03) |
| 4 | Skeptic | Declare "comment behind a base list's colon" unsupported (FMT-UNSUP-017 extension)? | Not an option: with Q2 dropped, the shape deletes no special case (AGENTS.md: a shape that deletes nothing is not declared); remove the spec author's recorded internal choice | Escalation 10 applies only where declaring deletes something | No refusal is added | (spec-author 03) |
| 5 | Skeptic | #1634's purpose with no new columns | `Closes #1634`: the issue's purpose was that no requirement owns these lines; after #1920 FMT-INDENT-034 owns them, which the spec now states for this class | Agent-filed goals are proposals; nothing visible changes | Output unchanged | (spec-author 03) |
| 6 | Skeptic | Amendments before the batch | Route to a fresh spec author: #1929 list-pattern element member, Q8 B wording; #1542 Q7b (`and` member), switch-expression arm, FMT-INDENT-006 dependency on Q7; drop the #1634 change doc unless a visible change remains | Skeptic findings within scope | Questions still go to the maintainer | (spec-author 03) |

## Follow-up candidates

- Qualified name wrapped behind its dot in a member's type is not joined (joined in a constraint) — 01-spec-author.md
- FMT-BREAK-017 should count a multi-line positional pattern as wrapping a pattern chain, if Q7 = A (may fold into Q7) — 01-spec-author.md
- FMT-INDENT-021 `Why:` contradicts its own example (spec wording) — 01-spec-author.md
- Qualified names wrapped behind their dot without a comment are not joined in using aliases, namespace names, explicit interface names, casts, typeof, default, new, member types (whole class) — 02-skeptic.md

## Worklist / next step

- Spec author done (26aff07), 8 output questions open in 01-spec-author.md. Lane check confirmed.
- Skeptic done (2a8cb38). Decisions 3–6.
- Spec author 03 done (fbf4bae): decisions 3–6 carried out; #1634 change doc deleted; FMT-INDENT-036/037/038 retired.
- Batch 1 answered: all A.
- Spec author 05 done (52e40b7): change docs Accepted.
- Test implementer 06 done (b4d741a tests, f2e5cb2 handover): 35 formatter + 2 analyzer tests fail as intended.
- Next: implementer (07).
