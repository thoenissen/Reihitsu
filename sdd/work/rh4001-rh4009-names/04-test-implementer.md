Role: test implementer
Commit: bde2b43
Verdict: tests written

Issue thoenissen/Reihitsu#1085, lane behavior-change. My prompt carried no intended fix or hoped-for verdict. Expected values come from the examples of RH4001-006, RH4001-008, and RH4009-003 and the skeptic's candidates R1–R7, N1–N5 (02-skeptic.md), never from code.

## Tests

`R` = red on bde2b43 for the stated difference, to turn green; `G` = green on bde2b43, must stay green.

File `tests/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs` (new private helper `VerifyRename` in a new `#region Helpers`: one-type file, `Verify(source, null, …)` with the renamed document in `FixedState`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixKeepsTimestampPrefix (R1) | RH4001-008 | `VerifyRename` → `Verify` | R: expected `20210501083748_SomeMigration.cs`, was `SomeMigration.cs` |
| CodeFixKeepsTimestampPrefixAndDottedFileSuffix (R2) | RH4001-008 | `VerifyRename` | R: expected `20210501083748_SomeMigration.Part1.cs`, was `SomeMigration.Part1.cs` |
| CodeFixKeepsTimestampPrefixForGenericType (R3) | RH4001-003, RH4001-008 | `VerifyRename` | R: expected `20210501083748_Cache{T}.cs`, was `Cache{T}.cs` |
| CodeFixKeepsTimestampPrefixAndRenamesAfterValueOfVerbatimTypeName (FC-023) | RH4001-008 | `VerifyRename` | R: expected `20210501083748_SomeMigration.cs`, was `SomeMigration.cs` |
| CodeFixReplacesThirteenDigitPrefixWithTheName (R4, FC-003) | RH4001-005, RH4001-008 | `VerifyRename` | G: `2021050108374_InitialCreate.cs` → `SomeMigration.cs` |
| CodeFixReplacesFifteenDigitPrefixWithTheName (FC-003) | RH4001-005, RH4001-008 | `VerifyRename` | G: `202105010837480_InitialCreate.cs` → `SomeMigration.cs` |
| CodeFixReplacesNonDigitPrefixWithTheName (R5) | RH4001-005, RH4001-008 | `VerifyRename` | G: `V2021050108374_InitialCreate.cs` → `SomeMigration.cs` |
| CodeFixKeepsTimestampPrefixWithoutNameBehindIt (skeptic optional) | RH4001-008 | `VerifyRename` | R: expected `20210501083750_SomeMigration.cs` from `20210501083750_.cs`, was `SomeMigration.cs` |
| CodeFixKeepsOnlyTheFirstOfTwoTimestampPrefixes (skeptic optional) | RH4001-008 | `VerifyRename` | R: expected `20210501083751_SomeMigration.cs` from `20210501083751_20210501083748_X.cs`, was `SomeMigration.cs` |
| NoFixIsOfferedWhenDocumentWithTheTimestampPrefixedNewFileNameExists (R6) | RH4001-006 | `Verify`, unchanged `FixedState` | R: the fix is offered today and renames to `SomeMigration.cs` (verifier reports a content mismatch of the document list) |
| FixIsOfferedWhenOnlyDocumentWithTheUnprefixedNameExists (R7) | RH4001-006, RH4001-008 | `Verify` | R: no fix today (`SomeMigration.cs` withholds it); expected rename to `20210501083748_SomeMigration.cs` next to the untouched `SomeMigration.cs` |

Every RH4001 fix test runs through `Verify`, which re-analyzes the fixed state, so convergence in one pass (FC-007) and "offered only when it changes something" (FC-009) are checked by each.

File `tests/Reihitsu.Analyzer.Test/Naming/RH4009NamespaceNotAllowedAnalyzerTests.cs` (new private helper `VerifyWithAllowListEntries(testCode, params string[])`; `using System.Linq;` added):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| NestedNamespaceWhoseFullNameIsOnTheListIsNotReported (N1) | RH4009-003 | `VerifyWithAllowListEntries` | R: `["A","A.B"]`, expected none, today `B` at (3,15) |
| NestedNamespaceWhoseOwnNameOnlyIsOnTheListIsReported (N2) | RH4009-003 | same | R: `["A","B"]`, expected `B`, today none |
| FileLevelNamespaceNextToAnotherIsComparedByItsOwnName (N2, other side of "nested") | RH4009-003 | same | G: file-level `A` and `B` with `["A","B"]`, none |
| NamespacesNestedOnSeveralLevelsAreComparedByTheirFullNames (N3, FC-027) | RH4009-003 | same | R: expected only `E`, today 3 diagnostics |
| EnclosingNamespaceIsComparedOnItsOwn (N4) | RH4009-003 | same | R: `["A.B"]`, expected only `A`, today `A` and `B` |
| IntermediateEnclosingNamespaceIsComparedByItsFullName (FC-027) | RH4009-003 | same | R: `["A","A.B.C"]` on `A { B { C } }`, expected only `B`, today `B` and `C` |
| NestedNamespaceIsReportedOnItsWholeOwnName (location) | RH4009-003 | same | R: `["A","B.C"]` on `A { B.C }`, expected on the whole `B.C`, today none |
| FullNameOfNestedNamespaceIsComparedCaseSensitively | RH4009-003 | same | R: `["A","B","a.B"]`, expected `B`, today none |
| VerbatimPrefixOnEnclosingNamespaceIsNotPartOfTheFullName (N5, FC-023/FC-015) | RH4009-003 | same | R: `@A { B }`, `["A","A.B"]`, expected none |
| VerbatimPrefixOnNestedNamespaceIsNotPartOfTheFullName (N5) | RH4009-003 | same | R: `A { @B }`, expected none |
| UnicodeEscapeInEnclosingNamespaceIsReplacedInTheFullName (N5) | RH4009-003 | same | R: `\u0041 { B }` (bytes checked), expected none |
| WhitespaceAndCommentInEnclosingNamespaceNameAreNotPartOfTheFullName (N5) | RH4009-003 | same | R: `A . /* x */ B { C }`, `["A.B","A.B.C"]`, expected none |

Run on bde2b43 plus these tests: 96 tests in the two classes, 19 red (exactly the `R` rows), 77 green (73 pre-existing plus the 4 `G` rows). `EveryRequirementOfAnEnforcedSpecIsCovered` passes with the new tags.

## Not covered, and why

- Existing tests whose asserted output changes: none. `rg` over `tests/` for `RH4001`, `RH4009`, `allowedNamespaceDeclarations`, and 14-digit prefixes: `TimestampPrefixedFileMismatch` pins only the diagnostic (unchanged), `TimestampPrefixedFileMatch` pins the matching prefixed file (unchanged); the tooling tests (`CodeFixRunCommandTests`, fake analyzer/fix helpers) and `RuleDocumentationExampleOptOuts` only mention the rules. No RH4009 test had a nested namespace. The non-prefixed collision examples of RH4001-006 stay pinned by `NoFixIsOfferedWhenDocumentWithTheNewFileNameExists` and its siblings.
- Non-ASCII digits in the prefix (skeptic internal choice 1): the spec leaves ASCII versus Unicode digits undecided, so no test asserts either outcome.
- Mixed file-scoped and block-scoped nesting: does not compile (CS8955), unspecified by lead decision 4; no test.
- LF/CRLF: no layout is touched — RH4001 changes only the file name and RH4009 only reports; no line-ending variant.
- Fix All (FC-008): the RH4001 provider has none (`SingleCodeFixTestsBase`), and the analyzer reports one diagnostic per file.
- FC-018 (Renamer): the fix renames the file, not the type; no reference to retarget.
- FC-004/019–021, formatter idempotency, token-gap comment/directive cases: no trivia is edited and no formatter path is involved.
- Unicode escape in the nested own name: one call site with the enclosing-name value is not proven, but the own-name value already goes through RH4009-002's `GetNamespaceName` path, which `UnicodeEscapeInNamespaceNameIsReplacedByItsCharacter` pins; the verbatim prefix on the nested name has its own test above.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests|FullyQualifiedName~Naming.RH4009NamespaceNotAllowedAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~EveryRequirementOfAnEnforcedSpecIsCovered"
```

## Follow-up candidates

_None._
