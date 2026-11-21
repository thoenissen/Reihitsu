Role: spec author
Commit: 2c19999
Verdict: #953: change doc written (with output questions for the lead); #1852: change doc written; #1896: change doc written; #1818: change doc written

Amendment round after the maintainer's batch-2 answers and the skeptic's `05-skeptic.md`. This handover supersedes `04-spec-author.md`; 01, 03, and 04 stay the record of how the drafts were established. The prompt carried no intended fix, suspected cause, or hoped-for verdict. The maintainer is not available; the cases the answers do not name are under *Output questions* with a recommendation on the scheme of the standing instruction (join where nothing stands in the gap; declare a comment or directive in a joined gap unsupported; continuation indent only where a line cannot be joined and is not refused), and the specs are written on that recommendation for the lead to decide.

Every change doc of the run has `Status: Accepted`.

Checked: every new *before* through `scripts/format.sh` on `main`'s code (fixtures in `/tmp/claude-0/-home-user-Reihitsu/067a7317-225f-510e-8f9b-6cda6989da93/scratchpad/sa6/`): the batch-2 rows (`Rows.cs`), the FMT-BREAK-041 example (`Ex41.cs`, a fixed point of `main`, and its joined form `Ex41a.cs`, also a fixed point, which gives the *after*), the pattern chains and the type test (`Chain.cs`), the other side of FMT-BREAK-017 (`Other.cs`, unchanged), the collection-element example of FMT-INDENT-034 (`Coll.cs`), the anonymous-object member and `let` example (`AnonLet.cs`), the FMT-UNSUP-019 example (`Arm.cs`), and `return⏎;`/`throw⏎;` (`Ret.cs`, kept by `main`). All 42 C# blocks of the new and changed requirements and change docs parse (`scripts/format.sh --check` on the extracted blocks: none skipped for syntax errors). The CLI parses without preprocessor symbols (`FormatCommandHandler` uses `CSharpParseOptions.Default`), which fixes the location given for the multi-targeting example. `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-INDENT-034, FMT-INDENT-035, FMT-BREAK-041, FMT-UNSUP-017, FMT-UNSUP-018, FMT-UNSUP-019 (no tests yet; expected). Uses, requirement IDs, and the `AGENTS.md` guard-name check pass.

## Change docs

- `sdd/changes/2026-11/continuation-join.md` — rewritten for batch 2: every row of answers 1 B and 2 B joined, the type-test `is` (4 B), the whole-chain combinator layout (5 B), the combinator gap declared (6 B), directives confirmed including conditional modifiers and `return⏎#if` with the CLI's line (3 A), one reason per gap (9 A), the parenthesized pattern (10 A); *Special cases* made consistent (finding 8): only `CreateReturnStatement` is deleted, the joins carry no refusal of their own, and `GetOnlyMemberExpressionBodyRewriteUtilities.HasLayoutOnlySeams` and the throw-operand trivia transfer are checked and kept with the reason (RH3209/RH3210 reach them on unformatted input). Accepted.
- `sdd/changes/2026-11/assignment-comment-in-arm-unsupported.md` — **new** (answer 8): FMT-UNSUP-019, with *Special cases* (deletes nothing; see *Internal choices*). Accepted.
- `sdd/changes/2026-11/continuation-indent.md` — the arm/member example replaced by a collection-expression element (the arm/member line is refused now); the query example uses `on x⏎equals b` (`let y⏎= x` is joined now); the split-field example uses a break behind the type (a break between modifiers is joined now); the nearest-construct decision records answer 8. Accepted.
- `sdd/changes/2026-11/omitted-type-argument-comment-unsupported.md` — answer 7 A recorded for directives. Accepted.
- `sdd/changes/2026-11/expression-body-join-across-blank-line.md` — unchanged. Accepted.

## thoenissen/Reihitsu#953

### Changed requirements

