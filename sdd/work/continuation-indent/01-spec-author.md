Role: spec author
Commit: 8774731
Verdict: #953: change doc written; #1852: change doc written; #1896: change doc written; #1818: change doc written

Change docs (both `Status: Draft`):

- `sdd/changes/2026-11/continuation-indent.md` — #953, #1852 (return value), #1896, #1818; one cause: the column of a continuation line no alignment requirement places.
- `sdd/changes/2026-11/expression-body-join-across-blank-line.md` — #1852 (join across a blank line); a different cause (the structural transform), so its own doc.

The prompt carried no intended fix or verdict.

How the statements were established: every "before" is `scripts/format.sh` on `main` at 8774731; every new example's output was formatted with today's CLI and differs from today only in the continuation lines the requirement moves (checked by a diff of each block). A throwaway Roslyn probe (scratchpad, not committed) applied the continuation-line definition of FMT-INDENT-034 to every C# block of `sdd/specs/**` and `documentation/rules/**` (formatted first) and to the production sources under `src/`: it lists the lines that today sit at the block or statement column and would move. Production sources: 0 lines. Rule pages: 0 lines. Specs: the examples updated below; the flagged analyzer-spec blocks are analyzer inputs or code-fix outputs that do not run the formatter (RH2005, RH6001, RH6003, RH6021, RH5408, RH3001, RH5042, RH6013, RH6018, RH6019, RH6022, RH8303, RH5020, RH5034, SHARED-TOKEN-GAP example) and were left alone.

## thoenissen/Reihitsu#953

### Changed requirements

