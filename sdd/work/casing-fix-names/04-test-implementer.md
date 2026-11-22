Role: test implementer
Commit: e33eea6
Verdict: #971: tests written; #1072: tests written; #1451: tests written; #1434: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md` (with *Maintainer answers*: all A), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, the three change docs under `sdd/changes/2026-11/`, and the spec diff `origin/main..6ebe40c` of `sdd/specs/shared/casing.md`, `shared/code-fixes.md`, and the RH4005, RH4102–RH4105, RH4112–RH4117, RH4121 specs. Expected values come from the spec examples and requirements. Every test below was run on `6ebe40c` (production code unchanged) and the "Expected now" column is what it did: **fails** means it failed on the assertion of the spec difference (the observed output is quoted where it is not obvious), **passes** means it pins behavior the change keeps.

Fixtures with combining marks, letter numbers, and connector punctuation are written with `\uXXXX` escapes in regular string literals (Core `DataRow`s, and `const string acute = "́"` etc. interpolated into `$$"""` raw strings); checked with `rg '\\u0301'`. No new test touches layout, so no CRLF variant or idempotency test is added (see *Not covered*).

`scripts/test.sh --project architecture`: 198 passed (SHARED-CASING-013 and SHARED-CASING-014 now have tagged tests). Analyzer, Naming namespace + RH2001: 1109 run, 58 failed, all of them listed below as failing; no other test fails. Core `CasingUtilitiesTests`: 132 run, 39 failed, all rows listed as failing below.

