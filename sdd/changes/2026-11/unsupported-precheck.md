# The formatter's Unsupported outcome

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP (new spec `formatter/unsupported-syntax.md`: FMT-UNSUP-001 to FMT-UNSUP-007), FMT-PIPE-001, FMT-PIPE-017, FMT-PIPE-027, FMT-PIPE-029, FMT-PIPE-031, FMT-STRUCT-022, FMT-STRUCT-023, FMT-STRUCT-026, FMT-STRUCT-030, FMT-INDENT-015, FMT-BLANK-018, FMT-SPACE-012, FMT-SPACE-014, FMT-DOC-003, RH2001-007, RH2001-009, RH7101-004, RH7101-006, RH7101-007, SHARED-FIX-010, CLI-001, CLI-014, CLI-017, CLI-019, CLI-020, PLAY-008 |

## What

The formatter gets an explicit *Unsupported* outcome. A new first phase, the precheck, checks the input the formatting phases receive against a list of shapes the formatter declares unsupported. When one matches, no other phase runs: the entry point returns its input unchanged and reports it as unsupported, with the reason and the location of the first such shape in the text (FMT-UNSUP-001, FMT-UNSUP-002). Every formatting entry point returns a result object, `FormattingResult<T>`, whose `Output` is the formatted or unchanged input and whose `Unsupported` carries the reason and the location, or is null (FMT-UNSUP-001). A phase may refuse an input during the run for a shape the precheck does not find, with the same outcome, produced in the same place; the refusal never escapes an entry point as an exception (FMT-UNSUP-006). The decision depends only on the tokens and trivia inside the input (FMT-UNSUP-003), formatted output is never unsupported (FMT-UNSUP-005), and `ReihitsuFormatter.FindUnsupportedSyntax` answers the precheck's question for a node with the precheck's own predicate (FMT-UNSUP-007).

The first unsupported shape is a comment inside a variable declaration (FMT-UNSUP-004): anywhere from the declaration's first token — its attributes and modifiers included — through its terminator, except the leading trivia of that first token, the trivia behind the `;`, and the inside of an initializer value. A comment in front of a value, also on its own line, between a value and the `,`, `;`, or `)` behind it, in front of the `;`, in an attribute's arguments, and on its own line in front of the `)` of a `using` or `fixed` statement is refused. A variable declaration inside a value is checked on its own.

The special cases this shape made necessary are removed from the specs and the formatter: the field split no longer carries comments at a comma or in front of a later declarator, and no longer refuses a declaration for a comment among its attributes, modifiers, or type, because the precheck refuses every such input first (FMT-STRUCT-022, FMT-STRUCT-026, FMT-STRUCT-030, FMT-PIPE-031). *Special cases* below lists what is deleted and what was checked and kept.

A code fix that formats asks the formatter at registration whether it refuses exactly the node or context the fix formats, and is withheld when it does (SHARED-FIX-010). The CLI never changes a file the formatter refuses and reports it on standard error as an error that makes the run exit with code 2 (CLI-019); `--suppress-unsupported` reports it as a warning that does not change the exit code (CLI-020). The Playground shows such an input unchanged with a message (PLAY-008).

## Why

Many formatter issues concern rare comment and directive shapes. Each one cost analysis, spec text, and special cases in the phases, and many ended in leaving the construct as written anyway — silently, and decided again for every case and every phase (`AGENTS.md`, *Special cases do not grow the source*). An explicit outcome lets a shape be refused once, in one place, and lets every phase drop the guards it needed for it; the user is told why a file was left as written. The goal is less formatter code, not more: each shape on the list has to let special cases go.

## Visible output changes

### 1. Formatter output — a comment inside a variable declaration refuses the whole input

```csharp
// input
class C{
    void M(){
        int /* c1 */ a=1;
        int b /* c2 */=1;
        int c=// c3
            1;
        int d=1, /* c4 */ e=2;
        int f, // c5
            g;
    }
}
```

```csharp
// before: formatted
class C
{
    void M()
    {
        int /* c1 */ a = 1;
        int b /* c2 */ = 1;
        int c =// c3
        1;
        int d = 1, /* c4 */ e = 2;
        int f, // c5
        g;
    }
}
```

