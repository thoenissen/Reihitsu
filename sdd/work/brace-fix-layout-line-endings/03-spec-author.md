Role: spec author
Commit: 57e86fb (amendment of c14bb8c; the one "Before" below measured on this tree with `scripts/apply-fix.sh`, LF and CRLF, fixtures in the session scratch directory)
Verdict: spec written

Repair-style amendment: the specs now state the maintainer's batch-1 answers 1–5 and the lead's decisions 5–9 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## thoenissen/Reihitsu#1416

### Changed requirements

- SHARED-BRACE-002 — The fix wraps the body in braces — clarified again:
  - Answer 1 (C): it splits by where the body starts. A body on the line of its `if`/`else` is one line and moves below `{`, four columns right of the braces. A body below its owner keeps every line in its column, continuation lines included. That sentence is scoped to a first token one level (four columns) right of the braces; see *Output questions*.
  - Answer 2 (A): code behind the body on its last line starts its own line below `}`, in the column of `}`. The whitespace in front of it is dropped. A comment behind the body stays inside the block.
  - Decision 5: an `else` of the same `if` behind the body stays behind the new `}`, with one space.
  - Answer 3 (A): blank lines directly below the owner's line are removed. From the first comment or body line on, every line stays. Measured on 57e86fb: `if (a)⏎⏎// c⏎⏎M(a, b);` → `{⏎// c⏎⏎M(a, b);⏎}`. This is the same as `main`, so nothing new is decided.
  - Answer 4 (A): whitespace at the end of the body's last line stays, and nothing is written behind `}`.
  - The whitespace in front of each brace is a copy of the whitespace in front of the owner line's first token.
  - The line-ending paragraph changed. Created breaks take the detected line ending. Kept breaks are moved code. The withhold is in SHARED-BRACE-003. The old lone-`\r` sentence ("created breaks are `\n`") is gone.
- SHARED-BRACE-003 — The fix is withheld where it cannot wrap the body as written — added. Five rare classes, each with an example on both sides (FC-002):
  - a body on the owner's line that spans several lines, a delimited comment behind it that continues onto a later line included (answer 1C);
  - other code in front of the owner on its line, other than `else` (for `else if`) or `}` directly before that `else` (decision 7);
  - a directive or disabled text between the header's last token and the body (answer 5, decision 7);
  - a tab in front of the owner line's first token (decision 7);
  - another line ending in the code from the header's last token through the line break that ends the body (decision 8, SHARED-FIX-008 second paragraph). As a result, an all-lone-`\r` document never gets the fix.
- Non-goal "brace column for an `if`/`else` behind other code" removed: that case is now withheld.
- SHARED-FIX-007 (*Moved code*) — clarified with two sentences. SHARED-BRACE-002 decides which body lines move. The brace fixes withhold for a tab in front of the owner's line (SHARED-BRACE-003).
- Rule pages of RH5405–RH5407 are unchanged: only withheld fixes and corner cases changed. The RH5406 page's example has its body below the `if`, which still gets the fix.

### Examples for the tests

- SHARED-BRACE-002: every example, including the new ones:
  - an RH5406 body below its owner keeps its continuation column;
  - `else` behind the body;
  - a statement behind the body (`/* c */ b++;`), in both body positions;
  - blank lines and a comment between the header and the body, with and without the comment;
  - trailing whitespace, with and without a comment, in both positions, for RH5405, RH5406, and RH5407;
  - LF and CRLF.
- SHARED-BRACE-003: every example. Each one is a "no code fix is offered" case with its other-side twin:
  - the multi-line comment twin;
  - `if (b) if (a)`, whose outer body gets the fix and whose inner body gets it after;
  - the `else` body, which gets its fix after the `if` body is wrapped;
  - `#pragma` and `#region` between the header and the body versus behind the body;
  - the tab;
  - an LF behind `if (a)` versus an LF on a line above, in a CRLF document;
  - an all-lone-`\r` document. This replaces the existing RH5406 lone-`\r` expectations, which assumed the fix is offered.
- Fix All (FC-008): `if (a) M(a, b); else M(a, b);` and `if (b) if (a) M(a, b);`. Each needs two iterations, because the second body becomes fixable only after the first is wrapped.

### Output questions

**1 · RH5405 · Braces must not be omitted — a body below its `if` that is not indented one level deeper**

This touches SHARED-BRACE-002 (body below its owner) and SHARED-FIX-007 (*Moved code*). Answer 1 says "a body below its header keeps its lines". The question it answered showed only bodies one level deeper than the `if`. For a misindented body, "keeps its lines" (A) and "indentation follows the new nesting" (B, the uniform shift of SHARED-FIX-007) give different output. The spec covers only the one-level case until this is answered. The tests for every other case can be written now.

Input:
```csharp
        if (a)
          M(a, b);
```

Before (`main`, measured on 57e86fb):
```csharp
        if (a)
        {
            M(a, b);
        }
```

After, A (recommended): the body keeps its line and column.
```csharp
        if (a)
        {
          M(a, b);
        }
```

