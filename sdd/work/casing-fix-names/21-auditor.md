Role: auditor
Commit: a85f8e09ef22df99b35cb5708fda83e800ba4d82
Verdict: #971: pass; #1072: pass; #1451: pass; #1434: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md` (*Maintainer answers*, *Decisions taken* 4 and 5, *Follow-up candidates*), `16-auditor.md`, `17-test-implementer.md`, `18-implementer.md`, `19-cleaner.md`, `20-spec-checker.md`, the repair diff `c94249b..a85f8e0`, SHARED-CASING-014, and these files outside the diff: `CasingAnalyzerBase.IsDictatedMember`/`GetImplementedInterfaceMembers`, and `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync`.

I ran the probes in a detached worktree of `a85f8e0` under the scratchpad, which I removed afterwards. They were one temporary MSTest class in `Reihitsu.Analyzer.Test`, run with `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~AuditProbeRH4116Tests"`. The only change in the shared tree is this handover.

## thoenissen/Reihitsu#971

The repair does not touch #971. The only change in `CasingUtilities` is the summary of `SplitIntoPascalCaseWords`, and the cleaner proved it text-only (`verify-text-only.sh`: PASS). Every section is unchanged from `16-auditor.md`.

## thoenissen/Reihitsu#1072

The repair does not touch #1072. `CasingRenameSafetyUtilities` and `CasingCodeFixProviderBase{T}` are byte-identical to `c94249b`. Every section is unchanged from `16-auditor.md`.

## thoenissen/Reihitsu#1451

### Scope
Repair delta against `c94249b`, for finding #4.

Previous findings: **#4 is closed.** The summary of `SplitIntoPascalCaseWords` (`src/Reihitsu.Core/CasingUtilities.cs:337-341`) now says the first character of a word is "written in uppercase when it is a letter and kept as written otherwise". That matches `ToPascalCaseWord` (`char.IsLetter`). The change is comment-only, and the cleaner proved it with `scripts/verify-text-only.sh --base 6b145e1 --head worktree` (PASS). It is not a line-based grep (FC-037).

### Findings
_None._

### Guard scope
_N/A: the repair changes no predicate for #1451._

### Policy ownership
_N/A: no policy of #1451 changed._

### Assertion adequacy
_N/A: no test of #1451 was added or changed._

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1434

### Scope
Repair delta against `c94249b`, for finding #3 and its architecture finding (lead decision 4). Head `a85f8e09ef22df99b35cb5708fda83e800ba4d82`, base `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b`.

The repair stays inside RH4116 and its tests. No spec, change doc, or rule page changed, so scope did not grow and this is a delta audit.

The new mechanism has three parts:
- `GetOutermostType` climbs from the tuple type to the outermost type that holds it.
- `GetDeclaredMembers` asks the compiler for the symbols declared by that type's parent: a parameter's containing member, a record positional parameter's synthesized property, each variable of a variable declaration, or the member itself.
- Each symbol is then judged by the unchanged `IsDictatedMember`.

Previous findings:
- **#1: closed** (as in `16-auditor.md`). It is now decided by the same cause-based path.
- **#3: closed.** Every member #3 listed now holds: (a) record class implementing `INamed.Pair`, (b) record struct, and (c) a synthesized override of a referenced abstract record property. The test implementer added the readonly record struct, the indirect override through a source `abstract override`, nested positions, and mixed dictated and non-dictated parameters. The implementer reports all 24 RH4116 tests green. I did not re-run them, because no finding turns on them.
- **#3 is repeated by #5.** A member of #3's class that #3's enumeration did not name still fails (finding #5). #3 claimed that "no other member kind can be dictated", but a positional parameter's type also stands in the signature of the compiler-synthesized `Deconstruct`.

Probed on `a85f8e0` with a referenced assembly. These hold:
- an indexer parameter of an implicit implementation of a referenced indexer (not reported), and of a non-implementing indexer (reported);
- an event declaration with accessors implementing a referenced event;
- a non-abstract virtual referenced record property matched by a positional parameter: no property is synthesized, `(int First, int Second)` compiles, so the names are not dictated, and `(int first, int second)` is reported;
- a record that declares the property explicitly: the parameter's names are not dictated (`(int First, int Second)` compiles), and they are reported;
- a lambda parameter, a local function's return and parameter types, a `default(...)` expression, and a delegate parameter inside or next to a dictated method: all still reported;
- a partial record whose positional parameter list and base list are on different parts (not reported).

