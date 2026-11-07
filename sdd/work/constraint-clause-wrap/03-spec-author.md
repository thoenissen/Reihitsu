Role: spec author
Commit: f65fbe2
Verdict: change doc written

This is a repair of the spec after the maintainer's batch-1 answers (`00-lead.md`, Q1–Q8 all A) and the lead's decisions 2–4. The skeptic's amendments 1–5 are in `02-skeptic.md`. The coordinator's message carried no intended fix or suspected cause.

## Changed requirements

- FMT-BREAK-037 — The constraints of a clause are on one line or one constraint per line — changed (rewritten; still unmerged, added in 44509cd).
  - It applies to every owner of a clause, extension blocks included (amendment 5, decision 4).
  - First step: the colon joins the type parameter's line (Q7), and every wrap inside one constraint is joined: qualified or alias-qualified name, type argument list, `new()`, `class?`, `allows ref struct` (Q6).
  - The clause is judged after those joins (amendment 3). A clause that is not wrapped is changed only by those joins (amendment 3).
  - A comment, a directive, or disabled text blocks a join (amendments 1 and 2). In that case the colon, the first constraint, or the comma keeps its line, and a constraint kept over two lines wraps its clause (Q5).
  - Blank lines in gaps that are not joined stay (FMT-BREAK-030).
  - The earlier coverage sentence that excluded the `where T⏎ :` gap is removed.
- FMT-INDENT-032 — Wrapped constraints align to the first constraint — changed.
  - It applies to every owner, extension blocks included.
  - A first constraint pushed down by a comment, a directive, or disabled text takes the column behind the colon plus one space (Q3, amendment 2).
  - A colon pushed down takes `where` + 4, and the constraints follow from the colon (Q7 fallback).
  - It places only lines that start with the colon, a constraint, or a comma. A constraint's inner line is placed by its own construct, measured from the constraint's final column (FMT-INDENT-030). FMT-INDENT-015 is named for a later type argument (Q5).
- RH5110-004 — The fix is withheld where the layout needs a comment or directive moved — added (Q8). Per `AGENTS.md`, *Code-fix coverage*, a withheld fix is recorded in the rule's spec. `Uses` now includes FMT-BREAK-001, FMT-BREAK-037, and SHARED-FIX-003. The RH5110 non-goal now reads "Reporting line breaks inside a constraint clause …", because the fix now lays those breaks out. The ledger includes RH5110-004.
- Change doc `sdd/changes/2026-11/constraint-clause-wrap.md`:
  - `Status: Accepted`. Every visible output change in it has an answer, and the run's batch-1 answers cover every entry.
  - `Affected requirements` adds RH5110-004.
  - *What* is rewritten.
  - New entries 9–13: the colon gap and its comment fallback, the join inside a constraint, directives, extension blocks, and RH5110 withheld.
  - Entry 6 is retitled.
  - *Decisions* records Q1–Q8 with the rejected options. It also records the extension-block scope and keeps the no-analyzer-change decision.
  - The claim that a multi-line constraint needs a type argument list is corrected (amendment 4, in Q5's decision).
  - New alternatives cover Q6, Q7, and Q8.
- `01-spec-author.md`: follow-up candidate 1 is dropped (it joined the run).
- `SpecStructureTests` and `SpecConsistencyTests` pass (26/26). `RequirementCoverageTests` will fail until FMT-BREAK-037, FMT-INDENT-032, and RH5110-004 have tests.
- No rule page changed. The RH5110 withholding is a corner case, and `general-notes.md` covers it ("A diagnostic without a code fix").

## How the "before" outputs were taken

- Entries 9–12 come from `scripts/format.sh` on c317972, with no production change since b1d4a94.
- Entry 13 comes from `scripts/apply-fix.sh RH5110` on the same tree. The fix is offered today and leaves `class, new()` and `, new()` at column 4, under LF and CRLF.
- The "after" outputs are derived from the answers. They have not been run.

## Examples for the tests

- FMT-BREAK-037 has four example pairs:
  1. All-or-none: wrap between constraints, behind the colon, leading comma; a one-line clause stays.
  2. Joins first: `where T⏎ :`, `IDictionary<int,⏎string>`, `System.⏎IDisposable` all end on one line.
  3. A comment-held multi-line constraint wraps its clause, with the inner `string>` at 30.
  4. Comments and directives: comment behind the type parameter; comment or `#if` behind the colon; comment or `#if` in front of a comma; comment behind a comma.
- FMT-INDENT-032 has two pairs:
  1. Type, method, and extension block aligned; a later `where` stays at FMT-INDENT-017's column.
  2. Comment or directive behind the colon, comment in front of a comma, and the colon pushed down by a comment (colon at 12, constraints at 14).
- RH5110-004 has a negative block (comment behind the colon, comment in front of a comma: reported, no fix) and a positive pair (comment behind a comma: the fix is offered, and the result is aligned).
- Qualifiers with an example on each side:
  - "wrapped": FMT-BREAK-037 pairs 1–3.
  - "kept by a comment or directive" vs. "joined": FMT-BREAK-037 pairs 2 and 4.
  - "a gap the layout joins": RH5110-004's two blocks.
- An extension block needs C# 14. The repository uses `LangVersion` `latest`.

## Output questions

_None._

## Internal choices

- **Wording of Q8.** `00-lead.md` records Q8 only in summary ("withheld for inputs where the layout could only be reached by moving a comment, directive, or literal"). I read it as follows: RH5110-004 withholds the fix exactly when a comment, a directive, or disabled text stands in a gap that FMT-BREAK-037 joins. Those gaps are type parameter to colon, behind the colon of a wrapped clause, in front of a comma, and inside a constraint. A comment behind a comma or above a `where` does not withhold the fix.
  - The requirement does not name literals, because a constraint clause cannot contain one.
  - **Lead: check this reading against the input the maintainer saw.** If Q8's question showed different inputs, the requirement's gap list needs adjusting.
- **Unchanged FMT-BREAK-023 and FMT-INDENT-017.** I left both unchanged. The "every owner" scope sits in the new requirements instead of widening FMT-BREAK-023 (decision 4).
- **Q5's "inner line aligns inside the constraint".** I specified this only for lines that an existing requirement places (FMT-INDENT-015 for a later type argument), measured after the constraint moves (FMT-INDENT-030). Other inner lines held by a comment are not decided here, for example `System. // c⏎ IDisposable`. That case is the skeptic's follow-up 7 / 01's candidate 3, already in `00-lead.md`.

## Follow-up candidates

No new ones. Candidate 1 from `01-spec-author.md` is removed. Candidates 2–4 there and 5–7 from `02-skeptic.md` stand as listed in `00-lead.md`.
