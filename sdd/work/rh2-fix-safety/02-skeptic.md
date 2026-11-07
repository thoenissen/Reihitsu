Role: skeptic
Commit: e18cc30
Verdict: #967: proceed narrower; #1286: proceed; #1287: output question; #1288: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Fixture runs used `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF, identical results; LF quoted). Compiler facts come from building scratch projects (net10.0) or from Roslyn 5.6.0 against the reference assemblies named. All fixtures lived in the session scratchpad, outside the repository.

## thoenissen/Reihitsu#967

### Premise

- **Case 1 does not reproduce as the issue writes it.** `static public int Counter;` in a class with no attribute list gives `    private static int Counter;` on `main`. The output is correct. `AddAccessibilityModifiers(MemberDeclarationSyntax, …)` moves the declaration's leading trivia, indentation included, onto the new first token. The issue says this case is broken, and that claim is wrong.
- **The defect is real for a declaration with attribute lists.** The declaration's leading trivia then sits on `[`. The token after the attribute lists keeps its own leading trivia (the indentation), and the inserted modifier goes in front of it with no trivia. Observed:
  - RH2005: `[Obsolete]⏎    static public int Counter;` becomes `private     static int Counter;` at column 0.
  - RH2004: `[Obsolete]⏎    void M()` becomes `private     void M()`, as the issue comment says. The same happens for `[Obsolete]⏎    static void N()` and for `[Serializable]⏎    class D` in a namespace, which becomes `internal     class D`.
  - Not affected: `[Obsolete] static int F;` on one line, and `[NonSerialized]⏎    public int Value;`, where the old `public` token gives its trivia to `private`.
- **Same mechanism, worse results.** A comment or directive between the attribute lists and the declaration is relocated or broken:
  - `[Obsolete]⏎    // note⏎    void M()` gives `private     // note⏎    void M()`. The comment now stands behind `private` (SHARED-FIX-003). Same for RH2005 with `static public`.
  - `[Obsolete]⏎#pragma warning disable CS0612⏎    void M()` gives `private #pragma warning disable CS0612`, which does not parse (apply-fix: `invalid-result`, RH2004 and RH2005). This hits a safety rail.
- **Case 2 reproduces.** `s.Value` from `User` gives `private int Value;` and leaves the reference. `nameof(Sample.Value)` from another type does too; compiled, it is CS0122.
- **Reachability.** Both cases are reached through the RH2004 and RH2005 code fixes and Fix All in the IDE.

### Mechanism and candidates

- **Case 1.** When a declaration has attribute lists, the fix puts the new accessibility keyword in front of the first token after the attribute lists without taking over that token's leading trivia, unless the old accessibility keyword is that first token. So whatever stands in that trivia ends up behind the new keyword: indentation, comment, directive.
  - Dispatch: the two callers of `DeclarationModifierUtilities.AddAccessibilityModifier(MemberDeclarationSyntax, …)` (`src/Reihitsu.Core/DeclarationModifierUtilities.cs`), which are the RH2004 and RH2005 fix providers. No other production caller exists.
  - Candidates are every `MemberDeclarationSyntax` kind the RH2004 fix reaches (type, delegate, method, property, field, event, event field, constructor, indexer), plus RH2005 fields.
- **Case 2.** The RH2005 fix does not look at references at all.

### Breaking inputs

FC-034, FC-033, FC-004, FC-005, FC-020, FC-021, FC-016, FC-006.

Matrix for case 1 (both fixes):

