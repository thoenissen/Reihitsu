Role: spec author
Commit: 2bbb7781320afab210a9ec1fce63686f17be5fd9
Verdict: spec written

This is the second repair round, approved by the maintainer (`squad/decisions/2026-10-17-second-repair-round-naming-specs.md`, `00-lead.md` *Escalation 7*). It covers findings 1, 2, 4 and 5 of `14-auditor.md`, plus the spec half of finding 3. I re-derived every finding with my scratch harness outside the repository, which runs the shipped analyzer and fix, applies the single fixes until the diagnostic is gone, and compiles the result. All of them reproduced.

- **Finding 1:**
  - RH4106 `__1st` → `_1st`, `_1_value` → `_1Value`, `注文_明細` → `_注文明細`; each is still reported, with no further fix. `__value` → `_value` and `Order_2` → `_order2` are corrected.
  - RH4002 `_注文` → `注文` and `注文_明細` → `注文明細`; both are still reported. `_2nd` gets no fix.
  - RH4105 `_注文` → `注文`, still reported.
  - RH4005 `_2x` → `II2x` and `注文` → `II注文`, each in two applications.
- **Finding 2:** with `reihitsu.json` = `{ "naming": { "allowedNamespaceDeclarations": [ "A" ] } } garbage`, `namespace Company.Product` is reported. With the file cut off inside its root object (`… [ "A" ] `), nothing is reported.
- **Finding 3:** `class Outer { class order { public int Order; } }` is reported with no fix offered (I verified this in round 1). The RH4001 other-folder side follows from `TargetDocumentAlreadyExists`, which compares `Folders` with `SequenceEqual`. My harness cannot place documents in folders, so the test implementer verifies that side.
- **Finding 4:** a file-level `class order { public int Order; }` gets the fix and fails with CS0542, so "directly in a namespace" includes the global namespace.
- **Finding 5:** the names are taken from the test project. I ran the renamed RH4001 examples:
  - `V2021050108374_Order.cs` with `V2021050108374_Order` is not reported;
  - `order.cs` with `Order` is reported and renamed to `Order.cs`;
  - `Invoice.Part1.cs` with `partial class Order` is renamed to `Order.Part1.cs`.

After the edit I re-ran every example of `shared/casing.md`. All match, except the documented Fix All-only result of SHARED-CASING-010. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 39 passed, 0 failed.

## Changed requirements

Every requirement listed here was added in this run. This round narrows or clarifies them.

- **SHARED-CASING-006 — narrowed (finding 1).**
  - The false sentence "The result is the PascalCase name" is replaced. The joined words give the new name for a PascalCase target; for a camelCase target, the first character is written in lowercase; for `_camelCase`, a `_` is added in front.
  - New coverage sentence: "This requirement covers an old name whose first word, the first non-empty part between underscores, starts with a letter that has both an uppercase and a lowercase form. A first word that starts with a digit, or with a letter without case such as `注`, is not covered."
  - The #971 condition is now "For a camelCase or `_camelCase` target, it further covers only …".
  - These inputs belong to thoenissen/Reihitsu#1016, filed as a bug, and are not filed as open questions.
- **SHARED-CASING-007 — narrowed (findings 1, 4).** Its coverage now starts "covers an old name that SHARED-CASING-006 covers, and a derived name …". Every rule fix requirement inherits that through "This requirement covers the renames that SHARED-CASING-007 covers", RH4005-002 included. The namespace sentence now reads "For a type declared directly in a namespace, including the global namespace (a type declared at file level), …".
- **SHARED-CASING-008 — clarified.** "the name SHARED-CASING-006 derives" became "the name that the steps of SHARED-CASING-006 derive". The withholding conditions keep applying to old names outside 006's coverage: `__`, and `_1st` for RH4105 and RH4106.
- **SHARED-CASING-009 — clarified (findings 3, 4).**
  - New sentence: "A symbol declared directly in a namespace includes a type declared at file level, which belongs to the global namespace; a nested type is not one."
  - New example for the nested side; see below.
- **RH4005-002 — narrowed (finding 1).** Its coverage is now "of an old name whose rest is already in PascalCase or is a name that SHARED-CASING-006 covers". It was "… that contains a letter or a digit".
- **RH4009-001 — narrowed (finding 2).** "when the file is not valid JSON" became "when the file's JSON is malformed before its root value ends".
- **RH4009-002 — narrowed (finding 2).** New sentence: "This requirement covers a configuration file that has no content after its root object." Without it, RH4009-002 would claim the report caused by the garbage-tailed file.
- **RH4001-001, -002, -005, -006 — examples renamed (finding 5).** RH4001-006 also gains the other-folder side (finding 3).

RH4106 is unchanged. The retag table in `06-spec-author.md` still holds.

## Changed examples, exact new text

**SHARED-CASING-009** (`sdd/specs/shared/casing.md`). A new example after the existing RH4002 prose:

Example — RH4002, for a nested type:

```csharp
class Outer
{
    class {|RH4002:order|}
    {
        public int Order;
    }
}
```

is reported, and no code fix is offered, because renaming the class to `Order` would add a compile error inside `Outer`: a member cannot have the name of its enclosing type.

**RH4009-001** (`sdd/specs/analyzer/RH4009.md`). The code block (`namespace Company.Product { }`) is unchanged. The sentence after it now reads:

> is not reported without a configuration file, with `{ "naming": { } }`, with `{ "naming": { "allowedNamespaceDeclarations": [] } }`, with `{ "naming": { "allowedNamespaceDeclarations": [ 1 ] } }`, and with a file whose JSON is malformed inside its root object, such as `{ "naming": { "allowedNamespaceDeclarations": [ "A" ] `. With `{ "naming": { "allowedNamespaceDeclarations": [ "A", 1 ] } }`, the same declaration is reported, because `"A"` stays in effect.

**RH4001-001** (`sdd/specs/analyzer/RH4001.md`). In a file `Outer.cs` (`Second` renamed to `Customer`):

```csharp
public class Outer
{
    public class Inner
    {
    }
}

public class Customer
{
}
```

is not reported. In a file `Invoice.cs` (it was `Mismatched.cs`, `EmptyNamespace`, `PopulatedNamespace`, `LaterType`):

```csharp
namespace Billing
{
}

namespace Sales
{
    internal class {|RH4001:Order|}
    {
    }
}
```

is reported, because `Order` is the first type of the file. In a file `Order.cs` (it was `Test0.cs` and `class Test0`):

```csharp
public delegate void {|RH4001:Handler|}();

public class Order
{
}
```

is reported, because the delegate comes first. A file `Order.cs` that contains only `namespace Sales { }` is not reported (it was `Test0.cs` with `namespace N { }`).

**RH4001-002.** The prose now reads:

> `public class Order { }` is not reported in the files `Order.cs`, `Order.razor.cs`, `Order.xaml.cs`, and `Order.Part1.cs`. `public class InitialCreate { }` is not reported in `20210501083748_InitialCreate.cs`, and `public class V2021050108374_Order { }` is not reported in `V2021050108374_Order.cs`, whose prefix is not all digits.

In a file `order.cs` (it was `testclass.cs`):

```csharp
public class {|RH4001:Order|}
{
}
```

is reported, and so is `public class SomeMigration { }` in `20210501083748_InitialCreate.cs` (unchanged).

**RH4001-003** is unchanged (`Cache`).

**RH4001-005.** In a file `Invoice.cs`:

```csharp
public class {|RH4001:Order|}
{
}
```

the fix renames the file to `Order.cs`. It renames `Invoice.razor.cs` that declares `OrderView` to `OrderView.razor.cs`, `Invoice.Part1.cs` that declares `partial class Order` to `Order.Part1.cs`, and `Invoice.cs` that declares `Order<T>` to `Order{T}.cs`.

**RH4001-006.** The example now reads:

> a project with `Invoice.cs`, which declares `class Order`, and `Order.cs` in the same folder, which declares another `class Order` in another namespace: `Invoice.cs` is reported, and no code fix is offered. The same holds when the other document is named `order.cs`. When `Order.cs` is in another folder of the project, such as `Archive`, the fix is offered and renames `Invoice.cs` to `Order.cs` in its own folder.

## Examples for the tests

- **SHARED-CASING-009:** the nested-type example, with no fix offered.
- **RH4001-006:** the other-folder side, where the fix is offered. Give `Order.cs` the folder `Archive` and leave `Invoice.cs` at the project root; the result is `Order.cs` at the root.
- **RH4001-001, -002, -005, -006:** the existing tests should mirror the new names.
- **RH4009-001:** the cut-off file `{ "naming": { "allowedNamespaceDeclarations": [ "A" ] ` reports nothing. The existing `InvalidConfiguration` test (a missing `]`) still fits the narrowed condition.
- Do not test the inputs of thoenissen/Reihitsu#1016, or content after the root object.

## Open questions to file

One new issue. Everything else stays as listed in `06-spec-author.md` (filed as thoenissen/Reihitsu#1009 to #1014).

**New issue: `SDD open questions: sdd/specs/analyzer/RH0001.md`**

No such issue exists. `sdd/specs/analyzer/RH0001.md` already names this case as open inside a non-goal; that predates this run.

**Q1 — Content after the root object of `reihitsu.json` is ignored, and the configuration stays in effect.**

Input: `reihitsu.json` = `{ "naming": { "allowedNamespaceDeclarations": [ "A" ] } } garbage`, or the same object followed by `{ }`, together with:

```csharp
namespace Company.Product
{
}
```

Real output:

- RH0001 reports nothing for the file.
- RH4009 is active with the list `[ "A" ]` and reports `Company.Product`.
- A file that is malformed inside its root object instead disables RH4009.

Why it looks wrong: the file is not valid JSON, yet it is accepted without a diagnostic. RH4009 then behaves as if the file were valid, while a malformed file elsewhere is treated as no configuration at all.

Touches: RH0001 (the non-goal "Content after the root object"), RH4009-001 and RH4009-002, which now leave this file out.

The inputs of auditor finding 1 are **not** open questions; the maintainer decided them as the bug thoenissen/Reihitsu#1016.

Overlap for the lead: RH4005 Q1 in thoenissen/Reihitsu#1010 asks about `_` and `__`, which need two applications. That shape is the same failure as #1016; whether to cross-link them is the lead's call.

## Follow-up candidates

- thoenissen/Reihitsu#1015 (RH4102 mixed-kind Fix All) and thoenissen/Reihitsu#1016 (fix results still reported) are already filed.
- The correction of "RH4120" in thoenissen/Reihitsu#1009 Q2 is the lead's.
- **Auditor hint (RH4121):** when RH4121 is specified, `CasingUtilities` is already owned by the shared casing spec, so the type-parameter style has to be stated there or in RH4121 without claiming the type.
- **Auditor hint ("in source"):** not verified is whether "in source" in SHARED-CASING-009 includes a project reference in the same solution. The text stays as it is.

## Decisions needed

_None._
