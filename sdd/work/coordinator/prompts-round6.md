# Squad prompts — round 6 (2026-10-19)

Running together: #984 (D3), #991 (LA), #992 (LC), #994 (O2), #996 (B1), #997 (B2).
Later: #985 after #984; #995 after #994; #993 after #994 and #996; then #939, #998.

The shared block is the same in every prompt; only the "This batch" part differs.

---

## Shared block (part of every prompt)

```
Lane: "Specs and tests for existing behavior" (confirm with scripts/lane-check.sh). Base the PR on main; no stacked PRs. Parent: #899, question tracker #912.

Read first: AGENTS.md ("Code-fix coverage", "Issue intake"), sdd/README.md (especially "Open questions"), squad/README.md, squad/lead.md, sdd/specs/shared/code-fixes.md, sdd/specs/shared/diagnostics.md, the newest entries in squad/decisions/, and this issue with all its comments.

Running at the same time (other squads — do not touch their rules, specs, or shared files): #984 D3 docs RH8101–RH8204, #991 LA RH5301–RH5408, #992 LC RH5409–RH5417, #994 O2 RH7201–RH7311, #996 B1 RH0002–RH2109, #997 B2 RH3001–RH3208. Expect only text conflicts in sdd/specs/requirement-ids.txt and sdd/specs/README.md; resolve them by keeping both sides.

Decided principles (#912). A case one of them decides is not an open question:
- P1 a fix that could break the code is withheld (SHARED-FIX-003, incl. the recorded readings in squad/decisions/).
- P2 names dictated from outside are not reported. P3 names are compared by value (SHARED-DIAG-002).
- P5 only the text of a literal or comment is content (SHARED-DIAG-003). P6 a `/** */` that documents nothing is a block comment.
- P7 SHARED-FIX-001 per rule; SHARED-FIX-004 no fix undoes another; SHARED-FIX-005 no no-op fix.
- P8 documentation is what the compiler produces (#1091); P8b a parameter is documented exactly when the compiler binds a `<param>`/`<typeparam>` to it; P8c only content that renders text counts as text (#1153).
- P9 a fix changes only the code its diagnostic is about; P10 the code a fix changes is laid out as the formatter lays it out, in one application (both #1138).
- P11 the formatter may do more than the analyzers. A construct the rule does not name is not a question (mention it in the run report as a new-rule candidate, no issue). A gap inside the construct the rule names is the rule's own gap: file it marked "Decided by principle P11 (#912)".
- P12 a comment at the start of a line is the line's first element; P13 every ordinary comment kind is treated alike (both #1139).
- P4: any other analyzer/formatter disagreement is decided case by case → open question.

Decided but not implemented: the spec states current behavior. Behavior that is decided but not yet implemented stays out of the spec (no requirement, no non-goal, no note); the test that pins current behavior is a characterization test. A further instance of an existing follow-up issue gets a one-line comment by the lead on that issue — not a new question. A new instance of a principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".

Done means: a spec for every rule of this batch (shared specs only where more than one spec relies on the behavior), requirement IDs registered, every requirement covered by tagged tests, rule pages corrected only where they are wrong, open questions filed per sdd/README.md ("SDD open questions: <spec path>", one issue per spec, sub-issue of #912) and linked from the PR, CI green, PR "Closes #<batch>".

Must not: change production code or behavior; fix a bug, even an obvious one; write an open question into a spec; retire, move, or renumber an existing requirement ID without escalating to me first; claim ownership of a shared utility another batch or spec owns without escalating; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #984 — D3 parameter / return / typeparam / inheritdoc docs

```
/squad #984

