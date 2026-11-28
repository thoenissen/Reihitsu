Role: auditor
Commit: ae16c90b16f6ec5e0444210f1d467dc6595c8bf2
Verdict: findings

The prompt carried no intended fix, cause, or hoped-for verdict. I worked from the handovers 00 to 06, the specs, the change doc, and the diff.

## Scope

Full audit of head ae16c90 against `origin/main` 13b0ed238ef4e0fb71015a5967dd3d6431f61cd7. The checkout is ae16c90. `git ls-remote origin refs/heads/main` returns 13b0ed2, and that commit is an ancestor of the head. The diff matches the handovers: 3 production files (`RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs`, `DocumentOrderFixAllProvider.cs`, `CasingAnonymousMemberUtilities.cs`), 2 test files, the specs RH2001, casing, README and requirement-ids, the change doc, and `sdd/work/`. `scripts/lane-check.sh behavior-change`: confirmed.

How I checked:
- Static tracing of every changed method and its counterparts (`CasingCodeFixProviderBase<T>` rename and Fix All, `GetOtherSymbolReferencesAsync`, `ReplaceAnnotatedPropertyWithFieldAsync`).
- `scripts/apply-fix.sh RH2001` over 21 fixtures (LF and CRLF) on the head, and over the null-forgiving fixtures on a `main` worktree as well.
- `scripts/apply-fix.sh RH4106` over 3 casing fixtures on both `main` and head.
- One temporary probe test class in a separate head worktree, run with and without a mutation of the new re-analysis branch.