| Axis | Members | Fails on `main` |
|---|---|---|
| Attribute lists | none / on the declaration's line / on their own line(s) | only "own line" |
| First token after the attribute lists | old accessibility keyword / another modifier (`static`, `new`, `readonly`, `volatile`, `unsafe`) / type or keyword (`void`, `int`, `class`, `delegate`, `event`, a constructor's name) | every member except "old accessibility keyword" |
| Trivia of that token | indentation only / a comment line / a directive line (`#pragma`, a balanced `#region`/`#if` that compiles) | all three fail: column, relocated comment, unparsable |

Inputs the test implementer must cover:

- **RH2005 reproduction.** It needs an attribute list on its own line in front of `static public`. The new second example under RH2005-003 (`static public int Counter;`, no attribute) passes on `main`, so it cannot be the reproduction (FC-034). The first example already has `[System.Obsolete]` and `public` first, so it passes too.
- **RH2004.** The spec's `[Obsolete]`/`void M()` example, plus a type declaration (`internal`) and a modifier-first member (`static void N()`).
- **The comment variant and the directive variant**, for both rules. RH2004-006 ("on that token's line and in its column") and RH2005-003 ("on its line and in its column") decide them: the modifier goes on the declaration's line and the comment or directive line stays in front of it.
- **Case 2:**
  - a reference from an unrelated type;
  - `nameof(C.Value)` from outside;
  - a derived type;
  - a type nested in a derived type;
  - another part of a partial type;
  - a nested type;
  - inside a nested class `Inner`, a public field read by the enclosing `Outer`. `Outer` is outside `Inner`, so the fix is withheld.
  - `public int A, B;` where only `B` is referenced outside. The fix changes the whole declaration (RH2005-002), so a reference to any declarator withholds it. RH2005-004 says "the field"; see *Internal choices*.
  - Fix All with one withheld and one offered field (SHARED-FIX-002).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2005 analyzer | must stay |
| RH2005 code fix | must change (case 2 guard; case 1 through the shared helper or the fix) |
| RH2004 analyzer | not involved for #967 |
| RH2004 code fix | must change (case 1, same mechanism) |
| `Reihitsu.Core` `DeclarationModifierUtilities` | the one place both fixes share; `src/Reihitsu.Core.Test/DeclarationModifierUtilitiesTests.cs` covers it |
| Fix All | must stay `BatchFixer`; withholding is per declaration (SHARED-FIX-002) |
| Rule pages `RH2005.md`, `RH2004.md` | must stay (I agree with the spec author: `general-notes.md` covers a withheld fix) |
| Formatter | not involved |

### Simpler option

_None._ The shared helper is the single owner of the case-1 decision, so one change covers both fixes.

### Output questions

**3 · RH2005 · Fields must be private — a reference outside the type that still compiles once the field is private**

RH2005-004 withholds the fix for "code that lies in the body of no declaration of that type". Its Why is CS0122. Two kinds of reference lie outside that body but compile after the fix (built on net10.0, no error and no warning):

Input:
```csharp
using System.Diagnostics;

[DebuggerDisplay(nameof(C.Value))]
public class C
{
    public int Value;
}

/// <summary>See <see cref="C.Value"/>.</summary>
public class User
{
}
```

Before (`main`): fix offered → `private int Value;`. Compiles.

After, A (recommended): fix offered, as on `main`. The attribute on `C` and the `cref` in `User` stay valid.
```csharp
[DebuggerDisplay(nameof(C.Value))]
public class C
{
    private int Value;
}
```

After, B: reported, no code fix offered. This is the spec text as written: "body", and every reference counts.

Why A: RH2005-004 exists to prevent CS0122. Withholding the fix where the result compiles narrows a guard for no gain (`AGENTS.md`, *Code-fix coverage*, last bullet). If A is chosen, RH2005-004 should say "where the field is not accessible once it is private" rather than "body".

### Internal choices

- RH2005-004 should say "any field the declaration declares" instead of "the field", because the fix changes every declarator at once. There is only one safe reading, so I recommend the wording without asking.
- The new RH2005-003 example `static public int Counter;` stays as an example: it is correct today and pins the behavior. Add an attribute-list example as the reproduction. Recommend: replace the new example with `[Obsolete]` on its own line in front of `static public int Counter;`, or add that as a third example.

### Issue goals

- Case 1 "Expected" says `private` takes over the leading trivia of the first modifier. This prescribes the solution, and it rests on a wrong reading: without attributes the output is already right. Keep the visible target (`private` first, in the declaration's column, with comments and directives staying in front of it). Drop the mechanism. No visible change.
- "both open-question lines are removed from the non-goals" is stale. The spec author found nothing to remove. Drop it. No visible change.
- "Update `documentation/rules/RH2005.md`" — drop. A withheld fix is covered by `general-notes.md`. The rule page is visible, but leaving it unchanged keeps today's output, so nothing is in question.

### Architecture findings

_None._ Both fixes share one helper, and there is no copy.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1286

### Premise

All four instances reproduce on `main` (apply-fix, RH2001):

- **Comments.** `private int Value /* name */ { get; /* inner */ set; } // trailing` gives `private int _value; // trailing`. `/* name */` and `/* inner */` are dropped.
- **`#if NEVER`.** `return Value;` in `#if NEVER` stays when the property becomes `private int _value;`.
- **Mutable struct.** `Counter Current { get; set; }` with `Current.Increment()` gives `private Counter _current;` and `_current.Increment()`.
- **`readonly`.** `private readonly int Value { get; }` in a struct gives `private readonly readonly int _value;`.

The same class has more failing members, outside the issue's exact positions:

- **Comment after the accessor list.** `private int Value { get; set; } /* init */ = 5;` gives `private int _value = 5;`. The comment is dropped.
- **Multi-line accessor list with a comment line.** `{⏎ // the getter⏎ get;⏎ set;⏎ }` drops the comment.
- **Directive in the accessor list.** `{⏎ get;⏎#if DEBUG⏎ set;⏎#endif⏎ }` gives `private readonly int _value;`. The directives and the disabled `set;` are dropped. Because the field is now `readonly`, a DEBUG build that assigns `Value` no longer compiles.

Comments that are kept today:

- between modifiers (`private /* m */ int`);
- between type and name (`int /* t */ Value`);
- inside the initializer (`= /* v */ 5 /* s */;`);
- behind the last token.

Reachability: through the RH2001 code fix and Fix All in the IDE. The property must be private, so references from other documents are rare but possible (another partial part).

### Mechanism and candidates

- **Comments and directives.** The fix builds the field from the property's modifiers, type, initializer, and its outer leading and trailing trivia. Every trivia attached to the identifier, to any accessor-list token, or to the accessor list's closing brace when an initializer follows is lost. Dispatch: `TryCreateFieldDeclaration` in `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`.
- **`readonly readonly`.** A get-only property gets `readonly` added whether or not it already carries one.
- **`#if`.** The fix renames through the Roslyn renamer and has no check for excluded code. The casing fixes have that check (`CasingCodeFixProviderBase<T>.HasExcludedCodeOccurrenceAsync`, private, `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:517`).
- **Mutable struct.** The fix turns a value that the property returns as a copy into a storage location, so any member that changes the value in place now changes the field.

### Breaking inputs

FC-004, FC-005, FC-020, FC-021, FC-019, FC-003, FC-006, FC-036 (the produced name is allowed to change, per the issue).

Matrix for the comment/directive class (position × trivia kind):

| Position | Comment | Directive | Fails on `main` |
|---|---|---|---|
| between modifiers / between type and name | kept | not tried | no |
| behind the name, before `{` | dropped | — | yes |
| inside the accessor list (any token, one-line or multi-line) | dropped | dropped (with disabled text) | yes |
| between `}` and `=` of an initializer | dropped | — | yes |
| inside the initializer | kept | — | no |
| in front of the first token / behind the last token | kept | — | no |

- **RH2001-007 coverage.** It names "between its name and the end of its accessor list". The `} /* init */ = 5` row and every directive row are outside that sentence. SHARED-FIX-003 (bullet 3, in `Uses`) still decides them: the fix must not be offered. Tests must cover them and tag SHARED-FIX-003, and should test a directive separately from a comment (FC-005).
- **`#if` axis.** The class covers `#if`, `#elif`, `#else`, `#if false`, and an excluded declaration of the new name `_value` (SHARED-CASING-007 and the issue comment).
- **`readonly` axis:**
  - `readonly` get-only in a struct (fails);
  - get-only in a class (already right: `private readonly int _value;`);
  - `static` get-only (`private static readonly`).
- **Mutable-struct axis.** These compile through a property and change the field in place after the fix. Each is a separate member of the class (scratch build, net10.0):
  - a non-`readonly` instance method;
  - a non-`readonly` property getter (`_ = Current.Next;`);
  - an event subscription (`Current.Changed += h;`);
  - a mutating `GetEnumerator` used by `foreach`.

  `Current.Count = 1;` does not compile through a property (CS1612), so it is not a member.

  RH2001-008 specifies only the method call. The other three wait on question 2.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 analyzer | must stay |
| RH2001 code fix | must change |
| Fix All | must stay `BatchFixer`; withholding per property |
| `CasingCodeFixProviderBase<T>` excluded-code check | involved, see *Architecture findings* |
| Rule page `RH2001.md` | must stay |
| Formatter | not involved (the fix already delegates through `FormatNodeInDocumentAsync`) |

### Simpler option

_None._

### Output questions

The pending **Q2** (mutable-struct boundary) has a fact that changes its options:

- **Whether a struct is "declared `readonly`" depends on the target framework of the user's project.** Roslyn 5.6.0, `ITypeSymbol.IsReadOnly`:
  - against `netstandard.dll` 2.0.3: `System.Int32`, `System.DateTime`, `System.Guid`, and `Nullable<T>` all report `False`;
  - against net10.0: `Int32`, `DateTime`, and `Guid` report `True`, and `Nullable<T>` reports `False`.
- **`Nullable<T>`'s members are not all `readonly`.** `GetValueOrDefault` is `readonly` on net10.0. `Equals`, `GetHashCode`, and `ToString` are not, and on netstandard2.0 none is.
- **No member of `Nullable<T>` changes the value in place,** and `.Value` returns a copy, whatever `T` is.

Consequences for the options as written:

- **A and C** withhold the fix for every `int`, `bool`, `double`, `DateTime`, or `Guid` property in a netstandard2.0 or .NET Framework project. That includes this repository's own `Reihitsu.Core`, `Reihitsu.Analyzer`, and `Reihitsu.Formatter`. On net10.0 these keep the fix.
- **B** withholds the fix for `Value.ToString()` on an `int` property in such projects, and for `Cache.ToString()` or `Cache.Equals(x)` on an `int?` property everywhere.
- **The analyzer tests run against `ReferenceAssemblies.Net.Net90`,** so no test would show the netstandard2.0 result.

My view: the question should be asked again with this consequence shown. A visible output then differs by target framework, which the current options do not say. Recommendation: **C′**, which is C plus two exemptions:

- `Nullable<T>` counts as `readonly` for every `T`;
- the special types (`bool`, `char`, the numeric types, `decimal`, `DateTime`) count as `readonly` whatever their metadata says.

With C′, `int?` and `int` properties keep the fix on every target framework, while a user struct that is not `readonly`, or a type parameter, loses it.

Example input for the re-ask (netstandard2.0 project):
```csharp
class C
{
    private int Count { get; set; }

    void M() => Count++;
}
```
- **A or C as written:** no fix offered.
- **B or C′:** fix → `private int _count;`.

The pending **Q1** (location of the one diagnostic of a partial member): see #1287.

### Internal choices

- RH2001-007 names comments only. Recommend widening it to "a comment or directive between the name and the end of the declaration, other than inside the initializer", or keeping it narrow and tagging the other rows SHARED-FIX-003. Both readings withhold the fix, so no visible output differs. I recommend the widening so the rule spec states the whole class in one place.
- The `#if` example tags SHARED-CASING-007 and SHARED-FIX-003. Agreed.

### Issue goals

- "A property used only in `#if NEVER` code → fix withheld or correct": the spec picks withheld (SHARED-CASING-007 coverage). Keep it. No visible change beyond the issue.
- The other goals stand as written.

### Architecture findings

- **One decision for two families of rename fix.** The decision is "withhold a rename whose old or new name occurs in excluded `#if` code" (SHARED-CASING-007 coverage). It lives in a private method of `CasingCodeFixProviderBase<T>`. The RH2001 fix renames through the same renamer and does not derive from that base.
- **A narrow fix would add a second owner** (FC-015): RH2001 code that repeats the check. Per `squad/README.md`, *Architecture findings*, a narrow fix that adds a copy is a reason to change the structure in this run. The components are `CasingCodeFixProviderBase<T>` and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`, both in `Reihitsu.Analyzer.CodeFixes`. The input that shows them disagreeing is the `#if NEVER` input above: the casing fixes withhold for it, and RH2001 does not.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1287

### Premise

Real. Existing characterization tests pin today's per-part behavior, and all four pass on `e18cc30`:

- `RH2002…Tests.VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations`
- `RH7401…Tests.VerifyPartialOverrideMethodIsReportedOncePerPart`
- `RH2101…Tests.VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported`
- the RH2004 partial property/indexer/event test near `RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs:881`

Other facts:

- **RH7002.** The analyzer (`RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.OnSyntaxTree`) reports every top-level `BaseTypeDeclarationSyntax` after the first. It has no `partial` check, so `partial class C {} partial class C {}` reports the second part. The old RH7002-001 left partial declarations outside its coverage. The new text keeps today's report for a partial type that follows another type, so its only visible change is RH7002-004, which the C1 decision demands.
- **RH2004 partial constructor.** Today the defining part `partial P(int x);` is not reported. The fix on the implementing part writes `private partial P(int x)` on that part only.
- **RH2101.** `NestedTypeAnalyzerHelper.IsStaticType` reads one part's modifiers. RH2101 is its only caller.
- **Reachability.** These are analyzer diagnostics in the IDE and the build. Partial properties, indexers, events, and constructors (C# 13/14) and partial methods are real inputs, commonly with a source generator writing one part.

### Mechanism and candidates

Each of these rules judges a declaration node (syntax-node or syntax-tree action), not the symbol, so every part is judged and reported on its own.

Candidates by dispatch:

- **RH2002:** method declarations.
- **RH2004:** `ShouldSkip` skips partial types and partial methods only; partial properties, indexers, and events are reported per part, and of a partial constructor only the implementing part.
- **RH2101:** class declarations.
- **RH7002:** top-level `BaseTypeDeclarationSyntax`.
- **RH8306:** property declarations.
- **RH7401–RH7412:** two shared bases, `MembersShouldBeGroupedByRegionsAnalyzerBase` and `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`. None of the twelve files mentions `partial`. Each reaches the double report through a partial member of its kind: method (C# 9), property and indexer (C# 13), event (C# 14).

### Breaking inputs

FC-024, FC-016, FC-002.

- **RH2101.** Parts in two different files, and a part inside a different part of a partial `Outer`. "Any part carries `static`" has to hold across documents.
- **RH7002 "same type".** These must be covered:
  - `namespace A { namespace B { partial class C {} } }` and `namespace A.B { partial class C {} }` in one file: the same namespace, so the second is not reported;
  - `partial class C<T>` and `partial class C<U>`: the same type, so the second is not reported;
  - two separate `namespace N { … }` blocks.

  Kind mismatches (`partial class C` with `partial record C`) are CS0261 and are not candidates.
- **RH7401–RH7412.** Each rule × a partial member of its kind × parts in two regions. Which of them double-report today has to be settled by test, not assumed.
- **Generated parts.** A partial member whose other part is in generated code: SHARED-DIAG-001 forbids a diagnostic there. See Q1.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2101 analyzer, `NestedTypeAnalyzerHelper.IsStaticType` | must change |
| RH7002 analyzer | must change |
| RH2002, RH2004, RH8306, RH7401–RH7412 analyzers | must change once Q1 is answered |
| RH2004 code fix | must change: withheld on a partial member (SHARED-DIAG-004, first paragraph) |
| RH7104/RH7105/RH7106 | must stay (P28, issue comment) |
| `SHARED-NESTED-002` | must stay |
| Rule pages | not involved |

### Simpler option

_None._

### Output questions

The pending **Q1** needs two more axes before it can be answered without leaving a gap. My view on its options:

1. **Under A, "first in document order" is undefined when the parts are in different files**, which is the normal case for partial members implemented by a source generator.
2. **A part in generated code may not carry a diagnostic (SHARED-DIAG-001).**
   - Under C (always the implementing part), RH2004 and RH8306 on a user-written defining part whose implementing part is generated would land in generated code, so the diagnostic would be lost.
   - Under A and B the user's part carries it, as long as "first" skips generated parts.
3. **For RH2002, A and C always coincide.** `async` on the defining part is CS1994 (scratch build: "The 'async' modifier can only be used in methods that have a body"), so `async` only ever stands on the implementing part. B is the only option that puts RH2002 on a part that does not write `async`.

Recommendation: **A**, with "first" spelled out:

- Of the parts that qualify, the one that is not in generated code, the defining part before the implementing part.
- When no part qualifies by what it writes (RH2004: both parts omit the accessibility), the defining part.

This order does not depend on files. It gives the same result as A's "document order" whenever the defining part comes first in one file. It differs from document order only when the implementing part stands above the defining part in the same file.

Example for the re-ask:
```csharp
public partial class P
{
    partial int Prop { get => 1; set { } }

    partial int Prop { get; set; }
}
```
- A, document order: `{|RH2004:Prop|}` on the first line (the implementing part).
- A, defining part first: `{|RH2004:Prop|}` on the second line.

The overlap with #1334 (P25, "which part decides" for RH7401–RH7412) should be stated in the question, so the two answers cannot contradict each other.

### Internal choices

- **RH8306 is a per-part text rule by SHARED-DIAG-004's own second paragraph:** a summary is text one part writes independently. The maintainer still classified #1311 Q5 as P15 (round 8, recorded on #1311). The decision stands. Recommend that SHARED-DIAG-004's second paragraph be worded so it does not seem to cover a documentation comment, and that the spec author's follow-up candidate 2 (summaries that disagree) be folded into Q1's answer: under A, the first part whose summary is a sentence carries it. It needs no separate issue.
- **The spec author's follow-up candidate 1 (RH2101 on a non-static nested partial class, each part reported) is not an open question.** SHARED-NESTED-002 decides it with a Why. Only the wording conflicts with SHARED-DIAG-004's exclusion list, so this is a spec-wording follow-up, not an output question.

### Issue goals

- **"The RH2004-002 requirement and its tests are updated" is stale.** RH2004-002 covers partial types and partial methods, which stay unreported. The per-part report of partial properties, indexers, events, and constructors sits in RH2004-001's coverage and in the characterization test near `RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs:881`. Change the goal to name those. No visible change.
- **"Lane: bug fix with a behavior change (change doc)"** is superseded by the triage comment, as `00-lead.md` decision 2 records.

### Architecture findings

_None._ (The twelve RH74xx rules share two bases, so a change there is one change, not a copy.)

### Follow-up candidates

- **Wording of SHARED-DIAG-004.** Its exclusion list should name the per-part reports that SHARED-NESTED-002 requires, and its second paragraph should not appear to cover RH8306's summaries. This replaces the spec author's candidates 1 and 2.

## thoenissen/Reihitsu#1288

### Premise

Real. `static N IAdd<N>.operator +` and `static explicit IAdd<N>.operator int` are reported today, and the fix writes `private static …`:

- apply-fix: fixed, with `private static N IAdd<N>.operator +` and `private static explicit IAdd<N>.operator int`;
- `RH2004AccessModifierMustBeDeclaredAnalyzerTests.VerifyExplicitInterfaceOperatorImplementationsAreReportedAndGetPrivate` passes on `e18cc30`.

`ShouldSkip` exempts explicit implementations of methods, properties, indexers, and events, but not `OperatorDeclarationSyntax` or `ConversionOperatorDeclarationSyntax` with an `ExplicitInterfaceSpecifier`.

Reachability: static abstract interface members (C# 11) in user code, through the analyzer.

### Mechanism and candidates

The exemption for explicit interface implementations is keyed on four of the six member kinds that can carry an explicit interface specifier.

Dispatch: the `switch` in `RH2004AccessModifierMustBeDeclaredAnalyzer.ShouldSkip`. Candidates: operator and conversion operator, each implicit and explicit conversion, both checked and unchecked (`operator checked +`).

### Breaking inputs

FC-024, FC-006.

- the user-defined operator (`+`, a unary `-`, `checked +`);
- an implicit and an explicit conversion;
- with and without an expression body.

The characterization test above has to flip to "not reported".

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2004 analyzer | must change |
| RH2004 code fix | not involved (nothing is reported, so no fix) |
| Rule page | must stay |

### Simpler option

_None._

### Output questions

_None._ Maintainer round 7b (option A) decides it.

### Internal choices

_None._

### Issue goals

- "the diagnostic stays, the fix is withheld" is superseded by round 7b option A (not reported). Drop it. The maintainer decided the visible result.
- The ordinary-operator `public` part is out of scope (#1435).

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Commands:
  - `scripts/apply-fix.sh RH2005|RH2004|RH2001 <scratch fixtures>`: the outcomes quoted above.
  - `scripts/test.sh --project analyzer --filter "Name=VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations|Name=VerifyPartialOverrideMethodIsReportedOncePerPart|Name=VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported|Name=VerifyExplicitInterfaceOperatorImplementationsAreReportedAndGetPrivate"`: 4 passed.
- Compiler facts:
  - scratch `net10.0` builds: a `cref` and a class-level attribute referencing a private field compile; `nameof` from outside is CS0122; `Current.Count = 1` through a struct property is CS1612; event `+=`, a non-readonly getter, and `foreach` through a struct property compile; `async` on a partial definition is CS1994;
  - `ITypeSymbol.IsReadOnly` and `IMethodSymbol.IsReadOnly` were queried with Roslyn 5.6.0 against `netstandard.library/2.0.3` and net10.0.
- The output-question batch for the lead:
  - Q1 needs the generated-part and cross-file axes added;
  - Q2 needs the target-framework consequence added;
  - Q3 (RH2005 references that still compile) is new.
