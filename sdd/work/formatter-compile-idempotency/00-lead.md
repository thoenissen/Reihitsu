Role: Lead
Commit: (claim) on origin/main f3d514d
Verdict: lane bug-fix claimed for all five issues

# Run

- Issues, in prompt order: thoenissen/Reihitsu#948, #949, #954, #1390, #888 (milestone 1.2.0).
- Slug: `formatter-compile-idempotency`.
- Branch: `claude/charming-dijkstra-xnue25`. The session environment pins this branch name, so the playbook's `claude/batch-948-<slug>` name is not used.
- Draft PR (claim): thoenissen/Reihitsu#1455, based on `main`.

# Lane: bug fix

Every issue reports an observed-versus-expected difference against existing requirements; none asks for new behavior, so no change doc.

- #948 — switch-case brace phase braces a section whose directly-declared local (or local function, per the issue comment) is used by another section; output fails to compile (FMT-PIPE-003, FMT-SWITCH-011).
- #949 — comment leading the terminal `break` gets its blank line only in pass 2 (FMT-PIPE-004, FMT-BLANK-011, SHARED-BLANK-004, FMT-SWITCH-007).
- #954 — empty region directly after `{`: BlankLinePhase and LineBreakPhase undo each other's edit (FMT-PIPE-004, FMT-BLANK-007, SHARED-BLANK-005/-006, FMT-BREAK-002).
- #1390 — structural transforms: expression-bodied `async` member returning through a task alias does not compile (CS1997); comments of a converted expression body; `with` initializer moves again in pass 2 (FMT-PIPE-003/-004, FMT-STRUCT-004/-006, FMT-BREAK-021).
- #888 — chain link whose line is begun by the tail of a multi-line block comment.

Roles: spec author (clarifications only — see below), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. None skipped.

# Maintainer clarifications (summarized)

- All five are critical: formatted code no longer compiles or changes again on a second pass.
- #888: the maintainer decided that SHARED-COMMENT-012 binds both RH5201 and the formatter. The RH5201 report is correct; the defect is in the formatter, which does not break the line before `.D` behind a multi-line comment tail. The formatter is fixed; the delegating RH5201 code fix must then converge (SHARED-FIX-001). SHARED-COMMENT-012 is to be added under `Uses` of the formatter line-break spec as a clarification. RH5201 analyzer behavior is not changed.
- Specs are clarified where an issue needs it (for example #948 asks for a guard requirement next to FMT-SWITCH-011). No change doc.
- Regression test first for each issue; formatter tests include an idempotency / second-pass check for #949, #954, #1390.
- Must not: change behavior beyond these issues; implement split-off extension parts; add a change doc; create issues (follow-up candidates go into handovers / run report).
- `sdd/specs/requirement-ids.txt`: insert new IDs at the sorted position.

# Files owned by concurrent runs — do not touch

- R2: formatter documentation-comment phase, CleanupPhase, documentation-comment code fixes, `sdd/specs/formatter/documentation-comments.md`, `cleanup.md`, `sdd/specs/shared/element-documentation.md`, `sdd/specs/README.md`.
- R3: RawStringAlignment phase, Indentation phase, Core `FormattingTextAnalysisUtilities`/`RawStringLiteralUtilities`/`StringInterpolationUtilities`, RH5601/RH5104 fixes, `raw-string-alignment.md`, `indentation.md`.
- R4: ordering/modifier fix bases, RH7xxx, Core `OrderingMoveSafety`/`RegionDirectiveUtilities`/`DeclarationModifierUtilities`/`ModifierOrderingUtilities`, `sdd/specs/shared/code-fixes.md`.
- R5: casing fix base, `CasingUtilities`, `sdd/specs/shared/casing.md`.
- R7: RH3xxx fixes/analyzers, RH5417 analyzer.
- Also: `sdd/README.md`, `sdd/templates/`, `squad/`.

A role that finds it needs one of these files stops and reports it in its handover.
