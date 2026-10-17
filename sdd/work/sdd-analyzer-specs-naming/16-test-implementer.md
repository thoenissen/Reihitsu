Role: test implementer
Commit: 93b1d59b70488aa3f74348a62da8dfefaf67b26b
Verdict: tests written

This is the second repair round, approved in `squad/decisions/2026-10-17-second-repair-round-naming-specs.md`. My inputs were findings 3 and 5 of `14-auditor.md` and the changed examples listed in `15-spec-author.md`, checked against `git diff 472d5a2..HEAD -- sdd/specs`. The coordinator's message carried no intended fix or hoped-for verdict.

No production code or spec changed. Every test passes on the unchanged code. No test asserts anything about an input of thoenissen/Reihitsu#1016, or about content after the root object of `reihitsu.json`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4001 `FixIsOfferedWhenDocumentWithTheNewFileNameIsInAnotherFolder` (new) | RH4001-006 (other-folder side, finding 3), RH4001-005; FC-003 | `Verify(source, null, …)`: `/0/Invoice.cs` and `/0/Archive/Order.cs`; fixed state `/0/Archive/Order.cs` and `/0/Order.cs` | pass |
| RH4002 `NoFixIsOfferedWhenRenamingANestedClassAddsACompileErrorInItsContainingType` (new) | SHARED-CASING-009 (nested-type example, finding 3); FC-003 | `Verify(source, source)` | pass |
| RH4009 `ConfigurationMalformedInsideItsRootObjectIsInactive` (new) | RH4009-001 (the new example `{ "naming": { "allowedNamespaceDeclarations": [ "A" ]`) | `reihitsu.json` additional file | pass |
| RH4001 `NestedAndLaterTypesAreNotComparedWithTheFileName` | RH4001-001 — `Second` → `Customer` | file `Outer.cs` | pass |
| RH4001 `LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed` | RH4001-001, -005 — `Invoice.cs` with `Billing`/`Sales`/`Order`, renamed to `Order.cs` | `Verify(source, null, …)` | pass |
| RH4001 `DelegateDeclaredBeforeTheMatchingTypeIsComparedWithTheFileName` | RH4001-001, -004 — file `Order.cs` with `class Order` | file set explicitly | pass |
| RH4001 `FileWithoutTypeIsNotReported` | RH4001-001 — `Order.cs` with `namespace Sales { }` | file set explicitly | pass |
| RH4001 `TypeNameMatch`, `RazorCodeBehindMatch`, `XamlCodeBehindMatch`, `SplitPartialFileMatch` | RH4001-002 — `public class Order` in `Order.cs`, `Order.razor.cs`, `Order.xaml.cs`, `Order.Part1.cs` | file set explicitly | pass |
| RH4001 `NonDigitPrefixWithUnderscoreIsNotStripped` | RH4001-002 — `V2021050108374_Order` in `V2021050108374_Order.cs` | same | pass |
| RH4001 `TypeNameIsComparedWithTheFileNameCaseSensitively` | RH4001-002 — `Order` in `order.cs` | same | pass |
| RH4001 `CodeFixRenamesFile`, `CodeFixRenamesFileForGenericType`, `CodeFixRenamesRazorFile`, `CodeFixPreservesDottedFileSuffix` | RH4001-005 (and -003 for the generic one). The examples run: `Invoice.cs` → `Order.cs`; `Invoice.cs` with `Order<T>` → `Order{T}.cs`; `Invoice.razor.cs` with `OrderView` → `OrderView.razor.cs`; `Invoice.Part1.cs` with `partial class Order` → `Order.Part1.cs` | `Verify(source, null, …)` | pass |
| RH4001 `NoFixIsOfferedWhenDocumentWithTheNewFileNameExists`, `…InOtherCasingExists`, `CodeFixNotOfferedWhenTargetFileAlreadyExists` | RH4001-006 — `Invoice.cs` (`namespace Sales`, `class Order`) next to `Order.cs` or `order.cs` (`namespace Archive`) | `Verify(source, null, …)` / `AdhocWorkspace` | pass |

Notes on these tests:

- **The RH4001 rewrites (finding 5).** The rewritten tests are the ones that run the RH4001 examples word for word. Several of them predate this run, but each is the covering test I tagged for its example in `07-test-implementer.md`. Their requirement tags did not change.
- **The `order.cs` variant.** In `…InOtherCasingExists`, `order.cs` declares `class order`, so that this document is not reported itself. The spec text says only that "the other document is named `order.cs`" and gives no code for it.
- **The other-folder test can fail.** I gave the renamed document the expected path `/0/Other/Order.cs`. The verifier then failed with "folders was expected to be 'Other' but was ''". So the test asserts that the file is renamed in its own folder. The same-folder tests show that the fix is withheld when the other document is in the same folder.
- **Order of documents in the expected state.** The verifier matches the documents of the expected state by their order, and the renamed document moves to the end. So the fixed state lists `/0/Archive/Order.cs` first.
- **Tests left as they were.** The tests whose example did not change keep their names, and so do the tests that run no example. These are `TypeNameMismatch`, the kind-mismatch tests, `RazorCodeBehindMismatch`, and `TimestampPrefixedFileMatch`/`…Mismatch` (`InitialCreate` and `SomeMigration` are unchanged in the spec).

## Tags whose coverage changed

I checked every tagged test against the narrowed requirements:

- **SHARED-CASING-006 and -007:** no first word starting with a digit or a caseless letter.
- **RH4005-002:** a rest that is already in PascalCase, or a name that SHARED-CASING-006 covers.
- **RH4009-001:** malformed before the root value ends.
- **RH4009-002:** no content after the root object.

No tagged test has an input outside its narrowed requirement. `rg` finds no `__1st`, `_1_`, `_2x`, `_2nd`, or caseless letter in the test sources. So no tag had to move:

- `_1st` and `__` are tagged SHARED-CASING-008 only, which still covers them.
- `InvalidConfiguration` is a missing `]` inside the root object, which still fits RH4009-001.

## Not covered, and why

- **The inputs of thoenissen/Reihitsu#1016:** filed as a bug, and no requirement covers them.
- **Content after the root object of `reihitsu.json`:** it goes to the new RH0001 open question, and RH4009-001 and -002 leave it out.
- **Finding 4** (the global namespace): it changes wording only. The cross-document tests (`TopLevelType*` in RH4115, tagged SHARED-CASING-009) and the RH4002 namespace tests already run file-level types.

## Commands

```shell
scripts/format.sh src/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Naming/RH4002ClassNameCasingAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Naming/RH4009NamespaceNotAllowedAnalyzerTests.cs
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

Results on this head:

- `scripts/format.sh`: `Formatted 0 of 3 file(s)`.
- Naming tests: 405 passed.
- `Sdd` architecture tests: 39 passed.
