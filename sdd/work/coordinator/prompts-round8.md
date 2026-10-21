# Squad prompts — round 8 (2026-10-21)

Running together: #939, #1138, #1260, #1139, #1161 (5 squads). Overlap check: `inventory/batches-round8.md`; no pair collides if each lead keeps to the ID plan in its prompt.
Later: #1331 (P25-P31; after #1260 and #1138, preferably after #1161), then #998 last.

Each prompt is complete and self-contained; paste one per squad chat.

---

## #939 — tag formatter and Playground tests

```
/squad #939

This batch: tag the formatter and Playground tests with requirement IDs. State on main (checked 2026-10-21): every shared/ spec and cli/ are already fully covered; all 13 sdd/specs/formatter/ specs (202 requirements) and sdd/specs/playground/playground.md (7) are uncovered. Formatter.Test and Playground.Test have no `RequirementAttribute` yet — create it in both, modeled on Analyzer.Test/Cli.Test/Core.Test. Add `formatter/` and `playground/` to `RequirementCoverageTests.EnforcedSpecPaths` once covered. Tag existing tests; write a focused test only where no existing test covers a requirement. "Incidental tests do not tag" (#998 discussion) still holds: tag the tests that prove the requirement, not every test that passes through it.
Read also: #998 (runs after you; do not implement its opt-out switch or the every-test-tagged rule), #1138 (amends the `Why:` of FMT-PIPE-010 in parallel — do not edit formatter spec text yourself), #1148, #1149 (formatter behavior changes decided but not implemented: their current behavior gets characterization tags, not new requirements).
Lane: "Specs and tests for existing behavior", tests only (confirm with scripts/lane-check.sh). No spec text changes except: a requirement that is wrong about current behavior is an open question ("SDD open questions: <spec path>"), not an edit. Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report, I track them on #912).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their files): #939 tags Formatter.Test/Playground.Test; #1138 shared/code-fixes.md (+ FMT-PIPE-010 `Why:`); #1139 shared/comment-placement.md + `Uses` lines of RH51xx/RH52xx specs; #1161 sdd/README.md; #1260 shared/diagnostics.md, type-regions.md, using-directive-ordering.md, element-documentation.md. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides. ID plan (do not mint anyone else's IDs): #1260 SHARED-DIAG-004…008, SHARED-DOC-017, P18 as an amendment of the existing SHARED-USING-007 or as SHARED-USING-011; #1138 SHARED-FIX-006…008; #1139 SHARED-COMMENT-011 only; #939 and #1161 no new IDs; SHARED-DIAG-009/010 reserved for #1331 (runs later).

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153), also for word checks like RH8306.
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers; no widening a rule because it roughly fits (specialized rule instead); a gap inside the construct the rule names is the rule's own gap (#1161).
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with every earlier element of the same group, whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the span the rule judges means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- Round 8 (#1331, runs later): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: every requirement of the 13 formatter specs and playground.md covered by a tagged test, `EnforcedSpecPaths` extended, RequirementCoverageTests green, open questions filed per sdd/README.md and linked from the PR, CI green, PR "Closes #939".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; mint IDs reserved for another run (see ID plan); put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text. Do not edit any spec text under sdd/specs/.
```

---

## #1138 — P9/P10, P14a-d, P23, P24 in shared/code-fixes.md

```
/squad #1138

This run: record P9, P10, P14a-d, P23, P24 (and the SHARED-FIX-002 clarification) in sdd/specs/shared/code-fixes.md as the issue body says — new SHARED-FIX-006, -007, -008; amendments of SHARED-FIX-002/003 (incl. the replaced RH3202 layout example); the `Why:` of FMT-PIPE-010 in sdd/specs/formatter/pipeline.md may reference SHARED-FIX-006. Nothing else.
This run records decided principles in the shared specs. The principle texts in the issue body are decided (#912) — write them, do not reopen them. Where a principle states behavior the code does not yet meet, the requirement still states the principle, and its covering tests pin only the shapes that already conform; the non-conforming shapes are tracked by the follow-up bugs named in the issue (no open question, no characterization test that contradicts the requirement). If a requirement has no conforming shape to test at all, escalate to me before writing it.
Follow-ups that depend on you (do not implement them, only make sure the requirement they cite exists): #1140, #1141, #1142, #1143, #1144, #1263, #1269, #1283, #1284, #1335, #1342. #1331 (P26 narrows SHARED-FIX-007 for moved code) runs after you — do not write P26.
Lane: "Specs and tests for existing behavior" (code-fixes.md is enforced, so the new requirements need covering tests; despite the issue's wording this is not the docs lane — confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their files): #939 tags Formatter.Test/Playground.Test; #1138 shared/code-fixes.md (+ FMT-PIPE-010 `Why:`); #1139 shared/comment-placement.md + `Uses` lines of RH51xx/RH52xx specs; #1161 sdd/README.md; #1260 shared/diagnostics.md, type-regions.md, using-directive-ordering.md, element-documentation.md. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides. ID plan (do not mint anyone else's IDs): #1260 SHARED-DIAG-004…008, SHARED-DOC-017, P18 as an amendment of the existing SHARED-USING-007 or as SHARED-USING-011; #1138 SHARED-FIX-006…008; #1139 SHARED-COMMENT-011 only; #939 and #1161 no new IDs; SHARED-DIAG-009/010 reserved for #1331 (runs later).

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153), also for word checks like RH8306.
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers; no widening a rule because it roughly fits (specialized rule instead); a gap inside the construct the rule names is the rule's own gap (#1161).
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with every earlier element of the same group, whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the span the rule judges means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- Round 8 (#1331, runs later): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: the requirements written as the issue states, IDs registered, every new requirement covered by tagged tests (conforming shapes), open questions filed per sdd/README.md and linked from the PR, CI green, PR "Closes #1138".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; mint IDs reserved for another run (see ID plan); put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #1260 — P15-P22 + alias/Razor extensions

```
/squad #1260

