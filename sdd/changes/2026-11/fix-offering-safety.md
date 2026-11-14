# Code another build hides, and comments behind a conditional's operators

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-003, FMT-PIPE-005, FMT-PIPE-033, FMT-UNSUP-001, FMT-UNSUP-002, FMT-UNSUP-008, FMT-BREAK-018, SHARED-DIAG-010, SHARED-FIX-003, SHARED-FIX-011, SHARED-FIX-012, SHARED-ORDER-008, RH5308-005, RH5308-006, CLI-014, CLI-015, CLI-021, PLAY-009 |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported](unsupported-shape-as-refusal.md) |

## What

**Text that another build compiles as code.** A string literal or a delimited comment that spans lines, starts inside a conditional group, and holds a line that another build reads as an `#elif`, `#else`, or `#endif` directive — a *hiding carrier* — hides code from the build being formatted or analyzed: in the build that excludes the carrier's branch, that text is code, and the code behind the carrier can be the content of a string or a comment of that build. Two components change for it:

- Whole-document formatting (`FormatDocumentAsync`, `FormatSyntaxTree`) leaves a document that holds a hiding carrier unchanged, as it leaves a document with a syntax error; the document is not reported as unsupported (FMT-PIPE-033). The CLI leaves such a file unchanged and counts it as skipped, with a message of its own, and the Playground shows the input unchanged with a message of its own, as both do for a syntax error (CLI-021, PLAY-009).
- No code fix is offered in such a document, for any diagnostic, and a fix that changes other documents too, such as a rename, is not offered when one of them holds a hiding carrier. Every fix learns this when it registers: the check of the diagnostic's own document is made once, before any fix registers, and a fix that changes other documents checks those as well, all by the definition of FMT-PIPE-033 (SHARED-FIX-012). A Fix All, at any scope, leaves such a document unchanged also where it applies fixes without registering them, and fixes the other documents in its scope; the same holds for a document with a syntax error and a fix whose result the formatter lays out (SHARED-FIX-011). The diagnostics are still reported.

A string or a comment that only an excluded branch opens stays a decided gap in both: the formatter formats, and the fixes edit, the code behind its opening (FMT-PIPE-033, SHARED-FIX-012).

**Comments behind a conditional's operators.** A comment, a directive, or disabled text between a conditional expression's `?` and its `true` branch, or between its `:` and its `false` branch, is a shape the formatter declares unsupported (FMT-UNSUP-008), also a comment behind the operator with the branch on the operator's line. Every entry point refuses an input that holds it and reports it; the CLI leaves the file unchanged, reports it as an error, and exits with code 2 (CLI-017, CLI-019); and every fix whose formatted node holds it is withheld (SHARED-FIX-010), RH5308's among them (RH5308-005). RH5308 keeps reporting such a conditional: a shape the formatter refuses as unsupported is not one it keeps (SHARED-DIAG-010). A comment written in front of the operator, on the line above the operator's line or behind the code before it, stays supported. The line-break phase's two refusals to move a branch onto its operator's line are deleted (*Special cases*).

**Fixes the formatter cannot lay out.** A code fix whose result a formatter entry point lays out is not offered in a file with a syntax error, where the formatter returns its input unchanged (FMT-PIPE-005); the diagnostic is still reported (SHARED-FIX-011). The RH5308 fix is not offered for a conditional that stands in no statement and no member, where its action would return the document unchanged (RH5308-006). Both are bug fixes against SHARED-FIX-005 and SHARED-FIX-007, not new behavior: SHARED-FIX-011 writes down the maintainer's 2026-11-06 decision recorded in #1150, and RH5308-006 is the fix for #1673.

**Fields of a partial type that only excluded code lays out.** Nothing changes: the gap SHARED-ORDER-008 records stays, and its reason now names the build the analyzers analyze.

## Why

