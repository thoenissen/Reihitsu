Role: spec author
Commit: d196347
Verdict: text written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 25, which I carried out as recorded in `00-lead.md`.

## Changed requirements

_None._ Process text only (decision 25, auditor finding 4 of `15-auditor.md`).

- `scripts/README.md`, *The lane check*: deleted the derived sentence after rule 5 ("A changed test fails the text-only proof, so a C# file outside the test projects … only when the diff holds no such file."). Rules 1–5 alone decide the lane. The paragraph keeps its lead-in ("these rules decide which") and the sentence on a change doc without a wording path.
- `sdd/changes/2026-11/squad-wording-lane-and-ci-only-watch.md`, *No test path in the wording lane*: deleted the clause "a changed test also fails the text-only proof, so it decides what a C# comment edit next to it counts as". The sentence now ends with "the lane-check rules in `scripts/README.md` decide which."
- `squad/lead.md` §4, row *Spec author: needs a test (wording lane)*: the row pointed to the deleted sentence ("it says what a changed test does to a C# comment edit next to it"). That clause is removed. The row still points to `scripts/README.md`, *The lane check*, as the one statement of the rules, and the rest of the row reads correctly.

Search after the edit: `rg -i "fails the text-only proof|changed test does|changed test also"` over `squad`, `scripts`, `sdd/changes`, `CLAUDE.md`, `AGENTS.md`, and `.claude` finds nothing. No other process file predicts the proof's verdict for a test change. `squad/roles/spec-author.md` *Wording* only says that the bare proof fails on a resource file, which holds.

## Examples for the tests

_None._ No test is needed.

## Output questions

_None._

## Internal choices

- I kept the sentence "A change doc without a wording path still needs `behavior-change`." in `scripts/README.md`. It restates part of rule 5 correctly and makes no prediction about the proof, so it is not the class of finding 4. Decision 25 named only the derived sentence. The alternative was to delete it too.

## Follow-up candidates

_None._

## Whole run

`scripts/verify-text-only.sh --base 4982c76 --head worktree` (with this handover staged):

```text
TEXT-ONLY PROOF: PASS — base=4982c76 head=worktree — 6 changed file(s) (6 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
```
