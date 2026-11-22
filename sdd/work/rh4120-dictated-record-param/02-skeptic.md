Role: skeptic
Commit: 4f880ec
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's *Affected* section names `CasingAnalyzerBase.IsDictatedMember` and `RecordPositionalParameterUtilities` as the place of the change; that is an implementation proposal and is not evaluated here.

## Premise

Real. Today's behavior follows from the code, and the issue's claim that a rename breaks the build was checked by compiling.

- **What reaches it.** The analyzer runs in any compilation that references an assembly declaring a lowercase interface or abstract property, in the IDE and in a command-line build. In the IDE it also runs when another project of the solution declares that property. The case is reachable.
- **Today's output, from the code.** `RH4120RecordPrimaryConstructorParameterCasingAnalyzer.GetLocations` yields every positional parameter of a record except `_`, using syntax only. RH4120 does not override `CasingAnalyzerBase.IsDictatedName`, whose base implementation returns `false`. So `CheckCasing` reports every positional parameter that is not in PascalCase, whatever its property implements or overrides. This is plain C# control flow, not a Roslyn result.
- **The rename breaks the build.** I built a scratch two-project solution, with `Meta` holding `INamed { string name { get; } }` and `abstract record MetaRecord { public abstract string title { get; init; } }`. `dotnet build` reported:
  - `public record Person(string Name) : INamed;` gives `error CS0535: 'Person' does not implement interface member 'INamed.name'`.
  - `public record Item(string Title) : MetaRecord;` gives `error CS0534: 'Item' does not implement inherited abstract member 'MetaRecord.title.get'` (and `.init`).
  - `public record Item2(string title) : MetaRecord;` compiles, and `MetaRecord m = new Item2("t"); m.title` runs.
- **The issue's framing is not entirely accurate.** The issue says "the code would stop compiling or change what `INamed` binds to". With a plain interface property, it stops compiling (CS0535). With a default interface member, the build succeeds and the binding changes. Both are what SHARED-CASING-014 already says about a property declared in the record's body, so the issue's severity argument holds.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug. The cause in one sentence: RH4120 checks the parameter's name by syntax and never asks whether the property that the compiler declares for the parameter is dictated.

The candidate dispatch is `RecordDeclarationSyntax.ParameterList` (the `record`, `record class`, `record struct`, and `readonly record struct` kinds) crossed with the compiler's property synthesis. The synthesis has three outcomes:

1. A property is declared for the parameter.
2. No property is declared, because the record declares a member of that name.
3. No property is declared, because the record inherits a member of that name.

Scratch check, listing the public instance properties each record declares (reflection, `DeclaredOnly`):

```text
Person: name                 // record Person(string name) : INamed
DerivedLower:                // record DerivedLower(string name) : Person(name)
DerivedUpper: Name           // record DerivedUpper(string Name) : Person(Name)
BodyLower: name              // record BodyLower(string name) : INamed { public string name { get; init; } = name; }
BodyUpper: Name,name         // record BodyUpper(string Name) : INamed { public string name { get; init; } = Name; }
Explicit: name               // record Explicit(string name) : INamed { string INamed.name => "x"; }
V:                           // record V(string title) : MetaVirtual   (virtual, not abstract, title in Meta; warning CS8907)
LocalFromMeta:               // record LocalFromMeta(string name) : MetaPerson(name)   (MetaPerson(string name) in Meta)
Gen: name                    // record Gen(string name) : IGen<string>
ViaLocal: name               // record ViaLocal(string name) : ILocal   (interface ILocal : INamed in source)
Part: name                   // partial record Part(string name); partial record Part : INamed;
```

The run used `$HOME/.dotnet/dotnet run --project App` on a scratch project under the session scratchpad, which has been deleted. `Explicit`'s `INamed.name` returned `"x"`, so the generated `name` implements nothing.

## Breaking inputs

Failure classes that reach the change:

- **FC-003** — test both sides of the boundary.
- **FC-015** and **FC-016** — one owner for the mapping from a parameter to its property, and for the dictated decision. RH4116 already exempts the tuple element names of the same parameters.
- **FC-024** — partial records and explicit implementations.
- **FC-044** — which interface member the property implements is the compiler's result. It must be read, not modelled.

FC-023 is reachable but low value: `@class`-style names go through `ValueText`, and I found no separate behavior. FC-036, FC-004, and the formatter classes do not reach the change, because no code fix or formatter path changes.

Axes:

- the relation to the outside member: implicit implementation, override of an abstract property, a chain through source, a source interface that extends an outside one, a generic interface, or an explicit implementation in the body;
- where the outside member is declared: a referenced assembly or another project;
- the record kind;
- whether the record's own type implements the interface, or only a derived type does;
- property synthesis outcomes 1, 2, and 3 above.

