Role: spec author
Commit: f9726ba9e8150d73c994f6a334c701dc539cffa1
Verdict: spec written

This round repairs R1–R4 from `05-skeptic.md` and acts on its two smaller points. This handover supersedes the open-question lists of `02-spec-author.md` and `04-spec-author.md`. The list below is complete and final.

Before changing anything, I re-derived every item myself with my scratch harness (outside the repository). The harness runs the shipped analyzer and code fix, applies the single fixes until the diagnostic is gone, and compiles the result. It can also add a referenced-assembly project.

The repair changes requirement text only; no C# example block changed (`git diff 747b31a -- sdd/specs` touches no code fence). No change doc and no rule page change. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 38 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` reports the same 55 uncovered new IDs as before, which are for the test implementer.

## What I re-derived

- **R1** is confirmed by reading the specs. The seven listed fix requirements stated an unrestricted MUST.
- **R2** is confirmed by running RH4105:
  - `int Await` in an async method → `@await`, at the declaration and at the use;
  - `int Await` in a method that is not async → `await`;
  - `int Var` → `var`;
  - `int Nameof` → `nameof`.
- **R3** is confirmed by running RH4005. `interface _` → `II`, and `interface __` → `I__`, which is still reported. On its own, `I__` → `II`, so the fix needs two applications.
- **R4** is confirmed by running RH4001. File `Cache{TKey}.cs` with `public class Cache<@TKey> { }` is reported, and the fix renames the file to `Cache{@TKey}.cs`.
- **Smaller point 1** is confirmed by running RH4002. `class ClassⅫ` (U+216B ROMAN NUMERAL TWELVE, category Nl) is reported, and the fix renames it to `class Class`.
- **Smaller point 2** (transitive override) is verified with a referenced assembly. The result is worse than "ambiguous":
  - A referenced assembly declares `public class Base { public virtual void process() { } }`.
  - In source, `Middle : Base` overrides `process`, and `Leaf : Middle` overrides it again.
  - `Middle.process` is reported, and no fix is offered.
  - `Leaf.process` is reported, and the fix is offered. It renames `Leaf.process` and `Middle.process` to `Process`, and the result fails with CS0115 ("no suitable method found to override").
  - The same holds for events: a `Middle`/`Leaf` chain over `abstract event EventHandler changed` of a referenced class leads to CS0115 and CS0534.

## Changed requirements

All of these were added in this run; the repair narrows or clarifies them.

**R1 — the rule fix requirements now inherit the coverage of SHARED-CASING-007.**

- Every casing rule's fix requirement now ends with "This requirement covers the renames that SHARED-CASING-007 covers." These are RH4002-002, RH4003-002, RH4004-002, RH4005-002, RH4006-002, RH4010-002, RH4011-002, RH4101-002, RH4102-003, RH4103-003, RH4104-002, RH4105-003, and RH4107-002 to RH4110-002.
- I added the sentence to all of them, not only the seven the skeptic named, so that every casing fix requirement reads the same.
- RH4106-004 is left unchanged: a `_camelCase` name can be neither a keyword nor a type name.

**R2 — SHARED-CASING-007.** The keyword sentence now describes the outcome. It covers a derived name that is written without the verbatim prefix `@` at the declaration and at every reference: a name that is not a reserved C# keyword, and not `await` inside an async member. Contextual keywords such as `value`, `var`, and `nameof` are covered and stay as they are.

**R3 — RH4005-002** now covers only old names that contain a letter or a digit.

**R4 — RH4001-003** now covers a generic type whose type parameters are written without `@`.

**Smaller point 1 — SHARED-CASING-001, -002, -003 and -006.**

- The covered domain is now names whose characters are all letters of the Unicode categories Lu, Ll, Lm, or Lo, decimal digits, or `_`. Letter numbers (Nl), combining marks, and titlecase letters (Lt) are outside it.
- "Uppercase" is category Lu and "lowercase" is category Ll, which matches `char.IsUpper` and `char.IsLower`.

