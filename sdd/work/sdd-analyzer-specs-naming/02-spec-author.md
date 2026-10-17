Role: spec author
Commit: 358e59b5c8c43854ae4126f6ef12c6462d59147a
Verdict: spec written

The specs follow the maintainer's answer to escalation 1 (`squad/decisions/2026-10-17-rh4106-shared-casing-move.md`):

- No RH4106 ID is retired. All seven IDs are rewritten.
- Shared behavior is stated once, in `sdd/specs/shared/casing.md`.
- The acronym non-goal is removed.
- `CasingUtilities` is a code owner of the shared spec.

No change doc: intended behavior does not change, so the lane rules require none. No rule page under `documentation/rules/` changes, because the documented behavior is the same.

## Files

- New shared spec: `sdd/specs/shared/casing.md` (SHARED-CASING-001 … 010). Code owners: `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `NamespaceCasingHelper`, `CasingUtilities`. `NamespaceCasingHelper` is new as a code owner; it implements the segment-by-segment check that RH4007 and RH4008 share.
- New rule specs under `sdd/specs/analyzer/`: RH4001–RH4011 and RH4101–RH4105, RH4107–RH4110 (20 rules).
  - RH4001 also owns `RH4001TypeNameShouldMatchFileNameHelper`.
  - RH4102 owns both of its code-fix providers.
- Rewritten: `sdd/specs/analyzer/RH4106.md`. Its header now lists only the rule's own two types; `Uses` lists SHARED-CASING-003, 004, 006 to 010.
- `sdd/specs/requirement-ids.txt`: 55 IDs added in ordinal order, none removed. `sdd/specs/README.md`: 21 rows added and the RH4106 row updated.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`:

