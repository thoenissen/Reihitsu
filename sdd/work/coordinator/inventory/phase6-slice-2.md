# Phase 6 slice 2 (issues #1461–#1567)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1461 | Maintenance | formatter/DocumentationCommentFormattingPhase | one owner per decision | open | — | S | no | — | no | Private BOM/whitespace test still at `DocumentationCommentFormattingPhase.cs:94`. |
| 1472 | Maintenance | formatter/IndentationPhase | indentation review | open | — | L | no | — | no | Written review only; any rewrite becomes its own issue. |
| 1474 | Maintenance | formatter/node-level entry points | partial write-back | open | — | M | yes | — | no | Blank lines above a `FormatNodeInDocumentAsync` target are still unspecified (B4 #1489 kept them); same write-back area as #1521. |
| 1505 | Maintenance | tests/formatter helpers | test infrastructure | partly done | `ObservedPipeline.Run` now takes parse options (`ObservedPipeline.cs:63`) | S | no | — | no | Remaining: `FormatterPhaseTestsBase.ApplyPhase` still parses with default options, and neither helper uses `SecondPassInput.Create`. |
| 1515 | vNext | formatter/StructuralTransformPhase (expression-body conversion) | comment placement at wraps | open | — | M | yes | #1539 | yes | FMT-STRUCT-006 still leaves a comment directly at the arrow unspecified. Settle with #1439/#963 part 1. A declared unsupported shape is an alternative. |
| 1516 | Maintenance | formatter/all phases + tests | one owner per decision | open | — | M | no | — | no | FMT-STRUCT-026 covers only the structural transforms; no FMT-PIPE requirement for the later phases. |
| 1517 | Maintenance | formatter/LineBreakPhase | one owner per decision | open | — | M | no | — | no | `RemoveTrailingWhitespace` is still called in LineBreakInitializer, List, Chain and Ternary rewriters. |
| 1518 | Maintenance | formatter/SwitchCaseBracePhase | switch-section braces | open | — | M | no | — | no | `SwitchCaseBraceRewriter.AddBraces` still places braces itself instead of through `BracePlacer`. |
| 1519 | Maintenance | formatter/LineBreakPhase | one owner per decision | open | — | M | no | — | no | `DeclarationBraceLineBreakRewriter` (indexers) and `LineBreakBlockRewriter.VisitAccessorList` both place accessor-list braces. |
| 1521 | vNext | formatter/node-level entry points | partial write-back | open | — | L | yes | #1140, #1774 | no | Needs the partial write-back design; affects code-fix output only if a fix passes such a target (unchecked). |
| 1533 | vNext | formatter/DocumentationCommentFormattingPhase | `/** */` doc-comment placement | partly done | Switch-label positions decided (option C, FMT-BREAK-033–035, comment of 2026-11-02) | M | yes | — | yes | Still open: `/** */` behind `{` and behind an earlier comment. Overlaps #1077 (slice 1). |
| 1534 | vNext | formatter/LineBreakPhase + analyzer/RH5308 | unsupported shapes | superseded by #1718 | FMT-UNSUP-008 refuses every comment (`///` and `/** */` included) behind `?`/`:`; RH5308-005 withholds the fix | S | yes | — | no | Both shapes in the issue, and the own-line variant, fall under FMT-UNSUP-008. |
| 1537 | vNext | formatter/StructuralTransformPhase + SwitchCaseBracePhase | switch-section braces | open | — | M | yes | #1538 | yes | Rare input. Only a cheap directive-kind check fits *Special cases do not grow the source*; balanced-`#if` analysis likely does not. Keeping the guards is a valid close. |
| 1538 | Maintenance | specs only (FMT-STRUCT-003) | spec clarification | open | — | S | no | — | no | The Why text at `structural-transforms.md:113` is still stale. |
| 1539 | Maintenance | formatter/StructuralTransformPhase | comment placement at wraps | open | — | M | no | — | no | The converter still has its own `TrySplitOwnLineComments`/`ExtractSignificantTrivia`. |
| 1540 | vNext | formatter/SwitchCaseBracePhase | switch-section braces | open | — | S | yes (only if B) | — | yes | The recommended option A is today's output, so A is only a clarification of FMT-SWITCH-007. |
| 1542 | vNext | formatter/LineBreakPhase + IndentationPhase | pattern/continuation layout | open | — | M | yes | — | yes | Comments add more shapes: `not (…)`, a positional `C(…)`, a combinator content, a case label, a leading comma (cf. FMT-UNSUP-009 / #1739). |
| 1549 | vNext | formatter/LineBreakPhase (attributes) | attribute lists | open | — | M | yes | — | yes | Colon spacing is settled by C5 #1548. Still open: wraps inside a declaration's list, and a lambda behind its attribute list. |
| 1550 | vNext | formatter/HorizontalSpacingPhase + LineBreakPhase | spacing gaps | open | — | S | yes | — | yes | FMT-SPACE-024 still covers only argument/parameter/expression `ref`. |
| 1552 | vNext | formatter/LineBreakPhase (accessor lists) | attribute lists | open | — | S | yes | — | yes | Option A matches FMT-BREAK-024's text, so it may be a plain bug fix. |
| 1553 | Maintenance | formatter/HorizontalSpacingPhase | whitespace ownership | open | — | S | no | — | no | The only known instance is fixed. No reachable input is named; close as Supported inputs if none is found. |
| 1554 | vNext | formatter/BlankLinePhase | attribute lists | open | — | S | yes | — | yes | Rare input (the compiler warns with CS1587). |
| 1555 | Maintenance | code fixes (shared) + formatter | attribute lists | open | — | M | no | — | no | Weak premise: the code-fix split is now a deliberate text replacement (SHARED-ATTR-017) and is withheld for line breaks (SHARED-ATTR-023), so the two-pass evidence is likely gone. Close candidate after a check. |
| 1556 | vNext | formatter/HorizontalSpacingPhase | spacing gaps | open | — | S | yes | — | yes | Needs an FMT-SPACE requirement first. |
| 1557 | Maintenance | specs only (FMT-BREAK-027) + test | spec clarification | open | — | S | no | — | no | `line-breaks.md:1547` still leaves the placement unspecified. |
| 1559 | vNext | formatter/HorizontalSpacingPhase | spacing gaps | open | — | S | yes | — | yes | No FMT-SPACE requirement covers the empty statement behind a label. |
| 1562 | Maintenance | formatter/BlankLinePhase + analyzer/RH5020 | blank lines | partly done | Option A is on main via C3 #1496 / D1 #1498 (comment of 2026-11-05) | S | no | — | no | Only the regression test with the issue's input remains. |
| 1563 | Maintenance | specs only (FMT-BREAK-008) + test | `/** */` doc-comment placement | open | — | S | no | #1533 | no | Becomes an output question if the comment should move with the `{`. Decide with #1533. |
| 1564 | Maintenance | specs only (FMT-BREAK-033) + test | `/** */` doc-comment placement | open | — | S | no | — | no | DocumentationMode dependency; the same mode clause already exists in FMT-BREAK-012/021. |
| 1566 | Maintenance | formatter/LineBreakPhase + BlankLinePhase | C# language support | open | — | M | yes | — | no | Below C# 11 only; reproduced on main. Triage: not critical (used from C# 12). |
| 1567 | vNext | formatter/LineBreakPhase | pattern/continuation layout | open | — | S | yes | — | yes | Option B is #953's continuation rule. Neighbour of #1549's lambda part. |

## Closure candidates

- #1534 — superseded by FMT-UNSUP-008 (#1718), which refuses every comment (`///` and `/** */`) between `?`/`:` and its branch, and by RH5308-005, which withholds the fix. Close with that reference.
- #1562 — partly done: option A shipped with C3 #1496 / D1 #1498. Close once the regression test with the issue's input lands; this is a small `tests` task.
- #1505 — partly done: `ObservedPipeline` now parses with the fixture's options. What remains is small: route `FormatterPhaseTestsBase` and `ObservedPipeline` through `SecondPassInput.Create`.
- #1533 — partly done: the switch-label positions are decided. Narrow the issue to `{` and an earlier comment.
- Soft candidates (verdict `open`, premise weak):
  - #1553 — no reachable input is named; Supported inputs.
  - #1555 — the code-fix split differs from the formatter's by design (text replacement with the SHARED-ATTR-023 withhold).
  - #1537 — keeping the guards is a valid end state under *Special cases do not grow the source*.

## Overlaps

- #1515 + #1539 (+ #1439, #963 part 1): expression-body conversion; settle placement and carrier together.
- #1518 + #1537 + #1540 (+ #1538): `SwitchCaseBraceRewriter` / FMT-SWITCH. #1538 is the stale Why of the same FMT-STRUCT-003 guard that #1537 would narrow.
- #1521 + #1474 + #1140 + #1774: node-level write-back design.
- #1533 + #1563 + #1564 + #1077 (slice 1): `/** */` placement and documentation mode.
- #1549 + #1552 + #1554 + #1555 + #1556 + #1557: attribute-list layout and spacing.
- #1542 + #1567 + #1549 (lambda part) + #953 (slice 1) + #1442: continuation lines with no owner. #1542's leading-comma shape ties to #1739.
- #1516 + #1517 + #1519 + #1553 + #1461: "one owner per decision" cleanups (FMT-PIPE-021, whitespace ownership).
