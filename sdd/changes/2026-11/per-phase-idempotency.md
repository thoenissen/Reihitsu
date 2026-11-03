# No single phase changes the formatter's output

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-027 (added), FMT-PIPE-004 (clarified: points to FMT-PIPE-027) |

## What

The formatter's output is now a fixed point of every phase, not only of the whole pipeline. FMT-PIPE-027 requires that every phase, each sub-phase of the preparation phase included, run alone on the output of `FormatDocumentAsync` or `FormatSyntaxTree`, returns that output with the same text. FMT-PIPE-004 keeps its rule for whole passes and points to the new requirement.

The phase-level stability check of the formatter tests becomes the check of this requirement without exceptions: the exception list that let a fixture's output be changed by a phase, and every part of the check that only serves that list, are removed, so a phase that changes a fixture's final output fails the test.

This implements thoenissen/Reihitsu#1500.

## Why

FMT-PIPE-004 compares the text of two whole passes. Two phases that undo each other in every pass — one removes a blank line and a later one writes it back, one strips the indentation of a line and a later one restores it — meet it, although two phases own one decision, which FMT-PIPE-001 and FMT-PIPE-025 rule out, and an analyzer code fix that applies one of the two rules writes the output the other one reverts. The using phase and the blank-line phase were such a pair for the blank line above a comment that leads a using directive of the same group, until [A blank line above a comment does not split a using group](using-comment-blank-line.md) gave that blank line one owner.

The formatter tests already ran every phase alone on each fixture's final output, with an exception list of the fixtures that still failed for known causes. The restructuring that precedes this change removed those causes, and on `main` at `fe24dcc` the list is empty: every formatter fixture already meets FMT-PIPE-027. What remains is to make the property a requirement, so a new reversal is a defect against a spec and not an entry on a list.

## Visible output changes

None — no output an end user sees changes. The requirement writes down a property that every fixture of the formatter tests already has; the change removes test infrastructure and changes no production code. The issue's example is kept unchanged today, and the trace (`scripts/trace.sh`) shows no phase changing it in a pass:

```csharp
using System;

// About Linq
using System.Linq;

class C;
```

## Decisions

### A requirement of its own

The per-phase property is FMT-PIPE-027, not a second sentence of FMT-PIPE-004. The two requirements have different reasons: FMT-PIPE-004 protects the users of the output (`--check`, the code fixes, CI gates) from an endless diff, while FMT-PIPE-027 rules out two phases that own one decision, a reversal FMT-PIPE-004 cannot see. A requirement is one decision with its own `Why:` (`sdd/README.md`, *Spec format*). FMT-PIPE-004 keeps its text and its tests and gains a sentence that points to FMT-PIPE-027.

### Document and tree entry points only

FMT-PIPE-027 covers `FormatDocumentAsync` and `FormatSyntaxTree`. The node-level entry points pass the pipeline a detached root together with the facts of FMT-PIPE-024; a phase run alone on the text of their output would receive neither, so it would check another input than the one they format. They run the same phases, so a reversal between two phases shows in the document entry points as well, and a node a code fix formats is covered once its document is formatted. FMT-PIPE-021 draws the same line for the same reason.

### Output of a run that formatted

An input with a syntax error or a generated-code marker is returned unchanged (FMT-PIPE-005, FMT-PIPE-006); such text is not formatted output, and the requirement says nothing about a phase run alone on it.

### Compared as text

A phase's result is compared with the output as text, as the phase-level stability check does. A phase may give a run of whitespace or a line break to the other of the two tokens around it (FMT-PIPE-021) without changing anything FMT-PIPE-027 sees.

### No exception list

The exception list, its causes, its entries, and the check that an entry is still reached are removed. A future reversal is a defect against FMT-PIPE-027 and is fixed or escalated like any other; a list would let it be accepted without changing the spec. The list is empty on `main`, so its removal turns no passing test into a failing one.

### Tests

FMT-PIPE-027 is covered by tests tagged with it:

- a pipeline test that formats each example of FMT-PIPE-027 and runs every production phase alone on the output, under LF and CRLF;
- the tests of the phase-level stability check that show it runs the production phases in pipeline order and finds a phase that changes the text it runs on.

Every other formatter test keeps running the check on its fixtures through the test base, untagged, since it verifies FMT-PIPE-027 only incidentally (`sdd/README.md`, *Tests and requirements*).

## Alternatives considered

- **Keep text-level idempotency only** — rejected: two phases that undo each other in every pass meet it, so a disagreement between two of the formatter's own rules stays invisible.
- **Extend FMT-PIPE-004 instead of adding a requirement** — rejected: the two properties have different reasons, and one heading per decision keeps each `Why:` precise.
- **Cover the node-level entry points too** — rejected: their phases receive a root and facts that the text of their output does not carry, so a phase run alone on that text would check another input; the document entry points already run the same phases.
- **Keep the exception list for future known causes** — rejected: an accepted reversal is a spec violation recorded outside the spec.
- **Add the per-phase check to the self-hosting scans** — not part of this change: the issue asks for the formatter test suite, and the scans run outside `scripts/test.sh`.
