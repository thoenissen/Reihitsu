Role: spec author
Commit: f5d9c31
Verdict: spec written

Inputs: thoenissen/Reihitsu#1331 (body and both comments), `00-lead.md`, `sdd/README.md`, the shared specs named below, RH7102–RH7110, RH7004, RH7309, RH7409, RH8301–RH8303, RH8307, RH8401, RH8402, RH8501, and the analyzers/fixes named under "How conformance was established". No intended fix or verdict was passed in my prompt.

No change doc: the lane is "specs and tests for existing behavior", the principles were decided in thoenissen/Reihitsu#912, and the behavior changes they imply belong to the follow-ups named in the issue's first comment.

## Changed requirements

- SHARED-DIAG-009 — A verdict does not depend on incidental placement or order — added (P25), `shared/diagnostics.md`
- SHARED-DIAG-010 — A layout rule does not report a shape the formatter keeps — added (P31), `shared/diagnostics.md`
- SHARED-DIAG-004 — A partial declaration is one declaration — changed (P28): the "not covered" sentence about what one part writes became a MUST (per-part rules judge, report and fix each part on its own); names/parameters/members/regions of one part stay not covered; Why extended; RH7104 two-part example added.
- SHARED-FIX-003 — A fix that could break the code is withheld — clarified (wording only, issue comment 2 item 1): "a rule reports the construct it names for every input" became "withholding a fix never removes a report"; which inputs are reported at all points to SHARED-DIAG-008, SHARED-DIAG-010, SHARED-JOIN-001.
- SHARED-FIX-007 — The changed code is laid out as the formatter lays it out — changed (P26): new first bullet "Moved code" (keeps its own layout, only indentation follows the new nesting, separators per SHARED-ORDER-002, created code formatted); Why extended; RH7309 and RH7004 examples added.
- SHARED-ORDER-004 — A region boundary is not crossed — changed (P27): the requirement now also states the verdict scope (each top-level region checked on its own, nested region belongs to its top-level region) before the existing move guard; Why extended; two examples added before the existing one.
- SHARED-DOC-004 — The documentation comments of a declaration precede it — changed (P29): a `///` comment behind code on its line counts as a documentation comment of the following declaration, layout rules judge it as if it started its line, only the RH8303 fix moves it; Why extended; RH8026, RH8302/RH8303 and RH8303 fix examples added.
- SHARED-DOC-008 — title changed to "Documentation elements are not judged when documentation is not parsed" — changed (P30): the gate applies to rules that read documentation elements; documentation rules that read none (RH8401, RH8402, RH8501) check in every mode; Why extended; RH8501 and RH8401 examples added.
- SHARED-DIAG-008 (P22 clarification) — no change. Already present: the judged-span list (modifier list, declarator list, accessor list, comma gap, namespace header), the sentence that a conditional directive elsewhere (body) does not stop the report, and the RH7003 body example. I did not add the issue's RH7101 declarator-list example: the requirement already states it, and `RH7101DoNotCombineFieldsAnalyzer` reports regardless of directives (non-conforming, thoenissen/Reihitsu#1337).
- SHARED-USING-007 (P18 clarification) — no change. Already present: its second paragraph extends the every-earlier-element comparison to every ordering rule, and its last example is the issue's RH7110 example.

Other text edits:
- `sdd/README.md`, P11 section closing paragraph: a layout rule that does not report a shape the formatter deliberately keeps is not a gap of either P11 kind; SHARED-DIAG-010 (P31) asks for that silence.
- `sdd/specs/requirement-ids.txt`: SHARED-DIAG-009, SHARED-DIAG-010 inserted after SHARED-DIAG-008.
- Uses lines: `shared/diagnostics.md` (+FMT-DOC-012, +SHARED-FIX-005), `shared/code-fixes.md` (+SHARED-DIAG-008, +SHARED-DIAG-010, +SHARED-JOIN-001, +SHARED-ORDER-002), `shared/ordering-move-safety.md` (+SHARED-TYPEREGION-006), `shared/element-documentation.md` (+SHARED-FIX-006), `analyzer/RH7110.md` (+SHARED-ORDER-004).
- `shared/ordering-move-safety.md` intro and two non-goals now name the region scope of SHARED-ORDER-004 (RH7110 included); the non-goal "Layout of the moved code … Every fix lays the result out with the formatter" now points at SHARED-FIX-007 instead of asserting formatter layout, which P26 contradicts.
- `shared/element-documentation.md` intro ("in which documentation modes documentation rules run") and the RH8301–RH8309 non-goal ("apart from which comments they judge (SHARED-DOC-004)").
- RH7102-003, RH7103-004, RH7109-003, RH7110-003: one reference to SHARED-ORDER-004 each, so the restated region scope points at the shared statement. No normative change.

