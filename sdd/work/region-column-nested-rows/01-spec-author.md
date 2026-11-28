Role: spec author
Commit: 4a91817
Verdict: change doc written

Change doc: `sdd/changes/2026-11/region-column-nested-rows.md` (Status: Draft). Spec diff: `sdd/specs/formatter/indentation.md`, FMT-INDENT-010 and FMT-INDENT-023. No new requirement ID; the ledger is unchanged. No rule page changes (formatter only). `scripts/test.sh --project architecture`: 221 passed.

All "before" outputs come from `scripts/format.sh` (whole document) or `scripts/apply-fix.sh` on 4a91817. A second `--check` run over all fixtures reported `0 of 23 file(s) need formatting`, so every "before" is stable. The "after" outputs are not built yet; their unchanged lines match today's output.

Premise notes:
- The issue's "today" for item 21 is out of date. On the base, the elements of a nested row already stand one level deeper than the row's `{`. What still differs from a complex element initializer: the row's `}` stands with the elements instead of under `{` (change doc entry 8), a row whose `{` opens behind `}, ` (entry 9), and a row whose `{` is led by a comment (entry 10).
- Regions inside method bodies (entries 1–6) are reported by RH7303 (SHARED-TYPEREGION-005, "inside executable code"), so `AGENTS.md` counts them as rare. The maintainer decided the behavior anyway (thoenissen/Reihitsu#1370 round-9 comment: items 20 and 21, option b). Regions in a field initializer, a parameter list, or an attribute argument list are not reported by RH7303: SHARED-TYPEREGION-005 names neither position.
- The second comment's claim is wrong for a region inside a switch section whose `#endregion` stands among the statements; that case is already correct (fixture below). It is right for an `#endregion` after a section's last statement: see Output question 1.

## Changed requirements

- FMT-INDENT-010 — Region directives are indented like the code around them — changed:
  - New paragraph: inside the constructs of FMT-INDENT-012 and FMT-INDENT-022 to FMT-INDENT-027, the directive takes the column FMT-INDENT-006 gives a comment on its own line in the same position. That is the element's column, and in front of the closing delimiter, the column of the elements it closes. The innermost construct or block decides.
  - Added a coverage sentence: other constructs that are not block scopes are unspecified.
  - Clarification of today's behavior: "the statement column … between the statements of a switch section".
- FMT-INDENT-023 — Initializers without a keyword align to their target or their brace — changed:
  - A row is now laid out like a complex element initializer. A row is an array initializer whose parent is an array initializer. This includes the comment-led paragraph.
  - The other side of the qualifier: `new[] { 5 }` inside an array initializer keeps its `new` anchor (FMT-INDENT-022).
  - Added a `Why:` sentence and an example.

## Examples for the tests

- FMT-INDENT-010: the two new examples. The first covers object initializer, argument list, and switch expression; the statement-level region is the outside side. The second covers `#endregion` in front of `]` and `}`; the lambda block inside an argument is the innermost-block side.
- FMT-INDENT-023: the new rows example; the jagged `new[]` part is unchanged and is the other side.
- Change doc entries 1–10 are inputs with real "before" outputs, ready to become regression tests with an idempotency check, as the issue's "Done when" asks.
- Coverage-first hint: FMT-INDENT-010's first example (type, switch labels) is already covered.

## Output questions

Coverage of the change doc's entries by the maintainer's round-9 answer (thoenissen/Reihitsu#1370, items 20 and 21, option b):
- Covered: entries 1 (object and collection initializer), 2, 3, 4, 6 (Q-I1's input ends with `#endregion` in front of `};`), and 8 (nested rows of a declaration's array initializer, and by the same rule `new int[,]` and 3-D).
- Not explicitly covered, so they need the maintainer's answer:
  - the anonymous object in entry 1;
  - entry 5 (list, property, and parenthesized patterns; only agent comments name them);
  - entry 7 (parameters, tuples, bracketed arguments, attribute arguments: the same FMT-INDENT-012 alignment as argument lists);
  - entries 9 and 10 (row shapes the decision did not show);
  - the code-fix results (RH5305, RH5206, measured from the node).

The spec is written on including all of them.

### 1 · Formatter · an `#endregion` that closes a region opened among a switch section's statements

Reachable through the CLI. The issue's second comment raises it. Requirement: FMT-INDENT-010, which says "the label column between the sections of a switch statement". Today's output follows that literal wording. The `#region` stands among the statements, but the `#endregion` after the section's last statement belongs to the leading trivia of the next `case` label or of the switch's `}`. So the pair ends up four columns apart, which contradicts the requirement's `Why:` that a region takes the column of what it groups.

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                #region R
                M(2);
                break;
                #endregion
            case 2:
                break;
        }
    }
}
```

Before (base, the same with the `#endregion` in front of the switch's `}` instead of `case 2:`):

