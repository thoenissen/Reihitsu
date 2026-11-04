# Lead — thoenissen/Reihitsu#1502

Role: lead
Commit: df4f828 (origin/main at claim)
Verdict: lane claimed

## Run

- Issue: thoenissen/Reihitsu#1502 — "FMT-PIPE: RH7101 and RH7201–RH7207 reach the formatter through a specified entry point". Label `agent-filed`: its goals, "Done when" items, target behavior and prescribed solution are proposals; a change to what an end user sees stays a maintainer question.
- Blocked-by issue thoenissen/Reihitsu#1501 (E1) is closed, merged with PR #1580, which is the current `origin/main` (df4f828).
- Slug: `fix-entry-point`
- Branch: `claude/zen-mayer-s173ql` (the session's assigned branch, instead of `claude/issue-1502-<slug>`; see Decisions taken).
- Draft PR: thoenissen/Reihitsu#1581
- Lane: **behavior change**. The issue asks for changed formatter structure and spec requirements (FMT-PIPE, FMT-USING, FMT-STRUCT, SHARED-USING-010) and requires a change doc; shipped source under `src/` with a change doc is the behavior-change lane.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; the issue manager before publishing when follow-up candidates exist. No role is skipped.

## Maintainer clarifications (summarized)

- The node-level formatting methods of `ReihitsuFormatter` are not a public API. A change to them only has to cover the cases the code fixes actually pass (the RH7101 and RH7201–RH7207 code fixes here, and the existing callers); inputs that only a direct call would produce need no handling, no requirement, and no test (`AGENTS.md`, *Supported inputs*).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's assigned branch `claude/zen-mayer-s173ql` instead of `claude/issue-1502-<slug>` | The session environment permits pushes only to its assigned branch | Branch names are internal | claim commit |
| 2 | Spec author | Drop the RH7201–RH7207 goals of the issue (format through an entry point, remove `GetScopeIndentation`) | Dropped; FMT-USING, SHARED-USING-010 and the using phase stay | No entry point takes a using block as target; FMT-USING already names the dependency; FMT-USING-003/SHARED-USING-010 specify the indentation (01-spec-author, 02-skeptic agree) | The seven fixes' outputs do not change | 6e272bb |
| 3 | Spec author | Entry point for RH7101 | Existing `FormatNodeInDocumentWithContextAsync` with FMT-PIPE-029; no new "run one phase" entry point | A one-phase entry point keeps a second layout, which the issue removes | Internal API; the visible results are asked as output questions | 6e272bb |
| 4 | Spec author | Done-when "RH7101 calls no phase or transform" | Read as "runs none"; the static guard `CarriesDirective` stays callable | It is the single statement of FMT-STRUCT-023 | Internal | 6e272bb |
| 5 | Spec author | Field split keeps one space before a comment that trailed a comma and the line break behind a moved comment | Kept, stated in FMT-STRUCT-026 | No later phase redoes them; dropping them changes document-level output | Document-level output stays unchanged | 6e272bb |
| 6 | Skeptic | Case 6 and the mirror case (another member behind the split declaration on its line) | Both: the other member starts the next line; the spec diff gains the mirror clause, and the comment-in-the-gap reading that matches document-level output (`_p; /* c */` stays, `_x` starts the next line) | SHARED-FIX-007 *Shared line*, unchanged, demands it | Demanded by an unchanged requirement (bug fix back to the spec) | pending |
| 7 | Skeptic | Architecture finding: the fix's offer guard must predict every refusal of the context entry point (syntax error, generated header, split type kinds, directives) | Change the structure in this run: the formatter states the refusal conditions once, in one predicate the entry point itself uses, and the RH7101 fix's guard calls it (together with the split's own applicability, which `FieldDeclarationSplitTransform` states once); no copy of the checks in the fix | A guard inside the fix would add a second copy of FMT-PIPE-005/006 checks; the structure stays within components the run changes | Internal structure; withholding a no-op fix is demanded by SHARED-FIX-005 | pending |
| 8 | Skeptic | Field in an extension block: fix offered as a no-op (pre-existing) | Adjacent fix: the fix is withheld there, through the applicability predicate of decision 7 | Small, same component, gets a test, SHARED-FIX-005 demands it | Demanded by unchanged SHARED-FIX-005 | pending |
| 9 | Skeptic | Whole-type pipeline per fix | Accepted | Nothing visible depends on it | Internal | — |

## Follow-up candidates

- A `#region` (or other directive) in a combined field's leading trivia blocks the split in the formatter and the RH7101 fix (FMT-STRUCT-023 guard scope); permanent diagnostic. Output-question form in 02-skeptic. — 02-skeptic
- A later split field repeats the line break written behind the type (`private int` / `_y;`), document-level. — 02-skeptic
- Code fixes whose whole edit is a node-level formatting call (RH5111, RH5417) may be offered as no-ops in a document with a syntax error or a generated-file header analyzed under `generated_code = false`; whether their analyzers report there is unverified. — 01-spec-author, 02-skeptic
