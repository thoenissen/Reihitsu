Role: skeptic
Commit: 7d6b36c77d008777938c4364ee542c48ef95e7e2
Verdict: proceed narrower

I checked the spec diff (`git diff 4efe6ae -- sdd/specs`) against the code that decides each requirement, and ran every input below through `scripts/apply-fix.sh <RH####> <fixture-dir>` (LF and CRLF agreed in every arm). Where I compiled a fixed output, I used a scratch `net10.0` library project. All fixtures were outside the repository and have been deleted. My prompt carried no intended fix and no hoped-for verdict.

Most requirements state what the code does. Seven of them need repair before tests are written (F1–F7), because they claim inputs where the code does something else, or where its output looks wrong. F8 lists more cases that look wrong, for the spec author's open-question judgment.

## Premise

### Confirmed against the code (I tried to break each and could not)

- SHARED-CASING-001, -002 and -003 are exactly `CasingUtilities.IsPascalCase`, `IsCamelCase` and `IsUnderlineCamelCase`.
- SHARED-CASING-004 is `NamingAccessibilityClassifier.GetEffectiveAccessibility`. The group is decided by modifiers only. A member with no modifier is public in an interface and private elsewhere, and an explicit interface property is `NotApplicable`.
- SHARED-CASING-005 is `NamespaceCasingHelper.GetLocations`: one check per `IdentifierNameSyntax`, located on that node.
- SHARED-CASING-006 matches `ConvertToPascalCase` for every rule that converts the whole reported name (but see F2 and F7).
- SHARED-CASING-007 matches `GetDeclaredSymbol` and `Renamer.RenameSymbolAsync`.
- SHARED-CASING-008 matches `TryGetFixedIdentifier`, apart from keywords (F3).
- SHARED-CASING-010 matches `AddDuplicateTargetsForCandidateAsync`: same identifier, `SymbolEqualityComparer`-equal containing symbol, and a conflict when the two renames are composed.
- SHARED-CASING-009 holds for symbols that are not declared directly in a namespace. A nested `class Outer { class order<T> { } class Order { } }` is fixed to `Order<T>`, which is consistent with the arity sentence.
- The RH4106 decision is honored:
  - all seven IDs are kept;
  - the ledger diff is +55 lines and −0;
  - the acronym non-goal is gone;
  - `CasingUtilities` is owned by the shared spec.
- SHARED-CASING-010 contradicts SHARED-FIX-002 for a colliding group, but that is a deliberate deviation. `sdd/README.md` (*Analyzer specs*) sanctions it ("skips colliding renames"), and RH4106-007 on `main` set the precedent. It is not a new conflict.
- RH4102 Fix All with both event kinds: I agree with the spec author that this is a bug against SHARED-FIX-002. `GetFixAllCandidatesAsync` keeps only nodes of the provider's `T`, so each provider's Fix All skips the other kind. Filing it needs the maintainer's approval. Until then, no test may assert a mixed-kind Fix All for RH4102.

### F1 — SHARED-CASING-009 claims a guarantee the code does not give for types declared directly in a namespace (FC-002, FC-016)

Observed with `apply-fix.sh`, then compiled:

| Rule | Input | Fix result | Compiles |
|---|---|---|---|
| RH4002 | `class order { public int Order; }` | `class Order { public int Order; }` | CS0542 |
| RH4002 | `class order<Order> { }` | `class Order<Order> { }` | CS0694 |
| RH4003 | `struct order { public int Order; }` | `struct Order { public int Order; }` | CS0542 |
| RH4006 | `delegate void handler<Handler>();` | `delegate void Handler<Handler>();` | CS0694 |
| RH4002 (nested, control) | `class Outer { class order { public int Order; } }` | no-fix-offered | — |
| RH4004 (control) | `enum color { Color }` | `enum Color { Color }` | yes |

Mechanism:

- When the renamed symbol's containing symbol is a namespace, `PrepareRenameAsync` returns `HasNamespaceOwner = true`.
- `RenameWithoutConflictAsync` then skips `HasAddedBindingErrorsAsync`.
- So a compile error that the rename adds inside the renamed type itself does not withhold the fix. The error comes from one of the type's own members (CS0542) or one of its own type parameters (CS0694) having the new name.

Affected rules:

- RH4002, RH4003 and RH4006 are verified above.
- RH4005, RH4010 and RH4011 go through the same branch; that is derived from the code, not run.
- Fix All goes through the same `RenameWithoutConflictAsync`.