I could not probe a function pointer type in a dictated record parameter: `CompiledAssemblyReference` compiles without `/unsafe` (CS0227). The climb through `FunctionPointerParameterSyntax` is the same syntax-only walk as the other positions.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | high | `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` `GetDeclaredMembers`, `ParameterSyntax` branch (it yields the constructor and the synthesized property only) | FC-016, FC-044 | #3 (and through it #1) | same requirement | SHARED-CASING-014 bullet 2: an element name in "the types of [the] parameters" of a dictated member must not be reported. For a record or record struct, the compiler synthesizes `public void Deconstruct(out T1 P1, …)` from the positional parameter list, so each positional parameter's type is also the type of an out-parameter of that method. When the synthesized `Deconstruct` implicitly implements a `Deconstruct` declared in a referenced assembly, its tuple element names are dictated. `GetDeclaredMembers` never asks for `Deconstruct`, so the names are reported. **Probed on `a85f8e0`:** with a referenced `public interface IDecon { void Deconstruct(out (int first, int second) Pair); }`, `public record R((int first, int second) Pair) : IDecon;` reports RH4116 on `first` and `second`: 2 diagnostics, expected 0. `public record R((int First, int Second) Pair) : IDecon;` gives **CS8141** ("tuple element names in the signature of method 'R.Deconstruct(out (int First, int Second))' must match … interface method 'IDecon.Deconstruct(out (int first, int second))'"), so the names are dictated. With a referenced `IDecon2 { void Deconstruct(out int Count, out List<(int low, int high)> Ranges); }`, `public record struct S(int Count, List<(int low, int high)> Ranges) : IDecon2;` and `public readonly record struct T(…) : IDecon2;` report 4 diagnostics, expected 0. This is the type-argument position. **Members that fail:** record class, record struct, and readonly record struct, each with the tuple as the parameter type itself and nested in it. **Members that hold:** the synthesized property (finding #3), a record that declares its own `Deconstruct` (then nothing is synthesized from the parameter list), and class or struct primary constructors (no `Deconstruct`). **No further member:** besides the primary constructor (never dictated), one property per parameter, and `Deconstruct`, the compiler synthesizes nothing from a positional parameter's type. `Equals`, `GetHashCode`, `PrintMembers`, `<Clone>$`, the copy constructor, and `EqualityContract` do not carry it. The summary of `GetDeclaredMembers` ("the members the compiler declares whose signature holds a type") and the `IsDictatedName` remarks overstate completeness for the same reason (FC-031, part of this finding). | This is the same decision failing between the same components as #1 and #3, so the lead decides it under *Architecture findings*. Option one is to read the compiler's result: for a positional parameter, also ask the record's implicitly declared `Deconstruct` (`IsImplicitlyDeclared`, `WellKnownMemberNames.DeconstructMethodName`). That completes the set the compiler synthesizes from a positional parameter's type, so no further cell exists. A broader filter such as "any implicitly declared member" is wrong, because the synthesized `Equals(R)` implements `IEquatable<R>` from the BCL and would exempt every record. Option two is to refuse the class. Today that means it stays reported, which contradicts SHARED-CASING-014 as written, so the spec would have to name the exception, and that is a visible-output decision. With either option, add tests for the failing members and for the other side (a `Deconstruct` that implements a source interface stays reported), and fix both comments. |
| 6 | medium | `RH4116TupleElementCasingAnalyzer.GetDeclaredMembers` (`reference.Span == parameter.Span`) and `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs:635-678` `GetPositionalPropertyFlagsAsync` (`parameter.Span.Contains(reference.Span)`, already on `main`) | FC-015 | – | PR-introduced | The rule "which property the compiler declares for a record's positional parameter" now has two owners, with different boundary predicates: span equality in RH4116, span containment in the rename-safety utility. Both find the synthesized property today, because its syntax reference is the `ParameterSyntax` itself, so I found no input where they diverge. This is a parity divergence without confirmed corruption. `18-implementer.md` states "no other owner", but `rg -n "DeclaringSyntaxReferences" src --glob '*.cs'` lists line 672 of `CasingRenameSafetyUtilities`. The RH4120 follow-up in `00-lead.md` would add a third consumer, and finding #5 extends the same mapping. | Give the mapping one owner in `Reihitsu.Analyzer`, which `Reihitsu.Analyzer.CodeFixes` already sees through `InternalsVisibleTo`. A natural home is a helper next to `CasingAnalyzerBase` or in `Reihitsu.Analyzer/Core` that returns the members the compiler declares from a positional parameter. Have both callers use it, with one span predicate. |
| 7 | medium | `tests/Reihitsu.Analyzer.Test/Naming/RH4116TupleElementCasingAnalyzerTests.cs` | FC-003, FC-017 | – | same requirement | The repair moved every member kind onto a new path. Indexer parameters now go through `ParameterSyntax` → `IParameterSymbol.ContainingSymbol` instead of `BasePropertyDeclarationSyntax` parameters. Events with accessors now go through the `MemberDeclarationSyntax` branch instead of the old `BasePropertyDeclarationSyntax` arm. No RH4116 test covers the dictated side of either (`rg -n "this\[\|add \{" ` over the test class finds nothing). SHARED-CASING-014 bullet 2 names both "indexer" and "event". My probes show both hold today, but nothing pins them, so a later change to the parameter branch could regress them silently. | Add a dictated indexer whose tuple-typed parameter and return type implement a referenced interface indexer. Add an event with `add`/`remove` accessors implementing a referenced event. Add the other side for each: a source interface or no implementation stays reported. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Outermost type a tuple stands in (`GetOutermostType`) | Climbs while the parent is `TypeSyntax`, `TupleElementSyntax`, `TypeArgumentListSyntax`, `FunctionPointerParameterSyntax`, or `FunctionPointerParameterListSyntax` | SHARED-CASING-014: "as the type itself, as a type argument, as the element type of an array, or as an element of another tuple type" | Agree, and it is wider in a way that is correct: nullable, pointer, `ref`/`scoped`, a qualified name's type arguments, and function pointer parameters are also parts of the declared type whose names the compiler matches. The climb stops at `ExplicitInterfaceSpecifierSyntax`, base types, constraints, `ArrayRankSpecifierSyntax`, and every expression, which are not signatures (unchanged hint in `16-auditor.md`) | ok |
| Declared members of the outermost type's parent (`GetDeclaredMembers`) | `ParameterSyntax` with `Type == type`; `VariableDeclarationSyntax` with `Type == type`; any `MemberDeclarationSyntax` | SHARED-CASING-014: return type, parameter types, type of a property, indexer, or event of a dictated member | A type whose direct parent is a member declaration is always that member's type or return type. Explicit interface names sit under the specifier. Delegate and type declarations yield non-member symbols that `IsDictatedMember` rejects. Lambda, local-function, delegate, and extension-block parameters yield symbols that are never dictated. The positional-parameter branch misses the synthesized `Deconstruct` | finding #5 |
| Synthesized property of a positional parameter | Same-name `IPropertySymbol` whose `DeclaringSyntaxReferences` has the parameter's tree and **equal** span | `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync`: same-name property whose reference span is **contained in** the parameter span | Same result for every input I found; predicates differ | finding #6 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Dictated member | `rg -n "IsDictatedMember" src --glob '*.cs'`: decided only in `CasingAnalyzerBase.IsDictatedMember`. RH4116 names the symbols to ask | one owner |
| Members the compiler declares from a record positional parameter | `rg -n "DeclaringSyntaxReferences" src --glob '*.cs'`: `RH4116TupleElementCasingAnalyzer.GetDeclaredMembers` (new, property only) and `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync` (`main`, property only). `SingleLetterIdentifierUtilities.cs:65` recognizes a positional parameter syntactically, which is a different decision | two owners (finding #6). Both miss `Deconstruct`, but only RH4116's decision needs it (finding #5) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `…RecordPositionalParameterWhosePropertyImplementsAReferencedAssemblyPropertyAreNotReported`, `…RecordStructPositionalParameter…`, `…WhosePropertyOverridesAReferencedAssemblyProperty…` | no RH4116 diagnostic for a dictated synthesized property, for record class, record struct, readonly record struct, and direct and indirect overrides | any RH4116 diagnostic | analyzer `Verify` (exact) + `CompiledAssemblyReference` | adequate |
| `…NestedInTheTypeOfADictatedRecordPositionalParameterAreNotReported` | nesting axis (type argument, array element, inner tuple) | any diagnostic | same | adequate |
| `…SuppliedByATypeArgumentAreReportedWhereWrittenAndNotInARecordPositionalParameter` | names a source type argument supplies are reported once, in the base list (Q12 A) | a diagnostic on the parameter, or none in the base list | exact markup | adequate |
| `…ThatImplementsNothingAreReportedNextToADictatedOne`, `…OfAPositionalParameterThatIsNotDictatedAreReported` | the other side (FC-003): source interface, source abstract record, no implementation, class primary constructor | a missing diagnostic | exact markup | adequate |
| (missing) synthesized `Deconstruct`; dictated indexer parameter; event with accessors | — | — | — | findings #5 and #7 |

### Visible output is decided
Every new expected value comes from SHARED-CASING-014 bullet 2 / RH4116-001, both unchanged (lead decision 4), and from Q12 A for the type-argument test. No rule page changed.

### Architecture findings
**Finding #5 repeats #3, which repeated #1.** The decision is the same: which declared symbols own the signature a tuple type stands in. It fails between the same components: RH4116's choice of symbols to ask, and `CasingAnalyzerBase.IsDictatedMember`. Lead decision 4 replaced the syntax-kind list with the compiler's declared symbols, which closes the cells for explicitly declared members. For a record positional parameter, though, the set of synthesized members is still listed by hand (constructor and property), and that list misses `Deconstruct`. The inputs that show it are in finding #5. The complete set is now known: the primary constructor, one property per parameter, and `Deconstruct`. So a read of the compiler's synthesized `Deconstruct` ends the enumeration, and no later cell remains on this axis.

**Finding #6: one rule is stated in two places.** `RH4116TupleElementCasingAnalyzer.GetDeclaredMembers` and `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync` both map a positional parameter to the property the compiler declares for it, with different span predicates. Both components are changed by this run (`CasingRenameSafetyUtilities` in `bc565c2`), and the RH4120 follow-up needs the same mapping. Finding #5's repair should extend one shared mapping rather than RH4116's copy.

### Hints
- The explicit-interface-specifier hint from `16-auditor.md` is unchanged. The climb stops at `ExplicitInterfaceSpecifierSyntax`, so the behavior is as before.

### Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`. If finding #6 is taken, the shared mapping is the piece RH4120 would reuse.

## Whole run

- **State:** checkout `a85f8e09ef22df99b35cb5708fda83e800ba4d82`. `origin/main` is `75cc39fa61efdb2aa564ba57f36e86127b33485b` (`git fetch` agrees) and is merged (`git merge-base --is-ancestor` true). The diff `c94249b..a85f8e0` matches `17`–`20`: `RH4116TupleElementCasingAnalyzer.cs`, the `SplitIntoPascalCaseWords` summary, `RH4116TupleElementCasingAnalyzerTests.cs`, and handovers.
- **Lane check:** `scripts/lane-check.sh behavior-change --base 75cc39f…` → `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- **Comments:** `GetOutermostType` describes its code. `GetDeclaredMembers` and the `IsDictatedName` remarks describe the code but claim a complete set (part of finding #5). No `TODO`, no commented-out code, and no tracker numbers (`git diff c94249b..HEAD -- src tests` grepped for `TODO|#[0-9]{3,4}`: none).
- **Code quality:** two private methods replace three. `GetDeclaredSymbol` runs only after a name fails its casing check (`CheckCasing` short-circuits), so the per-declarator lookup for locals and fields is not a hot-path cost. A null symbol from `GetDeclaredSymbol` is rejected by `IsDictatedMember`'s type test.
- **Cost of special cases:** none. The mapping is general, not a rare-shape guard.
- **Trusted, not re-run:** the cleaner's build (`19`), the implementer's focused and default-set runs (`18`), and the spec checker's architecture run (`20`).

### Failure classes dismissed
- FC-001 / FC-002: the climb and the branches cover the spec's positions and more (guard table). The remaining gap is a missed member (finding #5), not a narrowed span.
- FC-003: both sides are tested for record properties. The indexer and event-with-accessors sides are missing (finding #7).
- FC-004, FC-005, FC-019–FC-021, FC-026: no trivia, gap, or token edit. RH4116 has no code fix.
- FC-006, FC-018, FC-036: no rename path changed.
- FC-007–FC-009, FC-030: no code fix.
- FC-010–FC-014, FC-028, FC-029, FC-038–FC-043: no formatter phase or entry point.
- FC-015: finding #6.
- FC-016 / FC-044: finding #5.
- FC-017: the new tests use exact markup, and the missing coverage is finding #7.
- FC-022, FC-023, FC-025, FC-027: no literal, identifier derivation, expression body, or enclosing-scope change. Lambdas, local functions, and `default(...)` inside a dictated member were probed and are still reported.
- FC-024: partial records were probed and hold. Static interface members, explicit implementations, and `file` types are not on the changed path beyond `IsDictatedMember`, which is unchanged.
- FC-031: part of finding #5. Finding #4 is closed.
- FC-032: no formatter change.
- FC-033–FC-035: expected values come from the unchanged spec, and the probes used the spec's shapes and compiler errors (CS8141, CS8139) as evidence of dictation.
- FC-037: the only comment-only claim was proven by `verify-text-only.sh`.