## thoenissen/Reihitsu#971

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Core `CasingUtilitiesTests.ToCamelCaseLowercasesTheLeadingUppercaseRunOfTheFirstWord` (`HTTPClient`, `XMLParser`, `IOStream`, `HTTP2Client`, `ABc`, `URLs`, `XML_Parser`, `HTTPClient_timeout`) | SHARED-CASING-006 | `Assert.AreEqual` | fails on 6 rows (`hTTPClient` etc.); `ABc`, `XML_Parser` pass |
| Core `ToCamelCaseTakesTheLeadingUppercaseRunFromTheFirstWordOnly` (`A_B`, `A_B_Cd`, `m_HTTPClient`, `X_Y_Z`, `X_YZ`, `I_O_Stream`) | SHARED-CASING-006 | `Assert.AreEqual` | passes (guards against the joined-name reading, question 10 A) |
| Core `ToUnderlineCamelCaseLowercasesTheLeadingUppercaseRunOfTheFirstWord` | SHARED-CASING-006 | `Assert.AreEqual` | fails on `HTTPClient`, `HTTP2Client`, `URLs`, `HTTPClient_timeout`; passes on `A_B`, `m_HTTPClient` |
| Core `ToPascalCaseKeepsALeadingAcronym` | SHARED-CASING-006 | `Assert.AreEqual` | passes (other side: PascalCase unchanged) |
| `RH4106PrivateFieldCasingAnalyzerTests.FixWritesALeadingAcronymInLowercase` (spec example, `URLs` row included) | SHARED-CASING-006, RH4106-004 | `Verify` | fails: `_hTTPClient`, `_xMLParser`, `_iOStream`, `_hTTP2Client`, `_uRLs` |
| `RH4106…FixTakesTheLeadingUppercaseRunFromTheFirstWordOnly` (spec example "a leading run that ends with the first word") | SHARED-CASING-006, RH4106-004 | `Verify` | passes |
| `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField` (`HTTPClient` + `HttpClient`: single fixes rename the first, withhold the second; Fix All skips both) | SHARED-CASING-010, RH4106-007; FC-008 | `Verify` with `BatchFixedState` | fails: today `_hTTPClient` and `_httpClient`, no collision |
| `RH4105MethodParameterCasingAnalyzerTests.FixWritesALeadingAcronymInLowercase` (spec example `XMLParser`) | SHARED-CASING-006, RH4105-003 | `Verify` | fails: `xMLParser` |
| `RH4107ProtectedFieldCasingAnalyzerTests.FixWritesALeadingAcronymInLowercase` | SHARED-CASING-006, RH4107-002 | `Verify` | fails: `_hTTPClient` |
| `RH4115LocalVariableCasingAnalyzerTests.FixWritesALeadingAcronymInLowercase` (`HTTPClient`, `URLs`) | SHARED-CASING-006, RH4115-003 | `Verify` | fails: `hTTPClient`, `uRLs` |
| `RH4117DeconstructionVariableCasingAnalyzerTests.FixWritesALeadingAcronymInLowercase` | SHARED-CASING-006, RH4117-002 | `Verify` | fails: `hTTPClient`, `iOStream` |
| `RH4110ConstFieldCasingAnalyzerTests.FixKeepsALeadingAcronym` (spec example `HTTPClient_timeout` → `HTTPClientTimeout`) | SHARED-CASING-006, RH4110-002 | `Verify` | passes |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixWritesALeadingAcronymOfTheFieldNameInLowercase` — **existing** characterization `VerifyFixKeepsUppercaseRunOfLeadingAcronymInFieldName`, renamed, tagged, assertion changed from `_hTTPClient` to `_httpClient` | RH2001-004, SHARED-CASING-006 | `Verify` | fails: `_hTTPClient` |

### Existing tests checked
- `rg '\b_?[a-z][A-Z]{2,}'` over the analyzer tests found only the RH2001 characterization above. `RH4106…FixDerivesUnderlineCamelCaseFromTheWordsOfTheOldName` (`FirstName`, `LAST_NAME`, `m_value`, `__double`, `URL`, `iPhone`) and Core `ToCamelCaseTest`/`ToUnderlineCamelCaseTest` (`FName`, `first_SName`) pin unchanged rows; left as they are, they pass.

### Not covered, and why
- A `_camelCase` RH4110 variant (`HTTPClient_timeout` → `_httpClientTimeout`) is pinned at the derivation level by the Core `ToUnderlineCamelCase…` row; the five rule-level tests share `CasingUtilities.ToCamelCase`/`ToUnderlineCamelCase` (`rg -l "ToCamelCase|ToUnderlineCamelCase" src` lists only `CasingUtilities` and the RH4105, RH4115, RH4117, RH4106, RH4107, and RH2001 fix providers), so one rule test per target style and per rule the skeptic named is enough.

## thoenissen/Reihitsu#1072

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4105…NoFixIsOfferedWhenTheDerivedNameNeedsTheVerbatimPrefix` (spec example: `Class`, `Object`, `Await` in an async method) | SHARED-CASING-013, RH4105-003 | `Verify(source, source)` | fails: `@class` written |
| `RH4105…FixRenamesToAContextualKeywordThatNeedsNoVerbatimPrefix` (spec example: `Var`, `Nameof`, `Await` in a non-async method) | SHARED-CASING-013, RH4105-003 | `Verify` | passes (other side) |
| `RH4105…NoFixIsOfferedWhenTheDerivedNameIsAwaitInsideAnAsyncLocalFunction` | SHARED-CASING-013; FC-027 | `Verify(source, source)` | fails: `int await` / `return await;` written |
| `RH4105…NoFixIsOfferedForAVerbatimParameterWhoseDerivedNameIsAReservedKeyword` (`@Class`) | SHARED-CASING-013; FC-023 | `Verify(source, source)` | fails: `@class` |
| `RH4105…FixAllRenamesTheOtherParametersAndKeepsTheParameterWhoseDerivedNameIsAKeyword` (`Class`, `Count`) | SHARED-CASING-013; FC-008 | `Verify`, markup allowed in fixed and batch state | fails: `@class` |
| `RH4115…NoFixIsOfferedWhenTheDerivedNameIsAReservedKeyword` (`String`) | SHARED-CASING-013, RH4115-003 | `Verify(source, source)` | fails: `@string` |
| `RH4115…NoFixIsOfferedWhenTheDerivedNameIsAwaitInsideAnAsyncLambda` | SHARED-CASING-013; FC-027 | `Verify(source, source)` | fails: `@await` |
| `RH4115…NoFixIsOfferedWhenAReferenceInsideAQueryExpressionWouldNeedTheVerbatimPrefix` (`Select` in `select Select`) | SHARED-CASING-013; FC-044 | `Verify(source, source)` | fails: `var select`, `select @select` |
| `RH4115…FixRenamesToAContextualKeywordThatNeedsNoVerbatimPrefix` (`Yield` in an iterator, `Dynamic`, `Async`) | SHARED-CASING-013, RH4115-003 | `Verify` | passes (other side) |
| `RH4117…NoFixIsOfferedWhenTheDerivedNameIsAReservedKeyword` (`Object`, `Total` in one deconstruction; `Total` renamed) | SHARED-CASING-013, RH4117-002 | `Verify`, markup allowed | fails: `@object` |

