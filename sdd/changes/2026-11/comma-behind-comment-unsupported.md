# A comment or directive in front of a comma on a later line is unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-009 (added), FMT-BREAK-011 (changed: no held comma), FMT-BREAK-012 (changed: held-comma paragraph and examples deleted), FMT-BREAK-037 (changed: held comma deleted from the joins and the examples), FMT-INDENT-032 (changed: no comma line to place), FMT-STRUCT-027 (changed: no comment leads a removed comma on a line of its own), RH5110-003 (changed: the fix is withheld for the shape) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md), [A comma that starts a line in an initializer](initializer-leading-comma.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-009): a comment, a preprocessor directive, or disabled text between a comma and the token in front of it, with the comma on a later line of the text than the line that token ends on. It holds for every comma token of the code — separators and trailing commas, in every kind of list — and counts every line break of the text, also one inside a multi-line comment. Every entry point refuses an input that holds the shape and reports it with the reason `a comment or directive between a list element and the comma behind it`; the CLI leaves the file unchanged, reports it as an error, and counts it as skipped (CLI-019); and every code fix whose formatted node holds it is withheld (SHARED-FIX-010), RH5110's among them (RH5110-003).

Not refused: a comment behind a comma (`1⏎, /* c */ 2`, `1, // c`), a comment in front of a comma on the element's own line (`Foo(true /* force */, false)`), a comma at the end of its element's line in front of a directive (`2,⏎#if X⏎3,⏎#endif`), and a comma in disabled text, which is no token of the build being formatted.

The phases lose the handling of a comma that such trivia keeps at the start of a line — in argument, parameter, and attribute argument lists, in constraint clauses (line break and column), and in the removal of a trailing comma — and this run does not write the same handling for initializers, collection expressions, and patterns (*Special cases*).

## Why

A comma that a comment or a directive keeps off its element's line cannot move behind that element without crossing it (FMT-BREAK-001), so every phase that places commas carried a guard that found such a comma and left it as written, and thoenissen/Reihitsu#1738 would have added a further copy — a line rule and a column of its own — for the initializer family. The shape is rare: in parameter, argument, attribute, base, and type lists a rule already reports the comment (RH5035, RH5038, RH5043, RH5044, RH5046), and elsewhere it takes comma-first code with a comment or a directive written above the comma. Refusing it removes the shape from every phase at once.

## Visible output changes

Input:

```csharp
class C
{
    void M<T>(int a // first
              , int b)
        where T : class // k
            , new()
    {
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
        int[] g = new[] { 1,
            2 /* two */
            , };
    }
}
```

Before (`main`, `scripts/format.sh`, run twice and stable):

```csharp
class C
{
    void M<T>(int a // first
              , int b)
        where T : class // k
                  , new()
    {
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

        int[] g = new[] { 1,
                      2 /* two */
                  };
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(3,21): unsupported syntax: a comment or directive between a list element and the comma behind it. The file was left unchanged.
```

Code fixes that format code holding such a comma are no longer offered there (SHARED-FIX-010), for example RH5110's on the `where` clause of `M` and RH5301's on the initializer of `e`; the diagnostics stay. The analyzers do not change: RH5107 still does not report a comma that a comment keeps off its parameter's line (SHARED-JOIN-001), and analyzers do not consult the formatter's precheck.

No rule page changes: RH5110 loses its fix only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Decisions

### Declare the shape (maintainer, escalation 10, question 4: A)

The maintainer considered declaring "a comment between a list element and its following comma" unsupported ("no comment belongs there") and asked which special cases it would delete. The skeptic found that a comments-only shape deletes nothing — every guard is one predicate over comments, directives, and disabled text, and a directive-held comma (`b⏎#if DEBUG⏎, c⏎#else⏎, d⏎#endif`) reaches every one of them through the CLI — so only a shape that includes directives and disabled text is worth declaring (`AGENTS.md`: a shape that deletes nothing is not declared). The maintainer answered A: declare that shape, with the definition of the skeptic's handover (`sdd/work/comma-comment-layout/04-skeptic.md`, §1). Its main cost is the comma-first conditional argument, an uncommon but deliberate idiom, which the formatter now refuses wherever the active branch holds the comma.

### In this run (maintainer, escalation 10, question 5: A)

