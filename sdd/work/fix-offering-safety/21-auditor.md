Role: auditor
Commit: 970f447c5db57c04654f770b3a8ec389282772ec
Verdict: pass, text cleanup (#1600: pass, text cleanup; #1150: pass; #1274: pass; #1143: pass; #1673: pass; #1678: pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1600

### Scope

Repair delta for audit findings #1, #2, and #3 (`13-auditor.md`): `ReihitsuCodeFixProviderBase.MayOfferFixesAsync`/`CreateFixAllProvider`, the RH3002 and RH7101 `GetFixAllProvider`, the CLI's `ISourceFormatter.HoldsHidingCarrier` seam, the new Fix All tests, the word-boundary rows, `FixAllProviderCreationTests`, and the SHARED-FIX-011/012 clarification (decision 25). The spec change only states behavior that SHARED-FIX-012 already demanded ("every code fix … wherever its edit lies"), so the scope did not grow beyond the repair. Because of the spec sentence, I checked every Fix All path against it, not only the two repaired ones.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Both custom delegates now go through `CreateFixAllProvider`. It asks `MayOfferFixesAsync` for each document the delegate receives, and that is the same predicate `RegisterCodeFixesAsync` asks. The four new RH3002/RH7101 tests failed on 417800b with the auditor's counterexample at project and solution scope (14), and they pass on 66a181f (15). Members that held in #1 still hold: BatchFixer and `DocumentOrderFixAllProvider` re-register through the sealed `RegisterCodeFixesAsync` (`DocumentOrderFixAllProvider.cs:167`), and the new RH3102 project test pins the latter. `CasingFixAllProvider` checks every changed document through `HoldsHidingCarrierAsync` (`CasingCodeFixProviderBase{T}.cs:772-780`). The full `GetFixAllProvider` inventory: 88 BatchFixer, 15 `DocumentOrderFixAllProvider.Create`, 2 `CreateFixAllProvider`, 1 `CasingFixAllProvider`, and 1 `null` (RH4001). No path is left that bypasses the decision. |
| 2 | closed | `CarrierWithoutALineThatEndsTheGroupIsFormatted` now has rows where the word goes on with a letter (`#elsewhere`, `#elifx Y`), a digit (`#endif2`), or an underscore in a verbatim string (`#else_1`). `EveryLineKindThatEndsTheGroupCounts` now has rows where it is followed by a comment (`#else// c`) or a parenthesis (`#elif(Y)`). Dropping any part of `StartsWithWord`'s boundary condition (`ReihitsuFormatterHelpers.cs:371`) now fails a row. |
| 3 | closed | `FormatCommandHandler.cs:331` asks `_formatter.HoldsHidingCarrier`. `DefaultSourceFormatter` delegates to `ReihitsuFormatter.HoldsHidingCarrier`. The NSubstitute doubles in `FormatCommandHandlerTests` return `false` by default, so their branches are unchanged. |

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `src/Reihitsu.Analyzer.CodeFixes/Base/ReihitsuCodeFixProviderBase.cs:16-18` (class summary); `src/Reihitsu.ArchitectureTests/Analyzer/FixAllProviderCreationTests.cs:15-19` (class summary) | FC-031 | – | PR-introduced | Both summaries say that a Fix All that applies its fixes without registering them asks the base through `CreateFixAllProvider`. `CasingFixAllProvider` is such a Fix All: `GetFixAsync` calls `ApplyFixAllAsync` directly. It does not use `CreateFixAllProvider`. Instead, it asks the base's `HoldsHidingCarrierAsync` for every document the rename changes. Behavior is correct: casing fixes do not format, so for them `MayOfferFixesAsync` reduces to the carrier check. Only the text misdescribes one of the four Fix All kinds. | Name the casing path in both summaries, for example: "… through `CreateFixAllProvider`, or, for the casing Fix All, which changes several documents per fix, through `HoldsHidingCarrierAsync` for each changed document". |

Why `pass, text cleanup` for #4: the finding is confined to two XML summaries. It cannot reach compiled behavior, a diagnostic, or rule metadata. Neither summary is public API documentation in the sense of that exclusion. `Reihitsu.Analyzer.Package` packs only the four DLLs (`Reihitsu.Analyzer.Package.csproj:47-50`), no XML documentation file, and `AGENTS.md` (*Supported inputs*) states that no assembly is a public API. The second summary sits on a test class.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| May fixes be offered in a document (`MayOfferFixesAsync`) | The document's syntax tree: no tree, a hiding carrier (cached per tree), or a syntax error when `FormatsThroughFormatter` | The former inline condition of `RegisterCodeFixesAsync` (9d38a0b): `syntaxTree == null \|\| HoldsHidingCarrier \|\| (FormatsThroughFormatter && HasSyntaxErrors)` | It is the exact negation of the former condition, and both paths now ask the one method | holds |
| Custom Fix All, per document (`CreateFixAllProvider`) | The `document` argument the framework passes per document, not `context.Document` (the trigger document) | Registration on that document | Equal. A version that asked the trigger document would edit `/0/Test1.cs` and fail the new project-scope tests, whose trigger document holds no carrier | holds |
| Casing Fix All | Every document `renamedSolution.GetChanges(solution)` changes | `MayOfferFixesAsync` | Equal for casing providers: none overrides `FormatsThroughFormatter` (`rg` finds only RH2001 among the multi-document fixes, and RH2001 uses BatchFixer) | holds; text: #4 |
| CLI carrier check | The parsed tree, after the syntax-error and generated checks | `ReihitsuFormatter.HoldsHidingCarrier` (FMT-PIPE-033) | Same call through the seam | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether fixes may be offered in a document (SHARED-FIX-011/012) | `rg -n "MayOfferFixesAsync\|HoldsHidingCarrier" src` (non-test): `ReihitsuCodeFixProviderBase.MayOfferFixesAsync`, asked by `RegisterCodeFixesAsync` and `CreateFixAllProvider`. The other-document extension goes through the base's `HoldsHidingCarrierAsync`, called by the casing base (`:777`) and RH2001 (`:422`). No copy of the condition exists elsewhere. `rg "FixAllProvider.Create\("` finds only the base (`:74`) and `DocumentOrderFixAllProvider.cs:56` | one owner (the architecture finding of `13-auditor.md` is resolved) |
| Carrier definition | `ReihitsuFormatterHelpers.HoldsHidingCarrier`. Its callers are `ReihitsuFormatter` (`:68`, `:104`, `:471`), the CLI through `DefaultSourceFormatter`, the Playground, and the code-fix base | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3002 `FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged` / `…InTheSolution…InAnotherProjectUnchanged` | A project or solution Fix All started outside the carrier document fixes the starting document and leaves the carrier document byte-identical, with its diagnostic still reported | `value = (value); //";` is rewritten, or the diagnostic disappears | `Verify` with the second document in the fixed and batch state, markup allowed, one iteration per mode | adequate. It failed on 417800b with the audit's counterexample |
| RH7101, the same two tests | The same, for the field split | `private int c, d;` is split | as above | adequate. It failed on 417800b |
| RH3102 `FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged` | `DocumentOrderFixAllProvider` keeps leaving the carrier document unchanged | The empty statement in `/0/Test1.cs` is removed | as above | adequate (characterization of a path that already held) |
| `FixAllProviderCreationTests.FixAllProviderCreateIsCalledOnlyByTheProviderBaseAndTheDocumentOrderFixAllProvider` | Only the base and `DocumentOrderFixAllProvider` call `FixAllProvider.Create`; calls inside closures and state machines count for their outermost type | Another type calls it | ArchUnitNET call dependencies. The assertion is an exact ordered sequence, so the test cannot pass on an empty scan | adequate for the `Create` route. It failed on 417800b and named RH3002 and RH7101 (16). A direct `FixAllProvider` subclass is outside it (see *Hints*) |
| `EveryLineKindThatEndsTheGroupCounts` (+2 rows), `CarrierWithoutALineThatEndsTheGroupIsFormatted` (+4 rows) | Both sides of the word boundary | A boundary-less match refuses a non-counting row, or a strict match lets a counting row through | `AssertLeftAlone` checks exact text, both line endings, and a fixture without errors. `AssertFormattedThroughEntryPointContains` requires that the fixture needs formatting | adequate |

### Visible output is decided

No expected value of a visible surface changed beyond SHARED-FIX-012. The new Fix All tests assert what SHARED-FIX-012 already demanded ("every code fix … wherever its edit lies"), and decision 25 put it into words. The formatter rows assert FMT-PIPE-033's existing text. No CLI output or rule page changed. No `undecided output`.

### Cost of special cases

`CreateFixAllProvider` replaces a per-provider path with the base's one decision. It adds no special case. The SHARED-FIX-011 branch of `MayOfferFixesAsync` cannot be reached on the custom path today, because RH7101 reports nothing in a file with a syntax error (SHARED-JOIN-002) and RH3002 does not format. It is the same predicate as registration, not an extra guard. No FMT-UNSUP shape is involved.

## thoenissen/Reihitsu#1150

The repair routes SHARED-FIX-011 through the same `MayOfferFixesAsync` on the custom Fix All path. RH7101 sets `FormatsThroughFormatter` and RH3002 does not, matching the 29 overrides the first audit checked. The new SHARED-FIX-011 sentence holds on every Fix All path: BatchFixer and DocumentOrder re-register, `CreateFixAllProvider` asks the base, and casing fixes do not format. No findings.

## thoenissen/Reihitsu#1274

The repair does not touch FMT-UNSUP-008, RH5308, or the conditional-operator phases. Nothing to audit; evidence of `13-auditor.md` unchanged.

## thoenissen/Reihitsu#1143

The repair does not change `DocumentOrderFixAllProvider`. The new RH3102 project test exercises it with a carrier document and passes. Nothing else to audit; evidence of `13-auditor.md` unchanged.

## thoenissen/Reihitsu#1673

The repair does not touch RH5308-006. Nothing to audit.

## thoenissen/Reihitsu#1678

The repair does not touch SHARED-ORDER-008. Nothing to audit.

## Whole run

### Scope

- Head: `970f447c5db57c04654f770b3a8ec389282772ec` (the checkout, tree clean, up to date with the remote branch).
- Base: `origin/main` = `23cf11ace7b6d5f6f0edbbfa25da893a45b04699`, confirmed by `git ls-remote origin refs/heads/main`. It is merged into the head (`git merge-base --is-ancestor`).
- Audit: repair delta against `9d38a0b75c4e48c2cf20913ec0b4d11b8d0f3418`.

The commits are 417800b (tests), 66a181f (base and CLI), 079709c (architecture test), 5517687 (cleaner), ea52344 (spec), plus handover and lead-note commits. The production delta is 6 files: the base, RH3002, RH7101, `ISourceFormatter`, `DefaultSourceFormatter`, and `FormatCommandHandler`. That is what handovers 14 to 20 describe. `scripts/lane-check.sh behavior-change`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

Added source lines carry no `TODO` and no tracker number.

I executed nothing. Analyzer code has not changed since 66a181f, where the implementer ran the full analyzer, formatter, CLI, and architecture projects green (15). The architecture test was last run green after the cleaner's edit (17, 18, 20). No finding turns on a run.

### Failure classes dismissed

- FC-001, FC-002: traced in *Guard scope*. The per-document span is the `document` argument, equal to registration's.
- FC-003: the word-boundary rows close #2. The carrier and no-carrier sides of the Fix All are both in each new test (starting document fixed, carrier document unchanged).
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-022, FC-023, FC-024, FC-025, FC-026, FC-027, FC-029, FC-036: no rewrite, trivia handling, or literal handling changed. The delegates' bodies are untouched.
- FC-006: compiles, with 0 warnings (17). The `Task<Document>` delegate is wrapped into `FixAllProvider.Create`'s `Task<Document?>` signature.
- FC-007, FC-008, FC-009: the wrapper returns the document unchanged, or the delegate's result. Iteration counts in the new tests are pinned to one. RH4001's `null` Fix All is unchanged.
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-038 to FC-043: no formatter phase, precheck, or analyzer changed.
- FC-015, FC-016: *Policy ownership*. Every sibling Fix All path was enumerated (see the inventory under #1).
- FC-017: *Assertion adequacy*.
- FC-018: renames unchanged.
- FC-028: no formatter entry point changed.
- FC-030: no fix formats differently.
- FC-031: #4. The summaries of `MayOfferFixesAsync`, `CreateFixAllProvider`, `RegisterCodeFixesAsync`, `ISourceFormatter.HoldsHidingCarrier`, and the CLI comment describe the new code.
- FC-032: no result predates a compiled change it depends on (see *Scope*).
- FC-033, FC-034, FC-035: the RH3002 carrier document is the first audit's counterexample verbatim. Expected values come from SHARED-FIX-012 and FMT-PIPE-033, not from patched code. The tests failed on 417800b for the right reason (14, 16).
- FC-037: no text-only claim is made.

### Architecture findings

_None._ The first audit's architecture finding (two owners of "may a fix be offered in this document") is resolved. `MayOfferFixesAsync` is the one statement, and every Fix All path reaches it or the base's `HoldsHidingCarrierAsync`.

### Hints

- `FixAllProviderCreationTests` pins only the `FixAllProvider.Create` route (decision 24's scope). A future provider that subclasses `FixAllProvider` directly, as `CasingFixAllProvider` does, or subclasses Roslyn's `DocumentBasedFixAllProvider`, could bypass the base without failing any test. One more assertion in the same class would close the gap: within `Reihitsu.Analyzer.CodeFixes`, only `CasingFixAllProvider` derives from `FixAllProvider`. No such type exists today, so this is not a finding.
- The `DocumentOrderFixAllProvider` hint from `13-auditor.md` (the starting-code filter applies to the first pass only) still stands. The repair does not touch it.
