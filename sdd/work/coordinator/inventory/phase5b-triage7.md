# Phase 5b — triage 7 (#966 relevance, #1736–#1742)

## #966 — partly fixed (defect 1 remains)

- (2) fixed by #1735: `GetSortKey` → `GetNameValue` → `StripGlobalQualifier` (`src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs:91,385-406`); SHARED-USING-004/-007 now say `global::` is part of neither root nor sort key, and equal keys keep source order (P5-2 decision, not PR #1669's "unqualified first" tiebreak).
- (1) still open: `UsingDirectiveOrderingSafety.CanSafelyReorder` still ANDs `ContainsPreprocessorDirective` = regex `(^|\r?\n)\s*#` over `usingDirectives.ToFullString()` (`src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs:24,37-48`), so a `#` line inside a `/* */` (incl. a file header on the first using) blocks the formatter and silences RH7201–RH7207; SHARED-USING-009 only names real directives. Confirmed by code reading, no test run.
- PR #1669: open, `mergeable=false` (conflicts with #1735 in the same two Core files + spec); its sort-key half is superseded and its tiebreak contradicts the decided SHARED-USING-007 → close it; redo only the regex removal (+ regression tests from its Core/RH72xx tests) as a small bug-fix change.
- Severity of remaining part: minor (missing reports / no reordering, no damage) → #966 stays deferred (no milestone), retitle to case 1 only.

## Issues #1736–#1742

| Issue | Title (short) | Class | Proposed milestone | Labels add/remove | Run | One-line reason |
|---|---|---|---|---|---|---|
| #1736 | RH5109 reports/splits `N(⏎a, b)` the formatter joins | Minor bug (output question) | none (deferred) | — | — | Caused by #1733 (FMT-BREAK-012 now joins it), but no damage, no ping-pong: fix result is multi-line and stable, formatter output is not reported; reported input is fixable, so not a suppress-only FP. |
| #1737 | RH5107 fix vs formatter place comment behind leading comma differently | Minor bug (output question) | none (deferred) | — | — | Comment kept on its line in both outputs, nothing deleted/compile-relevant; rare shape (RH5035/RH5038 already report it). |
| #1738 | Initializer trailing comma alone on its line at statement column | Minor bug | none (deferred) | — | — | Layout-only on a rare shape, no spec requirement, idempotency not claimed broken; pre-dates #1733 per report; "leave as written + record in spec" is a valid outcome. |
| #1739 | New rule: argument-list commas end their line (RH5107 counterpart) | Extension | vNext (#2) | — (has `enhancement`,`analyzer`) | — | New rule (P11 candidate); overlaps #956 ("formatter only, no rule") and #1159 — needs maintainer's yes/no first. |
| #1740 | Remove dead `OrderUsings` from Reihitsu.Core | Maintenance | Maintenance (#4) | — (has `maintenance`) | — | Verified: only declaration at `UsingDirectiveOrderingUtilities.cs:344`, no non-test caller. |
| #1741 | Analyzer specs refer to SHARED-DIAG-008; RH5101-004 vs RH5102-004 | Maintenance | Maintenance (#4) | — (has `maintenance`,`analyzer`) | — | Spec text only, no behavior change; touches the specs of Q1's #1387 — sequence after Q1. |
| #1742 | Defining part of a partial constructor not analyzed | Minor bug | none (deferred) | — (has `bug`,`analyzer`) | — | Missing diagnostic only (report says so), C# 14-only, no code changed wrongly; related #1338. |

No critical bug → nothing for 1.2.2 / run Q1; none of the seven damages code or is non-idempotent. #1736 is the only one caused by a batch-2 PR (#1733); none by #1734/#1735. Not triaged by reproduction (all assessed from reports + specs/code).

## Doubts

1. **#1736 severity.** `void N(⏎int a, int b)` is a common style for long signatures; since #1733 the CLI joins it while RH5109 reports and splits it (SHARED-DIAG-010 violation introduced by a merged change). **A** minor, deferred (no damage, fix exists). **B** 1.2.2 as a small follow-up run to #1733 (RH5109 spec + guard, likely also RH5101/RH5102 for `M(⏎a, b)` argument lists — check). **Rec: A** — no criterion of "critical" is met; but if the maintainer wants analyzer/CLI consistency for 1.2.2, B fits as one tiny run, not into Q1.
2. **#1739 vs #956.** **A** vNext as a P11 new-rule candidate, ask the maintainer whether #956's "no analyzer rule" stands. **B** close as contradicting #956's decided scope. **Rec: A** — #956's statement is about #956's scope, the new rule is a separate maintainer call.
3. **#966 handling.** **A** keep #966 open (deferred, minor), close PR #1669 with a note that #1735 fixed case 2, narrow the issue to case 1. **B** rebase #1669 down to the regex removal and ship it now. **Rec: A** — minor bug policy defers it; #1669 cannot be salvaged without rewriting it.
