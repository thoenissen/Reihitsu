# Open questions — using directives (SHARED-USING, FMT-USING)

## Q1 — Formatter output for a comment inside a using group is reported by RH7207

Input:

```csharp
using System;
// I/O helpers
using System.IO;

class C
{
    int x;
}
```

Output:

```csharp
using System;

// I/O helpers
using System.IO;

class C
{
    int x;
}
```

Phases: `BlankLine, Cleanup`.

Why it looks wrong: The input is the exact case of the RH7207 test `NoDiagnosticWhenCommentSeparatesSameGroupUsings` (no diagnostic). The using-directive phase leaves it alone, but the blank-line phase inserts a blank line before the comment. RH7207's `HasBlankLineBefore` counts the line breaks before the directive's first comment, so on the formatter's output it sees a blank line inside one group and — by code reading, not run — reports RH7207. Its code fix runs only the using-directive phase, which removes the blank line again, so analyzer and full formatter disagree in a loop. The same happens when a reorder places a comment-led directive in the middle of its group (input `using System;` / `using Microsoft.CodeAnalysis;` / `// About Linq` / `using System.Linq;` gives `using System;`, blank line, `// About Linq`, `using System.Linq;`, phases `UsingDirectiveOrdering, BlankLine, Cleanup`). Which surface should change: should the blank-line phase not add a blank line before a comment that leads a same-group using directive, or should RH7207 accept a blank line before a comment?

Touches: SHARED-USING-008 (written as the analyzer and the using-directive phase currently agree), FMT-USING-002, FMT-USING-008, and the FMT-BLANK rule for blank lines before comments.

## Q2 — A block comment line starting with `#` blocks reordering

Input:

```csharp
/*
# not a directive
*/
using System.Linq;
using System;

class C
{
    int x;
}
```

Output: unchanged apart from the final newline. Phases: `Cleanup`.

Why it looks wrong: `UsingDirectiveOrderingSafety.ContainsPreprocessorDirective` matches the regex `(^|\r?\n)\s*#` against the full text of the block, so a line inside a multi-line comment that starts with `#` (a Markdown heading, an issue reference) counts as a preprocessor directive. The structural check `HasUnsafeTrivia` already catches every real directive in the block's trivia. The result is safe (nothing is moved), but the RH7201–RH7207 analyzers are also silenced on such a block. Is the textual check intended as a deliberate extra margin, or should it be dropped or limited to text that is not inside a comment?

Touches: SHARED-USING-009 (written with only real preprocessor directives as the trigger).

## Q3 — `global::`-qualified names sort by their qualifier inside a group

Input:

```csharp
using System.Linq;
using global::System.Text;
using System;

class C
{
    int x;
}
```

Output:

```csharp
using global::System.Text;
using System;
using System.Linq;

class C
{
    int x;
}
```

Phases: `UsingDirectiveOrdering, Cleanup`.

Why it looks wrong: For grouping, `GetRootNamespace` strips a leading `global::`, so `global::System.Text` joins the `System` group (SHARED-USING-004/-005). For the order within the group, `GetSortKey` uses the name with the qualifier, so every `global::` directive sorts by the letter `g` instead of by its namespace — here before `System`, but after any root starting with `a`–`f`. RH7203 uses the same key, so analyzer and formatter agree; the question is whether the key should strip `global::` like the group key does.

Touches: SHARED-USING-007 (written as "the imported namespace or type name" without specifying the qualifier).
