Role: spec author
Commit: ce7b071
Verdict: spec written

This is repair round 3, the last one, and it includes the structural change of `00-lead.md` row 30. It also covers rows 29, 31, and 33, which hold auditor findings #7 and #8 and the RH5020 hint. The prompt named the four changes it wanted. It carried no suspected cause and no hoped-for verdict. I re-derived each change against a CLI built from `main` (741157c) and one built from the branch (ce7b071). Each fixture ran two passes. The fixtures are in the session scratchpad under `r25/`.

## Changed requirements

- FMT-BLANK-018 — A comment glued to a token belongs to the token's line — changed. The requirement now states one classification of a comment's position relative to the token behind it, with exactly two positions:
  - **On the token's line**: no line break outside the text of a comment stands between the comment and the token. So a delimited comment that starts on an earlier line and ends on the token's line is on it (SHARED-COMMENT-012, SHARED-BLANK-002). So is every comment in front of that comment on the line it starts on (`/* a */ /* x⏎ */ }`).
  - **On its own line**: every other comment.

  Two cases fall on their own line by definition. The first is a comment in front of the end-of-file token, which was the second reading of row 28 and is kept. The second is a comment in front of a token that the formatter moves off the comment's line behind a multi-line `*/` (FMT-BREAK-019, chain links). The comments on the token's line are glued when the first of them is an ordinary comment. A documentation comment that starts the token's line glues nothing, as before. The old paragraph "is not glued" (the first reading of row 28) is gone. The last example is replaced by one that shows both sides: a glued multi-line comment before a statement and before braces, a chain link, and the end of file. Uses gains FMT-PIPE-004, FMT-BREAK-019, and SHARED-COMMENT-012.
- FMT-BLANK-011 — changed. It applies to the first comment of the gap that is not glued, which is a comment on its own line or a documentation comment that starts the token's line. When the first comment is glued, FMT-BLANK-011 inserts nothing in the gap.
- FMT-BLANK-020 — changed. The trigger is now "an ordinary comment on its own line, as FMT-BLANK-018 classifies it". A new sentence gives `// c⏎⏎/* x⏎ */ }` to FMT-BLANK-018.
- Change doc `sdd/changes/2026-11/blank-line-sole-owner.md`:
  - The *What* bullet for FMT-BLANK-018 now describes the classification.
  - The FMT-BREAK-030 bullet names entry 22.
  - The RH5020 sentence is widened to every glued comment (row 33), and so is the RH5020 decision.
  - The intro of the visible changes names entries 20 to 22.
  - New entries 20, 21, and 22.
  - A new decision, "One classification of a comment's position", which includes the chain-link case.
  - Two rejected alternatives: the row-28 reading, and gluing only before the tokens FMT-BLANK-020 names.
  - Status stays Accepted. Entry 20 is covered by the guidance, and entries 21 and 22 are demanded by unchanged requirements.

`scripts/test.sh --project architecture`: 92/92.

## Visible output changes against `main` (re-derived)

**Restored to `main`'s output** (the branch at ce7b071 differs from `main` here; after the change it matches `main` again). These are the comments that are on the token's line under FMT-BLANK-018. No blank line is inserted above the comment, and the author's blank line is removed. This holds before:
- a method-body `{` and a type `{`
- a closing brace, including `/* a */ /* x⏎ */ }`
- the `{` and `}` of an object initializer
- the `{` of an assigned collection initializer (`L =⏎/* x⏎ */ {`)
- a collection-expression `]` and a list-pattern `]`
- a parenthesized-pattern `)`

These are fixtures `m1`, `m2`, `m5`, and `e20b`, and they cover all the failing members of audit finding #6.

**Changed against `main`, comment on a line with code, covered by the guidance (entry 20).** On `main`, FMT-BLANK-011 inserted a blank line above `/* x⏎ */ T`. After the change, the following happens:
- The blank lines are removed above a comment glued to:
  - an array-initializer `{`
  - a complex-element `{` (this is also answer 1A)
  - an assigned collection initializer's `}`
  - `else`, `catch`, `finally`, or a do-statement `while`
  - `/* x⏎ */ /** d */ {`
- No blank line is inserted, and the author's is kept, above a comment glued to:
  - a statement or a member
  - an argument-list `)`, an attribute list, a constraint clause, a parameter, or an argument
  - a binary or conditional operator, an array element, a query clause, a switch arm, or `=>`
  - the first link of a chain (`a⏎/* x⏎ */ .B()`)

