# Member-body brace placement without a cross-rewriter prediction

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

To give member-body braces one path, `LineBreakBlockRewriter` skips blocks whose `Parent` is a member declaration and event accessor lists, predicting that `DeclarationBraceLineBreakRewriter` places them; that rewriter only visits declarations inside the formatting root, so node-level formatting of a member-body block leaves `{ Foo(); }` where `main` breaks it. Options: A skip only when the owning declaration lies inside the root and file a follow-up; B invert the ownership — `LineBreakBlockRewriter` places every block's and event accessor list's braces, member bodies included, and `DeclarationBraceLineBreakRewriter` stops placing them, with the final output as on `main` or the case coming back; C stop the run.

## Answer

B — `LineBreakBlockRewriter` is the one owner of block and event-accessor-list braces; `DeclarationBraceLineBreakRewriter` no longer places them; the final output stays as on `main`, or the case comes back to the maintainer.

## Reason

None given beyond accepting the recommendation.
