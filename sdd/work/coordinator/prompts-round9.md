# Squad prompts — round 9 (2026-10-22)

---

## #1331 — P25–P31 + P18/P22 clarifications

```
/squad #1331

This run: record P25–P31 and the P18/P22 clarifications in the shared specs as the issue body and its comments say. Its blockers #1260 (PR #1359), #1138 (PR #1357) and #1161 (PR #1360) are merged — read what they wrote first and build on it: P18 and P22 now live where #1359 put them (check whether the clarifications are already in; write only what is missing), P11 is in sdd/README.md (section added by #1360) — P31 is its mirror, and the closing sentence of the P11 section must be narrowed as the PR #1360 body and the comment on this issue say.
ID plan: new SHARED-DIAG-009 (P25) and SHARED-DIAG-010 (P31) only; everything else amends existing requirements (SHARED-DIAG-004 for P28, SHARED-FIX-007 narrowed by P26 for moved code, SHARED-ORDER-004 for P27, SHARED-DOC-004 for P29, SHARED-DOC-008 for P30). If an amendment does not fit, escalate before minting another ID. Also from the comments: SHARED-FIX-003's "reports the construct it names for every input" is reconciled with the not-reported cases of SHARED-DIAG-008 and SHARED-JOIN-001 (wording only).
This run records decided principles. The principle texts are decided (#912) — write them, do not reopen them. Where a principle states behavior the code does not yet meet, the requirement still states the principle, and its covering tests pin only the shapes that already conform; the non-conforming shapes are tracked by the follow-ups named in the issue (#1332–#1347, #1354 and others listed there) — no open question, no characterization test that contradicts the requirement. If a requirement has no conforming shape to test at all, escalate to me before writing it.
Lane: "Specs and tests for existing behavior" for the spec/test part; the sdd/README.md P11 sentence is docs (confirm the combined diff with scripts/lane-check.sh; do not touch AGENTS.md). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912 (at its sub-issue limit: name new question issues in your run report).

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions" and the P11 section), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time: nothing else touches the shared specs. Expect only text conflicts in sdd/specs/requirement-ids.txt; resolve them by keeping both sides.

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
- Round 8 (#1331 — THIS RUN writes them): P25 a verdict does not depend on incidental placement or order (base-list order, which partial part holds something). P26 code a fix moves keeps its own layout; only its indentation follows the new nesting. P27 member-ordering rules check each top-level region on its own. P28 a rule whose subject is what each partial part writes (RH7104-RH7106) judges each part. P29 a documentation comment written behind code documents the next declaration and is judged as if it started its line. P30 the documentation-mode gate applies only to rules that read documentation elements. P31 a layout rule whose fix delegates to the formatter does not report a shape the formatter deliberately keeps.
- Round 9 (decided, written by a later run — do not write them): P32 wrapping keeps the author's comment layout; P33 a multi-line comment belongs to the line it starts on; P34 one treatment per syntax family in the formatter; P35 a fix writes the line ending FMT-PIPE-007 detects (lone CR counts as LF); P36 the formatter does not apply a structural rewrite that would break the build, drop documentation or move a comment; P37 a required blank line goes after directives, never directly before one; P12 clarification: a line led by a misplaced comment is reported.
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior except where this run records a decided principle as described above. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: P25–P31 and the clarifications written as the issue and its comments state, IDs registered, every new requirement covered by tagged tests (conforming shapes), `Uses` lines of the affected rule specs updated, open questions filed per sdd/README.md and linked from the PR, CI green, PR "Closes #1331".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; write P32–P37; edit AGENTS.md; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```
