# Lead — comment-directive-layout-reports

- Branch: `claude/nice-hypatia-whvcc2` (session-designated branch; replaces the playbook's `claude/batch-1147-…` name). PR: thoenissen/Reihitsu#1753 (draft).
- Slug: `comment-directive-layout-reports`
- Lane: **behavior change** (maintainer chose one run in this lane, 2026-11-15).

## Issues

| Issue | agent-filed | Own lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1147 | yes | bug fix | Layout rules RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5308 measure the token behind a comment that leads the line; P12 / SHARED-COMMENT-011 make the comment the line's first element. Comments add instances (RH5205, RH5302, RH5305, RH5308, RH5301/RH5303, RH7304 comment-led column) and a clarification: a comment-led line whose comment is itself misplaced is still reported (test next to each not-reported input). Triage: bug against SHARED-COMMENT-011, no change doc; the rule's coverage sentence widened as a clarification. SHARED-INIT-001's one-comment limit amended. |
| thoenissen/Reihitsu#1152 | yes | bug fix | RH5204 diagnostic on an indented `///` should start at `///` and exclude the line break (RH5204-005). |
| thoenissen/Reihitsu#1386 | yes | bug fix | Comment-placement rules RH5033–RH5049 skip a `///` or `/** */` that documents nothing; SHARED-COMMENT-001 makes it an ordinary comment. Check every rule in the range. |
| thoenissen/Reihitsu#1387 | yes | bug fix | RH5102, RH5109, RH5111, RH5501–RH5531: a conditional directive in the judged span stops the report (SHARED-DIAG-008); `#region`/`#pragma` stay reported, fix withheld. Audit RH3002-009 and RH5408-003 and list findings in the PR. Triage: no change doc; RH5102-004, RH5109-004, RH5111-005 aligned as clarification. |
| thoenissen/Reihitsu#1594 | yes | bug fix | RH7304: input `/* c */ #region` is invalid C# (CS1040); maintainer triage: the run first establishes a valid input a caller reaches. |
| thoenissen/Reihitsu#1655 | yes | bug fix | RH5111 reports an enum-member `=` gap that holds a comment; SHARED-DIAG-010 says not reported. Triage: the declarator example is refused as unsupported (FMT-UNSUP-004); the enum-member example remains; overlaps #1387. |
| thoenissen/Reihitsu#1736 | yes | behavior change | RH5109 vs FMT-BREAK-012: maintainer triage (2026-11-15) chose A: a parameter list whose parameters all stand on one line is not reported by RH5109, also when that line is not the `(` line. Needs a change doc for RH5109-001/-003. |

## Roles

Behavior-change lane: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Maintainer answers

1. 2026-11-15, lane: run all seven issues as one run in the behavior-change lane (not split off #1736).
2. #1736 (triage on the issue, 2026-11-15): RH5109 does not report a parameter list whose parameters all stand on one line, even when that line is not the line of `(`; whether such a layout should be reported belongs to a different rule.

3. Batch 1 (2026-11-15):
   - OQ1 RH7304 → **A**: comment-led region line (one-line and multi-line comment): directives still reported when misaligned; the fix is offered and indents them to the column where the comment starts.
   - OQ2 RH5111 → **B**: a comment, `#pragma` or `#region` in the wrapped `=` gap (enum member, property, assignment, parameter default) stays reported with no fix. The maintainer: the formatter cannot and should not handle it, and such syntax is unwanted. SHARED-DIAG-010 gets the exception for RH5111; conditional directives stay unreported (SHARED-DIAG-008).
   - OQ3 RH5111 declarator with directive-only gap and comment elsewhere → **B**: reported, no fix (follows from OQ2 B with no extra check). Maintainer asked whether the check is simple / unsupported syntax; answered: under B no copy of FMT-UNSUP-004 is needed.
   - OQ4 RH5109 → **A**: `void N(⏎int a, int b)`, `void N(int a, int b⏎)`, and `)` held by a comment are not reported; rule page Description changes as in change-doc entry 5.
   - OQ5 RH5109 → **A**: a comment or directive keeping the first parameter off the `(` line → not reported. RH5109-001's "as FMT-BREAK-012 decides it" and the change doc's "exactly when FMT-BREAK-012 does" must be corrected.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/nice-hypatia-whvcc2` | The session allows pushes only to that branch | Branch names are internal | claim commit |
| 2 | Spec | Change doc for the clarifications (SHARED-INIT-001, RH7304-001, RH5204-005, SHARED-ATTR-019/020, RH5111-005) | No change doc; clarifications/bug fixes against existing principles P12, P13, P22, SHARED-DIAG-010 | Triage comments on the issues say so | Their outputs are decided by unchanged principles or by output questions 1–2 | 3b1b3d3 |
| 3 | Spec | New IDs vs combined headings for SHARED-ATTR-019/020 | Keep combined headings | Avoids editing ~17 rule specs' `Uses` | Spec structure is internal | 3b1b3d3 |

| 4 | Skeptic | #1147 architecture finding: SHARED-COMMENT-011 decided privately per rule | Change the structure in this run: one shared method for the column of a line's first element (one or more comments, a multi-line comment's start), placed on an existing analyzer utility class (no new type); the failing rules use it and the private copies (RH5201, RH5204, initializer utilities, RH7304) move to it. Implementer reports if it exceeds ~100 production lines (escalation 9) | A narrow fix adds ~9 more copies of one rule; within components the run changes | Outputs are decided by SHARED-COMMENT-011 / SHARED-INIT-001 | |
| 5 | Skeptic | #1147 scope | Add RH5304 and RH5306 (failing members); replace "same shapes without the comment" Done-when with the misplaced-comment variant (still reported) | Skeptic evidence | SHARED-COMMENT-011 decides | |
| 6 | Skeptic | #1594 multi-line comment | Reading (i): compare directives with the column where the comment starts; include it in RH7304-001 (F3 dropped as candidate, absorbed) | Consistent with SHARED-INIT-001; cheap trivia check | Formatter output not reported (SHARED-DIAG-010); misaligned stays reported as on main; fix part asked in OQ1 | |
| 7 | Skeptic | #1386 SHARED-COMMENT-001 list | Clarify: a doc comment followed by a later doc comment in the same leading trivia with an ordinary comment or directive between them documents nothing (compiler CS1587 rule); instance 4 then reports neither comment per unchanged RH5034-001; PR states the difference from the issue | Compiler verdict decides | Unchanged RH5034-001 demands it | |
| 8 | Skeptic | #1387 RH5102/RH5109/RH5111 conditional-directive instances | Already unreported on main: characterization tests only; fix `TargetAttributeListShapeAnalyzerBase` for the 16 list-shape rules | Skeptic evidence | SHARED-DIAG-008 decides | |
| 9 | Skeptic | #1736 issue claim "fix result not a fixed point" | Wrong; not repeated in PR; keep fix-stability test as regression | Skeptic ran the formatter | Internal | |

## Follow-up candidates

- F1 (01-spec-author): RH5102 has the #1736 contradiction for argument lists (`M(⏎1, 2)`).
- F2 (01-spec-author): RH3002-009 keeps a conditional directive inside the parentheses reported, against SHARED-DIAG-008.
- F3 (01-spec-author): dropped — absorbed into #1594 (decision 6).
- F4 (02-skeptic): no rule reports a parenthesized lambda / anonymous method parameter list wrapped only behind `(` after #1736; formatter joins it (P11: new rule or RH5108 gap).
- F5 (02-skeptic): formatter writes a blank line between `=` and an own-line comment (`y =⏎// c⏎2;` → blank line inserted); may already be tracked (FMT-BLANK-011 issue listed on #912) — issue manager dedupes.

## State

- Last step: spec author done (3b1b3d3); lane check confirmed behavior-change.
- Open output questions (from 01-spec-author): OQ1 RH7304 fix for comment-led region line; OQ2 RH5111 comment/#pragma in wrapped gap; #1736 change-doc visible output changes (5 entries, entry 3 comment keeping first parameter off `(` line).
- Skeptic done (829026e). Output-question batch 1 posted to maintainer (OQ1 RH7304 fix, OQ2 RH5111 gap, OQ3 RH5111 declarator directive-only gap, OQ4 #1736 entries 1/2/`)`-comment/5, OQ5 #1736 entry 3).
- Answers recorded. Next: fresh spec author for amendments (03), then test implementer.
- (old) Next: on answers, spec author amends (SHARED-COMMENT-001 clarification, RH7304-001 multi-line, change doc per answers, RH5111-005 per OQ2/3), then test implementer.