The spec gets this wrong in two places:

- SHARED-CASING-009 says the fix MUST NOT be offered "when the rename adds a compile error inside the declarations that contain the renamed symbol". For a type declared in a namespace, that is the namespace declaration or compilation unit, and the error is inside it.
- The rule pages also promise that a colliding name gets no fix.

The current behavior produces code that does not compile, so it must not be written into the spec either. Two repairs are needed:

- Narrow the compile-error clause of SHARED-CASING-009 to symbols that are not declared directly in a namespace, or otherwise keep these inputs out of it.
- File the inputs in `SDD open questions: sdd/specs/shared/casing.md`. The behavior is shared, so the shared spec's issue owns it.

### F2 — RH4005-002 states an output the code does not produce when the rest of the name is all uppercase

Observed with `apply-fix.sh RH4005`:

- `interface URL` → `IURL`
- `interface iURL` → `IURL`
- `interface DB` → `IDB`
- Control: `interface Document_STORE` → `IDocumentStore`

RH4005-002 says the rest "is converted to the PascalCase name that SHARED-CASING-006 derives", which gives `IUrl` and `IDb`. The code differs because `OnTransformIdentifier` calls `CasingUtilities.ToPascalCase`, and that method returns its input unchanged when the input is already PascalCase. A rest without separators is therefore never converted.

Only RH4005 reaches this early return:

- Every other PascalCase rule passes a reported name, which is never PascalCase.
- `ToCamelCase` and `ToUnderlineCamelCase` have the same early return, but their rules report only names that fail the same check.

The result `IURL` is accepted by RH4005-001, like `IHTTPClient`. It does conflict with the `Why:` of SHARED-CASING-006 ("`URL` is a shouted word"). The spec author has two options:

- record the current output: a rest that is already PascalCase is kept as written; or
- if that looks wrong, keep these inputs out of RH4005-002 and file them in RH4005's open-question issue next to Q1.

### F3 — SHARED-CASING-008 "not a valid identifier" reads as withheld for keywords; the code renames to a verbatim identifier (FC-023)

Observed with `apply-fix.sh RH4105`, each fixed in one iteration:

- `void M(int Class)` → `int @class`
- `object Object` → `object @object`
- `int M(string String) { return String.Length; }` → `@string` in the declaration and at the use

The code differs because `TryGetFixedIdentifier` uses `SyntaxFacts.IsValidIdentifier`, which checks characters only, and the renamer escapes the keyword.

Only camelCase targets can reach this, because every C# keyword is lowercase. In this batch that is RH4105; RH4115, RH4117 and RH4120 are out of scope.

A reader of SHARED-CASING-008 will conclude that `class` is not a valid identifier and that no fix is offered. The spec has to say which outcome holds:

- a name that is a keyword is renamed to its `@` form; or
- if an `@` the author never wrote looks wrong, keep it out of the spec and file it in the shared casing spec's issue.

### F4 — RH4103 writes in a fix that breaks P/Invoke at run time (FC-036)

Observed with `apply-fix.sh RH4103`:

- Input: `[DllImport("libc")] internal static extern int getpid();` with a call to `getpid()`.
- Result: both are renamed to `Getpid`, and the code compiles.
- The runtime binds a `DllImport` that has no `EntryPoint` by the method name, so the fixed code fails at run time (entry point not found).

RH4103-001 (any method without `override`) and RH4103-003 cover this input today, as if it were intended. A sibling derived from the code, not run: a `[LibraryImport]` partial method without `EntryPoint`, whose generator also uses the method name.

The name is dictated by the native library, which is the same reasoning as Q1. Keep extern and `LibraryImport` methods without an explicit `EntryPoint` out of RH4103-001 and RH4103-003, and file them in RH4103's open-question issue.

### F5 — The `override` carve-out of RH4103 is not applied to its siblings

RH4103-001 carves out every `override` method because of Q1, where the base method is in a referenced assembly. The same reason applies to two cases that stay covered:

- **RH4103:** an implicit implementation of an interface method declared in a referenced assembly. `RH4103MethodNameCasingAnalyzer.GetLocations` excludes only `ExplicitInterfaceSpecifier`, so this case stays covered by RH4103-001.
- **RH4102:** RH4102-001 covers "every event", which includes `override` events and implicit implementations of events from a referenced assembly. `RH4102EventNameCasingAnalyzer` excludes only explicit interface implementations.

