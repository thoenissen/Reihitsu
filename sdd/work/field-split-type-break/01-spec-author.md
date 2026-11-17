Role: spec author
Commit: e07a89a
Verdict: open question

The spec cannot be written without the answer: what the repeated head of a split field carries is the whole of what thoenissen/Reihitsu#1587 asks FMT-STRUCT-021 to state, and the two readings give different output. Per `sdd/README.md` (*Open questions*) nothing is written into a spec and no change doc is written until the maintainer answers; what each answer leads to is listed under *After the answer* below so the continuation is one step.

## Changed requirements

_None yet_ — pending the output question.

## Examples for the tests

_None yet._ Note for later: `FieldDeclarationSplitTests.WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` (untagged) already pins today's output for `private int⏎_first, _second;`; options B and C change its expected value.

## Established facts (measured on e07a89a)

- Whole-document output, `scripts/format.sh` on fixtures; RH7101 fix output, `scripts/apply-fix.sh RH7101`, LF and CRLF, identical to the formatter output in every case (FMT-PIPE-029).
- `FieldDeclarationSplitTransform.CreateSplitField` copies the declaration's attributes, modifiers, and type with all their trivia to every field; it drops only a later declarator's own leading trivia and the leading trivia of the declaration on later fields. So every line break the author wrote inside the head — behind the type (in the type's trailing trivia), between modifiers, between a modifier and the type — is repeated on every field. No FMT-BREAK requirement joins any of these gaps, so the line breaks survive the later phases; the single-declarator field keeps its break too.
- FMT-UNSUP-004 and FMT-UNSUP-010 refuse every comment, directive, and disabled text in a multi-declarator field outside its first token's leading trivia and its initializer values, so the head's trivia below the first token is only whitespace and line breaks (`private int // c⏎_g, _h;` measured: skipped as unsupported).
- The column of the declarator behind the break (member column, no continuation indent) is thoenissen/Reihitsu#953's and is the same in every option below.

| Input (inside `class C`) | Output today, formatter and RH7101 fix |
|---|---|
| `private int⏎    _g, _h;` | `private int⏎_g;⏎private int⏎_h;` |
| `private int⏎    _g = 1, _h = 2, _i;` | each of the three fields `private int⏎_x…;` |
| `private int⏎    _x;` (no split) | `private int⏎_x;` |
| `private int _g,⏎    _h;` | `private int _g;⏎private int _h;` |
| `private⏎    int _g, _h;` | `private⏎int _g;⏎private⏎int _h;` |
| `[System.NonSerialized]⏎private static readonly⏎    int⏎    _g, _h;` | both fields repeat attribute line, `private static readonly⏎int⏎_x;` |

(all lines shown at the member column, as measured.)

## Output questions

**1 · Formatter field split and RH7101 · Do not combine fields — a line break written inside the field's head**

Input:
```csharp
class C
{
    private int
        _g, _h;
}
```

Before (`main`):
```csharp
class C
{
    private int
    _g;
    private int
    _h;
}
```

After, A (recommended) — every field repeats the head as written, line breaks included (today's output, written into the spec):
```csharp
class C
{
    private int
    _g;
    private int
    _h;
}
```

After, B — the first field keeps the author's line breaks; every later field gets its attributes, modifiers, and type written on one line in front of its declarator:
```csharp
class C
{
    private int
    _g;
    private int _h;
}
```

After, C — the line break between a field's type and its declarator is joined on every field, split or not (also `private int⏎_x;` becomes `private int _x;`):
```csharp
class C
{
    private int _g;
    private int _h;
}
```

Why A: each split field keeps exactly the shape the author gave the declaration, like the attributes and modifiers it repeats; the break is the author's layout, which the formatter keeps on an unsplit field too, and B makes fields of one declaration differ. (The issue's original reason for B — the break held a comment in front of the first declarator — no longer applies: that input is now refused as unsupported.)

Measured: A is today's output; the B and C outputs above are stable under today's formatter (reformatting them changes nothing). With B, the input `[System.NonSerialized]⏎private static readonly⏎int⏎_g, _h;` gives the first field as written and `[System.NonSerialized]⏎private static readonly int _h;` for the second. C is a new line-break join beyond the field split (it changes unsplit fields, and arguably properties and locals with the same break); if chosen, it is better done as a change of its own, and this run would then close nothing but record A or B for the split.

## After the answer

- **A** — a clarification, no change doc: FMT-STRUCT-021 (or a new FMT-STRUCT-031 "A split field repeats its head as written") states that every field repeats the attributes, modifiers, and type with the whitespace and line breaks between and behind them as written, the first field also keeping the declaration's leading trivia and the first declarator's leading trivia; example = the input above; the existing test gets the tag. The lane narrows to tests (no production change).
- **B** — change doc `sdd/changes/2026-11/field-split-repeated-head.md` and FMT-STRUCT-031: every field after the first writes its head with the minimal separators of FMT-STRUCT-026 in place of the whitespace and line breaks inside and behind it; the first field unchanged; examples from the table above; RH7101 fix result changes identically (FMT-PIPE-029). Existing test's expected value changes.
- **C** — out of this issue's split mechanism; becomes a follow-up (new FMT-BREAK join); the split part of this issue is then answered A or B as well.

## Internal choices

- No spec text and no change doc written before the answer — the alternative (a Draft change doc for B) would put an undecided case into `sdd/` and be deleted if A is chosen.
- The question covers every line break in the head (behind the type, between modifiers, behind the modifiers), not only the one behind the type: the mechanism is the same copy of the head's trivia, and deciding only the type break would leave the others an open question of the same kind.

## Follow-up candidates

_None._ (The declarator column is already thoenissen/Reihitsu#953.)
