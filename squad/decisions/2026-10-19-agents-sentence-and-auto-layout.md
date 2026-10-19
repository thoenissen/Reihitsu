# Keep the issue's AGENTS.md sentence, and does LayoutKind.Auto count for field order?

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | 1 |
| Run | issue #1068 |

## Question

1. The new `AGENTS.md` sentence, taken word for word from the issue, says SHARED-FIX-003 states what makes a fix unsafe, while SHARED-FIX-003 does not cover relocating or reordering comments, directives, or disabled text (FC-004), which the preceding bullet also calls unsafe. Options: keep the sentence and track the gap as a follow-up that extends SHARED-FIX-003; or reword the sentence.
2. The spec author excluded structs with `[StructLayout(LayoutKind.Auto)]` from the field-order condition of SHARED-FIX-003, because with Auto layout the runtime chooses the order. Options: exclude Auto; or include Auto.

## Answer

1. Keep the issue's sentence unchanged. The relocation and reordering gap becomes a follow-up issue, drafted for the maintainer's approval, that extends SHARED-FIX-003.
2. Exclude Auto: only types whose field order actually sets the layout count.

## Reason

none given
