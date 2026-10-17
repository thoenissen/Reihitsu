Role: spec author
Commit: 125901c5f76ad2a75736e4a8efeaa74aeb2eae8b
Verdict: spec written

This round repairs the specs for the skeptic's findings F1–F8 (`03-skeptic.md`). I re-derived every finding myself before changing anything. A scratch file-based harness outside the repository ran each input through the shipped analyzer and code fixes, and compiled the fixed result. Every finding reproduced exactly as the skeptic described, with three exceptions:

- F5 was not run by the skeptic. My metadata-reference fixtures show both reported cases with no fix offered.
- F8c is the same as described, plus the observation below.
- The `[LibraryImport]` sibling in F4 was derived from the code, not run.

The open-question list below replaces the one in `02-spec-author.md` and is complete. No change doc and no rule page change, because intended behavior does not change.

Checks after the repair:

- I re-ran every example of the repaired specs. All reported spans and fix results match. The only exceptions are the documented Fix All-only results (SHARED-CASING-010, RH4106-007) and the RH4102 provider that does not apply to the example's event kind.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 38 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` reports the same 55 uncovered new IDs as before, and nothing else.

## Changed requirements

Every requirement listed here was added in this run. This round narrows or clarifies them, and none changes meaning outside its carve-out.

**`sdd/specs/shared/casing.md`**

- **SHARED-CASING-001, -002, -003 — clarified (F8a).** Each style is now stated for a name whose characters are all letters, digits, or `_`. A combining mark, which is neither a letter nor a digit, is left out of the spec.
- **SHARED-CASING-006 — clarified (F7, F8a).**
  - It covers an old name of letters, digits, and `_`, and splits that name at `_`.
  - Its camelCase/`_camelCase` coverage condition is now "the first word does not start with two uppercase letters, or consists of uppercase letters only", which matches the domain of thoenissen/Reihitsu#971.
  - The `_camelCase` example gains `iPhone` → `_iPhone`.
- **SHARED-CASING-007 — clarified (F1, F3, F8b).**
  - It renames "every reference to it in the active code of the solution".
  - It covers a derived name that is not a C# keyword.
  - For a type declared directly in a namespace, it covers a type none of whose type parameters, and none of whose members other than enum members, already has the new name.
- **SHARED-CASING-008 — clarified (F3).** "Not a valid identifier" is made concrete: the derived name starts with a digit, which is the only invalid shape left within the covered domain.
- **SHARED-CASING-009 — clarified (F1).** The added-compile-error clause now applies only to a symbol that is not declared directly in a namespace. The renamer-conflict clause and the namespace-member clause are unchanged.

**Rule specs**

- **RH4001-001 — clarified (F8c):** it covers a file without top-level statements.
- **RH4001-002 and RH4001-005 — clarified (F6):** both cover only a type name written without the verbatim `@`.
- **RH4005-002 — changed to the current output (F2).**
  - A rest that is already PascalCase is kept as written: `URL` → `IURL`, `DB` → `IDB`.
  - Any other rest is converted as SHARED-CASING-006 derives it: `Document_STORE` → `IDocumentStore`.
  - The example and the `Why:` now show this. I recorded rather than filed it: `IURL` is accepted by RH4005-001 like `IHTTPClient`, and thoenissen/Reihitsu#971 decided that PascalCase targets keep acronyms.
- **RH4009-001 — clarified:**
  - A `naming` section without the list, and a list without string entries (`[ 1 ]`), are inactive.
  - Entries that are not strings are ignored, and the strings stay in effect (`[ "A", 1 ]`).
- **RH4009-002 — clarified (F6):** it covers a name written as identifiers joined by dots, without `@`, whitespace, or comments.
- **RH4102-001 — changed (F5).**
  - Overrides of a source event and implicit implementations of a source interface event are now named as checked, and the example shows them.
  - The requirement covers an event that neither overrides nor implicitly implements an event declared in a referenced assembly.
  - RH4102-003 refers to the events RH4102-001 covers.
- **RH4103-001 — changed (F4, F5).**
  - The blanket `override` carve-out is replaced. Virtual methods, overrides of a source method, and implicit implementations of a source interface method are checked; I ran them, and the fix renames base and override, and interface and implementation, together.
  - The requirement covers a method that neither overrides nor implicitly implements a method of a referenced assembly, and that is neither `extern` nor `[LibraryImport]`.
- **RH4103-003 — changed:** it refers to the methods RH4103-001 covers. A new example shows an override and an implementation renamed with their base.
- **RH4105-001 — clarified:** anonymous methods (`delegate (int Size) { }`) are added to the list and the example.

RH4106 is unchanged in this round. The retag table from `02-spec-author.md` still holds and is repeated here.

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

`02-spec-author.md` (*Examples for the tests*) still holds: which existing tests to tag, and which examples need a new focused test. This round adds these focused tests:

