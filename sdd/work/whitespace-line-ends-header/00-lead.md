# Lead — whitespace-line-ends-header

- Issue: thoenissen/Reihitsu#1754 — `agent-filed` (goals are proposals). Labels: bug, formatter.
- Slug: `whitespace-line-ends-header`
- Branch: `claude/wonderful-brahmagupta-6qwj6v` (session-designated branch; see decision 1)
- PR: #1768 (draft claim)
- Lane: **bug fix** — the issue reports an observed difference: a whitespace-only line above the first using directive is not taken as the blank line that ends the header (FMT-USING-009), while SHARED-FIX-007 treats such a line as blank. It may need a spec clarification of FMT-USING-009.
- Roles: spec author (clarification of FMT-USING-009, if needed) → skeptic (premise, reachability, spec reading) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Issue manager only if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `lead.md` prescribes `claude/issue-1754-<slug>`, the session's environment mandates a designated branch | Use `claude/wonderful-brahmagupta-6qwj6v` | The session may push only to its designated branch | Branch name is internal | claim commit |
| 2 | Skeptic | FC-015: `GetHeaderSplitIndex` keeps a private measure of "blank line" that drifted from SHARED-BLANK-001 | Fix by reusing the existing shared measure (Core `HasBlankLineAbove` / `SyntaxTriviaUtilities.EndsLine` or whatever the shared one is); no new private copy | The narrow fix would otherwise add another copy of the rule; reuse stays within the components the run touches | SHARED-BLANK-001 and FMT-USING-009/010 already decide the output | pending |
| 3 | Skeptic | `///` comment above the first using (m10, m11; rare, CS1587) | In scope only as far as it falls out of the shared measure; no dedicated code. Test m10 as the skeptic lists | Covered for free by decision 2; rare classes get no special code | SHARED-BLANK-001 decides it | pending |
| 4 | Skeptic | FMT-USING-009's restated definition reads narrower than SHARED-BLANK-001 | Trim to the citation of SHARED-BLANK-001 (spec author, continued once) | One statement of the rule | Spec wording | pending |
| 5 | Skeptic | Issue's "Required output" omits blank line above `// About Linq` | Drop it; follow FMT-USING-010 | Unchanged requirement demands the blank line | Demanded by FMT-USING-010 | — |

## Follow-up candidates

None yet.

## Worklist / next step

- Done: claim, draft PR #1768, `scripts/prepare.sh` green; spec author (01, commit 487f1ec): FMT-USING-009 clarified by citing SHARED-BLANK-001, no output questions; issue's "Required output" contradicts FMT-USING-010 (blank line above `// About Linq` stays).
- Skeptic (02, 0910a41): proceed; class m01–m12 listed; no output question; no follow-ups.
- Next: spec author trim (decision 4), then test implementer.
