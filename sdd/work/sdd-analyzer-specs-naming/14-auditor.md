Role: auditor
Commit: 0e549e2596e1199a25b700c7bd586ee2a7a3c438
Verdict: findings

My prompt named the issue, the slug, the base and head, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. I changed no file other than this handover.

## Scope

- Head: `0e549e2596e1199a25b700c7bd586ee2a7a3c438` (`claude/sleepy-thompson-gikv37`), checked out and clean.
- Base: `origin/main` = `git ls-remote origin refs/heads/main` = `4efe6aea3f2db03ee4b91ed03edf56c958a8afe3`, an ancestor of the head.
- Full audit, not a repair delta.
- The diff matches the handovers:
  - 21 new or rewritten specs under `sdd/specs/analyzer/`, `sdd/specs/shared/casing.md`, the index, and the ledger;
  - the decisions entry `squad/decisions/2026-10-17-rh4106-shared-casing-move.md`;
  - 22 test files under `src/Reihitsu.Analyzer.Test/Naming/` and `src/Reihitsu.Core.Test/CasingUtilitiesTests.cs`;
  - `sdd/work/`.
- No production file changed. The only specs that existed on `main` and changed are RH4106 (approved, escalation 1), `sdd/specs/README.md`, and `requirement-ids.txt`.
- `scripts/lane-check.sh tooling`: `confirmed — claimed tooling, diff needs tooling`.

How I verified:

