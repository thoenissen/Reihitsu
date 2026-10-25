Role: implementer
Commit: 6b458c1
Verdict: tests pass

Tooling lane with no tests: the change is prose in squad process files. "tests pass" stands for "the change the issue's *Proposed changes*, *Files or areas* and *Done when* sections define is made". There is no test-implementer handover. My prompt carried no intended fix or hoped-for verdict, only pointers to files and to thoenissen/Reihitsu#1457.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `squad/README.md` | *Escalations*: new item 12, an architecture finding or a repeated audit finding class, asked at once before the next role starts. Items 1–11 keep their numbers, so existing decisions entries and role references stay valid. New section *Architecture findings* covers the three structural causes (two owners of one decision, a component predicting or copying another's behavior, a rule stated in more than one place), the handover section `## Architecture findings`, and the lead's three options (narrow fix plus follow-up, structure first, stop). It also holds **Failure classes, not instances** (the skeptic and the auditor enumerate the class matrix and report all failing members together) and **A repeated class is an architecture signal** (same class means the same mechanism, not merely a shared FC ID). *Repairs*: a further round starts only when no finding repeats an earlier audit finding's class. | Issue proposals 1, 2, 3 |
| `squad/lead.md` | Verdict table: rows `Any role: architecture finding` and `Auditor: a finding that repeats the class of an earlier audit finding` route to escalation 12. New paragraph **Architecture finding.** gives the options A/B/C and what each means for the run; B's change-doc or scope needs go through escalations 2 and 5. Section 6: repeat and architecture findings are not routed into a repair round. *Never*: no spawning over an unanswered architecture finding, and no repair round for a repeated class. | 1, 3 |
| `squad/roles/skeptic.md` | Question 5: enumerate the defect's class from the catalog matrix and report all failing members together. New question 9, *Is the cause structural?*. Handover: `Breaking inputs` carries the class matrix, new section `## Architecture findings`, and a closing line saying an architecture finding is escalation 12 whatever the verdict. | 1, 2 |
| `squad/roles/auditor.md` | Check 2 walks the class matrices too. New check 3, *Classes, not instances*, and new check 10, *Structure*. The checks in between are renumbered 4–9; `rg` found no reference to auditor check numbers. *The repair round* marks every new finding that repeats an earlier audit finding's class. Handover: `Repeats` column in the findings table, new section `## Architecture findings`, and the closing note says that a repeat or an architecture finding is escalation 12. | 1, 2, 3 |
| `squad/roles/implementer.md` | *Scope*: report an architecture finding (including when the tests could pass only by writing a prediction or copy). Make the narrowest change, with no restructuring, and leave the structure to escalation 12. In a repair round, repair every failing member of an enumerated class. Handover: new section `## Architecture findings`. | 1, 2 |
| `squad/failure-classes.md` | The intro points to the matrices. New section *Class matrices* with the matrix *Tab width across formatter phases* (FC-012, FC-015, FC-016). It names the decision and its three components (`CleanupPhase` writes the column; `IndentationPhase` and `RawStringAlignmentPhase` predict it) and the axes: shift right / none / left ≤ spaces before the tab / left > those spaces; tab before the first token / a comment / nested opening quotes; unanchored / anchored. It also lists the four failing cells in the order the run found them. | "Done when": tab-width matrix |
| `squad/decisions/README.md` | Template range `<1–11 …>` becomes `<1–12 …>`. | Consistency |

How the matrix was derived: `git fetch origin 2bbdeec` and `git show 2bbdeec:sdd/work/raw-string-literal-content/07-auditor.md`, the auditor handover of thoenissen/Reihitsu#1457 with every round. The cells come from first-audit finding 2, repair-round-1 findings 2 (residual) and 8, and repair-round-2 finding 10. I took the shift direction of the misaligned nested-quotes cell (right, by 13) from the test table in commit `b8cc3e0`. The auditor's round-2 note that "every existing tab test shifts right or not at all" is the source of the FC-003 boundary sentence.

## Policy owners

- Escalation list: `grep -rn -i 'escalation' squad .claude AGENTS.md CLAUDE.md sdd/README.md scripts`. Every numbered reference (README items 1–11, lead verdict table, skeptic 3/1, implementer 4/5, cleaner 6, spec checker 1, decisions template `1–11`) is unchanged in meaning. Only the decisions template range changed. `.claude/agents/*.md` and `.claude/commands/squad.md` only point at the README and need no change.
- Auditor check numbers: `grep -rn 'check [0-9]\|Check [0-9]' squad .claude` found no references to renumber.
- The repository does not test squad files: `grep -rln 'squad/' src scripts .github` finds only the lane classifier and its tests, which classify paths.
- `scripts/lane-check.sh tooling --head worktree` reports `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Tests run

_N/A — prose-only change; no test project covers squad files._ The lane check above is the only mechanical check.

## Disputed tests or scope

- I added no new FC ID, on purpose. The catalog says "a new class is added only together with a `squad/decisions/` entry", and the issue rules out a decisions entry. So the tab matrix is attached to the existing classes the PR #1457 audit tagged (FC-012, FC-015, FC-016), and a matrix is defined as an enumeration of a known class, not a new class.
- `squad/README.md`, *Escalations*, closing paragraph: "a new rule is added only together with a decisions entry naming the failure it prevents". This run adds rules without an entry, as the issue directs ("the PR description names the run of #1457"). I left that sentence unchanged. The lead should make sure the PR description names thoenissen/Reihitsu#1457 (issues #951, #1133, #1146) as the failure the rules prevent.
- The squad files name that run descriptively ("the run that aligned interpolation holes of multi-line raw strings"), not by number, in keeping with the catalog's descriptive *Origin* values.
- I did not change `squad/roles/test-implementer.md`, `spec-author.md`, `cleaner.md`, `spec-checker.md` or `validator.md`, because the issue does not name them. The README rule ("a role that finds one reports it in its handover under `## Architecture findings`") covers every role. The test implementer already covers every candidate the skeptic lists and walks the catalog, which now contains the matrices.

## Architecture findings

_None._

## Follow-up candidates

- The FMT-CLEAN-002 tab rule is still stated twice in the formatter: `CleanupPhase.NormalizeWhitespaceTabs` (`src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs:176`) writes it, and `RawStringLineShifts.GetColumnAfterTabReplacement` (`src/Reihitsu.Formatter/Pipeline/RawStringAlignment/Utilities/RawStringLineShifts.cs:147,167`) predicts it for both `LayoutComputer` and `RawStringAlignmentPhase`. Under the new rules this is the kind of structural cause that would be escalated: a component predicting another's behavior. This is outside this tooling run.