- FMT-INDENT-034 — A continuation line no other requirement places is indented one level deeper than the line its construct starts on — added (replaces the indentation spec's non-goal for the fallback, which is removed)
- FMT-INDENT-002 — Block indentation follows the scope depth — changed (one sentence: a continuation line takes FMT-INDENT-034's column unless another requirement places it)
- FMT-INDENT-013 — A line inside a list element aligns with the element — changed (query clauses now point to FMT-INDENT-035; Why text no longer names the statement column)
- FMT-INDENT-015 — Attribute lists and angle-bracket lists align to their first element — changed (a wrapped list of omitted type arguments: block column → continuation line; example `>);` 8 → 12)
- FMT-DOC-003 — changed (text: "placed like any other line (FMT-INDENT-002, FMT-INDENT-034)"; the `;` example is now one block that is left unchanged with `;` at 8)
- FMT-STRUCT-007 — example changed (`;` behind the conditional group in the created block: 8 → 12)
- FMT-STRUCT-021 — example changed (declarator/modifier line behind the head's line break: 4 → 8; the sentence "shown here as today's output" replaced by the FMT-INDENT-034 reference)
- FMT-BLANK-020 — example changed (`// e` and `{ })` behind `if (o is C`: 8 → 12)
- FMT-SPACE-001 — example changed (`;` behind `/// stray` written at 12 so the block is still "left unchanged" by the whole formatter)
- FMT-PIPE-017 — text changed ("The context's layout puts `b` in column 12, as a continuation line …")
- SHARED-BLANK-008 — example and text changed (`>` 8 → 12)
- SHARED-BODY-001 — example changed (RH3203 output: `;` 8 → 12)
- SHARED-ATTR-022 — example changed (RH5514 output on a lambda: `[B]` 8 → 12)
- SHARED-USING-009 — example changed (`.Text;` written at 4 so the block is still "left unchanged")
- RH5111-004 — text changed ("would put it in column 12")
- Uses rows: FMT-INDENT-034 added to `formatter/documentation-comments.md`, `formatter/pipeline.md`, `formatter/structural-transforms.md`, `shared/blank-line-spacing.md`. Ledger: FMT-INDENT-034, FMT-INDENT-035 inserted.
- No rule page changes (probe: no rule page shows a moved line). No code owner added: the requirement names no type; the implementer adds any new type to the indentation spec's code owners and `sdd/specs/README.md`.

### Examples for the tests

FMT-INDENT-034: seven examples (value/operand/pattern/`return` value; nested constructs — anonymous-object member, parenthesized pattern, switch-arm arrow; declarators — documented field kept whole, event field behind `#endif`, local; headers — base list `:`, modifier, `catch … when`; lambda bodies — plain and inside a collection expression, `with`; verbatim interpolated hole; the negative example of start lines that keep their column). FMT-INDENT-015 (omitted type arguments). The changed examples of FMT-DOC-003, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-SPACE-001, SHARED-BLANK-008, SHARED-BODY-001 (RH3203 fix), SHARED-ATTR-022 (RH5514 fix), SHARED-USING-009 pin values existing tests already assert; those tests change rather than new ones being added (`README.md`, *Coverage first*). Worth one test of both entry points (CLI and RH5305 fix) for the lambda inside a collection expression: `scripts/apply-fix.sh RH5305` gives `1,` at column 24 today, the CLI column 8; FMT-INDENT-034 makes both 32 (the case in the last-but-one comment of #953).

Qualifiers of FMT-INDENT-034 with both sides (FC-002): "starts on an earlier line" — `2` of `case 1 or⏎2:` (own pattern starts on the line, does not count; the `or` pattern does); "a part block indentation places by its scope" — negative example (attribute lists, token behind them, label and its statement, `else`, `do … while`) against the positive header example (modifier behind the first one, base list colon, `catch` filter); "its line begins inside text that started on an earlier line" — the verbatim hole example against every other example.

Instances of #953 no longer reachable, so no test and no requirement: comment behind `?`/`:` (FMT-UNSUP-008), comment inside a variable declaration `var x = // c` (FMT-UNSUP-004), anything behind or in front of `=>` incl. the `#pragma`/`#if` cases and the region between signature and arrow (FMT-UNSUP-012, FMT-UNSUP-013), a directive in a multi-declarator field (FMT-UNSUP-010), primary-constructor base list (joined, FMT-BREAK-039), `new[] { 1,⏎2 }` in a split argument list (already one level deeper on `main`). The issue's "Done when" items FMT-BLANK-013 clarification and showing FMT-STRUCT-007/023 refusal examples as code are dropped for that reason (FMT-STRUCT-023 is retired). The issue's own input example is now refused as unsupported (FMT-UNSUP-012).

### Output questions

**1 · Formatter — where a continuation inside a construct laid out to the right is measured from (FMT-INDENT-034)**

The maintainer's decision (#921 Q1) says "one level deeper than the line on which its statement or member starts"; its examples all have the construct starting on the statement's line. Two readings differ where it does not.

Input (today's output on `main`, the same before and after for reading B except that the moved lines take column 12):

```csharp
class C
{
    int M(int x, object o)
    {
        var w = new
                {
                    A // c
        = 1
                };
        var d = o is
                  (
        int or long
                  );

        return x switch
               {
                   1
        => 2,
                   _ => 0,
               };
    }
}
```

Before (`main`): as the input.

After, A (recommended) — from the line of the nearest construct that starts on an earlier line:

```csharp
                    A // c
                        = 1
...
                  (
                      int or long
                  );
...
                   1
                       => 2,
```

After, B — from the statement's line:

```csharp
                    A // c
            = 1
...
                  (
            int or long
                  );
...
                   1
            => 2,
```

Why A: B leaves the line left of the construct it belongs to, outside its own braces or parentheses. The spec is written on A.

**2 · Formatter — expression body converted across a blank line (FMT-STRUCT-006, #1852)**

Input:

```csharp
int Get() =>

    2;
```

Before (`main`): `int Get()⏎{⏎    return⏎    2;⏎}` (value in the column of `return`).

After, A (recommended): `int Get()⏎{⏎    return 2;⏎}`.

After, B: `int Get()⏎{⏎    return⏎        2;⏎}` (the break stays; the value is a continuation line).

Why A: a body converted to an expression statement already joins across the blank line on `main` (`void B() =>⏎⏎Console.WriteLine();` gives `Console.WriteLine();`), and the blank line carries no meaning. Same for the RH3202/RH3205/RH3206/RH3208 fixes. The spec is written on A.

**3 · Formatter — query clauses outside a list element (FMT-INDENT-035, #1896)**

Input / Before (`main`):

```csharp
var q = from x in arr
let y = x
select y;
```

After, A (recommended): clauses under `from`.

```csharp
var q = from x in arr
        let y = x
        select y;
```

After, B: the general continuation rule (one level deeper than the statement).

```csharp
var q = from x in arr
    let y = x
    select y;
```

After, C: unchanged.

Why A: FMT-INDENT-013 already aligns the clauses under `from` inside a list element. The spec is written on A.

**4 · Formatter — a wrapped list of omitted type arguments (FMT-INDENT-015)**

Input / Before (`main`):

```csharp
        var type = typeof(System.Collections.Generic.Dictionary<, // c
        >);
```

After, A (recommended): `>);` one level deeper (column 12), as any continuation line.

After, B: unchanged; FMT-INDENT-015 keeps its explicit block-column clause as an exception to FMT-INDENT-034.

Why A: FMT-INDENT-015's reason (no column to align to) is exactly the general rule's case, and an exception would be a special case. The spec is written on A.

All other visible changes listed in `continuation-indent.md` (including the `return` value of #1852 and the declarator of #1818) are applications of the rule the maintainer already decided (#921 Q1, #918 Q5, #913 Q2); the lead's batch can present them as confirmations.

### Internal choices

- The continuation-line definition is stated by cause (the token does not begin a part block indentation places by its scope) with the closed list of such parts as explanation; alternative: a contributor-style list of continuation shapes. The list was checked by the probe against all spec examples and the production sources (no start line misclassified: labels, `else`, `catch`, `finally`, `do … while`, attribute lists and the token behind them, accessors, enum members, embedded statements).
- A line that begins inside text started earlier (verbatim string, multi-line comment) takes the line on which that text starts; alternative: the line's literal whitespace as anchor (column 6 in the verbatim example). Chosen because literal whitespace is no column this phase sets.
- FMT-INDENT-013's query sentence now points to FMT-INDENT-035 instead of stating the rule twice.
- FMT-SPACE-001 and SHARED-USING-009 examples were rewritten so their "left unchanged" claim stays true for the whole formatter, instead of being turned into before/after blocks.

### Follow-up candidates

- Open issues whose "Before" this rule changes and whose recommended option is a different alignment: thoenissen/Reihitsu#1633 (base list wrapped behind its colon: column 0 → 4, their option B), thoenissen/Reihitsu#1634 (first element of an angle-bracket list / qualified name behind a comment: block column → one level deeper, their option B), thoenissen/Reihitsu#1905 (qualified name split at `.`: their option B), thoenissen/Reihitsu#1542 (content of a parenthesized or positional pattern: now one level deeper than the line of `(`; the maintainer's #921 Q1 already put parenthesized patterns under the general rule). Each needs a comment that its "Before" changed with this run and that an alignment it chooses later takes precedence over FMT-INDENT-034. Not a new issue.

## thoenissen/Reihitsu#1852

### Changed requirements

- FMT-INDENT-034 — added (covers `return⏎x` and `return // c⏎x`; see #953)
- FMT-STRUCT-006 — The converted body keeps the expression's trivia — changed: "an expression written on a later line than the arrow MUST join the created `return` keyword, also when blank lines stand between the arrow and the expression"; Why extended; example gains `int Third() =>⏎⏎3;` → `return 3;`

### Examples for the tests

FMT-INDENT-034 first example (`return⏎x;`); add `return // c⏎_v;` in a getter (issue text) as a second test. FMT-STRUCT-006 example `Third`; the RH3202 fix of the same input (`scripts/apply-fix.sh RH3202` gives `return⏎        2;` today).

### Output questions

Question 2 above (blank line). The value on the next line is the general rule (question 1 does not touch it: the construct starts on the statement's line).

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Changed requirements

- FMT-INDENT-035 — Query clauses align with the query's first token — added
- FMT-INDENT-013 — changed (see #953)

### Examples for the tests

FMT-INDENT-035 example (clauses including `let`, `where`, `select`, a clause continuation `let y⏎= x` at one level deeper than `let`, and `into z⏎select z` under the outer `from`).

### Output questions

Question 3 above.

### Internal choices

- The clauses behind `into` align with the `from` of the query expression that holds the continuation; a nested query (a query inside a clause) aligns with its own `from`.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Changed requirements

- FMT-INDENT-034 — added (third example: event field and local declaration with a declarator behind `#endif`)

### Examples for the tests

FMT-INDENT-034 declarator example (`D;` 4 → 8, `b = 2;` 8 → 12, the disabled `B,` unchanged); add the issue's own input (`int a,⏎#if X⏎b,⏎#endif⏎c;`) as a test.

### Output questions

None of its own: a direct application of the decided rule.

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-INDENT-034 and FMT-INDENT-035 have no covering test yet (expected; the test implementer adds them). `UsesListsExactlyTheReferencedRequirements` passes.
- Scope: one change touches every spec example that showed a continuation line at the old column (12 spec files). The implementer's change is the indentation phase's fallback plus FMT-INDENT-035 and the FMT-STRUCT-006 join; existing tests that assert old continuation columns across the formatter, code-fix, and CLI test projects will change with it.
