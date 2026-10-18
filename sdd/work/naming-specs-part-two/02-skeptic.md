Role: skeptic
Commit: bba3f32
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Neither did `01-spec-author.md`.

The run should go ahead in the tests lane, but the spec author should repair five things first, because tests derived from the spec as written would pin behavior that looks wrong:

1. RH4112, RH4113 and RH4114 cover properties whose original definition is in a referenced assembly (finding A).
2. Four fix requirements cover a record member whose name is shared with a positional parameter (finding B).
3. RH4116 covers tuple element names that a referenced assembly dictates (finding E).
4. RH4115-001 leaves out two kinds of pattern designation that the analyzer reports (finding C).
5. RH4112-002 to RH4114-002 leave out the cascade of a rename to overrides and implicit implementations (finding D).

Every other requirement states what the code does today. I checked each one against the analyzer's dispatch and `GetLocations`, against `CasingCodeFixProviderBase<T>`, `CasingUtilities`, `NamingAccessibilityClassifier` and `SingleLetterIdentifierUtilities`, and against the runs below.

## Premise

The issue is a spec backfill. Its premise checks out, with one correction:

- **RH4116/RH4118 "share a class name".** They do not. The types are `RH4116TupleElementCasingAnalyzer` (registered on `SyntaxKind.TupleType`) and `RH4118TupleElementCasingAnalyzer` (registered on `SyntaxKind.TupleExpression`). They share only the suffix. The specs describe each correctly (RH4116-001, RH4118-001). Q6 of the spec author files the naming as a question, not a defect.
- **thoenissen/Reihitsu#971.** Kept out through the camelCase coverage limit of SHARED-CASING-006. RH4121's PascalCase body cannot reach it.

### How I checked

- **Runs.** Every run used the spec author's scratch probe (`/tmp/claude-0/-home-user-Reihitsu/153ec12d-0883-5726-b799-f5a8330ef8f9/scratchpad/probe`). It runs the shipped analyzer and code fix of `cc63994`, and the production code is unchanged at `bba3f32`.
- **Why a probe.** `scripts/apply-fix.sh` cannot run here. Eight of the sixteen rules have no code-fix provider, and the runner gives each fixture a project with no extra references.
- **Referenced-assembly inputs.** For these I copied that probe to `.../scratchpad/skeptic/probe2`. The copy adds one option, `LIB=<file>`, which compiles the file into a metadata reference.
- **Inputs.** All inputs are in `.../scratchpad/skeptic/*.cs`.

### A — Properties whose original definition is in a referenced assembly (RH4112, RH4113, RH4114)

**Input.** A referenced assembly (`lib.cs`, `lib2.cs`, `lib3.cs`) declares:
- `public class Base { public virtual int count { get; set; } protected virtual int size { get; set; } }`
- `public interface ILib { int total { get; } }`
- with `InternalsVisibleTo("P")`: `internal virtual int size`

The source declares:
- `public class Impl : ILib { public int total => 0; }`
- `public class Middle : Base { public override int count { get; set; } }`
- `public class Leaf : Middle { public override int count { get; set; } }`
- the same `Middle`/`Leaf` chain with `protected override int size` and with `internal override int size`.

**Output:**
- Every property is reported: RH4114 for `total` and `count`, RH4112 and RH4113 for `size`.
- `Impl.total` and `Middle.*` get no fix.
- `Leaf.*` gets a fix that renames `Leaf` and `Middle` together to `Count` or `Size`.
- Compiling the fixed `Middle`/`Leaf` text gives `CS0115 'Middle.Count': no suitable method found to override`.

This is the mechanism of thoenissen/Reihitsu#1013 Q1 (RH4103) and thoenissen/Reihitsu#1014 Q1 (RH4102). RH4102-001 and RH4103-001 leave it out with "This requirement covers a … whose original definition is declared in source".

RH4112-001/-002, RH4113-001/-002 and RH4114-001/-002 instead cover it: "every property declaration that belongs to the … group". By SHARED-CASING-007's coverage, the fix that breaks the build is a covered rename.

SHARED-CASING-009 does not withhold that fix. The added error is in `Middle`, which is not a declaration that contains the renamed `Leaf.count`.

RH4111 is not reachable: a private property cannot override, and it cannot implement an interface implicitly.

### B — A positional parameter and an explicit member of the same name (RH4111–RH4114, RH4120)

**Input** (`s4120.cs`, `s4120c.cs`, `s4114rec.cs`, `priv.cs`):

```csharp
public record R(int count)
{
    public int count { get; init; } = count;
}
```