- I read the shared casing types, every analyzer and code-fix provider of the 20 rules, `ConfigurationManager`, and the RH4001 helper against every requirement.
- `scripts/apply-fix.sh` over fixtures in the scratchpad: RH4106, RH4107, RH4005, RH4002, and RH4105, LF and CRLF agreeing in every arm.
- One `dotnet build` of two fixed outputs.
- Five probe tests in a `git archive HEAD` copy of the repository in the scratchpad. In that copy only, the analyzer test project had `RunAnalyzers=false`. Command: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~AuditProbe"`, result `Passed: 5`.
- All fixtures and the copy were deleted afterwards.
- I trust the cleaner's Release build and the roles' focused test results.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/shared/casing.md` SHARED-CASING-006 (and SHARED-CASING-008); through it the fix requirements RH4002-002, RH4003-002, RH4004-002, RH4006-002, RH4010-002, RH4011-002, RH4101-002, RH4102-003, RH4103-003, RH4104-002, RH4105-003, RH4106-004, RH4107-002, RH4108-002, RH4109-002, RH4110-002; and RH4005-002 | FC-007, FC-016, FC-023 | same requirement | See *Finding 1* below. | See *Finding 1* below. |
| 2 | high | `sdd/specs/analyzer/RH4009.md` RH4009-001 | FC-002 | same requirement | See *Finding 2* below. | See *Finding 2* below. |
| 3 | medium | `sdd/specs/analyzer/RH4001.md` RH4001-006 and its tests; `sdd/specs/shared/casing.md` SHARED-CASING-009 and its tests | FC-003 | same requirement | See *Finding 3* below. | See *Finding 3* below. |
| 4 | low | `sdd/specs/shared/casing.md` SHARED-CASING-007 (coverage sentence) and SHARED-CASING-009 | FC-002 | same requirement | See *Finding 4* below. | See *Finding 4* below. |
| 5 | low | `sdd/specs/analyzer/RH4001.md` examples of RH4001-001, -002, -005, -006 | repository convention (`sdd/README.md`, *Spec format*) | same requirement | See *Finding 5* below. | See *Finding 5* below. |

### Finding 1 — the fix requirements cover renames that do not correct the diagnostic

**What goes wrong.** SHARED-CASING-006 puts every name made of Lu/Ll/Lm/Lo letters, decimal digits, and `_` into its domain. It then says "The result is the PascalCase name". That is false when the first derived word starts with a digit or with a letter that has no uppercase form (Lo/Lm). For such names the fix requirements say MUST rename, and SHARED-FIX-001 says one application corrects the diagnostic. But SHARED-CASING-001/-003 and RH4005-001 still reject the result.

Observed with `apply-fix.sh`. In every case the result is a valid identifier that differs from the old name, so SHARED-CASING-008 does not withhold the fix:

- **RH4106** (`private int __1st;`) and **RH4107** (`protected int __1st;`): renamed to `_1st`, which is still reported, and then no fix is offered.
- **RH4106** `_1_value` → `_1Value`, still reported, no fix.
- **RH4106** `注文_明細` → `_注文明細`, still reported, no fix.
- **RH4002** `class _注文` → `class 注文`, and `class 注文_明細` → `class 注文明細`. Both are still reported, with no fix.
- **RH4105** `int _注文` → `int 注文`, still reported, no fix.
- **RH4005** `interface _2x` → `I2x` and `interface 注文` → `I注文`. Both are still reported. A second application makes them `II2x` and `II注文` ("fixed (1 action, 2 iterations)").

Each of these inputs is inside the coverage of SHARED-CASING-006 and of RH4005-002 ("an old name that contains a letter or a digit"). Skeptic R3 found the same failure for `_` / `__`, and RH4005-002 was narrowed for it; these are sibling shapes of it. The `__1st` shape was already in RH4106-004 on `main`. This run makes it shared and extends it to RH4107 and RH4005.

**Suggested change.**

- Correct "The result is the PascalCase name".
- Narrow the coverage of SHARED-CASING-006, and of RH4005-002, to old names whose derived name is in the target style. For example: the first derived word starts with a letter that has an uppercase and a lowercase form, and for `_camelCase` it does not start with a digit.
- File the excluded inputs in thoenissen/Reihitsu#1009 and thoenissen/Reihitsu#1010. Alternatively, because SHARED-FIX-001 already applies to every casing fix, file them as a bug against SHARED-FIX-001, with the maintainer's approval.

### Finding 2 — RH4009-001 claims trailing content after the root object

**What goes wrong.** RH4009-001 says "MUST NOT report anything ... when the file is not valid JSON". `ConfigurationManager.ParseConfiguration` stops reading at the root object's `EndObject` and never reads the rest.

Probe test, passing: with `reihitsu.json` =

```json
{ "naming": { "allowedNamespaceDeclarations": [ "A" ] } } garbage
```

or with `{ … } { }`, `namespace Company.Product { }` is reported. Neither file is valid JSON.

`sdd/specs/analyzer/RH0001.md` already lists this behavior ("Content after the root object (`{ } junk`) …") as undecided.

**Suggested change.** State the condition as the loader failing to parse the file, rather than "not valid JSON". Keep content after the root object out of RH4009-001, and route that case to the open question that RH0001's text already names.

### Finding 3 — two qualifiers are tested on one side only

**What goes wrong.**

- **RH4001-006**, "in the same folder": the spec has no example, and the tests nothing, for the other side — a document with the new file name in another folder, where the fix is offered. The four withheld tests all use the same folder.
- **SHARED-CASING-009**, "for a symbol that is not declared directly in a namespace, when the rename adds a compile error": this qualifier was added in this run. Its coverable side, for example `class Outer { class order { public int Order; } }` with no fix offered, has no example and no tagged test. The tagged SHARED-CASING-009 tests exercise:
  - the renamer conflict: RH4106 `Value`/`_value`, RH4109 `amount`;
  - the namespace-member clause: the RH4002 and RH4115 cross-document tests.

**Suggested change.** Add an example and a tagged test for each missing side: RH4001-006 with a same-named document in another folder, and a nested type whose member has the new name for SHARED-CASING-009.

### Finding 4 — "declared directly in a namespace" does not say it includes the global namespace

**What goes wrong.** Both requirements split on "declared directly in a namespace". The code splits on `ContainingSymbol is INamespaceSymbol`, which includes the global namespace. The inputs of thoenissen/Reihitsu#1009 Q1 are file-level types, so they depend on that reading.

A reader who takes "a namespace" to exclude file level would conclude two things for `class order { public int Order; }` at file level: SHARED-CASING-007 covers it, and the compile-error clause of SHARED-CASING-009 withholds the fix. The code offers the fix.

**Suggested change.** Say "in a namespace, including the global namespace (a type at file level)" in both requirements.

### Finding 5 — the RH4001 examples use names from the test project

**What goes wrong.** The RH4001 examples use the verifier's default file `Test0.cs` and the test-project names `TestClass`, `TestComponent`, `Mismatched.cs`, `EmptyNamespace`, `PopulatedNamespace`, and `LaterType`. `sdd/README.md` asks for neutral examples, with no names taken from test projects. This is the same check under which RH4008-001 and RH4009-001 were repaired in round 1. Any neutral name shows the behavior, because the tests set file paths explicitly (`/0/Outer.cs`, `/0/Cache.cs`).

**Suggested change.** Use neutral names (`Order.cs`, `class Order`, …), and have the covering tests mirror the new text.

## Guard scope

The run adds no code predicate. The decisions below are the specs' qualifiers and carve-outs against the predicates that decide them in code.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-CASING-009 compile-error clause applies off-namespace only | Symbols "not declared directly in a namespace" | `PrepareRenameAsync`: `declaredSymbol.ContainingSymbol is INamespaceSymbol` → `HasNamespaceOwner`, which skips `HasAddedBindingErrorsAsync`; the domain is `ContainingSymbol.DeclaringSyntaxReferences`, with the whole document as fallback | Agrees when "namespace" includes the global namespace | Finding 4 |
| SHARED-CASING-009 namespace-member clause, arity, file-local | Members of the containing namespace in source | `HasNamespaceCollision` / `IsNamespaceCollision`: `GetMembers(identifier)`, non-empty `DeclaringSyntaxReferences`, `Arity`, `IsFileLocal` plus a shared tree; a nested namespace counts as a member | Agrees | ok |
| SHARED-CASING-008 withhold conditions | Old name, derived name | `TryGetFixedIdentifier`: empty, ordinal-equal, `SyntaxFacts.IsValidIdentifier` | Agrees within the domain (digit start is the only invalid shape) | ok; but see Finding 1 for valid-but-unstyled results |
| SHARED-CASING-006 derivation | Whole old name, split at `_` | `ConvertToPascalCase` (split at non-letter-or-digit, `ToUpperInvariant` first character, `TrueForAll(char.IsUpper)` lowercasing), plus the early returns of `ToPascalCase` / `ToCamelCase` / `ToUnderlineCamelCase` | The derivation agrees; the claim "the result is the PascalCase name" does not | Finding 1 |
| SHARED-CASING-007 coverage (namespace types, verbatim targets) | Types in a namespace with a member or type parameter of the new name; reserved keywords and `await` in async | No predicate; these are the carve-outs for #1009 Q1 and Q2 | Agrees with observed behavior | ok |
| SHARED-CASING-010 group | Same target, same containing symbol, conflict when composed | `AddDuplicateTargetsForCandidateAsync`: ordinal identifier, `SymbolEqualityComparer` on `ContainingSymbol`, `CandidatesConflictAsync` | Agrees | ok |
| SHARED-CASING-004 groups | Modifiers of the declaration; parent interface | `NamingAccessibilityClassifier.GetEffectiveAccessibility` | Agrees (`protected internal` → internal, `private protected` → protected, interface default public, explicit interface property `NotApplicable`) | ok |
| SHARED-CASING-005 segments | Each identifier of the dotted name | `NamespaceCasingHelper.GetLocations` (`QualifiedNameSyntax` / `IdentifierNameSyntax`, `ValueText`, node location) | Agrees | ok |
| RH4005-001 acceptance | Characters 0 and 1, whole name | `IsInterfacePascalCase` | Agrees | ok |
| RH4005-002 prefix drop and coverage | Characters 0 and 1; the rest | `OnTransformIdentifier` (`i`/`I` plus `char.IsUpper(identifier[1])`, then `ToPascalCase`, then `I` +) | The derivation agrees; the coverage is too wide | Finding 1 |
| RH4001-001 first type | Root children, recursing into namespaces only | `FindFirstTypeDeclaration` (`BaseTypeDeclarationSyntax` or `DelegateDeclarationSyntax`) | Agrees | ok |
| RH4001-002 file-name stem | Up to the first dot, without the extension; 14-digit prefix | `GetComparableFileName` / `StripTimestampPrefix` (`Length > 14`, `[14] == '_'`, 14 × `char.IsDigit`); ordinal `!=` | Agrees | ok |
| RH4001-006 target taken | Same folder, case-insensitive name | `TargetDocumentAlreadyExists` (`Name` with `OrdinalIgnoreCase`, `Folders.SequenceEqual`, skips itself) | Agrees; one side untested | Finding 3 |
| RH4009-001 activity | Configuration file state | `OnCompilationStart` (`AllowedNamespaceDeclarations?.Count > 0`), `GetConfiguration` (a `JsonException` gives a null configuration), `ParseAllowedNamespaceDeclarations` (only strings kept) | Disagrees for content after the root object | Finding 2 |
| RH4009-002 comparison | Name as written, file-level declarations | `Name.ToString()` with ordinal `List<string>.Contains`; `Name.GetLocation()` | Agrees within the carve-outs (#1012) | ok |
| RH4105-002 exclusions | Parameter name, parent list | `ValueText != "_"`; `Parent is not ParameterListSyntax { Parent: RecordDeclarationSyntax }` (also covers `record struct`) | Agrees | ok |
| RH4102-002 / RH4103-002 explicit implementations | Declaration's explicit interface specifier | `ExplicitInterfaceSpecifier: null` / `is null` | Agrees | ok |
| RH4106–RH4110 `const` split | Field modifiers | `Modifiers.Any(SyntaxKind.ConstKeyword)` (negated for RH4106–RH4109) | Agrees | ok |
| RH4010 / RH4011 split | Record kind | `SyntaxKind.RecordDeclaration` (plus a redundant struct-keyword check) / `SyntaxKind.RecordStructDeclaration` | Agrees | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Naming styles and the name derivation | `CasingUtilities` (`IsPascalCase`, `IsCamelCase`, `IsUnderlineCamelCase`, `ToPascalCase`, `ToCamelCase`, `ToUnderlineCamelCase`, `ConvertToPascalCase`). Consumers: every RH40xx/RH41xx naming analyzer and provider, and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` (consumer only). RH4005 `IsInterfacePascalCase` / `OnTransformIdentifier` build on it and are owned by RH4005. No other casing predicate in `src/` outside `RegionNamingRewriter` / RH7302, which concern region names. | One owner |
| Accessibility groups | `NamingAccessibilityClassifier.GetEffectiveAccessibility`. Consumers: RH4106–RH4109, RH4111–RH4114. | One owner |
| Namespace segment check | `NamespaceCasingHelper.GetLocations`. Consumers: RH4007, RH4008. | One owner |
| Rename, withholding, Fix All | `CasingCodeFixProviderBase<T>` (`TryGetFixedIdentifier`, `HasRenameConflictAsync`, `RenameWithoutConflictAsync`, `HasNamespaceCollision`, `CasingFixAllProvider`). No naming provider overrides `CanRegisterCodeFix`; rg finds only the base. | One owner |
| RH4001 expected file name | `RH4001TypeNameShouldMatchFileNameHelper`, shared by analyzer and fix. The analyzer and the helper each carry an identical private `GetIdentifier(MemberDeclarationSyntax)`; that predates this run and is owned by the one spec. | One owner (hint) |
| Configuration loading | `ConfigurationManager` (RH0001's code owner). RH4009-001 restates its parse-failure behavior. | Finding 2 (the restatement disagrees) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Analyzer-only example tests (for example RH4002 `PascalCaseAcceptsLettersAndDigitsAndRejectsLowercaseStartsAndSeparators`, RH4106 `FieldsWithoutAccessibilityModifierBelongToThePrivateGroupOutsideInterfaces`, RH4102 `EveryKindOfEventIsCheckedWithOneDiagnosticPerEvent`) | Exactly the marked spans are reported | A missing or extra diagnostic, or a different span | `Verify(source)`, exact diagnostic set | Adequate |
| Fix example tests (for example RH4106 `FixDerivesUnderlineCamelCaseFromTheWordsOfTheOldName`, RH4103 `FixRenamesOverridesAndImplementationsWithTheirBase`, RH4105 `FixRenamesToContextualKeywordsWithoutVerbatimPrefix`) | Exact fixed text, no diagnostic left, and Fix All gives the same text | A different text, a remaining diagnostic (the default fixed-state markup handling counts it), more iterations, or a Fix All difference | `Verify(source, fixed)`: iterative, then Fix All | Adequate |
| Withheld-fix tests (for example RH4106 `NoFixIsOfferedWhenTheNewNameIsTaken`, RH4105 `NoFixIsOfferedWhenNoUsableCamelCaseNameCanBeDerived`, RH4002 `NoFixIsOfferedWhenFileLocalTypeOfTheNewNameIsInTheSameFile`) | Reported, and no action changes the code | Any applied action | `Verify(source, source)`; the test implementer showed it fails when the collision is removed | Adequate |
| RH4107 `FixAllSkipsFieldsThatWouldReceiveTheSameNameAndRenamesTheOthers`, RH4109 `FixRenamesPublicFieldAndWithholdsTheFixWhenTheNewNameIsTaken` | Single-fix result and Fix All result, with the kept diagnostics | A kept diagnostic missing or extra, or a renamed colliding field | `MarkupMode.Allow` makes the kept diagnostics expected, which is stronger than the default; `NumberOfFixAllIterations = -2` is an upper bound | Adequate |
| RH4001 `NoFixIsOfferedWhenDocumentWithTheNewFileNameExists` / `…InOtherCasingExists` | No fix when the target name is taken (both casings) | A rename being applied | `Verify(source, null, …)` with fixed = test sources | Adequate on the tested side; the other-folder side is missing (Finding 3) |
| RH4009 `EntriesThatAreNotStringsAreIgnoredAndStringEntriesStayInEffect`, `AllowListWithoutStringEntryIsInactive`, `NamingSectionWithoutAllowListIsInactive`, `InvalidConfiguration` | Active or inactive per configuration | A report where none is expected or the reverse; the `[ "A", 1 ]` test is the positive side that makes the negatives falsifiable | Analyzer `Verify` with an additional file | Adequate; the requirement itself is too wide (Finding 2) |
| RH4105 and RH4106 `FixAllCorrectsEveryDiagnostic` overrides (SHARED-CASING-010) | Fix All scenario of the base | As in the base | `BatchCodeFixTestsBase` | Adequate |
| `CasingUtilitiesTests` `ToPascalCaseTest` / `ToCamelCaseTest` / `ToUnderlineCamelCaseTest` (SHARED-CASING-006) | Utility conversions | A different string | Exact `Assert` per row | Adequate. The `FName` row is outside the coverage (thoenissen/Reihitsu#971), as the test implementer noted |

## Failure classes dismissed

- **FC-001:** no code guard changed. The spec carve-outs match their code predicates; see *Guard scope*.
- **FC-004, FC-005, FC-019, FC-021:** every fix in scope is a `Renamer.RenameSymbolAsync` call or a document rename; none joins lines or edits a token gap.
- **FC-006:** not dismissed in full. Namespace-type and referenced-assembly compile breaks are filed: thoenissen/Reihitsu#1009 Q1, #1013 Q1, and #1014 Q1. Partial types and members:
  - a `partial class order` across two documents is renamed together (probe, passing);
  - a partial method's two parts and their call are renamed together (probe, passing);
  - a partial method's parameter in both parts is renamed together (probe, passing).

  Explicit interface implementations and `file` types are tested.
- **FC-008:** multiple declarators of one declaration (RH4107-002 and RH4106-003 examples) and annotation-tracked candidates cover stale positions.
- **FC-009:** `TryGetFixedIdentifier` rejects an unchanged name, and the withheld tests assert zero actions.
- **FC-010 to FC-014, FC-028 to FC-030:** no formatter phase renames identifiers, no casing fix formats, and no rule is new.
- **FC-015:** one owner per policy; see *Policy ownership*.
- **FC-017:** helpers are as strong as the guarantee; see *Assertion adequacy*.
- **FC-018:** the renamer is used, and every fix test shows references renamed, including `nameof`.
- **FC-020:** the inactive-`#if` reference is thoenissen/Reihitsu#1009 Q4, carved out by "active code".
- **FC-022:** the renamer leaves literal text alone, and `nameof` is covered.
- **FC-024:** the partial probes above; static interface fields are in the SHARED-CASING-004 example; explicit interface properties are in the RH4111 test.
- **FC-025, FC-026, FC-027:** a rename is independent of member body shape. Same-line declarators and nested local functions are tested.
- **FC-029:** no key/value layout.
- **FC-031:** no production method changed. The summaries of the new tests describe what they assert; I read every one in RH4106, RH4107, RH4109, RH4009, and RH4105, and sampled the rest.
- **FC-032:** no compiled production change.
- **FC-033, FC-034:** no reproduction; this run writes specs for existing behavior.
- **FC-035:** expected values come from spec examples run on unchanged code.
- **FC-036:** `nameof` is tested. P/Invoke and `[LibraryImport]` are carved out (thoenissen/Reihitsu#1013 Q2). Public-API and serialization renames are inherent to every naming fix, and no spec promises they preserve behavior.
- **FC-037:** no comment-only claim.

FC-002, FC-003, FC-007, FC-016, and FC-023 are findings 1 to 4.

## Hints

- thoenissen/Reihitsu#1009 Q2 says the keyword case reaches "RH4115, RH4117 and RH4120 later". RH4120 checks and converts with `IsPascalCase` / `ToPascalCase`, so it cannot reach a lowercase keyword. The issue text could drop RH4120.
- `sdd/specs/analyzer/RH0001.md` (on `main`) records the content-after-root-object case as an open question inside a non-goal, which `sdd/README.md` (*Non-goals*) does not allow. This predates the run and is unrelated, except that Finding 2 touches the same case.
- `CasingUtilities`, now a code owner of the shared casing spec, also implements RH4121's type-parameter style (`IsTypeParameterName`, `ToTypeParameterName`). When RH4121 is specified, its spec cannot list `CasingUtilities` as a code owner. This follows the maintainer's decision; it is noted for the RH4121 batch.
- The RH4105 `FixAllCorrectsEveryDiagnostic` override tags only the shared SHARED-CASING-010. That fits the base summary ("a Fix All requirement of its rule's spec"), since RH4105 lists it under `Uses`, but it reads more loosely than the `sdd/README.md` sentence "of its own rule".
- `RH4001TypeNameShouldMatchFileNameAnalyzer` and `RH4001TypeNameShouldMatchFileNameHelper` each carry an identical private `GetIdentifier`. This predates the run.
- Not verified: whether SHARED-CASING-009's "in source" includes types of a referenced project of the same solution (a compilation reference). The code skips only members without `DeclaringSyntaxReferences`.
