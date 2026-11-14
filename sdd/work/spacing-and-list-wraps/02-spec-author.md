Role: spec author
Commit: 62e50b5
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's binding answers and decisions in `00-lead.md`.

## thoenissen/Reihitsu#1642

No spec change: FMT-SPACE-001 and FMT-SPACE-013 already require today's output (decision 2); the characterization test derives from FMT-SPACE-001's sentence on multi-line comments.

## thoenissen/Reihitsu#1635

Change doc: `sdd/changes/2026-11/list-wrap-by-elements.md` (`Status: Draft`). Spec diff: `sdd/specs/formatter/line-breaks.md`. Every "before" in the change doc was taken with `scripts/format.sh` at 62e50b5; scratch fixtures only, no repository file changed besides the three listed.

### Changed requirements

- FMT-BREAK-012 — A list is on one line or one element per line — changed:
  - multi-line is decided by the elements (two elements start on different lines, a line break in front of a comma included, or an element spans lines per FMT-BREAK-013); a line break only behind the opening delimiter leaves the list single-line, and it is joined;
  - in multi-line argument and attribute argument lists every comma ends the line of the element in front of it;
  - a list with a comment, directive, or disabled text between an element and a comma on a later line is left as written (no line break added between elements, no comma moved); a comment in front of a comma on the element's own line does not count.
- FMT-BREAK-011 — Parameter lists hug their declaration — changed: the comma rule yields to a list FMT-BREAK-012 leaves as written.
- Non-goal in `line-breaks.md` on argument lists — changed: only the closing parenthesis of an argument list stays a non-goal (#956); commas are FMT-BREAK-012's.

No new IDs; `requirement-ids.txt` and `sdd/specs/README.md` unchanged (no new code owner).

### Examples for the tests

FMT-BREAK-012: all five examples (W1 join `Foo(⏎a, b)`; leading commas in argument, attribute argument, two-element, and nested lists; the as-written exception for argument and parameter lists with its counter-example `Foo(1 /* one */, 2,⏎3)`; indexer split and indexer W1 join). Change doc entries 1–8, entry 8 being the RH5102 fix result on the issue's input. Existing test `IndexerParameterListLayoutTests.FirstIndexerParameterJoinsOpeningBracket` expects the old split and must flip to `int this[int a, int b] => a;`.

### Output questions

Entries 1–5 of the change doc carry the maintainer's answers. Two listed changes follow from those answers but were not shown to the maintainer; the lead decides whether they need asking:

- **Entry 6 — indexer wrapped only behind `[`.** `int this[⏎    int a, int b] => a;` was split on `main` (`int this[int a,⏎         int b]`), and becomes `int this[int a, int b] => a;`. It is the maintainer's definition applied to the one list kind that implemented the old "wrapped behind the opening delimiter" clause (#1644). Recommendation: treat as covered by the clarification; the alternative keeps an indexer-only exception to the definition.
- **Entry 7 — comment-held comma in a parameter list with another leading comma.** `void N(int a⏎, int b // keep⏎, int c)` gives `int a,⏎int b // keep⏎,⏎int c` on `main` (a comma alone on its line) and stays unchanged after. Rare class (RH5035), decision 7 ("otherwise leave as written"). Alternative: move the free comma only, `int a,⏎int b // keep⏎, int c`. Recommendation: leave as written, as the change doc says.

### Internal choices

- The as-written exception covers the whole list (no comma moved, no break added between elements), not only the held comma, and applies to every list kind of FMT-BREAK-012. Alternative: move the free commas only (see entry 7). Chosen because "leave as written" is the cheap refusal decision 7 names and needs no layout around an immovable comma.
- RH5102's spec is unchanged: its non-goal "the position of a comma … in an argument list" is about what it reports, which does not change; its fix result follows FMT-BREAK-012 through RH5102-003. Rule pages RH5101/RH5102 unchanged (nothing reported or fixed differs in the normal case beyond the formatter layout they already describe). Alternative: add an RH5102-003 example with a leading comma.
- One change doc covers both the definition change and the comma change, since they share FMT-BREAK-012 and the same mechanism.

### Non-goals (for this run)

- The closing parenthesis of an argument list (`Foo(1, 2, 3⏎)`) stays a non-goal, tracked in #956.
- No analyzer starts reporting leading commas in argument lists; the formatter may do more than the analyzers (P11).
- Tuples, collection expressions, bracketed argument lists, type argument and type parameter lists are not touched.

### Follow-up candidates

- RH5109's fix and the formatter disagree on `void N(⏎int a, int b)`: RH5109 reports it (parentheses on different lines, two parameters share a line) and its own layout (RH5109-003) gives `void N(int a,⏎       int b)`, while FMT-BREAK-012 now explicitly joins it to `void N(int a, int b)`. Pre-existing (the formatter joined it on `main` too); outside the run's issues. Candidate for an `agent-filed` issue on whether RH5109-001 should count a list as multi-line by its elements, as FMT-BREAK-012 now does.
