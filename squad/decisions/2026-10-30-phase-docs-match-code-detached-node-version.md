# Which language version FMT-PIPE-008 names for a node without a document

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 4 |
| Run | issue #1484 |

## Question

The reworded FMT-PIPE-008 said a detached node passed to `FormatNode` belongs to a tree with the default parse options. A covering test showed that a node parsed with `SyntaxFactory.ParseCompilationUnit(text, options: C# 11)` is formatted for C# 11 (`class Empty { }` stays braced), because Roslyn keeps the options the node was parsed with; only a node parsed or built without parse options gets the newest version.

- **A** — Narrow the clause to the code: a node parsed with parse options keeps them; a node parsed or built without them gets the default parse options. The failing test expects the C# 11 result.
- **B** — Drop the detached-node clause and the failing test.
- **C** — Change `FormatNode` so that every detached node gets the newest version (behavior change, change doc, production change).

## Answer

A — narrow the FMT-PIPE-008 clause to what the code does and have the covering test expect the C# 11 result for a node parsed with C# 11 options.

## Reason

None given.
