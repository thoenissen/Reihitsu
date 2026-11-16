# Phase 6 slice 6 (issues #1293–#1451)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1293 | vNext | analyzer/RH8106, RH8107 documentation | documentation-comment rules | open | — | S | yes | — | no | RH8106-001 leaves `void` outside coverage, but the analyzer still reports `<returns/>` on void (no return-type check). |
| 1294 | vNext | analyzer/RH8203 documentation | documentation-comment rules | open | — | S | yes | — | no | RH8203-001 coverage still excludes base constructors differing in `ref`/`in`/`out`/`params` or `private`. |
| 1295 | vNext | analyzer/RH1001, RH1002 performance | struct-equality rules | open | — | M | yes | #1290 | no | RH1001-001 still leaves collection expressions out; no Nullable/tuple look-through in spec; #1260 (blocker) closed. |
| 1296 | vNext | analyzer/RH1002 performance | struct-equality rules | open | — | M | yes | — | no | RH1002-001 method list lacks CountBy/AggregateBy/LeftJoin/RightJoin/Create*; query clauses explicitly excluded (RH1002.md:28). |
| 1297 | vNext | analyzer/RH1002 + Base/EqualityComparerArgumentUtilities | struct-equality rules | open | — | S | yes | — | no | Helper matches any `IEqualityComparer<>` parameter without its type argument; SHARED-EQUAL-002 leaves other `T` unspecified. |
| 1298 | vNext | analyzer/RH2006, RH2007 design | rule report scope | open | — | S | yes | — | no | SHARED-DEBUG-001 coverage still excludes `string.Empty` and all-empty conditionals; the `const` instance is already reported. |
| 1299 | vNext | analyzer/RH2001 design | rule report scope | open | — | M | yes | — | no | RH2001-001 coverage excludes any attribute and `field` bodies; behavior unchanged. |
| 1300 | vNext | analyzer/RH2002 + code fix | rule report scope | open | — | S | yes | — | no | RH2002-001 still covers only non-explicit implementations. |
| 1330 | Maintenance | tests/Core (FormattingTextAnalysisUtilities) | test infrastructure | partly done | `FormattingTextAnalysisUtilitiesTests.GetStringLineIndicesReturnsLinesForAllSupportedStringVariants` covers multi-line interpolated raw and single-line `u8` | S | no | — | no | Remains: multi-line UTF-8 raw literal and `///`/whitespace-only content lines through `GetNonFormattableLineIndices`. |
| 1338 | vNext | analyzer/RH7105, RH7106 organization | rule report scope | open | — | S | yes | — | no | SHARED-MODORDER-001 still lists no enum, destructor, local function or accessor. |
| 1341 | vNext | analyzer/RH8301–RH8309, RH8401 documentation | documentation-comment rules | open | — | L | yes | — | no | Specs mark body/stranded `///` "not covered"; nine rules move together; #1139 blocker closed. |
| 1349 | vNext | analyzer/RH8306 documentation | documentation-comment rules | open | — | S | yes | — | no | RH8306-001: "Indexers are not covered." |
| 1350 | vNext | analyzer/RH8501 documentation | rule report scope | open | — | S | yes | — | no | RH8501 spec still excludes `UnconditionalSuppressMessage` and `<Pending>`. |
| 1351 | vNext | analyzer/RH8308 documentation | documentation-comment rules | open | — | S | yes | — | no | RH8308-001 coverage excludes text separated by an XML comment. |
| 1352 | vNext | analyzer/RH8309 documentation + fix | documentation-comment rules | open | — | M | yes | — | no | RH8309-001 still excludes `/** */`; custom-element move not specified. |
| 1353 | Maintenance | specs only (RH8402) | spec clarification | partly done | RH8402-006 states run replacement for all-`copyright` runs; Q3 split to #1450 | S | no | — | no | Remains: spec text for a run that also holds a comment without `copyright` (Q2), which RH8402-006 leaves "not covered". |
| 1356 | vNext | analyzer/RH8xxx new rule + fix | new rule | open | — | M | yes | #1091, #1355 | yes | No tag-casing rule exists; namespaced/mismatched/`/** */` behavior still to be shown as outputs. |
| 1394 | Maintenance | specs only (shared comment-placement, code-fixes) | spec clarification | partly done | Item 8 stated by RH5111-005 (comments keep the report, fix withheld) | S | no | — | no | Remains: item 10 (P14b only for fixes that lay out the member, SHARED-FIX-007) and item 12 (disabled-text line endings, SHARED-FIX-008). |
| 1433 | vNext | analyzer/RH5104 layout + Core string-line helper | rule report scope | open | — | M | yes | — | no | Q6 already superseded (comment); Q3/Q5 open: RH5104-001 still excludes `///` and multi-line interpolated trivia; #951 blocker closed. |
| 1434 | vNext | analyzer/RH41xx naming (shared casing) | naming rules | open | — | M | yes | — | no | ID SHARED-CASING-011 proposed in the body is now taken by another requirement; needs a new ID. |
| 1435 | vNext | code fixes/RH2004 | code-fix output | open | — | S | yes | — | no | RH2004-006 still excludes operators (RH2004.md:235). |
| 1436 | vNext | code fixes/RH3004 | code-fix output | open | — | S | yes | — | no | Fix still drops `static` (`CreateLambdaExpression`) and RH3004-005 withholds operand positions. |
| 1437 | vNext | code fixes/RH7501 | code-fix output | open | — | S | yes | — | no | RH7501-004 "does not decide the fix for a `break` with a blank line in front of it". |
| 1443 | vNext | code fixes/RH5417 (SHARED-FIX-007) | code-fix output | open | — | S | yes | — | no | Shared-line bullet still names members only; check against #1760's shared-line change first. |
| 1444 | vNext | analyzer/RH5601, RH6004, RH6005, RH6021, RH7302 | rule report scope | open | — | M | yes | — | no | All five requirement texts still exclude the inputs. |
| 1445 | vNext | analyzer/RH55xx attribute placement (SHARED-ATTR) | attribute placement | open | — | L | yes | — | yes | SHARED-ATTR-002/-003 still decide by specifier; #1093 closed as superseded by this. |
| 1447 | vNext | code fixes/statement blank lines (BlankLineCodeFixUtilities) | code-fix output | open | — | M | yes | — | yes | Two placements coexist via `behindLeadingDirectives`; 2026-11-12 comment asks the maintainer to pick one. |
| 1448 | vNext | analyzer/RH8017, RH8018 documentation | documentation-comment rules | open | — | S | yes | #1092 | no | SHARED-DOC-002 still excludes interface static constructors; #1092 body still carries the same Q4. |
| 1449 | vNext | analyzer/RH8030–RH8033, RH81xx, RH8202, RH8306 (shared element-documentation) | documentation-comment rules | open | — | L | yes | #1091 | yes | SHARED-DOC-010/-012 still cover text-only summaries; several-summaries verdict (RH8030/RH8031) still to be picked. |
| 1450 | vNext | analyzer/RH8402 documentation | documentation-comment rules | open | — | S | yes | — | yes | RH8402-003 coverage excludes the input; whether a fix is offered is unsettled ("Unsure"). |
| 1451 | vNext | code fixes/casing (CasingUtilities, RH4005, RH4121) | naming rules | open | — | M | yes | — | yes | RH4005-002/RH4121-003/SHARED-CASING-006 unchanged; `Café` outcome (not reported vs. renamed) is unpicked. |