A fix or a formatter run that edits the code behind a hiding carrier changes the value of a string or the text of a comment in another build (FMT-PIPE-002, FMT-PIPE-003, SHARED-FIX-003, *Build configurations*), and where that build's string or comment ends is something no component may work out (SHARED-FIX-009, `AGENTS.md`, *Special cases do not grow the source*). Leaving the whole document alone is the one cheap refusal. The class is rare, and limiting it to carriers inside a conditional group that hold `#elif`, `#else`, or `#endif` keeps commented-out code with directives and raw strings with Markdown or generator templates formattable and fixable wherever they stand outside every conditional group, or hold no such line. Inside a conditional group, a carrier whose `#elif`, `#else`, or `#endif` lines close only groups that the carrier itself opens hides nothing either, but it counts: telling it apart means pairing directives inside the text, and the refusal only ever leaves such a file alone. The CLI and the Playground name the skipped file, as they name a file with a syntax error, so the user learns why a file that needs formatting was left as written.

A comment behind `?` or `:` kept the branch off its operator's line: the line-break phase refused to move the operator, left the conditional half wrapped, and RH5308 reported it again after its fix, which could not converge. Declaring the shape removes it from every phase, and a comment that explains a branch belongs above the operator's line.

## Visible output changes

### 1. Whole-document formatting — a comment that is code in another build

```csharp
// input, formatted by the CLI, which defines no symbols
using System;

class C
{
    void M()
    {
#if !A
        /*
#else
        var s = @"
        */
        Action a = delegate ()
            {
                Console.WriteLine(1);
            }; //";
#endif
    }
}
```

```csharp
// before: the block's lines move from column 12 to column 8, which changes s in a build with A
        Action a = delegate ()
        {
            Console.WriteLine(1);
        }; //";
```

```csharp
// after: the input unchanged; nothing is reported as unsupported
```

The CLI, given the file as `C.cs` with `--check --verbose`, before:

```text
Not formatted: /work/C.cs

1 of 1 file(s) need formatting.
```

exit code 1. After:

```text
Skipped (code hidden in another build configuration): /work/C.cs

0 of 1 file(s) need formatting.
Skipped 1 file(s) with code hidden in another build configuration.
```

exit code 0. Without `--verbose`, the first line is not printed; the summary line is. In write mode the file is not written, also with `--utf8-bom`, and the summary reads `Formatted 0 of 1 file(s).` followed by the same skip line.

The Playground, given the same input, before: shows the formatted text, with no message. After: shows the input unchanged, and the message "The input contains code hidden in another build configuration and was left unchanged." below the button.

A carrier inside a conditional group whose directive lines close only groups it opens itself is left alone in the same way, although it hides nothing, such as a commented-out block with its own `#if … #endif` inside an `#if DEBUG` group:

```csharp
// input
class C
{
    int M(int a, int b)
    {
#if DEBUG
        /*
#if TRACE
        Log();
#endif
        */
#endif
        return a+b;
    }
}
```

```csharp
// before: formatted, `return a + b;`
```

```csharp
// after: the input unchanged, and the CLI and the Playground name it as skipped as above
```

### 2. Code fixes — every fix in a document with such a carrier

```csharp
// input, analyzed in a build with A defined
using System;

class C
{
    int M(int a, int b)
    {
#if A
        /*
#else
        var s = @"
        */
        Action c = {|RH3004:delegate|} ()
            {
                Console.WriteLine(1);
            }; //";
#endif
        return a{|RH6024:+|}b;
    }
}
```

```csharp
// before: both fixes are offered; RH3004 rewrites the anonymous method into a lambda and moves its lines,
// which changes s in a build without A, and RH6024 writes `return a + b;`
```

```csharp
// after: both diagnostics are reported, and no code fix is offered for either
```

The same input with the same conditional group around a commented-out `#if … #endif` block of its own, as in #1, also withholds every fix.

### 3. A string that only an excluded branch opens

None — the formatter and the fixes keep editing the code behind it, as today; the gap is recorded in FMT-PIPE-033 and SHARED-FIX-012.

### 4. A comment or directive between a conditional's operator and its branch

```csharp
// input
class C
{
    int M(bool a)
    {
        var x = a ? // c
            1
            : 2;

        return x;
    }
}
```

