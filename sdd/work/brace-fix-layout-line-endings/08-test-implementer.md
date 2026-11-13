Role: test implementer
Commit: b599eba (every "Expected now" below measured on this production code, Debug build; tests added on top)
Verdict: tests written

Follow-up round for decision 12, answer 7 / SHARED-ATTR-022 (decision 14), and decision 13 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

### Decision 12 — Fix All scenarios of the split rules

File `src/Reihitsu.Analyzer.Test/Formatting/RH55xx…AttributeListsMustFollowShapeRulesAnalyzerTests.cs` for RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531. Each `GetFixAllScenario` is rewritten the way RH5514's was in 05. The expected `[Second]⏎[Third]` becomes `[Second] [Third]` (with the rule's target where it has one, such as `[assembly: Second] [assembly: Third]`). `NumberOfFixAllIterations` goes from `2` to `-2`, which means at most two. The comment now states what the scenario asserts. The tags are unchanged: they are inherited from the base class.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixAllCorrectsEveryDiagnostic` ×13 (rules above) | SHARED-FIX-001, SHARED-FIX-002 (inherited tags); FC-008 | `Verify` (batch base) | green |

### Answer 7 — SHARED-ATTR-022

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5518 `VerifyDiagnosticWithoutCodeFixForMergedFieldSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` (was `VerifyDiagnosticFor…`, asserted the broken-open accessor list; now the spec example `public int Value { [field: First, Second] get => 1; }`) | RH5518-004, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5528 `VerifyDiagnosticWithoutCodeFixForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` (was `VerifyDiagnosticFor…`, same input with `return:`) | RH5528-004, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5528 `VerifyDiagnosticWithoutCodeFixForReturnSpecifierListOnTheLineOfTheAccessorListBrace` (RH5528-004 example `int P { [return: A, B] get => 1; }`) | RH5528-004, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5528 `VerifyDiagnosticWithoutCodeFixForReturnSpecifierListBehindAnEarlierAccessorOnTheLineOfTheAccessorListBrace` (`int P { get => _p; [return: A, B] set { _p = value; } }`, the shape 07 names as covered) | SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5528 `VerifyCodeFixSplitsAReturnSpecifierListBelowTheLineOfTheAccessorListBrace` [LF/CRLF] (`{` on its own line; `[return: A]⏎[return: B] get => 1;`) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5516 `VerifyDiagnosticWithoutCodeFixForPropertySpecifierListOnTheLineOfTheAccessorListBrace` (RH5516-004 example) | RH5516-004, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5518 `VerifyCodeFixSplitsAFieldSpecifierListBelowAnAccessorListBraceOnItsOwnLine` [LF/CRLF] (SHARED-ATTR-022 offered example, `Value`) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5518 `VerifyCodeFixSplitsAFieldSpecifierListBelowAnAccessorListBraceBehindThePropertyName` [LF/CRLF] (SHARED-ATTR-022 offered example, `Other {`) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5520 `VerifyDiagnosticWithoutCodeFixForEventSpecifierListOnTheLineOfTheAccessorListBrace` (RH5520-004 example) | RH5520-004, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |
| RH5531 `VerifyDiagnosticWithoutCodeFixForAccessorListOnTheLineOfTheAccessorListBrace` (RH5531-006 and SHARED-ATTR-022 example, two-line accessor list) | RH5531-006, SHARED-ATTR-022 | `Verify` + `GetCodeFixActionsAsync` | red (fix offered) |

Every red test fails on `Assert.IsEmpty(actions)`: the diagnostic is reported as the spec states, and the split fix is still offered. The `Verify` step before it passes in each.

### Decision 13 — SHARED-BRACE-002, header that spans several lines

File `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixDecidesWhereTheBodyStartsByTheLastLineOfAHeaderThatSpansSeveralLines` [LF/CRLF] (the spec example as written, both statements, one document) | SHARED-BRACE-002 | `Verify` on converted text | green |
| `FixMovesABodyBehindTheLastLineOfAHeaderThatSpansSeveralLinesBelowTheOpeningBrace` [LF/CRLF] (first statement alone, raw line endings) | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | green |
| `FixKeepsABodyBelowTheLastLineOfAHeaderThatSpansSeveralLinesOnItsLine` [LF/CRLF] (second statement, body indented two columns further than one level, keeps its column) | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | green |
| `FixKeepsAMultiLineDelimitedCommentBehindTheHeaderAndMovesTheBodyBelowTheOpeningBrace` [LF/CRLF] (`if (a) /* x⏎    y */ M(a, b);`, the prose case) | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | green |

These are green on the implementer's code from 06. Decision 13 words the spec after the implementation, so they lock in the reading that was chosen. The first test's other side is the second, and the comment test is the other side of the "line break outside a comment" condition.

## Scenarios (bug fix)

_N/A._ This round reproduces no bug. The red tests are spec-derived tests for the withhold that implementer 09 adds.

## Not covered, and why

- RH5502 and RH5504 have no SHARED-ATTR-022 test. Their specs reference it without an example, and an `assembly:` or `module:` list on an accessor is not a shape anyone writes (07). Every split rule reaches the withhold through the one call site of 05 (`rg -n "ApplySplitCodeFixAsync" src/Reihitsu.Analyzer.CodeFixes`, in `TargetAttributeListShapeCodeFixProviderBase`). The five rules with examples are tested.
- There is no CRLF variant of the withheld tests. When a fix is withheld, it inserts no line break. The offered side has LF and CRLF.
- There is no comment or directive case for SHARED-ATTR-022. SHARED-ATTR-019 already withholds a list with a comment or directive inside, and its tests stay. A comment between the `{` and the list does not change the line comparison.
- RH5011-004 has no new test. 07 states that the two existing line-ending tests already assert the reworded text.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~AttributeListsMustFollowShapeRules|FullyQualifiedName~RH5405BracesMustNotBeOmitted"
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~VerifyDiagnosticWithoutCodeFixFor"
scripts/test.sh --project architecture --configuration Debug
```

Measured: the first command gives `Failed: 7, Passed: 351, Total: 358`, and the 7 failures are the red SHARED-ATTR-022 tests above. The architecture run gives `Passed: 117`. `EveryRequirementOfAnEnforcedSpecIsCovered` is green now that SHARED-ATTR-022 is tagged. `scripts/format.sh` changed none of the 14 files.
