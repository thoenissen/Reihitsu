Role: spec author
Commit: 667c2ba
Verdict: change doc written; open question (blocking for one part: Question 1, the context entry point's output for a target inside a moving raw string literal)

The change doc `sdd/changes/2026-11/phase-tree-round-trip.md` (`Status: Draft`) needs the maintainer's approval of What and Why (escalation 2). It was put under `2026-11` because the run cannot merge before November; move it if it merges in October. My prompt carried no intended fix or verdict to ignore.

## Statements of thoenissen/Reihitsu#1488 checked against 57f0f45

Established by reading the code and by running `scripts/trace.sh` on scratch inputs (the issue's first input, and `void M() {` / `if (true) {`); no test was run for them.

- Still true: `ExpressionBodyToBlockConverter.CreateReturnStatement` creates `return` without trailing trivia and drops the expression's whitespace-only leading trivia; the trace shows `int Get(){return42;}` after `StructuralTransformPhase` and `return 42` only after `HorizontalSpacingPhase`.
- Still true: `ControlFlowBraceTransform.NormalizeTrailingTrivia` keeps the trailing comment with no line break behind it; the trace shows `{Foo(); // trailing}`. `LineBreakPhase` repairs it.
- Still true: `ControlFlowBraceTransform.WrapWithBraces` writes an end-of-line after the inserted `}` and after the open brace when leading comments exist; `NormalizeLeadingTrivia` writes one after every leading comment and drops the original line breaks.
- Still true: `ControlFlowBraceTransformTests.TrailingCommentOnIfBodyIsPreserved` only checks that `// note` is in the text and that the reparsed `if` body is a block.
- Still true: `RawStringLineShifts.MoveLines`/`ComputeMovedLiteral` reparse with `SyntaxFactory.ParseExpression` and `root.SyntaxTree.Options` (fallback `LanguageVersion.Latest`).
- Still true (by the code path; not reproduced): `FormatNodeInDocumentWithContextAsync` annotates the target's first token and returns `document` when `FindCorrespondingNode` finds no annotated token.
- Still true: `LineBreakTriviaUtilities.MoveTokenToNewLine` trims the previous token's trailing whitespace only when `TokenContainmentUtilities.ContainsToken(node, previousToken)`. The trace keeps the space behind `if (true) ` after `LineBreakPhase` and trims the one behind `void M()`; `CleanupPhase` removes the remaining one.
- Still true: member-body braces go through `LineBreakBlockRewriter.VisitBlock` (`BracePlacer.EnsureBraceOnOwnLine`) and `DeclarationBraceLineBreakRewriter` (`BracePlacer.NormalizeOwnedBraces`).
- Still true: `RH7101DoNotCombineFieldsCodeFixProvider` runs `FieldDeclarationSplitTransform` on its own, and the transform writes `_context.EndOfLine` and the member indentation; thoenissen/Reihitsu#1502 is open.
- Already moot: the blocker thoenissen/Reihitsu#1487 is closed (merged through thoenissen/Reihitsu#1510). None of the issue's statements was made true or moot by it.
- Not in the issue but also covered by FMT-STRUCT-026: `AccessorExpressionBodyTransform` writes a space after the accessor keyword and after the created `=>`, and `GetOnlyMemberExpressionBodyTransform` writes a space on both sides of the created `=>`. The parser does not need any of these spaces, so FMT-STRUCT-026 forbids them, as the issue says ("the structural transforms … no other layout"). `FieldDeclarationSplitTransform`'s spaces in front of moved comments fall under its recorded exception.
- Found while checking: for leading comments of an unbraced `if` body, today's final output does not meet FMT-PIPE-013. `if (a)` + `/* x */ /* y */ Foo();` ends with each comment and `Foo();` on its own line, and a blank line between two leading `//` comments is dropped (trace on the scratch input). FMT-PIPE-013 already specifies the intended output, so this is not an open question. FMT-STRUCT-026 requires the transform to keep the comments' own line breaks and spaces, which corrects both inputs. The change doc lists them under What as the two inputs whose final output changes. No existing test asserts today's output for them (`ControlFlowBraceCommentTests`, `PipelineRequirementTests`, and `ControlFlowBraceTransformTests` cover only one-line `//` comments).

## Changed requirements

- FMT-PIPE-021 — Every phase returns a tree its text parses back to — added
- FMT-PIPE-022 — Every phase keeps the annotations it does not own — added
- FMT-STRUCT-026 — The phase writes only the separators its text needs — added
- FMT-BREAK-029 — Whitespace in front of an inserted line break is left to the cleanup phase — added
- FMT-PIPE-019 — example sentence changed: the transform writes the line break behind the opening brace, and the one behind the comment is the author's
- `structural-transforms.md` first non-goal — changed to except FMT-STRUCT-026's separators and the field split's layout
- `Uses` rows updated in `pipeline.md`, `structural-transforms.md`, and `line-breaks.md`. The ledger has the four new IDs. `SpecConsistencyTests` and `SpecStructureTests` pass (26/26). `RequirementCoverageTests` will fail until the new requirements have tests.

No rule page changes, because no analyzer behavior changes. No code owner changes.

## Examples for the tests

- FMT-PIPE-021: the first example (the issue's first input). Reparse each phase's output and compare it with the phase's tree by the definition in the requirement. The second example is the qualifier on both sides: whitespace and line break owned by the other token passes, and a leading comment behind an inserted `{` fails. The issue's Done-when asks for this test over the formatter test inputs and the spec examples.
- FMT-PIPE-022: the raw-string example. Annotations on `Call` and on the text token holding `a ` survive the indentation phase. Test it at document level, without the context entry point.
- FMT-STRUCT-026: both examples (phase output, plus the unchanged final output of the first). The second covers the qualifiers on both sides: `//` vs block comment, a comment after `{` vs none, and a directive after `}` vs none. `TrailingCommentOnIfBodyIsPreserved` has to assert the brace outside the comment on the phase output (Done-when).
- FMT-BREAK-029: the `void M() {` / `if (true) {` example (final output; the phase output may keep the spaces).
- FMT-PIPE-013 (existing, not changed): the two inputs listed above need tests of their final output, tagged FMT-PIPE-013.

## Open questions for the maintainer

_None._ (The question that came up is blocking and is under Decisions needed.)

## Decisions needed

### 1. Output of `FormatNodeInDocumentWithContextAsync` for a target inside a raw string literal that the indentation phase moves

**Input** (the issue's second input): target is the invocation `Call(1, 2)`, context is the local declaration.

```csharp
class C
{
    void M()
    {
        var s = $"""
            a {Call(1,
            2)} b
            """;
    }
}
```

**Real output today:** the document comes back unchanged, because the raw-string move rebuilds the annotated token (code path above).

**Why the issue's target does not follow from the specs:** the issue expects "`2` is placed under `1`, as it is today when the literal is already aligned and as `FormatNodeInDocumentAsync` on the declaration places it". The phases lay out the context. The trace of the same document gives `                 a {Call(1,` / `                         2)} b`: the literal moves +5 under its opening quotes, `1` lands in column 25 and `2` in column 25. FMT-PIPE-018 and FMT-PIPE-010 make the entry point write back only the target. The literal text `            a {` in front of `Call` is outside the target and stays where it is in the document, and so does the closing `"""`. So once FMT-PIPE-022 keeps the annotation, today's write-back gives `Call` in column 15, `1` in column 20, and `2` in the phases' column 25. `2` is not under `1`. This is the gap FMT-PIPE-017 records ("a target whose first token the phases place in another line or column than the one it keeps in the document"), in a third shape it does not list: the phases move the line the target starts on instead of breaking or re-indenting it.

**Competing readings and their output:**

- **A — Record a third case of FMT-PIPE-017's gap.** The fix keeps the annotation and nothing else changes. Output:

  ```csharp
          var s = $"""
              a {Call(1,
                           2)} b
              """;
  ```

  This is consistent with the two recorded cases, which produce the same kind of misalignment (FMT-PIPE-017's RH5111 example). FMT-PIPE-017 gains the third case, and the Done-when test asserts this output. Closing the gap is left to thoenissen/Reihitsu#1501 (E1).
- **B — `2` under `1` where `1` stays in the document (column 20).** Output: `2)} b` with `2` in column 20, and the literal unchanged. The entry point has to lay out the target from its first token's column in the document, against the raw-string move inside the phases. That is the restructuring FMT-PIPE-018's Why defers and E1's subject, so it grows this run's scope (escalation 5).
- **C — `2` under `1` with the literal moved (`1` and `2` in column 25), as the issue's wording "as `FormatNodeInDocumentAsync` on the declaration places it" reads.** The entry point writes back the literal's lines outside the target. That contradicts FMT-PIPE-010 ("rewrite only the target").
- **D — Keep returning the document unchanged for such a target, recorded as a refusal.** This drops the issue's Done-when item "a test covers the context entry point with a target inside a moving literal" in its intended form.

**Recommendation: A.** It is what FMT-PIPE-017, FMT-PIPE-018, and FMT-PIPE-010 already imply once the annotation survives. It adds no mechanism, and it leaves the alignment to the package that closes the gap (E1). B would be the better end state, but only through E1's restructuring. What would change the pick: if the maintainer wants this run to close the gap for this shape, B.

Until this is answered, nothing about this input's output is in the spec. FMT-PIPE-022's example deliberately tests annotations at document level only, and FMT-PIPE-017 and FMT-PIPE-018 are unchanged. Everything else in the change doc and the spec diff is independent of the answer.
