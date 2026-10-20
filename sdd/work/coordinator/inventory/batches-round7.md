# Inventory: batches #985 (D4), #995 (O3), #993 (O1) and #939 (round 7)

Read 2026-10-20 against `origin/main` at 8ed8a45 (round-6 PRs #1162-#1167 merged). Sources: batch bodies #985/#993/#995/#939, all open issues (176, titles) with bodies read for #968, #969, #970, #1134, #1136, #1139, #1143, #1144, #1148, #1150, #1153, #1091, #1132, #1076, #1242, #1260, #1264; `src/` helper usage per rule file; `sdd/specs/` on main.

## What round 6 changed for these batches

- On main now: `analyzer/RH7103.md`, `RH7201-7311`, `RH8001-8034`, `RH8101-8204`, `RH0001/2`, `RH1xxx-3xxx`, `RH4xxx`, `RH5xxx`; shared specs `element-documentation.md` (SHARED-DOC-001...016), `ordering-move-safety.md` (SHARED-ORDER-001...007), `region-descriptions.md` (SHARED-REGDESC), `type-regions.md` (SHARED-TYPEREGION-001...006), `comment-placement.md` (SHARED-COMMENT-001...010), `empty-type-declarations`, `trailing-commas`, `statement-braces`, `statement-blocks`, `expression-body-conversion`, `nested-types`, `struct-equality`, `initializer-layout`, `blank-line-after-brace-fixes`, `debug-message-text`.
- Enforced for coverage: `analyzer/`, `cli/`, `shared/code-fixes.md`, `shared/diagnostics.md`, plus every shared requirement an enforced spec lists under `Uses`. So the three batches' own tags are enforced; the formatter/other shared specs are not (that is #939's job).
- Ownership facts that differ from batches-round6.md:
  - `CopyrightHeaderTemplateResolver` is now a code owner of `analyzer/RH0001.md` (RH8402 `Uses` RH0001 IDs only; it is not isolated any more).
  - `DeclarationModifierUtilities` is owned by `analyzer/RH2005.md` (merged); `ModifierOrderingUtilities` is still owned by nobody (RH2001/RH2004 specs do not list it, `Uses` is empty there).
  - `RegionCategoryUtilities` is owned by `analyzer/RH7309.md`.
  - `RegionDirectiveUtilities` is owned by `formatter/region-formatting.md` (FMT-REGION-001...006).
  - `DocumentationCommentCodeFixUtilities` is owned by `analyzer/RH8107.md` (RH8107-006/007); RH8202 already references those IDs under `Uses` (decision `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md`); #1242 (open) asks whether they move to shared.
  - `XmlDocumentationElementOrderingUtilities`: still unowned. RH8204 (merged) does not use it; only RH8309 does, so #985 may claim it alone.
  - `OverrideMemberUtilities`, `InterfaceImplementationUtilities`, `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`: unowned, only RH7401-RH7412 use them.
- Reserved IDs by open spec issue #1260 (SDD, not yet merged): SHARED-DIAG-004...008, SHARED-DOC-017, SHARED-USING-007, SHARED-USING-004 text change, SHARED-TYPEREGION-002/006 text change. #1091 will edit SHARED-DOC-005/006/007/010/011/012/013 and retire the case-insensitive part of RH8107-001. #1136 will extend SHARED-FIX-003; #1138 adds SHARED-FIX-006/007 (fix scope/layout). None of the three batches may mint any of these IDs or edit those texts.

## #985 — D4 RH8301-RH8309, RH8401, RH8402, RH8501 (12 rules)

