Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, the three change docs under `sdd/changes/2026-11/`, and the spec diff against `origin/main` (`shared/casing.md`, `shared/code-fixes.md`, RH4005, RH4102–RH4105, RH4112–RH4117, RH4121). Every handed-over test passes except one, `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`, whose Fix All configuration contradicts the existing Fix All tests (*Disputed tests or scope*). No test assertion, input, or expected output was changed.

## thoenissen/Reihitsu#971

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/CasingUtilities.cs` | The derivation splits the name into PascalCase words (`SplitIntoPascalCaseWords`, `ToPascalCaseWord`) instead of building one string; `ToCamelCase` and `ToUnderlineCamelCase` lowercase the leading uppercase run of the first word only (`LowercaseLeadingUppercaseRun`): a run of two or more letters followed directly by a lowercase letter keeps the case of its last letter. `ToPascalCase` is unchanged in result for these names. | SHARED-CASING-006 |

The first-word-only reading (question 10 A) falls out of the word list: the run is taken from `words[0]` after the PascalCase step, so `A_B` → `aB`, `m_HTTPClient` → `mHTTPClient`, `XML_Parser` → `xmlParser`. RH2001's fix calls `ToUnderlineCamelCase` and needed no change of its own.

## thoenissen/Reihitsu#1072

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` | New `WritesNameAsKeywordAsync` (with `CountVerbatimIdentifiers`): reads the renamer's result in every changed document. It withholds when the renamed document holds more identifiers spelled `@` + new name than before, or when an identifier the renamer wrote as the bare new name is read as something other than an identifier when the document's text is parsed again. | SHARED-CASING-013 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `RenameWithoutConflictAsync` calls the check next to the conflict-annotation check; summary updated. Single fix and Fix All share the path. | SHARED-CASING-013 |

No keyword list (FC-044). The second arm exists because the renamer does not escape `await` inside an async local function: it writes `int await` / `return await;`, which binds in the renamer's tree (no added compile error) but is an await expression when the text is read again (`RH4105…AwaitInsideAnAsyncLocalFunction`). A first version compared the whole re-parsed tree with `IsEquivalentTo`; that refused `RH4002…FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`, because the renamer's qualified name is not the shape the parser gives `N.Order`, so the check is narrowed to the tokens spelled as the new name.

## thoenissen/Reihitsu#1451

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/CasingUtilities.cs` | `IsWordCharacter`: letters and digits as before, plus Nl, Mn, Mc, and Pc other than `_`. `IsPascalCase`/`IsCamelCase` (and through it `IsUnderlineCamelCase`) accept these behind the first character. The derivation splits only at characters that are no word character (only `_` among the covered categories), keeps every other character in its word, ignores combining marks when deciding whether a word is all uppercase, and lets a combining mark neither end the leading run nor count as the lowercase letter behind it (`IsCombiningMark`). | SHARED-CASING-001, -002, -003, -006 |
| `src/Reihitsu.Core/CasingUtilities.cs` | `ToTypeParameterName`: `t`/`T` followed by `_` is a prefix; the underscores behind it are dropped; an empty body gives `T`. | RH4121-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4005InterfaceNameCasingCodeFixProvider.cs` | `i`/`I` is a prefix when it is the whole name or is followed by an uppercase letter, a digit, or `_`; underscores behind it are dropped; nothing left behind the prefix returns the old name, which withholds the fix (SHARED-CASING-008). `i2Factory`/`i18nService` derive `I2Factory`/`I18nService`, which the rule still reports, so the existing SHARED-CASING-008 check withholds them. | RH4005-002 |

Lt (titlecase letters) stay accepted as before, because `char.IsLetterOrDigit` includes them; Cf stays a separator, as before. Both are outside the spec's covered categories, so no behavior changes for them.

