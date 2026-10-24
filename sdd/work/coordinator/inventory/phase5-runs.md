# Phase 5 — squad runs, overlap matrix, waves

Source: `phase5-consolidated.md` (A, B, C, H, J, K, M), `phase5-f1.md` … `phase5-f8.md` (section 5, Groups), `squad/README.md` (Lanes; Several issues in one run), file names grepped in `src/` on 2026-10-24. Every run claims the **bug-fix lane**, no change doc (C issues included, as decided). Split parts that are D (behavior change) stay outside every run (list in section 4).

Paths are abbreviated: `FMT/` = `src/Reihitsu.Formatter/Pipeline/`, `CF/` = `src/Reihitsu.Analyzer.CodeFixes/` (`CF/Base/`, `CF/Core/`, providers as `RH####` fix), `AN/` = `src/Reihitsu.Analyzer/` (analyzers as `RH####` an.), `Core/` = `src/Reihitsu.Core/`.

## 1. Milestone 1.2.0 — critical set (34 issues, 8 runs)

| Run | Issues | Cause / code area | Main files likely touched | Requirement IDs | Blockers | J to decide first |
|---|---|---|---|---|---|---|
| R1 | #948 #949 #954 #1390 #888 | Formatter output breaks the build or is not idempotent (switch bracing, blank lines vs line breaks, expression-body converter, chain break behind comment tail) | `FMT/SwitchCaseBraces/` (SwitchCaseBraceRewriter); `FMT/BlankLines/` (BlankLineStatementSpacingRewriter, BlankLineRegionDirectiveRewriter, BlankLineEditor); `FMT/LineBreaks/Rewriter/` (LineBreakBlockRewriter, ChainLineBreakRewriter); `FMT/StructuralTransforms/` (ExpressionBodiedMethodTransform, ExpressionBodyToBlockConverter, ExpressionBodiedTransformUtilities); `Core/TokenGapAnalysis.cs`; RH5201 an./fix, `CF/Core/FluentChainCodeFixHelper.cs` | FMT-PIPE-003/-004, FMT-SWITCH-011, FMT-BLANK-011, FMT-STRUCT-006, SHARED-COMMENT-012, SHARED-FIX-001 | — (#888: rewrite to the real defect, formatter break before `.D`) | **J19** (#888: does SHARED-COMMENT-012 bind the formatter/RH5201) |
| R2 | #1329 #1389 #1241 #1344 | Doc comments behind code / element without end tag turn code into comment or lose text (formatter doc phase + doc fixes) | `FMT/DocumentationComments/` (DocumentationCommentFormattingPhase, DocCommentElementNormalizer); `FMT/Cleanup/CleanupPhase.cs` (BOM); `CF/Core/DocumentationCommentCodeFixUtilities.cs`; RH8204, RH8304, RH8305, RH8307, RH8309 fixes; `Core/DocumentationCommentUtilities.cs`, `Core/XmlDocumentationElementOrderingUtilities.cs` | FMT-DOC-*, FMT-CLEAN-009, FMT-PIPE-002, SHARED-DOC-017, SHARED-FIX-003 | Owner of `DocumentationCommentCodeFixUtilities` (#1242) decided first (M) | — |
| R3 | #951 #1133 #1146 | Literal / raw-string content treated as layout (raw-string idempotency, RH5601 closing-line tab, RH5104 comment into literal) | `FMT/RawStringAlignment/`, `FMT/Indentation/Utilities/` (IndentationRewriter, LayoutComputer); `Core/FormattingTextAnalysisUtilities.cs` (GetStringLineIndices), `Core/RawStringLiteralUtilities.cs`, `Core/StringInterpolationUtilities.cs`; RH5601 an./fix; RH5104 fix; RH5204 an. (shared helper) | FMT-PIPE-004, FMT-RAW-*, RH5601-002, RH5104-*, SHARED-FIX-003, SHARED-DIAG-003 | #1146 Q5 → #951 (same run); coordinate with #1075 (R8, sequential) | **J4** (#1133), **J1** (#1146) |
| R4 | #1134 #1135 #1262 #1332 #1336 #1340 | Organization fixes that break code or change meaning must withhold (SHARED-FIX-003) | `CF/Base/TypeMemberOrderingCodeFixProviderBase.cs`, `CF/Base/ModifierOrderingCodeFixProviderBase.cs`; RH7004, RH7101, RH7105, RH7106, RH7109, RH7303, RH7304, RH7306, RH7309, RH7501 fixes; RH7003 an. (no provider in repo); `Core/OrderingMoveSafety.cs`, `Core/RegionDirectiveUtilities.cs`, `Core/DeclarationModifierUtilities.cs`, `Core/ModifierOrderingUtilities.cs` | SHARED-FIX-003, RH7003-001, RH7501-004, RH7309-004, SHARED-ORDER-* | #1136 related, not blocking (K) | **J12** (#1262) |
| R5 | #1016 #1070 #1073 | Casing fixes produce a still-reported or uncompilable name | `CF/Base/CasingCodeFixProviderBase{T}.cs`; RH4xxx casing fixes; `Core/CasingUtilities.cs`; `AN/Base/CasingAnalyzerBase.cs` | SHARED-FIX-001/-003, SHARED-CASING-008 | — | — |
| R6 | #967 #1286 #1287 #1288 | Design modifier fixes (RH2001/2004/2005) break the build; partials | RH2001, RH2004, RH2005 fixes + an.; RH7104 fix; `Core/DeclarationModifierUtilities.cs` | SHARED-FIX-003/-007, SHARED-DIAG-004/-011, RH2004-002 | lane owner of #1287 (M) | **J17** (#1287) |
| R7 | #1257 #1279 #1281 #1282 #1417 #1422 | Clarity fixes that break code / delete comments; reports where the form is invalid (P38) | RH3001, RH3004, RH3005, RH3101, RH3103, RH3104, RH3105, RH3106, RH3107, RH3201 fixes; RH3103, RH3104, RH5417 an.; `AN/Base/EmptyParenthesesAnalyzerBase{TNode}.cs` | SHARED-FIX-003/-007, RH3004-003, SHARED-DIAG-011, RH3103-001 | — (#1281 RH3103 part superseded by #1417, same run) | **J14** (#1417) |
| R8 | #946 #1075 #1261 | Layout fix bases (statement blank line, brace blank line, braces/empty type) do not converge or edit literal/comment/directive text | `CF/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs`, `BlankLineSpanRemovalCodeFixProviderBase.cs`, `StatementBracesCodeFixProviderBase.cs`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs`; `CF/Core/BlankLineCodeFixUtilities.cs`; RH5005/06, RH5011, RH5025, RH5027, RH5030, RH5405–07, RH5412–17 fixes; possibly `Core/FormattingTextAnalysisUtilities.cs` | SHARED-STMT-004, SHARED-FIX-001/-003, RH5011/5025/5027/5030-* | #1261 needs reproduction first (M) | **J5** (#1261) |

