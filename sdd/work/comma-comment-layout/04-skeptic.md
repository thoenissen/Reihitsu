Role: skeptic
Commit: 1ebfddac0ff97eed029f85967e510e1c58e37eb4
Verdict: output question (escalation 10)

The prompt carried no intended fix or hoped-for verdict; the shape the maintainer is considering is taken as the question, not as a decision. Outputs below come from `scripts/format.sh` on scratch fixtures (run twice, stable) and from reading the code regions named; the branch has no source change yet, so its `src/` is `main`'s.

## Whole run

### 1. The shape

**Definition (recommended).** An input is refused when, for a comma token in the input, a comment (`//`, `/* */`, `///`, `/** */`, every documentation mode), a preprocessor directive, or disabled text stands between the comma and the token in front of it — in that token's trailing trivia or in the comma's leading trivia — **and the comma stands on a later line of the text than the line on which that token ends**. "Later line of the text" counts a line break inside a multi-line comment's text too (as `TokenGapUtilities.HasLineBreakBetween` does, unlike SHARED-BLANK-008). Reason text, e.g. `a comment or directive between a list element and the comma behind it`; location: the first such trivia.

- **Span:** from the end of the token in front of the comma through the comma. Nothing behind the comma is in it: `1⏎, /* c */ 2` and `1, // c` are not refused (that is #1737's P/Q/R, which RH5107-004 withholds and FMT-BREAK-012/038 lay out).
- **Lists:** every comma token of the input, so one rule and no list of list kinds — separators and trailing commas alike, omitted elements (`Dictionary<⏎// c⏎,>`) included. A comma in disabled text is no token and is not checked (`1⏎#if NEVER⏎, 2⏎#endif` stays supported). A comma inside a directive or a doc comment's `cref` is structured trivia, not a token of the input. Restricting it to the lists that carry a special case (parameter, argument, attribute-argument, constraint, initializer family, trailing commas) saves only refusals in lists where nothing is deleted: enums, tuples, switch arms, positional patterns, base lists, type argument lists, `for` expression lists. Those are also comma-first-with-comment, so rare; the uniform rule is the cheaper statement.
- **FMT-UNSUP contract.** Cheap: one walk over comma tokens, two trivia lists, and one line comparison. Local (FMT-UNSUP-003): the element and the comma always lie in one list node, and every phase site below works on that list node, so no input that holds only part of the shape reaches a deleted guard. An element-only target (its trailing comment without the comma) is not refused and needs no guard: no phase moves a comma that is not in its input. Not produced by a phase (FMT-UNSUP-005), checked by reading every rewriter that touches commas: the comma moves (`CollapseSeparatorsToPreviousElementLine`, FMT-BREAK-038's move) carry only whitespace into the gap, trailing-comma removal leaves no comma behind the element, the field split is under FMT-UNSUP-004, and the attribute split does not run on a list that holds a comment.
- **Why the line condition.** Without it, a same-line comment in front of a comma is refused too: `Foo(true /* force */, false)` and data tables such as `{ 0x01 /* SOH */, 0x02 /* STX */ }`. It deletes no more code, because every guard below is reached only when a line break stands in the gap. **Why text lines and not SHARED-BLANK-008:** `CollapseSeparatorsToPreviousElementLine` tests the gap with `HasLineBreakBetween`, which counts a break inside `/* a⏎ b */`. Its comment guard is then what keeps `Foo(1 /* a⏎ b */, 2)` on one line (FMT-BREAK-012). Under SHARED-BLANK-008 counting that input is not refused, and the guard at that call site has to stay.
- **Comments only vs. with directives.** Every formatter guard below is the single predicate `WouldJoinAcrossUnjoinableTrivia` (comment, directive, or disabled text) or a test that a held comma still starts its line, whatever held it. A comments-only shape leaves directive-held commas (`b⏎#if DEBUG⏎, c⏎#else⏎, d⏎#endif` — reachable through the CLI, whose parse has `DEBUG` undefined, so `, d` is active and held), so **every code site stays and nothing is deleted** (`AGENTS.md`: a shape that deletes nothing is not declared). The directive and disabled-text part is what makes the declaration worth anything.

### 2. Special cases

**Deleted (formatter, `main`), with the recommended definition:**

| Site | What goes | Why it becomes unreachable |
|---|---|---|
| `LineBreakListRewriter.CollapseSeparatorsToPreviousElementLine` | the `WouldJoinAcrossUnjoinableTrivia(previousToken, separator)` → `continue` guard and its doc sentence | reached only after `HasLineBreakBetween` is true, i.e. the comma is on a later text line |
| `LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine` | the `HasLeadingEndOfLine(separator)` operand (a held separator counts as a line break and is left as written; FMT-BREAK-012's "held comma alone on its line keeps the line break behind it") and its doc sentences | runs after the collapse in all three visitors (argument, attribute-argument, parameter/bracketed parameter list); after the collapse only a held comma still starts its line |
| `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` | the `|| AreSeparatedByEndOfLine(constraint.GetLastToken(), separator)` operand and the doc sentence on a held comma | after `JoinGap`, only a comma whose gap holds unjoinable trivia is still separated |
| `GenericConstraintContributor` | the `ListElementAligner.AlignSeparators(constraint.Constraints, …)` call | a comma between constraints starts a line only when held (unheld ones make the clause wrapped and are joined) |
| `TrailingCommaRemovalUtilities.GetTriviaToPreserve` | the leading-trivia branch and `ContainsNonFormattingTrivia` | a comma's leading trivia holds a comment or directive only on a line below the element, which the shape refuses |

**Never written (this run):** FMT-BREAK-038's held paragraph ("A comma MUST NOT be moved across … is held …", alone-on-its-line in the FMT-BREAK-021/022 constructs, element kept behind it in arrays) and its third example; **all of FMT-INDENT-033** — its `Why:` states that the line exists only where a comment or directive holds the comma — with the separator alignment it needs in `ObjectInitializerContributor`, `AnonymousObjectContributor`, `CollectionExpressionContributor`, `ListPatternContributor`, `RecursivePatternContributor`; the indentation non-goal line that refers to FMT-INDENT-033 (reworded, see below); batch 1b's answer (moot); the held-comma test cases in the test implementer's list (directive-held, `// k` held in each construct).

**Spec text deleted (`main`):**
- FMT-BREAK-011 — the clause "except a comma that a comment, a directive, or disabled text keeps off that line (FMT-BREAK-001, FMT-BREAK-012)".
- FMT-BREAK-012 — the held-comma paragraph (including the SHARED-BLANK-008 sentence about `Foo(1 /* a⏎ b */, 2,⏎3)`, which the shape refuses), the matching `Why:` sentences, and the held parts of the example: `N`, `P`, `3 // keep⏎, 4`, the `#if X` list and its explanation. Kept: the comment behind a moving comma (`, /* c */ 2`, `, // c`), and `Foo(1 /* one */, 2,⏎3)` (same line).
- FMT-BREAK-037 — "a comma kept off the previous line stays at the start of the line of the constraint behind it", the "comment or a directive in front of a comma" part of its comment example, and `O` of the multi-line-comment example (refused under text-line counting).
- FMT-INDENT-032 — "and every comma between two constraints" in its rule, and the held `, new()` lines of `O` in its example.
- FMT-STRUCT-027 — "or the comment led the comma on a line of its own", and the `/* two */ // before comma` part of its example. Kept: a comma on a later line with its own comment (`B⏎, // comma⏎}`).

**Checked and kept:**
- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` / `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and `LineBreakTriviaUtilities.CollapseTokenToSameLine`'s refusal — about 20 other callers (open paren, close paren, operators, chains, assignments, accessors).
- `ListElementAligner.AlignSeparators` and its calls in `ArgumentAlignmentContributor.AlignToOpenToken`/`AlignDictionaryKey` and `AlignToFirstElement` — these also place unheld commas that start a line: in tuple expressions (`(1⏎, 2)` is not moved, checked), in tuple types, in dictionary keys (FMT-INDENT-014 example), in element-access and angle-bracket lists that a comment elsewhere keeps from collapsing, and in calling-convention lists. FMT-INDENT-012's "every separator" stays.
- `LineBreakListRewriter.CanSafelyCollapseInteriorList` / `SyntaxNodeUtilities.InteriorContainsCommentOrDirective` — refuse for any comment in the interior.
- `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps`'s trivia filter — the colon gap and gaps inside one constraint.
- `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`'s `EndsLine` branch — also serves an unheld comma-first trailing comma (`2⏎, // after⏎}`, checked); `ContainsConditionalCompilationBoundary` guard — about the gap behind the comma.
- FMT-STRUCT-028's "no split with a comment or directive between the brackets", and `FieldDeclarationSplitTransform.CarriesDirective` — any position in the list or declaration.
- `ListElementAligner.AlignToFirstElement`'s zero-width-anchor return — also `Dictionary<⏎,>` without a comment.
- Stranded `///` handling (`StrandedDocumentation*` tests hold `1 /// stray⏎, 2`) — general handling for a `///` with its own line break in front of any token.
- RH5107 analyzer guard (`WouldJoinAcrossUnjoinableTrivia(previousParameter.GetLastToken(), comma)`), its not-reported requirement, and SHARED-JOIN-001 — analyzers do not consult the precheck (FMT-UNSUP non-goal). RH5107's fix is a text edit outside the formatter, so SHARED-FIX-010 would not withhold it, and dropping the guard would offer a fix that moves the comma across the comment (SHARED-FIX-003, FMT-BREAK-001). RH5102/RH5109 `ContainsConditionalCompilationBoundaryInElementGaps` and RH5203's first-on-line check are broader than the shape.
- RH5107-004 (this run) — a comment *behind* the comma: a different shape, not affected.
- Code fixes: none has its own held-comma special case (`TrailingCommaCodeFixHelper` is line-based and generic; RH6002's `AnalyzeSpacing` is generic). The formatter-delegating fixes need no change: SHARED-FIX-010 withholds them for a node that holds the shape.
- Nothing to delete for enum member lists (the formatter keeps their comma layout as written: `A // k⏎, B⏎, C` and `A⏎, B` both unchanged, checked), base lists, switch arms, positional patterns, or same-line spacing (`CommaSpacingRule` has no comment case). The follow-up candidates about unheld commas in switch arms, positional patterns, and base lists stay.

Size: about 15 production lines deleted on `main`, plus the six contributor additions, the held half of FMT-BREAK-038's rewriter, and FMT-INDENT-033 that this run would not write. The larger part is spec text and tests. Affected test classes found by search: `ListWrapByElementsTests`, `ConstraintClauseLineBreakTests`, `ConstraintClauseAlignmentTests`, `TrailingCommaCommentOwnerTests`, `ArgumentContinuationLineAlignmentTests`, `ParameterContinuationLineAlignmentTests`, `NestedListInParameterAlignmentTests`, `StrandedDocumentation*Tests`, `ZeroWidthTokenLineStartTests`, the RH5107/RH5203 formatter tests, plus the FMT-BREAK-011/012/037 requirement tests. Their held cases turn into refusal tests or are deleted.

### 3. Rarity and commonness

- **Comment variant, reported by a rule (rare by definition):** parameter lists including lambdas and function pointers (RH5035), argument lists including element access and attribute arguments (RH5038), declarator lists (RH5041, already FMT-UNSUP-004), base lists (RH5043), attribute lists (RH5044), type parameter, type argument, and tuple type lists (RH5046).
- **Comment variant, reported by no rule:** object, collection, array, anonymous, and `with` initializers, collection expressions, list/property/positional patterns, tuple expressions, enum members, switch arms, constraint clauses, `for` expression lists. It occurs only in comma-first code with a comment behind the element or on a line above the comma. Comma-first is itself an uncommon C# style; this repository's production code has none (only tests do).
- **Directive/disabled-text variant, reported by no rule:** the comma-first conditional element `b⏎#if DEBUG⏎, c⏎#else⏎, d⏎#endif` is refused wherever the active branch holds the comma (CLI: `#else` and `#if !X` branches; IDE: whatever configuration is active). It is an uncommon but deliberate idiom: argument and parameter lists allow no trailing comma, so it is the way to add a conditional argument without repeating the call. Enums and initializers usually write conditional members comma-last (`B,⏎#if X⏎C,⏎#endif`), which is not the shape. This is the main cost, and what the maintainer should weigh.
- **What refusal means:** the whole file is left unformatted, not only the list. Every formatter-delegating code fix whose formatted node holds the shape is withheld (SHARED-FIX-010), e.g. RH5301 on `new D { X = 3 // k⏎, Y = 4 }`, and the list and constraint layout fixes on lists that hold it. Today `main` formats the argument, parameter, and constraint variants correctly and stably, so for them declaring is a regression of output users get today; the initializer variant is broken on `main` (statement column), which is #1738.

### 4. Timing

- **T1 — declare in this run** (recommended). The shape is half of what #1738 is about: the held comma in initializers and batch 1b. Declaring later would mean writing FMT-INDENT-033 and the held half of FMT-BREAK-038 now and deleting them in the next change. The run grows by the five `main` deletions, the spec edits above, the test rewrites, and a change doc with *Special cases*. The lane stays behavior change.
- **T2 — a change of its own.** Pick this if the lead wants the PR to touch only what #1737/#1738 name. This run then drops FMT-INDENT-033 and FMT-BREAK-038's held paragraph and leaves held commas in initializers as on `main` (statement column) until the declaration lands. It must not write the held handling and then delete it.
- If the maintainer does not declare: the run proceeds as specified at 19e8ee2 (FMT-INDENT-033 and the held paragraph stay).

### Output questions

For the lead to ask, in `AGENTS.md`'s format. `Before (main)` was produced by `scripts/format.sh`, run twice and stable.

**1 · Formatter — comment or directive between a list element and the comma behind it**

Input:
```csharp
Foo(1 // first
    , 2);
var e = new D { X = 3 // k
    , Y = 4 };
Foo(a, b
#if DEBUG
    , c
#else
    , d
#endif
    );
```

Before (`main`):
```csharp
        Foo(1 // first
            , 2);

        var e = new D
                {
                    X = 3 // k
        ,
                    Y = 4
                };

        Foo(a,
            b
#if DEBUG
            , c
#else
            , d
#endif
            );
```

After, A (recommended): the file is unchanged.
`error: <file>(<line>,<col>): unsupported syntax: a comment or directive between a list element and the comma behind it. The file was left unchanged.`
Code fixes that format code holding such a comma, such as the initializer and argument-list layout fixes, are no longer offered there.
Declared in this run (2A) or in a change of its own (2B).

After, B — not declared; this run's layout:
```csharp
        Foo(1 // first
            , 2);

        var e = new D
                {
                    X = 3 // k
                    ,
                    Y = 4
                };

        Foo(a,
            b
#if DEBUG
            , c
#else
            , d
#endif
            );
```

Why A: such a comma is rare — a comment there is already reported in parameter, argument, attribute, base, and type lists — and refusing it removes the formatter's handling of commas held at the start of a line in every list instead of adding another copy of it for initializers.
Special cases it deletes: the argument and parameter lists' keeping of a comma that a comment holds at the start of a line, the same for constraint clauses (line break and column), the carry-over of a comment written in front of a removed trailing comma, and this run's column and line rules for such a comma in initializers, collection expressions, and patterns.

**2 · When** — **2A** (recommended) in this run; **2B** in a change of its own, with this run leaving held initializer commas as on `main` until then. Why 2A: otherwise this run writes the initializer handling that the declaration deletes.

A comments-only shape is not offered as an option: it deletes no code, because directive-held commas need every guard above.

### Internal choices

- Line condition counted by text lines, not by SHARED-BLANK-008. Only text lines make the guard in `CollapseSeparatorsToPreviousElementLine` unreachable; the difference is `Foo(1 /* a⏎ b */, 2)`, which is refused under text lines and kept under SHARED-BLANK-008. Recommend text lines.
- The token in front of the comma: either reading of a zero-width omitted element refuses the same inputs (the comment lies in the comma's leading trivia either way); no visible difference.
- The indentation non-goal added at 19e8ee2 ("FMT-INDENT-033 covers…"): under A, reword it to name the unheld comma-first lines of switch arms, positional patterns, and base lists without FMT-INDENT-033.

### Follow-up candidates

_None new._ The switch-arm, positional-pattern, and base-list follow-ups in `00-lead.md` are about unheld commas and are not affected.
