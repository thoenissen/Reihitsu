Role: skeptic
Commit: 085fcad
Verdict: #1595: proceed (the issue's inputs need correcting); #1593: proceed narrower and different (the issue's example is no defect); #1616: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All three issues are `agent-filed`, so their goals were treated as proposals. All fixtures below ran through `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF, same result in both unless noted) or `scripts/format.sh`, in a scratch directory outside the repository, after `scripts/build.sh` at 085fcad. No preprocessor symbol is defined in the runner, so `#if DEBUG` code is disabled text.

## thoenissen/Reihitsu#1595

### Premise

Real, but **the issue's two inputs do not reproduce as written**. In both, the fields are `private` and the constant is `public`. Those are different accessibility groups, so RH7102 does not report `K` (RH7102-002). `apply-fix.sh RH7102` gives `no-diagnostic` for both inputs. A test written from them that asserts "no code fix offered" would pass on `main` without testing anything (FC-033/FC-034).

With `private const int K = 1;` both reproduce. The fix is offered, and the excluded code moves with `K` in front of `_x`/`_i`:

```csharp
// input (struct S)                       // fix result
private int _x;                           #if DEBUG
#if DEBUG                                     private int _y;
    private int _y;                       #endif
#endif                                        private const int K = 1;
private const int K = 1;                      private int _x;
```

In a build with `DEBUG`, `_y` now comes before `_x`, which changes the struct's layout. In the class input, `Log("c")` now runs before `Log("i")`. Both changes are forbidden by SHARED-FIX-003 (*Build configurations*, *Run time*), which applies to every fix. The behavior SHARED-ORDER-007 describes for excluded code in the moved declaration's own leading trivia is not applied to RH7102.

Who reaches it: the RH7102 code fix, single fix and Fix All (`TypeMemberOrderingCodeFixProviderBase.RegisterCodeFixesAsync` → `IsMoveSafe`). RH7102's provider does not override `IsMoveSafe`, so it only gets the base check `OrderingDeclarationUtilities.MoveRangeContainsDirectives`. That check lets a balanced `#if` pair travel with the moved member (SHARED-ORDER-006). RH7103 and RH7109 also call `OrderingDeclarationUtilities.ChangesMemberOrder`, at `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103…CodeFixProvider.cs:77` and `RH7109…:77`.

Spec: SHARED-FIX-003 already requires the fix to be withheld, so a fix that withholds it changes no visible output that is in question. **A spec clarification is advisable, but it is internal.** RH7102's `Uses` lists neither SHARED-FIX-003 nor SHARED-ORDER-007. RH7102-006 names only SHARED-ORDER-003/004/005. SHARED-ORDER-007 ends with "RH7103 and RH7109 apply both guards". Its first sentence ("a fix that can move fields … with a runtime initializer") can be read as excluding RH7102, which only moves constants. The spec author would add RH7102 there and in RH7102-006, so the new tests have a requirement to cite.

### Mechanism and candidates

Mechanism: the RH7102 fix moves the constant together with its leading trivia, and that trivia can hold code an `#if` excludes. The fix never asks the shared member-order guard, so the excluded code's members cross the target and everything between the target and the constant, with no check of initializer order or layout. Active code can never fail, because a `const` has no runtime initializer and takes no place in the layout.

Membership is decidable. An input fails when excluded code in the moved constant's leading trivia declares either:

- a member with a runtime initializer, while a crossed member of the same context (static or instance, active or excluded) has one; or
- an instance layout field (a field, a field-like event, or an auto-property or `field`-keyword property), while the type's field order sets its layout and the move crosses an instance layout field.

Dispatch: `HasInitializer`, `IsStatic`, and `IsLayoutField` in `OrderingDeclarationUtilities` decide over `FieldDeclarationSyntax`, `EventFieldDeclarationSyntax`, and `PropertyDeclarationSyntax`. `FieldOrderSetsLayout` decides over `TypeKind.Struct`/`Class` plus `StructLayoutAttribute`.

### Breaking inputs