The shape is half of what thoenissen/Reihitsu#1738 is about: the comma that a comment holds at the start of a line in an initializer. Declaring it later would mean writing FMT-INDENT-033 and the held half of FMT-BREAK-038 now and deleting them in the next change. This answer supersedes the maintainer's batch-1 answer to question 2 ("not declared") for this shape, and makes the batch-1b answer (a held comma in an array keeps `, 2` on its line) moot.

### Every comma token, not a list of list kinds

The shape names every comma of the code, so it is one rule with no list of constructs. Restricting it to the lists that carry a special case would only spare refusals in lists where nothing is deleted — enums, tuples, switch arms, positional patterns, base lists, type argument lists, `for` lists — whose shape is just as rare.

### Lines of the text, not SHARED-BLANK-008

A comma behind a multi-line comment whose text spans a line break (`Foo(1 /* a⏎ b */, 2)`) is refused. The comma move tests the gap with a text-line test, and only that counting makes its comment guard unreachable; under SHARED-BLANK-008 counting the input stays supported and the guard has to stay.

### FMT-UNSUP-004 keeps its reason

A comment in front of a comma between two declarators of a variable declaration is refused by FMT-UNSUP-004 already. FMT-UNSUP-001 gives the reason of the shape that starts first in the text, and here both start at the same comment; the reason stays FMT-UNSUP-004's, which is what such a declaration reports today. A directive or disabled text in that gap, which FMT-UNSUP-004 does not cover, is refused under FMT-UNSUP-009.

Only a comma between two declarators keeps FMT-UNSUP-004's reason. Every other comma in a declaration's span — in its type, such as a tuple type (`(int A // c⏎, int B) t = default;`) or a function pointer type (`delegate*<int // c⏎, void> _f;`), or in an attribute argument list — is refused with FMT-UNSUP-009's reason where a comment in front of it holds both shapes. Those commas separate the elements of a list like any other, and FMT-UNSUP-009's reason names what the user has to move; the declarator comma is the one place where FMT-UNSUP-004 already gave a reason before this change. Alternative rejected: FMT-UNSUP-004's reason for every comma in a declaration's span, which ties the message to where the list stands rather than to the comma.

## Special cases

### Deleted

- `LineBreakListRewriter.CollapseSeparatorsToPreviousElementLine`: the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, separator)` refusal and its documentation sentence. It is reached only after `TokenGapUtilities.HasLineBreakBetween(previousToken, separator)`, a text-line test, so every comment, directive, and disabled text it finds is in FMT-UNSUP-009's span.
- `LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine`: the `LineBreakTriviaUtilities.HasLeadingEndOfLine(separator)` operand, by which a held separator counted as a line break and was left as written, and its documentation sentences. It runs after the comma collapse in the argument, attribute argument, parameter, and bracketed parameter list visitors, and after the collapse only a held comma still starts its line.
- `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine`: the `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(constraint.GetLastToken(), separator)` operand and its documentation sentence on a held comma. After `JoinGap` only a comma whose gap holds a comment, a directive, or disabled text is still separated from its constraint.
- `GenericConstraintContributor`: the `ListElementAligner.AlignSeparators(constraint.Constraints, …)` call. A comma between two constraints starts a line only when it is held; unheld ones are joined behind their constraint.
- `TrailingCommaRemovalUtilities.GetTriviaToPreserve`: the branch that keeps the comma's leading trivia, and `ContainsNonFormattingTrivia`. A comma's leading trivia holds a comment, a directive, or disabled text only below a line break, so the comma stands on a later line than the element in front of it and the input is refused.
- Spec text: FMT-BREAK-011's exception for a comma kept off the previous line; FMT-BREAK-012's held-comma paragraph, its SHARED-BLANK-008 sentence on `Foo(1 /* a⏎ b */, 2,⏎3)`, the matching `Why:` sentences, and the held parts of its example (`N`, `P`, `3 // keep⏎, 4`, the `#if X` list); FMT-BREAK-037's held comma in its joins and the `P`, `Q`, and multi-line-comment `O` examples; FMT-INDENT-032's comma lines and the held `O` example; FMT-STRUCT-027's comment that led the comma on a line of its own and the `// about B` and `/* two */ // before comma` parts of its example.
- Never written, from this run's earlier spec diff (19e8ee2): FMT-BREAK-038's held paragraph and its third example, and all of FMT-INDENT-033 (retired), with the separator alignment it needed in `ObjectInitializerContributor`, `AnonymousObjectContributor`, `CollectionExpressionContributor`, `ListPatternContributor`, and `RecursivePatternContributor`.

