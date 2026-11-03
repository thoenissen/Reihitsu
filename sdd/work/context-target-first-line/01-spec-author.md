Role: spec author
Commit: 6a7ed36
Verdict: change doc written

The prompt carried no intended fix or verdict. The issue's "Proposed Behavior" is a prescribed solution of an `agent-filed` issue and was treated as a proposal (one part is not followed, see *Internal choices*).

## Issue statements checked against `main` (`21c2ff4`)

Measured with a scratch harness (outside the repository) that runs each rule's analyzer and code fix from the Release build on an input, plus `FormatSyntaxTree` for comparison.

- "FMT-PIPE-017 records this as a gap with two cases": out of date. FMT-PIPE-017 now records three cases; the third (a raw string literal's line moved in front of the target) was added after the issue. All three are covered by the change.
- The RH5111 declarator example and its output (`b` column 14, `2` column 18, `1` column 24): reproduced exactly.
- "The indentation phase already pins the context's first token (FMT-PIPE-017's offset)": true (`rootFirstTokenColumn` from the context's first token in `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`).
- "The entry point writes back the target's leading trivia as in the document": true; it replaces the formatted first token's leading trivia with the document's after the pipeline.
- Blockers #1487 and #1489: merged; the change builds on FMT-PIPE-024's fact list, which now gets the target-position fact.
- FMT-PIPE-017's second-case example (argument `Wrap(…)`) and third-case example (`Call(1, 2)` in a raw-string hole) use targets no code fix passes. Replaced by code-fix inputs (*Supported inputs*).

## Callers that reach the change

`FormatNodeInDocumentWithContextAsync` has three callers, all code fixes:

- RH5111 (`RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider`): target a `VariableDeclaratorSyntax` with its `LocalDeclarationStatementSyntax`/`FieldDeclarationSyntax` (grandparent) as context, a `ParameterSyntax` with its `ParameterListSyntax`, an `EnumMemberDeclarationSyntax` with its `EnumDeclarationSyntax`. Measured: case 1 (later line, column differs), case 2 (`int b, string c` — layout breaks in front of `string c`), case 3 (declarator behind a moved raw-string closing line) all reached, with visibly misaligned output today.
- RH3208 (`RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider`): target the local function, context the labeled statement that starts its line. Measured: `L:    int F(int a,` / `int b) => …` gives `int b` in column 17 under `int a` in column 20 today (the layout puts one space behind `L:`). One space behind the label: no difference.
- RH5112/RH5201 (`FluentChainCodeFixHelper.FormatChainAsync`): target the outermost chain node, context the statement/member that starts its line. The helper keeps the chain-only result only when chain text and first-token column equal the whole-context result; measured `var x = Call(1, s.Trim()` / `.ToUpper());` falls back to the context result. Reasoned (not measured exhaustively): the fall-back decision and both results are unchanged by the change, so chain-fix output does not change. The test implementer should confirm with the existing RH5112/RH5201 tests.

## Changed requirements

- FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — changed: recorded gap removed; the context entry point's target keeps its first token's line and column (each character in front counted as one column), its other lines follow it where aligned to an anchor, depth-placed lines stay; three ways the layout can differ, each with a code-fix example, plus an other-side example.
- FMT-PIPE-018 — The context entry point writes back its target's leading trivia as in the document — changed: the phases keep that trivia as the preparation phase leaves it and place the first token in its document column; the write-back only undoes the preparation phase's tab and line-ending rewrites and changes no line or column. Normative first sentence and both examples unchanged.
- FMT-PIPE-024 — Node-level input facts are plain values — changed: new fact row "Target position" (context entry point only); plain values may include an annotation that marks a token of the root; two decision bullets (line-break phase keeps the target's first token on its line; documentation, line-break, blank-line phases keep its leading trivia; indentation phase places it in the fact's column and leaves the trivia's lines).
- SHARED-FIX-007 — The changed code is laid out as the formatter lays it out — changed: the "recorded gap" bullet replaced by "Changed code formatted within a larger node"; the RH5111 example pointer updated.
- RH5111-004 — The fix lays out the construct — changed: recorded gap replaced by the normative layout; Why rewritten; declarator example output changed to the aligned one; new example (parameter behind another parameter on its line).
- `AGENTS.md` architecture paragraph: the context entry point is no longer named as the one exception to "no step after the last phase changes a line's column".
- No new requirement IDs; `requirement-ids.txt` unchanged. `SpecConsistencyTests`/`SpecStructureTests`/`RequirementCoverageTests` pass (51 tests).
- Rule page `documentation/rules/RH5111.md` unchanged: the normal case of the fix does not change for the user.

## Examples for the tests

- FMT-PIPE-017: the six new examples (first way: declarator `b` with `Other(1, 2)`; declarator with object creation and with lambda; enum member `B` and parameter `string c`; second way: parameter `string c` behind `int b,`; second way: RH3208 `L:    int F(int a, int b)`; third way: declarator behind a raw-string closing line; other side: RH3208 `L: int F(…)`). Each names the fix and target/context; tests can call `FormatNodeInDocumentWithContextAsync` with those nodes or go through the fix.
- FMT-PIPE-018: the two existing examples keep their output; the tab-indented declarator test (`DeclaratorTargetOnALaterLineOfATabIndentedContextKeepsTheWhitespaceInFrontOfIt`) shows the write-back keeps tabs.
- RH5111-004: the declarator example (new output) and the new parameter example.
- Existing tests whose expectations the change alters (facts): `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumnAndLeavesTheWrappedArgumentInTheFormattersLayout`; formatter `ReihitsuFormatterNodeColumnTests.ArgumentTargetOnALaterLineOfItsContextKeepsItsFirstLineColumnAndLeavesTheAlignedLineInThePhaseLayout` and `InvocationTargetInsideAMovingRawStringLiteralTakesThePhasesColumnOnItsSecondLine` — the last two use targets no code fix passes (an argument, an invocation), so their outputs are not in question; whether to keep them is the lead's call.
- The XML doc comments of `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` still describe the gap; they belong to the implementer.

## Output questions

All within thoenissen/Reihitsu#1501; the change doc lists six visible output changes (RH5111 ×5, RH3208 ×1), each to be asked. Two of them are choices between readings, not only a change:

**1 · RH5111 · Assignments must have proper line breaks — declarator on a later line of its declaration, wrapped value** (FMT-PIPE-017, RH5111-004)

Input:
```csharp
        int a = 1,
              b =
                  Other(1,
                        2);
```
Before (`main`):
```csharp
        int a = 1,
              b = Other(1,
                  2);
```
After, A (recommended):
```csharp
        int a = 1,
              b = Other(1,
                        2);
```
After, B (first line in the formatter's column):
```csharp
        int a = 1,
        b = Other(1,
                  2);
```
Why A: the fix keeps the construct's first token where the user wrote it, as every other node-level fix does, and the value follows it; B rewrites the whitespace in front of the construct that FMT-PIPE-018 keeps. The same choice applies to a parameter or enum member on a later line (change doc, changes 2 and 3).

**2 · RH5111 · Assignments must have proper line breaks — parameter behind another parameter on its line, wrapped value** (FMT-PIPE-017, RH5111-004)

Input:
```csharp
    void M(int a,
           int b, string c
               = "x" +
                 "y")
```
Before (`main`):
```csharp
    void M(int a,
           int b, string c = "x"
                      + "y")
```
After, A (recommended):
```csharp
    void M(int a,
           int b, string c = "x"
                             + "y")
```
After, B (break in front of the parameter, as formatting the document does):
```csharp
    void M(int a,
           int b,
           string c = "x"
                      + "y")
```
Why A: the fix changes only the reported parameter's own lines and does not move it off the line it shares with `int b`; B also rewrites the gap behind `int b,`, which lies outside what the fix formats.

The remaining changes (4: raw-string closing line; 6: RH3208 wrapped parameters behind a label) have only one output consistent with FMT-PIPE-010, because what stands in front of the target on its line is outside the target; they are listed in the change doc for acceptance.

## Internal choices

- The entry point keeps writing back the document's leading trivia (the issue's "Done when: no longer replaces the target's leading trivia after the last phase" is not followed). Reason: the preparation phase rewrites tabs and foreign line endings in the whole root (FMT-PIPE-019), and FMT-PIPE-018 keeps the document's tabs (tab-indented RH5111 test); writing back the phases' trivia would change that output. Instead the phases keep the trivia and pin the first token, so the write-back changes no line or column. Alternative: exempt the target's leading trivia from the preparation phase — rejected, it breaks FMT-PIPE-019's "no later phase sees a tab".
- Lines the indentation phase places by the depth do not follow the target's first token; only anchored lines do. Alternative: shift every target line by the first token's displacement — rejected, it moves the RH3208 block off its line start (RH3208-003).
- FMT-PIPE-024's plain values now include "an annotation that marks a token of the root"; the target is located by the existing first-token annotation (FMT-PIPE-022). Alternative: a separate positional fact — no benefit.
- Requirement IDs kept (changed in place) rather than retiring FMT-PIPE-017/018 and adding new ones.
- `AGENTS.md` architecture sentence updated in this run, since it would otherwise state a removed exception. Alternative: leave it to the lead.
- RH5111 rule page left unchanged.

## Follow-up candidates

_None._