### Not covered, and why
- `Field` in an accessor and `Value` in a setter (skeptic: no fix today, "not this issue's concern"): their withholding comes from an existing guard, not from SHARED-CASING-013; no test added.
- No `_camelCase` or PascalCase test: SHARED-CASING-013's *Why* states only a camelCase target can produce a keyword.
- The query-keyword and lambda tests are the FC-044 check: a keyword list would miss `@select`, so they are the tests that tell a list-based guard from one that reads the Renamer's result.

## thoenissen/Reihitsu#1451

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Core `IsPascalCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation` (U+0301, U+216B, U+203F, U+FF3F, U+0903 Mc; negatives `café`, `Order_Line`, leading `Ⅻ`) | SHARED-CASING-001; FC-023 | `Assert.AreEqual` | fails on the five `true` rows |
| Core `IsCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation` | SHARED-CASING-002 | `Assert.AreEqual` | fails on the three `true` rows |
| Core `IsUnderlineCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation` | SHARED-CASING-003 | `Assert.AreEqual` | fails on the three `true` rows |
| Core `ToPascalCaseKeepsEveryIdentifierCharacterInItsWord` (`café_bar`, `order‿line`, `CAFÉ_NAME`, `CLASSⅫ_NAME` → `CLASSⅫName`, `ORDER‿LINE_NAME` → `ORDER‿LINEName`, `a_́b` → `Áb`) | SHARED-CASING-006 | `Assert.AreEqual` | fails on every row (marks and `‿` dropped) |
| Core `ToCamelCaseCountsACombiningMarkAsPartOfTheLetterInFrontOfIt` (`CAFÉ_NAME` → `caféName`, `ÉTATName` → `étatName`, `XÉ` → `xé`, `HTTPⅫClient` → `httpⅫClient`) | SHARED-CASING-006 | `Assert.AreEqual` | fails on every row |
| Core `ToTypeParameterNameDropsTheUnderscoresBehindAnExistingPrefix` (`T_Key`, `t_value`, `t_key`, `T_1`, `T_`, `t_`, `T__Key`, `T_T`) | RH4121-003 | `Assert.AreEqual` | fails on every row (`TTKey`, `TT`, …) |
| `RH4002ClassNameCasingAnalyzerTests.CombiningMarksAndLetterNumbersAreAcceptedInPascalCase` (`Café`, `ClassⅫ` not reported; `résumé` reported and fixed to `Résumé`) | SHARED-CASING-001, RH4002-001 | `Verify` | fails: `Café` and `ClassⅫ` reported |
| `RH4002…ConnectorPunctuationOtherThanUnderscoreIsAcceptedInPascalCase` (`First＿Name`, `Order‿line`) | SHARED-CASING-001, RH4002-001 | `Verify(source)` | fails: both reported |
| `RH4002…FixKeepsCombiningMarksAndConnectorPunctuationInTheirWord` (spec example `café_bar`, `order‿line`) | SHARED-CASING-006, RH4002-002 | `Verify` | fails: `CafeBar`, `OrderLine` |
| `RH4110…FixKeepsACombiningMarkInAWordOfUppercaseLetters` (spec example `CAFÉ_NAME`) | SHARED-CASING-006, RH4110-002 | `Verify` | fails: `CafeName` |
| `RH4105…CamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation` (`café`, `classⅫ`, `order‿line` not reported; `Résumé` → `résumé`) | SHARED-CASING-002, SHARED-CASING-006 | `Verify` | fails: all four reported |
| `RH4106…UnderlineCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation` (`_café`, `_classⅫ`, `_order‿line` not reported; `_Résumé` → `_résumé`) | SHARED-CASING-003, SHARED-CASING-006 | `Verify` | fails: all four reported |
| `RH4005InterfaceNameCasingAnalyzerTests.FixDropsTheUnderscoresBehindAnExistingPrefix` (spec `i_document`, `I_reader`; skeptic `I_O_Stream`; `i__writer`) | RH4005-002 | `Verify` | fails: `IIDocument`, `IIReader`, `IIOStream`, `IIWriter` |
| `RH4005…NoFixIsOfferedWhenNoUsableNameCanBeDerived` — **existing** data test, rows added: `I`, `i`, `I_`, `i_`, `i2Factory`, `i18nService` (spec example "a prefix with nothing usable behind it"); summary extended | RH4005-002, SHARED-CASING-008 | `Verify(source, source)` | new rows fail (`II`, `II2Factory`, `II18nService`); old rows pass |
| `RH4121TypeParameterNameCasingAnalyzerTests.FixDropsTheUnderscoresBehindAnExistingPrefix` (spec example `E<T_Key, t_value, T_1>`, `F<T_>`; `G<T__Key>`) | RH4121-003 | `Verify` | fails: `TTKey`, `TTValue`, `TT1`, `TT`, `TTKey` |
| `RH4121…NoFixIsOfferedWhenThePrefixWithAnUnderscoreDerivesTheNameOfAnotherTypeParameter` (`C<T_, T>`) | RH4121-003, SHARED-CASING-009 | `Verify(source, source)` | fails: `TT` written |