```csharp
// after: the input unchanged, also the code that holds no such comment, and reported as unsupported
class C{
    void M(){
        int /* c1 */ a=1;
        int b /* c2 */=1;
        int c=// c3
            1;
        int d=1, /* c4 */ e=2;
        int f, // c5
            g;
    }
}
```

The same holds for a comment inside the type, among the attributes or the modifiers, on its own line in front of the value, and on its own line in front of the `;` (maintainer's answer to question 5, option B):

```csharp
// input
class C
{
    System.Collections.Generic.Dictionary<int, // first
        string> _map;

    private
        // c
        int _b;

    void M()
    {
        int c =
            // c
            1;
        int d
            // c
            ;
    }
}
```

```csharp
// before
class C
{
    System.Collections.Generic.Dictionary<int, // first
                                          string> _map;

    private

    // c
    int _b;

    void M()
    {
        int c =

        // c
        1;
        int d

        // c
        ;
    }
}
```

```csharp
// after: the input unchanged, reported as unsupported
```

The same holds for a comment between a value and the `,`, `;`, or `)` behind it (maintainer's answer to question 9, option A), for a comment on its own line in front of the `)` of a `using` or `fixed` statement (question 10, A), and for a comment in an attribute's arguments (question 11, A):

```csharp
// input
using System;

class C
{
    [Obsolete("old" /* why */)]
    private int _e;

    void M()
    {
        var timeout = 30 /* seconds */;
        int b = 1 /* c */, c = 2;

        using (var s = Open()
            // c
            )
        {
        }
    }

    IDisposable Open() => null;
}
```

```csharp
// before
using System;

class C
{
    [Obsolete("old" /* why */)]
    private int _e;

    void M()
    {
        var timeout = 30 /* seconds */;
        int b = 1 /* c */, c = 2;

        using (var s = Open()

        // c
        )
        {
        }
    }

    IDisposable Open()
    {
        return null;
    }
}
```

```csharp
// after: the input unchanged, reported as unsupported
```

A comment above the declaration, behind its `;` or the `)` of a `using` or `fixed` statement, or inside an initializer value or a lambda body — between the value's first and last token — keeps the input formatted as today (FMT-UNSUP-004).

### 2. Formatter output — fields with a comment inside the declaration are no longer split

```csharp
// input
class C
{
    private int _x, // horizontal
                /* vertical */ _y;
    private int _first = /* c */ 1, _second;
}
```

```csharp
// before
class C
{
    private int _x; // horizontal

    /* vertical */
    private int _y;
    private int _first = /* c */ 1;
    private int _second;
}
```

```csharp
// after: the input unchanged, reported as unsupported
class C
{
    private int _x, // horizontal
                /* vertical */ _y;
    private int _first = /* c */ 1, _second;
}
```

The same holds for `///` and `/** */` comments in front of a later declarator, which used to be moved in front of that declarator's field, in every documentation mode. A declaration with a comment among its attributes or modifiers, such as `private /* c */ int _a, _b;`, or in an attribute's arguments, such as `[System.Obsolete("old" /* why */)] private int _c, _d;`, was left unsplit and otherwise formatted, and is now refused with its input. A declaration with a comment between a value and the `,` behind it, such as `private int _a = 1 /* c */, _b;`, was split into `private int _a = 1 /* c */;` and `private int _b;`, and is now refused with its input (question 9, A).

### 3. Code fixes — withheld where the formatter refuses the node they format

Every code fix that formats through the formatter is withheld when the formatter refuses the node it formats, or, for a fix that formats in a context, the context (SHARED-FIX-010). For RH7101, which formats the declaration in its type, a comment inside any variable declaration of the type withholds the fix, also one far away from the reported declaration.

```csharp
// input — RH7101
class C
{
    private int _a, {|RH7101:_b|};

    void M()
    {
        int /* c */ x = 1;
    }
}
```

```csharp
// before
class C
{
    private int _a;
    private int _b;

    void M()
    {
        int /* c */ x = 1;
    }
}
```

```csharp
// after: reported, and no code fix is offered
```

```csharp
// input — RH7101, the old RH7101-004 example
class C
{
    private int _first, // first
                {|RH7101:_second|}; // second
}
```

```csharp
// before
class C
{
    private int _first; // first
    private int _second; // second
}
```

```csharp
// after: reported, and no code fix is offered
```

RH2001 builds a field from the property and formats the field; the fix is withheld when that field would hold a comment the formatter refuses (maintainer's answer to question 7, option A):

```csharp
// input — RH2001
class C
{
    private int /* note */ {|RH2001:Value|} { get; set; }

    private /* a */ static int {|RH2001:First|} { get; }
}
```

```csharp
// before
class C
{
    private int /* note */ _value;

    private /* a */ static readonly int _first;
}
```

```csharp
// after: both reported, and no code fix is offered
```

The same holds for `private int Value { get; set; } = /* five */ 5;`, which became `private int _value = /* five */ 5;`, and for `private int Value { get; set; } = 5 /* five */;`, which became `private int _value = 5 /* five */;` (question 9, A). The RH7105 and RH7106 fixes, which reorder a member's modifiers and format the member, are withheld for a field with a comment among its modifiers or in front of its type. The diagnostics themselves do not change. A fix that formats a node below a declaration, such as RH5303, RH5304, and RH5305 on an equals-value clause, stays offered when the comment is in the declaration but not in that node (FMT-UNSUP-003).

### 4. CLI

- A new option, `--suppress-unsupported` (CLI-001, CLI-020).
- A file the formatter refuses is never written, in any mode, also with `--utf8-bom`, and is reported on standard error (CLI-019); the summary counts it.

```shell
reihitsu-format /work/u.cs        # u.cs holds `int /* c */ a = 1;` at line 5, column 9
```

Before: `Formatted: /work/u.cs` when the rest of the file needed formatting, `Formatted 1 of 1 file(s).`, exit code 0.

After, on standard error:

```text
error: /work/u.cs(5,13): unsupported syntax: a comment between a variable declaration's own tokens. The file was left unchanged.
```

and on standard output:

```text

Formatted 0 of 1 file(s).
Skipped 1 file(s) with unsupported syntax.
```

exit code 2. With `--check` or `--dry-run`, the same `error:` line, no `Not formatted:` or `Would format:` line and no diff for the file, and the file is not counted as needing formatting; exit code 2, also when another file needs formatting. With `--suppress-unsupported`, the line starts with `warning:`, and the exit code is the one the run has without the file.

### 5. Playground

The Playground shows an unsupported input unchanged and, below the button, the message `The input contains syntax the formatter does not support and was left unchanged: line 5, column 13 — a comment between a variable declaration's own tokens.` (PLAY-008). Before, it showed the formatted code.

### 6. Formatter API

Every formatting entry point of `ReihitsuFormatter` returns `FormattingResult<T>` instead of the bare `Document`, `SyntaxTree`, or node; `FindUnsupportedSyntax` is new (FMT-UNSUP-001, FMT-UNSUP-007). No assembly of this repository is a public API (`AGENTS.md`, *Supported inputs*); the change is listed because the maintainer decided it.

### 7. Rule pages

`documentation/rules/RH7101.md`, *How to fix it*. The page no longer promises that the code fix carries the comments written on the declaration and on the individual variables over to the new fields: RH7101-007 withholds the fix for a comment inside the declaration. It says only that the fix performs the split; a fix withheld for a comment is covered by `documentation/rules/general-notes.md` ("A diagnostic without a code fix", and comments blocking a fix). No rule's report changes, and no other rule page promises what the fix now withholds.

Before:

> Split the combined declaration into one field declaration per variable. The code fix carries the comments written on the declaration and on the individual variables over to the fields it creates.

After:

> Split the combined declaration into one field declaration per variable. The code fix performs the split for you.

## Decisions

### The design decisions of the issue are binding

The maintainer decided, in the conversation that produced thoenissen/Reihitsu#1670 and again for this run: the precheck phase is the primary filter; a phase may throw `SyntaxNotSupportedException` for edge cases the precheck misses; the whole file is refused; the list of unsupported shapes belongs to the formatter and depends on no analyzer; `--suppress-unsupported` turns the error into a warning and the file stays unchanged; and the first shape is a comment in a variable declaration, which is not refused in the leading trivia of the declaration's first token, behind its `;`, or inside an initializer value.

### The issue's contract is binding

One check site (exactly what the phases receive, the context for `FormatNodeInDocumentWithContextAsync`), a local decision, closure, the exception boundary, cheap checks only with a requirement and a test for each throw site, code fixes that withhold themselves, and the removal of every special case the first shape makes unnecessary, in the same change.

### Every formatting entry point returns a result object (question 1, A)

`FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` return `FormattingResult<T>`: `Output`, and `Unsupported` — an `UnsupportedSyntax` with the reason and the location, or null when the input is formatted or left untouched by FMT-PIPE-005 or FMT-PIPE-006 (FMT-UNSUP-001). No caller can format and overlook a refusal. `OrganizeUsingDirectivesInDocumentAsync`, which refuses nothing, and `SplitsFieldDeclaration` keep their return types.

### The location is the first shape in the text

An input with several unsupported shapes reports the one that starts first in the text (FMT-UNSUP-001), so the CLI and the Playground print one location per file.

### The CLI's report, exit code, and modes (questions 2, 3, and 4, A)

One line on standard error, `error: <path>(<line>,<column>): unsupported syntax: <reason>. The file was left unchanged.`, with one-based line and column; `warning:` instead of `error:` with `--suppress-unsupported`; the summary line `Skipped <n> file(s) with unsupported syntax.` after the line for files with syntax errors (CLI-014, CLI-019). Without `--suppress-unsupported`, the run exits with code 2, also in `--check` and `--dry-run` when another file needs formatting (CLI-017). `--check` and `--dry-run` print the same line and nothing else for the file, and do not count it as needing formatting. `--verbose` prints no further line for it. The confirmation of a large write run (CLI-018) counts the selected files before any is formatted and stays unchanged, as it does for files with syntax errors.

### The first shape covers the whole declaration (question 5, B)

All four cases the maintainer was asked about refuse the input: a comment inside the type, a comment between a modifier and the type, a comment on its own line in front of the value, and one in front of the `;`. FMT-UNSUP-004 therefore refuses a comment anywhere in the declaration's span — from its first token, attributes and modifiers included, through its terminator — except in the leading trivia of the first token, in the trailing trivia of the `;`, and inside an initializer value. The brackets and size of a fixed-size buffer are part of the span. For a `for` initializer the terminator is the `for` statement's first `;`. The comment kinds are refused in every documentation mode, so the mode no longer decides which declarations the split refuses.

### The value's inside ends at its last token (question 9, A)

A comment between a value's last token and the `,` or `;` behind it is refused, like one in front of the value and one in front of the `;`: the inside of a value is the trivia from its first token's trailing trivia through its last token's leading trivia (FMT-UNSUP-004). Whether the parser attaches such a comment to the value or to the `;` — `/** c */` in `int a = 1 /** c */;` goes to the `;` when documentation comments are parsed — therefore no longer decides the outcome, and the CLI and a code fix agree for the same text. As a consequence, the RH2001 fix is withheld for a property whose initializer has a comment behind its value on the value's line (RH2001-007).

### The `)` of a `using` or `fixed` statement ends its declaration (question 10, A)

The span of the declaration of a `using` or `fixed` statement runs through the `)` behind its last declarator, as a local's runs through its `;`: a comment in front of the `)`, on its own line or behind the value, is refused, and a comment behind the `)` is excepted like one behind the `;` (FMT-UNSUP-004). The statement's keyword and `(` stay outside the span.

### Attribute arguments are part of the span (question 11, A)

A comment inside an attribute's arguments, such as `[Obsolete("old" /* why */)] private int _e;`, is refused: the attribute lists of a field or an event field are wholly in the span, and an attribute argument is no initializer value (FMT-UNSUP-004).

### Nested declarations are checked on their own

A variable declaration inside an initializer value, such as a local in a lambda body, is a declaration of its own; the exception for the inside of a value applies to the declaration around it only (FMT-UNSUP-004).

### The Playground shows a message (question 6, A)

PLAY-008, in the form of PLAY-006's message, with the line, the column, and the reason.

### RH2001 is withheld for a field the formatter refuses (question 7, A)

The fix asks about the field it builds, not about the property (SHARED-FIX-010). As a consequence, the comment rules of RH2001-009 — which comments a reordered modifier crosses, and where `readonly` goes relative to a comment — no longer apply: every comment among the modifiers or in front of the type withholds the fix.

### Code fixes ask one query at registration (question 8, A)

`ReihitsuFormatter.FindUnsupportedSyntax` runs the precheck's own predicate on a node (FMT-UNSUP-007). Every formatting fix calls it when it registers, on exactly the node or context it formats, or on the node it builds (SHARED-FIX-010). About 45 call sites are accepted.

### `SplitsFieldDeclaration` does not answer for unsupported comments

FMT-PIPE-031 states no condition for a comment FMT-UNSUP-004 refuses: inside the phase, a refused declaration never reaches the split, and for the RH7101 fix SHARED-FIX-010 decides through the type the fix formats. A second statement would put a guard for a refused shape back into the split.

### A phase's refusal is turned into the outcome where the precheck's is produced

The refusal is caught once, in the pipeline run every entry point uses, also when an observer or the trace watches the run, and never wrapped in another exception there (FMT-UNSUP-006).

### The context of a context entry point is checked as a whole

FMT-UNSUP-002 checks the context of `FormatNodeInDocumentWithContextAsync`, not only its target, because the phases receive the context. A fix that formats in a context is therefore withheld for a refused shape anywhere in it (visible output change 3).

### `OrganizeUsingDirectivesInDocumentAsync` is not checked

It runs only the using phase on one scope (FMT-PIPE-030), not the formatting phases, so nothing that FMT-UNSUP-004 names reaches a phase that needs a guard.

### The written-back boundary trivia is not covered

The node-level entry points write back their target's leading and trailing trivia. The first shape excludes both the leading trivia of the declaration's first token and the trivia behind the `;` or `)` that ends it, so the precheck covers no written-back trivia, and the write-back paths stay as they are.

### Spec examples that hold a refused shape for another reason are rewritten

Formatter examples whose subject is not a variable declaration but whose input held one with a refused comment now use a construct that holds no variable declaration, with the same output: FMT-INDENT-015 (an expression-bodied property instead of the field `_map`), FMT-BLANK-018 (`int[] G { get; } =`), FMT-SPACE-012 (a fixed-size buffer without the comment behind `fixed`), FMT-SPACE-014 (`int P => 1 /** x */;`), and FMT-DOC-003 (`int A => 1 /// stray`). Each output was produced with the CLI built from `main`. Answers 9 to 11 make no further formatter or formatting-fix example refused: the analyzer examples that hold the shapes they add belong to rules whose fix does not format (RH3204, RH5041, RH5517 to RH5520) or already state that no fix is offered (RH7101-007), and RH2001-007 now names the comment behind the value.

## Special cases

The maintainer's acceptance criterion: every special case the first shape makes unnecessary is deleted in this change and listed.

### Deleted

- `FieldDeclarationSplitTransform.CarriesCommentInRepeatedPart`, and its condition in `Splits`. Every comment it found — inside or behind an attribute list, between modifiers, between a modifier and the type, inside or behind the type, and in front of the first declarator's name — is in FMT-UNSUP-004's span.
- `FieldDeclarationSplitTransform.GetComments`, `BuildTrailingTrivia`, and `BuildLeadingTrivia`, the `nextFieldHasLeadingComment` argument, and the comment carry-over in `CreateSplitField`: a `,` and a later declarator's name hold no comment any more.
- The field `_context` of `FieldDeclarationSplitTransform` and its constructor parameter, which only the two builders used.
- `TokenGapUtilities.HasLineBreak(SyntaxTriviaList)`, whose only caller is `BuildTrailingTrivia`.
- The comment clauses of FMT-STRUCT-022, FMT-STRUCT-026, and FMT-STRUCT-030 that described them, and the comment conditions of FMT-PIPE-031.

### Checked and kept

- `FieldDeclarationSplitTransform.IsDocumented`: a documentation comment in the declaration's leading trivia is not refused (FMT-STRUCT-030).
- `FieldDeclarationSplitTransform.CarriesDirective`: the precheck refuses comments only (FMT-STRUCT-023).
- The carry-over of the terminator's leading trivia and of each declarator's trailing trivia, and the dropped leading trivia of a later declarator, in `CreateSplitField`: they now carry whitespace and line breaks only (a comment behind a value's last token is refused under question 9), which FMT-STRUCT-026 keeps as written; they are not comment special cases.
- `LineBreakAssignmentRewriter` (`WouldJoinAcrossUnjoinableTrivia` in the equals-value and operator placement): shared with property initializers, enum members, parameter defaults, and assignment expressions, and an equals-value clause target with a comment still reaches the phases (FMT-UNSUP-003).
- The spacing rules around `=` and `,` (`OperatorSpacingRule`, `CommaSpacingRule`, `DeclaredNameSpacingRule`, `NoSpaceSpacingRule`): they have no comment special case; comment gaps are handled by the shared next-token leading-comment guard and `NormalizeGapBehindLeadingComment`.
- `DeclarationSemicolonLineBreakRewriter` and `LineBreakTriviaUtilities.CollapseTokenToSameLine`: shared with delegates, properties with an initializer, and event fields.
- The blank line in front of a comment on its own line (FMT-BLANK-011) and the indentation of a comment-led first token (FMT-INDENT-023, FMT-INDENT-030): generic, shared with assignments, property initializers, and arguments.
- The attribute-list split and merge guards of FMT-STRUCT-028 and FMT-STRUCT-029: they serve methods, properties, types, and parameters.
- `ReihitsuFormatter.FormatSplitFieldInContext` and the write-back paths: no comment-specific code.
- Code fixes: the RH2001 fix's `KeepsEveryCommentAndDirective` (comments the field would drop: at the name, in the accessor list, in front of the `;`) and its call of `ModifierOrderingUtilities.TryWriteModifiersInOrder` (still needed for directives, and the utility serves RH7105 and RH7106 on other members); the RH5111 fix's `ContainsJoinBlockingTrivia` (assignments, properties, directives); the RH5113 fix's comment check (delegates, properties); the RH7004 fix's carry-over of the `;`'s trivia (whitespace, and the comment behind the `;`).

## Alternatives considered

- **Leave constructs as written, silently** (the policy before this change) — rejected: each shape is still analyzed, specified, and guarded case by case, and users see no signal.
- **Refuse per construct and format the rest of the file** — rejected: the phases would still meet the shape and need guards around it, so no complexity is removed.
- **Base the list on analyzer rules** — rejected: the formatter would depend on the analyzers, and a shape without a rule could not be declared.
- **`--suppress-unsupported` formats anyway** — rejected: the phases would have to keep supporting the shape.
- **Check only the target of `FormatNodeInDocumentWithContextAsync`** — rejected: the phases receive the context, so a refused shape elsewhere in it would still reach them, against the one-check-site contract.
- **Keep the existing return types and add parallel `Try…` methods** (question 1, B) — rejected by the maintainer: a caller could ignore the outcome by calling the old method.
- **Only the trivia of the type, the names, `=`, and `,`** (question 5, A) — rejected by the maintainer; it would also have kept `CarriesCommentInRepeatedPart`.
- **Except a comment between a value and the `,` or `;` behind it** (question 9, B) — rejected by the maintainer: whether `int a = 1 /** c */;` was refused would have depended on the documentation mode.
- **End the span of a `using` or `fixed` statement at its last declarator** (question 10, B) — rejected by the maintainer: the `)` ends that declaration as the `;` ends a local's, and one rule covers both.
- **Treat attribute arguments like initializer values** (question 11, B) — rejected by the maintainer: it would carve the attributes out of the one check from the first token through the terminator.
- **Format at registration to learn the outcome** — rejected: it would format a whole type for each RH7101 diagnostic shown.
- **A condition for refused comments in `SplitsFieldDeclaration`** — rejected: it restates SHARED-FIX-010 and puts a guard back into the split.
