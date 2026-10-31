# Approve the change doc that detaches every node-level root and specifies its input facts as plain values

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 2 |
| Run | issue #1489 |

## Question

Approve What and Why of `sdd/changes/2026-10/node-root-detached.md`: FMT-PIPE-023 (every node-level entry point passes a detached root; no phase reads the document beyond it), FMT-PIPE-024 (the input facts as plain values and which entry point supplies each; `FormatNode` supplies only the depth and the documentation flag), FMT-PIPE-012 (the depth is the indentation phase's depth at the node's position; no level per initializer; the formatter no longer calls `ComputeBaseIndentLevel`), and FMT-CLEAN-008 (every node-level root). Changed outputs: `FormatNode` loses the leading line break behind a blank line or in front of a moved documentation comment, places an unbraced embedded statement one level deeper, and lays out initializer members at the holding statement's depth. (A approve as written; B keep the extra level per initializer; C keep the preceding-token and list facts for an attached `FormatNode` root)

## Answer

A — approved as written.

## Reason

None given beyond accepting the recommendation.
