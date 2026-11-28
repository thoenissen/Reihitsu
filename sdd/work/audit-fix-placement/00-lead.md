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
| 1 | skeptic | Guard precision for the new brace-split guards | Skeptic option A: withhold when the braced span holds a `#pragma warning`/`#nullable`/`#line` directive while a same-kind directive stands outside it; for statement braces also an unbalanced conditional or region directive | Cheap kind-and-position check, no pragma-pair modelling (FC-044); keeps the existing lone-disable tests | Withholding a fix for a rare class (AGENTS.md, Code-fix coverage) | |
| 2 | skeptic | Where the expression-body guard lives | Fix side only (`ExpressionBodyToBlockCodeFixProviderBase` registration), not `BlocksRewrite` | `BlocksRewrite` also drives reporting (SHARED-BODY-001) and the formatter (FMT-STRUCT-007) | Diagnostics and formatter output stay unchanged | |
| 3 | skeptic | RH7310: comment directly above an empty region whose `#endregion` is directly followed by code | Withhold the fix (skeptic option 2) | SHARED-FIX-003's reason (the comment would describe other code) applies; rare class; cheap check | Withholding for a rare class | |
| 4 | skeptic | Architecture finding FC-005: each brace-adding fix guards its own subset of directive kinds | One shared Core check ("a directive inside the span with a same-kind or partner directive outside it") used by RH3202–RH3208 and RH5405–RH5407 (and the formatter guard of decision 5); RH7004 left as is | Two or three consumers; avoids a second statement of the rule; small, no new public surface beyond a helper method | Internal structure | |
| 5 | skeptic | Formatter `ControlFlowBraceTransform` braces an if/else body holding a conditional group that ends behind the body (other build loses `}`) | Adjacent fix: leave the construct as written (guard), with a regression test | Defect against unchanged FMT-STRUCT-024 / FMT-PIPE-003; few lines; counterpart of the statement-brace fix; lane stays bug-fix | Brings the formatter back to unchanged requirements; leaving a construct as written for a rare class | |
| 6 | skeptic | Issue goals (agent-filed) | Drop RH3204 (wraps nothing); "Done when" read under Coverage first (existing tests count; no test for unreachable RH5405); out-of-scope families as listed in 01-skeptic.md | Evidence in 01-skeptic.md | No visible output involved | |
| 7 | skeptic | SHARED-BRACE-003 sentence "a directive between two lines of a body … does not withhold the fix" contradicts SHARED-FIX-003 | Spec author clarification: narrow it to directives whose effect does not cross a new brace | Two requirements contradict; SHARED-FIX-003 is the principle the issue enforces | Withholding for a rare class; spec wording internal | |

## Follow-up candidates

(none yet)

## State

- Skeptic done: `01-skeptic.md` (commit 2a95077). Verdict: proceed narrower + output question.
- Open: output question 1 (RH7201–RH7207 comment above first using) and escalation 10 (declare the unbraced-body directive-pair shape unsupported) asked in one batch; waiting for the maintainer.
- Next after answers: spec author (SHARED-BRACE-003 clarification, SHARED-BODY wording of new withheld case, SHARED-FIX-003 sentence if Q1 = B), then test implementer.