### Not covered, and why
- The SHARED-CASING-001 example names `café` as the reported class; the test reports `résumé` instead, because `café` → `Café` collides with the class `Café` of the same example and the collision (SHARED-CASING-009) would hide whether the mark is kept. The reporting side of `café` is pinned by the Core `IsPascalCase` row `café` → `false`.
- `Café`/`ClassⅫ` through the other PascalCase rules: each rule analyzer calls the shared `CasingUtilities.IsPascalCase`/`IsCamelCase`/`IsUnderlineCamelCase` (`rg -l "IsPascalCase|IsCamelCase|IsUnderlineCamelCase" src` lists only `CasingUtilities` and the rule analyzers); the decision is inside the shared method, so the rule tests check the wiring, not the arms; one rule test (RH4002) plus the Core rows. Same for `IsCamelCase` (RH4105) and `IsUnderlineCamelCase` (RH4106).
- `First＿Name` and `‿` are tested; Lt and Cf stay outside the covered set (spec author's internal choice) and get no test.
- `_`/`__` for RH4005/RH4121: pinned by the existing `NoFixIsOfferedWhenNoUsableNameCanBeDerived` rows and Core `ToTypeParameterNameTest`.
- Core `IsTypeParameterNameTest` row `T_Key` → `false` stays: the RH4121 analyzer is unchanged.

## thoenissen/Reihitsu#1434

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.MethodsWhoseNamesAReferencedAssemblyDictatesAreNotReported` (spec `Worker`/`Leaf`: `process` twice, `pair`, `run`) | SHARED-CASING-014, RH4103-001 | analyzer `Verify` + `CompiledAssemblyReference` | fails: 4 RH4103 reported |
| `RH4103…AnImplementationOfAStaticAbstractMethodOfAReferencedAssemblyIsNotReported` | SHARED-CASING-014, RH4103-001; FC-024 | analyzer `Verify` | fails: `create` reported |
| `RH4103…AnImplementationOfAnInterfaceMethodOfAReferencedProjectIsNotReportedAndRenamedWithIt` (`IContract.run` in project `Contracts` reported and fixed; `Impl.run` not reported, renamed with it) | SHARED-CASING-014, SHARED-CASING-007 | `Verify` with additional project | fails: `Impl.run` reported |
| `RH4103…ANativeEntryPointTakenFromTheMethodNameIsNotReportedAndALiteralEntryPointIsRenamed` — **existing** `FixIsWithheldForANativeEntryPointTakenFromTheMethodName`; `getpid` unmarked, `getppid` renamed to the spec's `parentId` → `ParentId` | SHARED-CASING-014, SHARED-FIX-003 | `Verify` | fails: `getpid` reported |
| `RH4103…ADllImportMethodWithoutEntryPointIsNotReported` — **existing** `NoFixIsOfferedForADllImportMethodWithoutEntryPoint` | SHARED-CASING-014, RH4103-001 | analyzer `Verify` | fails: `getpid` reported |
| `RH4103…ALibraryImportMethodWithoutEntryPointIsNotReported` — **existing** `NoFixIsOfferedForALibraryImportMethodWithoutEntryPoint`; CS8795 still expected | SHARED-CASING-014, RH4103-001 | analyzer `Verify` | fails: `getpid` reported |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWithoutEntryPoint`, `…ToALibraryImportImplementationWithoutEntryPoint`, `…OfAStaticVirtualMethodCascadesToADllImportImplementation`, `…ToADllImportImplementationInADependentProject` — **existing**; implementation unmarked, interface member still reported without a fix; tag added | SHARED-CASING-014, SHARED-FIX-003 | `Verify(source, source)` | fail: implementation reported |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToAnOverrideOfAReferencedAssemblyMember` — **existing**, rewritten to the new SHARED-CASING-009 example (`IWorker.process` reported, no fix; `Middle : MetaBase, IWorker` not reported) | SHARED-CASING-009, SHARED-CASING-014, SHARED-FIX-003 | `Verify(source, source)` | fails: `Middle.process` reported |
| `RH4103…ADirectOverrideOfAReferencedAssemblyMemberIsNotReported` — **existing** `NoFixIsOfferedForADirectOverrideOfAReferencedAssemblyMember` | SHARED-CASING-014, RH4103-001 | analyzer `Verify` | fails |
| `RH4103…AnImplementationOfAReferencedAssemblyInterfaceAndItsOverrideAreNotReported` — **existing** `NoFixIsOfferedWhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterface` | SHARED-CASING-014, RH4103-001 | analyzer `Verify` | fails: 2 reported |
| `RH4103…NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` — **existing**, assertion kept (question 11 A), tag and summary sentence added | SHARED-CASING-009, SHARED-CASING-014, SHARED-FIX-003 | `Verify(source, source)` | passes |
| `RH4103…FixRenamesAnOverrideChainDeclaredInSourceTogether` — **existing**, assertion kept (in-compilation chain reported), tag added | SHARED-CASING-009, SHARED-CASING-014 | `Verify` | passes |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAnOverrideToAMethodOfAReferencedProjectReferencedInExcludedCode` — **existing**, `Leaf.process` unmarked (changed SHARED-CASING-007 example) | SHARED-CASING-007, SHARED-CASING-014, SHARED-FIX-003 | `Verify(source, source)` with additional project | fails: `Leaf.process` reported |
| `RH4103…FixRenamesAnOverrideTogetherWithTheOverriddenMethodOfAReferencedProject` — **existing**, `Leaf.process` unmarked; the fix of `Base.process` in `BaseProject` renames both (question 8 A) | SHARED-CASING-007, SHARED-CASING-014 | `Verify` with additional project | fails: `Leaf.process` reported |
| `RH4102EventNameCasingAnalyzerTests.AFieldLikeEventOverrideOfAReferencedAssemblyEventIsNotReported`, `…AnEventOverrideWithAccessorsOfAReferencedAssemblyEventIsNotReported` — **existing** `NoFixIsOfferedWhenTheRenameCascadesTo…EventOverrideOfAReferencedAssemblyEvent` (two levels) | SHARED-CASING-014, RH4102-001 | analyzer `Verify` | fail: 2 reported each |
| `RH4102…AnImplementationOfAReferencedAssemblyInterfaceEventIsNotReported` | SHARED-CASING-014, RH4102-001 | analyzer `Verify` | fails |
| `RH4112…`, `RH4113…`, `RH4114…APropertyOverrideOfAReferencedAssemblyPropertyIsNotReported` — **existing** `NoFixIsOfferedWhenTheRenameCascadesToAPropertyOverrideOfAReferencedAssemblyProperty` (two levels) | SHARED-CASING-014, RH4112-001 / RH4113-001 / RH4114-001 | analyzer `Verify` | fail: 2 reported each |
| `RH4114…AnImplementationOfAReferencedAssemblyInterfacePropertyAndItsOverrideAreNotReported` — **existing** `NoFixIsOfferedWhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterfaceProperty` | SHARED-CASING-014, RH4114-001 | analyzer `Verify` | fails |
| `RH4114…NeitherPartOfAPartialPropertyThatImplementsAReferencedAssemblyInterfacePropertyIsReported` | SHARED-CASING-014, RH4114-001; FC-024 | analyzer `Verify` | fails: both parts reported |
| `RH4104LocalFunctionNameCasingAnalyzerTests.NoFixIsOfferedForAnExternLocalFunctionWhoseEntryPointIsNameofItself` — **existing** `NoFixIsOfferedForAnExternLocalFunctionBoundByItsName`, attribute changed to `EntryPoint = nameof(getpid)` (changed RH4104-002 example) | RH4104-002, SHARED-FIX-003 | `Verify(source, source)` | passes |
| `RH4104…AnExternLocalFunctionIsReportedOnlyWhenItsAttributeNamesAnEntryPoint` (spec native block: `getuid` not reported; `localPid` reported and fixed to `LocalPid`) | SHARED-CASING-014, RH4104-001 | `Verify` | fails: `getuid` reported |
| `RH4116TupleElementCasingAnalyzerTests.TupleElementNamesInTheSignatureOfAnOverrideOfAReferencedAssemblyMemberAreNotReported` (`pair()` two levels) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | fails: 4 reported |
| `RH4116…TupleElementNamesInTheSignatureOfAnExplicitImplementationOfAReferencedAssemblyMemberAreNotReported` (spec `IPairs.get` vs `ILocal.range`) | SHARED-CASING-014, RH4116-001; FC-024 | analyzer `Verify` | fails: `first`, `second` reported |
| `RH4116…TupleElementNamesSuppliedByATypeArgumentInThisCompilationAreReportedWhereTheTypeArgumentIsWritten` (spec `Pairs : IEnumerable<(string key, int value)>`, question 12 A) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | fails: `GetEnumerator` signature reported |
| `RH4116…TupleElementNamesNestedInTheSignatureOfADictatedMemberAreNotReported` (type argument, array element type, parameter type, tuple inside a tuple, property type) | SHARED-CASING-014, RH4116-001; FC-027 | analyzer `Verify` | fails: 8 reported |

### Existing tests checked and left
- `RH4103…NoFixIsOfferedWhenASourceInterfaceMethodIsImplementedByAReferencedAssemblyMember`: `ISource.process` overrides and implements nothing, so it is not dictated; unchanged, passes.
- `RH4103…NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWhoseEntryPointIsNameofTheMethod`, `…ForADllImportMethodWhoseEntryPointIsNameofTheMethod`, `FixRenamesADllImportMethodWithALiteralEntryPoint`, `FixRenamesALibraryImportMethodWithALiteralEntryPoint`, `FixRenamesAStaticAbstractMethodTogetherWithADllImportImplementationWithALiteralEntryPoint`: each attribute has an `EntryPoint` argument, so they stay reported; unchanged, pass.
- `RH4103…FixRenamesAFileLocalOverrideTogetherWithTheOverriddenMethodWhenNoExcludedCodeNamesIt` and the other file-local/nested excluded-code tests: same compilation; unchanged.
- `rg 'DllImport\("|LibraryImport\("'` with RH41 markup and `rg 'AdditionalProjects\['` over the analyzer tests: no other casing test reports a dictated name (`RH4103MethodNameCasingNameBindingTests` uses a consumer project only).
- The private helper `VerifyCustomEventWithReference` in `RH4102EventNameCasingAnalyzerTests` was used only by the rewritten custom-event test and is removed (unused-member build error otherwise).

### Not covered, and why
- "The same holds when `MetaBase` and `IMeta` are declared in another project of the solution" for events and properties (RH4102, RH4112–RH4114): the cross-project boundary is tested for methods (an override and an implementation). The decision is the declaring compilation of the overridden or implemented member; whether the implementer shares that check across RH4102/RH4103/RH4112–RH4114/RH4116 is not known before the code exists, so the lead may want one property test across projects once the implementation shows separate code paths.
- RH4112/RH4113 with an implicit interface implementation: protected and internal properties cannot implicitly implement an interface property; not reachable.
- RH4111 (private properties) and RH4105/RH4121 (non-goal of `shared/casing.md`): not affected by SHARED-CASING-014; no test.
- A `[DllImport]` method with `EntryPoint = nameof(getpid)` stays reported: pinned by the existing RH4103 test above.
- A `[LibraryImport]` local function: not valid C# (`LibraryImport` needs a `partial` method), no test.

## Failure classes

| Class | Applies | Covered by |
|---|---|---|
| FC-003 both sides | yes | each changed condition has a passing other-side test: PascalCase keeps acronyms, first-word-only run, contextual keywords fixed, `EntryPoint` keeps reporting, in-compilation chain and derived-type implementer stay reported |
| FC-008 Fix All | yes | RH4106 collision, RH4105 `Class`/`Count`, RH4117 `Object`/`Total` |
| FC-016 siblings | yes | every rule of SHARED-CASING-014 (RH4102, RH4103, RH4104, RH4112–RH4114, RH4116) and every camelCase rule for SHARED-CASING-013 (RH4105, RH4115, RH4117) has a test |
| FC-018 references through the Renamer | yes | every fix test has a reference to the renamed symbol (cross-project for `IContract.run` and `Base.process`) |
| FC-023 Unicode and verbatim identifiers | yes | #1451 tests, `@Class` |
| FC-024 partial, explicit, static interface members | yes | partial property, explicit `IPairs.get`, static abstract `create` |
| FC-027 nesting and enclosing scopes | yes | async lambda, async local function, query expression, nested tuple positions |
| FC-044 modelling the Renamer or compiler | yes | query-reference `@select` test (#1072); derived-type implementer and type-argument tuple tests (#1434) |
| FC-007 one-pass convergence | covered by the verifier | `Verify` re-analyzes the fixed code and fails on any remaining unmarked diagnostic |
| FC-017 weak assertion | checked | every test asserts exact output or exact diagnostic set |
| FC-001/002/015/036, trivia classes FC-004/005/019–022/026, formatter classes FC-010–014/028–030, unsupported-shape classes FC-038–043 | no | the fixes rename through `Renamer.RenameSymbolAsync` only and the analyzers judge names; no trivia, gap, layout, or formatter entry point is touched (skeptic, *Whole run*) |
| FC-033/034/035 | checked | reproductions use the spec's own inputs; expected values are from the spec examples, not from code |

## Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming|FullyQualifiedName~RH2001PrivateAutoProperties"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None_ beyond the RH4120 candidate already in `00-lead.md`.
