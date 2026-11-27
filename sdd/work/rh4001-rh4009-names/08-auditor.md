Role: auditor
Commit: 0ef284fbf4b51a4cb7b87188d000f5142f3c8d14
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1085

### Scope

Full audit. Head `0ef284f` (checked out, clean tree); base `origin/main` = `fe7b8f85d45a85e3354da783ca73a20b3e39eb6d`, an ancestor of the head. The diff matches handovers 01–07: two production files (`RH4001TypeNameShouldMatchFileNameHelper.cs`, `RH4009NamespaceNotAllowedAnalyzer.cs`), two test files, the RH4001/RH4009 specs, `requirement-ids.txt`, the change doc, two rule pages, and `sdd/work/`. `scripts/lane-check.sh behavior-change`: confirmed.

Requirements checked against the code: RH4001-008 (prefix kept as written, name behind it from RH4001-001/003/005, suffix from the first dot, extension), RH4001-006 (collision check reads `GetNewFileName` → `GetRenamedFileName`, the same name `ApplyCodeFixAsync` writes), RH4001-005 unchanged for unprefixed names, RH4001-002 comparison unchanged (`stem.Substring(GetTimestampPrefixLength(stem))` is the old `StripTimestampPrefix`), RH4009-003 (full name from every enclosing `BaseNamespaceDeclarationSyntax`, outermost first, values by `NamespaceCasingHelper.GetLocations`; location stays `namespaceSyntax.Name`), RH4009-002 unchanged for a file-level declaration (`AncestorsAndSelf` yields only itself). Every example of RH4001-008, the new RH4001-006 sentence, and RH4009-003, and every skeptic input R1–R7 and N1–N5, has a test that asserts it. Both maintainer decisions of the issue are met; the issue's "Done when" holds.