`scripts/test.sh --project architecture --filter "…SpecConsistencyTests|…SpecStructureTests|…RequirementCoverageTests"`: 38 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, only SHARED-DIAG-009 and SHARED-DIAG-010 uncovered (expected; the test implementer adds them).

## Rules each requirement applies to, and which shapes conform today

How conformance was established: by reading the analyzer/fix source named, not by running the analyzers. The test implementer has to confirm each "conforms" by a passing test.

- **SHARED-DIAG-009 (P25)** — every rule; the instances named by the sources are RH7401–RH7412 (thoenissen/Reihitsu#1334).
  - Conforms: second example (RH7409, base list in the other part of a partial type; `InterfaceImplementationUtilities` is symbol-based), and class `D : IB, IA` with region `IB` of the first example.
  - Does not conform: class `C : IA, IB` with region `IB` of the first example (RH7409 asks for `IA`) — thoenissen/Reihitsu#1334. Also #1304 Q1 (RH7405 counts only override methods), Q4 (override in another part), Q6 (partial members) — #1334.
- **SHARED-DIAG-010 (P31)** — layout rules whose fix lays out the reported code as the formatter does: the line-join rules using SHARED-JOIN-001 (RH5101, RH5105–RH5108, RH5113), and RH8307; per the 1260 auditor decision also RH5408-003 and RH5307-001 are P31 shapes. Not: order/content rules (RH7102–RH7110, RH7203…, RH8306, RH8309).
  - Conforms: the `///` element of the RH8307 example (RH8307-001), the RH7103 order-rule example, and every SHARED-JOIN-001 shape (comment or directive in the gap not reported), which the requirement names as an instance.
  - Does not conform: the `/** */` element of the RH8307 example — thoenissen/Reihitsu#1347.
- **SHARED-DIAG-004 amendment (P28)** — RH7104, RH7105, RH7106 (per-part); the symbol part is unchanged.
  - Conforms: the RH7104 two-part example (`RH7104PartialElementsMustDeclareAccessModifierAnalyzer` reports per declaration; the fix writes `DeclaredAccessibility` of the symbol, `public`). RH7104-001 covers only a single-declaration type, so this shape is specified only here.
- **SHARED-FIX-003 wording** — every code fix; no behavior change; existing covering tests stay.
- **SHARED-FIX-007 Moved code (P26)** — every fix that moves or re-nests code: RH7004, RH7102, RH7103, RH7107, RH7108, RH7109, RH7309, the using-directive fixes RH7201–RH7207 (thoenissen/Reihitsu#1335 for the organization fixes).
  - Conforms: the RH7309 example (the fix reorders region blocks as text and does not call the formatter). RH7004-004's and RH7004-006's own examples conform as well (moved statements already canonical, only re-indented).
  - Does not conform: the RH7004 example with `s.Flush( );` (`RH7004…CodeFixProvider` formats the new `using` statement including the moved statements) — thoenissen/Reihitsu#1335. The member and accessor ordering fixes format the containing type/member (`TypeMemberOrderingCodeFixProviderBase`, RH7107, RH7108 fixes call `FormatNodeInDocumentAsync`), so a moved member with non-canonical layout does not conform either — #1335.
- **SHARED-ORDER-004 amendment (P27)** — RH7102, RH7103, RH7109, RH7110 (verdict scope); the move guard as before for RH7102, RH7103, RH7107, RH7108, RH7109.
  - Conforms: the first new example (RH7103 and RH7110 use `RegionDirectiveUtilities.GetTopLevelRegions`); the existing RH7103-004 and RH7110-003 tests.
  - Does not conform: the RH7102 example (`RH7102ConstantsMustAppearBeforeFieldsAnalyzer` ignores regions) — thoenissen/Reihitsu#1339. Note: RH7102-001/-003 and RH7109-001/-003 already specified the region scope before this run, so RH7102 and RH7109 were already out of line with their own specs on `main` (same follow-up).
- **SHARED-DOC-004 amendment (P29)** — the documentation layout rules RH8301–RH8305, RH8307, RH8308; the fix clause binds every documentation fix and names RH8303's.
  - Conforms (expected): the RH8026 example — Roslyn's lexer never puts a `///` comment into trailing trivia, so it is leading trivia of `B`; RH8026 reads leading trivia. Possibly RH8304/RH8305/RH8308/RH8309 already judge such a comment (they read the documentation trivia without a line-start check); not verified.
  - Does not conform: the RH8302/RH8303 reporting example and the RH8303 fix example — thoenissen/Reihitsu#1345. RH8301-001, RH8302-001 and RH8303-001 exclude "a documentation comment that starts behind code" as not covered, so the rule specs do not contradict the amendment.
- **SHARED-DOC-008 amendment (P30)** — gate: every rule using `DocumentationModeAnalysisContextExtensions` that reads documentation elements (RH8013, RH8014, RH8030–RH8033, RH8101–RH8111, RH8201–RH8204, RH8301–RH8309, and the split element rules); every-mode: RH8401, RH8402, RH8501.
  - Conforms: the existing RH8001 example; the RH8501 example (`RH8501…Analyzer` registers without a documentation-mode check).
  - Does not conform: the RH8401 example (registered with `RegisterSyntaxNodeActionWithDocumentationModeCheck`), and RH8402 (returns early for `DocumentationMode.None`) — thoenissen/Reihitsu#1346. RH8401-001 and RH8402-003 cover only `Parse`/`Diagnose`, so they do not contradict.

## Examples for the tests

SHARED-DIAG-009, SHARED-DIAG-010, SHARED-DIAG-004, SHARED-FIX-007, SHARED-ORDER-004, SHARED-DOC-004, SHARED-DOC-008 — only the shapes listed as conforming above. No test for a non-conforming shape, and no characterization test that pins the opposite of a requirement (lead's instruction).

## Could not fit into the ID plan

Nothing needed a new ID. Two fits the lead should know about:

- SHARED-ORDER-004 lives in "Ordering move safety", whose scope was the fixes' moves. Adding the verdict scope there required widening the intro and two non-goals (above). The alternative home would have been SHARED-TYPEREGION-006, which already excludes "a rule that uses a region only as the scope within which it compares members" from its innermost-region sentence.
- SHARED-DOC-008's title changed, because "Nothing is reported…" is no longer true for RH8401/RH8402/RH8501. The ID is unchanged.

## Open questions to file

None blocking. Principle instances found in passing (each "Decided by principle Pn (#912)"; the lead files them):

1. **P26, `sdd/specs/shared/statement-braces.md` — SHARED-BRACE-002.** It says the brace fix wraps the body "by laying out the `if` statement that owns it as the formatter does". Under SHARED-FIX-007's moved-code bullet, a body the fix nests into a new block keeps its own layout (only its indentation follows). Input: `if (a) M(a,b);` reported by RH5405; real output (per the requirement, not run): the body is formatted, `M(a, b);`. Decided: `M(a,b);` keeps its spacing. Touches SHARED-BRACE-002, SHARED-FIX-007. Belongs to `SDD open questions: sdd/specs/shared/statement-braces.md` (create if none exists). If the lead reads the brace body as code the diagnostic names rather than moved code, it is no instance.
2. **P26, `sdd/specs/analyzer/RH7103.md` non-goals.** Two non-goals describe the fix formatting the whole containing type and call its scope "an open question"; P26 now decides it for the moved member. No requirement contradicts SHARED-FIX-007. Probably a note on thoenissen/Reihitsu#1335 rather than a new question.

## Concurrency

`shared/code-fixes.md` header (Uses line) changed besides SHARED-FIX-003 and SHARED-FIX-007; thoenissen/Reihitsu#1380 edits SHARED-FIX-008 in the same file, so a merge may touch the header line. SHARED-FIX-008 is untouched.
