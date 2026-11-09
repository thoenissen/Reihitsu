Role: test implementer
Commit: 87ce0ad (implementer head; handover b09d21e), changes committed with this handover
Verdict: tests written

Repair round for lead decisions 8–11. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those decisions. No asserted output of an existing test changed except as decision 9 asks.

## thoenissen/Reihitsu#1597

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103…FixAllSkipsTheRenameThatWouldChangeTheMethodA{Foreach,QueryClause,Deconstruction,CollectionInitializer}Calls` (4) | SHARED-CASING-011, RH4103-003 (unchanged) | `Verify(src, fixed)` with `FixedState`/`BatchFixedState.MarkupHandling = MarkupMode.Allow` added (decision 8) | green |
| `RH4105…FixAllSkipsTheParameterNamedByACallerArgumentExpression` | SHARED-CASING-012, RH4105-003 (unchanged) | same, decision 8 | green |
| `RH4120…FixAllSkipsTheParameterNamedByACallerArgumentExpression` | SHARED-CASING-012, RH4120-003 (unchanged) | same, decision 8 | green |
| `RH4103…FixRenamesAnExtensionMethodThatNoCallOfAnotherDocumentSees` (replaces `FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees`, decision 9): extension `first` → `First` in `Ext`; another document of the project calls `values.First()` with `using System.Linq;` only and stays unchanged | SHARED-CASING-009 | `Verify(FirstExtensionSource, fixed)` + second document | green |
| `RH4114PublicPropertyCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameReachesAPositionalParameterThatACallerArgumentExpressionNames` (decision 11): interface property `count` implemented by `record Shape(int count, [CallerArgumentExpression("count")] string Expression = null) : IShape` | SHARED-FIX-003 | `Verify(src, src)` | green |
| `RH4114…FixRenamesThePositionalParameterOfARecordPropertyThatImplementsTheInterfaceProperty` (other side of decision 11: same shape without the attribute; the fix renames the interface property and the record's positional parameter `count` → `Count`) | SHARED-CASING-007 | `Verify(src, fixed)` | green |

## thoenissen/Reihitsu#1598

No change.

## thoenissen/Reihitsu#1016

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106…FixAllSkipsTheFieldsWhoseDerivedNamesTheRuleStillReports` | SHARED-CASING-008, RH4106-006 (unchanged) | decision 8 harness change | green |
| `RH4002…FixAllSkipsTheClassWhoseDerivedNameTheRuleStillReports` | SHARED-CASING-008, RH4002-002 (unchanged) | decision 8 | green |
| `RH4105…FixAllSkipsTheParameterWhoseDerivedNameTheRuleStillReports` | SHARED-CASING-008, RH4105-003 (unchanged) | decision 8 | green |

## thoenissen/Reihitsu#1015

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4102EventNameCasingAnalyzerTests` helpers `VerifyCustomEvent`, `VerifyCustomEventWithReference` now instantiate `RH4102EventNameCasingCodeFixProvider` (decision 10); doc comments updated; the four tests using them are unchanged | unchanged | `CSharpCodeFixVerifierTest<RH4102EventNameCasingAnalyzer, RH4102EventNameCasingCodeFixProvider>` | green |

No test references `RH4102CustomEventNameCasingCodeFixProvider` any more (`rg RH4102CustomEventNameCasingCodeFixProvider src --type cs` → only its own file). It is still named as a code owner in `sdd/specs/analyzer/RH4102.md:11` and `sdd/specs/README.md:109`; those rows must go with the type.

## thoenissen/Reihitsu#1084

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4005…FixAllSkipsTheInterfacesWithoutAUsableDerivedName` | RH4005-002, SHARED-CASING-008 (unchanged) | decision 8 | green |

## Whole run

### Not covered, and why

- **A "no fix" counterpart of the reworked SHARED-CASING-009 test with a name outside the SHARED-CASING-011 list.** I built `first` → `First` with a call `values.First()` that sees both `Enumerable.First` and `Ext.MyLinq.First`, once in another document of the same project and once in a dependent project (`using Ext;`). Both times the fix is offered: observed `/0/Test0.cs` renamed to `First`; expected by the test: no fix. The renamer resolves an ambiguous call by rewriting the call site, which is the follow-up candidate already in `00-lead.md`; a test asserting otherwise would be outside this repair, so both were dropped. Observation for the lead: the three existing tests `NoFixIsOfferedWhenTheNewNameMakesAQueryOf{AnotherDocument,TheSameDocument,ADependentProject}Ambiguous` (tagged SHARED-CASING-009) use `select` and now pass through SHARED-CASING-011 alone, so their SHARED-CASING-009 path is no longer exercised by them. SHARED-CASING-009's other no-fix tests (type, member, namespace collisions) are unaffected.
- **Decision 11 on RH4111–RH4113.** A property with the name of the record's positional parameter is already withheld by RH411x-002's split rule (`NoFixIsOfferedWhenTheRenameWouldSplitThePropertyFromItsPositionalParameter`), so the reachable cascade from a property to a positional parameter is through an interface property (RH4114) that the record's synthesized property implements. The check is one call site for every property rule: `rg -n "HasCallerArgumentExpressionNaming" src --type cs` → only `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`; one test.
- **CRLF, formatter surface, FC catalog.** Unchanged from `03-test-implementer.md`: renames only, no layout.

### Build observation (production, not mine)

`scripts/test.sh --project analyzer` in the default Release configuration does not build: `src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs(81,19): error RH7110: Members must be ordered from most to least accessible` (the new `internal IsValidName`). Debug reports it as a warning, plus `S2094` on `RH4102CustomEventNameCasingCodeFixProvider.cs(7,23)`. All runs below are Debug, as in `04-implementer.md`.

### Commands

```shell
scripts/test.sh --configuration Debug --project analyzer      # Passed: 7842, Failed: 0
scripts/test.sh --configuration Debug --project architecture  # Passed: 117, Failed: 0
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4114PublicPropertyCasingAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4103MethodNameCasingAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4102EventNameCasingAnalyzerTests"
```