This batch: RH8101–RH8106, RH8108–RH8111, RH8201–RH8204. Reuse sdd/specs/shared/element-documentation.md (SHARED-DOC); new SHARED-DOC IDs continue after the highest existing one — #985 extends the same spec later, not in parallel.
Read also: sdd/specs/shared/element-documentation.md, sdd/specs/analyzer/RH8107.md, #1091, #1153, #1132, squad/decisions/2026-10-18-shared-doc-coverage-and-extension-block.md.
- #1153 (P8b, P8c) already decides `name` matching (case, namespace, white space, escapes, unnamed parameters) and what counts as text in `<param>`, `<returns>`, `<typeparam>`: these are instances of #1153 (one-line comment), not new questions. P8 shapes (`<inheritdoc>` case/namespace, dropped comments, merged partial parts) are instances of #1091.
- #1132: check whether RH8101/RH8108 and RH8102/RH8109 match `name` the same way; where they read it through SHARED-DOC-012 reference it under `Uses`; differences are open questions.
- Moving a requirement out of analyzer/RH8107.md (e.g. `DocumentationCommentCodeFixUtilities`) retires its ID → escalate to me first. `XmlDocumentationElementOrderingUtilities` (RH8204, also RH8309 in #985): do not claim it for a shared spec in this run; reference it from RH8204 only.
<shared block>
```

---

## #991 — LA initializers / expressions / braces

```
/squad #991

This batch: RH5301–RH5308, RH5401–RH5408. A shared spec for statement braces (RH5405–RH5407) only if more than one spec relies on it.
Read also: #1138, #1140, #1144, #1150, sdd/specs/shared/line-join-rules.md, sdd/specs/shared/token-gap-fixes.md.
- #1150 names `StatementBracesCodeFixProviderBase` and the RH53xx/RH54xx providers (fix withheld when the formatter returns the document unchanged, P7): instances get a comment there.
- Fix scope (P9) and fix layout (P10) of these fixes: instances of #1140 / #1144 or "Decided by principle P9/P10 (#912)".
- `SyntaxNodeUtilities` is also used by #997: neither batch claims it.
<shared block>
```

---

## #992 — LC trailing commas, empty types

```
/squad #992

This batch: RH5409–RH5417. Shared specs for trailing comma / empty type declaration only if more than one spec relies on them.
Read also: sdd/specs/formatter/structural-transforms.md.
- `TrailingCommaRemovalUtilities` and `EmptyTypeDeclarationSemicolonTransform` belong to formatter/structural-transforms.md: reference the FMT-STRUCT IDs under `Uses`, do not claim the types.
- No follow-up names these rules yet; #1150 (no-op fix when the formatter refuses) applies to any fix that delegates to the formatter.
<shared block>
```

---

## #994 — O2 usings, regions

```
/squad #994

This batch: RH7201–RH7207, RH7301–RH7311, RH7305A. Reuse sdd/specs/shared/using-directive-ordering.md (SHARED-USING); region description as a shared spec only if more than one spec relies on it. #995 (member grouping by regions) runs after this batch, not in parallel.
Read also: #1105, #1151, #1135, #1143, #965, #966, #964, #1083, sdd/specs/shared/ordering-move-safety.md, sdd/specs/formatter/region-formatting.md.
- #1105: RH7201/RH7203/RH7207 compare names by spelling (P3) — instances go there.
- #1151 (RH7302 `#endregion` comment rename) and #1135 (RH7309 changes static initializer order, SHARED-ORDER-007) are filed; #1143 covers RH7310's two Fix All passes — current behavior, not a requirement.
- Do not file #964, #965, #966, #1083 again. `RegionDirectiveUtilities` (formatter/region-formatting.md) and `OrderingDeclarationUtilities` (shared/ordering-move-safety.md) are owned already: reference, do not claim; add no requirement to shared/ordering-move-safety.md without escalating.
<shared block>
```

---

## #996 — B1 RH0002–RH2109

```
/squad #996

This batch: RH0002, RH1001, RH1002, RH2001–RH2004, RH2006, RH2007, RH2101–RH2109. Shared struct-equality spec (RH1001/RH1002) only if more than one spec relies on it.
Read also: the #978 note on this issue, sdd/specs/shared/casing.md, sdd/specs/analyzer/RH2005.md, #971, #967, #1137.
- RH2001 lists the shared casing IDs under `Uses` (#978 note); #971 (acronym run) also affects the field name RH2001's fix creates — instance, comment there.
- #967 and `DeclarationModifierUtilities` (owned by analyzer/RH2005.md): reference, do not claim. `ModifierOrderingUtilities` is also used by #993 (later): do not claim it in this run.
- #1137: SHARED-FIX-003 now decides the old RH2005/RH7103 non-goals — do not write such cases as non-goals.
<shared block>
```

---

## #997 — B2 clarity

```
/squad #997

This batch: RH3001–RH3007, RH3101–RH3107, RH3201, RH3203–RH3208. Shared expression-body → block spec (may take `ExpressionBodyRewriteUtilities` out of RH3202) and empty-parentheses spec (RH3106/RH3107) only if more than one spec relies on them.
Read also: sdd/specs/analyzer/RH3202.md, #1067, #963, #1143.
- Moving a requirement out of analyzer/RH3202.md retires its ID → escalate to me first.
- #1067 is open: who owns `BlankLineCodeFixUtilities` (RH3208 and RH5030) — escalate to me before claiming it.
- #1143 covers the two Fix All passes of RH3001, RH3002, RH3004, RH3102 — current behavior, not a requirement. Do not file #963 again. RH3201 only uses SHARED-DOC. `SyntaxNodeUtilities` is also used by #991: neither claims it.
<shared block>
```