```csharp
            case 1:
                #region R

                M(2);
                break;

            #endregion // R

            case 2:
                break;
```

After, A: unchanged; the `#endregion` keeps the label column.

After, B (recommended):

```csharp
            case 1:
                #region R

                M(2);
                break;

                #endregion // R

            case 2:
                break;
```

A region that opens at the label column in front of `case 1:` keeps its `#endregion` at the label column in both options. That shape is FMT-INDENT-010's first example, and the fixture is the same with `case 2:`.

Why B: the region groups the section's statements, and its closing directive should stand with its opening one, as item 20 decided for the other constructs.

Without B, the run's specs and tests for everything else can still be written. With B, FMT-INDENT-010's switch-statement wording changes, and so does the code: to tell this shape from a region spanning whole sections, the code has to know where the matching `#region` stands.

## Internal choices

- Changed FMT-INDENT-010 instead of adding a requirement: same decision, same `Why:`. The alternative was a new FMT-INDENT-039.
- Stated the column by reference to FMT-INDENT-006 (the comment on its own line) instead of a column for each construct. The alternative was listing each construct's element column, which would be a second statement of the alignment.
- Named the constructs by the alignment requirements that place them (FMT-INDENT-012, FMT-INDENT-022 to FMT-INDENT-027). The alternative was only the cells the issue names. The visible consequence is listed as change doc entries 5 and 7.
- Narrowed FMT-INDENT-010's first sentence to the positions it names and added a coverage sentence. The alternative was keeping "the block indentation of the position", which would literally keep deciding the statement column for method chains and binary expressions.
- Recognize a row by its parent kind (an array initializer under an array initializer). The alternative was "any array initializer without `new`", which would also catch the declaration's own initializer.

## Follow-up candidates

### F1 · Formatter · blank line after an opening `[` or `(` in front of a `#region`, and in front of a closing `]` or `)` after an `#endregion`

Reachable through the CLI. This is the blank-line phase, outside the run. A `#region` directly after an initializer's `{` gets no blank line, and neither does an `#endregion` directly before its `}`. Behind `[` and `(`, and in front of `]` and `)`, a blank line is inserted.

Input:

```csharp
class C
{
    int[] F = [
                  #region R
                  1
                  #endregion
              ];
}
```

Real output (base):

```csharp
    int[] F = [

    #region R

                  1

    #endregion // R

              ];
```

The same happens in a parenthesized pattern `x is (` (blank after `(`, before `)`) and in an argument list `N(` / `)`. Compare `new P {⏎#region X⏎…⏎#endregion⏎}`, which gets no blank line after `{` or before `}`.

Readings:
- (a) intended.
- (b) `[` and `(` behave like `{`: no blank line directly behind the opening delimiter or in front of the closing one.

Recommendation: (b), for consistency with `{`.

### F2 · Formatter · region directive inside a wrapped method chain, binary expression, or query

Reachable through the CLI. After this change, FMT-INDENT-010 leaves these positions unspecified.

Input:

```csharp
        return a.Where(x => x > 0)
                #region Map
                .Select(x => x * 2)
                #endregion
                .Sum();
```

Real output (base):

```csharp
        return a.Where(x => x > 0)

        #region Map

                .Select(x => x * 2)

        #endregion // Map

                .Sum();
```

Readings:
- (a) the statement column, as today;
- (b) the column of the link or operand it stands in front of, as a comment there.

Recommendation: (b), by item 20's principle. These are rare: RH7303 reports them inside bodies.

## Fixtures (scratchpad, not committed)

`/tmp/claude-0/-home-user-Reihitsu/0e8a6c60-6f5e-59aa-82c2-08021193ae4c/scratchpad/orig/` holds the unformatted inputs, and `.../scratchpad/fx`, `fx2`, `fx3`, `fx5` hold the formatted outputs:
- `a_objinit`, `b_collexpr`, `c_switchexpr`, `d2_args`, `e_anon`, `f_listpat`, `g_parenpat`, `h_switchsection` (correct today), `i_nested`, `j_collinit`, `k_propPat`;
- `s1`–`s4` (switch-section shapes), `t1`, `t2` (closing delimiters), `n1`, `n2`, `m1`, `m2`, `r1` (rows);
- `.../scratchpad/fx4/p1.cs` (the FMT-INDENT-012 lists), `.../scratchpad/af/` (apply-fix inputs).
