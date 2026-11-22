# Lead — sdd-inactive-region-rh7003

- Issues: thoenissen/Reihitsu#1240 (agent-filed), thoenissen/Reihitsu#1591 (agent-filed)
- Branch: claude/squad-1240-1591-4t8962; PR #1938 (draft)
- Slug: sdd-inactive-region-rh7003
- Lane: docs. Reason: #1240 narrows a non-goal in `sdd/specs/shared/diagnostics.md` (text only, no behavior change); #1591 removes the "Exceptions" section of `documentation/rules/RH7003.md` (text only). Both change only text the lane-check table classifies as text.
- Roles: spec author (writes the texts), spec checker. Skipped: skeptic, test implementer, implementer, cleaner, auditor (docs lane); validator only if a compiled file changed.
- Clarifications: none from the maintainer. #1240 comments: maintainer principle P22 gives the wording — a construct containing `#if`/`#elif`/`#else`/`#endif` or excluded code is not reported, but `#region`/`#endregion`/`#pragma` inside it do not stop the report; shared requirement is SHARED-DIAG-008 (#1260). The RH7401–RH7412 base classes treat region directives in inactive `#if` code the same way.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Does the rewritten RH7003 page (Exceptions folded into Description, nested-namespace case added) need an output question? | No | The page's statements are each demanded by unchanged requirements RH7003-001/-002/-003/-005 and the rule-page conventions in AGENTS.md | No requirement changes; the page only restates specified behavior | e0a2683 |
| 2 | spec author | #1240 lists RH7309 among reporting rules, but its spec excludes such regions | Non-goal names no rule and defers to each rule's spec | agent-filed goal adjusted on evidence; purpose (non-goal no longer contradicts region rules) met | Spec text only, no behavior change | e0a2683 |

## Follow-up candidates

(none yet)

## State

- Last step: spec author done (e0a2683), lane check docs confirmed, no output questions.
- Next: spec checker.