```csharp
// before: formatted as below, RH5308 still reports the `?`, and its fix changes nothing more
        var x = a ? // c
        1
                    : 2;
```

```csharp
// after: the input unchanged, and the CLI reports on standard error
// error: <path>(5,21): unsupported syntax: a comment or directive between a conditional operator and its branch. The file was left unchanged.
// and exits with code 2, in write mode and with --check or --dry-run; with --suppress-unsupported the line starts with
// `warning:` and the exit code is unaffected.
// RH5308 reports the `?`, and no code fix is offered.
```

The same holds for the `:` (`? 1 : // c`), for a directive and disabled text behind the operator, and for a comment behind the operator that does not end its line. A comment between the operator and its branch on the same line is refused as well, also where nothing reports the conditional and the formatter keeps it as written today:

```csharp
// input
class C
{
    int M(bool a)
    {
        var x = a ? /* yes */ 1 : /* no */ 2;
        var y = a
                    ? /* yes */ 1
                    : /* no */ 2;

        return x + y;
    }
}
```

```csharp
// before: formatted as written; RH5308 reports nothing, and every fix is offered
```

```csharp
// after: the input unchanged, and the CLI reports
// error: <path>(5,21): unsupported syntax: a comment or directive between a conditional operator and its branch. The file was left unchanged.
// and exits with code 2. Every fix whose formatted node holds either conditional is withheld, such as the RH7101 fix for a field declaration in this type.
```

A comment behind the operator of a conditional that is not laid out yet, which RH5308 reports and which is formatted and fixed today:

```csharp
// input
class C
{
    int M(bool a)
    {
        var z = a ? /* c */ 1
                : 2;

        return z;
    }
}
```

```csharp
// before: formatted, and the RH5308 fix gives the same in one application
        var z = a
                    ? /* c */ 1
                    : 2;
```

```csharp
// after: the input unchanged and reported as unsupported, with exit code 2; RH5308 reports the `?`, and no fix is offered
```

Every other code fix whose formatted node holds such a conditional is no longer offered either (SHARED-FIX-010), such as the RH7101 fix for a field declaration in a type that holds one.

A comment in front of the operator is not affected, and is formatted as today, with the blank line in front of a comment line that FMT-BLANK-011 writes:

```csharp
// input
class C
{
    int M(bool a)
    {
        var x = a
                    // c
                    ? 1
                    : 2;

        return x;
    }
}
```

```csharp
// before and after
        var x = a

                    // c
                    ? 1
                    : 2;
```

### 5. Fields of a partial type that only excluded code lays out

None — the RH7103, RH7109, and RH7309 fixes stay offered as today.

### 6. Code fixes in a file with a syntax error

```csharp
// input: the parameter list of N is not closed
class C
{
    int M(bool a)
    {
        var x = a {|RH5308:?|}
                1 :
                2;

        return x;
    }

    void N(
}
```

```csharp
// before: the RH5308 fix is offered, and applying it changes nothing, because the formatter returns the document unchanged
```

```csharp
// after: RH5308 reports the `?`, and no code fix is offered
```

The same holds for every fix whose result a formatter entry point lays out, also one that edits first and then formats its edit, such as the RH3004 fix, which before wrote its lambda for `Action a = delegate () { M(); };` unformatted in such a file and after is not offered. A fix that changes the text without a formatter entry point stays offered: the RH6023 fix still writes `a = 1;` for `a=1;` in the same file.

### 7. The RH5308 fix for a conditional outside any statement or member

```csharp
// input
[assembly: System.CLSCompliant(true {|RH5308:?|}
    true :
    false)]
```

```csharp
// before: the fix is offered, and applying it changes nothing
```

```csharp
// after: RH5308 reports the `?`, and no code fix is offered
```

## Decisions

### Whole-document formatting leaves such a file alone

Maintainer's answer to question 1, option B. The file is returned unchanged like a file with a syntax error and is not reported as unsupported. Declaring the shape unsupported (option A) was not chosen: no phase has a special case for it, so declaring it would delete nothing. Keeping the formatting (option C) would keep changing strings in the other build.

