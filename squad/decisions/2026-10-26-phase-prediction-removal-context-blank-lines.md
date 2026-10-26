# Blank lines in front of node-level targets

| | |
|---|---|
| Date | 2026-10-26 |
| Escalation | 2, 1 |
| Run | issue #1463 |

## Question

1. The change doc gained FMT-PIPE-018 (the context entry point keeps the document's blank lines in front of a target that starts its line, as it did before the run) and "change no line's column" wording for the node-level guarantee and the `AGENTS.md` sentence. Approve? A approve, B reject.
2. No spec states how `FormatNodeInDocumentAsync` treats blank lines in front of a target that starts its line; it may conflict with SHARED-FIX-006. A — present "keep the document's blank lines there too" as a follow-up after probing the fixes; B — specify today's behavior in this run.

## Answer

1A, 2A — taken by the lead as its recommended options under the maintainer's standing instruction of 2026-10-25. Checkpoint before this decision: 5a8bc77.

## Reason

1: the sentence records kept behavior and tightens wording; no decision changes. 2: this run did not change that entry point's blank lines, and the possible SHARED-FIX-006 conflict needs probing first.
