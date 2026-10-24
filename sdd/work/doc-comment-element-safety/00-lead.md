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

## Maintainer answers to the spec author's questions (escalation 1, 2026-10-24)

Recorded here rather than in `squad/decisions/`, because the maintainer put `squad/` off-limits for this run.

1. #1329 — **C**: a documentation comment that holds an element without an end tag is not laid out at all; incomplete documentation is left as written and corrected by hand. Both shapes of the issue stay unchanged, including a complete one-line `<summary>` in the same comment; the declaration stays code.
2. #1389 — **(ii)**: a documentation comment the compiler does not read as documentation (stranded behind code, FMT-DOC-003) is an ordinary comment and is left as written, byte for byte. The maintainer points to the existing rule that such comments are treated as ordinary comments (SHARED-COMMENT-001 / FMT-DOC-003); consequently FMT-DOC-005 does not apply to them either. The BOM part (FMT-CLEAN-009) is unaffected.
3. #1344, RH8304 — **A**: with code in front of `///`, the fix inserts only the line's indentation; RH8304-004's coverage widens to that input; no withhold, no rule-page note.
4. #1241 — **A**: the RH8107 and RH8202 instances (an end tag with another name, e.g. `<remarks><returns>Nothing</remarks>`) are in this run; both fixes are withheld for them, with tests.
5. RH8309, text before the first or after the last top-level element — **B**: the fix is withheld, as RH8309-005 does for text between elements.
6. RH8309, an element without an end tag of its own that the fix would not move — **B** (follows from 1C): the fix is withheld whenever any top-level element lacks an end tag of its own.

## Maintainer answers to the skeptic's questions (escalations 1, 3, 5; 2026-10-24)

Recorded here, not in `squad/decisions/` (off-limits for this run).

1. **A** — RH8301, RH8305 and RH8307 do not report inside a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written (SHARED-DIAG-010). The content rules RH8107, RH8202, RH8204, RH8309 keep reporting.
2. **A** — a comment behind code that FMT-DOC-001 relocates and that holds an element without an end tag is moved, and its text is left as written; the following declaration stays code.
3. **B** — the maintainer's reasoning: an end tag with another name is not the element's end, so the comment is not well formed and is not laid out. FMT-DOC-013 therefore covers "no end tag of its own" (none at all, or one with another name), consistent with P19 / SHARED-DOC-017. (The maintainer asked whether FMT-DOC-013 is new: yes, the spec author added it in this run from answer 1C.)
4. **A** — this run may make the one narrow change in the BlankLines phase (`CollapseLeadingBlankLines`) needed to keep a BOM on a first line that holds only the BOM and white space; the maintainer coordinates with the run that owns the phase. #1389 closes.
5, 6, 7 — not yet answered; asked back.
