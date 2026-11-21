Role: skeptic
Commit: 8c2ce8b
Verdict: proceed

Issue: thoenissen/Reihitsu#1432 (`agent-filed`). Reviewed against the change doc `sdd/changes/2026-11/raw-string-tab-indentation.md` and the spec diff `c50b341..8c2ce8b`. The prompt carried no intended fix and no hoped-for verdict. `→` is one tab in every block below. Fixtures were written with `printf` into a scratch directory outside the repository, checked with `cat -A`, and deleted afterwards. The production code at 8c2ce8b is still `main`'s.

## Premise

Real, and a caller reaches it. **Caller:** whole-document formatting through the CLI (`reihitsu-format`, `scripts/format.sh`) on any file that holds a multi-line raw string whose stripped indentation uses tabs. The RH5202 code fix reaches it too (`scripts/apply-fix.sh RH5202`), and so does every code fix that formats a node holding such a literal, such as the RH3202 fix in change 8. Tab-indented code is common, and an editor that indents with tabs writes them into the raw string text as well. Code whitespace loses its tabs to FMT-TAB-001, but the literal keeps its tabs. So the mixed output is what every tab user gets. It is not a rare class.

Observed on 8c2ce8b (= `main` code). `scripts/format.sh` was run twice, and the second pass was stable:

```
input (f01)                      output
        var a = """                      var a = """
→→Hello                                        ··············→→Hello   (14 spaces + 2 tabs)
→→""";                                         ··············→→""";
```

`scripts/apply-fix.sh RH5202` on the same input gives `fixed (1 action(s), 1 iteration(s))` with the same 14 spaces + 2 tabs, under both LF and CRLF.

