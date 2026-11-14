# Phase 5b — triage batch 6 (#1720–#1731)

Read-only triage (sections 1–3 of `.claude/commands/triage.md`). Nothing reproduced; severity from the reports and the open-issue overlap. Open milestones: vNext (#2), Maintenance (#4), 1.2.1 (#12, closing), 1.2.2 (#13), 1.3.0, 1.4.0, 1.5.0. No issue in this batch is critical, so nothing goes to 1.2.2 and no pending run grows.

| Issue | Title (short) | Class | Proposed milestone | Labels to add/remove | Run | One-line reason |
|---|---|---|---|---|---|---|
| #1720 | RH5603: final line break behind a directive or `///` (OQ) | Minor bug | none (deferred) — or close as duplicate of #1082 (doubt 1) | — | — | The fix clears the report and no code is damaged; its recommended option A contradicts the maintainer's decision in #1082 (vNext: formatter removes the line break behind a trailing directive, RH5603 keeps reporting). |
| #1721 | All-lone-`\r` document ends with mixed line endings (unconfirmed) | Minor bug | none (deferred) | — | — | Unconfirmed, classic-Mac-only input, literal contents kept; at worst a cosmetic line-ending mix, no damage. |
| #1722 | `RawStringLineShifts` uses Core `IsInterpolationBrace` | Maintenance | Maintenance | — | — | Internal single-owner refactor, no output change. |
| #1723 | Playground asks a public syntax-error query | Maintenance | Maintenance | — | — | Internal dedup between Playground and formatter, no user-visible change. |
| #1724 | `trace.sh` reports the hiding-carrier skip | Maintenance | Maintenance | — | — | Repository tooling message only. |
| #1725 | Squad/AGENTS.md: "new type" escalation not limited to base classes | Maintenance | Maintenance | — | — | Process/docs wording; maintainer confirms the wording at merge. |
| #1726 | Comment/directive behind a binary operator is unsupported | Extension | vNext | — | — | Maintainer-decided unsupported-shape declaration, own change; visible CLI/fix change. |
| #1727 | No blank line inside an expression before a comment above wrapped `?`/`:` (OQ) | Extension | vNext — or close as covered by #1148 (doubt 2) | — | — | Behavior change of FMT-BLANK-011/RH5020; its option A is already decided by #1148 (FMT-BLANK-011 only between statements and members, RH5020 to follow). |
| #1728 | Directive inside a multi-declarator field is unsupported | Extension | vNext | — | — | Maintainer-decided unsupported-shape declaration, own change; distinct from #1586 (fields inside `#region`). |
| #1729 | Directive between embedded-statement header and unbraced body is unsupported | Extension | vNext | — | — | Maintainer-decided unsupported-shape declaration; it reverses #1537's `if`/`else` half (doubt 3). |
| #1730 | RH5401 reports a property with a comment behind `=>` (OQ) | Minor bug | none (deferred) | — | — | Rare shape (comment behind `=>`), a report without fix, no damage; output question on a non-critical bug is deferred. Overlaps #1731 (same input) and #1277 (RH5401 guard scope). |
| #1731 | Continuation behind `//` at the arrow lands in the member column | Minor bug | none (deferred) | — | — | Layout outcome on a rare shape, idempotent, no code damaged; same input as #1730, near #1515 (vNext, comment layout at the arrow on conversion). |

Labels: every issue already carries a group label and its surface label; nothing to add or remove (`agent-filed` stays).

## Doubts

**1 · #1720 vs #1082.** #1082 (vNext) records the maintainer's decision for the directive case: the formatter removes the final line break behind a trailing directive and RH5603 keeps reporting — #1720's option B. #1720 recommends A, which reverses that decision.
- **A** — Close #1720 as a duplicate of #1082, adding the `///` case to #1082 by comment (issue-manager, later).
- **B** — Keep #1720 open and deferred (no milestone) as a separate output question.
- **Recommendation: A** — the directive half is already decided; only the `///` tail is new and fits #1082's scope (FMT-CLEAN-005 without exception).

**2 · #1727 vs #1148.** #1148 (vNext) already holds the maintainer's decision that FMT-BLANK-011 applies only between statements and members, with RH5020 to be checked for the same scope; a comment above a wrapped `?`/`:` is inside an expression, so #1727's option A follows from it.
- **A** — Close #1727 as covered by #1148, adding its input as a test case there.
- **B** — Keep #1727 in vNext as a separate output question.
- **Recommendation: A** — re-asking a decided output question invites a contradictory answer.

**3 · #1729 vs #1537.** #1537 (vNext) asks to brace `if`/`else` bodies (and switch sections, FMT-SWITCH-010) whose trivia carries a movable directive; #1729 declares the `if`/`else` shape unsupported, the opposite decision.
- **A** — Milestone #1729 to vNext and narrow #1537 to switch sections only (or close it if the maintainer wants the same decision there).
- **B** — Milestone #1729 to vNext and leave #1537 as is, reconciled when either is implemented (as #1729 itself says).
- **Recommendation: A** — two contradictory open vNext requests for the same input will mislead the next run.

**4 · #1730/#1731 — comment behind `=>`.** Same input (`int A => // c⏎    1;`) produces two minor bugs. Under *Special cases do not grow the source* this shape could be declared unsupported like #1726, which would settle both.
- **A** — Defer both without milestone as classified; note the unsupported-shape option for when they are picked up.
- **B** — Raise the declaration as an output question to the maintainer now (would become a vNext extension like #1726).
- **Recommendation: A** — both are minor and the rule for minor bugs is deferral; the declaration option must still be named, which the note does.
