# Lead — attribute-list-structural

| | |
|---|---|
| Issue | thoenissen/Reihitsu#1492 — FMT-STRUCT: attribute lists are split and merged by the structural transforms |
| `agent-filed` | yes — goals, "Done when", and the prescribed solution are proposals |
| Parent | thoenissen/Reihitsu#1482 (package C6); blocker #1488 is closed (PR #1513 merged) |
| Slug | `attribute-list-structural` |
| Branch | `claude/brave-curie-dwg8sl` (session-designated branch, used instead of `claude/issue-1492-...`) |
| Draft PR | thoenissen/Reihitsu#1551 |
| Lane | behavior change |

## Lane and reason

The issue requests moving an existing formatter decision (FMT-BREAK-027's attribute-list split and merge) from the line-break phase into the structural-transform phase, with a change doc that re-homes the requirement and updates the code-owner lists and the `AGENTS.md` phase-ownership rows. Shipped source under `src/` plus a change doc under `sdd/changes/`: behavior-change lane. The issue states that formatter output stays unchanged.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

Note: the `AGENTS.md` ownership table edit is a tooling path; the behavior-change lane admits it (tooling → behavior change).

## Maintainer clarifications

None. The issue text asks the run to first check every statement about today's code against current `main` and drop statements an earlier package already made true or moot.

## Maintainer answers

1. Output question — splitting an attribute list whose attributes are separated by a line break that is not right after the comma (`[A⏎ , B]`, `[A,⏎⏎ B]`): the maintainer chose A, both become `[A]⏎[B]`. The maintainer also asked the run to proceed on every recommendation without further stops, as they can only respond after the run is finished.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/brave-curie-dwg8sl` instead of `claude/issue-1492-<slug>` | The session environment only permits pushing to its designated branch | Branch names are internal | claim commit |
| 2 | Spec author | Accessor attribute lists: move their split/merge too (B: predict line-break layout; C: decide on written layout) or keep them in the line-break phase (A)? | A — accessor list shape stays in the line-break phase (FMT-BREAK-032); the issue's first "Done when" item is narrowed to declarations, parameters and type parameters | The accessor decision depends on layout the line-break phase produces; B would add a prediction of a later phase (an architecture smell), C changes output and is not idempotent | Formatter output is unchanged under A | 91e64ce |
| 4 | Skeptic | `[A,⏎ B]` needs two passes to settle (`[A]⏎[     B]` after pass 1) | Fix in this run: the split leaves no whitespace between a created bracket and its attribute; the change doc names the changed first-pass output | FMT-PIPE-004 (idempotency) and FMT-SPACE-004 demand it; unchanged requirements | The fixed point is unchanged; only a non-idempotent intermediate output changes, which unchanged requirements decide |
| 5 | Skeptic | Does the structural merge keep a line break the author wrote between merged lists? | Yes — FMT-STRUCT-029 keeps it, so output stays unchanged | Keeps today's output and the consistency with the placement-only neighbor | Output unchanged |
| 6 | Skeptic | Lambda attribute-list split | Moves with the declaration split; the change doc states it | Output unchanged; only accessor shape stays in FMT-BREAK | Output unchanged |
| 7 | Skeptic | Architecture findings: two splits (structural + accessor `BuildSplitAttributeLists`) and two merges (structural + Core `MergeAttributeListGroup`) | Change the structure in this run: one shared split and one shared merge, callers differing only in the separators FMT-STRUCT-026 and SHARED-ATTR-017/018 give them | The narrow fix would itself add a copy; both stay within the run's components (formatter attribute rewriter, Core attribute utilities) and their counterparts | Code-fix output is held by SHARED-ATTR-017/018; formatter output unchanged except the answered output question |
| 8 | Spec author (amend) | Accessor merge separators | Keep the code fix's separators (one space, no line break); FMT-BREAK-031 exception stays for it | Output unchanged; accessor shape stays in FMT-BREAK (decision 2) | Output unchanged |
| 9 | Spec author (amend) | Where the line break FMT-STRUCT-029 keeps stands | Left open in the spec | FMT-BREAK-027 joins the lists onto the parameter's line, so the final output does not depend on it | Output unchanged |
| 10 | Spec author (amend) | Lambda split in FMT-STRUCT-028 | Recorded only in the change doc; lambdas stay unspecified there | Today's lambda output looks wrong and is follow-up candidate 1 | Output unchanged |
| 11 | Spec author (amend) | Does the single split also replace the code fix's split (SHARED-ATTR-017)? | No; this run unifies the formatter's structural and accessor splits only; the code-fix split becomes follow-up candidate 6 | The code fix is not changed by this run and has its own separator contract | Code-fix output unchanged |
| 12 | Spec author (amend) | Accept the change doc | Accepted at `bfa84b7` | Skeptic has run and its amendments are written; its visible output changes are decision 4 (demanded by FMT-PIPE-004/FMT-SPACE-004) and maintainer answer 1 | Covered by answer 1 and unchanged requirements |
| 13 | Implementer | New public Core members `AttributeListMergeSeparators` and the three-argument `MergeAttributeListGroup` overload | Accepted | It is the shape decision 7 asks for (one merge, callers differ in separators); no assembly is a public API (`AGENTS.md`, *Supported inputs*) | API is internal; the two-argument overload keeps code-fix output | 44e6f8c |
| 3 | Spec author | When to update code-owner rows and `AGENTS.md` phase-ownership rows | After the implementer, by a fresh spec-author agent, once the new transform types exist (`SpecConsistencyTests` rejects unknown type names) | Ordering constraint of the consistency test | Internal documentation | — |

## Follow-up candidates

1. Lambda attribute lists are split onto their own lines (`var f = [A, B] (int x) => x;`) — outside the run's issue; carries an output question. Source: `01-spec-author.md`.
2. One accessor attribute expands a one-line auto-property whose object initializer spans several lines (`X P { [A] get; set; } = new X { A = 1 };`), apparent conflict with FMT-BREAK-024 — outside the run's issue; carries an output question. Source: `01-spec-author.md`.
3. Spacing phase never normalizes same-line whitespace held as the right token's leading trivia (general cause behind decision 4). Source: `02-skeptic.md`.
4. Line breaks inside attribute-list brackets are never joined (`[A⏎ ]` stays on two lines). Source: `02-skeptic.md`.
5. A blank line is inserted before a `///` comment inside a parameter's attribute group (CS1587 input). Source: `02-skeptic.md`.
6. The code fix's attribute-list split (SHARED-ATTR-017) is a separate implementation beside the formatter's single split. Source: `03-spec-author.md`, lead decision 11.
7. `void M(/* c */ [A] int a)` loses the space behind the comment on `main` (spacing, outside the run). Source: `04-test-implementer.md`.
