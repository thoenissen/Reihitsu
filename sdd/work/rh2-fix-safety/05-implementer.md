Role: implementer
Commit: bd592fd
Verdict: #967: test disputed; #1286: test disputed; #1287: tests pass; #1288: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the reference to `00-lead.md` decision 5, which is a recorded lead decision and was followed. No test was changed. On `bd592fd` the analyzer project gives 3 failed and 7100 passed. All 3 failures are tests whose harness configuration is wrong (see *Disputed tests or scope* under #967 and #1286). Each one passes when only that harness setting is added locally; the edit was reverted, and nothing of it was committed.

## thoenissen/Reihitsu#967

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | `AddAccessibilityModifiers(MemberDeclarationSyntax, …)` now takes the leading trivia from the first token behind the attribute lists (the first modifier or, without modifiers, the token that starts the rest of the declaration), not from the declaration's first token. It puts that trivia on the new first token behind the attribute lists. Without attribute lists this is the same token as before, so doc comments still stay in front. New private `GetFirstTokenBehindAttributeLists`. Both overloads' remarks were rewritten. | RH2004-006, RH2005-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2005FieldsMustBePrivateCodeFixProvider.cs` | The fix is not registered when any field the declaration declares is referenced outside the bodies of the containing type's declarations. Active references come from `SymbolFinder.FindReferencesAsync` across the solution. A position counts as inside when it lies between the braces of a `TypeDeclarationSyntax` of the containing type, so nested types and other parts count as inside. A field name in excluded `#if` code outside those bodies also withholds the fix, through the shared `ExcludedCodeUtilities`. New `IsReferencedOutsideContainingTypeAsync` and `IsInsideBody`. | RH2005-004, SHARED-FIX-003 |

### Policy owners

- `rg -n "AddAccessibilityModifiers?\(" src --glob '*.cs'`: the one owner of the inserted-modifier trivia is `DeclarationModifierUtilities`. Its production callers are only the RH2004 and RH2005 fix providers. No private copy exists.
- `rg -n "FindReferencesAsync" src/Reihitsu.Analyzer.CodeFixes`: the casing base (rename cascade) and now RH2005. They make different decisions, so neither copies the other.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2005FieldsMustBePrivate|FullyQualifiedName~RH2004AccessModifier"`: every test passes except `VerifyFixAllSkipsTheFieldReadOutsideItsType` (disputed below).
- `scripts/test.sh --configuration Debug --project core`: 649 passed (includes `DeclarationModifierUtilitiesTests`).

### Disputed tests or scope

- **`RH2005FieldsMustBePrivateAnalyzerTests.VerifyFixAllSkipsTheFieldReadOutsideItsType`**
  - What it asserts: Fix All makes `First` and `Second` private and leaves `Used` reported. The remaining diagnostic is written as `{|RH2005:Used|}` markup in the fixed code.
  - What happens: the test fails with "Diagnostics of fixed state: expected 0, actual 1". The code output matches the expected text.
  - Why: `Microsoft.CodeAnalysis.Testing` ignores markup for a fixable diagnostic ID in the fixed state unless `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` are set to `MarkupMode.Allow`. Every other test in the repository with remaining fixable markup sets both, for example `RH8204…Tests.VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged`.
  - The spec and the test agree on the output. Only the harness setting is missing.
  - With both settings added through the `onConfigure` overload, the test passes on `bd592fd`.
  - Route to the test implementer. No visible output is in question.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1286

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/ExcludedCodeUtilities.cs` (new) | The excluded-`#if` check, moved out of `CasingCodeFixProviderBase<T>` (decision 5). It holds `HasExcludedCodeOccurrenceAsync(solution, symbols, names, [isCounted], ct)` together with `GetReferringDocuments`, `GetDependentProjectIds`, and `GetReferenceScope`, which the check depends on. The optional `isCounted` filter limits which disabled-text trivia counts; RH2005 uses it. | SHARED-CASING-007, SHARED-FIX-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | The private check and its three helpers are removed. The base now calls the shared check with the old names plus the new name, which is the same set it searched before, and calls the shared `GetDependentProjectIds`. Casing-fix behavior is unchanged: 552 `Naming.RH40*`/`RH41*` tests pass. | SHARED-CASING-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Four changes. (1) `readonly` is added only when the property does not already carry it. (2) New `HasCommentOrDirectiveInsideDeclaration` withholds the fix for a comment, directive, or disabled text in these places: behind the name, on any accessor-list token, behind the closing brace when an initializer follows, or in front of the initializer's `=`. It is called from `IsSupportedPropertyShape`. (3) New `HasValueSafeFieldType` withholds the fix for a struct not declared `readonly`, read from `ITypeSymbol.IsReadOnly`, and for a type parameter that is not a reference type. `Nullable<T>` is judged by its `T` (an enum or a `readonly` struct). Enums keep the fix. (4) New `HasExcludedCodeOccurrenceAsync` withholds the fix when excluded code in a referring document names the property's name or the new field name, through the shared check. Checks (3) and (4) run in `RegisterCodeFixesAsync`. | RH2001-004, RH2001-007, RH2001-008, SHARED-CASING-007, SHARED-FIX-003 |

