# Output of the context entry point for a target inside a raw string literal the indentation phase moves

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1488 |

## Question

The issue expected `FormatNodeInDocumentWithContextAsync`, with the invocation `Call(1, 2)` inside a misaligned interpolated raw string literal as target and the local declaration as context, to place `2` under `1`. Under FMT-PIPE-010, FMT-PIPE-017, and FMT-PIPE-018, once the target annotation survives, `1` stays in its document column 20 while the phases put `2` in column 25. Options: A record a third case of FMT-PIPE-017's gap and assert that output; B place `2` in column 20, which needs the entry-point restructuring of thoenissen/Reihitsu#1501 and grows the scope; C write back the moved literal lines outside the target, against FMT-PIPE-010; D keep returning the document unchanged as a recorded refusal.

## Answer

A — FMT-PIPE-017 gains the third gap case (the phases move the line the target starts on); the run keeps the annotation and asserts `2` in column 25 with the rest of the literal as in the document. Closing the gap stays with thoenissen/Reihitsu#1501.

## Reason

None given beyond accepting the recommendation.