Traced RH4001 convergence for the shapes the prefix can take: a renamed `<prefix><Stem><suffix>.cs` has a stem of `<prefix><Stem>` (the expected stem holds no dot: identifier values, `{`, `,`, `}`), the same predicate finds the same 15-character prefix, and the comparable name is `Stem`. An unprefixed name cannot gain a prefix through the fix: the expected stem starts with an identifier-start character, never a `char.IsDigit` (Nd) character. `_` and `_20210501083748_A` as type names converge the same way.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer/Rules/Naming/RH4009NamespaceNotAllowedAnalyzer.cs:58` `GetFullNamespaceName`; `src/Reihitsu.Analyzer/Rules/Organization/RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.cs:53-56` `GetTypeKey` | FC-015 | – | PR-introduced | The value of "the full namespace a declaration declares" — the `GetLocations` values of every enclosing `BaseNamespaceDeclarationSyntax`, outermost first, joined by `.` — now has two private owners. RH7002 already computed it (multi-line chain, which the implementer's `rg "Ancestors.*NamespaceDeclaration"` could not match); the diff adds a second copy and neither justifies nor states parity. Both produce the same string today (same value helper, same order, same separator), so no input fails; a later change to one (e.g. how a value is taken, or a file-scoped/block-scoped rule) would not reach the other. | Put one method in `src/Reihitsu.Analyzer/Core/NamespaceCasingHelper.cs`, which both already use (e.g. the full name of a `BaseNamespaceDeclarationSyntax`), and call it from RH4009 and from RH7002's `GetTypeKey` (on the type's nearest namespace ancestor); RH7002's existing tests pin its side. If the lead keeps RH7002 out of the run, record the parity in the PR and file the extraction as a follow-up. |
| 2 | low | `src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs:22-24` and `:63-65` | FC-015 | – | PR-introduced | "The stem is the file name without extension up to its first dot" is stated twice in one class: `GetComparableFileName` and `GetRenamedFileName` each compute `fileNameWithoutExtension`, `firstDotIndex`, and now `stem`. The prefix predicate is shared, but the span it is applied to is not; if one stem computation changes, the analyzer strips one prefix and the fix keeps another, and the fix no longer converges. Identical today. | Compute the stem in one private method used by both (the fix still needs `firstDotIndex` for the suffix, or derive the suffix as `fileNameWithoutExtension.Substring(stem.Length)`). |
| 3 | low | `documentation/rules/RH4001.md:55-62` | – | – | PR-introduced | The added sentence "The code fix keeps the prefix and replaces only the name after it:" now owns the colon, so the example behind it (`20210501083748_InitialCreate.cs` holding `class InitialCreate`) reads as an illustration of the fix, although it shows an accepted file with no before/after. Text only; cannot reach compiled behavior, diagnostics, or rule metadata. | End the first sentence with the colon and place the fix sentence after the example block, or end the fix sentence with a period. No content change, so the maintainer's answer 1 still covers the page. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Timestamp prefix present (RH4001-002 / RH4001-008) | first 15 chars of the stem (file name without extension, up to its first dot); 14 `char.IsDigit` + `_` at index 14, stem length > 14 | the same `GetTimestampPrefixLength` on the same-built stem in `GetComparableFileName` | same function, same span (two copies of the stem computation, finding #2) | holds |
| Collision check (RH4001-006) | `GetNewFileName(document, type)` vs other documents' `Name`, OrdinalIgnoreCase, same `Folders` | `ApplyCodeFixAsync` writes `GetNewFileName(document, type)` | same name source | holds |
| "Nested" qualifier (RH4009-003, FC-002) | every `BaseNamespaceDeclarationSyntax` in `AncestorsAndSelf` | spec: block-scoped inside block-scoped | the predicate also joins a file-scoped/block-scoped mix, which is CS8955 and does not compile (lead decision 4) | holds |
| Diagnostic location (RH4009-003) | `namespaceSyntax.Name` | RH4009-002 location | unchanged | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Migration timestamp prefix | `rg -i "timestamp\|migration" src`: only `RH4001TypeNameShouldMatchFileNameHelper.GetTimestampPrefixLength` | one owner |
| Stem up to the first dot | `RH4001TypeNameShouldMatchFileNameHelper.GetComparableFileName`, `.GetRenamedFileName` | two copies, finding #2 |
| Value of a namespace name | `NamespaceCasingHelper.GetLocations` (RH4007, RH4008, RH4009, RH7002) | one owner |
| Full namespace of a declaration | `RH4009NamespaceNotAllowedAnalyzer.GetFullNamespaceName`, `RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.GetTypeKey` (`rg "AncestorsAndSelf\(\)\|Ancestors\(\)" src \| rg -i namespace`) | two owners, finding #1 |
| New file name of the RH4001 fix | `GetRenamedFileName`; `GetNewFileName`'s `FilePath is null` branch (see dismissed) | one owner for reachable inputs |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CodeFixKeepsTimestampPrefix`, `…AndDottedFileSuffix`, `…ForGenericType`, `…AndRenamesAfterValueOfVerbatimTypeName`, `…WithoutNameBehindIt`, `CodeFixKeepsOnlyTheFirstOfTwoTimestampPrefixes` | the fix renames to the exact prefixed name, content unchanged, and the renamed file is not reported (one iteration) | another document name, changed content, a diagnostic in the fixed state, or a second iteration | `VerifyRename` → `Verify` with `FixedState.Sources` (name and content compared; fixed-state diagnostics and iteration count verified by the testing framework) | adequate |
| `CodeFixReplacesThirteenDigitPrefixWithTheName`, `…FifteenDigit…`, `…NonDigit…` | both sides of the prefix boundary (FC-003) rename as RH4001-005 | a kept 13/15-digit or `V…` prefix | `VerifyRename` | adequate |
| `NoFixIsOfferedWhenDocumentWithTheTimestampPrefixedNewFileNameExists` | no action when the prefixed target exists | any renamed document | `Verify`, fixed state = test state with markup | adequate (red on base per 04) |
| `FixIsOfferedWhenOnlyDocumentWithTheUnprefixedNameExists` | `SomeMigration.cs` does not withhold the fix; rename next to it | no fix, or a different name | `Verify` | adequate |
| 12 RH4009 tests (`NestedNamespaceWhoseFullNameIsOnTheListIsNotReported` … `WhitespaceAndCommentInEnclosingNamespaceNameAreNotPartOfTheFullName`) | full-name comparison, location on the nested declaration's whole own name, enclosing declarations on their own, case sensitivity, value semantics of the enclosing names | a missing or extra diagnostic, or another span | `VerifyWithAllowListEntries` → `Verify` with markup (ID and exact span) | adequate; `A` is literal in the raw string (line 804, checked) |

