Role: auditor
Commit: e63741f8e577c9029fa0e188ccaaaddbff0c5d46
Verdict: #971: pass; #1072: pass; #1451: pass; #1434: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md` (*Maintainer answers*, *Decisions taken* 6–9, *Follow-up candidates*), `21-auditor.md`, `22-test-implementer.md` through `27-spec-checker.md`, the repair diff `a85f8e0..e63741f`, and, outside the diff, `CasingRenameSafetyUtilities.ChangesPositionalPropertiesAsync` and `IsCompilerLookupMethodRename`, `SingleLetterIdentifierUtilities`, RH4105 and RH4120 (other readers of a record's positional parameter list).

I ran the probes in a detached worktree of `e63741f` under the scratchpad, which I removed afterwards. They were one temporary MSTest class in `Reihitsu.Analyzer.Test`, run with `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~AuditProbeRH4116Tests"`. The only change in the shared tree is this handover.

## thoenissen/Reihitsu#971

The repair does not touch #971: `CasingUtilities` and its tests are byte-identical to `a85f8e0`. Every section is unchanged from `21-auditor.md`.

## thoenissen/Reihitsu#1072

The repair does not touch #1072. The only change in `CasingRenameSafetyUtilities` is the positional-property lookup (see #1434); `CasingCodeFixProviderBase{T}` and the keyword handling are byte-identical. Every section is unchanged from `21-auditor.md`.

## thoenissen/Reihitsu#1451

The repair does not touch #1451. Every section is unchanged from `21-auditor.md`; finding #4 stays closed.

## thoenissen/Reihitsu#1434

### Scope
Repair delta against `a85f8e0`, for findings #5, #6, #7 and the architecture finding on #5 (lead decisions 6–9). Head `e63741f8e577c9029fa0e188ccaaaddbff0c5d46`, base `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b`.

The repair adds the internal `RecordPositionalParameterUtilities` (`src/Reihitsu.Analyzer/Core`), routes RH4116 and `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync` through it, adds ten RH4116 tests, and adds the new type to the SHARED-CASING code-owner lists. No requirement text, ID, change doc, or rule page changed, so scope did not grow and this is a delta audit.

Previous findings:
- **#5: closed.** `RecordPositionalParameterUtilities.GetDeclaredMembers` yields, for a parameter of a record's parameter list, the synthesized property and the record's implicitly declared `Deconstruct` of the parameter list's arity; RH4116 asks them after the containing constructor. Every member #5 listed now holds and is pinned: record class, record struct, readonly record struct, each with the tuple as the parameter type and nested (type argument, array element, inner tuple). The other side holds and is pinned: a `Deconstruct` implementing a source interface, a record declaring its own `Deconstruct`, and `Equals(R)` through `IEquatable<R>` (which guards against an "any implicitly declared member" filter). Both comments #5 named (`GetDeclaredMembers` summary, `IsDictatedName` remarks) now name the property and `Deconstruct`.
- **#6: closed.** The mapping "property the compiler declares for a positional parameter" has one owner, `RecordPositionalParameterUtilities.GetDeclaredProperty`, with one predicate (same tree, `reference.Span == parameter.Span`). The rename-safety predicate moved from containment to equality; that changes no result, because the synthesized property's declaring syntax reference is the `ParameterSyntax` itself (the RH4116 record-property tests, which only pass when equality matches, prove it), and the code-fix tests that flip a positional flag (`RH4111…NoFixIsOfferedWhenTheRenameWouldMatchAPositionalParameter`, `RH4114…WouldSplitThePropertyFromItsPositionalParameter`, `RH4109…WouldSplitTheFieldFromItsPositionalParameter`, `RH4120…NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord`) are in the default set the implementer reports green.
- **#7: closed.** Dictated indexers (implicit and explicit; tuple as type, as parameter, nested in a `List<>` parameter) and events with accessors (implicit and explicit) are pinned, each with its other side (source interface, no implementation) in the same class.
- **Architecture finding on #5: closed.** The set of members the compiler declares from a positional parameter's type (primary constructor, property, `Deconstruct`) is now read from the compiler's symbols in one place. My probes (below) found no further member on the axis.

