Role: skeptic
Commit: e96b352
Verdict: proceed (two output-question candidates for the lead: Q1 likely covered by answer A, Q2 the rule page)

Inputs: thoenissen/Reihitsu#1925 and its one comment (maintainer: option A), `00-lead.md`, `01-spec-author.md`, the spec diff of `sdd/specs/analyzer/RH5202.md` and the change doc `sdd/changes/2026-11/rh5202-tab-aligned-raw-strings.md` (both at e96b352), FMT-RAW-006 and FMT-RAW-010, RH5601-002, the earlier change doc `sdd/changes/2026-11/raw-string-tab-indentation.md` (*Decisions*), the RH5202 analyzer, the code fix's shift entry, `RawStringLiteralUtilities`, `RawStringLineShifts.MoveLines`, and the four RH5202 test classes. The prompt carried no intended fix, cause, or hoped-for verdict.

Fixture directory for every run below: `$S=/tmp/claude-0/-home-user-Reihitsu/8454a2c2-597e-59f6-a041-7ba5648efd58/scratchpad/fx` (outside the repository, scratch only). `→` is one tab.

## Premise

Real, and reached by the RH5202 analyzer and its code fix on any document (IDE, build) that holds such a literal, and by Fix All.

- Code: `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.CheckRawStringAlignment` reports only when `RawStringLiteralUtilities.GetClosingLineShift(...) != 0`; that helper counts every whitespace character as one column, so a literal aligned in characters behind tabs returns 0 and is not reported. The fix also returns early on `delta == 0` (`RH5202...CodeFixProvider.cs:160`).
- Run (branch at e96b352, no production change): `scripts/apply-fix.sh RH5202 $S/fix` over five fixtures, LF and CRLF — every one `no-diagnostic`:
  - `a_issue.cs` — the issue's input (12 spaces + `→→→→` in front of `x` and of the closing quotes, opening quotes in column 16);
  - `b_smarttab.cs` — tab-indented file, `→→var a = """` then `→→        x` / `→→        """;` (two tabs + eight spaces = column 10);
  - `c_verbatimhole.cs` — `var b = $@"start {"""` with the nested raw literal's lines at 24 spaces + `→→` (column 26);
  - `d_regularhole.cs` — same in a regular `$"start {"""` (23 spaces + `→→`, column 25);
  - `e_wsonly.cs` — the issue's input plus a whitespace-only content line `            →→` (14 characters, shorter than the width 16).
- The formatter rewrites all five (`scripts/format.sh $S/fmt`, run twice, second pass identical): every tab in front of the closing quotes and within the width on content lines becomes one space, no line moves; `e_wsonly.cs` gets a whitespace-only line of 14 spaces. So the analyzer is today silent on a literal the formatter changes inside RH5202's own construct — a P11 gap of the rule, as the issue says, not FC-013.

## Mechanism and candidates

_N/A_ (behavior change). Dispatch: the analyzer registers `StringLiteralExpression`, `Utf8StringLiteralExpression` (token kinds `MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`) and `InterpolatedStringExpression` (start token `InterpolatedMultiLineRawStringStartToken`, any number of `$`). Single-line raw tokens are filtered by kind. Nested literals are visited by the same registrations, so a nested literal is judged on its own. The candidate set is: plain, `u8`, interpolated (`$`, `$$`), each top-level or nested in a hole of a raw interpolated literal (any depth), or of a verbatim or regular interpolated string.

## Breaking inputs

