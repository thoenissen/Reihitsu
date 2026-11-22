Role: test implementer
Commit: <filled by commit below>
Verdict: tests written

Repair round 1, thoenissen/Reihitsu#1929, audit finding #1 (`11-auditor.md`), answer 9A (`00-lead.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values come from FMT-INDENT-026's last example (`sdd/specs/formatter/indentation.md`, identical to the pair in `12-spec-author.md`, compared mechanically) and from the member table in `12-spec-author.md`. A single-member test that stands alone (`return a is [...]` instead of `&& a is [...]`) uses the spec example's layout moved by the different start column.

## Tests
All formatter tests use `AssertRuleResult` (LF and CRLF, second-pass idempotency, phase stability). File: `tests/Reihitsu.Formatter.Test/Regression/Indentation/PatternAnchorBehindWrapperTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleTypedAndParenthesizedPatternsBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` (the whole spec example; input and output in constants because RH2008 limits a method to 120 lines) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindOrInASwitchArmStandUnderTheArmsFirstToken` (`P {…} or Q {…}`, arm) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindNotInASwitchArmStandUnderTheNot` (`not Q {…}`, arm) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindOrAfterNullInASwitchArmStandUnderTheNull` (`null or P {…}`, arm) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindOrInASwitchArmStandsUnderTheArmsFirstToken` (`0 or (1 or 2)`, arm) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindNotInAListPatternElementStandUnderTheNot` (`[1, not Q {…}]`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindNotInASliceStandUnderTheNot` (`[1, .. not Q {…}]`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindNotInAListPatternElementStandsUnderTheNot` (`[1, not (1 or 2)]`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindNotInASliceStandsUnderTheNot` (`[1, .. not (1 or 2)]`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindNotInACaseLabelStandUnderTheNot` (`case not Q {`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `TypedBracesBehindOrAfterNullInACaseLabelStandUnderTheNull` (`case null or P {`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindNotInACaseLabelStandsUnderTheNot` (`case not (`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindAndInACaseLabelStandsUnderTheLabelPatternsFirstToken` (`case > 5 and (`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `ParenthesizedPatternBehindOrInACaseLabelStandsUnderTheLabelPatternsFirstToken` (`case 3 or (`) | FMT-INDENT-026 | `AssertRuleResult` | pass |
| `RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests.VerifyFixPutsBracesOfTypedPropertyPatternBehindNotUnderTheArmsFirstToken` (LF and CRLF data rows; the typed arm `not Q {` puts its braces under `not`, column 19; the left operand `P {…}` keeps its own first token) | RH5206-003, FMT-INDENT-026 | `VerifyCodeFixWithEndOfLine` (one-pass convergence, re-analysis) | pass |

Failing for the right reason: in a temporary worktree at head `7f41ed4` with only `PatternAnchor.cs:49` reverted to `pattern.GetFirstToken()` (the `main` fallback), all 14 new formatter tests fail, as do both data rows of the new RH5206 test. `PropertyPatternsWithoutAWrapperKeepTheirAnchors` (the other side) keeps passing. Against `main` (`75cc39f`) they fail too, but that run also lacks the run's other layout changes, so I used the single-line revert as the evidence. The worktree is removed.

## Scenarios (bug fix)
_Not a bug-fix stage._

## Not covered, and why
- Members that keep their columns (left operand of `and`/`or`, no wrapper, `is`-introduced, subpattern, pattern inside a parenthesized pattern): pinned by `PropertyPatternsWithoutAWrapperKeepTheirAnchors` and the first `P {…}` in the new spec-example test and RH5206 test. No new test.
- Untyped braces behind a wrapper and `not (` in an arm: pinned by the existing tests from 06/08 in the same class.
- Node-level parity: `ReihitsuFormatterNodeColumnTests` (target `not {…}`) already pins it. No code fix formats a recursive or parenthesized pattern on its own (`11-auditor.md`), so no further node-level test.
- Existing tests that assert a changed output: I searched the test projects for typed property patterns and parenthesized patterns behind `and`/`or`/`not`. Both full projects pass on head, so no existing test asserts the `main` columns. None changed.
- Comment/directive in the gap: this change moves no token or line join. It only changes the anchor column. FMT-UNSUP-011/017 already refuse comments behind a combinator or `not`.
- Fix All: no new fix action. `GetFixAllScenario` is unchanged.
- FC-001–FC-009, FC-011, FC-013, FC-018–FC-026, FC-029, FC-030, FC-036–FC-044: no new guard, join, rename, fix action, or unsupported shape. FC-010/FC-012 (idempotency) and FC-017 (assertion strength) are covered by `AssertRuleResult`. FC-016/FC-027 (all instances and nesting levels) are the member table, covered above.

## Spec finding
The spec author's RH5206 code-fix example (`12-spec-author.md`, and the change doc's *Code fixes* section if it quotes it) does not compile: `not Q { A: 3, A: 4 }` can never fail to match, so the following `_ => 0` arm is unreachable (CS8510). The test gives `Q` a second property `B` and uses `B: 4`. This changes no column of the asserted output. The lead should have the spec author correct the example wherever it is quoted.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PatternAnchorBehindWrapperTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5206"
```
Full projects on head: formatter 7188/7188, analyzer 11939/11939 passed.

## Follow-up candidates
_None._