- FMT-BREAK-041 — retitled; gaps added (answers 1 B, 2 B, 4 B): `yield return`, `await foreach`/`await using`, `new[]`, the type-test `is`, `not`, a switch arm's `when`, every modifier of a member, type, delegate, local, local function, accessor, lambda, or anonymous method (between two modifiers and in front of `class`, `event`, a constructor's name, `implicit`, `delegate`), `foreach … in`, a cast's operand; the anonymous-object `=` moved to FMT-BREAK-016; "with an expression" for `return` and `throw`, and `return⏎;`/`throw⏎;` named on the other side (finding 7); the cross-reference to FMT-UNSUP-017 states which gaps other requirements join (FMT-BREAK-015 for parameter modifiers, FMT-BREAK-016, FMT-BREAK-017) and that FMT-UNSUP-011 refuses the type-test gap; `new()` stays FMT-BREAK-039's; the main example rebuilt with the batch-2 rows — changed.
- FMT-BREAK-016 — changed: the `=` of an anonymous-object member and of a `let` clause joins both sides (answer 1 B; placement per 05 finding 6); a comment there is refused (FMT-UNSUP-017), a comment behind an assignment operator inside an arm or member is refused (FMT-UNSUP-019); new example.
- FMT-BREAK-017 — changed: the type-test `is` stays at the line end and its type joins it (4 B); a binary pattern is laid out as a chain, defined by cause (nested binary patterns, not through parentheses), wrapped when a line break stands next to one of its combinators or a delimited operand next to one spans lines, and then every combinator starts its line (5 B); the right pattern joins its combinator except where FMT-BREAK-022 keeps its opening delimiter on its own line (finding 2); the combinator gap is refused (6 B), so the FMT-BREAK-001 paragraph is gone; two new examples (partial chain, property-pattern operand, type test; and the other side).
- FMT-BREAK-031 — clarified: the list of joins names FMT-BREAK-041 and the combinator and type-test joins.
- FMT-UNSUP-017 — changed: table rows added for the combinators and `not` (6 B), a switch arm's `when`, `foreach … in`, a cast, both sides of the anonymous-object `=` and the `let` `=`; the keyword row covers `yield return`, `await foreach`/`await using`, `new[]`; the modifier row covers every modifier list with the reason `a comment or directive behind a modifier`; one reason per gap (9 A); the other-side list updated (in front of a combinator, in front of the first modifier, in front of `foreach`'s `in`, inside a cast's parentheses, `return /* c */;`, `new /* c */ ()` → FMT-UNSUP-014, anonymous `new`); the conditional-modifier case stated per build (3 A); a second refused example (combinator, cast, `foreach`, `let`, `yield return`, conditional accessibility on a type, locations per build); the other-side examples no longer use inputs that are refused now (`o is not /* c */ null`, `A = /* c */ 1`), plus a second other-side example.
- FMT-UNSUP-019 — Comments and directives behind an assignment operator inside a switch arm or an anonymous-object member are unsupported — added (answer 8).
- FMT-UNSUP-011 — clarified: the pattern operators' bullet points the combinator gap to FMT-UNSUP-017; `Why:` no longer claims the combinators hold no refusal (finding 9) and names the type-test join; the other-side example drops `or /* c5 */ 3`.
- FMT-INDENT-034 — changed: the list of remaining lines drops the combinator case and names FMT-UNSUP-019; the arm/member example replaced by a collection-expression element (`2` in column 26), with a sentence that the arm/member case is refused; `Why:` adjusted.
- FMT-INDENT-035 — example changed: `join b in brr on x⏎equals b` (the `equals` line a continuation line of the clause) replaces `let y⏎= x`.
- FMT-STRUCT-021 — example changed: `private static⏎readonly` is joined in every split field.
- RH2001-007, RH7101-007 — clarified: FMT-UNSUP-019 named among the shapes that withhold the fix.
- `AGENTS.md` phase table — PrecheckPhase: the eleventh to thirteenth shapes (FMT-UNSUP-017, 018, 019); LineBreakPhase: pattern chains, the joins, "keeps the blank lines of every gap it does not join"; IndentationPhase: the continuation indent. Keywords are written without code font in the table, because the guard-name check resolves code-font names.
- Unchanged from 04 and kept: FMT-BREAK-022, FMT-INDENT-015, FMT-INDENT-026, FMT-UNSUP-009, 010, 012, 013, 018, FMT-PIPE-024, FMT-STRUCT-006, SHARED-BLANK-008, and the others 04 lists.

### Examples for the tests

