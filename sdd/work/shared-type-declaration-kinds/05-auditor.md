Role: auditor
Commit: 4e7be9f1d0d5c340f538422456c440c03a198f2c
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 4e7be9f1d0d5c340f538422456c440c03a198f2c, base `origin/main` 17fb08c5a5ab7a84f58db1559fd13163279ffbcb.

State checks:
- The checkout is the head SHA.
- `git fetch origin main` resolves to the base SHA, and the base is an ancestor of head.
- The diff is what the handovers describe: one new test file, `src/Reihitsu.ArchitectureTests/General/TypeDeclarationKindsTests.cs`, plus this run's `sdd/work/` files.
- `scripts/lane-check.sh bug-fix` gives: confirmed, the claimed bug-fix lane, and the diff needs only tests.

Issue thoenissen/Reihitsu#1788 (`agent-filed`) is checked against `00-lead.md`, decisions 2–4:
- The goals "one shared kind set", "one edit", and "exclusion-table architecture test" are dropped as lead decisions. That is allowed for an `agent-filed` issue when no visible output changes.
- The kept goal, "no diagnostic or formatter output changes", holds because the diff contains no production code.
- The reworded goal, "a test fails when Roslyn's set of type-declaration kinds changes", is met by the new test.
- No issue requirement is left unaddressed or unrecorded.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A — the diff adds or changes no production predicate, guard, or exemption._

