# Inventory: batches #984, #985, #991–#997 (round 6)

Read 2026-10-19: every batch body and all comments; open follow-ups #948–#976, #1068–#1105, #1131–#1153 (titles + bodies grepped for the batch's RH IDs and shared-spec prefixes); `src/` helper usage per rule file (analyzer, code fix, tests).

Shared specs on disk (`sdd/specs/shared/`): attribute-targets, blank-line-spacing, casing, code-fixes, comment-placement, diagnostics, element-documentation (SHARED-DOC-001…013), fluent-chain-layout, line-join-rules, operator-spacing, ordering-move-safety (SHARED-ORDER, 7 requirements), region-directive-blank-lines, single-letter-names, statement-blank-lines, token-gap-fixes, using-directive-ordering (SHARED-USING), whitespace-removal-fixes. Not on disk: any modifier-ordering, member-region-grouping, region-description, trailing-comma, empty-type, statement-braces, expression-body, empty-parentheses, struct-equality, nested-type spec.

Comments common to all nine: P1–P7 note (2026-10-18; P1/P2/P3/P5/P6/P7 cases filed marked `Decided by principle Pn (#912)`, P4 stays a normal question). #984/#985 also carry the P8 note (#1091). Every batch edits `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` (append-only, textual merge conflicts only).

## #984 — D3 RH8101–RH8204 (14)

