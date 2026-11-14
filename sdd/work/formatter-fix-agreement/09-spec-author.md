Role: spec author
Commit: 90080a6
Verdict: spec written (repair round 1, worklist item a)

My prompt carried no intended fix beyond the auditor's suggestion in `08-auditor.md` finding #1, which I re-derived: in `FluentChain.ContinuesChain` the assignment arm is the only read two levels above a node (`IsConditionalAccessBranch(assignment, boundary)`), so the only root for which the boundary's second flag answers is an assignment that is a conditional access's `WhenNotNull`; after RH5111-006 no caller of `FormatNodeInDocumentAsync` passes one (auditor's caller list), and the RH6023 fix formats nothing (SHARED-OPSPACE-004).

## thoenissen/Reihitsu#1529

### Changed requirements

- FMT-PIPE-024 — Node-level input facts are plain values — changed: the *Conditional-access branch* row is removed; the SHARED-CHAIN-001 bullet now names only the chain-continuation fact, and states that no caller passes `FormatNodeInDocumentAsync` an assignment behind a `?` (the one root for which the rule would read further above the root), so the requirement does not decide that root (`AGENTS.md`, *Supported inputs*; same form as FMT-PIPE-010's "does not decide that case"). No visible output changes.
- Change doc `sdd/changes/2026-11/conditional-access-assignment-fix-layout.md` (unmerged, this run's): Affected requirements gains FMT-PIPE-024 (changed); new *Decisions* subsection "The conditional-access-branch fact goes"; alternative E's reason amended (the fact is removed under B as well). Status stays Accepted: no visible output change added. `node-root-detached.md` is history and not edited.
- `FluentChainBoundary` stays a code owner of `pipeline.md` (`RootContinuesChain` remains); `sdd/specs/README.md` unchanged.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51/51 pass.

### Examples for the tests

_None._ The removed fact had no test left (`08-auditor.md`); the implementer deletes `FluentChainBoundary.RootIsConditionalAccessBranch`, its constructor parameter and `From` argument, and the `node.Parent == null` branch of `FluentChain.IsConditionalAccessBranch`, keeping `RootContinuesChain`.

### Output questions

_None._

### Internal choices

- The case "assignment behind `?` as root of `FormatNodeInDocumentAsync`" is stated as not decided rather than silently dropped, so the bullet does not claim the chain-continuation fact covers every read above the root. Alternative: drop the sentence — rejected, the bullet would then overstate.
- Recorded in this run's change doc rather than a new one: the fact became dead through this run's change. Alternative: a separate change doc — rejected as noise for a no-output deletion.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1543

Nothing to do in this repair round.