FMT-BREAK-041 (all examples), FMT-BREAK-016 (anonymous `=` and `let`), FMT-BREAK-017 (chain example, partial chain with property-pattern operand and type test, other side), FMT-UNSUP-017 (both refused examples, each reason and location per table row, both builds of the conditional modifiers, both other-side examples), FMT-UNSUP-019 (refused and other side; the RH5302 fix inside an arm stays offered, FMT-UNSUP-003), FMT-UNSUP-018, FMT-INDENT-026, FMT-INDENT-034 (all examples; RH5302 and RH5308 through the fix and the CLI), FMT-INDENT-035, FMT-STRUCT-021, RH2001-007 (`= new { A = s_count += /* three */ 3 };`), RH7101-007.

Existing tests that change (05's list still applies): `WithExpressionSpacingTests.CommentInFrontOfWithKeepsItsPlace` and its `/** a */` variant; `ThrowOperandPrecedenceExpressionBodyTests.CommentAfterThrowKeywordStaysBeforeOpeningParenthesis` and the accessor variant; `DelimitedDocumentationInExpressionTests.MovesDelimitedDocumentationBehindLambdaArrow`; `ExpressionBodiedAccessorTests.CommentAfterReturnKeywordKeepsBlock`; `ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionKeepsExpressionOnOwnLine`; `UnsupportedListSeparatorCommentTests` (omitted type argument row); `ZeroWidthTokenLineStartTests`; the other-side tests of FMT-UNSUP-011 (`or /* c5 */ 3`), FMT-UNSUP-012, FMT-UNSUP-013; the FMT-STRUCT-021 test of `private static⏎readonly`; every test that pins `o⏎is string` from `o is⏎string`, a line break behind a modifier, a parameter modifier with a comment, or `new⏎[]`. Search the formatter tests for each shape of the FMT-UNSUP-017 table rather than relying on this list. Not involved: RH2005, RH6022, RH7105, RH7106 (their fixes do not call the formatter).

### Output questions

For the lead to decide on the scheme; the specs are written on the recommendation.

**1 · Formatter — modifiers of an accessor, a lambda, and an anonymous method (FMT-BREAK-041, FMT-UNSUP-017)**

Input / Before (`main`, as written):

```csharp
    public int P
    {
        get;
        private
        set;
    }

    Func<int, int> f = static
    x => x;
```

After, A (recommended, spec as written): `private set;` and `static x => x` joined; `private /* c */ set;` and `async /* c */ x => x` refused with `a comment or directive behind a modifier`.

After, B: only the declarations batch 2 named; these lines stay continuation lines, and comments there stay supported.

Why A: the maintainer joined every modifier gap the run showed (answers 1 B, 2 B); the gap is the same cause, a modifier and what it modifies.

**2 · Formatter — the scope of FMT-UNSUP-019 (answer 8)**

Input:

```csharp
        int[] z = [
                      1,
                      y += // c
        2
                  ];
        var w = new { A = x += /* c */ 3 };
        var r = y switch
                {
                    1 => () => { x += // c
        3; },
                    _ => null,
                };
```

After, A (recommended, spec as written): the collection element is formatted, `2` in column 26 (continuation from the element's line); `A = x += /* c */ 3` on one line refuses the file like the multi-line form; the statement inside the lambda's block inside the arm is not refused (the walk up stops at the statement).

After, B: refuse a comment behind any assignment operator inside an expression (collection elements, arguments, initializers too), so no continuation line is measured from a construct to the right of the statement.

After, C: refuse only when the gap holds a line break (the one-line `/* c */` stays formatted).

Why A: the answer names the arm and the member; elsewhere the scheme's third clause applies, and "on any line" is what every other shape of the spec does.

**3 · Formatter — the hole of a verbatim interpolated string inside a switch arm or an anonymous-object member (FMT-INDENT-034)**

Input / Before (`main`): a hole line inside `1 => $@"a⏎{⏎x⏎}"` stays in the statement's column.

After, A (recommended, spec as written): continuation indent from the line where the string's text starts (the arm's line), four columns right of it.

After, B: declare it unsupported like answer 8.

Why A: the line cannot be joined and is not refused, so the scheme gives it the continuation indent; declaring it would delete nothing either.

**4 · Formatter — a chain whose only line break is a combinator next to a multi-line delimited pattern (FMT-BREAK-017)**

Input / Before (`main`):

```csharp
        var a = s is null or "" or
        {
        Length: 2
        };
```

`main`: `s is null or ""`, then `or` alone, `{` on its own line (under `is`).

After, A (recommended, spec as written): the chain counts as wrapped, so every combinator starts its line: `s is null` / `or ""` / `or` / `{ … }`.

After, B: only the combinator next to the delimited pattern starts its line (as on `main`), and `null or ""` stays together.

Why A: under B the output has a line break in front of a combinator, so the second pass would see a wrapped chain and move the other combinators; A is the only reading that is idempotent with answer 5 B.

### Internal choices

- FMT-UNSUP-019 deletes no special case (its change doc says so). `AGENTS.md` says a shape that deletes nothing is not declared; the maintainer decided this one explicitly (answer 8), so it is written, and the lead should name it in the PR's review notes.
- The anonymous-object `=` and the `let` `=` are in FMT-BREAK-016 (05 finding 6), not FMT-BREAK-041; alternative: both sides in FMT-BREAK-041.
- The combinator gap and `not` are one row of FMT-UNSUP-017 with one reason (`between and, or, or not and its pattern`), like the four keywords of the first row; alternative: two rows.
- The modifier reason changes from `between a modifier and the type` to `behind a modifier`, because the gap now includes two modifiers and non-type tokens.
- A parameter's modifiers are joined by FMT-BREAK-015 (as on `main`), so FMT-BREAK-041 does not list them; FMT-UNSUP-017 refuses a comment there.
- `new⏎()` needs nothing new: FMT-BREAK-039 already joins it (`main` does) and FMT-UNSUP-014 already refuses a comment in front of its `(`.
- FMT-UNSUP-019 finds the arm or the member by walking up to the nearest statement or member declaration.
- The `Uses` row of `indentation.md` is now sorted (the test added three IDs; the earlier order was not sorted).

### Follow-up candidates

- Gaps next to the joined ones that no answer and no row named, which stay continuation lines: in front of a `foreach`'s `in`, a query's `from x in⏎arr`, `on x⏎equals y`, a relational pattern `>⏎1`, a prefix unary operator, the `;` of `return⏎;` and `throw⏎;`. If the lead wants the scheme applied to them, one more change; otherwise not filed.
- P11 new-rule candidates (from 05): a combinator counterpart of RH5302; an anonymous-object member and `let` counterpart of RH5111.
- A multi-line positional pattern behind `is` (05): elements and `)` become continuation lines; a P34 candidate.
- `case (⏎…⏎):` puts `case` alone on its line (04, agreed by 05).
- Comments on #1634, #1905, #1542 (their *before* changes under FMT-INDENT-034) and #1633 (its case no longer exists: the base list `:` joins).

## thoenissen/Reihitsu#1852

### Changed requirements

FMT-STRUCT-006 as in 04; the `return` value is joined (FMT-BREAK-041), and a comment or directive behind `return` is refused (FMT-UNSUP-017; directives confirmed by answer 3 A).

### Examples for the tests

FMT-STRUCT-006 (`Third`), FMT-BREAK-041 (`return⏎⏎x`), and the RH3202, RH3205, RH3206, RH3208 fixes of `int Get() =>⏎⏎2;`.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Changed requirements

FMT-INDENT-035 (example changed to `on x⏎equals b`); the `let` clause's `=` joins both sides (FMT-BREAK-016).

### Examples for the tests

FMT-INDENT-035, FMT-BREAK-016's `let` example, and `return⏎from x in a⏎select x` (joined, clauses under `from`, 05).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Changed requirements

FMT-INDENT-034's declarator example, unchanged (answer 6A).

### Examples for the tests

FMT-INDENT-034's declarator example, plus `#else`/`#elif` branches and a declarator behind `#endif` first after the type (02).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Files

- `AGENTS.md` (phase table)
- `sdd/specs/formatter/line-breaks.md` (FMT-BREAK-016, 017, 031, 041, Uses)
- `sdd/specs/formatter/unsupported-syntax.md` (FMT-UNSUP-011, 017, 019, Uses)
- `sdd/specs/formatter/indentation.md` (FMT-INDENT-034, 035, Uses)
- `sdd/specs/formatter/structural-transforms.md` (FMT-STRUCT-021)
- `sdd/specs/analyzer/RH2001.md`, `sdd/specs/analyzer/RH7101.md`
- `sdd/specs/requirement-ids.txt` (FMT-UNSUP-019)
- `sdd/changes/2026-11/continuation-join.md`, `assignment-comment-in-arm-unsupported.md` (new), `continuation-indent.md`, `omitted-type-argument-comment-unsupported.md`