The test has its own discovery filters, and I checked how well they agree:
- **Pairs from parsed source.** A pair is recorded when `node is BaseTypeDeclarationSyntax || SyntaxFacts.IsTypeDeclaration(node.Kind())`.
- **Unpaired-type check.** It looks only at concrete `BaseTypeDeclarationSyntax` subtypes, so `DelegateDeclarationSyntax`, a `MemberDeclarationSyntax`, is outside it.
- **How a new delegate-like type is still caught.** Its kind would satisfy `IsTypeDeclaration`, so the unpaired-kind check would report it.
- **Result.** The two checks together cover both axes. Neither filter drops a member that the other one is needed for.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Pinned list of Roslyn type-declaration syntax types | `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyBaseTypeDeclarationTypesAreFullyEnumerated` (types only, tied to RH5404's registration and its comment at `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:88`); `TypeDeclarationKindsTests.TypeDeclarationSyntaxTypesAndKindsAreFullyEnumerated` (types and kinds, repository-wide) | Two tests pin the same external fact. Both compare against Roslyn itself, so they cannot drift apart silently: a Roslyn change fails both. Keeping RH5404's canary is recorded as skeptic internal choice 1A, adopted by lead decision 2. Not a finding. |
| Kind lists of type-declaration consumers (analyzer registrations, `ParameterListParentPolicy`, `AttributeTargetUtilities`, formatter visitors) | Unchanged by the diff | Not in scope. The skeptic's table in `01-skeptic.md` shows each list agrees with its own spec. |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `Reihitsu.ArchitectureTests.General.TypeDeclarationKindsTests.TypeDeclarationSyntaxTypesAndKindsAreFullyEnumerated` | The set of (type-declaration syntax type, kind) pairs in Roslyn 5.12.0 is exactly the nine pinned pairs, with no unpaired kind or type | A Roslyn version that adds a concrete `BaseTypeDeclarationSyntax` subtype, adds a kind that `IsTypeDeclaration` reports, or adds a kind on a `TypeDeclarationSyntax` type that `SyntaxFactory.TypeDeclaration` creates. Any of these changes the ordered sequence. | `Assert.AreSequenceEqual` over the full ordinally sorted set (not a subset or a count) | Adequate (FC-017). See the expected-value check and the hint below. |

The expected values come from an independent source (FC-035). I wrote a separate console probe outside the repository, in the session scratchpad, against Microsoft.CodeAnalysis.CSharp 5.12.0. It does not use the test's code. It found:
- **`SyntaxFactory.TypeDeclaration(SyntaxKind, string)`** accepts exactly Class, Struct, Interface, Record, RecordStruct, and Union.
- **The generated overloads `SyntaxFactory.RecordDeclaration(SyntaxKind, …)`** accept exactly RecordDeclaration and RecordStructDeclaration.
- **`SyntaxFacts.IsTypeDeclaration`** is true for ExtensionBlockDeclaration, UnionDeclaration, EnumDeclaration, DelegateDeclaration, and RecordStructDeclaration.
- **The concrete `BaseTypeDeclarationSyntax` subtypes** are Class, Struct, Interface, Record, Union, ExtensionBlock, and Enum. `DelegateDeclarationSyntax` derives from `MemberDeclarationSyntax`.

These results match the nine pinned pairs, and the expected array is in ordinal order.

The parse guard `Assert.IsFalse(root.ContainsDiagnostics, …)` stops the parsed-source probe from passing vacuously on a parse failure (FC-033). The test implementer's report that the test failed with both unpaired markers was not re-run. No finding depends on it.

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-005, FC-044: no production guard or predicate is added or changed.
- FC-004, FC-019, FC-020, FC-021: no edit touches trivia, gaps, or directives. The test formats no source.
- FC-006, FC-022–FC-027, FC-029, FC-036: no rewrite.
- FC-028: no formatter entry point is involved.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix.
- FC-010, FC-011, FC-012: no formatter phase.
- FC-013, FC-014: no rule or formatter behavior changes.
- FC-015, FC-016: no production policy changes. The overlap between the two canaries is assessed under *Policy ownership*.
- FC-038–FC-043: no unsupported shape is declared or checked.
- FC-031: no existing method changes. The new XML summaries describe the new code accurately, including the `RecordStructDeclaration` example.
- FC-032: no compiled production change, so no earlier project result is invalidated.
- FC-034: no reproduction. The run is a canary over existing behavior.
- FC-037: not applicable. The diff is not presented as text-only, and `verify-text-only.sh` correctly reports compiled content.
- Class matrices: none reaches a test-only, non-formatting change.

## Other checks
- **Specs:** the run names no requirement, and no spec changes. This is consistent with `04-spec-checker.md`.
- **Visible output:** the new test asserts no visible surface (no formatter output, diagnostic, code-fix result, rule page, or CLI behavior). Nothing is undecided.
- **Cost of special cases:** there is no production code, so none applies.
- **Comments:** no `TODO`, no commented-out code, and no tracker number in the test file.
- **Scope:** no edit outside the run's scope.
- **Code quality:**
  - The catch clauses `InvalidOperationException` and `ArgumentException` are exactly what `SyntaxFactory.TypeDeclaration` throws for a kind it does not create. Any other exception fails the test loudly, which is fine for a canary.
  - `SortedSet` with `StringComparer.Ordinal` makes the order deterministic.
  - Parsing with `LanguageVersion.Preview` keeps the extension-block and union probes valid if those features leave preview.

## Architecture findings
_None._

## Hints
- **The test only detects a new kind on an existing type through two hand-maintained Roslyn sources.** Those sources are the hand-written `SyntaxFactory.TypeDeclaration(SyntaxKind, …)` switch and `SyntaxFacts.IsTypeDeclaration`. A kind that Roslyn adds to `EnumDeclarationSyntax`, `ExtensionBlockDeclarationSyntax`, or `DelegateDeclarationSyntax`, but leaves out of `IsTypeDeclaration`, would not be detected. Roslyn's generated multi-kind factories, such as the `SyntaxFactory.RecordDeclaration(SyntaxKind, …)` overloads my probe found, are generated from the syntax model, so they would list such a kind. This is uncertain rather than a finding: Roslyn defines what a type declaration is through `IsTypeDeclaration`, and today every pinned kind passes through it. A later hardening could also try every kind on each public `SyntaxFactory` method whose first parameter is `SyntaxKind` and whose return type is a type-declaration type.
- **`03-cleaner.md` has placeholders.** It records "Commit: see git log" and "see below" instead of a commit and a test result. The spec checker already noted this. It is handover text that is deleted before publishing, and the validator runs the test.

## Follow-up candidates
_None_ beyond the four already listed in `00-lead.md` from `01-skeptic.md`. I spot-checked candidate 3: `rg ExtensionBlock src/Reihitsu.Formatter` finds `VisitExtensionBlockDeclaration` only in `LineBreakListRewriter` and `MemberLineStartRewriter`, not in `DeclarationBraceLineBreakRewriter`, which matches the skeptic's description.