### Policy owners

- `rg -n "DisabledTextTrivia" src/Reihitsu.Analyzer.CodeFixes`: the excluded-code identifier search now lives only in `ExcludedCodeUtilities`. The casing base, RH2001, and RH2005 all call it. The other hits are different decisions (trivia-preservation guards), not this check.
- `rg -n "IsReadOnly" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Analyzer`: no other owner of a "struct declared `readonly`" decision exists.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAuto"`: every test passes except `VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType` and `VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld` (disputed below).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Naming.RH41|FullyQualifiedName~Naming.RH40"`: 552 passed.

### Disputed tests or scope

- **`RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType`** and **`…VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld`**
  - The cause is the same as for the RH2005 test under #967. The fixed code keeps `{|RH2001:…|}` markup for the withheld properties, but neither test sets `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` to `MarkupMode.Allow`.
  - Both fail with "Diagnostics of fixed state: expected 0, actual 1 or 2". Both pass with only that setting added.
  - Route to the test implementer. No visible output is in question.
- **Scope.** `ExcludedCodeUtilities` is a shared helper with three consumers: the casing base, RH2001, and RH2005. This is the structure that decision 5 asked for. Including RH2005 as a consumer goes beyond decision 5 and is my choice: it avoids a second copy for the RH2005 excluded-code test.

### Architecture findings

_None beyond decision 5, which this change carries out._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1287

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/PartialMemberUtilities.cs` (new) | The one owner of SHARED-DIAG-012. It contains: `TryGetParts` (defining and implementing part of a partial method, constructor, property, indexer, or event, from either part's symbol); `SelectReportedPart` (generated parts are dropped, then the first qualifying part, defining before implementing, otherwise the first remaining part); `GetReportedPart` (for a node analysis, the part to report on, or `null`); and `GetDeclaredSymbol` (a member's symbol; for an event field, the symbol of its first variable). A private `IsGeneratedCode` reads the compiler's rule: `SyntaxTreeOptionsProvider.IsGenerated` first, then the generated-file-name patterns and the `<auto-generated>`/`<autogenerated>` header comment. | SHARED-DIAG-012, SHARED-DIAG-004 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2004AccessModifierMustBeDeclaredAnalyzer.cs` | A `partial` declaration is reported on the part that `GetReportedPart` returns. A part qualifies when it states no accessibility. | RH2004-001, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs` | No fix is registered for a declaration with the `partial` modifier. | RH2004-001, SHARED-DIAG-004 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2002AsyncVoidShouldNotBeUsedAnalyzer.cs` | A partial method is reported once, on the selected part. A part qualifies when it writes `async`. The symbol action runs for both parts, and each part reports only when it is the selected one. | RH2002-001, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.cs` | A partial property is reported once, on the first non-generated part whose summary reads as a sentence. New static `GetSentenceSummary`. | RH8306-001, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Base/MembersShouldBeGroupedByRegionsAnalyzerBase.cs` | A partial member is reported only on the selected part. A part qualifies when it lies outside a region with the expected name. That check is syntax only and works for a part in any tree. (RH7401–RH7404, RH7409–RH7412) | RH74nn-001, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Base/NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs` | Same as above for RH7405–RH7408. A part qualifies when it lies in an override region of its own type declaration. New `GetOverrideRegionNames`, extracted from the loop, and `LiesInOverrideRegion`. A part in another syntax tree counts as qualifying, because its override regions need a semantic model of that tree. | RH74nn-001, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Core/NestedTypeAnalyzerHelper.cs`, `src/Reihitsu.Analyzer/Rules/Design/RH2101NestedClassesShouldNotBeUsedAnalyzer.cs` | `IsStaticType` reads `INamedTypeSymbol.IsStatic`, so a `static` on any part counts. It falls back to the declaration's modifiers when there is no symbol. | RH2101-002 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.cs` | A `partial` top-level declaration whose type was already declared earlier in the file is not reported. The type key is the namespace name parts by value (through `NamespaceCasingHelper.GetLocations`), the identifier's value text, and the type-parameter count. New `GetTypeKey`. | RH7002-001, RH7002-004 |

Driver facts, established with a scratch `CompilationWithAnalyzers` probe on Roslyn 5.9.0, C# latest:

- Node actions and symbol actions run for both parts of a partial method and a partial property.
- For a partial constructor, the driver runs neither for the defining part. It visits only the implementing part.
- `GetReportedPart` therefore reports a constructor's diagnostic from the implementing part's analysis, at the selected part's location, which may be in another document. For every other kind, each part reports only itself.

### Policy owners

- `rg -n "PartialDefinitionPart|PartialImplementationPart" src --glob '!*Test*'`: only `PartialMemberUtilities`.
- `rg -n -i "auto-generated|\.g\.cs|IsGenerated" src --glob '*.cs' --glob '!*Test*'` found three generated-code checks. Each serves a different surface:
  - the CLI's `GeneratedFileUtilities.IsGeneratedFile` (file names only);
  - the formatter's `ReihitsuFormatterHelpers.IsAutoGeneratedSource` (header only);
  - the new private `PartialMemberUtilities.IsGeneratedCode`, which mirrors the compiler's rule that SHARED-DIAG-001 names.

  The new one is not a copy of the other two: their rules differ, and changing them would change CLI or formatter output. See *Architecture findings*.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2002AsyncVoid|FullyQualifiedName~RH8306Property|FullyQualifiedName~Formatting.RH74|FullyQualifiedName~RH2004AccessModifier"`: 259 passed.
