# Outline: sdd/changes/2026-10/introduce-sdd.md (replaces introduce-sdd.md, establish-squad.md, requirement-coverage-opt-out.md)

Header: Status Accepted · Date 2026-10 (original) · Affected requirements: none — process change. No `Depends on` row.

- **What** — SDD as it is now in six bullets (specs, IDs + ledger + enforced coverage, open-question tracker, change docs + work docs, squad, principles P1–P39); points to `sdd/README.md` and `squad/` as the rulebooks.
- **Why** — navigation, a code-independent reference, traceable decisions, a smaller workflow, structural independence of roles, one agent-neutral instruction source.
- **Decisions**
  - *Specs follow behavior, not projects* — current `sdd/` layout tree; no spec for Core/Tooling; formatter per phase; `shared/` specified once, referenced by ID; behavior not implementation.
  - *Specs and user documentation are different documents* — rule pages are user docs derived from the spec; spec changes first.
  - *Spec format* — header (prefix, status, code owners, Uses; analyzer descriptor rows), non-goals, one requirement per heading, normative sentence → Why → examples, reference not restate.
  - *Requirement IDs and the ledger* — format, immutable, retired in place, append-only `requirement-ids.txt` guarded by CI.
  - *Tests name requirements, and coverage is enforced for every spec* — `[Requirement]` tag, derived tests tagged, untagged tests allowed, opt-out coverage with reasoned (empty) exemption list, shared `Uses` rule, spec checker checks tags.
  - *Mechanical checks* — SpecConsistencyTests, SpecStructureTests, RequirementCoverageTests in one line each.
  - *Open questions and the question tracker* — leave out of spec, one `SDD open questions: <spec path>` issue per spec, self-contained questions, three decision paths, never only in a PR.
  - *Principles* — how a principle turns instances into bugs; table P1–P39: number, one-line gist, where it lives.
  - *Change docs* — when required / not; content; Accepted before ready; no task lists; history.
  - *Temporary working docs* — `sdd/work/<slug>/`, committed, deleted before ready.
  - *Agent-neutral instructions* — AGENTS.md source, CLAUDE.md import, `.claude/agents` wrappers, `/squad`, Bash-only scripts, two reference commands.
  - *The squad: roles* — lead's limits; role table (job / must not); strict separation.
  - *Lanes* — path-decided lanes, lane table with admitting paths and roles, order, widening orders, run report.
  - *Handovers are files* — `<nn>-<role>.md`, header lines, facts only, `[skip ci]` commits, lead alone pushes.
  - *Escalations and the decisions log* — the eleven escalations; chat format; decisions log by its standing purpose only (one file per answer, recorded before continuing, recurring answers become rules; squad never tunes itself).
  - *Repairs* — routing by owner, one worklist, one round, text-only proof spares a second audit.
  - *Issue intake* — issues as hypotheses; reproduction confirms; non-reproduction → characterization test + `Refs`.
  - *Several issues in one run* — prompt sets the batch, one agent per role, per-issue handover sections, one lane, shared change doc, run-wide stop, batch scope, one link per issue.
  - *Claiming, publishing, and CI* — draft-PR claim, `[skip ci]` until one trigger commit, publish ready, watch CI.
  - *The failure-class catalog* — purpose and who uses it.
  - *Keeping it lean* — sorting of old workflow rules, no TODO / tracker numbers, short role files.
- **Alternatives considered** — merged, de-duplicated list from the three old docs plus the open-question and lane-judging alternatives.

## Links to update

`git grep -n -E 'establish-squad|requirement-coverage-opt-out' -- . ':!sdd/changes'` (no tracked file outside `sdd/changes` names `introduce-sdd.md`):

| File:line | Kind | Suggested replacement |
|---|---|---|
| `squad/README.md:3` | Real link: `[Establish the squad](../sdd/changes/2026-10/establish-squad.md) records why it is built this way.` | `[Introduce Spec Driven Development](../sdd/changes/2026-10/introduce-sdd.md) records why it is built this way.` |
| `squad/decisions/2026-10-22-1403-batch-roles-and-change-doc.md:12` | Path named in a decisions entry (history) | None — the entry is deleted when `squad/decisions/` is emptied in the same PR. If it were kept, leave it (entries are history). |
| `squad/decisions/2026-10-23-998-change-doc-approved.md:11` | Path named in a decisions entry (history) | None — deleted with the decisions log in the same PR. |
| `src/Reihitsu.Tooling.Test/Unit/LaneClassifierTests.cs:21` | Test fixture data, not a reference. `[DataRow("sdd/changes/2026-10/establish-squad.md", PathCategory.ChangeDoc)]` feeds `LaneClassifier.ClassifyPath(path, false)`, which only matches the `sdd/changes/` prefix (`src/Reihitsu.Tooling/LaneClassifier.cs:49`) and never touches the file system; its neighbours are likewise invented paths (`sdd/work/run/01-spec-author.md`, `notes.txt`, `src/Reihitsu.Formatter.Test/Unit/SampleTests.cs`). Any `sdd/changes/...` path would do. | Optional. Nothing breaks if left. To avoid naming a deleted doc, change it to `sdd/changes/2026-10/introduce-sdd.md` (or a neutral `sdd/changes/2026-10/example.md`). Editing it is a test-file change, so the cleanup run's lane must admit test paths (tests or wider; tooling already is). |
