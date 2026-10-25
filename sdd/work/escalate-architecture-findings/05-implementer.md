Role: implementer
Commit: 415f6cb
Verdict: tests pass

This is repair round 1 for thoenissen/Reihitsu#1464, worked from `04-auditor.md` (audited head `82b0037`; I read it on head `341e381`). I re-derived each repair from the audit's evidence, using the auditor's suggestions only as suggestions. As in round 0, "tests pass" means the worklist is repaired: no test project covers squad files. My prompt carried no intended fix.

## Repairs

| Finding | File | What changed |
|---|---|---|
| 1 (medium, FC-017) | `squad/failure-classes.md` | Changed the generic cell check in *Class matrices* to "the output meets the requirement the decision serves, and a second application (a second formatting pass, or a second code-fix iteration) leaves it unchanged, under LF and CRLF". It says outright that a stable cell can still be wrong, and requires each matrix to name its requirement, so the check no longer depends on the surface. The tab matrix gains a **Check** paragraph. After one pass, every token of the hole line must stand at the FMT-RAW-007 column. Every anchored line must stand at the column its alignment requirement gives, measured after the tab replacement. A second pass must change nothing. The *Decision* consequence now separates the two failure modes. A tab counted as one, or only part of the whitespace counted, leaves the token three columns off, and the next pass moves it. A move clamped at the tab leaves the token off by the clamped amount, and the output can be stable but wrong. The closing note names the stable-but-wrong cell of the last row. |
| 2 (medium, FC-001) | `squad/roles/auditor.md` (*The repair round*) | A previous finding that now fails on a member its enumeration did not name counts as a repeat. That member is reported as a new finding, with `Repeats` naming the previous finding. The previous finding is `open` only for a listed member that still fails, and `closed` otherwise. The decision now covers the same span as `README.md` (*A repeated class*). Lead §6 ("a finding marked as repeating") needs no change, because the repeat is now always a marked new finding. |
| 3 (low, FC-015) | `squad/README.md`, `squad/roles/skeptic.md` (Q9), `squad/roles/auditor.md` (check 10), `squad/roles/implementer.md` (*Scope*) | README *Architecture findings* now states the scope once. It covers the defects that reach the run: a defect of one of the run's issues, a defect the change introduces, and a pre-existing defect the change depends on or exposes. These are the same defects the auditor's *Severity* may block on. A structural defect unrelated to the run is a follow-up candidate that names its structural cause. The section says it is the one statement, and the three role files now point to it instead of restating the causes. Auditor check 10 says an unrelated pre-existing structural defect is a hint, which matches *Severity*. Lead :48 needs no change: only in-scope findings are listed, so "whatever its verdict" holds. I derived the scope from the issue's purpose (steering the run) and the existing blocking rule. The issue text itself does not limit the scope. If the lead reads it more widely, this is a maintainer question. |
| 4 (low) | `squad/failure-classes.md` | Changed "each of three audit rounds found one more failing cell" to "three consecutive audits each found failing cells that the audit before had not enumerated". This matches the table: one cell in the first audit, two in repair round 1, one in repair round 2. |
| 5 (low, FC-003) | `squad/failure-classes.md` | Split the shift axis into five members: right; none; left by fewer than the spaces in front of the tab; left by exactly those spaces; left by more than those spaces. The boundary is now a cell the axis table spans. The closing note says the last repair pinned it. |

## Hints

- **A**: not addressed; it is the lead's step at publish. The PR description has to name the run of thoenissen/Reihitsu#1457 (issues #951, #1133, #1146).
- **B**: addressed. `auditor.md`'s handover note now says finding numbers continue across a run's audits, so `#<n>` in `Repeats` names exactly one finding. Without that, the column I introduced is ambiguous.
- **C**: lane part addressed. Option B in `lead.md` now lists escalation 10 next to 2 and 5, because a change doc after the implementer started widens the lane. Not addressed: whether option B uses up a repair round, and what "the first role of the lane it affects" means. Both are new process decisions that the issue does not make, so they are left to the maintainer.
- **D**: not addressed. Whether FC-013 analyzer–formatter disagreements count as "two components own the same decision" is the maintainer's call, as the auditor says. P11 and SHARED-DIAG-010 already define that relation.
- **E**: already listed as a follow-up candidate in `01-implementer.md`.

## Clarifications

I carried out the maintainer's answers recorded in `00-lead.md`, section *Maintainer answers* (commit `a7dde33`):

| Answer | File | What changed |
|---|---|---|
| 1 — scope stays as written | – | Nothing; the README scope statement from finding 3 already matches. |
| 2 — option B does not use up a repair round | `squad/README.md` (*Repairs*), `squad/lead.md` (section 4, *Architecture finding*, option B) | *Repairs*: "a structural change the maintainer chose under escalation 12 is not one of them". Option B: "the structural change does not use up one of the run's repair rounds". |
| 3 — FC-013 is not by itself an architecture finding | `squad/README.md` (*Architecture findings*) | New paragraph after the list of causes: an analyzer–formatter disagreement (FC-013) is not by itself an architecture finding, because P11 and SHARED-DIAG-010 define how the two surfaces relate. It stays an ordinary finding and reaches escalation 12 only through the repeated-class trigger. The role files point to this section, so they need no change. |
| 4 — tab-rule duplication already tracked | – | Nothing. The squad files cite no tracker number. |

Since this round's commit, the hints C (repair-round part) and D are settled by answers 2 and 3. `scripts/lane-check.sh tooling --head worktree` still reports `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Policy owners

- Architecture-finding definition and scope: `grep -n 'structural\|Architecture findings\|architecture finding' squad/roles/*.md squad/lead.md`. The owner is now `squad/README.md`, *Architecture findings*. Skeptic Q9, auditor check 10 and implementer *Scope* point to it without restating the causes. The lead routes by the handover section only.
- Repeat definition: the owner is `README.md`, *A repeated class*. `auditor.md`, *The repair round*, marks repeats, including on unlisted members of a previous finding. Lead §4, §6 and *Never* route on the mark.

## Tests run

`scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`. No test project covers squad files.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None new._