Where a document holds both a hiding carrier and a shape declared unsupported, FMT-PIPE-033 applies first, as FMT-PIPE-005 does, and the document is not reported as unsupported; the CLI and the Playground name it as a file whose code another build hides (CLI-021, PLAY-009).

### The CLI and the Playground name the skipped file

Maintainer's answer to question 8, option B. The CLI counts the file as skipped and prints `Skipped (code hidden in another build configuration): <path>` with `--verbose` and `Skipped <n> file(s) with code hidden in another build configuration.` in the summary, and the Playground shows "The input contains code hidden in another build configuration and was left unchanged.", as both do for a syntax error (CLI-021, PLAY-009). Counting the file as unchanged (option A) would tell the user that a file the formatter did not look at is clean. The file does not affect the exit code, as a file with a syntax error does not: the tool did its job by leaving the file as written, and nothing in the file is wrong in the build being formatted. With `--utf8-bom` the file is not rewritten either, as a file with a syntax error is not. Both components decide it by the definition of FMT-PIPE-033, not by one of their own.

### A string that only an excluded branch opens stays a decided gap

Maintainer's answer to question 2, option A. Finding it means reading excluded text as a compiler would, and the only cheap refusal would withhold fixes and formatting in every file whose `#if DEBUG` branch holds an ordinary verbatim string.

### One check for the diagnostic's document, before any fix registers

Maintainer's answer to question 3, option B: a common base for the code-fix providers checks the diagnostic's document before any fix registers, instead of a guard in each fix. The maintainer approved the new type for it. A fix that changes other documents, such as a rename, checks those when it computes them, by the same definition. The check withholds every fix in the document, because neither the diagnostic nor the fix's own edit bounds where a fix writes in the other build's string: a rename reaches every reference, and a move writes where the member lands.

### The lines that count are `#elif`, `#else`, and `#endif`

A carrier hides code only when it holds the next `#elif`, `#else`, or `#endif` of the group it starts in; without one, the other build skips it whole. A plain `#` line, such as a Markdown heading in a raw string, does not count, and a carrier outside every conditional group is a carrier in every build. Both narrowings only offer more fixes and format more files than a check for any `#` line. A carrier inside a conditional group whose directive lines close only groups that it opens itself still counts, although it hides nothing: telling it apart means pairing `#if` with `#endif` inside the text, which edges toward the parser-like rules over text that *Special cases do not grow the source* rules out, and the class is rare and only ever refused.

### A comment or directive behind a conditional's operator is declared unsupported in this work

Maintainer's answer to question 4, option C. Comments are supported in front of the `?` or `:`, on the line above the operator's line, where they do not break formatting. The shape is the subject of the work's issue, so it is declared here and not in a change of its own. RH5308 still reports the conditional, as every rule does for a shape the formatter refuses, and its fix is withheld (RH5308-005); SHARED-DIAG-010 now says that a shape the formatter refuses as unsupported is not a shape it keeps, because the formatter asks the user to rewrite it.

Every comment between the operator and its branch is refused, also one with the branch on the operator's line, such as `a ? /* yes */ 1 : /* no */ 2` (maintainer's answer to question 9, option A): comments are supported in front of the operator only.

### A fix the formatter cannot lay out is not offered

The maintainer's decision of 2026-11-06, recorded in #1150: in a file with a syntax error the diagnostic stays and the fix is not offered, and a formatter-delegating fix that changes nothing is a defect of the formatter or of the diagnostic, unless a named formatter requirement declines the input (SHARED-FIX-011). It is a bug fix against SHARED-FIX-005 and SHARED-FIX-007, so it was not asked as an output question of this run.

### The RH5308 fix is withheld where it changes nothing

A bug fix against SHARED-FIX-005, for #1673 (RH5308-006): the fix formats the statement or member that holds the conditional, and a conditional in an `[assembly:]` or `[module:]` attribute argument has neither. Formatting the attribute list as a target of its own would be a new path for a rare input, so the conditional is laid out by hand. As a bug fix, it was not asked as an output question of this run.

