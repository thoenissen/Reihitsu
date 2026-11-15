Role: test implementer
Commit: (this commit; base 6f846be)
Verdict: reproduced

The prompt carried no intended fix or hoped-for verdict. The issue's "Cause" and "Required output" were not used: expected values come from FMT-USING-009 (as amended), FMT-USING-010, FMT-USING-011, FMT-BLANK-010, FMT-CLEAN-001 and SHARED-BLANK-001. The issue's "Required output" (no blank line above `// About Linq`) is not asserted, per lead decision 5.

Fixtures write a space as `·` and a tab as `→`; each test class's private `ReplacePlaceholders` turns them into whitespace, so the whitespace-only lines are visible in the source and do not depend on editors keeping trailing whitespace.

## Tests

Formatter: `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveHeaderBlankLineTests.cs` (new). All use `AssertRuleResult`, which runs LF and CRLF and checks the second pass and phase stability.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineOfSpacesEndsTheHeader` (m01, spec example; reproduction) | FMT-USING-009, FMT-USING-010 | `AssertRuleResult` | fails |
| `LineOfTabEndsTheHeader` (m02) | FMT-USING-009 | `AssertRuleResult` | fails |
| `SeveralLinesOfSpacesEndTheHeader` (m03) | FMT-USING-009 | `AssertRuleResult` | fails |
| `LineOfSpacesBelowEmptyLineEndsTheHeader` (m04) | FMT-USING-009 | `AssertRuleResult` | passes |
| `EmptyLineBelowLineOfSpacesEndsTheHeader` (m05, holding side of FC-003) | FMT-USING-009 | `AssertRuleResult` | passes |
| `LineOfSpacesBetweenHeaderAndCommentKeepsEarlierHeaderCommentsAtTheTop` (m07) | FMT-USING-009 | `AssertRuleResult` | fails |
| `EmptyLineAboveCommentKeepsHeaderWithLineOfSpacesAtTheTop` (m06) | FMT-USING-009 | `AssertRuleResult` | passes |
| `LineOfSpacesDirectlyAboveDirectiveKeepsTheHeaderAtTheTop` (m08) | FMT-USING-009 | `AssertRuleResult` | passes |
| `LineOfSpacesEndsTheHeaderInsideNamespaceBody` (m09) | FMT-USING-009, FMT-USING-011 | `AssertRuleResult` | fails |
| `LineOfSpacesBelowDelimitedCommentEndsTheHeader` (m12) | FMT-USING-009 | `AssertRuleResult` | fails |
| `EmptyLineBelowDocumentationCommentEndsTheHeader` (m10, `DocumentationMode.Parse`) | FMT-USING-009, FMT-BLANK-010 | `AssertRuleResult` with parse options | fails |
| `LineOfSpacesBelowDocumentationCommentEndsTheHeader` (m11, `DocumentationMode.Parse`) | FMT-USING-009 | `AssertRuleResult` with parse options | fails |

RH7203 code fix: added to `src/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs` (existing `BatchCodeFixTestsBase` class; `using System;` and `using Reihitsu.Analyzer.Test.Verifiers;` added). Each runs for `"\n"` and `"\r\n"`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CodeFixTakesLineOfSpacesAsEndOfHeader` (m01; reproduction) | FMT-USING-009, FMT-USING-010, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fails (LF, CRLF) |
| `CodeFixTakesLineOfTabAsEndOfHeader` (m02; the tab survives only here) | FMT-USING-009, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fails (LF, CRLF) |
| `CodeFixKeepsLineOfSpacesBelowEmptyLineInTheHeader` (m04) | FMT-USING-009, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fails (LF, CRLF) |
| `CodeFixTakesEmptyLineBelowLineOfSpacesAsEndOfHeader` (m05, holding side) | FMT-USING-009, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | passes |
| `CodeFixKeepsHeaderCommentAboveLineOfSpacesAtTheTop` (m07) | FMT-USING-009, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fails (LF, CRLF) |
| `CodeFixKeepsHeaderEndingInLineOfSpacesAtTheTop` (m08, holding) | FMT-USING-009, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | passes |
| `CodeFixTakesEmptyLineBelowDocumentationCommentAsEndOfHeader` (m10, `DocumentationMode.Parse`) | FMT-USING-009, FMT-USING-011 | `Verify` + `ApplyDocumentationModeParseToTestProject` | fails (LF, CRLF) |
| `CodeFixTakesEmptyLineBelowUnparsedDocumentationCommentAsEndOfHeader` (m10, `DocumentationMode.None`, FC-040 other mode) | FMT-USING-009, FMT-USING-011 | `Verify` + `ApplyDocumentationModeNoneToTestProject` | passes |

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| m01, issue's input | LF, CRLF | RH7203 fix | reproduced | observed `// Header`/`····`/`// About Linq`/`using System;`/`using System.Linq;`; expected `// Header`/`····`/`using System;`/(empty)/`// About Linq`/`using System.Linq;` |
| m01 | LF (assertion stops at first ending; CRLF seen via the fix rows and the skeptic's run) | formatter, whole document | reproduced | observed `// Header`/(empty)/`// About Linq`/`using System;`/…; expected `// Header`/(empty)/`using System;`/(empty)/`// About Linq`/`using System.Linq;` |
| m02 tab | LF, CRLF | fix; formatter | reproduced | as m01, tab kept in the fix |
| m03 | LF | formatter | reproduced | as m01 |
| m04 | LF, CRLF | fix | reproduced | observed: the line of spaces is dropped from the header; expected kept as written |
| m04 | LF, CRLF | formatter | passes | — |
| m05, m06, m08 | LF, CRLF | formatter; fix (m05, m08) | passes | — |
| m07 | LF, CRLF | fix; formatter | reproduced | observed: `// B` moves below `using System;`; expected it stays in the header |
| m09 namespace body | LF | formatter | reproduced | as m01 at namespace indentation |
| m10 / m11 `///`, Parse | LF, CRLF | fix (m10); formatter (m10, m11) | reproduced | observed `// About Linq` stays at the top; expected it moves with `using System.Linq;` |
| m10 `///`, None | LF, CRLF | fix | passes | — |
| m12 `/* Header */` | LF | formatter | reproduced | as m01 |

## Not covered, and why

- m09 and m12 on the fix path: same single call site (`UsingDirectiveOrderingRewriter.OrganizeUsingDirectives` → `UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia` → `GetHeaderSplitIndex`, the only caller per the skeptic's `rg`) as the formatter tests; the fix path for a namespace body additionally depends on the whitespace of the blank line FMT-USING-010 inserts there, which no example states, so no assertion was made on it.
- m11 and m03 on the fix path: same call site and the same trivia shape class as m01/m10; formatter covers them.
- Directive or disabled text in the gap (comment/directive case of the token-gap checklist): `UsingDirectiveOrderingSafety.CanSafelyReorder` refuses any block holding a directive, so the split is never reached; existing `SHARED-USING-009` tests cover the refusal.
- Fix All: a block has one header and one original first directive; several diagnostics in one block are organized by one fix (existing `GetFixAllScenario`). No new co-occurrence.
- FC-017/FC-033: every formatter test asserts the exact first-pass output, not only convergence (the defect is a stable fixed point).
- `UsingLeadingTriviaBuilderTests` (unit): the method is private; the observable behavior is covered through both public surfaces. No unit test added.
- Naming fixes, unsupported shapes (FC-038–043): not involved, no shape declared.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives.UsingDirectiveHeaderBlankLineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests"
```

On 6f846be: formatter 8 failed / 4 passed; RH7203 class 10 failed (5 tests × 2 endings) / 92 passed. `scripts/test.sh --project architecture` passes (118/118) with the new requirement tags.
