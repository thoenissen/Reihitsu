Role: spec author
Commit: f76edf4
Verdict: change doc written

My prompt carried the maintainer's answer (1C) and the lead's decisions, which I recorded. It carried no intended fix and no hoped-for verdict.

## Changed requirements

- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — changed as in `01-spec-author.md`. This round changed only the documentation example: its input writes `/** first */` on its own line behind `(` (`void M(⏎    /** first */ int a, int b)`), so the example pins only FMT-BLANK-011's own decision and not the move FMT-DOC-001 leaves undecided. The output block is unchanged. `scripts/format.sh` on f76edf4 gives the same *before* for the new input as for the written-behind input: the blank line, then `/** first */ int a,` and `int b)` aligned behind `(`.
- RH8303-002 — The first documentation comment behind an opening token is not reported — changed again. Its token list now equals FMT-BLANK-011's and RH5020-002's: `{`, `(`, `[` of any construct, and the `<` of a type parameter list, a type argument list, or a function pointer parameter list. A `<` that is an operator opens nothing, and a comma, an operator, or any other token keeps the report. The first parameter and the first type parameter are given as examples only. The *Why* now says that the formatter leaves no blank line there. I added one example (not reported): `///` behind the `<` of a type argument list, the `(` of an argument list, and the `[` of a collection expression. It notes that RH8303-001 does not cover these either. The spec does not reference FMT-BLANK-011 by ID, because a `Uses` row from an analyzer spec to a formatter requirement failed `SpecStructureTests.UsesListsExactlyTheReferencedRequirements`. It names "the same opening tokens the formatter exempts" in words instead.
- RH5020-002 — unchanged since `01-spec-author.md`.
- Change doc `sdd/changes/2026-11/first-comment-behind-opening-token.md`, now `Status: Accepted`:
  - *Decisions* gains "The maintainer's answer to the run's output question": 1C on 2026-11-16, with its scope and the visible output changes it decides.
  - "RH5020 and RH8303 change with the formatter" now states that the analyzer consequence was folded into question 1 and not asked separately (lead decision 1). It is decided by option C itself and by SHARED-DIAG-010 with FMT-BLANK-011's mirror statement.
  - "Every opening token" now notes why the generic `<` stays (skeptic choice 5, decided).
  - *What* and *Alternatives considered* now name the maintainer's choice where they said "this draft".
  - The *Why* cost argument is reworded (skeptic choice 1).
  - Entry 8 lists the RH8303 page edit.
- Rule page `documentation/rules/RH8303.md`, *Exemptions*: the first bullet is widened to "the first thing behind an opening parenthesis or bracket, such as the first element of a parameter list". The page keeps three positions, so "Three positions are deliberately not reported" stays true.
- No new ID. `requirement-ids.txt`, code owners, and `Uses` are unchanged. `scripts/test.sh --project architecture`: 153 passed.

## Examples for the tests

These are the examples from `01-spec-author.md`, with two changes:

- FMT-BLANK-011's documentation example now has an own-line `/** first */` input.
- RH8303-002 has a new example, not reported: `///` first behind a type-argument `<`, an argument-list `(`, and a collection-expression `[`.

`scripts/apply-fix.sh RH8303` on f76edf4 reports and fixes all three comments of that example today, which is the over-report this change removes. The fixture compiles. Change-doc entry 4 keeps the `/** first */` written behind `(`. That is where a test shows that the documentation phase's move survives while the blank line goes.

## Output questions

_None._ Question 1 is answered (1C). Question 2 was folded into it (lead decision 1).

## Internal choices

- Skeptic choice 1 (change-doc cost argument): taken. *Why* now rests C on consistency with the `{` exemption and on the collection-expression case. It states the check as "the token kind, plus the parent kind for `<`".
- Skeptic choice 2 (RH8303-002 token list): taken. Alternative: keep the parameter-list apposition. Rejected, because it would leave RH8303 reporting the formatter's output behind an argument-list `(`, a collection `[`, and a type-argument `<` (FC-013).
- Skeptic choice 3 (RH8303 rule page): taken. I widened the first bullet in the RH5020 page's words and listed it in entry 8. Alternative: leave the page as it is. Rejected, because its exemption would then be narrower than the analyzer's behavior in the parameter-list case the issue is about.
- Skeptic choice 4 (FMT-BLANK-011 documentation example): taken. The input is on its own line, and the written-behind case stays in change-doc entry 4 only.
- Skeptic choice 5 (keep `<`): decided by the lead and recorded in the change doc.
- RH8303-002 names the formatter in words, not by FMT-BLANK-011's ID. A cross-spec `Uses` row from an analyzer spec to a formatter requirement fails the spec-structure test, and RH5020-002 does not cite it either.
- FMT-BLANK-012 is left unchanged, as in `01-spec-author.md`. Its parenthetical "none when it is the first content of a block, a type body, or a switch section" covers only comments FMT-DOC-001 moves, and those always precede a type or member declaration. Alternative: replace the parenthetical with a reference to FMT-BLANK-011's exemption. Not done, because no input this run changes reaches it.

## Follow-up candidates

These are unchanged from `00-lead.md`:

- RH8303 over-reports. After this run, the over-report remains only behind a comma, where the formatter also writes the blank line.
- The indentation of the first element behind an uncovered opener, including the operator-wrap member.

Files:
- /home/user/Reihitsu/sdd/changes/2026-11/first-comment-behind-opening-token.md
- /home/user/Reihitsu/sdd/specs/formatter/blank-lines.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH8303.md
- /home/user/Reihitsu/documentation/rules/RH8303.md
