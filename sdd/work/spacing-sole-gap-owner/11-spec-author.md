Role: spec author
Commit: 9af2141
Verdict: change doc written

Repair round 1, finding 1 of `10-spec-checker.md`. Only `sdd/changes/2026-11/spacing-sole-gap-owner.md` changes; no requirement, example, or visible output changes.

- `Visible output changes` intro: "it is a lead decision" replaced. Entry 14 is now stated as decided on the recommendation under the maintainer's standing instruction for this run. "Entry 10 was not a choice" now gives its reason: the output on `main` does not compile.
- `Decisions` › *One-line keyword positions no join reaches*: "(lead decision; the output question goes to a follow-up)" replaced by the reason on its own terms. No join reaches those positions, so the change leaves today's output, and their spacing is an output question outside this change, left to a separate issue. "Question 10 had shown" now reads "The examples answered with 10B had shown", matching the answer codes used elsewhere in `Decisions`.
- `Decisions` › *A rule decides the gap behind a right-carried comment in both directions*: "Lead decision in this run (row 19)" replaced. The entry now says it was decided on the recommendation under the maintainer's standing instruction (an output question raised after the answers is settled by its recommended option and reported at the end of the run). It names the chosen output (`b /** sum */+b` gives `b /** sum */ + b`, `main`: `b /** sum */+ b`; `c ? b /** yes */:b` gives `c ? b /** yes */ : b`) and the rejected alternative (keep the author's whitespace behind the comment where the rule wants a space).
- Checked: the whole change doc and the run's spec diff (`AGENTS.md`, `sdd/specs/**`) since `3a9d334^` for "lead", "row N", handover names, role names, `sdd/work`, and numbered questions. Nothing else remains. The `(1B)`…`(14A)` answer codes stay as the checker accepted them.

For the lead: entry 14 (question 15) must be in the end-of-run report to the maintainer.

## Changed requirements
_None._ Only the change doc is reworded.

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- The `ref`/`readonly` tuple-type positions are described as "left to a separate issue" without an issue number, because the follow-up is not filed yet. Alternative: cite the issue once it is filed. The lead may add the number before publishing.

## Follow-up candidates
_None._
