# Squad prompts — Phase 5 (from 2026-10-24)

## #1441 — follow-up issue format (P5-1 item 7A)

```
/squad #1441

This run: add the follow-up issue format as a repository document, as #1441 states — `sdd/templates/follow-up-issue.md` with three variants (spec bug, clarification, internal), plus pointers in sdd/README.md (layout section) and squad/lead.md (where the lead drafts a follow-up issue for my approval). Decided by me: the format is NOT a public issue template; `.github/ISSUE_TEMPLATE/` stays unchanged, because every file there is offered to users in the "New issue" chooser. Behavior changes and new rules keep using the public feature-request templates.
Lane: tooling. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md ("Issue intake", "Asking the maintainer", "Code-fix coverage"), sdd/README.md, sdd/templates/ (existing templates as style reference), squad/README.md (lanes), squad/lead.md, .github/ISSUE_TEMPLATE/ (to keep the variants consistent with the public bug/feature fields), and #1441.

Running at the same time: nothing that touches sdd/README.md, sdd/templates/ or squad/.

Done means: the document exists with the three variants, each a short fill-in skeleton with the fields #1441 lists; sdd/README.md and squad/lead.md point to it; no public template, spec, code or test changed; CI green; PR "Closes #1441".

Must not: change .github/ISSUE_TEMPLATE/, specs, code or tests; rewrite existing issues; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

---

# Wave 1 (1.2.0 critical runs R1 R2 R3 R4 R5 R7) — 2026-10-24

Six runs in parallel. #1441 (tooling: sdd/README.md, sdd/templates/, squad/lead.md) may run at the same time — no W1 run touches those files. R6 and R8 follow in wave 2 (they overlap R4 and R3/R7).

Common decisions (P5-2, maintainer 2026-10-24) that every W1 prompt repeats: bug-fix lane without change doc; where a rule's older requirement text lags a newer shared requirement that records a decided principle, the run fixes the code to the newer requirement and aligns the older text in the same PR as a clarification (retire with reason or reword); a rule's coverage sentence that names fewer forms than a binding shared requirement is widened as a clarification. Extension parts were split off into separate vNext issues and are out of scope.

## R1 — formatter output that breaks the build or is not idempotent

```
/squad #948 #949 #954 #1390 #888

This run (5 issues, one lane: bug fix, no change doc), all milestone 1.2.0, critical: formatted code no longer compiles or changes again on a second pass.
- #948 switch bracing scopes a local a later section uses (CS0103) — FMT-PIPE-003, FMT-SWITCH-011.
- #949 comment before a terminal `break` gets its blank line only in pass 2 — FMT-PIPE-004, FMT-BLANK-011.
- #954 empty region right after `{`: BlankLinePhase and LineBreakPhase undo each other — FMT-PIPE-004.
- #1390 structural output does not compile (expression-bodied `async` member returning through an alias, CS1997) or moves again in pass 2 (`with` initializer) — FMT-PIPE-003/-004, FMT-STRUCT-006.
- #888 decided by me: SHARED-COMMENT-012 binds RH5201 and the formatter. The RH5201 report is correct; the defect is that the formatter does not break the line before `.D` behind a multi-line comment tail. Fix the formatter; the delegating RH5201 fix then converges (SHARED-FIX-001). Add SHARED-COMMENT-012 under `Uses` of the formatter line-break spec as a clarification.
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md, squad/README.md (incl. "Several issues in one run"), squad/lead.md, sdd/specs/formatter/pipeline.md, switch-case-braces.md, blank-lines.md, line-breaks.md, structural-transforms.md, sdd/specs/shared/comment-placement.md (SHARED-COMMENT-012), each issue with its comments.

Running at the same time (do not touch their files): R2 formatter documentation-comment phase, CleanupPhase, doc code fixes, sdd/specs/formatter/documentation-comments.md, cleanup.md, shared/element-documentation.md, sdd/specs/README.md; R3 RawStringAlignment, Indentation phase, Core FormattingTextAnalysisUtilities/RawStringLiteralUtilities/StringInterpolationUtilities, RH5601/RH5104 fixes, raw-string-alignment.md, indentation.md; R4 ordering/modifier fix bases, RH7xxx, Core OrderingMoveSafety/RegionDirectiveUtilities/DeclarationModifierUtilities/ModifierOrderingUtilities, shared/code-fixes.md; R5 casing fix base, CasingUtilities, shared/casing.md; R7 RH3xxx fixes/analyzers, RH5417 analyzer. Also do not touch sdd/README.md, sdd/templates/, squad/. Edits to sdd/specs/requirement-ids.txt: insert at the sorted position; on a merge conflict keep both sides sorted.