**Smaller point 2 — RH4102-001 and RH4103-001.**

- The coverage now reads "whose original definition is declared in source": following the members it overrides, through every level, and the interface members it implicitly implements never leads to a member of a referenced assembly.
- The normative sentence names "overrides and implicit interface implementations whose original definition is declared in source".
- The source-only examples remain valid.

RH4106 is unchanged in this round. The retag table:

| ID | Old meaning | New meaning | Tagging consequence |
|---|---|---|---|
| RH4106-001 | Private fields are checked, including the accessibility classification and the `const` exclusion | Non-`const` fields of the private group (SHARED-CASING-004) are checked, static and readonly included | Scope tests keep the tag. The tests for implicitly private fields and for `private protected` also cover SHARED-CASING-004 |
| RH4106-002 | Definition of `_camelCase` | A checked field is reported when its name is not `_camelCase`; the definition is now SHARED-CASING-003 | Tests keep the tag. `VerifyNoDiagnosticsForUnderlineCamelCasePrivateField` also covers SHARED-CASING-003 |
| RH4106-003 | One diagnostic per declarator, on its identifier | Unchanged | No change |
| RH4106-004 | The word-splitting algorithm of the fix | The fix renames to the `_camelCase` name; the algorithm is now SHARED-CASING-006 | Tests keep the tag. Tests that exercise `_` separators or all-uppercase words cover SHARED-CASING-006 |
| RH4106-005 | Rename through the Roslyn renamer; not offered when the symbol is unresolved | The fix renames the field and every reference as SHARED-CASING-007 specifies | Test keeps the tag, and may also tag SHARED-CASING-007 |
| RH4106-006 | Full list of the withholding conditions | The fix is withheld as SHARED-CASING-008 and -009 specify | `NoCodeFixForLetterlessPrivateField` keeps the tag and also covers SHARED-CASING-008 |
| RH4106-007 | The full Fix All collision rule | Fix All skips colliding fields as SHARED-CASING-010 specifies | The `FixAllCorrectsEveryDiagnostic` override keeps the tag and also covers SHARED-CASING-010 |

## Examples for the tests

The lists in `02-spec-author.md` and `04-spec-author.md` (*Examples for the tests*) still hold; this round changed no example. Contextual-keyword targets are now explicitly covered by SHARED-CASING-007, so these may be added as test seeds for it and for RH4105-003:

- `Value` → `value` (already an RH4105-003 example);
- `Var` → `var`;
- `Nameof` → `nameof`;
- `Await` in a method that is not async → `await`.

Do not write tests for any input of the open questions below.

## Open questions to file

These are six new issues, each a sub-issue of thoenissen/Reihitsu#912. None of these specs has an open-question issue yet. Every input was run against the shipped analyzer and code fix unless it says otherwise.

### 1. `sdd/specs/shared/casing.md` — issue `SDD open questions: sdd/specs/shared/casing.md`

**Q1 — A type declared in a namespace is renamed into code that does not compile.**

Input, one fixture each:

```csharp
class order { public int Order; }        // RH4002
class order<Order> { }                   // RH4002
struct order { public int Order; }       // RH4003
delegate void handler<Handler>();        // RH4006
record order(int Order);                 // RH4010
```

Real output: the fix is offered and renames to `Order`, `Order<Order>`, `Order`, `Handler<Handler>` and `Order`. The result fails with CS0542 ("member names cannot be the same as their enclosing type") or CS0694 ("type parameter has the same name as the containing type"). The same class nested in another class (`class Outer { class order { public int Order; } }`) gets no fix. An enum with a member of the new name (`enum color { Color }` → `enum Color { Color }`) compiles.

Why it looks wrong: the fix produces code that does not compile, while `AGENTS.md` (*Code-fix coverage*) prefers no fix over an unsafe one. For a symbol declared directly in a namespace, the check for added compile errors, which withholds the fix everywhere else, is skipped.