Failure classes that reach the change: FC-003 (both sides of "tab in front of the closing quotes" and of "in the column"), FC-007 (one fix application silences it), FC-008 (Fix All, nested), FC-009 (no fix is offered that changes nothing), FC-013 (checked: no formatter output keeps a tab in front of a raw string's closing quotes in any of the five fixture shapes above, so the new report cannot flag formatter output), FC-015 (the condition "closing whitespace holds a tab" is already stated once, `RawStringLiteralUtilities.HasTabInLeadingWhitespace`, and used by the fix and through `GetStrippedIndentationSpacesWidth` by the formatter), FC-022, FC-027 (nesting, enclosing string kinds), FC-033 (line endings).

Matrix of the new condition. Axes: literal kind {plain, `u8`, `$`, `$$`} × position {top level, nested in a raw hole, nested two deep, in a verbatim/regular interpolated hole} × closing whitespace {tabs only, spaces then tabs, tabs then spaces (smart tabs), tab plus no-break space, no tab} × content {nothing behind the width, tab behind the width, whitespace-only line shorter than the width}. Every member with a tab in the closing whitespace and the closing quotes in the opening column is reported under RH5202-009; every member without a tab is not. No member has a fix that changes nothing: a reported member always has at least one tab to write as a space, unless RH5202-008 withholds the fix of an outer literal.

Candidate inputs, with the test that already pins them (`README.md`, *Coverage first*):

| # | Input | Expected | Existing test |
|---|---|---|---|
| 1 | Issue input (`a_issue.cs`), LF + CRLF | reported, fix = 16 spaces | `TabIndentationTests.VerifyLiteralAlignedInCharactersBehindTabsIsNotReported` pins the opposite — must flip |
| 2 | Same input through the formatter test base | input now reported | `FormatterTests.VerifyFormatterWritesTabsOfLiteralAlignedByTheAnalyzersMeasureAsSpaces` passes no expected diagnostic, so it asserts the input is clean — must expect one |
| 3 | RH5202-009 example 2 (`$`, tabs behind the width kept) | reported, fix as in the spec | none found |
| 4 | `u8` and `$$` aligned behind tabs | reported | none found |
| 5 | Closing whitespace without a tab, aligned, tab on a content line (RH5202-009 example 3) | not reported | none found for the aligned case (`VerifyFixKeepsTheTabOfTheContentLineWhenTheClosingWhitespaceHoldsNoTab` is a shift case) |
| 6 | Aligned behind no-break spaces only | not reported | `FormatterTests.VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindNoBreakSpaces` |
| 7 | Aligned behind a tab and a no-break space | reported, both become spaces | none found |
| 8 | `e_wsonly.cs` (whitespace-only line shorter than the width) | reported, that line becomes 14 spaces (formatter output above) | none found for the aligned case |
| 9 | Smart tabs (`b_smarttab.cs`) | reported; see Output question 1 | none found |
| 10 | Nested literal aligned behind tabs, outer misaligned (RH5202-008 example 1) | nested reported, outer fix withheld, nested fix then outer fix | `HoleTests.VerifyFixIsWithheldWhenUnreportedRawStringNestedInHoleIsIndentedWithTabs` (line 2342) — nested now reported, markup must change |
| 11 | Nested two deep, nested `$`, nested `u8`, outer shifting left — each nested literal aligned behind tabs (measured: columns 17/17, 12/12, 11/11, 29/29) | nested now reported | `HoleTests` lines 2477, 2504, 2529, 2554 — markup must add the nested diagnostic |
| 12 | Outer literal aligned behind tabs, nested literal without a tab | outer reported; fix writes the outer's own lines only; nested lines untouched | none found |
| 13 | Outer and nested both aligned behind tabs | both reported, outer fix withheld until the nested fix | none found |
| 14 | `c_verbatimhole.cs`, `d_regularhole.cs` | nested raw literal reported, fix = formatter output above | none found |
| 15 | Single-line raw string (`"""x"""`, `$"""{1}"""`) on a line indented with a tab | not reported (the other side of "multi-line": the opening line's own leading whitespace must not count as closing whitespace) | none found |
| 16 | Lone `\r` document with one aligned-behind-tabs literal | reported, fixed in one application | none found (pattern: `VerifyAlignedLiteralIsNotReportedAndMisalignedLiteralIsReportedWithLoneCarriageReturns`) |
| 17 | Fix All over two aligned-behind-tabs literals in one document | both fixed | none found |
| 18 | Aligned behind tabs with a multi-line interpolation hole whose line starts in hole code | hole line untouched by the fix (zero shift); content lines written as spaces | none found |

Not failing members: a literal in a file the precheck refuses (formatter leaves it, RH5202 reports — same as for misaligned literals today, not new).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5202 analyzer | must change (report condition) |
| RH5202 code fix | must change (zero shift no longer ends the fix) |
| Fix All | must stay (batch provider; cover with #17) |
| `Reihitsu.Core` `RawStringLiteralUtilities` | must stay in meaning; `GetClosingLineShift` is also used by the fix's RH5202-006 check, so it must keep returning the shift |
| Formatter (FMT-RAW) | not involved |
| RH5601 | not involved (RH5601-002 already hands this whitespace to RH5202) |
| Resources (message, titles) | must stay (lead decision 1; `AnalyzerPackageMetadataTests` pins message against title) |
| Rule page `documentation/rules/RH5202.md` | Output question 2 |
| Tests | the six existing tests in rows 1, 2, 10, 11 must change; new tests for the rest |

## Refuse option

- **Refuse (leave unreported)** is option B of the issue, which the maintainer rejected. Not available.
- **Declare the shape unsupported** — named as `AGENTS.md` requires. Span: the whitespace between the last line break inside a multi-line raw string literal (any kind, any nesting) and its closing quotes, when it holds a tab. It would delete FMT-RAW-010's writes in `RawStringLineShifts`, `GetStrippedIndentationSpacesWidth`, RH5202-007's spaces, and RH5202-008's withholding. It would refuse every tab-indented file that holds an indented multi-line raw string — code people commonly write — and it contradicts the maintainer's answers 1A–3A of `raw-string-tab-indentation.md` (write the indentation as spaces, also for an aligned literal, also in the fix) and option A of #1925 (report and fix this literal). I judge it decided by those answers; if the lead does not, it is escalation 10 with the recommendation *do not declare*.
- Cost of the requested change: a few production lines in the analyzer and the fix, no new type, no new mechanism (the condition and the spaces writing already exist).
- Simpler option: _None._

## Output questions

**1 · RH5202 · Raw string literals should be formatted correctly — literal written with smart tabs (tabs for indentation, spaces for alignment)**

The lead decides whether answer A already covers this: it is "aligned in characters behind tabs", but it is the one population for which the issue's reason ("does not stand under its opening quotes in an editor that renders a tab wider") is false, and for which the fix's output looks worse in such an editor while RH5601 is turned off. With RH5601 on (the default), both rules' fixes together converge on the formatter's output: `scripts/apply-fix.sh RH5601 $S/t1` leaves the literal's tabs and spaces the rest; then `scripts/apply-fix.sh RH5202 $S/t2` on `main` already shifts the literal 6 columns and writes 16 spaces.

Input:
```csharp
class C
{
→void M()
→{
→→var a = """
→→        x
→→        """;
→}
}
```

Before (`main`): not reported.

After, A (recommended):
```csharp
class C
{
→void M()
→{
→→var a = """
          x
          """;
→}
}
```

After, B: not reported when the whitespace in front of the closing quotes is the opening line's indentation followed only by spaces.

Why A: the repository writes no tabs (RH5601 is on by default and reports the opening line), the fix already writes spaces for every tab-indented literal it shifts, and B would give the analyzer a second condition next to the formatter's.

**2 · RH5202 · Raw string literals should be formatted correctly — rule page for a literal aligned behind tabs**

The issue's *Additional context* proposes keeping the page as it is; the maintainer's answer does not mention the page. The page tells the reader to align the closing quotes with the opening quotes; for input 1 above, which an editor shows aligned, it does not say why the literal is reported or how to fix it.

Before (`main`): `documentation/rules/RH5202.md` — *Description*: "This rule ensures that multi-line raw string literals have their opening and closing quote markers (`"""`) aligned to the same column. The content lines are adjusted uniformly to maintain consistent indentation."

After, A: unchanged.

After, B (recommended): *Description* gains one sentence: "The indentation in front of the closing marker, and the same indentation of the content lines, is written with spaces, not tabs."

Why B: the page then describes every literal the rule reports; what would change the pick to A is the maintainer treating tab-indented code as outside the page's audience, as the earlier tab change did when the page stayed silent about the spaces the fix already writes.

## Internal choices

_None._ The spec's condition (a tab in front of the closing quotes, not a tab anywhere within the width) is the one the formatter and the fix already use, and the compiler makes the two equivalent for every content line that is not whitespace-only (each content line must start with the closing whitespace), so the alternative would differ only on whitespace-only lines.

## Issue goals

- "With A, the rule page stays as short as it is" — a proposal touching a visible surface; see Output question 2.
- "The diagnostic message may need a wording adjustment" — dropped by lead decision 1; nothing visible changes.
- Every other goal (report, fix writes the formatter's layout, nested literal reported on its own, RH5202-008 keeps withholding the outer fix) serves the issue and is in the spec.

## Architecture findings

_None._ The report condition, the fix's spaces, and the formatter's rewrite share one stated condition in `Reihitsu.Core` (`HasTabInLeadingWhitespace`); the change must not add a second statement of it (FC-015).

## Follow-up candidates

_None._
