Role: test implementer
Commit: ade0e76 (head the tests were run on; tests and this handover are in the commit that adds this file)
Verdict: #1071: reproduced; #1268: reproduced; #1289: reproduced; #974: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Lead decisions 2, 3, 6, 7 and answers Q1–Q3 applied. Every "Expected now: fail" below was run on this head (`ade0e76` plus test files only) and fails on the issue's own expected-versus-actual difference; every "pass" test passes on this head and guards behavior that must stay. Test files are LF, formatted with `scripts/format.sh` (no change).

Note for readers: the escape inputs are written in the test sources as a backslash, `u`, and four hex digits (`\u0041bc`, `\u0054Key`, `namespace \u0041.B`, and in RH0001 the C# escape `\u00A0` inside a regular string). Some tool transports render these as the character itself; check bytes with `od -c` if a fixture looks wrong.

## thoenissen/Reihitsu#1071

File: `src/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Naming/RH4009NamespaceNotAllowedAnalyzerTests.cs` (new private helper `VerifyWithAllowList` in a `Helpers` region).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4001…VerbatimKeywordTypeNameMatchesFileNamedAfterItsValue` (`class @class` in `class.cs`) | RH4001-002 | `Verify(source, onConfigure)` | fail — reported at (1,14) |
| `RH4001…VerbatimTypeNameMatchesFileNamedAfterItsValue` (`@Order` in `Order.cs`) | RH4001-002 | same | fail — reported |
| `RH4001…VerbatimTypeNameIsReportedInFileNamedWithTheVerbatimPrefix` (`@class` in `@class.cs`) | RH4001-002, RH4001-004 | same | fail — not reported |
| `RH4001…VerbatimDelegateNameMatchesFileNamedAfterItsValue` (`delegate void @Handler()` in `Handler.cs`) | RH4001-002 | same | fail — reported |
| `RH4001…UnicodeEscapedTypeNameMatchesFileNamedAfterItsValue` (`\u0041bc` in `Abc.cs`) | RH4001-002 | same | fail — reported |
| `RH4001…UnicodeEscapedTypeNameIsReportedInFileNamedAfterAnotherType` (in `Invoice.cs`) | RH4001-002, RH4001-004 | same | pass (must stay) |
| `RH4001…VerbatimTypeParameterMatchesFileNamedAfterItsValue` (`Cache<@TKey>` in `Cache{TKey}.cs`) | RH4001-003 | same | fail — reported |
| `RH4001…VerbatimTypeParameterIsReportedInFileNamedWithTheVerbatimPrefix` (in `Cache{@TKey}.cs`) | RH4001-003 | same | fail — not reported |
| `RH4001…UnicodeEscapedTypeParameterMatchesFileNamedAfterItsValue` (`Cache<\u0054Key>` in `Cache{TKey}.cs`) | RH4001-003 | same | fail — reported |
| `RH4001…CodeFixRenamesFileAfterValueOfVerbatimTypeName` (`Invoice.cs` → `Order.cs`) | RH4001-005 | `Verify(source, null, onConfigure)` with `FixedState` | fail — renamed to `@Order.cs` |
| `RH4001…CodeFixRenamesFileWithVerbatimPrefixAfterValueOfKeywordTypeName` (`@class.cs` → `class.cs`) | RH4001-005 | same | fail — no diagnostic in test state |
| `RH4001…CodeFixRenamesFileAfterValueOfVerbatimTypeParameter` (`Invoice.cs` → `Cache{TKey}.cs`) | RH4001-003, RH4001-005 | same | fail — renamed to `Cache{@TKey}.cs` |
| `RH4001…CodeFixRenamesFileAfterValueOfUnicodeEscapedTypeName` (`Invoice.cs` → `Abc.cs`) | RH4001-005 | same | fail — renamed to `\u0041bc.cs` |
| `RH4001…NoFixIsOfferedWhenDocumentWithFileNameOfVerbatimTypeNameValueExists` (`@Order` in `Invoice.cs`, `Order.cs` beside it) | RH4001-006 | same, fixed state = test state | fail — fix offered (renames to `@Order.cs`) |
| `RH4009…NamespaceNamesAreComparedByTheirValue` (spec example, four declarations) | RH4009-002 | `Verify(source, onConfigure)` | fail — 4 reported, 1 expected |
| `RH4009…VerbatimPrefixOnFirstPartIsNotPartOfTheName` | RH4009-002 | `VerifyWithAllowList` | fail — reported |
| `RH4009…VerbatimPrefixOnLaterPartIsNotPartOfTheName` (`MyCompany.@Project`) | RH4009-002 | same | fail — reported |
| `RH4009…SpacesAroundDotAreNotPartOfTheName` | RH4009-002 | same | fail — reported |
| `RH4009…CommentBetweenPartsIsNotPartOfTheName` | RH4009-002 | same | fail — reported |
| `RH4009…LineBreakBetweenPartsIsNotPartOfTheName` | RH4009-002 | same | fail — reported |
| `RH4009…FileScopedNamespaceNameIsComparedByItsValue` (`namespace @MyCompany . Project;`) | RH4009-002 | same | fail — reported |
| `RH4009…FileScopedNamespaceNotOnTheListIsReportedOnItsWholeWrittenName` | RH4009-002 | same | pass (must stay; location on the whole written name) |
| `RH4009…UnicodeEscapeInNamespaceNameIsReplacedByItsCharacter` (`namespace \u0041.B`, list `["A.B"]`) | RH4009-002 | same | fail — reported |
| `RH4009…EntryWithVerbatimPrefixMatchesNoNamespace` (list `["@MyCompany.Project"]`, decision 5) | RH4009-002 | same | fail — `@MyCompany.Project` not reported |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `@class`/`@Order`/`@Handler`/escape name in file named after the value | LF | RH4001 analyzer | reproduced | reported vs not reported |
| `@class` in `@class.cs`, `Cache<@TKey>` in `Cache{@TKey}.cs` | LF | RH4001 analyzer | reproduced | not reported vs reported |
| Fix renames from `@`/escape names | LF | RH4001 code fix | reproduced | `@Order.cs`, `Cache{@TKey}.cs`, `\u0041bc.cs` vs `Order.cs`, `Cache{TKey}.cs`, `Abc.cs`; `@class.cs` gets no diagnostic, so no fix |
| RH4001-006 with `@Order` and `Order.cs` present | LF | RH4001 code fix | reproduced | fix offered vs withheld |
| Namespace with `@` (first/later part), spaces, comment, line break, escape; block- and file-scoped | LF | RH4009 analyzer | reproduced | reported vs not reported |
| Entry `"@MyCompany.Project"` | LF | RH4009 analyzer | reproduced | `namespace @MyCompany.Project` not reported vs reported |

CRLF not run: neither rule reads layout, and the inputs hold no line break that the comparison sees except `LineBreakBetweenPartsIsNotPartOfTheName`, whose trivia is the case under test. No formatter counterpart (analyzer-only names). Fix All: RH4001 returns none.

## thoenissen/Reihitsu#1268

File: `src/Reihitsu.Analyzer.Test/Design/RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests.cs`. The characterization `VerifyCreationThroughUsingAliasIsNotReported` (green on this head, asserting the issue's observed output) was rewritten to the specified output as `VerifyCreationThroughUsingAliasOfTheTypeIsReported` — the reproduction.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCreationThroughUsingAliasOfTheTypeIsReported` (issue's `throw new NIE()`) | RH2003-001, RH2003-004 | `Verify(source)` | fail — `NIE` not reported |
| `VerifyCreationsThroughTypeAndNamespaceAliasesAreReported` (spec example: `NIE`, `S.NotImplementedException`, `S::NotImplementedException`) | RH2003-001, RH2003-004 | same | fail — `NIE` not reported; `S.` and `S::` are (compiled; `S::` with a using alias works today) |
| `VerifyCreationThroughUsingAliasWithArgumentsAndInitializerIsReported` | RH2003-001 | same | fail |
| `VerifyCreationThroughVerbatimAliasNameIsReported` (`new @NIE()`) | RH2003-001; FC-023 | same | fail |
| `VerifyCreationThroughAliasDeclaredInNamespaceIsReported` | RH2003-001; FC-016 | same | fail |
| `VerifyCreationThroughGlobalAliasOfAnotherFileIsReported` (`global using NIE = …` in `Test1.cs`) | RH2003-001; FC-016 | `Verify(source, onConfigure)` | fail |
| `VerifyCreationThroughAliasOfAnotherTypeIsNotReported` (`NSE` alias not reported next to `NIE` reported) | RH2003-001, RH2003-003; FC-003 | `Verify(source)` | fail — on the `NIE` half |
| `VerifyAliasNamedNotImplementedExceptionOfAnotherTypeIsNotReported` (spec example) | RH2003-003 | same | pass (must stay) |
| `VerifyCreationThroughAliasOfDerivedTypeIsNotReported` | RH2003-003 | same | pass (must stay) |
| `VerifyTargetTypedCreationThroughAliasIsReportedOnNewKeyword` (`NIE _e = new();`) | RH2003-002 | same | pass (must stay; target-typed path is not pre-filtered) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `using NIE = System.NotImplementedException; throw new NIE();` | LF | RH2003 analyzer | reproduced | not reported vs reported on `NIE` |
| Sibling shapes: `@NIE`, alias in namespace block, global alias in another file, with arguments/initializer | LF | RH2003 analyzer | reproduced | not reported vs reported |

No code fix, no formatter counterpart; line endings irrelevant to a semantic check.

## thoenissen/Reihitsu#1289

File: `src/Reihitsu.Analyzer.Test/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.cs` (new constants `SharedExampleSource`, `CommentedOutCodeBlockSource`). The three characterizations green on this head (`VerifyCodeBlockTextInCommentsAndStringsIsReported`, `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsReported`, `VerifyEscapedAndEmailLikeCodeBlockTextIsReported`) assert the issue's observed output; they were rewritten, same inputs, to the specified output and renamed `…IsNotReported` — the reproductions. `VerifyFunctionsBlockIsNotReported` is unchanged (thoenissen/Reihitsu#1452). Per decision 7, C# comment and literal cases are written only inside `@code`/`@functions` blocks.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCodeBlockTextInCommentsAndStringsIsNotReported` | RH2109-001, SHARED-DIAG-003 | `Verify(TestCode, onConfigure, RazorCodeBlockSpan…)` | fail — 5 reported, only (3,1) expected |
| `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsNotReported` | RH2109-001, SHARED-DIAG-003; FC-022 | same | fail — 4 reported, only (1,1) expected |
| `VerifyEscapedAndEmailLikeCodeBlockTextIsNotReported` | RH2109-001, SHARED-DIAG-003 | same | fail — 2 reported, none expected |
| `VerifySharedExampleReportsOnlyTheDirective` (SHARED-DIAG-003 last example) | RH2109-001, SHARED-DIAG-003 | same | fail — 5 reported, only (5,1) |
| `VerifySharedExampleWithCarriageReturnLineFeedReportsOnlyTheDirective` | RH2109-001, SHARED-DIAG-003; FC-033 | same | fail |
| `VerifyCodeBlockInTheMiddleOfALineIsNotReported` (RH2109-001 `<div>` example, Q3) | RH2109-001, RH2109-003 | same | fail — (1,6) also reported; (3,5) expected |
| `VerifyCodeBlockIndentedWithTabIsReported` | RH2109-001, RH2109-003; FC-003 | same | pass (must stay) |
| `VerifyCodeWithoutBraceIsNotReported` | RH2109-001 | same | pass (must stay) |
| `VerifyEscapedCodeBlockTextAtStartOfLineIsNotReported` (`@@code {` starting a line) | RH2109-001, SHARED-DIAG-003 | same | fail |
| `VerifyCodeBlockInsideMultiLineRazorCommentIsNotReported` (commented-out block, then real block) | RH2109-001, SHARED-DIAG-003; FC-027 | same | fail |
| `VerifyCodeBlockInsideMultiLineRazorCommentWithCarriageReturnLineFeedIsNotReported` | RH2109-001, SHARED-DIAG-003; FC-033 | same | fail |
| `VerifyCodeBlockInsideMultiLineHtmlCommentIsNotReported` | RH2109-001, SHARED-DIAG-003; FC-027 | same | fail |
| `VerifyCodeBlockLineInsideRazorCommentWithinCodeBlockIsNotReported` | RH2109-001, SHARED-DIAG-003 | same | fail |
| `VerifyCodeBlockLineInsideDocumentationBlockCommentIsNotReported` (`/** */`) | RH2109-001, SHARED-DIAG-003; FC-022 | same | fail |
| `VerifyCodeBlockLineInsideVerbatimStringIsNotReported` (with `""`) | RH2109-001, SHARED-DIAG-003; FC-022 | same | fail |
| `VerifyCodeBlockLineInsideInterpolatedVerbatimStringIsNotReported` (`$@"…{CurrentCount}` / `@code {{`) | RH2109-001, SHARED-DIAG-003; FC-022 | same | fail |
| `VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported` (`$$"""` whose hole holds braces, real block after) | RH2109-001, SHARED-DIAG-003; FC-022 | same | fail |
| `VerifyCodeBlockLineInsideBlockCommentOfFunctionsBlockIsNotReported` | RH2109-001, SHARED-DIAG-003 | same | fail |
| `VerifyClosingBracesInsideLiteralsAndCommentsDoNotEndCodeBlock` (`"}"`, `'}'`, `// }` before a `/* @code { */`) | RH2109-001, SHARED-DIAG-003; FC-003 | same | fail |
| `VerifyOpeningBracesInsideLiteralsAndCommentsDoNotKeepCodeBlockOpen` (`"{"`, `'{'`, `// {`, then `<p>/*</p>` and a real block) | RH2109-001; FC-003 | same | pass (must stay) |
| `VerifyNestedBracesKeepCodeBlockOpenUntilItsClosingBrace` | RH2109-001, SHARED-DIAG-003; FC-027 | same | fail |
| `VerifyCommentAndStringStartsInsideLiteralsStartNothing` (`"/* \" "`, `'"'`, `// " /*`, `@"\"`) | RH2109-001; FC-022 | same | pass (must stay) |
| `VerifyCSharpCommentStartInMarkupDoesNotHideDirective` (`<p>/*</p>` before a real block) | RH2109-001; FC-003 | same | pass (must stay) |

The "pass (must stay)" block-extent tests pass today only because the regex reports everything; they become meaningful once the implementation tracks C# block extents and comment state, where getting a boundary wrong turns them red.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue's three inputs (HTML/Razor comment, C# comment/string/raw string in block, `@@`/email) | LF | RH2109 analyzer | reproduced | every occurrence reported vs only real directives |
| SHARED-DIAG-003 last example | LF, CRLF | RH2109 analyzer | reproduced | 5 vs 1 |
| Multi-line Razor comment around a block (common member, skeptic) | LF, CRLF | RH2109 analyzer | reproduced | 2 vs 1 |
| Mid-line `<div>@code {` (Q3) | LF | RH2109 analyzer | reproduced | reported vs not |

No code fix and no formatter surface.

## thoenissen/Reihitsu#974

Files: `src/Reihitsu.Analyzer.Test/Analyzer/RH0001ConfigurationFileMustBeValidAnalyzerTests.cs` (new private helper `VerifyConfiguration`), `RH4009NamespaceNotAllowedAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests.cs`. The CLI is not tested (decision 2).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TokenAfterRootObjectIsInvalidJson` (`{ } junk` at (1,5)) | RH0001-003 | `VerifyConfiguration` | fail — nothing reported |
| `SecondValueAfterRootObjectIsInvalidJson` (`{ } { }` at (1,5)) | RH0001-003 | same | fail |
| `LineCommentAfterRootObjectIsInvalidJson` (`{ } // c` at (1,5)) | RH0001-003 | same | fail |
| `BlockCommentAfterRootObjectIsInvalidJson` (`{ } /* c */` at (1,5)) | RH0001-003 | same | fail |
| `TokenOnLaterLineAfterRootObjectIsInvalidJson` (LF, (4,3)) | RH0001-003 | same | fail |
| `TokenOnLaterLineAfterRootObjectWithCarriageReturnLineFeedIsInvalidJson` (CRLF, (4,3)) | RH0001-003; FC-033 | same | fail |
| `NonJsonWhiteSpaceAfterRootObjectIsInvalidJson` (U+00A0 after a root holding `ä`, at character column 66) | RH0001-002, RH0001-003 | same | fail |
| `JsonWhiteSpaceAfterRootObjectIsValid` (spaces, tabs, CR, LF) | RH0001-003; FC-003 | same | pass (must stay) |
| `ContentAfterRootObjectReportsOnlyInvalidJson` (`{ "formatting": 1 } junk` at (1,21)) | RH0001-003 | same | fail — unknown section reported instead |
| `UnknownSectionIsReportedWhenOnlyWhiteSpaceFollowsRootObject` | RH0001-004; FC-003 | same | pass (must stay) |
| `ContentAfterRootArrayReportsOnlyInvalidJson` (`[ ] junk` at (1,5)) | RH0001-003 | same | fail — "root must be a JSON object" instead |
| `ContentAfterRootStringReportsOnlyInvalidJson` (`"a" junk` at (1,5)) | RH0001-003 | same | fail — "root must be a JSON object" instead |
| `RootArrayFollowedOnlyByWhiteSpaceIsReportedAsNotAnObject` | RH0001-004; FC-003 | same | pass (must stay) |
| `RH4009…ConfigurationWithTokenAfterItsRootObjectIsInactive` (spec example, Q2) | RH4009-001 | `Verify(source, onConfigure)` | fail — reported |
| `RH4009…ConfigurationWithSecondValueAfterItsRootObjectIsInactive` | RH4009-001 | same | fail |
| `RH4009…ConfigurationWithCommentAfterItsRootObjectIsInactive` | RH4009-001 | same | fail |
| `RH4009…ConfigurationWithOnlyWhiteSpaceAfterItsRootObjectStaysInEffect` | RH4009-001; FC-003 | same | pass (must stay) |
| `RH8402…ConfigurationWithContentAfterItsRootObject` (specification configuration + ` junk`) | RH8402-001 | `VerifySpecificationFile` | fail — RH8402 reported |

Positions (the `j`, the second `{`, the `/`) are the spec's claims (RH0001-003 example); they are asserted as written.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `{ } junk`, `{ } { }`, `{ } // c`, `{ } /* c */`, U+00A0 | LF | RH0001 analyzer | reproduced | nothing vs invalid JSON at the first offending character |
| Token on a later line | LF, CRLF | RH0001 analyzer | reproduced | nothing vs (4,3) |
| Root with its own problem, or non-object root, plus `junk` | LF | RH0001 analyzer | reproduced | root's problem reported vs only invalid JSON |
| Allow-list config plus `junk`/`{ }`/`// c` | LF | RH4009 analyzer | reproduced | reported vs inactive |
| Copyright config plus `junk` | LF | RH8402 analyzer | reproduced | reported vs inactive |

## Not covered, and why

- FC-001, FC-002, FC-004/005, FC-019–021, FC-026, FC-029: no edit of tokens, gaps, or line joins; none of the rules rewrites code. RH4001's fix renames a file only.
- FC-006, FC-009, FC-030: RH4001's fix changes no content (existing tests assert unchanged content); no other rule has a fix.
- FC-007: one-pass convergence of RH4001's fix is checked by `Verify` re-analyzing the fixed state (`@Order` in `Order.cs` must then be clean). FC-008: RH4001 returns no Fix All provider; the other rules have no fix.
- FC-010–014, FC-028, FC-038–043: no formatter change, no declared shape.
- FC-018, FC-036: RH4001 renames the file, not a symbol.
- FC-024, FC-025: no member-kind or partial dimension in these rules; RH4001's two identifier arms (`BaseTypeDeclarationSyntax`, `DelegateDeclarationSyntax`) are each tested with `@`.
- RH4001 kind arms (struct/interface/enum/record) with `@`: one call site — `rg -n "GetIdentifier|Identifier.Text" src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs`, shared symbol `GetIdentifier` returns `BaseTypeDeclarationSyntax.Identifier` for all of them; class tested.
- RH4009 nested namespace declarations: follow-up candidate (skeptic 02), no spec decides it; not tested.
- RH2109 C# comments and literals in `@{ }`, control-flow, and expression hosts: outside the run's scope by decision 7 (follow-up); no test written, so no test pins the known gap.
- RH2109 UTF-8 raw string (`"""…"""u8`) and `///` comments: `///` cannot hold a line-starting `@code` (mid-line, Q3); the UTF-8 suffix comes after the closing quotes of a raw string already covered by `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsNotReported`.
- RH2003 CRLF and the formatter counterpart: a semantic check of an object creation reads no layout.
- RH0001: the U+00A0-only file (reported as empty today, Unicode white space) is pre-existing and outside the issue (skeptic 02).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4009NamespaceNotAllowedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Analyzer.RH0001ConfigurationFileMustBeValidAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests"
```

On this head the six classes give 61 failures, all among the tests listed above as "fail", and no previously passing test fails.
