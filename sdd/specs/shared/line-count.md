# Shared — Line count of a declaration

| | |
|---|---|
| ID prefix | SHARED-LINES |
| Status | Active |
| Code owners | — |
| Uses | RH0001-003, RH0001-007, SHARED-DIAG-008 |

This spec covers the rules that count the lines of code of a declaration's body and report the declaration when the count exceeds a limit configured in `reihitsu.json`: RH2008 for methods and the other members with a body, RH2111 for types. It defines what a line of code is and how the count is compared with the limit. Each rule that uses this spec states which declarations it judges, which span of each declaration it counts, which setting holds its limit, and where its diagnostic is located.

## Non-goals

- Which declarations a rule judges and which span of them it counts — each rule's spec.
- Loading and validating the configuration file and the settings that hold the limits — RH0001 (RH0001-007).
- A code fix: making a declaration shorter is a design decision — extracting a method, splitting a type — that no fix can make.
- Any other measure of size or complexity, such as the number of members, statements, or branches.

## Requirements

### SHARED-LINES-001 — A line of code holds a character of a token of the counted span

A rule MUST count, as the lines of code of a span, the lines of the text on which at least one character of a token of the span lies. A token of the span is a token of the syntax tree whose text lies inside the span; the tokens inside structured trivia — a documentation comment or a preprocessor directive — are trivia, not tokens of the span. A line MUST be counted once, however many tokens lie on it, and a token whose text runs over several lines, such as a verbatim or raw string literal, MUST make each line it covers a line of code. Trivia MUST NOT make a line a line of code: a line that holds only whitespace, a comment, a documentation comment, a preprocessor directive, or code that a conditional directive excludes in the analyzed build is not counted. A line that holds a token and a comment is counted.

Why: The count stands for what a reader has to read as code. Blank lines, comments, and directives structure or explain the code without adding to it, and counting them would make a rule punish documentation and readable spacing. A token is code wherever a line break puts it, and a line is read once however many tokens it holds. Code that a conditional directive excludes is not compiled in the analyzed build and counts as absent there (SHARED-DIAG-008); its directive is a directive line in every build.

Example — RH2008 with `{ "design": { "maximumMethodLines": 3 } }`. The body of `M` holds four lines of code: the two lines of the calls `A();`, and the two lines over which the call of `B` and its verbatim string run; the comment line, the blank line, the directive lines, and the line that the undefined symbol `FEATURE` excludes are not counted:

```csharp
class C
{
    void {|RH2008:M|}()
    {
        // Prepare.
        A();

#if FEATURE
        A();
#endif
        B(@"first
second"); // Trailing comment.
        A();
    }

    void A()
    {
    }

    void B(string text)
    {
    }
}
```

Example — the same body without the line `A();` in front of the blank line holds three lines of code and is not reported.

### SHARED-LINES-002 — A declaration is reported when its count is greater than its limit

A rule MUST report a declaration whose count of lines of code is greater than the limit configured for the rule, and MUST NOT report a declaration whose count equals the limit or is lower. When the limit is not configured, or its setting has a problem that RH0001 reports (RH0001-007), or the configuration file cannot be parsed (RH0001-003), the rule MUST NOT report anything. A problem RH0001 reports elsewhere in the configuration file MUST NOT keep a valid limit from taking effect.

Why: The limit is the largest size the project accepts, so a declaration of exactly that size is accepted. How large a declaration may grow is the project's own decision, and no single limit suits every project, so the rule checks nothing until the project sets one. A setting with a problem has no value the rule could trust; a problem in another section does not change the value of a valid setting.

Example — with `{ "design": { "maximumMethodLines": 2 } }`, `M` holds two lines of code and is not reported; `N` holds three and is reported:

```csharp
class C
{
    void M()
    {
        M();
        M();
    }

    void {|RH2008:N|}()
    {
        N();
        N();
        N();
    }
}
```

Without a `reihitsu.json`, with `{ "design": { } }`, with `{ "design": { "maximumMethodLines": 0 } }`, and with `{ "design": { "maximumMethodLines": "2" } }`, neither method is reported. With `{ "design": { "maximumMethodLines": 2 }, "formatting": 1 }`, `N` is reported, although RH0001 reports the unknown section `formatting`.
