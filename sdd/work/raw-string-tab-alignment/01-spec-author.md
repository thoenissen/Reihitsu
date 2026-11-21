Role: spec author
Commit: c50b341
Verdict: change doc written

Issue: thoenissen/Reihitsu#1432 (`agent-filed`). Change doc: `sdd/changes/2026-11/raw-string-tab-indentation.md` (Status: Draft). Every *before* was produced on `c50b341` with `scripts/format.sh` / `scripts/apply-fix.sh` (RH5202, RH3202); every *after* was formatted on `c50b341` and is stable apart from the final newline, and every FMT-RAW-010 input formats today to the same character columns as its *after*.

Premise notes on the issue text:
- Today's output for the issue's input is 14 spaces followed by the two tabs, not 16 spaces (`main` adds the 14-column shift; each tab counts as one column).
- `RawStringAlignmentPhase` does not exist; the move is `RawStringLineShifts`, run by the indentation phase.
- The issue's comment (hole line aligned under a token behind a tab prefix) reproduces on `main`: the wrapped argument lands in column 38 counted in characters, beside `"a"` on screen. FMT-RAW-010 resolves it without changing any column; its example is the last-but-two block of FMT-RAW-010 and change 6 of the change doc.

## Changed requirements

- FMT-RAW-010 — The stripped indentation of a tab-indented literal is written as spaces — added
- FMT-RAW-001 — tab sentence and four-tab example output changed (tabs now become spaces via FMT-RAW-010)
- FMT-RAW-002 — changed: "whitespace the shift does not remove stays" now excepts FMT-RAW-010
- FMT-RAW-006 — changed: an aligned literal with a tab in front of its closing marker still gets FMT-RAW-010 (no line moves)
- FMT-RAW non-goal on tab-width alignment — rewritten (counting a tab wider than one column stays a non-goal; the #1432 "planned extension" text removed)
- FMT-TAB-003 — changed: exception for the tabs in a raw string's stripped indentation (FMT-RAW-010); non-goal of tab-replacement.md rewritten
- FMT-INDENT-011 — clarified: names FMT-RAW-010 as the only rewrite of raw-string text whitespace
- RH5202-007 — The fix writes the stripped indentation of a tab-indented literal as spaces — added
- RH5202-003 — clarified: points to RH5202-007
- Uses rows: raw-string-alignment.md drops FMT-TAB-003 (no longer referenced); tab-replacement.md, indentation.md, RH5202.md add FMT-RAW-010. Ledger: FMT-RAW-010, RH5202-007 inserted.

`scripts/test.sh --project architecture`: only `EveryRequirementOfAnEnforcedSpecIsCovered` fails, for FMT-RAW-010 and RH5202-007 (no covering test yet — expected until the test implementer runs). Everything else passes.

## Examples for the tests

- FMT-RAW-010 — nine examples, one per condition side: right shift, extra tab behind width kept, no tab in closing whitespace (tab in content kept), aligned-in-characters literal rewritten, left shift over twenty tabs, whitespace-only line shorter than the closing whitespace, hole line aligned to a token on a tab content line, nested raw string decided by its own closing marker, verbatim continuation line in a hole keeps its tab.
- FMT-RAW-001 — the changed four-tab example.
- RH5202-007 — one fix example (tab behind the width kept); the "no tab" and "aligned, not reported" sides are stated in prose and are worth a test each.
- Change 8 of the change doc (RH3202 fix over a tab-indented literal) shows that formatting code fixes pick up the change; FMT-RAW-010 covers it, no RH3202 requirement changes.

## Output questions

All within thoenissen/Reihitsu#1432. `→` is one tab, `·` one trailing space.

**1 · Formatter — tab-indented multi-line raw string (FMT-RAW-010; changes 1, 2, 4, 6, 8)**

Input:
```csharp
        var a = """
→→Hello
→→→World
→→""";
```
Before (`main`): 14 spaces, then the tabs kept:
```csharp
        var a = """
              →→Hello
              →→→World
              →→""";
```
Looks wrong: mixed indentation; with any tab width above 1 the content and closing quotes stand right of the opening quotes.
After, A (recommended): stripped indentation written as spaces, tab behind it kept (value unchanged):
```csharp
        var a = """
                Hello
                →World
                """;
```
After, B: unchanged from `main` (tab-indented raw strings stay a non-goal).
Why A: it is the issue's request, keeps the value and every character column, and fixes the visual misalignment, including hole lines aligned to content tokens.

**2 · Formatter — a literal already aligned in characters behind tabs (FMT-RAW-006; change 3)**

Input (= `main` output for the four-tab FMT-RAW-001 example):
```csharp
        var a = """
            →→→→x
            →→→→""";
```
Before (`main`): unchanged.
After, A (recommended):
```csharp
        var a = """
                x
                """;
```
After, B: unchanged (only literals that move get their tabs written as spaces).
Why A: otherwise the output depends on whether the input happened to be aligned in characters, and B keeps exactly the mixed indentation question 1 removes.

**3 · RH5202 · Raw string literals should be formatted correctly — fix on a tab-indented literal (RH5202-007; change 7)**

Input:
```csharp
        var a = {|RH5202:"""
→→Hello
→→"""|};
```
Before (`main`):
```csharp
        var a = """
              →→Hello
              →→""";
```
After, A (recommended):
```csharp
        var a = """
                Hello
                """;
```
After, B: unchanged from `main`.
Why A: the fix's result must be the formatter's layout (SHARED-FIX-007); under 1A, B would leave a result that formatting changes again. The analyzer reports the same literals either way (it still counts each whitespace character as one column).

**4 · Formatter — whitespace-only content line holding only part of the closing whitespace (FMT-RAW-010; change 5)**

Input (middle line: one tab; the compiler accepts it because it is a start of the closing `→→`; it contributes an empty line to the value):
```csharp
        var a = """
→→Hello
→
→→""";
```
Before (`main`): middle line 14 spaces + one tab.
After, A (recommended): middle line 15 spaces (moved as today, each character one space):
```csharp
        var a = """
                Hello
···············
                """;
```
After, B: middle line 16 spaces (the full width).
After, C: middle line empty.
Why A: it falls out of the one rule (one space per character after the move) with no second case; all three keep the value.

## Internal choices

- Trigger is a tab in the closing marker's whitespace only; a literal whose closing whitespace holds only spaces or other whitespace (no-break space) keeps today's output (FMT-RAW-002 example unchanged). Alternative: rewrite every non-space character in the stripped indentation — would change FMT-RAW-002 output nobody asked for.
- Stated as "one space per whitespace character, after the move" rather than "replace the closing whitespace by the target column's spaces": same output on every line that holds the whole closing whitespace, keeps RH5202 and the formatter on one measure, and changes no column, so FMT-INDENT-030 alignment needs no new rule.
- Applies at every depth (each nested raw string decided by its own closing marker), consistent with FMT-RAW-008; the RH5202 fix rewrites only the reported literal's own lines (nested literal reported and fixed on its own).
- RH5202 rule page left unchanged: what is reported and how to fix it in the normal case do not change.
- AGENTS.md's IndentationPhase row ("the move of multi-line raw-string content lines…") does not name the rewrite; recommend the lead or implementer adds "and the rewrite of their stripped tab indentation as spaces" when the code lands. Not edited here.

## Follow-up candidates

_None._