Rules / classes (src, Analyzer.Test):
- RH8301: `DocumentationAnalysisUtilities`, `DocumentationCommentUtilities`, `FormattingTextAnalysisUtilities`; fix too. Tests `Formatting/RH8301...Tests.cs` (+ `Formatter/Formatting/RH8301...FormatterTests.cs`), Batch base, `NumberOfFixAllIterations = 1`.
- RH8302, RH8303: `DocumentationAnalysisUtilities`, `FormattingTextAnalysisUtilities` (RH8303 also `SyntaxTriviaUtilities`). Tests in `Formatting/` plus `Formatter/Formatting/*FormatterTests.cs`.
- RH8304 (fix: `DocumentationCommentCodeFixUtilities`), RH8305 (+ FormatterTests), RH8306 (analyzer only, `AnalyzerTestsBase`), RH8307 (+ FormatterTests), RH8308 (fix: `DocumentationCommentCodeFixUtilities`), RH8309 (`XmlDocumentationElementOrderingUtilities`, analyzer + fix). Tests in `Documentation/`.
- RH8401 (fix: `DocumentationCommentUtilities`), RH8402 (`CopyrightHeaderTemplateResolver`, tests in `Documentation/`), RH8501 (analyzer only, test in `Design/`).
Shared specs: extends or references `shared/element-documentation.md` only as `Uses` (SHARED-DOC-001...016; mint SHARED-DOC-018+ only if truly needed, never -017), `shared/comment-placement.md` is not relevant; references `formatter/documentation-comments.md` (FMT-DOC-005 one space, -006 exterior-only lines, -007 later slashes, -008 summary three lines, -009 content moved off start-tag line, -010 canonical shapes, -012 delimited comments) for RH8301/RH8304/RH8305/RH8307/RH8308/RH8401, and FMT-BLANK-011 (`formatter/blank-lines`) for RH8302/RH8303; RH8402 `Uses` RH0001 IDs; RH8501 standalone.
Test files/bases: all existing bases; no base-class edit. Core.Test `XmlDocumentationElementOrderingUtilitiesTests.cs` (RH8309 owns, if claimed), Analyzer.Test `Core/DocumentationAnalysisUtilitiesTests.cs` (SHARED-DOC, do not retag).

### Lead prompt (a): read first
- #985 body and 2 comments (P1-P7 note, P8 note); `sdd/specs/shared/element-documentation.md`, `analyzer/RH8107.md` (RH8107-006/007), `analyzer/RH8202.md` (model for rule-to-rule `Uses`), `analyzer/RH8204.md` (neighbour; non-goal names RH8301-RH8309), `formatter/documentation-comments.md`, `shared/code-fixes.md`, `shared/diagnostics.md`, `analyzer/RH0001.md` (copyright header).
- Decisions: #1242 (owner of `DocumentationCommentCodeFixUtilities`), #1153, #1091, #1148, #1150, #1139/#1077, #1260 (reserved IDs).

### (b) Decided follow-ups touching these rules
- #1242 [open]: RH8304 and RH8308 fixes use `DocumentationCommentCodeFixUtilities` (RH8107-006/007, element removal incl. `CanRemoveElement`). Do as RH8202 did (list RH8107-006/007 under `Uses` where behavior relied on), claim nothing, and add a comment on #1242 that two more specs rely (this is the "third spec" question). Moving them to shared = retiring RH8107 IDs = maintainer.
- #1150 (P7): RH83xx fixes delegate to the formatter and are offered as no-ops on syntax-error/generated documents. Plain instance for a one-line comment; do not refile. Applies to RH8301-RH8308 fixes.
- #1148: FMT-BLANK-011 narrowed; "RH8303 is mirrored as well" - RH8303 spec describes current scope only; plain instance.
- #1153 / #1091 (P8, P8c): affect RH8305 / RH8306 (summary "text that renders", which summary), tag case/namespace (P8) and RH8309 element order; plain instances, not new questions. #1132 (name matching) is RH8101-8109 only, not these rules.
- #1139 (P12/P13) and #1077 (P6): a `///` that documents nothing and `/** */` are ordinary comments - plain instance for RH8401 when its report/fix depends on this.
- #961 (doc element end tag), #962 (stray `///` relocation), #959 (comment line ends): formatter/FMT-DOC side, do not refile.
- #1144 (P10), #1140 (P9): fix layout/scope principles for RH83xx fixes; #1143: RH8301 test uses one Fix All iteration (fine), no two-pass requirement.
- #1241 (P19, SHARED-DOC-017 end tag with another name): RH8107/RH8202/RH8204 only; RH8304/RH8308/RH8309 have the same exposure only if their fixes touch mismatched end tags - plain instance, do not mint SHARED-DOC-017.
- #1240 (diagnostics.md non-goal on inactive `#if`): not applicable here except as a general note.

