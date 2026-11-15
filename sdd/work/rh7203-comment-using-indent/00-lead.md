# Lead — rh7203-comment-using-indent

- Issue: thoenissen/Reihitsu#1746 — RH7203 fix writes the moved using at column 0 in a block-scoped namespace whose first using has a leading comment. Labels: bug, analyzer, `agent-filed` (goals are proposals).
- Slug: `rh7203-comment-using-indent`
- Branch: `claude/gallant-edison-ndliur` (session-designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1752 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference of a code fix's output against an unchanged requirement (SHARED-FIX-007).
- Roles run: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author only if a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

1. Output question 1 (indented usings with a header comment in a file-scoped namespace or compilation unit, RH7201–RH7207 fixes): **A** — every directive of the organized block keeps the indentation of the original first directive's line (column 4 in the example), as the same block without the comment is fixed today.
2. Output question 2 (tab-indented block namespace with a header comment): **A** — the moved directive keeps the tab, as the same block without the comment is fixed today.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/gallant-edison-ndliur` instead of `claude/issue-1746-<slug>` | The session environment permits pushing only to that branch | Branch names are internal | 4959026 |
| 2 | Skeptic | Scope: only RH7203 or all using-ordering fixes | All seven fixes RH7201–RH7207, both sides of the header split (01-skeptic) | Same phase, same cause; all six other rules reproduce | SHARED-FIX-007 (unchanged) decides the block-namespace members | — |
| 3 | Skeptic | Issue goals | Widen "moved directive" to every line of the organized block; narrow "same text as whole-document formatting" to the using block's lines; drop the issue's naming of the code-fix utilities as cause | 01-skeptic, Issue goals | Demanded by SHARED-FIX-007; no other output changes | — |
| 4 | Skeptic | Refuse the class instead of fixing | No: fix in the using phase | The class is common code (usings inside block namespaces with a header comment); declaring unsupported deletes nothing, so not an option | Fixing restores an unchanged requirement | — |
| 5 | Skeptic | Test placement | RH7203 matrix plus one block-namespace test per other rule, plus Fix All and whole-document guard | One entry point and one cause; each provider still exercised | Internal | — |
| 6 | Skeptic | FMT-USING-009 clarification | Add it (maintainer answered 1A): spec author records answers 1 and 2 in FMT-USING-009 (or where the spec author finds they belong), with a block-namespace example | Records the maintainer's answers where they take effect | Covered by maintainer answers 1A/2A | — |

## Follow-up candidates

1. (07-auditor, hint 1; pre-existing on `main`) In the using phase's header split (FMT-USING-009), a line holding only whitespace is not treated as the blank line that ends the header (`UsingLeadingTriviaBuilder.GetHeaderSplitIndex` needs two consecutive line breaks), so a comment below such a line stays at the top instead of moving with its directive. Reached through the RH7201–RH7207 code fixes (`scripts/apply-fix.sh RH7203`). Input: `// Header` / a line of spaces / `// About Linq` / `using System.Linq;` / `using System;`.

## Worklist / state

- Last step: skeptic done (01-skeptic.md, fa73983): proceed; two output questions (file-scoped/compilation-unit indented usings with header; tab-indented block namespace).
- Maintainer answered 1A, 2A.
- Spec author done (02-spec-author.md, a8adc34): FMT-USING-011 added; lane check confirmed bug-fix.
- Test implementer done (03-test-implementer.md, a9b2d9b): reproduced; 17 formatter + RH7201–RH7207 code-fix tests failing as expected.
- Implementer done (04-implementer.md, e360f52): fix in UsingLeadingTriviaBuilder + UsingDirectiveOrderingRewriter (+30/-2); all tests pass in Debug.
- Cleaner done (05-cleaner.md, 500e3cc): clean, Release build 0 warnings.
- Spec checker done (06-spec-checker.md, da0fc6d): consistent, no findings.
- Auditor passed, no findings (07-auditor.md, e008d35). Audited SHA: 1c383897ec19d845dd9e0466845ac79546c60bde (base main 6ab3d75).
- Next: validator, then issue manager (1 candidate), note, publish.
