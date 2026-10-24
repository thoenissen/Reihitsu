Role: lead
Commit: f3d514d0b1cbd8b3e540f2b2c7243dcd21760929
Verdict: lane claimed — bug fix

## Run

- Issues, in prompt order: thoenissen/Reihitsu#1329, thoenissen/Reihitsu#1389, thoenissen/Reihitsu#1241, thoenissen/Reihitsu#1344, thoenissen/Reihitsu#1242.
- Slug: `doc-comment-element-safety`.
- Branch: `claude/dreamy-albattani-e7phpl` (the session environment permits pushing only to this branch, so the playbook's `claude/batch-1329-<slug>` name is not used). Base: `main`. Draft PR: thoenissen/Reihitsu#1456.
- Lane: **bug fix**, set by the maintainer and shared by all five issues.

## Lane per issue

- #1329 — formatter: observed vs. expected (a declaration after a doc element without an end tag becomes comment text); violates the formatter's non-destructive requirements. Bug fix.
- #1389 — formatter: observed vs. expected against FMT-CLEAN-009 (BOM before a line break dropped) and FMT-PIPE-002 (doc text behind code deleted, single- and multi-line variants). Bug fix.
- #1241 — analyzer code fixes RH8204, RH8305, RH8307, RH8309 rewrite an element without its own end tag (none at all, or one with another name) and pull the next declaration into the comment; decided behavior is SHARED-DOC-017 / SHARED-FIX-003: the diagnostic stays, the fix is withheld. Bug fix.
- #1344 — RH8304 / RH8309 fixes produce CS0102 or unparsable output (continuation prefix copies code before `///`) or relocate free text / XML comments between top-level elements; decided behavior: withhold (SHARED-FIX-003, AGENTS.md "Code-fix coverage" — relocating a comment is unsafe). Bug fix.
- #1242 — spec-owner move, decided by the maintainer; text in `sdd/` only, carried in the bug-fix run as its spec-author clarification.

## Roles

All roles of the bug-fix lane run: spec author (needed: the #1242 owner move, and coverage sentences / rule-page gap notes the withholds change), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator.

## Maintainer clarifications (summarized)

- One run, one PR, one lane (bug fix), no change doc. Base on `main`, no stacked PRs.
- #1242: `DocumentationCommentCodeFixUtilities` moves as code owner from `sdd/specs/analyzer/RH8107.md` to `sdd/specs/shared/element-documentation.md` as a shared doc-fix utility; update the owner index in `sdd/specs/README.md` and the `Uses` lines of the rules that use it (RH8107, RH8202, RH8204, RH8304, RH8308, RH8309) as needed.
- #1344: relocating comment text counts as unsafe under AGENTS.md "Code-fix coverage"; the SHARED-FIX-003 wording for relocation is being added in a parallel run (#1136). Do not edit `sdd/specs/shared/code-fixes.md` here.
- Done: each defect reproduced by a test first; the formatter preserves the declaration and the doc text; the four fixes withhold for the unsafe inputs and still fix the safe ones; owner moved; CI green; PR closes all five issues.
- Must not: widen any rule's reports; implement the formatter doc-relocation extensions (#962 and similar); edit `shared/code-fixes.md`; add a change doc; leave `sdd/work/` in the PR; create issues without the maintainer approving the exact text.
- Files owned by parallel runs, not to be touched: SwitchCaseBraces, BlankLines, LineBreaks, StructuralTransforms phases and RH5201; formatter specs pipeline / switch / blank-lines / line-breaks / structural-transforms (pipeline.md is read-only here); RawStringAlignment, Indentation, RH5601/RH5104, Core `FormattingTextAnalysisUtilities`; ordering / modifier / region fix bases, RH7xxx, `shared/code-fixes.md`; casing; RH3xxx; `sdd/README.md`, `sdd/templates/`, `squad/`. `sdd/specs/requirement-ids.txt`: sorted inserts only; on conflict keep both sides.

## Read first (given by the maintainer)

AGENTS.md, sdd/README.md, squad/README.md, sdd/specs/formatter/documentation-comments.md, cleanup.md, pipeline.md, sdd/specs/shared/element-documentation.md, sdd/specs/shared/code-fixes.md (SHARED-FIX-003, read only), rule specs RH8107, RH8202, RH8204, RH8304, RH8305, RH8307, RH8309, and every issue with its comments.