- `SpecConsistencyTests` and `SpecStructureTests` pass: 38 passed.
- `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails with 55 uncovered IDs. These are exactly the new IDs, and they become covered once the tests are tagged.

## Changed requirements

Added (every requirement of the new specs):

- SHARED-CASING-001 PascalCase; -002 camelCase; -003 `_camelCase`; -004 accessibility groups of fields and properties; -005 namespace names checked segment by segment; -006 how the fix derives the new name; -007 the fix renames the declaration and every reference; -008 fix withheld when no usable name can be derived; -009 fix withheld when the new name would collide; -010 Fix All skips names that would collide with each other.
- RH4001-001 … -006.
- RH4002 / RH4003 / RH4004 / RH4006 / RH4010 / RH4011 / RH4101 / RH4104 / RH4107 / RH4108 / RH4109 / RH4110: -001 (what is reported) and -002 (the fix).
- RH4005-001, -002.
- RH4007-001 and RH4008-001 (no code fix).
- RH4009-001, -002.
- RH4102-001 … -003, RH4103-001 … -003, RH4105-001 … -003.

RH4106 changed. The old and new meaning of each ID, for retagging:

| ID | Old meaning | New meaning | Tagging consequence |
|---|---|---|---|
| RH4106-001 | Private fields are checked, including the accessibility classification and the `const` exclusion | Non-`const` fields of the private group (SHARED-CASING-004) are checked, static and readonly included | Scope tests keep the tag. The tests for implicitly private fields and for `private protected` also cover SHARED-CASING-004 |
| RH4106-002 | Definition of `_camelCase` | A checked field is reported when its name is not `_camelCase`; the definition is now SHARED-CASING-003 | Tests keep the tag. `VerifyNoDiagnosticsForUnderlineCamelCasePrivateField` also covers SHARED-CASING-003 |
| RH4106-003 | One diagnostic per declarator, on its identifier | Unchanged | No change |
| RH4106-004 | The word-splitting algorithm of the fix | The fix renames to the `_camelCase` name; the algorithm is now SHARED-CASING-006 | Tests keep the tag. Tests that exercise separators or all-uppercase words cover SHARED-CASING-006 |
| RH4106-005 | Rename through the Roslyn renamer; not offered when the symbol is unresolved | The fix renames the field and every reference as SHARED-CASING-007 specifies | Test keeps the tag, and may also tag SHARED-CASING-007 |
| RH4106-006 | Full list of the withholding conditions | The fix is withheld as SHARED-CASING-008 and -009 specify | `NoCodeFixForLetterlessPrivateField` keeps the tag and also covers SHARED-CASING-008 |
| RH4106-007 | The full Fix All collision rule | Fix All skips colliding fields as SHARED-CASING-010 specifies | The `FixAllCorrectsEveryDiagnostic` override keeps the tag and also covers SHARED-CASING-010 |

## Examples for the tests

How the examples were checked:

- I ran every C# example of the 20 rule specs, the shared spec, and RH4106 against the shipped analyzers and code fixes. The harness is a scratch file-based app outside the repository: it runs the analyzer, applies the single fixes one after another, and runs each provider's Fix All in solution scope.
- Every reported span matched the `{|RH####:…|}` markup.
- Every "after" block matched the result of the single fixes. The only exceptions are the documented Fix All-only results of SHARED-CASING-010 and RH4106-007, which matched the Fix All result.
- The examples of RH4001 and RH4009 need file names and a `reihitsu.json`, so I verified them as separate fixtures.

Examples that an existing test already shows can be tagged directly. All other examples need a focused test:

- **Tag existing tests:**
  - RH4106 tests: see the table above.
  - RH4001: the reported-name, Razor, XAML, split-partial, timestamp, generic-rename, and existing-target tests cover RH4001-002, -004, -005, -006. `LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed` covers -001.
  - RH4002–RH4011, RH4101–RH4110: the existing report and fix tests cover each rule's -001 and -002 (the RH4102/RH4103/RH4105 IDs as named in their specs).
  - The `GetFixAllScenario` cases cover:
    - RH4109: SHARED-CASING-009.
    - RH4105: SHARED-CASING-010, the different-containing-symbol side.
    - RH4103: SHARED-CASING-007.
  - RH4115 tests:
    - `DuplicateNormalizedTargetsInSiblingScopesRemainFixableTogether` covers SHARED-CASING-010, the sibling-scope side.
    - `TopLevelTypeCollisionAcrossDocumentsIsRefused` covers SHARED-CASING-009.
  - `NoCodeFixForLetterlessMethodParameter` covers SHARED-CASING-008.
  - RH4007/RH4008 segment tests cover SHARED-CASING-005.
  - `CasingUtilitiesTests` (Core.Test) cover SHARED-CASING-001/002/003/006.
- **New focused tests needed** (verified only by running):
  - SHARED-CASING-001 (`Order_Line`, `_Invoice`, `HTTPClient` accepted).
  - SHARED-CASING-002.
  - SHARED-CASING-003 (`_1st`, `__double`, `_` accepted).
  - SHARED-CASING-004:
    - struct/record/interface defaults through RH4106;
    - nested private class and interface static field through RH4109;
    - explicit interface property through RH4111.
  - SHARED-CASING-006: the three examples.
  - SHARED-CASING-008: `_1st` through RH4105 and RH4106.
  - SHARED-CASING-009:
    - `Value` next to `_value`;
    - the RH4002 cross-file cases (same-arity type withheld, `order<T>` fixed, `file class Order` in another file fixed, in the same file withheld).
  - SHARED-CASING-010: the RH4107 `Total`/`total`/`Limit` group.
  - RH4001-001: `Outer.cs` with a nested and a later type; delegate first; namespace only.
  - RH4001-002: `testclass.cs` reported.
  - RH4001-003: `Cache{TKey,TValue}.cs` accepted, `Cache.cs` reported.
  - RH4005-001: `IHTTPClient` accepted; `IOrder_Line` and `I` reported.
  - RH4005-002: `IOrder_Line` → `IOrderLine`.
  - RH4009-001: missing `allowedNamespaceDeclarations`.
  - RH4009-002: child namespace, different case, file-scoped declaration.
  - RH4102-001: a field-like event declaration with two declarators; an interface event; a static event; an event with accessors, all in one class.
  - RH4103-002: the fix renames the explicit implementation with the interface method.
  - RH4103-003: `run_all` → `RunAll`.
  - RH4105-001: delegate, operator, class primary constructor, simple lambda `Number => Number`.
  - RH4105-002: lambda discards `(_, _)`.
  - RH4105-003.
  - RH4106-001, -002, -003 examples.
  - RH4107-001, RH4108-001, -002, RH4109-001, RH4110-001 examples.
  - RH4004-002, RH4006-002, RH4010-002 (`orderrecord2`), RH4101-002 combined examples.

Qualifiers stated as explicit conditions:

- SHARED-CASING-004 lists every modifier combination, the interface default, explicit interface properties, and that the containing type does not count; there is an example for each.
- SHARED-CASING-009 states the type-parameter count and the file-local conditions, each with examples on both sides.
- SHARED-CASING-010 states same target, same containing symbol, and conflicting when composed, with three examples.
- SHARED-CASING-006 limits the camelCase and `_camelCase` derivation to a first word that has no uppercase second character or is all uppercase. The other side is the leading-acronym case that thoenissen/Reihitsu#971 decides, so it has no example here.

## Open questions to file

None of the specs below has an open-question issue yet, so each needs a new `SDD open questions: <spec path>` sub-issue of thoenissen/Reihitsu#912. I verified every input by running it against the shipped analyzer and fix.

### `SDD open questions: sdd/specs/analyzer/RH4009.md`

- **Q1 — A nested namespace declaration is compared by its own name only.**
  - Input: `namespace A { namespace B { } }` with `"allowedNamespaceDeclarations": [ "A", "A.B" ]`.
  - Real output: `B` is reported, although the namespace it declares, `A.B`, is allowed.
  - Why it looks wrong: the rule page says the comparison uses "the full declared namespace". With the list `[ "A", "B" ]`, nothing would be reported for `A.B`.
  - Touches RH4009-002, which covers only declarations at file level for this reason.
- **Q2 — Whitespace or comments inside the name defeat the comparison.**
  - Input: `namespace A . B { }` with `[ "A.B" ]`.
  - Real output: `A . B` is reported. The name is compared as written, including the trivia between its tokens.
  - Touches RH4009-002.

### `SDD open questions: sdd/specs/analyzer/RH4005.md`

- **Q1 — An `i`/`I` followed by a non-letter, or a bare `I`, is not taken as the prefix, so the fix doubles it.**
  - Input: `interface i_document`, `interface I_reader`, `interface I`, `interface i2Factory`.
  - Real output: the fix renames them to `IIDocument`, `IIReader`, `II`, `II2Factory`.
  - Why it looks wrong: the result carries two `I`s. A reader would expect `IDocument`, `IReader`, and `I2Factory`. For `I` alone there is no name to keep.
  - Touches RH4005-002, whose conditions leave these inputs out.

### `SDD open questions: sdd/specs/analyzer/RH4001.md`

- **Q1 — A verbatim identifier is compared with its `@`.**
  - Input: file `class.cs` with `public class @class { }`.
  - Real output: reported, and the fix renames the file to `@class.cs`.
  - Why it looks wrong: the type's name is `class`; the `@` only escapes the keyword.
  - Touches RH4001-002 and RH4001-005.
- **Q2 — The fix drops the timestamp prefix of a migration file.**
  - Input: file `20210501083748_InitialCreate.cs` with `public class SomeMigration { }`.
  - Real output: the fix renames the file to `SomeMigration.cs`.
  - Why it looks wrong: the analyzer ignores the prefix because Entity Framework Core needs it to order migrations, but the fix removes it. `20210501083748_SomeMigration.cs` would keep the order.
  - Touches RH4001-005, which covers only file names without the prefix for this reason.

### `SDD open questions: sdd/specs/analyzer/RH4103.md`

- **Q1 — An override of a method from a referenced assembly is reported, and the diagnostic cannot be fixed.**
  - Input: a referenced assembly declares `public class Base { public virtual void process() { } }`; the source has `public class Derived : Base { public override void process() { } }`.
  - Real output: `process` in `Derived` is reported, and no code fix is offered.
  - Why it looks wrong: the name is dictated by the base type, which the author does not control. An explicit interface implementation is excluded for the same reason.
  - Touches RH4103-001, which covers only methods without `override` for this reason.

## Follow-up candidates

- **Possible bug against SHARED-FIX-002, not an open question.** For a document with both a field-like event and an event with accessors that are reported (for example `public event EventHandler dataChanged;` and `public event EventHandler saved { add { } remove { } }`), Fix All corrects only the events of the kind it was invoked from:
  - Fix All from `RH4102EventNameCasingCodeFixProvider` leaves `saved`.
  - Fix All from `RH4102CustomEventNameCasingCodeFixProvider` leaves `dataChanged`.
  - The single fixes correct both.
  - SHARED-FIX-002 requires one Fix All application to correct every diagnostic, with the same result as the single fixes.
  - RH4102's spec states no deviation and shows the two kinds in separate examples. Filing a bug needs the maintainer's approval.
- `scripts/lane-check.sh` classifies `squad/decisions/` as tooling (already noted by the lead in `00-lead.md`).

## Decisions needed

_None._