## thoenissen/Reihitsu#1434

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs` | `CheckCasing` asks a new virtual hook `IsDictatedName` (default `false`) only for a name the rule would report. New shared helpers: `IsDictatedMember` (walks the override chain; at each level, an overridden member or an explicitly or implicitly implemented interface member of the member's own containing type declared in another assembly makes it dictated), `HasNativeEntryPointFromName` (the declaration's `DllImport`/`LibraryImport` attribute, bound through the semantic model, has no `EntryPoint` named argument), with private `GetOverriddenMember`, `GetImplementedInterfaceMembers`, `IsDeclaredOutside`. | SHARED-CASING-014 |
| `RH4102EventNameCasingAnalyzer.cs` | Hook: the declarator at the reported location, or the event declaration, is a dictated member. | RH4102-001 |
| `RH4103MethodNameCasingAnalyzer.cs` | Hook: dictated member, or native entry point taken from the name. | RH4103-001 |
| `RH4104LocalFunctionNameCasingAnalyzer.cs` | Hook: native entry point taken from the name. | RH4104-001 |
| `RH4112…`, `RH4113…`, `RH4114…PropertyCasingAnalyzer.cs` | Hook: dictated member. | RH4112-001, RH4113-001, RH4114-001 |
| `RH4116TupleElementCasingAnalyzer.cs` | Hook: the tuple type lies inside the return type or a parameter type of a method, the type or an indexer parameter type of a property, indexer, or event, or the type of a field-like event, and that member is dictated. A tuple in a base list (question 12 A) or a body is not in a signature and stays reported. | RH4116-001 |

The boundary is the containing assembly of the overridden or implemented member's original definition, so a member of another project of the solution counts as outside, as the spec states (tested for methods across projects). `DllImport` is read from syntax because the first, symbol-based version (`GetDllImportData().EntryPointName == null`) did not recognize the missing `EntryPoint` (the `DllImport` tests kept reporting); `[DllImport]` is a pseudo-attribute. The fix side needed no change: the cascade tests pass with the existing native-entry-point and compile-error checks.

## Policy owners
- Leading-run lowercasing and word splitting: `rg -n "IsLetterOrDigit|ConvertToPascalCase|IsUpper\(" src/Reihitsu.Core src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` — only `CasingUtilities` (owner), plus the prefix tests in `RH4005InterfaceNameCasingAnalyzer.IsInterfacePascalCase` (unchanged; it calls `IsPascalCase`) and the RH4005 fix's prefix detection (changed above).
- Verbatim prefix: `rg -n '"@" \+|IsReservedKeyword|GetKeywordKind' src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Analyzer` — only the new check.
- Override and interface-implementation walks: `rg -n "FindImplementationForInterfaceMember|OverriddenMethod|OverriddenProperty|ExplicitInterfaceImplementations" src` — `InterfaceImplementationUtilities` (region names per interface, SHARED-REGGROUP-004), `OverrideMemberUtilities` (original declaring type name for regions), `DocumentationAnalysisUtilities` (inheritdoc), `SingleLetterIdentifierUtilities`, RH7405/RH7406/RH7408, and the new `CasingAnalyzerBase` helpers. None of them answers "declared in another compilation"; they decide region names and documentation, so none is reused. `PartialMemberUtilities.GetDefiningPartSymbol` is reused for comparing partial parts instead of a private copy.
- Native entry points: `rg -n "DllImportAttribute|LibraryImportAttribute|GetDllImportData" src` — `CasingRenameSafetyUtilities.GetNativeEntryPoint` (code-fix side: which entry point name a cascade member binds to, compared before and after the rename) and the new `CasingAnalyzerBase.HasNativeEntryPointFromName` (analyzer side: whether an `EntryPoint` argument is written). Two decisions; the analyzer assembly cannot call the code-fix assembly. See *Architecture findings*.

## Production diff size
`git diff --shortstat origin/main -- src`: 12 files changed, 520 insertions(+), 74 deletions(-). No new type. New members: `CasingAnalyzerBase.IsDictatedName` (private protected virtual), `IsDictatedMember`, `HasNativeEntryPointFromName` (private protected static), and private helpers; `CasingRenameSafetyUtilities.WritesNameAsKeywordAsync` (internal) and `CountVerbatimIdentifiers`; `RH4116TupleElementCasingAnalyzer.GetParameters`; `CasingUtilities` private helpers. The handovers named `CasingUtilities`, the casing fix base, the RH4005 fix, and the casing analyzers; the change stays in those.

## Deleted special cases
_N/A._ No shape is declared unsupported.

## Tests run
```shell
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"
# Passed!  - Failed: 0, Passed: 132, Total: 132
scripts/test.sh --no-build --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming|FullyQualifiedName~RH2001PrivateAutoProperties"
# Failed!  - Failed: 1, Passed: 1108, Total: 1109   (the disputed test below)
scripts/test.sh            # default run, Release build: 0 warnings
# Reihitsu.Analyzer.Test: Failed: 1, Passed: 11977, Total: 11978   (the disputed test); the run stops at the first failing project, so:
scripts/test.sh --project core          # Passed: 872
scripts/test.sh --project formatter     # Passed: 7130
scripts/test.sh --project cli           # Passed: 440, Skipped: 1
scripts/test.sh --project architecture  # Passed: 198
scripts/test.sh --project tooling       # Passed: 204
```
The self-hosting scan was not run locally.

## Disputed tests or scope

**`RH4106PrivateFieldCasingAnalyzerTests.FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`** (SHARED-CASING-010, RH4106-007). It sets `BatchFixedState` to the unchanged input (`HTTPClient` and `HttpClient` both kept). The production code does exactly that: the single fix renames `HTTPClient` to `_httpClient` and is withheld for `HttpClient` (the `FixedState` part passes), and Fix All skips both. It fails in the testing framework's "Fix all in document" step with *"No code fixes were expected, but a fix was offered that made no changes to the code."*: when the expected batch state equals the input, the framework requires that no Fix All action is offered at all. The casing Fix All provider always offers its action and computes the renames when it is applied. I tried computing the renames in `GetFixAsync` and returning no action when nothing changes; that broke seven existing tests that require the action to exist for a Fix All that changes nothing (`RH4115LocalVariableCasingAnalyzerTests.SameScopeExistingTargetRemainsUnchangedUnderFixAll` ×4, `TopLevelTypeCollisionAcrossDocumentsIsRefusedByProjectFixAll`, `NamedNamespaceTypeCollisionAcrossDocumentsIsRefusedByProjectFixAll`, `DuplicateNormalizedTargetGroupIsRefusedBeforeFixAll`: "Failed to create the Fix All action."), so I reverted it. The asserted output agrees with the spec; only the test's configuration conflicts with the framework and the existing tests. Suggested repair for the test implementer: add a third, non-colliding field to the input so Fix All changes something (the shape of the SHARED-CASING-010 RH4107 example), or use the `ApplyFixAllAsync` helper style of the RH4115 tests.

No scope growth beyond the named components.

## Architecture findings
_None._ The analyzer and the code fix each read a `DllImport`/`LibraryImport` attribute, but for different decisions (whether an `EntryPoint` argument is written; which name the cascade binds to before and after a rename), and the analyzer assembly cannot reference the code-fix assembly; the lead may still want to look at it.

## Follow-up candidates
- The renamer writes `await` unescaped as a parameter name and reference inside an async local function, producing a tree that binds but whose text does not compile. The casing fixes now withhold; the only other code fix that renames through `Renamer.RenameSymbolAsync`, RH2001's, renames a property to a `_camelCase` field name, which is never a keyword, so it cannot hit this. Recorded for the lead only in case another rename-based fix is added; likely no issue needed.