**Output:**
- RH4120 and RH4114 both report `count`. With `private int count` the property is reported by RH4111 instead.
- **RH4120's fix:** gives `public record R(int Count) { public int count { get; init; } = Count; }`.
  - `r.Count` then compiles. Before the fix it fails with CS1061.
  - So the compiler has synthesized a new public property `Count` next to `count`.
- **RH4114's fix:** gives `public int Count { get; init; } = count;`.
  - `r.count` still compiles afterwards, now as a synthesized property.
  - The same happens with RH4111's fix of the private property, which adds a new public `count`.
- In each case the fix adds a public property. That changes the record's equality, `ToString`, `Deconstruct` and public API (FC-006, FC-036).

RH4120-003 says the fix renames "the parameter, the property it declares, and every reference to them". Here the parameter declares no property. RH4120-001's `Why:` ("The compiler turns each of these parameters into a public property") is false for this input. RH4111-002 to RH4114-002 cover the rename without exception.

The field rule RH4109, specified in thoenissen/Reihitsu#978, does the same (`field.cs`: `public int count = count;` → `public int Count = count;`). That is outside this run and is a follow-up candidate.

### C — RH4115 reports designations that RH4115-001 does not list

**Input** (`s4115.cs`):
- `input is string { Length: > 0 } Text`
- `pair is (1, 2) Pair`
- `values is [_, _] Items`

**Output:**
- RH4115 reports `Text`, `Pair` and `Items` (`SingleVariableDesignation` whose parent is not a parenthesized designation).
- `Text` and `Items` are fixed. `Pair` got no fix only because the fixture also has a parameter `pair` (SHARED-CASING-009).

RH4115-001 lists only "a declaration pattern or `var` pattern that declares a single variable". The designation of a recursive pattern (property or positional) and of a list pattern are missing. Nothing about them looks wrong; this is a clarification of current behavior.

Also confirmed, all covered by RH4115-001 as written:
- `case long Big:`
- the switch-expression arm `int Number => …`
- `await foreach (var Element in …)`
- `.. var Rest`
- `out var Parsed` in a field initializer and in a constructor initializer

### D — Property renames cascade to overrides and implementations in source (RH4112–RH4114)

**Input** (`cascade.cs`):
- `interface I { int value { get; } }` and `class C : I { public int value => 0; }`
- `class B { public virtual int size => 0; }` and `class D : B { public override int size => 1; }`

**Output.** Fixing any one of the four renames its pair, in both directions: the interface and its implementation, the base and its override.

RH4114-002 mentions only explicit implementations. RH4103-002 states the cascade for methods ("also renames the methods that override it and the implementations of it"). This is current behavior the spec does not mention. Limit it to source chains, because of A.

### E — Tuple element names a referenced assembly dictates (RH4116)

**Input** (`lib.cs` + `meta.cs`): `Base` declares `public virtual (int first, int Second) Pair()`, and the source overrides it with the same element names.

**Output:** RH4116 reports `first` in the override (no code fix exists).

Changing the name in the override is a compile error (CS8139), and the base cannot be edited, so the diagnostic can only be suppressed. The same holds for an implicit implementation (CS8141). This is the same family as A, without a broken fix. RH4116-001 covers it ("wherever the tuple type is written").

### F — Extension-block receiver parameter (RH4122)

**Input** (`partial.cs`, `ext.cs`):

```csharp
public static class E
{
    public static int Ext(this int s) => s;
    extension(string t) { public int Size => t.Length; }
}
```

**Output:**
- RH4122 reports `s` and does not report `t`. The receiver's `ContainingSymbol` is the extension type, not a method, so `IsSupportedParameter` returns false.
- RH4105 does report an extension-block receiver `Text`.

RH4122-001 does not list the receiver, so the spec needs no change. The input looks inconsistent: the receiver of a classic extension method is reported, and RH4105 checks the same parameter.

### Checked and found consistent

- **SHARED-SINGLE-001:**
  - `é` in the spec is precomposed U+00E9.
  - `Ⅻ` (U+216B, Nl) is reported.
  - `a` is reported as `a`.
  - A supplementary-plane letter (`𝑥`) is not a valid identifier in Roslyn, so "one character" holds for every identifier that compiles.
- **SHARED-SINGLE-002:** `IsForEachIterationVariable` and `ForEachStatement.Identifier` match it.
- **RH4121-003 and RH4121-004:**
  - Probed: `tkey` → `TTkey`, `Tkey` → `TTkey`, `tKEY` → `TKEY`, `KEY_NAME` → `TKeyName`.
  - `T2b` gets no fix.
  - `class A<T, t>` and `class A<TKey, key>` get no fix (collision).
  - `tkey` → `TTkey` follows the same rule as `Type` → `TType`, which the spec states, and is not part of Q5.
