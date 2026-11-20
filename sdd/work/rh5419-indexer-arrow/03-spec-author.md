Role: spec author
Commit: c26a8d2
Verdict: spec written

Inputs: `00-lead.md` (*Maintainer answers*: batch 1 Q1 A; Q2 follow-up D with the remark on the formatter), `01-spec-author.md`, `02-skeptic.md`, the change doc and specs of commit 7f9df95. The prompt carried no intended fix or verdict beyond the maintainer's answers.

## Files

- Change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`: `Status: Accepted`; title, What, Why, visible output changes (item 2 rewritten, item 3 extended, new item 5 for comments in the parameter list, items renumbered 6 and 7), Decisions (Q1 A, Q2 follow-up D, formatter deferred, block comment in a one-line list), Alternatives (Q2 A, B, C rejected by the maintainer; formatter join deferred).
- `sdd/specs/analyzer/RH5419.md`: intro, non-goals, RH5419-001, -003, -004 rewritten; `Uses` gains FMT-BREAK-012, SHARED-FIX-003, SHARED-FIX-007.
- `sdd/specs/shared/diagnostics.md`: SHARED-DIAG-010 names RH5419 as an exception (as it names RH5111), with a `Why:` sentence; `Uses` gains FMT-BREAK-012.
- Non-goal pointers: RH5107, RH5108, RH5109 ("The bracketed parameter list of an indexer — RH5419 reports one that spans several lines in an expression-bodied indexer."); SHARED-GETONLY non-goal.
- Rule page `documentation/rules/RH5419.md`: Description, Why, How to fix now ask for the parameter list on one line; example unchanged (BOM kept). Package README row unchanged (Formatter ✔ kept, as for RH5111, whose formatter coverage is partial too).

## Changed requirements

- RH5419-001 — A wrapped indexer is reported — changed (also reports a parameter list on several lines, with the arrow on the `]` line; a `this`/`[` break is not reported)
- RH5419-002 — The diagnostic is on the indexer — unchanged
- RH5419-003 — The fix puts the indexer on one line — changed (the fix joins the parameter list, then formats)
- RH5419-004 — The fix is withheld where the indexer cannot be put on one line — changed (comment, directive, or disabled text inside a parameter list on several lines; parameter list or `=>`…`;` still on several lines; FMT-UNSUP-012/013 and RH5401-004 cases unchanged)
- SHARED-DIAG-010 — A layout rule does not report a shape the formatter keeps — changed (RH5419 exception)

## Examples for the tests

RH5419-001, RH5419-002, RH5419-003, RH5419-004 (two examples: withheld and offered).

## How the examples were checked

- Every spec code block was extracted (markup removed) and run through `scripts/format.sh`: all parse; the RH5419-004 withheld block is refused as unsupported (FMT-UNSUP-013), as the requirement states; no syntax error anywhere.
- The fix results of RH5419-003 and RH5419-004 (second example) were checked as "joined parameter list, then formatter": the joined inputs formatted to exactly the spec's output blocks, and a second run left them unchanged (SHARED-FIX-007). `int this\n    [ulong a, ulong b] => 1;` → `int this[ulong a, ulong b] => 1;`; `int this /* c */ [sbyte a, sbyte b] => a;` stays; `int this[byte a /* c */]\n    => a;` → one line.
- `int this[string s = """ ... """] => 1;` compiles as a constant default value; the formatter keeps it on three lines (withheld under RH5419-004).
- The `#if` example uses `uint`/`ulong` so neither build declares a duplicate indexer.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed — the same three expected until the rule exists and is tested (`AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType`, `EveryRequirementOfAnEnforcedSpecIsCovered`). `UsesListsExactlyTheReferencedRequirements` passes.

## Output questions

**1 · RH5419 · Expression style get-only indexers should be single lined — block comment inside a parameter list on one line** (not blocking; the spec is written on A)

The maintainer's answer D withholds the fix "when a comment or directive sits inside the parameter list". It was given for a wrapped parameter list. A parameter list on one line can hold only a block comment (a directive or line comment forces a line break), and the fix joins nothing there.

Input:

```csharp
class C
{
    int this[short a /* c */]
        => a;
}
```

Before (`main`): not reported.

After, A (recommended): reported; the fix is offered and gives

```csharp
class C
{
    int this[short a /* c */] => a;
}
```

After, B: reported; no fix offered; the code stays as written.

Why A: the comment does not move, so nothing makes the fix unsafe; withholding it would be a refusal without a cause (`AGENTS.md`, imprecise guard). Requirement touched: RH5419-004 (its sentence "a block comment inside a parameter list that stands on one line does not withhold the fix"), change-doc item 5 and the last Decision.

## Internal choices

- **Formatter not changed in this run.** The maintainer allowed it "if the fix needs it". The fix does not: it removes the line breaks in the trivia between `[` and `]` (a cheap gap edit) and formats; a one-line parameter list is one the formatter keeps (FMT-BREAK-012), so SHARED-FIX-007 holds. A formatter join would change every whole-document run and contradict the family requirement that an indexer's parameter list is laid out as a method's is (FMT-PIPE-014, FMT-BREAK-011, FMT-BREAK-012). Consequence: RH5419 reports a shape the formatter keeps, so SHARED-DIAG-010 gains an RH5419 exception next to RH5111's, and a CLI user who only formats keeps the parameter-list diagnostic until the follow-up lands. Alternative: specify an FMT-BREAK requirement joining an expression-bodied indexer's parameter list now (no SHARED-DIAG-010 exception, but a formatter output change across all documents).
- **Withhold condition by cause, not "whole declaration".** RH5419-004 withholds when, after the join and formatting, the parameter list or the code from `=>` through `;` still spans several lines — not when the indexer's code from its first token does. So `int this // c\n    [int a,\n     int b] => a;` (rare; comment between `this` and `[`) gets the fix and the formatter's `int this // c\n    [int a, int b] => a;`, which RH5419 no longer reports. Alternative: measure from the first token, which withholds it. Kept as it was accepted under Q1 A.
- "Spans several lines" for the parameter list counts any cause (line break, multi-line comment, directive, multi-line literal); only the fix's guard distinguishes them.
- Package README Formatter column stays ✔ (the formatter clears the arrow cases), as RH5111's does with partial coverage.

## Follow-up candidates

- **Formatter: join the parameter list of an expression-bodied indexer onto one line** (maintainer answer, Q2 follow-up; already in `00-lead.md`). Input `int this[uint a,\n         uint b] => 1;`; today's whole-document output keeps the two lines (FMT-BREAK-012); wanted `int this[uint a, uint b] => 1;`, withheld where a comment, directive, or disabled text sits inside the list. Landing it lets SHARED-DIAG-010's RH5419 exception shrink to the comment case. It also concerns FMT-STRUCT-015 and RH3210, whose output from a get-only indexer with a wrapped parameter list RH5419 now reports.