## 2. Bug pool (71 issues, 16 runs)

Pool = B 43 (without #946 #888 #1016) + non-critical C 15 (incl. #1384) + bug parts of H 13 (#1353 has none).

| Run | Issues | Cause / code area | Main files likely touched | Requirement IDs | Blockers | J to decide first |
|---|---|---|---|---|---|---|
| R10 | #1384 #1395 #963 (field part) #1381 #1398 | Formatter structural/switch rewrites lose or move comments/docs (field split, brace insertion, break-only section) | `FMT/StructuralTransforms/Rewriter/` (FieldDeclarationSplitTransform, ControlFlowBraceTransform), `ExpressionBodiedTransformUtilities.cs`; `FMT/SwitchCaseBraces/` | FMT-STRUCT-022/-024, FMT-PIPE-013, FMT-SWITCH-006 | after R1 (same files) | **J3** (#1398) |
| R11 | #1392 #1385 (formatter part) #952 #1391 #1239 | Formatter blank-line, indentation, raw and region-line cases missed | `FMT/BlankLines/` (BlankLineTriviaBoundaryRewriter, BlankLineStatementSpacingRewriter); `FMT/Indentation/` (IndentationRewriter, LayoutComputer, Contributors); `FMT/RawStringAlignment/`; `FMT/RegionFormatting/` | FMT-BLANK-008/-016, FMT-INDENT-009/-012/-013/-015/-016, FMT-RAW-001, FMT-REGION-001 | after R1, R3 | **J20** (#1385) |
| R12 | #1382 #1383 #958 (arrow part) #1397 (item 16) #1243 #1093 (formatter `[param: A] set;`) | Formatter line breaks: token behind comment tail (P33), one treatment per family (P34), joins | `FMT/LineBreaks/Rewriter/` (ChainLineBreakRewriter, LineBreakAssignmentRewriter, BinaryOperatorLineBreakRewriter, LineBreakListRewriter, ListElementInteriorJoinRewriter, AttributeTargetFormattingRewriter); `FMT/HorizontalSpacing/`; RH5203 an. | SHARED-COMMENT-012, FMT-PIPE-014, FMT-BREAK-005/-017/-018/-027 | after R1 (#888 same cause); #1243 blocks #1273 (D) | **J19**, **J16** (#1382) |
| R13 | #1140 #1144 #1145 #1393 (RH5104 part) | Line-break/alignment fixes format more than their node or not as the formatter (FIX-SCOPE) | RH5101, RH5102, RH5103, RH5105, RH5104 fixes (RH51xx/52xx delegating); `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (FormatNodeInDocumentAsync) | SHARED-FIX-006/-007, RH5103-003/-004, RH5104-005 | after R3 (RH5104 fix) | **J2** (#1140), **J1** (#1393) |
| R14 | #1147 #1152 #1386 #1387 | Layout analyzers' report scope: comment-led line, indented `///`, comment kinds, directive in list | RH5110, RH52xx, RH53xx, RH5102/5109/5111, RH55xx an. (`AN/Base/TargetAttribute*AnalyzerBase.cs`), RH5033–5049 an. (`AN/Base/CommentPositionAnalyzerBase.cs`), RH5204 an. | SHARED-COMMENT-001/-011, SHARED-DIAG-008, RH5204-005 | after R12 (RH5203); #1147 before #1271–#1273 (D) | **J16** (#1147), **J8** (#1387) |
| R15 | #972 #1096 #1076 #1416 | Statement/brace blank-line and brace fixes: two blank lines, indent, no-op/undo, wrapped layout | `CF/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs`, `BlankLineSpanRemovalCodeFixProviderBase.cs`, `StatementBracesCodeFixProviderBase.cs`; RH5001–5019, RH5011, RH5022–27, RH5032, RH5405–07 fixes | SHARED-STMT-004, RH5011-001, SHARED-FIX-001/-004/-005/-007 | #1416 after #1261 (R8); #1076 RH5024 instance: verify | **J5** (#1416) |
| R16 | #1150 #1274 #1015 #1143 | Convergence: fixes withheld when formatter returns input, one application, Fix All in one pass | ~39 FormatNode-delegating providers (e.g. RH3202, RH5101, RH5204, RH5308, RH5417 fixes); RH5308 fix; RH4102 fixes; 26 test classes (`BatchCodeFixTestsBase`) | SHARED-FIX-001/-002/-005 | **#1143 → #1140 (R13), #1141, #1142 (R17)** | — |
| R17 | #1094 #1141 #1142 #1388 | Attribute-list and operator-spacing fixes change more than their gap; fixes lose line ending | `CF/Base/TargetAttributeListShapeCodeFixProviderBase.cs`, `TargetAttributePlacementCodeFixProviderBase.cs`, `OperatorSpacingCodeFixProviderBase.cs`; RH5405, RH5011, RH8304 fixes; `ReihitsuFormatter.cs` (FormatNode); `Core/LineEndingUtilities.cs` | SHARED-ATTR-017, SHARED-FIX-006/-007/-008, FMT-PIPE-007 | after R8 (RH5405/RH5011), R2 (RH8304) | **J6** (#1388) |
| R18 | #969 #1335 #1263 #1151 | Organization and region fixes reformat the whole type / leave blank line / rename `#endregion` loosely | `CF/Base/TypeMemberOrderingCodeFixProviderBase.cs`, `ModifierOrderingCodeFixProviderBase.cs`; RH7004, RH7101, RH7102–09 fixes; RH7302, RH7303, RH7310 fixes | SHARED-FIX-006/-007, RH7302-005 | after R4 (same bases) | — |
| R19 | #1264 #1333 #1334 #1339 #1337 #1412 | Organization analyzers' verdicts: region ownership, declarators, base order, top-level regions, directive lists, partial ctor | RH7101, RH7102, RH7105–08, RH7109, RH7308/09/11, RH7401–7412 an.; `AN/Base/MembersShouldBeGroupedByRegionsAnalyzerBase.cs`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs`; `Core/RegionCategoryUtilities.cs` | SHARED-TYPEREGION-006, SHARED-DIAG-004/-006/-008/-009, RH7101-001, RH7102-003, RH7109-003 | after R4, R18; #1412 needs reproduction | **J8** (#1337), **J10** (#1333), **J11** (#1334) |
| R20 | #966 #1105 #1266 #1267 | Using ordering: name by value, earlier directive of group, alias to non-name, `#` in comment | `FMT/UsingDirectives/`; `CF/Base/UsingDirectiveOrderingCodeFixProviderBase.cs`, `CF/Core/UsingDirectiveCodeFixUtilities.cs`; `Core/UsingDirectiveOrderingUtilities.cs`, `Core/UsingDirectiveOrderingSafety.cs`; RH7201–7207 an. | SHARED-DIAG-002, SHARED-USING-004/-007/-009 | #965 (D) after this run | **J9** (#1266) |
| R21 | #1269 #1342 #1343 | Doc fixes format more than the comment, need several applications, lose EOL | RH8201, RH8202, RH8204, RH8305, RH8307, RH8308, RH8309, RH8401, RH8402 fixes; `CF/Core/DocumentationCommentCodeFixUtilities.cs` | SHARED-FIX-001/-005/-006/-008, SHARED-DOC-004 | after R2; #1242 decided | — |
| R22 | #1345 #1346 #1347 #1348 #1270 #1092 (Q3b) #1153 (RH8032 location) | Doc analyzers' scope: behind-code `///`, every doc mode, `/** */`, every element, partial doc group, missing location | RH8001/02/17/18, RH8032, RH8106, RH8203, RH8301–8303, RH8305–8307, RH8401/02 an.; `AN/Core/DocumentationAnalysisUtilities.cs`, `AN/Base/DocumentationModeAnalysisContextExtensions.cs` | SHARED-DOC-004/-008, SHARED-DIAG-004/-006/-007/-010, SHARED-FIX-004 | after R6 (J17 partials) | **J13** (#1345), **J17** (#1092) |
| R23 | #1283 #1284 #1285 | Clarity fixes: keep EOL, expression-body fixes format only the body, RH3001 in list positions | RH3001, RH3006, RH3202–3208 fixes; `CF/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | SHARED-FIX-006/-008, RH3001-004 | after R7 (RH3001); check #1388 (R17) covers RH3006 | **J7** (#1284) |
| R24 | #1071 #1268 #1084 (`_`→`I_`) #1289 (Q1–Q3) #974 | Naming/design analyzers: names by value/alias, Razor text, casing of `_`, config root | RH4001, RH4009, RH2003, RH2109, RH0001 an.; RH4005, RH4121 fixes; `Core/CasingUtilities.cs` | SHARED-DIAG-002/-003, SHARED-CASING-008, RH0001-003 | after R5 (CasingUtilities) | **J15** (#1071, #1268) |
| R25 | #1074 (RH6009/6010) #1079 (RH5603) #1276 (`Location.None`) | Small layout/spacing rule bugs: literal whitespace, RH5603 convergence, missing location | RH6009, RH6010 an./fix; RH5603 fix; RH5404 an. | SHARED-DIAG-003/-007, SHARED-FIX-001 | — | — (J4 only for the excluded RH5601/5602 part) |

### Proposed split 1.2.1 / 1.2.2

| Run | Milestone | Why |
|---|---|---|
| R10 | 1.2.1 | Same phases as R1; #1384 borderline critical (CS1591), comment moves |
| R11 | 1.2.1 | Same blank-line / raw cause as R1 (#949 #954) and R3 (#951) |
| R12 | 1.2.1 | Same P33 cause as #888 (R1); user-visible formatter output |
| R13 | 1.2.1 | Fix scope SHARED-FIX-006/-007 |
| R14 | 1.2.2 | Analyzer false negatives only; no fix base |
| R15 | 1.2.1 | Shares R8 fix bases; convergence SHARED-FIX-001/-005 |
| R16 | 1.2.1 | Convergence SHARED-FIX-001/-002/-005 across shared bases (last: blocked by R13, R17) |
| R17 | 1.2.1 | Fix scope SHARED-FIX-006 + EOL SHARED-FIX-008 in shared bases |
| R18 | 1.2.1 | Fix scope SHARED-FIX-006/-007; shares R4 bases |
| R19 | 1.2.2 | Analyzer verdict scope; no fix base |
| R20 | 1.2.2 | Self-contained using area; ordering differences, no code loss |
| R21 | 1.2.1 | Doc fix scope / convergence / EOL; shares R2 helper |
| R22 | 1.2.2 | Analyzer scope only |
| R23 | 1.2.1 | EOL SHARED-FIX-008 + fix scope SHARED-FIX-006; shares R7 cause (RH3001) |
| R24 | 1.2.2 | Analyzer report scope; singletons |
| R25 | 1.2.2 | Small singletons, low impact |

1.2.1: 10 runs (R10 R11 R12 R13 R15 R16 R17 R18 R21 R23). 1.2.2: 6 runs (R14 R19 R20 R22 R24 R25).

## 3. File overlaps and waves

Overlapping pairs (must not run in parallel; `(w)` = weak, probable shared helper):

| Run | Overlaps with |
|---|---|
| R1 | R10, R11, R12 |
| R2 | R17 (RH8304 fix), R21 |
| R3 | R8 (FormattingTextAnalysisUtilities), R11, R13 (RH5104 fix), R25 (w) |
| R4 | R6 (DeclarationModifierUtilities), R18, R19 |
| R5 | R16 (RH4102 casing base), R24 (CasingUtilities) |
| R6 | R4, R19 (w, partial helpers), R22 (J17 partials) |
| R7 | R8 (RH5417), R23 (RH3001) |
| R8 | R3, R7, R15, R17, R23 (BlankLineCodeFixUtilities via RH3208) |
| R10 | R1 |
| R11 | R1, R3 |
| R12 | R1, R14 (RH5203) |
| R13 | R3, R16 |
| R14 | R12 |
| R15 | R8, R16, R17 (RH5011, RH5405) |
| R16 | R5, R13, R15, R17, R21, R23 (delegating providers) |
| R17 | R2, R8, R15, R16, R21 (doc fix helper), R23 (FormatNode EOL) |
| R18 | R4, R19 (w, RH7102/7109) |
| R19 | R4, R6 (w), R18 (w), R22 (w, partial helpers) |
| R20 | R24 (w, SHARED-DIAG-002 name helper) |
| R21 | R2, R16, R17 |
| R22 | R6, R19 (w) |
| R23 | R7, R8, R16, R17 |
| R24 | R5, R20 (w) |
| R25 | R3 (w) |

Waves (≤ 6 parallel; no overlapping pair and no blocker in the same wave):

| Wave | Runs | Note |
|---|---|---|
| W1 | R1 R2 R3 R4 R5 R7 | 1.2.0 |
| W2 | R6 R8 · R10 R11 R12 R13 | 1.2.0 completes; 1.2.1 fills spare slots (R10–R12 after R1, R11/R13 after R3) |
| W3 | R15 R18 R21 R23 · R14 R20 | 1.2.1 (R15 after R8/#1261, R18 after R4, R21 after R2, R23 after R7); 1.2.2 fillers |
| W4 | R17 · R19 R24 R25 | R17 apart from R15/R21/R23; R19 after R18, R24 after R20 |
| W5 | R16 · R22 | R16 last (#1143 after #1140/#1141/#1142); R22 apart from R19 |

A critical run whose J item is still open slips to the next wave; W1 needs J4, J1, J12, J14, J19 answered, W2 needs J17, J5, J3, J20, J16, J2.

## 4. Unplaced issues and lane doubts

| Issue | Status |
|---|---|
| #1353 | Unplaced: no bug part (Q1/Q2 → E clarification, Q3 → D) |
| #950, #889, #1259, #1244, #1330 | Not in the pool (F internal); optional ride-alongs in R1/R10, R1/R12, R7, (#1244 closes with #1271, D), R3 |
| D split parts (outside runs) | #951 case 3, #1146 Q3/Q5/Q6, #1070 no-report part, #1288 `public` operators, #1279 `static`/parentheses, #1332 Q10, #1382 RH5204/SHARED-INIT-001, #1334 Q6, #1094 Q3/Q4, #1385 fix part, #1093/#1074/#1079/#1276/#1092/#1153/#1084/#1289/#958/#1397/#963 remainders |
| #1393 (R13) | Bug only if J1 goes to SHARED-FIX-006; otherwise a spec change |
| #1416 (R15) | Bug only if J5 goes to SHARED-FIX-007; otherwise behavior change |
| #1385 formatter part (R11) | Depends on J20 |
| #1287 (R6) | Changes RH2004-002 text; behavior change if J17 widens SHARED-DOC-002 coverage |
| #1243 (R12) | No requirement ID: needs an FMT-BREAK clarification by the spec author (bug lane allows it) or becomes behavior change |
| #966 case 2 (R20) | `global::` in sort key undecided; may be D |
| #888 (R1), #1382 (R12) | Bug lane only if J19 binds SHARED-COMMENT-012 to the formatter/RH5201 |
| #1261 (R8), #1412 (R19), #1076 RH5024 instance (R15) | Need reproduction; may end as characterization test (`Refs`) |
