Role: skeptic
Commit: 18233f0
Verdict: #953: output question (with spec amendments); #1852: output question; #1896: output question; #1818: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Fixtures were run with `scripts/format.sh` (CLI, whole document) and `scripts/apply-fix.sh` (code fix, LF and CRLF) on the tree at 18233f0, which holds `main`'s code. Columns below are 0-based, counted in spaces.

## thoenissen/Reihitsu#953

### Premise

Real, and a caller reaches it: whole-document formatting (CLI) puts every continuation line that no alignment contributor owns at the block column. Reproduced:

```csharp
// input (inside a method)
        return
            2;
        var p = o is
            not null;
        System.Func<int, System.Func<int, int>> f = x =>
            (y) => y;
        var b = from v in arr
                select v;
        var r = o switch
        {
            int i
                when i > 0 => 1,
            _ => 0,
        };
```

`scripts/format.sh` puts `2;`, `not null;`, `(y) => y;` and `select v;` in column 8, and `when i > 0 => 1,` in column 8, left of its arm in column 20. The mechanism is in the code: `LayoutComputer.ComputeBlockIndentation` gives every first-on-line token the depth from `SyntaxIndentationUtilities.GetChildIndentLevel` (scope braces and embedded statements only), and only the 17 contributors of `LayoutComputer.CreateContributors` override it.

The class is wider than the issue and the change doc list. These common shapes reach the same fallback today and move under FMT-INDENT-034 (all checked with `scripts/format.sh`, all in column 8 today):

```csharp
        for (var i = 0;
            i < 3;
            i++)
        foreach (var x in
            xs)
        throw
            new Exception();
        yield return
            x;
        var d = await
            t;
        var a = (int)
            x;
        var b = new
            List<int>();
        int[]
            f = null;
        Func<int, int> g = [Obsolete]
            (z) => z;
```

Instances of the issue that are no longer reachable (no test, no requirement): the issue's own input example (comment behind `?`, FMT-UNSUP-008; directive behind `=>`, FMT-UNSUP-012), `var x = // c`, the `=>` cases, a directive in a multi-declarator field, a primary-constructor base list — as the spec author lists — and, re-run here, `x⏎+= 1;` and `x ??=⏎2;`, which the line-break phase now joins (`x += 1;`, `y ??= 2;`, the closure of #1383).

### Mechanism and candidates

_N/A_ (behavior change). Dispatch: the lines in question are every first-on-line token that `LayoutComputer.ComputeBlockIndentation` places and no contributor of `CreateContributors` overrides; FMT-INDENT-034's "continuation line" definition is checked against that below.

The closed list of FMT-INDENT-034's non-continuation parts was checked against the syntax kinds that can start a line in a statement, member, or using directive: nested statements (block, embedded, labeled, local function, statements of lambda and anonymous-method blocks), members (types, namespaces, enum members — all `MemberDeclarationSyntax`), accessors, switch sections and labels, scope braces, `else`/`catch`/`finally`, the `while` of `do`, and attribute lists with the token behind them. No missing part was found. Every other line-starting token that is not owned by a contributor or FMT-RAW-007 is a continuation line, which matches the code's fallback set.

### Breaking inputs

**Finding 1 — FC-028: node-level root that does not start its line (spec gap, visible code-fix output).** FMT-INDENT-034 measures from "the column at which the line of the construct begins". When a code fix formats a detached root that starts mid-line (`FormatNodeInDocumentAsync`, FMT-PIPE-023), and the construct starts on the root's first line, that line begins with a token outside the root, which FMT-PIPE-023 forbids a phase to read. FMT-PIPE-024 lists no fact for it (depth, first-token column, construct). Inside the detached root, the only thing the phase can measure is the root's first token. The spec's `Why:` says the column is the same through every entry point, but the normative text does not say how, and the implementation cannot do it without a new fact. RH5302-00x and RH5308 say their fixes format "as the formatter formats it in place", so the intended output is decided: the CLI's column. The missing piece is internal (a fact), not an output question.

Today's divergence, reproduced:

