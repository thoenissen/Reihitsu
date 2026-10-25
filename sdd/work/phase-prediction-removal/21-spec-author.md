Role: spec author
Commit: 5f7396c
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It told me to apply the lead's decision on C1 and the wording points C2 to C7 from `20-skeptic.md`. The C1 decision (option B) is recorded at the end of `00-lead.md` and in `squad/decisions/2026-10-25-phase-prediction-removal-tab-before-node-target.md`; it reverses Q2 A. This round applies all of them.

The new examples were observed on 5f7396c with `scripts/format.sh` (document level) and `scripts/apply-fix.sh` (today's fix output), on fixtures in the scratchpad. Columns are 0-based.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` **stays `Status: Accepted`**. What and Why change only as these decisions require. What changed:

- **Affected requirements**: + SHARED-FIX-007, which this round clarifies.
- **What, node-level bullet**:
  - The node is laid out with its first token kept in its document column and every other line placed by the indentation phase.
  - A tab in the node's own leading whitespace counts as four columns. A tab in front of a node that does not start its line counts as one column, as the rules count it (C1 B, replacing "counting a tab in front of it as four columns").
  - "As document-level formatting does" becomes "in the columns the formatter gives it" (C7).
  - The RH5303/RH5304 bullet no longer says "also where the brace stands behind the last line of a multi-line comment" (C2).
- **Third table, row 3**: the complex element behind a tail is replaced by v1. Input: `var d = /* a` / `b */ new List<int>` / misplaced braces.
  - Today the fix puts the braces at 30 and `1` at 34, and RH5303 reports its own result (`no-progress`).
  - After this change: braces at 24 under `new`, `1` at 28. This equals document-level output, observed.
- **Decision "The node-level entry points lay out their node at its own column"**:
  - It now says the lines placed by the depth keep their place relative to the first token, as today (C3).
  - A new paragraph says how the first token's column is counted (C1 B, C2a, C6) and why SHARED-FIX-007 does not count the move that later formatting makes once it replaces the outside tab.
  - "Lay it out from the brace" becomes "lay it out as the formatter does" (C2).
- **Alternatives**: new rejected alternative "Count a tab in front of a node that does not start its line as four columns" (C1 A). It also mentions C1 C and C1 D.

## Changed requirements

- **FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — changed** (the requirement was added in round 18 and is not yet implemented):
  - **C3, reading X.** The phase lays out the node with the columns it gives it at its depth, shifted by the node's offset: the first token's document column less the column the phase gives that token at the depth. The first token stays in place, a line placed by the depth keeps its place relative to the first token, and an aligned line follows its anchor. Behind a multi-line comment's last line the offset is zero.
  - **C1 B, C2a, C6.** The first token's column is the number of characters in front of it, with two exceptions:
    - a tab in whitespace that belongs to the node and that the entry point does not keep counts as four columns;
    - a byte order mark at the start of the text counts as zero; FMT-RAW-005 still counts it for the raw-string move.
    - A tab in the whitespace in front of a node that does not start its line, and a tab inside a comment or a literal, count as one column.
  - **C4, C5.** The leading trivia FMT-PIPE-010 keeps is written back as it stands and is outside the requirement. So are the context node's lines that are not written back. The normative sentence now speaks of "every line they write back".
  - **C7.** The Why speaks of the columns the formatter gives the code. It also adds why outside whitespace is counted as the rules count it (SHARED-FIX-001).
  - **Examples** (the expected values come from document-level output observed on 5f7396c, or from the reading-X arithmetic):
    - v1 replaces the complex element behind a tail (C2). It is a value expression behind the tail, which SHARED-COMMENT-012 does not reach. Braces at 24, `1` at 28.
    - The raw-string example is unchanged.
    - New, through RH3202 in a two-space class (n4): `  int Get()` / `  {` / `      return 1;`, and the raw-string half with quotes, content, and closing marker at 13.
    - The tab prose now has two cases. Tabs in the target's own leading whitespace give 8 and 13. Two tabs in front of an equals-value target (n1) put `new` in column 10, with braces written at ten spaces.
  - `Uses` + SHARED-FIX-001, FMT-INDENT-007, FMT-RAW-005, FMT-RAW-007.
- **SHARED-FIX-007 — The changed code is laid out as the formatter lays it out — clarified (C1 B).**
  - New first bullet, "Whitespace in front of the changed code". A tab in the whitespace in front of the changed code on its first line is not part of that code. The changed code is laid out from the column that counts the tab as one. A later formatting run that replaces the tab, and moves the code with its line, is not a difference the requirement counts.
  - `Uses` + FMT-PIPE-017.
- **RH5303-005 — changed (example, C2).** The complex element behind a tail is replaced by v1: `{|RH5303:new List<int> … }|}` with misplaced braces, fixed to braces at 24 and `1` at 28.
- **RH5304-004 — changed (example, C2).** It is replaced by v2: `{|RH5304:L =` / `{` / `1` / `}|}` inside `new C` behind the tail, fixed to `L = {` at 28, `1` at 36, `}` at 32. This is the document-level output, observed. Today's fix puts the creation's braces at 30, as the move did.

No new IDs. `scripts/test.sh --configuration Debug --project architecture` gives 87 passed and 2 failed. Both failures are the pending ones from round 18:
- FMT-PIPE-017 has no test;
- 11 tests are still tagged RH5303-006 or RH5304-005.

## Examples for the tests

The FMT-PIPE-017 examples are seeds for these node-level tests:
- v1 through RH5303 (braces at 24);
- the raw-string target;
- n4 through RH3202, both halves;
- the target's own tabs (8 and 13);
- n1 through RH5303 (ten spaces). Also apply the fix a second time, which must report nothing (SHARED-FIX-001).

Further cases:
- **SHARED-FIX-007**: n1 again, as the case the new bullet covers.
- **RH5303-005 and RH5304-004**: the v1 and v2 examples.
- The skeptic's further inputs from `20-skeptic.md`:
  - h1 in a two-space document: every line of the literal at 14, `x` at 18;
  - the C2a input `var d /*→*/ = new List<int>`: braces at `new`'s character column;
  - the C4 input: a top-level `// c` at 0 stays above `  class D`;
  - a misindented statement led by a one-line comment, `     /* c */ Call(1,` / `2);`: the first token stays, and `2` goes under `1`.
- The tests that still tag RH5303-006 or RH5304-005 and expect the complex element behind a tail should, per C2, assert that node-level output equals document-level output rather than fixed columns, or use v1/v2. That shape changes when thoenissen/Reihitsu#1382 lands.

## Open questions for the maintainer

_None._ C2 to C7 had one reading each, and I applied it.