Any node-level input that holds a comma holds the list node the comma belongs to, and with it the element in front of the comma and the whole gap, so no input that reaches the phases holds part of the shape (FMT-UNSUP-003). No phase writes a comment, a directive, or disabled text into that gap: the comma moves carry only whitespace into it, and the trailing-comma removal leaves no comma behind the element (FMT-UNSUP-005).

### Checked and kept

- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and the refusal in `LineBreakTriviaUtilities.CollapseTokenToSameLine`: about twenty other callers (parentheses, operators, chains, assignments, accessors, the single-line join of bracketed argument lists).
- `ListElementAligner.AlignSeparators` and its calls in `ArgumentAlignmentContributor.AlignToOpenToken`, `AlignDictionaryKey`, and `ListElementAligner.AlignToFirstElement`: they also place unheld commas that start a line — in tuple expressions and tuple types, dictionary keys, element-access and angle-bracket lists that a comment elsewhere keeps from joining, and calling-convention lists. FMT-INDENT-012's "every separator" stays.
- `LineBreakListRewriter.CanSafelyCollapseInteriorList` and `SyntaxNodeUtilities.InteriorContainsCommentOrDirective`: they refuse a join for a comment anywhere in the interior, not only in front of a comma.
- `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps`' trivia filter: it serves the colon's gap and the gaps inside one constraint.
- `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`'s branch for a comma that ends its line: it also serves an unheld comma-first trailing comma (`2⏎, // after⏎}`, FMT-STRUCT-027's remaining example); the `ContainsConditionalCompilationBoundary` guard concerns the gap behind the comma.
- FMT-STRUCT-028's "no split with a comment or a directive between the brackets" and `FieldDeclarationSplitTransform.CarriesDirective`: they hold for any position in the list or the declaration.
- `ListElementAligner.AlignToFirstElement`'s return for a zero-width anchor: it also serves `Dictionary<⏎,>` without a comment.
- The handling of a stranded `///` comment (`1 /// stray⏎, 2`): it serves a `///` comment with its own line break in front of any token.
- RH5107's analyzer guard (`WouldJoinAcrossUnjoinableTrivia(previousParameter.GetLastToken(), comma)`), its not-reported case, and SHARED-JOIN-001: analyzers do not consult the precheck, and RH5107's fix is a text edit outside the formatter, which SHARED-FIX-010 does not withhold, so without the guard it would offer a fix that moves the comma across the comment (SHARED-FIX-003, FMT-BREAK-001). RH5102's and RH5109's `ContainsConditionalCompilationBoundaryInElementGaps` and RH5203's first-on-line check are broader than the shape.
- RH5107-004 (thoenissen/Reihitsu#1737): a comment behind the comma, a different shape.
- The code fixes: none has a held-comma special case of its own (`TrailingCommaCodeFixHelper` is line-based and generic; RH6002's `AnalyzeSpacing` is generic), and the formatter-delegating fixes are withheld for the shape by SHARED-FIX-010 without a change.
- Enum member lists, base lists, switch-expression arms, positional patterns, and same-line comma spacing: the formatter holds no comment special case for their commas, so nothing is deleted there.

## Alternatives considered

- **Do not declare the shape** (escalation 10, question 4, B) — rejected by the maintainer: the run would write the held-comma line and column for initializers, collection expressions, and patterns, a further copy of the handling every other list already carries.
- **Declare comments only** — not offered: directive-held commas reach every guard, so the declaration would delete nothing.
- **Declare it in a change of its own** (escalation 10, question 5, B) — rejected by the maintainer: this run would leave held initializer commas at the statement's column until then, or write handling the declaration then deletes.
- **Only the lists that carry a special case** — rejected: a list of list kinds is a longer statement that spares only refusals of an equally rare shape where nothing is deleted.
- **Count line breaks as SHARED-BLANK-008 does** — rejected: the comma move's guard would have to stay for `Foo(1 /* a⏎ b */, 2)`.
- **Also refuse a comment in front of a comma on the element's line** — rejected: it refuses common code such as `Foo(true /* force */, false)` and annotated data tables, and deletes nothing more, because every guard is reached only with a line break in the gap.
