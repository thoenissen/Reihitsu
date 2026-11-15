# Code fixes end the changed code's lines as the formatter does

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH8402-002 (changed: the fix part), RH8402-005 (changed), FMT-PIPE-010 (changed: whitespace and line break at the end of the target's trailing trivia) |

## What

Two code-fix outputs at the end of a line follow what the formatter writes instead of what the document happens to hold.

- **RH8402 writes the detected line ending.** The header the RH8402 fix inserts, and the line break it puts between the header and the text behind it, use the line ending FMT-PIPE-007 detects for the file (SHARED-FIX-008): `\n` in a file whose line breaks are all a lone `\r`, and `Environment.NewLine` in a file without a line break. Until now the fix wrote the line break of the file's first line, or `\n` when the file had none (RH8402-002, RH8402-005). The analyzer's comparison stays in the file's first line ending (RH8402-002): after the fix, the header's own first line break is the file's first, so the comparison still finds the header.
- **The node-level entry points end the target's last line as document formatting does.** `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` write the line break that ends the target's trailing trivia in the detected line ending, and remove the whitespace directly in front of that line break, as FMT-CLEAN-001 removes it in document formatting. This applies behind the result's last token and behind a `/* */` comment in the trivia, whether or not formatting changed the target's last line (FMT-PIPE-010). Until now FMT-PIPE-010 said "keep the target's trailing trivia" and the entry points wrote it back character for character. The line-ending half only states what SHARED-FIX-008 already required ("its trailing trivia included"). The whitespace half changes FMT-PIPE-010 for a target whose last line formatting leaves unchanged, which no other requirement decided, so it is listed here.

## Why

- RH8402: a header in the first line's ending can be a minority ending in a file with mixed line endings. Its three lines can then outnumber the others, so the formatter detects the header's ending for the whole file and rewrites every other line (observed: `namespace N;\nclass C\r\n{\r\n}\r\n` becomes all `\n` after the fix and `scripts/format.sh`). Every other fix writes the detected line ending (SHARED-FIX-008); RH8402 was the exception, and RH8402-005's `Why:` gave no reason for it.
- Node-level whitespace: the entry points wrote the target's original trailing whitespace back behind the result after the last phase, so the cleanup phase never saw it. A fix such as RH3202 that turns `=> _value;···` into a block left `}···` behind, which formatting the result removes again (SHARED-FIX-007) and RH5602 reports. One rule for the end of the target's last line, the same as document formatting, needs no test of whether formatting changed that line.

## Visible output changes

`␍` marks a `\r`; `·` marks a space. Line breaks shown without `␍` are `\n`.

RH8402 · Files must start with configured copyright header, with the configuration of the RH8402 spec, file `C.cs`, first line ending in `\n`, the others in `\r\n`:

```csharp
namespace N;
class C␍
{␍
}␍
```

Before:

```csharp
// <copyright file="C.cs" company="Example">
// Copyright (c) Example.
// </copyright>
namespace N;
class C␍
{␍
}␍
```

After:

```csharp
// <copyright file="C.cs" company="Example">␍
// Copyright (c) Example.␍
// </copyright>␍
namespace N;
class C␍
{␍
}␍
```

In a file whose line breaks are all a lone `\r`, the header's line breaks change from lone `\r` to `\n`. In a file of one line without a line break, they change from `\n` to `Environment.NewLine` (unchanged on Linux and macOS, `\r\n` on Windows).

RH3202 · Expression-style methods should not be used, with three spaces at the end of the method's line:

```csharp
class C
{
    private int _value;

    public int GetValue() => _value; /* c */···
}
```

Before:

```csharp
class C
{
    private int _value;

    public int GetValue()
    {
        return _value;
    } /* c */···
}
```

After:

```csharp
class C
{
    private int _value;

    public int GetValue()
    {
        return _value;
    } /* c */
}
```

The same holds without the comment (`}···` becomes `}`), and for every formatter-delegating fix whose target ends in whitespace before its line break, also where the target's last token stays its last token (an RH5404 property `int P { get { return 1; } }···` ends in `}` without the spaces). Spaces behind a `//` comment are the comment's text and stay; spaces in front of a comment stay.

Every formatter-delegating fix whose target's line ends in a line break other than the detected one, in a document with mixed or lone-`\r` line breaks, now writes the detected line ending there (SHARED-FIX-008, unchanged; stated in FMT-PIPE-010).

## Decisions

### RH8402 writes the detected line ending

The maintainer chose option B of the run's output question 2: the RH8402 fix writes the line ending FMT-PIPE-007 detects, `\n` for an all-lone-`\r` file and `Environment.NewLine` for a file without a line break. The analyzer keeps comparing in the file's first line ending (RH8402-002), which still matches after the fix and changes no diagnostic.

### The whitespace at the end of the target's last line is always removed

The maintainer chose option B of the run's output question 1: both node-level entry points remove the whitespace in front of the line break at the end of the fixed target's last line in every case, as document formatting does.

## Alternatives considered

- **RH8402 keeps the first line's ending** — rejected by the maintainer: it can flip a mixed file's detected line ending and is the only fix that does not follow SHARED-FIX-008.
- **RH8402 withholds its fix in a file with non-uniform line breaks** — rejected: it would withhold a whole header insertion for a harmless shape, at the same cost as writing the detected ending.
- **Remove the whitespace only where formatting changed the target's last line** — rejected by the maintainer: it needs the entry points to compare the phases' result with the input, and it leaves whitespace that document formatting removes.
- **Keep the whitespace** — rejected by the maintainer: it leaves trailing whitespace on a line the fix wrote, which formatting the result removes again.
