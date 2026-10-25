Role: spec author
Commit: 741d3fb
Verdict: text written

Post-audit text cleanup for thoenissen/Reihitsu#1464 (tooling lane), worked from `08-auditor.md` findings 6, 7 and 8 (audited tree `2b38f7e`). I re-derived each fix from the owner statement the auditor names and used its suggestions only as suggestions. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No spec, change doc, rule page, or C# file changed.

## Changed text

| Finding | File | What changed | Why this direction |
|---|---|---|---|
| 6 (FC-002, FC-015) | `squad/README.md`, *Architecture findings*, scope paragraph | "a pre-existing defect the change depends on or exposes" now reads "… the change **newly** depends on or exposes". | The sentence says it is the set *Severity* may block on, and *Severity* (`roles/auditor.md:47`, unchanged from main) says "newly". README is changed, not *Severity*, so main's blocking rule stays as audited. This keeps maintainer answer 1 (only defects that reach the run; an unrelated one is a follow-up candidate): a pre-existing defect the change already depended on before is not newly reached, which agrees with auditor check 10 and *Severity* making it a hint. |
| 7 (FC-016, FC-015) | `squad/failure-classes.md`, *Class matrices* | "a class whose members fail between two components is also an architecture finding" now reads "… can be an architecture finding; whether it is one of the run is decided by `README.md`, *Architecture findings*" (linked). | README calls itself the one statement of what an architecture finding is. The unconditional copy contradicted it for FC-013 (maintainer answer 3) and for a structural defect unrelated to the run (answer 1). The sentence keeps its reason for naming components and defers the decision to the owner. |
| 8 (FC-017) | `squad/failure-classes.md`, *Class matrices* and the tab matrix *Check* | The generic definition now reads "A cell holds when, under LF and under CRLF alike, the output meets the requirement … and a second application … leaves it unchanged." The tab matrix *Check* no longer defines "a cell holds" a second time: "The cells hold as Class matrices says, checked against these requirements:" followed by the FMT-RAW-007 column and the anchored-line column, measured after the tab replacement. | One definition of a holding cell, with both line endings applying to both clauses; the matrix only names its requirements, as the generic text asks ("Each matrix names the requirement its cells are checked against"). The second-pass condition dropped from *Check* is carried by the generic definition it now points to. |

Nothing else changed. `grep -rn 'depends on or exposes\|is also an architecture finding' squad .claude AGENTS.md sdd/README.md` finds no other copy of the changed wording. `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Changed requirements

_None._ No spec under `sdd/specs/` changed.

## Examples for the tests

_None._ No test project covers squad files.

## Open questions for the maintainer

_None._