- **RH4122, implicit interface implementations:**
  - RH4122-001 states they are checked, and the code does check them. The split is coherent: named arguments bind to the declaration that member lookup finds. For an override that is the base, and for an explicit implementation only the interface can be used. An implicit implementation is found through the class, so its own names are used there.
  - No existing test covers it, so the test implementer has to add one.
  - The `Why:` of RH4122-002 would read as deliberate if it said this. That is optional.
- **Restating shared behavior.** No requirement restates shared behavior.
  - RH4121-004 repeats the first clause of SHARED-CASING-008 ("no letter or digit"). It cannot reference SHARED-CASING-008, because that requirement is tied to the derivation of SHARED-CASING-006 and RH4121 derives the name in its own way. That is acceptable.

## Mechanism and candidates

_N/A._ This run specifies existing behavior; there is no bug to explain.

For reference, these are the dispatch sets I derived candidates from:

| Rule | Registered on | Filter |
|---|---|---|
| RH4111–RH4114 | `PropertyDeclaration` | `NamingAccessibilityClassifier` |
| RH4115 | `LocalDeclarationStatement`, `ForStatement`, `ForEachStatement`, `UsingStatement`, `FixedStatement`, `SingleVariableDesignation`, `CatchDeclaration` | — |
| RH4116 | `TupleType` | — |
| RH4117 | `DeclarationExpression` | parenthesized designation |
| RH4118 | `TupleExpression` | `NameColon` |
| RH4120 | `Parameter` | parent is the parameter list of a `RecordDeclarationSyntax`, and the name is not `_` |
| RH4121 | `TypeParameter` | — |
| RH4122–RH4124 | `Parameter` | `IParameterSymbol.ContainingSymbol` and its `MethodKind` |
| RH4125 | `SingleVariableDesignation`, `VariableDeclarator` | `ILocalSymbol` |
| RH4126 | `ForEachStatement`, `SingleVariableDesignation` | — |
| RH4127 | `CatchDeclaration` | — |

## Breaking inputs

### Inputs the tests must cover once the spec is repaired

- **FC-024, partial members.** Each partial part gets its own diagnostic (`partial.cs`):
  - `partial int count` declared and implemented → two RH4114 diagnostics;
  - `partial class C<key>` in two parts → two RH4121 diagnostics per type parameter;
  - `partial void M<item>(int a)` → two RH4122 diagnostics and two RH4121 diagnostics.

  The specs say "one diagnostic per property / type parameter / parameter". The spec author should say "per declaration" or name the partial case, so that a test does not assert one diagnostic.
- **FC-024, explicit implementations and static interface members.**
  - Explicit interface implementations: no group for RH4111/RH4114; exempt for RH4122.
  - A static interface property belongs to the public group (RH4114).
- **FC-023, verbatim names and `_`.**
  - `@b` (RH4122) and `@count` (RH4118): the reported span includes the `@`.
  - `_` is exempt in RH4120, RH4122–RH4127 and in lambda discards `(_, _)`.
  - RH4116 and RH4118 report a tuple element named `_`.
  - Precomposed `é` and `Ⅻ` count as single letters.
- **FC-027, nesting.**
  - Nested tuple types (RH4116) and nested tuple expressions (RH4118).
  - `var (a, (b, c))` (RH4117).
  - `foreach ((int c, var (d, e)) in …)` (SHARED-SINGLE-002).
  - A designation in a `foreach` collection expression or body counts for RH4125, not RH4126.
  - Recursive and list pattern designations (C).
- **FC-003, both sides of each boundary.**
  - RH4121 accepts `T1`, `TKEY`; reports `T1Key`, `Type`, `tKey`, `t1`.
  - RH4121's fix: `tKey` → `TKey` versus `Type` → `TType`.
  - Single-letter versus two-letter names.
  - A `catch` with and without a variable.
- **FC-009.** No action is registered when the name is unchanged or not usable: RH4121 `_` and `T1Key`.
- **FC-006 and FC-036, source chains only.**
  - RH4120: the parameter, its synthesized property and `r.firstName` are renamed together.
  - RH4112–RH4114: the cascade to overrides and implementations in source (D).
- **FC-025.** Expression-bodied and static properties (RH4111-001 to RH4114-001).

