Role: spec author
Commit: c5473ef
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; the auditor's suggested change was treated as a suggestion and re-derived.

## Changed requirements
_None._ No spec under `sdd/specs/` covers the architecture tests. The only change is text: `squad/notes/README.md`, the "Never referenced" rule.

Re-derivation of auditor finding 1: the README stated the rule without exception and claimed `SquadNoteReferenceTests` enforces it, while the test now skips `sdd/work/` and its class summary allows the run's handovers to name the run's note. The README now names that exception and its cause — the folder is deleted before the pull request is merged (`sdd/README.md`, *Temporary working docs*; enforced at merge by `WorkingDocumentsTests`) — and says the test skips `sdd/work/` for that reason. "Merged" rather than "published" is deliberate: `squad/lead.md` recreates `sdd/work/<slug>/` for a CI repair after publishing, and the test summary also says "merged", so the two statements now agree. `rg` over the tree (excluding `sdd/work/`) finds no other statement of the rule outside the notes themselves.

Proof: Markdown only, no `.cs` file changed, so `verify-text-only.sh` has nothing to compare. `scripts/lane-check.sh tooling --head worktree`: "LANE CHECK: confirmed — claimed tooling, diff needs only tests" (`squad/notes/README.md` classified Text).

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Wrote the exception into the existing "Never referenced" item rather than a separate bullet; alternative: a new bullet, which would split one rule across two items.

## Follow-up candidates
_None._
