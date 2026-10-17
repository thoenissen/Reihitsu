Role: spec author
Commit: 46f4b0d05c2bab31e6102b35baed672aa476f28c
Verdict: blocking open question

The maintainer's stage gate (`00-lead.md`, clarification 4) applies: six of the seven existing RH4106 requirements state behavior that the other casing rules share. So no spec was written or moved, and `sdd/specs/analyzer/RH4106.md`, `sdd/specs/README.md`, and `sdd/specs/requirement-ids.txt` are unchanged. This handover is the only file in this commit.

## How it was established

I read the code of the four shared types (`CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `CasingUtilities`), `RH4106PrivateFieldCasingAnalyzer.GetLocations`, and the `GetLocations` methods of RH4107–RH4110. I also used `rg` to find which `CasingUtilities` check and conversion every analyzer and code-fix provider under `Rules/Naming` passes to its base:

- `IsUnderlineCamelCase` / `ToUnderlineCamelCase`: RH4106 and RH4107.
- `IsCamelCase` / `ToCamelCase`: RH4105, RH4115, RH4117.
- `IsPascalCase` / `ToPascalCase`: every other casing rule in this batch.
- `NamingAccessibilityClassifier.GetEffectiveAccessibility`: RH4106–RH4109 and RH4111–RH4114.
- `CasingCodeFixProviderBase<T>`: every casing code-fix provider in this batch, which is RH4002–RH4008 and RH4010–RH4110 except RH4007 and RH4008, which have none. Each provider supplies only its conversion and node type.

I ran nothing for this gate. The classification below comes from reading the code.

## Changed requirements

_None._ The gate stopped the run before any spec edit.

## Examples for the tests

_None yet._

## Open questions to file

_None._ (The acronym case `HTTPClient` → `_hTTPClient` belongs to thoenissen/Reihitsu#971 and is not filed again.)

## Decisions needed

### Which RH4106 requirements state shared behavior

| ID | Shared part | Rule-specific part | Affected |
|---|---|---|---|
| RH4106-001 Private fields are checked | How accessibility is classified (`NamingAccessibilityClassifier`): no modifier in a class, struct, or record means private; no modifier in an interface means public; `private protected` counts as protected (RH4107); `protected internal` counts as internal (RH4108). Each field or property gets exactly one owner among RH4106–RH4109 / RH4111–RH4114. The `const` exclusion is shared by RH4106–RH4109, because RH4110 owns `const`. | The private group, and that static, readonly, and static readonly fields are included. | yes, mixed |
| RH4106-002 What `_camelCase` means | The whole definition of the `_camelCase` style (`IsUnderlineCamelCase`), which RH4107 uses too. Its tail after `_` is the camelCase style (`IsCamelCase`) of RH4105, RH4115, and RH4117. | Only that RH4106 applies `_camelCase` to its names. | yes, wholly shared |
| RH4106-003 The diagnostic is on the field name | — | Where the diagnostic goes is chosen by each rule's own `GetLocations` override. The rulebook and SHARED-STMT also leave "where the diagnostic is" to each rule. RH4107–RH4110 make the same choice, but each states it for itself. | **no** |
| RH4106-004 The fix derives the new name from the words of the old one | How words are split and recased (`CasingUtilities.ConvertToPascalCase`: split at non-letter-or-digit characters, capitalize each word's first character, lowercase an all-uppercase word). Every PascalCase, camelCase, and `_camelCase` fix of the batch uses it. Lowercasing the first character and adding `_` is the `_camelCase` conversion, which RH4107 shares too. | That RH4106's target style is `_camelCase`. | yes, mixed |
| RH4106-005 The fix renames every reference | Renaming with the Roslyn renamer, and not offering the fix when the symbol does not resolve (`CasingCodeFixProviderBase<T>.RegisterCodeFixesAsync`, `ApplyCodeFixAsync`). | None. Only the example is RH4106's own. | yes, wholly shared |
| RH4106-006 The fix is withheld when the new name is unusable | The checks in `TryGetFixedIdentifier` (empty, unchanged, or not a valid identifier) and in `HasRenameConflictAsync` / `RenameWithoutConflictAsync` (renamer conflict annotation, a collision with a namespace member, added compile errors in the binding domain). The namespace-member clause only applies to symbols that a namespace contains, which means types, never fields. | None. Only the example is RH4106's own. | yes, wholly shared |
| RH4106-007 Fix All skips names that would collide with each other | All of `CasingFixAllProvider` / `ApplyFixAllAsync` / `GetDuplicateTargetCandidatesAsync`. It differs from the shared code-fix behavior, so it belongs in the shared casing spec, not in `shared/code-fixes.md`. | None. Only the example is RH4106's own. | yes, wholly shared |

Affected: RH4106-001, -002, -004, -005, -006, -007. Not affected: RH4106-003.

The shared-spec IDs in the options use a proposed prefix `SHARED-CASING`, which is not in the ledger yet. The numbering is only for illustration:

- 001 PascalCase
- 002 camelCase
- 003 `_camelCase`
- 004 effective accessibility of fields and properties
- 005 word derivation
- 006 rename of every reference
- 007 fix withheld
- 008 Fix All skips colliding targets

### Option A — retire and move (the RH5002 → SHARED-STMT precedent)

Retire each affected ID with a pointer, and give the rule-specific remainder of RH4106-001 a new ID. RH4106-003 stays as it is. Tests that are tagged with a retired ID get retagged: shared IDs on the tests that verify shared behavior, the new RH4106 ID on the scope tests. `RequirementCoverageTests` fails on a retired tag.

```markdown
### RH4106-001 — Private fields are checked

Status: Retired — the accessibility classification moved to SHARED-CASING-004; the private scope is RH4106-008.

### RH4106-002 — What `_camelCase` means

Status: Retired — moved to SHARED-CASING-003, which states it for every rule that uses the style.

### RH4106-004 — The fix derives the new name from the words of the old one

Status: Retired — moved to SHARED-CASING-005 and SHARED-CASING-003, which state it for every casing rule.

### RH4106-005 … RH4106-007

Status: Retired — moved to SHARED-CASING-006 / -007 / -008, which state it for every casing code fix.

### RH4106-008 — Private non-constant fields are checked against `_camelCase`

The analyzer MUST check every variable declarator of a non-`const` field declaration whose effective accessibility is private as SHARED-CASING-004 defines it, and MUST report it when its name is not in the `_camelCase` style of SHARED-CASING-003. Static, readonly, and static readonly fields are included.
```

`Uses`: SHARED-CASING-003 through -008. Ledger: RH4106-008 plus the SHARED-CASING IDs are added; no line is removed.

### Option B — keep the IDs active, narrow their text to pointers (the RH5001-001 pattern)

Every ID stays active, and no test tag changes. The text of RH4106-001, -002, and -004 through -007 is rewritten so that it only references the shared IDs. That changes existing requirements, so the maintainer has to approve it.

```markdown
### RH4106-002 — What `_camelCase` means

The analyzer MUST accept a name exactly when it is in the `_camelCase` style SHARED-CASING-003 defines, and MUST report every other name.

### RH4106-006 — The fix is withheld when the new name is unusable

The code fix MUST be withheld as SHARED-CASING-007 specifies. The diagnostic is still reported.
```

Downside: RH4106-005, -006, and -007 become pure pointers with nothing of RH4106's own, against the rulebook's "requirements state only what is specific to the rule". Their tests would then cover a shared requirement only through a rule ID.

### Option C — leave RH4106's requirements as they are

Only the code owners move. The shared spec states the behavior for the other rules, and RH4106 lists the shared IDs under `Uses` while keeping its own wording. This breaks "a rule a shared policy already states is referenced, never restated", so two texts of the same behavior can drift apart. Not recommended.

### Recommendation

Option A. It is what the repository already did when it introduced SHARED-STMT (RH5002-002, -003, -005 retired with "moved to SHARED-STMT-00x"). It also keeps RH4106 down to what is its own: the private scope (new RH4106-008) and the diagnostic location (RH4106-003, unchanged).

### Side points for the same answer

1. The RH4106 non-goal "Names that start with an acronym in PascalCase (`HTTPClient`) … is an open question" is an open question written as a non-goal, which `sdd/README.md` (*Non-goals*, *Open questions*) does not allow. Under options A and B, word derivation moves to the shared spec. I propose removing this non-goal from RH4106 and adding no acronym text to the shared spec, since thoenissen/Reihitsu#971 adds that requirement. Please confirm, because it changes RH4106's text.
2. `CasingUtilities.ToUnderlineCamelCase` is also called outside the casing rules, by `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`. The rulebook says "a general-purpose utility [is a code owner] to none". The clarification explicitly says to move `CasingUtilities` to the shared casing spec, and I will follow it unless the maintainer says otherwise. I am recording it only so the answer covers it.
