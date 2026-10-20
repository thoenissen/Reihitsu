# Squad prompts — round 7 (2026-10-20)

Running together: #985 (D4), #993 (O1), #995 (O3). Overlap check: `inventory/batches-round7.md`: no pair collides if each lead follows the constraints inside its prompt. Fallback if #993 cannot hold to them: run #995 first, then #993; #985 never needs to wait.
Later: #939 (tagging; after these three, it tags the specs they create), then #998 last (after everything).

Each prompt is complete and self-contained; paste one per squad chat.

---

## #985 — D4 documentation layout + comments

```
/squad #985

This batch: RH8301–RH8309, RH8401, RH8402, RH8501. Reuse sdd/specs/shared/element-documentation.md and its neighbour specs only under `Uses` (SHARED-DOC IDs, RH8107-006/007 as RH8202 does); do not edit element-documentation.md and do not mint SHARED-DOC-017 (reserved by #1260).
Read also: #1242, #1153, #1091, #1148, #1150, #1139, #1260, #1077.
- #1150: the RH83xx fixes that delegate to the formatter are offered as no-ops → instances get a one-line comment there. #1148: RH8303 mirrors the narrowed FMT-BLANK-011 (comment). #1153 / #1091: RH8305, RH8306, RH8309 (summary text, tag case) are instances. #1139 / #1077: RH8401 (a `///` that documents nothing is an ordinary comment). #1242: RH8304 and RH8308 use `DocumentationCommentCodeFixUtilities`, owned by RH8107.md — reference RH8107-006/007 under `Uses` and comment on #1242 that two more specs rely on it; do not move it.
- You may claim `XmlDocumentationElementOrderingUtilities` (only RH8309 uses it; if RH8204 also references it, reference from RH8204 only). Do not claim `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationCommentCodeFixUtilities`, `CopyrightHeaderTemplateResolver` (owned by RH0001.md; RH8402 only references RH0001 IDs). Do not mint SHARED-DIAG-004 and above.
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912.

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their rules, specs, or shared files): #985 D4 documentation layout RH8301–RH8501, #993 O1 RH7001–RH7110, #995 O3 RH7401–RH7501. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides.

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153).
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (mention it in the run report as a new-rule candidate, no issue). A gap inside the construct the rule names is the rule's own gap: file it marked "Decided by principle P11 (#912)".
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with the previous same-group element whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the reported construct means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: a spec for every rule of this batch (shared specs only where more than one spec relies on the behavior), requirement IDs registered, every requirement covered by tagged tests, rule pages corrected only where they are wrong, open questions filed per sdd/README.md ("SDD open questions: <spec path>", one issue per spec, sub-issue of #912 — if #912 is at its sub-issue limit, say so in the issue and tell me) and linked from the PR, CI green, PR "Closes #<batch>".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; claim ownership of a shared utility another batch or spec owns without escalating; mint IDs reserved by #1260 (SHARED-DIAG-004…008, SHARED-DOC-017, SHARED-USING-007, edits to SHARED-TYPEREGION-002/006), #1138, #1091 or #1136; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #993 — O1 file layout, member/modifier ordering

```
/squad #993

This batch: RH7001–RH7004, RH7101, RH7102, RH7104–RH7110. Reference SHARED-ORDER-001…007 and the RH2005 spec under `Uses`; mint no SHARED-ORDER ID (#970 and #1134 will add them). You may claim only the modifier-ordering helpers `ModifierOrderingUtilities` and `ModifierOrderingCodeFixProviderBase`. Do not claim the SHARED-ORDER owner types, `DeclarationModifierUtilities` (RH2005.md), `RegionDirectiveUtilities`, `OrderingDeclarationUtilities`, and do not edit `OrderingDeclarationUtilitiesTests.cs` / `RegionDirectiveUtilitiesTests.cs`.
Read also: #968, #969, #970, #1134, #1136, #1137, #1143, #1150, #1138.
- Instances (one-line comments): #968 (RH7103 groups by written, not effective, accessibility — RH7110 and RH7104 may share the cause), #969 (ordering fixes format the whole type — RH7102, RH7109; with P14d), #970 (balanced `#pragma` blocks a move — RH7102, RH7107, RH7108, RH7109), #1134 (RH7109 reorders fields of sequential-layout structs), #1143 (RH7004, RH7102, RH7109 test classes use two Fix All passes: do not encode that as a requirement), #1150 (RH7004 and RH710x delegate to the formatter), #1136 / #1137 (RH7102/RH7109 follow the RH7103 model).
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912.

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their rules, specs, or shared files): #985 D4 documentation layout RH8301–RH8501, #993 O1 RH7001–RH7110, #995 O3 RH7401–RH7501. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides.

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153).
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (mention it in the run report as a new-rule candidate, no issue). A gap inside the construct the rule names is the rule's own gap: file it marked "Decided by principle P11 (#912)".
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with the previous same-group element whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the reported construct means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: a spec for every rule of this batch (shared specs only where more than one spec relies on the behavior), requirement IDs registered, every requirement covered by tagged tests, rule pages corrected only where they are wrong, open questions filed per sdd/README.md ("SDD open questions: <spec path>", one issue per spec, sub-issue of #912 — if #912 is at its sub-issue limit, say so in the issue and tell me) and linked from the PR, CI green, PR "Closes #<batch>".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; claim ownership of a shared utility another batch or spec owns without escalating; mint IDs reserved by #1260 (SHARED-DIAG-004…008, SHARED-DOC-017, SHARED-USING-007, edits to SHARED-TYPEREGION-002/006), #1138, #1091 or #1136; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #995 — O3 member grouping by regions, RH7501

```
/squad #995

This batch: RH7401–RH7412, RH7501. Create your own shared spec with a unique prefix: sdd/specs/shared/member-region-grouping.md with `SHARED-REGGROUP` (`SHARED-REGION` is taken) — only if more than one spec relies on the behavior; reference FMT-REGION and the SHARED-TYPEREGION IDs only under `Uses`. Do not claim `RegionDirectiveUtilities`, `OrderingDeclarationUtilities`, `RegionCategoryUtilities` (RH7309.md), and do not edit `OrderingDeclarationUtilitiesTests.cs` / `RegionDirectiveUtilitiesTests.cs`. `OverrideMemberUtilities` and `InterfaceImplementationUtilities` are unowned and used only by RH7401–RH7412: you may claim them.
Read also: #1264, #1240, #1260, #1263, #1262, sdd/specs/shared/type-region specs.
- #1264 (P17: the innermost region/declaration owns a member) and #1240 (inactive `#if` non-goal covers region directives) are plain instances for the RH74xx bases: one-line comments. RH7501 relates to FMT-SWITCH-002 and the formatter bugs #948–#950: reference them, do not refile.
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912.

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their rules, specs, or shared files): #985 D4 documentation layout RH8301–RH8501, #993 O1 RH7001–RH7110, #995 O3 RH7401–RH7501. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides.

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153).
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (mention it in the run report as a new-rule candidate, no issue). A gap inside the construct the rule names is the rule's own gap: file it marked "Decided by principle P11 (#912)".
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with the previous same-group element whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the reported construct means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: a spec for every rule of this batch (shared specs only where more than one spec relies on the behavior), requirement IDs registered, every requirement covered by tagged tests, rule pages corrected only where they are wrong, open questions filed per sdd/README.md ("SDD open questions: <spec path>", one issue per spec, sub-issue of #912 — if #912 is at its sub-issue limit, say so in the issue and tell me) and linked from the PR, CI green, PR "Closes #<batch>".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; claim ownership of a shared utility another batch or spec owns without escalating; mint IDs reserved by #1260 (SHARED-DIAG-004…008, SHARED-DOC-017, SHARED-USING-007, edits to SHARED-TYPEREGION-002/006), #1138, #1091 or #1136; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

