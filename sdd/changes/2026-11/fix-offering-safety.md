# Code another build hides, and comments behind a conditional's operators

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-003, FMT-PIPE-005, FMT-PIPE-033, FMT-UNSUP-001, FMT-UNSUP-002, FMT-UNSUP-008, FMT-BREAK-018, SHARED-FIX-003, SHARED-FIX-012, SHARED-ORDER-008, RH5308-005 |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported](unsupported-shape-as-refusal.md) |

## What

**Text that another build compiles as code.** A string literal or a delimited comment that spans lines, starts inside a conditional group, and holds a line that another build reads as an `#elif`, `#else`, or `#endif` directive — a *hiding carrier* — hides code from the build being formatted or analyzed: in the build that excludes the carrier's branch, that text is code, and the code behind the carrier can be the content of a string or a comment of that build. Two components change for it:

- Whole-document formatting (`FormatDocumentAsync`, `FormatSyntaxTree`, and so the CLI) leaves a document that holds a hiding carrier unchanged, silently, as it leaves a document with a syntax error; the document is not reported as unsupported (FMT-PIPE-033).
- No code fix is offered in such a document, for any diagnostic, and a fix that changes other documents too, such as a rename, is not offered when one of them holds a hiding carrier. Every fix learns this when it registers, from one check made before any fix registers, by the definition of FMT-PIPE-033 (SHARED-FIX-012). The diagnostics are still reported.

A string or a comment that only an excluded branch opens stays a decided gap in both: the formatter formats, and the fixes edit, the code behind its opening (FMT-PIPE-033, SHARED-FIX-012).

**Comments behind a conditional's operators.** A comment, a directive, or disabled text between a conditional expression's `?` and its `true` branch, or between its `:` and its `false` branch, is a shape the formatter declares unsupported (FMT-UNSUP-008). Every entry point refuses an input that holds it and reports it; the CLI leaves the file unchanged and reports it; and every fix whose formatted node holds it is withheld (SHARED-FIX-010), RH5308's among them (RH5308-005). A comment written in front of the operator, on the line above the operator's line or behind the code before it, stays supported. The line-break phase's two refusals to move a branch onto its operator's line are deleted (*Special cases*).

**Fields of a partial type that only excluded code lays out.** Nothing changes: the gap SHARED-ORDER-008 records stays, and its reason now names the build the analyzers analyze.

## Why

A fix or a formatter run that edits the code behind a hiding carrier changes the value of a string or the text of a comment in another build (FMT-PIPE-002, FMT-PIPE-003, SHARED-FIX-003, *Build configurations*), and where that build's string or comment ends is something no component may work out (SHARED-FIX-009, `AGENTS.md`, *Special cases do not grow the source*). Leaving the whole document alone is the one cheap refusal. The class is rare, and limiting it to carriers inside a conditional group that hold `#elif`, `#else`, or `#endif` keeps commented-out code with directives and raw strings with Markdown or generator templates formattable and fixable.

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
// after: the input unchanged; nothing is reported, and with --verbose the CLI prints `Unchanged: <path>`
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
// after: the input unchanged, and the CLI reports
// error: <path>(5,21): unsupported syntax: a comment or directive between a conditional operator and its branch. The file was left unchanged.
// RH5308 reports the `?`, and no code fix is offered.
```

The same holds for the `:` (`? 1 : // c`), for a directive and disabled text behind the operator, and for a comment behind the operator that does not end its line, which is formatted and fixed today:

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
// after: the input unchanged and reported as unsupported; no RH5308 fix is offered
```

Every other code fix whose formatted node holds such a conditional is no longer offered either (SHARED-FIX-010), such as the RH7101 fix for a field declaration in a type that holds one.

A comment in front of the operator is not affected:

```csharp
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

### 5. Fields of a partial type that only excluded code lays out

None — the RH7103, RH7109, and RH7309 fixes stay offered as today.

## Decisions

### Whole-document formatting leaves such a file alone silently

Maintainer's answer to question 1, option B. The file is returned unchanged like a file with a syntax error and is not reported. Declaring the shape unsupported (option A) was not chosen: no phase has a special case for it, so declaring it would delete nothing. Keeping the formatting (option C) would keep changing strings in the other build.

Where a document holds both a hiding carrier and a shape declared unsupported, FMT-PIPE-033 applies first, as FMT-PIPE-005 does, and the document is not reported.

### A string that only an excluded branch opens stays a decided gap

Maintainer's answer to question 2, option A. Finding it means reading excluded text as a compiler would, and the only cheap refusal would withhold fixes and formatting in every file whose `#if DEBUG` branch holds an ordinary verbatim string.

### One check before every fix registers

Maintainer's answer to question 3, option B: a common base for the code-fix providers runs the check before any fix registers, instead of a guard in each fix. The maintainer approved the new type for it. The check withholds every fix in the document, because neither the diagnostic nor the fix's own edit bounds where a fix writes in the other build's string: a rename reaches every reference, and a move writes where the member lands.

### The lines that count are `#elif`, `#else`, and `#endif`

A carrier hides code only when it holds the next `#elif`, `#else`, or `#endif` of the group it starts in; without one, the other build skips it whole. A plain `#` line, such as a Markdown heading in a raw string, does not count, and a carrier outside every conditional group is a carrier in every build. Both narrowings only offer more fixes and format more files than a check for any `#` line.

### A comment or directive behind a conditional's operator is declared unsupported in this work

Maintainer's answer to question 4, option C. Comments are supported in front of the `?` or `:`, on the line above the operator's line, where they do not break formatting. The shape is the subject of the work's issue, so it is declared here and not in a change of its own. RH5308 still reports the conditional, as every rule does for a shape the formatter refuses, and its fix is withheld (RH5308-005).

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
- The RH5308 fix: it already asks `ReihitsuFormatter.FindUnsupportedSyntax` at registration (SHARED-FIX-010) and has no guard of its own; the RH5308 analyzer has no comment special case.

## Alternatives considered

- **Declare the hiding carrier unsupported** (question 1, A) — rejected by the maintainer: it would delete no special case.
- **Keep formatting such a file** (question 1, C) — rejected by the maintainer: it changes strings in another build.
- **Refuse every document whose disabled text opens a string or a comment** (question 2, B) — rejected by the maintainer: it refuses common code.
- **Guard each fix on its own** — rejected by the maintainer (question 3): about a hundred registration sites would each carry the same check.
- **Withhold only the fixes whose diagnostic lies behind the carrier** — rejected: a rename or a move writes behind the carrier from a diagnostic in front of it, and finding where the other build's string ends is SHARED-FIX-009 machinery.
- **Count any line that starts with `#`** — rejected: it withholds fixes and formatting for raw strings that hold Markdown, which people write.
- **Break the line in front of `?` and keep the comment behind it**, or **stop reporting the conditional** (question 4) — rejected by the maintainer in favor of declaring the shape.
- **Withhold the reorder of instance fields in every `partial` type** (question 5, B), or in a `partial` type whose document holds disabled text — rejected with the maintainer's answer A: the analyzers judge the build the project defines, and both refusals give away fixes on ordinary code.