That these cases are reported follows from the code. I did not run them: `apply-fix.sh` fixtures are single-document projects, and the BCL has no lowercase member to override. So whether a fix is offered is not established. Either treat these cases like Q1 (widen Q1 and the carve-outs) or state why they differ.

### F6 — RH4001-002 and RH4009-002 claim the inputs of their own open questions

- **RH4001 Q1** (`@class`):
  - RH4001-002 says it compares "the type name". The name of `@class` is `class`, so as written the requirement says `class.cs` matches.
  - `GetExpectedFileNameStem` uses `Identifier.Text` (`@class`), so the file is reported. RH4001-005 has the same problem.
  - Q2 (the timestamp) is carved out explicitly in RH4001-005; Q1 is not carved out anywhere.
- **RH4009 Q2** (`namespace A . B`):
  - RH4009-002 says "whose dotted name is not exactly one of the entries", and the dotted name of `A . B` is `A.B`.
  - The code compares `Name.ToString()`, i.e. the name as written.
  - Q1 (nesting) is carved out by "declared at file level"; Q2 is not.
  - Add a third shape to Q2, derived from `OnNamespaceDeclarationSyntax` and not run, because `apply-fix.sh` cannot supply `reihitsu.json`: `namespace @MyCompany.Project` with `[ "MyCompany.Project" ]` is reported, because the `@` stays in the compared text.

### F7 — The coverage condition of SHARED-CASING-006 leaves out more than thoenissen/Reihitsu#971 decides (FC-002)

The condition excludes every name whose first word has an uppercase second character. thoenissen/Reihitsu#971's domain is only a leading run of two or more uppercase letters.

Excluded by the condition but outside #971, observed with `apply-fix.sh RH4106`:

- `iPhone` → `_iPhone`
- `eTag` → `_eTag`

#971's stated rule leaves both unchanged. A condition that matches #971's scope: "whose first word does not start with two uppercase letters, or consists of uppercase letters only". `XValue` → `_xValue` today. That is also #971's expected output, but the name is in #971's domain, so leave it out. Low severity: this is coverage the spec gives away, not a wrong statement.

### F8 — Outputs that look wrong and that the specs currently cover as intended (for the spec author's open-question judgment)

- **a. Combining marks (FC-023).** RH4002 `class Café`, written as `e` + U+0301, is reported and renamed to `class Cafe`.
  - `char.IsLetterOrDigit` is false for a nonspacing mark. SHARED-CASING-001 therefore rejects the name, and SHARED-CASING-006 drops the mark.
  - The fix changes the name's letters, not only their case. This is shared behavior, so it belongs to the shared spec's issue.
- **b. A reference in inactive `#if` code (FC-020).** RH4106 `private int Value;`, used once inside `#if NEVER` and once outside:
  - Every active reference becomes `_value`.
  - The inactive line keeps `Value`, so it stops compiling once `NEVER` is defined.
  - SHARED-CASING-007 says "every reference to it in the solution".
- **c. A file of top-level statements (RH4001).** `Program.cs` containing `System.Console.WriteLine(new Helper());` followed by `internal class Helper { }`:
  - The file is reported, and the fix renames it to `Helper.cs`.
  - RH4001-001 covers this input. Such a file is about the program, not about the type that follows it.

### Inputs the code decides that the specs do not mention (no defect)

- **RH4105-001:** the list of parameter kinds omits anonymous methods. `delegate (int Value) { }` is reported and fixed to `value`.
- **SHARED-CASING-007:** `nameof(Value)` is renamed with the other references to `nameof(value)`, so the string value changes (FC-036). This is a test seed for "every reference".
- **RH4009-001:** "missing or empty" does not say what happens to entries that are not strings. In `ParseAllowedNamespaceDeclarations`:
  - `[ 1 ]` parses to an empty list, so the rule is inactive;
  - `[ "A", 1 ]` keeps `"A"`, so the rule is active.
- **RH4001:** a document without a file path is never reported (`OnSyntaxTreeAction` returns early).
- **Partial members and types:** the defining and the implementing part each get a diagnostic, and one rename fixes both. This is derived from the code: Fix All finds both candidates and renames once.

## Mechanism and candidates

_N/A_ for a run that writes specs of existing behavior. The one mechanism behind a defect is F1:

- Mechanism: a type declared directly in a namespace is renamed without the added-compile-error check.
- Dispatch: `PrepareRenameAsync`, on `declaredSymbol.ContainingSymbol is INamespaceSymbol`.
- Candidates: RH4002, RH4003, RH4005, RH4006, RH4010 and RH4011, through every type declared at file level or in a namespace, with a member (class, struct, record) or a type parameter (any generic type, delegate) that has the new name.

## Breaking inputs

For the test implementer, once the specs are repaired:

- **FC-002 / FC-003:**
  - Both sides of each SHARED-CASING-004 group: no modifier in a class, struct, record and interface; `private protected`; `protected internal`; an explicit interface property.
  - Both sides of the SHARED-CASING-009 arity and file-local conditions.
  - The nested different-arity case above.
- **FC-006 / FC-024:**
  - explicit interface implementations (RH4102-002, RH4103-002);
  - partial types and methods with two diagnostics;
  - `file` types (SHARED-CASING-009);
  - static interface fields (SHARED-CASING-004).
- **FC-007:**
  - every fix converges in one iteration;
  - `@class` (F3) is not reported again.
- **FC-008:** several declarators in one declaration under Fix All, as in the RH4107-002 and RH4106-003 examples.
- **FC-016:**
  - F1 across all type rules;
  - RH4102's two providers, each kind on its own and never mixed until the bug is decided.
- **FC-018:** references in another member, a `nameof`, and a call site, for each rule's -002 test.
- **FC-020:** the inactive-`#if` reference (F8b), only once its open question is decided; until then, keep it out of the tests.
- **FC-023:**
  - `_` (RH4105-002, SHARED-CASING-003);
  - `__` and `_1st` (SHARED-CASING-008);
  - keyword targets (F3);
  - verbatim names (RH4001 Q1, RH4009);
  - combining marks (F8a), kept out until decided.
- **FC-036:**
  - `nameof` string change (SHARED-CASING-007);
  - P/Invoke (F4), kept out of RH4103 tests.

Do not write tests for the inputs of F1, F2, F3, F4 or F8 until the spec author has stated them or filed them.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/casing.md` | Must change: SHARED-CASING-009 (F1), -008 (F3), -006 coverage condition (F7); -007 depending on F8b |
| `sdd/specs/analyzer/RH4005.md` | Must change: RH4005-002 (F2) |
| `sdd/specs/analyzer/RH4103.md` | Must change: RH4103-001 and -003 (F4, F5) |
| `sdd/specs/analyzer/RH4102.md` | Must change, or state why it differs: RH4102-001 (F5) |
| `sdd/specs/analyzer/RH4001.md` | Must change: RH4001-002 and -005 (F6); RH4001-001 depending on F8c |
| `sdd/specs/analyzer/RH4009.md` | Must change: RH4009-002 (F6); optionally RH4009-001 (non-string entries) |
| `sdd/specs/analyzer/RH4105.md` | Should change: the list in RH4105-001 (anonymous methods) |
| `sdd/specs/analyzer/RH4106.md` | Must stay; the decision is honored |
| The other rule specs (RH4002–RH4004, RH4006–RH4008, RH4010, RH4011, RH4101, RH4104, RH4107–RH4110) | Must stay, except where the repaired shared requirements are referenced |
| Analyzers, code fixes, `CasingUtilities`, Fix All provider | Must stay; this lane changes no production code. F1 and the RH4102 Fix All are candidates for the maintainer to decide as bugs |
| `documentation/rules/RH4xxx.md` | Not involved |
| Tests (`Reihitsu.Analyzer.Test`, `Reihitsu.Core.Test`) | Must change: tags and focused tests, after the spec repair |
| Formatter / FC-013 / FC-014 | Not involved: no formatter phase renames identifiers, and no rule is new |

## Simpler option

_None._ Each repair narrows an existing requirement or states the current output; none adds behavior.

## Questions for the maintainer

_None._ F1–F8 are open questions that the squad may file without approval, in `SDD open questions: sdd/specs/shared/casing.md` (F1, F3, F8a, F8b) and in each rule spec's issue (F2, F4, F5, F6, F8c). Filing the RH4102 Fix All bug needs the maintainer's approval; the spec author already lists it as a follow-up. If the maintainer prefers F1 to be filed as a bug as well, that needs the same approval.

## Follow-up candidates

- F1 is defective under the safety policy in `AGENTS.md` ("Prefer no fix over an unsafe one"): the fix produces code that does not compile. It is likely to be decided as a bug.
- The RH4102 mixed-kind Fix All (spec author, `02-spec-author.md`).
