# Squad prompts — round 10 (2026-10-23)

Running together: #1401+#1377 (docs, one run), #1402 (tooling). Then #998 last.

---

## #1401 + #1377 — docs follow-ups

```
/squad #1401 #1377

This run (several issues, one lane — see squad/README.md "Several issues in one run"): two text-only follow-ups.
- #1401: sdd/README.md "Tests and requirements" (and its "Enforced today" list) is stale after #939 (PR #1358 added formatter/ and playground/ to the enforced paths and `RequirementAttribute` to Formatter.Test and Playground.Test) — bring it in line with RequirementCoverageTests as it stands on main. The formatter specs' non-goals still call #951, #953, #956, #958, #959, #960, #976 open although they are decided — reword each to the decided state, as the issue says.
- #1377: the requirements that restate the line-ending rule cite SHARED-FIX-008 instead of restating it (list in the issue and its comments). The comment from round 10 adds RH5011-004 and RH8304-004 to the list (P35: a fix writes the line ending FMT-PIPE-007 detects; a lone `\r` counts as `\n`). Citation and wording only — no requirement changes meaning; where a requirement's text says something SHARED-FIX-008 does not, keep it and tell me in the run report.
Lane: docs for both (confirm with scripts/lane-check.sh; a docs-lane diff cannot add a requirement to an enforced spec — if one seems needed, escalate). Base the PR on main; no stacked PRs. Parent: #899.

Read first: AGENTS.md, sdd/README.md, squad/README.md (incl. "Several issues in one run"), squad/lead.md, sdd/specs/shared/code-fixes.md (SHARED-FIX-008), the newest entries in squad/decisions/, and both issues with all comments.

Running at the same time: #1402 (one line in AGENTS.md). Do not touch AGENTS.md. #998 runs after you and changes sdd/README.md "Tests and requirements" again for its opt-out switch — do not anticipate it.

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
- Round 8 (#1331, in the specs since PR #1404): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- Round 9 (#1380, in the specs since PR #1405): P32 wrapping keeps the author's comment layout; P33 a multi-line comment belongs to the line it starts on; P34 one treatment per syntax family in the formatter; P35 a fix writes the line ending FMT-PIPE-007 detects (lone CR counts as LF); P36 the formatter does not apply a structural rewrite that would break the build, drop documentation or move a comment; P37 a required blank line goes after directives, never directly before one; P12 clarification: a line led by a misplaced comment is reported.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.


Done means: both issues done as stated, SpecConsistencyTests, SpecStructureTests and RequirementCoverageTests green, CI green, PR "Closes #1401" and "Closes #1377".

Must not: change code, tests or behavior; add, retire, move or renumber a requirement ID; edit AGENTS.md; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #1402 — AGENTS.md pointer to P11

```
/squad #1402

This run: add one line to AGENTS.md, section "Issue intake", pointing to the P11 section of sdd/README.md (the formatter may do more than the analyzers; no widening a rule because it roughly fits; a gap inside the rule's own construct is the rule's gap). The P11 section's closing sentence was narrowed by PR #1404 — read the section as it stands on main. Keep the pointer to one sentence with a relative link to the section anchor. CLAUDE.md imports AGENTS.md — do not edit CLAUDE.md.
Lane: tooling (AGENTS.md). Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs. Parent: #899.

Read first: AGENTS.md, sdd/README.md (P11 section), squad/README.md, squad/lead.md, and this issue with its comments.

Running at the same time: #1401 + #1377 (sdd/README.md "Tests and requirements", formatter spec non-goals, SHARED-FIX-008 citations). Do not touch sdd/.

Done means: the one-line pointer in AGENTS.md, link resolves, CI green, PR "Closes #1402".

Must not: change any other instruction in AGENTS.md; change code, specs or tests; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

---

## #1415 — P38, P39, P29/P35 clarifications

```
/squad #1415

This run: record P38, P39 and the P29/P35 clarifications in the shared specs exactly where the issue body says (shared/diagnostics.md or code-fixes.md for P38 — pick one home and cite it from analyzer/RH3103.md; code-fixes.md next to SHARED-FIX-003 and element-documentation.md SHARED-DOC-004 for P39; SHARED-FIX-008 for P35; SHARED-DOC-004 for P29). Read the comment on the issue (follow-ups #1417, #1388, #1262).
ID plan: P38 and P39 may mint one new ID each in their home spec (SHARED-DIAG-011 or SHARED-FIX-009 — check requirement-ids.txt on main for the next free number); the clarifications amend SHARED-FIX-008 and SHARED-DOC-004 only.
This run records decided principles. The texts are decided (#912) — write them, do not reopen them. Despite the issue's "docs/spec" wording this is the "Specs and tests for existing behavior" lane: shared/ specs are enforced, so every new requirement needs covering tests (confirm with scripts/lane-check.sh). Where a principle states behavior the code does not yet meet (P38 for RH3103, P39 for RH7309, P35 for kept breaks), the requirement still states the principle and its covering tests pin only the shapes that already conform; the non-conforming shapes are tracked by #1417, #1262, #1388 — no open question. Existing tests that pin today's contradicting behavior stay as they are and are not tagged with the new requirement; if one is already tagged with a requirement whose amended wording it now contradicts, escalate to me. If a requirement has no conforming shape to test at all, escalate before writing it.
Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions" and the P11 section), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time: #1401 + #1377 (PR #1414: sdd/README.md "Tests and requirements", formatter spec non-goals, citations of SHARED-FIX-008 in other specs) and #1402 (AGENTS.md). If #1414 is not merged yet, keep your SHARED-FIX-008 edit to the lone-`\r` clause; expect text conflicts in code-fixes.md or requirement-ids.txt and resolve them by keeping both sides.

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
- Round 8 (#1331, in the specs since PR #1404): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- Round 9 (#1380, in the specs since PR #1405): P32 wrapping keeps the author's comment layout; P33 a multi-line comment belongs to the line it starts on; P34 one treatment per syntax family in the formatter; P35 a fix writes the line ending FMT-PIPE-007 detects (lone CR counts as LF); P36 the formatter does not apply a structural rewrite that would break the build, drop documentation or move a comment; P37 a required blank line goes after directives, never directly before one; P12 clarification: a line led by a misplaced comment is reported.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.
- Round 10 (#1415 — THIS RUN writes them): P38 a rule reports only what the language lets the user write instead (P23 stays where the form is valid but the fix unsafe); P39 a fix that cannot move a declaration together with its documentation is withheld; P35 clarified (every break a fix creates or keeps gets the detected ending); P29 clarified (doc mode None: a behind-code `///` is an ordinary trailing comment).

Decided but not implemented: the spec states current behavior except where this run records a decided principle as described above. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question.

Done means: P38, P39 and the two clarifications written as the issue states, IDs registered, every new requirement covered by tagged tests (conforming shapes), `Uses` lines of affected specs (RH3103, RH7309, RH7103 and others the spec author finds) updated, SpecConsistencyTests, SpecStructureTests and RequirementCoverageTests green, CI green, PR "Closes #1415".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; edit AGENTS.md; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```