Probed on `e63741f` with a referenced assembly; all hold:
- a referenced generic `IDeconOf<T> { void Deconstruct(out T Pair); }` with `record R((int first, int second) Pair) : IDeconOf<(int first, int second)>`: the parameter is not reported, the base-list type argument is (Q12 A, consistent with the existing property test);
- a source interface `IMine : IDecon` in the base list: not reported (implementation found through the inherited interface);
- an explicit `void IDecon.Deconstruct(...)` declared in the record (also in a generic record `R<T>`): the synthesized `Deconstruct` implements nothing, so the positional names are reported — correct, `(int First, int Second)` would compile;
- a partial record with the parameter list and the base list on different parts: not reported;
- a declared `Deconstruct(out object)` of the same arity next to the synthesized one: the synthesized one is still selected (`IsImplicitlyDeclared`), not reported;
- a derived record `D(int Count, (int first, int second) Pair) : B(Count), IDecon2` whose `Count` property is inherited: not reported (the `Deconstruct` arity follows the parameter list, not the synthesized properties).

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Parameter is a record positional parameter (`GetDeclaredMembers`) | `parameter.Parent is ParameterListSyntax { Parent: RecordDeclarationSyntax }` and the symbol's containing member is a constructor | RH4105/RH4120/`SingleLetterIdentifierUtilities` use the same syntactic shape for the same question | Agree (class/struct primary constructors excluded; no `Deconstruct` exists for them, pinned by the earlier `PrimaryConstructor` test) | ok |
| Synthesized property (`GetDeclaredProperty`) | Same-name `IPropertySymbol` whose declaring reference has the parameter's tree and equal span | Previously two predicates (equality, containment); now one | One owner | ok |
| Synthesized `Deconstruct` | `GetMembers("Deconstruct")`, `IsImplicitlyDeclared`, `Parameters.Length == parameterList.Parameters.Count` | Compiler: synthesizes `Deconstruct` unless a method of the same signature is declared | Agree: a declared one is never implicitly declared; a declared overload is excluded by `IsImplicitlyDeclared`; a base record's `Deconstruct` is not among `GetMembers` of the derived type | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Property the compiler declares for a positional parameter | `rg -n "DeclaringSyntaxReferences" src --glob '*.cs'`: only `RecordPositionalParameterUtilities.GetDeclaredProperty` answers it (`SingleLetterIdentifierUtilities.cs:65` asks whether a parameter is positional, a different decision) | one owner |
| Members declared from a positional parameter's type | `rg -n "\"Deconstruct\"\|DeconstructMethodName\|WellKnownMemberNames" src --glob '*.cs'`: `RecordPositionalParameterUtilities` only; `CasingRenameSafetyUtilities.cs:252` lists `Deconstruct` as a compiler-lookup method name, a different decision | one owner |
| Dictated member | `rg -n "IsDictatedMember" src --glob '*.cs'`: `CasingAnalyzerBase.IsDictatedMember`, unchanged | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `…WhoseDeconstructImplementsAReferencedAssemblyDeconstructAreNotReported` (record; record struct + readonly record struct) and `…NestedInTheTypeOfARecordPositionalParameterWhoseDeconstructIsDictatedAreNotReported` | no RH4116 diagnostic when the synthesized `Deconstruct` is dictated, at every nesting position and record kind | any RH4116 diagnostic | analyzer `Verify` (exact) + `CompiledAssemblyReference` | adequate (failed with 2, 4, 24 diagnostics before the repair) |
| `…WhoseDeconstructImplementsASourceDeconstructAreReported`, `…ReportedWhenTheRecordDeclaresItsOwnDeconstruct`, `…ReportedAlthoughTheGeneratedEqualsImplementsIEquatable` | the other side (FC-003): source interface, declared `Deconstruct`, `Equals(R)` not asked | a missing diagnostic | exact markup | adequate |
| `…IndexerThatImplementsAReferencedAssemblyIndexerAreNotReported`, `…IndexerThatIsNotDictatedAreReported` | indexer type and parameter types follow the indexer's dictation | a diagnostic on the dictated indexer, or a missing one on the others | exact markup + reference | adequate |
| `…EventWithAccessorsThatImplementsAReferencedAssemblyEventAreNotReported`, `…EventWithAccessorsThatIsNotDictatedAreReported` | an event with accessors follows its dictation | same | exact markup + reference | adequate |
| Rename-safety predicate change | positional flags unchanged | a positional-flag code-fix test turning green/red | existing RH4109/RH4111/RH4114/RH4120 code-fix tests | adequate (no new test needed: the predicate's result is unchanged and pinned from both callers) |

### Visible output is decided
Every new expected value comes from SHARED-CASING-014 bullet 2 / RH4116-001, both unchanged (lead decisions 6 and 8). No rule page changed.

### Architecture findings
_None._ The repeated class of #1, #3, and #5 is closed by reading the compiler's declared symbols, with the positional-parameter set in one owner.

### Hints
- The explicit-interface-specifier hint from `16-auditor.md` is unchanged.

### Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`, which can now reuse `RecordPositionalParameterUtilities.GetDeclaredProperty`.

## Whole run

- **State:** checkout `e63741f8e577c9029fa0e188ccaaaddbff0c5d46`; `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b` is merged (`git merge-base --is-ancestor` true). The diff `a85f8e0..e63741f` matches `22`–`27`: the new utility, RH4116, `CasingRenameSafetyUtilities`, the RH4116 tests, the two code-owner lists, and handovers.
- **Lane check:** `scripts/lane-check.sh behavior-change --base 75cc39f…` → `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- **Comments:** the new type's and members' summaries and remarks describe the code; the RH4116 summary and remarks name the property and `Deconstruct`; `GetPositionalPropertyFlagsAsync`'s summary still holds. No `TODO`, no commented-out code, no tracker numbers (`git diff a85f8e0..HEAD -- src tests` grepped for `TODO|#[0-9]{3,4}`: none).
- **Code quality:** the utility is internal, in `Reihitsu.Analyzer/Core` next to its peers, and reached from CodeFixes through the existing `InternalsVisibleTo`. `GetDeclaredMembers` runs only after a name fails its casing check, so the extra `GetMembers` lookups are off the hot path.
- **Cost of special cases:** none; the mapping is general.
- **Trusted, not re-run:** the cleaner's build (`24`), the implementer's focused and default-set runs (`23`), and the spec checker's architecture runs (`25`, `27`).

### Failure classes dismissed
The catalog is unchanged since `21-auditor.md`, and every dismissal there still holds for this delta. Changes against it:
- FC-015: closed (finding #6).
- FC-016 / FC-044: closed (finding #5); the set is read from the compiler, not modelled.
- FC-003 / FC-017: closed (finding #7); both sides pinned with exact markup.
- FC-031: comments match the code.
- FC-006, FC-018, FC-036 (rename paths): the rename-safety predicate change is result-preserving (see #6).
