# attribute-list-structural

| | |
|---|---|
| Date | 2026-11-01 |
| Run | issue #1492 |
| Pull request | #1551 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Splitting an attribute list with a line break that is not right after the comma (`[A⏎ , B]`, `[A,⏎⏎ B]`): drop the break (`[A]⏎[B]`) or keep today's output | A — drop it; the maintainer also asked the run to proceed on every recommendation without further stops | yes |

## Cost

- One stop (the output-question batch before the test implementer).
- Three repair rounds, the maximum: round 1 a change-doc status line; round 2 four audit findings plus an architecture finding (the merged-list shape decided per list and per owner); round 3 a missing test and stale text. A text-only cleanup after the third audit closed the last finding without a fourth round.
- The spec author ran six times (initial, skeptic amendment, code owners after the implementer, three repairs), always as a fresh agent.
- Most expensive: round 2. The first spec and implementation narrowed the merge to parameters and type parameters, following the issue's wording, although `main` merged `param:`/`typevar:` lists on every owner. The skeptic's matrix enumerated line-break shapes but not specifier-by-owner shapes.

## Decisions taken

25. The ones the maintainer may want to see: 2 (accessor attribute-list shape stays in the line-break phase, narrowing the issue's first "Done when"), 7/16 (one shared split and merge, new public Core members), and 25 (the plain text-only proof accepted although `--strict-docs` fails on a test-class summary).

## Suggestions

- When an issue moves an existing decision between components with "output unchanged", the skeptic could enumerate the moved decision's input axes from the old code's predicates (here: owner kind × target specifier × layout), not only from the issue's example. Round 2 came from an owner × specifier member nobody enumerated.
- A text that restates a requirement's scope (spec introduction, class summary, `AGENTS.md` row, change doc) drifted twice in this run. Role files could ask summaries to name the requirement ID instead of restating its owner set.
- `verify-text-only.sh --strict-docs` treats a public test class's summary as public API documentation; the lead playbook's text-only proof could exclude test assemblies.