### (c) Open question issues: none for RH8301-RH8501 (no `SDD open questions: analyzer/RH83xx.md` exists; closed round-7 question issues #1195-#1200 concern RH81xx/RH82xx).

### (d) Must not claim
`DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander` (element-documentation owners), `DocumentationCommentCodeFixUtilities` (RH8107), `CopyrightHeaderTemplateResolver` (RH0001), `DocumentationCommentUtilities`/`FormattingTextAnalysisUtilities`/`SyntaxTriviaUtilities` (unowned Core helpers; reference only unless clearly single-owner); no edits to `element-documentation.md` text, SHARED-DOC-017, SHARED-DIAG-004+. May claim: `XmlDocumentationElementOrderingUtilities` (via RH8309).

## #995 — O3 RH7401-RH7412, RH7501 (13 rules)

Rules / classes: RH7401-RH7404 (override grouping) and RH7409-RH7412 (interface grouping) via `MembersShouldBeGroupedByRegionsAnalyzerBase` + `OverrideMemberUtilities` / `InterfaceImplementationUtilities`; RH7405-RH7408 via `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase` + `OverrideMemberUtilities`. All eight bases call `RegionDirectiveUtilities` (6 call sites) and `OrderingDeclarationUtilities` (2). RH7501: analyzer + fix, no helper. No code fixes for RH74xx.
Tests: `Formatting/RH74xx...AnalyzerTests.cs` (all `AnalyzerTestsBase<TAnalyzer>`, no fix tests), `Formatting/RH7501...` (Batch base). No Core.Test files dedicated to these utilities.
Shared specs: creates one new shared spec for region-based member grouping (suggested file `shared/member-region-grouping.md`, prefix `SHARED-REGGROUP` - do not use `SHARED-REGION` (taken by region-directive-blank-lines), `SHARED-REGDESC`, `SHARED-TYPEREGION`). `Uses` FMT-REGION-003 (nested regions, as type-regions.md does) and type-regions.md's SHARED-TYPEREGION-006 where the region ownership is relied on; SHARED-ORDER only if relied on. RH7501 standalone, `Uses` FMT-SWITCH-002 (terminal `break` stays outside the block), FMT-SWITCH-001/-008/-009 where relevant.

### (a) Read first
#995 body and 1 comment (P1-P7); `shared/type-regions.md` and `shared/region-descriptions.md` (format model for a region-based shared spec; code-owner pattern is a base class), `analyzer/RH7309.md`, `analyzer/RH7308.md` (member/region judgments), `formatter/region-formatting.md`, `formatter/switch-case-braces.md`, `shared/code-fixes.md`; #1264, #1260.

### (b) Decided follow-ups
- #1264 (P17, behavior change, blocked by #1260): innermost type-like declaration (enums included) and innermost region own a member; exclusive `#if/#else` branches not compared. Names RH7308/RH7309/RH7311 but the same ownership question arises for RH74xx bases (regions around members of a nested enum/type, nested regions). Plain instance - describe current behavior only, file nothing, one-line comment on #1264 if the RH74xx bases are affected.
- #1240 (diagnostics.md inactive `#if` non-goal also covers region directives): applies to the grouping bases that read region directives; plain instance.
- #1135 (RH7309 static init order), #1262/#1263/#1265 (RH7303/RH7304/RH7306/RH7309 fixes): not RH74xx.
- RH7501 fix: #1150 (P7, formatter-delegation no-op; verify RH7501 fix path, it uses `SyntaxFactory` not the formatter, so probably not applicable); formatter side #948 (bracing breaks cross-section local), #949 (comment before terminal break), #950 (dead `RemoveBraces` path): FMT-SWITCH side, do not refile. #1143 (Fix All one application): RH7501 test class has no two-pass.
- #1136/#1137/#1138 (SHARED-FIX-003/006/007 text): general fix principles; RH7501 fix is the only fix in the batch.