### Visible output is decided

Every new expected value: the prefixed renames and the RH4009 nested results are maintainer answers 1–2 (`00-lead.md`, change doc `Decisions`); the 13/15-digit and `V…` renames are demanded by the unchanged RH4001-005; the two collision results follow from the unchanged normative sentence of RH4001-006 applied to the name of answer 1; RH4009 location, the enclosing declarations' reports, case sensitivity, and value semantics are demanded by the unchanged RH4009-002 and today's location. Both rule-page sentences restate answers 1–2 (change doc `Decisions`, change 3). Nothing undecided.

### Failure classes dismissed

- FC-001 — no guard was narrowed; the prefix span is unchanged.
- FC-002 — checked under Guard scope; the excess over "block-scoped" is non-compiling code.
- FC-003 — both sides tested: 13/14/15 digits, non-digit prefix, empty name behind the prefix; nested versus file-level.
- FC-005, FC-019, FC-020, FC-021, FC-004 — no trivia, gap, or token is edited: RH4001 renames a document, RH4009 only reports.
- FC-006, FC-022, FC-025, FC-026, FC-029 — no syntax rewrite.
- FC-007, FC-009 — traced (Scope) and asserted by every fix test through the fixed-state diagnostics and iteration count.
- FC-008 — the RH4001 provider returns no Fix All provider (`SingleCodeFixTestsBase`).
- FC-010–FC-014, FC-028, FC-030, FC-032, FC-038–FC-043 — no formatter path; the formatter judges neither file names nor allow-lists.
- FC-016 — the other namespace-name consumers (RH4007, RH4008) check each identifier's casing, which needs no full name; no sibling compares a nested namespace by its own name.
- FC-018, FC-036 — the fix renames the file, not the type.
- FC-023 — verbatim, Unicode-escape, and comment-in-name inputs tested on both rules; `_`-only and digit-bearing type names traced to converge.
- FC-024 — partial type covered by the dotted-suffix test.
- FC-027 — several levels, siblings, a dotted intermediate name, and the intermediate declaration's own report are tested.
- FC-031 — the summaries of `GetComparableFileName`, `GetRenamedFileName`, `GetTimestampPrefixLength`, `GetFullNamespaceName`, and `OnNamespaceDeclarationSyntax` describe the new code.
- FC-033, FC-034, FC-035 — tests run the reported shapes on real file paths, and expected values come from the spec examples (04).
- FC-037 — no text-only claim.
- FC-044 — the full name is syntactic concatenation, not a modelled compiler result.
- `GetNewFileName`'s `document.FilePath is null` branch writes `GetExpectedFileNameStem + ".cs"`, which drops the prefix (and, already on `main`, the suffix). The skeptic's reason for unreachability ("the analyzer returns early when the tree path is empty") may not hold, since a path-less document's tree can carry its name; but no caller of the repository produces a document without a file path (IDE projects and `apply-fix.sh` load files from disk), so it is dropped under *Supported inputs*.

### Architecture findings

Finding #1 has a structural cause: one rule — the full namespace a declaration declares — is stated in two places, `RH4009NamespaceNotAllowedAnalyzer.GetFullNamespaceName` and `RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.GetTypeKey`, next to the shared value helper `NamespaceCasingHelper`. No input shows them disagreeing today.

### Hints

_None._

### Follow-up candidates

_None_, unless the lead keeps RH7002 out of the run under finding #1; then the extraction into `NamespaceCasingHelper` is one (structural cause: one rule stated in two places).

## Whole run

Lane check confirmed (behavior-change). No `TODO` and no tracker number in the added source or test lines. No test or build was re-run: no finding turned on one; cleaner (06) build and implementer (05) analyzer/architecture runs trusted.