The issue's claim of "16 spaces followed by the two tabs" is wrong: the output has 14, because each tab counts as one column (FMT-RAW-001). The spec author already noted this. The issue's phase name, `RawStringAlignmentPhase`, is also stale. The move is `RawStringLineShifts`, which the indentation phase runs (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`).

**The value-preservation premise was compiled, not derived.** The rewrite keeps the value only if every character the change turns into a space is a copy of the closing whitespace. I checked this with `scripts/apply-fix.sh RH5202`, using a `parse-error` result to mean that the fixture does not compile (closing line `→→`, content `→→Hello`, plus one extra line):

| Extra line | Result |
|---|---|
| `→` (whitespace only, shorter, a start of the closing whitespace) | parses; formatter: 14 spaces + `→` |
| `→→→` (whitespace only, longer, starts with the closing whitespace) | parses; formatter: 14 spaces + `→→→` |
| `··` (two spaces), `···`, `·→`, `→·→` | `parse-error` |

So within the width, every content line, whitespace-only lines included, holds exactly a prefix of the closing whitespace. Triggering on the closing line alone (the spec author's first internal choice) is complete: no content line can hold a tab within the width while the closing whitespace holds none. One space per character therefore keeps the value on every line that compiles.

The issue's comment (a hole line aligned to a token that stands behind a tab prefix) reproduces, per the spec author, and the same rule resolves it without moving a column (change 6). One residual stays: a tab *behind* the width is part of the value, so a hole line aligned to a token behind such a tab still stands off it visually. The rewritten non-goal (counting a tab wider than one column) covers that case. It needs no question.

## Mechanism and candidates

This is a behavior change, so there is no defect mechanism. The decision the change adds is "which whitespace characters at the start of a raw-string line become spaces". It is decided per line, by the innermost multi-line raw string whose own text the line starts in, and by that literal's closing whitespace. Dispatch:

- Formatter: `RawStringLineShifts.MoveLines` walks only the outermost multi-line raw literals (`IsNestedInRawStringHole`). The lines of a nested literal are handled as lines of the outermost one, through `CollectEdits`, which skips any line where `newSpaces == indentation`. Today an aligned literal therefore gets no edit at all, and FMT-RAW-006/010 now needs one (change 3).
- Edits land in `MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`, `InterpolatedStringTextToken`, and `InterpolatedRawStringEndToken` (`HoldsRawStringText`), or in hole whitespace trivia, which holds no tab after FMT-TAB-001.
- RH5202 fix: `ApplyCodeFixAsync` shifts every line that `RawStringLineUtilities.MovesLine` admits, nested raw-literal lines included, through the shared `RawStringLiteralUtilities.GetLineShiftEdit`.

## Breaking inputs

Failure classes that reach the change: FC-022 (raw and interpolated raw literals), FC-027 (nesting and enclosing scope), FC-010/FC-017 (second pass, CRLF), FC-008 (Fix All on overlapping spans), FC-015 (one rule, two owners: formatter and RH5202 fix), FC-028 (`FormatNode` vs. `FormatNodeInDocumentAsync`), FC-013 (analyzer vs. formatter; see P11 below), and FC-034 (test the issue comment's own shape). The *Tab width across formatter phases* matrix applies. Its *shift* and *anchoring* axes are what the cells below enumerate.

**Defect found in the spec (RH5202-007, third bullet).** The bullet says that the lines of a nested raw literal "keep their characters: the nested literal is reported on its own, and its own fix writes its indentation". That is false when the nested literal is aligned in characters behind tabs, because RH5202 does not report it. Observed on 8c2ce8b:

```csharp
// n01 input
        var a = $"""
    Outer {"""
         →→inner
         →→"""}
    """;
```

`scripts/apply-fix.sh RH5202`: `fixed (1 action(s), 1 iteration(s))`, LF and CRLF. The nested literal is not reported before or after the fix:

```csharp
        var a = $"""
                 Outer {"""
                      →→inner
                      →→"""}
                 """;
```

Under FMT-RAW-010 the formatter then writes those two tabs as spaces, so the fix's result is not the formatter's layout. That violates SHARED-FIX-007, which is unchanged. This input shape is exactly what `main`'s formatter writes for a nested tab-indented literal (FMT-RAW-008), so it exists in code formatted today. Class members:

| Outer closing whitespace | Nested closing whitespace | Nested aligned in characters before the outer fix | Fails |
|---|---|---|---|
| spaces | tabs | yes (n01) | **yes**: the nested literal is not reported and keeps its tabs |
| spaces | tabs | no (n02: `→→inner` / `→→"""}`) | no: the nested literal is reported, and its own fix runs in iteration 2 (observed `fixed (1 action(s), 2 iteration(s))` today) |
| tabs | tabs | yes | **yes**: same as n01; the outer fix rewrites only its own lines |
| tabs or spaces | spaces only | either | no: nothing to rewrite |

See *Internal choices* 1 for the repair.

Candidate inputs for the test implementer. Each is whole-document formatting unless stated otherwise, under LF and CRLF, with a second pass expected to be a no-op:

1. Right shift: change 1 and change 2. Pinned today with the old values by `RawStringClosingWhitespaceMeasureTests.TabsCountAsOneColumnEachAndStay` (`src/Reihitsu.Formatter.Test/Regression/FullPipeline/`) and `RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsLiteralBehindTabsAndClearsTheDiagnostic` (`src/Reihitsu.Analyzer.Test/Formatter/Formatting/`). **Update these tests; do not add new ones** (*Coverage first*).
2. No shift, literal aligned in characters (change 3). Pinned with the old values by `RawStringClosingWhitespaceMeasureTests.LiteralAlignedBySharedMeasureWithTabsStays` and `RH5202…FormatterTests.VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindTabs`. Update them.
3. Left shift, along the matrix's leftward axis, with spaces in front of the tabs in the closing whitespace. The target column is 16. Three cases: `··········` + 8 tabs (width 18, shift −2, fewer than the spaces); `····` + 16 tabs (width 20, shift −4, exactly the spaces); `··` + 18 tabs (shift −4, more than the spaces). The 20-tab example covers only the "no spaces in front" member. None of these is found pinned with tabs; the no-break-space versions (`LeftShift…`) exist.
4. Whitespace-only content lines. Shorter than the width (change 5, `→`): none found. Longer than the width (`→→→` under `→→`, expected 16 spaces + `→`): none found, and the spec has no example for it.
5. Closing whitespace that mixes a tab with a no-break space, a vertical tab, or a form feed: all of them become spaces. None found.
6. Closing whitespace with no tab and a tab behind the width on a content line: unchanged (the third FMT-RAW-010 example). Partly pinned by the no-break-space tests; none with a tab.
7. Literal kinds: UTF-8 `"""u8`, interpolated `$"""`, and `$$"""`. In interpolated literals, also a content line that is only the prefix followed by a hole (`→→{x}`), both as the first content line (behind the start token) and as a later line (behind a `}` that closed a hole on the line above). For the closing line, the whitespace lies in `InterpolatedRawStringEndToken`. None found with tabs.
8. Nesting (FC-027) at every combination of outer and nested closing whitespace (spaces/tabs × spaces/tabs). The spec shows only outer-spaces/nested-tabs. Also a raw literal in a hole of a regular `$"…"` or verbatim `$@"…"` string (an outermost raw literal in another enclosing scope), and a verbatim continuation line in a hole (spec example). Today's `NestedLiteralOpening…TabIndentedHoleLine…` tests in `RawStringInterpolationHoleTests` hold tabs in hole trivia, not in literal text; check whether any of their literal text holds tabs before reusing them.
9. Anchoring: a wrapped argument in a hole aligned to a token on a tab content line (change 6). Also the issue comment's own shape (FC-034): a `delegate { … }` or block-lambda body in a hole on a `→`-indented content line, with the closing marker behind one tab. Also an anchored hole line in an aligned literal (no shift). None found.
10. RH5202 fix (`scripts/apply-fix.sh`/verifier): change 7, n01 (after the repair, *Internal choices* 1), and Fix All over outer plus nested where both are reported (n02 shape, FC-008). With RH5202-007, the nested literal's change at a line start is no longer zero-length, and the outer fix inserts at the same position. `GetFixAllScenario` exists, and none found for nesting with tabs.
11. Node-level entry points: change 8 (RH3202 through `FormatNodeInDocumentAsync`) and the same node through `FormatNode` (FC-028). None found with a tab literal.

The test implementer should run the formatter, analyzer, and Core suites after the implementation lands. Other tests that hold tab-indented raw-string text may change. `WhitespaceCleanupTests.TabsAndSpacesInsideCommentsLiteralsAndDisabledTextAreKept` was checked: its tab is behind the width, so it stays.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `IndentationPhase` (`RawStringLineShifts`) | Must change: the FMT-RAW-010 rewrite, also on aligned literals |
| `Reihitsu.Core` `RawStringLiteralUtilities` | Must change: should hold the one statement of the rewrite (*Internal choices* 2) |
| RH5202 code fix | Must change (RH5202-007), including nested lines (*Internal choices* 1) |
| RH5202 Fix All | Must stay correct; test it (candidate 10) |
| RH5202 analyzer | Must stay (same measure, same literals reported); see the P11 follow-up |
| Other code fixes that format through the formatter (RH3202 etc.) | Not changed; their output follows (change 8) |
| RH5601 analyzer and fix | Must stay. SHARED-DIAG-003 keeps a code rule out of literal text, so RH5601-002 stays. Only its `Why:` wording goes stale (*Internal choices* 3) |
| `TabReplacementPhase` | Must stay (FMT-TAB-003 exception is the indentation phase's, not this phase's) |
| Precheck / unsupported shapes | Not involved |
| Rule pages (`RH5202.md`, `RH5601.md`) | Must stay. RH5601's page says raw-string literal tabs are left untouched, and the analyzer and its fix still leave them, so the page stays true |
| `AGENTS.md` IndentationPhase row | Should name the rewrite when the code lands (spec author's note) |

## Refuse option

- **Leave the construct as written.** This keeps the non-goal, which is today's output. It costs zero production lines. But it leaves mixed tab/space indentation in every tab-indented raw string, which is common code, not a rare class. So refusing is not covered by *Special cases do not grow the source*, and it would be a maintainer decision (option B of question 1). The requested change is small: a per-character rewrite in an edit that already exists, the aligned-literal path that `CollectEdits` skips today, and the same rewrite in the RH5202 fix. It needs no new type and no new mechanism, and it adds no guard.
- **Declaring a shape unsupported** is not an option. The shape would be "a tab in the whitespace in front of the closing marker of a multi-line raw string literal". No special case handles that shape today (a tab simply counts as one column), so declaring it would delete nothing, and *Special cases do not grow the source* says a shape that deletes nothing is not declared. It would also refuse whole files that people commonly write. No escalation 10.
- **Narrower:** for the n01 class only, withhold the RH5202 fix (*Internal choices* 1, B).

## Output questions

The spec author's four questions stand, and I agree with each recommendation (1A, 2A, 3A, 4A). Two remarks for the lead:

- Q2 is not an independent choice in practice. Under 1A with 2B, the output depends on whether the input happened to be aligned in characters, and `main`'s output for every tab-indented literal is exactly the 2B shape. 2B would leave every file formatted today in the mixed state. Recommend asking Q1 and Q2 together as they stand.
- Q3's example is unaffected by the n01 repair below.

No new output question from me: the n01 repair is demanded by the unchanged SHARED-FIX-007 once 1A and 3A are answered, or it is a rare-class withholding, which puts nothing in question.

## Internal choices

1. **Repair RH5202-007's nested bullet (n01).** Options:
   - **A:** the fix also writes, on the lines it shifts that start in a nested raw literal's own text, that literal's stripped indentation as spaces when its closing whitespace holds a tab. This is decided per literal, as FMT-RAW-010 decides it "at any depth". The fix's result is then the formatter's layout. It costs little if the per-line "innermost owning raw literal and its width" lookup is the shared Core helper, which the formatter needs anyway for FMT-RAW-010's nested lines.
   - **B:** withhold the outer fix when a nested multi-line raw literal's closing whitespace holds a tab. This is a cheap check inside the existing `ChangesValueOfNestedRawString` loop. It is a rare class (a nested raw string in a hole, tab-indented), the diagnostic stays without a fix, and it puts nothing in question.

   **Recommend A**, because it keeps one rule for formatter and fix and adds no guard. Fall back to B if the shared helper does not cover nested lines. Either way, the spec author amends the third bullet of RH5202-007 before the test implementer starts. The text "the nested literal is reported on its own" is false for n01.
2. **One owner for the rewrite (FC-015).** The formatter and the RH5202 fix already share the shift through `RawStringLiteralUtilities.GetLineShiftEdit`. The "which characters within the width become spaces after the move" rule should live there too, as one helper that both call. Otherwise the rewrite is stated twice and can drift, which is the structure *Architecture findings* names. Recommend the shared helper; the cost is the same.
3. **Spec wording, RH5601-002 `Why:`.** It says that raw-string tabs are "user-authored content the formatter keeps as well (FMT-TAB-003)". After this change, the formatter rewrites the stripped ones. Recommend that the spec author amends that sentence, citing FMT-RAW-010 as FMT-TAB-003 does now, and leaves the requirement itself unchanged. SHARED-DIAG-003 keeps code rules out of literal text and names RH5202 as the rule that judges a literal's spelling. No visible output changes. `pipeline.md` FMT-PIPE-019 (line 1167, "keep their tabs (FMT-TAB-003)") stays accurate through FMT-TAB-003's new exception; it can optionally name FMT-RAW-010.

## Issue goals

- "Today's output: 16 spaces followed by the two tabs": stale and wrong; it is 14 spaces. Drop it. No visible effect.
- "Phase: `RawStringAlignmentPhase`": stale; the phase is `IndentationPhase` / `RawStringLineShifts`. Drop it. No visible effect.
- "Replace that prefix on every line with the spaces of the target column": the spec reformulates this as one space per character after the move. Both give the same output on every line except a whitespace-only line shorter than the width, which is question 4. Keep the reformulation. Its visible part is already asked.
- The issue names only the formatter. Adding RH5202-007 is required, not scope creep: without it, the RH5202 fix's result would be reformatted (SHARED-FIX-007). Its visible part is question 3.
- The issue comment's case: confirmed and resolved by the same rule. Keep it, and test it with its own shape (candidate 9).

## Architecture findings

_None_ in today's code. The duplication risk in *Internal choices* 2 is prospective: it becomes a finding only if the implementation states the rewrite twice.

## Follow-up candidates

**P11, RH5202's own gap: an aligned literal with tabs in its stripped indentation is not reported.** The formatter now rewrites it (change 3), and RH5202 stays silent. SHARED-DIAG-003 names RH5202 as the rule that judges how a literal spells its value, and the stripped indentation is that spelling. So by P11 this is the rule's own gap, a behavior change of RH5202 that adds diagnostics. It is not a SHARED-DIAG-010 conflict, because the formatter's output is not flagged. It is outside #1432, which asks only for the formatter. If it is filed, the RH5202-007 claim "the nested literal is reported on its own" would become true, and *Internal choices* 1 would matter only within a single fix application.

**RH5202 · Raw string literals should be formatted correctly: tab-indented literal already aligned**

Input:
```csharp
        var a = """
            →→→→x
            →→→→""";
```

Before (`main`): not reported (`scripts/apply-fix.sh RH5202`: `no-diagnostic`, LF and CRLF).

After, A (recommended):
```csharp
        var a = {|RH5202:"""
            →→→→x
            →→→→"""|};
```
The fix writes the formatter's layout:
```csharp
        var a = """
                x
                """;
```

After, B: not reported (an analyzer-only user keeps the mixed indentation).

Why A: the analyzer then reports everything the formatter rewrites in this construct, and users who run only the analyzer get the same layout.