Reading A: withhold the fix, as for nested types. Reading B: keep the current output. Recommendation: A.

Touches: SHARED-CASING-007 and SHARED-CASING-009, which now cover neither input; RH4002-002, RH4003-002, RH4005-002, RH4006-002, RH4010-002 and RH4011-002, which inherit that coverage. The RH4005 and RH4011 cases go through the same code path; that is derived from the code, not run.

**Q2 — A camelCase target that has to be written as a verbatim identifier gets an `@` the author never wrote.**

Input (RH4105):

```csharp
using System.Threading.Tasks;

class C
{
    void M(int Class, object Object) { }
    int N(string String) { return String.Length; }
    async Task<int> O(int Await) { await Task.Yield(); return Await; }
}
```

Real output: the parameters become `@class`, `@object`, `@string` and `@await`, and so do their uses (`@string.Length`, `return @await;`). Contextual keywords are renamed without `@`: `Var` → `var`, `Nameof` → `nameof`, and `Await` → `await` in a method that is not async.

Why it looks wrong: the fix writes an escape the author never wrote, and the new name stays a keyword in disguise. A reader would expect either no fix or a name that needs no escape. Only camelCase targets can reach this, because every keyword is lowercase: RH4105 in this batch, and RH4115, RH4117 and RH4120 later.

Touches: SHARED-CASING-007, which now covers only derived names written without `@`; SHARED-CASING-008; RH4105-003.

**Q3 — A name with a character outside letters, digits, and `_` is reported, and the fix drops that character.**

Input (RH4002):

- `class Café { }`, with `é` written as `e` followed by U+0301 COMBINING ACUTE ACCENT (category Mn);
- `class ClassⅫ { }`, with U+216B ROMAN NUMERAL TWELVE (category Nl).

Real output: both are reported. The fix renames them to `Cafe` and `Class`.

Why it looks wrong: both are valid C# identifiers, which the C# grammar builds from letters including letter numbers, combining marks, and digits. The style check rejects them because `char.IsLetterOrDigit` is false for those characters. The fix then drops the characters, so it changes the letters of the name, not only their case.

Touches: SHARED-CASING-001, -002, -003 and -006, which now cover only names of letters of categories Lu, Ll, Lm, or Lo, decimal digits, and `_`.

**Q4 — A reference in an inactive `#if` branch is not renamed.**

Input (RH4106):

```csharp
class C
{
    private int Value;

    int M()
    {
#if NEVER
        return Value + 1;
#else
        return Value;
#endif
    }
}
```

Real output: the field and the active reference become `_value`. The line under `#if NEVER` keeps `Value`, so the code no longer compiles once `NEVER` is defined.

Why it looks wrong: the fix breaks another build configuration without any sign. FC-020 counts inactive code as code an edit must not leave inconsistent.

Reading A: withhold the fix when the name appears in inactive code of the document. Reading B: keep the current output, and state that only active code is renamed. Recommendation: A.

Touches: SHARED-CASING-007, which now says "in the active code of the solution" and states nothing about inactive text.

### 2. `sdd/specs/analyzer/RH4005.md` — issue `SDD open questions: sdd/specs/analyzer/RH4005.md`

**Q1 — Without a usable prefix, the fix doubles the `I` or needs two applications.**

Input:

```csharp
interface i_document { }
interface I_reader { }
interface I { }
interface i2Factory { }
interface _ { }
interface __ { }
```

Real output:

- The fix renames the first four to `IIDocument`, `IIReader`, `II` and `II2Factory`.
- `_` becomes `I_` and is still reported; a second application makes it `II`.
- `__` becomes `I__` and is still reported; a second application makes it `II`.