Done means: each issue reproduced by a regression test first (formatter tests incl. an idempotency/second-pass check for #949, #954, #1390), code fixed, specs clarified where the issue needs it, CI green, PR with one "Closes #N" per issue (or "Refs #N" for one that does not reproduce).

Must not: change behavior beyond these issues; implement split-off extension parts; add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text; follow-up candidates go into the run report.
```

## R2 — doc comments behind code or without end tag turn code into comment or lose text

```
/squad #1329 #1389 #1241 #1344 #1242

This run (5 issues, one lane: bug fix, no change doc), milestone 1.2.0 except #1242 (closes with this run):
- #1329 formatter: a declaration after an unclosed doc element becomes comment text.
- #1389 formatter: BOM before a line break dropped (FMT-CLEAN-009); doc text behind code deleted (FMT-PIPE-002).
- #1241 RH8204, RH8305, RH8307, RH8309 fixes rewrite an element without its own end tag and comment out the next declaration — SHARED-DOC-017, SHARED-FIX-003: withhold.
- #1344 RH8304/RH8309 fixes produce CS0102 / unparsable output or relocate comment text — SHARED-FIX-003: withhold. Relocation of a comment is unsafe per AGENTS.md "Code-fix coverage" (the SHARED-FIX-003 wording for relocation is being added by run R4 via #1136; do not edit shared/code-fixes.md here).
- #1242 decided by me: `DocumentationCommentCodeFixUtilities` moves as code owner from sdd/specs/analyzer/RH8107.md to sdd/specs/shared/element-documentation.md (shared doc-fix utility); update sdd/specs/README.md's owner index and the `Uses` lines of the doc rules that use it (RH8107, RH8202, RH8204, RH8304, RH8308, RH8309) as needed.
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md, squad/README.md, squad/lead.md, sdd/specs/formatter/documentation-comments.md, cleanup.md, pipeline.md, sdd/specs/shared/element-documentation.md, code-fixes.md (SHARED-FIX-003, read only), the rule specs RH8107, RH8202, RH8204, RH8304, RH8305, RH8307, RH8309, each issue with comments.

Running at the same time (do not touch their files): R1 SwitchCaseBraces, BlankLines, LineBreaks, StructuralTransforms, RH5201, formatter specs pipeline/switch/blank-lines/line-breaks/structural-transforms; R3 RawStringAlignment, Indentation, RH5601/RH5104, Core FormattingTextAnalysisUtilities; R4 ordering/modifier/region fix bases, RH7xxx, shared/code-fixes.md; R5 casing; R7 RH3xxx. Also not sdd/README.md, sdd/templates/, squad/. requirement-ids.txt: sorted insert; keep both sides on conflict.

Done means: each defect reproduced by a test first; the formatter preserves the declaration and the doc text; the four fixes withhold for the unsafe inputs and still fix the safe ones; owner moved; CI green; PR "Closes #1329", "Closes #1389", "Closes #1241", "Closes #1344", "Closes #1242".

Must not: widen any rule's reports; implement the formatter doc-relocation extensions (#962 etc.); edit shared/code-fixes.md; add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

## R3 — literal and raw-string content treated as layout

```
/squad #951 #1133 #1146

This run (3 issues, one lane: bug fix, no change doc), milestone 1.2.0:
- #951 raw strings: interpolation holes and nested literals are not idempotent (cases 1 and 2; FMT-PIPE-004, FMT-RAW). Case 3 (tabs) was split off as a vNext extension — out of scope.
- #1133 RH5601 reports the closing-line tab of an interpolated raw string; the fix breaks the build. Decided by me (J4): SHARED-DIAG-003 wins over RH5601-001/RH5602-001 — whitespace inside a literal or hole content is code; align RH5601-001/RH5602-001 as a clarification.
- #1146 Q1 RH5104 fix writes a comment into a verbatim/raw string (meaning change) and Q4 reorders two comments — withhold (SHARED-FIX-003; relocation per AGENTS.md "Code-fix coverage", wording added to SHARED-FIX-003 by run R4 — do not edit shared/code-fixes.md here). Decided by me (J1): SHARED-FIX-006 wins over RH5104-005 — the fix inserts no blank line before the moved comment; reword RH5104-005 as a clarification. Q3/Q5/Q6 were split off — out of scope.
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md, squad/README.md, squad/lead.md, sdd/specs/formatter/raw-string-alignment.md, indentation.md, pipeline.md, sdd/specs/shared/diagnostics.md (SHARED-DIAG-003), code-fixes.md (read only), sdd/specs/analyzer/RH5601.md, RH5602.md, RH5104.md, each issue with comments.

Running at the same time (do not touch their files): R1 SwitchCaseBraces, BlankLines, LineBreaks, StructuralTransforms, RH5201; R2 DocumentationComments, CleanupPhase, doc fixes; R4 ordering/modifier/region fix bases, RH7xxx, shared/code-fixes.md; R5 casing; R7 RH3xxx and RH5417. Also not sdd/README.md, sdd/templates/, squad/. requirement-ids.txt: sorted insert; keep both sides on conflict. Note: R8 (wave 2, #1075) will touch Core/FormattingTextAnalysisUtilities.cs after you.

Done means: reproductions first (second-pass formatter tests for #951); raw-string output stable; RH5601 no longer reports literal content; RH5104 fix withheld for literal/relocation inputs, no blank line inserted; specs aligned; CI green; PR "Closes #951" (state that case 3 moved to its split issue), "Closes #1133", "Closes #1146".

Must not: implement tab normalization in raw strings or the RH5104 widenings; edit shared/code-fixes.md; add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

## R4 — organization fixes that break code or change meaning must withhold

```
/squad #1136 #1134 #1135 #1262 #1332 #1336 #1340

This run (7 issues, one lane: bug fix, no change doc), milestone 1.2.0 except #1136 (Maintenance clarification, runs here first):
- #1136 decided by me: a clarification without change doc — SHARED-FIX-003 gains that a fix which relocates or reorders a comment or directive it does not own, or drops, edits or relocates disabled text, is withheld. AGENTS.md "Code-fix coverage" already states this policy. Write it first; the fixes below that relocate are bugs against it.
- #1134 RH7109 reorders layout-defining struct fields (offsets change) — withhold.
- #1135 RH7309 swap changes static initializer order — withhold.
- #1262 region fixes (RH7303/04/06/09) break the build or lose docs — withhold. Decided by me (J12): SHARED-FIX-003 wins over RH7309-004; align RH7309-004 as a clarification.
- #1332 RH7501 fix moves directives / breaks code (CS0103) — withhold. Q10 was split off — out of scope.
- #1336 RH7004, RH7101, RH7105/06 fixes produce unparsable code around directives — withhold.
- #1340 RH7003 must skip a namespace that contains a namespace (RH7003-001 already says so); the fix gives CS8955 today.
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs. Seven issues: the lead may ask me to split if the scope grows (escalation 11 applies only above eight).

Read first: AGENTS.md ("Code-fix coverage"), sdd/README.md, squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, ordering-move-safety.md, type-regions.md, member-region-grouping.md, the rule specs RH7003, RH7004, RH7101, RH7105, RH7106, RH7109, RH7303, RH7304, RH7306, RH7309, RH7501, each issue with comments.

Running at the same time (do not touch their files): R1 formatter phases; R2 doc phase/doc fixes, shared/element-documentation.md, sdd/specs/README.md; R3 raw strings, Indentation, RH5601/RH5104; R5 casing; R7 RH3xxx. You own sdd/specs/shared/code-fixes.md in this wave (R2 and R3 rely on the new SHARED-FIX-003 wording but do not edit it). Also not sdd/README.md, sdd/templates/, squad/. requirement-ids.txt: sorted insert; keep both sides on conflict. Note: R6 (wave 2) will touch Core/DeclarationModifierUtilities.cs after you.

Done means: SHARED-FIX-003 clarified with a covering test; each unsafe input reproduced first and the fix withheld (diagnostic stays), safe inputs still fixed; RH7003 silent for nested namespaces; RH7309-004 aligned; CI green; PR with "Closes #N" for all seven.

Must not: change what the rules report beyond #1340; implement split-off parts; add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

## R5 — casing fixes produce a still-reported or uncompilable name

```
/squad #1016 #1070 #1073

This run (3 issues, one lane: bug fix, no change doc), milestone 1.2.0:
- #1016 casing fixes produce a name the rule still reports — SHARED-FIX-001, SHARED-CASING-008.
- #1070 the casing fix renames names dictated by metadata or native interop (override of a metadata member → CS0115; DllImport → EntryPointNotFoundException) — withhold (SHARED-FIX-003). Only this fix part is in scope: the "do not report such names" part was split off as a vNext extension; the issue body still describes only that part — the triage comment on the issue states what stays.
- #1073 the casing fix makes unsafe renames (member named like its type → CS0542; record positional member) — withhold (SHARED-FIX-003).
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md, squad/README.md, squad/lead.md, sdd/specs/shared/casing.md, code-fixes.md (read only), each issue with comments.

Running at the same time (do not touch their files): R1 formatter phases; R2 doc phase/fixes; R3 raw strings/Indentation/RH5601/RH5104; R4 ordering/modifier/region fixes, Core DeclarationModifierUtilities, shared/code-fixes.md; R7 RH3xxx. Also not sdd/README.md, sdd/templates/, squad/. requirement-ids.txt: sorted insert; keep both sides on conflict.

Done means: each defect reproduced first; casing fixes produce a name the rule accepts or are withheld; metadata/interop-dictated and conflicting renames withheld; CI green; PR "Closes #1016", "Closes #1073", and for #1070 "Refs #1070" if its no-report part remains open there, else "Closes".

Must not: stop reporting any name (that is the split-off extension); add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

## R7 — clarity fixes that break code or delete comments; reports where the form is invalid

```
/squad #1257 #1279 #1281 #1282 #1417 #1422

This run (6 issues, one lane: bug fix, no change doc), milestone 1.2.0:
- #1257 RH3001/3005/3101/3105 fixes turn a comparison into a generic call (CS0307) — withhold.
- #1279 RH3004 fix: operator precedence and layout break the build (CS0019) — withhold (the keep-`static`/parenthesize extension was split off — out of scope).
- #1281 RH3005/3101/3106/3201 fixes glue tokens (`returna;`) or turn `base.M()` into a virtual call — withhold. Its RH3103 part is superseded by #1417 (this run).
- #1282 RH3101/3104–3107 fixes delete comment text — withhold.
- #1417 (P38) RH3103 does not report `Nullable<T>` where `T?` is not valid C# (dotted `nameof`, member-access receiver, `typeof` of an unbound generic; alias only from C# 12) — SHARED-DIAG-011. Decided by me (J14): SHARED-DIAG-011 wins over RH3103-006; align RH3103-006 as a clarification.
- #1422 (P38) RH3104 and RH5417 do not report where the form they ask for is not valid (`new S();` → `default(S);` CS0201) — SHARED-DIAG-011.
Lane: bug fix. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md, squad/README.md, squad/lead.md, sdd/specs/shared/diagnostics.md (SHARED-DIAG-011), code-fixes.md (read only), empty-parentheses.md, the rule specs RH3001, RH3004, RH3005, RH3101, RH3103, RH3104, RH3105, RH3106, RH3107, RH3201, RH5417, each issue with comments.

Running at the same time (do not touch their files): R1 formatter phases; R2 doc phase/fixes; R3 raw strings/Indentation/RH5601/RH5104; R4 ordering/modifier/region fixes, shared/code-fixes.md; R5 casing. Also not sdd/README.md, sdd/templates/, squad/. requirement-ids.txt: sorted insert; keep both sides on conflict. Note: R8 (wave 2) touches the RH5417 fix after you; R23 (later) touches the RH3001 fix.

Done means: each defect reproduced first; every unsafe fix withheld with the diagnostic kept, safe inputs still fixed; RH3103/RH3104/RH5417 silent where the form is invalid; RH3103-006 aligned; CI green; PR with "Closes #N" for all six.

Must not: implement split-off parts; add a change doc; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```