### Inputs the tests must not pin until the maintainer decides

- Q1–Q7 of `01-spec-author.md`.
- Findings A, B, E and F above.
- thoenissen/Reihitsu#971.
- The questions of thoenissen/Reihitsu#1009, including references in inactive code (FC-020).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| The 16 analyzers and 8 code-fix providers | Must stay (no production change in this lane) |
| `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `CasingUtilities`, `SingleLetterIdentifierUtilities` | Must stay |
| Fix All | Must stay; SHARED-CASING-010 covers it, no rule-specific Fix All requirement is needed |
| `sdd/specs/analyzer/RH4112.md`, `RH4113.md`, `RH4114.md` | Must change: limit -001/-002 to properties whose original definition is in source (A); state the cascade in source (D); limit -002 for B |
| `sdd/specs/analyzer/RH4111.md` | Must change: limit -002 for B |
| `sdd/specs/analyzer/RH4120.md` | Must change: limit -003 for B, and correct the -001 `Why:` that claims every parameter becomes a property |
| `sdd/specs/analyzer/RH4115.md` | Must change: add recursive and list pattern designations to -001 (C) |
| `sdd/specs/analyzer/RH4116.md` | Must change when E is filed: leave the dictated names out of -001 |
| `sdd/specs/analyzer/RH4121.md`, `RH4122.md` | Wording only: "per declaration" for partial members; RH4122 optionally explains the check of implicit implementations |
| The other specs and `shared/single-letter-names.md` | Must stay |
| `sdd/specs/shared/casing.md` | Must stay (lead's constraint); B touches SHARED-CASING-007, and that goes to an issue, not into the spec |
| `requirement-ids.txt`, `sdd/specs/README.md` | Must stay, unless the spec author makes C or D a new requirement instead of extending the existing one |
| Analyzer tests | Must change: tagging and focused tests (test implementer) |
| Rule pages | Not involved; no documented behavior changes |
| Formatter, CLI | Not involved; no formatter phase renames identifiers, and no analyzer behavior changes (FC-013, FC-014 not reachable) |

`SingleLetterIdentifierUtilities` is the code owner of the shared spec, but it also implements RH4122-002 and RH4122-003 (`IsFrameworkMandatedSignature`, `IsRecordPrimaryConstructorParameter`). The structure rules allow this. It is noted so a later change to those methods is checked against RH4122 too.

## Simpler option

_None._ Limiting the coverage, as RH4102 and RH4103 already do, is the narrowest change that keeps wrong-looking behavior out of the specs.

## Questions for the maintainer

None of these blocks the run. Each case is left out of the spec and filed per `sdd/README.md` (*Open questions*), as the spec author's Q1–Q7 are. The inputs and outputs are under *Premise* above.

- **A — properties whose original definition is in a referenced assembly** (RH4112, RH4113, RH4114).
  - Same question as thoenissen/Reihitsu#1013 Q1 and thoenissen/Reihitsu#1014 Q1.
  - Reading A: do not report them, and withhold the fix that renames a source override chain of a metadata property.
  - Reading B: keep the current output, including the `Leaf` fix that fails with CS0115.
  - File in new `SDD open questions: sdd/specs/analyzer/RH4112.md`, `…/RH4113.md` and `…/RH4114.md` issues. The lead may group them, provided each spec's issue links the question.
- **B — a record positional parameter and an explicit member of the same name** (RH4111–RH4114, RH4120, and the RH4106–RH4110 field rules of thoenissen/Reihitsu#978).
  - Reading A: withhold the fix when the rename would separate a positional parameter from its member of the same name.
  - Reading B: rename the parameter and the member together.
  - Reading C: keep the current output. The fix then adds a public property (`Count` next to `count`), which changes equality, `ToString`, `Deconstruct` and the public API.
  - Every rule that reaches it shares the fix behavior of SHARED-CASING-007, so the question fits a new entry in thoenissen/Reihitsu#1009. It is not one of the questions already filed there.
- **E — tuple element names fixed by an overridden or implemented member of a referenced assembly** (RH4116).
  - Reading A: do not report them.
  - Reading B: keep reporting them, though only a suppression can resolve the diagnostic.
  - File in a new `SDD open questions: sdd/specs/analyzer/RH4116.md` issue.
- **F — the receiver parameter of a C# 14 extension block** (RH4122).
  - Reading A: report it, like the `this` parameter of an extension method.
  - Reading B: leave it unchecked, and record that as a non-goal once decided.
  - File in a new `SDD open questions: sdd/specs/analyzer/RH4122.md` issue.
