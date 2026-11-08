Role: skeptic
Commit: 4741cc1
Verdict: #963: output question (one member of its class; the reported instances proceed); #1384: proceed

The prompt carried no intended fix or hoped-for verdict. `00-lead.md` and `01-spec-author.md` were read as facts; their readings were checked against the code and against runs, not adopted.

All runs below are on the branch at 4741cc1, which carries no production change, so they show `main`'s behavior. Fixtures lived under the session scratchpad and are deleted. Commands: `scripts/format.sh <file>` (whole-document formatting, the CLI path) and `scripts/apply-fix.sh <RH####> <dir>` (LF and CRLF; both endings gave the same result in every fixture).

## thoenissen/Reihitsu#963

Scope: only the field split. The `{// why` expression-body part is #1439, not this run.

### Premise

The issue is real, and callers reach it.

- **CLI, whole-document formatting.** `FormatCommandHandler` → `StructuralTransformPhase` → `FieldDeclarationSplitTransform`. `scripts/format.sh` on `private int /* c */ _first, _second;` gives `private int /* c */ _first;` / `private int /* c */ _second;`, so the comment is copied. The same copying happens for `private /* c */ int _a, _b;`, `[Obsolete /* c */] private int _c, _d;`, `[Obsolete] /* c */ private int _s, _t;` (comment behind the attribute list on its line), `[Obsolete]⏎// c⏎private int _e, _f;` (own-line comment between the attribute list and the modifiers), `public /* c */ static int _q, _r;` (between modifiers), and `private System. /* c */ Int32 _o, _p;` (inside the type).
- **RH7101 fix.** `FormatNodeInDocumentWithContextAsync(field, type)` → `SplitsFieldDeclaration` → `FormatSplitFieldInContext`. `scripts/apply-fix.sh RH7101` offers the fix for every input above and writes the same copies. For `[Obsolete]⏎// c⏎private int _e, _f;` it also writes a blank line between `[Obsolete]` and `// c` on the first field.
- **Code fixes that format a whole type** (RH7103 and the others listed under #1384) go through the same transform.

FMT-STRUCT-024 (P36) already forbids copying a comment next to code it was not written next to. The maintainer's decision #913 Q3 (issue comment 6232668547) says "a comment between the type and the first declarator blocks the split". FMT-STRUCT-030 states that guard correctly for every repeated position.

One member of the class behaves differently today and is not decided by FMT-STRUCT-024; see *Output questions*. It is a comment in the **first declarator's own leading trivia**, which is a comment on a line after the type, in front of the first name:

```csharp
private int
    /* c */ _first, _second;
```

Today the formatter and the RH7101 fix split it and keep the comment only in front of `_first`. No copy is made. Tests tagged `RH7101-004` pin this output on purpose.

### Mechanism and candidates

Mechanism: `CreateSplitField` builds each generated field from the whole original declaration and replaces only its declarator list. Every trivia of the tokens in front of the first declarator therefore appears in every generated field: attribute lists, modifiers, the type, and the type's trailing trivia. `Splits` has no condition on comments there. The first declarator itself is carried, with its leading trivia, onto the first field only.

Dispatch: `SplitFields` is called from `VisitClassDeclaration`, `VisitStructDeclaration`, `VisitRecordDeclaration` (record class and record struct), and `VisitInterfaceDeclaration`, nested types included. The guard is `Splits(FieldDeclarationSyntax)`, which `ReihitsuFormatter.SplitsFieldDeclaration` returns unchanged. Field kinds: plain, `const`, `static`, `readonly`, fixed-size buffer, with and without initializers.

### Breaking inputs

Classes: FC-021 (comment between modifiers or on the header), FC-016 (sibling shapes), FC-001 (the guard span must be the full span the split repeats), FC-003 (both sides of every boundary), FC-007/FC-009 (no fix offered where the formatter does not split), FC-032 (the change flips RH7101 and every fix that formats a type).

Class matrix, measured today. The axes are the comment slot, the comment kind, and the container. Every member below copies the comment except the last two rows.

| Slot (Roslyn owner) | Example | Today | FMT-STRUCT-030 |
|---|---|---|---|
| Inside an attribute list | `[Obsolete /* c */] private int _c, _d;` | copied | not split |
| Behind an attribute list, same line (trailing of `]`) | `[Obsolete] /* c */ private int _s, _t;` | copied | not split |
| Own line between attribute list and modifiers (leading of `private`) | `[Obsolete]⏎// c⏎private int _e, _f;` | copied | not split |
| Between modifiers | `public /* c */ static int _q, _r;` | copied | not split |
| Between modifier and type | `private /* c */ int _a, _b;` | copied | not split |
| Inside the type | `private System. /* c */ Int32 _o, _p;` | copied | not split |
| Behind the type on its line (trailing of `int`), `/* */` and `//` | `private int /* c */ _first, _second;` | copied | not split |
| `///` after an attribute list (Parse; not documentation per SHARED-DOC-004) | `[Obsolete]⏎/// <summary>m</summary>⏎public int U, V;` | copied, fix offered | not split, no fix |
| **Leading trivia of the first declarator** (own line in front of the first name), `/* */`, `//`, `///`, `/** */` | `private int⏎    /* c */ _first, _second;` | split, comment on the first field only | **depends on the reading; see Output questions** |
| Inside or behind the first declarator | `_k = /* c */ 1, _l`, `_m /* c */, _n` | split, comment stays with the first field | split |

Containers to vary in tests: class, struct, record, record struct, interface, nested type. The transform shares one predicate for all of them, so one container per slot is enough, plus one nested case.

Candidates for the test implementer:

- every row above that is not split, through the formatter and the RH7101 fix (diagnostic reported, no fix);
- the other side of each boundary: a comment in the leading trivia, in the first declarator's initializer, behind the name, and behind the separating comma, each still split;
- `[Obsolete]⏎/// …⏎public int U, V;` in Parse and in None. In None it is an ordinary comment between the first token and the first declarator, so it is still not split.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FieldDeclarationSplitTransform.Splits` | Must change (comment guard). Its summary names only FMT-STRUCT-021/023 (FC-031). |
| `ReihitsuFormatter.SplitsFieldDeclaration` | Must stay (it delegates). Review its summary. |
| `FormatNodeInDocumentWithContextAsync` | Must stay. It branches on `SplitsFieldDeclaration`, which is the phase's own predicate, not a prediction (Q10: none). |
| RH7101 analyzer | Must stay. RH7101 is about a syntax form, not layout, so SHARED-DIAG-010 does not cover it, and reporting a declaration the formatter keeps already happens under RH7101-005. |
| RH7101 code fix | Behavior changes only through `SplitsFieldDeclaration`. No code change is needed for #963. |
| Fix All (RH7101) | Must stay. It filters through the same predicate. |
| Rule page `documentation/rules/RH7101.md` | Must stay (`general-notes.md` covers a diagnostic without a fix). |
| `Reihitsu.Core` | Not involved. `SyntaxTriviaUtilities.IsCommentTrivia` already exists. |
| Other phases | Not involved. |
| Tests | `FieldDeclarationSplitTests.CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` flips. The RH7101 tests that pin the slots above flip too. The family `…BeforeFirstDeclarator…` in `FieldDeclarationSplitTests` (lines 1241–1701), and `VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix` and `VerifyFixLaysOutTheCommentBeforeFirstDeclaratorAsTheFormatterLaysItOut` (tagged `RH7101-004`) in `RH7101DoNotCombineFieldsAnalyzerTests`, flip only under reading B below. |

### Simpler option

Reading A below is the narrower change. The guard covers exactly the tokens every generated field repeats: from the first token through the type's last token, including that token's trailing trivia. It leaves the first declarator's leading trivia to FMT-STRUCT-022, as later declarators are left to it.

### Output questions

**1 · Formatter field split and RH7101 · Do not combine fields — comment on its own line in front of the first declarator**

Input:
```csharp
class C
{
    private int
        /* c */ _first, _second;
}
```

Before (`main`), formatter and RH7101 fix alike (measured):
```csharp
class C
{
    private int
    /* c */ _first;
    private int
    _second;
}
```

After, A (recommended): the same as Before. The comment stands in front of the first declarator, not in the part every field repeats, so the declaration is split and the comment stays in front of `_first`. The RH7101 fix is offered.

After, B: not split, and the RH7101 diagnostic stays without a fix. The declarator line is laid out as a single declaration on two lines is laid out today:
```csharp
class C
{
    private int
    /* c */ _first, _second;
}
```

Why A: the comment keeps describing the declarator it was written next to, as FMT-STRUCT-022 already does for a later declarator, and nothing is copied, so FMT-STRUCT-024 does not refuse the split.

Notes for the lead. These do not belong in the question:

- The B output's column is taken from the single-declarator analog (`private int⏎        /* c */ _first;` formats to `private int⏎    /* c */ _first;` today). I did not run it on an unsplit two-declarator field.
- FMT-STRUCT-030 as written supports both readings. Its bullet says "between the type and the first declarator" (B), but it also exempts "a comment inside the first declarator", and Roslyn's `VariableDeclaratorSyntax` includes its leading trivia (A). The requirement's own `Why:` argues only about "the attributes, modifiers, and type that every generated field repeats", which is A.
- If the lead reads #913 Q3 ("a comment between the type and the first declarator blocks the split") as covering this slot, it decides B. In that case the spec still needs the slot named explicitly, and the existing `RH7101-004`-tagged tests change with a maintainer answer behind them. Q3's example and rationale are about duplication, which this slot does not have. That is why I count it as open.
- The same choice decides `///` and `/** */` in that slot (Parse mode). Under SHARED-DOC-004 they are not documentation of the declaration.

### Internal choices

Whichever answer the maintainer gives, FMT-STRUCT-030 bullet 2 and RH7101-007 should name the first declarator's leading trivia explicitly, on the side the answer puts it. An own-line comment in front of the first name is the case whose owner token is not obvious. I recommend that the spec author amends both before the test implementer starts.

### Issue goals

- "Expected 1" (`{// why`) and "Specs: FMT-STRUCT-006 gains this example": drop. They moved to #1439 in triage. No visible change in this run.
- "The regression test is replaced by one that expects the input unchanged": keep.
- Spec targets FMT-STRUCT-021/-022/FMT-PIPE-002 become FMT-STRUCT-030, with a pointer from FMT-STRUCT-022. This is internal.

### Architecture findings

_None for #963._

### Follow-up candidates

- Pre-existing and outside both issues: a declaration with a line break between the type and the first declarator is split into `private int⏎_second;`. Every generated field repeats the type's trailing end-of-line, and the declarator lands at the member column with no continuation indent (`private int⏎        _g, _h;` → `private int⏎    _g;⏎    private int⏎    _h;`). The single-declarator `private int⏎        _x;` → `private int⏎    _x;` shows the column part without a split. Whether that layout is intended is a formatter question for a follow-up issue, not for this run.

## thoenissen/Reihitsu#1384

### Premise

The issue is real, and three caller paths reach it.

- **CLI, whole-document formatting** (documentation mode Parse; `FormatCommandHandler` uses `CSharpParseOptions.Default`). `/// <summary>The coordinates</summary>⏎public int X, Y;` gives `/// <summary>⏎/// The coordinates⏎/// </summary>⏎public int X;⏎public int Y;`, and `Y` is undocumented. `/** <summary>The sizes</summary> */⏎public int Width, Height;` is split the same way (measured). A documentation comment written behind code (`private int _p; /// …⏎public int X, Y;` and `_q; /** … */ public int W, H;`) is split too, and the documentation phase then moves the comment onto the first field. Roslyn lexes a `///`/`/**` behind code as leading trivia of the next token: the RH7101 fix is withheld for it, and only its leading-trivia check could cause that. So the leading-trivia condition of FMT-STRUCT-030 catches this case.
- **RH7101 fix.** Already withheld for a documented declaration (`apply-fix.sh RH7101`: no fix offered). The fix's private check in `TryGetFixableFieldDeclaration` does this, not `SplitsFieldDeclaration`.
- **Fixes that format a whole type.** `scripts/apply-fix.sh RH7103` on a class with `/// <summary>The coordinates</summary>⏎public int X, Y;`, `Run()`, then `static Create()` moves `Create` and also splits the untouched field into `X;`/`Y;` (LF and CRLF). This confirms the issue comment of 2026-10-23.

The severity holds. The compiler applies the comment to every declarator, so the split drops `Y`'s documentation, and RH8026/RH8027 (enabled by default) then report the formatter's output. FMT-STRUCT-024 forbids it ("would leave a declaration without documentation that applied to it").

### Mechanism and candidates

Mechanism: `Splits` checks only the declarator count, the container kind, and directives. The first generated field takes the declaration's leading trivia (`CreateSplitField`, `variableIndex == 0`), so a documentation comment there documents the first field only. The RH7101 fix escaped this through a private copy of the condition. No other caller has one.

Dispatch and candidates: the same as #963 (four container visitors, one predicate). Callers: whole-document formatting, the RH7101 fix (`FormatNodeInDocumentWithContextAsync`), and every fix that formats a type through `FormatNodeInDocumentAsync` (the issue comment names RH7103, RH7109/RH7102 via `TypeMemberOrderingCodeFixProviderBase`, RH7104, RH5110, RH8305).

### Breaking inputs

Classes: FC-015 (the private copy in the RH7101 fix), FC-016, FC-003, FC-032, FC-033 (documentation mode: formatter tests parse in Parse by default, so the mode-None side needs explicit options), FC-030 (the RH7103 fix formats the whole type), FC-034 (the code-fix regression must use the reported shape).

Matrix. The axes are the comment kind (`///`, `/** */`), its position (own line in front, behind code on the previous member's line, in front of the attribute lists), the documentation mode (Parse, Diagnose, None), and the surface (formatter, RH7101 fix, a whole-type fix).

- `///` and `/** */`, own line, Parse or Diagnose: split today → not split.
- `///` and `/** */` behind the previous member's code on its line: split today → not split. Roslyn puts them in the declaration's leading trivia (see Premise).
- `///` in front of `[Attr]`: today the formatter splits and the fix is withheld (existing test `DocumentationCommentInFrontOfTheAttributesOfTheDeclarationWithholdsTheFix`) → not split.
- Mode None, any of the above: split today and stays split. The trivia is an ordinary comment (`SingleLineCommentTrivia`), so no mode lookup is needed.
- A `///` in front of a later declarator: split today and stays split (FMT-STRUCT-022 moves it).
- An ordinary `//` in front: split, unchanged.

Candidates for the test implementer:

- the two instances of the issue plus the undocumented control;
- the behind-code form;
- `Diagnose`, and `None` with explicit parse options;
- one code-fix regression: RH7103 on the issue comment's input. Use a multi-line `<summary>` in the input. Formatting the whole type also expands a one-line `<summary>`, so the one-line form would pin layout RH7103 leaves unspecified (RH7103 non-goal "layout of the fixed type around the moved member") instead of the split.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FieldDeclarationSplitTransform.Splits` | Must change (documentation guard). |
| `ReihitsuFormatter.SplitsFieldDeclaration` | Must stay. |
| RH7101 code fix `TryGetFixableFieldDeclaration` | Its private documentation check and its summary ("although the formatter splits it") become a second statement of the guard. See *Architecture findings*. |
| RH7101 analyzer | Must stay. |
| RH7103 / RH7109 / RH7102 / RH7104 / RH5110 / RH8305 fixes | Must stay. Their output changes through the formatter. One regression test (RH7103). |
| `DocumentationCommentFormattingPhase` | Not involved. It runs after the structural phase and never sees the declaration split. |
| `FormatNodeInDocumentWithContextAsync` (FMT-PIPE-029) | Must stay. The removed examples with a documented target were never reachable: RH7101-006 already withheld the only caller's fix, so the spec removal costs no supported behavior. |
| Rule page RH7101.md | Must stay. |
| Tests | `FieldDeclarationSplitTests` `Documented…AreSplitAtMemberIndentation` (lines 340–653), `MultiLineDocumentationCommentCombinedFieldsAreSplitAtMemberIndentation`, `FieldAndDeclaratorDocumentationCommentsAreBothPreserved`, `FieldDocumentationAndCommentBeforeFirstDeclaratorAreBothPreserved` flip. `ReihitsuFormatterFieldSplitTests` tagged FMT-PIPE-029 with a documented target change with the spec. The RH7101 mode-None tests stay. |

### Simpler option

_None._ The guard is the condition that already sits in the RH7101 fix, moved to the predicate that FMT-PIPE-031 says owns it.

### Output questions

_None._ The maintainer decided on 2026-11-06 that a documented declaration is not split. Mode None keeps today's output.

### Internal choices

- The documentation guard follows the trivia kind, which encodes the documentation mode, rather than refusing `///` in every mode. This matches RH7101-006 and SHARED-FIX-003. I agree with the spec author.

### Issue goals

- "A change doc records the decision": drop. Triage put the issue in the bug-fix lane because FMT-STRUCT-024 already requires the behavior. Not visible.
- "FMT-STRUCT-021/-022 state the guard": change to FMT-STRUCT-030 with a pointer from FMT-STRUCT-022. Internal.
- "Each input above has a regression test": keep. Add the RH7103 code-fix test requested in the comment of 2026-10-23.
- "Lane: behavior change" in the body: stale. Superseded by triage.

### Architecture findings

- **One rule stated twice (FC-015).** After the guard moves into `FieldDeclarationSplitTransform.Splits`, the RH7101 fix's `TryGetFixableFieldDeclaration` still holds its own documentation-comment condition (`GetLeadingTrivia().Any(IsDocumentationCommentTrivia)`), with a summary saying the formatter splits such a declaration. FMT-PIPE-031's `Why:` says the fix asks the formatter "instead of keeping a copy of the split's conditions". The components are the RH7101 code fix and the split transform, and they share the decision "is this declaration split". Today the two agree only because the copy exists. After the narrow fix the copy is redundant, and it can drift: for example, a later change to the guard's documentation-mode handling would not reach the copy. Removing it stays inside the counterpart of the component this run changes, and the RH7101 tests already change with the run.

### Follow-up candidates

- RH7103 (and the other type-formatting fixes) reformat untouched members of the type, for example expanding a one-line `/// <summary>` of a field the fix does not move (measured with `apply-fix.sh RH7103`). RH7103 lists this layout as a non-goal, so it is no spec violation. It is FC-030 in kind and worth a follow-up only if the maintainer wants SHARED-FIX-006 ("MUST NOT format … the type that contains that node") applied to the reordering fixes. I name it for the lead to decide. I do not recommend filing it without that check.

## Whole run

- Lane: bug fix holds for #1384. For #963 it holds under reading A, because FMT-STRUCT-024 and #913 Q3 decide every member that is copied. Under reading B it holds only if the maintainer's answer covers the first-declarator slot.
- SHARED-DIAG-010 / FC-013: no conflict. RH7101 is not a layout rule.
- P11: not involved. No analyzer is asked to follow the formatter.