Everything ran outside the repository, and the worktrees have been removed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs:346-352` (`ApplySolutionFixAllAsync`, re-analysis of a document whose text an earlier document's fixes changed) | FC-008, FC-017 | – | new mechanism | No test covers the new solution-scoped branch. It is needed for SHARED-FIX-002 across `partial` parts, and no test fails when it is removed. Evidence: I mutated the condition to `... == false && false`, and all 340 RH2001 tests still pass. A probe with two documents fails under that mutation (Fix All in project: "Expected '1' iterations but found '2' iterations") and passes on the head. Probe: document 0 is `partial class Sample { private int {|RH2001:Value|} { get; set; } }`. Document 1 is `partial class Sample { int Sum() => Value + Value + … (40 references) + Name; private int {|RH2001:Name|} { get; set; } }`. Expected result: `private int _value;` / `int Sum() => _value + … + _name; private int _name;`. The probe has `NumberOfFixAllInProjectIterations = 1` and `NumberOfFixAllIterations = 1`. With a shift of only one reference, the stale span still lands inside `Name`'s declaration, so the mutant survives that version. The shift has to be larger than the gap between the declaration and the code above it. The only multi-document RH2001 tests have a single diagnostic (`...InAPartialPartInAnotherDocument`), so none of them reaches this branch. A second probe, with anonymous members named in both parts (`new { Name, this.Value }` / `new { Value, Name }`), also passes on the head. | Test implementer: add a Fix All test over two `partial` parts with a diagnostic in each, where the first part's fix shifts the second part's diagnostic past its own declaration. Pin one Fix All iteration in project and solution scope. Tag it with RH2001-012 or SHARED-FIX-002. |
| 2 | low | `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests.cs:906-909` (`WithEndOfLine`) | – | – | PR-introduced | A private helper repeats `AnalyzerTestsBase<TAnalyzer, TCodeFix>.ConvertEndOfLine` (the same `Replace("\r\n", "\n").Replace("\n", endOfLine)`), which the class already inherits. | Use `ConvertEndOfLine` and delete `WithEndOfLine`. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which anonymous-object member takes its name from a reference (now only `CasingAnonymousMemberUtilities.GetMemberNamedByReference`) | The reference token found by `SymbolFinder`. The token must be an `IdentifierNameSyntax`, the nearest `AnonymousObjectMemberDeclaratorSyntax { NameEquals: null }` must end with that token, and `SyntaxFacts.TryGetInferredMemberName(member.Expression)` must not be empty | The compiler's inferred-name rule, and the deleted RH2001 branch on `main`: inferred name == property name and the last `IdentifierName` binds to the property | Agree on compiling code. Whenever a name is inferred, the identifier that supplies it is the expression's last token: an `IdentifierName`, the `Name` of a member access, or the `MemberBinding` name of a conditional access. Probed `this.`, `other.`, `other?.`, `other?.Inner.Value`, `Sample.Value`, `global::Sample.Value`, `@Value`, `Value.Value`, and `Value.ToString().Length`, which is only renamed. `Value!` gets no inferred name: the head output equals `main` byte for byte (5 fixtures) | holds |
| RH2001-011 tuple guard (`GivesInferredTupleElementNameAsync`) | Tuple arguments without `NameColon` | The previous combined predicate, restricted to tuple arguments | Same span and the same conditions; only the anonymous branch was deleted | holds |
| Rename-binding check against the named solution | `GetOtherSymbolReferencesAsync` over identifiers spelled like the field name, original solution against renamed solution | The casing fix compares the named solution against the renamed solution | RH2001 compares the solution before the names are written against the renamed one. The written name is the property's old name, which never equals the field name (`GetFieldName` returns false when the two are equal), so the extra `NameEquals` identifiers are never counted. `a12` (`nameof`) and the spec example with a local `_value` hold | holds |
| Diagnostics of a document that an earlier document's fixes changed (solution mode) | All diagnostics the analyzer reports on the current text, filtered by the fixes offered on that current text | Document mode filters by the fixes offered on the starting document (SHARED-FIX-002). Its own second pass after a lost diagnostic also takes every re-analyzed diagnostic | Diverges only for a guard that another document's fix releases. Lead decision 10 accepts this as a rare class, and I could not construct an RH2001 input for it | holds (recorded under decision 10) |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Anonymous member name kept on rename (SHARED-CASING-015, RH2001-012) | `rg "TryGetInferredMemberName\|AnonymousObjectMemberDeclaratorSyntax\|NameAnonymousObjectMembersAsync" src`: `CasingAnonymousMemberUtilities` (the owner), called from `CasingCodeFixProviderBase<T>.PrepareNamedRenameAsync` and `RH2001...CodeFixProvider.RenamePropertyToFieldNameAsync`. The other hits are unrelated: formatter `JoinedGapUtilities` and `LineBreakAssignmentRewriter` (layout), RH3002 (parentheses), and the RH2001 tuple guard | one owner; lead decision 4 is carried out |
| Fix All composed sequentially on an evolving solution | `DocumentOrderFixAllProvider` solution mode (new, only user RH2001) and `CasingCodeFixProviderBase<T>.CasingFixAllProvider` (existing, private, with rename-candidate annotations and duplicate-target detection) | Two owners. The diff extends the shared owner instead of adding a private copy (lead decision 8). The casing provider existed before the diff and carries casing-specific decisions. Not PR-introduced; listed under follow-up candidates |
| `=` token without an elastic marker in the written name | `CasingAnonymousMemberUtilities.WriteMemberName` only | one owner. Casing output is unchanged: RH4106 on three shapes (multi-line, multi-line `this.`, single line) gives byte-identical output on `main` and head |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 19 single-fix tests in `...AnonymousMemberTests` (`VerifyFixWritesTheName...`, `VerifyFixKeeps...`, `VerifyFixOnlyRenames...`) | Exact fixed text per RH2001-012 | Any other text, a missing `Name = `, a moved comment | `Verify` (exact text, single fix, iterative fix, Fix All in document, project, and solution) | adequate; LF and CRLF on the issue input and the multi-line comment case |
| `VerifyFixAllWritesTheNamesOfTheMembersOfTwoProperties`, `...ChainPassesThroughAnotherFixedProperty`, `...ReadThroughAnotherFixedProperty`, inherited `FixAllCorrectsEveryDiagnostic` | One Fix All application gives the sequential result (SHARED-FIX-002) | A second iteration is needed (the `BatchFixer` merge loss) | `Verify` with `NumberOfFixAllIterations = 1`; `GetFixAllScenario` | adequate for one document. The multi-document branch is not covered (finding #1) |
| `VerifyNoFixIsOfferedForAMemberNextToATupleElementInAWithheldPosition`, `...LambdaParameterInABlockBodyIsNamedLikeTheField` | Other RH2001-011 conditions still withhold the fix | A fix is registered | `VerifyNoCodeFixIsOffered` (diagnostic asserted, no action registered) | adequate |
| Existing `...InAPartialPartInAnotherDocument` under the new provider | A rename in another part is kept by Fix All | The rename in the other part is lost (it fails under `DocumentOrderFixAllProvider.Create`, per 04-implementer) | `Verify` | adequate; it kills the `keepsSolution` branch |

## Visible output is decided
Every expected value the diff adds is the `Name = <renamed expression>` output that the maintainer's answer B covers (change doc `Decisions`, `00-lead.md` *Maintainer answers*). It is the RH2001-012 example text or follows from it. The remaining layout comes from requirements the diff does not change: RH2001-004 and RH2001-007 for the field, and the four-line field initializer in `VerifyFixWritesTheNameOfAMemberInThePropertysOwnInitializer`, which the explicit-name control already gives on `main` (02-skeptic row 19). The six removed no-fix tests are the ones answer B reverses. No rule page changed. Nothing is undecided.

## Failure classes dismissed
- FC-001, FC-002, FC-003: see *Guard scope*. Both sides of "named by a reference" are tested: spec example 5 (named by another identifier), the explicit-name tests, and the bare and qualified references.
- FC-004, FC-005, FC-019, FC-020, FC-021: the rewrite only inserts `Name = ` and moves the expression's leading trivia in front of the name. Probed on the head and kept: an `#if !NEVER` group around the member (fixed, directive kept), `#if NEVER … #else` around it (no fix offered: excluded code, RH2001-004), `#pragma` lines around the member, a trailing `// c`, and comments between every token of `this . Value`.
- FC-006, FC-024: the results compile (apply-fix reported no `invalid-result` on 21 fixtures). That covers expression-tree lambdas, `with`, `join`, array unification, interpolation, a static field initializer, a nested type, and `global::`. The `partial` parts over two documents hold (probe).
- FC-007, FC-009: one application silences the diagnostic. `Value = _value` is not a redundant name, so no other RH rule reports it, and a registered action always changes the document.
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-029: no formatter phase changed. The fix still formats only the field it builds.
- FC-015, FC-016: see *Policy ownership*. The sibling shapes (`this.`, `other.`, `?.`, type-qualified, query, nested anonymous object, initializer) all hold.
- FC-018: the rename goes through `Renamer.RenameSymbolAsync`.
- FC-022: no literal is touched; `$"{new { Value }}"` holds.
- FC-023: `new { @Value }` becomes `new { @Value = _value }` and compiles.
- FC-025: expression and block bodies are both tested.
- FC-026: every member of one document is rewritten in one `ReplaceNodes` pass over the original spans. `new { Value, Count }` on one line and across two lines holds.
- FC-027: nested, query, lambda, expression-tree, initializer, and interpolation scopes hold.
- FC-030: only the field is formatted (`FormatMemberOnLinesOfItsOwnAsync`).
- FC-031: the changed summaries and remarks describe the new code (`RenamePropertyToFieldNameAsync`, `RenameChangesBindingAsync`, `GivesInferredTupleElementNameAsync`, the `DocumentOrderFixAllProvider` class, `CreateSolutionScoped`, `ApplySolutionFixAllAsync`, `GetFixAsync`, the inline comment in `ApplyPassAsync`). No `TODO` and no tracker numbers in the added lines.
- FC-032: the round-2 full analyzer run (12428 passed) postdates the last compiled change. The cleaner made no compiled change.
- FC-033, FC-034, FC-035: the issue input itself is tested under LF and CRLF. Expected values come from the spec and from controls on `main` (03-test-implementer).
- FC-036: `ToString()`, reads, `with`, `join`, and expression-tree member names keep the written name. `nameof(Value)` → `nameof(_value)` is existing RH2001 behavior and is specified (RH2001 spec, the example with `nameof(Name)` → `nameof(_name)`).
- FC-037: no text-only claim.
- FC-038 to FC-043: no unsupported shape is declared or checked differently. The node the fix formats (the field) is unchanged.
- FC-044: the helper reads the compiler's inferred name and the reference search. It does not model them.

