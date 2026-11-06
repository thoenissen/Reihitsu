Role: spec author
Commit: 65882555
Verdict: #1257: nothing to do; #1279: spec written; #1281: nothing to do (RH3106-003 already decides finding 10); #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Repair round 3 after audit 3, carrying out lead rows 36–39 (`00-lead.md`) for auditor findings 10–13 and hint 3 (`20-auditor.md`). Bug-fix lane: every edit is a clarification of RH3004 or a decision on recommendation under the maintainer's standing instruction. No change doc, no new requirement ID, no rule page change (a withheld fix narrowed, and layout corner cases; the normal case is unchanged). My prompt restated the lead's decided rows and named no intended fix, cause, or verdict beyond them, so I had nothing to set aside.

## thoenissen/Reihitsu#1279

### Changed requirements

- RH3004-006 — The lambda is laid out as the formatter lays it out — clarified (row 39, finding 13). The layout paragraphs are replaced, not extended, by one rule:
  - Every non-blank line of the block gets exactly one outcome, decided on its source text alone.
  - **Stays** (keeps its columns): a line that starts inside a verbatim string literal begun earlier; a non-`#region`/`#endregion` directive line in column 0; a line that holds no code whose first comment starts in column 0 (also in disabled text); a line that holds no code and continues a comment whose first line stays.
  - **Moves**: every other line — every line that holds code (active, or non-comment text in disabled text), also behind a leading comment in column 0; a code-free line whose comment starts elsewhere or continues a moving comment; a directive not in column 0; `#region`/`#endregion` in any column.
  - The shift is measured on the first statement line, from its first character (also a leading comment). That line holds code, so it always moves. A block without a statement is measured on its first moving line.
  - Both leftmost-line paragraphs (shared brace line, own-line block crossing column 0) count only moving lines. A line that stays is never measured or counted.
  - The Why gives the closing argument: the line that sets the shift always moves by it.
  - The old exception list in the shift sentence, the separate "directive in column 0" and "comment in column 0" sentences, and "lines that keep their columns do not count" are gone, folded into the two lists.
- RH3004-007 — retitled "A multi-line raw string literal in a block body withholds the fix" — clarified (row 38, finding 12). The fix is withheld only where it keeps the block as a block body. Where it writes an expression body (single `return` with an expression, no comment or directive), a multi-line raw string does not withhold it: the formatter lays out the expression body (FMT-RAW-001, FMT-RAW-002) and the literal keeps its value. `Uses` gains FMT-RAW-001 and FMT-RAW-002.

### Examples for the tests

- RH3004-006, new:
  - the audit's input: `/* note */ Console.WriteLine(x);` in column 0, then `Console.WriteLine(x + 1);` in column 12, braces in column 8. Output: braces at 24, the first line at 28 (shift 28, measured from the comment in column 0), the second at 40. This is the output `2284decb` gave.
  - a block comment over two lines starting in column 0 (both lines stay at 0 and 3) next to one starting in column 12 (both lines move by 16, to 28 and 31). Each comment is one side of "continues a comment whose first line stays"; the test implementer may split it into two tests.
  - The other side of "holds code" is the existing `// note` column-0 example (a code-free line stays).
- RH3004-007, new: `Func<string> f = delegate () { return """⏎a⏎  b⏎"""; };` becomes `Func<string> f = () => """` with the content and closing quotes in column 31 (the opening quotes' column), the value unchanged. It replaces the refusal pinned by `FixWithheldForASingleReturnOfAMultiLineRawStringLiteral`. The block-body example (`Console.WriteLine("""…""")`) stays withheld.

### Output questions

_None._ The standing instruction applies.

### Decided on recommendation

1. **RH3004 · Use lambda syntax — a statement line that starts with a comment in column 0** (row 39, finding 13). The line moves and sets the shift from its first character: output as above (28 and 40). Alternative: measure from the first token behind the comment (comment at 17, statement at 28, the second line at 29). Rejected: it splits one line's measurement from the line's own first character and adds a line kind instead of closing the class; neither alternative gives a clean layout for this odd source, and the chosen one is the one row 39 and the auditor's simplest option name.
2. **RH3004 · Use lambda syntax — continuation lines of a comment that starts in column 0.** They stay with the comment's first line (the `/* Console.WriteLine(x + 1);⏎   Console.WriteLine(x + 2); */` example). Before, they moved by the shift while the first line stayed, splitting one comment across columns (the round-2 internal choice assumed the comment's first line moves). Every line kind needs exactly one outcome, and a comment's lines belong together. **This changes the implementation**: `StaysInColumnZero` today does not keep such a continuation line (`20-auditor.md`, *Checked and holding*).
3. **Hint 3 — a moved line landing in the brace column or left of it, own-line block.** Kept as decided: the shift applies unless a moving line would cross column 0; the spec now says "in the column of the braces or left of them" explicitly. Alternative: lift the block whenever a moving line would land left of the statement column, as behind a shared brace line. Rejected: in an own-line block the author placed every line, and lines written left of the statements (an outdented label or comment) are common; lifting would push the statements right for each of them, the effect row 33 removed for column-0 comments. Behind a shared brace, the first statement's column is no line start the author chose for later lines, which is why that paragraph measures differently; the Why says so. No existing example changes (the column-8 comment example already lands left of the braces).

### Internal choices

- RH3004-007 keeps its ID and is narrowed in place rather than retired and replaced: the requirement's subject (multi-line raw strings withhold the fix) stays, only its scope narrows.
- The disabled-text classification stays textual ("anything other than whitespace and comments"), as before; the stay/move rule applies to it unchanged.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1281

Rows 36/37 (findings 10, 11): RH3106-003 already decides finding 10's members. Its normative sentence is an outcome — no delegate type is the anonymous method's target, and it converts to its own natural type — and its first listed place is "the initializer of an implicitly typed local (`var`)", with no condition on the statement that holds the declaration or on later uses of the local. `if (b) { var a = delegate () { }; a(); }`, the same in a lambda body, in top-level statements, and the other shapes the auditor lists are not reported by the spec as it stands; the later use is not part of the condition. No clarification written. Tests derive from the first bullet of RH3106-003 with the auditor's shapes (one in an `if` block, one in a lambda body, one in top-level statements, each with the later use). Finding 11 is internal (cost) and needs no spec text.

## thoenissen/Reihitsu#1257, #1282, #1417, #1422

Nothing to do: no audit-3 finding touches them.

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md` only. `requirement-ids.txt` unchanged.
- `scripts/test.sh --project architecture`: 115 passed, 0 failed.
- No rule page changed (`documentation/rules/RH3004.md`).
