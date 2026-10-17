Role: spec author
Commit: 6fd360cf557b933ec0dbe056c9daf05826c045d8
Verdict: text written

## Finding

`09-spec-checker.md` finding 1 holds. `sdd/README.md` (*Spec format*) requires examples to be minimal and neutral, with no names taken from test projects. Two examples break that:

- **RH4008-001** used `Reihitsu.analyzer.Naming.resources`, the repository's own namespace.
- **RH4009-001** used `TestNameSpace`, a name taken from the test class `RH4009NamespaceNotAllowedAnalyzerTests`.

In both specs I replaced the names with the neutral `Company.Product` family that RH4007-001 and the shared spec already use. Only the names changed: no requirement sentence changed, and no other example changed.

## Changed examples

**RH4008-001** (`sdd/specs/analyzer/RH4008.md`). The first declaration of the example block was

```csharp
namespace Reihitsu.{|RH4008:analyzer|}.Naming.{|RH4008:resources|}
{
}
```

and is now

```csharp
namespace Company.{|RH4008:product|}.Data.{|RH4008:access|}
{
}
```

The second declaration, `namespace Company.Product { }`, is unchanged. So the whole block now reads:

```csharp
namespace Company.{|RH4008:product|}.Data.{|RH4008:access|}
{
}

namespace Company.Product
{
}
```

**RH4009-001** (`sdd/specs/analyzer/RH4009.md`). The example block was `namespace TestNameSpace { }` and is now:

```csharp
namespace Company.Product
{
}
```

The prose after the block is unchanged. Each configuration it names still applies to this declaration:

- no configuration file, `{ "naming": { } }`, `[]`, `[ 1 ]`, and malformed JSON: not reported;
- `[ "A", 1 ]`: reported, because `"A"` stays in effect.

## Verification

I ran both new blocks through my scratch harness outside the repository, which runs the shipped analyzer:

- **RH4008:** two diagnostics, on `product` (1:19) and on `access` (1:32), and no code fix. `Company.Product` is not reported.
- **RH4009:** with `[ "A", 1 ]`, one diagnostic on the whole name `Company.Product` (1:11).

## Tests that run these examples word for word

These tests still use the old names. Tests are not mine to edit; the test implementer decides whether to make them mirror the new text.

- **RH4008:** `RH4008NamespaceCasingAnalyzerTests.EachBlockScopedNamespaceSegmentThatIsNotPascalCaseIsReported` still uses `namespace Reihitsu.{|RH4008:analyzer|}.Naming.{|RH4008:resources|}`.
- **RH4009:** three tests still use `namespace TestNameSpace`:
  - `NamingSectionWithoutAllowListIsInactive`
  - `AllowListWithoutStringEntryIsInactive`
  - `EntriesThatAreNotStringsAreIgnoredAndStringEntriesStayInEffect`

  The older tests use the same name through the `TestCode` constant and its configuration entries (`ValidNamespace`, `InvalidConfiguration`).

## Changed requirements

None. RH4008-001 and RH4009-001 keep their meaning; only their examples changed.

## Examples for the tests

The two examples above.

## Open questions to file

_None._ This replaces nothing: the final list is still the one in `06-spec-author.md`.

## Follow-up candidates

The RH4001 examples use the test harness's default file name `Test0.cs` and the names `TestClass` and `TestComponent`. The spec checker did not flag them. They are file names that the rule's behavior depends on, and the covering tests are bound to the verifier's `/0/Test0.cs`. I left them as they are; the lead can decide whether they need the same treatment.

## Decisions needed

_None._
