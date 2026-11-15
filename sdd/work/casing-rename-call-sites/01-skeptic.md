Role: skeptic
Commit: e0b9ff2
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue body's option recommendation ("A") is a proposal of an `agent-filed` issue and was weighed as such, not taken as given.

## Premise

**The issue's two inputs do not reproduce on HEAD, but the class it describes does, and it is wider than the issue says.**

- Both example inputs are withheld already. The derived names `Select` and `Add` are on the SHARED-CASING-011 list (`IsCompilerLookupMethodRename`, `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:807`). That list landed in #1675 on 2026-11-09, the day the issue was filed. `scripts/apply-fix.sh RH4103` reports `no-fix-offered` (LF and CRLF) for the `select` input and for the `add(int)`/`Add(object)` input. `sdd/specs/shared/casing.md:828` already specifies the `select` case as withheld.
- With names outside that list, the class reproduces through the single code fix. Every fixture below was run with `scripts/apply-fix.sh <ID> <dir>`; the LF and CRLF results are identical. The fix renames the declaration and also rewrites code that refers to a *different* symbol already spelled with the new name, so that this code keeps its binding.
- **Reachability.** A user reaches this by invoking the casing code fix (the 25 providers deriving from `CasingCodeFixProviderBase<T>`) on a reported declaration: single fix through `RegisterCodeFixesCoreAsync` → `ApplyCodeFixAsync` → `Renamer.RenameSymbolAsync`, and Fix All through `ApplyFixAllAsync` → `RenameWithoutConflictAsync` → the same `Renamer.RenameSymbolAsync` call. Fix All was not run; that it behaves the same is read from the code: same renamer call, same arguments. The renamer works across the whole solution, so a rewritten reference can stand in another document. That case was not run either, because the runner builds one document per project.
- **Severity.** The issue's framing that "SHARED-FIX-003 and SHARED-FIX-006 pull in different directions" is overstated. Withholding the fix satisfies both. SHARED-FIX-003 never demands a fix; it only forbids a fix that changes run time. Only option B needs a spec change.
- The issue also understates the class. In one sub-class the renamer does **not** keep the binding: the reference changes its binding silently, with no rewrite and no conflict annotation. This is a run-time change that SHARED-FIX-003 forbids today (sub-class R2 below). For example, f06 after the fix:

```csharp
public class Base { public int Process() => 1; }
public class Derived : Base { public int Process() => 2; }   // was process(); fix renamed it
public class User { public int Use(Derived derived) => derived.Process(); }  // unchanged text; now calls Derived.Process
```

I ran the post-fix code as a file-based app (`dotnet run`). It prints `2`; before the fix `derived.Process()` bound to `Base.Process` (returns `1`). The compiler adds only warning CS0108, so `HasAddedCompileErrorsAsync` does not catch it.

## Mechanism and candidates

**Mechanism.** The rename changes which symbol an existing reference spelled like the new name binds to. That reference may be a simple name, a member access, a `nameof` operand, an overload candidate set, or an extension-method candidate set. Roslyn's renamer either rewrites that reference so it keeps its old binding (R1), or misses the change (R2). The casing base accepts both results: a resolved conflict carries no `ConflictAnnotation`, and both results compile, so none of its own checks fires. Those checks are the lookup-name list, the namespace collision check, added compile errors, positional properties, excluded code, and caller-argument expressions.

**Dispatch type.** `GetDeclaredSymbol` (`CasingCodeFixProviderBase{T}.cs:99`) switches over `VariableDeclaratorSyntax`, `MemberDeclarationSyntax`, `ParameterSyntax`, `SingleVariableDesignationSyntax`, `ForEachStatementSyntax`, `CatchDeclarationSyntax`, `LocalFunctionStatementSyntax`, and `TypeParameterSyntax`. The candidates, by declared symbol kind, with the rules that report them:

| Declared kind | Rules | R1 (rewrite) run | R2 (silent rebind) run |
|---|---|---|---|
| Named type | RH4002–RH4006, RH4010, RH4011 | f09 fails | not applicable as constructed (hiding a nested type not run) |
| Method | RH4103 | f03, f04, f05, f07, f08, f10, f12, f16, f17, f21 fail | f06, f20, f23 fail |
| Local function | RH4104 | f24 fails | not applicable: a local function inherits nothing |
| Parameter (method, record primary constructor) | RH4105, RH4120 | f11, f14, f18 fail (RH4105); RH4120 not run, same symbol kind | not applicable |
| Field | RH4106–RH4110 | f25 fails (RH4109) | f26 fails (RH4109) |
| Property | RH4111–RH4114 | f19 fails (RH4114) | f22 fails (RH4114) |
| Event | RH4102 | f27 fails | not run |
| Enum member | RH4101 | f28 fails | not applicable |
| Local, deconstruction, foreach and catch variables | RH4115, RH4117 | f13 fails (RH4115); others not run, same symbol kind | not applicable |
| Type parameter | RH4121 | f29 fails | not applicable |