These are fixtures `m3`, `m4`, `m5`, `m6`, `m8`, `m9`, and `e20`.

**Changed against `main`, decided by earlier answers:**
- A block statement after `}` gets the blank line above `/* x⏎ */ {` (answer 7, entry 3), and a block statement after `break` gets it too (answer 1, entry 8).
- In `A();⏎⏎/* x⏎ */ {`, the author's blank line is kept (maintainer revision, entry 4).
- `a++;⏎// c⏎⏎/* x⏎ */ }` gives `a++;⏎⏎// c⏎/* x⏎ */ }` (answer 4, entry 9).

**Unchanged, equal to `main`:**
- The statement rules still insert above a glued comment (`return`, `case` after `break`, a member or statement after `}`).
- A documentation comment that starts the line (`/** d⏎ */ {`, `/** d */ /* x⏎ */ {`).
- A comment in front of the end of the file.
- A comment in front of a chain link that FMT-BREAK-019 breaks off (`.C()⏎/* c⏎ */ .D()` gives `.C()⏎⏎/* c⏎ */⏎.D()`). The same holds for a prefix link `.C`.

**Entry 21** (row 29, auditor #7): `a++;⏎#pragma warning disable CS0168⏎⏎while (a > 0)`. `main` removes the blank line and the branch keeps it, as the unchanged FMT-BLANK-008 and FMT-BLANK-009 demand. Fixture `d1`.

**Entry 22** (row 31, auditor #8): `L =⏎⏎#pragma warning disable CS0168⏎{`. `main` removes the blank line above the directive and the branch keeps it, as FMT-BLANK-009 and FMT-BREAK-030 demand. Fixture `d2`.

## Examples for the tests

The examples come from FMT-BLANK-018 (the new last example), FMT-BLANK-020 (the new sentence), and change-doc entries 20, 21, and 22. Expected values to add or change:

1. **FMT-BLANK-018, last example (new).** Use the exact input and output from the spec. Add `[Requirement("FMT-BLANK-018")]` and `[Requirement("FMT-BLANK-011")]`.
2. **Multi-line glued comment before the removal tokens: blank line removed, none inserted.** Use `AssertRuleResult` under LF and CRLF, with a second pass. Cover each of these, with and without the author's blank line:
   - a method-body `{`
   - a type `{`
   - a method `}`
   - `/* a */ /* x⏎ */ }`
   - an object-initializer `{` and `}`
   - an assigned collection initializer's `{` behind `=`
   - a collection-expression `]`
   - a list-pattern `]`
   - a parenthesized-pattern `)`

   The input `void M8(int a)⏎/* x⏎ */ {` must be left unchanged. These are the members of audit #6.
3. **Entry 20 example.** The input is left unchanged. With the author's blank line, the line above `/* g` and above `/* k` is removed, and the line above `/* m` and above `/* s` is kept. Also cover, each with one small test:
   - `/* x⏎ */ catch`, `finally`, and do-`while`: the blank lines are removed.
   - the complex-element `{` and the assigned collection initializer's `}`: removed.
   - `/* x⏎ */ /** d */ {`: removed.
   - an argument-list `)` and the first chain link `a⏎/* x⏎ */ .B()`: nothing inserted.
4. **Statement rules above a multi-line glued comment.** `a.B();⏎/* x⏎ */ return;` gets the blank line above `/* x`, and so do `break;⏎/* x⏎ */ case 2:` and `}⏎/* x⏎ */ int I;`.
5. **Answered cases with a multi-line comment:**
   - `if (a)⏎{⏎}⏎/* x⏎ */ {` gets the blank line above `/* x` (answer 7).
   - `A();⏎⏎/* x⏎ */ {` is left unchanged, and `A();⏎/* x⏎ */ {` is left unchanged too.
   - `a++;⏎// c⏎⏎/* x⏎ */ }` gives `a++;⏎⏎// c⏎/* x⏎ */ }` (answer 4).
6. **Not glued, both sides:**
   - `/** d⏎ */ {` keeps its FMT-BLANK-011 blank line.
   - `/* x⏎ */ // c⏎⏎}` gets the blank line above `/* x` and loses the one above `}` (FMT-BLANK-020).
   - `TrailingCommentWithNoBlankLineGainsOneAtEndOfFile` is unchanged.
   - `ChainLinkBehindMultiLineCommentTests` is unchanged: its expected values (a blank line above the comment, the link on its own line) still hold under the chain-link case. Add `[Requirement("FMT-BLANK-018")]` to one of these tests, or add one test for the prefix link `.C`.
7. **Entry 21.** `a++;⏎#pragma …⏎⏎while (a > 0)` is left unchanged. Put it next to the do-statement case, `}⏎#pragma …⏎⏎while (a > 0);`, where the blank line is removed. Add `[Requirement("FMT-BLANK-008")]` and `[Requirement("FMT-BLANK-009")]`.
8. **Entry 22.**
   - `L =⏎⏎#pragma …⏎{` is left unchanged.
   - `L =⏎#pragma …⏎⏎{` loses the blank line between the directive and `{`.
   - `L =⏎#pragma …⏎{` is left unchanged.

   Add `[Requirement("FMT-BLANK-009")]` and `[Requirement("FMT-BREAK-030")]`.

**Existing tests whose expected value may change.** A scan for a multi-line comment that ends on a token's line found these. Their shapes are first-in-construct, so they need no blank line under either reading, and I expect no change:
- `CommentLedFirstTokenAnchorTests` (lines 102 and 151)
- `CommentLedLineAlignmentTests`
- `BracedSwitchSectionCommentLayoutTests` (lines 294 and 311)
- the RH5303 and RH5304 formatter tests

The test implementer should confirm this by running the full suites.

## Output questions

_None._ Every output this round changes falls into one of these groups:
- It restores `main`'s output: before braces, brackets, and pattern parentheses, the assigned collection initializer's `{` included.
- It is a comment on a line with code, which the guidance covers (entry 20).
- An earlier answer decides it (1A, 1, 4, 7, and the 2026-10-28 revision).
- An unchanged requirement demands it (entries 21 and 22).

The one case where row 30's wording, read literally, would give a different output is the chain link, covered under *Internal choices*. There the literal reading gives no stable output at all, so it is not an alternative the maintainer could pick.

## Internal choices

- **Chain link (needs the lead's attention).** A link that stands behind the `*/` of a multi-line comment does not start its line, so FMT-BREAK-019 (through `ChainLineBreakRewriter.EnsureLineBreak`) moves it onto its own line. In the output, the comment then ends its line. If the classification read only the input line, the blank-line phase would insert nothing in pass 1. In pass 2 the comment would be on its own line and would get the FMT-BLANK-011 blank line, which violates FMT-PIPE-004.

  I therefore wrote the classification by the line the token ends up on. Where the formatter breaks the line in front of the token behind such a comment, the comments of that line are on their own line. This keeps `main`'s output and today's branch output, and the tests in `ChainLinkBehindMultiLineCommentTests` stay unchanged. I probed every other token kind in `r25/m8.cs` (statements, members, braces, arguments, operators, clauses, switch arms, `=>`), and the line-break phase moves none of them off such a comment. Only the chain rewriter calls `IsBehindMultiLineCommentTail`.

  Implementation consequence: the blank-line phase must answer "will FMT-BREAK-019 break in front of this link?" from the chain and its trivia, which no phase between the two changes. The answer should come from one predicate that `ChainLineBreakRewriter` also reads, so the decision has one owner.

  Alternative: the literal "any part on the token's line" reading. Rejected because it is not idempotent.
- **Comments in front of a multi-line comment on its first line** (`/* a */ /* x⏎ */ }`) are on the token's line. The alternative was "own line, not ending its line", which would be a third position and would give `/* a */` an FMT-BLANK-011 line that `main` does not give it.
- **A documentation comment that starts the token's line** is classified as on the token's line but not glued, so FMT-BLANK-011 applies above it as before. This keeps entry 15's documented output and RH8303. The alternative was a third position.
- **The classification lives in FMT-BLANK-018**, and FMT-BLANK-011 and FMT-BLANK-020 point to it. The alternative was a new requirement. It would add an ID, while FMT-BLANK-018 already owns "glued", which the classification defines.

## Follow-up candidates

_None new._ One pre-existing observation, not in scope and not a candidate: SHARED-COMMENT-012 asks the formatter to break in front of any token that a requirement asks to start its own line behind a multi-line `*/`. Today only chain links are broken, and `MemberLineStartRewriter` documents that it leaves such gaps alone. This run does not change that, and the classification follows whatever the line-break phase does.