```csharp
// input
        var r = o is
            not null &&
            x > 0;
```

- `scripts/format.sh`: `not null` in column 8, `&& x > 0;` in column 16.
- `scripts/apply-fix.sh RH5302`: `not null` in column 16, `&& x > 0;` in column 16.

```csharp
// input
        var r = o is
            not null ? 1 :
            2;
```

- `scripts/format.sh`: `not null` in column 8; `? 1` and `: 2;` in column 20.
- `scripts/apply-fix.sh RH5308`: `not null` in column 16; `? 1` and `: 2;` in column 20.

Under FMT-INDENT-034 the CLI gives `not null` column 12. A fix that measures inside the detached root gives 20 (the root's first token `o` is in column 16, plus 4).

Class matrix (axes: entry point × where the root's first token stands × where the continuation's construct starts):

| Entry point | Root's first token | Construct starts | Result |
|---|---|---|---|
| whole document | n/a | anywhere | holds (the reference) |
| `FormatNodeInDocumentAsync` | starts its line | on the root's first line | holds: the first-token column fact is the line's column |
| `FormatNodeInDocumentAsync` | starts its line | on a later line inside the root | holds |
| `FormatNodeInDocumentAsync` | mid-line | on a later line inside the root | holds: RH5305's lambda in a collection expression (`() =>` gets its own line), the case the spec examples show |
| `FormatNodeInDocumentAsync` | mid-line | on the root's first line | **fails without a new fact**: RH5302 and RH5308 above. Other fixes whose root starts mid-line (RH5301, RH5303, RH5306, RH5307, RH5206, RH5101/RH5102) were not settled one by one, because their own contributors own most of their lines. The test implementer covers RH5302 and RH5308 |
| `FormatNodeInDocumentWithContextAsync` | context starts its line (RH5111 local or field declaration, RH7101) | anywhere | holds |
| `FormatNodeInDocumentWithContextAsync` | context mid-line (RH5111 parameter list) | on the context's first line | not reached: the lines inside a parameter are owned by FMT-INDENT-012 and FMT-INDENT-013 |

Recommendation (internal, for the spec author): state in FMT-PIPE-024 that a node-level entry point passes the document column of the first token or comment on the root's first line, measured as FMT-PIPE-017 measures the first token. State in FMT-INDENT-034 that a construct starting on the root's first line measures from that column. That value is a document fact, not a prediction of a phase (question 10): the fix keeps everything in front of the root as written. Alternative: a sentence that a node-level root measures from its own first token. That contradicts RH5302 and RH5308 ("as the formatter formats it in place") and the requirement's own `Why:`, so I do not recommend it. Tests: RH5302 and RH5308 inputs above, through the fix and through the CLI, both columns 12.

**Finding 2 — SHARED-ATTR-022's changed example contradicts SHARED-ATTR-017 (spec error).** The RH5514 split fix calls no formatter: `TargetAttributeListShapeCodeFixProviderBase.ApplySplitCodeFixAsync` replaces the text and writes each further list "behind the whitespace that stands at the start of the line" (SHARED-ATTR-017, unchanged). Reproduced:

```csharp
// input
        Func<int> f = [A, B] () => 1;
```

`scripts/apply-fix.sh RH5514`:

```csharp
        Func<int> f = [A]
        [B] () => 1;
```

FMT-INDENT-034 does not change that (`[B]` stays in column 8). The spec diff's `[B]` in column 12 in SHARED-ATTR-022 and the change doc's RH5514 bullet under *Code fixes* are wrong and must be reverted. The CLI would then put `[B]` in column 12 on its next run. That is P11, not FC-013: no rule reports the column of `[B]`.

**Finding 3 — RH5204 does not conflict (FC-013 checked).** `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.ShouldAnalyzeToken` checks only a token that is the first token of a namespace, member, enum member, statement, accessor, `else`/`catch`/`finally`, or switch label, or a scope brace. A continuation line never starts with such a token (`private static⏎readonly …`: `readonly`'s parent starts with `private`). Its non-goals already leave continuation lines to FMT-INDENT. No other layout rule (RH5201, RH5203, RH5205, RH5206, RH5301–RH5308, RH5110) checks a line that FMT-INDENT-034 places. No analyzer spec covers query clauses. No conflict.

**Finding 4 — Reihitsu.Core is shared.** `SyntaxIndentationUtilities.GetChildIndentLevel`/`ComputeIndentLevel` feed RH5204, RH5110, RH5206, RH5301, RH5303, RH5304, `InitializerLayoutAnalysisUtilities`, `CommentPositionUtilities`, and several code fixes. FMT-INDENT-034 is a column of the formatter's layout, not a scope depth. If it is implemented by changing what Core counts as a level, those analyzers start reporting continuation lines. It must stay out of Core (surface table).

Failure classes walked: FC-002 (the three qualifiers have two-sided examples in the spec; add `when` of a switch arm), FC-003, FC-010/FC-017 (idempotency: FMT-INDENT-034's anchor is always on an earlier line, and so is every contributor's anchor, so no cycle. Two passes and CRLF per test as usual), FC-019/FC-020/FC-021 (comment- and directive-held continuation lines: `x += // c⏎3`, the `#endif` declarator), FC-022 (verbatim hole: the "Its line" clause; raw hole: FMT-RAW-007 keeps the line, reproduced: `{(o is⏎not null)}` in a `$"""` literal moves only with the literal. FMT-INDENT-034 must not touch it), FC-027 (nesting: continuation of a continuation, lambda in collection, query in argument), FC-028 (finding 1), FC-013 (finding 3).

Candidate inputs for the test implementer (CLI unless stated):

| Input | Expected | Existing test |
|---|---|---|
| `x += // c⏎3;`, `case 1 or⏎2:`, `var p = o is⏎not null;`, `return⏎x;` | FMT-INDENT-034 example 1 | none found for the new columns; tests that pin column 8 change (search `LongExpressionLayoutTests`, `StrandedDocumentation*Tests`, `WrappedAngleBracketListTests`, `BaseListContinuationAlignmentTests`, `CollectionExpressionAlignmentTests`) |
| anonymous-object member, parenthesized pattern, switch-arm `=>` | example 2 | none found |
| switch-arm `int i⏎when i > 0 => 1,` | `when` in column 24 (arm in 20) | none found |
| `for (var i = 0;⏎i < 3;⏎i++)`, `foreach (var x in⏎xs)`, `throw⏎new …`, `yield return⏎x`, `await⏎t` | continuation lines in column 12 | none found |
| `return x is⏎1 or⏎2 or⏎3;` | Q1 reading A: `1` 12, `2` 16, `3` 16 (see Output questions) | none found |
| declarators: documented field, event field behind `#endif`, local; the issue's `int a,⏎#if X⏎b,⏎#endif⏎c;` | example 3 | none found |
| base list `:`, modifier, `catch … when` | example 4 | `BaseListContinuationAlignmentTests` (check) |
| lambda body, lambda in collection (CLI **and** `apply-fix RH5305`), `with` | example 5; RH5305 column 32 both | none found for 32 |
| verbatim interpolated hole | example 6 | none found |
| raw interpolated hole `{(o is⏎not null)}` | unchanged by FMT-INDENT-034 (FMT-RAW-007) | check `RawString*` hole tests |
| negative example (attributes, label, `else`, `do … while`) | unchanged | none found |
| RH5302 and RH5308 inputs of finding 1, fix and CLI | `not null` column 12 in both | none found |
| RH5514 lambda split | `[B]` column 8 (unchanged) | the SHARED-ATTR-022 example test, which keeps 8 |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `IndentationPhase` / `LayoutComputer` and its contributors | must change (FMT-INDENT-034, FMT-INDENT-035) |
| `ReihitsuFormatter` node-level entry points, `FormattingContext` facts | must change if finding 1 is decided as recommended (one more fact) |
| `LineBreakPhase` | must stay |
| `Reihitsu.Core` `SyntaxIndentationUtilities` | must stay (finding 4) |
| Analyzers (RH5204 and the layout rules) | not involved (finding 3) |
| Code fixes through the formatter (RH3202–RH3208, RH5302, RH5305, RH5308, RH7101, RH5111) | no code change; output changes through the formatter |
| RH5514 / RH5518 split fix | must stay (text replacement, SHARED-ATTR-017) |
| Rule pages | not involved (the spec author's probe: no rule page shows a moved line) |
| CLI | not involved (code); output changes |

### Refuse option

Leaving the construct as written is today's behavior (block column). It is not available as a "rare class" refusal: the class holds code people commonly write (wrapped `for` headers, declarator lists, LINQ queries, `return⏎value`, lambda bodies on the next line), and the maintainer already decided the rule (#921 Q1). Declaring a shape unsupported: not an option. FMT-INDENT-034 is one general rule and adds no special case per shape, so a declared shape would delete nothing. The comment-held members (`x += // c⏎3`, `A // c⏎= 1`) need no guard of their own. The one exception FMT-INDENT-034 removes is FMT-INDENT-015's block-column clause for omitted type arguments (Q4). Cost of the requested change: one fallback for lines no contributor places, plus FMT-INDENT-035 and finding 1's fact. That is cheaper than the contributor-per-construct alternative. Simpler option: none found.

### Output questions

Question 1 of the spec author (where a continuation inside a construct is measured from) is real, but its example set shows only the side where reading A wins. Add the member where the readings differ the other way, a left-associative pattern chain whose first operand starts a continuation line:

```csharp
        var k = x is
            1 or
            2 or
            3;
```

Before (`main`): `1 or`, `2 or`, `3;` all in column 8.

After, A: `1 or` in 12, `2 or` in 16, `3;` in 16. `1 or 2` is a node that starts on the line of `1`, so `2` continues it, and `3` continues `(1 or 2) or 3`, which also starts there.

After, B: all three in 12.

The same staircase appears for a curried lambda (`x =>⏎(y) =>⏎y` gives `y` one level deeper than `(y)` under A, the same column under B). For the curried lambda, A matches the nesting. For the pattern chain, B reads better. I still recommend A, because the right-laid-out cases (a line left of its own braces or parentheses) are worse, and the staircase needs a line break behind `is` that people rarely write. The maintainer should see both sides. Under A, the `case 1 or⏎2 or⏎3:` form gives `2` and `3` both one level deeper than `case` (the `or` patterns start on the line of `case`), with no staircase.

Questions 2 to 4: real, and no further reading found. For question 4, note that the shape (omitted type arguments kept wrapped by a comment) is rare.

The visible-change list should show `for (…;⏎…;⏎…)` and `foreach (… in⏎…)` with the confirmations (see *Issue goals*). They are the most common inputs the rule moves, and the change doc does not show them.

### Internal choices

- Finding 1: a document fact for the column of the root's first line (recommended) or measuring from the root's first token (contradicts RH5302 and RH5308). The visible output is already decided by RH5302 and RH5308, so this is internal.
- The spec author's "Its line" choice (the line where the text starts, rather than the text's literal whitespace) is fine. It is also the only cheap reading that serves multi-line comments, so no shape declaration would remove it.

### Issue goals

- "One level deeper than the line on which its statement or member starts": changed by the spec to the nearest construct that starts on an earlier line. That changes visible output → Q1 (asked).
- "FMT-BLANK-013 gains the region-inside-a-declaration case as a clarification": drop. FMT-UNSUP-013 refuses the region between a signature and `=>`. No visible change.
- "FMT-STRUCT-007 and FMT-STRUCT-023 show their refusal examples as code": drop. FMT-STRUCT-023 is retired, and FMT-STRUCT-007's example only changes its `;` column. No visible change beyond FMT-INDENT-034.
- The issue's *Input Example* and *Desired Formatted Output*: the input is now refused as unsupported (FMT-UNSUP-008, FMT-UNSUP-012). Only `case 1 or⏎2:` survives and is covered. Drop as a test. No visible change.
- "A test of both entry points" for the collection-expression lambda (comment of 2026-11-17): keep. It is one member of finding 1's matrix; widen it to RH5302 and RH5308.

### Architecture findings

_None._ Finding 1 is FC-028 between the node-level entry points and whole-document formatting. Its cause is a missing input fact, not a duplicated decision or a prediction. The fix keeps it a single owner: `LayoutComputer` reads one more fact.

### Follow-up candidates

- **A wrapped `for` header aligned to its parenthesis** (P34: one treatment per syntax family, as FMT-INDENT-012 aligns argument and parameter lists to the opening delimiter). In the form of `Output questions`:

  Input: `for (var i = 0;⏎i < 3;⏎i++)`. Before (`main`): `i < 3;` and `i++)` in column 8. After, A (this run, FMT-INDENT-034): column 12. After, B: column 13, under `var`. Not this run's: FMT-INDENT-034 places it until a requirement aligns it. A follow-up `agent-filed` issue, like #1633/#1634/#1905/#1542.
- The comments on #1633, #1634, #1905, #1542 that the spec author lists: agreed.

## thoenissen/Reihitsu#1852

### Premise

Real, and the CLI reaches both parts (input of the issue, `scripts/format.sh`): `return⏎2;` puts `2;` in column 8. `int Get() =>⏎⏎2;` becomes `{⏎return⏎2;⏎}` with `2;` in column 8. `return // c⏎_v;` puts `_v;` in column 12, the column of `return`. `scripts/apply-fix.sh RH3202` on `int Get() =>⏎⏎2;` writes `return⏎        2;` (LF and CRLF). Reproduced as the issue states.

The code that keeps the break is deliberate but undecided. `ExpressionBodyToBlockConverter.CreateReturnStatement` drops the expression's leading trivia only when it is all whitespace ("Leading trivia that carries a line break is kept, because the layout it describes is then the author's"). The test `ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionKeepsExpressionOnOwnLine` pins it as "keep their current layout". It came from #1824 and #1839 (`git log -S`), whose change doc says the blank-line handling "is layout and stays". That is a characterization, not a maintainer answer. So the question is open, and that test changes under A.

### Mechanism and candidates

_N/A_ (behavior change). The value line is the general rule (FMT-INDENT-034). The join is `CreateReturnStatement`'s leading-trivia condition. Since FMT-UNSUP-012 refuses a comment or directive in the gap, that trivia can only hold whitespace and line breaks, so a blank line is the only input the "kept" branch still serves.

### Breaking inputs

Covered by #953's matrix for the value line. For the join, check both sides (FC-003): no blank line (joined today, `ExpressionBodyNextLineExpressionTests`), one blank line, a blank line holding whitespace, and two blank lines. Check all four statement-creating conversions (FMT-STRUCT-004/005: method, operator, conversion operator, local function; FC-025) and the fixes RH3202, RH3205, RH3206, RH3208. Existing tests: the `…OnNextLineConvertsInOnePass` tests pin the no-blank-line side. `BlankLineBeforeExpressionKeepsExpressionOnOwnLine` pins today's other side and changes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `IndentationPhase` | must change (value line, via #953) |
| `StructuralTransformPhase` / `ExpressionBodyToBlockConverter` | must change under Q2 A (one condition); must stay under B |
| RH3202, RH3205, RH3206, RH3208 fixes | output changes through the formatter, no code change |
| Analyzers, rule pages | not involved |

### Refuse option

B (keep the break; the value then takes FMT-INDENT-034's column) costs nothing beyond #953. A costs one widened condition and changes one test. Both are cheap; nothing to refuse or declare (a blank line is layout, not a shape).

### Output questions

Q2 of the spec author, unchanged, with one correction to its *Why*. "A body converted to an expression statement already joins across the blank line" is not what happens. For `void B() =>⏎⏎Console.WriteLine();` (`scripts/format.sh`), nothing joins: the statement starts its own line in the created block, and the blank line behind `{` is removed. The true sentence is: "the blank line disappears there too". Reword it before asking. The recommendation (A) stands.

### Internal choices

_None._

### Issue goals

The issue's three parts match the spec. The comment-behind-`return` part follows Q1/FMT-INDENT-034 (the construct starts on the statement's line, so both readings agree). No goal to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Premise

Real (CLI): `var b = from v in arr⏎select v;` puts `select v;` in column 8. A whole query outside a list element (`join`, `on`, `orderby x,⏎y`, `group … into g`, `select g`) puts every line in column 8. The same query inside an argument (`System.Console.WriteLine(from x in arr⏎select x)`) already aligns `select` under `from` (FMT-INDENT-013). Reproduced as the issue states.

### Mechanism and candidates

_N/A_ (behavior change). Lines a query can start with: the clause keywords (`from`, `let`, `where`, `join`, `orderby`, `select`, `group`), `into` (`QueryContinuationSyntax`), and the clause-internal continuations (`on`/`equals` of a join, a later ordering behind `,`, `by` of `group`, the value behind `=` of `let`). FMT-INDENT-035 places the first two; the third falls to FMT-INDENT-034 (one level deeper than the clause's line).

### Breaking inputs

FC-027: a query in a `return` (`return from x in arr⏎select x;`, clauses under `from` in column 15), a query inside a list element (unchanged, `ArgumentContinuationLineAlignmentTests.QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart` pins it), a nested query in a `from` or `let` clause (aligns with its own `from`), `select y into z⏎select z` and `into` starting a line, `join … ⏎on …`, `orderby x,⏎y`. Existing tests for the outside-list case: none found.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `IndentationPhase` / `LayoutComputer` | must change |
| Analyzers (no layout rule judges queries), rule pages, code fixes | not involved |

### Refuse option

Leaving the clauses at the statement column is today's output and is option C of Q3. Queries are common code, so refusing is not a rare-class option. Declaring: not applicable. Cost of A: the existing FMT-INDENT-013 query alignment applied without its list-element condition, which is probably smaller than a new mechanism.

### Output questions

Q3 of the spec author, unchanged. Add `into` starting its own line to the example only if the lead wants it shown. It aligns under `from` like any clause.

### Internal choices

_None_ beyond the spec author's (`into` clauses belong to the outer query).

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Premise

Real (CLI, the issue's input): `D;` lands in column 4 and `c;` in column 8. The disabled `B,`/`b,` keep their columns. Event fields and locals are not refused (FMT-UNSUP-010 covers only `FieldDeclarationSyntax`). Reproduced as the issue states.

### Mechanism and candidates

_N/A_. A direct member of #953's class: the declarator after `#endif` is a first-on-line token that no contributor places. Every later declarator of an event field or local measures from the declaration's line (the declaration is the nearest construct that starts earlier), so siblings do not staircase.

### Breaking inputs

Covered by #953's FMT-INDENT-034 example 3 and the issue's own input (both declarations). FC-020: also `#else` and `#elif` branches, and a declarator behind `#endif` that is the first after the type (`int⏎#if X …`). Existing tests: none found.

### Surfaces

As #953 (indentation phase only).

### Refuse option

Not a rare class in the sense of *Code-fix coverage*: the class is "a declarator on its own line", and the directive only exposes it. Declaring a directive inside an event or local declaration unsupported would delete nothing, because FMT-INDENT-034 needs no guard for it.

### Output questions

_None_: a direct application of the decided rule. The lead can show it as a confirmation.

### Internal choices

_None._

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Amendments for the spec author before the output-question batch: finding 1 (FMT-PIPE-024 fact and an FMT-INDENT-034 sentence for a node-level root that starts mid-line); finding 2 (revert SHARED-ATTR-022's `[B]` to column 8, drop the RH5514 bullet from `continuation-indent.md`); add the `for` and `foreach` headers to the change doc's visible changes; correct Q2's *Why*; add the pattern-chain staircase to Q1.
- No tests or builds were run beyond `scripts/format.sh` and `scripts/apply-fix.sh` on scratch fixtures. One scratch directory (`…/scratchpad/fx2`) already held files from an earlier role, which a `format.sh` run on that directory reformatted. They are throwaway fixtures outside the repository; no repository file changed.