### (c) Open question issues: none (no `SDD open questions` for RH74xx/RH7501; none named in the search of the open issues).

### (d) Must not claim
`RegionDirectiveUtilities` (FMT-REGION), `OrderingDeclarationUtilities` (SHARED-ORDER), `RegionCategoryUtilities` (RH7309); must not add requirements to `ordering-move-safety.md`, `type-regions.md`, `region-descriptions.md`, `formatter/*`. May claim: the two bases (as code owners of the new shared spec), `OverrideMemberUtilities`, `InterfaceImplementationUtilities` (as owners of the new shared spec or leave unowned like RH7309's utilities - choose one, do not put them in a rule spec as well).

## #993 — O1 RH7001-RH7004, RH7101, RH7102, RH7104-RH7110 (13 rules)

Rules / classes: RH7001/RH7002 (`Design/` tests), RH7003, RH7004 (analyzer + fix, `Formatting/`, test sets `NumberOfFixAllIterations = 2`), RH7101 (fix: `LanguageVersionResolver`; tests `Formatting/RH7101...` + `Formatter/Formatting/RH7101...FormatterTests.cs`); RH7102 and RH7109 (`OrderingDeclarationUtilities` + `TypeMemberOrderingCodeFixProviderBase`, test iter=2); RH7104 (`OrderingDeclarationUtilities`, fix `DeclarationModifierUtilities`); RH7105/RH7106 (`ModifierOrderingUtilities`, `DeclarationModifierUtilities`, fix base `ModifierOrderingCodeFixProviderBase`); RH7107/RH7108 (`AccessorOrderingUtilities`, analyzer + fix); RH7110 (`OrderingDeclarationUtilities`, `RegionDirectiveUtilities`, analyzer only). Tests in `Ordering/` (RH7102-RH7110), `Formatting/` (RH7003/7004/7101), `Design/` (RH7001/7002).
Shared specs: `Uses` SHARED-ORDER-001...007 for RH7102/RH7109 (and RH7107/RH7108 where accessor moves cross directives), exactly like `analyzer/RH7103.md` (copy its non-goals/model, with #1137 replacing the open-question lines); `Uses` FMT-REGION for RH7110; `Uses` RH2005 requirements for `DeclarationModifierUtilities` behavior (RH7104-RH7106); modifier-ordering shared spec only if RH7105 and RH7106 rely on the same requirement (`ModifierOrderingCodeFixProviderBase`; unowned `ModifierOrderingUtilities`). Next free ID in ordering-move-safety.md would be SHARED-ORDER-008 but #970 and #1134 add there later: do not extend it.

### (a) Read first
#993 body + 1 comment (P1-P7; #968/#969/#970 named); `analyzer/RH7103.md` (model), `shared/ordering-move-safety.md`, `analyzer/RH2005.md`, `analyzer/RH2004.md` (neighbours using `DeclarationModifierUtilities`), `shared/code-fixes.md`, `formatter/region-formatting.md`; #1137, #1136, #1143, #1150.

### (b) Decided follow-ups (all plain instances unless marked; describe current behavior only where it is not the reported defect)
- #968 [bug]: `OrderingDeclarationUtilities.GetAccessibilityGroup` groups unmodified members as `None` (written vs effective accessibility). Issue asks whether RH7110/RH7104/other ordering rules share the cause - RH7110 and RH7109 (`GetEffectiveAccessibilityGroupForReadonlyOrdering` already treats interface members as public) are the comparison; RH7110 spec must leave the unmodified-member question out and cite #968 (no refile).
- #969 [bug]: RH7102/RH7109 fixes format the whole containing type (`TypeMemberOrderingCodeFixProviderBase`). The same RH7103 test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` pattern may exist in RH7102/RH7109 tests; do not encode it as a requirement; plain instance.
- #970 [bug]: balanced `#pragma` pair in a crossed body blocks a move - SHARED-ORDER behavior for RH7102/7107/7108/7109; plain instance.
- #1134 [bug]: RH7109 fix reorders fields of structs with sequential layout / `[StructLayout(Sequential)]` (SHARED-FIX-003); plain instance, current fix reorders - not specified as intended.
- #1143: RH7004, RH7102, RH7109 test classes use `NumberOfFixAllIterations = 2` - plain instance, do not encode a two-pass behavior; keep the existing pass in the tests.
- #1150 (P7): RH7004 and the RH710x providers delegate to the formatter, offered as no-ops on syntax errors/generated documents - plain instance.
- #1136 (SHARED-FIX-003 relocation/reordering of comments/directives/disabled text) and #1137 (replace RH7103/RH2005 non-goals): the RH7102/RH7109 specs follow the model of RH7103 after #1137; comment relocation by the ordering fixes is a plain instance of #1136.
- #1135 (RH7309) shares SHARED-ORDER-007 but is in #994/RH7309, not here; #1140/#1144 (fix scope/layout P9/P10): RH7101 fix (field split) and RH7004 fix are covered by those principles, plain instances.
- #1076: blank-line fixes (RH5xxx) only; no RH710x.
- #410 closed (RH7107/RH7108 conditional directives) - already fixed, characterization tests exist.

### (c) Open question issues: none for RH70xx/RH71xx (none exist; #926 and #927 were closed in earlier rounds).

### (d) Must not claim
`OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` (SHARED-ORDER owners), `DeclarationModifierUtilities` (RH2005), `RegionDirectiveUtilities` (FMT-REGION); must not extend `ordering-move-safety.md` (no new SHARED-ORDER IDs); `SyntaxTriviaUtilities` (unowned Core helper). May claim: `ModifierOrderingUtilities` + `ModifierOrderingCodeFixProviderBase` (RH7105/RH7106 owners, or a shared modifier-ordering spec if both rely).

## #939 — Tag formatter, shared, CLI, Playground tests

Scope: tag Formatter.Test, Cli.Test, Playground tests with `[Requirement("FMT-...")]`/`CLI-...`/`SHARED-...` for the formatter specs, `shared/` specs, CLI and Playground; analyzer tests are not its scope.
Touchpoints with the three batches:
- Core.Test: `OrderingDeclarationUtilitiesTests.cs`, `AccessorOrderingUtilitiesTests.cs` (SHARED-ORDER), `RegionDirectiveUtilitiesTests.cs` (FMT-REGION), `XmlDocumentationElementOrderingUtilitiesTests.cs`, `ModifierOrderingUtilitiesTests.cs`, `DeclarationModifierUtilitiesTests.cs`, Analyzer.Test `Core/DocumentationAnalysisUtilitiesTests.cs` (SHARED-DOC): none carries `[Requirement]` today. #939 may tag the SHARED-ORDER/FMT-REGION/SHARED-DOC ones; the batches must not retag them (they cover their own requirements in Analyzer.Test rule files). Only edit these files if a new focused test of own requirement is unavoidable.
- `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md`: only #939 changes nothing there unless it invents IDs (it must not), so only textual conflicts among the three batches.
- Requirement attributes: `Core.Test/RequirementAttribute.cs`, `Analyzer.Test/Base/RequirementAttribute.cs`, `Cli.Test/Helpers/RequirementAttribute.cs` exist; #939 creates the attribute in the other test projects (Formatter.Test, Playground.Test) - no overlap.
- FMT-DOC, FMT-SWITCH, FMT-REGION, FMT-BLANK IDs: #985/#995/#993 only reference them under `Uses`; #939 tags the formatter tests of those IDs. No file overlap (formatter tests are in `Reihitsu.Formatter.Test`).
- A requirement of a new `SHARED-REGGROUP` (#995) spec is tagged by #995 itself (Analyzer.Test RH74xx files); #939 must skip it.
- #939 depends on nothing, and shared specs covered by `Uses` of enforced specs are already enforced: no wait necessary, but run #939 after the shared-spec text of #1260/#1091/#1136 is settled if it tags SHARED-DOC-005...013/SHARED-ORDER-006 tests (tag stability).

## Pairwise overlaps

| Pair | Shared spec file | Shared source / helper | Test files / bases | Verdict |
|---|---|---|---|---|
| #985-#993 | none (SHARED-DOC vs SHARED-ORDER) | none | none | free |
| #985-#995 | none | none | none | free |
| #993-#995 | `ordering-move-safety.md` only if #993 extends it (it must not); FMT-REGION referenced by both (`Uses` only) | `OrderingDeclarationUtilities` (#993 RH7102/7104/7109/7110, #995 bases x2), `RegionDirectiveUtilities` (#993 RH7110, #995 bases x6): both `Uses` only | Core.Test `OrderingDeclarationUtilitiesTests.cs`, `RegionDirectiveUtilitiesTests.cs` (neither edits) | low; OK if both only reference |
| #939-#985 | `element-documentation.md`/FMT-DOC referenced only | Analyzer.Test `Core/DocumentationAnalysisUtilitiesTests.cs`, Core.Test `XmlDocumentationElementOrderingUtilitiesTests.cs` | #939 tags; #985 may touch only for RH8309 | low |
| #939-#993 / #995 | SHARED-ORDER, FMT-REGION, FMT-SWITCH referenced | Core.Test util test files listed above | same | low; fenced by rule "batches tag only their own rule test files" |
| all | `requirement-ids.txt`, `specs/README.md` | - | - | textual merge only (append-only) |
| open spec issues | #1260 (SHARED-DOC-017, SHARED-DIAG-004...008, SHARED-USING-007, SHARED-TYPEREGION edits), #1091, #1136, #1138 | - | - | batches must not mint those IDs; the batches' `Uses` stay on existing IDs |

## Recommendation

All three batches (#985, #993, #995) can run in parallel as three squads, and #939 can run alongside them. Precondition (one line each in the lead prompts):
1. #985: `Uses` only for SHARED-DOC and RH8107-006/007 (comment on #1242); claims `XmlDocumentationElementOrderingUtilities`; no new SHARED-DOC-017 (reserved for #1260), no edit of element-documentation.md text.
2. #993: `Uses` SHARED-ORDER-001...007 and RH2005; adds no SHARED-ORDER ID; claims only modifier-ordering helpers (`ModifierOrderingUtilities`, `ModifierOrderingCodeFixProviderBase`); leave `DeclarationModifierUtilities` and the ordering utilities unclaimed.
3. #995: creates its own new shared spec with a unique prefix (`SHARED-REGGROUP`, file `shared/member-region-grouping.md`); `Uses` FMT-REGION/SHARED-TYPEREGION IDs only; claims neither `RegionDirectiveUtilities` nor `OrderingDeclarationUtilities`.
4. #939: does not retag the Core.Test/Analyzer.Test utility test files unless a batch has merged; avoid minting IDs; if run concurrently, tag FMT/CLI/Playground first and the SHARED-ORDER/SHARED-DOC/FMT-REGION utility tests last.
If any lead cannot honor 1-3, order: #995 first, then #993 (the only real collision is RH7110/RH7102/RH7109 vs the #995 bases sharing `OrderingDeclarationUtilities`/`RegionDirectiveUtilities` references), while #985 never needs to wait.
