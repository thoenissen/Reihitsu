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