Why it looks wrong: the result carries two `I`s; a reader would expect `IDocument`, `IReader` and `I2Factory`. `I` alone, `_` and `__` have no name to keep. For `_` and `__`, the fix also contradicts SHARED-FIX-001, because one application does not correct the diagnostic. SHARED-CASING-008 would withhold a fix for an old name without letters, but the prefix the fix puts in front makes the new name differ from the old one.

Touches: RH4005-002, whose conditions now leave all these inputs out.

### 3. `sdd/specs/analyzer/RH4001.md` — issue `SDD open questions: sdd/specs/analyzer/RH4001.md`

**Q1 — A verbatim name is compared with its `@`.**

Input:

- file `class.cs` with `public class @class { }`;
- file `Cache{TKey}.cs` with `public class Cache<@TKey> { }`.

Real output: both are reported. The fix renames the files to `@class.cs` and `Cache{@TKey}.cs`.

Why it looks wrong: the names are `class` and `TKey`; the `@` only escapes a keyword, or nothing at all.

Touches: RH4001-002, RH4001-003 and RH4001-005, which now cover only names written without `@`.

**Q2 — The fix drops the timestamp prefix of a migration file.**

Input: file `20210501083748_InitialCreate.cs` with `public class SomeMigration { }`.

Real output: reported, and the fix renames the file to `SomeMigration.cs`.

Why it looks wrong: the analyzer ignores the prefix because Entity Framework Core orders migrations by it, but the fix removes it. `20210501083748_SomeMigration.cs` would keep the order.

Touches: RH4001-005, which covers only file names without the prefix.

**Q3 — A file of top-level statements is compared with the type that follows them.**

Input: file `Program.cs`:

```csharp
System.Console.WriteLine(new Helper());

internal class Helper
{
}
```

Real output: `Helper` is reported, and the fix renames the file to `Helper.cs`.

Why it looks wrong: the file is the program's entry point, not the home of `Helper`, and `Program.cs` is the conventional name of such a file.

Touches: RH4001-001, which now covers only files without top-level statements.

### 4. `sdd/specs/analyzer/RH4009.md` — issue `SDD open questions: sdd/specs/analyzer/RH4009.md`

**Q1 — A nested namespace declaration is compared by its own name only.**

Input: `namespace A { namespace B { } }` with `{ "naming": { "allowedNamespaceDeclarations": [ "A", "A.B" ] } }`.

Real output: `B` is reported, although the namespace it declares, `A.B`, is allowed. With `[ "A", "B" ]`, nothing is reported.

Why it looks wrong: the rule page says the comparison uses "the full declared namespace".

Touches: RH4009-002, which covers only declarations at file level.

**Q2 — The name is compared as written.**

Input:

- `namespace A . B { }` with `[ "A.B" ]`;
- `namespace @MyCompany.Project { }` with `[ "MyCompany.Project" ]`.

Real output: `A . B` and `@MyCompany.Project` are reported. The comparison includes the whitespace or comments between the tokens and the verbatim `@`.

Why it looks wrong: both declarations declare exactly the allowed namespace.

Touches: RH4009-002, which now covers only names written without `@`, whitespace, or comments.

### 5. `sdd/specs/analyzer/RH4103.md` — issue `SDD open questions: sdd/specs/analyzer/RH4103.md`

**Q1 — A method whose original definition is in a referenced assembly is reported, and the fix is either missing or breaks the build.**

Input: a referenced assembly declares:

```csharp
public class Base { public virtual void process() { } }
public interface ILib { void process(); }
```

The source declares:

```csharp
public class Impl : ILib { public void process() { } }
public class Middle : Base { public override void process() { } }
public class Leaf : Middle { public override void process() { } }
```

Real output:

- All three `process` methods are reported.
- `Impl.process` and `Middle.process` get no code fix.
- `Leaf.process` gets a fix that renames `Leaf.process` and `Middle.process` to `Process`. The result fails with CS0115 ("'Middle.Process()': no suitable method found to override").

