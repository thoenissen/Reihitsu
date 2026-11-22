# Playground: syntax highlighting, no line wrapping, and visible error locations

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | PLAY-002, PLAY-006, PLAY-010, PLAY-011, PLAY-012, PLAY-013, PLAY-014 |

## What

The Playground's two panes show C# with syntax highlighting, keep every line on one line, and point at the code an error is about:

- Both panes colour their text by the C# syntax tree the page already builds with Roslyn: keywords, identifiers, string and character literals, numbers, comments, preprocessor directives, and disabled text each get their own colour (PLAY-010). The output becomes a read-only highlighted code view instead of a text area; the input stays a text area, with the browser's own editing and undo, whose highlighting lies under its text (PLAY-002, PLAY-011).
- The input's colours follow every keystroke; the output still changes only on "Format" (PLAY-011, PLAY-004 unchanged).
- Neither pane wraps a line; a pane whose lines are wider than the pane scrolls horizontally (PLAY-012).
- The message below the button becomes an alert box (PLAY-013). For a syntax error it keeps its sentence and lists every syntax error below it, one line each, as `line <line>, column <column> — <ID>: <message>` (PLAY-006).
- The input marks the span of each syntax error, and the span of the shape the formatter refuses as unsupported, with a red wavy underline. Code hidden in another build configuration gets no mark, because the formatter reports no location for it (PLAY-014).

## Why

The formatter's main output is where it breaks lines. A text area that wraps long lines makes the formatter's line breaks indistinguishable from soft wraps, and plain text makes the formatted code hard to read. An error message without a location leaves the user to find the error in the input by hand. thoenissen/Reihitsu#1941 records the problem; the maintainer chose the approach before the issue was filed.

## Visible output changes

All changes are in the Playground page; the formatter, the analyzers, the code fixes, the rule pages, and the CLI do not change. The Playground shows no C# output that differs from today: the output pane's text is the same text, now coloured. The changes are therefore described as what the page shows.

### 1. Both panes are highlighted

Input, as typed or on load:

```csharp
public class Sample{public int Add(int a,int b){return a+b;}}
```

Before: the input and the formatted output are plain text in one colour.

After: in both panes `public`, `class`, `int`, and `return` show in the keyword colour; `Sample`, `Add`, `a`, and `b` in the identifier colour; braces, parentheses, `,`, `;`, and `+` in the page's normal text colour. A string or character literal, a number, a comment, a preprocessor directive, and the text of an inactive `#if` branch each show in a colour of their own:

```csharp
#if DEBUG
inactive text
#endif
// comment
class C { string s = "text"; char c = 'x'; int n = 42; }
```

The input is coloured while the user types, also when the text does not parse yet; the output is coloured when "Format" replaces it. Contextual keywords that the parser reads as keywords (`partial`, `async`, `await`, `get`, `set`, `where` in a constraint) show as keywords; a name the parser reads as an identifier (`var`, `nameof`, `dynamic`, `value`) shows as an identifier — see the open question in the spec author's handover.

### 2. Lines do not wrap

Input:

```csharp
public class Sample { public int Add(int first, int second, int third, int fourth, int fifth, int sixth, int seventh) { return first + second + third + fourth + fifth + sixth + seventh; } }
```

Before: in both panes, each line longer than the pane continues on the next visual line.

After: each line stays on one visual line; the pane shows a horizontal scroll bar.

### 3. Syntax errors are listed in an alert box

Input:

```csharp
public class Sample{public int Add(int a,int b){return a+b}
```

Before, below the button in red text:

```text
The input contains syntax errors and was left unchanged.
```

After, in an alert box below the button:

```text
The input contains syntax errors and was left unchanged.
line 1, column 59 — CS1002: ; expected
line 1, column 60 — CS1513: } expected
```

### 4. Error spans are marked in the input

For the input of 3, the input pane underlines the `}` at column 59 (the span of CS1002) and the place behind the end of the text (the empty span of CS1513), one character wide, with a red wavy line.

For the input of PLAY-008:

```csharp
class C
{
    void M()
    {
        int /* c */ a = 1;
    }
}
```

Before: the message of PLAY-008 below the button, no mark.

After: the same message in an alert box, and `/* c */` underlined in the input.

For the input of PLAY-009: the same message in an alert box, and no mark.

### 5. The unsupported-syntax and hidden-code messages move into the alert box

Their text does not change (PLAY-008, PLAY-009); they are shown in the alert box of PLAY-013 instead of as red text.

## Decisions

### The maintainer's plan is binding

The issue is labeled `agent-filed`, but its plan was chosen by the maintainer before the issue was filed: colours from Roslyn's syntax tree with no editor library and no download from another origin; live colouring of the input while the output still changes only on "Format"; the syntax error sentence kept with every error listed below it with line, column, ID, and message, and a red wavy underline under each error span; no wrapping in either pane; errors in a clearly styled alert box; the unsupported shape underlined at its reported location; hidden code with the alert box only.

### Highlighting is syntactic

The Playground parses the text but builds no compilation, so it has no semantic model: a category is decided by the kind of the token or trivia in the syntax tree. Types are not told apart from other names, and a contextual keyword counts as a keyword exactly when the parser gives its token a keyword kind (PLAY-010).

### The categories

Keyword, identifier, string (string and character literals and the text parts of interpolated and raw strings), number, comment (documentation comments included), preprocessor directive (the whole directive line except a comment behind it), and disabled text. Every other token and trivia — punctuation, operators, whitespace — keeps the normal text colour. The colours themselves are styling, a non-goal of the spec; PLAY-010 requires only that each category has its own colour in both colour schemes the page supports.

### Error positions

An error's line and column are one-based and counted like PLAY-008's: the column counts characters from the start of the line, a tab as one. The errors are listed by the position of their span's first character, and errors at the same position in the order the parser reports them. Only errors are listed and marked; a parser warning is not (PLAY-006, as today's message). An empty span — the parser reports many errors such as `} expected` with one — is marked one character wide at its position (PLAY-014).

### The marks are in the input only

The output shows the unchanged input when an error is shown (PLAY-006, PLAY-008, PLAY-009); marking it too would repeat the input's marks in a pane that is a result, not a place to edit.

## Alternatives considered

- **An editor library such as Monaco or CodeMirror** — rejected by the maintainer: a new dependency and, from a CDN, a download from another origin next to PLAY-001's promise.
- **Roslyn's `Classifier`** — rejected: it needs a workspace and a semantic model, which the Playground does not build.
- **Wrapping with a marker for soft wraps** — rejected by the maintainer in favour of no wrapping.
- **Showing only the first syntax error** — rejected by the maintainer: every error is listed.
