Role: spec author
Commit: 953ffae
Verdict: spec written

Repair round 3 (last), `00-lead.md` row 26: auditor finding #2 and the parameter-list hint of `18-auditor.md`. The prompt carried a fix direction (`unsafe struct`, completing the parameter bullet); it matches the auditor's suggested change and my own reading, so I followed it on the evidence below, not on the prompt.

## Changed requirements

- FMT-SPACE-021 — One space between a type or pattern and the name it declares — clarified:
  - The second example ("for the members and statements that declare a variable") declares its type as `unsafe struct C` instead of `unsafe class C`, in both blocks; nothing else in either block changes. A fixed-size buffer is legal only in a struct (CS1642 for a class).
  - The parameter bullet names every parameter list whose parameters can carry a type, and says "in every parameter list" instead of "of any parameter list": a method, a local function, a constructor or a primary constructor, an operator or a conversion operator, an indexer, a delegate, an anonymous method (`delegate(int? x) { }`), a lambda with explicitly typed parameters, and an extension block (`extension(string? s)`). These are all the `ParameterSyntax` owners Roslyn has; a function pointer's parameters (`FunctionPointerParameterSyntax`) have no name, and a destructor has no parameters. No visible output changes: the rule's single `ParameterSyntax` arm already covers these lists.

No change doc edit: change-doc entry 6 has no fixed-size buffer and names parameters generically. No other spec or change doc declares a fixed-size buffer (`grep 'fixed int|fixed byte|fixed char'` over `sdd/specs/formatter` and the change doc).

## Evidence

- Scratch projects (`net10.0`, `AllowUnsafeBlocks`, `Nullable` enabled) under `/tmp/claude-0/-home-user-Reihitsu/6ed598ec-bf0c-58dc-9713-1c7e929b4b44/scratchpad/ex21/in` and `.../ex21/out`, holding the two example blocks extracted from the spec file: both `Build succeeded`, zero errors, only CS0169 (`_f` unused) and CS0067 (`E` unused) warnings.
- Branch CLI (`scripts/format.sh`, working tree at 953ffae plus this spec edit) over the input block: the result equals the output block byte for byte except for the final newline, which the cleanup phase removes (the test's raw-string literal has none either).
- Branch CLI over a probe with `explicit operator D(int?v)`, `delegate(int?x) { … }` and `extension(string?s) { public int L(int[]n) … }`: each gets `int? v`, `int? x`, `string? s`, `int[] n`.

## Text the verbatim test must mirror

`DeclaredNameSpacingTests.MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample`: in both `input` and `expected`, replace the line

    unsafe class C

with

    unsafe struct C

Every other line of both literals stays as it is. The full input block is now:

```csharp
using System;
using System.IO;

unsafe struct C
{
    private int?_f;
    private event Action?E;
    private fixed int  _buf[4];

    void M(int[] xs)
    {
        fixed (int*p = xs)
        {
        }

        for (int?i = 0; i < 1; i++)
        {
        }

        using (Stream?s = null)
        {
        }

        using Stream?t = null;
    }
}
```

and the expected block:

```csharp
using System;
using System.IO;

unsafe struct C
{
    private int? _f;
    private event Action? E;
    private fixed int _buf[4];

    void M(int[] xs)
    {
        fixed (int* p = xs)
        {
        }

        for (int? i = 0; i < 1; i++)
        {
        }

        using (Stream? s = null)
        {
        }

        using Stream? t = null;
    }
}
```

Optional for the test implementer, not required by the spec: the summary of `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` lists fewer lists than the bullet now does; whether to add anonymous-method, conversion-operator, and extension-block parameters to a test is the test implementer's call.

## Examples for the tests

FMT-SPACE-021 (second example only; the first example is unchanged).

## Output questions

_None._

## Internal choices

- Enumerated every parameter list rather than writing "such as …": an exhaustive list closes the boundary the auditor hinted at, matching how the variable-declaration bullet was made exhaustive in round 2. Alternative: "every parameter list, such as …".

## Follow-up candidates

_None._
