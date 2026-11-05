# Phase 5 — bug pool re-inventory after the formatter rebuild (1.2.1, 1.2.2)

Date 2026-11-05. Base: `origin/main` at `a59a003` (after #1581). Issue list taken from the REST API (milestone 12 = 1.2.1: 41 open + 5 closed; milestone 13 = 1.2.2: 29 open). Inputs: issue bodies and every comment (incl. the #1503 triage comments of 2026-11-05), PR bodies #1455–#1581, requirement texts diffed between `1b9a028` (#1429, plan time) and `origin/main` (`sdd/specs/**`, `requirement-ids.txt`), and source on `origin/main`.

**Nothing was built or run.** Every verdict is a judgment from code, specs, PR bodies and issue comments. "LIKELY RESOLVED" means a run must still reproduce it and keep the passing test as a characterization test (`Refs`, not `Closes`), unless the #1503 triage already reproduced it (noted).

Requirement IDs: no ID cited by any pool issue was removed or renumbered (`requirement-ids.txt` only gained 69 IDs). 127 requirement texts changed; the table names a changed ID only where the change matters for the issue. "exist, unchanged" = every cited ID exists with byte-identical text.

Rebuild PRs referenced below: #1508 preparation phase (tabs, EOL), #1510 one line-start fact (SHARED-BLANK-008), #1513 round-trip, #1520 detached node roots, #1532 doc phase owns behind-code `///`, #1535 switch block-wrap carrier, #1541 blank-line sole owner (FMT-BLANK-017..020, FMT-BREAK-030), #1547 using comment blank line, #1548 spacing sole owner, #1551 attribute lists in structural phase (FMT-STRUCT-028/029), #1558 switch-section line starts, #1565, #1570 line breaks before switch braces/blank lines (FMT-BREAK-036, FMT-BREAK-005 retired in practice), #1573 region blank line via Core, #1575 FMT-PIPE-027, #1578 region line form (FMT-REGION-007) and no region rewriting in fixes (FMT-PIPE-028), #1580 context entry point first-line column (FMT-PIPE-017/018), #1581 fixes only through `ReihitsuFormatter` (FMT-PIPE-029..031; node-level entry points no longer check the generated header; RH7101 fix rebuilt). Pre-rebuild merges that also touched the pool: #1455 (R1), #1456 (R2), #1457 (R3), #1469 (phase-prediction removal, `AdjustNodeIndentation` removed).

## 1. Per issue

### Milestone 1.2.1 (46)

| # | Title (short) | MS | State | Rebuild-affected (PR) | Reqs on main | Verdict | Note |
|---|---|---|---|---|---|---|---|
| #946 | RH5005/RH5006 fix never converges on `await using`/`await foreach` | 1.2.1 | open | no — `StatementShouldBePrecededByABlankLineCodeFixProviderBase` untouched | exist, unchanged | VALID | Moved from 1.2.0 on 11-05. Same base as #972/#1096. |
| #952 | Directive after disabled text keeps its indentation | 1.2.1 | open | yes — #1510 (shared line-start fact) | FMT-INDENT-009, FMT-PIPE-002 unchanged | LIKELY RESOLVED | #1503 triage reproduced it as fixed on `a59a003`; only a regression test with the issue's input is missing (tests-lane work, closes the issue). |
| #958 | Arrow spacing (P34) | 1.2.1 | closed 11-05 | yes — #1548 | — | CLOSED | Done by #1548 (FMT-SPACE-017), comment 11-05. Other pairs live in #1440. |
| #963 | Comment between type and first declarator copied onto every split field | 1.2.1 | open | area touched — #1581 rewrote `FieldDeclarationSplitTransform` (layout only) | FMT-STRUCT-022/-024 same meaning; FMT-STRUCT-006 changed only for the `=>` part moved to #1439 | VALID | `FieldDeclarationSplitTests.CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` still pins the duplication on main (type-token trailing trivia copied by `CreateSplitField`). |
| #969 | Member-ordering fixes reformat the whole type | 1.2.1 | open | partly — #1578 (FMT-PIPE-028: fixes no longer rewrite regions), #1520/#1469 (node-level layout) | FMT-PIPE-010 reworded (adds the FMT-PIPE-029 exception), same meaning | VALID | Cause (base formats the whole type node) untouched. The region-renaming symptom is gone. |
| #972 | Statement blank-line fixes insert two blank lines behind a same-line block comment | 1.2.1 | open | no | exist, unchanged | VALID | `TokenGapAnalysis` changed in #1510 (line-start fact) but not `RequiredLineBreakCountForBlankLine`'s use in the base; reproduce. |
| #1015 | RH4102 Fix All only corrects one event kind | 1.2.1 | open | no | SHARED-FIX-002 unchanged | VALID | Casing area; shares files with R5. |
| #1016 | Casing fixes produce a name the rule still reports | 1.2.1 | open | no | exist, unchanged | VALID | Moved from 1.2.0 on 11-05; `CasingUtilities` untouched; overlaps R5. |
| #1076 | Blank-line fixes converge, never undo another rule, never no-op | 1.2.1 | open | yes — #1541 (RH5025 analyzer, RH5025-005, block statement after `}` exempt) | FMT-BLANK-013 gained an example (same meaning); others unchanged | RESHAPED | The RH5025/RH5030 cycle instance (comment 10-21, tracked by #1396) is gone: #1396 closed not planned, C3 keeps the blank line and exempts RH5025. Body/comments should drop it; RH5022–27 run, RH5023 EOF, RH5011 `else`/`while`, RH5024/RH5025 region instances stay. |
| #1093 | `[param: A]` before `set;` joined by the formatter (FMT-BREAK-027) | 1.2.1 | open | yes — #1551 rewrote FMT-BREAK-027 | FMT-BREAK-027 now: "leaves … the placement of a `param:` or `typevar:` list on an owner other than a parameter or type parameter" **unspecified** | SUPERSEDED (by #1445) | The remaining formatter part is no longer a bug against any requirement; the decided behavior is the analyzer+formatter widening already split to #1445 (vNext). Maintainer to confirm and close/merge. |
| #1094 | RH5513 fix: break before the list, indent correctly | 1.2.1 | open | partly — #1510 touched `TargetAttributePlacementCodeFixProviderBase`; #1508/#1469 tabs replaced first; SHARED-FIX-007 gained a tab bullet (#1580) | SHARED-ATTR-004/-008/-009 unchanged | RESHAPED | Q2/Q3 still valid. Q4 (tab-indented code → declaration in spaces at the attribute's visual column) must be re-checked against the new SHARED-FIX-007 bullet "a tab in front of the changed code is not replaced; counts as one column" (FMT-PIPE-017). |
| #1096 | Statement blank-line fixes: `case … when` indentation, blank line after `///` | 1.2.1 | open | possibly — #1510 changed `SyntaxTriviaUtilities` | exist, unchanged | VALID | Q2's cause (`ContainsBlankLine` resets on doc-comment trivia) is in `SyntaxTriviaUtilities`; re-check after #1510 but no PR claims it. |
| #1140 | Line-break and alignment fixes change only their code (P9) | 1.2.1 | open | yes — #1580 (RH5111 fix formats within its enum/parameter list/declaration), #1578, #1469 | RH5111-004 rewritten (new "formatted within a larger node" rule); FMT-PIPE-010 reworded; others unchanged | RESHAPED | The RH5111 instances are now governed by RH5111-004/SHARED-FIX-007 "changed code formatted within a larger node" (first token keeps column) — re-state them against that text. RH5101/5102/5110/5113/5204/5206/chain instances unchanged in cause. |
| #1141 | RH6023/RH6024 fixes rewrite only the two gaps | 1.2.1 | open | no — `OperatorSpacingCodeFixProviderBase` untouched | FMT-PIPE-010 reworded only | VALID | |
| #1142 | Attribute-list split fixes change only the list, keep EOL | 1.2.1 | open | indirectly — #1551 moved list split/merge into the structural phase (FMT-STRUCT-028/029) | SHARED-ATTR-017 and RH55xx-003 unchanged | VALID | `TargetAttributeListShapeCodeFixProviderBase` untouched; the formatter now owns the split, which gives the run a delegation option. |
| #1143 | Fix All finishes in one application | 1.2.1 | open | partly — RH5104 fix changed by #1457 | SHARED-FIX-002 unchanged | VALID | Depends on the scope fixes of #1140/#1141/#1142; keep last. |
| #1144 | Fix output laid out as the formatter lays it out (P10) | 1.2.1 | open | yes — #1469 (raw-string lines owned by indentation, `AdjustNodeIndentation` removed), #1520, #1580 | FMT-PIPE-010, SHARED-FIX-007 changed (new FMT-PIPE-017 bullets) | RESHAPED | #1503 kept it open without re-description. The "misindented target keeps its column" instance (comment 10-21) is now the specified behavior (FMT-PIPE-017, SHARED-FIX-007: first token keeps its column) — drop or turn into a question. RH5101/RH5102 raw-string off-by-one likely fixed by #1469; RH5205, RH5203, RH5417/RH3202 (P14b) instances need reproduction. |
| #1145 | RH5103 fix behind embedded/labeled statements, indent into one-line blocks | 1.2.1 | open | no | SHARED-FIX-007 gained bullets, no effect on this case | VALID | |
| #1149 | FMT-BREAK-016 covers enum members, so the RH5111 fix converges | 1.2.1 | open | area — #1580 changed the RH5111 fix | FMT-BREAK-016 unchanged (still names no enum member), FMT-PIPE-014 unchanged in meaning | VALID | `LineBreakAssignmentRewriter` normalizes break-*before*-`=` only for variable declarators and property initializers; enum members and parameter defaults (the issue's `A⏎= 1`) are still kept. |
| #1150 | Formatter-delegating fixes withheld when the formatter returns the document unchanged | 1.2.1 | open | yes — #1581 | FMT-PIPE-005 reworded (adds `OrganizeUsingDirectivesInDocumentAsync`); FMT-PIPE-006 **changed**: node-level entry points no longer check the generated header | RESHAPED | Generated-header instances (`<AUTO-GENERATED>`, RH5412–17/RH5417, RH6024) are resolved by #1581. For syntax errors #1581 recorded the maintainer principle "where an analyzer reports, a fix is offered" and rejected a withholding fix guard (change doc `rh7101-fix-through-context-entry-point.md`, rejected alternative); RH7101 now does not report (SHARED-JOIN-002). The issue's expected behavior (withhold the fix) contradicts that principle → maintainer question before a run. The RH5405–07 directive instance (FMT-STRUCT-003) stays. |
| #1151 | RH7302 fix renames an `#endregion` comment only when exactly the name | 1.2.1 | open | indirectly — #1578 changed `Core/RegionDirectiveUtilities` | SHARED-FIX-003 unchanged | VALID | |
| #1239 | Rewritten `# region` keeps the whitespace between `#` and `region` | 1.2.1 | open | yes — #1578 | FMT-REGION-001 now defers the line form to new FMT-REGION-007 ("written as `#region`, exactly one space, …") | RESHAPED | Still reproduces (comment 11-03: `#  region bar` → `#  region Bar`). Requirement to cite is FMT-REGION-007; the comment suggests a general policy for whitespace between `#` and a directive keyword (low priority) — maintainer to confirm the reading before a run. |
| #1243 | LineBreakPhase writes a blank line when joining a lone operator | 1.2.1 | open | yes — #1541 (FMT-BREAK-030) | FMT-BREAK-017/-018/-030 unchanged since body update | VALID | Re-described by #1503 against FMT-BREAK-030; reproduces on `a59a003`. Blocks #1273. |
| #1263 | Region-removing fixes clean the blank line they leave (RH7303, RH7310) | 1.2.1 | open | no | SHARED-FIX-007 gained bullets, no effect | VALID | RH7303 fix shared with R4 (#1262). |
| #1269 | Doc fixes RH8201/RH8202/RH8204: only the reported markup, one application | 1.2.1 | open | partly — #1456 (R2) touched RH8202/RH8204 fixes | RH8202-003, RH8204-002/-003 narrowed to "an element closed by its own end tag" (R2), same meaning for these inputs | VALID | Re-check RH8204 content-verbatim instance after #1456. |
| #1274 | RH5308 fix converges or is withheld | 1.2.1 | open | no | exist, unchanged | VALID | |
| #1283 | RH3006 fix keeps the document's line ending | 1.2.1 | open | no | exist, unchanged | VALID | |
| #1284 | Expression-body fixes format only the replaced body | 1.2.1 | open | partly — #1455 (base + RH3202 fix), #1510/#1580 (RH3208 fix) | SHARED-BODY-003 gained "SHARED-BODY-004 → no fix" (same meaning); SHARED-FIX-007 bullets | VALID | Cause (base formats the whole member) untouched. |
| #1285 | RH3001 fix in every argument/list position, withheld for constant patterns | 1.2.1 | open | no | exist, unchanged | VALID | RH3001 fix shared with R7 (#1257). |
| #1335 | Organization fixes change only the node they rewrite or move | 1.2.1 | open | yes — #1581 (RH7101 fix rebuilt), #1578 | RH7101-004 rewritten (fix splits only the reported declaration via FMT-PIPE-029); SHARED-FIX-007 bullets | RESHAPED | RH7101 instance (#1319 B1, fix on `_b` splits all three declarations) resolved by #1581. RH7004 (A3, A4), RH7104 (D1), RH7105/06 (E1), RH7107/08 (F1, G1, P26) unchanged. |
| #1342 | Doc layout fixes format only the comment, leave behind-code `///` | 1.2.1 | open | partly — #1456 (RH8309 fix), #1532 (doc phase alone moves behind-code docs), #1578 | RH8305-004/RH8307-004/RH8309-004 narrowed ("closed by its own end tag"), same meaning here | VALID | |
| #1343 | Doc fixes finish in one application (RH8308, RH8309, RH8401, RH8305 at file start) | 1.2.1 | open | partly — #1456 | RH8309-004 narrowed, same meaning | VALID | Its 10-23 instance (RH8305 on the first type of a file) is #1459's input. |
| #1381 | Comment layout when braces are inserted (P32) | 1.2.1 | closed 11-05 | yes — #1535 | — | CLOSED | Resolved by C1 (#1535), triage comment 11-05. |
| #1382 | Token behind a multi-line comment tail (P33) | 1.2.1 | closed 11-05 | yes — #1558/#1570 | — | CLOSED | Not planned (maintainer): code behind a comment's closing line keeps its line (SHARED-COMMENT-012 narrowed, FMT-BREAK-006/-033). |
| #1383 | One treatment per syntax family (P34) | 1.2.1 | open | yes — #1548 | FMT-BREAK-011 rewritten (lambda `(` exception), FMT-INDENT-006 rewritten (FMT-INDENT-030 comment-led lines); FMT-PIPE-014 same | RESHAPED | Attribute-target colon done by #1548 (triage comment 11-05). Open: indexer parameter lists, compound assignment operators, comment before `]`. Body still lists the done part. |
| #1384 | Formatter does not split a documented field declaration (P36) | 1.2.1 | open | yes — #1581 (split transform, new `ReihitsuFormatter.SplitsFieldDeclaration`, FMT-PIPE-031) | FMT-STRUCT-024 same meaning; **FMT-STRUCT-022's own example on main splits a documented declaration** | RESHAPED | Still splits (`Splits()` has no documentation guard; `Documented…AreSplitAtMemberIndentation` tests pin it). Body should note: FMT-STRUCT-022's example contradicts FMT-STRUCT-024 and must change; the guard now also decides whether the RH7101 fix is offered (FMT-PIPE-031, RH7101-005), so RH7101 tests change with it. |
| #1385 | No blank line directly before a directive (formatter part) | 1.2.1 | open | yes — #1541/#1573 | FMT-BLANK-004/-005 reworded (FMT-BREAK-033 context), FMT-BLANK-016 example only | VALID | #1503 triage: still reproduces on `a59a003`. |
| #1388 | Fixes keep the document's line ending (RH5405, RH5011, RH8304, `FormatNode`) | 1.2.1 | open | partly — #1508 (preparation phase), #1456 (`DocumentationCommentCodeFixUtilities`) | RH8304-004 narrowed, SHARED-FIX-007 bullets; SHARED-FIX-008, FMT-PIPE-007 unchanged | VALID | `FormatNode` still detects the line ending from the detached node (`ReihitsuFormatter.cs`, `DetectEndOfLine(node)`); RH5011/RH8304 private line breaks unchanged. |
| #1391 | Indentation misses specified cases | 1.2.1 | open | yes — #1510, #1469 | FMT-INDENT-013 (hole lines), FMT-INDENT-015 (omitted type args), FMT-EOL-001 rewritten | RESHAPED | #1503: omitted type argument and lone-`\r` raw string done; open: base-list continuation (`ICloneable` at column 0) and empty wrapped list `)`. Body still lists four. |
| #1392 | Blank line after a comment before `else`/`catch`/`finally`/`while` | 1.2.1 | closed 11-05 | yes — #1541 | — | CLOSED | Merged into #1543 (FMT-BLANK-020 token list). |
| #1393 | RH5104 fix inserts a blank line before the moved comment (RH5104 part) | 1.2.1 | open | yes — #1457 (R3) | RH5104-005 retitled "No blank line is inserted above the moved comment" | LIKELY RESOLVED | #1457 states "The fix inserts no blank line before the moved comment" and `Refs #1393` ("its RH5104 half is delivered here"); the RH5417 half went to #1443. Reproduce + close. |
| #1395 | Comment before a later declarator moves onto its own line (FMT-STRUCT-022) | 1.2.1 | open | yes — #1581 | FMT-STRUCT-022 unchanged text, but **#1581 added `TrailingBlockCommentAfterSeparatorOnTheDeclaratorsLineIsPreserved` (FMT-STRUCT-022)**: `_a,/* first */ _b` → `_a; /* first */` | RESHAPED | Main now pins the opposite of the round-9 decision for a same-line comment between the comma and the next declarator (Roslyn makes it comma trailing trivia, which FMT-STRUCT-022's first clause keeps behind the field). Maintainer must say which clause wins before a run. |
| #1397 | Line breaks item 16 (FMT-BREAK-005) | 1.2.1 | closed 11-05 | yes — #1570 | — | CLOSED | Settled by D1 (FMT-BREAK-036). |
| #1398 | Break-only section never makes the switch braced | 1.2.1 | open | area — #1535/#1558/#1570 rebuilt the switch phase | FMT-SWITCH-004/-006 unchanged | VALID | #1503: "remains unchanged". |
| #1416 | Brace fixes RH5405–RH5407 keep the wrapped statement's layout (P26) | 1.2.1 | open | no — `StatementBracesCodeFixProviderBase` untouched | SHARED-FIX-007 gained bullets; SHARED-BRACE-002 unchanged | VALID | Same fixes as R8 (#1261) and #1388. |
| #1459 | RH8305 fix makes no progress on the first type of a file | 1.2.1 | open | area — FMT-PIPE-010 now pins that `FormatNodeInDocumentAsync` keeps the leading trivia of a root child | RH8305-004 narrowed, same meaning | VALID | Same input as #1343's 10-23 instance (duplicate instance). The fix cannot rely on the entry point to rewrite the summary of a root child. Moved from 1.2.0 on 11-05. |

### Milestone 1.2.2 (29)

| # | Title (short) | MS | State | Rebuild-affected (PR) | Reqs on main | Verdict | Note |
|---|---|---|---|---|---|---|---|
| #966 | `#` in a block comment blocks reordering; `global::` sort key | 1.2.2 | open | area — #1547 (Core ordering utilities), #1581 (new using entry point) | exist, unchanged | VALID | `UsingDirectiveOrderingSafety.ContainsPreprocessorDirective` and `GetSortKey` unchanged. |
| #974 | RH0001 accepts content after the root object | 1.2.2 | open | no | RH0001-003 unchanged | VALID | |
| #1071 | RH4001, RH4009 compare names by value | 1.2.2 | open | no | SHARED-DIAG-002 unchanged | VALID | |
| #1074 | Spacing rules: whitespace next to literals and in holes is code | 1.2.2 | open | partly — #1457 (RH5601) | RH5601-001 references FMT-TAB instead of FMT-CLEAN (same meaning) | VALID | #1457 names #1074 as the remaining SHARED-DIAG-003 gap. |
| #1079 | RH5603 fix removes the whole whitespace tail | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1084 | Casing: interface named `_` → `I_` still reported | 1.2.2 | open | no | exist, unchanged | VALID | Same files as #1016 (1.2.1) and R5. |
| #1092 | Doc group from the partial type's accessibility (Q3b) | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1105 | Using rules compare names by value | 1.2.2 | open | area — #1547, #1581 | SHARED-DIAG-002 unchanged | VALID | |
| #1147 | Comment-led line not reported (P12) | 1.2.2 | open | partly — #1510 (RH5203/RH5204 use SHARED-BLANK-008) | RH5203-001 now cites SHARED-BLANK-008 (same meaning); FMT-BLANK-011 reworded | VALID | P12 (SHARED-COMMENT-011) untouched by the narrowing of SHARED-COMMENT-012 (#1570). |
| #1152 | RH5204 diagnostic on indented `///` starts at `///` | 1.2.2 | open | no (location logic) | RH5204-005 unchanged | VALID | |
| #1153 | RH8032 without a source location (`extension(long)`) | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1264 | Region rules: innermost declaration and region own a member | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1266 | Using rules compare with every earlier directive of the group | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1267 | Using alias to a non-name forms its own group | 1.2.2 | open | area — #1547 (`UsingGrouping` neighbours) | exist, unchanged | VALID | |
| #1268 | RH2003 resolves the exception type semantically | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1270 | RH8106, RH8203, RH8305, RH8306 check every element | 1.2.2 | open | no | exist, unchanged | VALID | RH8306 shared with R6 (#1287). |
| #1276 | RH5404 extension block reported at its keyword | 1.2.2 | open | no | FMT-BREAK-002 changed only for the part split to #1446 | VALID | |
| #1289 | RH2109 reads Razor correctly | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1333 | RH7403/RH7407/RH7411 every declarator of a field-like event | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1334 | RH7401–RH7412 verdict independent of order/parts | 1.2.2 | open | no | exist, unchanged | VALID | RH74xx shared with R6 (#1287). |
| #1337 | Declaration rules: conditional directive in the judged span | 1.2.2 | open | partly — #1581 rewrote the RH7101 analyzer | RH7101-001 (adds syntax-error exception) and RH7101-005 (asks FMT-PIPE-031) reworded; directive part same meaning | VALID | RH7101 analyzer on main still reports a declaration with `#if` between declarators. |
| #1339 | RH7102/RH7109 check each top-level region on its own | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1345 | RH8301–RH8303 judge a behind-code `///` as if it started its line | 1.2.2 | open | partly — #1456 (RH8301 analyzer), #1532 (FMT-DOC-001) | FMT-DOC-001 retitled "moves onto a new line" (#1532) | VALID | The RH8303 fix should still move the comment "as FMT-DOC-001 does"; re-read FMT-DOC-001 on main. |
| #1346 | RH8401/RH8402 check in every documentation mode | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1347 | RH8307 does not report `/** */` and inline formatting elements | 1.2.2 | open | partly — #1456 (RH8307 analyzer) | FMT-DOC-009/-012 and RH8307-004 reworded, same meaning for these inputs | VALID | |
| #1348 | RH8302 does not report a blank line before `#region`/`#endregion` | 1.2.2 | open | yes — #1573 (formatter follows RH5031 via Core) | FMT-BLANK-013 example added | VALID | Scope extended by #1573's follow-up: FMT-BLANK-010 must state it does not remove the blank line above a region directive (formatter output unchanged). Analyzer part untouched. |
| #1386 | RH5033–RH5049 treat a `///` that documents nothing as a comment | 1.2.2 | open | partly — #1510 named RH5048 as a further instance | exist, unchanged | VALID | Add RH5048 to the instance list. |
| #1387 | Layout rules: conditional directive in a list stops the report | 1.2.2 | open | no | exist, unchanged | VALID | |
| #1412 | RH7105/RH7106 miss the declaring part of a partial constructor | 1.2.2 | open | no | exist, unchanged | VALID | Source-audit report; needs reproduction (may end as characterization). |

### Counts

| Verdict | 1.2.1 | 1.2.2 | Issues |
|---|---|---|---|
| VALID | 27 | 29 | — |
| RESHAPED | 11 | 0 | #1076 #1094 #1140 #1144 #1150 #1239 #1335 #1383 #1384 #1391 #1395 |
| LIKELY RESOLVED | 2 | 0 | #952 #1393 |
| SUPERSEDED | 1 | 0 | #1093 (→ #1445) |
| CLOSED | 5 | 0 | #958 #1381 #1382 #1392 #1397 |

Before the runs that hold them, the RESHAPED issues need a re-description the maintainer approves; three of them carry a real question: **#1150** (withhold the fix vs. "where an analyzer reports, a fix is offered"), **#1395** (main's new FMT-STRUCT-022 test pins the opposite of the decision), **#1384** (FMT-STRUCT-022's example splits a documented field). #1093 needs a close/merge decision.

## 2. New run plan

All runs claim the **bug-fix lane**. The 1.2.1/1.2.2 split logic is kept: 1.2.1 = fix scope, convergence and EOL in shared fix bases plus user-visible formatter output; 1.2.2 = analyzer report scope and small singletons. The rebuild does not change that logic; it only removes R10/R11/R12's cause (most of their issues closed or narrowed) and moves the formatter runs into smaller, phase-aligned groups. Only change: #1084 (1.2.2) shares every file with #1016 (1.2.1) → proposed to move it to 1.2.1 (run P10); otherwise P10 has two issues.

Paths: `FMT/` = `src/Reihitsu.Formatter/Pipeline/`, `CF/` = `src/Reihitsu.Analyzer.CodeFixes/` (`CF/Base/`, `CF/Core/`, providers as `RH#### fix`), `AN/` = `src/Reihitsu.Analyzer/`, `Core/` = `src/Reihitsu.Core/`. A rule's analyzer and fix tests share `src/Reihitsu.Analyzer.Test/**/RH####…Tests.cs`, so an analyzer run and a fix run on the same rule ID overlap.

### 1.2.1 (11 runs, 40 issues)

| Run | Issues | Cause / area | Main files likely touched | Prerequisite |
|---|---|---|---|---|
| P1 | #963 #1384 #1395 | Formatter field split keeps comments and documentation (FMT-STRUCT-022/-024) | `FMT/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs`; `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (`SplitsFieldDeclaration`); `FieldDeclarationSplitTests`; RH7101 fix tests; `sdd/specs/formatter/structural-transforms.md`, `analyzer/RH7101.md` | #1384, #1395 re-described/decided |
| P2 | #1243 #1383 #1149 #1391 #952 | Formatter line breaks and indentation miss specified cases | `FMT/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs`, `TernaryLineBreakRewriter.cs`, `LineBreakAssignmentRewriter.cs`, `LineBreakListRewriter.cs`; `FMT/Indentation/Contributors/BaseTypeListContributor.cs`, `ArgumentAlignmentContributor.cs`, `CommentIndentationContributor.cs`, `FMT/Indentation/Utilities/LayoutComputer.cs`; RH5111 tests (#1149 regression); `line-breaks.md`, `indentation.md` | #1383, #1391 bodies narrowed |
| P3 | #1385 #1398 #1239 | Formatter blank-line / switch / region decisions against their requirement | `FMT/BlankLines/Rewriter/BlankLineStatementSpacingRewriter.cs`, `BlankLineBreakSpacingRewriter.cs`, `FMT/BlankLines/Utilities/BlankLineEditor.cs`; `FMT/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs`; `FMT/RegionFormatting/Utilities/RegionNamingRewriter.cs`; `blank-lines.md`, `switch-case-braces.md`, `region-formatting.md` | #1239 reading confirmed (FMT-REGION-007) |
| P4 | #946 #972 #1096 #1076 | Statement / brace blank-line fixes: one blank line, converge, no undo | `CF/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs`, `BlankLineSpanRemovalCodeFixProviderBase.cs`; `CF/Core/BlankLineCodeFixUtilities.cs`; `Core/SyntaxTriviaUtilities.cs` (`ContainsBlankLine`), `Core/TokenGapAnalysis.cs`; RH5005/06, RH5011, RH5022–RH5027 fixes/analyzers; `AN/Base/StatementShouldBe…AnalyzerBase{TStatement}.cs` | after R8; #1076 body trimmed |
| P5 | #1416 #1388 #1142 | Brace and attribute-list fixes keep the wrapped layout and the document's line ending | `CF/Base/StatementBracesCodeFixProviderBase.cs` (RH5405–07), RH5011 fix, RH8304 fix, `CF/Core/DocumentationCommentCodeFixUtilities.cs` (`GetLineBreak`), `CF/Base/TargetAttributeListShapeCodeFixProviderBase.cs`; `ReihitsuFormatter.cs` + `ReihitsuFormatterHelpers.DetectEndOfLine` (`FormatNode`); fix test bases (CRLF check) in `src/Reihitsu.Analyzer.Test/Base/` | after R8, P4 |
| P6 | #1140 #1141 #1144 #1145 #1094 #1393 | Line-break, alignment, spacing and placement fixes change only their code and lay it out as the formatter does (P9/P10) | RH5101, RH5102, RH5103, RH5104, RH5110, RH5111, RH5113, RH5203–RH5206 fixes; `CF/Core/FluentChainCodeFixHelper.cs`; `CF/Base/OperatorSpacingCodeFixProviderBase.cs` (RH6023/24); `CF/Base/TargetAttributePlacementCodeFixProviderBase.cs` (RH5513) | #1140, #1144, #1094 re-described |
| P7 | #969 #1335 #1263 #1151 | Organization and region fixes change only what they move or remove | `CF/Base/TypeMemberOrderingCodeFixProviderBase.cs`, `ModifierOrderingCodeFixProviderBase.cs`; RH7004, RH7104, RH7105/06, RH7107, RH7108, RH7302, RH7303, RH7310 fixes; `Core/RegionDirectiveUtilities.cs` | after R4, R6; #1335 trimmed |
| P8 | #1269 #1342 #1343 #1459 | Documentation fixes: only the reported markup, one application | RH8201, RH8202, RH8204, RH8305, RH8307, RH8308, RH8309, RH8401, RH8402 fixes; `CF/Core/DocumentationCommentCodeFixUtilities.cs` | — |
| P9 | #1283 #1284 #1285 | Clarity fixes: EOL, body-only formatting, RH3001 positions | RH3001, RH3006, RH3202–RH3208 fixes; `CF/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | after R7 |
| P10 | #1016 #1015 #1084 (moved) | Casing fixes converge in one application, Fix All covers every event kind | `Core/CasingUtilities.cs`; `CF/Base/CasingCodeFixProviderBase{T}.cs`; RH4002, RH4005, RH4102 (both providers), RH4105–RH4107 fixes | after R5; #1084 → 1.2.1 |
| P11 | #1150 #1274 #1143 | Convergence: no no-op fix, one application, Fix All in one pass | every formatter-delegating provider (#1150), RH5308 fix, the Fix-All test classes (`BatchCodeFixTestsBase` subclasses, `NumberOfFixAllIterations`) | last: after P5, P6, P8, P9; #1150 decided |

Not planned: #1093 (SUPERSEDED → #1445), and the 5 CLOSED.

### 1.2.2 (6 runs, 28 issues)

| Run | Issues | Cause / area | Main files likely touched | Prerequisite |
|---|---|---|---|---|
| Q1 | #1147 #1152 #1386 #1387 | Layout analyzers' report scope (comment-led line, `///` location, comment kinds, directive in a list) | RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5308, RH5204 analyzers; `AN/Base/CommentPositionAnalyzerBase.cs` (RH5033–RH5049); RH5102, RH5109, RH5111, RH5501–RH5531 analyzers (`AN/Base/TargetAttribute*AnalyzerBase.cs`) | after P6 (shared RH51xx/52xx tests); not with P5/P11 (RH55xx tests) |
| Q2 | #1264 #1333 #1334 #1339 #1337 #1412 | Organization analyzers' verdicts | RH7101, RH7102, RH7105–RH7109, RH7308/09/11, RH7401–RH7412 analyzers; `AN/Base/MembersShouldBeGroupedByRegionsAnalyzerBase.cs`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs`; `Core/RegionCategoryUtilities.cs` | after R4, R6, P7 |
| Q3 | #966 #1105 #1266 #1267 | Using ordering: names by value, earlier directive of the group, alias to a non-name, `#` in a comment | `Core/UsingDirectiveOrderingSafety.cs`, `Core/UsingDirectiveOrderingUtilities.cs`; `FMT/UsingDirectives/Utilities/UsingGrouping.cs`; RH7201–RH7207 analyzers; `CF/Base/UsingDirectiveOrderingCodeFixProviderBase.cs` | — |
| Q4 | #1345 #1346 #1347 #1348 #1270 #1092 #1153 | Documentation analyzers' scope | RH8001/02/17/18, RH8032, RH8106, RH8203, RH8301–RH8303, RH8305–RH8307, RH8401/02 analyzers; `AN/Core/DocumentationAnalysisUtilities.cs`; `blank-lines.md` (FMT-BLANK-010 clarification, #1348) | after R6, P8, P3 |
| Q5 | #1071 #1268 #1289 #974 | Naming/design analyzers: names by value/alias, Razor text, config root | RH4001, RH4009, RH2003, RH2109, RH0001 analyzers | after R5 (weak: naming analyzers) |
| Q6 | #1074 #1079 #1276 | Small layout/spacing singletons | RH5601, RH5602, RH6009, RH6010 analyzers/fixes; RH5603 fix; RH5404 analyzer | — |

### File-overlap check

Pool runs against the pending 1.2.0 runs (R4 running: ordering/region/modifier fixes incl. RH7004, RH7101, RH7105/06, RH7109, RH7303, RH7309; R5 casing; R7 RH3001–RH3201 fixes, RH5417; #1458 RH5202 fix; R6 after R4: RH2001/2004/2005, RH7104 fix, RH74xx/RH8306 partials; R8 after R7: blank-line rule/fix bases, RH5405–07, RH5412–17):

| Run | Overlaps with (`w` = weak / probable) |
|---|---|
| P1 | R4 (RH7101 fix tests), P7 (w, RH7101 listed in #1335), P5 (w, `ReihitsuFormatter.cs`) |
| P2 | P6 (w, RH5111 tests via #1149) |
| P3 | Q4 (`blank-lines.md`), P7 (w, region utilities) |
| P4 | R8, P5 (RH5011) |
| P5 | R8 (RH5405–07), P4, P8 (`DocumentationCommentCodeFixUtilities`), P9 (fix test base CRLF), P11, Q1 (RH55xx tests), P1 (w) |
| P6 | Q1 (RH51xx/52xx tests), P11, P2 (w), #1458 (w, RH52xx raw strings) |
| P7 | R4, R6 (RH7104), Q2, P1 (w), P3 (w) |
| P8 | P5, Q4 (RH8305/07/8401/02 tests) |
| P9 | R7 (RH3001), P5, P11 |
| P10 | R5, Q5 (w) |
| P11 | P4, P5, P6, P8, P9 (all delegating fixes) |
| Q1 | P5, P6, P11 |
| Q2 | R4, R6, P7 |
| Q3 | — |
| Q4 | R6 (RH8306), P3, P8 |
| Q5 | R5 (w), P10 (w) |
| Q6 | — |

### Waves (≤ 6 parallel, no shared files, no dependency inside a wave)

| Wave | Runs | Note |
|---|---|---|
| W-A | R4 (running) · R5 · R7 · #1458 · **P2** · **P3** | 1.2.0 continues; formatter runs P2/P3 share nothing with them |
| W-B | R6 (after R4) · R8 (after R7) · **P6** · **P8** · **Q3** · **Q6** | P6/P8 untouched by R6/R8 |
| W-C | **P1** (after R4) · **P4** (after R8) · **P7** (after R4, R6) · **P9** (after R7) · **P10** (after R5) · **Q1** (after P6) | P1↔P7 weak only (RH7101 instance of #1335 resolved; P7 leaves RH7101) — otherwise put P1 into W-D |
| W-D | **P5** (after R8, P4) · **Q2** (after P7) · **Q4** (after R6, P3, P8) · **Q5** (after R5, P10) | |
| W-E | **P11** | last: #1143 needs P5/P6 fixes; #1150 touches every delegating fix |

1.2.1 runs: P1–P11 (11). 1.2.2 runs: Q1–Q6 (6). Gating outside the waves: the re-descriptions of the 11 RESHAPED issues (questions on #1150, #1395, #1384), the #1093 close/merge, and the #1084 milestone move.
