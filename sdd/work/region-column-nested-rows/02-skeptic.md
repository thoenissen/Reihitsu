Role: skeptic
Commit: 493c3a8
Verdict: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Every output below comes from `scripts/format.sh <dir>` (whole document, through the CLI) on 493c3a8, whose production code is that of 13b0ed2. A second `scripts/format.sh --check` run reported `0 of N file(s) need formatting` for each fixture set, so every "today" output is stable. Unless a fixture states otherwise, it is one method body in `class C`.

## Premise

**Reachable.** Every case is reached through the CLI (whole-document formatting). The code fixes that format such a node reach it too (change doc, *Code-fix results*).

**Item 20 (region column) is real.** The directive column comes from `LayoutComputer.SetDirectiveIndentation` (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs:466`), which uses `SyntaxIndentationUtilities.GetTriviaIndentLevel`: a block level, whatever construct the token belongs to. A comment in the same position takes the placed column of the following token, plus 4 in front of a closing brace, `]`, or a pattern `)` (`CommentIndentationContributor.AlignCommentsBeforeToken`). I confirmed the change doc's "before" columns on my own fixtures (entries 1 and 2 at member and statement level, below).

**Severity of item 20 is split.**
- *Inside method bodies (entries 1, 3–6, the bracketed-argument and tuple parts of entry 7, and the switch-section case):* RH7303 reports the directives (SHARED-TYPEREGION-005, executable code), so `AGENTS.md` counts these inputs as rare. The maintainer decided them anyway (round 9, items 20 and 21).
- *Member-level positions (a field's initializer or collection expression, attribute arguments, a base list):* these are not rare in that sense. RH7303 does not report them. RH7304, the counterpart layout rule, does report today's formatter output, and its fix moves the directives to the column of the region's first token, which the formatter then moves back. Fixture `field.cs`, formatted, then `scripts/apply-fix.sh RH7304 <dir>` (exit 0, `2 fixed` under LF and CRLF):

```csharp
class C
{
    int[] F = [

    #region R

                  1,

    #endregion // R

                  2
              ];

    P G = new P
          {
    #region X

              X = 1,

    #endregion // X

              Y = 2
          };
}
```

RH7304's fix turns `    #region R` / `    #endregion // R` into column 18, and `#region X` / `#endregion // X` into column 14. These are exactly the columns the change specifies. So for these positions the change removes an existing ping-pong between the CLI and RH7304. RH7304's fix writes trivia itself instead of calling the formatter, so SHARED-DIAG-010 does not formally cover it, and this is not FC-013 by the letter. It is still the strongest evidence for the premise.

**Item 21 (array rows) is real, but the issue's "today" is stale**, as the spec author found. Fixture `rows.cs`, today:

```csharp
        int[,] a = {
                       {
                           1, 2
                           },
                       {
                           3, 4
                           }
                   };
        int[][] j = {
                        new[]
                        {
                            5
                        }
                    };
        var n = new int[,]
                {
                    {
                        1, 2
                        }
                };
```

The elements already stand one level deeper. Only the row's `}` differs from a complex element initializer. The jagged `new[]` row (the other side of FMT-INDENT-023's new qualifier) already gives the spec example's output.

**Entry 10 (comment-led row) matches today's complex element initializer exactly.** Fixture `dict.cs`, today:

```csharp
        var e = new Dictionary<int, int>
                {
                    {
                        1,
                        2
                    },
                    /* c */ {
                                3,
                                4
                            }
                };
```

**Entry 9 (row behind `}, `) has no precedent in complex element initializers.** For a complex element initializer, the line-break phase never keeps `{ {` or `}, {`. The same fixture's `new Dictionary<int, int> { {⏎1, 2⏎}, {⏎3, 4⏎} };` is broken into one brace and one element per line. A row keeps that shape (change doc entry 9, "before"). So "laid out like a complex element initializer" for entry 9 means applying FMT-INDENT-023's brace anchor to a brace that does not start its line, which is the rule it already states for `int[] a = {`. The staircase in entry 9's "after" follows from that rule. It is still an output question (the spec author's list, 01-spec-author.md).

