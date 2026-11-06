Role: skeptic
Commit: 40108eb
Verdict: #1070: proceed narrower; #1073: proceed

How the facts below were established: `scripts/apply-fix.sh <RH####> <fixture-dir>` on fixtures in a scratch directory outside the repository (LF and CRLF arms agreed in every run; fixtures deleted afterwards), reading `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` (`RegisterCodeFixesAsync`, `PrepareRenameAsync`, `RenameWithoutConflictAsync`, `AnnotateBindingDomainsAsync`, `HasNamespaceCollision`, `ApplyFixAllAsync`), and the issues thoenissen/Reihitsu#1070, #1073 with their source questions #1009 (Q1, Q4, Q5), #1013, #1014, #1023, #1025, and the split-off #1434. `git log` shows no change to the casing fix base since the source questions were run (2026-10-17), so their recorded outputs still describe `main`; every single-file instance was re-run here. The cases that need a metadata reference cannot be run through `apply-fix.sh` (its project references only the host's runtime assemblies); for them the recorded output of #1013 Q1, #1014 Q1, #1025 Q1 is the evidence, and the mechanism below explains it from the code.

The lead handover repeats the issue's suspected cause for #1009 Q1 ("the compile-error check is skipped for namespace-level types"). I checked it against the code independently rather than taking it as given; it holds, and it is one member of a wider mechanism (*Whole run*).

## thoenissen/Reihitsu#1070

### Premise

Scope after the triage comment on #1070: fix safety only; the no-report part is #1434 and the diagnostics stay. Within that scope the issue is real, but narrower than its instance list suggests.

