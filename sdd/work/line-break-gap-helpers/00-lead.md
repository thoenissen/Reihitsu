# Lead — line-break-gap-helpers

- Issues: thoenissen/Reihitsu#1640 (`agent-filed`), thoenissen/Reihitsu#1651 (`agent-filed`). Goals of both are proposals.
- Slug: `line-break-gap-helpers`
- Branch: `claude/loving-mayer-uxnwuu` (session-designated). Draft PR: #1858.
- Lane: **bug fix**. Both issues ask to move private copies of a line-break-phase decision onto the shared owner in `LineBreakTriviaUtilities` (#1640: batch gap joins onto `JoinGaps`; #1651: "line break in front of a moved token, keeping an existing one" onto `EndLineInFrontOf`), with no change of formatter output. Production paths only, no change doc expected. Shared mechanism: `LineBreakTriviaUtilities` and `ChainLineBreakRewriter`, which both issues touch.
- Roles run: skeptic (premise), test implementer (characterization/coverage of the shared helpers and the rewriters' outputs; no reproduction exists for a pure refactor), implementer, cleaner, spec checker, auditor, validator. Spec author skipped unless a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

1. Escalation 10 (declare the shape "code behind the closing `*/` of a multi-line block comment" unsupported?): **A — not declared.** Formatter output stays as on `main` in this run; the missing line break is a follow-up bug.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | #1640: how the chain's joins compose with its line-break insertions | Every layout decision keeps reading the tree before the joins; tokens are never found again by text or position (no two-pass re-resolution, FC-026). The implementer picks the mechanism within that (e.g. `JoinGaps` composing with edits it does not make), and the `JoinGaps` summary must state its real precondition | Skeptic cells J, K, H, I; FC-026 | Pure refactor; existing formatter tests lock the output | — |
| 2 | skeptic | #1640 "Done when" grep is not checkable | Replace with: `ChainLineBreakRewriter` keeps no join bookkeeping of its own, its joins go through `JoinGaps`, formatter output unchanged | Agent-filed goal; grep hits single joins that are not batch loops | No output involved | — |
| 3 | skeptic | #1651 goal "no rewriter carries a private copy" | Narrowed: chain, list and constraint sites write the break through `EndLineInFrontOf` where the helper's decision is theirs; the chain's comment-tail branch (FMT-BREAK-019 ¶2, SHARED-COMMENT-012) stays chain-only; the list's split flag stays; the prepend family is out of scope. Issue's stale second list site already shared (`c6d199c`) | Skeptic: comment-tail branch is not a copy; moving it into the helper would change binary/ternary output | Final output unchanged at every site |
| 4 | skeptic | #1651 whitespace in front of the inserted break | Option a: the helper stays as it is; migrated sites keep the whitespace in the phase's output, cleanup removes it (FMT-BREAK-029, FMT-CLEAN-001). Phase-level tests that lock stripped text may change; final output may not | Matches FMT-BREAK-029's single owner of trailing whitespace | Final formatter output unchanged; phase output is internal | — |
| 5 | skeptic | #1651 architecture/unit test | Keep a unit test of `EndLineInFrontOf`'s branches (and `JoinGaps`, which has none either); drop the architecture-test idea | A source-pattern ban is grep-like and defeated by a rename | Tests only | — |
| 6 | skeptic | Architecture finding: comment-tail class (SHARED-COMMENT-012) fails in every non-chain rewriter | Not a finding of this run: decision 3 keeps the chain branch, so the run neither depends on nor exposes it. Follow-up bug (it violates unchanged requirements and leaves RH5302/RH5308 fixes without progress on formatter output), carrying the FMT-BREAK-037 output question for cell F. Escalation 10 asked now | `squad/README.md`, Architecture findings: unrelated structural defect → follow-up | No output change in this run | — |
| 7 | skeptic | #1651 fact evaluated twice at the list site | Accept; pure predicate | Skeptic recommendation | Internal | — |
| 8 | test implementer | Two new chain tests lock the member name behind a `//` comment or a directive between `.` and the member name at the statement's column (possible indentation defect) | Keep the tests as characterization of today's output; drop the candidate: a comment or directive between `.` and its member name is a rare input class (`AGENTS.md`, Code-fix coverage) | Not a follow-up per `lead.md`, Follow-ups | The run changes no output there; the tests lock `main`'s output | c92003c |
| 9 | implementer | `LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed` asserts the line-break phase's own text with the space before the inserted LF removed; the phase now keeps it | The test's expectation changes to allow the whitespace in front of the line break; its purpose (the break is LF, no CRLF) stays. Routed to the test implementer | Decision 4: phase-level whitespace is internal; cleanup owns it (FMT-BREAK-029, FMT-CLEAN-001) | Final formatter output unchanged; the test asserts a single phase's output | f1a1732 |
| 10 | implementer | Selfhosting `WorkingDocumentsDirectoryContainsNoFiles` fails | Expected while `sdd/work/` exists; it is deleted before publishing | `lead.md` section 8 | Internal | — |

## Follow-up candidates

1. Bug: tokens behind a multi-line comment's closing line are not broken onto their own line (cells C, U, D, R, E, P, Q, S, T; SHARED-COMMENT-012 with FMT-BREAK-012/017/018/022/023; RH5302/RH5308 fixes make no progress on formatter output, RH5203 reports it). Carries the FMT-BREAK-037 output question for cell F. — `01-skeptic.md`, #1651 Follow-up candidates 1 and 2. The maintainer chose not to declare the shape unsupported (escalation 10, answer A); the follow-up fixes the line break instead.

## State

- Last step: skeptic (`01-skeptic.md`, f206305): #1640 proceed; #1651 proceed narrower.
- Escalation 10 answered (A, not declared).
- Test implementer done (`02-test-implementer.md`, c92003c); lane check confirmed.
- Implementer done (`03-implementer.md`, c35dfca): one disputed phase-level test.
- Test implementer follow-up done (`04-test-implementer.md`, f1a1732): formatter 5781/0.
- Cleaner done (`05-cleaner.md`, 510ec3b): clean.
- Next: spec checker.
