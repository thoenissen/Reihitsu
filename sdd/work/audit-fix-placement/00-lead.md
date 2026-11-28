# Lead — audit-fix-placement

- Issue: thoenissen/Reihitsu#1589 — `agent-filed` (goals are proposals). Labels: analyzer, maintenance, agent-filed. Milestone 1.6.0.
- Slug: `audit-fix-placement`
- Branch: `claude/squad-1589-qmwsl8` (session-designated)
- Draft PR: #2003
- Base at claim: origin/main `c33770023bcb67d6b37f35254db21ef259c8de87`

## Lane

`bug-fix`. The issue asks for an audit of existing code fixes against an unchanged requirement (SHARED-FIX-003: placement condition and directive-pair split), withholding a fix where it is violated. Withholding to meet an unchanged requirement is a bug fix; if every audited fix holds, the diff is tests only, which a bug-fix claim also covers.

Roles: skeptic (premise and reachability, which fixes and input classes the audit covers), test implementer (reproductions / characterization tests per fix), implementer, cleaner, spec checker, auditor, validator. Spec author runs only for a clarification. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications

- Issue comments (maintainer): the run waits for #1971 (narrowed directive guard of the RH72xx using fixes) and #1990 (RH2001 anonymous-member fix case). Both merged into main before the claim (PRs #1996, #1997), so the audit sees them.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, lane, prepare.
- Next: skeptic.
