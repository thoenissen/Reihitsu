Role: test implementer
Commit: c492781
Verdict: tests written

My prompt named the worklist (decision 15; RH5401-003/-004 as amended in 4b24821, 26-spec-author.md) and carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the RH5401-003/-004 examples and the normative sentences of RH5401-004, not from the code.

All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`. Every changed or new test runs as `\n` and `\r\n` (DataRow).

Run on c492781 (production code unchanged since 21-implementer): 124 tests, **18 fail, 106 pass**. The 18 failures are nine tests on both line endings, each failing with `Assert.IsEmpty(actions)` ("No code fix may be offered for the diagnostic at …"): the fix is offered where RH5401-004 now withholds it. In each, the diagnostic is reported as expected (`Verify` inside `VerifyNoCodeFixIsOffered` passes first). No failure is a compile error or unrelated layout.

Why each fails on the current code: the current gate refuses a list for a blank line or an own-line comment between its brackets, and compares blank-line counts behind each list; it does not look at directives or disabled text between the brackets. A directive or disabled text between the brackets adds no blank line and no comment, so every member below passes the gate and is offered.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixIsWithheldForABlankLineADirectiveDisabledTextOrACommentOnItsOwnLineBetweenTheBracketsOfAnAttributeList` (renamed from `…ForABlankLineOrACommentOnItsOwnLine…`; members `I`, `X`, `Z` added) — the full RH5401-004 example `N`, `O`, `P`, `Q`, `R`, `T`, `W`, `I`, `X`, `Z` | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (offered for `I`, first hit) |
| `VerifyFixIsWithheldForAConditionalDirectiveWithDisabledTextInsideTheArgumentsOfAnAttribute` (new) — `I` | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForARegionBetweenTheBracketsOfAnAttributeList` (new) — `X` (auditor `d01`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForOneDirectiveBetweenTheBracketsOfAnAttributeListWhoseCounterpartStandsInFrontOfIt` (new) — `Z` (auditor `e01`) | RH5401-004; FC-003 (boundary: one directive inside, one outside) | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForAPragmaInsideTheArgumentsOfAnAttribute` (new) — auditor `f03`, "whatever kind the directive is" | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForANullableDirectiveInsideTheArgumentsOfAnAttribute` (new) — auditor `d06` | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForDirectivesBehindTheOpeningAndInFrontOfTheClosingBracketOfAnAttributeList` (new) — auditor `e04`, "behind the `[`, in front of the `]`" | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForADirectiveInFrontOfTheClosingBracketOfAnAttributeListOnThePropertysLine` (new) — `[Obsolete⏎#pragma …⏎    ] int G` (auditor `g01`, second member; list on the property's line) | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForDisabledTextOfAnInactiveBranchBetweenTheBracketsOfAnAttributeList` (new) — `#if NOT_DEFINED` with the disabled attribute (auditor `c02`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixKeepsADirectiveInFrontOfTheOpeningBracketOfAnAttributeList` (new) — `E2`, exact output | RH5401-003, RH5401-004; FC-003 (outside side) | `VerifyCodeFixWithEndOfLine` | pass |

Both sides of the changed condition (FC-003):
- Directive or disabled text between the brackets (withheld): `I`, `X`, `Z`, pragma, nullable, behind `[` / in front of `]`, on the property's line, inactive branch.
- Directive outside the brackets (offered): `E2` (in front of `[`, new), and the existing `VerifyFixKeepsAConditionalDirectiveBetweenTwoAttributeLists` (behind `]`, in front of the next `[`) and `VerifyFixKeepsAConditionalCommentBetweenAnAttributeListAndTheProperty` (behind `]`, in front of the property). `Z` sits on the boundary: its `#region` is outside, its `#endregion` inside.

Existing tests checked against the amended spec, found with `rg -n "#if|#region|#pragma|#endregion|#else|#nullable"` in the test projects for RH5401:
- `VerifyFixKeepsAConditionalDirectiveBetweenTwoAttributeLists` and `VerifyFixKeepsAConditionalCommentBetweenAnAttributeListAndTheProperty`: directives behind a `]`, not between brackets. Consistent with the amended RH5401-004 ("a directive there counts only where it lies in a gap behind an attribute list"); the per-gap counts are unchanged in both. Left unchanged.
- `VerifyFixIsWithheldForACommentOrDirectiveInFrontOfTheArrow` (`#pragma` before `=>`): withheld by FMT-UNSUP-013, unaffected. Left unchanged.
- No existing test asserted a fix offered with a directive or disabled text between the brackets, so no existing test contradicts the amended spec. The combined RH5401-004 example test was extended, not reversed.
- `RH5401…FormatterTests` and `RH5408…AnalyzerTests` assert formatter output or another rule. Left as they are.

## Not covered, and why

- **`#warning` and `#line` between the brackets** (auditor `f01`, `f02`). `#warning` emits compiler warning CS1030 and `#line` remaps diagnostic locations, both of which the verifier would have to accommodate, which tests the test setup rather than the rule. "Whatever kind the directive is" is covered by four kinds (`#if`/`#else`, `#region`, `#pragma`, `#nullable`).
- **Auditor `c01`, `c10`, `d03`, `d04`, `d07`, `e02`, `e03`, `g02`.** Same kinds in other positions already covered (conditional around an attribute, `#pragma` in front of `]`, blank line in disabled text, comment in an active branch, `]` inside branches, indented region, region with nothing between, region on the property's line). One test per position and kind suffices; the refusal is a single "between the brackets" condition in RH5401-004 with no per-kind or per-position branch in its wording.
- **Fix All over the new class.** Unchanged reason from 20: the Fix All path runs the same per-diagnostic registration gate, and `VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` covers a withheld member next to offered ones.
- Unchanged from 05, 08, 14, 20: FC-018, FC-036, FC-010 to FC-012, FC-038 to FC-043, FC-029, FC-040.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
scripts/test.sh --project architecture
```

Architecture: 198 passed. `scripts/format.sh` changed nothing in the test file.

## Follow-up candidates

_None._