- The RH2101 and RH7002 tests pass in the full run below.

### Disputed tests or scope

- **Scope.** There are two new shared helpers, `PartialMemberUtilities` and `ExcludedCodeUtilities` (#1286), and 15 production files. That is more than the handovers named. `PartialMemberUtilities` has six consumers: RH2002, RH2004, RH8306, and the two RH74 bases (twelve rules).
- **Not covered by tests; decided here for the lead.** In RH7405–RH7408, a part in another syntax tree counts as lying in an override region. For a member covered by RH74nn-001, where all parts would be reported, this gives the specified result. In the uncovered case where only some parts would be reported (#1334), it can report nothing.

### Architecture findings

- **One rule stated in several places: "what is generated code".**
  - Components: `PartialMemberUtilities.IsGeneratedCode` (new, analyzer side, the compiler's rule), `Reihitsu.Cli/GeneratedFileUtilities.IsGeneratedFile` (`.Designer.cs`, `.g.cs`, `.g.i.cs`), and `ReihitsuFormatterHelpers.IsAutoGeneratedSource` (header comment).
  - Where they disagree: `Foo.generated.cs` is generated for the compiler and the new check, but not for the CLI's file-name check.
  - The analyzers need the compiler's rule for a tree other than the one under analysis, because the driver only reports `IsGeneratedCode` for its own tree. So the new check cannot reuse either existing one.
  - Unifying the three would change CLI and formatter behavior. That is outside this run.
  - Narrow fix taken: one private copy in the analyzer, with a remark that it mirrors the compiler.

### Follow-up candidates

- One owner for the generated-code rule across the analyzer, the CLI, and the formatter, as described in the finding above. Whether the CLI should also skip `*.generated.cs`, `*.designer.cs` in any casing, and `TemporaryGeneratedFile_*` is a visible CLI question for that follow-up.

## thoenissen/Reihitsu#1288

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2004AccessModifierMustBeDeclaredAnalyzer.cs` | `ShouldSkip` also skips `OperatorDeclarationSyntax` and `ConversionOperatorDeclarationSyntax` with an `ExplicitInterfaceSpecifier`. | RH2004-003 |

### Policy owners

- `rg -n "ExplicitInterfaceSpecifier" src/Reihitsu.Analyzer/Rules/Design/RH2004*`: one `switch` in `ShouldSkip`.

### Tests run

- Covered by the RH2004 filter under #1287: all three `VerifyExplicitInterface…NotReported` tests pass.

### Disputed tests or scope

_None._

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --configuration Debug --project analyzer`: Failed 3, Passed 7100. The failures are the three disputed tests above.
- `scripts/test.sh --configuration Debug --project core`: Passed 649.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115.
- `scripts/build.sh` (Release): 0 errors and 0 warnings once the RH7103, RH1003, and RH5201 findings in my own changes were fixed.
- `scripts/format.sh` over every changed file: it formatted the two new files; nothing else changed.
- Spec note for the spec checker: the code owners of SHARED-DIAG-012 (`PartialMemberUtilities`) and of RH2005-004 (`ExcludedCodeUtilities`, `SymbolFinder` use in the RH2005 fix) are new. No spec table names them yet.
