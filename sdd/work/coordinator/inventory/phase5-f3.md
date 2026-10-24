# Phase 5 — family F3: layout rules RH51xx–RH53xx (statements, line starts, indentation, initializers, comment placement)

25 issues, all open, all unlabeled, all in milestone `SDD`. Every issue is an outcome of the #912 question rounds, filed by the coordinator or squad, in the "Decision / Lane / Instances / Blocked by / Done when" shape, not one of the issue templates. Spec text checked on `main` (no builds or tests run). Every requirement ID cited in these issues still exists in `sdd/specs/requirement-ids.txt`; none cites an ID that #1429 renumbered or removed.

Blockers already closed: #1138 (SHARED-FIX-006/-007 are in `shared/code-fixes.md`), #1139 (SHARED-COMMENT-001 P13 wording and SHARED-COMMENT-011 P12 are in `shared/comment-placement.md`), #1161 (P11 is in `sdd/README.md`), #1380, #1377. Blockers still open: #951, #1243, #1078, #1075.

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle | Duplicate/overlap | Template? | Proposed milestone | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1077 | `/** */` that documents nothing = block comment | — | SDD | behavior change (partly bug) | SHARED-COMMENT-001; RH6001/6005/6009/6011/6023/6024, RH5114, RH5104, RH5033; FMT-SPACE, FMT-BLANK-012 | — | P6, P13 | #1386 (RH5033 instance), #1146 Q3 | no | vNext | Only the comment-placement rules use SHARED-COMMENT-001; spacing rules and the formatter have no such requirement |
| 1097 | Comment rules: chain anchor model, `is`/`as` | — | SDD | behavior change | SHARED-COMMENT-004/-007/-008/-009, RH5049-001; RH5035/38/39/43/44/46/47/49 | — | (round 4) | #1098 (same rules) | no | vNext | Explicit change doc; flips two pinned tests |
| 1098 | A construct owns only what is inside its braces/arms | — | SDD | behavior change | SHARED-COMMENT-004/-006, FMT-BREAK-015; region rules; LineBreakPhase | — | (round 4) | #1097 | no | vNext | Analyzer and formatter share one narrowed table; needs a change doc |
| 1140 | Line-break/alignment fixes change only their code | — | SDD | bug | SHARED-FIX-006, SHARED-CHAIN-009, FMT-PIPE-010; RH5101/02/10/11/12/13, RH5201/04/06 | (#1138 closed) | P9, P14a | #1144, #1393; #969 | partly | bug-pool | Violates SHARED-FIX-006; RH5204 `else` fix raises the diagnostic count from 4 to 8 |
| 1144 | Fix output = formatter layout (RH5101/02/05/03) | — | SDD | bug | SHARED-FIX-007, FMT-PIPE-010, FMT-INDENT-012; RH5101/02/03/05 (+RH5417, RH3202 in comments) | (#1138 closed) | P10, P14b | #1140, #1145 Q4, #1393 | partly | bug-pool | Violates SHARED-FIX-007; also a FMT-PIPE-010 clarification |
| 1145 | RH5103 fix behind embedded/labeled stmts, one-line blocks | — | SDD | bug | RH5103-003/-004, SHARED-FIX-007, FMT-INDENT-003 | (#1138 closed) | P10, guard scope | #1156 (same rule) | partly | bug-pool | Q2 violates RH5103-003/-004 (whitespace-only gap); Q4 violates SHARED-FIX-007 |
| 1146 | RH5104: literal-safe fix, comment kinds, interpolations | — | SDD | bug + behavior change | RH5104-001/-002/-004/-005, SHARED-FIX-003 | (#1139 closed); coordinate with #1075, #951 | P1, P5, P13 | #1393 (RH5104-005), #1077 | partly | 1.2.0 (Q1, Q4); vNext (Q3, Q5, Q6) | CRITICAL Q1: fix writes a comment into a verbatim/raw string |
| 1147 | Comment-led line is not reported (RH5110, 52xx, 53xx) | — | SDD | bug | SHARED-COMMENT-011, SHARED-INIT-001; RH5110, RH5203/05/06, RH5301–05/08 | (#1139 closed) | P12 (+P7) | #1271–#1273 (same RH53xx rules) | partly | bug-pool | Violates SHARED-COMMENT-011; SHARED-INIT-001's one-comment limit must be amended |
| 1150 | Delegating fixes withheld when formatter returns input | — | SDD | bug | SHARED-FIX-005, FMT-PIPE-005/-006, FMT-STRUCT-003 | — | P7 | #1076 (other cause) | partly | bug-pool | Violates SHARED-FIX-005; about 39 providers |
| 1152 | RH5204 span on indented `///` starts at `///` | — | SDD | bug | RH5204-005 | — | — | #1157 | partly | bug-pool | Violates RH5204-005; span starts after `///` and includes the EOL |
| 1154 | RH5110 checks record structs | — | SDD | behavior change | RH5110-001, FMT-BREAK-023 | — | P11 | — | partly | vNext | Rule's own gap; RH5110-001 lists no record struct |
| 1155 | RH5111 checks nested assignments | — | SDD | behavior change | RH5111-001/-002/-004, FMT-BREAK-016 | — | P11, P10 | #1140 (RH5111 scope) | partly | vNext | Rule's own gap |
| 1156 | RH5103: predecessor's last line; top-level statements | — | SDD | behavior change | RH5103-001, FMT-BREAK-006 | — | P11 | #1145 | partly | vNext | RH5103-001 compares start lines; FMT-BREAK-006 rewording |
| 1157 | RH5204 checks lines not led by a keyword | — | SDD | behavior change | RH5204-001/-002, FMT-INDENT-002 | #951 (open) | P11 | #1152 | partly | vNext | Rule's own gap; blocked by #951 |
| 1158 | RH5108: primary ctor, lambda, anon method, extension params | — | SDD | behavior change | RH5108-001, FMT-BREAK-012 | — | P11 | #1144 | partly | vNext | Rule's own gap |
| 1160 | New rule: a comment does not touch code | — | SDD | behavior change | new RH6### rule; FMT-SPACE-013 | #1078 (open, for the analyzer) | P13 | #1078 | partly | vNext | New rule plus a new formatter behavior |
| 1271 | RH5301/RH5304 nested initializers, brace-line members | — | SDD | behavior change | RH5301-001/-002, RH5304-001/-002, FMT-BREAK-021, FMT-INDENT-023 | (#1161 closed) | P11 | #1244 (probable cause), #1147 | partly | vNext | Rule's own gap |
| 1272 | RH5305/RH5306 check whole collection expr / list pattern | — | SDD | behavior change | RH5305-002, RH5306-001/-002/-004, FMT-INDENT-025, FMT-BREAK-013/-022 | (#1161, #1139 closed) | P11, P13 | #1147 | partly | vNext | Issue says "and bug fix", but no instance breaks a current requirement; RH5306 does not use SHARED-COMMENT-001 |
| 1273 | RH5302/RH5308 report an operator alone on its line | — | SDD | behavior change | RH5302-002, RH5308-002, FMT-BREAK-017/-018 | #1243 (open) | P11 | #1274, #1147 | partly | vNext | Rule's own gap; needs the #1243 formatter join |
| 1274 | RH5308 fix converges or is withheld | — | SDD | bug | RH5308-001/-004, SHARED-FIX-001, SHARED-FIX-005 | — | P7 | #1273 | partly | bug-pool | Violates SHARED-FIX-001; the fix does not converge |
| 1277 | Fix guards: RH5307 reported list, RH5401 own lines | — | SDD | behavior change + clarification | RH5307-001/-004, RH5401-002/-003/-004, FMT-BREAK-014 | — | guard scope | — | partly | vNext | RH5307-004 and RH5401-003/-004 limit the fix today; issue calls it a bug |
| 1386 | RH5033–RH5049 treat a stray `///`/`/** */` as ordinary | — | SDD | bug | SHARED-COMMENT-001, RH5033-001, RH5034, RH5038-001 | — | P13 | #1077 (RH5033 instance), #1341 | partly | bug-pool | Violates SHARED-COMMENT-001, which these rules list under Uses |
| 1387 | Conditional directive in a list: not reported (RH5102, 5109, 5111, 55xx) | — | SDD | bug | SHARED-DIAG-008; RH5102-004, RH5109-004, RH5111-005, SHARED-ATTR-011/-019/-020, RH5511/14/24 | — | P22 | #1337 | partly | bug-pool | Violates SHARED-DIAG-008; the rule specs still say "still reported" |
| 1393 | RH5104 no blank line before moved comment; RH5417 brace | — | SDD | bug (spec conflict) | SHARED-FIX-006/-007, RH5104-005, RH5417 | — | P14a, P14b | #1146, #1261, #1140, #1144 | partly | bug-pool | Violates SHARED-FIX-006, but RH5104-005 requires that blank line today |
| 1394 | Shared-spec clarifications from round 9 | — | SDD | clarification | SHARED-COMMENT-011, RH5111-001/-002/-005, SHARED-FIX-007, SHARED-FIX-008 | coordinate with #1380 (closed), #1377 (closed) | P12, P14b | — | no | Maintenance | Spec text only; title says "(clarification)" |

No title carries "(behavior change)". #1394's title carries "(clarification)".

## 2. Wrong bug labels

No issue in F3 has a `bug` label. Some issues call themselves "bug fix" in their Lane line, but no requirement on `main` states the wanted behavior:

- #1272: the Lane says "behavior change and bug fix", but every instance needs a requirement to change (RH5305-002, RH5306-001). The `///` bullet is not a bug: RH5306 does not list SHARED-COMMENT-001 under Uses.
- #1277: the Lane says "bug fix". But RH5307-004 limits the fix to an element access that is not nested in another bracketed list, and RH5401-003/-004 limit it to properties without attribute lists. Widening the fix changes those requirements.
- #1146 Q3, Q5 and Q6, described as bugs:
  - Q3: RH5104-001 excludes comments that start with `///`.
  - Q5: RH5104-001 exempts the trivia of every token of a multi-line interpolated string.
  - Q6: RH5104-005 names only `//` lines.
  - Today's behavior matches the spec text in all three.
- #1393, RH5104 part: RH5104-005 explicitly requires the blank line. The fix follows the rule spec but contradicts SHARED-FIX-006.
- #1393, RH5417 part: SHARED-FIX-007's shared-line bullet covers another member, not the type's closing brace.
- #1077 (no Lane given; the decision says the instances are bugs): only the comment-placement rules list SHARED-COMMENT-001 under Uses. RH6001/6005/6009/6011/6023, RH5114, RH5104 and the formatter have no requirement that makes a stray `/** */` behave like `/* */`.

## 3. Critical candidates

- #1146 Q1 (RH5104 fix): `@"a⏎b" + /* c */ x;` → the fix inserts `/* c */` inside the verbatim string, which changes the string's value (program meaning). Raw strings are affected the same way. RH5104-004 and SHARED-FIX-003 both forbid it.
- #1146 Q4 (RH5104 fix, weaker): `/* a */ /* b */` → `/* b */⏎/* a */`, which reorders user comments (SHARED-FIX-003).
- Close to critical, not in the critical classes: #1140 RH5204 `else` fix makes the code worse (the diagnostic count rises from 4 to 8). #1147, #1274 and #1150 offer no-op or non-converging fixes.

## 4. Real bugs

- #1140, SHARED-FIX-006: the RH5110 fix on `class C<T> where T : class` with member `int  x  =  1;` gives `int x = 1;` as well; spec: only the clause moves. The RH5101 fix on `M(⏎1+2,⏎3)` gives `M(1 + 2,⏎3)`; spec: `1+2` stays.
- #1144, SHARED-FIX-007: the RH5101 fix on `M(⏎"""⏎a⏎"""⏎,2)` puts the raw string at column 9; document formatting puts it at column 10, and RH5202 then reports the fix's output. The RH5203 fix on `M(1,⏎ // two⏎ 2);` puts `// two` at column 0; spec: aligned with the argument.
- #1145:
  - Q2, RH5103-003/-004: `if (a) M(); M();` → reported with no fix; spec: fix offered (the gap is whitespace only).
  - Q4, SHARED-FIX-007: `void N() { M(); M(); }` → the moved `M();` lands at the method's column; spec: the block's statement column.
- #1146:
  - Q1, SHARED-FIX-003 and RH5104-004: see Critical.
  - Q4, RH5104-001: `/* a */ /* b */` on its own line → both reported; spec: not reported (no token on the line).
- #1147, SHARED-COMMENT-011: `void N<T>()⏎    /* c */ where T : class` → RH5110 reports it; spec: not reported. The reverse also fails: a misplaced comment at column 0 is not reported by RH5110/5203/5205/5206/5302/5305/5306/5308; spec: reported.
- #1150, SHARED-FIX-005: `class C { void M(int a) { a=1; } void N( }` → the RH6023 fix is offered and changes nothing; spec: not offered.
- #1152, RH5204-005: the misindented `      /// <summary>M</summary>` gets a span that starts after `///` and includes the EOL; spec: the span is on the comment.
- #1274, SHARED-FIX-001: `a ? // c⏎ 1⏎ : 2` → after the RH5308 fix, `?` is still behind `a` and the diagnostic remains; spec: one application clears it, or the fix is withheld.
- #1386, SHARED-COMMENT-001: `[Obsolete]⏎/// <summary>M</summary>⏎void M()` → RH5033 does not report it; spec: reported like `// M`.
- #1387, SHARED-DIAG-008: `M(a, b,⏎#if DEBUG⏎c,⏎#endif⏎d);` → RH5102 reports it and withholds the fix; spec: not reported.
- #1393, SHARED-FIX-006 (conflicts with RH5104-005): `M();⏎M(); // second` → `M();⏎⏎// second⏎M();`; spec: `M();⏎// second⏎M();`.

## 5. Groups

- **G1 — comment kinds alike (P6/P13), analyzer bug lane:** #1386 plus the RH5033 instance of #1077. #1077's spacing-rule and formatter instances need a change doc (vNext). #1146 Q3/Q6 and the `///` bullet of #1272 are the same principle in other rules. #1341 (RH83xx) is the sibling in another family.
- **G2 — comment region-rule model (one change doc, `CommentRegionAnalyzerBase`):** #1097 and #1098. #1098 also changes FMT-BREAK-015 (formatter lane).
- **G3 — fix scope and layout (SHARED-FIX-006/-007), code-fix bug lane:** #1140, #1144, #1145 Q4, #1393. These touch the same node-level formatter entry points (`FormatNodeInDocumentAsync`, `AdjustNodeIndentation`).
- **G4 — comment-led line (P12), analyzer bug lane:** #1147 alone. It touches the same RH53xx analyzers as G5, so order it before G5.
- **G5 — initializer, collection and operator layout (P11), behavior-change lane:** #1271, #1272 and #1273. #1274 is the RH5308 bug fix and can ride with #1273 if the lane allows a mixed run, otherwise it goes first. #1244 can close with #1271.
- **G6 — RH5104 fix:** #1146 (Q1/Q4 bug; Q3/Q5/Q6 change) and #1393 (RH5104-005). The `GetStringLineIndices` helper is shared with #1075, and #951 constrains it for RH5204.
- **G7 — RH5103:** #1145 (bug) and #1156 (change). Different lanes, same code.
- **G8 — RH5204:** #1152 (bug, unblocked) and #1157 (change, blocked by #951).
- **G9 — P11 own-gap extensions on RH51xx (one change doc possible):** #1154, #1155, #1156, #1158 (and #1157 once #951 is done).
- **Singles:**
  - #1150: cross-cutting fix helper; next to #1076.
  - #1160: new rule; after #1078.
  - #1277: fix guards.
  - #1387: P22, with #1337 in another family.
  - #1394: spec only.
- **Duplicates:** no full duplicates. The RH5033 instance in #1077 comment 4 is the same finding as #1386.

## 6. Doubts

- #1393 and #1140: rule specs contradict the shared requirement. RH5104-005 requires the blank line that SHARED-FIX-006 forbids. RH5111-004 and RH5401-003 say "format as the formatter formats it in place", while SHARED-FIX-006 forbids formatting the containing member or statement. Decide whether this counts as a bug (the shared requirement wins) or a requirement change.
- #1387: SHARED-DIAG-008 applies to every rule, but RH5102-004, RH5109-004 and RH5111-005 still say "the diagnostic is still reported" for a directive. Treated as a bug; the rule texts need aligning in the same run.
- #1077: the maintainer's decision ("no change doc; the instances are bugs") predates #1139, which put P13 only into SHARED-COMMENT-001 for the comment-placement rules. Either add a cross-cutting requirement (clarification, then the instances are bugs) or treat the spacing and formatter instances as an extension. Classified here as behavior change.
- #1146: mixed issue. Consider splitting Q1/Q4 (critical bug, 1.2.0) from Q3/Q5/Q6 (RH5104-001/-005 changes, vNext).
- #1277: a guard-scope widening that changes explicit fix-restriction requirements (RH5307-004, RH5401-003/-004). Classified as behavior change; it could be argued as a bug under the AGENTS.md "Code-fix coverage" guard-scope rule.
- #1393, RH5417 part: SHARED-FIX-007's shared-line bullet names members only. Extending it to the type's `}` is a requirement change unless it is read as covered.
- #1157 and #1146 Q5: both wait for #951, which is in Triage. Check that it is scheduled before them.
- #1273: blocked by the open formatter bug #1243 (another family).
