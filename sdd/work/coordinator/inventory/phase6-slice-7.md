# Phase 6 slice 7 (issues #1452–#1709)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1452 | vNext | analyzer/RH2109 (Razor) | Razor rule scope | open | — | S | yes | — (builds on the deferred #1289 remainder) | no | Output already decided (#1256 Q4 A); analyzer and RH2109-001 still know only `@code`. |
| 1453 | Maintenance | specs only (RH6001) | spec clarification | done | RH6001-001 lists `fixed` (`sdd/specs/analyzer/RH6001.md:27`); `VerifyFixedKeywordGluedToParenthesisIsDetectedAndFixed` tagged RH6001-001 (`RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.cs:922`) | S | no | — | no | Both "Done when" items met. |
| 1460 | Maintenance | code fixes (RH8309) | internal refactor | open | — | S | no | — | no | Comment at `RH8309…CodeFixProvider.cs:111-112` still calls `<inheritdoc>`, `<include>`, `<seealso>` unknown. |
| 1476 | Maintenance | analyzer/RH5201 tests | test coverage | open | — | S | no | — | no | Existing tests cover comments in front of links, not a chain root behind a comment that leads its line. |
| 1477 | Maintenance | analyzer/RH5304 tests | test coverage | open | — | S | no | — | no | No RH5304 fix test on a statement or expression-body target found. |
| 1512 | Maintenance | Core shared line facts; RH5202, RH5401, RH5530/5531, RH5603, RH8402 | shared line facts | open | — | M | yes (where a report changes) | — | no | Comment widens it to every analyzer using `IsFirstOnLine` where the spec says `StartsLine`. |
| 1546 | vNext | analyzer/RH5104 | comment placement | done | RH5104 "Comments must be on their own line" already reports every ordinary comment that shares a line with code (RH5104-001, `sdd/specs/analyzer/RH5104.md:24`), incl. `/* c */ {` and trailing `// c` | S | no | — | no | Issue overlooked RH5104; overlaps #1160. |
| 1560 | vNext | new analyzer rule (RH5### layout) | new rule (P11 counterpart) | open | — | M | yes | — | yes | Optional per the issue; RH5103-001 excludes the label line on purpose; pairs with #1559 (`case 1: ;`). |
| 1582 | Maintenance | code fixes (RH5114) | internal refactor | open | — | S | no | — | no | File is under `Rules/Layout/` (inline pair at lines 94-95), not `Organization/` as the issue says. |
| 1584 | vNext | analyzer (shared, generated code) | generated code | done | SHARED-DIAG-001 (`sdd/specs/shared/diagnostics.md:27`) covers `[GeneratedCode]` and every form the compiler treats as generated (so also `.editorconfig` `generated_code`); test `GeneratedCodeTests.CodeMarkedWithGeneratedCodeAttributeIsNotAnalyzed`; `general-notes.md:25` | S | no | — | no | Premise wrong: options A and B are both what Roslyn already honors. #1585 (formatter side) stays separate. |
| 1589 | Maintenance | code fixes (RH3202–3208, RH5405–5407, RH72xx, RH7310, others that move code) | code-fix safety audit | open | — | L | yes (fixes withheld) | — | no | An audit that spawns per-fix withholdings; #1759's fix half confirms the directive condition. |
| 1590 | vNext | analyzer/RH7003 | SHARED-DIAG-008 reversal | superseded by #1759 | #1759: every rule judges the active build's version (option B), and the fix is withheld because the namespace holds a directive | S | yes | #1759 | no | Handle as one of #1759's affected rules; `VerifyDirectiveContainingNamespaceIsReported` already pins B. |
| 1591 | Maintenance | documentation/rules/RH7003.md | rule pages | open | — | S | no | — | no | "Exceptions" section still at `RH7003.md:24`. |
| 1601 | vNext | code fixes (RH3004) | fix coverage widening | contradicts decision #1759 (fix half) and "Special cases do not grow the source" | Deciding "under every build" reconstructs other build configurations (ruled out by AGENTS.md); #1759 keeps fixes withheld across directives | M | yes | — | yes (raw-string part) | Raw-string remainder (comment 2026-11-13) is a cheap-check widening like #1689; move it there if kept. |
| 1602 | vNext | analyzer/RH3106 | rule report scope | open | — | M | yes | — | no | Goal stale: RH3106-003 (#1599, spec :408-412) says the outcome is "not reported", not "reported and fixed"; closing the gap needs a consuming-vs-binding distinction (watch Special cases). |
| 1603 | Maintenance | Core `RewriteParseUtilities`; analyzer/RH3002 | internal refactor | open | — | M | no | — | no | `ScanTypeArgumentList` still at `RH3002…Analyzer.cs:215`; overlaps #877 item 1. |
| 1606 | Maintenance | specs RH3001/3005/3101/3105/3201 + tests | spec clarification | open | — | M | yes (item 2) | — | yes (item 2: `/**/+1;` lands in column 0) | Item 1 may be stale after #1599 reshaped the -004 requirements; item 2 is a fix-output bug candidate to split out. |
| 1607 | Maintenance | specs RH3004 + tests; formatter `IndentationRewriter` | spec clarification | open | — | S | no | — | no | Item 2 largely answered: RH3004 spec (line 188) measures from the line's first character also where a comment starts it. |
| 1612 | vNext | code fixes (RH5202) | raw/interpolated strings | open | — | S | yes | — | yes | RH5202 spec decides only whitespace-only content lines (:323), not hole-code lines. |
| 1615 | Maintenance | specs only (SHARED-DIAG-004) | spec clarification | open | — | S | no | #1710 | no | SHARED-DIAG-004 still names no SHARED-NESTED-002 per-part reports; #1710 (RH8306 both parts) rewrites the same paragraph. |
| 1628 | vNext | analyzer/RH5023 (Core `FormattingTextAnalysisUtilities`) | raw/interpolated strings | open | — | S | yes | — | yes | Constraint via #951 is gone (#951 closed in 1.2.0); rare input. |
| 1637 | vNext | new analyzer rule (RH5### layout) | new rule (P11 counterpart) | open | — | M | yes | — | yes | Unblocked: #1631 closed via #1632; RH5110 non-goal confirms. |
| 1638 | vNext | new analyzer rule (RH6### spacing) | new rule (P11 counterpart) | open | — | S | yes | — | yes | Unblocked (#1632); RH6021 non-goal (`RH6021.md:21`) confirms. |
| 1649 | vNext | analyzer/RH5111 | rule report scope (P11) | open | — | S | yes | — | yes | Non-goal still in `RH5111.md:18`; overlaps #1155. |
| 1650 | vNext | analyzer/RH5106, RH5107 | rule report scope (P11) | open | — | M | yes | — | yes | Non-goal still in `RH5106.md:19`, `RH5107.md:19`; overlaps #1158, #1349. |
| 1668 | vNext | Core `OrderingDeclarationUtilities` (RH7102/7103/7109/7309 fixes) | conditional-directive guards | open | — | M | yes (fewer fixes) | — | yes | Maintainer asked for closer analysis (2026-11-09); refusing any `#if` block fits #1759's fix half. |
| 1681 | vNext | analyzer/RH8307 | documentation-comment rules | done | Option B shipped in #1700 (`11d10ac0`, 2026-11-14): RH8307-002 does not report `/** */` elements, RH8307-006 retired | S | no | — | no | — |
| 1684 | Maintenance | code fixes (statement blank-line base, RH5103, RH3208) | fix layout delegation | open | — | L | no | #1605 | no | May close with reason if delegation needs #1605's layout-only mode. |
| 1689 | vNext | code fixes (RH7004) | fix coverage widening | open | — | M | yes | — | yes | Maintainer-deferred; only per-kind cheap checks (raw strings first). |
| 1694 | Maintenance | specs only (SHARED-DIAG-002) + tests | spec clarification | open | — | M | no (unless a non-compliant rule is found) | — | no | Escape rule still absent from SHARED-DIAG-002; overlaps #1105, #1132 and SHARED-USING-004. |
| 1709 | vNext | analyzer/RH8107, RH8202 | documentation-comment rules | open | — | S | yes | — | yes | Specs still report only the first element (`RH8107.md:87`, `RH8202.md:25`); same pattern as #1700. |

## Closure candidates

- #1453 — done: RH6001-001 lists `fixed` and a tagged test covers the example.
- #1546 — done: RH5104 already reports a comment sharing a line with code; the issue overlooked it.
- #1584 — done: SHARED-DIAG-001 plus Roslyn already skip `[GeneratedCode]` declarations and `.editorconfig` `generated_code` files; test and general-notes exist.
- #1590 — superseded by #1759: the general SHARED-DIAG-008 reversal decides it (reported, no fix).
- #1601 — contradicts #1759 (fix half) and "Special cases do not grow the source"; keep only the raw-string remainder, best folded into #1689.
- #1681 — done in #1700 (option B).

## Overlaps

- #1590, #1668, #1601 with #1759 (SHARED-DIAG-008 reversal and its fix half).
- #1601 (raw-string remainder) + #1689 — same cheap raw-string shift for moved blocks.
- #1637 + #1638 (+ #1639, #955 item 2) — constraint-clause counterparts of #1632.
- #1559 + #1560 — switch-label line layout.
- #1546 ~ #1160 and existing RH5104.
- #1649 ~ #1155; #1650 ~ #1158, #1349 — P11 rule-scope questions, one output-question round.
- #1709 ~ #1293 and #1700's per-element decision; #1615 ~ #1710.
- #1603 ~ #877 item 1.
- #1512 + #1582 + #1460 — small shared-helper/comment refactors, one maintenance batch.
- #1476 + #1477 — test-coverage batch.
- #1684 ~ #1605 (layout-only mode).
- #1584 ~ #1585 (formatter generated-code detection).
- #1452 ~ #1289 remainder (Razor reading).
- #1612 + #1628 — interpolated/raw-string hole handling, one output-question round.
