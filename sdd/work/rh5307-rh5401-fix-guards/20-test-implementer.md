Role: test implementer
Commit: 149fb69
Verdict: tests written

My prompt named the worklist: bring the RH5401 tests in line with RH5401-003/-004 as amended in 926712f (decisions 12, 13; 19-spec-author.md with its Follow-up). It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the RH5401-003 and RH5401-004 examples and the normative sentences of RH5401-004, not from the code or the auditor's suggestion.

All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`. Every changed or new test runs as `\n` and `\r\n` (DataRow).

Run on 149fb69 (code unchanged since 15-implementer): 106 tests, **12 fail, 94 pass**. The 12 failures are six tests on both line endings, each failing with `Assert.IsEmpty(actions)` (one action registered): the fix is offered where RH5401-004 now withholds it. In each, the diagnostic itself is reported as expected (`Verify` inside `VerifyNoCodeFixIsOffered` passes first). No failure is a compile error or unrelated layout.

Why each fails on the current code: the current gate compares blank-line counts (per gap behind each list, and the total over the attribute region). It does not look at what stands between the brackets, so:
- `Q` (blank line inside an unsplit list) keeps every count, and is offered.
- `W` and the collapse variant (`b01`) offset a dropped or collapsed blank line in one list against one inserted in front of an own-line comment in another (auditor 18, finding 4).
- An own-line comment behind `[`, and an own-line block comment followed by code inside an argument list, change no count, and are offered.

`T` and an own-line comment in front of `]` are already withheld today (the formatter's insertion in front of the comment changes the total), so their tests pass.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixKeepsTheBlankLinesAndCommentsOfAttributeListsThatTheFormatterKeeps` (renamed from `VerifyFixKeepsABlankLineBehindAnAttributeListThatTheFormatterKeeps`; `Q` removed; `J`, `U`, `V`, `Y` added) — the full RH5401-003 example `E`, `F`, `J`, `U`, `V`, `Y` | RH5401-003, RH5401-004; FC-003 | `Verify` | pass |
| `VerifyFixIsWithheldForABlankLineInsideAnAttributeListTheFormatterDoesNotSplit` (was `VerifyFixKeepsABlankLineInsideAnAttributeListTheFormatterDoesNotSplit`, asserted `Q` offered; now withheld) — `Q` | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForABlankLineOrACommentOnItsOwnLineBetweenTheBracketsOfAnAttributeList` (was `VerifyFixIsWithheldWhenFormattingWouldDropABlankLineInsideAnAttributeListItSplits` with `N`, `O`, `P`, `R`, `S`) — the full RH5401-004 example `N`, `O`, `P`, `Q`, `R`, `T`, `W` | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (offered for `Q`, `W`) |
| `VerifyFixIsWithheldForACommentOnItsOwnLineInsideTheArgumentsOfAnAttribute` (new) — `T` | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixIsWithheldWhenABlankLineDroppedInOneAttributeListOffsetsOneInsertedInFrontOfACommentInAnother` (new) — `W` (auditor `a09`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldWhenABlankLineCollapsedInOneAttributeListOffsetsOneInsertedInFrontOfACommentInAnother` (new) — auditor `b01` with real attributes: `[Obsolete("a",⏎⏎⏎ false)]⏎[DefaultValue(typeof(int),⏎ // why⏎ "1")]`, no list split | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForABlockCommentThatStartsItsOwnLineInFrontOfCodeBetweenTheBracketsOfAnAttributeList` (new) — `[Obsolete(⏎ /* c */ "a")]`: nothing but whitespace in front of the comment, code behind it | RH5401-004; FC-003 (boundary of "starts its own line") | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForACommentOnItsOwnLineBehindTheOpeningBracketOfAnAttributeList` (new) — `[⏎ // why⏎ Obsolete]` ("behind the `[`") | RH5401-004 | `VerifyNoCodeFixIsOffered` | **fail** (fix offered) |
| `VerifyFixIsWithheldForACommentOnItsOwnLineInFrontOfTheClosingBracketOfAnAttributeList` (new) — `[Obsolete("a",⏎ false)⏎ // why⏎ ]` ("in front of the `]`") | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixKeepsABlockCommentBehindCodeBetweenTheBracketsOfAnAttributeListAboveTheProperty` (new) — `V` alone, exact output | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixKeepsALineCommentBehindCodeInsideTheArgumentsOfAnAttribute` (new) — `Y` alone, exact output | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixIsWithheldWhenTheFormatterDropsABlankLineInsideASplitListAndInsertsOneInFrontOfAComment` (`S`) — **removed** | — | — | — |

Both sides of the new condition (FC-003):
- Own-line comment between the brackets (withheld): `T`, the `/* c */ "a"` boundary case, behind `[`, and in front of `]`.
- Comment between the brackets with code in front of it on its line (offered): `U` (existing `VerifyFixKeepsAnAttributeListWithACommentBetweenItsBracketsOnThePropertysLine`, unchanged), `V`, `Y`.
- Blank line between the brackets (withheld): `N`, `O`, `P`, `R` (existing tests, unchanged), and `Q`, now in an unsplit list too.
- No blank line or own-line comment between the brackets: offered, also when the formatter splits the list (`L`, existing).
- Trivia outside the brackets (offered): `J` (and the existing `VerifyFixKeepsALineCommentBehindAnAttributeList`), `E`, `F`.

Existing tests whose asserted output the amendment changes, found with `rg` for each changed member in `src/*Test*/` (`Obsolete("a",` followed by a blank line; `Obsolete /* c */`; `Obsolete("a" /* c */)`; `S`):
- The `Q` offered test and the `Q` member of the combined RH5401-003 test contradicted the spec. Changed as above.
- `S` was removed. It existed only to show the per-gap check next to the total count, which decision 12 removed. Its input holds `N`, so the new condition withholds it, and `N` is tested on its own. The per-gap condition is still covered by `D`, `G`, `H`, `K`, `M` (withheld) and `E`, `F` (offered).
- `VerifyFixKeepsAnAttributeListWithACommentBetweenItsBracketsOnThePropertysLine` (`U`) is left unchanged. It matches the amended spec (decision 13); under decision 12 alone it would have contradicted it.
- `VerifyFixIsWithheldForAnAttributeListTheFormatterRefuses` (`[Obsolete("a" // c⏎ , false)]`) is left unchanged. Its comment has code in front of it, so the new condition does not apply, and SHARED-FIX-010 withholds it (FMT-UNSUP-009).
- `RH5401…FormatterTests` and `RH5408…AnalyzerTests` assert formatter output or another rule. Left as they are.

## Not covered, and why

- **Directives or disabled text between the brackets.** RH5401-004 says they do not withhold the fix *by this condition*. It gives no example and no output for them, and other conditions (per gap, one-line check) may still withhold. An "offered" test would assert more than the spec guarantees, and an exact-output test would pin an unspecified layout (FMT-BREAK-027 leaves it open).
- **`K5`** (`[Obsolete("a",⏎ false) // c⏎ ]`, the `]` moved one column right). The spec author's observation is that it is offered and that the `]` placement belongs to the formatter and is unspecified (lead decision 10). A test would pin a disputed output.
- **Fix All over the new class.** The Fix All path runs the same per-diagnostic registration gate as the single fix. `VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` already covers a withheld member next to offered ones in one document (unchanged reason from 14).
- Unchanged from 05, 08, 14: FC-018, FC-036, FC-010 to FC-012, FC-038 to FC-043, FC-029, FC-040.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
scripts/test.sh --project architecture
```

Architecture: 198 passed. `scripts/format.sh` changed nothing in the test file.

## Follow-up candidates

_None._