**The second issue comment is right.** In the switch-section fixture `sw.cs`, `#endregion` lands at 12 after `#region R` at 16. A comment `// d` in the same position (after a section's last statement, in front of the next `case`) also lands at 12, so today's output follows the comment rule.

## Mechanism and candidates

_N/A._ This is a behavior change.

The deciding code is a single site for directives (`SetDirectiveIndentation`) and a single site for comments (`CommentIndentationContributor`). Today the two agree in every position FMT-INDENT-010 decides: type members, statements, switch labels, the `}` of a type, a block, or a switch, and unbraced embedded statements (fixture `pos.cs`: `#region Emb` lands at 12, like a comment there). They disagree in every position outside a block scope. Fixtures `chain.cs`, `midelem.cs`, `pos.cs`, and `baselist.cs` give these columns today:

| Position | Comment column | Directive column |
|---|---|---|
| in front of `.Select` of a wrapped chain | 16 | 8 |
| in front of `+ b` of a wrapped binary expression | 15 | 8 |
| in front of `where` of a query | 16 | 8 |
| in front of `? 1` of a conditional | 20 | 8 |
| in front of a later base type (`class C : B,⏎#region⏎I`) | 10 | 0 |
| in front of `+ 2` inside the element `X = 1 + 2` of an object initializer | 24 | 8 |

Inside a lambda block passed as an argument, both are already at the lambda block's statement column (16), also when the input has the directive at 8 (fixture `pos.cs`, `Run(1, () => { #region Body ... })`). The spec example's first block is its input, not today's output, so this case is unchanged and needs no change-doc entry.

## Breaking inputs

Failure classes that reach the change: FC-002, FC-003, FC-005, FC-010, FC-017, FC-020/FC-021, FC-022, FC-027, FC-028, and FC-031 (test doc comments).

Coverage first: no existing test asserts a directive's column inside any of these constructs, or a row's `}` column. The blank-line tests below carry such directives in their inputs but assert only blank lines, and their doc comments say that no requirement decides the column. Once the spec states the column, those sentences are stale (FC-031). The test implementer updates them; their assertions do not change:
- `tests/Reihitsu.Formatter.Test/Regression/BlankLines/DirectiveBeforeEndRegionBlankLineTests.cs` (around lines 258–330)
- `tests/Reihitsu.Formatter.Test/Regression/BlankLines/ClosingBracketBlankLineTests.cs` (around line 122)
- `tests/Reihitsu.Formatter.Test/Regression/BlankLines/PositionalPatternClosingParenthesisBlankLineTests.cs` (around line 760)
- `tests/Reihitsu.Formatter.Test/Regression/BlankLines/GluedCommentBlankLineTests.cs` (around line 440)

Inputs the tests must cover. "None found" means no existing test pins the value.

1. Change doc entries 1–10, each with a second pass and under CRLF (FC-010, FC-017) — none found.
2. Member level: a field's collection expression and object initializer (fixture `field.cs` above). After formatting, RH7304 reports nothing (the counterpart check) — none found.
3. Closing delimiters, one each (FC-021): an initializer's `}`, the `]` of a collection expression or list pattern, the `)` of a positional pattern, the `)` of a parenthesized pattern (FMT-INDENT-026: four right of `(`), a switch expression's `}`, and an argument list's `)`. A comment in front of that `)` takes the `)` column (fixture `cmtclose.cs`: `// c` at 10 under `N(1,`), which FMT-INDENT-012 puts at the elements' column — none found.
4. Nesting (FC-027): a region inside a nested initializer inside an initializer; a region in a lambda block inside an argument (already correct, keep it); a region in front of the statement that holds the construct (keeps the statement column) — the lambda case is pinned by none found; the statement case is FMT-INDENT-010's existing first example.
5. Mixed trivia (FC-005): `// c` above `#region R` above an element, and `#region R` above a line that starts with `/* c */ X = 1,`. FMT-INDENT-006's second sentence gives the leading comment's column — none found.
6. A region directive in a disabled `#if` branch inside an initializer stays untouched (FMT-INDENT-010, -011) — none found for this position.
7. An element that is a multi-line raw string, followed by `#region` in front of the next element (FC-022; the column is read after the raw-string moves) — none found.
8. Node level against whole document (FC-028): the RH5305 and RH5206 fix results of the change doc — none found.
9. Rows: entries 8–10, `new int[,]`, three dimensions, a comment in front of a row's `}`, and the jagged `new[]` other side (FC-003) — the jagged side is not pinned, none found.
10. Not layout cases, because the precheck refuses them: a directive between an element and a comma on a later line (FMT-UNSUP-009), and a directive in front of a switch arm's `=>` or `when` (FMT-UNSUP-017). Do not write column tests for them.

The inputs of *Output questions* 1 and 2 depend on the maintainer's answers.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Indentation phase (directive column, array-row anchors) | must change |
| Line-break phase (rows keep `{ {` and `}, {`) | must stay |
| Blank-line phase | must stay (F1 is a follow-up) |
| Region formatting phase | must stay |
| Precheck / unsupported shapes | not involved |
| Code fixes that format such nodes (RH5101, RH5206, RH5304, RH5305, RH5306, RH5307, member- and type-level fixes) | output changes through the formatter; no code change |
| RH7304 analyzer and fix | must stay; after the change it agrees with the formatter for pairs wholly inside a construct |
| RH7303 | not involved |
| `Reihitsu.Core` (`RegionDirectiveUtilities`) | not involved |
| Rule pages, CLI options | not involved |
| Tests | new regression tests; stale doc comments in the four blank-line test files above |

## Refuse option

- **Leave the directive as written inside these constructs.** The change doc rejects this. It would also keep the RH7304 ping-pong at member level, so I agree with the rejection. The requested change is small by comparison: one directive-column source and one array-row anchor.
- **Declare the shape unsupported.** The shape would be an active `#region` or `#endregion` in the leading trivia of a token below the opening delimiter of an initializer, collection expression, pattern, switch expression, or FMT-INDENT-012 list. It would delete no special case: today no phase guards it, and the directive simply gets the block column. It would refuse files that people write, such as static data tables in a field initializer grouped by regions. A shape that deletes nothing is not declared (`AGENTS.md`), so this is not an option to escalate.
- **Array rows:** no cheaper option. The anchor already exists for complex element initializers.

## Output questions

### 1 · Formatter · how far the region-column rule reaches

The spec diff gives directives the comment column only inside a list of constructs and leaves every other non-block position "unspecified". "Unspecified" does not license a change, so in practice today's column stays in those positions. That costs a construct list in the code, and it gives two different columns to two directives in the same kind of position, depending on what encloses them. It also leaves the spec sentence ambiguous for a token inside an element: "the column of the construct's elements" (20) against "the column FMT-INDENT-006 gives a comment" (24) against "a wrapped binary expression ... unspecified" (8).

Input:

```csharp
        var p = new P
                {
                    X = 1
                        #region R
                        + 2,
                        #endregion
                    Y = 2
                };
        return a.Where(x => x > 0)
                #region Map
                .Select(x => x * 2)
                #endregion
                .Sum();
```

Before (`main`):

```csharp
        var p = new P
                {
                    X = 1

        #region R

                        + 2,

        #endregion // R

                    Y = 2
                };
        return a.Where(x => x > 0)

        #region Map

                .Select(x => x * 2)

        #endregion // Map

                .Sum();
```

After, A (the spec as written, with the in-element token stated as unspecified): unchanged from before. Only the listed constructs move, as in change doc entries 1–7.

After, B (recommended): a region directive takes the column of a comment on its own line in the same position, in every position:

```csharp
        var p = new P
                {
                    X = 1

                        #region R

                        + 2,

                    #endregion // R

                    Y = 2
                };
        return a.Where(x => x > 0)

                #region Map

                .Select(x => x * 2)

                #endregion // Map

                .Sum();
```

Under B, change doc entries 1–7 come out the same as under A. B also moves query clauses (16), conditional branches (20), wrapped binary operands, and later base types (10; that one also stops RH7304 from reporting it). In every position FMT-INDENT-010 already decides, B gives today's column (fixtures `pos.cs`, `sw.cs`, `lambda.cs`).

Why B: it is item 20's principle stated once, as one rule with no list of constructs. It decides follow-up F2 the same way, and the same directive no longer moves depending on what encloses it. What would change the pick: a wish to keep the run to exactly what round 9 decided. Then A, with the spec author removing the in-element ambiguity.

### 2 · Formatter · `#endregion` after a switch section's last statement (the spec author's question 1)

The input and both outputs are in 01-spec-author.md. I recommend **A** (unchanged: `#endregion` at the label column), against the spec author's B.
- A comment in the same position takes the label column today (`sw.cs`: `// d` at 12 before `case 3:`), and this change adopts the comment rule as the directive's column.
- B makes a directive's column depend on where its matching `#region` stands, a dependency the indentation phase does not have today.
- The class is rare: RH7303 reports every region inside a switch section.

Under question 1 B, option B here would be the one position where a directive leaves the comment rule.

### 3 · Entries the round-9 answer does not cover

These are the spec author's list: the anonymous object, entry 5, entry 7, entries 9 and 10, and the code-fix results. I have no different recommendation for them, with two observations:
- Entry 10 is exactly today's complex-element-initializer layout (`dict.cs` above).
- Entry 9's staircase follows from FMT-INDENT-023's existing brace anchor, because the line-break phase keeps `}, {` only for rows.

## Internal choices

- **If question 1 is answered A**, the spec text has to say what a directive in front of a token that neither starts an element nor is the closing delimiter gets. Recommend: unspecified, named next to the wrapped chain. It is internal only if the code then keeps today's column there.
- **Stale doc comments** in the four blank-line test files (see *Breaking inputs*). Recommend the test implementer updates the sentences and leaves the assertions alone.

## Issue goals

- *Item 21's "today"* (contents and `}` at the statement column) is stale. Only the row's `}`, the row behind `}, `, and the comment-led row change. Correcting the goal's description changes no output beyond the change doc.
- *"Each input has a regression test with the idempotency check"* is kept. Each test runs under CRLF as well (FC-010).
- *Item 20's construct list* is widened by the spec author to patterns, the anonymous object, and the FMT-INDENT-012 lists. That is visible and is already in question 3.

## Architecture findings

_None for the run._

Note on question 1: under A, the code needs a construct list that decides when a directive follows the comment rule. That is a second statement, next to `CommentIndentationContributor`, of which column a line-starting trivia in front of a token gets. Under B the directive reads the one existing statement.

## Follow-up candidates

- **F3 · RH7304 against FMT-INDENT-010 for regions that cross a construct's end, or that stand in a declaration header.** RH7304 aligns both directives of a pair to the region's first token. The formatter places each directive by its own position.
  - `[A(1,⏎#region At⏎2)]⏎#endregion⏎void N()`: after the change, the formatter writes `#region` at 7 and `#endregion` at 4. RH7304 wants 7 for both (fixture `field.cs`: RH7304's fix writes `       #endregion // At`).
  - A base-list region (`class C : B,⏎#region⏎I⏎#endregion⏎{`): the formatter writes 0, and RH7304 wants 10 (`baselist.cs`). Under question 1 B, `#region` moves to 10, but the `#endregion` in front of `{` takes `{`'s column, 0.
  - Separately, `RegionDirectiveUtilities.IsWithinElementBody` treats initializers, parameter lists, attribute arguments, and base lists as "between declarations" for RH7304. SHARED-TYPEREGION-005 names none of these positions. That is a spec gap of RH7304 (`sdd/specs/analyzer/RH7304.md`, RH7304-001).
  - Reachable through the CLI together with the analyzer. The structural cause is the region column stated in two places with different mechanisms (pair-based in RH7304, position-based in FMT-INDENT-010).