This run: record P15-P22 plus the alias (P3) and Razor/HTML (P5) extensions in the shared specs as the issue body says: sdd/specs/shared/diagnostics.md (new SHARED-DIAG-004…008; SHARED-DIAG-002/003 amended), type-regions.md (SHARED-TYPEREGION-002/006, P17 incl. enums), using-directive-ordering.md, element-documentation.md (new SHARED-DOC-017). Also include the comments on the issue: SHARED-DIAG-006 (P20) states one diagnostic per declarator, at its identifier (round 8); P18 is worded "compared with every earlier element of the same group, whatever lies between" (round 8 clarification, #1328); P22's "construct" is the span the rule judges (modifier list, declarator list, accessor list, namespace header), not the whole declaration with its body (round 8 clarification, #1321).
ID collision: the body names a new "SHARED-USING-007" for P18, but SHARED-USING-007 already exists ("Alphabetical order within a group, stable on ties"). Amend SHARED-USING-007 if P18 is its natural home; otherwise mint SHARED-USING-011. Never reuse 007 for a different meaning. Item 18 (alias to a non-name forms its own group at the end) amends SHARED-USING-004.
This run records decided principles in the shared specs. The principle texts in the issue body are decided (#912) — write them, do not reopen them. Where a principle states behavior the code does not yet meet, the requirement still states the principle, and its covering tests pin only the shapes that already conform; the non-conforming shapes are tracked by the follow-up bugs named in the issue (no open question, no characterization test that contradicts the requirement). If a requirement has no conforming shape to test at all, escalate to me before writing it.
#1331 (P25, P28 amending DIAG-004, P22/P18 clarifications) runs after you — write the P18/P22 clarifications above, nothing of P25-P31.
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their files): #939 tags Formatter.Test/Playground.Test; #1138 shared/code-fixes.md (+ FMT-PIPE-010 `Why:`); #1139 shared/comment-placement.md + `Uses` lines of RH51xx/RH52xx specs; #1161 sdd/README.md; #1260 shared/diagnostics.md, type-regions.md, using-directive-ordering.md, element-documentation.md. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides. ID plan (do not mint anyone else's IDs): #1260 SHARED-DIAG-004…008, SHARED-DOC-017, P18 as an amendment of the existing SHARED-USING-007 or as SHARED-USING-011; #1138 SHARED-FIX-006…008; #1139 SHARED-COMMENT-011 only; #939 and #1161 no new IDs; SHARED-DIAG-009/010 reserved for #1331 (runs later).

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153), also for word checks like RH8306.
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers; no widening a rule because it roughly fits (specialized rule instead); a gap inside the construct the rule names is the rule's own gap (#1161).
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with every earlier element of the same group, whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the span the rule judges means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- Round 8 (#1331, runs later): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: the requirements written as the issue and its comments state, IDs registered, every new requirement covered by tagged tests (conforming shapes), open questions filed per sdd/README.md and linked from the PR, CI green, PR "Closes #1260".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; mint IDs reserved for another run (see ID plan); put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #1139 — P12/P13

```
/squad #1139

This run: record P12 (a comment at the start of a line is the line's first element) and P13 (every ordinary comment kind is treated alike, in every documentation mode; a `///` or `/** */` that documents nothing is an ordinary comment) as the issue body says. Home of P12: sdd/specs/shared/comment-placement.md as SHARED-COMMENT-011 (NOT diagnostics.md — #1260 and #1331 mint SHARED-DIAG IDs there). P13 amends SHARED-COMMENT-001 and the P6 wording. Add the new ID under `Uses` in the rule specs that judge line starts (RH5110, RH5203, RH5204, RH5206 and the other RH51xx/RH52xx layout specs the issue names).
This run records decided principles in the shared specs. The principle texts in the issue body are decided (#912) — write them, do not reopen them. Where a principle states behavior the code does not yet meet, the requirement still states the principle, and its covering tests pin only the shapes that already conform; the non-conforming shapes are tracked by the follow-up bugs named in the issue (no open question, no characterization test that contradicts the requirement). If a requirement has no conforming shape to test at all, escalate to me before writing it.
Follow-ups that depend on you: #1147, #1341 (G1: documentation layout rules treat a `///` that documents nothing as an ordinary comment), #1346 (P30). Do not write P29/P30 (#1331).
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their files): #939 tags Formatter.Test/Playground.Test; #1138 shared/code-fixes.md (+ FMT-PIPE-010 `Why:`); #1139 shared/comment-placement.md + `Uses` lines of RH51xx/RH52xx specs; #1161 sdd/README.md; #1260 shared/diagnostics.md, type-regions.md, using-directive-ordering.md, element-documentation.md. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides. ID plan (do not mint anyone else's IDs): #1260 SHARED-DIAG-004…008, SHARED-DOC-017, P18 as an amendment of the existing SHARED-USING-007 or as SHARED-USING-011; #1138 SHARED-FIX-006…008; #1139 SHARED-COMMENT-011 only; #939 and #1161 no new IDs; SHARED-DIAG-009/010 reserved for #1331 (runs later).

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value, aliases resolved semantically (SHARED-DIAG-002; extension in #1260).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153), also for word checks like RH8306.
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138). P14a-d (#1138): a fix inserts no blank line before the changed statement; a line the fix edits is laid out completely; the blank line left by a removed directive/region is cleaned by the fix; only the replaced node is formatted, not the whole member.
- P11 the formatter may do more than the analyzers; no widening a rule because it roughly fits (specialized rule instead); a gap inside the construct the rule names is the rule's own gap (#1161).
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P15 a partial declaration is one declaration, reported once, decided on the symbol. P16 a value judgement looks through parentheses, `!` and `throw` arms (an extension, behavior change). P17 regions belong to the innermost type-like declaration (enums included). P18 ordering rules compare with every earlier element of the same group, whatever lies between. P19 an end tag with another name is no end tag, fix withheld. P20 a rule checks every element of its kind. P21 a diagnostic always has a location. P22 a conditional directive or excluded code inside the span the rule judges means not reported. P23 where a fix would break code the diagnostic stays and no fix is offered. P24 a fix keeps the document's line ending (P15-P22 in #1260, P23/P24 in #1138).
- Round 8 (#1331, runs later): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: the requirements written as the issue states, ID registered, `Uses` lines updated, every new requirement covered by tagged tests (conforming shapes), open questions filed per sdd/README.md and linked from the PR, CI green, PR "Closes #1139".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; mint IDs reserved for another run (see ID plan); put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #1161 — P11 in sdd/README.md

```
/squad #1161

This run: record P11 in sdd/README.md (section "Open questions", or a short section next to it): "The formatter may do more than the sum of the analyzers. A behavior is never forced onto an analyzer rule because it roughly fits — that needs a specialized rule. A gap inside the construct a rule names is that rule's own gap (a bug or behavior change against the rule), not an open question about the formatter." Include one short example of each half from the issue body.
Write it in sdd/README.md only. Do NOT edit AGENTS.md (that turns the run into the tooling lane); if you think AGENTS.md needs a pointer, say so in the run report and I decide.
#1331 (runs later) will place P31 — "a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps" — as the mirror of P11; it only needs to find P11 by a stable heading or anchor, so give the text one.
Lane: docs (confirm with scripts/lane-check.sh). No tests, no spec requirement, no new IDs. Base the PR on main; no stacked PRs. Parent: #899.

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their files): #939 tags Formatter.Test/Playground.Test; #1138 shared/code-fixes.md (+ FMT-PIPE-010 `Why:`); #1139 shared/comment-placement.md + `Uses` lines of RH51xx/RH52xx specs; #1161 sdd/README.md; #1260 shared/diagnostics.md, type-regions.md, using-directive-ordering.md, element-documentation.md. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides. ID plan (do not mint anyone else's IDs): #1260 SHARED-DIAG-004…008, SHARED-DOC-017, P18 as an amendment of the existing SHARED-USING-007 or as SHARED-USING-011; #1138 SHARED-FIX-006…008; #1139 SHARED-COMMENT-011 only; #939 and #1161 no new IDs; SHARED-DIAG-009/010 reserved for #1331 (runs later).

Done means: the wording in sdd/README.md, CI green, PR "Closes #1161".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; mint IDs reserved for another run (see ID plan); put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```