- **SHARED-CASING-006:** `iPhone` → `_iPhone` (RH4106).
- **SHARED-CASING-007:** a `nameof(Value)` use is renamed with the field to `nameof(_value)`. This was observed, but no example is written; it is a test seed for "every reference".
- **RH4005-002:** `URL` → `IURL` and `Document_STORE` → `IDocumentStore`.
- **RH4009-001:** `{ "naming": { } }` and `[ 1 ]` report nothing; `[ "A", 1 ]` reports `TestNameSpace`.
- **RH4102-001:** an override of a source event and an implicit implementation of a source interface event are reported.
- **RH4103-001:** a virtual method, an override, and an implicit implementation are reported.
- **RH4103-003:** an override and an implementation are renamed with their base.
- **RH4105-001:** the anonymous-method parameter `Size`.

Do not write tests for the inputs of the open questions below. The new carve-outs exist so that no requirement covers them.

## Open questions to file

Five new issues, each a sub-issue of thoenissen/Reihitsu#912. None of these specs has an open-question issue yet. Every input below was run against the shipped analyzer and code fix unless it says otherwise.

### Issue `SDD open questions: sdd/specs/shared/casing.md`

**Q1 — A type declared in a namespace is renamed into code that does not compile.**

Input, one fixture each:

```csharp
class order { public int Order; }        // RH4002
class order<Order> { }                   // RH4002
struct order { public int Order; }       // RH4003
delegate void handler<Handler>();        // RH4006
record order(int Order);                 // RH4010
```

Real output: the fix is offered, and renames to `Order` / `Order<Order>` / `Order` / `Handler<Handler>` / `Order`. The result fails with CS0542 ("member names cannot be the same as their enclosing type") or CS0694 ("type parameter has the same name as the containing type"). The same class nested in another class gets no fix (`class Outer { class order { public int Order; } }`).

Why it looks wrong: the fix produces code that does not compile. `AGENTS.md` (*Code-fix coverage*) prefers no fix over an unsafe one. For a symbol declared directly in a namespace, the fix skips the check for added compile errors that withholds it everywhere else.

Reading A: withhold the fix, as for nested types. Reading B: keep the current output. I recommend A.

Touches: SHARED-CASING-007 and SHARED-CASING-009, which cover neither input; RH4002-002, RH4003-002, RH4005-002, RH4006-002, RH4010-002, RH4011-002. RH4005, RH4010 and RH4011 go through the same code path; this was derived from the code, and only the RH4010 case was run.

**Q2 — A camelCase target that is a C# keyword becomes a verbatim identifier.**

Input:

```csharp
class C
{
    void M(int Class, object Object) { }
    int N(string String) { return String.Length; }
}
```

Real output (RH4105): the parameters become `@class`, `@object` and `@string`, and so does the use (`@string.Length`).

Why it looks wrong: the fix writes an `@` the author never wrote, and the name stays a keyword in disguise. A reader would expect either no fix or a different name. Only camelCase targets can reach this, because every keyword is lowercase. That is RH4105 in this batch, and RH4115, RH4117 and RH4120 later.

Touches: SHARED-CASING-007, which now covers only derived names that are not keywords, and SHARED-CASING-008.

**Q3 — A name with a combining mark is reported, and the fix drops the mark.**

Input (RH4002): `class Café { }` with `é` written as `e` followed by U+0301 COMBINING ACUTE ACCENT.

Real output: reported, and the fix renames the class to `Cafe`.

Why it looks wrong: the name is a valid PascalCase name in the author's language. The fix changes its letters, not only their case. A nonspacing mark is neither a letter nor a digit (`char.IsLetterOrDigit`), so the style check rejects it and the word split drops it.

Touches: SHARED-CASING-001, -002, -003 and -006, which now cover only names made of letters, digits, and `_`.

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

Reading A: withhold the fix when the symbol's name appears in inactive code of the document. Reading B: keep the current output, and state that only active code is renamed. I recommend A.

Touches: SHARED-CASING-007, which now says "in the active code of the solution" and does not state what happens to inactive text.

### Issue `SDD open questions: sdd/specs/analyzer/RH4005.md`

**Q1 — An `i`/`I` followed by a non-letter, or a bare `I`, is not taken as the prefix, so the fix doubles it.**

Input:

```csharp
interface i_document { }
interface I_reader { }
interface I { }
interface i2Factory { }
```

Real output: the fix renames them to `IIDocument`, `IIReader`, `II` and `II2Factory`.

Why it looks wrong: the result carries two `I`s. A reader would expect `IDocument`, `IReader` and `I2Factory`. For `I` alone there is no name to keep.

Touches: RH4005-002, whose conditions leave these inputs out.

### Issue `SDD open questions: sdd/specs/analyzer/RH4001.md`

**Q1 — A verbatim type name is compared with its `@`.**

Input: file `class.cs` with `public class @class { }`.

Real output: reported, and the fix renames the file to `@class.cs`.