## Architecture findings
_None._ The skeptic's FC-015 finding (decision 4) is resolved: RH2001 no longer decides which anonymous member takes its name from a reference.

## Hints
- Through the solution mode, the RH2001 Fix All action's title is now `CodeFixResources.RH2001Title` (`DocumentOrderFixAllProvider.GetFixAsync`). Before, it was the default Fix All title that `BatchFixer` and `DocumentOrderFixAllProvider.Create` produce. The casing Fix All already does the same. Neither the analyzers nor the tests show it.
- The mode depends on whether `_solutionScopedTitle` is null (`DocumentOrderFixAllProvider.cs:273, 368`). An explicit flag would read more clearly. The only caller passes a non-null resource, so no input is affected.
- In solution mode, `ApplyPassAsync` still computes `currentText.WithChanges(changes)`, which that mode never reads.
- Fix All cost: every RH2001 diagnostic now runs the rename check twice (the offered filter, then the registration in the pass) as well as the applied rename, and documents are processed one after another. This matches the other document-order users. It is not a defect.
- `WriteMemberName` now builds `=` without the elastic marker, and only the RH2001 own-initializer test pins that. A short comment on why would keep the constructor call from being "simplified" back.

## Follow-up candidates
- Casing Fix All: `CasingCodeFixProviderBase<T>.CasingFixAllProvider` composes renames on an evolving solution with its own machinery. The shared `DocumentOrderFixAllProvider` now has a solution-scoped mode for the same decision. Structural cause: two owners of the sequential Fix All composition across documents (FC-015). This existed before the run.
- Already recorded by the lead: a casing test that pins the layout of a multi-line anonymous object creation whose member name the fix writes. I found no casing output that differs from `main`.
