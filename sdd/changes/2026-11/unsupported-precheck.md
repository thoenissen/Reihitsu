# The formatter's Unsupported outcome

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP (new spec `formatter/unsupported-syntax.md`: FMT-UNSUP-001 to FMT-UNSUP-006), FMT-PIPE-001, FMT-PIPE-027, FMT-PIPE-029, FMT-PIPE-031, FMT-STRUCT-022, FMT-STRUCT-026, FMT-STRUCT-030, RH7101-004, RH7101-006, RH7101-007, SHARED-FIX-010, CLI-001, CLI-019, CLI-020 |

## What

The formatter gets an explicit *Unsupported* outcome. A new first phase, the precheck, checks the input the formatting phases receive against a list of shapes the formatter declares unsupported. When one matches, no other phase runs: the entry point returns its input unchanged and reports it as unsupported, with the reason and the location (FMT-UNSUP-001, FMT-UNSUP-002). A phase may refuse an input during the run for a shape the precheck does not find, with the same outcome; the refusal never escapes an entry point as an exception (FMT-UNSUP-006). The decision depends only on the tokens and trivia inside the input (FMT-UNSUP-003), and formatted output is never unsupported (FMT-UNSUP-005).

The first unsupported shape is a comment between a variable declaration's own tokens: behind the type, at a declarator's name, at the `=` of an initializer, or at a `,` between declarators (FMT-UNSUP-004). A comment above or in front of the declaration, behind its `;`, or inside an initializer value is not refused.

The special cases this shape made necessary are removed from the specs: the field split no longer carries comments at a comma or in front of a later declarator (FMT-STRUCT-022, FMT-STRUCT-026), and the boundary between the part every split field repeats and the first declarator, with its documentation-mode rules, is gone (FMT-STRUCT-030). `SplitsFieldDeclaration` answers false for a declaration the precheck refuses (FMT-PIPE-031).

A code fix whose formatting the formatter refuses is withheld (SHARED-FIX-010). The CLI never changes a file the formatter refuses and reports it as an error (CLI-019); `--suppress-unsupported` reports it as a warning that does not change the exit code (CLI-020).

## Why

Many formatter issues concern rare comment and directive shapes. Each one cost analysis, spec text, and special cases in the phases, and many ended in leaving the construct as written anyway — silently, and decided again for every case and every phase (`AGENTS.md`, *Special cases do not grow the source*). An explicit outcome lets a shape be refused once, in one place, and lets every phase drop the guards it needed for it; the user is told why a file was left as written. The goal is less formatter code, not more: each shape on the list has to let special cases go.

## Visible output changes

### 1. Formatter output — the five shapes of FMT-UNSUP-004 refuse the whole input

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

A comment above the declaration, behind its `;`, or inside an initializer value or a lambda body keeps the input formatted as today (the second example of FMT-UNSUP-004).

### 2. Formatter output — fields with a comment at a comma or at a declarator are no longer split

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

The same holds for the comments the old FMT-STRUCT-030 assigned to the first declarator, such as `private int /** c */ _first, _second;` in the documentation mode `Parse`, which used to be split and is now refused, and for `///` and `/** */` comments in front of a later declarator, which used to be moved in front of that declarator's field (old RH7101-006 example).

### 3. Code fixes — withheld where the formatter refuses the node they format

Every code fix that formats through the formatter is withheld when the node it formats holds an unsupported shape (SHARED-FIX-010). For a fix that formats its target in a context — RH7101 in the type, RH3208, RH5111, and the fluent-chain fixes in their context — the context counts (FMT-UNSUP-002): a comment between the tokens of any variable declaration in that context withholds the fix, also one far away from the reported code.

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

