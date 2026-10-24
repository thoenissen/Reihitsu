# Phase 5 — family F1 inventory: formatter structure, documentation comments, regions, blank lines, switch braces, cleanup

25 issues: #948, #949, #950, #952, #954, #959, #960, #961, #962, #963, #964, #1082, #1083, #1148, #1239, #1329, #1355, #1381, #1384, #1389, #1390, #1392, #1395, #1398, #1400. Read on 2026-10-24 against `main` (spec text as it stands after #1405 and #1429).

Classification rules (from the coordinator's plan): **bug** = the code differs from a requirement as written today (the requirement is named); **behavior change** = intended behavior changes (default: extension → vNext); **clarification** = the spec gains a requirement for behavior that already exists; **internal** = cleanup, tests, docs, tooling. Critical = breaks the build, changes meaning, loses user code or comments, crashes, or is non-idempotent / phases undo each other. No FMT ID cited by these issues was renumbered by #1429.

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | P# | Duplicate/overlap | Template? | Proposed milestone | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 948 | Bracing a switch section breaks a cross-section local | bug, formatter | Triage | bug | FMT-PIPE-003; FMT-SWITCH-011 (sibling guard); SwitchCaseBracePhase | — | — | #1398, #1381 (same phase) | yes | 1.2.0 | Violates FMT-PIPE-003. CRITICAL: CS0103, output no longer compiles |
| 949 | Comment before terminal break gets no blank line in pass 1 | bug, formatter | Triage | bug | FMT-BLANK-011, FMT-PIPE-004, SHARED-BLANK-004, FMT-SWITCH-007; RH5020 | — | — | #1148 (same req) | yes | 1.2.0 | Violates FMT-BLANK-011 and FMT-PIPE-004. CRITICAL: non-idempotent |
| 950 | Remove dead switch brace-removal path; fix contributor summary | formatter | Maintenance | internal | FMT-SWITCH-008, FMT-INDENT-027 (unchanged); switch-case-braces non-goal | — | — | #948/#1398 (same file) | no | Maintenance | Dead code, doc comment, test rename; no output change |
| 952 | Directive after disabled text keeps its indentation | bug, formatter | Triage | bug | FMT-INDENT-009; IndentationPhase | — | — | #1400 item 22 (directives in inactive code) | yes | bug-pool | Violates FMT-INDENT-009 (every active directive to column 0) |
| 954 | Empty region after `{` gains a blank line every pass | bug, formatter | Triage | bug | FMT-BLANK-007, FMT-BREAK-002, SHARED-BLANK-005/-006; LineBreakPhase vs BlankLinePhase | — | — | related #1263 (fix side) | yes | 1.2.0 | Violates FMT-BLANK-007. CRITICAL: two phases undo each other each pass |
| 959 | Normalize line ends inside comments and disabled text | enhancement, formatter | vNext | behavior change | FMT-PIPE-002/-007, FMT-CLEAN-001/-004, FMT-DOC-006/-008/-011, FMT-EOL-001/-004; RH8305 | — | — | #960 (same cleanup.md non-goal); absorbed #1310 Q7 | yes | vNext | FMT-PIPE-002 gains an allowance; change doc |
| 960 | Trailing whitespace at directive line end and after last token | bug, formatter | Triage | behavior change | FMT-CLEAN-001/-003/-005; cleanup.md non-goal (line 18) | — | — | #959 | yes | vNext | Wrong bug label: cleanup.md lists both cases as non-goals today |
| 961 | Doc elements: end tag on last content line; wrapped inline elements | bug, formatter | Triage | behavior change | FMT-DOC-008/-009/-010 | — | — | #1329, #962 (same phase) | yes | vNext | Wrong bug label: FMT-DOC-010 keeps case 1; FMT-DOC-009 leaves inline undecided |
| 962 | `///` behind a statement or in an expression is relocated | bug, formatter | Triage | behavior change | FMT-DOC-001/-003, FMT-BLANK-012 | — | — | #1389, #1148 (RH8303 shape) | yes | vNext | Wrong bug label: FMT-DOC-001 "does not yet decide" statements/expressions |
| 963 | Comment after `=>` glued to new brace; comment duplicated on split fields | bug, formatter | Triage | bug (partly) | FMT-STRUCT-006/-021/-022/-024, FMT-PIPE-013, FMT-PIPE-002 | — | P32, P36 | #1381, #1390 (converter); #1384, #1395 (field split) | yes | bug-pool | Field part violates FMT-STRUCT-024; `{// why` part is unspecified (extension) |
| 964 | Normalize the `#region` line independently of capitalization | enhancement, formatter | vNext | behavior change | FMT-REGION-001; region analyzers | — | — | #1239, #1083 | yes | vNext | The coordinator's own extension example (`#region   Fields`) |
| 1082 | File ending in a directive drops its final line break too | — | SDD | behavior change | FMT-CLEAN-005/-007, FMT-PIPE-004; RH5603 (clarification) | — | — | #960 (EOF), #959 | no | vNext | FMT-CLEAN-007 changes; directives-only file left to change doc |
| 1083 | Replace tabs in region names only | — | SDD | behavior change | FMT-CLEAN-003, RH5601, SHARED-FIX-003 (no exception, per comment) | — | — | #964, #1239 | no | vNext | FMT-CLEAN-003 changes; RH5601 widened |
| 1148 | Blank line before line-leading comment only between statements/members | — | SDD | behavior change | FMT-BLANK-011; RH5020, RH8303; RH5110/RH5203 (#1147) | — | — | #949, #1392, #1390 (comment after arrow) | partly | vNext | FMT-BLANK-011 narrowed; comments add more shapes |
| 1239 | Rewritten `# region` keeps whitespace after `#` | bug, formatter | vNext | bug | FMT-REGION-001; RH7302 (#1174) | — | — | #964, #1083 | yes | bug-pool | Violates FMT-REGION-001 (rewritten line written as `#region`, one space) |
| 1329 | Declaration after unclosed doc element becomes a comment | bug, formatter | — | bug | FMT-PIPE-002, FMT-PIPE-003, FMT-DOC-008/-009; RH8305/8307/8309 fixes (#1241) | — | P19 | #961, #1389 | yes | 1.2.0 | Violates FMT-PIPE-003. CRITICAL: declaration lost, program changes/breaks |
| 1355 | Doc tag names matched case-sensitively | — | SDD | behavior change | FMT-DOC-008 (says case-insensitive today) | #1356 (paired rule), #1091 (P8) | P8 | #1356 | no | vNext | FMT-DOC-008 changes; change doc |
| 1381 | Keep comment layout when inserting braces (P32) | — | SDD | bug | FMT-PIPE-013, FMT-SWITCH-007, FMT-STRUCT-002 | #1380 (closed by #1405) | P32 | #963, #948 | no | bug-pool | Violates FMT-PIPE-013 (recorded by #1405); issue still says change doc |
| 1384 | Do not split a documented field (P36) | — | SDD | bug | FMT-STRUCT-024, FMT-STRUCT-021/-022, FMT-PIPE-003; RH8026/RH8027; RH7103/7104/7109/5110/8305 fixes | #1380 (closed) | P36 | #963, #1395 | no | 1.2.0 | Violates FMT-STRUCT-024. CRITICAL (borderline): later fields lose documentation |
| 1389 | BOM before line break dropped; doc text behind code deleted | — | SDD | bug | FMT-CLEAN-009, FMT-PIPE-002, FMT-DOC-003/-008/-009/-011 | — | P29 | #962, #1329, #1345 | no | 1.2.0 | Violates FMT-CLEAN-009, FMT-PIPE-002. CRITICAL: comment text deleted |
| 1390 | Structural output does not compile / not stable | — | SDD | bug | FMT-PIPE-003/-004, FMT-STRUCT-004/-006/-024, FMT-BREAK-021 | — | P36 | #963, #1381, #1148 | no | 1.2.0 | Violates FMT-PIPE-003/-004, FMT-STRUCT-006. CRITICAL: CS1997, non-idempotent |
| 1392 | Blank line after comment before else/catch/finally/while kept | — | SDD | bug | FMT-BLANK-008; RH5026, RH5027 | — | P31 | #1148 | no | bug-pool | Violates FMT-BLANK-008 |
| 1395 | Comment before later declarator moves above split field | — | SDD | bug | FMT-STRUCT-022, FMT-PIPE-002 | — | — | #963, #1384 | no | bug-pool | Title says behavior change, but FMT-STRUCT-022 already requires it |
| 1398 | Break-only section never braces the switch | — | SDD | bug | FMT-SWITCH-006 (vs -004) | — | — | #948, #1381 | no | bug-pool | Title says behavior change; FMT-SWITCH-006 already requires it; -004 conflicts |
| 1400 | Spec clarifications: inactive directives, lambda anchor, multi-line comment gaps | — | SDD | clarification | FMT-INDENT-010/-028, FMT-CLEAN-002/-003, FMT-REGION-006, FMT-SPACE-001/-013 | — | — | #952 | no | Maintenance | Spec text only; behavior confirmed |

Counts — Kind: bug 14 (#963 partly), behavior change 9, clarification 1, internal 1. Proposed milestone: 1.2.0 7, bug-pool 7, vNext 9, Maintenance 2.

## 2. Wrong bug labels

- #960 — `cleanup.md` lists whitespace at a directive line's end and behind the last token as non-goals; no requirement asks for removal (maintainer called it a bug, see Doubts).
- #961 — FMT-DOC-010 explicitly leaves a three-line summary whose content starts below the start tag alone; FMT-DOC-009 says it "does not yet decide" inline elements.
- #962 — FMT-DOC-001 says it "does not yet decide" comments before a statement or inside an expression.
- #963 (half) — the `{// why` part has no requirement saying where the comment goes; only the field-duplication part violates FMT-STRUCT-024.

The reverse: #1381, #1384, #1395, #1398 are framed as behavior changes (change doc) but the current spec already states the wanted behavior, so by the rule they are bugs.

## 3. Critical candidates

- #948 — formatted code does not compile: a local or local function declared in one section is out of scope in another after bracing (CS0103).
- #949 — non-idempotent: the second pass inserts the blank line before `// before break`.
- #954 — LineBreakPhase inserts and BlankLinePhase removes the same blank line every pass; the output ends with a blank line after `{`.
- #1329 — `int A;` after an unclosed `<summary>` becomes `///     int A;`: the declaration disappears from the program.
- #1389 — `int A; /// <summary>end</summary>` loses `end</summary>`; the multi-line variant loses a line; BOM before a line break dropped.
- #1390 — async alias `T` gets `return await …` (CS1997); `with` initializer moves `{` again in pass 2 (non-idempotent).
- #1384 (borderline) — `/// …⏎public int X, Y;` split leaves `Y` undocumented (CS1591, which breaks builds with warnings as errors); several code fixes inherit it.

## 4. Real bugs

- #948 — FMT-PIPE-003. `case 1: int y = 1; F(y); break; case 2: y = 2; F(y); break;` → both sections braced, CS0103 → switch unchanged.
- #949 — FMT-BLANK-011, FMT-PIPE-004. `A(); B();⏎// before break⏎break;` → after bracing, `}⏎// before break` (pass 2 adds the blank) → `}⏎⏎// before break` in pass 1.
- #952 — FMT-INDENT-009. `#if !NEVER … #else x = 2;⏎                #endif` → `#endif` keeps 16 spaces → `#endif` at column 0.
- #954 — FMT-BLANK-007. `{⏎    #region A⏎    #endregion // A⏎}` → blank line after `{` → none after `{`, one between the directives.
- #963 (field part) — FMT-STRUCT-024. `private int /* c */ _first, _second;` → comment copied onto both fields → unchanged.
- #1239 — FMT-REGION-001. `# region fields` → `# region Fields` → `#region Fields`.
- #1329 — FMT-PIPE-002/-003. `/// <summary>S⏎int A;` → `///     int A;` → `int A;` stays code.
- #1381 — FMT-PIPE-013 (FMT-SWITCH-007, FMT-STRUCT-002). `if (a)⏎    Foo(); /* a */ // b` → `/* a */// b` → `/* a */ // b`; blank line between leading comments dropped → kept.
- #1384 — FMT-STRUCT-024. `/// <summary>The coordinates</summary>⏎public int X, Y;` → split, `Y` undocumented → unchanged.
- #1389 — FMT-CLEAN-009, FMT-PIPE-002. `int A; /// <summary>end</summary>` → `int A; /// <summary>` → full text kept.
- #1390 — FMT-PIPE-003/-004, FMT-STRUCT-006. `async T RunAsync() => await T.Delay(1);` (alias) → `{ return await …; }` CS1997 → expression body kept; `int Other()⏎// before arrow⏎=> 1;` → comment between signature and `{` → inside the block.
- #1392 — FMT-BLANK-008. `}⏎// c⏎⏎else` → blank line kept → `}⏎// c⏎else`.
- #1395 — FMT-STRUCT-022. `private int _c, /* before d */ _d;` → `private int _c; /* before d */` → comment on its own line above `private int _d`.
- #1398 — FMT-SWITCH-006. Single-line switch with a break-only section whose `case Point { X: 1,⏎ Y: 2 }:` label wraps → whole switch braced → unbraced.

## 5. Groups

- **Switch-brace phase (bug lane):** #948, #1398, the switch part of #1381; #950 touches the same `SwitchCaseBraceRewriter` (internal, can ride along or follow).
- **Field splitting (bug lane):** #963 field part, #1384, #1395 — one guard family in `FieldDeclarationSplit` under FMT-STRUCT-022/-024; tests in `FieldDeclarationSplitTests` overlap.
- **Expression-body converter / wrap comment layout (bug lane):** #1390, the control-flow and expression-body parts of #1381, #963 `=>` part (once decided). FMT-PIPE-013 and FMT-STRUCT-006 must agree; #1390 instance 3 (comment after arrow) and #963 part 1 are the same shape.
- **Blank lines (bug lane):** #949, #954, #1392. Behavior-change twin: #1148 (FMT-BLANK-011 scope), which overlaps #949's requirement.
- **Documentation-comment phase:** bugs #1329, #1389 (doc part); behavior changes #961, #962, #1355 (FMT-DOC-008 matching), #959 doc part.
- **Cleanup / end of file (behavior-change lane):** #959, #960, #1082; #1389 BOM part is a bug in the same phase.
- **Region line (behavior-change lane):** #964, #1083; #1239 is the bug-lane sibling in the same rewriter.
- **Indentation of directives:** #952 (bug), #1400 item 22 (clarification).
- Duplicates: none exact within F1. #959 already absorbed #1310 Q7.

## 6. Doubts

1. #960: `cleanup.md` non-goal says the maintainer decided both cases "as a bug", but no requirement states it — keep it a bug (add requirement + fix) or reclassify as extension per the rule?
2. #961, #962: the maintainer decided both "as bugs", but FMT-DOC-010 and FMT-DOC-001/-009 say otherwise or leave them undecided — extension (vNext) per the rule?
3. #1381, #1384, #1395, #1398: the spec already states the wanted behavior (FMT-PIPE-013 and FMT-STRUCT-024 from #1405; FMT-STRUCT-022 and FMT-SWITCH-006 since #947), but each issue asks for a change doc. Treat as bugs with no change doc?
4. #1398: FMT-SWITCH-004 and FMT-SWITCH-006 conflict for a break-only section with a wrapping label; the spec text needs a precedence sentence (clarification) alongside the fix.
5. #963: split it — the field-duplication part is a bug under FMT-STRUCT-024, the `{// why` part has no requirement (extension, or folded into #1390 instance 3)?
6. #1384: critical or not? It does not lose text, but it changes which fields are documented (CS1591), and code fixes inherit it.
7. #954: the final output is stable across runs (the phases undo each other within each pass); critical by the "phases undo each other" criterion, but the visible harm is one blank line.
8. #1329 has no milestone; #1239 is a bug sitting in vNext; #1400 (spec-only clarification, milestone SDD) — Maintenance or keep with SDD work?
9. #1082 leaves one sub-decision to its change doc: whether a file with only directives stays "left unchanged" (FMT-CLEAN-007).
