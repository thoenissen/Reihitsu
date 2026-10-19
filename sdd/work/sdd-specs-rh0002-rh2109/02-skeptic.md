Role: skeptic
Commit: cf9cd72
Verdict: proceed narrower

My prompt named the lane and what to check. It carried no intended fix, suspected cause, or hoped-for verdict.

The spec diff is sound in structure. All three shared specs pass the bar of more than one spec relying on them: SHARED-EQUAL is used by RH1001 and RH1002, SHARED-NESTED by RH2101 and RH2104–RH2108, SHARED-DEBUG by RH2006 and RH2007. Each claims only types that the batch's rules alone use (`rg` over `src/`). No claimed type is owned by another spec or batch. `DeclarationModifierUtilities`, `ModifierOrderingUtilities`, `CasingUtilities`, and `PropertySymbolExtensions` are not claimed. No `src/` file changed since 78af33e.

Six requirements, though, state as intended behavior a case that looks wrong. Two of those cases are new instances of P1. One requirement states behavior the code does not have. Four of the proposed open questions need a correction or an addition. The spec author must narrow or correct the points below before the test implementer seeds tests from them. None of them blocks the run or needs the maintainer now.

## Premise

How I established the facts:

- **Code reading.** I read the analyzer, code fix, and helper of every rule in the batch: `StructEqualityPerformanceAnalyzerBase`, `EqualityComparerArgumentUtilities`, `NestedTypeAnalyzerHelper`, `DebugMessageTextAnalysisUtilities`, `DeclarationModifierUtilities`, `PropertySymbolExtensions.IsAutoProperty`, and every RH0002–RH2109 analyzer and provider.
- **Analyzer-only rules.** No repository entry point runs an analyzer without a code fix, because `scripts/apply-fix.sh` needs a provider. For those rules I used a scratch SDK project outside the repository and deleted it afterwards. It was `net10.0`, `LangVersion preview`, SDK 10.0.402 (compiler 5.9.0), with the branch's Debug `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` as `<Analyzer>` items and a `.razor` additional file. `dotnet build` gave both the compiler errors and the RH diagnostics.
- **Code fixes.** I ran the fixes with `scripts/apply-fix.sh` (LF and CRLF agree).

### A. Requirements that state a case that looks wrong as intended

1. **SHARED-EQUAL-002 counts a comparer for the *value* type as a custom comparer for the key.**
   - Mechanism: `HasExplicitEqualityComparerArgument` accepts an argument passed to any parameter whose type is `IEqualityComparer<>`, whatever its type argument.
   - Input: `items.ToImmutableDictionary(x => x, x => 0, null, valueComparer)` with `IEnumerable<S> items`, a struct `S` without equality members, and `IEqualityComparer<int> valueComparer`.
   - Observed: RH1002 reports nothing. `items.ToImmutableDictionary(x => x, x => 0)` is reported. The keys are still hashed by `EqualityComparer<S>.Default`.
   - The `ToImmutableDictionary` overloads with `keyComparer, valueComparer`, from a selector or from `KeyValuePair` pairs, are the shapes that reach this. The constructors RH1001 checks take only a key comparer.
   - The requirement's wording ("a parameter of type `IEqualityComparer<T>`") covers this case. It needs a coverage sentence, for example one that limits the requirement to a comparer of the compared type. The case is a new question for the `shared/struct-equality.md` issue. No principle decides it, and no existing issue covers it (searched).

2. **RH2101-002 together with SHARED-NESTED-002 reports the non-static part of a nested `static partial` class.**
   - Mechanism: `IsStaticType` reads the modifiers of one declaration part. It does not ask whether the class is static.
   - Input: `public class Outer3 { static partial class Helpers { } partial class Helpers { } }`.
   - Observed: it compiles without errors, and RH2101 reports the second `Helpers`.
   - The class is static. RH2101-002's Why ("A static class holds no instance state") exempts it. Read literally, "carries the `static` modifier" plus "each part … its own declaration" specifies the report.
   - RH2101-002 needs a coverage sentence. File a new issue, `SDD open questions: sdd/specs/analyzer/RH2101.md`. No principle decides the case (FC-024).