Classes: FC-016 (a sibling of RH7103/RH7109 that lacks their guard), FC-020 (`#if`/`#elif`/`#if false` in the gap between members), FC-003 (crossed member with and without an initializer), FC-033/FC-034 (the issue's inputs are not reported).

Matrix, with `private const int K = 1;` as the moved member and the target the first earlier `private` non-const field:

| # | Where the excluded code is | What it declares | Type / crossed | Result | Correct? |
|---|---|---|---|---|---|
| a2 | `K`'s leading trivia | instance field | struct, crosses `_x` | fixed | **fails** (layout) |
| o | `K`'s leading trivia | instance field | `record struct` | fixed | **fails** |
| i | `K`'s leading trivia | instance field | class with `[StructLayout(LayoutKind.Sequential)]` | fixed | **fails** |
| g | `K`'s leading trivia | auto-property `{ get; set; }` | struct | fixed | **fails** |
| h | `K`'s leading trivia | instance field with `[FieldOffset]` | struct with `LayoutKind.Explicit` | fixed | holds |
| f | `K`'s leading trivia | static field | struct | fixed | holds |
| c2 | `K`'s leading trivia | instance field without initializer | class, crosses `_i` without initializer | fixed | holds (the issue's counterpart) |
| b2 | `K`'s leading trivia | instance field with initializer | class, crosses `_i = Log("i")` | fixed | **fails** (initializer order) |
| d | `K`'s leading trivia | static field with initializer | crosses static `_i = Log("i")` | fixed | **fails** |
| e | `K`'s leading trivia | static field with initializer | crosses only an instance initializer | fixed | holds |
| p | `K`'s leading trivia | instance field with initializer | crosses `_i` without initializer | fixed | holds |
| q | `K`'s leading trivia | property `{ get; } = Log("p")` | crosses `_i = Log("i")` | fixed | **fails** |
| n | `K`'s leading trivia | field-like event with initializer | crosses `_i = Log("i")` | fixed | **fails** |
| r | `K`'s leading trivia | instance field with initializer | crosses `_i` (none) and a `public` property with an initializer between target and `K` | fixed | **fails** |
| l | `K`'s leading trivia, `#if false` | instance field with initializer | crosses `_i = Log("i")` | fixed | **fails** |
| m | `K`'s leading trivia, `#if DEBUG … #elif TRACE … #endif` | two initialized fields | crosses `_i = Log("i")` | fixed | **fails** |
| k | crossed span (between target and `K`) | instance field | struct | fixed | holds (crossed code keeps its order) |
| s | `K`'s leading trivia | static field and a const | struct, crosses an auto-property | fixed | holds |