After, B: the body moves to one level inside the braces, and every later line of a multi-line body moves by the same shift. The output is the same as `main` here. For `M(a,⏎    b);` at column 10, `b` moves two columns right. A left shift withholds the fix where a line has too little whitespace, or where a multi-line string or comment would have to move, as RH7004 does.

Why A: it is the maintainer's own wording for answer 1, it needs no shift or new withholds, and the indentation rules still report the misindented line.

### Internal choices

- The withholds are a new requirement, SHARED-BRACE-003, instead of more paragraphs in SHARED-BRACE-002. Alternative: keep everything in 002. One heading per requirement keeps the test tags precise.
- "Other code in front of the owner" counts a comment, as in `/* x */ if (a) M();`, so the fix is withheld there too. Alternative: count tokens only, which would need a brace column for a line that starts with a comment. Rare.
- A comment between `}` and `else` (`} /* c */ else M();`) withholds the fix ("only whitespace between them"). Rare.
- The span of the line-ending check is from the end of the header's last token through the line break that ends the body's last line. That matches the span SHARED-FIX-008 calls "the code it moves or rewrites", trailing trivia included.

### Issue goals

- "Loops": dropped, as in 01. SHARED-BRACE-001 does not check them.
- "keeps the wrapped statement's layout with a regression test per rule": kept, narrowed by answer 1C and SHARED-BRACE-003.

## thoenissen/Reihitsu#1388

### Changed requirements

- SHARED-FIX-008 — second paragraph clarified (decisions 8 and 9). The list of fixes that move code without the formatter now names the brace fixes (SHARED-BRACE-002/003) and the RH7309 fix (RH7309-007). The RH5405 example's lone-`\r` sentence now says "not offered" (SHARED-BRACE-003) instead of "creates `\n`". `Uses` gains SHARED-BRACE-002, SHARED-BRACE-003, RH7309-007.
- RH7309-007 — The fix is withheld when a moved block holds another line ending — added. The check covers the blocks whose place changes, from the `#region` line through the line break ending the `#endregion` line. A block that keeps its place, and the code between blocks, do not withhold the fix. All-lone-`\r` documents are withheld wherever a block moves. `Uses` gains SHARED-FIX-008 and FMT-PIPE-007.
- RH5011-004 and RH8304-004: unchanged since c14bb8c. They stay under the first paragraph (they create a break, they move nothing).

### Examples for the tests

- RH7309-007:
  - an LF behind `private int _a;` in the moved `Fields` block of a CRLF document: withheld;
  - the same LF behind `_between`, which stays between the blocks: offered, all other breaks CRLF;
  - an all-lone-`\r` document: withheld.
- RH5011-004 and RH8304-004 as in 01.
- SHARED-BRACE-003's line-ending bullet for the brace-fix instances.

### Output questions

_None._

### Internal choices

- RH7309-007 checks only the blocks that change place, not every block. Alternative: check all top-level blocks, which is slightly simpler, but it withholds where a foreign break lies in a block the fix does not move. The fix today replaces every block span, including those that keep their slot, with identical text.

## thoenissen/Reihitsu#1627

### Changed requirements

None of its own. SHARED-BRACE-002 now states answer 4: the whitespace stays at the end of the body's line, and nothing is written behind `}`. The example covers the shapes with and without a comment and on the owner's line; the text names RH5406 and RH5407.

### Examples for the tests

The SHARED-BRACE-002 whitespace example, for each of RH5405 (body below, body on the `if` line, with and without `/* x */`), RH5406 (body below), and RH5407 (`else` body), in LF and CRLF. A `// x···` comment keeps its spaces as comment text.

### Output questions

_None._

## thoenissen/Reihitsu#1142

### Changed requirements

- SHARED-ATTR-017 — clarified (decision 6, #1109 Q1). The spaces-only coverage sentence is removed. Each further list starts behind a character-for-character copy of the whitespace at the start of the reported list's line, a tab included, and the fix neither replaces nor counts a tab. A new tab example (`→[A]` / `→[B]`) was added, with a sentence for a tab plus four spaces.
- Rule pages are unchanged.

### Examples for the tests

The new SHARED-ATTR-017 tab example, and all the earlier ones (see 01).

### Output questions

_None._

## Whole run

### Checks

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 of 51 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for the new RH7309-007 and SHARED-BRACE-003. It goes green once the test implementer tags tests with `[Requirement("RH7309-007")]` and `[Requirement("SHARED-BRACE-003")]`. `UsesListsExactlyTheReferencedRequirements` passes.

### Notes for the test implementer

- The existing lone-`\r` tests of RH5406 (and any brace-fix test that expects a fix in a lone-`\r` or mixed document) contradict SHARED-BRACE-003 and become "no code fix is offered" tests.
- Tests that pin today's whole-`if` formatting become wrong under SHARED-BRACE-002: column 13 for `else if`, `else` moved off `}`, the other body braced, the inserted blank line, and the reformatted header or body.

### Follow-up candidates

_None_ beyond those already in `00-lead.md`.
