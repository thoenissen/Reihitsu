# Lead — rh7203-comment-using-indent

- Issue: thoenissen/Reihitsu#1746 — RH7203 fix writes the moved using at column 0 in a block-scoped namespace whose first using has a leading comment. Labels: bug, analyzer, `agent-filed` (goals are proposals).
- Slug: `rh7203-comment-using-indent`
- Branch: `claude/gallant-edison-ndliur` (session-designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1752 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference of a code fix's output against an unchanged requirement (SHARED-FIX-007).
- Roles run: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author only if a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/gallant-edison-ndliur` instead of `claude/issue-1746-<slug>` | The session environment permits pushing only to that branch | Branch names are internal | 4959026 |
| 2 | Skeptic | Scope: only RH7203 or all using-ordering fixes | All seven fixes RH7201–RH7207, both sides of the header split (01-skeptic) | Same phase, same cause; all six other rules reproduce | SHARED-FIX-007 (unchanged) decides the block-namespace members | — |
| 3 | Skeptic | Issue goals | Widen "moved directive" to every line of the organized block; narrow "same text as whole-document formatting" to the using block's lines; drop the issue's naming of the code-fix utilities as cause | 01-skeptic, Issue goals | Demanded by SHARED-FIX-007; no other output changes | — |
| 4 | Skeptic | Refuse the class instead of fixing | No: fix in the using phase | The class is common code (usings inside block namespaces with a header comment); declaring unsupported deletes nothing, so not an option | Fixing restores an unchanged requirement | — |
| 5 | Skeptic | Test placement | RH7203 matrix plus one block-namespace test per other rule, plus Fix All and whole-document guard | One entry point and one cause; each provider still exercised | Internal | — |
| 6 | Skeptic | FMT-USING-009 clarification | Decide after the maintainer answers output question 1 (add it if 1A) | Only needed to record the s1/u1 answer | Internal | — |

## Follow-up candidates

None yet.

## Worklist / state

- Last step: skeptic done (01-skeptic.md, fa73983): proceed; two output questions (file-scoped/compilation-unit indented usings with header; tab-indented block namespace).
- Next: waiting on the maintainer's answers to output questions 1 and 2 (asked before the test implementer); then spec author if needed, then test implementer.
