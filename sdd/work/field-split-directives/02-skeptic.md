Role: skeptic
Commit: 9a46ba8
Verdict: #1728: proceed; #1586: proceed narrower (the leading-trivia exception must not cover disabled text), output question stays open and needs amending

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Commands below ran on fixtures in the scratchpad, deleted afterwards. "Guard removed" means a throwaway worktree of 9a46ba8 with only the `CarriesDirective(fieldDeclaration) == false` operand of `FieldDeclarationSplitTransform.Splits` deleted. With that change and no precheck change, the split does what the spec diff of 3b1b782 states for every input that does not hold FMT-UNSUP-010. That makes it a faithful stand-in for the leading-trivia cases below. Do not use it as a source of expected test values (FC-035): every output quoted here is checked against FMT-STRUCT-022 as written.

## thoenissen/Reihitsu#1728

### Premise

The issue is real, and the CLI reaches it with whole-document formatting. On `main`, `FieldDeclarationSplitTransform.CarriesDirective` keeps every field declaration with a directive or disabled text anywhere in its trivia whole. The spec author's run (change doc, *A directive inside the declaration refuses the file*) shows today's output for `int a,⏎#if X⏎b,⏎#endif⏎c;`: the declaration stays whole, and `c;` moves to the member column. The code-fix side is reached through RH7101 (`TryGetFixableFieldDeclaration` calls `SplitsFieldDeclaration` and then `FindUnsupportedSyntax(fieldDeclaration.Parent)`) and through every fix that formats the type or the declaration.

The maintainer's escalation-10 answer is carried in the issue text: declare the shape, in a change of its own. The span "from its first token through its `;`" is read like FMT-UNSUP-004's span, which uses the same words and explicitly excepts the leading trivia of the first token. So excepting the leading trivia is the established reading of those words. Whether the declaration is then split is the #1586 question.

### Mechanism and candidates

_N/A_ (behavior change). The decision comes from the shape alone. A field declaration (`FieldDeclarationSyntax`) with more than one declarator in the build being formatted is refused when a directive or disabled text stands in the trivia of any of its tokens, outside the leading trivia of its first token. That includes the inside of initializer values. Candidate owners, from the split's visitor overrides: `ClassDeclarationSyntax`, `StructDeclarationSyntax`, `RecordDeclarationSyntax`, `InterfaceDeclarationSyntax`, `UnionDeclarationSyntax`. The spec decides the shape by syntax regardless of owner (FMT-UNSUP-003), which I agree with. `EventFieldDeclarationSyntax` and `LocalDeclarationStatementSyntax` are other types. The split never sees them, so they are correctly excluded: declaring a shape that deletes nothing for them would violate `AGENTS.md`.

### Breaking inputs

Failure classes that reach the change: FC-003, FC-020, FC-038, FC-039, FC-041, FC-042, FC-043, FC-031. FC-040 is not reached, because directives do not depend on the documentation mode. FC-001 and FC-044 reach it through the leading-trivia boundary, which is reported under #1586 because that boundary is the #1586 question.

Matrix for the refused side. Each cell must be refused, with the input returned unchanged and the reason being FMT-UNSUP-010's unless noted:

| Axis | Members |
|---|---|
| Position of the directive or disabled text | between two attribute lists; between attribute list and modifier; between two modifiers; between modifier and type; inside the type (`List<⏎#if X⏎int⏎#else⏎long⏎#endif⏎>`); between type and first name; between name and `=`; between `=` and value; deep inside a value (statement lambda body, a lambda nested in a collection expression: FC-039, because the walker must not skip values as FMT-UNSUP-004's does); between value or name and `,` on a later line (reason and location are **FMT-UNSUP-009's**, the tie); between `,` and the next declarator; in front of `;`; inside an attribute argument list |
| Kind | `#if`/`#else`/`#endif` group with disabled text; a group whose branch is active through a `#define` at the top of the file (no disabled text); `#if false` (FC-020); `#pragma warning`; `#region`/`#endregion`; `#nullable` |
| Declarator count | 1 (not refused, also with a directive among the modifiers: both sides, FC-003); 2 or more; build-dependent `private int _a⏎#if X⏎, _b⏎#endif⏎;` (one declarator without `X`: not refused under FMT-UNSUP-010) |
| Owner | class, struct, record, interface (static fields), union; a type nested in another type (the outer type and the document are refused, and the outer type's RH7101 fix is withheld); `const` fields; fixed-size buffers `fixed int a[2], b[3];` in an `unsafe struct` |
| Entry point | CLI or whole document (refused and reported); RH7101 fix on the declaration and on another declaration of the same type (withheld, diagnostic stays); RH5113 (declaration) and RH5111 on a declarator (declaration as context): withheld; a fix that formats only a value, such as RH5301/RH5303/RH5305: not refused (FMT-UNSUP-003, FC-041) |
| Co-occurrence | a comment (FMT-UNSUP-004) before and after the directive in the same declaration: the shape first in the text gives the reason (FMT-UNSUP-001) |

Not refused (both sides): an event field and a local with the same directive between declarators; a single-declarator field with a directive between attribute and modifier; a node-level `VariableDeclarationSyntax` or declarator holding the directive.

FC-038 / FMT-UNSUP-005: format the split output of every #1586 cell a second time. It must not be refused and must not change. The split leaves the leading trivia on a one-declarator field, so no phase produces the shape. I checked this by reading the split, not by running it.

FC-042: `CarriesDirective`, its operand, and the sentences in the documentation comments of `Splits` and `SplitFields`, of `SplitsFieldDeclaration`, and of `RH7101DoNotCombineFieldsCodeFixProvider.TryGetFixableFieldDeclaration` ("one with a directive") still describe the old guard (FC-031). RH7101's analyzer condition `ContainsConditionalCompilationBoundary` correctly stays: it is the rule's own decision.

FC-043: the RH7101 fix asks `FindUnsupportedSyntax` about `fieldDeclaration.Parent`, the same node it passes as context to `FormatNodeInDocumentWithContextAsync`. That path reaches the precheck through `FormatContext` (`ReihitsuFormatter.cs:705` ff.). Nothing to change.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` | must change (new shape and reason constant) |
| `FieldDeclarationSplitTransform` | must change (directive guard removed; see #1586 for what must stay) |
| `ReihitsuFormatter.SplitsFieldDeclaration` | follows `Splits`; documentation must change |
| RH7101 analyzer | must stay |
| RH7101 code fix and Fix All | code must stay; its documentation comment must change; results change through the formatter |
| Other formatting fixes (RH5110, RH5404, RH5113, RH5111, RH5112, RH5201) | no code change; withheld on code that holds the shape (visible, maintainer-decided) |
| CLI | no code change; reports through the existing unsupported path |
| `Reihitsu.Core` | not involved |
| Rule page `RH7101.md`, `general-notes.md` | not involved (no user document lists unsupported shapes; checked with `rg -i unsupported documentation/`) |
| Tests | the three FMT-STRUCT-023 tests invert to refusals; new FMT-UNSUP-010 tests |

### Refuse option

The issue is itself the refuse option (declaring the shape), and the maintainer chose it. The alternative was to keep the guard, which the maintainer rejected. The class is rare: a combined field already violates RH7101, so it is rare by definition (`AGENTS.md`, *Special cases*), and a directive between its tokens is rarer still. The cost is one cheap trivia check in the precheck, minus the guard. No simpler option.

### Output questions

_None_ beyond #1586's.

### Internal choices

- Not excepting initializer values: agreed. A group that crosses a comma cannot be told apart from one inside a value without matching directives.
- At the tie with FMT-UNSUP-009, the reason stays FMT-UNSUP-009's: agreed. It keeps today's CLI text.
- RH7101-005 is retitled in place rather than retired: agreed. This is internal.

### Issue goals

- "Deletes the directive condition of `SplitsFieldDeclaration`": deleted only for the span of the shape. A narrowed condition for disabled text in the leading trivia must stay (#1586 below). This is internal: that class keeps `main`'s output.
- "Deletes RH7101-005": changed in place instead. Internal.
- Everything else serves the issue's purpose.

### Architecture findings

_None._

### Follow-up candidates

- I agree with the spec author's follow-up (the event field or local declarator behind an `#endif` loses its continuation indent). I did not re-run it.

## thoenissen/Reihitsu#1586

### Premise

The issue is real. On the current code, `scripts/apply-fix.sh RH7101` on

```csharp
class C
{
    #region Fields

    private int _x, _y;

    #endregion // Fields
}
```

gives `no-fix-offered` (LF and CRLF), and the formatter keeps the declaration whole (the spec author's run). The severity holds: `#region Fields` in front of the fields is this repository's own house style, so for a user who follows it, every combined field carries a permanent RH7101 diagnostic.

The issue's reasoning is wrong in one point. "Nothing is dropped or relocated" because "the directive stands in front of the declaration" is true for directives, but **not for disabled text** in the leading trivia of the first token. Disabled text there can be part of this very declaration in another build: an attribute list or a modifier inside an inactive `#if`. The split keeps that trivia on the first field only (FMT-STRUCT-022), so in the other build the attribute or modifier then applies to the first field alone. The CLI parses with no preprocessor symbols (`FormatCommandHandler.cs:134`, `CSharpParseOptions.Default` with only a language version), so every `#if DEBUG` group is disabled text for the CLI. The RH7101 fix meets the same input in any configuration that does not define the symbol.

### Mechanism and candidates

The defect is in the spec diff (3b1b782), not in `main`. Mechanism: the exception for the first token's leading trivia admits disabled text, which another build compiles as tokens of the declaration, while the split gives that trivia to the first field only. Membership is decidable: the leading trivia of the declaration's first token holds `DisabledTextTrivia`. The members that actually fail are the ones whose disabled text holds a prefix of the declaration, and telling them apart needs the disabled text parsed (FC-044). Refuse by the cause, not by the cells.

FC-001 applies: the exception inspects a smaller span than the decision it protects.

FMT-UNSUP-010's own `Why` ("would put the repeated modifiers and type into the text of another build") and FMT-PIPE-003 ("MUST leave a construct unchanged when they cannot prove the rewrite equivalent") and FMT-STRUCT-024 ("drop or misplace a directive or disabled text, or change what the code compiles to") are unchanged requirements. They already forbid splitting this class. As written, FMT-UNSUP-010's third condition, FMT-STRUCT-022's new paragraph ("the `#endif` and the disabled text of a group in front of it"), RH7101-005, and the change doc's *What* contradict them.

### Breaking inputs

Matrix: what the first token's leading trivia holds × entry point. Outputs are with the guard removed (= the spec diff as written), using `scripts/format.sh` and `scripts/apply-fix.sh`, LF and CRLF:

| Cell | Leading trivia | Result as specified | Holds? |
|---|---|---|---|
| R | `#region Fields` | split | holds (spec author's run) |
| I | active `#if !RELEASE` around the declaration | split | holds (spec author's run) |
| L7 | `#pragma warning disable CS0169` (restore behind `;`) | split | holds (run) |
| L3 | disabled text: inactive `#if DEBUG` holding a whole member `private int _dbg;`, then `#endif` | split | holds (run): safe, but in the disabled-text class |
| L4 | disabled text: inactive `#if X` branch, declaration in the active `#else` | split | holds (run): safe, but in the disabled-text class |
| L8 | disabled text: `#if false` holding `[Obsolete]` | split | holds (run): no build defines `false`, but in the disabled-text class |
| **L5** | disabled text: inactive `#if DEBUG` holding an attribute of the declaration | split | **fails** |
| **L6** | disabled text: inactive `#if NETSTANDARD` holding a modifier of the declaration | split | **fails** |
| **L9** | disabled text: inactive `#if DEBUG` / `#elif TRACE` each holding an attribute | split | **fails** |

The failing cells fail on every entry point that splits. I ran CLI (L5, L6, L9), the RH7101 fix (L5, L6: `fixed (1 action(s), 1 iteration(s))`), and the RH5110 fix, which formats the type (L5 variant). On the current code, L5 and L6 stay whole under the CLI, and the RH7101 fix gives `no-fix-offered`.

L5. Input and the output as specified (CLI and RH7101 fix alike):

```csharp
using System.Diagnostics;

class C
{
#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    private int _a, _b;

    void M()
    {
        _ = _a + _b;
    }
}
```

```csharp
#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    private int _a;
    private int _b;
```

In a DEBUG build, the attribute now applies to `_a` only.

L6 (`static` lands on `_a` only in a NETSTANDARD build, so `_b` becomes an instance field):

```csharp
class C
{
#if NETSTANDARD
    static
#endif
    private int _a, _b;
}
```

L9:

```csharp
using System;

class C
{
#if DEBUG
    [Obsolete]
#elif TRACE
    [NonSerialized]
#endif
    private int _a, _b;
}
```

RH5110 (same split through the type, plus its own constraint fix):

```csharp
using System;

class C<T> where T : class
{
#if DEBUG
    [Obsolete]
#endif
    private int _a, _b;
}
```

Expected output with the guard removed: `where T : class` moves onto its own line, and the declaration splits into `_a;` / `_b;` under the `#endif`.

The test implementer needs, on the split side: R, I, L7, and an `#endif` of an *active* group in front (no disabled text; for example a `#define D` at the top and `#if D⏎int _x;⏎#endif` in front). On the kept side, whatever the amended spec decides: L5, L6, L9 at least, through CLI or document and through the RH7101 fix (diagnostic reported, no fix), and L3 and L4 under whichever check the spec settles on.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FieldDeclarationSplitTransform.Splits` | must change: the directive operand goes, a check for disabled text in the first token's leading trivia stays (under recommendation 1A below) |
| `SplitsFieldDeclaration` / FMT-PIPE-031 | keeps a narrowed condition (disabled text in the leading trivia); the spec must say so |
| RH7101 fix | no code change; withheld for the kept class through `SplitsFieldDeclaration` |
| Specs | FMT-UNSUP-010 (third condition), FMT-STRUCT-022 (new paragraph), RH7101-005, FMT-PIPE-031, change doc *What*, *Special cases → Checked and kept*, and the #1586 alternative "Refuse the leading trivia too" (it would now delete a special case) must be amended |
| Precheck, CLI, other fixes, Core, rule page | not involved beyond #1728 |

### Refuse option

- **Leave as written (keep whole, fix withheld) for disabled text in the first token's leading trivia.** This is `main`'s output for the class, and FMT-PIPE-003 and FMT-STRUCT-024 already demand it. It costs one cheap trivia-kind check in `Splits`, which FMT-PIPE-031 carries to the fix at no extra cost. The class is rare: a combined field directly behind an inactive conditional group. It is local and silent, so nothing visible is in question relative to `main`.
- **Narrower variant:** keep whole only when an `#endif` follows the disabled text in the same leading trivia, so that L4 is split. That still keeps L3 whole, buys one rare cell, and reads directive order. I do not recommend it.
- **Declare it unsupported (escalation 10).** Exact shape: disabled text in the leading trivia of the first token of a field declaration with more than one declarator, from the first trivia of that token through the token itself; nothing is excepted. Special case it would delete: the split's check for inactive code in front of a combined field (the narrowed `Splits`/FMT-PIPE-031 condition). Common code it would refuse: every file in which an inactive `#if DEBUG` member, or any inactive group, directly precedes a combined field. For the CLI that covers every `#if DEBUG` group (no symbols defined). That code is rare by `AGENTS.md`'s definition only because the combined field violates RH7101. It must be named to the maintainer (escalation 10). I recommend against it: it is a single construct, and leaving it as written is enough.
- **Option C of the drafted question (refuse the whole leading trivia)** would now delete that narrowed check too. The draft's line "C would remove no further special case beyond A's" is no longer true.

### Output questions

The drafted question 1 (01-spec-author.md) stays valid in its input and its A, B, and C outputs: R and I hold no disabled text. Two amendments are needed before the lead asks it:

- C's "deletes" line must name "the split's check for inactive code in front of a combined field" as well, because A now keeps it.
- A declare option for disabled text alone must be added (escalation 10), in the form *Output questions* prescribes:

**1D · RH7101 · Do not combine fields — inactive conditional code directly in front of a combined field**

Input:
```csharp
using System.Diagnostics;

class C
{
#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    private int _a, _b;
}
```

Before (`main`): the formatter keeps the declaration combined; RH7101 is reported, and no fix is offered.

After, A (recommended): as before. The declaration stays combined, and RH7101 is reported without a fix.

After, D: the input is unchanged.
The CLI reports: `error: <file>(5,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`
No code fix that formats this type or field is offered here (RH7101, RH5110, RH5404, RH5113, RH5111, among others).
Declared in this work.

Why A: splitting would leave the attribute on `_a` alone in a DEBUG build, and keeping the declaration whole is today's output for a rare shape. D would refuse the whole file to delete one cheap check in the field split.

The location (5,1) and the reason text in D are a reading of the spec, not a run. The reason text is the lead's to settle.

### Internal choices

- The check for the kept class should be "the first token's leading trivia holds disabled text", not a directive-order rule (above). This is internal relative to `main`, but it decides whether L4 changes, so the lead records it.
- Whether FMT-STRUCT-023 is reinstated narrowed or the condition lives in FMT-STRUCT-021/FMT-STRUCT-022: spec wording, which the lead decides. I recommend stating it in FMT-STRUCT-022 next to the leading-trivia rule, so the one place that keeps the trivia also says when it does not split.

### Issue goals

- "The maintainer's choice is specified in FMT-STRUCT-023 and RH7101-005": FMT-STRUCT-023 is retired and the choice lives in FMT-STRUCT-022, FMT-UNSUP-010, and RH7101-005. Internal.
- "Tests for a region, an `#if`, and a directive between declarators": keep these, and add the disabled-text cells above (L5, L6, L9 kept whole; L3, L4 per the settled check).
- The issue's B/A framing ("the guard should still hold where a directive stands between declarators or in a trailing position") is superseded by #1728: such a directive is now refused, not kept whole. Nothing new is visible beyond #1728's decided outputs.

### Architecture findings

_None._ The narrowed split check and the precheck's shape cover disjoint trivia, so neither copies the other.

### Follow-up candidates

_None._

## Whole run

- The leading-trivia exception and its defect belong to both issues' shared boundary. Amend the spec (spec author) before the test implementer starts, together with the amended question 1.
- Runs: `scripts/format.sh` on the current code (L5, L6) and with the guard removed (L3–L9); `scripts/apply-fix.sh RH7101` on the current code (R, L5) and with the guard removed (L5, L6); `scripts/apply-fix.sh RH5110` with the guard removed (L5 variant). Worktree and fixtures deleted.
