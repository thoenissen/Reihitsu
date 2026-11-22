# Run: rh4120-dictated-record-param

- Issue: thoenissen/Reihitsu#1939 — `agent-filed` (goals are proposals; the visible output was decided by the maintainer, see below)
- Branch: `claude/squad-1939-81sm3x`; draft PR #1947
- Lane: behavior-change — the issue asks for changed analyzer behavior (RH4120 stops reporting a positional record parameter whose generated property implements a property declared in a referenced assembly), which needs a spec change and production code.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

1. #1939 output question (answered on the issue, comment 2026-11-22): **option A** — RH4120 does not report a positional record parameter whose generated property implements a property of another (referenced) assembly; that name is dictated, as for properties under SHARED-CASING-014. Example: `public record Person(string name) : INamed;` with `INamed.name` declared in a referenced assembly is not reported.

2. Batch before the test implementer (asked in chat), answered "as recommended" = **1A, 2B**:
   - 1A: `public record Item(string title) : MetaRecord;` where `MetaRecord.title` is an abstract property of a referenced assembly — the parameter is not reported (override counts like implementation).
   - 2B: a positional parameter for which the compiler declares no property, because the record inherits or declares a member of its name, is not reported when that member is dictated (SHARED-CASING-014): `public record Derived(string name) : Person(name);` (Person.name implements referenced `INamed.name`) is not reported; same for a property declared in the record's body that implements a referenced-assembly member, e.g. `record R(string name) : INamed { public string name { get; init; } = name; }`. Option C (also exempt when the matching inherited member merely comes from another assembly without being dictated, e.g. `record Local(string name) : MetaPerson(name)`) was NOT chosen — that stays reported.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Where does the exemption live? | As a bullet of SHARED-CASING-014 referenced from RH4120-001, no new requirement ID | One statement of what makes a name dictated | Spec structure only | 243da8d |
| 3 | Skeptic | RH4120-001 claims to cover only parameters the compiler turns into a property, but the code and existing tests report every positional parameter | Widen RH4120-001's coverage statement to every positional parameter (minus the exemptions), describing today's code; route to the spec author with the answers | Spec matches existing behavior and tests | Describes unchanged output | 8d09179 |
| 2 | Spec author | Parameter of a derived record that inherits a dictated property (`Derived(string name) : Person(name)`) | Stays reported (no property is declared for it) — SUPERSEDED by maintainer answer 2B (not reported when the matching member is dictated) | Today's behavior; only the compiler-declared property decides | Unchanged output | 243da8d, superseded by 8d09179 |

| 4 | Auditor (08) | Architecture finding (FC-044): `GetMember` ignores accessibility when finding the inherited member a parameter matches | Fix narrowly within the run: skip a candidate the record cannot access, reading the compiler's accessibility result (one cheap condition, no new type); repair round 1 — test implementer adds the private/internal intermediate-member cases and the `new` field case (finding 2), implementer repairs | Reads a compiler result instead of modelling it; refusing (reporting) would contradict answer 2B for these inputs | Brings code to the unchanged requirement SHARED-CASING-014 as answered (2B); rare inputs | (pending) |
| 5 | Auditor (08) | RH4116 does not apply 2B to tuple element names (auditor hint, uncertain) | Dropped: outside the run, auditor judged probably correct, no defect shown | No evidence of a defect | Nothing changes | — |

## Follow-up candidates

(none yet)

## Audited SHA

3b066662dc49e2abcd3964664c6937adf78bed0c (audit 1, findings)

## Open worklist / next step

- Spec author done (01, 243da8d): change doc `sdd/changes/2026-11/record-parameter-dictated-name.md` (Draft).
- Open output question for the batch before the test implementer: the override case (`record Item(string title) : MetaRecord` where `MetaRecord.title` is an abstract property of a referenced assembly) — the maintainer's answer names only "implements"; a case the answer does not name is a new question.
- Skeptic done (02, 6b36c13): premise confirmed (CS0535/CS0534 on rename), reachable. Output questions Q1 (override) and Q2 (parameter with no generated property because the record declares/inherits a member of its name) asked to the maintainer in one batch.
- Answers received (1A, 2B). Lead accepts the change doc.
- Next: spec author follow-up (fresh: earlier agent large, cache cold) — answers 1A/2B into change doc Decisions + spec, decision 3, Status: Accepted. Then test implementer.
- Spec author follow-up done (03, 8d09179), change doc Accepted.
- Test implementer done (04, 3c509ca tests, 9e4ba00 handover): 16 new tests, 12 fail as intended.
- Implementer done (05, 28a5103 prod, 926139b handover): +43/-1, no new type.
- Cleaner done (06, 1cb0e2a): clean, 0 warnings.
- Spec checker done (07, f32e03b): consistent, no findings.
- Auditor done (08, 57ab48d) at audited SHA 3b066662: findings (1: accessibility in GetMember, 2: untested field branch).
- Repair round 1 of 3: test implementer (09, done 2bf27de/670609e: 3 tests, 2 fail as intended) → implementer (10) → cleaner → spec checker → auditor (repair only) → validator.
