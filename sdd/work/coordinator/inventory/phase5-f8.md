# Phase 5 — family F8 inventory

Clarity RH3xxx, new-rule feature requests, the shared code-fix contract, CLI/Playground/tooling/squad/SDD process. 31 issues, read 2026-10-24 against `main` (specs as of #1418/#1429). Blocker states come from `phase5-index.md` (absent there = closed).

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle (P#) | Duplicate/overlap | Template? | Proposed milestone | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 122 | VS extension for the formatter | enhancement, formatter | vNext | behavior change | new surface; CLI orchestration | — | — | — | no | vNext | New product surface; needs its own spec and change doc |
| 352 | Rule: cognitive complexity limit | enhancement, analyzer | 1.4.0 | behavior change | new RH2### rule | — | — | 353, 354 (family) | yes | vNext | New rule with option; no fix |
| 353 | Rule: method line-count limit | enhancement, analyzer | 1.4.0 | behavior change | new RH2### rule | — | — | 352, 354 | yes | vNext | New rule with option; no fix |
| 354 | Rule: type line-count limit | enhancement, analyzer | 1.4.0 | behavior change | new RH2### rule | — | — | 352, 353 | yes | vNext | New rule; per partial declaration |
| 849 | Rule: get-only property → expression body | enhancement, analyzer | 1.3.0 | behavior change | new RH3### rule; FMT-STRUCT-014 | #848 (closed) | P11 new-rule candidate | 850 | yes | vNext | Mirrors FMT-STRUCT-014; formatter already does it |
| 850 | Rule: get-only indexer → expression body | enhancement, analyzer | 1.3.0 | behavior change | new RH3### rule; FMT-STRUCT-014 | #848 (closed) | P11 new-rule candidate | 849 | yes | vNext | Indexer twin of 849 |
| 851 | Rule: expression-bodied indexer single-lined | enhancement, analyzer | 1.3.0 | behavior change | new RH54## rule (RH5401 twin) | #848 (closed) | P11 new-rule candidate | RH5401 | yes | vNext | RH5401 stays properties-only; next free ID re-check |
| 867 | Verifier compiles at warning level 4 | enhancement | Maintenance | internal | analyzer test base | — | — | — | no | Maintenance | Test-harness change; may surface real fix defects |
| 877 | RH3002 follow-ups (re-parse, cost, tests) | enhancement, analyzer | Maintenance | internal | RH3002 | — | — | 1278 (same code) | no | Maintenance | Design/perf/test hygiene, no behavior change intended |
| 898 | `scripts/trace.sh` lacks exec bit | — | Maintenance | internal | scripts | — | — | — | no | Maintenance | Still `100644` on main (verified) |
| 975 | `--dry-run` header `a//full/path` | bug, formatter | Triage | behavior change | CLI-013 | — | — | — | yes | vNext | Not a bug: CLI-013 example pins today's `a//work/src/A.cs` |
| 976 | Playground: note for generated input | enhancement | vNext | behavior change | PLAY-005/006 (new req), FMT-PIPE-006 | — | — | — | partly | vNext | Spec non-goal names it a decided behavior change |
| 1054 | lane-check: `squad/decisions/` = tooling | enhancement | Maintenance | internal | `LaneClassifier` | — | — | 1427 (squad) | no | Maintenance | Tooling/process decision |
| 1136 | SHARED-FIX-003: relocate/reorder, disabled text | — | SDD | clarification | SHARED-FIX-003 | — | P1 | 1282, 1332 | no | Maintenance | Text not yet extended; covering tests must pass today |
| 1143 | Fix All finishes in one application | — | SDD | bug | SHARED-FIX-002 | #1140, #1141, #1142 (open); #1138 closed | — | 1140–1142 | no | bug-pool | Violates SHARED-FIX-002; 26 test classes still set 2 iterations |
| 1240 | diagnostics.md non-goal vs inactive `#region` | — | SDD | clarification | SHARED-DIAG non-goal line 14, SHARED-DIAG-008 | — | P22 | — | no | Maintenance | Docs-only; non-goal text unchanged on main |
| 1257 | RH3001/3005/3101/3105 fixes make generic call | bug, analyzer | Triage | bug | SHARED-FIX-003; RH3001-004, RH3005-004, RH3101-004, RH3105-004 | — | P1, P23 | 1281, 1285 | yes | bug-pool | SHARED-FIX-003 (compile error). CRITICAL: fixed code fails CS0307 |
| 1259 | Share RH3106/RH3107 empty-list removal | — | Triage | internal | SHARED-PARENS-002 | — | — | 1282 | no | Maintenance | Refactor; do together with 1282 |
| 1278 | RH3002/RH3003 report constructs they name | — | SDD | behavior change | RH3002-001/002, RH3003-001/002 | #1161 (closed) | P11 own gap | 877, 1285 | no | vNext | Widening; spec position lists exclude these inputs |
| 1279 | RH3004 fix: `static`, layout, parentheses | — | SDD | bug | RH3004-003, SHARED-FIX-003, SHARED-FIX-007 | — | P1, P10 | — | no | bug-pool | Layout vs SHARED-FIX-007; cast/binary vs SHARED-FIX-003. CRITICAL (CS0019) |
| 1280 | RH3201 empty comments of every kind | — | SDD | behavior change | RH3201-001, RH3201-002 | #1139, #1161 (closed) | P13, P6, P11 | — | no | vNext | All instances contradict RH3201-001/002 as written; no bug part |
| 1281 | Unsafe fixes withheld: RH3005/3101/3103/3106/3201 | — | SDD | bug | SHARED-FIX-003; RH3005-004/005, RH3101-004, RH3106-001, RH3201-005 | — | P1, P23 | 1257, 1417 (RH3103 part moved) | no | bug-pool | SHARED-FIX-003. CRITICAL: `returna;`, CS0019/CS0121, virtual call changes meaning |
| 1282 | RH3101/3104–3107 fixes delete comment text | — | SDD | bug | SHARED-FIX-003, SHARED-PARENS-002, RH3101/3104/3105-004 | — | P1 | 1259, 1136 | no | bug-pool | SHARED-FIX-003 (comment text). CRITICAL: user comments lost |
| 1283 | RH3006 fix keeps line ending | — | SDD | bug | RH3006-003, SHARED-FIX-008 | #1138 (closed) | P24 | 1388 | no | bug-pool | SHARED-FIX-008: LF written into CRLF document |
| 1284 | Expression-body fixes format only the body | — | SDD | bug | SHARED-FIX-006, SHARED-FIX-007, SHARED-BODY-003, RH3202-002 | #1138 (closed) | P14d | — | no | bug-pool | SHARED-FIX-006; SHARED-BODY-003 wording conflicts (reword needed) |
| 1285 | RH3001 fix in argument/list positions | — | SDD | bug | RH3001-001, RH3001-004, SHARED-FIX-003 | — | P23 | 1257, 1278 | no | bug-pool | RH3001-004: fix not offered for `M(!x)`; extra parentheses (comment) |
| 1388 | Fixes keep line ending (RH5405, RH5011, RH8304, FormatNode) | — | SDD | bug | SHARED-FIX-008, FMT-PIPE-007, RH5011-004, RH8304-004 | #1415 (closed) | P24, P35 | 1283, 1142, 1261, 1416 | no | bug-pool | SHARED-FIX-008; RH5011-004/RH8304-004 still state lone `\r` |
| 1417 | RH3103 not reported where `T?` invalid (P38) | — | SDD | bug | SHARED-DIAG-011, RH3103-001/002/004/006 | #1415 (closed) | P38 | 1281 | no | bug-pool | SHARED-DIAG-011 (its own RH3103 example). CRITICAL: fix writes `typeof(?)` |
| 1422 | RH3104/RH5417 not reported where form invalid (P38) | — | — | bug | SHARED-DIAG-011, SHARED-FIX-003, RH3104, RH5417 | #1415 (closed) | P38 | 1417 | no | bug-pool | SHARED-DIAG-011. CRITICAL: RH3104 fix writes `default(S);` (CS0201) |
| 1427 | Handover templates: Follow-up candidates | — | — | internal | `squad/roles/` | — | — | 1054 | no | Maintenance | Squad instructions only |
| 1428 | Requirement-coverage exemption gaps | — | — | internal | `RequirementCoverageTests`, `sdd/README.md` | — | — | — | no | Maintenance | SDD tooling and rulebook text |

## 2. Wrong bug labels

- #975 — CLI-013's example output shows today's `--- a//work/src/A.cs`; no requirement asks for a relative path, so the wanted header is a spec change (behavior change), not a bug.

(The other `bug`-labeled issue, #1257, is a real bug against SHARED-FIX-003.)

## 3. Critical candidates

- #1257 — RH3001/RH3005/RH3101/RH3105 fixes turn `M(Max > G, B > (7))` into a generic invocation; fixed code fails CS0307/CS0246.
- #1279 — RH3004 fix writes `() => 1 + h` (CS0019) and `(Func<int,int>)(int x) => x + 1`; fixed code does not compile.
- #1281 — fixes break the build (`returna;`, `b == 1 == 1` CS0019, `delegate { }` CS0121/CS8917) or change program meaning (RH3101 `base.M()` → virtual `M()`).
- #1282 — fixes delete user comments (`base /* a */ . /* b */ M()` → `M()`, `[Obsolete(/* none */)]` → `[Obsolete]`).
- #1417 — RH3103 fix offered where `T?` is invalid: writes `typeof(?)`, `int?.Equals(1, 2)`, `using static int?;` (do not parse).
- #1422 — RH3104 fix writes the statement `default(S);` (CS0201).

## 4. Real bugs

- #1143 — SHARED-FIX-002. `a+b+c` with RH6024 Fix All → today `a + b+c` (still reported) → required `a + b + c` in one application. Also RH6023, RH6016, RH5502–RH5531, RH5104, RH5110, RH5204, RH8204.
- #1257 — SHARED-FIX-003 (compile error). `M(Max > G, B > (7))` RH3005 fix → today `M(G < Max, B > (7))` (CS0307) → required: fix withheld (diagnostic stays).
- #1279 — SHARED-FIX-007 / SHARED-FIX-003. `= delegate (int x)` + `{` on next line → today `= (int x) => {` → required formatter layout; `delegate () { return 1; } + h` → today `() => 1 + h` (CS0019) → required withheld (or parenthesized).
- #1281 — SHARED-FIX-003. `return/**/a;` RH3201 fix → today `returna;` → required fix withheld; `base.M()` (virtual, unsealed) → today `M()` → required withheld.
- #1282 — SHARED-FIX-003 (comment text). `this /* a */ . /* b */ _a` → today `_a` → required diagnostic kept, fix withheld.
- #1283 — SHARED-FIX-008. CRLF doc, `return a ||` / `/* x */ b && c;` RH3006 fix → today one LF → required CRLF only.
- #1284 — SHARED-FIX-006. RH3202 on `public int  GetValue( ) => _value;` → today signature rewritten to `public int GetValue()` → required only the body becomes a block.
- #1285 — RH3001-004. `M(!x)` → today reported without a fix → required fix `M(x == false)`; `B ??= !x` → today `(x == false)` → required no parentheses.
- #1388 — SHARED-FIX-008 / FMT-PIPE-007. CRLF doc, RH5401 via `FormatNode` on `() => { M(); }` → today LF breaks → required CRLF; lone-`\r` doc RH5011 → today `break;␍␍` → required `\n`.
- #1417 — SHARED-DIAG-011. `nameof(System.Nullable<int>)` → today reported → required not reported; `using N = System.Nullable<int>;` below C# 12 → not reported.
- #1422 — SHARED-DIAG-011. `new S();` statement → today reported, fix `default(S);` → required not reported; RH5417 lambda in interpolation hole below C# 11 → not reported.

## 5. Groups

- **Comparison-rewrite fix guards (analyzer bug fix):** #1257 + #1285 + the RH3005/RH3101 parts of #1281 — RH3001/RH3005/RH3101/RH3105 providers; a shared re-parse check could serve all.
- **Comment-deleting removals (analyzer bug fix):** #1282 + #1259 (shared RH3106/RH3107 removal) + RH3106/RH3201 parts of #1281.
- **RH3004 fix:** #1279 alone (or with #1281 as another SHARED-FIX-003 run).
- **Line endings (SHARED-FIX-008):** #1283 + #1388 (check whether the `FormatNode` fix covers RH3006); coordinate with #1142, #1261, #1416 (other families).
- **P38 / SHARED-DIAG-011:** #1417 + #1422 (same principle, same shared requirement).
- **Fix All convergence:** #1143, after #1140, #1141, #1142.
- **RH3002 code:** #877 + #1278 (RH3002 part); #1285's parenthesis comment notes the RH3002/RH3001 interplay (SHARED-FIX-004).
- **New expression-body rules:** #849 + #850 (+ #851, layout twin of RH5401).
- **Size/complexity rules:** #352 + #353 + #354.
- **Spec text (docs lane):** #1136 + #1240 (+ the SHARED-BODY-003 rewording of #1284 if done spec-first).
- **Tooling/process:** #898, #1054, #1427, #1428 (tooling lane); #867 (test infra, separate).
- **CLI/Playground:** #975, #976 (independent behavior changes).
- **Duplicates:** none exact. #1281's RH3103 part is superseded by #1417 (comment on #1281).

## 6. Doubts

- #975: maintainer decided "bug" in #933, but CLI-013's example still pins today's header — treat as bug (fix code and example together, no change doc) or behavior change?
- #1417, #1422: the code violates SHARED-DIAG-011 (added by #1418, its example is RH3103 `nameof`), yet RH3103-001 still lists `typeof`/dotted forms and both issues ask for a change doc — bug or behavior change?
- #1136: extending SHARED-FIX-003 turns existing fixes (e.g. #1332) into bugs — clarification (as filed) or behavior change with a change doc?
- #1279: keeping `static` is not stated by RH3004-003, and RH3004-003 excludes cast/binary operands — the `static` part and "parenthesize" option are extensions; only withholding is required by SHARED-FIX-003.
- #1280: the issue says "bug fix and behavior change", but every instance contradicts RH3201-001/002 as written — classified entirely behavior change.
- #1284: SHARED-BODY-003 ("laid out as the formatter lays it out") conflicts with SHARED-FIX-006 — which wins until reworded?
- #1388: RH5011-004 and RH8304-004 state today's lone-`\r` behavior, conflicting with SHARED-FIX-008 — rule-spec amendment is part of the fix.
- #867: scope unknown — raising the warning level may expose real fix defects that need separate bug issues.
- Critical bugs are proposed for bug-pool; decide whether #1257, #1279, #1281, #1282, #1417, #1422 move to 1.2.0.
- #352–#354 sit on 1.4.0 and #849–#851 on 1.3.0 today; proposed vNext. #1422, #1427, #1428 have no milestone. #851's suggested ID RH5418 needs re-checking.
- #122: very large (new projects, CLI extraction); maybe a roadmap item rather than one squad run.