- Rules: RH8101–RH8106, RH8108–RH8111, RH8201–RH8204. Reuses `shared/element-documentation.md` (SHARED-DOC); may extend it (next free ID SHARED-DOC-014) only when >1 spec relies on it (param/typeparam mirror). No new shared file expected.
- Comments: P1–P7 note; P8 note (#1091).
- Follow-ups: #1153 (P8b `name` read as compiler binds it; P8c summary text that renders — the `<param>`/`<returns>`/`<typeparam>` text shapes in RH8104/RH8106/RH8111 and `name` matching are instances of #1153, not new questions); #1132 (RH8101/RH8108 copy SHARED-DOC-012 matching, RH8102/RH8109 read `name` through syntax — establish agreement, reference SHARED-DOC-012 or file differences in the shared spec's open-questions issue); #1091 (P8 `<inheritdoc>` shapes, merged partial parts: RH8105/RH8201/RH8203); #1092 (doc group by symbol, minor).
- Code: `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander` (owned by element-documentation.md); `DocumentationCommentCodeFixUtilities` (owned by `analyzer/RH8107.md`, used by RH8202); `XmlDocumentationElementOrderingUtilities` (unowned, used by RH8204 + RH8309).
- Escalation rule: moving an RH8107 requirement to shared retires its ID → maintainer.
- Collisions: #985 (same shared file, same helpers, see below); #997 only via RH3201 → `DocumentationAnalysisUtilities` (Uses only).

## #985 — D4 RH8301–RH8501 (12)

- Rules: RH8301–RH8309, RH8401, RH8402, RH8501. Reuses element-documentation.md; references `formatter/documentation-comments.md` FMT-DOC IDs where the formatter owns the policy. Extends shared only if needed (ID SHARED-DOC-014+ — same counter as #984).
- Comments: P1–P7 note; P8 note (#1091).
- Follow-ups: #1148 (FMT-BLANK-011 scope narrowed; "RH8303 is mirrored as well" — RH8303 spec must describe current scope only, not refile); #1150 (P7: RH83xx formatter-delegating fixes withheld when formatter returns document unchanged — don't refile no-op fixes); #1153/#1091 (RH8305/RH8306 "which summary", tag case/namespace under P8; RH8309 element order); formatter bugs #961 (doc element end tag), #962 (stray `///` relocation), #959 (comment line ends) — FMT-DOC side, don't refile; #1077 (P6 `/** */` documenting nothing = block comment).
- Code: `DocumentationCommentCodeFixUtilities` (RH8107 spec owner; used by RH8304, RH8308), `XmlDocumentationElementOrderingUtilities` (RH8309, unowned), `DocumentationAnalysisUtilities` (RH8301–RH8303), `Data/Configuration*`/`CopyrightHeaderTemplateResolver` (RH8401/RH8402 copyright header, isolated).
- Collisions: #984 strongly.

## #991 — LA RH5301–RH5408 (16)

- Rules: RH5301–RH5308, RH5401–RH5408. References FMT-BREAK / FMT-INDENT / FMT-STRUCT. May create a statement-braces shared spec (`StatementBracesCodeFixProviderBase`, RH5405–RH5407) only if >1 spec relies on it.
- Comments: P1–P7 note only.
- Follow-ups: #1150 (P7; names `StatementBracesCodeFixProviderBase` and RH53xx/RH54xx providers); #1140/#1138 (P9 fix scope: line-break/alignment fixes change only their own code) and #1144 (P10 fix layout) — principles that apply to RH53xx fixes; #953 (continuation-line indentation), #956 (argument lists like parameter lists, initializers), #963 (FMT-STRUCT comment after `=>`) — formatter side.
- Code: `StatementBracesCodeFixProviderBase`, `InitializerLayoutAnalysisUtilities`, `StatementBlockParentPolicy` (all unowned, family-only); `SyntaxNodeUtilities` (unowned Core helper, also used by #997 RH3002/RH3004/RH3005/RH3102 and RH51xx/attribute bases) — reference, do not claim.
- Collisions: none material.

## #992 — LC RH5409–RH5417 (9)

- Rules: RH5409–RH5417. May create trailing-comma (`TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, RH5409–RH5411) and empty-type (`EmptyTypeDeclarationShouldUseSemicolon*Base`, RH5412–RH5416) shared specs if >1 spec relies. FMT-STRUCT owns `TrailingCommaRemovalUtilities`, `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`, `EmptyTypeDeclarationSemicolonTransform` → reference FMT-STRUCT IDs, do not take those types.
- Comments: P1–P7 note only.
- Follow-ups: none in range touch RH5409–RH5417 (#974 "trailing comma" is reihitsu.json, unrelated). Indirect: #1150 (RH54xx formatter-delegating providers, P7).
- Collisions: none (bases used only by this family).

## #993 — O1 RH7001–RH7110 (13)

- Rules: RH7001–RH7004, RH7101, RH7102, RH7104–RH7110. Reuses `shared/ordering-move-safety.md` (SHARED-ORDER; next ID SHARED-ORDER-008) for RH7102/RH7109 like `analyzer/RH7103.md`. May create a modifier-ordering shared spec (`ModifierOrderingCodeFixProviderBase`, RH7105/RH7106) if >1 spec relies.
- Comments: P1–P7 note. Body notes #968, #969, #970 (do not refile).
- Follow-ups: #1134 (RH7109 fix reorders layout-significant fields, SHARED-FIX-003); #1143 (Fix All one application — RH7004/RH7102/RH7109 test classes use `NumberOfFixAllIterations = 2`; don't encode two passes as a requirement); #1150 (P7, RH7004/RH710x providers); #1137 (RH7103 non-goals replaced by SHARED-FIX-003 — model for RH7102/RH7109 specs); #969/#970/#968 (named in body).
- Code: `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` (SHARED-ORDER owners); `ModifierOrderingUtilities` (unowned; also used by RH2001 in #996); `DeclarationModifierUtilities` (owned by `analyzer/RH2005.md`; also used by RH2004/RH7104–RH7106/`ModifierOrderingCodeFixProviderBase`); `RegionDirectiveUtilities` (owned by `formatter/region-formatting.md`; RH7110).
- Escalation: taking `DeclarationModifierUtilities` out of RH2005's code owners retires/moves an RH2005 requirement → maintainer.
- Collisions: #996 (ModifierOrderingUtilities, DeclarationModifierUtilities, Core.Test `ModifierOrderingUtilitiesTests.cs`/`DeclarationModifierUtilitiesTests.cs`); #994/#995 (ordering-move-safety.md, `OrderingDeclarationUtilitiesTests.cs`, `RegionDirectiveUtilities`).

## #994 — O2 RH7201–RH7311 (19)

- Rules: RH7201–RH7207, RH7301–RH7311, RH7305A. Reuses `shared/using-directive-ordering.md` (SHARED-USING) + FMT-USING; references `formatter/region-formatting.md` (FMT-REGION). May create a region-description (`RegionDescriptionAnalyzerBase`, RH7306/RH7307) and/or types-organized-with-regions (`TypesOrganizedWithRegionsAnalyzerBase`, RH7305/RH7305A) shared spec if >1 spec relies.
- Comments: P1–P7 note. Body notes #965, #966, #964 (do not refile).
- Follow-ups: #1105 (RH7201/RH7203/RH7207 compare names by spelling — P3 bug, don't refile); #1151 (RH7302 fix renames `#endregion` comment only when it is exactly the region name); #1135 (RH7309 region swap changes static initializer order; SHARED-ORDER-007 guard missing); #1143 (RH7310 test uses two Fix All passes); #1083 (tabs in region names, formatter/RH5601 side); #965/#966/#964 (body).
- Code: `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety` (SHARED-USING owners); `UsingDirectiveOrderingCodeFixProviderBase`, `RegionCategoryUtilities`, `RegionCategory` (unowned); `RegionDirectiveUtilities` (FMT-REGION owner; used by RH7301/7303/7304/7308–7311 and both region bases); `OrderingDeclarationUtilities` (SHARED-ORDER owner; RH7308).
- Collisions: #995 (RegionDirectiveUtilities, OrderingDeclarationUtilities, Core.Test `RegionDirectiveUtilitiesTests.cs`; both may create a region-named shared spec — agree file names: e.g. `shared/region-descriptions.md` vs `shared/member-region-grouping.md`); #993 (SHARED-ORDER via RH7308/#1135).

## #995 — O3 RH7401–RH7412, RH7501 (13)

- Rules: RH7401–RH7412, RH7501. Creates a new shared spec (member grouping by region; owners `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`; plus `OverrideMemberUtilities`, `InterfaceImplementationUtilities` unowned). RH7501 standalone, references `formatter/switch-case-braces.md` (FMT-SWITCH).
- Comments: P1–P7 note only.
- Follow-ups: none name RH74xx/RH7501. FMT-SWITCH formatter issues relevant to RH7501's policy reference: #948 (bracing breaks cross-section locals), #949 (comment before terminal break), #950 (dead `RemoveBraces` path) — don't refile.
- Code: both bases call `RegionDirectiveUtilities` (FMT-REGION owner) and `OrderingDeclarationUtilities` (SHARED-ORDER owner) → reference those IDs under `Uses`, do not claim the types.
- Collisions: #994 (see above); #993 (only if #993 extends SHARED-ORDER with what #995 would also reference/extend).

## #996 — B1 RH0002–RH2109 (18)

- Rules: RH0002, RH1001, RH1002, RH2001–RH2004, RH2006, RH2007, RH2101–RH2109. May create struct-equality (`StructEqualityPerformanceAnalyzerBase`, `EqualityComparerArgumentUtilities`, RH1001/RH1002) and nested-type (`NestedTypeAnalyzerHelper`, RH2101/RH2104–RH2108) shared specs if >1 spec relies. `analyzer/RH2005.md` neighbour.
- Comments: #978 note (2026-10-17) — `CasingUtilities` is a code owner of `shared/casing.md`; RH2001 fix uses `ToUnderlineCamelCase`, so RH2001 lists SHARED-CASING IDs under `Uses`, does not restate naming. P1–P7 note.
- Follow-ups: #967 (RH2005 fix / `DeclarationModifierUtilities`, maybe RH2004; body); #971 (casing acronym `_hTTPClient` — hits RH2001's field name via `CasingUtilities`; don't refile); #1137 (RH2005 non-goals now decided by SHARED-FIX-003); #1084/#1073 (casing fix guards — check whether RH2001 fix is affected, refer don't refile).
- Code: `DeclarationModifierUtilities` (RH2005 owner; RH2004), `ModifierOrderingUtilities` (RH2001; unowned), `CasingUtilities` (casing.md), `DebugMessageTextAnalysisUtilities`.
- Collisions: #993 (modifier helpers + their Core.Test files).

## #997 — B2 RH3001–RH3208 (21)

- Rules: RH3001–RH3007, RH3101–RH3107, RH3201, RH3203–RH3208. May create an expression-body shared spec (`ExpressionBodyAnalyzerBase<TNode>`, `ExpressionBodyToBlockCodeFixProviderBase<TNode>`; `ExpressionBodyRewriteUtilities` is today a code owner of `analyzer/RH3202.md` and is named in FMT-STRUCT) and an empty-parentheses spec (`EmptyParenthesesAnalyzerBase<TNode>`, RH3106/RH3107) if >1 spec relies.
- Comments: P1–P7 note. Body escalation rule: moving an existing RH3202 requirement retires its ID → escalate to maintainer first.
- Follow-ups: #1067 (out of range, open: decide spec owner of `BlankLineCodeFixUtilities`, shared by RH3208 and RH5030 — #997 must answer it); #963 (comment after `=>` glued to `{` — formatter, shared converter; body); #1143 (RH3001/RH3002/RH3004/RH3102 test classes use two Fix All passes); #1150 (P7, if any RH3xxx fix delegates to formatter).
- Code: `SyntaxNodeUtilities` (unowned, shared with #991 and many others), `SpeculativeRebindingHelper`, `StringInterpolationUtilities`, `BlankLineCodeFixUtilities` (unowned, RH5030 spec exists), `DocumentationAnalysisUtilities` (RH3201; element-documentation owner → Uses only).
- Collisions: minor with #984/#985 (RH3201 reads SHARED-DOC, must not extend element-documentation.md); minor with #991 (`SyntaxNodeUtilities` — neither should claim it).

## Collision matrix (material only)

| Pair | Shared spec | Shared source / tests | Risk |
|---|---|---|---|
| #984–#985 | element-documentation.md (both would mint SHARED-DOC-014+) | `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `DocumentationCommentCodeFixUtilities` (RH8107 owner; RH8202 vs RH8304/RH8308), `XmlDocumentationElementOrderingUtilities` (RH8204 vs RH8309, unowned — both may claim); Analyzer.Test `Core/DocumentationAnalysisUtilitiesTests.cs`, Core.Test `XmlDocumentationElementOrderingUtilitiesTests.cs` | High — must not run together (already noted) |
| #994–#995 | formatter/region-formatting.md (FMT-REGION) referenced by both; both may create a region-named shared spec; SHARED-ORDER referenced by both | `RegionDirectiveUtilities` (all region bases of both), `OrderingDeclarationUtilities` (RH7308 vs both #995 bases); Core.Test `RegionDirectiveUtilitiesTests.cs`, `OrderingDeclarationUtilitiesTests.cs` | Medium-high — avoid together |
| #993–#994 | ordering-move-safety.md (SHARED-ORDER-008+; #1135 RH7309 cites SHARED-ORDER-007) | `OrderingDeclarationUtilities`, `RegionDirectiveUtilities` (RH7110) | Medium — avoid together |
| #993–#995 | ordering-move-safety.md (if either extends) | `OrderingDeclarationUtilities` | Low-medium — OK if #995 only `Uses` SHARED-ORDER |
| #993–#996 | possible new modifier-ordering spec vs RH2001/RH2004 `Uses`; RH2005.md owner of `DeclarationModifierUtilities` | `ModifierOrderingUtilities` (RH2001 vs RH7105/RH7106), `DeclarationModifierUtilities`; Core.Test `ModifierOrderingUtilitiesTests.cs`, `DeclarationModifierUtilitiesTests.cs` | Medium — avoid together |
| #984/#985–#997 | element-documentation.md read by RH3201 | `DocumentationAnalysisUtilities` | Low |
| #991–#997 | — | `SyntaxNodeUtilities` (unowned) | Low |
| all | requirement-ids.txt, specs/README.md | test base classes untouched | Textual merge only |

## Parallel recommendation

Wave 1 (6, least collision): **#984, #991, #992, #994, #996, #997**.
- #984 vs others: only #997's RH3201 reads SHARED-DOC (tell #997: Uses only, no new SHARED-DOC IDs).
- #994 vs #996/#991/#992/#997: no shared helper beyond test infrastructure.
- #996 vs #994: none; #996's conflicting partner #993 waits.
- #991 vs #997: `SyntaxNodeUtilities` — tell both not to claim it.

Wave 2 (after wave 1 merges): **#985** (after #984), **#993** (after #994 and #996), **#995** (after #994). #993 and #995 may run together if #995 is told to only `Uses` SHARED-ORDER/FMT-REGION and not add requirements to ordering-move-safety.md; otherwise run #995 then #993.

Must not run together:
- #984 + #985 — element-documentation.md ID counter (SHARED-DOC-014+), `DocumentationCommentCodeFixUtilities` (RH8107 owner), `XmlDocumentationElementOrderingUtilities` (RH8204/RH8309).
- #994 + #995 — `RegionDirectiveUtilities`/FMT-REGION, `OrderingDeclarationUtilities`, both may create a region-named shared spec.
- #993 + #994 — ordering-move-safety.md (#1135 RH7309 ↔ SHARED-ORDER-007), `OrderingDeclarationUtilities`, `RegionDirectiveUtilities`.
- #993 + #996 — `ModifierOrderingUtilities`, `DeclarationModifierUtilities` (RH2005 owner), possible modifier-ordering shared spec that RH2001/RH2004 would `Use`.