### The partial-type gap stays

Maintainer's answer to question 5, option A. The analyzers analyze the build whose symbols the project defines as they stand, which is the common case, and the gap SHARED-ORDER-008 records stays a decision.

## Special cases

### Deleted

- The refusal in `TernaryLineBreakRewriter.MoveQuestionTokenToNextLine` (`LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` between the `?` and the first token of the `true` branch): every comment, directive, and disabled text it found is in FMT-UNSUP-008's span.
- The refusal in `TernaryLineBreakRewriter.NormalizeColonTokenPosition` (`WouldJoinAcrossUnjoinableTrivia` between the `:` and the first token of the `false` branch): the same.

Any node-level input that holds a conditional expression holds both of its operators and the trivia behind them, so no input that reaches the line-break phase holds part of the shape (FMT-UNSUP-003).

### Checked and kept

- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` and FMT-BREAK-001: shared with every other join of the line-break phase.
- `LineBreakTriviaUtilities.EndLineInFrontOf` in the two operator moves: it keeps a comment behind the condition's or the `true` branch's last token in front of the operator, a place that stays supported.
- `ConditionalExpressionContributor`, `ConditionalOperatorSpacingRule`, and `ListElementInteriorJoinRewriter.IsOperatorWrap`: they hold no comment special case.
- The horizontal-spacing phase's next-token leading-comment guard and `NormalizeGapBehindLeadingComment`: generic, shared with every token pair.
- SHARED-COMMENT-011, which RH5308-002 uses: it judges a line that starts with a comment, such as a comment line above the operator's line, which stays supported.
- The documentation-comment phase's relocation of a `/** */` comment, which puts the comment's line break into the owning token's leading trivia so that the token in front does not end its line in the trivia: it names the line placement of a conditional operator as a reader it protects, but it serves every operator and every owning token, not only a conditional's.
- The RH5308 fix: it already asks `ReihitsuFormatter.FindUnsupportedSyntax` at registration (SHARED-FIX-010) and has no guard of its own; the RH5308 analyzer has no comment special case.

## Alternatives considered

- **Declare the hiding carrier unsupported** (question 1, A) — rejected by the maintainer: it would delete no special case.
- **Keep formatting such a file** (question 1, C) — rejected by the maintainer: it changes strings in another build.
- **Refuse every document whose disabled text opens a string or a comment** (question 2, B) — rejected by the maintainer: it refuses common code.
- **Guard each fix on its own** — rejected by the maintainer (question 3): about a hundred registration sites would each carry the same check.
- **Withhold only the fixes whose diagnostic lies behind the carrier** — rejected: a rename or a move writes behind the carrier from a diagnostic in front of it, and finding where the other build's string ends is SHARED-FIX-009 machinery.
- **Count any line that starts with `#`** — rejected: it withholds fixes and formatting for raw strings that hold Markdown, which people write.
- **Break the line in front of `?` and keep the comment behind it**, or **stop reporting the conditional** (question 4) — rejected by the maintainer in favor of declaring the shape.
- **Count such a file as unchanged in the CLI and show no message in the Playground** (question 8, A) — rejected by the maintainer: it tells the user that a file the formatter left alone is clean.
- **Pair the directives inside a carrier**, so that a carrier whose `#elif`, `#else`, and `#endif` lines close only its own groups does not count — rejected: it is a parser-like rule over text for a rare class that is only ever refused.
- **Refuse only a comment that ends the operator's line, a documentation comment, a directive, or disabled text** behind the operator, and keep formatting an inline `/* */` comment there (question 9, B) — rejected by the maintainer: comments are supported in front of the operator only.
- **Withhold the reorder of instance fields in every `partial` type** (question 5, B), or in a `partial` type whose document holds disabled text — rejected with the maintainer's answer A: the analyzers judge the build the project defines, and both refusals give away fixes on ordinary code.