- **Native entry point — real.** Caller: the RH4103 code fix (single and Fix All), on any `extern` method with `[DllImport]` and no `EntryPoint`. Input `[DllImport("libc")] internal static extern int getpid();` plus a call: the fix is offered and renames method and call to `Getpid`. The program compiles and fails at run time with an entry-point-not-found error, because the runtime looks the native function up by the method's name. Re-run here; matches #1013 Q2.
- **Override or implementation of a metadata member — real only through a source cascade.** Per #1013 Q1, #1014 Q1, #1025 Q1 (run against this code): a member that itself overrides or implicitly implements a metadata member (`Impl.process`, `Middle.process`, `Derived.changed`, `Derived.opened`, `Impl.total`, `Middle.count`) already gets **no** fix — the existing added-error check sees CS0115/CS0534/CS0535 inside the member's own type. The defect is the member one level further down a source chain: `Leaf.process` (and `Leaf.changed`, `Leaf.count`) gets a fix whose rename cascades to `Middle`, and `Middle` then fails with CS0115 (events also CS0534). Caller: RH4103, RH4102, RH4112, RH4113, RH4114 code fixes, in a solution that references a compiled assembly declaring a non-PascalCase virtual or interface member (a package or DLL reference; a project reference makes the base source and the renamer renames it too).
- **Tuple element names (#1023 Q1) — not reachable for this issue.** RH4116 has no code fix (`src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/` has no RH4116 provider, and `GetDeclaredSymbol` excludes tuple elements). Nothing to withhold; the instance belongs entirely to #1434.

### Mechanism and candidates

Two mechanisms.

1. **Compile errors outside the checked domain** (shared with #1073 Q1; see *Whole run*): the fix looks for added compile errors only inside the declarations of the renamed symbol's containing symbol, while the Roslyn renamer also renames every source member of the symbol's override/implementation cascade, in other types and documents, and cannot rename the cascade members declared in metadata. An error the rename adds in another type of the cascade is not seen.
2. **Run-time binding by name**: the fix treats every compiling rename as safe, but an `extern` method whose native entry point is taken from its own name binds to a different native symbol after the rename.

Dispatch: the renamed symbol comes from `CasingCodeFixProviderBase<T>.GetDeclaredSymbol` (method, property, field-like event declarator, custom event, local function, parameter, type parameter, ...); the cascade is Roslyn's rename cascade (overridden, overriding, implemented, implementing members). Candidates for mechanism 1 are the member kinds that can override or implicitly implement: method (RH4103), property (RH4112 protected, RH4113 internal across assemblies via `InternalsVisibleTo`, RH4114 public; RH4111 private cannot override), field-like event (RH4102 `VariableDeclaratorSyntax`), custom event (RH4102 `EventDeclarationSyntax`). Parameters (RH4105) and method type parameters (RH4121) of an override may differ in name from the base, so a rename there adds no error — not candidates. Candidates for mechanism 2: method (RH4103) and local function (RH4104), the two declarations that can be `extern` with `[DllImport]` or `partial` with `[LibraryImport]`.

### Breaking inputs

Failure classes: FC-001 (the added-error check inspects a smaller region than the rename edits), FC-016 (sibling shapes), FC-024 (`partial` LibraryImport methods), FC-036 (a rename reaches source generators and native binding), FC-033/FC-034 (see test notes), FC-008 (Fix All shares the path).

Matrix, mechanism 1 — axes: member kind × relation to the metadata member × where the breaking source member sits.

| Member kind | Relation | Breaking member is the renamed one | Breaking member is another source type in the cascade |
|---|---|---|---|
| method | override chain (`Leaf : Middle : MetaBase`) | holds (no fix today) | **fails** — CS0115 in `Middle` (#1013 Q1) |
| method | interface implemented by a source base (`Middle : IMeta { public virtual void process() }`, `Leaf : Middle` overrides) | holds (CS0535 in own type) | **fails, derived** — CS0535 in `Middle` |
| method | implementation inherited by a source derived type (`class A { public void process() {} }`, `class B : A, IMeta {}`; fix on `A.process`) | — | **fails, derived** — CS0535 in `B`. The same shape with `IMeta` in source holds: the renamer renames the interface member too (run here) |
| property (public, protected, internal + IVT) | override chain | holds | **fails** — CS0115 in `Middle` (#1025 Q1; RH4112/RH4113 per #1026/#1027) |
| property | the two interface shapes above | holds | **fails, derived** |
| field-like event | override chain of an abstract metadata event | holds | **fails** — CS0115, CS0534 in `Middle` (#1014 Q1) |
| custom event (`add`/`remove`) | override chain | holds | **fails, derived** |
| any | source interface member renamed whose implementation in some type is inherited from a metadata base class | — | **fails, derived** — CS0535 in the implementing type |

"Derived" members follow from the mechanism and were not run (they need a metadata reference); the reproduction must run them. Must stay: a chain declared entirely in source is renamed together (re-run here: `Base`/`Middle`/`Leaf` all become `Process`); a direct metadata override keeps getting no fix.

Matrix, mechanism 2 — axes: declaration × import attribute × how the entry point is named.

| Declaration | Import | Entry point | Today | Expected |
|---|---|---|---|---|
| method | `[DllImport]` | none | fix offered (run) | **fails** |
| method | `[DllImport]` | `EntryPoint = "getpid"` | fix offered (run) | holds — keep the fix |
| method | `[DllImport]` | `EntryPoint = nameof(getpid)` | fix offered, `nameof` renamed, entry point becomes `"Getpid"` (run) | **fails** — the entry point is still the method's own name |
| method | `[LibraryImport]` (`partial`) | none | fix offered (run) | **fails** — the generator regenerates with the new name; not observable without the generator |
| method | `[LibraryImport]` | `EntryPoint` literal | — | holds |
| local function (RH4104) | `[DllImport]` | none | fix offered (run) | **fails** |

Not members: `extern` without an import attribute (it fails at run time before and after the rename), `[MethodImpl(InternalCall)]` (runtime-internal only), the parameters of a P/Invoke method (not bound by name).

Test-implementer notes: the metadata cases need a compiled image reference (an emitted assembly added with `AddMetadataReferences`, as `RH4115LocalVariableCasingAnalyzerTests` builds its own workspace), never a project reference — a project reference makes the base source, the renamer renames it, and the reproduction goes green for the wrong reason (FC-033, FC-034). RH4113 needs `InternalsVisibleTo` with a matching assembly name. A `[LibraryImport]` fixture compiles with CS8795 before and after the rename when no generator runs; the test must assert the withheld fix, not compile cleanliness. Each failing member needs the single fix and Fix All (SHARED-CASING-010 runs through the same `RenameWithoutConflictAsync`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingCodeFixProviderBase<T>` (single fix and Fix All, one shared path) | must change |
| Per-rule fix providers RH4102, RH4103, RH4104, RH4112–RH4114 | not involved, unless the lead places a guard there |
| Analyzers RH4102, RH4103, RH4112–RH4114, RH4116 | must stay — the diagnostics stay (#1434 owns not reporting) |
| RH4116 | not involved — it has no code fix |
| Rule pages | must stay — "a diagnostic without a code fix" is in `documentation/rules/general-notes.md` |
| Specs | spec clarification needed: SHARED-FIX-003 run-time bullet (see *Output questions*); RH4104 fix coverage (it has no `extern` exclusion, unlike RH4103's "neither `extern` nor `[LibraryImport]`") |
| Formatter, CLI, `Reihitsu.Core` | not involved |
| Tests | must change — analyzer tests per rule, a metadata-reference workspace for mechanism 1 |

### Simpler option

_None._ The direct metadata override is already handled; what remains is the cascade case and the native binding, both narrower than the issue's instance list.

### Output questions

_None open._ One case leans on a reading the lead should confirm before tests: SHARED-FIX-003's run-time bullet says "the new name ... that the fix exists to produce does not count", and its own example accepts that `nameof(Value)` changes its value. Read literally, the native entry point that follows the method's name is "the new name", and the DllImport rename is allowed. The maintainer's principle decisions P1 and P2 (#912) and the decision on #1013 Q2 treat the name as dictated from outside and the run-time failure as a defect, and the triage comment on #1070 keeps exactly this case in the fix-safety scope. I consider the withheld fix covered by those decisions; the spec needs a clarification that a binding to a native entry point by the method's name is not the new name. If the lead does not accept the decisions as covering it, ask:

```csharp
// RH4103 · Method names should be in PascalCase — P/Invoke method bound by its name
using System.Runtime.InteropServices;

internal static class Native
{
    [DllImport("libc")]
    internal static extern int getpid();
}
// Before (main): fix offered, renames to Getpid (program then fails with EntryPointNotFoundException)
// After, A (recommended): no fix offered; the diagnostic stays
// After, B: fix offered as today
```

### Internal choices

- **Where the cascade check looks.** Option 1: every declaration the rename changes (all changed documents' declarations of cascade members). Option 2: only the declarations of each renamed cascade member's containing type. Both withhold the failing members above and differ only in cost; recommend 2 as the narrower region that still covers every member, unless the implementer finds a cascade member whose error lands elsewhere.
- **`EntryPoint = nameof(self)`**: recommend treating it as "binds by its own name" (withhold), since the entry point value is the method's name in both readings.

### Issue goals

- "None of the inputs above is reported" and "a source-declared override chain still is [reported]" — drop: moved to #1434 by the triage; the diagnostics stay. No visible change from dropping (it removes a requested change).
- "regression test tagged with SHARED-CASING-011" — change to SHARED-FIX-003 (and SHARED-CASING-009 where the spec author amends it); SHARED-CASING-011 does not exist. Internal.
- #1023 Q1 (RH4116) — drop from this run: no fix exists, nothing to withhold. Not visible.
- Instances whose fix is already withheld (direct overrides) — keep as characterization tests only, not as defects.

### Architecture findings

See *Whole run* (shared with #1073 Q1).

### Follow-up candidates

_None_ beyond #1434, which already exists.

## thoenissen/Reihitsu#1073

### Premise

All three instances reproduce on this commit; caller in each case is the named code fix (single and Fix All), on ordinary single-file code.

- **Q1** — RH4002 `namespace N { class order { public int Order; } }`: fix offered → `class Order { public int Order; }` (CS0542). Also offered and failing (run here): `class order<Order>` → `Order<Order>` (CS0694); global namespace `class order { public int Order; }`; file-scoped namespace with method `Order()`; nested type `class Order` inside `order`; `partial class order` with the member in the second part; RH4003 struct; RH4005 `interface iOrder { void IOrder(); }` → `IOrder`; RH4006 `delegate void handler<Handler>()`; RH4010 `record order(int Order)`; RH4011 `record struct order(int Order)`. The CS codes for the interface, record and record-struct members are derived (member/synthesized property named like the type), not compiled here. Holds: RH4004 `enum color { Color }` → `enum Color { Color }` compiles, as SHARED-CASING-007 states.
- **Q4** — RH4106 field referenced in an `#if NEVER` branch: fix offered, only the active reference renamed. Also failing (run): an inactive `#elif` branch, `#if false`, RH4115 local referenced in an inactive branch, RH4002 type referenced in an inactive member, a parallel declaration of the same field in the inactive `#else`, and an inactive declaration of the **new** name (`#if NEVER private int _value; #endif` next to `Value` → duplicate member in that build). Holds (existing test `FixLeavesAnExcludedBranchThatDoesNotNameTheFieldUnchanged`): an inactive branch that does not name the symbol keeps the fix.
- **Q5** — `public record R(int count) { public int count { get; init; } = count; }`: RH4120 fix → `R(int Count) { public int count ... = Count; }`, RH4114 fix → `public int Count ... = count;`; in both the compiler now synthesizes a second public property. Also RH4111 (private property), RH4109 (field), record struct (RH4120, RH4114) — all run, all offered.

### Mechanism and candidates

- **Q1**: for a symbol declared directly in a namespace the fix checks only collisions with other namespace members and skips the added-error check, so an error the rename adds inside the renamed type's own declarations (a member, nested type, type parameter, or synthesized positional property now named like the type) is not seen. Same root as #1070 mechanism 1 (*Whole run*). Candidates: every type declaration whose containing symbol is a namespace — class, struct, interface, enum, delegate, record, record struct (RH4002–RH4006, RH4010, RH4011), in a block, file-scoped, or global namespace, partial or not.
- **Q4**: the renamer renames bound references only; disabled text is not bound, and the fix never looks at it, so any occurrence of the old name — or a declaration of the new name — in an excluded branch is left inconsistent for another build. Candidates: every symbol kind the casing fixes rename (`GetDeclaredSymbol` dispatch: types, enum members, events, methods, local functions, parameters, fields, properties, locals, deconstruction variables, type parameters).
- **Q5**: a record's positional parameter is matched by name to a member declared in the record or inherited from its base; the compiler synthesizes a property only when no such member exists. A rename that changes whether the names match adds or removes a synthesized public member (equality, `ToString`, `Deconstruct`, public API), and compiles. Candidates: RH4120 (parameter), RH4111–RH4114 (property), RH4106–RH4110 (field), on the record itself or its base record.

### Breaking inputs

Failure classes: FC-001, FC-016, FC-020 (including `#if false`), FC-024 (partial types), FC-036 (public API change through a rename), FC-006, FC-008.

Q1 matrix — axes: type kind × namespace form × what gets the type's name.

| Type kind | Colliding name | Result |
|---|---|---|
| class | field / method / nested type / member in another partial part / type parameter | **fails** (run) — CS0542 / CS0694 |
| struct | field | **fails** (run) |
| interface | method | **fails** (run; CS0542 derived) |
| delegate | type parameter | **fails** (run) — CS0694 |
| record, record struct | positional parameter (synthesized property) | **fails** (run; CS0542 derived) |
| enum | enum member | holds — compiles |
| any | inherited member with the new name (`class order : Base` where `Base` declares `Order`) | holds, derived — CS0542 applies to declared members only |
| any | a reference elsewhere that would rebind (`class X { class Order {} order o; }`) | holds, derived — the renamer reports a conflict; test as a must-stay case |

Namespace form (block, file-scoped, global) does not change the result (all three run, all fail). The nested-type counterpart is already withheld (SHARED-CASING-009 example).

Q4 matrix — axes: what is in the excluded text × branch kind × symbol kind.

| Excluded text holds | Branch | Result |
|---|---|---|
| a reference to the old name | `#if`, `#elif`, `#else`, `#if false` | **fails** (run: `#if`, `#elif`, `#if false`) |
| a second declaration of the old name | `#else` | **fails** (run) |
| a declaration of the new name | `#if` | **fails** (run) — duplicate member in that build |
| no occurrence of either name | any | holds (existing test) |

Symbol kinds run: field (RH4106), local (RH4115), class (RH4002); the others share the path and need at least one test per symbol family (type, member, local/parameter). Location axis: same member, another member, another document (a public symbol referenced in another file's excluded branch) — the last is derived, not run.

Q5 matrix — axes: renamed side × direction × where the counterpart is × record kind.

| Renamed side | Direction | Counterpart | Result |
|---|---|---|---|
| positional parameter (RH4120) | split | property / field in the record | **fails** (run) |
| property (RH4111, RH4114; RH4112, RH4113 share the path) | split | positional parameter | **fails** (run) |
| field (RH4109; RH4106–RH4110 share the path) | split | positional parameter | **fails** (run) |
| positional parameter of record struct / property of record struct | split | — | **fails** (run) |
| derived positional parameter `record D(int count) : B(count)` with `B(int Count)` | join | inherited property | **fails** (run) — `D.count` disappears |
| base positional parameter `record B(int count)` with `D(int Count) : B(Count)` | join | inherited by `D` | **fails** (run) — `D.Count` disappears |
| base and derived both `count` (`B(int count)`, `D(int count) : B(count)`) | split, single fix on either | — | **fails** (run: the runner needed two fix iterations, and each single step separates them; applied until clean, both end renamed as a matching pair) |
| private property `count` next to `record R(int Count)` (RH4111) | join | positional parameter | **fails** (run) — synthesized public `Count` replaced by the private one, or a compile error |
| positional parameter with no same-named member anywhere | — | — | holds — parameter and synthesized property renamed together; must stay offered |
| class or struct primary-constructor parameter with a same-named field | — | — | holds, derived — no property is synthesized |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingCodeFixProviderBase<T>` (single fix and Fix All) | must change |
| Per-rule fix providers | not involved, unless the lead places a guard there |
| All casing analyzers | must stay — diagnostics stay |
| Rule pages | must stay (`general-notes.md` covers diagnostics without a fix) |
| Specs | spec clarification needed: SHARED-CASING-007 states coverage only for namespace-level types without a same-named member (to be decided now); RH4120-001 covers a parameter that matches an *inherited* name only by exclusion, so its fix requirement reads as demanding the join renames above; RH4111–RH4114 carry a coverage note for positional names, RH4106–RH4110 do not |
| RH2001 fix (uses the renamer) | not involved — same Q4 defect, out of scope (*Follow-up candidates*) |
| Formatter, CLI, `Reihitsu.Core` | not involved |
| Tests | must change — RH4106 already has a holding test for Q4 |

### Simpler option

_None._ Every instance needs its fix withheld; the class is wider than the three instances, not narrower.

### Output questions

_None._ Q1 and Q4 are compile errors and excluded-code references, which SHARED-FIX-003 names outright. Q5's added or removed public member is a run-time change that is not the new name, and the maintainer decided #1009 Q5 as "withhold (P1)". The join members (inherited match, private property) are the same mechanism in the other direction and are decided by the same unchanged SHARED-FIX-003; RH4120's fix requirement, which reads as demanding them, needs the spec author's narrowing, not a maintainer answer — SHARED-FIX-003 applies to every code fix (`shared/code-fixes.md`, opening paragraph).

### Internal choices

- **Precision of the excluded-code check.** Excluded text is not bound, so an occurrence of the old name there cannot be told apart from an unrelated symbol of the same name. Option 1: withhold when the old or new name occurs as an identifier in excluded text of any document where the symbol is visible. Option 2: only in the documents the rename changes. Option 2 misses a public symbol referenced only in another file's excluded branch; recommend 1, accepting that an unrelated same-named identifier also withholds the fix (safe direction, `AGENTS.md`, *Code-fix coverage*).
- **Fix All for the base/derived record pair**: per SHARED-FIX-002 the Fix All result is the result of the single fixes offered on the starting code; with both single fixes withheld, Fix All renames neither, although renaming both together would end in a matching pair. Recommend following SHARED-FIX-002 (withhold both); renaming the pair together would be new behavior.

### Issue goals

- "Each input keeps its diagnostic and gets no fix, with regression tests tagged SHARED-FIX-003" — keep, and extend to the class members above (Q1 all type kinds, Q4 the new-name declaration and the parallel declaration, Q5 the join direction and the inherited counterpart). Extending puts no new output in question beyond what SHARED-FIX-003 already decides.
- "blocked by #1068" — stale; SHARED-FIX-003 is on `main`.

### Architecture findings

See *Whole run*.

### Follow-up candidates

- **RH2001 fix leaves excluded-code references behind.** `private int Value { get; set; }` referenced in `#if NEVER` code: the RH2001 fix converts it to `private int _value;` and renames only the active reference (run here). Same class as Q4, different component; the issue manager should check #1286 (RH2001 fix) before filing.

## Whole run

### Architecture findings

**One rule stated in two places, with the narrower statement implemented.** SHARED-FIX-003 withholds a fix whose result has *any* compile error the code did not have, in every build configuration. SHARED-CASING-009 restates the compile-error part for casing fixes but scopes it to "inside the declarations that contain the renamed symbol" for non-namespace symbols and to namespace-member collisions for namespace symbols, and SHARED-CASING-007 records the namespace-level self-collision as uncovered. `CasingCodeFixProviderBase<T>` implements the casing statement (`AnnotateBindingDomainsAsync` → containing symbol only; `PrepareRenameAsync` skips the check when the containing symbol is a namespace). The places where the two statements diverge are exactly #1073 Q1 (errors inside the renamed type itself) and #1070 mechanism 1 (errors in other types of the rename cascade). Patching each region separately would add a third partial statement of the same rule; the lead decides whether the casing spec keeps its own compile-error clause or refers to SHARED-FIX-003. Q4, Q5 and the native-entry-point case are separate mechanisms (excluded text, positional matching, run-time binding) and not part of this finding.

### Commands

```shell
scripts/apply-fix.sh RH4103 <fixtures>   # DllImport (+EntryPoint, nameof), LibraryImport, source override chain, interface via base
scripts/apply-fix.sh RH4104 <fixtures>   # local DllImport function
scripts/apply-fix.sh RH4002|RH4003|RH4004|RH4005|RH4006|RH4010|RH4011 <fixtures>   # Q1 type kinds
scripts/apply-fix.sh RH4106|RH4115|RH4002 <fixtures>   # Q4 excluded branches
scripts/apply-fix.sh RH4120|RH4114|RH4111|RH4109 <fixtures>   # Q5 split and join
scripts/apply-fix.sh RH2001 <fixture>   # follow-up candidate
```

All exits 0 (every fixture reached "fixed" or "no-diagnostic"); the outcomes are as stated per issue above.
