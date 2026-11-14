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

## Amendment after 03-skeptic (decisions 8–13)

Commit the amendment was derived from: 1dcd547. Verdict unchanged: change doc written.

### Changed requirements (amended)

- FMT-BREAK-012 — changed further:
  - the whole-list exception is gone. A comma that a comment, a directive, or disabled text keeps off the line of the element in front of it stays at the start of the line of the element behind it, and every other comma and element is laid out as usual — the per-gap refusal of FMT-BREAK-037 (decision 10);
  - a comment behind a leading comma, on the comma's line, moves with the comma and stays behind it: `Foo(1⏎, /* c */ 2, 3)` → `Foo(1, /* c */⏎2,⏎3)`, `Foo(1⏎, // c⏎2, 3)` → `Foo(1, // c⏎2,⏎3)` (decision 11);
  - whether a comma is kept off its element's line counts line breaks as SHARED-BLANK-008 does; `Foo(1 /* a⏎ b */, 2,⏎3)` keeps nothing (decision 12);
  - the third example now shows the per-gap outputs (parameter list N, the comment- and directive-held argument lists, both moved-comment cases, and the block comment on the element's line).
- FMT-BREAK-011 — the comma exception now reads "except a comma that a comment, a directive, or disabled text keeps off that line (FMT-BREAK-001, FMT-BREAK-012)".
- Skeptic 03's point D (what a whole-list hold still gets) is moot with the per-gap form.
- Deleting the indexer-only opener signal (decision 9) is internal and is not in the spec or the change doc.

### Change doc (amended)

`What` and `Decisions` describe the per-gap refusal and the moved comment; the whole-list form moved to `Alternatives considered`. Entries:
- 7 — held comma next to free ones: `N`, `O`, and three argument lists (`Foo(1⏎, 2 // keep⏎, 3)`, `Foo(1, 2⏎, 3 // keep⏎, 4)`, `Foo(1, 2⏎, 3, 4 // keep⏎, 5)`). `O` and the argument lists were unchanged on `main` and now change.
- 8 — comma held by a directive.
- 9 — comment behind a moved comma, block and `//`.
- 10 — RH5102 fix on the issue's input and on `Foo(⏎1⏎, 2, 3)` (both no progress before).
- 11 — RH5101 fix on `Foo(⏎1⏎, 2, 3)` (before `Foo(1⏎, 2, 3)`).

The "before" outputs of entries 7–11 are from 01/03-skeptic and my own `scripts/format.sh` runs on `main`-equivalent commits; the "after" outputs are the requirements' outputs, not observed.

### Examples for the tests (amended)

FMT-BREAK-012's third example replaces the old as-written example; change doc entries 7–11 add the fix results and the held/moved comma cases.

### Output questions (amended)

_None._ Entry 6 is covered by the clarification (decision 8); entries 7–11 follow from answer C and from decisions 10–13.

### Follow-up candidates (amended)

Unchanged from above; skeptic 03 adds two that the lead holds (RH5107 fix vs formatter on a comment behind a leading comma; no rule for leading commas in argument lists).

## Amendment after 04-test-implementer (decisions 14 and 15)

Commit the amendment was derived from: e518264. Verdict unchanged: change doc written. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions 14 and 15.

### Changed requirements (amended)

- FMT-BREAK-012 — clarified (decision 14): a held comma that stands alone on its line (the element behind it starts on a later line) keeps the line break behind it as written; it is not joined to `, int b`. A held comma with the element behind it on its line keeps that element there (`N`). The third example gains method `P` (`int a // keep⏎,⏎int b`, unchanged) as the case on the other side of the condition, and its caption says "alone on it when it was written alone".

### Change doc (amended)

`What` (held-comma bullet) and `Decisions` (*Only the held gap is left as written*) state the lone-comma case. No visible-output entry: the case is unchanged on `main` (both existing tests pass on 144e6fe), so nothing new reaches the maintainer.

### Examples for the tests (amended)

FMT-BREAK-012, third example, method `P`. The existing tests `ParameterContinuationLineAlignmentTests.CommaAloneOnItsLineStaysAligned` and `CommaAfterDirectiveHeldFirstParameterStaysAligned` match the spec as written and may be tagged `[Requirement("FMT-BREAK-012")]` by the test implementer; they need no change.

### Other (decision 15)

`03-skeptic.md` line 24 now names the earlier run's lead notes in words instead of a path under the squad notes folder; meaning unchanged. `scripts/test.sh --project architecture --filter "FullyQualifiedName~SquadNoteReferenceTests"` passes (3/3).

### Output questions (amended)

_None._

### Internal choices (amended)

- Stated the lone-comma case as part of FMT-BREAK-012's held-comma paragraph rather than as a new requirement ID. Alternative: a separate requirement; rejected as it is a qualifier of the same refusal.
