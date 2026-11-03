# Lead — per-phase-idempotency

- Issue: thoenissen/Reihitsu#1500 — "FMT-PIPE-004: no single phase changes the formatter's output". Label `agent-filed`: its goals are proposals.
- Parent: thoenissen/Reihitsu#1482 (package D3). Blockers thoenissen/Reihitsu#1483, #1499, #1497, #1494 are closed (merged); #1487 and #1496 are reached through #1499.
- Slug: `per-phase-idempotency`
- Branch: `claude/kind-bohr-ib25uy` (session-designated branch, see Decisions taken #1)
- PR: thoenissen/Reihitsu#1575 (draft claim)
- Base: `origin/main` at `fe24dcc`

## Lane

Claimed first as behavior change (the issue proposes a change doc). Narrowed to **tests** after the skeptic (Decisions taken #3): FMT-PIPE-027 writes down behavior the formatter already has (exception list empty on `main`, suite green without it), which `sdd/README.md` calls a clarification that needs no change doc. The diff is a spec and test-project files only.

Roles (tests lane): spec author → skeptic → test implementer → cleaner → spec checker → validator. Skipped: implementer and auditor (the tests lane runs neither; no shipped source changes). Issue manager before publishing when follow-up candidates exist.

## Notes for the roles

- The issue was written against `main` at `735d0dba`; every statement about today's code (the exception list in the formatter test helpers, which fixtures it holds) is to be checked against current `main`. A statement an earlier package already made true or moot is noted in the handover and dropped.
- The issue's comment (2026-10-28) states that thoenissen/Reihitsu#1497 keeps the blank line above a comment that leads a same-group using, so the example's expected output keeps it.
- The change doc has to decide whether the per-phase property also covers the node-level entry points, and which tests cover the extension.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/kind-bohr-ib25uy` instead of `claude/issue-1500-per-phase-idempotency` | The session's instructions fix the branch to push to | Branch names are internal | 7737a43 |
| 2 | Spec author | Extend FMT-PIPE-004 itself, or add a new requirement | New requirement FMT-PIPE-027; FMT-PIPE-004 gains a pointer to it. The node-level entry points are excluded; the comparison is on text; only output of a run that formatted is covered | One decision per heading; the per-phase property has a different reason than text idempotency. The issue is `agent-filed`, so "FMT-PIPE-004 states the property" is a proposal; skeptic agrees | Spec structure and requirement IDs are internal; the formatter's output is unchanged | 1e7c44a |
| 3 | Skeptic | Keep the change doc and the behavior-change lane | Drop the change doc; claim the tests lane | `sdd/README.md`: a requirement that writes down existing behavior is a clarification without a change doc; no code and no output changes | No visible output changes; the change doc listed none | (spec-author amendment) |
| 4 | Skeptic | Which tests carry `FMT-PIPE-027` | The new example test, `CheckFindsNoChangeInFormattedSource`, `CheckRunsTheProductionPhasesInPipelineOrder`, and `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup` (it is example 1); tests with a fake phase stay untagged | Tags mark tests of the formatter, not of the helper | Test tags are internal | (test implementer) |
| 5 | Skeptic | Entry point of the example test | Through `FormatSyntaxTree`, which the requirement names | The requirement covers the public document/tree entry points | Internal test choice | (test implementer) |
| 6 | Skeptic | The helper checks inputs with syntax errors too, stricter than FMT-PIPE-027 | Keep it, and state it in the helper's summary | Stricter and green; narrowing the check gains nothing | Test helper only | (test implementer) |
| 7 | Skeptic | Scope of the removal | Remove the exception list and all machinery that only serves it (`PhaseReversalCause(s)`, `PhaseReversalEntry`, `PhaseReversalLineEndings`, `PhaseReversalExceptionList`, unreached-entry reporting, the `entries` parameter, `DataRowKey`/`FormatTestName`/`FormatTestKey`/`ComputeFingerprint` and per-test identity tracking where only entries use them) and the doc comments that describe it (02-skeptic.md lists the files) | The issue's goal is the list's deletion; dead helpers and stale comments would be left behind otherwise (FC-031) | Test project only | (test implementer) |
| 8 | Skeptic | Issue clause "every formatter test that checks idempotency checks it per phase as well" | Dropped: seven classes check idempotency without `FormatterTestsBase` (02-skeptic.md); moving them is a follow-up candidate | The clause is false as a consequence of deleting the list; `agent-filed` goal | Test coverage only | — |

## Follow-up candidates

- Run the per-phase check in the self-hosting scans (`FormatterSelfHostingTests`) — 01-spec-author.md, 02-skeptic.md.
- Run the per-phase check in the seven formatter test classes that check idempotency without `FormatterTestsBase` (listed in 02-skeptic.md) — 02-skeptic.md.
