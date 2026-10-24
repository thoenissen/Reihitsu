# Phase 5 — family F4 inventory: layout RH50xx/RH54xx–RH56xx and spacing RH6xxx

25 issues, read 2026-10-24 (body, labels, milestone, all comments). Spec text checked on `main` (after #1429). No builds or tests were run.

Blockers checked against the open-issue index: #1068, #1069, #1138, #1260, #1380 are closed (SHARED-DIAG-003, SHARED-FIX-003 to -008 and FMT-BLANK-016 exist on main). #1240 is still open.

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle | Duplicate/overlap | Template? | Proposed milestone | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 946 | RH5005/RH5006 fix never converges on `await using`/`await foreach` | — | 1.2.0 | bug | SHARED-STMT-004, SHARED-FIX-001; RH5005, RH5006 | — | — | G1 (#972, #1096) | no | bug-pool | Violates SHARED-STMT-004/FIX-001: blank line goes between `await` and `using` |
| 972 | Statement blank-line fix inserts two blank lines with a same-line block comment | bug, analyzer | Triage | bug | SHARED-STMT-004, SHARED-BLANK-001; RH5001–RH5019 base | — | — | G1 | yes (analyzer bug) | bug-pool | Violates SHARED-STMT-004 "exactly one blank line"; RH5002 non-goal already removed |
| 1033 | Attribute placement fixes: missing-brace guard only for enclosing braces | enhancement, analyzer | vNext | behavior change | SHARED-ATTR-012; RH5501–RH5530 fix base | — | — | G5 | yes (analyzer feature) | vNext | Guard narrowing; ATTR-012 is silent on the owner's own brace |
| 1034 | Decide spec ownership of `AttributeTargetUtilities` | enhancement | Maintenance | internal | attribute-targets.md header, specs/README.md | — | — | — | no | Maintenance | Already done on main: listed as code owner in both places; close |
| 1074 | Spacing rules: whitespace around literals / in holes is code | — | SDD | bug | SHARED-DIAG-003; RH5601-001, RH5602-001, RH6009-001, RH6010-001 | — | P5 | G3 (#1133, #1075 comment) | no | bug-pool | DIAG-003 vs RH5601-001/RH5602-001, which exclude these cases explicitly (doubt) |
| 1075 | Blank-line rules: lines inside literals/comments neither checked nor edited | — | SDD | bug | SHARED-DIAG-003, SHARED-FIX-003, SHARED-FIX-001, SHARED-BLANK-001; RH5011, RH5023, RH5025, RH5027, RH5030 | — | P1, P5 | G2, G3 | no | 1.2.0 | CRITICAL: RH5025/RH5027 fixes delete lines inside string literals (value changes) |
| 1076 | Blank-line fixes: converge, never undo another rule, never no-op | — | SDD | bug | SHARED-FIX-001, -004, -005, SHARED-BLANK-006; RH5011, RH5022–RH5027, RH5032 | — | P7 | G2; RH5025/RH5030 instance = #1396 | no | bug-pool | Violates FIX-001/004/005; RH5024-001 already exempts `#region` line (verify code) |
| 1078 | Spacing rules: do not report whitespace touching a comment | — | SDD | behavior change | RH6003, RH6006, RH6007, RH6011–RH6022 specs; SHARED-WS-002, SHARED-GAP-001, FMT-SPACE-013 | — | — | G4; #1160 (F3) owns the missing-space gap | no | vNext | Title-less "(behavior change)" stated in body; change doc first |
| 1079 | Spacing: tab on later `///` lines, `# region`, spaces after `operator` (+RH5603, RH6021, RH7302) | — | SDD | behavior change (+1 bug) | RH5601-001, RH6004-002, RH6005-001/-002, RH6021-001/-002, RH7302-001, RH5603-001/-004, SHARED-FIX-001 | — | P7 | G4 | no | vNext (RH5603 part: bug-pool) | Body says bugs; specs exclude most of these explicitly — see section 2 |
| 1080 | RH5022: report blank line after `{` with trailing comment | — | SDD | behavior change | RH5022-001, FMT-BLANK-007, SHARED-BLANK-005 | — | P11 (own gap) | G2 | no | vNext | RH5022-001 covers only lines that *end* with `{`; not a bug |
| 1081 | Clarifications: RH5025 sibling braces, lone `\r` in FMT-PIPE-007 | — | SDD | clarification | RH5025-001, FMT-PIPE-007 | — | — | G2 | no | Maintenance | Item 2 already in FMT-PIPE-007 (U+2028/U+0085 comment instance still open); item 1 open |
| 1093 | Attribute separate-line rules: placement follows the attached declaration | — | SDD | behavior change (+ formatter bug) | SHARED-ATTR-002/-003/-006/-013/-014, RH5523-001, RH5530-001, RH5531-006, list-shape -001 reqs; FMT-BREAK-027 | — | — | G5 | no | vNext | ATTR-002 says specifier decides; formatter joining `[param: A] set;` violates FMT-BREAK-027 |
| 1094 | RH5513 fix: break before the list, indent correctly | — | SDD | bug | SHARED-ATTR-004/-008/-009, SHARED-FIX-007 | — | P10 | G5 | no | bug-pool | Q2 violates FIX-007 shared line; Q3/Q4 outside ATTR-008/009 coverage clauses (doubt) |
| 1095 | Deconstruction counts as local declaration (RH5029, RH5021, formatter) | — | SDD | behavior change | RH5029-001, RH5021-001/-002, FMT-BLANK-002 | — | — | — | no | vNext | Change doc first; specs silent on deconstruction |
| 1096 | Statement blank-line fixes: `case … when` indentation, blank line after `///` | — | SDD | bug | SHARED-STMT-004, SHARED-BLANK-001, RH5011-001 | — | — | G1 | no | bug-pool | Q1 violates STMT-004 "indented like the statement it follows"; Q2 RH5011-001/BLANK-001 |
| 1133 | RH5601 reports closing-line tab of interpolated raw string; fix breaks code | bug, analyzer | 1.2.0 | bug | RH5601-002, SHARED-DIAG-003, SHARED-FIX-003 | — | P5 | G3; #951 (F2), #1074 | partly (analyzer bug) | 1.2.0 | CRITICAL: fixed code no longer compiles; violates RH5601-002 |
| 1141 | RH6023/RH6024 fixes rewrite only the gaps next to the operator | — | SDD | bug | SHARED-FIX-006, SHARED-OPSPACE-004 | — | P9 | — | partly | bug-pool | Violates SHARED-FIX-006 (formats whole expression) |
| 1142 | Attribute-list split fixes change only the list, keep line ending | — | SDD | bug | SHARED-ATTR-017, SHARED-FIX-006, SHARED-FIX-008, RH55xx-003 reqs | — | P9 | G5 | partly | bug-pool | Violates ATTR-017 "nothing else changes" and FIX-008 (LF drift) |
| 1159 | New rule: attribute argument lists one line or one per line | — | SDD | behavior change | FMT-BREAK-012; new RH51## rule(s) | — | P11, P10 | #1144 (F3) | partly (feature) | vNext | New rule; change doc picks ID(s) |
| 1261 | Fixes of RH5405–RH5407, RH5412–RH5417 edit/duplicate comment and directive text | — | SDD | bug | SHARED-FIX-003, SHARED-EMPTY-003, RH5417-003, SHARED-BRACE-002 | — | P1 | G6; #1416 same fixes | no | 1.2.0 | CRITICAL candidate: duplicates a user comment; region text rewritten |
| 1275 | Empty-type rules report `};`, directives by kind | — | SDD | behavior change | SHARED-EMPTY-001/-004, FMT-STRUCT-017, SHARED-TRAIL-002, SHARED-DIAG-008 | #1240 (open); #1260 closed | P11, P22 | G6 | no | vNext | Comment: `#if DEBUG⏎#endif` body is reported today, so P22 part also changes |
| 1276 | RH5404: every one-line type body; extension block at keyword (+ formatter) | — | SDD | behavior change (+ bug) | RH5404-001/-002/-003, SHARED-DIAG-007, FMT-BREAK-002/-007 | #1260 closed | P11, P21 | G6 | no | vNext | `Location.None` violates SHARED-DIAG-007 (bug part); rest widens RH5404-001 |
| 1385 | Required blank line goes after directives (formatter, RH5002/RH5008) | — | SDD | behavior change | FMT-BLANK-001/-004/-005/-006/-016, SHARED-STMT-004, RH5011-002 | #1380 closed | P37 | G1 | no | vNext | FMT-BLANK-016 already on main: formatter part may now be a bug (doubt) |
| 1396 | RH5030 exempts `}` followed by a block statement | — | SDD | behavior change | RH5030-001, RH5025-001, SHARED-BLANK-003, FMT-BLANK-008, SHARED-FIX-004 | — | — | G2; resolves #1076's RH5025/RH5030 cycle | no | vNext | Title says "(behavior change)"; ends a FIX-004 cycle |
| 1416 | Brace fixes RH5405–RH5407 keep wrapped statement's layout | — | SDD | behavior change | SHARED-FIX-007, SHARED-BRACE-002 | — | P26 | G6; #1261, #1388 (F8) | no | vNext | SHARED-FIX-007 already requires it; SHARED-BRACE-002 contradicts (doubt) |

## 2. Wrong bug labels

Only #972 and #1133 carry the `bug` label; both are real bugs. Issues without the label whose body calls them a "bug fix" but are not bugs under the rule:

- #1080 — RH5022-001 covers only a line that *ends* with `{`; `{ // note` is outside it. P11 own gap, so a behavior change. SHARED-BLANK-005 is about region directives and does not apply.
- #1079 — only the RH5603 instance is a bug (SHARED-FIX-001). RH5601-001 explicitly excludes a tab on later `///` lines. RH6004-002 and RH7302-001 cover only `#region` written without whitespace after `#`. RH6005-001/-002 and RH6021-001/-002 leave two spaces unspecified. All of these are widenings.
- #1074 (partly) — RH5601-001 (tab before a literal) and RH5602-001 (lines an interpolated string spans, holes included) state today's behavior explicitly. It contradicts SHARED-DIAG-003 only at the shared level. The RH6009/RH6010 instance is a clean bug against RH6009-001/RH6010-001.
- #1094 (partly) — Q3 and Q4 fall outside the coverage clauses of SHARED-ATTR-008 (brace indented 4 per scope) and SHARED-ATTR-009 (no tab). Only SHARED-FIX-007 makes them bugs.

## 3. Critical candidates

- #1133 — the RH5601 fix makes code fail to compile: the raw-string closing line no longer matches the content indentation.
- #1075 — changes program meaning and user text. The RH5025 and RH5027 fixes delete lines inside a string literal, so the string's value changes. The RH5011 and RH5030 fixes insert lines inside a block comment, and RH5011 repeats it on every application.
- #1261 — the RH5405-family brace fix duplicates a trailing comment. As the issue writes it on one line, `{ M(a, b); // trail } // trail`, the `}` would be commented out and the build would break; this needs a reproduction to confirm. The empty-type and RH5417 fixes also rewrite `#region` text.
- Not critical but worth noting: #946 and #1076 (RH5023 no-op, RH5024/RH5032 and RH5011/RH5026 cycles) make Fix All or apply-fix loops never converge. They do no damage to code.

## 4. Real bugs

- #946 — SHARED-STMT-004, SHARED-FIX-001. Input `var x = 1;⏎await using (r) {…}`. Today the blank line lands between `await` and `using`, and every application adds one more. Spec: one blank line above `await`.
- #972 — SHARED-STMT-004. Input `a++;⏎/* block */ if (a > 7) { }`. Today two blank lines are inserted. Spec: exactly one.
- #1074 — SHARED-DIAG-003, RH6009-001, RH6010-001. Input `$"{new int[]{1}.Length}"`. Today the initializer brace is not checked. Spec: reported. The RH5601 and RH5602 instances conflict with their rule specs (section 6).
- #1075 — SHARED-FIX-003, SHARED-DIAG-003, SHARED-FIX-001.
  - RH5025: in `$"""⏎a⏎⏎{x}⏎"""` the empty line is deleted today. Spec: not reported.
  - RH5011: `break; /* a⏎b */⏎a++;` gets a line inside the comment today. Spec: the line goes after the comment, or the fix is withheld.
- #1076 — SHARED-FIX-001, SHARED-FIX-004, SHARED-FIX-005.
  - `{⏎⏎⏎M();` needs two applications today. Spec: one.
  - The RH5023 fix is a no-op at end of file today. Spec: not reported.
  - RH5011 inserts a line before `else`, which RH5026 then removes. Spec: not reported.
- #1079 (RH5603 part) — SHARED-FIX-001. Input `}⏎···⏎`. Today the fix leaves `}⏎···`, which is reported again. Spec: one application. RH5603-001's wording may not report the input at all; verify.
- #1094 — SHARED-FIX-007 (shared line), SHARED-ATTR-008/-009. Input `int _a; [A] void M()`. Today the fix gives `int _a; [A]⏎void M()`. Spec: `[A]` also gets its own line.
- #1096 — SHARED-STMT-004, RH5011-001, SHARED-BLANK-001.
  - `case 1 when⏎    Check(v): M(1); var y = 1;`: today `var y` is indented 4 columns too deep. Spec: indented like the statement it follows.
  - `break;⏎/// doc⏎⏎a++;` is reported today and gets a second blank line. Spec: not reported.
- #1133 — RH5601-002, SHARED-FIX-003. Input: a tab on the closing line of `$"""…"""`. Today it is reported and the fix breaks the build. Spec: not reported.
- #1141 — SHARED-FIX-006, SHARED-OPSPACE-004. Input `_ = a+M(a, () => { return 1; });`. Today the lambda is rewrapped. Spec: only `a + M` changes.
- #1142 — SHARED-ATTR-017, SHARED-FIX-006, SHARED-FIX-008. Input `    [A, B]⏎    void M()` under column-0 class braces. Today everything moves to column 8, with LF in CRLF files. Spec: `[A]⏎[B]` at the list's indentation, in the document's line ending.
- #1261 — SHARED-FIX-003. Input `#region legacy⏎#endregion⏎class A {}`. Today the fix rewrites the text to `#region Legacy`. Spec: directive text unchanged.
- Bug parts inside behavior-change issues:
  - #1093: the formatter joins `[param: A] set;`, against FMT-BREAK-027 (accessor attribute lists start their own line).
  - #1276: the extension block is reported with `Location.None` and the fix formats a different type, against SHARED-DIAG-007.

## 5. Groups

- **G1 — statement blank-line fix base** (`StatementShouldBePrecededByABlankLineCodeFixProviderBase`, SHARED-STMT; bug lane): #946, #972, #1096. #1385's analyzer part touches the same base but is a behavior change, so it is a separate run or comes after them.
- **G2 — brace and blank-line rules RH5011, RH5022–RH5027, RH5030** (bug lane): #1075 (blank-line part), #1076. Behavior-change lane: #1080, #1396. Spec-only lane: #1081 item 1.
  - Overlap: #1076's RH5025/RH5030 cycle (comment of 2026-10-21) is the same defect #1396 resolves.
- **G3 — literal content (SHARED-DIAG-003), shared helper `FormattingTextAnalysisUtilities.GetStringLineIndices`**: #1074, #1133, the hole instance in a #1075 comment. Related #951 (F2). The RH5204 hole indentation stays untouched until #951 is decided.
- **G4 — spacing rules, comments and gap width** (behavior-change lane): #1078 and the widening parts of #1079. Related #1160 (F3).
- **G5 — attribute placement and list-shape fix bases**:
  - bug lane: #1094, #1142;
  - behavior change: #1033, #1093;
  - new rule: #1159, separate, with #1144 (F3) for the layout model;
  - #1034 already done.
- **G6 — brace and empty-type fixes RH5404–RH5417**:
  - bug lane: #1261;
  - behavior change: #1416, #1275, #1276. #1416 and #1261 touch the same fixes and should be sequenced.
  - Related #1388 (F8, line endings).
- **Alone**: #1141 (operator-spacing fixes, bug lane); #1095 (deconstruction, behavior change).
- **Duplicates**: no whole-issue duplicates. Partial overlap: the #1076 comment instance equals #1396.

## 6. Doubts

- #1074: SHARED-DIAG-003 conflicts with RH5601-001 (tab before a literal) and RH5602-001 (interpolated lines, holes included), which state today's behavior. Is this a bug fix that amends the rule specs, or a behavior change with a change doc?
- #1079: most instances contradict explicit rule-spec text (RH5601-001, RH6004-002, RH7302-001) or leave it unspecified (RH6005, RH6021). Reclassify as a behavior change with a change doc and split RH5603 off as a bug?
- #1080: the issue says "bug against RH5022-001", but that requirement's wording excludes the case. Behavior change with a change doc, or a reading of "ends with `{`" that includes trailing comments?
- #1416: SHARED-FIX-007 already requires the wrapped statement to keep its layout, and SHARED-BRACE-002 says the opposite. Which requirement wins decides bug vs behavior change. In both cases BRACE-002 must be amended.
- #1385: FMT-BLANK-016 (#1405) already states the formatter behavior on main. Is the formatter part now a bug against FMT-BLANK-016, with the change doc needed only for SHARED-STMT-004? Untagged tests still pin the old output.
- #1094 Q3 and Q4: outside the coverage clauses of SHARED-ATTR-008/009. Does SHARED-FIX-007 alone make them bugs?
- #1076, RH5024 `#region` instance: RH5024-001 already exempts a blank line under `#region`. Possibly already fixed; reproduce before the run.
- #1081: item 2 (lone `\r`) is already in FMT-PIPE-007. The U+2028/U+0085 comment instance and item 1 remain.
- #1034: the decision is already applied on main. Close it?
- #1275 is blocked by #1240 (still open, family F8).
- Milestones: #946 is on 1.2.0 but not critical, so bug-pool is proposed. #972 is on Triage and gets bug-pool.