3. **RH2004-001 reports explicit interface implementations of operators and conversion operators.**
   - Mechanism: `ShouldSkip` exempts `ExplicitInterfaceSpecifier` only on methods, properties, indexers, and events.
   - Input: `interface IAdd<T> where T : IAdd<T> { static abstract T operator +(T a, T b); static abstract explicit operator int(T value); }` and `public struct N : IAdd<N> { static N IAdd<N>.operator +(N a, N b) => a; static explicit IAdd<N>.operator int(N value) => 0; }`.
   - Observed: it compiles without errors, and RH2004 reports both, on `+` and on `operator`.
   - RH2004-003's own Why, "Neither can carry an accessibility modifier; adding one does not compile", covers exactly these declarations.
   - The code fix is offered for them, because `TryGetMemberDeclaration` includes both kinds, and adds `private`. With `private static N2 IAdd<N2>.operator +(…)` the build fails with CS0106 ("The modifier 'private' is not valid for this item"). The fix half is decided by principle P1 (SHARED-FIX-003, first condition). Whether to report these declarations at all is an ordinary question, which RH2004-003's Why already points to.
   - RH2004-001 needs a coverage sentence. RH2004-006's coverage already excludes operators. Both halves go into the `analyzer/RH2004.md` issue.

4. **RH2004-006 adds the modifier to one part of a partial property, indexer, event, or constructor, which leaves a compile error.**
   - Mechanism: RH2004 exempts `partial` only on type declarations and methods (FC-024).
   - Observed reports: each part of a partial property, partial indexer, and partial event is reported. For a partial constructor, only the implementing part is reported. The defining `partial K1(int x);` is not.
   - `scripts/apply-fix.sh RH2004` on `partial int Prop { get; set; }` / `partial int Prop { get => 1; set { } }` converges in 2 iterations, one part at a time. The state after one application is `private partial int Prop { get; set; }` next to `partial int Prop { get => 1; set { } }`. A build of that state fails with CS8799 ("Both partial member declarations must have identical accessibility modifiers").
   - On `partial K1(int x);` / `partial K1(int x) { }` the fix converges, and the result still fails with CS8799 for good, because the defining part is never reported.
   - Decided by principle P1 (SHARED-FIX-003, first condition). RH2004-006's coverage sentence must also exclude partial members. The unreported defining part of a partial constructor is a separate, ordinary question. Both go into the `analyzer/RH2004.md` issue.

5. **RH2001-004 on a property that already carries `readonly`.**
   - Mechanism: `TryCreateFieldDeclaration` adds `readonly` to the property's modifiers without checking for one.
   - Input: `public struct S { private readonly int Value { get; } public S(int value) { Value = value; } public int Get() => Value; }`.
   - Observed: it compiles, RH2001 reports `Value`, and `scripts/apply-fix.sh RH2001` produces `private readonly readonly int _value;`. A build of that result fails with CS1004 ("Duplicate 'readonly' modifier").
   - The requirement says "keeps the property's … modifiers" and "a get-only property MUST become a `readonly` field", which together specify this output. It needs a coverage sentence.
   - New question for the `analyzer/RH2001.md` issue, decided by principle P1 (SHARED-FIX-003, first condition). No existing issue covers it (searched for RH2001 fix issues).

### B. A requirement that misstates current behavior

6. **RH1001-002: a target-typed `new()` as the operand of a cast to the collection type is not reported. The cast type is reported instead.**
   - Mechanism: `IsAnalyzedByTargetTypeReference` returns true for a `CastExpressionSyntax` of the same type, through parentheses, `!`, conditional branches, and `switch` arms.
   - Input: `object A() => (Dictionary<S1, int>)new();` and `object B(bool flag) => (Dictionary<S1, int>)(flag ? new() : new());`.
   - Observed: it compiles, and RH1001 reports only `S1` in each cast type, never a `new`.
   - The requirement says a `new()` "MUST be reported when it is not the initial value of a variable or property declared with that type". That is false for the cast. This is a spec correction, not a question: the behavior matches RH1001-003, which already treats the cast type as the owner.
   - Related: the Why of RH1001-002 ("A creation that initializes a declaration of the same type is the declaration's own collection, so one diagnostic on the declared type covers both") reads as if it applied to explicit creations too. `Dictionary<S, int> d = new Dictionary<S, int>();` gets two diagnostics, on the declared type and on the creation, as RH1001-003's example `j` shows. The Why should be limited to the target-typed creation, which names no type argument.

### C. Proposed open questions that need a correction or an addition

7. **RH2109 Q1 marking.** In #996 the coordinator wrote that a case marked `Decided by principle Pn (#912)` "becomes a bug without a question round". P5 is SHARED-DIAG-003, and its scope is C# literals and `//`, `/* */`, `///`, and `/** */` comments. Razor `@* *@` and HTML `<!-- -->` comments are outside that scope. The draft itself asks the maintainer to confirm that P5 extends to them. A question the maintainer has to confirm is not decided. Drop the marking, file it as an ordinary question, and name P5 as the precedent. Two more inputs belong with Q1 and Q2, for the same raw-text mechanism (`@code\s*\{` over the whole file):
   - an `@code {` that starts a line inside a multi-line C# `/* */` comment or a multi-line string in another code block;
   - an `@code{` inside a C# string, which I observed reported: `private string s = "@code{";` inside an `@code` block gave a third RH2109.

   The RH2109-001 coverage sentence ("starts a line, after optional white space, outside a Razor or HTML comment") still covers the first input. It should also exclude C# comments and literals.
