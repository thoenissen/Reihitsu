Role: test implementer
Commit: dd48da0
Verdict: tests written

Characterization tests of today's output, as decision 8 in `00-lead.md` sets them. No production code is touched. Every new test passes on `dd48da0`: 9 passed, 0 failed. Every test uses `FormatterTestsBase.AssertRuleResult`, which checks LF and CRLF, a second pass, and per-phase stability (FC-017). Tabs are written as `\t` escapes in ordinary string literals, so no editor or test-source format can normalize them away (FC-033). The inputs are the issue's own shapes: a tab in front of an inactive `#region`, and an inactive region inside a member (FC-034).

Mutation check: I temporarily changed four expected values: the region column 2 → 4 in r1, the `#pragma` column 4 → 0 in d2, the region column 8 → 4 in the two-tab variant, and the `d` block under `async` → under `static` in the FMT-INDENT-028 example. Each of the four tests failed with `Formatter output mismatch under LF line endings`. Then I restored the files.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## #1400 — formatter-spec clarifications from round 9

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `InactiveDirectiveColumnTests.SpecExampleInactiveRegionBetweenMembersKeepsItsColumns` (r1) | FMT-INDENT-010 | `AssertRuleResult` | pass |
| `InactiveDirectiveColumnTests.SpecExampleTabsInFrontOfInactiveRegionAreReplacedAndTabInDisabledTextIsKept` (r2) | FMT-INDENT-010 | `AssertRuleResult` | pass |
| `InactiveDirectiveColumnTests.TabsInFrontOfInactiveRegionGiveAColumnOffTheMemberColumnThatIsKept` (r2 with two tabs, column 8 ≠ member column 4) | FMT-INDENT-011 | `AssertRuleResult` | pass |
| `InactiveDirectiveColumnTests.SpecExampleInactiveRegionInsideMethodBodyKeepsItsColumns` (r3) | FMT-INDENT-010 | `AssertRuleResult` | pass |
| `InactiveDirectiveColumnTests.InactiveRegionInNestedTypeStaysInColumnZero` (r5, one nesting level deeper, FC-033) | FMT-INDENT-010 | `AssertRuleResult` | pass |
| `InactiveDirectiveColumnTests.SpecExampleInactiveNonRegionDirectivesKeepTheirColumns` (d1) | FMT-INDENT-011 | `AssertRuleResult` (input unchanged) | pass |
| `InactiveDirectiveColumnTests.SpecExampleTabsInFrontOfAndInsideInactivePragmaAreReplacedAndItsColumnIsKept` (d2) | FMT-INDENT-011 | `AssertRuleResult` | pass |
| `LambdaModifierAnchorTests.SpecExampleModifierAndReturnTypeAreNeverTheAnchor` (FMT-INDENT-028 second example: `int (x)`, `static int (x)`, `static x`, `static async x`, `static ()`) | FMT-INDENT-028 | `AssertRuleResult` | pass |
| `LambdaModifierAnchorTests.AsyncInFrontOfModifierOrReturnTypeAndArgumentPositionKeepTheAnchor` (skeptic's l1: `async static x`, `async Task<int> (x)`, `N(static int (x) => {…})`) | FMT-INDENT-028 | `AssertRuleResult` | pass |

Files:
- `tests/Reihitsu.Formatter.Test/Regression/Indentation/InactiveDirectiveColumnTests.cs`
- `tests/Reihitsu.Formatter.Test/Regression/Indentation/LambdaModifierAnchorTests.cs`

### Existing tests checked

This run changes no output, so no existing test asserts a changed value. Searches for `FMT-INDENT-010`, `FMT-INDENT-011`, `FMT-INDENT-028`, `#if NEVER`, `static async x`, `static () =>`, and `static int (x)` in the test projects turned up none that pins the cases above:
- `DisabledRegionLayoutTests` and `IndentationRewriterWholeFileTests.PlacesDirectivesRegionsDisabledTextAndCommentsWhileIndentingTheCodeAroundThem` put the inactive directives in the column that re-indentation would give.
- `DirectiveBehindDisabledTextTests` covers active directives.
- `LambdaReturnTypeJoinTests` is tagged FMT-BREAK-045.
- `LambdaReturnTypeSpacingTests` uses an expression body.

### Not covered, and why

- **Region text in inactive branches and SHARED-BLANK-007 (no padding).** Already pinned by `DisabledRegionLayoutTests.RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding`. The new tests show it as well, since none of them gains a blank line.
- **Item 24 (FMT-SPACE-001).** Already pinned by `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperatorBecomesOneSpaceAsBehindSingleLineComment`.
- **r4 (`#region   foo   ` / `#   endregion   ` in an inactive branch).** The FMT-REGION-007 tests pin the text. Decision 8 does not include r4. Its column falls under the same no-layout fallback as r1 and r3, which those tests pin.
- **r8 (inactive regions inside an argument list and an array initializer).** Left out. Its output also moves the initializer and removes a trailing comma, which other rules decide. A test would pin unrelated layout, and decision 8 does not include it.
- **`[Obsolete] (x) => {…}`.** Its layout is decided by FMT-BREAK-027 / RH5513 (the attribute list on its own line), not by the anchor alone. The spec states it only in prose. The skeptic confirmed the anchor under `(` with fixture l1. Left out to keep the test from pinning the attribute-line decision.
- **Failure classes.** The ones that apply:
  - FC-017: LF and CRLF, second pass, and phase stability through `AssertRuleResult`.
  - FC-020: the subject itself.
  - FC-033: tabs as escapes, plus the nesting sibling r5.
  - FC-034: the issue's exact shapes.
  - FC-035: the expected values come from the spec examples and the skeptic's fixtures, not from code.

  The rest cannot reach the change, because no production code changes. Code-fix classes, Fix All, and the Renamer do not apply: no code fix is involved. The unsupported-shape classes (FC-038 to FC-043) do not apply either: no shape is declared.
- **Code-fix and analyzer surface.** Not involved (skeptic *Surfaces*). The skeptic already ran the analyzer counterparts RH7301, RH7302, and RH7304.

## #1936 — FMT-INDENT-021 Why text

### Tests

_None._ The change is text only (the `Why:` of FMT-INDENT-021). The requirement is pinned by `ListAndHeaderAlignmentTests.WrappedWhenClauseIsIndentedOneLevelDeeperThanCase` and `CaseWhenClauseContributorTests.AlignsWrappedWhenKeywordToCasePlusIndent`. Decision 8 adds no test for #1936.

### Not covered, and why

The skeptic's fixture w2 (a guard that wraps) is pinned by the two tests above, as the skeptic and the spec author state. No output changes.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Indentation.InactiveDirectiveColumnTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Indentation.LambdaModifierAnchorTests"
```

## Follow-up candidates

_None._