Why it looks wrong: the name is dictated by a type the author does not control. An explicit interface implementation is excluded for the same reason. The fix for `Leaf` also produces code that does not compile. The same chain declared entirely in source is reported and fixed together with its base, which RH4103-001 and -003 cover.

Touches: RH4103-001 and RH4103-003, which now cover only methods whose original definition is declared in source.

**Q2 — The fix renames a P/Invoke method whose entry point is its name.**

Input:

```csharp
using System.Runtime.InteropServices;

internal static class Native
{
    [DllImport("libc")]
    internal static extern int getpid();

    internal static int Run() => getpid();
}
```

Real output: reported, and the fix renames the method and the call to `Getpid`. The code compiles, but the runtime binds a `DllImport` without `EntryPoint` by the method name, so the call fails with an entry-point-not-found error.

Why it looks wrong: the name is dictated by the native library, and the fix breaks the program at run time. With `EntryPoint = "getppid"` set, the rename is safe. The same is expected for a `[LibraryImport]` method without `EntryPoint`, whose source generator uses the method name; that is derived from the code, not run.

Touches: RH4103-001 and RH4103-003, which now cover only methods that are neither `extern` nor `[LibraryImport]`.

### 6. `sdd/specs/analyzer/RH4102.md` — issue `SDD open questions: sdd/specs/analyzer/RH4102.md`

**Q1 — An event whose original definition is in a referenced assembly is reported, and the fix is either missing or breaks the build.**

Input: a referenced assembly declares:

```csharp
using System;

public abstract class BaseSource { public abstract event EventHandler changed; }
public interface ISource { event EventHandler opened; }
```

The source declares:

```csharp
using System;

public class Derived : BaseSource, ISource
{
    public override event EventHandler changed;
    public event EventHandler opened;
}

public class Middle : BaseSource { public override event EventHandler changed; }
public class Leaf : Middle { public override event EventHandler changed; }
```

The `Middle`/`Leaf` chain was run as its own fixture.

Real output:

- Every event is reported.
- `Derived.changed`, `Derived.opened` and `Middle.changed` get no code fix.
- `Leaf.changed` gets a fix that renames `Leaf.changed` and `Middle.changed` to `Changed`. The result fails with CS0115 and CS0534 ("'Middle' does not implement inherited abstract member 'BaseSource.changed.add'").

Why it looks wrong: the names are dictated by types the author does not control. An explicit interface implementation is excluded for the same reason. The fix for `Leaf` also produces code that does not compile. The same events declared in source are reported and fixed together with their base, which RH4102-001 and -003 cover.

Touches: RH4102-001 and RH4102-003, which now cover only events whose original definition is declared in source.

## Follow-up candidates

These are bugs against an existing spec. Filing them needs the maintainer's approval.

- **RH4102 Fix All with both event kinds violates SHARED-FIX-002.** In a document with a reported field-like event (`public event EventHandler dataChanged;`) and a reported event with accessors (`public event EventHandler saved { add { } remove { } }`), Fix All corrects only the events of the kind it was invoked from:
  - `RH4102EventNameCasingCodeFixProvider` leaves `saved`.
  - `RH4102CustomEventNameCasingCodeFixProvider` leaves `dataChanged`.
  - The single fixes correct both.
  - `GetFixAllCandidatesAsync` keeps only nodes of the provider's own node type.
  - The RH4102 spec states no deviation and shows each kind in a separate example. No test may assert a mixed-kind Fix All until this is decided.

Not bugs against an existing spec, so filed as open questions above:

- Shared Q1. The namespace-type path was never specified before this run, and RH4106-006 on `main` covered only fields.
- The fixes that do not compile in RH4103 Q1 and RH4102 Q1.

If the maintainer prefers to decide any of them directly as a bug under the `AGENTS.md` safety policy ("prefer no fix over an unsafe one"), that needs the same approval.

`scripts/lane-check.sh` classifying `squad/decisions/` as tooling remains a tooling follow-up (lead, `00-lead.md`).

## Decisions needed

_None._
