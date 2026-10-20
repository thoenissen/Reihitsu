# Lead

Issue: thoenissen/Reihitsu#985 (parent #899, question tracker #912). Draft claim PR: thoenissen/Reihitsu#1301.
Slug: doc-layout-specs
Branch: claude/fervent-lamport-37siyv (designated by the session; based on main)
Lane: Specs and tests for existing behavior (spec author, skeptic, test implementer, cleaner, spec checker, validator; implementer and auditor skipped per lane table).

## Scope (maintainer's instructions, summarized)
- Rules: RH8301-RH8309, RH8401, RH8402, RH8501. One spec each under sdd/specs/analyzer/, from sdd/templates/analyzer-spec.md.
- Reuse shared/element-documentation.md and neighbour specs only under `Uses` (SHARED-DOC ids, RH8107-006/007). Do not edit element-documentation.md; do not mint SHARED-DOC-017, SHARED-DIAG-004 and above, or other reserved ids (SHARED-USING-007, SHARED-TYPEREGION-002/006 edits; ids reserved by #1138, #1091, #1136).
- May claim XmlDocumentationElementOrderingUtilities (RH8309; if RH8204 references it, RH8204 only references it). Must not claim DocumentationAnalysisUtilities, DirectDocumentationSyntaxChecker, XmlDocumentationExpander, DocumentationCommentCodeFixUtilities (owned by RH8107.md), CopyrightHeaderTemplateResolver (owned by RH0001.md; RH8402 only references RH0001 ids).
- Decided principles P1-P24 of #912 apply; a case they decide is filed marked "Decided by principle Pn (#912)", not a question. Decided-but-unimplemented behavior stays out of specs; tests pinning current behavior are characterization tests.
- Open questions: never in a spec; filed one issue per spec "SDD open questions: <spec path>", sub-issue of #912.
- Instances to note: #1150 (RH83xx formatter-delegating fixes offered as no-ops), #1148 (RH8303 mirrors narrowed FMT-BLANK-011), #1153/#1091 (RH8305, RH8306, RH8309), #1139/#1077 (RH8401), #1242 (RH8304, RH8308 use DocumentationCommentCodeFixUtilities; lead comments on #1242).
- No production code changes, no bug fixes, no sdd/work in the PR. No issues besides "SDD open questions" without maintainer approval of exact text.
- Concurrent squads: #993 (RH7001-RH7110), #995 (RH7401-RH7501). Expect text conflicts only in requirement-ids.txt and specs/README.md; keep both sides.
