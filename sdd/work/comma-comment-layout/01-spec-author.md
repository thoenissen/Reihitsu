Role: spec author
Commit: 8891adefaef8586e8467bbf4eceece71b6def35a
Verdict: #1737: open question (change doc written); #1738: open question (change doc written)

No spec diff is written yet. Both issues put a visible output in question that no unchanged requirement decides, and `sdd/README.md` (*Open questions*) keeps an undecided case out of every spec, so the requirements are written after the maintainer's answer. The prompt carried no intended fix or suspected cause beyond the issues' own proposals.

How the facts below were established: formatter outputs by `scripts/format.sh` on scratch files, re-run once to confirm they are stable; RH5107's fix output by `scripts/apply-fix.sh RH5107` (LF and CRLF, both `fixed`, 4 iterations, converged).

## thoenissen/Reihitsu#1737

### Findings

- Reproduces. Input `void P(int a⏎           , /* c */ int b)`: the formatter gives `void P(int a, /* c */⏎           int b)`, RH5107's fix gives `void P(int a,⏎           /* c */ int b)`. Same split for `, // c⏎int b` and for `, /* c */⏎int b` (the fix leaves the comment on its own line; the formatter puts it behind the comma).
- Neither component violates its own spec: RH5107-003 says the indentation, the comma, and the whitespace behind it are replaced with spaces up to the first parameter's column when the line holds more than whitespace — the comment stays put. FMT-BREAK-012 states for argument and attribute argument lists that a comment behind a moved comma goes with it "as in a parameter list"; FMT-BREAK-011 itself does not state the comment placement.
- Formatting the fix's result changes nothing (checked), so SHARED-FIX-007 holds today. The issue's premise ("P10, fix output laid out as the formatter lays it out") is therefore not a violated requirement: the disagreement is two stable outputs for one input. Changing either is a behavior change; which one is the output question below.
- Reachability: the fix is offered on a reported RH5107 diagnostic (the fix's guard only checks the gap between the previous parameter and the comma, not behind the comma); the formatter's output through the CLI and whole-document formatting.

### Changed requirements

None yet. Option A changes RH5107-003 (the fix keeps a comment directly behind the comma behind it, on the previous parameter's line; what follows the comment on that line starts the next line at the first parameter's column; a `//` comment keeps ending the line). Option B changes FMT-BREAK-011 and the "as in a parameter list" sentence of FMT-BREAK-012. Change doc: `sdd/changes/2026-11/rh5107-comment-behind-leading-comma.md`.

### Examples for the tests

After the answer: the three methods `P`, `Q`, `R` of the change doc, in the RH5107 code-fix tests (and in `RH5107CommaMustBeOnSameLineAsPreviousParameterFormatterTests` under B). Also `void S(int a, int b⏎, /* c */ int c, int d)` — fix today: `void S(int a, int b,⏎           /* c */ int c, int d)`; under A: `void S(int a, int b, /* c */⏎           int c, int d)` (the fix only moves its comma; RH5101/RH5109 own the rest).

### Output questions

**1 · RH5107 · Comma must be on same line as previous parameter — comment behind a leading comma**

Input:
```csharp
class C
{
    void P(int a
           , /* c */ int b)
    {
    }

    void Q(int a
           , // c
           int b)
    {
    }
}
```

Before (`main`): the formatter and the fix disagree.
```csharp
// formatter
    void P(int a, /* c */
           int b)

    void Q(int a, // c
           int b)

// RH5107 fix
    void P(int a,
           /* c */ int b)

    void Q(int a,
           // c
           int b)
```

After, A (recommended) — the fix follows the formatter:
```csharp
    void P(int a, /* c */
           int b)

    void Q(int a, // c
           int b)
```

After, B — the formatter follows the fix (parameter lists; argument lists keep the comment behind the comma):
```csharp
    void P(int a,
           /* c */ int b)

    void Q(int a,
           // c
           int b)
```

Why A: a comment behind a comma stays behind it in every other list the formatter lays out (argument lists, attribute arguments, constraint clauses), and B would make parameter lists the one exception.

### Internal choices

- One change doc per issue rather than one shared doc: the two issues have different causes (fix vs. formatter on parameter lists; an unspecified initializer layout). Alternative: one doc.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1738

### Findings

- Reproduces, and wider than the issue states. On `main` a comma that starts a line is left alone on a line at the enclosing statement's column (outside the braces) in every construct traced: object initializer (leading and trailing comma), anonymous object, `with` initializer, collection initializer, dictionary indexer initializer, nested collection initializer (`Items = { … }`), complex element initializer, collection expression, list pattern, property pattern clause (FMT-BREAK-021 and FMT-BREAK-022 constructs). In an array initializer (layout kept, FMT-BREAK-021) the comma keeps its line with the element behind it (`, 2`), but that line also goes to the statement's column. A comment behind the comma stays on the comma's line (`, /* c */`). All outputs stable on a second run.
- No requirement decides it: FMT-BREAK-021/022 say every element starts its own line but nothing about a comma; FMT-INDENT-022, -023, -025 place braces and elements that start a line, not a separator (FMT-INDENT-012 covers separators only for parenthesized and bracketed lists).
- Reachability: whole-document formatting and the CLI (traced through `scripts/format.sh`). No analyzer rule reports a leading comma in an initializer (P11: the formatter may do more; not a rule gap).
- Not a rare class under `AGENTS.md` (*Code-fix coverage*): comma-first code violates no other Reihitsu rule. Declaring a shape unsupported is not an option worth naming: it would delete no special case.

### Changed requirements

None yet. On the recommended answer: a new line-break requirement (next free ID FMT-BREAK-038) for the commas of FMT-BREAK-021 and FMT-BREAK-022 constructs, mirroring FMT-BREAK-012's comma paragraphs (comma ends the element's line, a comment behind it goes with it, a comma held by a comment/directive/disabled text stays at the start of the next element's line); array initializers keep their line breaks (FMT-BREAK-021 unchanged). A new indentation requirement (next free ID FMT-INDENT-033): a line that starts with a comma inside an initializer, a collection expression, or a pattern of FMT-BREAK-022 takes the column its elements take (FMT-INDENT-022, -023, -025). Change doc: `sdd/changes/2026-11/initializer-leading-comma.md`.

### Examples for the tests

After the answer: the five statements `a`–`e` of the change doc's input, plus one each for an anonymous object, a `with` initializer, and a property pattern (the shapes traced above).

### Output questions

**2 · Formatter — comma that starts a line inside an initializer or collection expression**

Input:
```csharp
var a = new D(1, 2) { X = 3
    , };
var b = new D { X = 3
    , /* c */ Y = 4 };
var c = new D
{
    X = 3 // k
    , Y = 4
};
int[] d = [1
    , 2];
```

Before (`main`):
```csharp
        var a = new D(1, 2)
                {
                    X = 3
        ,
                };
        var b = new D
                {
                    X = 3
        , /* c */
                    Y = 4
                };
        var c = new D
                {
                    X = 3 // k
        ,
                    Y = 4
                };
        int[] d = [
                      1
        ,
                      2
                  ];
```

After, A (recommended) — the comma ends the previous element's line, as in an argument list; one a comment holds off that line stays in front of the next element, at the elements' column:
```csharp
        var a = new D(1, 2)
                {
                    X = 3,
                };
        var b = new D
                {
                    X = 3, /* c */
                    Y = 4
                };
        var c = new D
                {
                    X = 3 // k
                    , Y = 4
                };
        int[] d = [
                      1,
                      2
                  ];
```

After, B — the comma keeps its own line, at the elements' column:
```csharp
        var a = new D(1, 2)
                {
                    X = 3
                    ,
                };
        var b = new D
                {
                    X = 3
                    , /* c */
                    Y = 4
                };
        var c = new D
                {
                    X = 3 // k
                    ,
                    Y = 4
                };
        int[] d = [
                      1
                      ,
                      2
                  ];
```

Why A: the comma then sits where every argument and parameter list puts it, and the element behind it starts its line like every other element.

**3 · Formatter — comma that starts a line inside an array initializer (line breaks kept as written)**

Input:
```csharp
var e = new int[]
{
    1
    , 2
};
```

Before (`main`):
```csharp
        var e = new int[]
                {
                    1
        , 2
                };
```

After, A (recommended) — line breaks stay as written, the comma's line takes the elements' column:
```csharp
        var e = new int[]
                {
                    1
                    , 2
                };
```

After, B — the comma moves behind the previous element, as in question 2 A, although array initializers otherwise keep the author's line breaks:
```csharp
        var e = new int[]
                {
                    1,
                    2
                };
```

Why A: array initializers keep their author's line breaks by design, so only the column, which today lies outside the braces, changes.

### Internal choices

- Scope of the decision is the whole construct family of FMT-BREAK-021 and FMT-BREAK-022 (every one traced fails the same way), not only the issue's object initializer; question 2's answer is written for all of them. Alternative: object initializers only, the rest as a follow-up.
- New requirements (FMT-BREAK-038, FMT-INDENT-033) rather than extending FMT-BREAK-021/022 and FMT-INDENT-022/023/025, so one decision has one heading. Alternative: a paragraph in each existing requirement.

### Follow-up candidates

_None._