The examples of the old RH7101-004 (comment behind a comma, comment in front of a later declarator), RH7101-006 (documentation comments in front of later declarators), and RH7101-007 (comment at `=` in the first declarator, comment on its own line in front of the first declarator's name, `/** */` behind the type in the documentation mode `Parse`) change the same way. The diagnostics themselves do not change.

### 4. CLI

- A new option, `--suppress-unsupported` (CLI-001, CLI-020).
- A file the formatter refuses is never written, in any mode, also with `--utf8-bom`, and is reported as an error naming the file, the location, and the reason (CLI-019). Before, such a file was formatted.
- With `--suppress-unsupported`, the report is a warning and the exit code is the one the run has without the file (CLI-020).

```shell
reihitsu-format u.cs        # u.cs holds `int /* c */ a = 1;`
```

Before: `Formatted: /work/u.cs` (when the rest of the file needed formatting) and exit code 0. After: the file unchanged and reported as an error; the message and the exit code are open (below).

### 5. Playground

The Playground formats with `FormatSyntaxTree` (PLAY-005), so an unsupported input is shown unchanged as the output. Whether a message says why is open (below).

### 6. Rule pages

None. A withheld fix is covered by `documentation/rules/general-notes.md` ("A diagnostic without a code fix", and comments blocking a fix), and no rule's report changes.

### Open — not decided by this change doc yet

These points are visible and await the maintainer's answer; the specs do not decide them until it is recorded below:

- the result type the public formatter entry points return, and how they carry the outcome, the reason, and the location;
- the CLI's message for an unsupported file: its wording, its stream, and whether the summary counts the file;
- the CLI's exit code for an unsupported file without `--suppress-unsupported`: `2` (`ExitCodes.Error`) or a new code;
- how `--check` and `--dry-run` report an unsupported file;
- the boundary of FMT-UNSUP-004 for a comment inside the type, a comment between a modifier and the type, and a comment on its own line in front of the value or the `;`;
- whether the Playground shows a message for an unsupported input.

## Decisions

### The design decisions of the issue are binding

The maintainer decided, in the conversation that produced thoenissen/Reihitsu#1670 and again for this run: the precheck phase is the primary filter; a phase may throw `SyntaxNotSupportedException` for edge cases the precheck misses; the whole file is refused; the list of unsupported shapes belongs to the formatter and depends on no analyzer; `--suppress-unsupported` turns the error into a warning and the file stays unchanged; and the first shape is a comment in the trivia of a variable declaration's own tokens — the type, each declarator's name, `=`, and `,` between declarators — and not in the leading trivia of the declaration's first token, behind its `;`, or inside an initializer value.

### The issue's contract is binding

One check site (exactly what the phases receive, the context for `FormatNodeInDocumentWithContextAsync`), a local decision, closure, the exception boundary in every public entry point, cheap checks only with a requirement and a test for each throw site, code fixes that withhold themselves, and the removal of every special case the first shape makes unnecessary, in the same change.

### The context of a context entry point is checked as a whole

FMT-UNSUP-002 checks the context of `FormatNodeInDocumentWithContextAsync`, not only its target, because the phases receive the context. A fix that formats in a context is therefore withheld for a refused shape anywhere in it (visible output change 3).

### `OrganizeUsingDirectivesInDocumentAsync` is not checked

It runs only the using phase on one scope (FMT-PIPE-030), not the formatting phases, so nothing that FMT-UNSUP-004 names reaches a phase that needs a guard.

### The written-back boundary trivia is not covered

The node-level entry points write back their target's leading and trailing trivia. The first shape excludes both the leading trivia of the declaration's first token and the trivia behind the `;`, so the precheck covers no written-back trivia, and the write-back paths stay as they are.

## Alternatives considered

- **Leave constructs as written, silently** (the policy before this change) — rejected: each shape is still analyzed, specified, and guarded case by case, and users see no signal.
- **Refuse per construct and format the rest of the file** — rejected: the phases would still meet the shape and need guards around it, so no complexity is removed.
- **Base the list on analyzer rules** — rejected: the formatter would depend on the analyzers, and a shape without a rule could not be declared.
- **`--suppress-unsupported` formats anyway** — rejected: the phases would have to keep supporting the shape.
- **Check only the target of `FormatNodeInDocumentWithContextAsync`** — rejected: the phases receive the context, so a refused shape elsewhere in it would still reach them, against the one-check-site contract.
