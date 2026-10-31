Role: spec author
Commit: e1cc51c
Verdict: spec written

Repair round 1: auditor finding #2 (`12-auditor.md`), the implementer's *Disputed tests or scope* notes (`14-implementer.md`), and decisions 14, 15, and 18 in `00-lead.md`. The prompt carried no intended fix beyond those; nothing was ignored. No test expectation was changed.

## What changed

**Code owners.** In `sdd/specs/formatter/pipeline.md` and in the `sdd/specs/README.md` index, I added `RootConstruct`, `RootConstructExtensions`, `TriviaFact`, and `FluentChainBoundary`.

`FluentChainBoundary` is a Core type, but it belongs to `formatter/pipeline.md`, not to `shared/fluent-chain-layout.md`:

- A type is a code owner of the spec whose behavior it implements (`sdd/README.md`, *Code owners*). `FluentChainBoundary` implements the node-level input fact that FMT-PIPE-024 specifies: two plain values about the code above a root.
- The rule it feeds is SHARED-CHAIN-001's. That rule's owner, `FluentChain`, stays a code owner of the shared spec.
- No other spec lists `FluentChainBoundary`, and the architecture tests confirm that it has a single owner.

**FMT-PIPE-024.**

- A table row was added: **Conditional-access branch**, "whether the root is the part of a conditional access behind its `?`, so that an assignment that is the root continues the chain of its left side". The **Chain continuation** row stays.
- The chain item of the readers list is now its own bullet. SHARED-CHAIN-001's rule — only the outermost node of a chain is laid out as a chain — decides for every node of the root which node is the outermost. Where the rule would read above the root, it reads the two chain facts. The line-break phase lays out, and the indentation phase aligns, a chain only at the node the rule names.
- The line-break bullet keeps the other construct facts.

**FMT-PIPE-012.** No change. Its depth rule refers to FMT-INDENT-002 to FMT-INDENT-004. `SyntaxIndentationUtilities`, which now holds `ComputeIndentLevel`, is already a code owner of `formatter/indentation.md`. A requirement does not name an implementing method (`sdd/README.md`, *Spec format*).

**Change doc** `sdd/changes/2026-10/node-root-detached.md`. New passages are marked *(amended 2026-10-31, audit stage)*, and the amendment note names them.

- **Chain continuation decision:**
  - One flag was not enough. The rule reads two levels up for the left side of an assignment that is a conditional access's branch behind `?`, as in `a?.B⏎.C = 5`. With that assignment as root, the read left the root, and the RH6023 fix laid the chain out differently from `main`.
  - A second flag answers that read.
  - The rule has one owner, `FluentChain`, which takes both flags as one boundary value and answers for every node of the root. The line-break phase and the chain alignment both ask it.
  - A formatter-side copy of the assignment arm was rejected.
- **What, the inner-chain-part bullet:** the chain alignment now asks the same rule, so it no longer aligns such a target as a chain of its own.
- **Out of scope, the inner-chain-part entry (decision 7):** the two decisions no longer disagree. The `.Where` line that was aligned in column 37 is no longer aligned as a chain's first link. The output still differs from document formatting, and no caller reaches it.
- **Visible output changes:** unchanged. The repair restores `main`'s RH6023 output (decision 14), and the inner chain part is reached by no caller (decision 18).

## Changed requirements

- FMT-PIPE-024 — Node-level input facts are plain values — changed: a conditional-access-branch row, and the chain rule answering for every node of the root through one owner.

No ID was added; the ledger is unchanged.

`scripts/test.sh --no-install --project architecture`: Passed 92/92.

## Examples for the tests

- FMT-PIPE-024's new row is covered by the round-1 RH6023 regression tests and by `ReihitsuFormatterDetachedRootTests.ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember`, both from `13-test-implementer.md` and `14-implementer.md`.
- Whether those tests carry the `FMT-PIPE-024` tag is for the spec checker.

## Output questions

_None._

## Internal choices

- **`FluentChainBoundary`'s owner:** `formatter/pipeline.md`, not `shared/fluent-chain-layout.md`. The alternative would add a code owner to the shared spec that no shared requirement describes.
- **FMT-PIPE-012:** names no method. The alternative is to name `SyntaxIndentationUtilities.ComputeIndentLevel` there.

## Follow-up candidates

_None._