## Breaking inputs

Failure classes: FC-036 (a rename treated as behavior-preserving although it reaches more than the symbol: here, other symbols' references) and FC-006 (meaning changes: overload selection, extension vs. instance lookup). FC-018 does not apply: references *are* retargeted through the renamer.

**Class matrix.** The axes are:

1. the declared kind (table above);
2. the lookup that the new name wins. The kinds seen are:
   - simple-name lookup in a nested scope, which covers a nested type over an outer type, a local or parameter over a field, and a local function over a method;
   - a `using` or `using static` import;
   - namespace vs. type vs. member, as in method-vs-type and property-vs-type;
   - overload resolution, non-generic and generic;
   - instance method over an extension method;
   - extension-method closeness, by namespace nesting;
   - ambiguity between extension methods;
   - member hiding of an inherited member;
   - `nameof`.
3. what the renamer writes: nothing (R2), or one of these rewrites (R1):
   - `this.` qualification;
   - `base.` qualification;
   - type qualification of a static member;
   - namespace qualification of a type;
   - extension call turned into a static call;
   - argument cast.
4. whether the reference stands in the declaring document or in another one (only the declaring document was run);
5. single fix vs. Fix All (only the single fix was run).

Every member constructed fails, with the fix offered on HEAD. `before → after` of the rewritten reference:

| # | Rule | Input shape (renamed declaration; existing reference) | Result |
|---|---|---|---|
| f03 | RH4103 | extension `toList(this IEnumerable<int>)` in `N`; `values.ToList()` | R1: `Enumerable.ToList(values)` |
| f16 | RH4103 | extension `count` in outer namespace `N`; `values.Count()` in `N.Inner` | R1: `Enumerable.Count(values)` |
| f05 | RH4103 | instance `first()` on `Bag : IEnumerable<int>`; `bag.First()` | R1: `Enumerable.First(bag)` |
| f12 | RH4103 | extension `shuffle` in class `B` next to `A.Shuffle` (same signature); `values.Shuffle()` | R1: `A.Shuffle(values)` (the SHARED-CASING-009 ambiguity path the issue mentions) |
| f04 | RH4103 | `write(int)` next to `Write(object)`; `box.Write(1)` | R1: `box.Write((object)1)` |
| f17 | RH4103 | `format<T>(T)` next to `Format(object)`; `box.Format(1)` | R1: `box.Format((object)1)` |
| f07 | RH4103 | nested `Inner.log(string)`; `Log("x")` binding to static `Outer.Log` | R1: `Outer.Log("x")` |
| f08 | RH4103 | `max(int,int)` under `using static System.Math`; `Max(1, 2)` | R1: `System.Math.Max(1, 2)` |
| f10 | RH4103 | method `color()` in class using enum `Color`; `Color.Red` | R1: `N.Color.Red` |
| f21 | RH4103 | `Derived.process()` hiding `Base.Process()`; `Process()` inside `Derived` | R1: `base.Process()` |
| f06 | RH4103 | `Derived.process()` hiding `Base.Process()`; `derived.Process()` in another type | **R2**: unchanged text, now calls `Derived.Process` (run, prints 2 instead of 1) |
| f20 | RH4103 | as f06 with `Base.Process` virtual | **R2**: unchanged text, rebinds (new slot, CS0114 warning only) |
| f23 | RH4103 | static `Derived.create()` hiding static `Base.Create()`; `Derived.Create()` | **R2**: unchanged text, rebinds |
| f24 | RH4104 | local function `helper()`; `Helper()` binding to the class method | R1: `this.Helper()` |
| f11 | RH4105 | parameter `X`; `nameof(x)` of static field `Guard.x` | R1: `nameof(Guard.x)` |
| f14 | RH4105 | parameter `Name`; field `name` used unqualified | R1: `this.name` |
| f18 | RH4105 | parameter `Name` in a static method; static field `name` | R1: `Person.name` |
| f13 | RH4115 | local `Total`; field `total` used unqualified | R1: `this.total` |
| f09 | RH4002 | class `order` in `N`; `Order` imported from `Other` by `using Other;` | R1: `Other.Order` |
| f19 | RH4114 | property `node`; static class `Node` used as `Node.Create()` | R1: `N.Node.Create()` |
| f22 | RH4114 | `Derived.size` hiding `Base.Size`; `derived.Size` | **R2**: unchanged text, rebinds |
| f25 | RH4109 | nested `Inner.limit`; `Limit` binding to static `Outer.Limit` | R1: `Outer.Limit` |
| f26 | RH4109 | `Derived.limit` hiding `Base.Limit`; `derived.Limit` | **R2**: unchanged text, rebinds |
| f27 | RH4102 | nested event `changed`; `Changed()` binding to static method `Outer.Changed` | R1: `Outer.Changed()` |
| f28 | RH4101 | enum member `red`; initializer `Blue = Red + 1` binding to `Outer.Red` | R1: `Outer.Red` |
| f29 | RH4121 | type parameter `item`; type `TItem` used in the class | R1: `N.TItem` (twice) |

Members that hold, which the test implementer can use as controls:
- the issue's `select` and `add` inputs: `no-fix-offered` (SHARED-CASING-011);
- `computeFactor` next to `ComputeFactor` in one class: `no-fix-offered` (SHARED-CASING-009, compile-error path);
- the plain RH4103 rename without a capture, `computeTotal`: fixed, only declaration and references change.

Candidates the test implementer must cover, beyond the issue's text:
- R1 with a name outside the SHARED-CASING-011 list, at least: an extension capture (f03), an overload capture (f04), a nested-scope capture (f07 or f25), a type capture (f09), `this.` qualification (f14 or f13), `nameof` (f11), and the ambiguity path (f12). Each in single-fix and Fix All mode.
- R2 (f06, f22, f26): a member access whose receiver's static type is the renaming type or one derived from it, where a base type has a member of the new name.
- One member with the rewritten reference in another document of the same project, and one in a dependent project (not run).
- The controls above.

R2 is relevant to how the class is detected, not only to scope: R2 changes no text other than the declaration. Any reading of "code other than the declaration and its references" judged only from the text the renamer changed decides R1 and leaves R2 untouched.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Casing analyzers (RH4002–RH4121) | Must stay: no diagnostic changes |
| `CasingCodeFixProviderBase<T>`, single fix (`RegisterCodeFixesCoreAsync`, `HasRenameConflictAsync`, `ApplyCodeFixAsync`) | Must change under A, for R1 and R2. Under B, must change for R2 only |
| `CasingCodeFixProviderBase<T>`, Fix All (`ApplyFixAllAsync`, and `CandidatesConflictAsync` for duplicate-target groups) | Same as single fix: they share `RenameWithoutConflictAsync` |
| The 25 derived casing providers | Not involved: they inherit the behavior |
| `Reihitsu.Core`, formatter, CLI | Not involved: the casing fixes use the renamer only |
| Rule pages, `general-notes.md` | Not involved: "A diagnostic without a code fix" already covers a withheld fix (`AGENTS.md`, *Code-fix coverage*) |
| `sdd/specs/shared/casing.md` | Must change under A: record the withheld class, most naturally under SHARED-CASING-009, which today withholds only when "the renamer reports a conflict" or a compile error appears, and so does not decide a conflict the renamer resolves. Must change under B as well: the exception to SHARED-FIX-006. R2 needs no spec change: SHARED-FIX-003 already decides it |
| `sdd/specs/shared/code-fixes.md` | Must change only under B (SHARED-FIX-006 exception) |
| Analyzer tests (`src/Reihitsu.Analyzer.Test/Naming/`) | Must change: add tests. No existing casing test asserts a rewritten reference (searched for `Enumerable.X(`, `(object)`, `this.x +`, `global::`, qualified `nameof`) |

## Refuse option

- **Refuse option = the issue's option A.** Withhold the casing fix when the rename changes what any reference to another symbol binds to, whether the renamer rewrites that reference (R1) or not (R2).
  - Cost: production code in one method region of one file, and no new mechanism beyond the base's existing set of rename checks. Option B costs zero production lines for R1 and a spec exception.
  - R2 has to be refused under either option, because SHARED-FIX-003 forbids a run-time change.
- **Is the class rare?** The renamed declaration always violates the rule, since that is the diagnostic. The *captured* symbol falls into two groups:
  - In the camelCase members (f11, f13, f14, f18) the captured symbol violates another rule (a public or static field named in camelCase), so they are rare by definition.
  - In the PascalCase members (f03, f05, f07–f10, f12, f16, f19, f24–f29) the captured symbol is a correctly named, ordinary symbol, such as LINQ `ToList`/`First`/`Count`, an imported type, or `Math.Max`.

  The input still needs a wrongly cased declaration whose PascalCase spelling is exactly a name already bound at some reference in its scope. I judge that uncommon, not common code. The casing fixes already withhold the neighbouring collision class (SHARED-CASING-009) and the whole SHARED-CASING-011 list unconditionally. The maintainer's answer to the output question settles escalation 9's "commonly written" clause in any case.
- **Declaring a shape unsupported is not an option.** Unsupported shapes are formatter precheck shapes, checked on the tokens and trivia of the formatter's input. The casing fixes never pass through the formatter, and the shape is semantic (which symbol a name binds to), which no cheap syntactic check of tokens and trivia decides.
- **Other simpler options.** None beyond B, which is the "no code change for R1" option.

## Output questions

**1 · RH4103 · Method names should use PascalCase — the rename would capture an existing call to another method**

The same answer decides every casing rule. The other rules' shapes are in *Breaking inputs*: for example, RH4002 rewrites `Order` to `Other.Order`, and RH4105 rewrites `name` to `this.name`.

Input:
```csharp
using System.Collections.Generic;
using System.Linq;

namespace N
{
    public static class Extensions
    {
        public static List<int> toList(this IEnumerable<int> source) => new List<int>();
    }

    public class User
    {
        public List<int> Get(int[] values) => values.ToList();
    }
}
```

Before (`main`):
```csharp
using System.Collections.Generic;
using System.Linq;

namespace N
{
    public static class Extensions
    {
        public static List<int> ToList(this IEnumerable<int> source) => new List<int>();
    }

    public class User
    {
        public List<int> Get(int[] values) => Enumerable.ToList(values);
    }
}
```

After, A (recommended): no fix offered; the diagnostic on `toList` stays.

After, B: as `main`. The fix renames `toList` to `ToList` and rewrites `values.ToList()` to `Enumerable.ToList(values)`.

Why A: the call belongs to another method, and a fix that rewrites code its diagnostic does not name is an edit nobody asked for, while a withheld fix is a supported end state for this uncommon collision.

## Internal choices

- **Where the withheld class is recorded (under A).** Option one: extend SHARED-CASING-009, whose heading "would collide with an existing name" covers it but whose body names only reported conflicts and compile errors. Option two: add a new requirement. Neither changes visible output. Recommend extending SHARED-CASING-009, which keeps one requirement for "the new name meets an existing binding".
- **R2 in this run or as a follow-up.** R2 is a SHARED-FIX-003 defect against an unchanged requirement, in the same method region and of the same decision: does the rename change what an existing reference binds to. Withholding is the only output SHARED-FIX-003 allows, so nothing visible is in question. Recommend pulling it into this run, because the class's detection has to cover it anyway (see the last paragraph of *Breaking inputs*). Under B it is the run's only code change.
- **Lane.** Under A the run stays bug fix: the code returns to SHARED-FIX-006, and the spec change only records the withheld class. Under B, R1 becomes a spec-only change and R2 remains a bug fix, so the bug-fix lane still admits the diff.

## Issue goals

- **"Today's output" (both examples):** stale. They are withheld on HEAD by SHARED-CASING-011 (#1675). Replace them with reproducing inputs, such as f03 and f04. No visible change.
- **"With A: no casing fix rewrites a call site it does not rename":** too narrow. The rewrites also hit type references (f09, f10, f19, f29), field accesses (f13, f14, f18, f25), `nameof` operands (f11), and enum initializers (f28). Change it to "no casing fix changes code other than the declarations and references of the rename cascade". It adds no visible change beyond what A already decides.
- **"each shape above has a test in single-fix and Fix All mode":** the issue's shapes (`select`, `add`) pass on `main` because of SHARED-CASING-011, so tests on them prove nothing about this issue. Drop them in favour of the reproducing members listed under *Breaking inputs*. No visible change.
- **"SHARED-CASING-009's ambiguity path has a test with a method name outside the fixed list":** keep. f12 (`shuffle`) gives one; the claim "today such a test cannot be written" holds only in the sense that the fix's result rewrites the call.
- **"With B: the spec states the exception to SHARED-FIX-006":** keep.
- **Missing goal: R2.** A rename that silently rebinds a member access to a hiding member must be withheld under either option. SHARED-FIX-003 demands it, so nothing visible is in question; the lead decides scope.
- **Framing "SHARED-FIX-003 and SHARED-FIX-006 pull in different directions":** drop. A withheld fix satisfies both.

## Architecture findings

_None._ The casing base and Roslyn's renamer share the safety decision. The base adds checks on top of the renamer's conflict report, which is a layered check, not two owners of one decision or a prediction of another component. R2 is a gap in what that combined check covers.

## Follow-up candidates

- If the lead does not take R2 into this run: a bug follow-up. Casing fixes rename a member to the name of an inherited member it then hides, silently rebinding member accesses whose receiver is the derived type. Members: f06, f20, f23 (RH4103), f22 (RH4114), f26 (RH4109). It is a SHARED-FIX-003 run-time change, so nothing visible is in question.
- Not run, for the test implementer or the auditor to settle:
  - R1 and R2 with the captured reference in another document or in a dependent project;
  - R2 for events (RH4102) and for a nested type that hides a base type's nested type;
  - RH4117, RH4120, foreach and catch variables (same symbol kinds as the members run).