Why it looks wrong: the type's name is `class`; the `@` only escapes the keyword.

Touches: RH4001-002 and RH4001-005, which now cover only names written without `@`.

**Q2 — The fix drops the timestamp prefix of a migration file.**

Input: file `20210501083748_InitialCreate.cs` with `public class SomeMigration { }`.

Real output: the fix renames the file to `SomeMigration.cs`.

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

Why it looks wrong: the file is the program's entry point, not the home of `Helper`. `Program.cs` is the conventional name of such a file.

Touches: RH4001-001, which now covers only files without top-level statements.

### Issue `SDD open questions: sdd/specs/analyzer/RH4009.md`

**Q1 — A nested namespace declaration is compared by its own name only.**

Input: `namespace A { namespace B { } }` with `"allowedNamespaceDeclarations": [ "A", "A.B" ]`.

Real output: `B` is reported, although the namespace it declares, `A.B`, is allowed. With `[ "A", "B" ]`, nothing is reported.

Why it looks wrong: the rule page says the comparison uses "the full declared namespace".

Touches: RH4009-002, which covers only declarations at file level.

**Q2 — The name is compared as written.**

Input:

- `namespace A . B { }` with `[ "A.B" ]`;
- `namespace @MyCompany.Project { }` with `[ "MyCompany.Project" ]`.

Real output: `A . B` and `@MyCompany.Project` are reported. The comparison includes the whitespace or comments between the tokens and the verbatim `@`.

Why it looks wrong: both declare exactly the allowed namespace.

Touches: RH4009-002, which now covers only names written without `@`, whitespace, or comments.

### Issue `SDD open questions: sdd/specs/analyzer/RH4103.md`

**Q1 — A method whose name is given by a referenced assembly is reported, and the diagnostic cannot be fixed.**

Input, where a referenced assembly declares:

```csharp
public class Base { public virtual void process() { } }
public interface ILib { void process(); }
```

and the source has `public class Derived : Base { public override void process() { } }` and `public class Impl : ILib { public void process() { } }`.

Real output: both `process` methods are reported, and no code fix is offered.

Why it looks wrong: the name is dictated by a type the author does not control. An explicit interface implementation is excluded for the same reason. The same input declared in source is reported and fixed together with its base, which is covered by RH4103-001 and -003.

Touches: RH4103-001 and RH4103-003, which now cover only methods that neither override nor implicitly implement a method of a referenced assembly.

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

Why it looks wrong: the name is dictated by the native library, and the fix breaks the program at run time. With `EntryPoint = "getppid"` set, the rename is safe. The same is expected for a `[LibraryImport]` method without `EntryPoint`, whose source generator uses the method name; this was derived from the code, not run.

Touches: RH4103-001 and RH4103-003, which now cover only methods that are neither `extern` nor `[LibraryImport]`.

### Issue `SDD open questions: sdd/specs/analyzer/RH4102.md`

**Q1 — An event whose name is given by a referenced assembly is reported, and the diagnostic cannot be fixed.**

Input, where a referenced assembly declares:

```csharp
public abstract class BaseSource { public abstract event EventHandler changed; }
public interface ISource { event EventHandler opened; }
```

and the source has:

```csharp
public class Derived : BaseSource, ISource
{
    public override event EventHandler changed;
    public event EventHandler opened;
}
```

Real output: both events are reported, and no code fix is offered.

Why it looks wrong: the names are dictated by types the author does not control. An explicit interface implementation is excluded for the same reason. The same input declared in source is reported and fixed together with its base, which is covered by RH4102-001 and -003.

Touches: RH4102-001 and RH4102-003, which now cover only events that neither override nor implicitly implement an event of a referenced assembly.

## Follow-up candidates

These are bugs against an existing spec. Filing them needs the maintainer's approval.

- **RH4102 Fix All with both event kinds violates SHARED-FIX-002.** In a document with a reported field-like event (`public event EventHandler dataChanged;`) and a reported event with accessors (`public event EventHandler saved { add { } remove { } }`), Fix All corrects only the events of the kind it was invoked from:
  - `RH4102EventNameCasingCodeFixProvider` leaves `saved`.
  - `RH4102CustomEventNameCasingCodeFixProvider` leaves `dataChanged`.
  - The single fixes correct both.
  - The skeptic confirmed the mechanism: `GetFixAllCandidatesAsync` keeps only nodes of the provider's own node type.
  - The RH4102 spec states no deviation and shows each kind in a separate example. No test may assert a mixed-kind Fix All until this is decided.

Shared Q1 is not against an existing spec: the namespace-type path was never specified before this run, and RH4106-006 on `main` covered only fields. So it is filed as an open question. If the maintainer prefers to decide it directly as a bug under the `AGENTS.md` safety policy, that needs the same approval.

`scripts/lane-check.sh` classifying `squad/decisions/` as tooling remains a tooling follow-up (lead, `00-lead.md`).

## Decisions needed

_None._