| # | Input (Meta = referenced assembly) | Expected per spec diff | Existing test |
|---|---|---|---|
| 1 | `record Person(string name) : INamed` | not reported | none found |
| 2 | `record struct Point(string name) : INamed`, `readonly record struct` likewise | not reported | none found |
| 3 | `record Item(string title) : MetaRecord` (override of an abstract Meta property) | not reported (output question 1) | none found |
| 4 | `abstract record Middle : MetaRecord { public abstract override string title { get; init; } }` + `record Leaf(string title) : Middle` (chain through source) | not reported | none found for RH4120; RH4116 has the same shape (`TupleElementNamesOfARecordPositionalParameterWhosePropertyOverridesAReferencedAssemblyPropertyAreNotReported`) |
| 5 | `interface ILocal : INamed { }` + `record ViaLocal(string name) : ILocal` | not reported | none found |
| 6 | `record Gen(string name) : IGen<string>` (generic Meta interface) | not reported | none found |
| 7 | `partial record Part(string name);` + `partial record Part : INamed;` | not reported | none found |
| 8 | Case 1 with `INamed` in another project of the solution (`AdditionalProjects`, the pattern RH4103's tests use) | not reported | none found |
| 9 | `record P(string name, string other) : INamed` | only `other` reported; its fix renames `other` alone | none found |
| 10 | `interface ILocal { string code { get; } }` in source + `record Local(string code) : ILocal` | reported | none found for RH4120 |
| 11 | `record Holder(string name);` + `record Sub(string other) : Holder(other), INamed` | `Holder.name` and `Sub.other` reported | none found |
| 12 | `record Explicit(string name) : INamed { string INamed.name => "x"; }` | reported (the generated property implements nothing) | none found |
| 13 | `record Derived(string name) : Person(name)` with case 1's `Person` | reported (output question 2) | in-compilation analogue pinned by `NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord` |
| 14 | `record BodyLower(string name) : INamed { public string name { get; init; } = name; }` | reported (output question 2) | in-compilation analogue pinned by `NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty` |
| 15 | `record LocalFromMeta(string name) : MetaPerson(name)`, `record V(string title) : MetaVirtual` | reported (output question 2, option C) | none found |
| 16 | `_`, PascalCase parameters, and non-record primary constructors | unchanged | `DiscardsAndParametersOfOtherConstructorsAndMembersAreNotChecked`, `VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter` |

For a referenced assembly, test helpers exist: `CompiledAssemblyReference.CreateAsync` plus `test.TestState.AdditionalReferences`, as in `RH4116TupleElementCasingAnalyzerTests`. Another project uses `TestState.AdditionalProjects`, as in `RH4103MethodNameCasingAnalyzerTests`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4120 analyzer | must change |
| `CasingAnalyzerBase.IsDictatedMember` (the one statement of "dictated") | must stay; no second copy (FC-015) |
| `RecordPositionalParameterUtilities` (the one owner of parameter to declared property) | must stay; no second copy (FC-015) |
| RH4120 code fix, Fix All, `CasingRenameSafetyUtilities` | not involved. A parameter that is not reported gets no fix, and the fix for the other parameters in the same record is unchanged (case 9). |
| RH4116 | not involved; it already exempts this parameter's tuple names, for both implicit implementation and override |
| RH4112–RH4114, RH4105 | not involved |
| Formatter, CLI, `Reihitsu.Core` | not involved |
| Rule page `documentation/rules/RH4120.md`, `general-notes.md` | not involved. No casing rule page mentions dictated names, so leaving it unchanged is consistent. |
| Specs | SHARED-CASING-014 and RH4120-001 changed (see *Internal choices* for one inconsistency) |
| Tests | must add (table above) |

## Refuse option

- **Refusing.** Report the parameter and offer no fix. This is option B on the issue, which the maintainer rejected (option A chosen). It is a visible output already decided, so it is not available.
- **Declaring a shape unsupported.** Not an option. The concept applies to formatter input shapes, and no formatter phase, code fix, or guard is involved here.
- **Cost of the requested change.** A small override in the RH4120 analyzer that reuses two existing helpers: no new type, no new mechanism, and a few production lines.
- **Rarity.** The input class is rare: lowercase property names imposed by another assembly.
- **Simpler options.** _None._

## Output questions

Output question 1 is listed because the lead flagged the narrow reading. My view is that the maintainer's answer covers it: the answer says "dictated, as for properties under SHARED-CASING-014", and that definition includes overrides. Output question 2 is a case the spec diff decides, outside the answer. If the lead treats it as outside issue #1939, the spec diff should not decide it either. That would mean removing the boundary sentence's verdict and the `Derived` marker from the SHARED-CASING-014 example, and filing a follow-up that carries this question.

**1 · RH4120 · Record primary constructor parameter names should be in PascalCase — parameter whose property overrides an abstract property of a referenced assembly**

Input, with a referenced assembly declaring `public abstract record MetaRecord { public abstract string title { get; init; } }`:
```csharp
public record Item(string title) : MetaRecord;
```

Before (`main`): `title` is reported; renaming it to `Title` fails to compile (CS0534).

After, A (recommended):
```csharp
public record Item(string title) : MetaRecord;
```
(not reported)

After, B:
```csharp
public record Item(string {|RH4120:title|}) : MetaRecord;
```
(reported as today)

Why A: the name cannot be changed without breaking the build, the same cause as the interface case. The tuple-name rule already exempts this same parameter for the override case, so B would make the two rules disagree on one parameter.

**2 · RH4120 · Record primary constructor parameter names should be in PascalCase — parameter that gets no property because its record declares or inherits a property of its name that another assembly fixes**

Input, with a referenced assembly declaring `public interface INamed { string name { get; } }` and `public record MetaPerson(string name);`:
```csharp
public record Person(string name) : INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Local(string name) : MetaPerson(name);
```

Before (`main`): `name` is reported on `Person`, `Derived`, `Annotated`, and `Local`, and no fix is offered on `Derived`, `Annotated`, or `Local`. Renaming any of these three parameters compiles, but it gives the record a second public property `Name` next to `name`, which also shows up in equality, `ToString`, `with`, and serialization.

After, A:
```csharp
public record Person(string name) : INamed;

public record Derived(string {|RH4120:name|}) : Person(name);

public record Annotated(string {|RH4120:name|}) : INamed
{
    public string name { get; init; } = name;
}

public record Local(string {|RH4120:name|}) : MetaPerson(name);
```

After, B (recommended):
```csharp
public record Person(string name) : INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Local(string {|RH4120:name|}) : MetaPerson(name);
```

After, C:
```csharp
public record Person(string name) : INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Local(string name) : MetaPerson(name);
```

Why B: the only manual way out (rename) adds a duplicate public property, the outcome the code fix already refuses as unsafe, so the warning has no acceptable resolution when the matching property's name is fixed by another assembly through an interface or an override. `Local`'s property merely comes from another assembly, which is a wider cause than the answered one, so B keeps it reported. Pick A if a rename that compiles counts as resolvable however it changes the public surface; pick C if any property from another assembly counts.

## Internal choices

- **RH4120-001's coverage sentence disagrees with the spec diff's example.** RH4120-001 says "This requirement covers a parameter that the compiler turns into a property". Meanwhile, the new SHARED-CASING-014 example marks `Derived.name` with `RH4120`, and the boundary sentence says such a parameter "is reported as its rule states". No RH4120 requirement states that it is reported. The code (`GetLocations`, syntax only) and two existing tests do report it: `NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord` and `NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty`, which are tagged RH4120-003 and SHARED-CASING-007, not RH4120-001. Whatever output question 2 decides, one RH4120 requirement should state whether a positional parameter without a declared property is checked. Recommendation: widen RH4120-001's coverage sentence to every positional parameter, whether or not the compiler declares a property, minus whatever exemption output question 2 adds. This states today's code, so no visible output changes beyond question 2.
- **Spelling of the exemption.** The spec author stated it as a bullet of SHARED-CASING-014 rather than as an RH4120 requirement. I agree, because it keeps one statement of what makes a name dictated (FC-015).

## Issue goals

- **"The maintainer has chosen A or B."** Met by the decision recorded in `00-lead.md`.
- **"A test covers it, with a counterpart test that a parameter whose generated property implements no referenced-assembly member is still reported."** Keep. The one counterpart test is too thin for FC-003 and FC-024: cases 10–12 are the counterparts, cases 1–9 the exempted side. Expanding the tests does not change visible output.
- **The case named as "implements".** It narrows what the maintainer's answer says ("dictated, as for properties"). See output question 1; that question changes visible output if the narrow reading is chosen.
- **The *Affected* section.** It prescribes the components to change, which is an implementation proposal. It should be neither dropped nor adopted as a goal; it changes no output.

## Architecture findings

_None._ The exemption reuses the single owner of each decision (`IsDictatedMember` and `RecordPositionalParameterUtilities.GetDeclaredProperty`). RH4116 already applies the same exemption to the same parameter through the same helpers, so RH4120 is catching up with an existing statement rather than copying one.

## Follow-up candidates

_None_ beyond output question 2, if the lead treats it as outside the issue (see above).
