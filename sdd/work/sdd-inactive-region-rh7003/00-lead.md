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

## Follow-up candidates

(none yet)

## State

- Last step: claim, lane, prepare.sh done.
- Next: spec author.
