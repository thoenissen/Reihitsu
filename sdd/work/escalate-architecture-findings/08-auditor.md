Role: auditor
Commit: 2b38f7ed053176c0952fd0fdc0ae8e9a3625a2a6
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It told me to run the audit by the role files on `origin/main` and to audit the branch's versions of those files. This handover therefore uses main's template, with no `Repeats` column and no `Architecture findings` section. Finding numbers continue from `04-auditor.md`.

## Scope

Repair delta for thoenissen/Reihitsu#1464 (tooling lane): head `2b38f7e` against the previous audit at `82b0037`. Base `de4c614`.

- State:
  - The checkout is `2b38f7e`.
  - `git ls-remote origin refs/heads/main` gives `de4c614`, which is the SHA in my prompt. `git merge-base --is-ancestor origin/main HEAD` holds.
  - `scripts/lane-check.sh tooling` reports `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
  - `sdd/work/` holds only this run.
- The repair is what the handovers describe:
  - `415f6cb` is the implementer's repair of findings 1–5 and hints B and C.
  - `0355998` writes maintainer answers 2 and 3 into `squad/README.md` and `squad/lead.md`, and appends a *Clarifications* section to `05-implementer.md`.
  - Every other commit in `82b0037..2b38f7e` adds a handover or `00-lead.md` text.
  - No C# file and no compiled path changed. I trust the cleaner's Release build on `0355998`; only handovers changed after it.
- Scope did not grow. The repair touches the six squad files the first audit covered (`README.md`, `failure-classes.md`, `lead.md`, and `roles/auditor.md`, `roles/implementer.md`, `roles/skeptic.md`). It adds no new surface and changes no spec. I audited the changed paragraphs and every place that states the same rule.
- Evidence reuse: the failing-cell table (`failure-classes.md:94–99`) and its FMT citations are byte-identical to `82b0037`, so I reused the first audit's check of them against `2bbdeec:sdd/work/raw-string-literal-content/07-auditor.md`. I re-checked the changed claims against that record:
  - The clamped cell is stable but wrong when unanchored: `name` lands at 4 where FMT-RAW-007 requires 2 (`:428`).
  - The anchored clamped cell breaks FMT-PIPE-004 (`:429`).
  - Round 3 pinned both the "exactly" boundary and the shift beyond it (`:526–527`).
  - The cells per audit are one row in the first audit, two in repair round 1, and one in repair round 2.
- Maintainer answers (`00-lead.md`, *Maintainer answers*):
  - 2 is carried out in `README.md:102` and `lead.md:48`.
  - 3 is carried out in `README.md:90`.
  - 1 and 4 need no text.
  - The FMT-RAW-007 conditions that the new *Check* paragraph relies on exist (`sdd/specs/formatter/raw-string-alignment.md:264–272`).

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `failure-classes.md:76`: a cell holds when the output meets the decision's requirement and a second application leaves it unchanged; "a cell can be stable and wrong". `:84` checks the tab matrix against FMT-RAW-007. `:82` separates the three-column miscount (moved by the next pass) from the clamped move (off by the clamped amount, possibly stable). `:101` names the stable-but-wrong cell. The surface clause covers formatter passes and code-fix iterations; for an analyzer matrix, the requirement clause carries the check. A wording gap in the new `:84` is finding 8. |
| 2 | closed | `auditor.md:51`: a previous finding that fails on a member its enumeration did not name is reported as a new, marked finding, and it stays `open` only for a listed member that still fails. This now spans `README.md:98` ("one enumeration already missed a member"). The lead routes on the mark (`lead.md:45,63,97`). See *Guard scope*, row 1. |
| 3 | closed for the case it named | An unrelated pre-existing structural defect now gets the same answer everywhere. `README.md:92` makes it a follow-up candidate. `auditor.md:32` and `:45` make it a hint. Skeptic Q9 (`skeptic.md:29`) and `implementer.md:38` point to README. The scope still disagrees in two places that finding 3 did not name: findings 6 and 7. |
| 4 | closed | `failure-classes.md:80` now matches the table, which has rows from three consecutive audits. |
| 5 | closed | `failure-classes.md:88` has five shift members, and "left by exactly those spaces" is a cell of its own. `:101` records that the last repair pinned it. |

Previous hints:
- A: still open. It is the lead's step at publish; see hint F.
- B: addressed (`auditor.md:97`).
- C: addressed. `lead.md:48` names escalation 10 and the repair-round count.
- D: settled by answer 3.
- E: tracked by thoenissen/Reihitsu#1463 (answer 4).

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 6 | low | `squad/README.md:92` ("a pre-existing defect the change depends on or exposes — the defects an audit finding may block on (`roles/auditor.md`, *Severity*)") against `squad/roles/auditor.md:47` ("pre-existing behavior the change **newly** depends on or exposes") | FC-002, FC-015 | PR-introduced (repair round 1) | README restates Severity's blocking set and says it is that set, but drops "newly". The two disagree for a confirmed pre-existing structural defect in code the change already depended on before the change. Example: a run changes how `RawStringAlignmentPhase` moves a hole line, while a failing tab cell on the measuring side stays as it was. The measuring side is `RawStringLineShifts` predicting `CleanupPhase`, the structure behind thoenissen/Reihitsu#1463. `README.md:92`, and auditor check 10 (`auditor.md:32`), which points to it, make that defect an architecture finding that stops the run (escalation 12). Severity (`auditor.md:45,47`) makes it a hint that does not block. | Use Severity's wording in `README.md:92` ("newly depends on or exposes"), or keep only the pointer to *Severity* and drop the restated list. |
| 7 | low | `squad/failure-classes.md:76`, last sentence ("because a class whose members fail between two components is also an architecture finding") | FC-016, FC-015 | PR-introduced | The repair made the role files point to `README.md` *Architecture findings*, which calls itself "the one statement of what an architecture finding is" (`:92`). This copy states the criterion with no condition, and it disagrees with README in two cases. (a) A class that fails between an analyzer and the formatter is an architecture finding here, but `README.md:90` says FC-013 is not by itself one. This case is latent: the catalog has no FC-013 matrix yet. (b) Take an auditor of an unrelated run who confirms a failing cell of the tab matrix on main, the defect behind thoenissen/Reihitsu#1463. Here it is an architecture finding, `README.md:92` makes it a follow-up candidate, and `auditor.md:32` makes it a hint. | Point instead of asserting, for example "…because a class whose members fail between two components can be an architecture finding (`README.md`, *Architecture findings*)". |
| 8 | low | `squad/failure-classes.md:84` (*Check*: "A cell holds when, after one pass, … and a second pass changes nothing") against `:76` ("… leaves it unchanged, under LF and CRLF") | FC-017 | PR-introduced (repair round 1) | The tab matrix defines "a cell holds" a second time and leaves out line endings. A reader who takes the matrix-specific definition checks one line ending, which is the weak check FC-017 names ("LF only for line-ending stability"). The generic sentence also attaches "under LF and CRLF" only to the stability clause. | Add "under LF and CRLF" to the *Check* paragraph and let it apply to both clauses. Alternatively, phrase *Check* as the requirement part of `:76`'s test, for example "Checked against FMT-RAW-007: …". |

Why `pass, text cleanup`:
- All three findings lie in Markdown under `squad/`. Nothing there compiles, and `scripts/verify-text-only.sh` treats Markdown as a known non-compiled text path, so a fix cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata.
- Each suggested change makes a second statement match an owner statement that an audit has already checked, or replaces it with a pointer. The owner for finding 6 is `auditor.md:47`, which is unchanged from main. The owner for finding 7 is `README.md:90,92`. The owner for finding 8 is `failure-classes.md:76`. So no fix adds a rule that has not been audited.
- For finding 6, the direction matters: changing Severity instead of README would change main's blocking rule, and that change would need a re-audit.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Is an audit finding a repeat? | `auditor.md:51`: a new finding of the same class, and a previous finding failing on a member its enumeration did not name. A listed member that still fails is plain `open` | `README.md:98`: same mechanism, differing only in input shape; "one enumeration already missed a member". `lead.md:45,63,97` route on the mark | A listed member that still fails has the same input shape, so README does not count it a repeat either | agree (finding 2 closed) |
| Does a matrix cell hold? | `failure-classes.md:84`: FMT-RAW-007 columns after one pass, and a second pass changes nothing | `:76`: requirement and second application, under LF and CRLF. Source record `2bbdeec:…/07-auditor.md:428–429` | Agree on the requirement and on stability; `:84` omits line endings | mismatch, finding 8 |
| Is a defect in architecture-finding scope? | `README.md:92`: the run's issues, the change's defects, and pre-existing defects the change "depends on or exposes" | `auditor.md:47` ("newly depends on or exposes"), `:45` and `:32`; `skeptic.md:29` and `implementer.md:38` point to README; `failure-classes.md:76` gives no scope | Disagree for a pre-existing defect the change already depended on, and for an unrelated defect enumerated from a matrix | mismatch, findings 6 and 7 |
| Is an analyzer–formatter disagreement an architecture finding? | `README.md:90`: not by itself; only through a repeat | `lead.md:45,63` (repeat route); `failure-classes.md:76` (any class failing between two components) | `:76` is wider | mismatch, finding 7 (latent) |
| Does a structural change under escalation 12 use a repair round? | `README.md:102`: not one of the three | `lead.md:48` (option B), `:63` (points to README), `:80` (CI repairs count), `:96` | Agree | agree |
| Which shift members does the tab matrix span? | `failure-classes.md:88`: five members | `:101`: boundary pinned as a cell | Agree | agree (finding 5 closed) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| What an architecture finding is, and its scope | Search: `rg -n -i 'architecture finding\|structural\|predict(s\|ing)? or cop\|own(s\|ing)? the same decision\|stated in more than one place' squad .claude AGENTS.md CLAUDE.md sdd/README.md scripts .github`. Owner: `README.md:84–92`. Pointers: `skeptic.md:29`, `auditor.md:32`, `implementer.md:38`. Restatements: `failure-classes.md:76` (no condition), and `README.md:92`'s copy of Severity's blocking set (`auditor.md:47`). `.claude/agents/*.md` only point at the role files | two drifting restatements: findings 6 and 7 |
| Repeat definition | Owner: `README.md:98`. Marker: `auditor.md:51,97`. Routing: `lead.md:45,63,97` | single owner, agree |
| Repair-round count | Search: `rg -n -i 'repair round\|use(s)? up'`. Owner: `README.md:102`. Restatements: `lead.md:48` (the option B exemption, consistent with the owner), `:63` (points to README), `:80` (CI repairs), `:96`. Escalation 7 at `README.md:73` | single owner, agree |
| When a matrix cell holds | `failure-classes.md:76` (generic) and `:84` (tab matrix) | two statements that differ on line endings: finding 8 |
| Finding numbering across audits | `auditor.md:97` | single owner |

## Assertion adequacy

_N/A — the repair adds or changes no test. No test project covers squad files; only `LaneClassifierTests` touches them, and it classifies paths. The prescribed cell check, the closest analog, is finding 8._

## Failure classes dismissed

- FC-001, FC-002, FC-003, FC-015, FC-016, FC-017: walked against the repaired predicates. They led to findings 6 (FC-002, FC-015), 7 (FC-016, FC-015) and 8 (FC-017), or to the `agree` rows above. FC-003 is the closed finding 5.
- FC-031: no changed paragraph keeps text that describes its old rule:
  - Skeptic Q9, auditor check 10 and implementer *Scope* no longer restate the causes.
  - `README.md:102` and `lead.md:48` carry answer 2.
  - `failure-classes.md:82,101` no longer claim a single failure mode.
  - The first audit's check-number search still holds, because the repair renumbers nothing.
- FC-032: the cleaner's build ran on `0355998`, and only handovers changed after it.
- FC-037: no C# file changed, so no text-only proof is needed for this audit.
- FC-004–FC-014 and FC-018–FC-036 (except FC-031 and FC-032): these concern compiled analyzer, code-fix, formatter, or test behavior. The repair changes no compiled file, test, spec, or rule document. FC-013 appears only as the subject of the new `README.md:90` rule, which is checked above.

## Hints

- **F** (previous hint A, extended). At publish, the PR description must name the run of thoenissen/Reihitsu#1457 (issues #951, #1133, #1146) as the failure the rules prevent. It must link thoenissen/Reihitsu#1463 under `Follow-up work` (answer 4). Answers 2 and 3 were escalation answers that went straight into rule text, with no decisions entry. `squad/decisions/README.md` says "the PR names the entries it folds in", so the run report should name those answers (`lead.md:73`).
- **G.** Under the branch's own repeat rule (`auditor.md:51,97`), findings 6 and 7 would repeat #3. Both concern the same decision, which defects are architecture findings. Finding 6 is between #3's components, README and auditor *Severity*. Finding 7 is a place #3's enumeration did not name. `lead.md:63` would then not route them and would stop the run at escalation 12, also under `pass, text cleanup`. This audit follows main's rules, which have no repeat trigger, so nothing here carries a mark. The issue sets no severity floor for the trigger. Whether a low, text-only repeat should stop a run is the maintainer's call.
- **H** (uncertain). `README.md:92` calls an unrelated structural defect "a follow-up candidate". `auditor.md:32` and `:45` make it a hint, and the auditor template has no follow-up section, while *Follow-ups* (`lead.md:88`) starts from "a follow-up candidate from any handover". The same split exists on main: README *Rules for every role* versus auditor *Severity*. In this run the lead took hint E to the maintainer, so it works in practice.
- **I.** The `Commit` line of `05-implementer.md` reads `415f6cb`, but its *Clarifications* section was added in `0355998` and describes that commit's changes. `sdd/work/` is deleted before publish, so the tree is not affected.
