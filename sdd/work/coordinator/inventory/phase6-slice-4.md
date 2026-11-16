# Phase 6 slice 4 (issues #1698–#1080, index order: #1698, #1713, #1722, #1723, #1726, #1728, #1732, #1776, #352–#1080)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1698 | vNext | formatter/IndentationPhase | interpolation-hole layout | open | — | S | yes | — | yes | Owner of hole-line columns in regular interpolated strings unchecked; the blank line in front of `// f` (BlankLinePhase) is decided with it. |
| 1713 | Maintenance | formatter/BlankLinePhase | internal refactor | open | — | S | no | — | no | `RemoveBlankLinesAfterLeadingDocumentationComments` still in BlankLineTokenCleanupRewriter.cs:482. |
| 1722 | Maintenance | formatter/IndentationPhase | internal refactor | open | — | S | no | — | no | `IsHoleOpeningLine` still private in RawStringLineShifts.cs:262. |
| 1723 | Maintenance | Playground + formatter public API | internal refactor | open | — | S | no | — | no | `PlaygroundFormatter.HasSyntaxErrors` copy still at line 50. |
| 1726 | vNext | formatter/PrecheckPhase + RH5302 fix | unsupported shapes | open | — | M | yes | — | no | Maintainer already decided (declare, own change); no new FMT-UNSUP yet (spec ends at -009). |
| 1728 | vNext | formatter/PrecheckPhase + StructuralTransformPhase + RH7101 fix | unsupported shapes | open | — | M | yes | — | no | Maintainer decided; deletes `CarriesDirective`, FMT-PIPE-031 directive condition, RH7101-005. |
| 1732 | vNext | formatter/PrecheckPhase + RH5401 | unsupported shapes | open | — | M | yes | — | no | Maintainer decided 2026-11-14; supersedes #1730/#1731; RH5401 report decision left to the change. |
| 1776 | Maintenance | formatter/Data (blank lines, indentation) | internal refactor | open | — | S | no | — | no | `PrecedingTokenFacts.cs`, `TriviaFact.cs` still exist; spec FMT-PIPE-024 code-owner row changes. |
| 352 | 1.5.0 | analyzer/RH2xxx design | new rule (size metrics) | open | — | L | yes | — | yes | Needs metric definition, option key and default limit chosen by maintainer. |
| 353 | 1.5.0 | analyzer/RH2xxx design | new rule (size metrics) | open | — | M | yes | — | yes | Line-count definition and default limit are output decisions. |
| 354 | 1.5.0 | analyzer/RH2xxx design | new rule (size metrics) | open | — | M | yes | 353 | yes | Shares the line-count definition with #353. |
| 849 | 1.4.0 | analyzer/RH3xxx clarity + code fix | new rule (get-only expression body) | open | — | M | yes | — | no | Formatter side exists (`GetOnlyMemberExpressionBodyTransform`); no analyzer yet; P11 new-rule candidate. |
| 850 | 1.4.0 | analyzer/RH3xxx clarity + code fix | new rule (get-only expression body) | open | — | M | yes | 849 | no | Indexer twin of #849; same report conditions. |
| 851 | 1.4.0 | analyzer/RH54xx layout | new rule (get-only expression body) | open | — | M | yes | 1732 | no | Indexer twin of RH5401; mirror its guards after #1732 changes RH5401's comment-behind-`=>` handling. |
| 867 | Maintenance | tests/analyzer verifier | test infrastructure | open | — | M | no | — | no | No warning level set in AnalyzerTestsBase; raising it may surface real fix defects. |
| 877 | Maintenance | analyzer/RH3002 | internal refactor | open | — | M | no | — | no | Per-node memo dictionary (line 1125) and uncancelled bound test remain; item 1 overlaps #1603. |
| 889 | Maintenance | tests/analyzer RH5112/RH5201 fixes | test infrastructure | open | — | S | no | — | no | No mid-line-statement chain fix test found; `IsFirstOnLine` at FluentChainCodeFixHelper.cs:80. |
| 892 | 1.4.0 | analyzer/RH5xxx layout (chains) | chain rules parity | open | — | M | yes | — | no | P11 new-rule candidate (attached parts are not RH5201's links). |
| 893 | 1.4.0 | analyzer/RH5201 | chain rules parity | open | — | M | yes | 892 | no | Case 1 is RH5201's own gap; case 2 (columns of trivia-kept pieces) may be dropped if #894's shape is declared unsupported. |
| 894 | 1.4.0 | analyzer/RH5xxx layout (chains) | chain rules parity | open | — | S | yes | — | yes | Comment/directive inside `?.`/`!.`/`?[` is a candidate shape to declare unsupported (like #1726/#1732) instead of a new rule; maintainer picks. |
| 897 | 1.4.0 | analyzer/RH5112, RH5048 resources + rule pages | rule wording | open | — | S | yes | — | yes | Titles still "first call" (RH5112.md, RH5048.md line 1). |
| 968 | vNext | analyzer/RH71xx ordering | ordering accessibility | open | — | M | yes | — | no | Triage: extension of RH7103-003; comment extends to RH7102, RH7109, RH7110; `OrderingAccessibilityGroup.None` unchanged. |
| 970 | vNext | code fixes (ordering, Core move safety) | ordering move safety | open | — | M | no | — | no | Triage: extension of SHARED-ORDER-006 (spec line 207 still refuses any `#pragma`); fix offered more often. |
| 971 | vNext | Core/CasingUtilities + casing fixes | casing derivation | open | — | S | yes | — | no | Triage: extension of SHARED-CASING-006 (rule unchanged); also hits RH2001 field name. |
| 973 | vNext | analyzer/RH1003 | rule report scope | open | — | S | yes | — | no | Triage: extension of RH1003-001 (flattening). |
| 1032 | Maintenance | code fixes (resources) | internal refactor | open | — | S | no | — | no | Both entries still in CodeFixResources.resx. |
| 1033 | vNext | code fixes (attribute placement base) | fix guard scope | open | — | S | no | — | no | Walk still starts at `attributeList.Parent` (TargetAttributePlacementCodeFixProviderBase.cs:211); SHARED-ATTR-012 needs the narrowed wording. |
| 1053 | Maintenance | documentation/rules RH8001–RH8018 | documentation-comment rules | open | — | S | yes | 1092, 1448 | yes | Group split on pages must follow the pending SHARED-DOC-002 changes; general-notes vs per-page is a user-facing text choice. |
| 1072 | vNext | code fixes (casing base) | casing derivation | open | — | S | yes | — | no | Decided (P3); SHARED-CASING-007 scope already excludes reserved keywords but nothing withholds; no keyword check in CasingCodeFixProviderBase. |
| 1078 | vNext | analyzer/RH6xxx spacing | comments next to code | open | — | M | yes | — | no | Decided; widened by comments to RH6013–RH6022 and RH6021 missing-space direction; specs (e.g. RH6006-003) unchanged. |
| 1080 | vNext | analyzer/RH5022 | rule report scope | open | — | S | yes | — | no | Decided bug fix; RH5022-001 still says "line that ends with `{`". |

## Closure candidates

None: every issue in this slice is still open on main. Superseded by this slice: #1732 supersedes #1730 and #1731 (outside this slice; close them if they are still open).

## Overlaps

- #1726, #1732, #1728 (unsupported shapes; each "in a change of its own" but share the precheck pattern) — candidate #894 (comment/directive inside `?.`) for the same treatment; #1728 also touches the field split of #1586/#1587 (slice 3).
- #1732 + #851 + RH5401: settle the comment-behind-`=>` shape before mirroring RH5401's guards for indexers; #849/#850 come together with #851 (get-only property and indexer family).
- #892, #893, #894, #897 (chain rules after #887), with the formatter-side chain features #895/#896 (slice 1).
- #352, #353, #354 (size-metric rules, one option and line-count design).
- #971, #1072 with #1451 (slice 6, casing fixes keep prefix and identifier characters) and #1771 (slice 8, one rename owner): all change the casing-fix derivation or guard.
- #968 (RH7102/RH7103/RH7109/RH7110 effective accessibility) with #970 (ordering move safety): both ordering family, separate code.
- #1053 with #1092 and #1448 (slices 5/6, private group and static constructor in SHARED-DOC-002).
- #1078 with #1160 and #1546 (comment-touches-code rules, slices 5/7).
- #877 item 1 with #1603 (slice 7, shared re-parse check).
- #1698 with #1566 and #1568 (interpolation-hole layout, slices 2/3).
- #1713, #1776 (blank-line phase internal refactors, same files) and #1722, #1723 (FC-015 one-owner refactors).