8. **SHARED-DEBUG Q1 is narrower than its coverage sentence.** Observed: `Debug.Assert(c, f ? "" : null)` is not reported by RH2006, and `Debug.Fail(f ? " " : "")` is not reported by RH2007. Both have no text on every path, like `string.Empty`, and SHARED-EQUAL-002 looks through such conditionals for comparers. Add these to Q1, or file them as Q2. In the other direction, the coverage sentence leaves out non-constant fields and properties in general, although only `string.Empty` among them is questionable. Each coverage sentence should keep out exactly the inputs that are filed.
9. **RH1002 Q1 misses two methods of the same class.** .NET 10 adds `Enumerable.LeftJoin` and `Enumerable.RightJoin`, which hash keys like `Join` (both names are present in `Microsoft.NETCore.App.Ref/10.0.13` `System.Linq.dll`). The fixed name list in `_relevantMethodNames` does not include them. They belong in Q1 with `CountBy` and `AggregateBy`. The factories `ImmutableHashSet.Create`/`CreateRange` and `ImmutableDictionary.CreateRange` are outside the list for the same reason. Neither RH1002 nor RH1001 reports them: the RH1001 non-goal covers `var` from a call, and the declared return type is in the framework. The spec author should decide whether they join Q1. I did not run them.
10. **RH2001 Q3 contains a wrong statement.** Its parenthetical says "A get-only property is not affected". Inside a constructor, a `readonly` field is a variable, so `_current.Increment()` mutates the field. The get-only property's getter returns a copy. `private Counter Current { get; }` with `Current.Increment();` in the constructor therefore changes behavior too. Correct the parenthetical. The question itself stands.

All other proposed questions are real, and I found no principle or existing issue that decides them. That covers struct-equality Q1 and Q2, RH2001 Q1, Q2, Q4, and Q5, RH2002 Q1 and Q2, RH2003 Q1, RH2004 Q1, and RH2109 Q2 and Q3. Notes on three of them:

- For struct-equality Q1, the rule pages of RH1001 and RH1002 already say "Implement `IEquatable<T>`, or override both". Expect "intended", unless the maintainer wants a change doc.
- RH2004 Q1 is about input that already fails with CS0558, so it is not a P1 case.
- Searches for RH2101 static partial classes, RH1002 value comparers, RH2004 partial members, and RH2001 `readonly` found no issue.

The #971 and #967 further instances in the spec author's handover hold. RH2004 reaches `AddAccessibilityModifiers(SyntaxTokenList)` through `AddAccessibilityModifier`. That inserted token takes no leading trivia, and the token that starts the line keeps its indentation behind it. This is the mechanism of #967 case 1.

### D. Smaller points for the spec author

- `documentation/rules/RH2004.md`: the added sentence is justified. The page said every member that supports accessibility must declare it, and members declared directly in an interface support it but are exempt.
- `documentation/rules/RH1002.md` is wrong, which clarification 9 allows to correct. It says "Only the `System.Linq.Enumerable` methods are checked." Yet its own list includes `ToImmutableHashSet`, `ToImmutableDictionary`, `ToFrozenSet`, and `ToFrozenDictionary`, which are declared on `ImmutableHashSet`, `ImmutableDictionary`, `FrozenSet`, and `FrozenDictionary` and are checked (RH1002-001). The sentence is true only of `Contains` and `SequenceEqual`.
- RH2004-006, `private` bullet: members of a C# 14 extension block are reported, and the fix adds `private`. That is their default: `extension(int number) { int Twice() => …; }` is not callable from another class (CS1061). The bullet names only class, struct, record, and record struct, so it leaves this out. The behavior is correct, so add it as a clarification.
- RH0002-002: the qualifier for an empty or comments-only file (the empty span at the end, from the zero-width end-of-file token) has no example, and no existing test covers it (`RH0002…Tests` has three tests, all with a `namespace` token).
- SHARED-EQUAL-001 coverage sentence: "a generic struct … whose type arguments are not structs that lack equality members" also leaves out `KeyValuePair<S, int>`, which lacks equality members itself and is reported either way. The questionable set is generic structs that have their own equality members (`S?`, `(int, S)`).
- Restated shared wording: RH2006-001's second sentence equals RH2007-001's, RH2006-002 equals RH2007-002 apart from the method name, and both have the same Why. The SHARED-DEBUG non-goal deliberately hands this to each rule. That is defensible, because the requirements are parallel and concern different methods. The lead decides whether they move into SHARED-DEBUG.
- Non-goals that restate a requirement:
  - RH2006 and RH2007, "The detail message … Only the `message` argument is checked", repeat RH2006-002 and RH2007-002.
  - RH2002, "`Task`, `ValueTask`, and every other return type are not reported", repeats RH2002-004.
  - SHARED-NESTED, "… do not change whether a declaration in it is nested", is a behavioral statement that SHARED-NESTED-001 already makes.
