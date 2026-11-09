Role: skeptic
Commit: 8db7e68
Verdict: #1666: proceed; #1667: proceed, one more member to refuse (the spec diff exempts it); #1665: proceed narrower (per-consumer goal narrowed)

The prompt carried no intended fix or hoped-for verdict. It named Decision #3 in `00-lead.md` as something to challenge, and that is done below. The issues' "Affected" sections and proposed checks were treated as proposals.

All measurements ran `scripts/apply-fix.sh <ID> <dir>` at `8db7e68`, LF and CRLF alike. `8db7e68` is `95aa3e6` plus `sdd/` files, so no production code has changed. The fixtures lived in the session scratchpad and are deleted. Every input that matters is quoted below. "fixed" means the fix is offered on `main` and applied.

## thoenissen/Reihitsu#1666

### Premise

The premise is real. The caller is the RH2001 code fix in an IDE or in a Fix All over the document.

- `extern` in `#if DEBUG` … `#endif` above `private int Value { get; }` in a class: **fixed**. The output is `#if DEBUG extern #endif private readonly int _value;`, which gives CS0106 in a DEBUG build.
- `readonly` in the same group in a struct: **fixed**. A DEBUG build then has `readonly` twice.
- The mechanism in the issue follows from the code. `HasConditionalDirectiveBetweenModifiers` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:246`) reads `node.Modifiers.Skip(1)` and the type's leading trivia. The first modifier's leading trivia is never read.

### Mechanism and candidates

Mechanism: the fix keeps the trivia in front of the first modifier verbatim in front of the field, but never reads the directives in that trivia. An excluded branch there becomes part of the field's declaration in the builds that include it. That holds for any branch that is not a sequence of whole members.

The candidates come from the property's token positions where trivia can hold a conditional directive:
- in front of the first modifier: not read today, the defect;
- in front of another modifier, or in front of the type: refused (RH2001-009);
- inside the accessor list or the initializer: RH2001-004 coverage, `KeepsEveryCommentAndDirective`;
- behind the last token: trivia of the next token.

### Breaking inputs

Failure classes: FC-001 (the guard reads a smaller span than the decision it protects), FC-020, FC-016.

Class matrix. The axes are: what the excluded branch holds; which directive of the group stands in the first modifier's trivia; whether the analyzed build defines the symbol; nesting; whether the property has a modifier. All measured on `main`:

| Member | Input (shape) | `main` | RH2001-010 |
|---|---|---|---|
| r1 | class, `#if DEBUG extern #endif` above `private int Value { get; }` | fixed (wrong) | withheld |
| r2 | struct, `#if DEBUG readonly #endif` | fixed (wrong) | withheld |
| r7 | `#if DEBUG static #endif` | fixed | withheld |
| r5 | `#if DEBUG [System.Obsolete] #endif` | fixed | withheld (output question 1) |
| r6 | `#if DEBUG extern #else private int _trace; #endif`, DEBUG undefined (excluded branch in front of `_trace`, only `#endif` in front of `private`) | fixed (wrong) | withheld; needs every branch of the group read, not only the text behind a directive in the first modifier's trivia |
| r8 | `#define DEBUG`, `#if DEBUG private int _trace; #else extern #endif` (excluded `#else` branch in the first modifier's trivia) | fixed (wrong) | withheld |
| r3 | `#if DEBUG private int _trace; #endif` (whole member) | fixed | fixed (counterpart) |
| r9 | whole-member groups in front of and behind the property | fixed | fixed |
| r4 | the excluded branch holds the property `Value` itself (`#if DEBUG private int Value { get; set; } #else private int Value { get; } #endif`) | no fix (the existing excluded-name rename check) | no fix; must stay, not via RH2001-010 |

Members not measured but decided by the spec: a nested group (refused by *No nesting*), and a property without a modifier (RH2001-009, refused whatever the branch holds).

The test implementer must cover r1, r2 (LF and CRLF), r6, r8, r5, r3, and one nested-group input. r4 is a regression guard for the rename check.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 analyzer | must stay: the diagnostic is still reported |
| RH2001 code fix (`TryCreateFieldDeclaration`) | must change |
| RH2001 Fix All (`BatchFixer`) | must stay: it runs the same `TryCreateFieldDeclaration` (line 438) |
| `Reihitsu.Core` (`OrderingDeclarationUtilities`) | must change if RH2001 reuses its whole-members reading (see *Architecture findings*) |
| Formatter | not involved |
| `documentation/rules/RH2001.md` | not involved: `general-notes.md` covers a withheld fix |
| Tests | must change |

### Refuse option

The refusal option is to withhold the fix for every conditional directive in the first modifier's trivia, the same blanket rule RH2001-009 applies to a property without a modifier.
- Cost: about one line (drop `.Skip(1)`), and no parse.
- Price: the fix is also withheld for r3 and r9, a whole-member `#if DEBUG` group directly above a property. `main` fixes those and RH2001-009's last example shows them fixed, so the blanket rule is a visible change.

The spec's option uses one parse of each excluded branch plus the no-nesting check, which `OrderingDeclarationUtilities` already implements (`WrapsWholeUnits`, `ReadsAsWholeUnits`). Its cost depends on whether that reading becomes one shared helper:
- With one Core helper that Decision #3 needs anyway, RH2001 costs a few lines.
- With a private copy in the fix, it is FC-015.

Whether the input class is rare: a modifier or an attribute list inside an `#if` above a property is rare, while a whole-member `#if DEBUG` group above a property is ordinary code. So the parse is justified by the common counterpart, not by the rare class. I agree with the spec.

Declaring the shape unsupported is not an option. The formatter has no special case for a conditional group in front of a member, so the declaration would delete nothing. I agree with the spec author.

### Output questions

The question for r5 (output question 1 of `01-spec-author.md`): recommend withholding. There is a stronger reason than the change doc gives. The fix already refuses every property with an active attribute list (`IsSupportedPropertyShape`, `node.AttributeLists.Count > 0`, line 287), so withholding it for an attribute in an excluded branch keeps one rule for attributes. Nothing else.

### Internal choices

- The RH2001-010 reading should be the Core helper that also serves SHARED-ORDER-008's first-token reading (Decision #3), not a copy. Recommended. See *Architecture findings*.
- The spec keeps an asymmetry. A property without a modifier is refused for any group (RH2001-009), while one with a modifier is parsed (RH2001-010). Widening RH2001-009 to the parse would offer fixes that `main` withholds: a visible change outside the issue. Recommend leaving it.

### Issue goals

- Drop "The refusal must not need an excluded branch to be parsed more than the code already does". It contradicts the issue's own counterpart goal: keeping the fix for a whole-member group needs a parse of the excluded branch, and the fix parses none today. Dropping the constraint changes no visible output. Keeping it would force the blanket refusal, which does change output (r3).
- Keep "no fix for `extern` and `readonly`, LF and CRLF" and "counterpart keeps the fix". Add r6 and r8: the excluded branch need not stand behind the directive in the first modifier's trivia.

## thoenissen/Reihitsu#1667

### Premise

The premise is real. The caller is the RH7109 code fix, and through `ChangesMemberOrder` also RH7102, RH7103 and RH7309.

- m01, the issue's input (partial class, `#if NET [StructLayout(Sequential)] #endif` on the other declaration): **fixed** (wrong).
- m02 (partial struct, `#if !X [StructLayout(LayoutKind.Auto)] #endif` on the other declaration): **fixed** (wrong).
- m03 (the same attribute on a single, non-partial declaration): **no fix offered**, as the issue says.
- The mechanism follows from the code. `DeclarationHoldsConditionalDirective(typeDeclaration)` (`src/Reihitsu.Core/OrderingDeclarationUtilities.cs:723`) reads only the reordered declaration, while `FieldOrderSetsLayout(typeSymbol)` reads every declaration's attributes.

### Mechanism and candidates

Mechanism: the conditional-directive half of the layout decision reads one syntax declaration of the type, while the layout itself comes from every declaration, including ones written only in excluded code.

The candidates come from where the layout-deciding text can stand (the axes of the matrix below).

### Breaking inputs

Failure classes: FC-024 (partial types), FC-016, FC-015, FC-020.

Matrix. Axis A is where the layout-deciding conditional text stands. Axis B is its direction: present only in another build (`#if NET [Sequential]`) or absent there (`#if !X [Auto]`); both are measured above.

| Member | Shape | `main` | Spec diff |
|---|---|---|---|
| A1 | reordered declaration's own header (m03) | no fix | no fix |
| A2 | header of another active declaration, same document (m01, m02, m09 `[Serializable]`) | fixed | no fix (m09 is output question 2) |
| A3 | header of another active declaration, **another document** | not measurable with `apply-fix.sh` (one document per fixture) | no fix; **needs a two-document test** (FC-024) |
| A4 | another declaration's first-token group whose excluded branch stands in front of a different declaration (Decision #3 shape, on the other part) | not measured | no fix, provided the shared reading of Decision #3 is used for every declaration |
| **A5** | **a whole `partial` declaration of the type, with the attribute, inside an excluded branch in front of the reordered declaration (m06)** | **fixed (wrong)** | **fixed (wrong): the spec diff exempts it explicitly** |
| **A6** | **a whole `partial` declaration inside excluded code elsewhere: behind the type (m07), in a group behind the spans that closes the body (m10), in another document, or in another declaration's body** | **fixed (wrong)** | **fixed (wrong): the spec says these are not read** |

m06, without `NET` defined:

```csharp
using System.Runtime.InteropServices;

#if NET
[StructLayout(LayoutKind.Sequential)]
public partial class C
{
}
#endif
public partial class C
{
    private int _a;
    private readonly int _c;
}
```

On `main`, RH7109 moves `_c` in front of `_a`. In a `NET` build, `C` has sequential layout, so the move changes it. The spec diff keeps the sentence "A group in the leading trivia of the declaration's first token whose excluded branches all read as whole members, such as a whole type declared in front of the reordered one, does not make the type count". It now stands next to the new statement that every declaration of a partial type counts. That sentence contradicts SHARED-FIX-009, which says a whole member of an excluded branch "counts as present at its position, in every build", and it contradicts SHARED-FIX-003, *Build configurations*. A5 is cheap to refuse: the branch is already parsed by `ReadsAsWholeUnits`, and the check is whether a type declaration in it carries `partial`, or matches the type's name.

m07 and m10, without `NET` or `X` defined:

```csharp
using System.Runtime.InteropServices;

public partial class C
{
    private int _a;
    private readonly int _c;
}
#if NET
[StructLayout(LayoutKind.Sequential)]
public partial class C
{
}
#endif
```

```csharp
using System.Runtime.InteropServices;

public partial class C
{
    private int _a;
    private readonly int _c;
#if X
}
[StructLayout(LayoutKind.Sequential)]
public partial class C
{
#endif
    private int _z;
}
```

Both are fixed on `main` and stay fixed under the spec diff. A6 has no cheap check. It would need every excluded branch of every document that could declare a part, or a blanket rule that counts every `partial` type as layout-sensitive, which would withhold instance-field reorders on common code (partial view models, generator-backed types). See *Follow-up candidates*.

The test implementer must cover:
- m01 (LF and CRLF);
- m02;
- the plain-partial counterpart;
- a **two-document** A3 input;
- an A4 input;
- m09 if the maintainer picks withholding;
- A5 once the lead decides it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `OrderingDeclarationUtilities.DeclarationHoldsConditionalDirective` (Core) | must change (every declaration via the symbol's `DeclaringSyntaxReferences`) |
| `TypeMemberOrderingCodeFixProviderBase`, `RegionBlockLayout`, RH7309 fix | must stay: they already pass the type symbol |
| RH7102, RH7103, RH7109, RH7309 analyzers | not involved |
| Fix All (`BatchFixer`) | must stay: the same guard runs per diagnostic |
| Rule pages | not involved |
| Tests | must change |

### Refuse option

The requested change is itself the refusal: one check per declaration, through the symbol, with the existing structural test. It needs no new type.

A narrower alternative is to count only branches that hold an attribute named `StructLayout`. It is rejected: an alias defeats it, it is a second statement of `FieldOrderSetsLayout`'s attribute reading (FC-015), and it would miss the conditional `Auto` direction.

On rarity: A2 to A5 are rare, and the single-declaration form is already withheld, so the partial extension adds no new kind of refusal. Declaring the shape unsupported is not an option: the formatter is not involved.

### Output questions

Output question 2 (m09, `[Serializable]` on the other part): recommend withholding. The same input with one declaration is withheld on `main` today, so withholding it here keeps one rule for both forms.

A5 is not an output question. Withholding there is demanded by SHARED-FIX-003 and SHARED-FIX-009, which are unchanged. The spec author has to change SHARED-ORDER-008's exemption sentence so it does not cover a `partial` declaration.

### Internal choices

- Read each other declaration "on its own, by the same test" with the Decision #3 reading, so A4 holds for every declaration. Recommended.

### Issue goals

- Keep all of them. Add the two-document test, because "every declaration of the type symbol" otherwise stays untested on its most likely shape.

## thoenissen/Reihitsu#1665

### Premise

The premise is real. The caller is the RH7109 code fix.

- m04, the issue's input: **fixed** (wrong). In a build with `X`, the fields belong to struct `D`.
- m13 (the same group in the *target's* leading trivia, so inside the crossed span): **no fix offered**. This confirms the boundary: the crossed span starts at the target's full span (`GetReorderedSpan`, `ChangesMemberOrder` line 365), and only groups in front of it are unread.
- s1 (RH7103, a static `_c` moved across instance `_a` behind the same closing group): **fixed**. That is safe in every build: no instance field is reordered.
- n4 (the closing group *behind* the spans, in a plain class): fixed, and safe. The spans stay in `C`.

### Mechanism and candidates

Mechanism: the guard reads the conditional groups inside the reordered span and in the type's header, but not those in the body between the opening brace and the reordered span. A branch there can change which type, or which kind of type, the spans belong to.

The candidates are the positions of a group relative to the spans:
- header: read;
- body in front of the spans: unread, the defect;
- target's leading trivia, or inside the spans: read;
- behind the spans: unread. Safe for membership, but see A6 (m10) for layout;
- enclosing type: harmless, the nested type's kind is fixed;
- a group straddling the spans (`#if` in front, `#endif` behind): the spec covers it.

### Breaking inputs

Failure classes: FC-001, FC-003 (boundary: a group directly in front of the target versus in the target's trivia), FC-016.

Matrix. The axes are the position of the group and what its excluded branch holds: a brace that closes the type and opens another; a brace that opens a nested struct; a modifier of an uncrossed member; whole members; whole statements in a method in front of the spans.

| Member | `main` | Spec diff |
|---|---|---|
| closing `}` plus `struct D {` in front (m04) | fixed (wrong) | no fix |
| same, static move, RH7103 (s1) | fixed | fixed |
| opens a nested `struct D {` in front, closed behind | not measured in front (measured only inside the crossed span, where it is refused) | no fix |
| `#if DEBUG static #endif` in front of an uncrossed member (m08) | fixed (safe in every build) | no fix (output question 3) |
| whole members in front (m11) | fixed | fixed |
| in the target's trivia (m13) | no fix | no fix |
| nested groups inside the moved span (n1), or around the first token (n2) | no fix | no fix |

The test implementer must cover:
- m04 for RH7109 and for RH7309, with the group in front of the first moved block;
- s1 for RH7103, fix kept;
- m11 as the counterpart;
- m08 if the maintainer picks withholding;
- an input one member in front of the target, next to the existing input in the target's trivia (FC-003).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `OrderingDeclarationUtilities.ChangesMemberOrder` (private, `Func<int,int>` overload) | must change |
| RH7102 and RH7103 fixes | must stay: their moves reorder no instance field |
| RH7109 and RH7309 fixes | behavior changes through Core; no change in the providers |
| Analyzers, rule pages, formatter | not involved |

### Refuse option

The spec chose to count such a group as making the field order layout-relevant, so only instance-field reorders are withheld. The alternative, refusing every move, costs about the same code and also withholds the safe static and const moves (s1). I agree with the spec.

The group shape is rare. A brace in an `#if` that closes a type and opens another is pathological.

The cheapest narrower option would be to count only branches that hold a brace token, which would keep m08. It is rejected as a parser-like rule over text.

Declaring the shape unsupported is not an option: there is no formatter special case to delete, and the group lies outside the node the fix formats.

### Output questions

Output question 3 (m08): recommend withholding. The class is rare, and the parse cannot tell a modifier-only branch from a brace-closing one without a token check over the failed parse. Note for the lead: m08 is safe in every build, so this is a true false refusal, not a borderline one.

### Internal choices

_None._

### Issue goals

- Narrow "A test per consumer asserts that no fix is offered for the input above". RH7102 (constants) and RH7103 (static) moves reorder no instance field and keep the fix. RH7109 and RH7309 assert no fix. RH7103 gets the s1 counterpart, fix kept. This changes no visible output beyond the spec.

## Whole run

### Decision #3: adjacent fix in `DeclarationHoldsConditionalDirective`

- The defect is confirmed. m05, the spec author's input with an excluded `#if X [StructLayout(Sequential)]` branch in front of an active `public class D {}`: **fixed** (wrong).
- It is a class, not an instance. n3, the same with `#if X [StructLayout] #elif Y class E {} #else class D {} #endif`: **fixed** (wrong).
- Cause: `GetExcludedText` returns only the disabled text that directly follows a directive in the *same* token's leading trivia. The first-token check never sees an excluded branch that stands in front of another declaration.
- Nested groups are safe on `main`. n2 (`#if A #if B [StructLayout] #endif #endif` around the first token) gets no fix, because the inner directives are structured trivia that the check iterates.
- The adjacent fix meets *Scope*: it is a defect against unchanged SHARED-ORDER-008, small, in a function the run changes anyway, testable, and puts no output in question.
- It is also the same reading RH2001-010 needs. I recommend taking it in, as the shared helper below. Tests: m05 and n3.

### Architecture findings

**One decision, two readings in `OrderingDeclarationUtilities`, a third reader coming.** "Does this conditional group wrap whole members" is decided twice, and the two readings disagree on m05 and n3:
- `WrapsWholeUnits` reads every excluded branch of the group (`GetExcludedText` over every directive of `GetRelatedDirectives`) and checks nesting;
- `DeclarationHoldsConditionalDirective` reads only the branches behind the directives in the first token's trivia, and does not check nesting.

RH2001-010 states the same test for a third consumer, the RH2001 fix. This is FC-015, one rule implemented in more than one place.

The scope is a pre-existing defect that the run newly depends on: #1667 extends the narrower reading to every partial declaration, and RH2001-010 needs the same reading. It is small and needs no new type: one Core helper that reads every branch of a group whose directive stands in a token's trivia, with no nesting.

### Follow-up candidates

- **A6: a `partial` declaration of the reordered type that only excluded code writes, anywhere except the A5 position** (m07, m10; also in another document, or inside another declaration's body). RH7109 reorders the fields of a type that has sequential layout in another build. The case violates SHARED-FIX-003 on `main` and after this run, and no cheap check refuses it. It belongs in an `agent-filed` follow-up that carries this question:

  **RH7109 · Readonly elements must appear before non-readonly elements: a partial declaration with a layout attribute exists only in excluded code**

  Input, without `NET` defined:
  ```csharp
  using System.Runtime.InteropServices;

  public partial class C
  {
      private int _a;
      private readonly int _c;
  }
  #if NET
  [StructLayout(LayoutKind.Sequential)]
  public partial class C
  {
  }
  #endif
  ```
  Before (`main`): the fix moves `_c` in front of `_a`.

  After, A (recommended): unchanged; the gap is recorded in SHARED-ORDER-008 as a decision.

  After, B: no fix offered for any reorder of instance fields in a `partial` type, which also withholds the fix on every ordinary partial class with fields.

  Why A: the shape is very rare, and B withholds a fix on code people commonly write.

  This is the lead's call to file. A records a spec gap against SHARED-FIX-003, so the follow-up must say so.
- None besides A6. A5 belongs in this run (see #1667).
