Role: Lead
Commit: 53c7f13
Verdict: lane claimed — bug fix

# Run

- Issues (prompt order): thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146. Milestone 1.2.0.
- Slug: `raw-string-literal-content`.
- Branch: `claude/sweet-knuth-bj050c` (the session's mandated branch; used instead of the playbook name `claude/issues-951-1133-1146-<slug>`).
- Draft PR: thoenissen/Reihitsu#1457, base `main`, no stacking.

# Lane: bug fix (no change doc)

All three issues report an observed-versus-expected difference against existing specs:

- #951 — formatter output of multiline raw strings is not idempotent (FMT-PIPE-004, FMT-RAW). Bug fix.
- #1133 — RH5601 reports whitespace inside a raw string literal and its fix breaks compilation (SHARED-DIAG-003). Bug fix.
- #1146 — RH5104's fix changes a literal's value / reorders comments (SHARED-FIX-003, RH5104-001). Bug fix.

Roles: spec author (clarifications only, see below) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

# Maintainer's clarifications (summarized)

- #951: only cases 1 (multi-line interpolation hole) and 2 (raw string nested in a hole) are in scope. Case 3 (tab-indented literals) was split into thoenissen/Reihitsu#1432 and is out of scope; no tab normalization in raw strings. Reproductions are second-pass formatter tests.
- #1133 (maintainer decision J4): SHARED-DIAG-003 takes precedence over RH5601-001/RH5602-001 — whitespace inside a literal or inside interpolation-hole content is code and is not reported. RH5601-001 and RH5602-001 are aligned to SHARED-DIAG-003 as a clarification.
- #1146: in scope are Q1 (the fix would write a comment into a verbatim/raw/interpolated-raw string or a documentation comment) and Q4 (two comments on a code-free line, the fix reorders them). The fix is withheld for these inputs (SHARED-FIX-003; relocation per AGENTS.md "Code-fix coverage"). Q3, Q5, Q6 moved to thoenissen/Reihitsu#1433 and are out of scope — no RH5104 widenings.
- #1146 (maintainer decision J1): SHARED-FIX-006 takes precedence over RH5104-005 — the fix inserts no blank line before the moved comment. RH5104-005 is reworded as a clarification.
- `sdd/specs/shared/code-fixes.md` is read-only for this run (another run owns it). No change doc. `sdd/work/` never enters the published PR.

# Files owned by concurrent runs — do not touch

SwitchCaseBraces, BlankLines, LineBreaks, StructuralTransforms phases and RH5201 (R1); DocumentationComments, CleanupPhase, doc fixes (R2); ordering/modifier/region fix bases, RH7xxx, `sdd/specs/shared/code-fixes.md` (R4); casing (R5); RH3xxx and RH5417 (R7); `sdd/README.md`, `sdd/templates/`, `squad/`. `requirement-ids.txt`: sorted insert only. `Core/FormattingTextAnalysisUtilities.cs` is touched by a later run (R8) — any change there must be minimal and named in the handover.

# Lane checks

# Escalations

1. Escalation 1 (#1133, after 01-spec-author): how far RH5601-001/RH5602-001 move toward SHARED-DIAG-003. Maintainer chose **A — narrow**: only whitespace that is part of a literal's text (raw-string line indentation, the closing line included, interpolated or not; hole content) is exempt from reporting. Whitespace next to a literal keeps its existing exemption; no new diagnostics for whitespace before/after a literal or in a hole's expression. `FormattingTextAnalysisUtilities.cs` is not touched for this. The remaining gap against SHARED-DIAG-003 becomes a follow-up candidate (text shown to the maintainer before creation). Answer is recorded here and in the PR run report only, not in `squad/decisions/` (maintainer's instruction).
2. Escalation 1 (after 02-skeptic, questions 1–4). Maintainer's answers:
   - Q1 (#951) **1B**: the formatter stops the double move of hole lines, but freezes only hole lines whose column comes from the scope depth alone; a hole line that an alignment requirement places relative to a token (FMT-INDENT-012 … 029, directly or through the construct around it) keeps that alignment, measured after the literal's shift. Today's stable output for anchored hole lines and for lines broken inside a hole is kept; no analyzer changes.
   - Q2 (#951) **2A**: in scope — lines inside the text of a verbatim string (or other non-raw literal) or a `/* */` comment nested in a hole stay as written; the formatter must not change a nested literal's value. The PR states it fixes this case beyond #951's text.
   - Q3 (#1133) **3A**: whitespace after the opening quotes of an interpolated multi-line raw string is part of the literal's text and is not reported, like the plain raw string. Maintainer's reason: the content of a string is program logic and is never changed.
   - Q4 (#1146) **4B**: when the comment's line begins inside an interpolation hole, RH5104 still reports the comment but withholds the fix; RH5104-006 names the hole case.
   Recorded here and in the PR run report only, not in `squad/decisions/`.
3. Escalation 1 (after the second spec-author revision; #951, FMT-RAW-007 comment lines). Maintainer chose **A**: a comment that starts a line inside a hole takes the column of the token it precedes, measured at that token's final column after FMT-RAW-007 (FMT-INDENT-006), e.g. `// c` above `name` both at the same column. Recorded here and in the PR run report only.

# Lead notes

- Skeptic re-check (12b09f9): R1–R4 routed to the spec author without a new escalation. R1 narrows "through the construct around it" (wording from the 1B option, not the maintainer's own) to constructs inside the hole; this serves the maintainer's stated goal of 1B — remove exactly the non-idempotent shape, keep stable layouts — and restores FMT-PIPE-004 for a raw literal inside a lambda body. To be named in the run report.