- Clarification 4 says "reference" `DeclarationModifierUtilities`. No RH2005 requirement states the behavior RH2004 relies on (RH2005-003 is RH2005's own replacement of `public`), so RH2004's `Uses —` is correct. Nothing to list.

## Mechanism and candidates

_N/A._ This run specifies existing behavior. The mechanism of each case above is given with it.

## Breaking inputs

| Class | Rules and inputs the test implementer must cover |
|---|---|
| FC-024 partial types and members, explicit interface implementations, static interface members | RH2004: partial property, indexer, event, and constructor parts (reported as observed, kept out of RH2004-006); explicit operator and conversion-operator implementations (kept out of RH2004-001 once narrowed); interface members and types nested in interfaces (RH2004-004). RH2101: static partial class parts (kept out). RH2002: explicit implementation and partial method (kept out, as the coverage sentence says) |
| FC-002 qualifier not tested | SHARED-EQUAL-002: every listed form needs an example on both sides. The current example lacks a `switch` expression, a null-conditional access with a `null` receiver, and a non-trailing named argument. RH0002-002: empty file and comments-only file. RH1001-003: several declarators in one declaration, where all must pass a custom comparer (`Variables.All`) |
| FC-027 other enclosing scope | RH1001: `new()` in a variable initializer, a property initializer, a cast (RH1001-002 once corrected), an arrow expression (reported on `new` and on the return type), and an assignment (reported on `new`) |
| FC-036 rename reaching `nameof` | RH2001-004's `nameof(Name)` → `nameof(_name)` is decided (SHARED-FIX-003 run-time bound). The test pins it |
| FC-023 verbatim identifiers | SHARED-DIAG-002 already covers RH2003's `new @NotImplementedException()`. RH2001 field naming uses the identifier's value |
| FC-020/FC-021 comments and directives | RH2001 Q1 and Q2 stay out of the spec. Do not seed tests that pin them as intended; at most write characterization tests |
| FC-007 a fix raising another RH diagnostic | RH2001 orders `readonly` through `OrderModifiersForRh7105`. RH2004 inserts the accessibility first |
| FC-008 Fix All | RH2001 and RH2004 use `BatchFixer`. A Fix All scenario with two diagnostics in one document already exists in their test bases |

Not reachable here: FC-010 to FC-014. No formatter phase owns these design and performance concerns, and neither code fix changes a formatter phase.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH0002–RH2109 | must stay (lane: no production change) |
| Code fixes RH2001, RH2004 and their Fix All | must stay. The P1 cases are filed, not fixed |
| `Reihitsu.Core` (`DeclarationModifierUtilities`, `ModifierOrderingUtilities`, `CasingUtilities`) | must stay, not claimed |
| Formatter | not involved |
| Specs | must change: A1–A5 (coverage sentences), B6 (RH1001-002 text and Why), D (RH2004-006 extension blocks, RH0002-002 example, coverage-sentence precision) |
| Rule docs | RH2004 change stays. RH1002's "Only the `System.Linq.Enumerable` methods are checked" is wrong and may be corrected. Others must stay |
| Open-question issue drafts | must change: RH2109 Q1 marking, SHARED-DEBUG Q1, RH1002 Q1, RH2001 Q3; add A1 (struct-equality), A2 (new RH2101 issue), A3 and A4 (RH2004), A5 (RH2001) |
| Tests | test implementer: tag and add per the specs once corrected |

## Simpler option

_None._ The narrowing above is the smaller change: coverage sentences and one corrected sentence, with no new requirements beyond the RH2004-006 extension-block clarification.

## Questions for the maintainer

_None that blocks this run._ Every competing reading found is filed through the open-question issues and not decided here: RH2001 Q5, where the attribute exemption has two readings, and RH2109 Q1, whether P5 extends to Razor and HTML comments.