## Closure candidates

- #1330 partly done: interpolated raw and single-line `u8` lines are already tested; narrow to multi-line UTF-8 raw and `///` content lines, or close as low-value.
- #1353 partly done: RH8402-006 now states the run replacement; only the mixed-run sentence (Q2) remains, so fold it into #1450 (same rule, same spec).
- #1394 partly done: item 8 is stated by RH5111-005; narrow to items 10 and 12.

## Overlaps

- #1353 + #1450: same rule and spec (RH8402-003/-006); run together.
- #1448 + #1092 (Q4) and #1091: #1092 still lists the static-constructor change; decide #1091/#1092/#1448 together (comment on #1092 asks for that).
- #1449 + #1091 + #1356 + #1355 (formatter tag case): P8 family; edit SHARED-DOC-010..013 once.
- #1341 + #1433 (Q3) + #1077 (slice 1) + #1148: P13 "a `///`/`/** */` that documents nothing is an ordinary comment"; #1352 (`/** */` order) touches the same rule set.
- #1295 + #1296 + #1297 + #1290: RH1001/RH1002 equality judgement; one run.
- #1298 + #1290: P16 transparency for argument values.
- #1434 + #1451: shared casing (`CasingUtilities`, SHARED-CASING-*).
- #1435 + #1436 + #1437 + #1443: small decided code-fix output extensions; batchable.
- #1443 + #1760 (merged shared-line change): re-check RH5417 before building.
- #1444 + #1078 / #1239: region directive spelling `# region`.
- #1445 + #1033: attribute placement.
- #1299 + #1350 + #1300 + #1338: small decided report-scope widenings; batchable by lane.