The test implementer should cover every **fails** row, plus h, f, c2, e, p, k as counterparts. Add a Fix All scenario: the provider returns the batch fixer, so `BatchCodeFixTestsBase` applies. Run every case under LF and CRLF.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7102 analyzer | must stay (the reporting is right; see the follow-up for regions) |
| RH7102 code fix (`RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider`) | must change |
| `TypeMemberOrderingCodeFixProviderBase` | lead's choice (see Architecture findings) |
| `OrderingDeclarationUtilities.ChangesMemberOrder` (Core) | must stay for this issue; it already handles excluded code in the leading trivia (see #1593 for its own gap) |
| RH7103 and RH7109 fixes | must stay |
| Fix All | involved: same guard through `RegisterCodeFixesAsync`, so it needs a test |
| Spec | clarification (internal): RH7102 `Uses` and RH7102-006 name SHARED-FIX-003 and SHARED-ORDER-007; the last sentence of SHARED-ORDER-007 adds RH7102 |
| Rule page `RH7102.md` | not involved (`general-notes.md` already covers a diagnostic without a fix) |
| Formatter | not involved |

### Simpler option

The existing helper: `OrderingDeclarationUtilities.ChangesMemberOrder(typeDeclaration, typeSymbol, member, target)` already answers the question, including excluded code in the leading trivia (SHARED-ORDER-007 examples). Nothing new has to decide it.

### Output questions

_None._ Every changed output is a fix withheld where SHARED-FIX-003 requires it. The rows that hold keep today's output.

### Internal choices

_None_ beyond where the guard call lives (Architecture findings).

### Issue goals

- **Inputs.** Change them to the same accessibility group (`private const int K`, or `public` fields). As written, both tests would pass vacuously. Visible output: none.
- **"Done when".** Widen it from two inputs to the failing rows above (layout: struct, record struct, Sequential class, auto-property; initializers: instance, static, property, event, `#if false`, `#elif`, and a crossed initializer that is not the target), with c2/h/f/e/p/k as counterparts. Visible output: none beyond what SHARED-FIX-003 requires.
- **"RH7102 has no member-order guard, while RH7103 and RH7109 ask the shared owner."** Accurate.

### Architecture findings

Borderline, reported for the lead to decide. `TypeMemberOrderingCodeFixProviderBase` moves a member together with its leading trivia for all three derived fixes. The member-order safety check that this move needs is opt-in per derived class, and RH7102 never opted in. SHARED-ORDER-007 repeats the split: it lists the rules that apply the guard instead of stating it for every member-moving fix. This is not one of the README's three structural causes in the strict sense (no second owner of the decision). It is the reason a sibling went without the guard (FC-016).

### Follow-up candidates

- **RH7102 analyzer ignores regions.** RH7102-001 ("compares the two fields when both lie outside every region, or both lie in the same top-level region") and RH7102-003 limit the comparison to one top-level region. The analyzer (`RH7102ConstantsMustAppearBeforeFieldsAnalyzer.OnTypeDeclaration`) never looks at regions. A `private const` in `#region Constants` after a `private int` in `#region Fields` is reported (`apply-fix.sh`: `no-fix-offered`, so the diagnostic exists). This is a bug against an unchanged requirement, it is outside this run's issues, and it changes a visible diagnostic. The existing test `NoCodeFixWhenDirectivesAreInLeadingTrivia` (field outside every region, constant in a region) asserts only that no action is offered, which hides it.

## thoenissen/Reihitsu#1593

### Premise

The issue's example is **not a defect**, and its stated mechanism is wrong for its own shape. The `Fields` block holds both the disabled `X`/`Y` fragment and the active `Y`, and the `IDisposable` block holds no initializer. The permutation therefore crosses no initializer, and the fix that is offered is correct (`apply-fix.sh RH7309`: fixed. In a `DEBUG` build, `X` and `Y` keep their order and nothing initialized is crossed).

The claim that the split member "does not parse as a member there" is false for this shape. The disabled run `private static readonly int X = Log("x");\n private static readonly int Y =` parses as members, and `X` is counted: the same shape with `private static readonly int A = Log("a");` added to `IDisposable`, and with the active `Y` given no initializer, is `no-fix-offered`.

A real defect of the same family exists elsewhere, in the shared guard. Who reaches it: the RH7309 fix (`TryCreateReorder` → `RegionBlockLayout.ChangesMemberOrder`), and the RH7103 and RH7109 fixes, all through `OrderingDeclarationUtilities.ChangesMemberOrder` → `GetMembersOfDisabledText` (`src/Reihitsu.Core/OrderingDeclarationUtilities.cs:519`). RH7102 joins them once #1595 is fixed.

That method parses only disabled-text runs whose start lies outside every active member's span, each run on its own as a member list. `HasInitializer`/`IsStatic`/`IsLayoutField` read only the active syntax of each member.

Failing inputs, all reached by the code fixes, all `fixed` today where SHARED-FIX-003 (*Build configurations*) requires withholding:

```csharp
// RH7309: IDisposable block holds `private static readonly int A = Log("a");`, Fields block holds:
private static readonly int B
#if DEBUG
    = Log("b")
#endif
    ;
```

The fix moves `Fields` in front, and a `DEBUG` build then runs `Log("b")` before `Log("a")`.

Spec: SHARED-FIX-003 decides all of these: a run-time difference that only a `DEBUG` build shows counts. RH7309-006 and SHARED-ORDER-007 point to it. No visible output is in question for the failing members. A one-sentence clarification in SHARED-ORDER-007 would be useful: a member that an `#if` splits counts with what each configuration gives it, such as an initializer, `static`, or an extra member. That is internal wording.

### Mechanism and candidates

Mechanism: the member-order guard learns the members of the other configurations only from disabled-text runs that start outside every active member's span, each run parsed alone as a member list, and it reads each active member's initializer, `static`, and layout field status from active syntax only. So it misses:

- anything an `#if` adds to or changes in a member inside that member's span (an initializer, a `static` modifier, a further member); and
- a disabled run that does not parse as a member on its own, such as `= Log("b");` behind an accessor list.

Dispatch: the active members come from `typeDeclaration.Members` (`FieldDeclarationSyntax`, `EventFieldDeclarationSyntax`, `PropertyDeclarationSyntax` through `HasInitializer`/`IsStatic`/`IsLayoutField`). The excluded members come from `DisabledTextTrivia` filtered by `member.Span.Contains`. The rank function comes from the caller: `RegionBlockLayout` segments for RH7309, the four-rank move for RH7103/RH7109.

### Breaking inputs

Classes: FC-001 (the guard inspects a smaller span than the decision: disabled text inside member spans is skipped), FC-020, FC-016 (one helper, four consumers), FC-034 (the issue's example is a cousin that holds).

Matrix (axes: what the `#if` contributes × where it sits × which side of the move × consumer):

| # | Consumer | Split contributes | Location of disabled text | Side | Result | Correct? |
|---|---|---|---|---|---|---|
| 1593/a | RH7309 | head fragment `X = …; Y =` (issue's example) | leading trivia of active `Y` | moved, nothing crossed | fixed | holds |
| 1593d/a | RH7309 | head fragment, active `Y` without initializer | leading trivia | moved across static `A = …` | no fix | holds (`X` counted) |
| 1593/b | RH7309 | the initializer `= Log("b")` | inside the active member's span | moved block | fixed | **fails** |
| 1593/c | RH7309 | a whole further static member `Z = Log("z")` | inside the active member's span (inside its initializer) | moved block | fixed | **fails** |
| 1593/e | RH7309 | a property initializer `= Log("b");` behind `{ get; }` | between members, does not parse alone | moved block | fixed | **fails** |
| 1593/g | RH7309 | `static` under `#if !DEBUG` (struct; instance in `DEBUG`) | inside the active member's span | moved block, crosses instance `_a` | fixed | **fails** (layout) |
| 1593/f | RH7309 | `static` under `#if DEBUG` (active is instance) | inside the span | moved block | no fix | holds |
| 1593d/b | RH7309 | the initializer | inside the span of a member in the gap between blocks (stays) | crossed by `A` | fixed | **fails** |
| 1593b/a | RH7109 | the initializer of the moved `readonly` member | inside its span | moved across `_a = Log("a")` | fixed | **fails** |
| 1593b/b | RH7109 | the initializer of the crossed member | inside its span | crossed | fixed | **fails** |
| 1593b/c | RH7109 | property initializer behind the accessor list of a crossed property | between members, does not parse alone | crossed | fixed | **fails** |
| 1593c/a | RH7103 | `static` under `#if !DEBUG` of the moved member (struct) | inside the span | moved across instance `_a` | fixed | **fails** (layout) |

Not constructed: a split `const` modifier. That would make the active member count as initialized when a configuration has none, which only withholds more, and over-withholding is safe. The test implementer should cover each **fails** row on its consumer, plus 1593/a, 1593d/a, and 1593/f as counterparts. Unit tests in `src/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs` reach the shared method directly. Next to them, the existing analyzer tests such as `NoCodeFixWhenAnInitializerInDisabledTextWouldChangeOrder` (`src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs:1655`) cover the consumer path.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `OrderingDeclarationUtilities.ChangesMemberOrder` / `GetMembersOfDisabledText` / the active-syntax predicates (Core) | must change |
| `RegionBlockLayout` (named by the issue) | not involved: it only supplies the rank function |
| RH7309, RH7103, RH7109 fixes | must stay (consumers); covered by tests |
| RH7102 fix | consumer once #1595 is fixed |
| Analyzers | not involved |
| Spec | optional internal clarification in SHARED-ORDER-007 (split members) |
| Rule pages | not involved |
| Formatter | not involved |

### Simpler option

_None_ for the failing members: they live in the one shared helper, which is already the narrowest place.

### Output questions

_None_, provided the issue's own example keeps its fix (see Internal choices).

### Internal choices

- **Precision of the guard for a member an `#if` splits.**
  - **A**: count what each configuration gives the member. Unknown or unparsed excluded code counts at its position, never less than what it could contribute. With A, the issue's example keeps today's fix (correct output, nothing crossed), and every failing row is withheld.
  - **B** (the issue's alternative): "the fix is withheld for a block holding such a split". B withholds the fix for the issue's own example, which is safe today. That changes a visible output that no requirement demands, so B would be an output question.

  Recommend A, which changes no visible output beyond what SHARED-FIX-003 requires.

### Issue goals

- **"Affected: `RegionBlockLayout`".** Wrong. The defect is in `OrderingDeclarationUtilities` (Core) and affects RH7103 and RH7109 as well as RH7309. Visible output: none beyond required withholding.
- **"Done when: a test with the split member keeps the diagnostic and offers no fix".** With the issue's example, such a test would lock in a visible change that no requirement demands, because its fix is correct. Replace it with the failing rows above, and keep the issue's example as a counterpart that is still fixed.
- **The statement that the split member "does not parse as a member".** False for the issue's shape. The real cases are disabled text inside an active member's span, and trailing fragments that do not parse alone.

### Architecture findings

_None._ The decision has one owner, `OrderingDeclarationUtilities`, and every consumer reaches it. That is why one fix covers all of them.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1616

### Premise

Real, reached by the RH2001 code fix (single fix and Fix All). The reorder happens only for a **get-only** property without `readonly`. `TryCreateFieldDeclaration` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:198-213`) then calls `ModifierOrderingUtilities.OrderModifiersForRh7105(modifiers.Add(readonly))`, which permutes the tokens together with their own trivia, and it writes `node.GetLeadingTrivia()` again onto the new first token. A `{ get; set; }` property keeps the written order: `static private int Value { get; set; }` → `static private int _value;` (fixture g). The issue title's "`static private` property" is narrower in fact: the defect needs a get-only property.

`scripts/apply-fix.sh RH2001` results (LF and CRLF identical):

| Fixture | Input (in `class C`) | Today's result |
|---|---|---|
| a | `static private int Value { get; }` | `private␣␣␣␣␣static readonly int _value;` |
| b | `static /* a */ private int Value { get; }` | `private␣␣␣␣␣static /* a */ readonly int _value;` |
| f | `static // a` + newline + `private int Value { get; }` | `private␣␣␣␣␣static // a` + newline + `readonly int _value;` |
| i | `static new private int Value { get; }` | `private␣␣␣␣␣static new readonly int _value;` |
| m | `new /* a */ static private int Value { get; }` | `private static␣␣␣␣␣new /* a */ readonly int _value;` |
| k | `static private int Value { get; } // t` | `private␣␣␣␣␣static readonly int _value; // t` |
| l | `static private string Value { get; } = "v";` | `private␣␣␣␣␣static readonly string _value = "v";` |
| j | a blank line above `static private int Value { get; }` | `private` + newline + `static readonly int _value;` (line break inside the modifiers) |
| c | `// c` above it | no fix offered |
| d | `/// <summary>…` above it | no fix offered |
| e | `#pragma warning disable CS0414` above it | no fix offered |
| n | `#region Properties` above it | no fix offered |
| o | `private /* a */ static int Value { get; }` (already ordered) | `private /* a */ static readonly int _value;` |
| h | `private static int Value { get; }` | `private static readonly int _value;` |
| p | `#if DEBUG internal #endif` between `static` and `private` | no fix offered |

The issue says "the formatter leaves the gap". That is wrong for whole-document formatting: `scripts/format.sh` on `private     static readonly int _value;` gives `private static readonly int _value;`. It is the fix's own node-scoped result that keeps the gap. So the gap violates SHARED-FIX-007, because formatting the result changes it. For j, `format.sh` keeps the line break, so SHARED-FIX-007 alone does not decide j.

Which spec decides what:

- **a, i, k, l, and the gap part of f and m.** SHARED-FIX-007 demands one space. RH2001-004 covers a, i, k, l.
- **c, d, e, n.** RH2001-004 covers a property with "no comment or directive inside its declaration other than before its first token". It keeps the documentation comment, and RH2001-007 says a comment in front of the first token does not withhold the fix. So the fix MUST be offered, with the comment or directive once in front of the field. Today's withholding is a bug against RH2001-004. The duplication that the `KeepsEveryCommentAndDirective` guard catches is how it fails.
- **b, f, m (a comment between two modifiers that the reorder moves across).** No spec decides these. RH2001-007 says such a comment "is not covered by this requirement". SHARED-FIX-003 *Comments and directives in place* does not decide "a comment between two tokens of one line". SHARED-MODORDER-003 decides exactly this case, but only for RH7105/RH7106/RH2005, and RH2001 does not use it. This makes them an output question.
- **j (blank line above).** Not decided either (see Output questions).
- **RH2001-004 does not mention that the fix reorders modifiers at all.** The issue's "Done when" asks the spec to state the outcome. This is a spec clarification for the spec author, after the maintainer's answers.

### Mechanism and candidates

Mechanism: when the fix adds `readonly`, it rebuilds the modifier list in RH7105 order by permuting the original tokens with their own leading and trailing trivia, and then puts the declaration's leading trivia on the new first token as well. So whenever the reorder moves the first modifier, the declaration's leading trivia (indentation, a blank line's end of line, comments, directives, a documentation comment) lands inside the list a second time. Whenever it moves a token across a comment, the comment travels with the token it is attached to.

Membership is decidable:

- Get-only property, no `readonly`, and RH7105 order changes the first modifier → gap, mid-list line break, or duplicated leading comment (the duplicate is then withheld).
- Get-only property, no `readonly`, and the reorder moves a modifier across trivia that holds a comment → the comment relocates.
- Otherwise (already ordered, or not get-only) → holds.

Dispatch: `IsGetOnlyProperty`, `OrderModifiersForRh7105` (`GetRh7105Rank` over modifier `SyntaxKind`s), and `KeepsEveryCommentAndDirective`.

### Breaking inputs

Classes: FC-004 and FC-021 (comment between modifiers, comment and leading trivia of the moved first modifier), FC-019 (the gap tested without and with a comment and a directive), FC-020 (`#pragma`, `#region`, `#if` in front of or between the modifiers), FC-015 (a private copy of the modifier reorder, see Architecture findings), FC-017 (LF and CRLF), FC-007 (check that the fix leaves no spacing diagnostic in the edited span).

Matrix (axes: what precedes the first modifier × what lies between moved modifiers × which modifier moves):

- Leading trivia (`static` is moved):
  - indentation only (a) — fails
  - blank line (j) — fails
  - `//` comment (c), doc comment (d), `#pragma` (e), `#region` (n) — fail by withholding
- Between modifiers: block comment (b), end-of-line comment (f), block comment with a non-first modifier moved (m) — output question
- Three modifiers (i) — fails
- Trailing comment (k) and initializer (l) — fail
- Hold: already ordered (h, o), `{ get; set; }` (g), `#if` between modifiers (p; withheld, outside RH2001-004's coverage)

The test implementer covers a, i, k, l, j, c, d, e, n plus the counterparts g, h, o, p, all under LF and CRLF. b, f, and m wait for the answer.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 code fix (`TryCreateFieldDeclaration`) | must change |
| `ModifierOrderingUtilities` / `ModifierOrderingCodeFixProviderBase` (`NormalizeModifierTrivia`, `ReorderingWouldDropCommentOrDirective`) | lead's choice (Architecture findings) |
| RH2001 analyzer | not involved |
| RH7105 and RH7106 fixes | must stay |
| Fix All | involved (same `TryCreateFieldDeclaration`) |
| Spec RH2001 | clarification needed: state the reorder and its trivia handling, depending on the answers |
| Rule page `RH2001.md` | not involved |
| Formatter | not involved |

### Simpler option

Not reorder at all: insert `readonly` without moving any token, giving `static private readonly int _value;` and leaving the order to RH7105, which already reports that property today. This removes the whole trivia class. It changes the visible output for every unordered get-only input compared with what the fix intends today, so it would be an output question. I do not recommend it: today's evident intent and the issue both produce `private static readonly`, and SHARED-MODORDER-002/003 already describe how to reorder safely.

### Output questions

**1 · RH2001 · Private auto-implemented properties should not be used: comment between two modifiers the fix reorders**

Input:
```csharp
class C
{
    static /* a */ private int Value { get; }
}
```

Before (`main`):
```csharp
class C
{
    private     static /* a */ readonly int _value;
}
```

After, A (recommended): no fix offered; the diagnostic stays.

After, B:
```csharp
class C
{
    private static /* a */ readonly int _value;
}
```

After, C:
```csharp
class C
{
    private /* a */ static readonly int _value;
}
```

Why A: it matches how RH7105 treats a comment next to a modifier it reorders, and the comment's place is the author's decision. A also covers `static // a` on its own line and `new /* a */ static private`. An already-ordered `private /* a */ static int Value { get; }` keeps today's fix (`private /* a */ static readonly int _value;`).

**2 · RH2001 · Private auto-implemented properties should not be used: blank line above a property whose modifiers the fix reorders**

Input:
```csharp
class C
{
    private int _x;

    static private int Value { get; }
}
```

Before (`main`):
```csharp
class C
{
    private int _x;

    private
    static readonly int _value;
}
```

After, A (recommended):
```csharp
class C
{
    private int _x;

    private static readonly int _value;
}
```

After, B: unchanged from `main` (line break between `private` and `static`).

Why A: the line break exists only because the fix carried the blank line's trivia into the modifier list. The author wrote the modifiers on one line.

### Internal choices

- **Already-ordered modifiers with a comment between them (fixture o).** If the answer to question 1 is A, a check that withholds whenever a comment follows a modifier would also withhold o, which is fixed safely today. That would be a visible change no requirement demands. The withholding should apply only where the reorder moves a token across the comment. Recommend scoping it that way, which keeps o's output unchanged.

### Issue goals

- **"The formatter leaves the gap".** Factually wrong (whole-document formatting removes it). The defect is the fix's own result, against SHARED-FIX-007. No visible effect on the goal.
- **"A comment between the modifiers stays where the requirement allows (or the fix is withheld)".** No requirement decides it. Output question 1.
- **"The third input is fixed with `// c` once".** Demanded by RH2001-004. Extend it to a doc comment, `#pragma`, and `#region` above the property (d, e, n), which are covered by the same requirement and fail the same way. Visible: fixes offered where RH2001-004 already requires them.
- **"Tests in both line-ending flavors".** Keep.
- **"The RH2001 spec states the outcome".** Keep: a spec author clarification after the answers.
- **Title "`static private` property".** The defect needs a get-only property (fixture g holds). Narrow the wording in the PR. Visible output: none.

### Architecture findings

**The modifier-reorder rewrite has two owners.**

- `ModifierOrderingCodeFixProviderBase` (RH7105, RH7106) reorders modifiers through `GetUpdatedModifiers`, then applies SHARED-MODORDER-002 in `NormalizeModifierTrivia` (the leading trivia stays in front of the new first modifier, and each modifier is followed by one space) and SHARED-MODORDER-003 in `ReorderingWouldDropCommentOrDirective`. Both are `private`.
- The RH2001 fix reorders the same list with the same `ModifierOrderingUtilities.OrderModifiersForRh7105` but has its own trivia handling: `WithLeadingTrivia(node.GetLeadingTrivia())` plus the `KeepsEveryCommentAndDirective` comparison. Fixtures a, j, c, b show the two disagreeing.

A narrow repair inside `TryCreateFieldDeclaration` would add a second copy of SHARED-MODORDER-002's trivia rule. Per `README.md`, *Architecture findings*, that is the case where the structure is changed in this run, within the components the run touches and their counterpart. The lead decides.

### Follow-up candidates

_None._

## Whole run

- `scripts/build.sh` (Release) at 085fcad: 0 errors.
- Fixtures and outputs were kept in the session scratchpad only, not in the repository.
- **Order dependency.** #1595 makes RH7102 a fourth consumer of the guard that #1593 changes, so the #1593 failing members also apply to RH7102 once both are fixed. A test for an RH7102 constant whose leading trivia holds a member split by `#if` would cover the join.
